using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>【W2 分层】内阁窗口的"政务页"层：法典（法律/国策）、认领、中央银行、财政、政策、法令、本体建筑。</summary>
    public partial class CabinetWindow : FloatingWindow
    {
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

        /// <summary>银行·商务页（v1.5.0）：三要素档位 + 预设通道 + 风险灯 + 商业政策/税收。</summary>
        private void BuildBankPage()
        {
            var cfg = UnrestConfig.Instance;
            AddLine(UIHelpers.Lf("cabinet_nation", NationEngine.NationName), UIStyles.Gold, 14f);
            AddDivider(DividerColor);

            // 统计与风险灯
            AddLine(UIHelpers.Lf("bank_stats_reserves", NationEngine.FormatGold(BankEngine.PlayerReservesTotal)), UIStyles.Info, 12f);
            AddLine(UIHelpers.Lf("bank_stats_loans", NationEngine.FormatGold(BankEngine.PlayerOutstandingTotal)), UIStyles.Info, 12f);
            AddLine(UIHelpers.Lf("bank_stats_default", (BankEngine.LastDefaultRate * 100f).ToString("F1")), UIStyles.TextPrimary, 12f);
            int risk = BankEngine.RiskTier;
            var riskColor = risk == 2 ? UIStyles.Danger : risk == 1 ? UIStyles.Warning : UIStyles.Positive;
            AddLine(UIHelpers.Lf("bank_risk_label", UIHelpers.L("bank_risk_" + risk)), riskColor, 13f);
            AddDivider(DividerColor);

            // 三要素：利率 / 额度 / 准备金率
            BuildTierRow("bank_rate_label", BankEngine.RateTier,
                new[] { "bank_rate_high", "bank_rate_mid", "bank_rate_low" },
                i => { BankEngine.RateTier = i; RefreshNow(); });
            BuildTierRow("bank_quota_label", BankEngine.QuotaTier,
                new[] { "bank_quota_tight", "bank_quota_mid", "bank_quota_loose" },
                i => { BankEngine.QuotaTier = i; RefreshNow(); });
            BuildTierRow("bank_reserve_label", BankEngine.ReserveTier,
                new[] { "bank_reserve_high", "bank_reserve_mid", "bank_reserve_low" },
                i => { BankEngine.ReserveTier = i; RefreshNow(); });

            // 预设通道
            var presetRow = NewRow(3, 30f, fill: true);
            AddRowButton(presetRow, UIHelpers.L("bank_preset_stimulus"), BtnGood, () =>
            {
                BankEngine.ApplyChannel(0); GameHelpers.NotifyLocalized("bank_preset_applied"); RefreshNow();
            });
            AddRowButton(presetRow, UIHelpers.L("bank_preset_neutral"), BtnColor, () =>
            {
                BankEngine.ApplyChannel(1); GameHelpers.NotifyLocalized("bank_preset_applied"); RefreshNow();
            });
            AddRowButton(presetRow, UIHelpers.L("bank_preset_suppress"), BtnBad, () =>
            {
                BankEngine.ApplyChannel(2); GameHelpers.NotifyLocalized("bank_preset_applied"); RefreshNow();
            });
            AddDivider(DividerColor);

            // 商业区：税收 + 两政策（互斥开关）
            AddLine(UIHelpers.L("bank_commerce_title"), UIStyles.Gold, 12f);
            AddLine(UIHelpers.Lf("bank_commerce_tax", NationEngine.FormatGold(BankEngine.LastCommerceTax)), UIStyles.TextPrimary, 12f);
            var policyRow = NewRow(2, 30f, fill: true);
            AddRowButton(policyRow, UIHelpers.Lf("bank_policy_franchise",
                    BankEngine.FranchiseOn ? UIHelpers.L("bank_policy_on") : UIHelpers.L("bank_policy_off")),
                BankEngine.FranchiseOn ? BtnGood : BtnColor, () =>
            {
                BankEngine.SetFranchise(!BankEngine.FranchiseOn); RefreshNow();
            });
            AddRowButton(policyRow, UIHelpers.Lf("bank_policy_fairprice",
                    BankEngine.FairPriceOn ? UIHelpers.L("bank_policy_on") : UIHelpers.L("bank_policy_off")),
                BankEngine.FairPriceOn ? BtnGood : BtnColor, () =>
            {
                BankEngine.SetFairPrice(!BankEngine.FairPriceOn); RefreshNow();
            });
            if (cfg != null && !cfg.BankEnabled)
            {
                AddLine(UIHelpers.L("bank_disabled_note"), Muted, 11f);
            }
        }

        /// <summary>三要素档位行：标签 + 3 个档位按钮（当前档高亮）。</summary>
        private void BuildTierRow(string labelKey, int current, string[] tierKeys, System.Action<int> onPick)
        {
            var row = NewRow(4, 30f);
            var lbl = UIHelpers.CreateText(UIHelpers.L(labelKey), row.transform, Fs(12f),
                UIStyles.TextPrimary, _gameFont, Fs(26f));
            var lle = lbl.GetComponent<LayoutElement>();
            if (lle == null) lle = lbl.AddComponent<LayoutElement>();
            lle.flexibleWidth = 1f;
            for (int i = 0; i < tierKeys.Length; i++)
            {
                int tier = i;
                AddRowButton(row, UIHelpers.L(tierKeys[i]),
                    i == current ? BtnGood : BtnColor,
                    () => { onPick(tier); }, 86f);
            }
        }

        private void BuildFinancePage()
        {
            int year = SafeYear();
            // 待决事件区（S3-2）：有挂起抉择事件时置顶显示，无则零占位
            if (DecisionEvents.PendingCount > 0) BuildPendingEventsRow();
            AddLine(UIHelpers.Lf("cabinet_nation", NationEngine.NationName), UIStyles.Gold, 14f);
            string rulerDesc = RulerEngine.DescribeRuler(NationEngine.NationKingdomId);
            if (rulerDesc != null)
                AddLine(UIHelpers.Lf("cabinet_ruler", rulerDesc), Muted, 11f);
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

            // 【玩家诉求】允许"不想扮演国家时退出"——原先只有国王灭亡才会自动解绑，
            // 玩家认领后无法主动退出。两步确认避免误触清空金库。
            AddDivider(DividerColor);
            BuildLeaveNationRow();
        }

        /// <summary>退出国家扮演（两步确认）。退出后金库清零、政策解除，可随时重新认领。</summary>
        private void BuildLeaveNationRow()
        {
            var btn = UIHelpers.CreateButton(
                UIHelpers.L(_leaveArmed ? "cabinet_leave_confirm" : "cabinet_leave_nation"),
                CurPage.transform, -1, Fs(30f), _gameFont, _leaveArmed ? BtnBad : BtnColor, Fs(12f));
            btn.onClick.AddListener(() =>
            {
                if (!_leaveArmed)
                {
                    _leaveArmed = true;   // 首次点击：进入待确认态
                    RefreshNow();
                    return;
                }
                _leaveArmed = false;
                NationEngine.Unbind("toast_nation_leave");
                RefreshNow();
            });
            CurLines.Add(btn.gameObject);
            AddLine(UIHelpers.L("cabinet_leave_hint"), Muted, 10f);
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
                ("hall", "_0", "build_native_hall", "hall_human_0"),
                ("windmill", "_0", "build_native_windmill", "windmill_human_0"),
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
    }
}