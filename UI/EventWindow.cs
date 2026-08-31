using System.Collections.Generic;
using UnityEngine;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    /// <summary>
    /// 经济事件流悬浮窗：独立于经济概览窗口的非模态窗口。
    /// 与富豪榜同款交互——标题栏拖拽、四边/四角缩放、右上角 × 关闭、可滚动内容。
    /// 展示（v0.8.3 双区块 + 统计行）：
    ///   1) 关键类型统计行（革命×N·起义×N·泡沫×N…仅发生过才显示，历史累计次数）；
    ///   2) 重大事件区块（史书级，容量 100 防覆盖，倒序最新在上）；
    ///   3) 普通事件区块（最近 60 条，倒序最新在上）。
    /// 由 EconomyUI 的"事件"工具按钮切换显隐，周期刷新时同步刷新。
    /// </summary>
    public class EventWindow : FloatingWindow
    {
        private static EventWindow _instance;
        private int _renderedVersion = int.MinValue;

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
            int version = EventStreamService.Version;
            if (_renderedVersion == version && _lines.Count > 0) return;
            ClearContent();
            BuildList();
            _renderedVersion = version;
        }

        public void InvalidateContent() => _renderedVersion = int.MinValue;

        /// <summary>语言切换：标题走基类；内容因含本地化文案，强制下次重建。</summary>
        public override void RefreshAllTexts()
        {
            base.RefreshAllTexts();
            InvalidateContent();
            if (_visible) RefreshNow();
        }

        public override void OnWorldUnavailable()
        {
            base.OnWorldUnavailable();
            InvalidateContent();
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

            // 关键类型统计行：历史累计次数，仅显示发生过的事件（革命/起义/泡沫/灾害/银行/崩溃/改革失败/王位/掠夺/时代）
            if (AddTypeStats() > 0)
            {
                AddDivider(DividerColor);
            }

            // 重大事件区块（史书级，容量 100 防覆盖，倒序最新在上）
            if (EventStreamService.MajorCount > 0)
            {
                AddLine(UIHelpers.L("events_major"), HeaderColor, 13f);
                var major = EventStreamService.GetMajorRecent(EventStreamService.MajorCapacity);
                for (int i = major.Count - 1; i >= 0; i--)
                {
                    var e = major[i];
                    AddEventRow(e);
                }
                AddDivider(DividerColor);
            }

            // 普通事件区块（最近 60 条，倒序最新在上）
            if (EventStreamService.Count > 0)
            {
                AddLine(UIHelpers.L("events_recent"), HeaderColor, 13f);
                var minor = EventStreamService.GetMinorRecent(EventStreamService.Capacity);
                for (int i = minor.Count - 1; i >= 0; i--)
                {
                    var e = minor[i];
                    AddEventRow(e);
                }
            }
        }

        private void AddEventRow(EventStreamService.EventEntry e)
        {
            string desc = EventDesc(e);
            string kingdomPart = string.IsNullOrEmpty(e.KingdomName) ? "" : " · " + e.KingdomName;
            AddLine(UIHelpers.Lf("events_row", e.GameYear, kingdomPart, desc),
                EventColor(e.TypeKey), 12f);
        }

        /// <summary>渲染关键类型统计行（仅发生过才显示，每行最多 3 项）。返回渲染行数。</summary>
        private int AddTypeStats()
        {
            var parts = _statsPool;
            parts.Clear();
            PushStat(parts, EventStreamService.TypeRevolution);
            PushStat(parts, EventStreamService.TypeUprising);
            PushStat(parts, EventStreamService.TypeBubbleBurst);
            PushStat(parts, EventStreamService.TypeDisaster);
            PushStat(parts, EventStreamService.TypeBanking);
            PushStat(parts, EventStreamService.TypeCollapse);
            PushStat(parts, EventStreamService.TypePolicyFail);
            PushStat(parts, EventStreamService.TypeKingInherit);
            PushStat(parts, EventStreamService.TypePlunder);
            int era = EventStreamService.GetTypeCount(EventStreamService.TypeEraGolden)
                    + EventStreamService.GetTypeCount(EventStreamService.TypeEraRevival)
                    + EventStreamService.GetTypeCount(EventStreamService.TypeEraFlourish);
            if (era > 0) parts.Add(UIHelpers.L("ev_era_combined") + "×" + era);

            if (parts.Count == 0) return 0;
            int lines = 0;
            for (int i = 0; i < parts.Count; i += 3)
            {
                int end = System.Math.Min(i + 3, parts.Count);
                var sb = new System.Text.StringBuilder();
                for (int j = i; j < end; j++)
                {
                    if (j > i) sb.Append("   ");
                    sb.Append(parts[j]);
                }
                AddLine(sb.ToString(), SubColor, 11f);
                lines++;
            }
            return lines;
        }

        private static readonly List<string> _statsPool = new List<string>(12);

        private static void PushStat(List<string> parts, string typeKey)
        {
            int c = EventStreamService.GetTypeCount(typeKey);
            if (c > 0) parts.Add(UIHelpers.L(typeKey) + "×" + c);
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
                    : e.Value == 4 ? UIHelpers.L("ev_desc_policy_fail_fiscal")
                    : e.Value == 5 ? UIHelpers.L("ev_desc_policy_fail_trade")
                    : UIHelpers.L("ev_desc_policy_fail_abdicate");
                case EventStreamService.TypeKingInherit:  return UIHelpers.L("ev_desc_king_inherit");
                case EventStreamService.TypeDisaster:     return UIHelpers.Lf("ev_desc_disaster", e.Value);
                case EventStreamService.TypeBanking:      return UIHelpers.Lf("ev_desc_banking", e.Value);
                case EventStreamService.TypeBubbleBurst:  return UIHelpers.Lf("ev_desc_bubble_burst", e.Value);
                case EventStreamService.TypeNationClaim:    return UIHelpers.Lf("ev_desc_nation_claim", e.KingdomName);
                case EventStreamService.TypeNationPolicy:   return UIHelpers.Lf("ev_desc_nation_policy", e.KingdomName);
                case EventStreamService.TypeNationRelief:   return UIHelpers.Lf("ev_desc_nation_relief", e.KingdomName, e.Value);
                case EventStreamService.TypeNationFestival: return UIHelpers.Lf("ev_desc_nation_festival", e.KingdomName);
                case EventStreamService.TypeNationBuild:    return UIHelpers.Lf("ev_desc_nation_build", e.KingdomName);
                case EventStreamService.TypeNationDiplomacy: return UIHelpers.Lf("ev_desc_nation_diplomacy", e.KingdomName, e.Value);
                case EventStreamService.TypeLawReform:
                    return e.Value >= 2 ? UIHelpers.L("ev_desc_law_reform_major") : UIHelpers.Lf("ev_desc_law_reform", e.KingdomName);
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
