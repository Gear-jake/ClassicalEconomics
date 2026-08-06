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

        private const float PanelWidth = 340f;
        private const float PanelHeight = 470f;

        private static readonly Color Bg           = new Color(0.02f, 0.02f, 0.05f, 0.92f);
        private static readonly Color TextColor    = Color.white;
        private static readonly Color HeaderColor  = new Color(1f, 0.85f, 0.35f);
        private static readonly Color SubColor     = new Color(0.75f, 0.75f, 0.8f);
        private static readonly Color DividerColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

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
                default:                                return UIHelpers.L(e.TypeKey);
            }
        }

        /// <summary>事件类型对应的强调色。</summary>
        private static Color EventColor(string typeKey)
        {
            switch (typeKey)
            {
                case EventStreamService.TypeUnrest:     return new Color(1f, 0.45f, 0.2f);
                case EventStreamService.TypeIncite:     return new Color(0.95f, 0.3f, 0.3f);
                case EventStreamService.TypeSuppress:   return new Color(0.4f, 0.7f, 1f);
                case EventStreamService.TypePlunder:    return new Color(0.95f, 0.8f, 0.3f);
                case EventStreamService.TypeRevolution: return new Color(0.85f, 0.25f, 0.4f);
                case EventStreamService.TypeUprising:   return new Color(0.9f, 0.15f, 0.1f);
                case EventStreamService.TypeBuildInv:     return new Color(0.85f, 0.75f, 0.5f);
                case EventStreamService.TypeCraftArsenal: return new Color(0.9f, 0.9f, 0.4f);
                case EventStreamService.TypeWholesale:    return new Color(1f, 0.7f, 0.3f);
                case EventStreamService.TypeEraGolden:    return new Color(1f, 0.85f, 0.3f);
                case EventStreamService.TypeEraRevival:   return new Color(0.4f, 0.8f, 0.8f);
                case EventStreamService.TypeEraFlourish:  return new Color(0.9f, 0.55f, 0.2f);
                case EventStreamService.TypeCollapse:     return new Color(0.55f, 0.15f, 0.15f);
                case EventStreamService.TypeUnrestPeace:  return new Color(0.5f, 0.9f, 0.6f);
                case EventStreamService.TypeUnrestResolved: return new Color(0.4f, 0.85f, 0.5f);
                case EventStreamService.TypePolicyFail:   return new Color(0.9f, 0.4f, 0.4f);
                case EventStreamService.TypeKingInherit:  return new Color(0.85f, 0.8f, 0.45f);
                default:                                return TextColor;
            }
        }
    }
}
