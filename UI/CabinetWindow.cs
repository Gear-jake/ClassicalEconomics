using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 内阁面板（中央银行家）：玩家认领国家的治理台。
    /// RulerBox 式枢纽-页面布局：顶部 Tab 切换 4 个页面——财税 / 政策 / 法令·建设 / 外交，
    /// 每个页面获得整幅内容宽度（不再挤在一个小页里）；窗口 600×760。
    /// 未认领时显示国家列表（认领页）。
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
        protected override Vector2 AnchoredPosition => new Vector2(560f, 0f);
        protected override Vector2 Size => new Vector2(600f, 760f);
        protected override Color BgColor => new Color(0.12f, 0.13f, 0.16f, 0.97f);

        private enum CabinetPage { Finance = 0, Policy, Decree, Diplomacy, Codex }
        private const int PageCount = 5;
        private static readonly string[] PageKeys = { "cabinet_tab_finance", "cabinet_tab_policy", "cabinet_tab_decree", "cabinet_tab_diplomacy", "cabinet_tab_codex" };

        private static readonly Color Muted = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color DividerColor = new Color(0.35f, 0.35f, 0.4f, 0.6f);
        private static readonly Color BtnColor = new Color(0.35f, 0.35f, 0.4f, 0.85f);
        private static readonly Color BtnGood = new Color(0.25f, 0.42f, 0.3f, 0.9f);
        private static readonly Color BtnBad = new Color(0.45f, 0.28f, 0.28f, 0.9f);
        private static readonly Color TabOn = new Color(0.25f, 0.42f, 0.3f, 0.95f);
        private static readonly Color TabOff = new Color(0.28f, 0.29f, 0.34f, 0.9f);

        private RectTransform _contentRect;
        private Button[] _tabButtons = new Button[PageCount];
        private readonly GameObject[] _pages = new GameObject[PageCount];
        private readonly List<GameObject>[] _pageLines = new List<GameObject>[PageCount];
        private CabinetPage _page = CabinetPage.Finance;

        private List<GameObject> CurLines => _pageLines[(int)_page];
        private GameObject CurPage => _pages[(int)_page];

        protected override void BuildPanel()
        {
            var canvas = GetComponent<Canvas>();
            UIHelpers.SetupCanvas(canvas, SortingOrder);

            _panelRect = UIHelpers.CreatePanelRoot(transform, WindowName + "Panel",
                AnchorMin, AnchorMax, Pivot, AnchoredPosition, Size, BgColor);
            _panelRoot = _panelRect.gameObject;

            UIHelpers.CreateDragArea(_panelRect, _panelRect, Padding + 36);
            _titleText = UIHelpers.CreateWindowTitle(_panelRect, UIHelpers.L(TitleKey), _gameFont,
                UIStyles.Gold, TitleFontSize, Padding, TitleLineHeight);
            UIHelpers.CreateResizeHandles(_panelRect, OnPanelResized);
            UIHelpers.CreateCloseButton(_panelRect, _gameFont, Hide);

            // Tab 栏（RulerBox 式页面切换）
            var tabBar = new GameObject("CabinetTabBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabBar.transform.SetParent(_panelRoot.transform, false);
            var tabRt = tabBar.GetComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0, 1); tabRt.anchorMax = new Vector2(1, 1);
            tabRt.pivot = new Vector2(0.5f, 1f);
            tabRt.anchoredPosition = new Vector2(0, -(Padding + TitleLineHeight));
            tabRt.sizeDelta = new Vector2(-Padding * 2, 30f);
            var hlg = tabBar.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true; hlg.childControlHeight = true;

            for (int i = 0; i < PageCount; i++)
            {
                int page = i;
                var btn = UIHelpers.CreateButton(UIHelpers.L(PageKeys[i]), tabBar.transform, -1, 28, _gameFont, TabOff, 12f);
                btn.onClick.AddListener(() => SwitchPage((CabinetPage)page));
                _tabButtons[i] = btn;
            }

            // 滚动内容区（页面容器挂这里）
            var scrollGo = UIHelpers.CreateScrollContent(_panelRect, Padding, Padding + 30f + 34f);
            _content = scrollGo.gameObject;
            _contentRect = scrollGo;

            // 4 个页面容器（SetActive 切换）
            for (int i = 0; i < PageCount; i++)
            {
                var go = new GameObject("CabinetPage" + i, typeof(RectTransform), typeof(VerticalLayoutGroup));
                go.transform.SetParent(_content.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = Vector2.zero;
                var vhl = go.GetComponent<VerticalLayoutGroup>();
                vhl.spacing = 4;
                vhl.padding = new RectOffset(2, 2, 2, 2);
                vhl.childControlWidth = true; vhl.childControlHeight = false;
                vhl.childForceExpandWidth = true; vhl.childForceExpandHeight = false;
                _pages[i] = go;
                _pageLines[i] = new List<GameObject>();
            }

            UpdateTabHighlights();
            SwitchPage(CabinetPage.Finance);
        }

        private void SwitchPage(CabinetPage page)
        {
            _page = page;
            for (int i = 0; i < PageCount; i++)
                _pages[i].SetActive(i == (int)page);
            RefreshNow();
        }

        private void UpdateTabHighlights()
        {
            for (int i = 0; i < PageCount; i++)
            {
                if (_tabButtons[i] == null) continue;
                var img = _tabButtons[i].GetComponent<Image>();
                if (img != null) img.color = i == (int)_page ? TabOn : TabOff;
            }
        }

        public override void RefreshNow()
        {
            // 只重建当前页；其余页保持隐藏
            foreach (var go in CurLines) Destroy(go);
            CurLines.Clear();

            var cfg = UnrestConfig.Instance;
            if (cfg == null) return;

            if (AnnualPipeline.IsSettling)
            {
                AddLine(UIHelpers.L("settling_marker"), UIStyles.Warning, 12f);
            }

            if (!cfg.NationPlayEnabled)
            {
                AddLine(UIHelpers.L("cabinet_disabled"), Muted, 12f);
                return;
            }

            if (NationEngine.NationKingdomId == 0) BuildClaimSection();
            else BuildPage((int)_page);
        }

        private void BuildPage(int page)
        {
            switch ((CabinetPage)page)
            {
                case CabinetPage.Finance: BuildFinancePage(); break;
                case CabinetPage.Policy: BuildPolicyPage(); break;
                case CabinetPage.Decree: BuildDecreePage(); break;
                default: BuildDiplomacyPage(); break;
                case CabinetPage.Codex: BuildCodexPage(null); break;
            }
        }

        /// <summary>法典页：两区（法律 24 条 5 档 / 国策 16 条 3 档）+ 顶部当前国个性与 AI 建议。
        /// targetKingdom 为空时展示本国（玩家）；否则展示指定国（外交页跳转用）。</summary>
        private void BuildCodexPage(Kingdom targetKingdom)
        {
            var kingdom = targetKingdom != null ? targetKingdom
                : GameHelpers.FindKingdom(NationEngine.NationKingdomId);
            if (kingdom == null || kingdom.data == null)
            {
                AddLine(UIHelpers.L("cabinet_no_nation"), Muted, 12f);
                return;
            }
            long kid = kingdom.data.id;
            bool own = NationEngine.NationKingdomId == kid;
            AddLine(UIHelpers.Lf("cabinet_codex_nation", GameHelpers.SafeKingdomName(kingdom)), UIStyles.Gold, 13f);
            int style = CodexEngine.GetStyle(kid);
            AddLine(UIHelpers.Lf("cabinet_codex_style", UIHelpers.L(CodexEngine.StyleKeys[style])), UIStyles.Info, 12f);
            // AI 建议改为逐条嵌入法律行（当前档→建议档小按钮），不再占顶部整块
            AddDivider(DividerColor);

            // 法律区
            foreach (var cat in CodexEngine.LawCategories)
            {
                AddLine(UIHelpers.L("codex_cat_" + cat.Category), UIStyles.Gold, 12f);
                foreach (var key in cat.Keys)
                {
                    BuildLawRow(kingdom, key);
                }
            }
            AddLine("", Muted, 6f);

            // 国策区
            AddLine(UIHelpers.L("cabinet_codex_policy_hdr"), UIStyles.Gold, 12f);
            foreach (var key in CodexEngine.PolicyKeys)
            {
                BuildCodexPolicyRow(kingdom, key);
            }
        }

        private void BuildLawRow(Kingdom kingdom, string key)
        {
            int level = CodexEngine.GetLawLevel(kingdom.data.id, key);
            string name = UIHelpers.L(key);
            AddLine(name, level > 0 ? UIStyles.Positive : UIStyles.TextPrimary, 10f);
            var row = NewRow(CodexEngine.LawTiers, 22f);
            // 档位按钮（0..4，小号横排）
            for (int lv = 0; lv < CodexEngine.LawTiers; lv++)
            {
                int target = lv;
                AddRowButton(row, UIHelpers.Lf("codex_lv" + lv),
                    lv == level ? BtnGood : BtnColor,
                    () =>
                    {
                        string msg; bool ok = CodexEngine.SetLawLevel(kingdom, key, target, out msg);
                        GameHelpers.NotifyLocalized(msg);
                        if (ok) RefreshNow();
                    }, 86f);
            }
            // AI 建议（仅玩家国）：显示 当前→建议，点击采纳
            if (NationEngine.NationKingdomId == kingdom.data.id)
            {
                var st = CodexEngine.Get(kingdom.data.id);
                int suggest = CodexEngine.SuggestLawLevel(kingdom, key, st);
                if (suggest >= 0 && suggest != level)
                {
                    int stLv = suggest;
                    AddRowButton(row, UIHelpers.Lf("codex_suggest_btn", suggest), BtnGood, () =>
                    {
                        string msg; bool ok = CodexEngine.SetLawLevel(kingdom, key, stLv, out msg);
                        GameHelpers.NotifyLocalized(msg);
                        if (ok) RefreshNow();
                    }, 56f);
                }
            }
            AddRowSpacer(row);
        }

        private void BuildCodexPolicyRow(Kingdom kingdom, string key)
        {
            int level = CodexEngine.GetPolicyLevel(kingdom.data.id, key);
            string name = UIHelpers.L(key);
            AddLine(name, level > 0 ? UIStyles.Info : UIStyles.TextPrimary, 11f);
            var row = NewRow(2);
            for (int lv = 0; lv < CodexEngine.PolicyTiers; lv++)
            {
                int target = lv;
                AddRowButton(row, UIHelpers.Lf("codex_pol_lv" + lv),
                    lv == level ? BtnGood : BtnColor,
                    () =>
                    {
                        string msg; bool ok = CodexEngine.SetPolicyLevel(kingdom, key, target, out msg);
                        GameHelpers.NotifyLocalized(msg);
                        if (ok) RefreshNow();
                    });
            }
            AddRowSpacer(row);
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
                if (shown >= 12) break;
                string name = GameHelpers.SafeKingdomName(kingdom);
                long gdp = 0;
                if (EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out var ks)) gdp = (long)ks.GDP;
                var btn = UIHelpers.CreateButton(
                    UIHelpers.Lf("cabinet_claim_row", name, gdp),
                    CurPage.transform, -1, 30, _gameFont, BtnGood);
                long kid = kingdom.data.id;
                btn.onClick.AddListener(() =>
                {
                    int year = SafeYear();
                    if (NationEngine.Claim(GameHelpers.FindKingdom(kid), year, out _)) RefreshNow();
                });
                CurLines.Add(btn.gameObject);
                shown++;
            }
            if (shown == 0) AddLine(UIHelpers.L("picker_empty"), Muted, 12f);
        }

        // ===== 已认领页面 =====

        private void BuildFinancePage()
        {
            int year = SafeYear();
            AddLine(UIHelpers.Lf("cabinet_nation", NationEngine.NationName), UIStyles.Gold, 14f);
            AddLine(UIHelpers.Lf("cabinet_treasury", NationEngine.FormatGold(NationEngine.Treasury)),
                UIStyles.Gold, 14f);
            AddLine(UIHelpers.Lf("cabinet_flow", NationEngine.FormatGold(NationEngine.LastIncome),
                NationEngine.FormatGold(NationEngine.LastExpense)), Muted, 12f);
            int cooldown = NationEngine.LastSwitchYear + 10 - year;
            if (cooldown > 0)
                AddLine(UIHelpers.Lf("cabinet_switch_cooldown", cooldown), Muted, 12f);
            AddDivider(DividerColor);
            BuildRecordSection();
        }

        private void BuildPolicyPage()
        {
            int year = SafeYear();
            var cfg = UnrestConfig.Instance;
            AddLine(UIHelpers.Lf("cabinet_policies", NationEngine.SlotCount, cfg.PolicySlots), UIStyles.Gold, 13f);
            AddLine(UIHelpers.L("cabinet_policy_hint"), Muted, 11f);
            AddDivider(DividerColor);
            for (int kind = 0; kind < NationEngine.PolicyKindCount; kind++)
            {
                BuildPolicyRow((NationEngine.PolicyKind)kind, year);
                AddLine("", Muted, 4f);
            }
        }

        private void BuildDecreePage()
        {
            int year = SafeYear();
            AddLine(UIHelpers.L("cabinet_decrees"), UIStyles.Gold, 13f);
            AddLine(UIHelpers.L("cabinet_decree_hint"), Muted, 11f);
            AddDivider(DividerColor);
            BuildDecreeRow("nation_decree_relief", UIHelpers.Lf("cabinet_decree_cost_relief"), year < NationEngine.ReliefReadyYear,
                () => { if (NationEngine.TryEmergencyRelief(SafeYear())) RefreshNow(); });
            BuildDecreeRow("nation_decree_festival", UIHelpers.Lf("cabinet_decree_cost_festival"), year < NationEngine.FestivalReadyYear,
                () => { if (NationEngine.TryFestival(SafeYear())) RefreshNow(); });
            AddLine("", Muted, 6f);
            BuildNativeBuildings();
        }

        /// <summary>原版建筑放置区（RulerBox 式）：点建筑 → 放置模式 → 鼠标点击地图（本国领土）。
        /// 建筑 ID 按本国种族拼接（house_human_3 等）；放置模式时黄字提示。</summary>
        private void BuildNativeBuildings()
        {
            AddLine(UIHelpers.L("cabinet_build_native"), UIStyles.Gold, 13f);
            if (NationEngine.IsNativePlacing)
            {
                AddLine(UIHelpers.Lf("cabinet_place_mode", NationEngine.NativeBuildName), UIStyles.Warning, 12f);
            }
            else
            {
                AddLine(UIHelpers.L("cabinet_place_hint"), Muted, 11f);
            }

            string race = ""; // 本国种族（如 human）；取不到时走通用 ID
            var kingdom = GameHelpers.FindKingdom(NationEngine.NationKingdomId);
            if (kingdom != null)
            {
                try
                {
                    var raceAsset = kingdom.getActorAsset();
                    if (raceAsset != null) race = raceAsset.id;
                }
                catch (System.Exception) { }
            }

            // 建筑清单：(无种族前缀基础ID, 种族后缀, 显示键, 通用ID兜底)
            var defs = new (string Base, string Suffix, string Key, string Plain)[]
            {
                ("house", "_1", "build_native_house_t1", "house_1"),
                ("house", "_3", "build_native_house_t3", "house_3"),
                ("house", "_5", "build_native_house_t5", "house_5"),
                ("barracks", "", "build_native_barracks", "barracks"),
                ("watch_tower", "", "build_native_watchtower", "watch_tower"),
                ("well", "", "build_native_well", "well"),
                ("mine", "", "build_native_mine", "mine"),
                ("statue", "", "build_native_statue", "statue"),
                ("temple", "", "build_native_temple", "temple"),
                ("bonfire", "", "build_native_bonfire", "bonfire"),
            };

            for (int i = 0; i < defs.Length; i += 3)
            {
                var row = NewRow(3, 24f);
                for (int j = i; j < i + 3 && j < defs.Length; j++)
                {
                    var d = defs[j];
                    string id = string.IsNullOrEmpty(d.Suffix) ? d.Plain
                        : (race.Length > 0 ? d.Base + "_" + race + d.Suffix : d.Plain);
                    string label = UIHelpers.L(d.Key);
                    AddRowButton(row, label, BtnGood, () =>
                    {
                        if (NationEngine.BeginNativePlacement(id, label)) RefreshNow();
                    });
                }
            }
        }

        private void BuildDiplomacyPage()
        {
            AddLine(UIHelpers.L("cabinet_diplomacy"), UIStyles.Gold, 13f);
            AddLine(UIHelpers.L("cabinet_diplomacy_hint"), Muted, 11f);
            AddDivider(DividerColor);
            BuildDiplomacyList();
        }

        /// <summary>外交列表：他国（按 GDP 前 12），每国一行：好感 + 5 个动作按钮。</summary>
        private void BuildDiplomacyList()
        {
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
                if (shown >= 12) break;

                long kid = k.data.id;
                string name = GameHelpers.SafeKingdomName(k);
                int score = NationDiplomacy.GetRelationScore(k);
                int goodwill = NationDiplomacy.GetGoodwill(kid);
                int pactTier = NationDiplomacy.PactTier(kid);
                string status = UIHelpers.Lf("cabinet_dip_row", name, score, goodwill,
                    pactTier >= 0 ? UIHelpers.Lf("cabinet_dip_pact_tier", pactTier + 1) : "");
                AddLine(status, UIStyles.TextPrimary, 11f);
                int style = CodexEngine.GetStyle(kid);
                AddLine(UIHelpers.Lf("cabinet_dip_style", UIHelpers.L(CodexEngine.StyleKeys[style])), Muted, 10f);

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

            // 一行：名称+档位状态 + 费用（右侧弱色）——不再用两行大按钮
            var row = NewRow(2, 24f);
            var nameTxt = UIHelpers.CreateText(
                tier >= 0 ? UIHelpers.Lf("cabinet_policy_active", name, tier + 1) : name,
                row.transform, 12f, tier >= 0 ? UIStyles.Positive : UIStyles.TextPrimary, _gameFont, 22f);
            var nameLe = nameTxt.GetComponent<LayoutElement>();
            if (nameLe == null) nameLe = nameTxt.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;
            var costTxt = UIHelpers.CreateText(costText, row.transform, 10f, Muted, _gameFont, 18f);
            var costLe = costTxt.GetComponent<LayoutElement>();
            if (costLe == null) costLe = costTxt.AddComponent<LayoutElement>();
            costLe.flexibleWidth = 1f;

            // 按钮行：小号按钮横排（启用→升档→取消），不再拉满整行
            var btnRow = NewRow(3, 24f);
            if (tier < 0)
            {
                AddRowButton(btnRow, UIHelpers.L("cabinet_enable"), BtnGood, () =>
                {
                    if (NationEngine.EnablePolicy(kind, 0, SafeYear())) RefreshNow();
                }, 90f);
                AddRowSpacer(btnRow);
            }
            else if (tier < NationEngine.TierCount - 1)
            {
                AddRowButton(btnRow, UIHelpers.L("cabinet_upgrade"), BtnGood, () =>
                {
                    if (NationEngine.EnablePolicy(kind, tier + 1, SafeYear())) RefreshNow();
                }, 90f);
                AddRowSpacer(btnRow);
            }
            AddRowButton(btnRow, UIHelpers.L("cabinet_disable"), BtnBad,
                () => { if (NationEngine.DisablePolicy(kind)) RefreshNow(); }, 90f);
            if (btnRow.transform.childCount == 1) AddRowSpacer(btnRow);
        }

        private void BuildDecreeRow(string nameKey, string costText, bool cooling, System.Action action)
        {
            string name = UIHelpers.L(nameKey);
            var row = NewRow(2, 24f);
            var nameTxt = UIHelpers.CreateText(
                cooling ? UIHelpers.Lf("cabinet_decree_cooling", name) : name,
                row.transform, 12f, cooling ? Muted : UIStyles.TextPrimary, _gameFont, 22f);
            var nameLe2 = nameTxt.GetComponent<LayoutElement>();
            if (nameLe2 == null) nameLe2 = nameTxt.AddComponent<LayoutElement>();
            nameLe2.flexibleWidth = 1f;
            var costTxt = UIHelpers.CreateText(costText, row.transform, 10f, Muted, _gameFont, 18f);
            var costLe2 = costTxt.GetComponent<LayoutElement>();
            if (costLe2 == null) costLe2 = costTxt.AddComponent<LayoutElement>();
            costLe2.flexibleWidth = 1f;
            AddRowButton(row, UIHelpers.L("cabinet_execute"), cooling ? BtnColor : BtnGood, () => action(), 100f);
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
                if (shown >= 12) break;
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

        // ===== 行/按钮小构件（挂当前页） =====

        /// <summary>向当前页添加一行文本。</summary>
        private void AddLine(string text, Color color, float size)
        {
            var go = UIHelpers.CreateText(text, CurPage.transform, size, color, _gameFont, 22f);
            CurLines.Add(go);
        }

        private void AddDivider(Color color)
        {
            CurLines.Add(UIHelpers.CreateDivider(CurPage.transform, color));
        }

        private GameObject NewRow(int expectButtons, float height = 28f)
        {
            var row = new GameObject("CabinetRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(CurPage.transform, false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleWidth = 1f;
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.childForceExpandWidth = false;   // 紧凑：按钮按 preferredWidth 排布，不拉满整行
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            CurLines.Add(row);
            return row;
        }

        private void AddRowButton(GameObject row, string label, Color bg, System.Action onClick, float width = 120f)
        {
            var btn = UIHelpers.CreateButton(label, row.transform, width, 24, _gameFont, bg, 10f);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.flexibleWidth = 0f;
            le.preferredHeight = 24f;
            btn.onClick.AddListener(() => onClick());
        }

        private void AddRowSpacer(GameObject row)
        {
            var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(row.transform, false);
            var le = go.GetComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.preferredWidth = 1f;
            le.flexibleHeight = 1f;
        }

        private static int SafeYear()
        {
            try { return EconomyModMain.GetCurrentGameYear(); }
            catch (System.Exception) { return 0; }
        }

        public override void RefreshAllTexts()
        {
            base.RefreshAllTexts();
            for (int i = 0; i < PageCount; i++)
            {
                if (_tabButtons[i] == null) continue;
                var t = _tabButtons[i].GetComponentInChildren<Text>();
                if (t != null) t.text = UIHelpers.L(PageKeys[i]);
            }
            if (_visible && _panelRoot != null) RefreshNow();
        }

        /// <summary>窗口内容区随改变缩放重建（页面结构不变，仅刷新当前页）。</summary>
        protected override void OnPanelResized()
        {
            if (_visible) RefreshNow();
        }
    }
}
