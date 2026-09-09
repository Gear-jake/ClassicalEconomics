using System.Collections.Generic;
using Newtonsoft.Json;
using EconomyMod.Models;

namespace EconomyMod.Services
{
    /// <summary>
    /// Mod 自建本地化服务（v1.6.0：六语言 zh / zh_tw / en / ru / ja / de）。
    /// 界面语言永远跟随游戏本体语言：简中（cz / zh-Hans 等）→ zh，繁中（ch / zh-Hant）→ zh_tw，
    /// en → en，ru → ru，日语（ja / 日本語）→ ja，德语（de / Deutsch）→ de，
    /// 其余语言回退 en（Get() 内部还有 en→zh 二级兜底）。运行时从 Locales/ 下
    /// ch.json / zh_tw.json / en.json / ru.json / ja.json / de.json 动态加载。
    /// </summary>
    public static class LocalizationService
    {
        private static Dictionary<string, string> _zh = new Dictionary<string, string>();
        private static Dictionary<string, string> _zhTw = new Dictionary<string, string>();
        private static Dictionary<string, string> _en = new Dictionary<string, string>();
        private static Dictionary<string, string> _ru = new Dictionary<string, string>();
        private static Dictionary<string, string> _ja = new Dictionary<string, string>();
        private static Dictionary<string, string> _de = new Dictionary<string, string>();
        private static bool _loaded;
        private static string _localeDirectory;
        private static bool _zhLoaded;
        private static bool _zhTwLoaded;
        private static bool _enLoaded;
        private static bool _ruLoaded;
        private static bool _jaLoaded;
        private static bool _deLoaded;
        private static string _lastGameLanguage; // auto 模式的游戏语言变化检测基线
        private static string _cachedCurrentLanguage;
        private static bool _languageCached;

        /// <summary>Mod 界面语言（v1.5.4：永远跟随游戏本体语言，不再有独立设置项）。"zh" / "zh_tw" / "en" / "ru"。</summary>
        public static string CurrentLanguage
        {
            get
            {
                if (!_languageCached) RefreshLanguageCache(GetGameLanguage());
                return _cachedCurrentLanguage;
            }
        }

        /// <summary>是否为中文系界面（简/繁）。</summary>
        public static bool IsChinese => CurrentLanguage == "zh" || CurrentLanguage == "zh_tw";

        /// <summary>读取游戏本体当前语言 id（cz / ch / zh-Hans / zh-Hant / en / ru / ...）；读不到返回 null。</summary>
        /// <summary>auto 模式下检测游戏语言变化：变化时返回 true（调用方触发全 UI 刷新）。首调用仅记基线。</summary>
        public static bool CheckGameLanguageChanged()
        {
            string cur = GetGameLanguage() ?? string.Empty;
            if (cur == _lastGameLanguage) return false;
            bool firstProbe = _lastGameLanguage == null;
            _lastGameLanguage = cur;
            RefreshLanguageCache(cur);
            return !firstProbe;
        }

        private static void RefreshLanguageCache(string gameLanguage)
        {
            _cachedCurrentLanguage = ResolveModLanguage(gameLanguage);
            _languageCached = true;
        }

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
                case "zh_cn":
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
                case "ja":          // 本体：日本語
                case "ja-jp":
                case "ja_jp":
                    return "ja";
                case "de":          // 本体：Deutsch
                case "de-de":
                case "de_de":
                    return "de";
                default:
                    return "en";
            }
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
                _localeDirectory = dir;
            }
            catch (System.Exception) { }
        }

        private static void EnsureLanguageLoaded(string lang)
        {
            if (string.IsNullOrEmpty(_localeDirectory)) return;
            switch (lang)
            {
                case "zh":
                    if (!_zhLoaded)
                    {
                        _zh = LoadFile(System.IO.Path.Combine(_localeDirectory, "ch.json"), _zh);
                        _zhLoaded = true;
                    }
                    break;
                case "zh_tw":
                    if (!_zhTwLoaded)
                    {
                        _zhTw = LoadFile(System.IO.Path.Combine(_localeDirectory, "zh_tw.json"), _zhTw);
                        _zhTwLoaded = true;
                    }
                    break;
                case "en":
                    if (!_enLoaded)
                    {
                        _en = LoadFile(System.IO.Path.Combine(_localeDirectory, "en.json"), _en);
                        _enLoaded = true;
                    }
                    break;
                case "ru":
                    if (!_ruLoaded)
                    {
                        _ru = LoadFile(System.IO.Path.Combine(_localeDirectory, "ru.json"), _ru);
                        _ruLoaded = true;
                    }
                    break;
                case "ja":
                    if (!_jaLoaded)
                    {
                        _ja = LoadFile(System.IO.Path.Combine(_localeDirectory, "ja.json"), _ja);
                        _jaLoaded = true;
                    }
                    break;
                case "de":
                    if (!_deLoaded)
                    {
                        _de = LoadFile(System.IO.Path.Combine(_localeDirectory, "de.json"), _de);
                        _deLoaded = true;
                    }
                    break;
            }
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
            EnsureLanguageLoaded(lang);
            if (TryGet(lang, key, out v)) return v;
            // 回退英文
            if (lang != "en")
            {
                EnsureLanguageLoaded("en");
                if (TryGet("en", key, out v)) return v;
            }
            // 回退简中
            if (lang != "zh")
            {
                EnsureLanguageLoaded("zh");
                if (TryGet("zh", key, out v)) return v;
            }
            return key;
        }

        private static bool TryGet(string lang, string key, out string value)
        {
            value = null;
            var dict = lang == "zh_tw" ? _zhTw : lang == "ru" ? _ru
                : lang == "ja" ? _ja : lang == "de" ? _de : lang == "en" ? _en : _zh;
            return dict.TryGetValue(key, out value) && !string.IsNullOrEmpty(value);
        }
    }
}
