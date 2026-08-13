using System.Collections.Generic;
using EconomyMod.Core;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 组件库：区块标题 / 指标卡 / 阶段徽章 / 状态徽章 / 王国行 / 图表容器。
    /// 全部组件挂在 VerticalLayoutGroup 的 content 下，用 LayoutElement 固定首选尺寸。
    /// 返回的 GameObject 由调用方加入 _lines（FloatingWindow.ClearContent 统一销毁）。
    /// </summary>
    internal static class UIComponents
    {
        // ===== 区块标题（金色粗体 + 底部 2px 金色线，容器化防泄漏）=====

        /// <summary>
        /// 创建区块标题：返回容器 GameObject（内含标题文本 + 分隔线），
        /// 调用方将容器加入 _lines，销毁时标题与分隔线一并销毁（修复分隔线泄漏）。
        /// </summary>
        public static GameObject CreateSectionHeader(Transform parent, string text, Font font, float width)
        {
            var container = new GameObject("SectionHeader", typeof(RectTransform), typeof(VerticalLayoutGroup));
            container.transform.SetParent(parent, false);
            var vlg = container.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 2f;
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false; vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperLeft;
            var crt = container.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(width, 0);
            float totalH = UIStyles.BodyLineHeight + 2f + 2f;
            container.AddComponent<LayoutElement>().preferredHeight = totalH;

            // 标题文本
            var go = UIHelpers.CreateText(text, container.transform, UIStyles.SectionHeaderSize, UIStyles.Gold,
                font, UIStyles.BodyLineHeight, "Title");
            var t = go.GetComponent<Text>();
            t.fontStyle = FontStyle.Bold;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, UIStyles.BodyLineHeight);
            var el = go.AddComponent<LayoutElement>();
            el.preferredHeight = UIStyles.BodyLineHeight;
            el.preferredWidth = width;

            // 分隔线（作为容器子物体，随容器销毁）
            var line = UIHelpers.CreateDivider(container.transform, UIStyles.GoldDeep);
            line.name = "SectionLine";
            var lrt = line.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(width, 2f);
            var lel = line.AddComponent<LayoutElement>();
            lel.preferredHeight = 2f;
            lel.preferredWidth = width;
            return container;
        }

        // ===== 指标卡片（标签 + 数值，圆角背景）=====

        /// <summary>创建单个指标卡（label 上 / value 下），固定宽高。</summary>
        public static GameObject CreateStatCard(Transform parent, string label, string value,
            Color valueColor, Font font, float w, float h)
        {
            var card = new GameObject("StatCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            var img = card.GetComponent<Image>();
            img.sprite = UIHelpers.RoundedSprite();
            img.type = Image.Type.Sliced;
            img.color = UIStyles.CardBg;
            img.raycastTarget = false;
            var rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(w, h);
            var el = card.AddComponent<LayoutElement>();
            el.preferredWidth = w;
            el.preferredHeight = h;

            // 标签（顶部，弱色）
            var lbl = UIHelpers.CreateText(label, card.transform, UIStyles.StatLabelSize,
                UIStyles.TextMuted, font, 16f, "Label");
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 1); lrt.anchorMax = new Vector2(1, 1);
            lrt.pivot = new Vector2(0.5f, 1);
            lrt.anchoredPosition = new Vector2(0, -4);
            lrt.sizeDelta = new Vector2(-8, 16f);
            lbl.GetComponent<Text>().alignment = TextAnchor.UpperCenter;

            // 数值（底部，强调色，粗体）
            var val = UIHelpers.CreateText(value, card.transform, UIStyles.StatValueSize,
                valueColor, font, 22f, "Value");
            var vrt = val.GetComponent<RectTransform>();
            vrt.anchorMin = new Vector2(0, 0); vrt.anchorMax = new Vector2(1, 0);
            vrt.pivot = new Vector2(0.5f, 0);
            vrt.anchoredPosition = new Vector2(0, 3);
            vrt.sizeDelta = new Vector2(-8, 22f);
            var vt = val.GetComponent<Text>();
            vt.alignment = TextAnchor.LowerCenter;
            vt.fontStyle = FontStyle.Bold;
            vt.horizontalOverflow = HorizontalWrapMode.Overflow;
            return card;
        }

        /// <summary>创建指标卡网格（每行 cols 张卡的多行网格，卡片按 stats 顺序逐行填充）。</summary>
        public static GameObject CreateStatGrid(Transform parent,
            (string label, string value, Color color)[] stats, Font font, float width, int cols = 3)
        {
            var grid = new GameObject("StatGrid", typeof(RectTransform), typeof(VerticalLayoutGroup));
            grid.transform.SetParent(parent, false);
            var vlg = grid.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = UIStyles.RowGap;
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false; vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperLeft;
            var gridRt = grid.GetComponent<RectTransform>();
            gridRt.sizeDelta = new Vector2(width, 0);
            float cardH = 52f;
            float gap = UIStyles.RowGap;
            int n = stats.Length;
            int rows = (n + cols - 1) / cols;
            float cardW = (width - gap * (cols - 1)) / cols;

            // 每行一个 HorizontalLayoutGroup，卡片按 cols 分组（修复：原单行布局在卡片数>cols 时横向溢出）
            for (int r = 0; r < rows; r++)
            {
                var rowGo = new GameObject("StatRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                rowGo.transform.SetParent(grid.transform, false);
                var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = gap;
                hlg.childControlWidth = true; hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
                hlg.childAlignment = TextAnchor.UpperLeft;
                rowGo.GetComponent<RectTransform>().sizeDelta = new Vector2(width, cardH);
                rowGo.AddComponent<LayoutElement>().preferredHeight = cardH;
                for (int c = 0; c < cols; c++)
                {
                    int i = r * cols + c;
                    if (i >= n) break;
                    var s = stats[i];
                    CreateStatCard(rowGo.transform, s.label, s.value, s.color, font, cardW, cardH);
                }
            }
            grid.AddComponent<LayoutElement>().preferredHeight = rows * cardH + Mathf.Max(0, rows - 1) * gap;
            return grid;
        }

        // ===== 徽章（胶囊背景 + 文本）=====

        /// <summary>创建胶囊徽章：深色底 + 彩色描边 + 彩色文本。</summary>
        public static GameObject CreateBadge(Transform parent, string text, Color accent,
            Font font, float w, float h)
        {
            var badge = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badge.transform.SetParent(parent, false);
            var img = badge.GetComponent<Image>();
            img.sprite = UIHelpers.RoundedSprite();
            img.type = Image.Type.Sliced;
            img.color = new Color(accent.r, accent.g, accent.b, 0.16f);
            img.raycastTarget = false;
            var rt = badge.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(w, h);
            var el = badge.AddComponent<LayoutElement>();
            el.preferredWidth = w;
            el.preferredHeight = h;

            var lbl = UIHelpers.CreateText(text, badge.transform, UIStyles.BadgeSize,
                accent, font, h, "Label");
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            lbl.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            lbl.GetComponent<Text>().fontStyle = FontStyle.Bold;
            return badge;
        }

        /// <summary>创建经济阶段徽章（色彩编码）。</summary>
        public static GameObject CreatePhaseBadge(Transform parent, EconomyPhase phase,
            string text, Font font, float w)
        {
            return CreateBadge(parent, text, UIStyles.PhaseColor(phase), font, w, 22f);
        }

        // ===== 数据行（带彩色前缀标记）=====

        /// <summary>创建带彩色圆点前缀的数据行（用于动荡/危机状态列表）。</summary>
        public static GameObject CreateStatusRow(Transform parent, string text, Color accent,
            Font font, float width)
        {
            var row = new GameObject("StatusRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            row.AddComponent<LayoutElement>().preferredHeight = UIStyles.BodyLineHeight;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, UIStyles.BodyLineHeight);

            // 圆点
            var dot = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(row.transform, false);
            var dotImg = dot.GetComponent<Image>();
            dotImg.sprite = UIHelpers.RoundedSprite();
            dotImg.type = Image.Type.Sliced;
            dotImg.color = accent;
            dotImg.raycastTarget = false;
            dot.GetComponent<RectTransform>().sizeDelta = new Vector2(8f, 8f);
            var dotEl = dot.AddComponent<LayoutElement>();
            dotEl.preferredWidth = 8f; dotEl.preferredHeight = 8f;

            // 文本
            var lbl = UIHelpers.CreateText(text, row.transform, UIStyles.BodySize,
                UIStyles.TextPrimary, font, UIStyles.BodyLineHeight, "Text");
            lbl.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(width - 18f, UIStyles.BodyLineHeight);
            var lel = lbl.AddComponent<LayoutElement>();
            lel.preferredWidth = width - 18f;
            lel.preferredHeight = UIStyles.BodyLineHeight;
            return row;
        }

        // ===== 王国排行行（排名 + 名称 + 关键指标）=====

        /// <summary>创建王国排行表头（列标题行：排名/王国/GDP/人均/基尼），弱色显示。</summary>
        public static GameObject CreateKingdomHeader(Transform parent, Font font, float width)
        {
            var row = new GameObject("KingdomHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            float h = 18f;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, h);
            row.AddComponent<LayoutElement>().preferredHeight = h;

            // 排名列
            var rankGo = UIHelpers.CreateText("", row.transform, 10f, UIStyles.TextMuted, font, h, "Rank");
            var rrt = rankGo.GetComponent<RectTransform>();
            rrt.sizeDelta = new Vector2(22f, h);
            var rel = rankGo.AddComponent<LayoutElement>();
            rel.preferredWidth = 22f; rel.preferredHeight = h;

            // 王国列
            var nameGo = UIHelpers.CreateText(UIHelpers.L("col_kingdom"), row.transform, 10f,
                UIStyles.TextMuted, font, h, "Name");
            var nrt = nameGo.GetComponent<RectTransform>();
            float nameW = width * 0.27f;
            nrt.sizeDelta = new Vector2(nameW, h);
            var nel = nameGo.AddComponent<LayoutElement>();
            nel.preferredWidth = nameW; nel.preferredHeight = h;

            // GDP列
            var gdpGo = UIHelpers.CreateText(UIHelpers.L("col_gdp"), row.transform, 10f,
                UIStyles.TextMuted, font, h, "Gdp");
            gdpGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var grt = gdpGo.GetComponent<RectTransform>();
            float gdpW = width * 0.24f;
            grt.sizeDelta = new Vector2(gdpW, h);
            var gel = gdpGo.AddComponent<LayoutElement>();
            gel.preferredWidth = gdpW; gel.preferredHeight = h;

            // 人均列
            var avgGo = UIHelpers.CreateText(UIHelpers.L("col_avg"), row.transform, 10f,
                UIStyles.TextMuted, font, h, "Avg");
            avgGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var art = avgGo.GetComponent<RectTransform>();
            float avgW = width * 0.18f;
            art.sizeDelta = new Vector2(avgW, h);
            var ael = avgGo.AddComponent<LayoutElement>();
            ael.preferredWidth = avgW; ael.preferredHeight = h;

            // 基尼列
            var giniGo = UIHelpers.CreateText(UIHelpers.L("col_gini"), row.transform, 10f,
                UIStyles.TextMuted, font, h, "Gini");
            giniGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var girt = giniGo.GetComponent<RectTransform>();
            float giniW = width * 0.15f;
            girt.sizeDelta = new Vector2(giniW, h);
            var giel = giniGo.AddComponent<LayoutElement>();
            giel.preferredWidth = giniW; giel.preferredHeight = h;

            // 本地价格列（v0.9：区域价格指数，1.0=基准 CPI）
            var priceGo = UIHelpers.CreateText(UIHelpers.L("col_price"), row.transform, 10f,
                UIStyles.TextMuted, font, h, "Price");
            priceGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var prt = priceGo.GetComponent<RectTransform>();
            float priceW = width * 0.16f;
            prt.sizeDelta = new Vector2(priceW, h);
            var pel = priceGo.AddComponent<LayoutElement>();
            pel.preferredWidth = priceW; pel.preferredHeight = h;
            return row;
        }

        /// <summary>创建王国排行行：排名徽章 + 名称 + GDP/人均/基尼/本地价格，返回 GameObject。</summary>
        public static GameObject CreateKingdomRow(Transform parent, int rank, string name,
            string gdp, string avg, string gini, string price, Font font, float width, bool highlight = false)
        {
            Color rankColor = rank == 1 ? UIStyles.Gold : rank == 2 ? UIStyles.Silver
                : rank == 3 ? UIStyles.Bronze : UIStyles.TextMuted;

            var row = new GameObject("KingdomRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            float h = 24f;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, h);
            row.AddComponent<LayoutElement>().preferredHeight = h;

            // 排名徽章（圆形数字）
            var rankGo = UIHelpers.CreateText(rank.ToString(), row.transform, 11f, rankColor,
                font, 20f, "Rank");
            rankGo.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            rankGo.GetComponent<Text>().fontStyle = FontStyle.Bold;
            var rrt = rankGo.GetComponent<RectTransform>();
            rrt.sizeDelta = new Vector2(22f, 20f);
            var rel = rankGo.AddComponent<LayoutElement>();
            rel.preferredWidth = 22f; rel.preferredHeight = 20f;

            // 名称（左对齐，可截断）
            var nameGo = UIHelpers.CreateText(name, row.transform, UIStyles.BodySize,
                highlight ? UIStyles.Gold : UIStyles.TextPrimary, font, h, "Name");
            nameGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            nameGo.GetComponent<Text>().fontStyle = highlight ? FontStyle.Bold : FontStyle.Normal;
            var nrt = nameGo.GetComponent<RectTransform>();
            float nameW = width * 0.27f;
            nrt.sizeDelta = new Vector2(nameW, h);
            var nel = nameGo.AddComponent<LayoutElement>();
            nel.preferredWidth = nameW; nel.preferredHeight = h;

            // GDP（右对齐）
            var gdpGo = UIHelpers.CreateText(gdp, row.transform, UIStyles.BodySize,
                UIStyles.TextSecondary, font, h, "Gdp");
            gdpGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var grt = gdpGo.GetComponent<RectTransform>();
            float gdpW = width * 0.24f;
            grt.sizeDelta = new Vector2(gdpW, h);
            var gel = gdpGo.AddComponent<LayoutElement>();
            gel.preferredWidth = gdpW; gel.preferredHeight = h;

            // 人均（右对齐）
            var avgGo = UIHelpers.CreateText(avg, row.transform, UIStyles.BodySize,
                UIStyles.TextSecondary, font, h, "Avg");
            avgGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var art = avgGo.GetComponent<RectTransform>();
            float avgW = width * 0.18f;
            art.sizeDelta = new Vector2(avgW, h);
            var ael = avgGo.AddComponent<LayoutElement>();
            ael.preferredWidth = avgW; ael.preferredHeight = h;

            // 基尼（右对齐，语义色）
            float giniVal = 0f;
            float.TryParse(gini, out giniVal);
            Color giniColor = giniVal >= 0.7f ? UIStyles.Danger
                : giniVal >= 0.55f ? UIStyles.Warning : UIStyles.TextSecondary;
            var giniGo = UIHelpers.CreateText(gini, row.transform, UIStyles.BodySize,
                giniColor, font, h, "Gini");
            giniGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            giniGo.GetComponent<Text>().fontStyle = FontStyle.Bold;
            var girt = giniGo.GetComponent<RectTransform>();
            float giniW = width * 0.15f;
            girt.sizeDelta = new Vector2(giniW, h);
            var giel = giniGo.AddComponent<LayoutElement>();
            giel.preferredWidth = giniW; giel.preferredHeight = h;

            // 本地价格（右对齐，语义色：高 1.3× 基准 → 通胀区；低 0.8× → 廉价区）
            float priceVal = 0f;
            float.TryParse(price, out priceVal);
            Color priceColor = priceVal >= 1.3f ? UIStyles.Warning
                : priceVal <= 0.8f ? UIStyles.Info : UIStyles.TextSecondary;
            var priceGo = UIHelpers.CreateText(price, row.transform, UIStyles.BodySize,
                priceColor, font, h, "Price");
            priceGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var prt = priceGo.GetComponent<RectTransform>();
            float priceW = width * 0.16f;
            prt.sizeDelta = new Vector2(priceW, h);
            var pel = priceGo.AddComponent<LayoutElement>();
            pel.preferredWidth = priceW; pel.preferredHeight = h;

            return row;
        }

        // ===== 图表容器卡片 =====

        /// <summary>创建图表卡片容器（圆角背景 + LayoutElement），返回容器 RectTransform。</summary>
        public static RectTransform CreateChartCard(Transform parent, float width, float height)
        {
            var card = new GameObject("ChartCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            var img = card.GetComponent<Image>();
            img.sprite = UIHelpers.RoundedSprite();
            img.type = Image.Type.Sliced;
            img.color = UIStyles.CardBg;
            img.raycastTarget = false;
            var rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            var el = card.AddComponent<LayoutElement>();
            el.preferredWidth = width;
            el.preferredHeight = height;
            return rt;
        }
    }
}
