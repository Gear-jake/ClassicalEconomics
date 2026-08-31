using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.Core
{
    /// <summary>
    /// 法典 AI 决策：每年末对每个非玩家王国做 1 次「国情评分 → 目标档位 → 支付能力 → 执行」，
    /// 叠加国家个性权重（2% 年漂移）；重大变法（互斥切换/≥2 档/军事法律变动）进事件流+横幅，
    /// 微调只静默生效。玩家国由 LawEngine.SetLawLevel/SetPolicyLevel 手动控制，AI 不代改。
    /// </summary>
    public static class LawAi
    {
        private static readonly System.Random _rng = new System.Random();

        public static void TickNation(Kingdom kingdom, LawEngine.NationState state, int year)
        {
            if (kingdom == null || kingdom.data == null) return;
            if (state.LastEvalYear == year) return;

            int style = state.Style;
            bool atWar = NationDiplomacy.IsAtWarWith(kingdom); // hoist：一年只判断一次（原来每条法律都查）
            int majorChanges = 0;
            bool mutexSwitch = false;
            bool _militaryChanged = false;

            // 1) 法律：逐条找目标档（SuggestLawLevel + 风格修正 + 概率），执行前检查支付
            for (int i = 0; i < LawEngine.LawKeys.Length; i++)
            {
                string key = LawEngine.LawKeys[i];
                int suggest = LawEngine.SuggestLawLevel(kingdom, key, state, atWar);
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
                        // 5 年互斥冷却（AI 防振荡）；玩家路径不受此约束
                        if (state.MutexCooldownYear > year)
                            continue;
                        state.MutexCooldownYear = year + 5;
                        state.MutexGroupId = g;
                        foreach (var other in LawEngine.MutexGroups[g])
                        {
                            if (other == key) continue;
                            int oi = System.Array.IndexOf(LawEngine.LawKeys, other);
                            if (oi >= 0 && state.LawLevels[oi] > 0) { state.LawLevels[oi] = 0; mutexSwitch = true; }
                        }
                    }
                }

                if (suggest > cur)
                {
                    long cost = (long)LawEngine.LawUpgradeCost(kingdom, suggest);
                    if (cost > 0 && !CollectAIFunds(kingdom, cost)) continue;
                }

                state.LawLevels[i] = suggest;
                majorChanges++;
                if (key == LawEngine.LawConscription || key == LawEngine.LawStandingArmy
                    || key == LawEngine.LawMilitarism || key == LawEngine.LawPacifism)
                {
                    _militaryChanged = true;
                }
            }

            // 2) 国策：每 2 年最多动 1 条（更保守）
            if (year % 2 == 0)
            {
                for (int i = 0; i < LawEngine.PolicyKeys.Length; i++)
                {
                    string key = LawEngine.PolicyKeys[i];
                    int cur = state.PolicyLevels[i];
                    int want = SuggestPolicy(kingdom, key, cur, style, state);
                    if (want == cur) continue;
                    if (_rng.NextDouble() > 0.3) continue;
                    if (want > cur)
                    {
                        long cost = (long)LawEngine.PolicyUpgradeCost(kingdom, want);
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
                state.Style = _rng.Next(LawEngine.StyleCount);
            }

            // 4) 重算聚合
            LawEngine.RecomputeMods(kingdom.data.id, state);

            // 5) 分级事件（只统计本 tick 变更过的军事法律；不得用"最终态"判定——
            //    否则年年 ≥2 的军事法国家会每一年都报"重大变法"）
            bool militaryTouched = _militaryChanged;
            if (mutexSwitch || majorChanges >= 2 || militaryTouched)
            {
                string name = GameHelpers.SafeKingdomName(kingdom);
                if (mutexSwitch)
                {
                    EventStreamService.Record(EventStreamService.TypeLawReform, name, 2);
                    GameHelpers.NotifyLocalized("toast_law_reform_major", name);
                }
                else
                {
                    EventStreamService.Record(EventStreamService.TypeLawReform, name, 1);
                    GameHelpers.NotifyLocalized("toast_law_reform", name);
                }
            }
        }

        private static int MutexGroupOf(string key)
        {
            for (int g = 0; g < LawEngine.MutexGroups.Length; g++)
                if (System.Array.IndexOf(LawEngine.MutexGroups[g], key) >= 0) return g;
            return -1;
        }

        /// <summary>个性风格修正具体法律目标档（0-4 内钳制）。</summary>
        private static int StyleAdjust(int style, string key, int suggest)
        {
            switch (style)
            {
                case 0: // 尚武好战
                    if (key == LawEngine.LawMilitarism) suggest += 1;
                    if (key == LawEngine.LawPacifism) suggest -= 2;
                    if (key == LawEngine.LawConscription || key == LawEngine.LawStandingArmy) suggest += 1;
                    break;
                case 1: // 重商开放
                    if (key == LawEngine.LawTradeFreedom || key == LawEngine.LawFreeMarket) suggest += 1;
                    if (key == LawEngine.LawPlannedEconomy || key == LawEngine.LawStateReligion) suggest -= 1;
                    break;
                case 2: // 仁政福利
                    if (key == LawEngine.LawEducation || key == LawEngine.LawHealthcare
                        || key == LawEngine.LawLandReform || key == LawEngine.LawPacifism) suggest += 1;
                    if (key == LawEngine.LawCapitalPun || key == LawEngine.LawMilitarism) suggest -= 1;
                    break;
                case 3: // 法理严明
                    if (key == LawEngine.LawJudicial || key == LawEngine.LawAntiCorrupt
                        || key == LawEngine.LawPress || key == LawEngine.LawGunControl) suggest += 1;
                    break;
                case 4: // 闭关自守
                    if (key == LawEngine.LawTradeFreedom) suggest -= 2;
                    if (key == LawEngine.LawFreeMarket) suggest -= 1;
                    if (key == LawEngine.LawPlannedEconomy) suggest += 1;
                    if (key == LawEngine.LawMigrant) suggest -= 1;
                    break;
                case 5: // 科技兴邦
                    if (key == LawEngine.LawEducation) suggest += 2;
                    if (key == LawEngine.LawSecularism) suggest += 1;
                    if (key == LawEngine.LawReligion) suggest -= 1;
                    break;
            }
            return System.Math.Max(0, System.Math.Min(LawEngine.LawTiers - 1, suggest));
        }

        /// <summary>国策目标档（3 档：0/1/2），按国情与个性。</summary>
        private static int SuggestPolicy(Kingdom kingdom, string key, int cur, int style, LawEngine.NationState state)
        {
            EconomyMod.Models.KingdomStats ks;
            if (!EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out ks) || ks == null) return cur;
            float gini = ks.GiniCoefficient;
            bool atWar = NationDiplomacy.IsAtWarWith(kingdom) || state.Mods.Military > 0.3f;
            switch (key)
            {
                case LawEngine.PolicyLowTax: return gini < 0.4f ? 2 : (gini > 0.6f ? 0 : 1);
                case LawEngine.PolicyAusterity: return gini > 0.7f ? 2 : (gini > 0.55f ? 1 : 0);
                case LawEngine.PolicySubsidy: return ks.GDP > 0f && ks.AvgWealth < 30f ? 2 : 0;
                case LawEngine.PolicyTradeDeal: return style == 1 ? 2 : 1;
                case LawEngine.PolicyPoorRelief: return gini > 0.6f ? 2 : (gini > 0.5f ? 1 : 0);
                case LawEngine.PolicyPublicWork: return style == 2 ? 2 : 1;
                case LawEngine.PolicyFamily: return ks.Population < 2000 ? 2 : 0;
                case LawEngine.PolicyFestival: return gini > 0.8f ? 0 : 1;
                case LawEngine.PolicyWarFund: return atWar ? 2 : 0;
                case LawEngine.PolicyRecruit: return atWar ? 2 : 0;
                case LawEngine.PolicyFortify: return atWar ? 2 : (cur > 0 ? 0 : 1);
                case LawEngine.PolicyBorderGuard: return atWar ? 2 : 1;
                case LawEngine.PolicyDiplomacy: return style == 1 ? 2 : (atWar ? 1 : 0);
                case LawEngine.PolicyIsolation: return style == 4 ? 2 : (style == 1 ? 0 : 1);
                case LawEngine.PolicyExpansion: return style == 0 ? 2 : 0;
                case LawEngine.PolicyReparations: return atWar ? 1 : 0;
                default: return cur;
            }
        }

        // 复用缓冲（年度多次调用，避免每次 new List）
        private static readonly List<Actor> _unitsBuf = new List<Actor>(64);

        /// <summary>AI 变法费用：城市仓库 + 居民征收（守恒）；不足返回 false。</summary>
        public static bool CollectAIFunds(Kingdom kingdom, long cost)
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
                _unitsBuf.Clear();
                try { foreach (var a in kingdom.units) if (a != null && a.isAlive()) _unitsBuf.Add(a); } catch (System.Exception) { }
                got += GameHelpers.DeductCoins(_unitsBuf, cost - got);
            }
            return got >= cost;
        }
    }
}
