using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    /// <summary>
    /// 经济事件流悬浮窗（v1.4.0 重制）：单列时间线 + 过滤 chips + 按年折叠。
    /// - 过滤：全部 / 抉择 / 国家·战争 / 经济·民生（family 归类既有事件类型）；
    /// - 折叠：按游戏年分组，默认展开最近 3 年，更早年份按年收起、点击展开（再点收起）；
    /// - 性能（S4-1）：隐藏时零调用；打开时渲染一次；此后仅 每个游戏年（OnYearBoundary，快照尾调用）
    ///   或 用户主动操作（切过滤/折叠）重建——RefreshOverview 不再触发事件窗重建。
    /// </summary>
    public class EventWindow : FloatingWindow
    {
        private static EventWindow _instance;
        private int _renderedVersion = int.MinValue;
        private int _filter;                       // 0=全部 1=抉择 2=国家·战争 3=经济·民生
        private readonly HashSet<int> _expandedYears = new HashSet<int>();

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

        /// <summary>年度边界（快照尾调用）：打开状态下每年重建一次。</summary>
        public void OnYearBoundary()
        {
            if (_visible) { InvalidateContent(); RefreshNow(); }
        }

        /// <summary>重建内容（打开/年度边界/用户操作时调用；版本无变化时跳过）。</summary>
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
            _expandedYears.Clear();
        }

        private static void Rebuild()
        {
            _instance.InvalidateContent();
            _instance.RefreshNow();
        }

        // ===== family 过滤（0=全部 1=抉择 2=国家·战争 3=经济·民生）=====

        private static int FamilyOf(string typeKey)
        {
            switch (typeKey)
            {
                case EventStreamService.TypeDecision:
                    return 1;
                case EventStreamService.TypeUnrest: case EventStreamService.TypeIncite:
                case EventStreamService.TypeSuppress: case EventStreamService.TypePlunder:
                case EventStreamService.TypeRevolution: case EventStreamService.TypeUprising:
                case EventStreamService.TypeUnrestPeace: case EventStreamService.TypeUnrestResolved:
                case EventStreamService.TypePolicyFail: case EventStreamService.TypeKingInherit:
                case EventStreamService.TypeLawReform: case EventStreamService.TypeNationClaim:
                case EventStreamService.TypeNationPolicy: case EventStreamService.TypeNationDiplomacy:
                    return 2;
                default: // 建造/军械/批发/时代/崩溃/政策/灾害/银行/泡沫/救济/庆典/建筑
                    return 3;
            }
        }

        private static readonly string[] FilterKeys = { "events_filter_all", "events_filter_decision", "events_filter_politics", "events_filter_economy" };
        private static readonly Color ChipOn = new Color(0.25f, 0.42f, 0.3f, 0.95f);
        private static readonly Color ChipOff = new Color(0.28f, 0.29f, 0.34f, 0.9f);

        private void BuildList()
        {
            int year = 0;
            try { year = EconomyModMain.GetCurrentGameYear(); } catch (System.Exception) { }
            int total = EventStreamService.TotalCount;

            // 副标题：当前年份 + 事件总数
            AddLine(UIHelpers.Lf("event_subtitle", year, total), SubColor, 12f);

            // 过滤 chips（一行四个，均分）
            var chipRow = new GameObject("FilterChips", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            chipRow.transform.SetParent(_content.transform, false);
            var chipLe = chipRow.AddComponent<LayoutElement>();
            chipLe.preferredHeight = 26f;
            chipLe.flexibleWidth = 1f;
            var hlg = chipRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childForceExpandWidth = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            _lines.Add(chipRow);
            for (int f = 0; f < FilterKeys.Length; f++)
            {
                int filter = f;
                var chip = UIHelpers.CreateButton(UIHelpers.L(FilterKeys[f]), chipRow.transform, -1, 26,
                    _gameFont, _filter == f ? ChipOn : ChipOff, 11f);
                chip.onClick.AddListener(() =>
                {
                    if (_filter == filter) return;
                    _filter = filter;
                    Rebuild();
                });
            }
            AddDivider(DividerColor);

            // 合并双缓冲 → 过滤 → 按年降序
            var all = new List<EventStreamService.EventEntry>(EventStreamService.MajorCount + EventStreamService.Count);
            var major = EventStreamService.GetMajorRecent(EventStreamService.MajorCapacity);
            for (int i = 0; i < major.Count; i++) all.Add(major[i]);
            var minor = EventStreamService.GetMinorRecent(EventStreamService.Capacity);
            for (int i = 0; i < minor.Count; i++) all.Add(minor[i]);
            if (_filter != 0)
            {
                for (int i = all.Count - 1; i >= 0; i--)
                    if (FamilyOf(all[i].TypeKey) != _filter) all.RemoveAt(i);
            }
            if (all.Count == 0)
            {
                AddLine(UIHelpers.L("events_none"), new Color(0.7f, 0.7f, 0.7f), 12f);
                return;
            }
            all.Sort((a, b) => b.GameYear.CompareTo(a.GameYear));

            // 按年折叠：默认展开最近 3 年，更早的收起（点击年份行展开/收起）
            const int DefaultExpandedYears = 3;
            int yearCursor = 0;
            int distinctYears = 0;
            while (yearCursor < all.Count)
            {
                int y = all[yearCursor].GameYear;
                int end = yearCursor;
                while (end < all.Count && all[end].GameYear == y) end++;
                int count = end - yearCursor;
                bool expanded = distinctYears < DefaultExpandedYears || _expandedYears.Contains(y);
                distinctYears++;

                if (!expanded)
                {
                    var fold = UIHelpers.CreateButton(
                        UIHelpers.Lf("events_fold_year", y, count),
                        _content.transform, -1, 24, _gameFont, new Color(0.28f, 0.29f, 0.34f, 0.7f), 11f);
                    fold.onClick.AddListener(() =>
                    {
                        if (!_expandedYears.Add(y)) _expandedYears.Remove(y);
                        Rebuild();
                    });
                    var foldLe = fold.gameObject.AddComponent<LayoutElement>();
                    foldLe.flexibleWidth = 1f;
                    _lines.Add(fold.gameObject);
                    yearCursor = end;
                    continue;
                }

                AddLine(UIHelpers.Lf("events_year_hdr", y), HeaderColor, 12f);
                for (int i = yearCursor; i < end; i++)
                    AddEventRow(all[i]);
                AddDivider(DividerColor);
                yearCursor = end;
            }
        }

        private void AddEventRow(EventStreamService.EventEntry e)
        {
            string desc = string.IsNullOrEmpty(e.Detail)
                ? EventDesc(e)
                : UIHelpers.L(e.Detail); // 抉择事件：Detail=结果键（含选项后果文案）
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
                case EventStreamService.TypeDecision:
                    return UIHelpers.Lf("ev_desc_decision", e.Value);
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
                case EventStreamService.TypeDecision:     return UIStyles.Gold;
                default:                                return TextColor;
            }
        }
    }
}
