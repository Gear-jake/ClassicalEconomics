using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>【W2 分层】内阁窗口的"图表与记录"层：本国 GDP 折线图、国家数据判定、财政记录区。</summary>
    public partial class CabinetWindow : FloatingWindow
    {

        /// <summary>本国 GDP 折线图（历史最近 40 期；复用 ChartMeshGraphic 顶点图表，宽度随窗口）。</summary>
        /// <summary>与 HUD 概览共用同一数据源（HistoryService 快照）与 ChartMeshGraphic 渲染组件；
        /// 此处仅抽取本国单系列序列，不重复实现绘图逻辑。</summary>
        private void BuildGdpChart()
        {
            long mine = NationEngine.NationKingdomId;
            if (mine == 0) return;
            var snaps = HistoryService.GetRecent(40);
            if (snaps == null || snaps.Count < 3 || HasNationData(snaps, mine) < 2)
            {
                AddLine(UIHelpers.L("cabinet_gdp_chart_short"), Muted, 11f);
                return;
            }

            AddLine(UIHelpers.L("cabinet_gdp_chart"), UIStyles.Gold, 12f);
            var values = new float[snaps.Count];
            var phases = new int[snaps.Count];
            float vmax = 1f;
            int life = 0;
            for (int i = 0; i < snaps.Count; i++)
            {
                var sn = snaps[i];
                phases[i] = sn.Phase;
                values[i] = float.NaN;
                if (sn.Kingdoms != null)
                {
                    for (int j = 0; j < sn.Kingdoms.Count; j++)
                    {
                        if (sn.Kingdoms[j].KingdomId == mine)
                        {
                            values[i] = sn.Kingdoms[j].GDP;
                            if (values[i] > vmax) vmax = values[i];
                            life++;
                            break;
                        }
                    }
                }
            }

            var chartCard = UIComponents.CreateChartCard(CurPage.transform, 560f, 130f);
            chartCard.gameObject.name = "NationGdpCard";
            CurLines.Add(chartCard.gameObject); // 先注册：任何后续异常都不会残留
            var el = chartCard.GetComponent<LayoutElement>();
            el.preferredWidth = -1f;
            el.flexibleWidth = 1f; // 随窗口宽度自适应

            // 折线网格作为卡的独立子物体（与 HUD 概览同模式）：ChartMeshGraphic 约定 pivot=(0,0)、
            // 原点在左下角；四角拉伸随卡自适应，禁止与卡 Image 同 GameObject 共存（双 Graphic 互扰 + 尺寸为 0）
            var meshGo = new GameObject("GdpChartMesh", typeof(RectTransform), typeof(ChartMeshGraphic));
            meshGo.transform.SetParent(chartCard, false);
            var meshRt = meshGo.GetComponent<RectTransform>();
            meshRt.anchorMin = Vector2.zero;
            meshRt.anchorMax = Vector2.one;
            meshRt.pivot = Vector2.zero;
            meshRt.offsetMin = new Vector2(10f, 8f);
            meshRt.offsetMax = new Vector2(-10f, -8f);
            var chart = meshGo.GetComponent<ChartMeshGraphic>();
            chart.raycastTarget = false;
            chart.yAxisWidth = 44f;
            chart.margin = 4f;
            try
            {
                // 自适应刻度：以实际最小/最大值为界（避免 0 起使波动脉成直线）
                float vmin = float.MaxValue;
                float vmax2 = float.MinValue;
                for (int i = 0; i < values.Length; i++)
                {
                    if (float.IsNaN(values[i])) continue;
                    if (values[i] < vmin) vmin = values[i];
                    if (values[i] > vmax2) vmax2 = values[i];
                }
                if (vmax2 <= vmin) { vmax2 = vmin + 1f; }
                float pad = (vmax2 - vmin) * 0.15f;
                chart.SetChartData(new float[][] { values }, new[] { UIStyles.Gold }, null,
                    phases, 1, vmin - pad, vmax2 + pad, true, false, 0f, 0f);
                AddLine(UIHelpers.Lf("cabinet_gdp_chart_now",
                    values[values.Length - 1].ToString("N0"),
                    vmin.ToString("N0"), vmax2.ToString("N0")), Muted, 11f);
            }
            catch (System.Exception) { }
        }

        /// <summary>快照中本国拥有 GDP 数据的期数（该国有数据 ≥2 期才画曲线）。</summary>
        private static int HasNationData(System.Collections.Generic.List<EconomyMod.Models.EconomySnapshot> snaps, long kingdomId)
        {
            int life = 0;
            for (int i = 0; i < snaps.Count; i++)
            {
                var sn = snaps[i];
                if (sn.Kingdoms == null) continue;
                for (int j = 0; j < sn.Kingdoms.Count; j++)
                {
                    if (sn.Kingdoms[j].KingdomId == kingdomId) { life++; break; }
                }
            }
            return life;
        }

        private void BuildRecordSection()
        {
            AddLine(UIHelpers.L("cabinet_records"), UIStyles.Gold, 13f);
            var records = NationEngine.GetRecentRecords(12);
            if (records.Count == 0)
            {
                AddLine(UIHelpers.L("cabinet_no_records"), Muted, 11f);
                return;
            }
            foreach (var r in records)
            {
                string text = r.Closed
                    ? UIHelpers.Lf("cabinet_record_row", r.Year, UIHelpers.L(r.Key),
                        NationEngine.FormatGold(r.Amount),
                        r.GiniBefore.ToString("F2"), r.GiniAfter.ToString("F2"),
                        r.AvgBefore.ToString("F0"), r.AvgAfter.ToString("F0"))
                    : UIHelpers.Lf("cabinet_record_open", r.Year, UIHelpers.L(r.Key),
                        NationEngine.FormatGold(r.Amount),
                        r.GiniBefore.ToString("F2"), r.AvgBefore.ToString("F0"));
                AddLine(text, r.Amount >= 0 ? UIStyles.TextSecondary : UIStyles.Positive, 11f);
            }
        }
    }
}