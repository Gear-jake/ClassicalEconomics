using EconomyMod.Core;
using UnityEngine;

namespace EconomyMod.UI
{
    /// <summary>
    /// 设计 Token（深色金融主题）：色彩 / 字号 / 间距 / 圆角 集中管理。
    /// 全部面板共用，便于整体换肤与一致性维护。
    /// </summary>
    internal static class UIStyles
    {
        // ===== 中性色 =====
        public static readonly Color PanelBg       = new Color(0.03f, 0.04f, 0.07f, 0.96f);
        public static readonly Color CardBg        = new Color(0.08f, 0.09f, 0.14f, 0.88f);
        public static readonly Color CardBgAlt     = new Color(0.11f, 0.12f, 0.18f, 0.92f);
        public static readonly Color TextPrimary   = new Color(0.95f, 0.95f, 0.96f);
        public static readonly Color TextSecondary = new Color(0.70f, 0.72f, 0.78f);
        public static readonly Color TextMuted     = new Color(0.49f, 0.51f, 0.58f);
        public static readonly Color Divider       = new Color(0.23f, 0.25f, 0.31f, 0.65f);

        // ===== 品牌/强调（金色系）=====
        public static readonly Color Gold        = new Color(1f, 0.85f, 0.40f);
        public static readonly Color GoldDeep    = new Color(0.79f, 0.63f, 0.29f);
        public static readonly Color Bronze      = new Color(0.69f, 0.55f, 0.34f);
        public static readonly Color Silver      = new Color(0.75f, 0.78f, 0.83f);

        // ===== 经济阶段（语义色）=====
        public static readonly Color PhaseBoom       = new Color(0.29f, 0.82f, 0.50f);
        public static readonly Color PhaseRecession  = new Color(0.98f, 0.75f, 0.14f);
        public static readonly Color PhaseDepression = new Color(0.97f, 0.44f, 0.44f);
        public static readonly Color PhaseRecovery   = new Color(0.38f, 0.65f, 0.98f);

        // ===== 语义色 =====
        public static readonly Color Positive = new Color(0.20f, 0.83f, 0.60f);
        public static readonly Color Negative = new Color(0.97f, 0.44f, 0.44f);
        public static readonly Color Warning  = new Color(0.98f, 0.75f, 0.14f);
        public static readonly Color Danger   = new Color(0.94f, 0.27f, 0.27f);
        public static readonly Color Info     = new Color(0.38f, 0.65f, 0.98f);

        // ===== 事件类型色 =====
        public static readonly Color EvUnrest    = new Color(0.97f, 0.44f, 0.44f); // 动荡
        public static readonly Color EvIncite    = new Color(0.95f, 0.32f, 0.36f); // 煽动
        public static readonly Color EvSuppress  = new Color(0.38f, 0.65f, 0.98f); // 镇压
        public static readonly Color EvPlunder   = new Color(0.98f, 0.75f, 0.14f); // 掠夺
        public static readonly Color EvRevolution= new Color(0.90f, 0.32f, 0.52f); // 革命
        public static readonly Color EvUprising  = new Color(0.93f, 0.16f, 0.12f); // 起义
        public static readonly Color EvBuild     = new Color(0.85f, 0.75f, 0.50f); // 建造
        public static readonly Color EvCraft     = new Color(0.90f, 0.90f, 0.40f); // 军械
        public static readonly Color EvWholesale = new Color(1f, 0.70f, 0.30f);   // 批发
        public static readonly Color EvGolden    = new Color(1f, 0.85f, 0.40f);   // 盛世
        public static readonly Color EvRevival   = new Color(0.40f, 0.91f, 0.91f); // 复兴
        public static readonly Color EvFlourish  = new Color(0.98f, 0.57f, 0.24f); // 强盛
        public static readonly Color EvCollapse  = new Color(0.72f, 0.11f, 0.11f); // 崩溃
        public static readonly Color EvDisaster  = new Color(0.76f, 0.25f, 0.05f); // 灾害
        public static readonly Color EvBanking   = new Color(0.93f, 0.29f, 0.60f); // 银行危机
        public static readonly Color EvBubble    = new Color(0.96f, 0.62f, 0.04f); // 泡沫破裂

        // ===== 字号 =====
        public const float TitleSize        = 15f;
        public const float SectionHeaderSize = 13f;
        public const float StatLabelSize    = 10f;
        public const float StatValueSize    = 16f;
        public const float BodySize         = 12f;
        public const float CaptionSize      = 10f;
        public const float BadgeSize        = 10f;

        // ===== 行高 =====
        public const float BodyLineHeight    = 22f;
        public const float CardLineHeight    = 26f;
        public const float TitleLineHeight   = 28f;

        // ===== 间距（4px 基准）=====
        public const float PanelPadding   = 14f;
        public const float CardGap        = 8f;
        public const float RowGap         = 4f;
        public const float SectionGap     = 10f;

        // ===== 圆角 =====
        public const int PanelRadius  = 10;
        public const int CardRadius   = 8;
        public const int BadgeRadius  = 12; // 胶囊

        // ===== 面板尺寸 =====
        public const float HudWidth   = 380f;
        public const float HudHeight  = 560f;
        public const float ListWidth  = 360f;
        public const float ListHeight = 500f;

        /// <summary>经济阶段 → 语义色。</summary>
        public static Color PhaseColor(EconomyPhase phase)
        {
            switch (phase)
            {
                case EconomyPhase.Boom:       return PhaseBoom;
                case EconomyPhase.Recession:  return PhaseRecession;
                case EconomyPhase.Depression: return PhaseDepression;
                default:                      return PhaseRecovery;
            }
        }
    }
}
