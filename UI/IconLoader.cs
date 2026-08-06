using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EconomyMod.UI
{
    /// <summary>
    /// 图标加载器：从 mod 根目录 Icons/ 加载 PNG 资源（替代运行时像素生成）。
    /// 资源目录 = ModDeclare.FolderPath/Icons；加载失败返回 null（按钮将无图标，不影响功能）。
    /// </summary>
    internal static class IconLoader
    {
        private static string _iconDir;
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        private static string IconDir
        {
            get
            {
                if (_iconDir != null) return _iconDir;
                try
                {
                    var decl = EconomyModMain.Instance?.GetDeclaration();
                    _iconDir = decl != null ? Path.Combine(decl.FolderPath, "Icons") : null;
                }
                catch (System.Exception) { _iconDir = null; }
                return _iconDir;
            }
        }

        /// <summary>加载（并缓存）指定名称的图标 Sprite；不存在或加载失败返回 null。</summary>
        public static Sprite Get(string name)
        {
            if (_cache.TryGetValue(name, out var s)) return s;
            try
            {
                string dir = IconDir;
                if (string.IsNullOrEmpty(dir)) return null;
                string path = Path.Combine(dir, name + ".png");
                if (!File.Exists(path)) return null;
                var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(File.ReadAllBytes(path))) return null;
                var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f));
                _cache[name] = spr;
                return spr;
            }
            catch (System.Exception) { return null; }
        }
    }
}
