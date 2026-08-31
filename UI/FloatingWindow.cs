using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 悬浮窗公共基类：非模态窗口骨架（Canvas + 可拖拽标题栏 + 四边缩放 + 关闭按钮 + 滚动内容区）。
    /// EconomyHUD / EventWindow / RichListWindow 共用，消除三份重复实现。
    /// </summary>
    public abstract class FloatingWindow : MonoBehaviour
    {
        // ===== 子类配置（必须提供）=====
        protected abstract string WindowName { get; }        // GameObject 名（如 "EconomyHUD"）
        protected abstract float SortingOrder { get; }
        protected abstract string TitleKey { get; }          // UIHelpers.L 的键
        protected abstract Vector2 AnchorMin { get; }
        protected abstract Vector2 AnchorMax { get; }
        protected abstract Vector2 Pivot { get; }
        protected abstract Vector2 AnchoredPosition { get; }
        protected abstract Vector2 Size { get; }
        protected abstract Color BgColor { get; }
        protected virtual float Padding => 14f;
        protected virtual float TitleFontSize => 15f;
        protected virtual float TitleLineHeight => 26f;

        // ===== 共享状态（子类可直接访问）=====
        protected GameObject _panelRoot;
        protected RectTransform _panelRect;
        protected GameObject _content;
        protected readonly List<GameObject> _lines = new List<GameObject>();
        protected Font _gameFont;
        protected bool _visible;
        protected Text _titleText;

        protected static T CreateWindow<T>(string goName) where T : FloatingWindow
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Canvas),
                                      typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(go);
            return go.AddComponent<T>();
        }

        public void Toggle()
        {
            _visible = !_visible;
            if (_visible) RefreshNow();
            if (_panelRoot != null) _panelRoot.SetActive(_visible);
        }

        public void Show() { _visible = true; if (_panelRoot != null) { _panelRoot.SetActive(true); RefreshNow(); } }
        public void Hide() { _visible = false; if (_panelRoot != null) _panelRoot.SetActive(false); }
        public bool IsVisible => _visible;

        /// <summary>世界退出时隐藏窗口并销毁动态内容，释放按钮委托及其捕获对象。</summary>
        public virtual void OnWorldUnavailable()
        {
            Hide();
            ClearContent();
        }

        protected virtual void Awake()
        {
            _gameFont = LocalizedTextManager.current_font;
            if (_gameFont == null) _gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildPanel();
            _panelRoot.SetActive(false);
        }

        protected virtual void BuildPanel()
        {
            var canvas = GetComponent<Canvas>();
            UIHelpers.SetupCanvas(canvas, SortingOrder);
            _panelRect = UIHelpers.CreatePanelRoot(transform, WindowName + "Panel",
                AnchorMin, AnchorMax, Pivot, AnchoredPosition, Size, BgColor);
            _panelRoot = _panelRect.gameObject;
            UIHelpers.CreateDragArea(_panelRect, _panelRect, Padding + 36);
            _titleText = UIHelpers.CreateWindowTitle(_panelRect, UIHelpers.L(TitleKey), _gameFont,
                UIStyles.Gold, TitleFontSize, Padding, TitleLineHeight);
            UIHelpers.CreateResizeHandles(_panelRect, OnPanelResized);
            UIHelpers.CreateCloseButton(_panelRect, _gameFont, Hide);
            _content = UIHelpers.CreateScrollContent(_panelRect, Padding, Padding + 32f).gameObject;
        }

        /// <summary>
        /// 语言切换后刷新窗口文本。基类只重写标题（BuildPanel 仅在启动时执行一次，
        /// 标题不会随 RefreshNow 重建）；子类重写时先调 base 再刷各自的按钮/内容。
        /// </summary>
        public virtual void RefreshAllTexts()
        {
            if (_titleText != null) _titleText.text = UIHelpers.L(TitleKey);
        }

        /// <summary>缩放结束后回调（子类可重写以重建自适应内容）。</summary>
        protected virtual void OnPanelResized() { }

        /// <summary>重建内容（打开/刷新时调用；子类实现具体列表构建）。</summary>
        public abstract void RefreshNow();

        protected virtual void ClearContent()
        {
            foreach (var go in _lines) Destroy(go);
            _lines.Clear();
        }

        protected void AddLine(string text, Color color, float size)
        {
            var go = UIHelpers.CreateText(text, _content.transform, size, color, _gameFont, 22f);
            _lines.Add(go);
        }

        protected void AddDivider(Color color)
        {
            _lines.Add(UIHelpers.CreateDivider(_content.transform, color));
        }
    }
}
