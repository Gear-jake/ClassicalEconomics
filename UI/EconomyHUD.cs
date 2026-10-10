using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    public partial class EconomyHUD : FloatingWindow
    {
        private static EconomyHUD _instance;

        private RectTransform _contentRect;

        // ===== 【W2.6】纯文本行池（本窗口刷新频繁，行对象必须复用）=====
        private const string HudTextName = "HudText";
        private const int TextPoolCap = 160;
        private readonly List<GameObject> _textPool = new List<GameObject>(64);

        private Button _btnOverview, _btnChart;
        private Image _btnOverviewImg, _btnChartImg;

        // 图表网格内边距（UI 单位，与 ChartMeshGraphic.margin 及刻度 Text 对齐）
        private const float ChartUiMargin = 4f;

        private enum Section { Overview, Chart }
        private Section _currentSection = Section.Overview;

        private static readonly Color Bg           = UIStyles.PanelBg;
        private static readonly Color BtnNormal    = UIStyles.CardBgAlt;
        private static readonly Color BtnActive    = UIStyles.Gold;
        private static readonly Color TextColor    = UIStyles.TextPrimary;
        private static readonly Color HeaderColor  = UIStyles.Gold;
        private static readonly Color DividerColor = UIStyles.Divider;

        private const float PanelWidth = UIStyles.HudWidth;
        private const float PanelHeight = UIStyles.HudHeight;
        private const float LineHeight = 22f;
        private const float HeaderSize = 14f;
        private const float TextSize = 12f;
        private const float BtnHeight = 28f;

        public static EconomyHUD Instance => _instance;

        // ===== FloatingWindow 配置 =====
        protected override string WindowName => "EconomyHUD";
        protected override float SortingOrder => 999;
        protected override string TitleKey => "economy_title";
        protected override Vector2 AnchorMin => new Vector2(0f, 0.5f);
        protected override Vector2 AnchorMax => new Vector2(0f, 0.5f);
        protected override Vector2 Pivot => new Vector2(0f, 0.5f);
        protected override Vector2 AnchoredPosition => new Vector2(12f, 0f);
        protected override Vector2 Size => new Vector2(PanelWidth, PanelHeight);
        protected override Color BgColor => Bg;
        /// <summary>【W2】套用本体窗口皮肤（不注册为 ScrollWindow，游戏不暂停）。</summary>
        protected override bool UseVanillaSkin => true;
        protected override float Padding => 16f;
        protected override float TitleFontSize => 14f;
        protected override float TitleLineHeight => 28f;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<EconomyHUD>("EconomyHUD");
        }

        public override void OnWorldUnavailable()
        {
            base.OnWorldUnavailable();
            ClearWorldCaches();
            ReleaseChartHosts();   // 【W2.6】常驻图表宿主跨世界复用：只收起，不销毁
            _currentSection = Section.Overview;
            UpdateTabButtons();
        }

        private static void ClearWorldCaches()
        {
            for (int i = 0; i < _seriesPool.Count; i++) _seriesPool[i].Values.Clear();
            for (int i = 0; i < _chartSeriesEntryPool.Count; i++) _chartSeriesEntryPool[i].Values.Clear();
            _seriesPool.Clear();
            _chartSeriesEntryPool.Clear();
            _rankBuf.Clear();
            _dynIndex.Clear();
            _dynLastSeen.Clear();
            _dynSeenBuf.Clear();
            _keepIds.Clear();
        }

        /// <summary>
        /// EconomyHUD 有 Tab 栏 + 多 Section，不能直接复用基类 BuildPanel 的默认 topInset，
        /// 因此自行完整实现：基类骨架（面板/拖拽/标题/缩放手柄/关闭按钮）+ TabBar + 更大的内容区 topInset。
        /// </summary>
        protected override void BuildPanel()
        {
            var canvas = GetComponent<Canvas>();
            UIHelpers.SetupCanvas(canvas, SortingOrder);

            // 面板主体（默认悬浮屏幕左侧中部，可拖拽移动）
            _panelRect = UIHelpers.CreatePanelRoot(transform, WindowName + "Panel",
                AnchorMin, AnchorMax, Pivot, AnchoredPosition, Size, BgColor);
            _panelRoot = _panelRect.gameObject;
            // 【W2 关键修复】EconomyHUD 重写了 BuildPanel，原先从不套皮肤 ——
            // 皮肤只在 Show() 时才应用，而那时布局已按"无边框"偏移算完，
            // 导致标题/页签/内容全部压在厚边框上。这里必须先套皮肤再布局。
            if (UseVanillaSkin) ApplyVanillaSkin();

            // 标题栏拖拽 + 标题 + 缩放手柄（拖拽结束后重建当前页内容）+ 关闭按钮（公共骨架）
            UIHelpers.CreateDragArea(_panelRect, _panelRect, Padding + 36);
            _titleText = UIHelpers.CreateWindowTitle(_panelRect, UIHelpers.L(TitleKey),
                // 标题只用 TitleTopOffset（本体皮肤下=在标题栏带内居中）
                _gameFont, HeaderColor, TitleFontSize, TitleTopOffset, TitleLineHeight);
            UIHelpers.CreateResizeHandles(_panelRect, OnPanelResized);
            var closeBtn = UIHelpers.CreateCloseButton(_panelRect, _gameFont, Hide);
            // 本体皮肤：关闭按钮避让厚边框
            if (UseVanillaSkin && closeBtn != null)
            {
                var crt = closeBtn.GetComponent<RectTransform>();
                if (crt != null) crt.anchoredPosition -= new Vector2(SkinSideInset, SkinTopInset * 0.6f);
            }

            // TabBar（EconomyHUD 独有：概览/趋势切换）
            var tabBar = new GameObject("TabBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabBar.transform.SetParent(_panelRoot.transform, false);
            var tabBarRt = tabBar.GetComponent<RectTransform>();
            tabBarRt.anchorMin = new Vector2(0, 1); tabBarRt.anchorMax = new Vector2(1, 1);
            tabBarRt.pivot = new Vector2(0.5f, 1f);
            tabBarRt.anchoredPosition = new Vector2(0, -BelowTitleOffset);
            tabBarRt.sizeDelta = new Vector2(-Padding * 2, BtnHeight);
            var hlg = tabBar.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true; hlg.childControlHeight = true;

            _btnOverview = CreateTabButton(UIHelpers.L("tab_overview"), tabBar.transform, out _btnOverviewImg);
            _btnChart = CreateTabButton(UIHelpers.L("tab_chart"), tabBar.transform, out _btnChartImg);
            _btnOverview.onClick.AddListener(() => SwitchSection(Section.Overview));
            _btnChart.onClick.AddListener(() => SwitchSection(Section.Chart));

            var divider = UIHelpers.CreateDivider(_panelRoot.transform, DividerColor);
            var divRt = divider.GetComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0, 1); divRt.anchorMax = new Vector2(1, 1);
            divRt.pivot = new Vector2(0.5f, 1);
            divRt.anchoredPosition = new Vector2(0, -(BelowTitleOffset + BtnHeight + 6));
            divRt.sizeDelta = new Vector2(-Padding * 2, 1);
            // 【W2.6 修正】这条分割线是**页签栏下的静态装饰**，BuildPanel 只跑一次。
            // 原先把它们注册进 _lines 会被每次刷新的 ClearContent 收走 → 第一次刷新后分割线就消失了。
            // 静态装饰不进 _lines（它不需要随内容清空）。

            // 滚动内容区（topInset 留出标题+TabBar+分割线空间）
            float topInset = BelowTitleOffset + BtnHeight + 12;
            _contentRect = UIHelpers.CreateScrollContent(_panelRect, Padding, topInset, false, SkinBottomInset);
            _content = _contentRect.gameObject;

            UpdateTabButtons();
        }

        /// <summary>
        /// 面板缩放结束后重建当前页内容（折线图按新面板宽度重绘）。
        /// </summary>
        protected override void OnPanelResized()
        {
            if (!_visible) return;
            RefreshCurrentSection();
        }

        private void SwitchSection(Section s)
        {
            _currentSection = s;
            UpdateTabButtons();
            RefreshCurrentSection();
        }

        /// <summary>【W2】套用页签外观：本体皮肤下用本体页签素材（选中/未选中），否则回退自绘圆角+底色。</summary>
        private static void ApplyTabVisual(Image img, bool selected)
        {
            if (img == null) return;
            // 【实机验证后调整】本体 tab_button_sort 是给 27x37 小方块按钮用的，
            // 拉宽成 ~140px 页签后会变成一条色块。改用本体**标准按钮**素材 + 着色区分选中态；
            // 九宫格拉伸下按钮素材在任意宽度都能保持正确边角。
            var btnArt = VanillaSkin.ButtonSprite;
            if (btnArt != null)
            {
                // 【实机验证后修正】本体 button 素材本身是暗色，着色只能变暗不能变亮 ——
                // 之前给选中态染橙 + 深色文字 → 文字糊在暗底上看不见。
                // 正解：保持素材原色，选中态改用**金色文字**标记（清晰且符合本体风格）。
                img.sprite = btnArt;
                img.type = Image.Type.Sliced;
                img.color = selected ? new Color(1f, 0.94f, 0.82f) : Color.white;
                img.pixelsPerUnitMultiplier = VanillaSkin.TabBorderMultiplier;
            }
            else
            {
                img.sprite = UIHelpers.RoundedSprite();
                img.type = Image.Type.Sliced;
                img.color = selected ? BtnActive : BtnNormal;
            }
        }

        private void UpdateTabButtons()
        {
            bool ov = _currentSection == Section.Overview;
            ApplyTabVisual(_btnOverviewImg, ov);
            ApplyTabVisual(_btnChartImg, !ov);
            var ot = _btnOverview.GetComponentInChildren<Text>();
            var ct = _btnChart.GetComponentInChildren<Text>();
            // 本体皮肤：选中态由本体素材区分，文字用金色强调；自绘皮肤沿用「黑字=选中」
            bool vanilla = VanillaSkin.ButtonSprite != null;
            // 本体素材下：选中态用金色文字标记（底色保持素材原色，保证可读）
            ot.color = vanilla ? (ov ? UIStyles.Gold : TextColor) : (ov ? Color.black : TextColor);
            ct.color = vanilla ? (!ov ? UIStyles.Gold : TextColor) : (!ov ? Color.black : TextColor);
        }

        public void RefreshCurrentSection()
        {
            ClearContent();
            // 非图表页：收起常驻图表宿主（不销毁，切回来直接用）
            if (_currentSection != Section.Chart) HideChartHosts();
            // 结算期标记：年度收尾在途时面板顶部固定提示行（完成后随刷新消失）
            if (AnnualPipeline.IsSettling)
            {
                AddLine(UIHelpers.L("settling_marker"), color: UIStyles.Warning);
                AddLine(UIHelpers.L("settling_hint"), color: UIStyles.Warning);
            }
            switch (_currentSection)
            {
                case Section.Overview: BuildOverview(); break;
                case Section.Chart:    BuildChart();    break;
            }
        }

        /// <summary>基类 Toggle/Show 通过 RefreshNow 触发刷新，等价于原实现按当前 Section 重建。</summary>
        public override void RefreshNow() => RefreshCurrentSection();

        /// <summary>
        /// 刷新所有静态文本（标题 + 标签页按钮 + 当前页内容）。
        /// 标题与标签页文本在 BuildPanel() 中只创建一次，语言切换后需整体重建，
        /// 否则只剩内容区变化而标题/标签页停留在旧语言。
        /// </summary>
        public override void RefreshAllTexts()
        {
            if (_panelRoot == null) return;
            base.RefreshAllTexts(); // 标题（基类 _titleText）
            if (_btnOverview != null) SetButtonText(_btnOverview, UIHelpers.L("tab_overview"));
            if (_btnChart != null) SetButtonText(_btnChart, UIHelpers.L("tab_chart"));
            UpdateTabButtons();
            if (_visible) RefreshCurrentSection();
        }

        private static void SetButtonText(Button btn, string text)
        {
            var t = btn.GetComponentInChildren<Text>();
            if (t != null) t.text = text;
        }

        protected override void ClearContent()
        {
            // 【W2.6 池化】本窗口的**纯文本行**回收进 _textPool 复用（原实现每次刷新都新建 ~40 个 Text）；
            // 复杂构件（统计卡/王国行/徽章/区块标题等，由 UIComponents 构建）仍随刷新销毁重建 ——
            // 它们内部结构各不相同，属 W4-UI 池化的后续范围，这里不假装已池化。
            for (int i = 0; i < _lines.Count; i++)
            {
                var go = _lines[i];
                if (go == null) continue;
                if (go.name == HudTextName && _textPool.Count < TextPoolCap)
                {
                    go.SetActive(false);
                    _textPool.Add(go);
                }
                else
                {
                    PerfCounters.AddUiDestroyed(1);
                    Destroy(go);
                }
            }
            _lines.Clear();
        }

        /// <summary>从文本行池取一个（没有则返回 null）。</summary>
        private GameObject RentHudText()
        {
            for (int i = _textPool.Count - 1; i >= 0; i--)
            {
                var go = _textPool[i];
                if (go == null) { _textPool.RemoveAt(i); continue; }
                _textPool.RemoveAt(i);
                return go;
            }
            return null;
        }

        /// <summary>内容行（带标题样式）。复用池中文本行，避免每次刷新新建 GameObject。</summary>
        private void AddLine(string text, bool isHeader = false, Color? color = null)
        {
            float size = Fs(isHeader ? HeaderSize : TextSize);
            float height = Fs(isHeader ? LineHeight + 4 : LineHeight);
            Color col = color ?? (isHeader ? HeaderColor : TextColor);
            var go = RentHudText();
            if (go != null)
            {
                go.SetActive(true);
                go.transform.SetAsLastSibling();   // VerticalLayoutGroup 按 sibling 排序，复用时必须复位
                var t = go.GetComponent<Text>();
                t.font = _gameFont;
                t.fontSize = Mathf.RoundToInt(size);
                t.fontStyle = isHeader ? FontStyle.Bold : FontStyle.Normal;
                t.color = col;
                t.text = text;
                go.GetComponent<RectTransform>().sizeDelta = new Vector2(0, height);
            }
            else
            {
                go = UIHelpers.CreateText(text, _content.transform,
                    size, col, _gameFont, height, HudTextName);
                if (isHeader) go.GetComponent<Text>().fontStyle = FontStyle.Bold;
                PerfCounters.AddUiCreated(1);
            }
            _lines.Add(go);
        }

        /// <summary>
        /// 趋势页：GDP 多国折线图 + 贫富差距趋势图。
        /// </summary>

        private Button CreateTabButton(string label, Transform parent, out Image img)
        {
            var btn = UIHelpers.CreateButton(label, parent, -1, BtnHeight, _gameFont, BtnNormal, HeaderSize);
            img = btn.GetComponent<Image>();
            ApplyTabVisual(img, false);
            var txt = btn.GetComponentInChildren<Text>();
            txt.font = _gameFont;
            txt.fontSize = Mathf.RoundToInt(HeaderSize);
            txt.color = TextColor;
            txt.alignment = TextAnchor.MiddleCenter;
            return btn;
        }
    }

    /// <summary>
    /// 图表悬停交互（借鉴 ECONOMYBOX CE 的 GDPMultiGraph）：透明接收层捕获鼠标，
    /// 把鼠标 x 归一化为数据索引（InverseLerp），实时更新悬停竖线与数值 Tooltip。
    /// 图表区域高度/宽度任意变化均自动适配（只依赖 rect 的 xMin/xMax）。
    /// </summary>
    public class ChartHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
    {
        private RectTransform _hoverRt;
        private RectTransform _lineRt;
        private RectTransform _tipRt;
        private Text _tipText;
        private int _pointCount;
        private int _lastIndex = -1;
        private System.Func<int, string> _textProvider;

        public void Init(RectTransform hoverRt, RectTransform lineRt, RectTransform tipRt, Text tipText,
            int pointCount, System.Func<int, string> textProvider)
        {
            _hoverRt = hoverRt;
            _lineRt = lineRt;
            _tipRt = tipRt;
            _tipText = tipText;
            _pointCount = pointCount;
            _textProvider = textProvider;
            _lastIndex = -1;
        }

        private void OnDestroy()
        {
            _textProvider = null;
            _hoverRt = null;
            _lineRt = null;
            _tipRt = null;
            _tipText = null;
        }

        /// <summary>【W2.6】刷新时只更新点数：数据源是复用缓冲（对象身份稳定），闭包无需重建。</summary>
        public void SetPointCount(int count)
        {
            _pointCount = count;
            _lastIndex = -1;
        }

        public void OnPointerEnter(PointerEventData eventData) { SetVisible(true); OnPointerMove(eventData); }

        public void OnPointerExit(PointerEventData eventData) => SetVisible(false);

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_hoverRt == null || _lineRt == null || _tipRt == null || _pointCount < 2) return;
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_hoverRt, eventData.position,
                    eventData.pressEventCamera, out localPoint))
                return;
            Rect rect = _hoverRt.rect;
            // CE 核心：局部坐标 → InverseLerp → 数据索引
            float normalized = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            int index = Mathf.Clamp(Mathf.RoundToInt(normalized * (_pointCount - 1)), 0, _pointCount - 1);

            // 竖线：anchor 在父左上角(pivot 0.5,1)，局部坐标原点在父中心(pivot 0.5,1)
            // 竖线 anchoredPosition.x = localPoint.x + rect.width/2（相对父左上角）
            _lineRt.anchoredPosition = new Vector2(localPoint.x + rect.width * 0.5f, 0f);

            // Tooltip 内容 + 定位（水平跟随鼠标并 clamp，垂直固定在图表顶部下方 6px）
            if (index != _lastIndex)
            {
                _lastIndex = index;
                if (_tipText != null && _textProvider != null)
                {
                    _tipText.text = _textProvider(index);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_tipRt); // 文本长度变化 → 刷新面板宽
                }
            }
            float tipW = _tipRt != null ? _tipRt.rect.width : 120f;
            float maxX = rect.width - tipW * 0.5f - 2f;
            float tipX = Mathf.Clamp(localPoint.x + rect.width * 0.5f, tipW * 0.5f + 2f, maxX);
            if (maxX < tipW * 0.5f + 2f) tipX = rect.width * 0.5f; // 面板太窄时居中
            _tipRt.anchoredPosition = new Vector2(tipX, -6f);
        }

        private void SetVisible(bool visible)
        {
            _lastIndex = -1;
            if (_lineRt != null) _lineRt.gameObject.SetActive(visible);
            if (_tipRt != null) _tipRt.gameObject.SetActive(visible);
        }
    }

    public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        private RectTransform _target;
        private Vector2 _startAnchoredPos;
        private Vector2 _startMousePos;

        public void Init(RectTransform target) => _target = target;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_target == null) return;
            _startAnchoredPos = _target.anchoredPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _target.parent as RectTransform, eventData.position,
                eventData.pressEventCamera, out _startMousePos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_target == null) return;
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _target.parent as RectTransform, eventData.position,
                eventData.pressEventCamera, out mousePos);
            _target.anchoredPosition = _startAnchoredPos + (mousePos - _startMousePos);
        }
    }

    /// <summary>
    /// 面板边缘/角拖拽缩放。
    /// dir.x：&gt;0 右边缘、&lt;0 左边缘；dir.y：&gt;0 上边缘、&lt;0 下边缘；0 表示该轴不缩放。
    /// 面板 pivot=(0, 0.5)，拖拽时固定对侧边缘，鼠标侧随动。
    /// </summary>
    public class UIResizeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static readonly Vector2 MinSize = new Vector2(280f, 320f);
        private static readonly Vector2 MaxSize = new Vector2(900f, 1400f);

        private RectTransform _target;
        private Vector2 _dir;
        private Vector2 _startSize;
        private Vector2 _startPos;
        private Vector2 _startMouseLocal;

        /// <summary>拖拽缩放结束后的回调（用于重绘自适应内容）。</summary>
        public System.Action OnResizeEnded;

        public void Init(RectTransform target, Vector2 dir)
        {
            _target = target;
            _dir = dir;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_target == null) return;
            _startSize = _target.sizeDelta;
            _startPos = _target.anchoredPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _target.parent as RectTransform, eventData.position,
                eventData.pressEventCamera, out _startMouseLocal);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_target == null) return;
            Vector2 mouseLocal;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _target.parent as RectTransform, eventData.position,
                eventData.pressEventCamera, out mouseLocal);
            Vector2 delta = mouseLocal - _startMouseLocal;

            float dw = 0f, dh = 0f;
            if (_dir.x > 0f) dw = delta.x;
            else if (_dir.x < 0f) dw = -delta.x;
            if (_dir.y > 0f) dh = delta.y;
            else if (_dir.y < 0f) dh = -delta.y;

            float newW = Mathf.Clamp(_startSize.x + dw, MinSize.x, MaxSize.x);
            float newH = Mathf.Clamp(_startSize.y + dh, MinSize.y, MaxSize.y);
            dw = newW - _startSize.x;
            dh = newH - _startSize.y;

            // 根据 pivot 动态计算位置补偿，支持任意 pivot（左对齐/居中/右对齐均正确）
            // 原理：sizeDelta 改变时元素以 pivot 为中心扩展，要保持某一边固定需反向位移
            Vector2 pivot = _target.pivot;
            Vector2 newPos = _startPos;
            if (_dir.x < 0f) newPos.x -= (1f - pivot.x) * dw;  // 左边缘拖动：右侧固定
            else if (_dir.x > 0f) newPos.x += pivot.x * dw;    // 右边缘拖动：左侧固定
            if (_dir.y > 0f) newPos.y += (1f - pivot.y) * dh;  // 上边缘拖动：底部固定
            else if (_dir.y < 0f) newPos.y -= pivot.y * dh;    // 下边缘拖动：顶部固定

            _target.sizeDelta = new Vector2(newW, newH);
            _target.anchoredPosition = newPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (OnResizeEnded != null) OnResizeEnded();
        }
    }
}
