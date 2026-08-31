using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EconomyMod.UI
{
    /// <summary>
    /// UI 底图加载器：从 mod 根目录 GameResources/ui/panels/ 加载程序化绘制的 9-slice 素材
    /// （石板金边面板 / 按钮底 / 页签 / 图标徽章），替换纯色半透明底。
    /// 与 IconLoader 同模式：缓存 + 失败集（L1），加载失败回退旧圆角 Sprite，不影响功能。
    /// </summary>
    internal static class UIArt
    {
        private static string _dir;
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
        private static readonly HashSet<string> _failed = new HashSet<string>();

        private static string ArtDir
        {
            get
            {
                if (_dir != null) return _dir;
                try
                {
                    var decl = EconomyModMain.Instance?.GetDeclaration();
                    _dir = decl != null ? Path.Combine(decl.FolderPath, "GameResources", "ui", "panels") : null;
                }
                catch (System.Exception) { _dir = null; }
                return _dir;
            }
        }

        /// <summary>按文件名加载 9-slice Sprite（border 16px，正素材为 64~96px 圆角石板）。失败返回 null。</summary>
        public static Sprite Get(string name)
        {
            if (_cache.TryGetValue(name, out var s)) return s;
            if (_failed.Contains(name)) return null;
            Texture2D tex = null;
            try
            {
                string dir = ArtDir;
                if (string.IsNullOrEmpty(dir)) { _failed.Add(name); return null; }
                string path = Path.Combine(dir, name + ".png");
                if (!File.Exists(path)) { _failed.Add(name); return null; }
                tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(File.ReadAllBytes(path)))
                {
                    Object.Destroy(tex);
                    tex = null;
                    _failed.Add(name);
                    return null;
                }
                var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                    new Vector4(16f, 16f, 16f, 16f)); // 9-slice border：四边各 16px
                _cache[name] = spr;
                return spr;
            }
            catch (System.Exception)
            {
                if (tex != null) Object.Destroy(tex);
                _failed.Add(name);
                return null;
            }
        }
    }
}
