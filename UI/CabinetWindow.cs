using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 内阁面板（中央银行家 v0.95）：玩家认领国家的治理台。
    /// 四区：国库（余额/收支/认领切换）、政策（6 个持续政策 × 三档、槽位上限）、
    /// 法令（紧急救济/庆典/建筑）、记录（政绩环形列表，含执行前后关键指标对比）。
    /// 与其他悬浮窗同构（FloatingWindow 骨架 + 深色金融主题）。
    /// </summary>
    public class CabinetWindow : FloatingWindow
    {
        private static CabinetWindow _instance;
        public static CabinetWindow Instance => _instance;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<CabinetWindow>("EconomyCabinet");
        }

        protected override string WindowName => "EconomyCabinet";
        protected override float SortingOrder => 10003f;
        protected override string TitleKey => "cabinet_title";
        protected override Vector2 AnchorMin => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchorMax => new Vector2(0.5f, 0.5f);
        protected override Vector2 Pivot => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchoredPosition => new Vector2(620f, 0f);
        protected override Vector2 Size => new Vector2(400f, 640f);
        protected override Color BgColor => new Color(0.12f, 0.13f, 0.16f, 0.97f);

        private static readonly Color Muted = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color DividerColor = new Color(0.35f, 0.35f, 0.4f, 0.6f);
        private static readonly Color BtnColor = new Color(0.35f, 0.35f, 0.4f, 0.85f);
        private static readonly Color BtnGood = new Color(0.25f, 0.42f, 0.3f, 0.9f);
        private static readonly Color BtnBad = new Color(0.45f, 0.28f, 0.28f, 0.9f);

        public override void RefreshNow()
        {
            ClearContent();
            var cfg = UnrestConfig.Instance;
            if (cfg == null) return;

            if (!cfg.NationPlayEnabled)
            {
                AddLine(UIHelpers.L("cabinet_disabled"), Muted, 12f);
                return;
            }
            if (AnnualPipeline.IsSettling)
            {
                AddLine(UIHelpers.L("settling_marker"), UIStyles.Warning, 12f);
            }

            if (NationEngine.NationKingdomId == 0) BuildClaimSection();
            else BuildNationSections();
        }

        // ===== 未认领：国家列表 =====

        private void BuildClaimSection()
        {
            AddLine(UIHelpers.L("cabinet_no_nation"), UIStyles.Gold, 13f);
            AddLine(UIHelpers.L("cabinet_claim_hint"), Muted, 12f);
            AddDivider(DividerColor);

            if (World.world == null || World.world.kingdoms == null)
            {
                AddLine(UIHelpers.L("picker_empty"), Muted, 12f);
                return;
            }
            var kingdomList = new List<Kingdom>(World.world.kingdoms);
            kingdomList.Sort((a, b) =>
            {
                long ga = 0, gb = 0;
                if (EconomyEngine.KingdomStats.TryGetValue(a.data.id, out var ka)) ga = (long)ka.GDP;
                if (EconomyEngine.KingdomStats.TryGetValue(b.data.id, out var kb)) gb = (long)kb.GDP;
                return gb.CompareTo(ga);
            });

            int shown = 0;
            foreach (var kingdom in kingdomList)
            {
                if (kingdom == null || kingdom.data == null) continue;
                if (shown >= 10) break;
                string name = GameHelpers.SafeKingdomName(kingdom);
                long gdp = 0;
                if (EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out var ks)) gdp = (long)ks.GDP;
                var btn = UIHelpers.CreateButton(
                    UIHelpers.Lf("cabinet_claim_row", name, gdp),
                    _content.transform, -1, 30, _gameFont, BtnGood);
                long kid = kingdom.data.id;
                btn.onClick.AddListener(() =>
                {
                    int year = SafeYear();
                    if (NationEngine.Claim(GameHelpers.FindKingdom(kid), year, out _)) RefreshNow();
                });
                _lines.Add(btn.gameObject);
                shown++;
            }
            if (shown == 0) AddLine(UIHelpers.L("picker_empty"), Muted, 12f);
        }

        // ===== 已认领：国库/政策/法令/记录 =====

        private void BuildNationSections()
        {
            int year = SafeYear();
            var cfg = UnrestConfig.Instance;

            // --- 国库 ---
            AddLine(UIHelpers.Lf("cabinet_nation", NationEngine.NationName), UIStyles.Gold, 13f);
            AddLine(UIHelpers.Lf("cabinet_treasury", NationEngine.FormatGold(NationEngine.Treasury)),
                UIStyles.Gold, 13f);
            AddLine(UIHelpers.Lf("cabinet_flow", NationEngine.FormatGold(NationEngine.LastIncome),
                NationEngine.FormatGold(NationEngine.LastExpense)), Muted, 12f);
            int cooldown = NationEngine.LastSwitchYear + 10 - year;
            if (cooldown > 0)
                AddLine(UIHelpers.Lf("cabinet_switch_cooldown", cooldown), Muted, 12f);

            // --- 持续政策 ---
            AddLine(UIHelpers.Lf("cabinet_policies", NationEngine.SlotCount, cfg.PolicySlots), UIStyles.Gold, 13f);
            for (int kind = 0; kind < NationEngine.PolicyKindCount; kind++)
            {
                BuildPolicyRow((NationEngine.PolicyKind)kind, year);
            }
            AddLine("", Muted, 8f);

            // --- 法令 ---
            AddLine(UIHelpers.L("cabinet_decrees"), UIStyles.Gold, 13f);
            BuildDecreeRow("nation_decree_relief", UIHelpers.Lf("cabinet_decree_cost_relief"), year < NationEngine.ReliefReadyYear,
                () => { if (NationEngine.TryEmergencyRelief(SafeYear())) RefreshNow(); });
            BuildDecreeRow("nation_decree_festival", UIHelpers.Lf("cabinet_decree_cost_festival"), year < NationEngine.FestivalReadyYear,
                () => { if (NationEngine.TryFestival(SafeYear())) RefreshNow(); });
            BuildBuildingRows();
            AddLine("", Muted, 8f);

            // --- 外交（实时生效：宣战/求和/结盟/协定/赠礼） ---
            BuildDiplomacySection();
            AddLine("", Muted, 8f);

            // --- 记录 ---
            BuildRecordSection();
        }

        /// <summary>外交区块：列出他国（按 GDP 前 8），每国一行：好感 + 5 个动作按钮。</summary>
        private void BuildDiplomacySection()
        {
            AddLine(UIHelpers.L("cabinet_diplomacy"), UIStyles.Gold, 13f);
            var mineId = NationEngine.NationKingdomId;
            if (World.world == null || World.world.kingdoms == null)
            {
                AddLine(UIHelpers.L("picker_empty"), Muted, 11f);
                return;
            }
            var kingdoms = new List<Kingdom>(World.world.kingdoms);
            kingdoms.Sort((a, b) =>
            {
                long ga = 0, gb = 0;
                if (EconomyEngine.KingdomStats.TryGetValue(a.data.id, out var ka)) ga = (long)ka.GDP;
                if (EconomyEngine.KingdomStats.TryGetValue(b.data.id, out var kb)) gb = (long)kb.GDP;
                return gb.CompareTo(ga);
            });

            int shown = 0;
            foreach (var k in kingdoms)
            {
                if (k == null || k.data == null) continue;
                if (k.data.id == mineId) continue;
                if (shown >= 8) break;

                long kid = k.data.id;
                string name = GameHelpers.SafeKingdomName(k);
                int score = NationDiplomacy.GetRelationScore(k);
                int goodwill = NationDiplomacy.GetGoodwill(kid);
                int pactTier = NationDiplomacy.PactTier(kid);
                string status = UIHelpers.Lf("cabinet_dip_row", name, score, goodwill,
                    pactTier >= 0 ? UIHelpers.Lf("cabinet_dip_pact_tier", pactTier + 1) : "");
                AddLine(status, UIStyles.TextPrimary, 11f);

                var row = NewRow(5);
                AddRowButton(row, UIHelpers.L("cabinet_dip_war"), BtnBad, () =>
                {
                    string msg; bool ok = NationDiplomacy.DeclareWar(k, out msg);
                    GameHelpers.NotifyLocalized(msg, name);
                    if (ok) RefreshNow();
                });
                AddRowButton(row, UIHelpers.L("cabinet_dip_peace"), BtnColor, () =>
                {
                    string msg; bool ok = NationDiplomacy.SueForPeace(k, out msg);
                    GameHelpers.NotifyLocalized(msg, name);
                    if (ok) RefreshNow();
                });
                AddRowButton(row, UIHelpers.L("cabinet_dip_alliance"), BtnGood, () =>
                {
                    string msg; bool ok = NationDiplomacy.FormAlliance(k, out msg);
                    GameHelpers.NotifyLocalized(msg, name);
                    if (ok) RefreshNow();
                });
                AddRowButton(row, pactTier >= 0
                    ? UIHelpers.Lf("cabinet_dip_pact_on", pactTier + 1)
                    : UIHelpers.L("cabinet_dip_pact"), BtnGood, () =>
                {
                    string msg; bool ok;
                    if (pactTier >= 0)
                        ok = NationDiplomacy.SignPact(k, pactTier + 1, out msg); // 升档
                    else
                        ok = NationDiplomacy.SignPact(k, 0, out msg); // 新签（少）
                    GameHelpers.NotifyLocalized(msg, name);
                    if (ok) RefreshNow();
                });
                AddRowButton(row, UIHelpers.L("cabinet_dip_gift"), BtnGood, () =>
                {
                    string msg; bool ok = NationDiplomacy.GiveGift(k, out msg);
                    GameHelpers.NotifyLocalized(msg, name);
                    if (ok) RefreshNow();
                });
                shown++;
            }
            if (shown == 0) AddLine(UIHelpers.L("cabinet_no_others"), Muted, 11f);
        }

        private void BuildPolicyRow(NationEngine.PolicyKind kind, int year)
        {
            int tier = NationEngine.GetPolicyTier(kind);
            string name = NationEngine.PolicyName(kind);
            string costText = UIHelpers.Lf("cabinet_policy_cost",
                NationEngine.FormatGold((long)NationEngine.PolicyAnnualCost(kind, 0)),
                NationEngine.FormatGold((long)NationEngine.PolicyAnnualCost(kind, 1)),
                NationEngine.FormatGold((long)NationEngine.PolicyAnnualCost(kind, 2)));

            if (tier >= 0)
            {
                AddLine(UIHelpers.Lf("cabinet_policy_active", name, tier + 1), UIStyles.Positive, 12f);
            }
            else
            {
                AddLine(name, UIStyles.TextPrimary, 12f);
            }
            AddLine(costText, Muted, 11f);

            // 按钮行：启用/切档（循环 少→中→大）+ 取消
            var row = NewRow(2);
            if (tier < 0)
            {
                AddRowButton(row, UIHelpers.L("cabinet_enable"), BtnGood, () =>
                {
                    if (NationEngine.EnablePolicy(kind, 0, SafeYear())) RefreshNow();
                });
                AddRowSpacer(row);
            }
            else if (tier < NationEngine.TierCount - 1)
            {
                AddRowButton(row, UIHelpers.L("cabinet_upgrade"), BtnGood, () =>
                {
                    if (NationEngine.EnablePolicy(kind, tier + 1, SafeYear())) RefreshNow();
                });
                AddRowSpacer(row);
            }
            AddRowButton(row, UIHelpers.L("cabinet_disable"), BtnBad,
                () => { if (NationEngine.DisablePolicy(kind)) RefreshNow(); });
            if (row.transform.childCount == 1) AddRowSpacer(row); // 单按钮行也拉满
        }

        private void BuildDecreeRow(string nameKey, string costText, bool cooling, System.Action action)
        {
            string name = UIHelpers.L(nameKey);
            AddLine(cooling ? UIHelpers.Lf("cabinet_decree_cooling", name) : name,
                cooling ? Muted : UIStyles.TextPrimary, 12f);
            AddLine(costText, Muted, 11f);
            var row = NewRow(1);
            AddRowButton(row, UIHelpers.L("cabinet_execute"), cooling ? BtnColor : BtnGood, () => action());
        }

        private void BuildBuildingRows()
        {
            AddLine(UIHelpers.L("cabinet_build_title"), UIStyles.TextPrimary, 12f);
            var kingdom = GameHelpers.FindKingdom(NationEngine.NationKingdomId);
            if (kingdom == null) return;
            var cities = new List<City>();
            try
            {
                var cs = kingdom.getCities();
                if (cs != null) foreach (City c in cs) if (c != null) cities.Add(c);
            }
            catch (System.Exception) { }
            if (cities.Count == 0)
            {
                AddLine(UIHelpers.L("cabinet_no_cities"), Muted, 11f);
                return;
            }
            int shown = 0;
            foreach (var city in cities)
            {
                if (shown >= 6) break;
                long cityId;
                try { cityId = city.id; } catch (System.Exception) { continue; }
                string cname = GameHelpers.SafeCityName(city);
                bool has = NationEngine.IsMarketCity(cityId) || NationEngine.IsGranaryCity(cityId);
                string status = NationEngine.IsMarketCity(cityId) ? UIHelpers.L("nation_build_market")
                    : NationEngine.IsGranaryCity(cityId) ? UIHelpers.L("nation_build_granary") : null;
                AddLine(status != null ? UIHelpers.Lf("cabinet_city_built", cname, status) : cname,
                    status != null ? UIStyles.Positive : UIStyles.TextPrimary, 11f);
                if (!has)
                {
                    var row = NewRow(2);
                    AddRowButton(row, UIHelpers.L("nation_build_market"), BtnGood, () =>
                    {
                        if (NationEngine.TryBuild(GameHelpers.FindKingdom(NationEngine.NationKingdomId), city, NationEngine.BuildingKind.Market, SafeYear())) RefreshNow();
                    });
                    AddRowButton(row, UIHelpers.L("nation_build_granary"), BtnGood, () =>
                    {
                        if (NationEngine.TryBuild(GameHelpers.FindKingdom(NationEngine.NationKingdomId), city, NationEngine.BuildingKind.Granary, SafeYear())) RefreshNow();
                    });
                }
                shown++;
            }
            if (cities.Count > shown) AddLine(UIHelpers.L("cabinet_more_cities"), Muted, 11f);
        }

        private void BuildRecordSection()
        {
            AddLine(UIHelpers.L("cabinet_records"), UIStyles.Gold, 13f);
            var records = NationEngine.GetRecentRecords(10);
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

        // ===== 行/按钮小构件 =====

        private GameObject NewRow(int expectButtons)
        {
            var row = new GameObject("CabinetRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(_content.transform, false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 28f;
            le.flexibleWidth = 1f;
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            _lines.Add(row);
            return row;
        }

        private void AddRowButton(GameObject row, string label, Color bg, System.Action onClick)
        {
            var btn = UIHelpers.CreateButton(label, row.transform, -1, 26, _gameFont, bg, 11f);
            btn.onClick.AddListener(() => onClick());
        }

        private void AddRowSpacer(GameObject row)
        {
            var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(row.transform, false);
            go.GetComponent<LayoutElement>().flexibleWidth = 1f;
        }

        private static int SafeYear()
        {
            try { return EconomyModMain.GetCurrentGameYear(); }
            catch (System.Exception) { return 0; }
        }

        public override void RefreshAllTexts()
        {
            base.RefreshAllTexts();
            if (_visible && _panelRoot != null) RefreshNow();
        }
    }
}
