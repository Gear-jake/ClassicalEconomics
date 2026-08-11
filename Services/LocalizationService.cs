using System.Collections.Generic;
using Newtonsoft.Json;
using EconomyMod.Models;

namespace EconomyMod.Services
{
    /// <summary>
    /// Mod 自建本地化服务：界面语言由 Mod 配置页切换（zh/zh_tw/en/ru），
    /// 与游戏本体语言设置完全解耦（不依赖 LocalizedTextManager）。
    /// 运行时从 Locales/ch.json（简中）、zh_tw.json（繁中）、en.json（英文）、ru.json（俄文）动态加载。
    /// </summary>
    public static class LocalizationService
    {
        private static Dictionary<string, string> _zh = new Dictionary<string, string>();
        private static Dictionary<string, string> _zhTw = new Dictionary<string, string>();
        private static Dictionary<string, string> _en = new Dictionary<string, string>();
        private static Dictionary<string, string> _ru = new Dictionary<string, string>();
        private static bool _loaded;

        /// <summary>当前 Mod 界面语言："zh" / "zh_tw" / "en" / "ru"（持久化于 config.json）。</summary>
        public static string CurrentLanguage => UnrestConfig.Instance.Language;

        /// <summary>是否为中文系界面（简/繁）。</summary>
        public static bool IsChinese => CurrentLanguage == "zh" || CurrentLanguage == "zh_tw";

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
