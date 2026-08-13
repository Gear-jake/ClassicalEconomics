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

        // 图表网格内边距（UI 单位，与 ChartMeshGraphic.margin 及刻度 Text 对齐）
        private const float ChartUiMargin = 4f;

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
            // 图表对象已随 _lines 清空；ChartMeshGraphic 组件自身不持有跨刷新资源，无需额外清理
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

            // 尺寸：宽自适应面板，高随宽度等比（窄窗不高耸、宽窗不扁平，任何分辨率观感一致）
            float chartW = Mathf.Round(Mathf.Max(150f, _panelRect.rect.width - Padding * 2f - 20f));
            float chartH = Mathf.Clamp(chartW * 0.24f, 110f, 260f);
            const float yAxisW = 44f;   // 左侧 GDP 数值尺宽度（UI 单位）
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

            // 图表主体（顶点渲染组件）：锚定 box 左下角、pivot=(0,0)，本地原点与刻度 Text 坐标系对齐
            var meshGo = new GameObject("GdpChartMesh", typeof(RectTransform), typeof(ChartMeshGraphic));
            meshGo.transform.SetParent(box.transform, false);
            var meshRt = meshGo.GetComponent<RectTransform>();
            meshRt.anchorMin = Vector2.zero; meshRt.anchorMax = Vector2.zero;
            meshRt.pivot = Vector2.zero;
            float chartBottom = boxH - chartH;
            meshRt.anchoredPosition = new Vector2(0f, chartBottom);
            meshRt.sizeDelta = new Vector2(chartW, chartH);
            var graph = meshGo.GetComponent<ChartMeshGraphic>();
            graph.raycastTarget = false;   // 射线由悬停层接收，图表自身不拦截
            graph.yAxisWidth = yAxisW;
            graph.margin = ChartUiMargin;

            // 组装顶点数据：vals[s][i]（NaN=王国不在榜，折线断裂）；phases 驱动阶段色带
            int sc = seriesList.Count;
            var vals = new float[sc][];
            var colors = new Color[sc];
            var areaColors = new Color[sc];
            for (int s = 0; s < sc; s++)
            {
                var ser = seriesList[s];
                var arr = new float[snaps.Count];
                for (int i = 0; i < snaps.Count; i++)
                    arr[i] = i < ser.Values.Count ? ser.Values[i] : float.NaN;
                vals[s] = arr;
                colors[s] = ser.Color;
                // 仅全球 GDP（首系列）做半透明面积填充；*0.5 后与原纹理版 0.14 一致
                areaColors[s] = s == 0 ? new Color(ser.Color.r, ser.Color.g, ser.Color.b, 0.28f) : Color.clear;
            }
            var phases = new int[snaps.Count];
            for (int i = 0; i < snaps.Count; i++) phases[i] = snaps[i].Phase;
            graph.SetChartData(vals, colors, areaColors, phases, sc, 0f, maxVal,
                drawArea: true, drawRefs: false, refHigh: 0f, refLow: 0f);

            // 悬停交互层（透明接收射线）+ 悬停竖线 + 数值 Tooltip（CE 式数据悬停查看）
            var hover = AddChartHover(box.transform, chartW, chartH, yAxisW, snaps, seriesList);
            _lines.Add(hover);

            // 左侧 GDP 数值尺（5 档刻度，与网格线对齐：底部=0 顶部=maxVal）
            // 网格线在 mesh 内 y = margin + plotH*frac，相对图表底部；刻度文本同为 box 左下角坐标系
            const float uiMargin = ChartUiMargin;
            float plotH = chartH - uiMargin * 2f;
            var scaleCol = new Color(0.92f, 0.92f, 0.92f);
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                float val = maxVal * frac;
                float yPos = chartBottom + uiMargin + plotH * frac;
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
                // mesh 内折线 x：x0 + plotW*frac（x0=yAxisW+margin, plotW=chartW-yAxisW-margin, margin=4）
                float xPos = yAxisW + (chartW - yAxisW - uiMargin) * frac;
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
            float chartH = Mathf.Clamp(chartW * 0.24f, 110f, 260f);
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

            // 图表主体（顶点渲染组件）：锚定 box 左下角、pivot=(0,0)，本地原点与刻度 Text 坐标系对齐
            var meshGo = new GameObject("GiniChartMesh", typeof(RectTransform), typeof(ChartMeshGraphic));
            meshGo.transform.SetParent(box.transform, false);
            var meshRt = meshGo.GetComponent<RectTransform>();
            meshRt.anchorMin = Vector2.zero; meshRt.anchorMax = Vector2.zero;
            meshRt.pivot = Vector2.zero;
            float chartBottom = boxH - chartH;
            meshRt.anchoredPosition = new Vector2(0f, chartBottom);
            meshRt.sizeDelta = new Vector2(chartW, chartH);
            var graph = meshGo.GetComponent<ChartMeshGraphic>();
            graph.raycastTarget = false;
            graph.yAxisWidth = yAxisW;
            graph.margin = ChartUiMargin;

            // 组装顶点数据：单系列基尼系数折线 + 阶段色带 + 危险/健康参考虚线
            var vals = new float[1][];
            var giniArr = new float[snaps.Count];
            for (int i = 0; i < snaps.Count; i++) giniArr[i] = snaps[i].GiniCoefficient;
            vals[0] = giniArr;
            var colors = new Color[] { new Color(1f, 0.62f, 0.18f) };
            var phases = new int[snaps.Count];
            for (int i = 0; i < snaps.Count; i++) phases[i] = snaps[i].Phase;
            graph.SetChartData(vals, colors, null, phases, 1, 0f, maxVal,
                drawArea: false, drawRefs: true, refHigh: cfg.CycleGiniHigh, refLow: cfg.CycleGiniLow);

            // 悬停交互层 + 悬停竖线 + Tooltip（基尼图无多系列，悬停显示年份/基尼/阶段）
            var hover = AddChartHover(box.transform, chartW, chartH, yAxisW, snaps, null);
            _lines.Add(hover);

            // 左侧基尼数值尺（0~maxVal，5 档，与网格线对齐）
            const float uiMargin = ChartUiMargin;
            float plotH = chartH - uiMargin * 2f;
            var scaleCol = new Color(0.92f, 0.92f, 0.92f);
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                float val = maxVal * frac;
                float yPos = chartBottom + uiMargin + plotH * frac;
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
                float xPos = yAxisW + (chartW - yAxisW - uiMargin) * frac;
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

        /// <summary>
        /// 为图表添加 CE 式悬停交互：透明接收层 + 跟随鼠标的数值竖线 + Tooltip 面板。
        /// 鼠标 x 经 InverseLerp 映射到快照索引，实时显示该期游戏年/全球值/各王国值（或基尼与阶段）。
        /// 返回交互层 GameObject（由调用方加入 _lines 以便清理）。
        /// </summary>
        private GameObject AddChartHover(Transform boxParent,
            float chartW, float chartH, float yAxisW,
            List<EconomySnapshot> snaps, List<ChartSeries> seriesList)
        {
            int n = snaps.Count;
            const float uiMargin = ChartUiMargin;

            // 悬停接收层：覆盖折线区域（左起数值尺右缘，右至图右缘），透明但仍接收射线
            var hoverGo = new GameObject("ChartHover", typeof(RectTransform), typeof(Image));
            hoverGo.transform.SetParent(boxParent, false);
            var hoverRt = hoverGo.GetComponent<RectTransform>();
            hoverRt.anchorMin = new Vector2(0, 1); hoverRt.anchorMax = new Vector2(1, 1);
            hoverRt.pivot = new Vector2(0.5f, 1f);
            hoverRt.anchoredPosition = Vector2.zero;
            hoverRt.offsetMin = new Vector2(yAxisW, -chartH);
            hoverRt.offsetMax = new Vector2(-uiMargin, 0);
            var hoverImg = hoverGo.GetComponent<Image>();
            hoverImg.color = new Color(0, 0, 0, 0.001f);
            hoverImg.raycastTarget = true;

            // 悬停竖线（跟随鼠标 x，初始隐藏）
            var lineGo = new GameObject("HoverLine", typeof(RectTransform), typeof(Image));
            lineGo.transform.SetParent(hoverRt, false);
            var lineRt = lineGo.GetComponent<RectTransform>();
            lineRt.anchorMin = new Vector2(0, 1); lineRt.anchorMax = new Vector2(0, 1);
            lineRt.pivot = new Vector2(0.5f, 1f);
            lineRt.anchoredPosition = Vector2.zero;
            lineRt.sizeDelta = new Vector2(2f, chartH);
            lineGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.65f);
            lineGo.SetActive(false);

            // Tooltip 面板：半透明深底 + 多行富文本（初始隐藏，ContentSizeFitter 自动贴合内容）
            var tipGo = new GameObject("ChartTooltip", typeof(RectTransform), typeof(Image), typeof(ContentSizeFitter));
            tipGo.transform.SetParent(hoverRt, false);
            var tipRt = tipGo.GetComponent<RectTransform>();
            tipRt.anchorMin = new Vector2(0, 1); tipRt.anchorMax = new Vector2(0, 1);
            tipRt.pivot = new Vector2(0.5f, 1f);
            tipRt.anchoredPosition = Vector2.zero;
            tipGo.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.07f, 0.94f);
            var tipFit = tipGo.GetComponent<ContentSizeFitter>();
            tipFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            tipFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var tipText = UIHelpers.CreateText("", tipGo.transform, 11f, Color.white, _gameFont, 17f);
            var tipTextRt = tipText.GetComponent<RectTransform>();
            tipTextRt.anchorMin = Vector2.zero; tipTextRt.anchorMax = Vector2.one;
            tipTextRt.offsetMin = new Vector2(8, 5); tipTextRt.offsetMax = new Vector2(-8, -5);
            var tipT = tipText.GetComponent<Text>();
            tipT.alignment = TextAnchor.UpperLeft;
            tipT.horizontalOverflow = HorizontalWrapMode.Overflow;
            tipGo.SetActive(false);

            var handler = hoverGo.AddComponent<ChartHoverHandler>();
            handler.Init(hoverRt, lineRt, tipRt, tipT,
                n,
                seriesList != null
                    ? (System.Func<int, string>)(idx => BuildGdpTooltip(snaps, seriesList, idx))
                    : (System.Func<int, string>)(idx => BuildGiniTooltip(snaps, idx)));
            return hoverGo;
        }

        /// <summary>构建 GDP 图悬停 Tooltip 文本：年份 + 全球 + 各在榜王国（富文本色条）。</summary>
        private static string BuildGdpTooltip(List<EconomySnapshot> snaps, List<ChartSeries> seriesList, int idx)
        {
            if (idx < 0 || idx >= snaps.Count) return "";
            var sb = new System.Text.StringBuilder(96);
            sb.Append("<b>").Append(UIHelpers.Lf("chart_year", snaps[idx].GameYear)).Append("</b>\n");
            sb.Append("<color=#FFD95A>").Append(UIHelpers.L("chart_global")).Append(" ")
              .Append(snaps[idx].GlobalGDP.ToString("F0")).Append("</color>");
            foreach (var s in seriesList)
            {
                if (s == null) continue;
                float v = idx < s.Values.Count ? s.Values[idx] : float.NaN;
                if (float.IsNaN(v)) continue; // 不在榜不显示
                sb.Append('\n').Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(s.Color))
                  .Append(">■ ").Append(s.Name).Append(" ").Append(v.ToString("F0")).Append("</color>");
            }
            return sb.ToString();
        }

        /// <summary>构建基尼图悬停 Tooltip 文本：年份 + 基尼 + 周期阶段。</summary>
        private static string BuildGiniTooltip(List<EconomySnapshot> snaps, int idx)
        {
            if (idx < 0 || idx >= snaps.Count) return "";
            var snap = snaps[idx];
            return "<b>" + UIHelpers.Lf("chart_year", snap.GameYear) + "</b>\n" +
                   "<color=#FF9E2E>" + UIHelpers.L("gini_chart_gini") + " " + snap.GiniCoefficient.ToString("F3") + "</color>\n" +
                   "<color=#8FC7FF>" + UIHelpers.L("gini_chart_phase") + " " + PhaseName((EconomyPhase)snap.Phase) + "</color>";
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

            // 核心指标卡（单行，窗口缩放保证不溢出；v0.9.1 恢复单行布局）
            var stats = new (string, string, Color)[]
            {
                ("GDP", EconomyEngine.GlobalGDP.ToString("F0"), UIStyles.Gold),
                ("人均", EconomyEngine.AvgWealth.ToString("F1"), UIStyles.Info),
                ("人口", EconomyEngine.AliveActorCount.ToString(), UIStyles.TextPrimary),
                ("基尼", EconomyEngine.GiniCoefficient.ToString("F3"), GiniColor(EconomyEngine.GiniCoefficient)),
                ("贸易", EconomyEngine.TotalTradeVolume.ToString("F0"), UIStyles.Positive),
                ("泡沫", EconomyCycleModulator.BubbleValue.ToString("F0"), UIStyles.Warning)
            };
            _lines.Add(UIComponents.CreateStatGrid(_content.transform, stats, _gameFont, contentW));

            // 地理贸易特征（v0.9.1：距离衰减/运输成本/区域套利实际生效值可视化）
            _lines.Add(UIComponents.CreateSectionHeader(_content.transform,
                UIHelpers.L("overview_geo_trade"), _gameFont, contentW));
            var geoStats = new (string, string, Color)[]
            {
                ("距离衰减", "×" + EconomyEngine.AvgDistanceFactor.ToString("F2"),
                    DistanceFactorColor(EconomyEngine.AvgDistanceFactor)),
                ("运输成本", (EconomyEngine.TransportCost * 100f).ToString("F0") + "%",
                    UIStyles.Warning),
                ("套利权重", (EconomyEngine.PriceDiffWeight * 100f).ToString("F0") + "%",
                    UIStyles.Info),
                ("价格离散", EconomyEngine.PriceDispersion.ToString("F3"),
                    PriceDispersionColor(EconomyEngine.PriceDispersion))
            };
            _lines.Add(UIComponents.CreateStatGrid(_content.transform, geoStats, _gameFont, contentW));

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
                    k.LocalPrice.ToString("F2"),
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

        /// <summary>价格离散度语义色（≥0.2 高离散/套利空间大 → 琥珀；≥0.1 中等 → 信息蓝；其他弱色）。</summary>
        private static Color PriceDispersionColor(float pd)
        {
            return pd >= 0.2f ? UIStyles.Warning : pd >= 0.1f ? UIStyles.Info : UIStyles.TextSecondary;
        }

        /// <summary>距离衰减因子语义色（≥0.9 几乎无衰减 → 绿；≥0.7 中等 → 信息蓝；更低远程贸易弱 → 琥珀）。</summary>
        private static Color DistanceFactorColor(float f)
        {
            return f >= 0.9f ? UIStyles.Positive : f >= 0.7f ? UIStyles.Info : UIStyles.Warning;
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
            if (_tipText != null && _textProvider != null)
            {
                _tipText.text = _textProvider(index);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_tipRt); // 文本长度变化 → 刷新面板宽
            }
            float tipW = _tipRt != null ? _tipRt.rect.width : 120f;
            float maxX = rect.width - tipW * 0.5f - 2f;
            float tipX = Mathf.Clamp(localPoint.x + rect.width * 0.5f, tipW * 0.5f + 2f, maxX);
            if (maxX < tipW * 0.5f + 2f) tipX = rect.width * 0.5f; // 面板太窄时居中
            _tipRt.anchoredPosition = new Vector2(tipX, -6f);
        }

        private void SetVisible(bool visible)
        {
            if (_lineRt != null) _lineRt.gameObject.SetActive(visible);
            if (_tipRt != null)
            {
                _tipRt.gameObject.SetActive(visible);
                // 首次显示时强制重算布局，保证 _tipRt.rect 宽度可用（clamp 定位依赖它）
                if (visible && _tipText != null)
                {
                    _tipText.text = _textProvider != null ? _textProvider(0) : "";
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_tipRt);
                }
            }
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
