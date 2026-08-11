using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    public class EconomyHUD : FloatingWindow
    {
        private static EconomyHUD _instance;

        private RectTransform _contentRect;

        private Button _btnOverview, _btnChart;
        private Image _btnOverviewImg, _btnChartImg;
        private Text _titleText;

        private Texture2D _chartTexture;
        private Sprite _chartSprite;
        private Texture2D _giniTexture;
        private Sprite _giniSprite;

        // 图表纹理缓存键（数据条数×10000 + 面板宽度）；一致则复用纹理，避免每次刷新重建/上传 GPU
        private int _chartCacheKey = -1;
        private int _giniCacheKey = -1;

        private enum Section { Overview, Chart, PickTarget }
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
        protected override float Padding => 16f;
        protected override float TitleFontSize => 14f;
        protected override float TitleLineHeight => 28f;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<EconomyHUD>("EconomyHUD");
        }

        /// <summary>
        /// 打开"选择目标国家"列表（由 Tab 内煽动工具按钮调用）：
        /// 列出所有王国，点击即可对该国触发原版暴动与叛乱。
        /// </summary>
        public void ShowKingdomPicker()
        {
            _visible = true;
            if (_panelRoot != null) _panelRoot.SetActive(true);
            _currentSection = Section.PickTarget;
            UpdateTabButtons();
            RefreshCurrentSection();
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

            // 标题栏拖拽 + 标题 + 缩放手柄（拖拽结束后重建当前页内容）+ 关闭按钮（公共骨架）
            UIHelpers.CreateDragArea(_panelRect, _panelRect, Padding + 36);
            _titleText = UIHelpers.CreateWindowTitle(_panelRect, UIHelpers.L(TitleKey),
                _gameFont, HeaderColor, TitleFontSize, Padding, TitleLineHeight);
            UIHelpers.CreateResizeHandles(_panelRect, OnPanelResized);
            UIHelpers.CreateCloseButton(_panelRect, _gameFont, Hide);

            // TabBar（EconomyHUD 独有：概览/趋势切换）
            var tabBar = new GameObject("TabBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabBar.transform.SetParent(_panelRoot.transform, false);
            var tabBarRt = tabBar.GetComponent<RectTransform>();
            tabBarRt.anchorMin = new Vector2(0, 1); tabBarRt.anchorMax = new Vector2(1, 1);
            tabBarRt.pivot = new Vector2(0.5f, 1f);
            tabBarRt.anchoredPosition = new Vector2(0, -(Padding + TitleLineHeight));
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
            divRt.anchoredPosition = new Vector2(0, -(Padding + TitleLineHeight + BtnHeight + 6));
            divRt.sizeDelta = new Vector2(-Padding * 2, 1);
            _lines.Add(divider); // 跟随清空逻辑，避免悬浮

            // 滚动内容区（topInset 留出标题+TabBar+分割线空间）
            float topInset = Padding + TitleLineHeight + BtnHeight + 12;
            _contentRect = UIHelpers.CreateScrollContent(_panelRect, Padding, topInset);
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

        private void UpdateTabButtons()
        {
            // 国家选择页（PickTarget）不属于任何子 Tab，概览/趋势标签页均不激活
            bool anyActive = _currentSection != Section.PickTarget;
            _btnOverviewImg.color = anyActive && _currentSection == Section.Overview ? BtnActive : BtnNormal;
            _btnChartImg.color    = anyActive && _currentSection == Section.Chart   ? BtnActive : BtnNormal;
            var ot = _btnOverview.GetComponentInChildren<Text>();
            var ct = _btnChart.GetComponentInChildren<Text>();
            ot.color = anyActive && _currentSection == Section.Overview ? Color.black : TextColor;
            ct.color = anyActive && _currentSection == Section.Chart   ? Color.black : TextColor;
        }

        public void RefreshCurrentSection()
        {
            ClearContent();
            switch (_currentSection)
            {
                case Section.Overview: BuildOverview(); break;
                case Section.Chart:    BuildChart();    break;
                case Section.PickTarget: BuildPickTarget(); break;
            }
        }

        /// <summary>基类 Toggle/Show 通过 RefreshNow 触发刷新，等价于原实现按当前 Section 重建。</summary>
        public override void RefreshNow() => RefreshCurrentSection();

        /// <summary>
        /// 刷新所有静态文本（标题 + 标签页按钮 + 当前页内容）。
        /// 标题与标签页文本在 BuildPanel() 中只创建一次，语言切换后需整体重建，
        /// 否则只剩内容区变化而标题/标签页停留在旧语言。
        /// </summary>
        public void RefreshAllTexts()
        {
            if (_panelRoot == null) return;
            if (_titleText != null) _titleText.text = UIHelpers.L("economy_title");
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
            base.ClearContent();
            // 图表纹理保留为缓存（数据条数/宽度变化时在 AddXxxChart 中重建），避免每次刷新重建
        }

        /// <summary>内容行（带标题样式）。</summary>
        private void AddLine(string text, bool isHeader = false, Color? color = null)
        {
            var go = UIHelpers.CreateText(text, _content.transform,
                isHeader ? HeaderSize : TextSize,
                color ?? (isHeader ? HeaderColor : TextColor),
                _gameFont, isHeader ? LineHeight + 4 : LineHeight,
                "Text_" + _lines.Count);
            if (isHeader) go.GetComponent<Text>().fontStyle = FontStyle.Bold;
            _lines.Add(go);
        }

        /// <summary>
        /// 趋势页：GDP 多国折线图 + 贫富差距趋势图。
        /// </summary>
        private void BuildChart()
        {
            AddGdpChart();
            AddGiniChart();
        }

        /// <summary>
        /// 在内容区追加 GDP 多线折线图（平面直角坐标系）：
        /// 全球财富 + 动态 TopN 王国财富折线（入榜显示、跌出断裂、再入榜续线），
        /// 背景为经济阶段色带，左侧数值尺（0~峰值），底部年份坐标尺。
        /// 一次显示最近 50 条记录（历史缓冲容量 100，足够支撑）。
        /// 王国折线仅保留最近 15 期内进过 Top5 的国家（图例上限 10 条），防止历史累计膨胀。
        /// </summary>
        private void AddGdpChart(int maxPoints = 50, int topN = 5)
        {
            var snaps = HistoryService.GetRecent(maxPoints);
            if (snaps.Count < 2)
            {
                AddLine(UIHelpers.L("chart_no_data"), color: new Color(0.7f, 0.7f, 0.7f));
                return;
            }

            // 构建系列：全球 GDP + 动态 TopN 王国（按每期快照 GDP 排名，折线随排名出现/断裂）
            var seriesList = BuildDynamicSeries(snaps, topN);

            int lastYear = snaps[snaps.Count - 1].GameYear;
            AddLine(UIHelpers.Lf("chart_title_dyn", topN, lastYear, snaps.Count), true);
            AddChartLegend(_content.transform, seriesList);

            // 宏观指标摘要（最新快照）
            var lastSnap = snaps[snaps.Count - 1];
            AddLine(UIHelpers.Lf("chart_macro",
                    lastSnap.TotalProduction.ToString("F0"),
                    lastSnap.PriceIndex.ToString("F2"),
                    lastSnap.AliveActorCount),
                color: new Color(0.7f, 0.85f, 1f));
            AddLine(UIHelpers.Lf("chart_macro_avg",
                    lastSnap.AvgWealth.ToString("F1"),
                    lastSnap.GiniCoefficient.ToString("F3")),
                color: new Color(0.7f, 0.85f, 1f));
            AddLine(UIHelpers.L("chart_legend_hint"), color: new Color(0.7f, 0.7f, 0.7f));

            // 尺寸：宽自适应面板，高固定扁平（平面直角坐标系观感）
            float chartW = Mathf.Round(Mathf.Max(150f, _panelRect.rect.width - Padding * 2f - 20f));
            const float chartH = 120f;
            const float yAxisW = 44f;   // 左侧 GDP 数值尺宽度
            const float bottomH = 22f;  // 底部年份坐标尺高度
            float boxH = chartH + bottomH + 4f;

            // Y 轴范围：0 ~ 所有系列最大值（NaN = 不在榜，跳过）
            float maxVal = 0f;
            foreach (var s in seriesList)
                for (int i = 0; i < s.Values.Count; i++)
                {
                    float v = s.Values[i];
                    if (!float.IsNaN(v) && v > maxVal) maxVal = v;
                }
            if (maxVal <= 0f) maxVal = 1f;

            // 容器：加 LayoutElement 固定首选尺寸，防止 VerticalLayoutGroup 布局错位导致与下方内容重叠
            var box = new GameObject("GdpChartBox", typeof(RectTransform), typeof(LayoutElement));
            box.transform.SetParent(_content.transform, false);
            var boxRt = box.GetComponent<RectTransform>();
            boxRt.sizeDelta = new Vector2(0, boxH);
            var boxEl = box.GetComponent<LayoutElement>();
            boxEl.preferredWidth = chartW;
            boxEl.preferredHeight = boxH;
            _lines.Add(box);

            // 图表纹理（左侧留数值尺空白）；缓存复用：
            // 键 = 条数×10000 + 宽度 + 内容指纹。原实现不含指纹，
            // 历史条数达上限（GetRecent 固定 50）后快照数量不再增长，
            // 王国变更（新王国入榜/旧王国灭亡）时图例已更新但纹理未重建 → 图表卡住错乱。
            int cacheKey = snaps.Count * 10000 + (int)chartW;
            cacheKey = cacheKey * 31 + ComputeSeriesSignature(snaps, seriesList);
            if (_chartCacheKey != cacheKey)
            {
                _chartCacheKey = cacheKey;
                if (_chartSprite != null) { Destroy(_chartSprite); _chartSprite = null; }
                if (_chartTexture != null) { Destroy(_chartTexture); _chartTexture = null; }
                _chartTexture = GenerateMultiLineChartTexture(snaps, (int)chartW, (int)chartH,
                    (int)yAxisW, seriesList, maxVal);
                if (_chartTexture == null)
                {
                    AddLine(UIHelpers.L("chart_fail"), color: new Color(0.7f, 0.7f, 0.7f));
                    return;
                }
                _chartSprite = Sprite.Create(_chartTexture,
                    new Rect(0, 0, _chartTexture.width, _chartTexture.height),
                    new Vector2(0.5f, 0.5f), 1f);
            }

            var imgGo = new GameObject("GdpChartImg", typeof(RectTransform), typeof(Image));
            imgGo.transform.SetParent(box.transform, false);
            var imgRt = imgGo.GetComponent<RectTransform>();
            imgRt.anchorMin = new Vector2(0, 1); imgRt.anchorMax = new Vector2(1, 1);
            imgRt.pivot = new Vector2(0.5f, 1f);
            imgRt.anchoredPosition = Vector2.zero;
            imgRt.sizeDelta = new Vector2(0, chartH);
            var img = imgGo.GetComponent<Image>();
            img.sprite = _chartSprite;
            img.raycastTarget = false;

            // 左侧 GDP 数值尺（5 档刻度，与网格线对齐：底部=0 顶部=maxVal）
            // 网格线在纹理内 y=mB+ch*frac（mB=4, ch=112）；imgRt 底部相对 box 底部 = boxH-chartH = 26
            float chartBottom = boxH - chartH;
            var scaleCol = new Color(0.92f, 0.92f, 0.92f);
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                float val = maxVal * frac;
                float yPos = chartBottom + 4f + 112f * frac;
                var tGo = UIHelpers.CreateText(val.ToString("F0"), box.transform, 10f, scaleCol, _gameFont, 13f);
                var tRt = tGo.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.zero;
                tRt.pivot = new Vector2(1f, 0.5f);
                tRt.anchoredPosition = new Vector2(yAxisW, yPos);
                tRt.sizeDelta = new Vector2(yAxisW - 4f, 13f);
                var t = tGo.GetComponent<Text>();
                t.alignment = TextAnchor.MiddleRight;
                t.fontStyle = FontStyle.Bold;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            // 底部年份坐标尺：0/25/50/75/100% 处显示对应采集点的游戏年
            var xCol = new Color(0.85f, 0.85f, 0.85f);
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                int idx = Mathf.RoundToInt((snaps.Count - 1) * frac);
                // 纹理内折线 x：mL + cw*frac（mL=yAxisW, cw=chartW-yAxisW-mR, mR=4）
                float xPos = yAxisW + (chartW - yAxisW - 4f) * frac;
                float pivotX = gi == 0 ? 0f : (gi == 4 ? 1f : 0.5f);
                var xGo = UIHelpers.CreateText(UIHelpers.Lf("chart_year", snaps[idx].GameYear), box.transform, 9f, xCol, _gameFont, 14f);
                var xRt = xGo.GetComponent<RectTransform>();
                xRt.anchorMin = Vector2.zero; xRt.anchorMax = Vector2.zero;
                xRt.pivot = new Vector2(pivotX, 0f);
                xRt.anchoredPosition = new Vector2(xPos, 3f);
                xRt.sizeDelta = new Vector2(44f, 14f);
                xGo.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
            }

            long last = snaps[snaps.Count - 1].GlobalGDP;
            AddLine(UIHelpers.Lf("chart_summary", maxVal.ToString("F0"), last.ToString("F0")),
                color: new Color(0.7f, 0.85f, 1f));
        }

        /// <summary>
        /// 贫富差距趋势图：全球基尼系数折线 + 危险线/健康线参考虚线 + 经济阶段背景色带。
        /// 直观呈现"贫富差距驱动周期阶段"的因果，与财富图并列展示。
        /// 一次显示最近 50 条记录。
        /// </summary>
        private void AddGiniChart(int maxPoints = 50)
        {
            var snaps = HistoryService.GetRecent(maxPoints);
            if (snaps.Count < 2) return;

            int lastYear = snaps[snaps.Count - 1].GameYear;
            AddLine(UIHelpers.Lf("gini_chart_title", lastYear, snaps.Count), true);
            AddLine(UIHelpers.L("gini_legend_phase"), color: new Color(0.7f, 0.7f, 0.7f));

            float chartW = Mathf.Round(Mathf.Max(150f, _panelRect.rect.width - Padding * 2f - 20f));
            const float chartH = 120f;
            const float yAxisW = 40f;
            const float bottomH = 22f;
            float boxH = chartH + bottomH + 4f;

            // Y 范围：0 ~ max(0.8, 快照最大基尼)，保证曲线与参考线比例可见
            float maxVal = 0.8f;
            foreach (var s in snaps)
                if (s.GiniCoefficient > maxVal) maxVal = s.GiniCoefficient;

            var cfg = UnrestConfig.Instance;
            var box = new GameObject("GiniChartBox", typeof(RectTransform), typeof(LayoutElement));
            box.transform.SetParent(_content.transform, false);
            var boxRt = box.GetComponent<RectTransform>();
            boxRt.sizeDelta = new Vector2(0, boxH);
            var boxEl = box.GetComponent<LayoutElement>();
            boxEl.preferredWidth = chartW;
            boxEl.preferredHeight = boxH;
            _lines.Add(box);

            // 贫富差距图纹理；缓存复用：键含基尼/阶段内容指纹，数据变化即重建
            int cacheKey = snaps.Count * 10000 + (int)chartW;
            cacheKey = cacheKey * 31 + ComputeGiniSignature(snaps);
            if (_giniCacheKey != cacheKey)
            {
                _giniCacheKey = cacheKey;
                if (_giniSprite != null) { Destroy(_giniSprite); _giniSprite = null; }
                if (_giniTexture != null) { Destroy(_giniTexture); _giniTexture = null; }
                _giniTexture = GenerateGiniChartTexture(snaps, (int)chartW, (int)chartH,
                    (int)yAxisW, cfg.CycleGiniHigh, cfg.CycleGiniLow, maxVal);
                if (_giniTexture == null)
                {
                    AddLine(UIHelpers.L("chart_fail"), color: new Color(0.7f, 0.7f, 0.7f));
                    return;
                }
                _giniSprite = Sprite.Create(_giniTexture,
                    new Rect(0, 0, _giniTexture.width, _giniTexture.height),
                    new Vector2(0.5f, 0.5f), 1f);
            }

            var imgGo = new GameObject("GiniChartImg", typeof(RectTransform), typeof(Image));
            imgGo.transform.SetParent(box.transform, false);
            var imgRt = imgGo.GetComponent<RectTransform>();
            imgRt.anchorMin = new Vector2(0, 1); imgRt.anchorMax = new Vector2(1, 1);
            imgRt.pivot = new Vector2(0.5f, 1f);
            imgRt.anchoredPosition = Vector2.zero;
            imgRt.sizeDelta = new Vector2(0, chartH);
            var img = imgGo.GetComponent<Image>();
            img.sprite = _giniSprite;
            img.raycastTarget = false;

            // 左侧基尼数值尺（0~maxVal，5 档，与纹理网格对齐）
            float chartBottom = boxH - chartH;
            var scaleCol = new Color(0.92f, 0.92f, 0.92f);
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                float val = maxVal * frac;
                float yPos = chartBottom + 4f + 112f * frac;
                var tGo = UIHelpers.CreateText(val.ToString("F2"), box.transform, 10f, scaleCol, _gameFont, 13f);
                var tRt = tGo.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.zero;
                tRt.pivot = new Vector2(1f, 0.5f);
                tRt.anchoredPosition = new Vector2(yAxisW, yPos);
                tRt.sizeDelta = new Vector2(yAxisW - 4f, 13f);
                var t = tGo.GetComponent<Text>();
                t.alignment = TextAnchor.MiddleRight;
                t.fontStyle = FontStyle.Bold;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            // 底部年份坐标尺
            var xCol = new Color(0.85f, 0.85f, 0.85f);
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                int idx = Mathf.RoundToInt((snaps.Count - 1) * frac);
                float xPos = yAxisW + (chartW - yAxisW - 4f) * frac;
                float pivotX = gi == 0 ? 0f : (gi == 4 ? 1f : 0.5f);
                var xGo = UIHelpers.CreateText(UIHelpers.Lf("chart_year", snaps[idx].GameYear), box.transform, 9f, xCol, _gameFont, 14f);
                var xRt = xGo.GetComponent<RectTransform>();
                xRt.anchorMin = Vector2.zero; xRt.anchorMax = Vector2.zero;
                xRt.pivot = new Vector2(pivotX, 0f);
                xRt.anchoredPosition = new Vector2(xPos, 3f);
                xRt.sizeDelta = new Vector2(44f, 14f);
                xGo.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
            }

            // 参考线说明
            AddLine(UIHelpers.L("gini_legend_danger"), color: new Color(1f, 0.5f, 0.45f));
            AddLine(UIHelpers.L("gini_legend_health"), color: new Color(0.5f, 0.85f, 0.5f));
        }

        // ===== BuildDynamicSeries 复用缓冲（每次刷新图表时复用，避免 GC）=====
        private static readonly List<ChartSeries> _seriesPool = new List<ChartSeries>(4);
        // ChartSeries 对象池（与 _seriesPool 配合，Values 列表也复用）
        private static readonly List<ChartSeries> _chartSeriesEntryPool = new List<ChartSeries>(4);
        // 每期排名用临时缓冲 + 系列索引（KingdomId -> 系列）
        private static readonly List<KingdomStats> _rankBuf = new List<KingdomStats>(8);
        private static readonly Dictionary<long, ChartSeries> _dynIndex = new Dictionary<long, ChartSeries>(8);
        // 王国最后上榜期索引（KingdomId -> 快照序号，用于图例膨胀保护排序）
        private static readonly Dictionary<long, int> _dynLastSeen = new Dictionary<long, int>(8);
        // 图例膨胀保护排序缓冲 + 保留集合
        private static readonly List<KeyValuePair<long, ChartSeries>> _dynSeenBuf = new List<KeyValuePair<long, ChartSeries>>(8);
        private static readonly HashSet<long> _keepIds = new HashSet<long>(8);
        // 动态王国折线色板（最多展示 topN=5 条，预留充足区分度）
        private static readonly Color[] _dynColors = new[]
        {
            new Color(0.30f, 0.80f, 1.00f), // 青
            new Color(0.45f, 0.90f, 0.45f), // 绿
            new Color(0.95f, 0.55f, 0.80f), // 品红
            new Color(1.00f, 0.70f, 0.30f), // 橙
            new Color(0.75f, 0.60f, 1.00f)  // 紫
        };

        /// <summary>
        /// 构建图表系列：全球 GDP + 动态 TopN 王国 GDP（按每期快照的 GDP 排名）。
        /// 国家在当期排名 ≤ topN 才绘制该点（否则记 NaN → 折线断裂）；
        /// 因此：入榜即出现，跌出即断裂消失，再入榜则重新续线。
        /// 王国集合只统计"最近 collectWindow 期"内进过 TopN 的王国（超出窗口自动滑出图例），
        /// 且数量有硬上限 maxSeries（按最后上榜期保留最近者），避免历史累计导致图例无限膨胀。
        /// 算法：对每条 snap 的 Kingdoms 排序取前 topN，O(snaps × kingdomsLogN)；
        /// 通过 _dynIndex 字典把 KingdomId 映射到系列，复用静态缓冲避免 GC。
        /// </summary>
        private static List<ChartSeries> BuildDynamicSeries(List<EconomySnapshot> snaps, int topN)
        {
            const int collectWindow = 15; // 只统计最近 15 期内的 TopN，更早的王国自动滑出
            const int maxSeries = 10;     // 王国折线硬上限（防极端换榜导致图例膨胀）

            var result = _seriesPool;
            // 归还上一轮的 ChartSeries 对象（Values 列表也复用，避免新建）
            for (int i = 0; i < result.Count; i++)
            {
                result[i].Values.Clear();
                _chartSeriesEntryPool.Add(result[i]);
            }
            result.Clear();
            _dynIndex.Clear();
            _dynLastSeen.Clear();

            ChartSeries global;
            if (_chartSeriesEntryPool.Count > 0)
            {
                global = _chartSeriesEntryPool[_chartSeriesEntryPool.Count - 1];
                _chartSeriesEntryPool.RemoveAt(_chartSeriesEntryPool.Count - 1);
            }
            else
            {
                global = new ChartSeries();
            }
            global.Name = UIHelpers.L("chart_global");
            global.Color = new Color(1f, 0.85f, 0.3f);
            for (int i = 0; i < snaps.Count; i++) global.Values.Add(snaps[i].GlobalGDP);
            result.Add(global);

            // 第一轮：收集出现在"最近 collectWindow 期"内 TopN 的王国，建立 KingdomId -> 系列（颜色按入榜顺序分配）。
            // 只统计滑动窗口而非全部快照：超出窗口未再上榜的旧王国自动滑出，图例数量有界。
            int windowStart = snaps.Count > collectWindow ? snaps.Count - collectWindow : 0;
            for (int j = windowStart; j < snaps.Count; j++)
            {
                var kingdoms = snaps[j].Kingdoms;
                if (kingdoms == null || kingdoms.Count == 0) continue;

                var buf = _rankBuf;
                buf.Clear();
                buf.AddRange(kingdoms);
                buf.Sort((a, b) => b.GDP.CompareTo(a.GDP));
                int take = buf.Count < topN ? buf.Count : topN;
                for (int i = 0; i < take; i++)
                {
                    var ks = buf[i];
                    if (_dynIndex.TryGetValue(ks.KingdomId, out var existing))
                    {
                        _dynLastSeen[ks.KingdomId] = j; // 更新最后上榜期
                        continue;
                    }
                    ChartSeries series;
                    if (_chartSeriesEntryPool.Count > 0)
                    {
                        series = _chartSeriesEntryPool[_chartSeriesEntryPool.Count - 1];
                        _chartSeriesEntryPool.RemoveAt(_chartSeriesEntryPool.Count - 1);
                    }
                    else
                    {
                        series = new ChartSeries();
                    }
                    series.Name = ks.KingdomName;
                    series.Color = _dynColors[result.Count % _dynColors.Length];
                    for (int k = 0; k < snaps.Count; k++) series.Values.Add(float.NaN); // 默认不在榜
                    _dynIndex[ks.KingdomId] = series;
                    _dynLastSeen[ks.KingdomId] = j;
                    result.Add(series);
                }
            }

            // 图例膨胀保护：王国系列超过上限时，按最后上榜期保留最近 maxSeries 条，其余归还对象池
            if (result.Count - 1 > maxSeries)
            {
                var seen = _dynSeenBuf;
                seen.Clear();
                foreach (var kv in _dynIndex) seen.Add(kv);
                seen.Sort((a, b) => _dynLastSeen[b.Key].CompareTo(_dynLastSeen[a.Key])); // 最近上榜在前
                int keepCount = seen.Count < maxSeries ? seen.Count : maxSeries;
                var keep = _keepIds;
                keep.Clear();
                for (int i = 0; i < keepCount; i++) keep.Add(seen[i].Key);
                for (int i = result.Count - 1; i >= 1; i--)
                {
                    var ser = result[i];
                    long id = -1;
                    for (int k = 0; k < seen.Count; k++)
                        if (seen[k].Value == ser) { id = seen[k].Key; break; }
                    if (id < 0 || !keep.Contains(id))
                    {
                        ser.Values.Clear();
                        _chartSeriesEntryPool.Add(ser);
                        if (id >= 0) _dynIndex.Remove(id);
                        result.RemoveAt(i);
                    }
                }
            }

            // 第二轮：按每期排名填入 TopN 王国的 GDP（不在榜保持 NaN，折线断裂）
            for (int j = 0; j < snaps.Count; j++)
            {
                var kingdoms = snaps[j].Kingdoms;
                if (kingdoms == null || kingdoms.Count == 0) continue;

                var buf = _rankBuf;
                buf.Clear();
                buf.AddRange(kingdoms);
                buf.Sort((a, b) => b.GDP.CompareTo(a.GDP));
                int take = buf.Count < topN ? buf.Count : topN;
                for (int i = 0; i < take; i++)
                {
                    var ks = buf[i];
                    if (_dynIndex.TryGetValue(ks.KingdomId, out var series))
                        series.Values[j] = ks.GDP;
                }
            }
            return result;
        }

        /// <summary>
        /// 计算 GDP 图表内容指纹：全球 GDP 序列 + 各系列（动态王国）名称与数值序列（含 NaN 标记）。
        /// 数据变化（含王国变更）→ 指纹变化 → 纹理重建；数据未变 → 复用缓存纹理。
        /// </summary>
        private static int ComputeSeriesSignature(List<EconomySnapshot> snaps, List<ChartSeries> seriesList)
        {
            int h = snaps.Count;
            for (int i = 0; i < snaps.Count; i++)
                h = h * 31 + (int)snaps[i].GlobalGDP;
            for (int s = 0; s < seriesList.Count; s++)
            {
                var ser = seriesList[s];
                h = h * 31 + (ser.Name != null ? ser.Name.GetHashCode() : 0);
                for (int i = 0; i < ser.Values.Count; i++)
                {
                    float v = ser.Values[i];
                    h = h * 31 + (float.IsNaN(v) ? int.MinValue : (int)v);
                }
            }
            return h;
        }

        /// <summary>计算贫富差距图表内容指纹：基尼系数序列 + 经济阶段序列。</summary>
        private static int ComputeGiniSignature(List<EconomySnapshot> snaps)
        {
            int h = snaps.Count;
            for (int i = 0; i < snaps.Count; i++)
                h = h * 31 + (int)(snaps[i].GiniCoefficient * 1000f) + snaps[i].Phase;
            return h;
        }

        /// <summary>
        /// 图例（竖直行列表）：色条 + 名称 + 最新值（在榜）/ 已跌出（不在榜）。
        /// 每个系列独立一行，行内色条 + 名称 + 尾部数值，信息比横向图例更丰富。
        /// </summary>
        private void AddChartLegend(Transform parent, List<ChartSeries> series)
        {
            foreach (var s in series)
            {
                var row = new GameObject("LegendRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                row.transform.SetParent(parent, false);
                var rowLg = row.GetComponent<HorizontalLayoutGroup>();
                rowLg.spacing = 6f;
                rowLg.childAlignment = TextAnchor.MiddleLeft;
                rowLg.childControlWidth = false;
                rowLg.childControlHeight = true;
                rowLg.childForceExpandWidth = false;
                rowLg.childForceExpandHeight = false;
                var rowRt = row.GetComponent<RectTransform>();
                rowRt.sizeDelta = new Vector2(0, 18f);
                row.GetComponent<LayoutElement>().preferredHeight = 18f;
                _lines.Add(row);

                // 色条
                var sw = new GameObject("Swatch", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                sw.transform.SetParent(row.transform, false);
                sw.GetComponent<Image>().color = s.Color;
                sw.GetComponent<Image>().raycastTarget = false;
                var swRt = sw.GetComponent<RectTransform>();
                swRt.sizeDelta = new Vector2(20f, 6f);
                sw.GetComponent<LayoutElement>().preferredWidth = 20f;
                sw.GetComponent<LayoutElement>().preferredHeight = 6f;

                // 名称（固定宽，超出省略号）
                var nm = UIHelpers.CreateText(s.Name, row.transform, 10f, TextColor, _gameFont, 18f);
                var nmT = nm.GetComponent<Text>();
                nmT.alignment = TextAnchor.MiddleLeft;
                nmT.horizontalOverflow = HorizontalWrapMode.Overflow;
                nm.GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 18f);
                var nmEl = nm.AddComponent<LayoutElement>();
                nmEl.preferredWidth = 150f;
                nmEl.preferredHeight = 18f;

                // 最新值 / 已跌出状态
                float last = float.NaN;
                for (int i = s.Values.Count - 1; i >= 0; i--)
                {
                    if (!float.IsNaN(s.Values[i])) { last = s.Values[i]; break; }
                }
                bool inRank = !float.IsNaN(last);
                string valueStr = inRank ? last.ToString("F0")
                    : (s.Values.Count > 0 ? UIHelpers.L("chart_dropped") : "");
                var vGo = UIHelpers.CreateText(valueStr, row.transform, 10f,
                    inRank ? new Color(0.95f, 0.85f, 0.5f) : new Color(0.55f, 0.55f, 0.6f),
                    _gameFont, 18f);
                var vT = vGo.GetComponent<Text>();
                vT.alignment = TextAnchor.MiddleRight;
                vT.horizontalOverflow = HorizontalWrapMode.Overflow;
                var vRt = vGo.GetComponent<RectTransform>();
                vRt.anchorMin = new Vector2(1, 0); vRt.anchorMax = new Vector2(1, 0);
                vRt.pivot = new Vector2(1, 0.5f);
                vRt.anchoredPosition = new Vector2(0, 9f);
                vRt.sizeDelta = new Vector2(70f, 18f);
                var vEl = vGo.AddComponent<LayoutElement>();
                vEl.preferredWidth = 70f;
                vEl.preferredHeight = 18f;

                row.GetComponent<LayoutElement>().preferredWidth = 250f;
            }
        }

        /// <summary>
        /// 图表系列：名称 + 颜色 + 与快照等长的取值序列。
        /// </summary>
        private class ChartSeries
        {
            public string Name;
            public Color Color;
            public List<float> Values = new List<float>();
        }

        // 图表像素缓冲复用：每次重绘只需 Clear，避免 ~1MB Color 数组的 GC
        // （chartW 最大 ~848，chartH=120，约 10 万 Color × 16B = 1.6MB）
        private static Color[] _chartPxBuffer;
        private static int _chartPxBufferSize;

        /// <summary>获取指定大小的复用像素缓冲；不足时按需扩容。</summary>
        private static Color[] RentPixelBuffer(int w, int h, Color bg)
        {
            int len = w * h;
            if (_chartPxBuffer == null || _chartPxBufferSize < len)
            {
                _chartPxBuffer = new Color[len];
                _chartPxBufferSize = len;
            }
            var px = _chartPxBuffer;
            for (int i = 0; i < len; i++) px[i] = bg;
            return px;
        }

        /// <summary>
        /// 绘制多线折线图纹理：暗色底、经济阶段背景色带、横向网格、每个系列一条折线，
        /// 首个系列（全球 GDP）附加半透明面积填充；系列值为 NaN 的点不连线（王国跌出排名即断裂）。
        /// y=0 在底部，显示时天然正向。
        /// </summary>
        private static Texture2D GenerateMultiLineChartTexture(
            List<EconomySnapshot> snaps, int w, int h, int yAxisW,
            List<ChartSeries> series, float maxVal)
        {
            int n = snaps.Count;
            if (n < 2 || w <= 4 || h <= 4 || series.Count == 0) return null;
            if (maxVal <= 0f) maxVal = 1f;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var bg = new Color(0.06f, 0.07f, 0.09f, 0.95f);
            var px = RentPixelBuffer(w, h, bg);

            int mL = yAxisW > 4 ? yAxisW : 4;
            const int mR = 4, mT = 4, mB = 4;
            int cw = w - mL - mR;
            int ch = h - mT - mB;

            // 阶段背景色带（按每个快照的阶段，半透明叠加在背景上）
            for (int i = 0; i < n - 1; i++)
            {
                int x0 = mL + Mathf.RoundToInt(cw * i / (float)(n - 1));
                int x1 = mL + Mathf.RoundToInt(cw * (i + 1) / (float)(n - 1));
                var band = PhaseBandColor(snaps[i].Phase);
                for (int x = x0; x <= x1; x++)
                    for (int y = mB; y <= mT + ch; y++)
                        BlendChartPx(px, w, h, x, y, band);
            }

            // 横向网格线（含底部坐标轴），从数值尺右缘开始
            var grid = new Color(0.35f, 0.35f, 0.4f, 0.45f);
            for (int gi = 0; gi <= 4; gi++)
            {
                int y = mB + ch * gi / 4;
                for (int x = mL; x < w - mR; x++) px[y * w + x] = grid;
            }

            // 每个系列绘制折线（NaN = 断裂，跳过该段）
            for (int si = 0; si < series.Count; si++)
            {
                var s = series[si];
                var pts = new Vector2Int[n];
                for (int i = 0; i < n; i++)
                {
                    float v = i < s.Values.Count ? s.Values[i] : float.NaN;
                    pts[i].x = mL + Mathf.RoundToInt(cw * i / (float)(n - 1));
                    if (!float.IsNaN(v))
                        pts[i].y = mB + Mathf.RoundToInt(ch * (v / maxVal));
                    else
                        pts[i].y = int.MinValue; // 断裂标记
                }
                // 首个系列（全球）附加面积填充（仅连续段）
                if (si == 0)
                {
                    var fill = s.Color;
                    fill.a = 0.14f;
                    for (int i = 0; i < n - 1; i++)
                        if (pts[i].y != int.MinValue && pts[i + 1].y != int.MinValue)
                            FillAreaSegment(px, w, h, pts[i], pts[i + 1], mB, fill);
                }
                // 折线 + 末端高亮点（断裂段跳过）
                bool lineActive = false;
                for (int i = 0; i < n - 1; i++)
                {
                    if (pts[i].y != int.MinValue && pts[i + 1].y != int.MinValue)
                    {
                        DrawLine(px, w, h, pts[i], pts[i + 1], s.Color);
                        lineActive = true;
                    }
                }
                if (lineActive)
                {
                    // 末端高亮点：取最后一个有效点
                    for (int i = n - 1; i >= 0; i--)
                    {
                        if (pts[i].y != int.MinValue)
                        {
                            DrawDot(px, w, h, pts[i], s.Color, 2);
                            break;
                        }
                    }
                }
            }

            // 复用缓冲可能大于 w*h，必须用 SetPixels(x,y,w,h,px) 仅写入前 w*h 个像素
            tex.SetPixels(0, 0, w, h, px);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 贫富差距趋势图纹理：阶段背景色带 + 网格 + 危险/健康参考虚线 + 基尼折线。
        /// </summary>
        private static Texture2D GenerateGiniChartTexture(
            List<EconomySnapshot> snaps, int w, int h, int yAxisW,
            float giniHigh, float giniLow, float maxVal)
        {
            int n = snaps.Count;
            if (n < 2 || w <= 4 || h <= 4) return null;
            if (maxVal <= 0f) maxVal = 1f;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var bg = new Color(0.06f, 0.07f, 0.09f, 0.95f);
            var px = RentPixelBuffer(w, h, bg);

            int mL = yAxisW > 4 ? yAxisW : 4;
            const int mR = 4, mT = 4, mB = 4;
            int cw = w - mL - mR;
            int ch = h - mT - mB;

            // 阶段背景色带（按每个快照的阶段，半透明叠加在背景上）
            for (int i = 0; i < n - 1; i++)
            {
                int x0 = mL + Mathf.RoundToInt(cw * i / (float)(n - 1));
                int x1 = mL + Mathf.RoundToInt(cw * (i + 1) / (float)(n - 1));
                var band = PhaseBandColor(snaps[i].Phase);
                for (int x = x0; x <= x1; x++)
                    for (int y = mB; y <= mT + ch; y++)
                        BlendChartPx(px, w, h, x, y, band);
            }

            // 横向网格线
            var grid = new Color(0.35f, 0.35f, 0.4f, 0.45f);
            for (int gi = 0; gi <= 4; gi++)
            {
                int y = mB + ch * gi / 4;
                for (int x = mL; x < w - mR; x++) px[y * w + x] = grid;
            }

            // 危险线（红虚线）/ 健康线（绿虚线）
            DrawDashedHLine(px, w, h, mL, w - mR,
                mB + Mathf.RoundToInt(ch * Mathf.Clamp01(giniHigh / maxVal)),
                new Color(1f, 0.35f, 0.3f, 0.85f));
            DrawDashedHLine(px, w, h, mL, w - mR,
                mB + Mathf.RoundToInt(ch * Mathf.Clamp01(giniLow / maxVal)),
                new Color(0.4f, 0.9f, 0.4f, 0.85f));

            // 基尼折线（橙色）+ 末端高亮点
            var pts = new Vector2Int[n];
            var lineC = new Color(1f, 0.62f, 0.18f);
            for (int i = 0; i < n; i++)
            {
                float v = Mathf.Clamp(snaps[i].GiniCoefficient, 0f, maxVal);
                pts[i].x = mL + Mathf.RoundToInt(cw * i / (float)(n - 1));
                pts[i].y = mB + Mathf.RoundToInt(ch * (v / maxVal));
            }
            for (int i = 0; i < n - 1; i++)
                DrawLine(px, w, h, pts[i], pts[i + 1], lineC);
            DrawDot(px, w, h, pts[n - 1], lineC, 2);

            // 复用缓冲可能大于 w*h，必须用 SetPixels(x,y,w,h,px) 仅写入前 w*h 个像素
            tex.SetPixels(0, 0, w, h, px);
            tex.Apply();
            return tex;
        }

        /// <summary>经济阶段背景色带（半透明，与深色背景混合）。</summary>
        private static Color PhaseBandColor(int phase)
        {
            switch (phase)
            {
                case (int)EconomyPhase.Boom:       return new Color(0.1f, 0.5f, 0.2f, 0.20f);
                case (int)EconomyPhase.Recession:  return new Color(0.5f, 0.35f, 0.1f, 0.16f);
                case (int)EconomyPhase.Depression: return new Color(0.5f, 0.12f, 0.1f, 0.22f);
                case (int)EconomyPhase.Recovery:   return new Color(0.1f, 0.3f, 0.5f, 0.16f);
                default:                           return new Color(0f, 0f, 0f, 0f);
            }
        }

        /// <summary>半透明叠加像素（阶段色带与背景混合）。</summary>
        private static void BlendChartPx(Color[] px, int w, int h, int x, int y, Color c)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) return;
            int i = y * w + x;
            px[i] = Color.Lerp(px[i], c, c.a);
        }

        /// <summary>水平虚线（参考线用）：3 像素实线 + 2 像素间隔循环。</summary>
        private static void DrawDashedHLine(Color[] px, int w, int h, int x0, int x1, int y, Color c)
        {
            for (int x = x0; x <= x1; x += 5)
                for (int k = 0; k < 3 && x + k <= x1; k++)
                    SetChartPx(px, w, h, x + k, y, c);
        }

        private static void SetChartPx(Color[] px, int w, int h, int x, int y, Color c)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) return;
            px[y * w + x] = c;
        }

        private static void DrawLine(Color[] px, int w, int h, Vector2Int a, Vector2Int b, Color c)
        {
            int x0 = a.x, y0 = a.y;
            int x1 = b.x, y1 = b.y;
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                SetChartPx(px, w, h, x0, y0, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        private static void FillAreaSegment(Color[] px, int w, int h,
            Vector2Int a, Vector2Int b, int baseY, Color c)
        {
            int x0 = Mathf.Min(a.x, b.x), x1 = Mathf.Max(a.x, b.x);
            for (int x = x0; x <= x1; x++)
            {
                float t = x1 > x0 ? (x - x0) / (float)(x1 - x0) : 0f;
                int top = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, t));
                for (int y = baseY; y <= top; y++)
                    SetChartPx(px, w, h, x, y, c);
            }
        }

        private static void DrawDot(Color[] px, int w, int h, Vector2Int p, Color c, int r)
        {
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                    if (dx * dx + dy * dy <= r * r)
                        SetChartPx(px, w, h, p.x + dx, p.y + dy, c);
        }

        private void BuildOverview()
        {
            float contentW = Mathf.Max(150f, _panelRect.rect.width - Padding * 2f - 16f);
            var phase = EconomyCycleModulator.CurrentPhase;

            // 周期 + 阶段徽章
            AddLine(UIHelpers.Lf("overview_cycle", EconomyEngine.CycleIndex), true);
            _lines.Add(UIComponents.CreatePhaseBadge(_content.transform, phase,
                PhaseName(phase), _gameFont, contentW * 0.5f));
            AddLine(UIHelpers.Lf("cycle_detail",
                EconomyCycleModulator.PhaseDuration,
                EconomyCycleModulator.GrowthRate.ToString("+0.0%;-0.0%"),
                EconomyCycleModulator.BubbleValue.ToString("F0")),
                color: new Color(0.8f, 0.9f, 0.7f));
            AddLine("");

            // 核心指标卡网格（3列）
            var stats = new (string, string, Color)[]
            {
                ("GDP", EconomyEngine.GlobalGDP.ToString("F0"), UIStyles.Gold),
                ("人均", EconomyEngine.AvgWealth.ToString("F1"), UIStyles.Info),
                ("人口", EconomyEngine.AliveActorCount.ToString(), UIStyles.TextPrimary),
                ("基尼", EconomyEngine.GiniCoefficient.ToString("F3"), GiniColor(EconomyEngine.GiniCoefficient)),
                ("贸易", EconomyEngine.TotalTradeVolume.ToString("F0"), UIStyles.Positive),
                ("泡沫", EconomyCycleModulator.BubbleValue.ToString("F0"), UIStyles.Warning)
            };
            _lines.Add(UIComponents.CreateStatGrid(_content.transform, stats, _gameFont, contentW, 3));

            // 王国排行
            _lines.Add(UIComponents.CreateSectionHeader(_content.transform,
                UIHelpers.L("overview_kingdoms"), _gameFont, contentW));
            _lines.Add(UIComponents.CreateKingdomHeader(_content.transform, _gameFont, contentW));
            var top = EconomyEngine.TopKingdoms(8);
            if (top.Count == 0)
            {
                AddLine(UIHelpers.L("overview_no_kingdom"), color: UIStyles.TextMuted);
            }
            int rank = 1;
            foreach (var k in top)
            {
                _lines.Add(UIComponents.CreateKingdomRow(_content.transform, rank, k.KingdomName,
                    k.GDP.ToString("F0"), k.AvgWealth.ToString("F1"), k.GiniCoefficient.ToString("F2"),
                    _gameFont, contentW, rank == 1));
                rank++;
            }

            // 社会动荡状态
            _lines.Add(UIComponents.CreateSectionHeader(_content.transform,
                UIHelpers.L("unrest_state_title"), _gameFont, contentW));
            int stateCount = 0;
            foreach (var k in top)
            {
                int st = UnrestEngine.GetState(k.KingdomId, out int elapsed);
                if (st == 1)
                {
                    _lines.Add(UIComponents.CreateStatusRow(_content.transform,
                        UIHelpers.Lf("unrest_state_accum", k.KingdomName, elapsed),
                        UIStyles.Warning, _gameFont, contentW));
                    stateCount++;
                }
                else if (st == 2)
                {
                    _lines.Add(UIComponents.CreateStatusRow(_content.transform,
                        UIHelpers.Lf("unrest_state_active", k.KingdomName),
                        UIStyles.Danger, _gameFont, contentW));
                    stateCount++;
                }
                else if (st == 3)
                {
                    _lines.Add(UIComponents.CreateStatusRow(_content.transform,
                        UIHelpers.Lf("unrest_state_uprising", k.KingdomName),
                        UIStyles.Negative, _gameFont, contentW));
                    stateCount++;
                }
            }
            if (stateCount == 0)
            {
                AddLine(UIHelpers.L("unrest_state_none"), color: UIStyles.TextMuted);
            }
            AddLine(UIHelpers.Lf("unrest_state_threshold", UnrestConfig.Instance.GiniThreshold.ToString("F3")),
                color: new Color(0.7f, 0.85f, 0.7f));
        }

        /// <summary>基尼语义色（≥0.7 红 / ≥0.55 琥珀 / 其他弱色）。</summary>
        private static Color GiniColor(float gini)
        {
            return gini >= 0.7f ? UIStyles.Danger : gini >= 0.55f ? UIStyles.Warning : UIStyles.TextSecondary;
        }

        /// <summary>
        /// 国家选择页：列出所有王国，每个王国提供"煽动"与"镇压"两个按钮，
        /// 可针对特定国家单独生效。由 Tab 内煽动（火焰）/镇压（盾牌）工具按钮调用 ShowKingdomPicker() 打开。
        /// </summary>
        private void BuildPickTarget()
        {
            AddLine(UIHelpers.L("picker_title"), true);
            AddLine("");

            // 返回概览页
            var btnBack = UIHelpers.CreateButton(UIHelpers.L("picker_back"), _content.transform, -1, 30,
                _gameFont, new Color(0.35f, 0.35f, 0.4f, 0.85f));
            btnBack.onClick.AddListener(() => SwitchSection(Section.Overview));
            _lines.Add(btnBack.gameObject);

            AddLine("");
            AddLine(UIHelpers.L("picker_hint"), color: new Color(0.7f, 0.7f, 0.7f));
            AddDivider(DividerColor);

            if (World.world == null || World.world.kingdoms == null)
            {
                AddLine(UIHelpers.L("picker_empty"), color: new Color(0.7f, 0.7f, 0.7f));
                return;
            }
            // 快照 + 按 GDP 降序（无统计的王国排最后）
            var kingdomList = new List<Kingdom>(World.world.kingdoms);
            if (kingdomList.Count == 0)
            {
                AddLine(UIHelpers.L("picker_empty"), color: new Color(0.7f, 0.7f, 0.7f));
                return;
            }
            kingdomList.Sort((a, b) =>
            {
                long ga = 0, gb = 0;
                if (EconomyEngine.KingdomStats.TryGetValue(a.data.id, out var ka)) ga = (long)ka.GDP;
                if (EconomyEngine.KingdomStats.TryGetValue(b.data.id, out var kb)) gb = (long)kb.GDP;
                return gb.CompareTo(ga);
            });

            int rank = 1;
            foreach (var kingdom in kingdomList)
            {
                if (kingdom == null || kingdom.data == null) continue;
                string name = kingdom.data.name ?? "?";
                long gdp = 0;
                if (EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out var ks)) gdp = (long)ks.GDP;

                AddLine(UIHelpers.Lf("picker_kingdom", rank, name, gdp.ToString("F0")));

                // 煽动按钮（红）
                var btnIncite = UIHelpers.CreateButton(UIHelpers.L("picker_incite"), _content.transform, -1, 30,
                    _gameFont, UIStyles.Danger);
                var inciteTarget = kingdom;
                btnIncite.onClick.AddListener(() =>
                {
                    int n = UnrestEngine.Incite(inciteTarget);
                    // 同步刷新统计：仅当无在途周期时执行。直接调 Collect() 会投递后台周期但无人消费
                    // （_cyclePending 未置位），导致 _posting 永久滞留、年度周期停摆（S2 根因）。
                    if (!TradeSimulationWorker.IsBusy())
                    {
                        DataCollector.Collect(applySideEffects: false, postCycle: false);
                        TradeSimulationWorker.ComputeAndConsumeSync(advanceCycle: false);
                    }
                    string nName = (inciteTarget.data != null && inciteTarget.data.name != null) ? inciteTarget.data.name : "?";
                    AddLine(n > 0
                            ? UIHelpers.Lf("picker_done", nName)
                            : UIHelpers.Lf("picker_failed", nName),
                        color: n > 0 ? new Color(1f, 0.7f, 0.3f) : new Color(0.9f, 0.5f, 0.5f));
                });
                _lines.Add(btnIncite.gameObject);

                // 镇压按钮（蓝）
                var btnSuppress = UIHelpers.CreateButton(UIHelpers.L("picker_suppress"), _content.transform, -1, 30,
                    _gameFont, UIStyles.Info);
                var suppressTarget = kingdom;
                btnSuppress.onClick.AddListener(() =>
                {
                    int n = UnrestEngine.Suppress(suppressTarget);
                    // 同步刷新统计：仅当无在途周期时执行（同煽动按钮，S2 修复）
                    if (!TradeSimulationWorker.IsBusy())
                    {
                        DataCollector.Collect(applySideEffects: false, postCycle: false);
                        TradeSimulationWorker.ComputeAndConsumeSync(advanceCycle: false);
                    }
                    string nName = (suppressTarget.data != null && suppressTarget.data.name != null) ? suppressTarget.data.name : "?";
                    AddLine(n > 0
                            ? UIHelpers.Lf("picker_done_suppress", nName)
                            : UIHelpers.Lf("picker_failed_suppress", nName),
                        color: n > 0 ? new Color(0.6f, 0.85f, 1f) : new Color(0.5f, 0.7f, 0.9f));
                });
                _lines.Add(btnSuppress.gameObject);

                rank++;
            }
        }

        /// <summary>经济周期阶段名（本地化）。</summary>
        private static string PhaseName(EconomyPhase p)
        {
            switch (p)
            {
                case EconomyPhase.Boom:       return UIHelpers.L("cycle_phase_boom");
                case EconomyPhase.Recession:  return UIHelpers.L("cycle_phase_recession");
                case EconomyPhase.Depression: return UIHelpers.L("cycle_phase_depression");
                default:                      return UIHelpers.L("cycle_phase_recovery");
            }
        }

        private Button CreateTabButton(string label, Transform parent, out Image img)
        {
            var btn = UIHelpers.CreateButton(label, parent, -1, BtnHeight, _gameFont, BtnNormal, HeaderSize);
            img = btn.GetComponent<Image>();
            img.color = BtnNormal;
            img.sprite = UIHelpers.RoundedSprite();
            img.type = Image.Type.Sliced;
            var txt = btn.GetComponentInChildren<Text>();
            txt.font = _gameFont;
            txt.fontSize = Mathf.RoundToInt(HeaderSize);
            txt.color = TextColor;
            txt.alignment = TextAnchor.MiddleCenter;
            return btn;
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
