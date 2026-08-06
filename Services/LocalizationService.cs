using System.Collections.Generic;
using Newtonsoft.Json;
using EconomyMod.Models;

namespace EconomyMod.Services
{
    /// <summary>
    /// Mod 自建本地化服务：界面语言由 Mod 配置页切换（zh/en），
    /// 与游戏本体语言设置完全解耦（不依赖 LocalizedTextManager）。
    /// 运行时从 Locales/ch.json、en.json 动态加载。
    /// </summary>
    public static class LocalizationService
    {
        private static Dictionary<string, string> _zh = new Dictionary<string, string>();
        private static Dictionary<string, string> _en = new Dictionary<string, string>();
        private static bool _loaded;

        /// <summary>当前 Mod 界面语言："zh" 或 "en"（持久化于 config.json）。</summary>
        public static string CurrentLanguage => UnrestConfig.Instance.Language;

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
                string chPath = System.IO.Path.Combine(dir, "ch.json");
                string enPath = System.IO.Path.Combine(dir, "en.json");
                if (System.IO.File.Exists(chPath))
                    _zh = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(chPath)) ?? _zh;
                if (System.IO.File.Exists(enPath))
                    _en = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(enPath)) ?? _en;
            }
            catch (System.Exception) { }
        }

        /// <summary>取当前语言的文本；缺失时回退英文，仍缺失则返回 key 本身。</summary>
        public static string Get(string key)
        {
            EnsureLoaded();
            if (CurrentLanguage == "zh")
            {
                if (_zh.TryGetValue(key, out var z) && !string.IsNullOrEmpty(z)) return z;
            }
            if (_en.TryGetValue(key, out var e) && !string.IsNullOrEmpty(e)) return e;
            return key;
        }
    }
}
