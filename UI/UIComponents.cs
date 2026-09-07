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
        public static GameObject CreateSectionHeader(Transform parent, string text, Font font, float width, float fontScale = 1f)
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
            float titleH = UIStyles.BodyLineHeight * fontScale;
            const float dividerH = 2f;
            // 容器高度 = 标题行 + 行间 2px + 金线 2px（与实际子项排布一致，防溢出重叠下一行）
            container.AddComponent<LayoutElement>().preferredHeight = titleH + 2f + dividerH;

            // 内层横排：左侧金色短竖条 + 标题文本（金条与标题同行，而非另一行）
            var row = new GameObject("TitleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(container.transform, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            var rowRt = row.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(width, titleH);
            var rowLe = row.AddComponent<LayoutElement>();
            rowLe.preferredWidth = width; rowLe.preferredHeight = titleH;

            var accentBar = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
            accentBar.transform.SetParent(row.transform, false);
            var abRt = accentBar.GetComponent<RectTransform>();
            abRt.sizeDelta = new Vector2(3, titleH);
            accentBar.GetComponent<Image>().color = UIStyles.SectionBar;
            accentBar.GetComponent<Image>().raycastTarget = false;
            var abLe = accentBar.AddComponent<LayoutElement>();
            abLe.preferredWidth = 3; abLe.preferredHeight = titleH;

            // 标题文本
            var go = UIHelpers.CreateText(text, row.transform, UIStyles.SectionHeaderSize * fontScale, UIStyles.Gold,
                font, titleH, "Title");
            var t = go.GetComponent<Text>();
            t.fontStyle = FontStyle.Bold;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width - 9f, titleH);
            var el = go.AddComponent<LayoutElement>();
            el.preferredHeight = titleH;
            el.preferredWidth = width - 9f;
            el.flexibleWidth = 1;

            // 分隔线（作为容器子物体，随容器销毁）
            var line = UIHelpers.CreateDivider(container.transform, UIStyles.GoldDeep);
            line.name = "SectionLine";
            var lrt = line.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(width, dividerH);
            var lel = line.AddComponent<LayoutElement>();
            lel.preferredHeight = dividerH;
            lel.preferredWidth = width;
            return container;
        }

        // ===== 指标卡片（标签 + 数值，圆角背景）=====

        /// <summary>创建单个指标卡（label 上 / value 下），固定宽高。</summary>
        public static GameObject CreateStatCard(Transform parent, string label, string value,
            Color valueColor, Font font, float w, float h, float fontScale = 1f)
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
            // 左侧色条（价值色 = valueColor，视觉锚定；不参与布局）
            var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(card.transform, false);
            strip.transform.SetAsFirstSibling();
            var srt = strip.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(0, 1);
            srt.pivot = new Vector2(0, 0.5f);
            srt.anchoredPosition = Vector2.zero;
            srt.sizeDelta = new Vector2(3, 0);
            strip.GetComponent<Image>().color = valueColor;
            strip.GetComponent<Image>().raycastTarget = false;
            var sIgnore = strip.AddComponent<LayoutElement>();
            sIgnore.ignoreLayout = true;

            // 卡片内部用 VerticalLayoutGroup 管理文本布局（锚点+BestFit 在缩放/换行时
            // 会互相挤压导致文字错位叠出卡片——布局组接管后由 LayoutElement 明确占位）
            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.padding = new RectOffset(6, 6, 4, 4);
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            // 标签（顶部，弱色；BestFit 自动缩小字号，窄卡不溢出）
            var lbl = UIHelpers.CreateText(label, card.transform, UIStyles.StatLabelSize * fontScale,
                UIStyles.TextMuted, font, 16f * fontScale, "Label");
            var lLe = lbl.AddComponent<LayoutElement>();
            lLe.preferredHeight = 16f * fontScale;
            lLe.flexibleHeight = 0;
            lLe.flexibleWidth = 1;
            var lText = lbl.GetComponent<Text>();
            lText.alignment = TextAnchor.UpperCenter;
            lText.resizeTextForBestFit = true;
            lText.resizeTextMinSize = 7;
            lText.resizeTextMaxSize = Mathf.RoundToInt(UIStyles.StatLabelSize * fontScale);

            // 数值（底部，强调色，粗体；BestFit 自动缩小字号）
            var val = UIHelpers.CreateText(value, card.transform, UIStyles.StatValueSize * fontScale,
                valueColor, font, 22f * fontScale, "Value");
            var vLe = val.AddComponent<LayoutElement>();
            vLe.preferredHeight = 22f * fontScale;
            vLe.flexibleHeight = 1;
            vLe.flexibleWidth = 1;
            vLe.minHeight = 18f * fontScale;
            var vt = val.GetComponent<Text>();
            vt.alignment = TextAnchor.LowerCenter;
            vt.fontStyle = FontStyle.Bold;
            vt.horizontalOverflow = HorizontalWrapMode.Overflow;
            vt.resizeTextForBestFit = true;
            vt.resizeTextMinSize = 9;
            vt.resizeTextMaxSize = Mathf.RoundToInt(UIStyles.StatValueSize * fontScale);
            return card;
        }

        /// <summary>
        /// 创建指标卡单行（所有卡片横向均分填满 width，不换行）。
        /// 卡片数较多时单卡变窄，由窗口缩放（拖拽边角拉宽）保证内容不溢出；
        /// 不再采用 v0.9.0 的多行网格，恢复 v0.8.x 单行观感。
        /// </summary>
        public static GameObject CreateStatGrid(Transform parent,
            (string label, string value, Color color)[] stats, Font font, float width, float fontScale = 1f)
        {
            var rowGo = new GameObject("StatRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowGo.transform.SetParent(parent, false);
            var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = UIStyles.CardGap;
            hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.UpperLeft;
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(width, 0);
            float cardH = 52f * fontScale;
            int n = stats.Length;
            float gap = UIStyles.CardGap;
            float cardW = n > 1 ? (width - gap * (n - 1)) / n : width;
            foreach (var s in stats)
                CreateStatCard(rowGo.transform, s.label, s.value, s.color, font, cardW, cardH, fontScale);
            rowGo.AddComponent<LayoutElement>().preferredHeight = cardH;
            return rowGo;
        }

        // ===== 徽章（胶囊背景 + 文本）=====

        /// <summary>创建胶囊徽章：深色底 + 彩色描边 + 彩色文本。</summary>
        public static GameObject CreateBadge(Transform parent, string text, Color accent,
            Font font, float w, float h, float fontScale = 1f)
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

            var lbl = UIHelpers.CreateText(text, badge.transform, UIStyles.BadgeSize * fontScale,
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
            string text, Font font, float w, float fontScale = 1f)
        {
            return CreateBadge(parent, text, UIStyles.PhaseColor(phase), font, w, 22f, fontScale);
        }

        // ===== 数据行（带彩色前缀标记）=====

        /// <summary>创建带彩色圆点前缀的数据行（用于动荡/危机状态列表）。</summary>
        public static GameObject CreateStatusRow(Transform parent, string text, Color accent,
            Font font, float width, float fontScale = 1f)
        {
            var row = new GameObject("StatusRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            row.AddComponent<LayoutElement>().preferredHeight = UIStyles.BodyLineHeight * fontScale;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, UIStyles.BodyLineHeight * fontScale);

            // 圆点
            var dot = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(row.transform, false);
            var dotImg = dot.GetComponent<Image>();
            dotImg.sprite = UIHelpers.RoundedSprite();
            dotImg.type = Image.Type.Sliced;
            dotImg.color = accent;
            dotImg.raycastTarget = false;
            dot.GetComponent<RectTransform>().sizeDelta = new Vector2(8f * fontScale, 8f * fontScale);
            var dotEl = dot.AddComponent<LayoutElement>();
            dotEl.preferredWidth = 8f * fontScale; dotEl.preferredHeight = 8f * fontScale;

            // 文本
            var lbl = UIHelpers.CreateText(text, row.transform, UIStyles.BodySize * fontScale,
                UIStyles.TextPrimary, font, UIStyles.BodyLineHeight * fontScale, "Text");
            lbl.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.sizeDelta = new Vector2(width - 18f, UIStyles.BodyLineHeight * fontScale);
            var lel = lbl.AddComponent<LayoutElement>();
            lel.preferredWidth = width - 18f;
            lel.preferredHeight = UIStyles.BodyLineHeight * fontScale;
            return row;
        }

        // ===== 王国排行行（排名 + 名称 + 关键指标）=====

        /// <summary>
        /// 王国排行列宽：总宽扣除排名列与 5 个列间距后按比例分配，
        /// 防止原 width×比例（合计 100%）与排名列/间距叠加后超出容器被 ScrollRect 截断。
        /// </summary>
        private static float ColumnWidth(float width, float fontScale, float frac)
        {
            return (width - 22f * fontScale - 6f * 5f) * frac;
        }

        /// <summary>创建王国排行表头（列标题行：排名/王国/GDP/人均/基尼），弱色显示。</summary>
        public static GameObject CreateKingdomHeader(Transform parent, Font font, float width, float fontScale = 1f)
        {
            var row = new GameObject("KingdomHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            float h = 20f * fontScale;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, h);
            row.AddComponent<LayoutElement>().preferredHeight = h;

            // 排名列
            var rankGo = UIHelpers.CreateText("", row.transform, 10f * fontScale, UIStyles.TextMuted, font, h, "Rank");
            var rrt = rankGo.GetComponent<RectTransform>();
            rrt.sizeDelta = new Vector2(22f * fontScale, h);
            var rel = rankGo.AddComponent<LayoutElement>();
            rel.preferredWidth = 22f * fontScale; rel.preferredHeight = h;

            // 王国列
            var nameGo = UIHelpers.CreateText(UIHelpers.L("col_kingdom"), row.transform, 10f * fontScale,
                UIStyles.TextMuted, font, h, "Name");
            var nrt = nameGo.GetComponent<RectTransform>();
            float nameW = ColumnWidth(width, fontScale, 0.27f);
            nrt.sizeDelta = new Vector2(nameW, h);
            var nel = nameGo.AddComponent<LayoutElement>();
            nel.preferredWidth = nameW; nel.preferredHeight = h;

            // GDP列
            var gdpGo = UIHelpers.CreateText(UIHelpers.L("col_gdp"), row.transform, 10f * fontScale,
                UIStyles.TextMuted, font, h, "Gdp");
            gdpGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var grt = gdpGo.GetComponent<RectTransform>();
            float gdpW = ColumnWidth(width, fontScale, 0.24f);
            grt.sizeDelta = new Vector2(gdpW, h);
            var gel = gdpGo.AddComponent<LayoutElement>();
            gel.preferredWidth = gdpW; gel.preferredHeight = h;

            // 人均列
            var avgGo = UIHelpers.CreateText(UIHelpers.L("col_avg"), row.transform, 10f * fontScale,
                UIStyles.TextMuted, font, h, "Avg");
            avgGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var art = avgGo.GetComponent<RectTransform>();
            float avgW = ColumnWidth(width, fontScale, 0.18f);
            art.sizeDelta = new Vector2(avgW, h);
            var ael = avgGo.AddComponent<LayoutElement>();
            ael.preferredWidth = avgW; ael.preferredHeight = h;

            // 基尼列
            var giniGo = UIHelpers.CreateText(UIHelpers.L("col_gini"), row.transform, 10f * fontScale,
                UIStyles.TextMuted, font, h, "Gini");
            giniGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var girt = giniGo.GetComponent<RectTransform>();
            float giniW = ColumnWidth(width, fontScale, 0.15f);
            girt.sizeDelta = new Vector2(giniW, h);
            var giel = giniGo.AddComponent<LayoutElement>();
            giel.preferredWidth = giniW; giel.preferredHeight = h;

            // 本地价格列（v0.9：区域价格指数，1.0=基准 CPI）
            var priceGo = UIHelpers.CreateText(UIHelpers.L("col_price"), row.transform, 10f * fontScale,
                UIStyles.TextMuted, font, h, "Price");
            priceGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var prt = priceGo.GetComponent<RectTransform>();
            float priceW = ColumnWidth(width, fontScale, 0.16f);
            prt.sizeDelta = new Vector2(priceW, h);
            var pel = priceGo.AddComponent<LayoutElement>();
            pel.preferredWidth = priceW; pel.preferredHeight = h;
            return row;
        }

        /// <summary>创建王国排行行：排名徽章 + 名称 + GDP/人均/基尼/本地价格，返回 GameObject。</summary>
        public static GameObject CreateKingdomRow(Transform parent, int rank, string name,
            string gdp, string avg, string gini, string price, Font font, float width, bool highlight = false, float fontScale = 1f)
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
            float h = 24f * fontScale;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, h);
            row.AddComponent<LayoutElement>().preferredHeight = h;
            // 前三名底色（金/银/铜淡染）
            if (rank == 1) { var bg = row.AddComponent<Image>(); bg.color = UIStyles.RowRank1; bg.raycastTarget = false; }
            else if (rank == 2) { var bg = row.AddComponent<Image>(); bg.color = UIStyles.RowRank2; bg.raycastTarget = false; }
            else if (rank == 3) { var bg = row.AddComponent<Image>(); bg.color = UIStyles.RowRank3; bg.raycastTarget = false; }

            // 排名徽章（圆形底 + 数字）
            var rankGo = UIHelpers.CreateText(rank.ToString(), row.transform, 11f * fontScale, rankColor,
                font, 20f * fontScale, "Rank");
            rankGo.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            rankGo.GetComponent<Text>().fontStyle = FontStyle.Bold;
            var rrt = rankGo.GetComponent<RectTransform>();
            rrt.sizeDelta = new Vector2(22f * fontScale, 20f * fontScale);
            // 圆形底（前三名有，其余透明）
            var rankBg = new GameObject("RankBg", typeof(RectTransform), typeof(Image));
            rankBg.transform.SetParent(rankGo.transform, false);
            var rbRt = rankBg.GetComponent<RectTransform>();
            rbRt.anchorMin = Vector2.zero; rbRt.anchorMax = Vector2.one;
            rbRt.offsetMin = Vector2.zero; rbRt.offsetMax = Vector2.zero;
            var rbImg = rankBg.GetComponent<Image>();
            rbImg.sprite = UIHelpers.RoundedSprite();
            rbImg.type = Image.Type.Sliced;
            rbImg.raycastTarget = false;
            rbImg.color = rank <= 3
                ? new Color(rankColor.r, rankColor.g, rankColor.b, 0.18f)
                : new Color(0, 0, 0, 0);
            rankBg.transform.SetAsFirstSibling();
            var rel = rankGo.AddComponent<LayoutElement>();
            rel.preferredWidth = 22f * fontScale; rel.preferredHeight = 20f * fontScale;

            // 名称（左对齐，可截断）
            var nameGo = UIHelpers.CreateText(name, row.transform, UIStyles.BodySize * fontScale,
                highlight ? UIStyles.Gold : UIStyles.TextPrimary, font, h, "Name");
            nameGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            nameGo.GetComponent<Text>().fontStyle = highlight ? FontStyle.Bold : FontStyle.Normal;
            var nrt = nameGo.GetComponent<RectTransform>();
            float nameW = ColumnWidth(width, fontScale, 0.27f);
            nrt.sizeDelta = new Vector2(nameW, h);
            var nel = nameGo.AddComponent<LayoutElement>();
            nel.preferredWidth = nameW; nel.preferredHeight = h;

            // GDP（右对齐）
            var gdpGo = UIHelpers.CreateText(gdp, row.transform, UIStyles.BodySize * fontScale,
                UIStyles.TextSecondary, font, h, "Gdp");
            gdpGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var grt = gdpGo.GetComponent<RectTransform>();
            float gdpW = ColumnWidth(width, fontScale, 0.24f);
            grt.sizeDelta = new Vector2(gdpW, h);
            var gel = gdpGo.AddComponent<LayoutElement>();
            gel.preferredWidth = gdpW; gel.preferredHeight = h;

            // 人均（右对齐）
            var avgGo = UIHelpers.CreateText(avg, row.transform, UIStyles.BodySize * fontScale,
                UIStyles.TextSecondary, font, h, "Avg");
            avgGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var art = avgGo.GetComponent<RectTransform>();
            float avgW = ColumnWidth(width, fontScale, 0.18f);
            art.sizeDelta = new Vector2(avgW, h);
            var ael = avgGo.AddComponent<LayoutElement>();
            ael.preferredWidth = avgW; ael.preferredHeight = h;

            // 基尼（右对齐，语义色）
            float giniVal = 0f;
            float.TryParse(gini, out giniVal);
            Color giniColor = giniVal >= 0.7f ? UIStyles.Danger
                : giniVal >= 0.55f ? UIStyles.Warning : UIStyles.TextSecondary;
            var giniGo = UIHelpers.CreateText(gini, row.transform, UIStyles.BodySize * fontScale,
                giniColor, font, h, "Gini");
            giniGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            giniGo.GetComponent<Text>().fontStyle = FontStyle.Bold;
            var girt = giniGo.GetComponent<RectTransform>();
            float giniW = ColumnWidth(width, fontScale, 0.15f);
            girt.sizeDelta = new Vector2(giniW, h);
            var giel = giniGo.AddComponent<LayoutElement>();
            giel.preferredWidth = giniW; giel.preferredHeight = h;

            // 本地价格（右对齐，语义色：高 1.3× 基准 → 通胀区；低 0.8× → 廉价区）
            float priceVal = 0f;
            float.TryParse(price, out priceVal);
            Color priceColor = priceVal >= 1.3f ? UIStyles.Warning
                : priceVal <= 0.8f ? UIStyles.Info : UIStyles.TextSecondary;
            var priceGo = UIHelpers.CreateText(price, row.transform, UIStyles.BodySize * fontScale,
                priceColor, font, h, "Price");
            priceGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            var prt = priceGo.GetComponent<RectTransform>();
            float priceW = ColumnWidth(width, fontScale, 0.16f);
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
