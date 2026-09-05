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

        private enum CabinetPage { Finance = 0, Policy, Decree, Diplomacy, Law }
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
        private long _dipTargetId; // 外交详情页目标国（0 = 列表模式）
        private bool _refreshing;  // 重入保护：年度刷新恰逢按钮触发时防止双重 Destroy

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
            tabRt.sizeDelta = new Vector2(-Padding * 2, Fs(30f));
            var hlg = tabBar.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true; hlg.childControlHeight = true;

            for (int i = 0; i < PageCount; i++)
            {
                int page = i;
                var btn = UIHelpers.CreateButton(UIHelpers.L(PageKeys[i]), tabBar.transform, -1, Fs(28f), _gameFont, TabOff, Fs(12f));
                btn.onClick.AddListener(() => SwitchPage((CabinetPage)page));
                _tabButtons[i] = btn;
            }

            // 滚动内容区（页面容器挂这里）
            var scrollGo = UIHelpers.CreateScrollContent(_panelRect, Padding, Padding + Fs(30f) + 34f);
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
            UpdateTabHighlights(); // 高亮随打开的页面（曾漏调：点击 Tab 后永远停在第一个）
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
            if (_refreshing) return; // 重入保护（年度刷新与按钮点击同帧交错时跳过本次）
            _refreshing = true;
            try
            {
                DoRefreshNow();
            }
            finally
            {
                _refreshing = false;
            }
        }

        private void DoRefreshNow()
        {
            // 整页彻底重建：销毁当前页所有子物体（防止 ChartCard 等因注册遗漏累积成黑块）
            for (int i = CurPage.transform.childCount - 1; i >= 0; i--)
                Destroy(CurPage.transform.GetChild(i).gameObject);
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
                case CabinetPage.Law: BuildLawPage(null); break;
            }
        }

        /// <summary>法典页：两区（法律 24 条 5 档 / 国策 16 条 3 档）+ 顶部当前国个性与 AI 建议。
        /// targetKingdom 为空时展示本国（玩家）；否则展示指定国（外交页跳转用）。</summary>
        private void BuildLawPage(Kingdom targetKingdom)
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
            AddLine(UIHelpers.Lf("cabinet_law_nation", GameHelpers.SafeKingdomName(kingdom)), UIStyles.Gold, 13f);
            int style = LawEngine.GetStyle(kid);
            AddLine(UIHelpers.Lf("cabinet_law_style", UIHelpers.L(LawEngine.StyleKeys[style])), UIStyles.Info, 12f);
            AddLine(UIHelpers.L("cabinet_law_effect_title"), UIStyles.Gold, 12f);
            BuildLawEffectSummary(kid);
            // AI 建议改为逐条嵌入法律行（当前档→建议档小按钮），不再占顶部整块
            AddDivider(DividerColor);

            // 法律区
            foreach (var cat in LawEngine.LawCategories)
            {
                AddLine(UIHelpers.L("law_cat_" + cat.Category), UIStyles.Gold, 12f);
                foreach (var key in cat.Keys)
                {
                    BuildLawRow(kingdom, key);
                }
            }
            AddLine("", Muted, 6f);

            // 国策区
            AddLine(UIHelpers.L("cabinet_law_policy_hdr"), UIStyles.Gold, 12f);
            foreach (var key in LawEngine.PolicyKeys)
            {
                BuildLawPolicyRow(kingdom, key);
            }
        }

        /// <summary>法典聚合总览：直接读 LawMods，把偏离中性的乘数实时格式化展示（无需等年度）。</summary>
        private void BuildLawEffectSummary(long kid)
        {
            var m = LawEngine.GetMods(kid);
            var parts = new List<string>();
            System.Action<string, float> addPct = (key, v) =>
            {
                if (System.Math.Abs(v - 1f) > 0.0001f)
                    parts.Add(UIHelpers.L(key) + (v > 1f ? " +" : " -") + (System.Math.Abs(v - 1f) * 100f).ToString("F1") + "%");
            };
            addPct("law_eff_production", m.Productivity);
            addPct("law_eff_tax", m.TaxRate);
            addPct("law_eff_price", m.Price);
            addPct("law_eff_consume", m.Consumer);
            addPct("law_eff_disaster", m.DisasterResist);
            addPct("law_eff_build", m.BuildCost);
            addPct("law_eff_wage", m.Wage);
            addPct("law_eff_unrest", m.UnrestAccum);
            addPct("law_eff_happy", m.Happiness);
            addPct("law_eff_birth", m.Birth);
            if (System.Math.Abs(m.GiniShift) > 0.0001f)
                parts.Add(UIHelpers.L("law_eff_gini") + (m.GiniShift > 0f ? " +" : " -") + System.Math.Abs(m.GiniShift).ToString("F2"));
            if (System.Math.Abs(m.Military) > 0.0001f)
                parts.Add(UIHelpers.L("law_eff_military") + (m.Military > 0f ? " +" : " -") + System.Math.Abs(m.Military).ToString("F1"));

            if (parts.Count == 0)
            {
                AddLine(UIHelpers.L("cabinet_law_effect_none"), Muted, 11f);
                return;
            }
            for (int i = 0; i < parts.Count; i += 3)
            {
                int end = System.Math.Min(parts.Count, i + 3);
                AddLine(string.Join("｜", parts.GetRange(i, end - i).ToArray()), UIStyles.Info, 12f);
            }
        }

        private void BuildLawRow(Kingdom kingdom, string key)
        {
            int level = LawEngine.GetLawLevel(kingdom.data.id, key);
            string name = UIHelpers.L(key);
            AddLine(name, level > 0 ? UIStyles.Positive : UIStyles.TextPrimary, 12f);
            var row = NewRow(LawEngine.LawTiers, 30f, fill: true);
            // 档位按钮（0..4，小号横排）：显示该法律自己的档位语义名（如 无贸易保护→闭关锁国），缺失回退 无/轻/中/重/极
            for (int lv = 0; lv < LawEngine.LawTiers; lv++)
            {
                int target = lv;
                string lvKey = key + "_lv" + lv;
                string lvLabel = UIHelpers.L(lvKey);
                if (lvLabel == lvKey) lvLabel = UIHelpers.L("law_lv" + lv);
                AddRowButton(row, lvLabel,
                    lv == level ? BtnGood : BtnColor,
                    () =>
                    {
                        string msg; bool ok = LawEngine.SetLawLevel(kingdom, key, target, out msg);
                        GameHelpers.NotifyLocalized(msg);
                        if (ok) RefreshNow();
                    }, 86f);
            }
            // AI 建议（仅玩家国）：显示 当前→建议，点击采纳
            if (NationEngine.NationKingdomId == kingdom.data.id)
            {
                var st = LawEngine.Get(kingdom.data.id);
                int suggest = LawEngine.SuggestLawLevel(kingdom, key, st);
                if (suggest >= 0 && suggest != level)
                {
                    int stLv = suggest;
                    AddRowButton(row, UIHelpers.Lf("law_suggest_btn", suggest), BtnGood, () =>
                    {
                        string msg; bool ok = LawEngine.SetLawLevel(kingdom, key, stLv, out msg);
                        GameHelpers.NotifyLocalized(msg);
                        if (ok) RefreshNow();
                    }, 56f);
                }
            }
            AddRowSpacer(row);
        }

        private void BuildLawPolicyRow(Kingdom kingdom, string key)
        {
            int level = LawEngine.GetPolicyLevel(kingdom.data.id, key);
            string name = UIHelpers.L(key);
            AddLine(name, level > 0 ? UIStyles.Info : UIStyles.TextPrimary, 11f);
            var row = NewRow(2);
            for (int lv = 0; lv < LawEngine.PolicyTiers; lv++)
            {
                int target = lv;
                AddRowButton(row, UIHelpers.Lf("law_pol_lv" + lv),
                    lv == level ? BtnGood : BtnColor,
                    () =>
                    {
                        string msg; bool ok = LawEngine.SetPolicyLevel(kingdom, key, target, out msg);
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
                    CurPage.transform, -1, Fs(30f), _gameFont, BtnGood, Fs(12f));
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
            BuildGdpChart();
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
                var row = NewRow(3, 28f, fill: true);
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
            if (_dipTargetId != 0)
            {
                var target = GameHelpers.FindKingdom(_dipTargetId);
                if (target != null && target.data != null) { BuildDiplomacyDetail(target); return; }
                _dipTargetId = 0;
            }
            AddLine(UIHelpers.L("cabinet_diplomacy"), UIStyles.Gold, 13f);
            AddLine(UIHelpers.L("cabinet_diplomacy_hint"), Muted, 11f);
            AddDivider(DividerColor);
            BuildDiplomacyList();
        }

        /// <summary>外交列表：他国（按 GDP 前 12），每国一行：好感 + 5 个动作按钮。</summary>
        /// <summary>
        /// 旗帜图腾 Sprite（原版「图腾」部分）：data.get_banner_icon_id() → kingdom_banners_library
        /// 的 BannerAsset.icons[index]（元素可能是 Sprite 或含 sprite 字段的对象）。
        /// 全反射定位（编译期签名不稳定），失败返回 null（调用方回退 getElementIcon）。
        /// </summary>
        private static Sprite GetBannerIcon(Kingdom kingdom)
        {
            try
            {
                var data = kingdom.data;
                if (data == null) return null;
                const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
                var idMethod = data.GetType().GetMethod("get_banner_icon_id", F);
                if (idMethod == null) return null;
                int iconId = System.Convert.ToInt32(idMethod.Invoke(data, null));

                var amType = typeof(AssetManager);
                var libField = amType.GetField("kingdom_banners_library",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                if (libField == null) return null;
                var lib = libField.GetValue(null);
                if (lib == null) return null;
                var getter = lib.GetType().GetMethod("get", F);
                if (getter == null || getter.GetParameters().Length != 1) return null;
                var banner = getter.Invoke(lib, new object[] { "kingdom" });
                if (banner == null) return null;

                var iconsField = banner.GetType().GetField("icons", F);
                if (iconsField == null) return null;
                var icons = iconsField.GetValue(banner) as System.Collections.IList;
                if (icons == null || iconId < 0 || iconId >= icons.Count)
                {
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 图腾诊断：iconId=" + iconId
                        + " icons=" + (icons != null ? icons.Count : -1) + " banner=" + banner.GetType().Name);
                    return null;
                }
                var part = icons[iconId];
                if (part == null) return null;
                var spr = part as Sprite;
                if (spr != null) return spr;
                var p = part.GetType().GetProperty("sprite", F) ?? part.GetType().GetProperty("sprite", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (p == null) return null; // 元素类型无 sprite：诊断一次
                return p.GetValue(part) as Sprite;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[ClassicalEconomics] 图腾诊断异常: " + e.Message);
            }
            return null;
        }

        /// <summary>详情页：国徽 + 该国数据 + 可用外交动作，返回按钮回列表。</summary>
        private void BuildDiplomacyDetail(Kingdom target)
        {
            long kid = target.data.id;
            string name = GameHelpers.SafeKingdomName(target);

            var topRow = NewRow(2, 34f);
            // 行不强制拉伸子项，防止旗章被拉成竖条
            topRow.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = false;
            // 国徽：RulerBox 同款双层旗章（最先创建 = 排最左）——底色背景（getElementBackground + 主色）+ 徽记图标
            // （getElementIcon + 旗帜色）叠加于固定 26x26 容器，防止被行布局拉伸成大色块
            try
            {
                var flagWrap = new GameObject("FlagWrap", typeof(RectTransform));
                flagWrap.transform.SetParent(topRow.transform, false);
                var fwRt = flagWrap.GetComponent<RectTransform>();
                fwRt.anchorMin = new Vector2(0, 0.5f); fwRt.anchorMax = new Vector2(0, 0.5f);
                fwRt.pivot = new Vector2(0, 0.5f);
                fwRt.sizeDelta = new Vector2(26f, 26f);
                var fwLe = flagWrap.AddComponent<LayoutElement>();
                fwLe.preferredWidth = 26f; fwLe.preferredHeight = 26f;
                fwLe.flexibleWidth = 0f; fwLe.flexibleHeight = 0f;
                fwLe.minWidth = 26f; fwLe.minHeight = 26f;

                var bgGo = new GameObject("FlagBg", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(flagWrap.transform, false);
                var bgRt = bgGo.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
                var bgImg = bgGo.GetComponent<Image>();
                try { bgImg.sprite = target.getElementBackground(); } catch (System.Exception) { }
                try { bgImg.color = target.kingdomColor.getColorMain32(); } catch (System.Exception) { }

                var icGo = new GameObject("FlagIcon", typeof(RectTransform), typeof(Image));
                icGo.transform.SetParent(flagWrap.transform, false);
                var icRt = icGo.GetComponent<RectTransform>();
                icRt.anchorMin = Vector2.zero; icRt.anchorMax = Vector2.one;
                icRt.offsetMin = Vector2.zero; icRt.offsetMax = Vector2.zero;
                var icImg = icGo.GetComponent<Image>();
                // 图腾：优先官方旗帜库（banner_icon_id -> kingdom_banners_library.icons），失败回退 getElementIcon
                Sprite totem = GetBannerIcon(target);
                if (totem == null) { try { totem = target.getElementIcon(); } catch (System.Exception) { } }
                icImg.sprite = totem;
                try { icImg.color = target.kingdomColor.getColorBanner(); } catch (System.Exception) { }
            }
            catch (System.Exception) { }

            var crest = UIHelpers.CreateText(name, topRow.transform, Fs(15f), UIStyles.Gold, _gameFont, Fs(28f));
            var crestLe = crest.GetComponent<LayoutElement>();
            if (crestLe == null) crestLe = crest.AddComponent<LayoutElement>();
            crestLe.flexibleWidth = 1f;
            AddRowButton(topRow, UIHelpers.L("cabinet_dip_back"), BtnColor,
                () => { _dipTargetId = 0; RefreshNow(); }, 90f);
            AddDivider(DividerColor);

            EconomyMod.Models.KingdomStats ks;
            bool hasStats = EconomyEngine.KingdomStats.TryGetValue(kid, out ks) && ks != null;
            AddLine(UIHelpers.Lf("cabinet_dip_stat_relation",
                NationDiplomacy.GetRelationScore(target), NationDiplomacy.GetGoodwill(kid)), UIStyles.Info, 12f);
            int style = LawEngine.GetStyle(kid);
            AddLine(UIHelpers.Lf("cabinet_dip_stat_style",
                UIHelpers.L(LawEngine.StyleKeys[style])), Muted, 11f);
            if (hasStats)
            {
                AddLine(UIHelpers.Lf("cabinet_dip_stat_gdp", ks.GDP.ToString("N0")), UIStyles.Gold, 13f);
                AddLine(UIHelpers.Lf("cabinet_dip_stat_pop", ks.Population, ks.AvgWealth.ToString("F1")), Muted, 11f);
            }
            int laws = 0, pols = 0;
            for (int i = 0; i < LawEngine.LawKeys.Length; i++)
                if (LawEngine.GetLawLevel(kid, LawEngine.LawKeys[i]) > 0) laws++;
            for (int i = 0; i < LawEngine.PolicyKeys.Length; i++)
                if (LawEngine.GetPolicyLevel(kid, LawEngine.PolicyKeys[i]) > 0) pols++;
            AddLine(UIHelpers.Lf("cabinet_dip_stat_laws", laws, pols), Muted, 11f);
            bool atWar = NationDiplomacy.IsAtWarWith(target);
            AddLine(UIHelpers.L(atWar ? "cabinet_dip_stat_war" : "cabinet_dip_stat_peace"), UIStyles.Warning, 11f);
            AddDivider(DividerColor);

            AddLine(UIHelpers.L("cabinet_dip_actions"), UIStyles.Gold, 12f);
            var row1 = NewRow(3, 30f, fill: true);
            AddRowButton(row1, UIHelpers.L("cabinet_dip_war"), BtnBad, () =>
            {
                string msg; bool ok = NationDiplomacy.DeclareWar(target, out msg);
                GameHelpers.NotifyLocalized(msg, name);
                if (ok) RefreshNow();
            }, 110f, fill: true);
            AddRowButton(row1, UIHelpers.L("cabinet_dip_peace"), BtnColor, () =>
            {
                string msg; bool ok = NationDiplomacy.SueForPeace(target, out msg);
                GameHelpers.NotifyLocalized(msg, name);
                if (ok) RefreshNow();
            }, 110f, fill: true);
            AddRowButton(row1, UIHelpers.L("cabinet_dip_alliance"), BtnGood, () =>
            {
                string msg; bool ok = NationDiplomacy.FormAlliance(target, out msg);
                GameHelpers.NotifyLocalized(msg, name);
                if (ok) RefreshNow();
            }, 110f, fill: true);
            var row2 = NewRow(3, 30f, fill: true);
            int pactTier = NationDiplomacy.PactTier(kid);
            AddRowButton(row2, pactTier >= 0
                ? UIHelpers.Lf("cabinet_dip_pact_on", pactTier + 1)
                : UIHelpers.L("cabinet_dip_pact"), BtnGood, () =>
                {
                    string msg; bool ok;
                    if (pactTier >= 0) ok = NationDiplomacy.SignPact(target, pactTier + 1, out msg);
                    else ok = NationDiplomacy.SignPact(target, 0, out msg);
                    GameHelpers.NotifyLocalized(msg, name);
                    if (ok) RefreshNow();
                }, 110f);
            AddRowButton(row2, UIHelpers.L("cabinet_dip_gift"), BtnGood, () =>
            {
                string msg; bool ok = NationDiplomacy.GiveGift(target, out msg);
                GameHelpers.NotifyLocalized(msg, name);
                if (ok) RefreshNow();
            }, 110f, fill: true);
        }

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
                if (shown >= 40) break;

                long kid = k.data.id;
                string name = GameHelpers.SafeKingdomName(k);
                EconomyMod.Models.KingdomStats ks;
                string gdpTxt = "?";
                if (EconomyEngine.KingdomStats.TryGetValue(kid, out ks) && ks != null)
                    gdpTxt = ks.GDP.ToString("N0");
                string line = UIHelpers.Lf("cabinet_dip_list_row", name, gdpTxt,
                    NationDiplomacy.GetRelationScore(k), NationDiplomacy.GetGoodwill(kid));

                // 整行按钮：点击进入详情（列表仅按页重建；详情单独取对象，无每帧开销）
                var btn = UIHelpers.CreateButton(line, CurPage.transform, -1, Fs(30f), _gameFont, BtnColor, Fs(12f));
                btn.onClick.AddListener(() => { _dipTargetId = kid; RefreshNow(); });
                CurLines.Add(btn.gameObject);
                shown++;
            }
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
            var row = NewRow(2, 28f);
            var nameTxt = UIHelpers.CreateText(
                tier >= 0 ? UIHelpers.Lf("cabinet_policy_active", name, tier + 1) : name,
                row.transform, Fs(12f), tier >= 0 ? UIStyles.Positive : UIStyles.TextPrimary, _gameFont, Fs(24f));
            var nameLe = nameTxt.GetComponent<LayoutElement>();
            if (nameLe == null) nameLe = nameTxt.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;
            var costTxt = UIHelpers.CreateText(costText, row.transform, Fs(11f), Muted, _gameFont, Fs(20f));
            var costLe = costTxt.GetComponent<LayoutElement>();
            if (costLe == null) costLe = costTxt.AddComponent<LayoutElement>();
            costLe.flexibleWidth = 1f;

            // 按钮行：小号按钮横排（启用→升档→取消），不再拉满整行
            var btnRow = NewRow(3, 28f);
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
            var row = NewRow(2, 28f);
            var nameTxt = UIHelpers.CreateText(
                cooling ? UIHelpers.Lf("cabinet_decree_cooling", name) : name,
                row.transform, Fs(13f), cooling ? Muted : UIStyles.TextPrimary, _gameFont, Fs(24f));
            var nameLe2 = nameTxt.GetComponent<LayoutElement>();
            if (nameLe2 == null) nameLe2 = nameTxt.AddComponent<LayoutElement>();
            nameLe2.flexibleWidth = 1f;
            var costTxt = UIHelpers.CreateText(costText, row.transform, Fs(11f), Muted, _gameFont, Fs(20f));
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

        /// <summary>本国 GDP 折线图（历史最近 40 期；复用 ChartMeshGraphic 顶点图表，宽度随窗口）。</summary>
        /// <summary>与 HUD 概览共用同一数据源（HistoryService 快照）与 ChartMeshGraphic 渲染组件；
        /// 此处仅抽取本国单系列序列，不重复实现绘图逻辑。</summary>
        private void BuildGdpChart()
        {
            long mine = NationEngine.NationKingdomId;
            if (mine == 0) return;
            var snaps = HistoryService.GetRecent(40);
            if (snaps == null || snaps.Count < 3 || HasNationData(snaps, mine) < 2)
            {
                AddLine(UIHelpers.L("cabinet_gdp_chart_short"), Muted, 11f);
                return;
            }

            AddLine(UIHelpers.L("cabinet_gdp_chart"), UIStyles.Gold, 12f);
            var values = new float[snaps.Count];
            var phases = new int[snaps.Count];
            float vmax = 1f;
            int life = 0;
            for (int i = 0; i < snaps.Count; i++)
            {
                var sn = snaps[i];
                phases[i] = sn.Phase;
                values[i] = float.NaN;
                if (sn.Kingdoms != null)
                {
                    for (int j = 0; j < sn.Kingdoms.Count; j++)
                    {
                        if (sn.Kingdoms[j].KingdomId == mine)
                        {
                            values[i] = sn.Kingdoms[j].GDP;
                            if (values[i] > vmax) vmax = values[i];
                            life++;
                            break;
                        }
                    }
                }
            }

            var chartCard = UIComponents.CreateChartCard(CurPage.transform, 560f, 130f);
            chartCard.gameObject.name = "NationGdpCard";
            CurLines.Add(chartCard.gameObject); // 先注册：任何后续异常都不会残留
            var el = chartCard.GetComponent<LayoutElement>();
            el.preferredWidth = -1f;
            el.flexibleWidth = 1f; // 随窗口宽度自适应

            // 折线网格作为卡的独立子物体（与 HUD 概览同模式）：ChartMeshGraphic 约定 pivot=(0,0)、
            // 原点在左下角；四角拉伸随卡自适应，禁止与卡 Image 同 GameObject 共存（双 Graphic 互扰 + 尺寸为 0）
            var meshGo = new GameObject("GdpChartMesh", typeof(RectTransform), typeof(ChartMeshGraphic));
            meshGo.transform.SetParent(chartCard, false);
            var meshRt = meshGo.GetComponent<RectTransform>();
            meshRt.anchorMin = Vector2.zero;
            meshRt.anchorMax = Vector2.one;
            meshRt.pivot = Vector2.zero;
            meshRt.offsetMin = new Vector2(10f, 8f);
            meshRt.offsetMax = new Vector2(-10f, -8f);
            var chart = meshGo.GetComponent<ChartMeshGraphic>();
            chart.raycastTarget = false;
            chart.yAxisWidth = 44f;
            chart.margin = 4f;
            try
            {
                // 自适应刻度：以实际最小/最大值为界（避免 0 起使波动脉成直线）
                float vmin = float.MaxValue;
                float vmax2 = float.MinValue;
                for (int i = 0; i < values.Length; i++)
                {
                    if (float.IsNaN(values[i])) continue;
                    if (values[i] < vmin) vmin = values[i];
                    if (values[i] > vmax2) vmax2 = values[i];
                }
                if (vmax2 <= vmin) { vmax2 = vmin + 1f; }
                float pad = (vmax2 - vmin) * 0.15f;
                chart.SetChartData(new float[][] { values }, new[] { UIStyles.Gold }, null,
                    phases, 1, vmin - pad, vmax2 + pad, true, false, 0f, 0f);
                AddLine(UIHelpers.Lf("cabinet_gdp_chart_now",
                    values[values.Length - 1].ToString("N0"),
                    vmin.ToString("N0"), vmax2.ToString("N0")), Muted, 11f);
            }
            catch (System.Exception) { }
        }

        /// <summary>快照中本国拥有 GDP 数据的期数（该国有数据 ≥2 期才画曲线）。</summary>
        private static int HasNationData(System.Collections.Generic.List<EconomyMod.Models.EconomySnapshot> snaps, long kingdomId)
        {
            int life = 0;
            for (int i = 0; i < snaps.Count; i++)
            {
                var sn = snaps[i];
                if (sn.Kingdoms == null) continue;
                for (int j = 0; j < sn.Kingdoms.Count; j++)
                {
                    if (sn.Kingdoms[j].KingdomId == kingdomId) { life++; break; }
                }
            }
            return life;
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
            var go = UIHelpers.CreateText(text, CurPage.transform, Fs(size), color, _gameFont, Fs(22f));
            CurLines.Add(go);
        }

        /// <summary>UI 整体缩放（设置页 ui_scale，0.8~1.6，默认 1.2）：字号/按钮宽高/行高统一乘此系数。</summary>
        private static float Fs(float size)
        {
            var cfg = UnrestConfig.Instance;
            float scale = cfg != null ? cfg.UiScale : 1.2f;
            return size * Mathf.Clamp(scale, 0.8f, 1.6f);
        }

        private void AddDivider(Color color)
        {
            CurLines.Add(UIHelpers.CreateDivider(CurPage.transform, color));
        }

        private GameObject NewRow(int expectButtons, float height = 28f, bool fill = false)
        {
            var row = new GameObject("CabinetRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(CurPage.transform, false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = Fs(height);
            le.flexibleWidth = 1f;
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            // fill=true：子按钮均分整行（随窗口拖拽缩放自适应）；
            // fill=false：紧凑——按钮按固定宽排布，不拉满整行
            hlg.childForceExpandWidth = fill;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            CurLines.Add(row);
            return row;
        }

        private void AddRowButton(GameObject row, string label, Color bg, System.Action onClick, float width = 120f, bool fill = false)
        {
            var btn = UIHelpers.CreateButton(label, row.transform, Fs(width), Fs(28f), _gameFont, bg, Fs(12f));
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = Fs(width);
            le.flexibleWidth = fill ? 1f : 0f; // fill=true 均分剩余（随窗口缩放）
            le.preferredHeight = Fs(28f);
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

        /// <summary>UI 缩放配置变更后整窗重建（Tab 栏高度/滚动区偏移都依赖 Fs，仅刷当前页不够）。</summary>
        public void RebuildPanelFromScale()
        {
            if (_panelRoot != null) Destroy(_panelRoot);
            for (int i = 0; i < PageCount; i++)
            {
                _pages[i] = null;
                _pageLines[i] = new List<GameObject>();
            }
            BuildPanel();
            if (_visible)
            {
                _panelRoot.SetActive(true);
                RefreshNow();
            }
        }
    }
}
