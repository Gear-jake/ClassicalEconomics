using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    /// <summary>【W2 分层】EconomyHUD 的图表层：GDP 多国折线图、贫富差距趋势图，及其悬停交互、图例与数据系列构建。
    ///
    /// 【W2.6 常驻化】图表宿主（容器 / ChartMeshGraphic / 悬停交互层 / 刻度文字 / 图例行）**只创建一次**，
    /// 之后每次刷新只更新数据与文字 —— 于是 ChartMeshGraphic 的 `_lastSig` 脏标记**真正生效**
    /// （原先每次刷新都新建 GameObject，签名从 int.MinValue 开始，等于每刷必重建）。
    /// 配色一律取自本体（见 VanillaChartPalette）。</summary>
    public partial class EconomyHUD : FloatingWindow
    {
        // ===== 【W2.6】常驻图表宿主 =====
        private GameObject _gdpBox;
        private RectTransform _gdpBoxRt;
        private LayoutElement _gdpBoxEl;
        private ChartMeshGraphic _gdpGraph;
        private ChartHoverHandler _gdpHover;
        private RectTransform _gdpHoverRt, _gdpHoverLineRt, _gdpTipRt;
        private Text _gdpTipText;
        private readonly Text[] _gdpScale = new Text[5];   // 纵轴刻度（0~maxVal）
        private readonly Text[] _gdpYears = new Text[5];   // 横轴年份

        private GameObject _giniBox;
        private RectTransform _giniBoxRt;
        private LayoutElement _giniBoxEl;
        private ChartMeshGraphic _giniGraph;
        private ChartHoverHandler _giniHover;
        private RectTransform _giniHoverRt, _giniHoverLineRt, _giniTipRt;
        private Text _giniTipText;
        private readonly Text[] _giniScale = new Text[5];
        private readonly Text[] _giniYears = new Text[5];

        // 图例行（常驻，按需增长；超出本次系列数的行隐藏而不销毁）
        private readonly List<GameObject> _legendRows = new List<GameObject>(12);
        private readonly List<Image> _legendSwatch = new List<Image>(12);
        private readonly List<Text> _legendName = new List<Text>(12);
        private readonly List<Text> _legendValue = new List<Text>(12);

        /// <summary>本次刷新实际显示的行数（供隐藏多余行；-1 = 全部隐藏）。</summary>
        private int _legendActive = -1;

        private void BuildChart()
        {
            AddGdpChart();
            AddGiniChart();
        }

        /// <summary>切到非图表页 / 世界退出时：把常驻宿主收起来（不销毁，下次直接复用）。</summary>
        private void HideChartHosts()
        {
            if (_gdpBox != null) _gdpBox.SetActive(false);
            if (_giniBox != null) _giniBox.SetActive(false);
            for (int i = 0; i < _legendRows.Count; i++)
                if (_legendRows[i] != null) _legendRows[i].SetActive(false);
            _legendActive = -1;
        }

        /// <summary>世界退出时重置图表宿主（常驻对象跨世界复用，不需要销毁，只需收起来）。</summary>
        private void ReleaseChartHosts()
        {
            HideChartHosts();
        }

        // =====================================================================
        // GDP 多国折线图
        // =====================================================================

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
                if (_gdpBox != null) _gdpBox.SetActive(false);
                AddLegendRows(null);
                AddLine(UIHelpers.L("chart_no_data"), color: new Color(0.7f, 0.7f, 0.7f));
                return;
            }

            // 构建系列：全球 GDP + 动态 TopN 王国（按每期快照 GDP 排名，折线随排名出现/断裂）
            var seriesList = BuildDynamicSeries(snaps, topN);

            int lastYear = snaps[snaps.Count - 1].GameYear;
            AddLine(UIHelpers.Lf("chart_title_dyn", topN, lastYear, snaps.Count), true);
            var titleLine = _lines[_lines.Count - 1];

            // 图例（常驻行，插到标题之后）
            AddLegendRows(seriesList);
            PlaceLegendRowsAfter(titleLine);

            // 宏观指标摘要（最新快照）
            var lastSnap = snaps[snaps.Count - 1];
            AddLine(UIHelpers.Lf("chart_macro",
                    UIHelpers.FormatCompact((long)lastSnap.TotalProduction),
                    lastSnap.PriceIndex.ToString("F2"),
                    lastSnap.AliveActorCount),
                color: VanillaChartPalette.Brighter);
            AddLine(UIHelpers.Lf("chart_macro_avg",
                    lastSnap.AvgWealth.ToString("F1"),
                    lastSnap.GiniCoefficient.ToString("F3")),
                color: VanillaChartPalette.Brighter);
            AddLine(UIHelpers.L("chart_legend_hint"), color: VanillaChartPalette.Grey);
            var hintLine = _lines[_lines.Count - 1];

            // 尺寸：宽自适应面板，高随宽度等比（窄窗不高耸、宽窗不扁平，任何分辨率观感一致）
            float chartW = Mathf.Round(Mathf.Max(150f, _panelRect.rect.width - Padding * 2f - 20f));
            float chartH = Mathf.Clamp(chartW * 0.24f, 110f, 260f);
            const float yAxisW = 44f;   // 左侧 GDP 数值尺宽度（UI 单位）
            const float bottomH = 22f;  // 底部年份坐标尺高度
            float boxH = chartH + bottomH + 4f;
            float chartBottom = boxH - chartH;

            // Y 轴范围：0 ~ 所有系列最大值（NaN = 不在榜，跳过）
            float maxVal = 0f;
            foreach (var s in seriesList)
                for (int i = 0; i < s.Values.Count; i++)
                {
                    float v = s.Values[i];
                    if (!float.IsNaN(v) && v > maxVal) maxVal = v;
                }
            if (maxVal <= 0f) maxVal = 1f;

            EnsureGdpHost(snaps);
            _gdpBox.SetActive(true);

            // 容器尺寸（固定首选尺寸，防止 VerticalLayoutGroup 布局错位导致与下方内容重叠）
            _gdpBoxRt.sizeDelta = new Vector2(0, boxH);
            _gdpBoxEl.preferredWidth = chartW;
            _gdpBoxEl.preferredHeight = boxH;

            // 图表主体：锚定 box 左下角、pivot=(0,0)，本地原点与刻度 Text 坐标系对齐
            var meshRt = _gdpGraph.rectTransform;
            meshRt.anchoredPosition = new Vector2(0f, chartBottom);
            meshRt.sizeDelta = new Vector2(chartW, chartH);
            _gdpGraph.yAxisWidth = yAxisW;
            _gdpGraph.margin = ChartUiMargin;

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
                // 仅全球 GDP（首系列）做面积填充；颜色交给组件按本体渐变口径（0.4 → 0.1）绘制
                areaColors[s] = s == 0 ? ser.Color : Color.clear;
            }
            var phases = new int[snaps.Count];
            for (int i = 0; i < snaps.Count; i++) phases[i] = snaps[i].Phase;
            _gdpGraph.SetChartData(vals, colors, areaColors, phases, sc, 0f, maxVal,
                drawArea: true, drawRefs: false, refHigh: 0f, refLow: 0f);

            // 悬停交互层：位置随尺寸更新；数据源是复用缓冲（同一对象），故闭包无需重建
            LayoutHoverRig(_gdpHoverRt, _gdpHoverLineRt, chartH, yAxisW);
            if (_gdpHover != null) _gdpHover.SetPointCount(snaps.Count);

            // 左侧 GDP 数值尺（5 档刻度，与网格线对齐：底部=0 顶部=maxVal）
            // 网格线在 mesh 内 y = margin + plotH*frac，相对图表底部；刻度文本同为 box 左下角坐标系
            const float uiMargin = ChartUiMargin;
            float plotH = chartH - uiMargin * 2f;
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                float val = maxVal * frac;
                float yPos = chartBottom + uiMargin + plotH * frac;
                var t = _gdpScale[gi];
                if (t != null)
                {
                    t.text = val.ToString("F0");
                    // 纵轴文字按取值上色：本体 GraphHelpers.verticalFormat 口径
                    t.color = VanillaChartPalette.AxisLabel(val, maxVal);
                    var tRt = t.rectTransform;
                    tRt.anchoredPosition = new Vector2(yAxisW, yPos);
                }
            }

            // 底部年份坐标尺：0/25/50/75/100% 处显示对应采集点的游戏年
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                int idx = Mathf.RoundToInt((snaps.Count - 1) * frac);
                // mesh 内折线 x：x0 + plotW*frac（x0=yAxisW+margin, plotW=chartW-yAxisW-margin, margin=4）
                float xPos = yAxisW + (chartW - yAxisW - uiMargin) * frac;
                var t = _gdpYears[gi];
                if (t != null)
                {
                    t.text = UIHelpers.Lf("chart_year", snaps[idx].GameYear);
                    t.rectTransform.anchoredPosition = new Vector2(xPos, 3f);
                }
            }

            // 图表容器插到提示行之后（此后追加的汇总行自然排在图表下方）
            _gdpBox.transform.SetSiblingIndex(hintLine.transform.GetSiblingIndex() + 1);

            long last = snaps[snaps.Count - 1].GlobalGDP;
            AddLine(UIHelpers.Lf("chart_summary", UIHelpers.FormatCompact((long)maxVal), UIHelpers.FormatCompact(last)),
                color: VanillaChartPalette.Brighter);
        }

        /// <summary>创建 GDP 图表宿主（仅一次）：容器 + 顶点图表 + 悬停交互 + 10 个刻度文字。
        /// 仅在首次创建时用到 snaps（给悬停闭包绑定数据源），之后刷新不再需要。</summary>
        private void EnsureGdpHost(List<EconomySnapshot> snaps)
        {
            if (_gdpBox != null) return;

            _gdpBox = new GameObject("GdpChartBox", typeof(RectTransform), typeof(LayoutElement));
            _gdpBox.transform.SetParent(_content.transform, false);
            _gdpBoxRt = _gdpBox.GetComponent<RectTransform>();
            _gdpBoxEl = _gdpBox.GetComponent<LayoutElement>();

            var meshGo = new GameObject("GdpChartMesh", typeof(RectTransform), typeof(ChartMeshGraphic));
            meshGo.transform.SetParent(_gdpBox.transform, false);
            var meshRt = meshGo.GetComponent<RectTransform>();
            meshRt.anchorMin = Vector2.zero; meshRt.anchorMax = Vector2.zero;
            meshRt.pivot = Vector2.zero;
            _gdpGraph = meshGo.GetComponent<ChartMeshGraphic>();
            _gdpGraph.raycastTarget = false;   // 射线由悬停层接收，图表自身不拦截
            _gdpGraph.gridColor = VanillaChartPalette.GridLine;

            _gdpHover = CreateHoverRig(_gdpBox.transform, true, snaps,
                out _gdpHoverRt, out _gdpHoverLineRt, out _gdpTipRt, out _gdpTipText);

            // 刻度文字：纵轴 5 + 横轴 5（常驻，刷新时只改文本/位置/颜色）
            const float yAxisW = 44f;
            for (int gi = 0; gi <= 4; gi++)
            {
                var tGo = UIHelpers.CreateText("", _gdpBox.transform, Fs(10f), VanillaChartPalette.Brighter,
                    _gameFont, Fs(13f), "GdpScale" + gi);
                var tRt = tGo.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.zero;
                tRt.pivot = new Vector2(1f, 0.5f);
                tRt.sizeDelta = new Vector2(yAxisW - 4f, 13f);
                var t = tGo.GetComponent<Text>();
                t.alignment = TextAnchor.MiddleRight;
                t.fontStyle = FontStyle.Bold;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                _gdpScale[gi] = t;
            }
            for (int gi = 0; gi <= 4; gi++)
            {
                var xGo = UIHelpers.CreateText("", _gdpBox.transform, Fs(9f), VanillaChartPalette.Brighter,
                    _gameFont, Fs(14f), "GdpYear" + gi);
                var xRt = xGo.GetComponent<RectTransform>();
                xRt.anchorMin = Vector2.zero; xRt.anchorMax = Vector2.zero;
                xRt.pivot = new Vector2(gi == 0 ? 0f : (gi == 4 ? 1f : 0.5f), 0f);
                xRt.sizeDelta = new Vector2(44f, 14f);
                xGo.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
                _gdpYears[gi] = xGo.GetComponent<Text>();
            }
        }

        // =====================================================================
        // 贫富差距趋势图
        // =====================================================================

        /// <summary>
        /// 贫富差距趋势图：全球基尼系数折线 + 危险线/健康线参考虚线 + 经济阶段背景色带。
        /// 直观呈现"贫富差距驱动周期阶段"的因果，与财富图并列展示。
        /// 一次显示最近 50 条记录。
        /// </summary>
        private void AddGiniChart(int maxPoints = 50)
        {
            var snaps = HistoryService.GetRecent(maxPoints);
            if (snaps.Count < 2)
            {
                if (_giniBox != null) _giniBox.SetActive(false);
                return;
            }

            int lastYear = snaps[snaps.Count - 1].GameYear;
            AddLine(UIHelpers.Lf("gini_chart_title", lastYear, snaps.Count), true);
            AddLine(UIHelpers.L("gini_legend_phase"), color: VanillaChartPalette.Grey);
            var legendLine = _lines[_lines.Count - 1];

            float chartW = Mathf.Round(Mathf.Max(150f, _panelRect.rect.width - Padding * 2f - 20f));
            float chartH = Mathf.Clamp(chartW * 0.24f, 110f, 260f);
            const float yAxisW = 40f;
            const float bottomH = 22f;
            float boxH = chartH + bottomH + 4f;
            float chartBottom = boxH - chartH;

            // Y 范围：0 ~ max(0.8, 快照最大基尼)，保证曲线与参考线比例可见
            float maxVal = 0.8f;
            foreach (var s in snaps)
                if (s.GiniCoefficient > maxVal) maxVal = s.GiniCoefficient;

            var cfg = UnrestConfig.Instance;

            EnsureGiniHost(snaps);
            _giniBox.SetActive(true);
            _giniBoxRt.sizeDelta = new Vector2(0, boxH);
            _giniBoxEl.preferredWidth = chartW;
            _giniBoxEl.preferredHeight = boxH;

            var meshRt = _giniGraph.rectTransform;
            meshRt.anchoredPosition = new Vector2(0f, chartBottom);
            meshRt.sizeDelta = new Vector2(chartW, chartH);
            _giniGraph.yAxisWidth = yAxisW;
            _giniGraph.margin = ChartUiMargin;

            // 组装顶点数据：单系列基尼系数折线 + 阶段色带 + 危险/健康参考虚线
            var vals = new float[1][];
            var giniArr = new float[snaps.Count];
            for (int i = 0; i < snaps.Count; i++) giniArr[i] = snaps[i].GiniCoefficient;
            vals[0] = giniArr;
            var colors = new Color[] { VanillaChartPalette.SeriesGini };
            var phases = new int[snaps.Count];
            for (int i = 0; i < snaps.Count; i++) phases[i] = snaps[i].Phase;
            _giniGraph.SetChartData(vals, colors, null, phases, 1, 0f, maxVal,
                drawArea: false, drawRefs: true, refHigh: cfg.CycleGiniHigh, refLow: cfg.CycleGiniLow);

            LayoutHoverRig(_giniHoverRt, _giniHoverLineRt, chartH, yAxisW);
            if (_giniHover != null) _giniHover.SetPointCount(snaps.Count);

            // 左侧基尼数值尺（0~maxVal，5 档，与网格线对齐）
            const float uiMargin = ChartUiMargin;
            float plotH = chartH - uiMargin * 2f;
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                float val = maxVal * frac;
                float yPos = chartBottom + uiMargin + plotH * frac;
                var t = _giniScale[gi];
                if (t != null)
                {
                    t.text = val.ToString("F2");
                    t.color = VanillaChartPalette.AxisLabel(val, maxVal);
                    t.rectTransform.anchoredPosition = new Vector2(yAxisW, yPos);
                }
            }

            // 底部年份坐标尺
            for (int gi = 0; gi <= 4; gi++)
            {
                float frac = gi / 4f;
                int idx = Mathf.RoundToInt((snaps.Count - 1) * frac);
                float xPos = yAxisW + (chartW - yAxisW - uiMargin) * frac;
                var t = _giniYears[gi];
                if (t != null)
                {
                    t.text = UIHelpers.Lf("chart_year", snaps[idx].GameYear);
                    t.rectTransform.anchoredPosition = new Vector2(xPos, 3f);
                }
            }

            _giniBox.transform.SetSiblingIndex(legendLine.transform.GetSiblingIndex() + 1);

            // 参考线说明（色块取自本体语义色，与虚线同源）
            AddLine(UIHelpers.L("gini_legend_danger"), color: VanillaChartPalette.Red);
            AddLine(UIHelpers.L("gini_legend_health"), color: VanillaChartPalette.Positive);
        }

        /// <summary>创建基尼图表宿主（仅一次）。</summary>
        private void EnsureGiniHost(List<EconomySnapshot> snaps)
        {
            if (_giniBox != null) return;

            _giniBox = new GameObject("GiniChartBox", typeof(RectTransform), typeof(LayoutElement));
            _giniBox.transform.SetParent(_content.transform, false);
            _giniBoxRt = _giniBox.GetComponent<RectTransform>();
            _giniBoxEl = _giniBox.GetComponent<LayoutElement>();

            var meshGo = new GameObject("GiniChartMesh", typeof(RectTransform), typeof(ChartMeshGraphic));
            meshGo.transform.SetParent(_giniBox.transform, false);
            var meshRt = meshGo.GetComponent<RectTransform>();
            meshRt.anchorMin = Vector2.zero; meshRt.anchorMax = Vector2.zero;
            meshRt.pivot = Vector2.zero;
            _giniGraph = meshGo.GetComponent<ChartMeshGraphic>();
            _giniGraph.raycastTarget = false;
            _giniGraph.gridColor = VanillaChartPalette.GridLine;
            _giniGraph.refHighColor = VanillaChartPalette.RefHigh;
            _giniGraph.refLowColor = VanillaChartPalette.RefLow;

            _giniHover = CreateHoverRig(_giniBox.transform, false, snaps,
                out _giniHoverRt, out _giniHoverLineRt, out _giniTipRt, out _giniTipText);

            const float yAxisW = 40f;
            for (int gi = 0; gi <= 4; gi++)
            {
                var tGo = UIHelpers.CreateText("", _giniBox.transform, Fs(10f), VanillaChartPalette.Brighter,
                    _gameFont, Fs(13f), "GiniScale" + gi);
                var tRt = tGo.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.zero;
                tRt.pivot = new Vector2(1f, 0.5f);
                tRt.sizeDelta = new Vector2(yAxisW - 4f, 13f);
                var t = tGo.GetComponent<Text>();
                t.alignment = TextAnchor.MiddleRight;
                t.fontStyle = FontStyle.Bold;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                _giniScale[gi] = t;
            }
            for (int gi = 0; gi <= 4; gi++)
            {
                var xGo = UIHelpers.CreateText("", _giniBox.transform, Fs(9f), VanillaChartPalette.Brighter,
                    _gameFont, Fs(14f), "GiniYear" + gi);
                var xRt = xGo.GetComponent<RectTransform>();
                xRt.anchorMin = Vector2.zero; xRt.anchorMax = Vector2.zero;
                xRt.pivot = new Vector2(gi == 0 ? 0f : (gi == 4 ? 1f : 0.5f), 0f);
                xRt.sizeDelta = new Vector2(44f, 14f);
                xGo.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
                _giniYears[gi] = xGo.GetComponent<Text>();
            }
        }

        // =====================================================================
        // 悬停交互宿主（创建一次，数据源是复用缓冲 → 闭包无需重建）
        // =====================================================================

        /// <summary>
        /// 创建悬停交互三件套（透明接收层 + 跟随鼠标的竖线 + Tooltip），只做一次。
        /// 数据源（HistoryService 的复用缓冲 / BuildDynamicSeries 的系列池）**对象身份稳定**，
        /// 因此闭包绑定一次即可，刷新时只更新点数量（SetPointCount）。
        /// ⚠ 这里**不得再调用 HistoryService.GetRecent**：它返回的是全模组共用的复用缓冲，
        /// 在构建图表的中途再取一次会原地重填同一个 List（当前内容相同所以无害，但属于别名隐患）。
        /// </summary>
        private ChartHoverHandler CreateHoverRig(Transform boxParent, bool gdp,
            List<EconomySnapshot> snaps,
            out RectTransform hoverRt, out RectTransform lineRt, out RectTransform tipRt, out Text tipText)
        {
            // 悬停接收层：覆盖折线区域（左起数值尺右缘，右至图右缘），透明但仍接收射线
            var hoverGo = new GameObject("ChartHover", typeof(RectTransform), typeof(Image));
            hoverGo.transform.SetParent(boxParent, false);
            hoverRt = hoverGo.GetComponent<RectTransform>();
            hoverRt.anchorMin = new Vector2(0, 1); hoverRt.anchorMax = new Vector2(1, 1);
            hoverRt.pivot = new Vector2(0.5f, 1f);
            hoverRt.anchoredPosition = Vector2.zero;
            var hoverImg = hoverGo.GetComponent<Image>();
            hoverImg.color = new Color(0, 0, 0, 0.001f);
            hoverImg.raycastTarget = true;

            // 悬停竖线（跟随鼠标 x，初始隐藏）
            var lineGo = new GameObject("HoverLine", typeof(RectTransform), typeof(Image));
            lineGo.transform.SetParent(hoverRt, false);
            lineRt = lineGo.GetComponent<RectTransform>();
            lineRt.anchorMin = new Vector2(0, 1); lineRt.anchorMax = new Vector2(0, 1);
            lineRt.pivot = new Vector2(0.5f, 1f);
            lineRt.anchoredPosition = Vector2.zero;
            lineGo.GetComponent<Image>().color = VanillaChartPalette.HoverLine;
            lineGo.SetActive(false);

            // Tooltip 面板：半透明深底 + 多行富文本（初始隐藏，ContentSizeFitter 自动贴合内容）
            var tipGo = new GameObject("ChartTooltip", typeof(RectTransform), typeof(Image), typeof(ContentSizeFitter));
            tipGo.transform.SetParent(hoverRt, false);
            tipRt = tipGo.GetComponent<RectTransform>();
            tipRt.anchorMin = new Vector2(0, 1); tipRt.anchorMax = new Vector2(0, 1);
            tipRt.pivot = new Vector2(0.5f, 1f);
            tipRt.anchoredPosition = Vector2.zero;
            tipGo.GetComponent<Image>().color = VanillaChartPalette.TooltipBg;
            var tipFit = tipGo.GetComponent<ContentSizeFitter>();
            tipFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            tipFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var tipTextGo = UIHelpers.CreateText("", tipGo.transform, Fs(11f), Color.white, _gameFont, Fs(17f));
            var tipTextRt = tipTextGo.GetComponent<RectTransform>();
            tipTextRt.anchorMin = Vector2.zero; tipTextRt.anchorMax = Vector2.one;
            tipTextRt.offsetMin = new Vector2(8, 5); tipTextRt.offsetMax = new Vector2(-8, -5);
            tipText = tipTextGo.GetComponent<Text>();
            tipText.alignment = TextAnchor.UpperLeft;
            tipText.horizontalOverflow = HorizontalWrapMode.Overflow;
            tipGo.SetActive(false);

            var handler = hoverGo.AddComponent<ChartHoverHandler>();
            // 数据源身份稳定（HistoryService 复用缓冲 + BuildDynamicSeries 系列池）→ 绑定一次即可；
            // 缓冲由调用方传入，避免在此再次 GetRecent 造成别名重填。
            handler.Init(hoverRt, lineRt, tipRt, tipText, snaps != null ? snaps.Count : 0,
                gdp ? (System.Func<int, string>)(idx => BuildGdpTooltip(snaps, _seriesPool, idx))
                    : (System.Func<int, string>)(idx => BuildGiniTooltip(snaps, idx)));
            return handler;
        }

        /// <summary>把悬停层尺寸对齐到当前图表尺寸（图表尺寸随面板宽度变化）。</summary>
        private static void LayoutHoverRig(RectTransform hoverRt, RectTransform lineRt, float chartH, float yAxisW)
        {
            if (hoverRt != null)
            {
                hoverRt.offsetMin = new Vector2(yAxisW, -chartH);
                hoverRt.offsetMax = new Vector2(-ChartUiMargin, 0f);
            }
            if (lineRt != null) lineRt.sizeDelta = new Vector2(2f, chartH);
        }

        // =====================================================================
        // 图例行（常驻，按需增长）
        // =====================================================================

        /// <summary>
        /// 更新图例行：series 为 null 表示本页没有图（全部隐藏）。
        /// 行对象常驻复用，超出本次系列数的行只隐藏不销毁 —— 这正是"刷新不产生 UI 垃圾"的落点。
        /// </summary>
        private void AddLegendRows(List<ChartSeries> series)
        {
            int need = series != null ? series.Count : 0;
            for (int i = 0; i < need; i++) EnsureLegendRow(i);
            for (int i = 0; i < need; i++)
            {
                var s = series[i];
                _legendRows[i].SetActive(true);
                _legendSwatch[i].color = s.Color;
                _legendName[i].text = s.Name;
                float last = float.NaN;
                for (int k = s.Values.Count - 1; k >= 0; k--)
                    if (!float.IsNaN(s.Values[k])) { last = s.Values[k]; break; }
                bool inRank = !float.IsNaN(last);
                _legendValue[i].text = inRank ? UIHelpers.FormatCompact((long)last)
                    : (s.Values.Count > 0 ? UIHelpers.L("chart_dropped") : "");
                _legendValue[i].color = inRank ? VanillaChartPalette.Brighter : UIStyles.TextMuted;
            }
            for (int i = need; i < _legendRows.Count; i++)
                if (_legendRows[i] != null) _legendRows[i].SetActive(false);
            _legendActive = need;
        }

        /// <summary>确保第 i 行图例存在（色条 + 名称 + 数值）。</summary>
        private void EnsureLegendRow(int i)
        {
            while (_legendRows.Count <= i)
            {
                var row = new GameObject("LegendRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                row.transform.SetParent(_content.transform, false);
                var rowLg = row.GetComponent<HorizontalLayoutGroup>();
                rowLg.spacing = 6f;
                rowLg.childAlignment = TextAnchor.MiddleLeft;
                rowLg.childControlWidth = false;
                rowLg.childControlHeight = true;
                rowLg.childForceExpandWidth = false;
                rowLg.childForceExpandHeight = false;
                var rowRt = row.GetComponent<RectTransform>();
                rowRt.sizeDelta = new Vector2(0, 18f);
                var rowEl = row.GetComponent<LayoutElement>();
                rowEl.preferredHeight = 18f;
                rowEl.preferredWidth = 250f;

                // 色条
                var sw = new GameObject("Swatch", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                sw.transform.SetParent(row.transform, false);
                var swImg = sw.GetComponent<Image>();
                swImg.raycastTarget = false;
                sw.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 6f);
                var swEl = sw.GetComponent<LayoutElement>();
                swEl.preferredWidth = 20f;
                swEl.preferredHeight = 6f;

                // 名称（固定宽，超出省略号）
                var nm = UIHelpers.CreateText("", row.transform, Fs(10f), TextColor, _gameFont, Fs(18f));
                var nmT = nm.GetComponent<Text>();
                nmT.alignment = TextAnchor.MiddleLeft;
                nmT.horizontalOverflow = HorizontalWrapMode.Overflow;
                nm.GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 18f);
                var nmEl = nm.AddComponent<LayoutElement>();
                nmEl.preferredWidth = 150f;
                nmEl.preferredHeight = 18f;

                // 最新值 / 已跌出状态
                var vGo = UIHelpers.CreateText("", row.transform, Fs(10f), VanillaChartPalette.Brighter, _gameFont, 18f);
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

                _legendRows.Add(row);
                _legendSwatch.Add(swImg);
                _legendName.Add(nmT);
                _legendValue.Add(vT);
            }
        }

        /// <summary>把图例行插到标题行之后（保持"标题 → 图例 → 正文"的阅读顺序）。</summary>
        private void PlaceLegendRowsAfter(GameObject titleLine)
        {
            if (titleLine == null) return;
            int idx = titleLine.transform.GetSiblingIndex();
            for (int i = 0; i < _legendActive && i < _legendRows.Count; i++)
            {
                var row = _legendRows[i];
                if (row == null || !row.activeSelf) continue;
                row.transform.SetSiblingIndex(idx + 1 + i);
            }
        }

        // =====================================================================
        // Tooltip 文本（静态，无闭包捕获）
        // =====================================================================

        /// <summary>构建 GDP 图悬停 Tooltip 文本：年份 + 全球 + 各在榜王国（富文本色条）。</summary>
        private static string BuildGdpTooltip(List<EconomySnapshot> snaps, List<ChartSeries> seriesList, int idx)
        {
            if (snaps == null || idx < 0 || idx >= snaps.Count) return "";
            var sb = new System.Text.StringBuilder(96);
            sb.Append("<b>").Append(UIHelpers.Lf("chart_year", snaps[idx].GameYear)).Append("</b>\n");
            sb.Append("<color=#FFD800>").Append(UIHelpers.L("chart_global")).Append(" ")
              .Append(UIHelpers.FormatCompact(snaps[idx].GlobalGDP)).Append("</color>");
            if (seriesList != null)
                foreach (var s in seriesList)
                {
                    if (s == null) continue;
                    float v = idx < s.Values.Count ? s.Values[idx] : float.NaN;
                    if (float.IsNaN(v)) continue; // 不在榜不显示
                    sb.Append('\n').Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(s.Color))
                      .Append(">■ ").Append(s.Name).Append(" ").Append(UIHelpers.FormatCompact((long)v)).Append("</color>");
                }
            return sb.ToString();
        }

        /// <summary>构建基尼图悬停 Tooltip 文本：年份 + 基尼 + 周期阶段。</summary>
        private static string BuildGiniTooltip(List<EconomySnapshot> snaps, int idx)
        {
            if (snaps == null || idx < 0 || idx >= snaps.Count) return "";
            var snap = snaps[idx];
            return "<b>" + UIHelpers.Lf("chart_year", snap.GameYear) + "</b>\n" +
                   "<color=#F3961F>" + UIHelpers.L("gini_chart_gini") + " " + snap.GiniCoefficient.ToString("F3") + "</color>\n" +
                   "<color=#4CCFFF>" + UIHelpers.L("gini_chart_phase") + " " + PhaseName((EconomyPhase)snap.Phase) + "</color>";
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
            global.Color = VanillaChartPalette.SeriesGlobal;   // 本体亮金
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
                    series.Color = VanillaChartPalette.SeriesNation(result.Count - 1);   // 本体色板
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
        /// 图表系列：名称 + 颜色 + 与快照等长的取值序列。
        /// </summary>
        private class ChartSeries
        {
            public string Name;
            public Color Color;
            public List<float> Values = new List<float>();
        }
    }
}
