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

        private static readonly Color Bg           = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color BtnNormal = new Color(0.18f, 0.18f, 0.22f, 0.85f);
        private static readonly Color BtnActive = new Color(0.9f, 0.75f, 0.2f, 0.95f);
        private static readonly Color TextColor = Color.white;
        private static readonly Color HeaderColor = new Color(1f, 0.9f, 0.4f);
        private static readonly Color DividerColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

        private const float PanelWidth = 340f;
        private const float PanelHeight = 520f;
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
        /// 全球财富 + 前三王国财富折线，左侧数值尺（0~峰值），底部年份坐标尺。
        /// 一次显示最近 50 条记录（历史缓冲容量 100，足够支撑）。
        /// </summary>
        private void AddGdpChart(int maxPoints = 50)
        {
            var snaps = HistoryService.GetRecent(maxPoints);
            if (snaps.Count < 2)
            {
                AddLine(UIHelpers.L("chart_no_data"), color: new Color(0.7f, 0.7f, 0.7f));
                return;
            }

            // 构建系列：全球 GDP + Top3 王国（按最近快照 GDP 排名）
            var seriesList = BuildChartSeries(snaps);

            int lastYear = snaps[snaps.Count - 1].GameYear;
            AddLine(UIHelpers.Lf("chart_title", lastYear, snaps.Count), true);
            AddChartLegend(_content.transform, seriesList);

            // 尺寸：宽自适应面板，高固定扁平（平面直角坐标系观感）
            float chartW = Mathf.Round(Mathf.Max(150f, _panelRect.rect.width - Padding * 2f - 20f));
            const float chartH = 120f;
            const float yAxisW = 44f;   // 左侧 GDP 数值尺宽度
            const float bottomH = 22f;  // 底部年份坐标尺高度
            float boxH = chartH + bottomH + 4f;

            // Y 轴范围：0 ~ 所有系列最大值
            float maxVal = 0f;
            foreach (var s in seriesList)
                foreach (var v in s.Values)
                    if (v > maxVal) maxVal = v;
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

        // ===== BuildChartSeries 复用缓冲（每次刷新图表时复用，避免 GC）=====
        private static readonly List<ChartSeries> _seriesPool = new List<ChartSeries>(4);
        private static readonly List<KingdomStats> _top3Pool = new List<KingdomStats>(3);
        // KingdomId -> 每个 snap 的 GDP 数组（缺失记为 -1，沿用前值）；KeyDictionary 避免重复构造
        private static readonly Dictionary<long, long[]> _gdpIndex = new Dictionary<long, long[]>();

        /// <summary>
        /// 构建图表系列：全球 GDP + Top3 王国 GDP（按最近快照排名）。
        /// 王国在某次快照缺失时沿用前一值，保持折线连续。
        /// 算法优化：原实现为 O(snaps × top3 × kingdomsPerSnap)（每条 snap 都 Find 一遍），
        /// 改为一次遍历所有 snap 构建索引 O(snaps × kingdomsPerSnap)，再 O(snaps × top3) 填充，
        /// 消除 List.Find 的 O(N) 线性查找。复用静态缓冲避免每次刷新分配。
        /// </summary>
        private static List<ChartSeries> BuildChartSeries(List<EconomySnapshot> snaps)
        {
            var result = _seriesPool;
            // 归还上一轮的 ChartSeries 对象（Values 列表也复用，避免新建）
            for (int i = 0; i < result.Count; i++)
            {
                result[i].Values.Clear();
                _chartSeriesEntryPool.Add(result[i]);
            }
            result.Clear();

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

            var lastK = snaps[snaps.Count - 1].Kingdoms;
            var top3 = _top3Pool;
            top3.Clear();
            top3.AddRange(lastK);
            top3.Sort((a, b) => b.GDP.CompareTo(a.GDP));
            if (top3.Count > 3) top3.RemoveRange(3, top3.Count - 3);

            var kColors = new[]
            {
                new Color(0.3f, 0.8f, 1f),     // 青
                new Color(0.45f, 0.9f, 0.45f), // 绿
                new Color(0.95f, 0.55f, 0.8f)  // 品红
            };

            // 预建索引：KingdomId -> 每个 snap 的 GDP 数组（缺失记为 -1）
            // 只收集 top3 出现的 KingdomId，避免为所有历史王国分配数组
            var gdpIndex = _gdpIndex;
            gdpIndex.Clear();
            // 先为 top3 分配数组（清空旧条目并标记为 -1）
            for (int i = 0; i < top3.Count; i++)
            {
                var arr = new long[snaps.Count];
                for (int j = 0; j < snaps.Count; j++) arr[j] = -1L;
                gdpIndex[top3[i].KingdomId] = arr;
            }
            // 一次遍历所有 snap 填充索引
            for (int j = 0; j < snaps.Count; j++)
            {
                var kingdoms = snaps[j].Kingdoms;
                if (kingdoms == null) continue;
                for (int i = 0; i < kingdoms.Count; i++)
                {
                    var k = kingdoms[i];
                    if (gdpIndex.TryGetValue(k.KingdomId, out var arr))
                        arr[j] = k.GDP;
                }
            }

            // 填充 top3 系列（缺失沿用前值，保持折线连续）
            for (int i = 0; i < top3.Count; i++)
            {
                var ks = top3[i];
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
                series.Color = kColors[i % kColors.Length];

                long[] arr = gdpIndex[ks.KingdomId];
                float prev = 0f;
                for (int j = 0; j < snaps.Count; j++)
                {
                    long v = arr[j];
                    if (v >= 0L) prev = v;
                    series.Values.Add(prev);
                }
                result.Add(series);
            }
            return result;
        }

        // ChartSeries 对象池（与 _seriesPool 配合，Values 列表也复用）
        private static readonly List<ChartSeries> _chartSeriesEntryPool = new List<ChartSeries>(4);

        /// <summary>
        /// 计算 GDP 图表内容指纹：全球 GDP 序列 + 各系列（Top3 王国）名称与数值序列。
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
                    h = h * 31 + (int)ser.Values[i];
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
        /// 图例行：色条 + 名称，HorizontalLayoutGroup 自动排布。
        /// </summary>
        private void AddChartLegend(Transform parent, List<ChartSeries> series)
        {
            var row = new GameObject("ChartLegend", typeof(RectTransform),
                typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var rowLg = row.GetComponent<HorizontalLayoutGroup>();
            rowLg.spacing = 12f;
            rowLg.childAlignment = TextAnchor.MiddleLeft;
            rowLg.childControlWidth = false;
            rowLg.childControlHeight = true;
            rowLg.childForceExpandWidth = false;
            rowLg.childForceExpandHeight = false;
            var rowRt = row.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(0, 20f);
            row.GetComponent<LayoutElement>().preferredHeight = 20f;
            _lines.Add(row);

            foreach (var s in series)
            {
                var item = new GameObject("LegendItem", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                item.transform.SetParent(row.transform, false);
                var itemLg = item.GetComponent<HorizontalLayoutGroup>();
                itemLg.spacing = 4f;
                itemLg.childAlignment = TextAnchor.MiddleLeft;
                itemLg.childControlWidth = false;
                itemLg.childControlHeight = true;
                itemLg.childForceExpandWidth = false;
                itemLg.childForceExpandHeight = false;

                // 色条
                var sw = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
                sw.transform.SetParent(item.transform, false);
                sw.GetComponent<Image>().color = s.Color;
                sw.GetComponent<Image>().raycastTarget = false;
                sw.GetComponent<RectTransform>().sizeDelta = new Vector2(18f, 5f);
                var swEl = sw.AddComponent<LayoutElement>();
                swEl.preferredWidth = 18f;
                swEl.preferredHeight = 5f;

                // 名称
                var nm = UIHelpers.CreateText(s.Name, item.transform, 10f, TextColor, _gameFont, 18f);
                var nmT = nm.GetComponent<Text>();
                nmT.alignment = TextAnchor.MiddleLeft;
                float nameW = nmT.preferredWidth + 2f;
                nm.GetComponent<RectTransform>().sizeDelta = new Vector2(nameW, 18f);
                var nmEl = nm.AddComponent<LayoutElement>();
                nmEl.preferredWidth = nameW;
                nmEl.preferredHeight = 18f;

                // 图例项
                var itemEl = item.AddComponent<LayoutElement>();
                itemEl.preferredWidth = 18f + 4f + nameW;
                itemEl.preferredHeight = 18f;
                item.GetComponent<RectTransform>().sizeDelta = new Vector2(itemEl.preferredWidth, 18f);
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
        /// 绘制多线折线图纹理：暗色底、横向网格、每个系列一条折线，
        /// 首个系列（全球 GDP）附加半透明面积填充。y=0 在底部，显示时天然正向。
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

            // 横向网格线（含底部坐标轴），从数值尺右缘开始
            var grid = new Color(0.35f, 0.35f, 0.4f, 0.45f);
            for (int gi = 0; gi <= 4; gi++)
            {
                int y = mB + ch * gi / 4;
                for (int x = mL; x < w - mR; x++) px[y * w + x] = grid;
            }

            // 每个系列绘制折线
            for (int si = 0; si < series.Count; si++)
            {
                var s = series[si];
                var pts = new Vector2Int[n];
                for (int i = 0; i < n; i++)
                {
                    float v = i < s.Values.Count ? s.Values[i] : 0f;
                    pts[i].x = mL + Mathf.RoundToInt(cw * i / (float)(n - 1));
                    pts[i].y = mB + Mathf.RoundToInt(ch * (v / maxVal));
                }
                // 首个系列（全球）附加面积填充
                if (si == 0)
                {
                    var fill = s.Color;
                    fill.a = 0.14f;
                    for (int i = 0; i < n - 1; i++)
                        FillAreaSegment(px, w, h, pts[i], pts[i + 1], mB, fill);
                }
                // 折线 + 末端高亮点
                for (int i = 0; i < n - 1; i++)
                    DrawLine(px, w, h, pts[i], pts[i + 1], s.Color);
                DrawDot(px, w, h, pts[n - 1], s.Color, 2);
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
            AddLine(UIHelpers.Lf("overview_cycle", EconomyEngine.CycleIndex), true);
            AddLine("");
            // 经济周期状态（Phase 4 周期调制器）
            AddLine(UIHelpers.Lf("cycle_phase", PhaseName(EconomyCycleModulator.CurrentPhase)), true);
            AddLine(UIHelpers.Lf("cycle_detail",
                EconomyCycleModulator.PhaseDuration,
                EconomyCycleModulator.GrowthRate.ToString("+0.0%;-0.0%"),
                EconomyCycleModulator.BubbleValue.ToString("F0")),
                color: new Color(0.8f, 0.9f, 0.7f));
            AddLine("");
            AddLine(UIHelpers.Lf("overview_gdp", EconomyEngine.GlobalGDP.ToString("F0")));
            AddLine(UIHelpers.Lf("overview_avg", EconomyEngine.AvgWealth.ToString("F2")));
            AddLine(UIHelpers.Lf("overview_pop", EconomyEngine.AliveActorCount));
            AddLine(UIHelpers.Lf("overview_gini", EconomyEngine.GiniCoefficient.ToString("F3")));
            AddLine(UIHelpers.Lf("overview_trade", EconomyEngine.TotalTradeVolume.ToString("F0")));
            AddDivider(DividerColor);
            AddLine(UIHelpers.L("overview_kingdoms"), true);

            var top = EconomyEngine.TopKingdoms(8);
            if (top.Count == 0)
            {
                AddLine(UIHelpers.L("overview_no_kingdom"), color: new Color(0.7f, 0.7f, 0.7f));
            }
            int rank = 1;
            foreach (var k in top)
            {
                AddLine(UIHelpers.Lf("overview_kingdom_name", rank, k.KingdomName));
                string trade = (k.TradeBalance > 0 ? "+" : "") + k.TradeBalance.ToString("F0");
                string pressure = k.PopulationCapacity > 0
                    ? (k.Population * 100 / k.PopulationCapacity) + "%"
                    : "-";
                AddLine(UIHelpers.Lf("overview_kingdom_detail",
                    k.GDP.ToString("F0"), k.AvgWealth.ToString("F1"),
                    k.GiniCoefficient.ToString("F2"), k.ActorCount, trade, pressure));
                rank++;
            }

            // 社会震荡状态（用状态表示：高基尼累积 X/10 年 / 暴动中）
            AddDivider(DividerColor);
            AddLine(UIHelpers.L("unrest_state_title"), true);
            int stateCount = 0;
            foreach (var k in top)
            {
                int st = UnrestEngine.GetState(k.KingdomId, out int elapsed);
                if (st == 1)
                {
                    AddLine(UIHelpers.Lf("unrest_state_accum", k.KingdomName, elapsed),
                        color: new Color(1f, 0.8f, 0.3f));
                    stateCount++;
                }
                else if (st == 2)
                {
                    AddLine(UIHelpers.Lf("unrest_state_active", k.KingdomName),
                        color: new Color(1f, 0.4f, 0.2f));
                    stateCount++;
                }
            }
            if (stateCount == 0)
            {
                AddLine(UIHelpers.L("unrest_state_none"), color: new Color(0.7f, 0.7f, 0.7f));
            }
            AddLine(UIHelpers.Lf("unrest_state_threshold", UnrestConfig.Instance.GiniThreshold.ToString("F3")),
                color: new Color(0.7f, 0.85f, 0.7f));
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
                    _gameFont, new Color(0.7f, 0.25f, 0.15f, 0.9f));
                var inciteTarget = kingdom;
                btnIncite.onClick.AddListener(() =>
                {
                    int n = UnrestEngine.Incite(inciteTarget);
                    DataCollector.Collect(); // 内部已含统计累积
                    string nName = (inciteTarget.data != null && inciteTarget.data.name != null) ? inciteTarget.data.name : "?";
                    AddLine(n > 0
                            ? UIHelpers.Lf("picker_done", nName)
                            : UIHelpers.Lf("picker_failed", nName),
                        color: n > 0 ? new Color(1f, 0.7f, 0.3f) : new Color(0.9f, 0.5f, 0.5f));
                });
                _lines.Add(btnIncite.gameObject);

                // 镇压按钮（蓝）
                var btnSuppress = UIHelpers.CreateButton(UIHelpers.L("picker_suppress"), _content.transform, -1, 30,
                    _gameFont, new Color(0.2f, 0.4f, 0.7f, 0.9f));
                var suppressTarget = kingdom;
                btnSuppress.onClick.AddListener(() =>
                {
                    int n = UnrestEngine.Suppress(suppressTarget);
                    DataCollector.Collect(); // 内部已含统计累积
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
