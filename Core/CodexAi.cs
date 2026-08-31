using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.Core
{
    /// <summary>
    /// 法典 AI 决策：每年末对每个非玩家王国做 1 次「国情评分 → 目标档位 → 支付能力 → 执行」，
    /// 叠加国家个性权重（2% 年漂移）；重大变法（互斥切换/≥2 档/军事法律变动）进事件流+横幅，
    /// 微调只静默生效。玩家国由 CodexEngine.SetLawLevel/SetPolicyLevel 手动控制，AI 不代改。
    /// </summary>
    public static class CodexAi
    {
        private static readonly System.Random _rng = new System.Random();

        public static void TickNation(Kingdom kingdom, CodexEngine.NationState state, int year)
        {
            if (kingdom == null || kingdom.data == null) return;
            if (state.LastEvalYear == year) return;

            int style = state.Style;
            int majorChanges = 0;
            bool mutexSwitch = false;

            // 1) 法律：逐条找目标档（SuggestLawLevel + 风格修正 + 概率），执行前检查支付
            for (int i = 0; i < CodexEngine.LawKeys.Length; i++)
            {
                string key = CodexEngine.LawKeys[i];
                int suggest = CodexEngine.SuggestLawLevel(kingdom, key, state);
                if (suggest < 0) continue;
                suggest = StyleAdjust(style, key, suggest);

                int cur = state.LawLevels[i];
                if (suggest == cur) continue;
                if (_rng.NextDouble() > 0.4 + System.Math.Abs(suggest - cur) * 0.1) continue;

                if (suggest > 0)
                {
                    int g = MutexGroupOf(key);
                    if (g >= 0)
                    {
                        foreach (var other in CodexEngine.MutexGroups[g])
                        {
                            if (other == key) continue;
                            int oi = System.Array.IndexOf(CodexEngine.LawKeys, other);
                            if (oi >= 0 && state.LawLevels[oi] > 0) { state.LawLevels[oi] = 0; mutexSwitch = true; }
                        }
                    }
                }

                if (suggest > cur)
                {
                    long cost = (long)CodexEngine.LawUpgradeCost(kingdom, suggest);
                    if (cost > 0 && !CollectAIFunds(kingdom, cost)) continue;
                }

                state.LawLevels[i] = suggest;
                majorChanges++;
            }

            // 2) 国策：每 2 年最多动 1 条（更保守）
            if (year % 2 == 0)
            {
                for (int i = 0; i < CodexEngine.PolicyKeys.Length; i++)
                {
                    string key = CodexEngine.PolicyKeys[i];
                    int cur = state.PolicyLevels[i];
                    int want = SuggestPolicy(kingdom, key, cur, style, state);
                    if (want == cur) continue;
                    if (_rng.NextDouble() > 0.3) continue;
                    if (want > cur)
                    {
                        long cost = (long)CodexEngine.PolicyUpgradeCost(kingdom, want);
                        if (cost > 0 && !CollectAIFunds(kingdom, cost)) continue;
                    }
                    state.PolicyLevels[i] = want;
                    majorChanges++;
                    break;
                }
            }

            // 3) 个性漂移：2% 概率随机换风格
            if (_rng.NextDouble() < 0.02)
            {
                state.Style = _rng.Next(CodexEngine.StyleCount);
            }

            // 4) 重算聚合
            CodexEngine.RecomputeMods(kingdom.data.id, state);

            // 5) 分级事件
            bool militaryTouched = false;
            for (int i = 0; i < CodexEngine.LawKeys.Length; i++)
            {
                var k = CodexEngine.LawKeys[i];
                if (k == CodexEngine.LawConscription || k == CodexEngine.LawStandingArmy
                    || k == CodexEngine.LawMilitarism || k == CodexEngine.LawPacifism)
                {
                    if (state.LawLevels[i] > 1) militaryTouched = true;
                }
            }
            if (mutexSwitch || majorChanges >= 2 || militaryTouched)
            {
                string name = GameHelpers.SafeKingdomName(kingdom);
                if (mutexSwitch)
                {
                    EventStreamService.Record(EventStreamService.TypeCodexReform, name, 2);
                    GameHelpers.NotifyLocalized("toast_codex_reform_major", name);
                }
                else
                {
                    EventStreamService.Record(EventStreamService.TypeCodexReform, name, 1);
                    GameHelpers.NotifyLocalized("toast_codex_reform", name);
                }
            }
        }

        private static int MutexGroupOf(string key)
        {
            for (int g = 0; g < CodexEngine.MutexGroups.Length; g++)
                if (System.Array.IndexOf(CodexEngine.MutexGroups[g], key) >= 0) return g;
            return -1;
        }

        /// <summary>个性风格修正具体法律目标档（0-4 内钳制）。</summary>
        private static int StyleAdjust(int style, string key, int suggest)
        {
            switch (style)
            {
                case 0: // 尚武好战
                    if (key == CodexEngine.LawMilitarism) suggest += 1;
                    if (key == CodexEngine.LawPacifism) suggest -= 2;
                    if (key == CodexEngine.LawConscription || key == CodexEngine.LawStandingArmy) suggest += 1;
                    break;
                case 1: // 重商开放
                    if (key == CodexEngine.LawTradeFreedom || key == CodexEngine.LawFreeMarket) suggest += 1;
                    if (key == CodexEngine.LawPlannedEconomy || key == CodexEngine.LawStateReligion) suggest -= 1;
                    break;
                case 2: // 仁政福利
                    if (key == CodexEngine.LawEducation || key == CodexEngine.LawHealthcare
                        || key == CodexEngine.LawLandReform || key == CodexEngine.LawPacifism) suggest += 1;
                    if (key == CodexEngine.LawCapitalPun || key == CodexEngine.LawMilitarism) suggest -= 1;
                    break;
                case 3: // 法理严明
                    if (key == CodexEngine.LawJudicial || key == CodexEngine.LawAntiCorrupt
                        || key == CodexEngine.LawPress || key == CodexEngine.LawGunControl) suggest += 1;
                    break;
                case 4: // 闭关自守
                    if (key == CodexEngine.LawTradeFreedom) suggest -= 2;
                    if (key == CodexEngine.LawFreeMarket) suggest -= 1;
                    if (key == CodexEngine.LawPlannedEconomy) suggest += 1;
                    if (key == CodexEngine.LawMigrant) suggest -= 1;
                    break;
                case 5: // 科技兴邦
                    if (key == CodexEngine.LawEducation) suggest += 2;
                    if (key == CodexEngine.LawSecularism) suggest += 1;
                    if (key == CodexEngine.LawReligion) suggest -= 1;
                    break;
            }
            return System.Math.Max(0, System.Math.Min(CodexEngine.LawTiers - 1, suggest));
        }

        /// <summary>国策目标档（3 档：0/1/2），按国情与个性。</summary>
        private static int SuggestPolicy(Kingdom kingdom, string key, int cur, int style, CodexEngine.NationState state)
        {
            EconomyMod.Models.KingdomStats ks;
            if (!EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out ks) || ks == null) return cur;
            float gini = ks.GiniCoefficient;
            bool atWar = NationDiplomacy.IsAtWarWith(kingdom) || state.Mods.Military > 0.3f;
            switch (key)
            {
                case CodexEngine.PolicyLowTax: return gini < 0.4f ? 2 : (gini > 0.6f ? 0 : 1);
                case CodexEngine.PolicyAusterity: return gini > 0.7f ? 2 : (gini > 0.55f ? 1 : 0);
                case CodexEngine.PolicySubsidy: return ks.GDP > 0f && ks.AvgWealth < 30f ? 2 : 0;
                case CodexEngine.PolicyTradeDeal: return style == 1 ? 2 : 1;
                case CodexEngine.PolicyPoorRelief: return gini > 0.6f ? 2 : (gini > 0.5f ? 1 : 0);
                case CodexEngine.PolicyPublicWork: return style == 2 ? 2 : 1;
                case CodexEngine.PolicyFamily: return ks.Population < 2000 ? 2 : 0;
                case CodexEngine.PolicyFestival: return gini > 0.8f ? 0 : 1;
                case CodexEngine.PolicyWarFund: return atWar ? 2 : 0;
                case CodexEngine.PolicyRecruit: return atWar ? 2 : 0;
                case CodexEngine.PolicyFortify: return atWar ? 2 : (cur > 0 ? 0 : 1);
                case CodexEngine.PolicyBorderGuard: return atWar ? 2 : 1;
                case CodexEngine.PolicyDiplomacy: return style == 1 ? 2 : (atWar ? 1 : 0);
                case CodexEngine.PolicyIsolation: return style == 4 ? 2 : (style == 1 ? 0 : 1);
                case CodexEngine.PolicyExpansion: return style == 0 ? 2 : 0;
                case CodexEngine.PolicyReparations: return atWar ? 1 : 0;
                default: return cur;
            }
        }

        /// <summary>AI 变法费用：城市仓库 + 居民征收（守恒）；不足返回 false。</summary>
        private static bool CollectAIFunds(Kingdom kingdom, long cost)
        {
            long got = 0;
            try
            {
                var cities = kingdom.getCities();
                if (cities != null)
                    foreach (var c in cities)
                    {
                        if (c == null || got >= cost) continue;
                        int gold;
                        try { gold = c.getResourcesAmount("gold"); } catch (System.Exception) { gold = 0; }
                        if (gold <= 0) continue;
                        int take = System.Math.Min(gold, (int)System.Math.Min(cost - got, int.MaxValue));
                        try { c.takeResource("gold", take); got += take; } catch (System.Exception) { }
                    }
            }
            catch (System.Exception) { }
            if (got < cost && kingdom.units != null)
            {
                var units = new List<Actor>();
                try { foreach (var a in kingdom.units) if (a != null && a.isAlive()) units.Add(a); } catch (System.Exception) { }
                got += GameHelpers.DeductCoins(units, cost - got);
            }
            return got >= cost;
        }
    }
}
