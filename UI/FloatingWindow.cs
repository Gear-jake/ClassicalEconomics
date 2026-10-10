using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
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

        /// <summary>本体皮肤下「标题栏带」的高度 = 窗口顶部边框厚度；自绘皮肤为 0。
        ///   · 标题应**居中在这条带子里**（而不是被推到带子下方）；
        ///   · 带子之后才是页签/正文内容区。
        /// 实机两次反馈"标题位置不对"，根因都是把"内容避让量"错加到了标题上。</summary>
        protected float TitleBand => UseVanillaSkin ? VanillaSkin.TopInset : 0f;

        /// <summary>标题垂直位置（CreateWindowTitle 的 padding 参数，向下为正）。
        /// 本体皮肤：在标题栏带内垂直居中；自绘皮肤：沿用 Padding。</summary>
        protected virtual float TitleTopOffset
        {
            get
            {
                float band = TitleBand;
                if (band <= 1f) return Padding;
                return Mathf.Max(2f, (band - Fs(TitleLineHeight)) * 0.5f);
            }
        }

        /// <summary>标题栏带之后的第一个内容 y 偏移（页签/正文的起点）。
        /// 本体皮肤取「带高 + 6」，自绘皮肤沿用原来的 Padding + 32。</summary>
        protected float BelowTitleOffset => Mathf.Max(Padding + 32f, TitleBand + 6f);

        /// <summary>【W2 PoC】是否套用本体窗口皮肤（底板/关闭按钮取自游戏自带窗口预制体）。
        /// 与本体窗口的区别：我们仍是自建 Canvas 悬浮窗，**不注册为 ScrollWindow，因此不会暂停游戏**。</summary>
        protected virtual bool UseVanillaSkin => false;

        /// <summary>子类可返回自带底图（9-slice）。非 null 时**优先于**本体皮肤与自绘圆角。
        /// 用途：报纸风等非本体风格的窗口（素材见 GameResources/ui/panels/）。</summary>
        protected virtual Sprite CustomPanelSprite => null;

        /// <summary>标题文字颜色（子类可覆盖，如报纸风用墨色/暗红）。</summary>
        protected virtual Color TitleColor => UIStyles.Gold;

        /// <summary>标题下划线颜色（自绘皮肤下才绘制；子类可覆盖）。</summary>
        protected virtual Color TitleRuleColor => UIStyles.AccentGold;

        /// <summary>本体皮肤下内容的顶部避让量（0 = 自绘皮肤不避让）。</summary>
        protected float SkinTopInset  => UseVanillaSkin ? VanillaSkin.TopInset  : 0f;
        /// <summary>本体皮肤下内容的左右避让量。</summary>
        protected float SkinSideInset => UseVanillaSkin ? VanillaSkin.SideInset : 0f;
        /// <summary>本体皮肤下内容的底部避让量。</summary>
        protected float SkinBottomInset => UseVanillaSkin ? VanillaSkin.BottomInset : 0f;

        // ===== 共享状态（子类可直接访问）=====
        protected GameObject _panelRoot;
        protected RectTransform _panelRect;
        protected GameObject _content;
        protected readonly List<GameObject> _lines = new List<GameObject>();
        /// <summary>【W4-UI 池化】回收行对象，避免每次刷新销毁重建（原实现每年销毁重建约 250 个 GO）。</summary>
        protected readonly List<GameObject> _linePool = new List<GameObject>();
        private const int LinePoolCap = 512;
        protected Font _gameFont;
        protected bool _visible;
        protected Text _titleText;
        protected Button _closeButton;
        private float _contentTopInset;    // 内容区顶部内缩（供自适应高度换算）
        private float _skinInset;      // 本体窗口顶部边框厚度（UI 单位）
        private float _skinInsetSide;  // 本体窗口左右边框厚度（UI 单位）

        protected static T CreateWindow<T>(string goName) where T : FloatingWindow
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Canvas),
                                      typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(go);
            return go.AddComponent<T>();
        }

        public virtual void Toggle()
        {
            bool was = _visible;
            _visible = !_visible;
            if (_visible) RefreshNow();
            if (_panelRoot != null) _panelRoot.SetActive(_visible);
            OnVisibilityChanged(_visible);
            NotifyPauseCoordinator(was, _visible);
        }

        public virtual void Show()
        {
            bool was = _visible;
            _visible = true;
            if (_panelRoot != null) { _panelRoot.SetActive(true); if (UseVanillaSkin) ApplyVanillaSkin(); RefreshNow(); }
            OnVisibilityChanged(true);
            NotifyPauseCoordinator(was, true);
        }

        public virtual void Hide()
        {
            bool was = _visible;
            _visible = false;
            if (_panelRoot != null) _panelRoot.SetActive(false);
            OnVisibilityChanged(false);
            NotifyPauseCoordinator(was, false);
        }

        /// <summary>【W2.8】仅在**可见性真正发生变化**时通知暂停协调器：
        /// 重复 Show()/Hide() 不得把计数打歪（否则会出现"窗口关了游戏还停着"）。</summary>
        private static void NotifyPauseCoordinator(bool wasVisible, bool nowVisible)
        {
            if (wasVisible == nowVisible) return;
            UIPauseCoordinator.SetVisible(nowVisible);
        }

        /// <summary>可见性变化钩子：子类在此挂遮罩/随窗资源（关闭按钮走基类 Hide，必须经此钩子）。</summary>
        protected virtual void OnVisibilityChanged(bool visible) { }

        public bool IsVisible => _visible;

        /// <summary>世界退出时隐藏窗口并销毁动态内容，释放按钮委托及其捕获对象。</summary>
        public virtual void OnWorldUnavailable()
        {
            Hide();
            ClearContent();
            // 【W2.8】世界退出是"最后一道保险"：无条件归还由本模组造成的暂停，
            // 不依赖计数是否归零（Hide 已减过计数，此处幂等）。
            UIPauseCoordinator.ResetAll();
        }

        protected virtual void Awake()
        {
            _gameFont = LocalizedTextManager.current_font;
            if (_gameFont == null) _gameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildPanel();
            _panelRoot.SetActive(false);
        }

        /// <summary>UI 整体缩放（设置页 ui_scale，0.8~1.6，默认 1.2）：全部悬浮窗字体/行高统一乘此系数。</summary>
        protected static float Fs(float size)
        {
            var cfg = UnrestConfig.Instance;
            float scale = cfg != null ? cfg.UiScale : 1.2f;
            return size * Mathf.Clamp(scale, 0.8f, 1.6f);
        }

        protected virtual void BuildPanel()
        {
            var canvas = GetComponent<Canvas>();
            UIHelpers.SetupCanvas(canvas, SortingOrder);
            _panelRect = UIHelpers.CreatePanelRoot(transform, WindowName + "Panel",
                AnchorMin, AnchorMax, Pivot, AnchoredPosition, Size, BgColor);
            _panelRoot = _panelRect.gameObject;
            // 优先级：自定义底图（报纸风等） > 本体皮肤 > 自绘圆角
            var customPanel = CustomPanelSprite;
            if (customPanel != null)
            {
                var cimg = _panelRoot.GetComponent<Image>();
                if (cimg != null)
                {
                    cimg.sprite = customPanel;
                    cimg.type = Image.Type.Sliced;
                    cimg.color = Color.white;
                    cimg.pixelsPerUnitMultiplier = 1f;   // 自带素材按原生像素渲染（ppu=100 与本画布一致）
                }
            }
            else if (UseVanillaSkin) ApplyVanillaSkin();
            UIHelpers.CreateDragArea(_panelRect, _panelRect, Padding + 36);
            _titleText = UIHelpers.CreateWindowTitle(_panelRect, UIHelpers.L(TitleKey), _gameFont,
                // 标题位置**只用 TitleTopOffset**（它已包含标题栏带的对齐）；
                // 绝不能再加 SkinTopInset —— 那是给内容避让用的，加到标题上会把标题推到带子下面。
                TitleColor, Fs(TitleFontSize), TitleTopOffset, Fs(TitleLineHeight));
            // 标题栏金色下划线（装饰）—— 本体皮肤下本体已有标题栏，不再自绘（否则线会跑出框外）
            if (!UseVanillaSkin)
            {
            var titleLine = new GameObject("TitleUnderline", typeof(RectTransform), typeof(Image));
            titleLine.transform.SetParent(_panelRect, false);
            var tlRt = titleLine.GetComponent<RectTransform>();
            tlRt.anchorMin = new Vector2(0, 1); tlRt.anchorMax = new Vector2(1, 1);
            tlRt.pivot = new Vector2(0.5f, 1);
            tlRt.anchoredPosition = new Vector2(0, -(Padding + Fs(TitleLineHeight)));
            tlRt.sizeDelta = new Vector2(-Padding * 2, 1.5f);
            titleLine.GetComponent<Image>().color = TitleRuleColor;
            titleLine.GetComponent<Image>().raycastTarget = false;
            }
            // 面板内边框（细金描边）—— 本体皮肤下由本体边框自带，不再叠加
            if (!UseVanillaSkin) UIHelpers.CreateInnerBorder(_panelRect, UIStyles.InnerBorder, 1.5f);
            UIHelpers.CreateResizeHandles(_panelRect, OnPanelResized);
            _closeButton = UIHelpers.CreateCloseButton(_panelRect, _gameFont, Hide);
            // 本体皮肤：关闭按钮需避让厚边框（实测原位置会落在边框之外）
            if (UseVanillaSkin && _closeButton != null)
            {
                var crt = _closeButton.GetComponent<RectTransform>();
                // 关闭按钮落在标题栏带内：横向避让边框，纵向与标题居中对齐
                if (crt != null) crt.anchoredPosition -= new Vector2(SkinSideInset, Mathf.Max(0f, (TitleBand - 26f) * 0.5f));
            }
            // 【实机反馈修正】左右不再额外避让：原先每边多占 ~43，内容只剩窗口宽度的一半（用户反馈"太小"）。
            // 本体边框的左右已由 Padding 覆盖，只有顶部标题栏必须避让。
            _contentTopInset = BelowTitleOffset;
            _content = UIHelpers.CreateScrollContent(_panelRect, Padding, _contentTopInset, UseVanillaSkin, SkinBottomInset).gameObject;
        }

        /// <summary>
        /// 语言切换后刷新窗口文本。基类只重写标题（BuildPanel 仅在启动时执行一次，
        /// 标题不会随 RefreshNow 重建）；子类重写时先调 base 再刷各自的按钮/内容。
        /// </summary>
        public virtual void RefreshAllTexts()
        {
            if (_titleText != null) _titleText.text = UIHelpers.L(TitleKey);
        }

        /// <summary>套用本体窗口皮肤：底板 9-slice + 关闭按钮图标。
        /// 任一元素取不到就沿用原外观 —— 皮肤是纯装饰，不得影响可用性。</summary>
        protected void ApplyVanillaSkin()
        {
            try
            {
                var bg = VanillaSkin.PanelSprite;
                if (bg != null)
                {
                    var img = _panelRoot.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = bg;
                        img.type = VanillaSkin.PanelType;
                        img.color = Color.white;
                        // 【关键修正】本体 UI 精灵的 pixelsPerUnit = 1（我们自绘的是 100），
                        // 而本 Canvas 用默认 referencePixelsPerUnit = 100 —— 不补偿的话
                        // 9-slice 边框会被放大 100 倍（6px 边框 → 600px），远超面板尺寸，
                        // 表现为几层错位的色块（实机截图已证实）。
                        // 补偿公式：pixelsPerUnitMultiplier = referencePPU / spritePPU。
                        var scaler = GetComponent<CanvasScaler>();
                        float refPpu = (scaler != null && scaler.referencePixelsPerUnit > 0f)
                            ? scaler.referencePixelsPerUnit : 100f;
                        // 【撤回边框缩放，恢复原生边框】
                        // 教训：windowBig 的装饰（城堡垛口）就长在 99~126px 的边框带里 ——
                        // 把边框压到 25px 等于把装饰一起压扁，窗口会退化成一个扁平色块（实机反馈"还是丑"）。
                        // 正确做法是**保持原生边框**（图片像素:UI 单位 = 1:1），让装饰完整呈现。
                        float spritePpu = Mathf.Max(1f, bg.pixelsPerUnit);

                        // 【边框厚度】默认值由**实机视觉对比**确定（在无头截图对比中试了 100/40/20/12/10/5）：
                        //   100 → 边框约 12 UI 单位，窗口退化成扁平色块；
                        //    20 → 边框约 55~60 UI 单位，本体窗口的垛口/角饰完整呈现且不挤压内容（选定）；
                        //     5 → 边框过大，内容区被挤扁。
                        // 调参文件 ce_ui_tune.txt 存在时覆盖此值（仅调试用）。
                        img.pixelsPerUnitMultiplier = VanillaSkin.PanelBorderMultiplier;
                        // 有效边框厚度（UI 单位）= 精灵边框 × refPPU ÷ (精灵PPU × multiplier)
                        float eff = img.pixelsPerUnitMultiplier;
                        _skinInset = eff > 0f ? (VanillaSkin.PanelBorder.w * refPpu / (spritePpu * eff)) : 0f;
                        _skinInsetSide = eff > 0f ? (VanillaSkin.PanelBorder.x * refPpu / (spritePpu * eff)) : 0f;
                        Debug.Log(string.Format("[ClassicalEconomics][SkinApply] 面板 {0}x{1} 原生边框 {2} → multiplier {3:F1}",
                            (int)Size.x, (int)Size.y, VanillaSkin.PanelBorder, img.pixelsPerUnitMultiplier));
                    }
                }
                var close = VanillaSkin.CloseSprite;
                if (close != null && _closeButton != null)
                {
                    var ci = _closeButton.GetComponentInChildren<Image>();
                    if (ci != null) { ci.sprite = close; ci.color = Color.white; }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>【布局修复·通用】是否按内容自适应窗口高度。
        /// 默认关：长列表窗口（事件流/HUD/内阁）应保持固定高度 + 滚动；
        /// 短卡片窗口（富豪榜 Top10 / 抉择事件）开启后**一次性完整显示**，不出现"最后一行被框裁掉"。</summary>
        protected virtual bool AutoFitHeight => false;
        protected virtual float MinAutoHeight => 240f;
        protected virtual float MaxAutoHeight => 880f;

        /// <summary>按当前内容高度自适应窗口高度（在 RefreshNow 末尾调用，且仅当 AutoFitHeight 为真）。</summary>
        protected void FitPanelToContent()
        {
            if (!AutoFitHeight) return;
            try
            {
                if (_content == null || _panelRect == null) return;
                var crt = _content.GetComponent<RectTransform>();
                if (crt == null) return;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(crt);
                // 窗口总高 = 内容高 + 顶部起点 + 底部内边距 + **底部边框避让**
                // （最初漏了 SkinBottomInset，窗口矮了约 55 -> 最后一行被框裁掉）
                float want = Mathf.Clamp(
                    crt.rect.height + _contentTopInset + Padding + SkinBottomInset,
                    MinAutoHeight, MaxAutoHeight);
                if (Mathf.Abs(_panelRect.sizeDelta.y - want) > 1f)
                    _panelRect.sizeDelta = new Vector2(_panelRect.sizeDelta.x, want);
            }
            catch (System.Exception) { }
        }

        /// <summary>缩放结束后回调（子类可重写以重建自适应内容）。</summary>
        protected virtual void OnPanelResized() { }

        /// <summary>重建内容（打开/刷新时调用；子类实现具体列表构建）。</summary>
        public abstract void RefreshNow();

        protected virtual void ClearContent()
        {
            // 【W4-UI 池化】改为回收复用，不再 Destroy。
            // 注意：VerticalLayoutGroup 按 sibling index 排序，复用时必须 SetAsLastSibling 复位顺序。
            for (int i = 0; i < _lines.Count; i++)
            {
                var go = _lines[i];
                if (go == null) continue;
                if (_linePool.Count < LinePoolCap)
                {
                    go.SetActive(false);
                    _linePool.Add(go);
                }
                else
                {
                    PerfCounters.AddUiDestroyed(1);
                    Destroy(go);
                }
            }
            _lines.Clear();
        }

        /// <summary>从池中取一个指定名字的行对象；没有可用则返回 null。</summary>
        private GameObject RentLine(string name)
        {
            for (int i = _linePool.Count - 1; i >= 0; i--)
            {
                var go = _linePool[i];
                if (go == null) { _linePool.RemoveAt(i); continue; }
                if (go.name == name) { _linePool.RemoveAt(i); return go; }
            }
            return null;
        }

        protected void AddLine(string text, Color color, float size)
        {
            var go = RentLine("Line");
            if (go != null)
            {
                go.SetActive(true);
                go.transform.SetAsLastSibling();   // 复位布局顺序
                var tx = go.GetComponent<Text>();
                if (tx != null) { tx.text = text; tx.color = color; tx.font = _gameFont; }
                var rt = go.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(0, Fs(22f));
            }
            else
            {
                go = UIHelpers.CreateText(text, _content.transform, Fs(size), color, _gameFont, Fs(22f), "Line");
                PerfCounters.AddUiCreated(1);
            }
            _lines.Add(go);
        }

        protected void AddDivider(Color color)
        {
            var go = RentLine("Divider");
            if (go != null)
            {
                go.SetActive(true);
                go.transform.SetAsLastSibling();
                var img = go.GetComponent<Image>();
                if (img != null) img.color = color;
            }
            else
            {
                go = UIHelpers.CreateDivider(_content.transform, color);
                go.name = "Divider";
                PerfCounters.AddUiCreated(1);
            }
            _lines.Add(go);
        }
    }
}
