using UnityEngine;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    /// <summary>
    /// 经济事件流悬浮窗：独立于经济概览窗口的非模态窗口。
    /// 与富豪榜同款交互——标题栏拖拽、四边/四角缩放、右上角 × 关闭、可滚动内容。
    /// 展示：按类型统计行（形似主页人口/死亡数据行）+ 最近事件时间线。
    /// 由 EconomyUI 的"事件"工具按钮切换显隐，周期刷新时同步刷新。
    /// </summary>
    public class EventWindow : FloatingWindow
    {
        private static EventWindow _instance;

        private const float PanelWidth = UIStyles.ListWidth;
        private const float PanelHeight = UIStyles.ListHeight;

        private static readonly Color Bg           = UIStyles.PanelBg;
        private static readonly Color TextColor    = UIStyles.TextPrimary;
        private static readonly Color HeaderColor  = UIStyles.Gold;
        private static readonly Color SubColor     = UIStyles.TextSecondary;
        private static readonly Color DividerColor = UIStyles.Divider;

        public static EventWindow Instance => _instance;

        // ===== FloatingWindow 配置 =====
        protected override string WindowName => "EconomyEvents";
        protected override float SortingOrder => 10001;
        protected override string TitleKey => "event_title";
        protected override Vector2 AnchorMin => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchorMax => new Vector2(0.5f, 0.5f);
        protected override Vector2 Pivot => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchoredPosition => new Vector2(-80f, 60f);
        protected override Vector2 Size => new Vector2(PanelWidth, PanelHeight);
        protected override Color BgColor => Bg;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<EventWindow>("EconomyEvents");
        }

        /// <summary>重建内容（打开或周期刷新时调用）。</summary>
        public override void RefreshNow()
        {
            ClearContent();
            BuildList();
        }

        private void BuildList()
        {
            int year = 0;
            try { year = EconomyModMain.GetCurrentGameYear(); } catch (System.Exception) { }
            int total = EventStreamService.TotalCount;

            // 副标题：当前年份 + 事件总数
            AddLine(UIHelpers.Lf("event_subtitle", year, total), SubColor, 12f);
            AddDivider(DividerColor);

            if (total == 0)
            {
                AddLine(UIHelpers.L("events_none"), new Color(0.7f, 0.7f, 0.7f), 12f);
                return;
            }

            // 瀑布流：全部事件按时间倒序（最新在上），每条 = 时间 · 地点：事件描述
            AddLine(UIHelpers.L("events_recent"), HeaderColor, 13f);
            var all = EventStreamService.GetRecent(EventStreamService.Capacity);
            for (int i = all.Count - 1; i >= 0; i--)
            {
                var e = all[i];
                string desc = EventDesc(e);
                string kingdomPart = string.IsNullOrEmpty(e.KingdomName) ? "" : " · " + e.KingdomName;
                AddLine(UIHelpers.Lf("events_row", e.GameYear, kingdomPart, desc),
                    EventColor(e.TypeKey), 12f);
            }
        }

        /// <summary>事件描述（含数值，如"饥荒蔓延（34 人饿死）"）。</summary>
        private static string EventDesc(EventStreamService.EventEntry e)
        {
            switch (e.TypeKey)
            {
                case EventStreamService.TypeUnrest:     return UIHelpers.Lf("ev_desc_unrest", e.Value);
                case EventStreamService.TypeIncite:     return UIHelpers.Lf("ev_desc_incite", e.Value);
                case EventStreamService.TypeSuppress:   return UIHelpers.L("ev_desc_suppress");
                case EventStreamService.TypePlunder:    return UIHelpers.Lf("ev_desc_plunder", e.Value);
                case EventStreamService.TypeRevolution: return UIHelpers.Lf("ev_desc_revolution", e.Value);
                case EventStreamService.TypeUprising:   return UIHelpers.Lf("ev_desc_uprising", e.Value);
                case EventStreamService.TypeBuildInv:     return UIHelpers.Lf("ev_desc_build_inv", e.KingdomName);
                case EventStreamService.TypeCraftArsenal: return UIHelpers.Lf("ev_desc_craft_arsenal", e.KingdomName, e.Value);
                case EventStreamService.TypeWholesale:    return UIHelpers.Lf("ev_desc_wholesale", e.KingdomName, e.Value);
                case EventStreamService.TypeEraGolden:    return UIHelpers.Lf("ev_desc_era_golden", e.KingdomName);
                case EventStreamService.TypeEraRevival:   return UIHelpers.Lf("ev_desc_era_revival", e.KingdomName);
                case EventStreamService.TypeEraFlourish:  return UIHelpers.Lf("ev_desc_era_flourish", e.KingdomName);
                case EventStreamService.TypeCollapse:     return UIHelpers.Lf("ev_desc_collapse", e.KingdomName);
                case EventStreamService.TypePolicy:       return UIHelpers.L("ev_desc_policy");
                case EventStreamService.TypeUnrestPeace:  return UIHelpers.L("ev_desc_unrest_peace");
                case EventStreamService.TypeUnrestResolved: return UIHelpers.L("ev_desc_unrest_resolved");
                case EventStreamService.TypePolicyFail:   return e.Value == 3 ? UIHelpers.L("ev_desc_policy_fail_civilwar")
                    : e.Value == 2 ? UIHelpers.L("ev_desc_policy_fail_death")
                    : UIHelpers.L("ev_desc_policy_fail_abdicate");
                case EventStreamService.TypeKingInherit:  return UIHelpers.L("ev_desc_king_inherit");
                case EventStreamService.TypeDisaster:     return UIHelpers.Lf("ev_desc_disaster", e.Value);
                case EventStreamService.TypeBanking:      return UIHelpers.Lf("ev_desc_banking", e.Value);
                case EventStreamService.TypeBubbleBurst:  return UIHelpers.Lf("ev_desc_bubble_burst", e.Value);
                default:                                return UIHelpers.L(e.TypeKey);
            }
        }

        /// <summary>事件类型对应的强调色（设计系统 token）。</summary>
        private static Color EventColor(string typeKey)
        {
            switch (typeKey)
            {
                case EventStreamService.TypeUnrest:     return UIStyles.EvUnrest;
                case EventStreamService.TypeIncite:     return UIStyles.EvIncite;
                case EventStreamService.TypeSuppress:   return UIStyles.EvSuppress;
                case EventStreamService.TypePlunder:    return UIStyles.EvPlunder;
                case EventStreamService.TypeRevolution: return UIStyles.EvRevolution;
                case EventStreamService.TypeUprising:   return UIStyles.EvUprising;
                case EventStreamService.TypeBuildInv:     return UIStyles.EvBuild;
                case EventStreamService.TypeCraftArsenal: return UIStyles.EvCraft;
                case EventStreamService.TypeWholesale:    return UIStyles.EvWholesale;
                case EventStreamService.TypeEraGolden:    return UIStyles.EvGolden;
                case EventStreamService.TypeEraRevival:   return UIStyles.EvRevival;
                case EventStreamService.TypeEraFlourish:  return UIStyles.EvFlourish;
                case EventStreamService.TypeCollapse:     return UIStyles.EvCollapse;
                case EventStreamService.TypeUnrestPeace:  return UIStyles.Positive;
                case EventStreamService.TypeUnrestResolved: return UIStyles.Positive;
                case EventStreamService.TypePolicyFail:   return UIStyles.Danger;
                case EventStreamService.TypeKingInherit:  return UIStyles.GoldDeep;
                case EventStreamService.TypeDisaster:     return UIStyles.EvDisaster;
                case EventStreamService.TypeBanking:      return UIStyles.EvBanking;
                case EventStreamService.TypeBubbleBurst:  return UIStyles.EvBubble;
                default:                                return TextColor;
            }
        }
    }
}
