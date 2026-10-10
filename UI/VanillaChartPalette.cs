using UnityEngine;

namespace EconomyMod.UI
{
    /// <summary>
    /// 图表配色层：**全部取自游戏本体**，不自绘、不拍脑袋定色。
    ///
    /// 取色依据（反编译源码，逐条可查）：
    ///   · 语义色常量： base/Toolbox.cs:237-297
    ///       color_positive(#43FF43) / color_positive_light(#95DD5D)
    ///       color_negative(#FB2C21) / color_negative_light(#FF8686)
    ///       color_hex_neutral(#F3961F) / color_hex_brighter(#FFBC66)
    ///       color_text_default(#FF9B1C) / color_text_default_bright(#FFBC66)
    ///       color_hex_blue(#4CCFFF) / color_hex_heal(#23F3FF) / color_hex_purple(#E060CD)
    ///       color_light(#FFD800) / color_grey(#AAAAAA) / color_hex_red(#FF637D)
    ///   · 图表轴标签按取值上色： base/GraphHelpers.cs:54-71 verticalFormat()
    ///       零值 #FFBC66；正值 colorBetween(0 → max, "#FFBC66" → "#F3961F")
    ///   · 折线面积填充是**渐变**： base/HistoryDataAsset.cs:86-96 getChartInnerFillMaterial()
    ///       _ColorFrom.a = 0.4 → _ColorTo.a = 0.1（同色相，上下不同透明度）
    ///   · 折线本体是本体材质 materials/graph/graph_base_line（_Color = 系列色），
    ///     故我们**不复刻材质**，只让颜色与不透明度口径与之一致。
    ///
    /// 安全约定：每一步都 try/catch，取不到就回退到字面量 —— 配色层永远不能让模组崩。
    /// 字面量兜底值与上面的本体常量逐一对齐（同一个十六进制），不引入"第二套配色"。
    /// </summary>
    internal static class VanillaChartPalette
    {
        // ===== 取色工具（失败即回退）=====
        private static Color Hex(string hex, Color fallback)
        {
            try { return Toolbox.makeColor(hex); }
            catch (System.Exception) { return fallback; }
        }

        // ===== 本体语义色 =====
        /// <summary>本体「好」的浅色（color_positive_light #95DD5D）。</summary>
        public static readonly Color Positive      = Hex("#95DD5D", new Color(0.584f, 0.867f, 0.365f));
        /// <summary>本体「好」的强色（color_positive #43FF43）。</summary>
        public static readonly Color PositiveStrong= Hex("#43FF43", new Color(0.263f, 1.000f, 0.263f));
        /// <summary>本体「坏」（color_negative #FB2C21）。</summary>
        public static readonly Color Negative      = Hex("#FB2C21", new Color(0.984f, 0.173f, 0.129f));
        /// <summary>本体「坏」的浅色（color_negative_light #FF8686）—— 日志/提示用。</summary>
        public static readonly Color NegativeLight = Hex("#FF8686", new Color(1.000f, 0.525f, 0.525f));
        /// <summary>本体红（color_hex_red #FF637D）—— 图表负向/危险线。</summary>
        public static readonly Color Red           = Hex("#FF637D", new Color(1.000f, 0.388f, 0.490f));
        /// <summary>本体中性（color_hex_neutral #F3961F）—— 图表 0 值/衰退。</summary>
        public static readonly Color Neutral       = Hex("#F3961F", new Color(0.953f, 0.588f, 0.122f));
        /// <summary>本体亮一级文字（color_text_default_bright #FFBC66）—— 图表刻度文字。</summary>
        public static readonly Color Brighter      = Hex("#FFBC66", new Color(1.000f, 0.737f, 0.400f));
        /// <summary>本体默认文字色（color_text_default #FF9B1C）。</summary>
        public static readonly Color BodyText      = Hex("#FF9B1C", new Color(1.000f, 0.608f, 0.110f));
        /// <summary>本体蓝（color_hex_blue #4CCFFF）。</summary>
        public static readonly Color Blue          = Hex("#4CCFFF", new Color(0.298f, 0.812f, 1.000f));
        /// <summary>本体治疗青（color_hex_heal #23F3FF）。</summary>
        public static readonly Color Cyan          = Hex("#23F3FF", new Color(0.137f, 0.953f, 1.000f));
        /// <summary>本体紫（color_hex_purple #E060CD）。</summary>
        public static readonly Color Purple        = Hex("#E060CD", new Color(0.878f, 0.376f, 0.804f));
        /// <summary>本体黄（color_hex_yellow #FFFF51）。</summary>
        public static readonly Color Yellow        = Hex("#FFFF51", new Color(1.000f, 1.000f, 0.318f));
        /// <summary>本体亮金（color_light #FFD800）—— 主系列（全球 GDP）。</summary>
        public static readonly Color Gold          = Hex("#FFD800", new Color(1.000f, 0.847f, 0.000f));
        /// <summary>本体灰（color_grey #AAAAAA）—— 网格线。</summary>
        public static readonly Color Grey          = Hex("#AAAAAA", new Color(0.667f, 0.667f, 0.667f));
        /// <summary>本体黑（color_hex_black）—— Tooltip 底。</summary>
        public static readonly Color Black         = Hex("#000000", Color.black);

        // ===== 图表专用派生（同源，只调透明度）=====

        /// <summary>网格线：本体灰 + 低透明度（不抢折线）。</summary>
        public static Color GridLine { get { var c = Grey; c.a = 0.28f; return c; } }
        /// <summary>危险参考线（基尼高位）：本体红。</summary>
        public static Color RefHigh { get { var c = Red; c.a = 0.85f; return c; } }
        /// <summary>健康参考线（基尼低位）：本体正向浅色。</summary>
        public static Color RefLow { get { var c = Positive; c.a = 0.85f; return c; } }
        /// <summary>悬停竖线：本体白，半透明。</summary>
        public static Color HoverLine { get { return new Color(1f, 1f, 1f, 0.65f); } }
        /// <summary>Tooltip 底：本体黑 + 0.85 不透明度。</summary>
        public static Color TooltipBg { get { var c = Black; c.a = 0.85f; return c; } }

        /// <summary>折线面积填充上端不透明度（本体 getChartInnerFillMaterial 的 _ColorFrom.a）。</summary>
        public const float AreaAlphaTop = 0.4f;
        /// <summary>折线面积填充下端不透明度（本体 getChartInnerFillMaterial 的 _ColorTo.a）。</summary>
        public const float AreaAlphaBottom = 0.1f;

        /// <summary>
        /// 经济阶段色带：色相取自本体语义色（好=正向浅色、衰退=中性橙、萧条=负向红、复苏=本体蓝），
        /// 透明度为色带专用（低透明度，避免盖住折线）。
        /// </summary>
        public static Color PhaseBand(int phase)
        {
            Color c;
            switch (phase)
            {
                case 0: c = Positive; break;   // Boom     繁荣
                case 1: c = Neutral;  break;   // Recession 衰退
                case 2: c = Negative; break;   // Depression 萧条
                case 3: c = Blue;     break;   // Recovery 复苏
                default: return new Color(0f, 0f, 0f, 0f);
            }
            c.a = phase == 2 ? 0.18f : (phase == 0 ? 0.16f : 0.14f);
            return c;
        }

        /// <summary>阶段徽章/文字用色（不透明版本，与色带同源）。</summary>
        public static Color PhaseSolid(int phase)
        {
            switch (phase)
            {
                case 0: return Positive;
                case 1: return Neutral;
                case 2: return Negative;
                case 3: return Blue;
                default: return Grey;
            }
        }

        /// <summary>
        /// 纵轴刻度文字按取值上色 —— 与本体 GraphHelpers.verticalFormat 同一套口径：
        /// 正值区间 #FFBC66 → #F3961F（越靠上端越"热"）。
        /// 取不到本体实现时回退为两端线性插值（视觉等价）。
        /// </summary>
        public static Color AxisLabel(float value, float max)
        {
            if (max <= 0f) return Brighter;
            try
            {
                return Toolbox.makeColor(Toolbox.colorBetween(value, 0.0, max, "#FFBC66", "#F3961F"));
            }
            catch (System.Exception)
            {
                float t = Mathf.Clamp01(value / max);
                return Color.Lerp(Brighter, Neutral, t);
            }
        }

        /// <summary>主系列（全球 GDP）折线色：本体亮金。</summary>
        public static Color SeriesGlobal { get { return Gold; } }

        /// <summary>基尼折线色：本体中性橙。</summary>
        public static Color SeriesGini { get { return Neutral; } }

        /// <summary>国家折线色板（5 色，全部取自本体常量，色相互相可分）。</summary>
        private static readonly Color[] _dyn =
        {
            Blue,           // #4CCFFF
            PositiveStrong, // #43FF43
            Purple,         // #E060CD
            Red,            // #FF637D
            Cyan            // #23F3FF
        };

        /// <summary>按序号取国家折线色（循环）。</summary>
        public static Color SeriesNation(int index)
        {
            if (index < 0) index = -index;
            return _dyn[index % _dyn.Length];
        }
    }
}
