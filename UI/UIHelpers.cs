using UnityEngine;
using UnityEngine.UI;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    /// <summary>
    /// 悬浮窗公共 UI 辅助：圆角 Sprite 缓存、缩放手柄、文本/按钮/分割线创建、本地化快捷方法。
    /// EconomyHUD / EventWindow / RichListWindow 共用，消除三份重复实现。
    /// </summary>
    internal static class UIHelpers
    {
        // ===== 圆角矩形 Sprite（静态缓存，全模组共用一份）=====

        private static Sprite _roundedSprite;

        /// <summary>获取共享的 9-slice 圆角白色 Sprite（着色由 Image.color 控制）。</summary>
        public static Sprite RoundedSprite()
        {
            if (_roundedSprite != null) return _roundedSprite;
            const int s = 24, r = 6;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[s * s];
            for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
            for (int x = 0; x < s; x++)
                for (int y = 0; y < s; y++)
                {
                    bool fill = true;
                    if (x < r && y < r) { int dx = r - 1 - x, dy = r - 1 - y; fill = dx * dx + dy * dy <= r * r; }
                    else if (x >= s - r && y < r) { int dx = x - (s - r), dy = r - 1 - y; fill = dx * dx + dy * dy <= r * r; }
                    else if (x < r && y >= s - r) { int dx = r - 1 - x, dy = y - (s - r); fill = dx * dx + dy * dy <= r * r; }
                    else if (x >= s - r && y >= s - r) { int dx = x - (s - r), dy = y - (s - r); fill = dx * dx + dy * dy <= r * r; }
                    if (fill) px[y * s + x] = Color.white;
                }
            tex.SetPixels(px);
            tex.Apply();
            _roundedSprite = Sprite.Create(tex, new Rect(0, 0, s, s),
                new Vector2(0.5f, 0.5f), s, 0, SpriteMeshType.FullRect,
                new Vector4(r, r, r, r));
            return _roundedSprite;
        }

        // ===== 缩放手柄 =====

        /// <summary>在目标 RectTransform 四边 + 四角创建 8 个透明缩放手柄。</summary>
        public static void CreateResizeHandles(RectTransform target, System.Action onResizeEnded = null)
        {
            CreateHandle(target, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(-1, 0), onResizeEnded);   // 左
            CreateHandle(target, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(8, 0), new Vector2(1, 0), onResizeEnded);    // 右
            CreateHandle(target, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 8), new Vector2(0, 1), onResizeEnded);    // 上
            CreateHandle(target, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(0, -1), onResizeEnded);   // 下
            CreateHandle(target, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, 16), new Vector2(-1, 1), onResizeEnded);    // 左上
            CreateHandle(target, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(16, 16), new Vector2(1, 1), onResizeEnded);     // 右上
            CreateHandle(target, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(16, 16), new Vector2(-1, -1), onResizeEnded);   // 左下
            CreateHandle(target, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(16, 16), new Vector2(1, -1), onResizeEnded);    // 右下
        }

        private static void CreateHandle(RectTransform target, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 size, Vector2 dir, System.Action onResizeEnded)
        {
            // 关键：手柄作为 target 的子物体，锚点相对于 target 设置，确保始终贴合窗口边缘
            // （若放在 target.parent，中心锚点窗口会导致手柄漂移到屏幕错误位置）
            var go = new GameObject("ResizeHandle", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(target, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.01f); // 近透明但仍可接收射线
            img.raycastTarget = true;
            // 置于最顶层，避免被其他子物体（图表/文本/按钮）遮挡导致 raycast 失效
            rt.SetAsLastSibling();
            var handler = go.AddComponent<UIResizeHandler>();
            handler.Init(target, dir);
            handler.OnResizeEnded = onResizeEnded;
        }

        // ===== 文本 / 按钮 / 分割线 =====

        /// <summary>创建一行文本 GameObject（已挂 Text + RectTransform，不设置布局）。</summary>
        public static GameObject CreateText(string content, Transform parent, float fontSize, Color color,
            Font font, float lineHeight = 22f, string name = "Text")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.font = font;
            t.fontSize = Mathf.RoundToInt(fontSize);
            t.color = color;
            t.alignment = TextAnchor.UpperLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, lineHeight);
            return go;
        }

        /// <summary>创建带 Image + Button 的按钮 GameObject（含 Label 子物体）。</summary>
        public static Button CreateButton(string label, Transform parent, float w, float h,
            Font font, Color bg, float labelSize = 12f)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = bg;
            var btnArt = UIArt.Get("button_9slice");
            img.sprite = btnArt != null ? btnArt : RoundedSprite();
            img.type = Image.Type.Sliced;
            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;

            var rt = go.GetComponent<RectTransform>();
            if (w > 0) rt.sizeDelta = new Vector2(w, h);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var lt = labelGo.GetComponent<Text>();
            lt.text = label;
            lt.font = font;
            lt.fontSize = Mathf.RoundToInt(labelSize);
            lt.color = Color.white;
            lt.alignment = TextAnchor.MiddleCenter;
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            return btn;
        }

        /// <summary>创建 1px 高的分割线 Image（不自适应布局，由调用者设置 sizeDelta）。</summary>
        public static GameObject CreateDivider(Transform parent, Color color)
        {
            var go = new GameObject("DivLine", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 1);
            go.GetComponent<Image>().color = color;
            return go;
        }

        // ===== 浮动窗口骨架（EconomyHUD / RichListWindow / EventWindow 共用，消除 BuildPanel 重复）=====

        /// <summary>配置 Canvas：Overlay 渲染 + 排序层级 + 参考分辨率缩放。</summary>
        public static void SetupCanvas(Canvas canvas, float sortingOrder)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = (int)sortingOrder;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        /// <summary>创建浮动窗口面板根节点（圆角背景 Image），返回 RectTransform。</summary>
        public static RectTransform CreatePanelRoot(Transform canvasTransform, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition,
            Vector2 size, Color bgColor)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvasTransform, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
            var bg = root.GetComponent<Image>();
            var art = UIArt.Get("panel_9slice");
            if (art != null)
            {
                // 石板金边底图：tint 白（保留底图本色），alpha 跟随窗口的不透明度
                bg.sprite = art;
                bg.color = new Color(1f, 1f, 1f, Mathf.Max(0.9f, bgColor.a));
                bg.type = Image.Type.Sliced;
            }
            else
            {
                bg.color = bgColor;
                bg.sprite = RoundedSprite();
                bg.type = Image.Type.Sliced;
            }
            return rt;
        }

        /// <summary>创建标题栏拖拽区（透明 Image，先于关闭按钮创建保证 × 始终可点）。</summary>
        public static void CreateDragArea(RectTransform panelRect, RectTransform target, float height)
        {
            var dragArea = new GameObject("DragArea", typeof(RectTransform), typeof(Image));
            dragArea.transform.SetParent(panelRect, false);
            var dragRt = dragArea.GetComponent<RectTransform>();
            dragRt.anchorMin = new Vector2(0, 1); dragRt.anchorMax = new Vector2(1, 1);
            dragRt.pivot = new Vector2(0.5f, 1);
            dragRt.anchoredPosition = Vector2.zero;
            dragRt.sizeDelta = new Vector2(0, height);
            var dragImg = dragArea.GetComponent<Image>();
            dragImg.color = new Color(0, 0, 0, 0);
            dragImg.raycastTarget = true;
            dragArea.AddComponent<UIDragHandler>().Init(target);
        }

        /// <summary>创建标题文本（顶部居中加粗），返回 Text 引用。</summary>
        public static Text CreateWindowTitle(RectTransform panelRect, string text, Font font,
            Color color, float fontSize, float padding, float lineHeight)
        {
            var go = CreateText(text, panelRect, fontSize, color, font, lineHeight, "Title");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -padding);
            rt.sizeDelta = new Vector2(-padding * 2, lineHeight);
            var t = go.GetComponent<Text>();
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;
            return t;
        }

        /// <summary>创建右上角红色关闭按钮。</summary>
        public static Button CreateCloseButton(RectTransform panelRect, Font font, UnityEngine.Events.UnityAction onClick)
        {
            var btn = CreateButton("X", panelRect, 26f, 26f, font, new Color(0.8f, 0.2f, 0.2f, 0.85f), 14f);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1); rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-8, -8);
            btn.onClick.AddListener(onClick);
            return btn;
        }

        /// <summary>
        /// 创建滚动内容区（ScrollRect + Mask + VerticalLayoutGroup + ContentSizeFitter），
        /// 返回 Content 的 RectTransform（调用方自行 AddLine 挂子物体）。
        /// </summary>
        public static RectTransform CreateScrollContent(RectTransform panelRect, float padding, float topInset)
        {
            var scrollObj = new GameObject("Scroll", typeof(RectTransform), typeof(Image),
                typeof(ScrollRect), typeof(Mask));
            scrollObj.transform.SetParent(panelRect, false);
            var scrollRt = scrollObj.GetComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero; scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(padding, padding);
            scrollRt.offsetMax = new Vector2(-padding, -topInset);
            scrollObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.18f);

            var content = new GameObject("Content", typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(scrollObj.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            var csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var sr = scrollObj.GetComponent<ScrollRect>();
            sr.content = contentRt;
            sr.viewport = scrollRt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 20f;
            scrollObj.GetComponent<Mask>().showMaskGraphic = false;
            return contentRt;
        }

        // ===== 本地化快捷方法 =====

        public static string L(string key) => LocalizationService.Get(key);

        public static string Lf(string key, params object[] args)
        {
            try { return string.Format(L(key), args); }
            catch (System.Exception) { return key; }
        }
    }
}
