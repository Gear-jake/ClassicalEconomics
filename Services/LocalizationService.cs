using System.Collections.Generic;
using Newtonsoft.Json;
using EconomyMod.Models;

namespace EconomyMod.Services
{
    /// <summary>
    /// Mod 自建本地化服务：界面语言由 Mod 配置页切换（auto / zh / zh_tw / en / ru）。
    /// 默认 auto —— 自动跟随游戏本体语言：简中（本体旧缩写 cz、规范化缩写 zh-Hans）→ zh，
    /// 繁中（ch / zh-Hant）→ zh_tw，en → en，ru → ru，其余语言回退 en；
    /// 手动指定 zh/zh_tw/en/ru 时覆盖 auto。运行时从 Locales/ch.json（简中）、
    /// zh_tw.json（繁中）、en.json（英文）、ru.json（俄文）动态加载。
    /// </summary>
    public static class LocalizationService
    {
        private static Dictionary<string, string> _zh = new Dictionary<string, string>();
        private static Dictionary<string, string> _zhTw = new Dictionary<string, string>();
        private static Dictionary<string, string> _en = new Dictionary<string, string>();
        private static Dictionary<string, string> _ru = new Dictionary<string, string>();
        private static bool _loaded;
        private static string _lastGameLanguage; // auto 模式的游戏语言变化检测基线

        /// <summary>Mod 界面语言（解析后的最终值）："zh" / "zh_tw" / "en" / "ru"。设置为 auto 时随游戏语言实时解析。</summary>
        public static string CurrentLanguage
        {
            get
            {
                string setting = UnrestConfig.Instance.Language;
                if (IsAuto(setting)) return ResolveModLanguage(GetGameLanguage());
                return setting;
            }
        }

        /// <summary>是否为中文系界面（简/繁）。</summary>
        public static bool IsChinese => CurrentLanguage == "zh" || CurrentLanguage == "zh_tw";

        /// <summary>配置值是否为"跟随游戏语言"模式（auto/空值）。</summary>
        private static bool IsAuto(string setting)
        {
            return string.IsNullOrWhiteSpace(setting)
                || string.Equals(setting.Trim(), "auto", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>读取游戏本体当前语言 id（cz / ch / zh-Hans / zh-Hant / en / ru / ...）；读不到返回 null。</summary>
        public static string GetGameLanguage()
        {
            try
            {
                var asset = LocalizedTextManager.current_language;
                if (asset != null && !string.IsNullOrEmpty(asset.id)) return asset.id;
            }
            catch (System.Exception) { }
            try
            {
                // 兜底：LocalizedTextManager.instance 的 language 字段为 internal（编译期不可见），
                // 运行时反射读取（与年份/寻路 API 同套路），失败回退 null。
                var mgr = LocalizedTextManager.instance;
                if (mgr != null)
                {
                    var t = mgr.GetType();
                    var field = t.GetField("language", System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    var lang = field != null ? field.GetValue(mgr) as string : null;
                    if (lang == null)
                    {
                        var prop = t.GetProperty("language", System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        lang = prop != null ? prop.GetValue(mgr, null) as string : null;
                    }
                    if (!string.IsNullOrEmpty(lang)) return lang;
                }
            }
            catch (System.Exception) { }
            return null;
        }

        /// <summary>
        /// 游戏语言 id → Mod 界面语言映射。兼容本体旧缩写（cz=简中 / ch=繁中）、
        /// 规范化缩写（zh-Hans/zh-Hant）及常见变体；非支持语言回退 en（Get() 内部还有 en→zh 二级兜底）。
        /// </summary>
        public static string ResolveModLanguage(string gameLanguage)
        {
            if (string.IsNullOrEmpty(gameLanguage)) return "en";
            switch (gameLanguage.ToLowerInvariant())
            {
                case "cz":          // 本体旧缩写：简体中文
                case "zh-hans":
                case "zh-cn":
                case "zh_hans":
                case "zh":
                    return "zh";
                case "ch":          // 本体旧缩写：繁体中文
                case "zh-hant":
                case "zh-tw":
                case "zh_hant":
                case "zh_tw":
                    return "zh_tw";
                case "en":
                    return "en";
                case "ru":
                    return "ru";
                default:
                    return "en";
            }
        }

        /// <summary>
        /// 游戏语言是否中文系（简/繁）。读不到游戏语言时默认 true——
        /// 保持旧 IsChinese 行为：模组加载早期 LocalizedTextManager 可能尚未初始化（作者原默认）。
        /// </summary>
        public static bool IsGameLanguageChinese
        {
            get
            {
                string lang = GetGameLanguage();
                if (string.IsNullOrEmpty(lang)) return true;
                string id = lang.ToLowerInvariant();
                return id.StartsWith("zh") || id == "cz" || id == "ch" || id.Contains("cn");
            }
        }

        /// <summary>
        /// auto 模式下检测游戏语言变化（供主循环每 0.5 秒轮询）：变化时走
        /// EconomyConfigCallbacks.OnLanguageChanged 的完整刷新路径（设置窗口标签 + 全窗口文本 + tooltip）。
        /// 首次调用只记录基线，不触发刷新。
        /// </summary>
        public static void CheckGameLanguageChanged()
        {
            try
            {
                if (!IsAuto(UnrestConfig.Instance.Language)) return;
                string cur = GetGameLanguage() ?? string.Empty;
                if (cur == _lastGameLanguage) return;
                bool firstProbe = _lastGameLanguage == null;
                _lastGameLanguage = cur;
                if (firstProbe) return;
                EconomyConfigCallbacks.OnLanguageChanged("auto");
            }
            catch (System.Exception) { }
        }

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                var main = EconomyModMain.Instance;
                var decl = main?.GetDeclaration();
                if (main == null || decl == null) return;
                string dir = main.GetLocaleFilesDirectory(decl);
                if (string.IsNullOrEmpty(dir)) return;
                _zh = LoadFile(System.IO.Path.Combine(dir, "ch.json"), _zh);
                _zhTw = LoadFile(System.IO.Path.Combine(dir, "zh_tw.json"), _zhTw);
                _en = LoadFile(System.IO.Path.Combine(dir, "en.json"), _en);
                _ru = LoadFile(System.IO.Path.Combine(dir, "ru.json"), _ru);
            }
            catch (System.Exception) { }
        }

        private static Dictionary<string, string> LoadFile(string path, Dictionary<string, string> fallback)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var loaded = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(path));
                    if (loaded != null) return loaded;
                }
            }
            catch (System.Exception) { }
            return fallback;
        }

        /// <summary>取当前语言的文本；缺失时依次回退英文、简中；仍缺失则返回 key 本身。</summary>
        public static string Get(string key)
        {
            EnsureLoaded();
            string lang = CurrentLanguage;
            string v;
            // 当前语言
            if (TryGet(lang, key, out v)) return v;
            // 回退英文
            if (lang != "en" && TryGet("en", key, out v)) return v;
            // 回退简中
            if (lang != "zh" && TryGet("zh", key, out v)) return v;
            return key;
        }

        private static bool TryGet(string lang, string key, out string value)
        {
            value = null;
            var dict = lang == "zh_tw" ? _zhTw : lang == "ru" ? _ru : lang == "en" ? _en : _zh;
            return dict.TryGetValue(key, out value) && !string.IsNullOrEmpty(value);
        }
    }
}
