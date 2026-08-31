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
        // L1：失败缓存——加载失败的名字直接记入，避免每次调用都重复读文件 + 重复创建纹理
        private static readonly HashSet<string> _failed = new HashSet<string>();

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
            if (_failed.Contains(name)) return null; // L1：失败已缓存，直接跳过
            Texture2D tex = null;
            try
            {
                string dir = IconDir;
                if (string.IsNullOrEmpty(dir)) { _failed.Add(name); return null; }
                string path = Path.Combine(dir, name + ".png");
                if (!File.Exists(path)) { _failed.Add(name); return null; }
                tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(File.ReadAllBytes(path)))
                {
                    // L1：LoadImage 失败（文件损坏/非 PNG）时销毁已创建的纹理，避免 GPU 纹理泄漏
                    UnityEngine.Object.Destroy(tex);
                    tex = null;
                    _failed.Add(name);
                    return null;
                }
                var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f));
                _cache[name] = spr;
                return spr;
            }
            catch (System.Exception)
            {
                if (tex != null) UnityEngine.Object.Destroy(tex); // L1：异常路径同样销毁纹理
                _failed.Add(name);
                return null;
            }
        }

    }
}
