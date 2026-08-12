using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;
using NeoModLoader.api.attributes;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 银行信贷与危机传染引擎：
    /// 富人（财富 > 2×人均）自动放贷给同城穷人，年利率 CreditRate。
    /// 萧条期违约率 DefaultRateDepression 飙升，违约导致放贷人（富人）财富损失。
    /// 违约率超过 CrisisContagionThreshold 时触发银行危机，沿贸易路线传染逆差贸易伙伴。
    /// 统计级模拟（不存储个债记录），零内存膨胀。
    /// </summary>
    public static class BankingEngine
    {
        // 复用缓冲（避免每年 GC 分配）
        private static readonly List<Actor> _richPool = new List<Actor>(16);

        /// <summary>本期违约导致的财富损失总量。</summary>
        public static long LastDefaultLoss { get; private set; }

        /// <summary>本期危机传染波及的王国数。</summary>
        public static int LastContagions { get; private set; }

        /// <summary>世界重置时清空信贷数据。</summary>
        public static void Reset()
        {
            LastDefaultLoss = 0;
            LastContagions = 0;
        }

        /// <summary>
        /// 每个采集周期调用一次：
        /// 1. 遍历王国，统计富人和穷人
        /// 2. 信贷规模 = 富人财富 × CreditRate
        /// 3. 违约 = 信贷规模 × 违约率（萧条期 DefaultRateDepression，其他期 2%）
        /// 4. 违约导致富人财富损失
        /// 5. 违约率 > CrisisContagionThreshold 时，逆差贸易伙伴遭受传染损失
        /// </summary>
        [Hotfixable]
        public static void Evaluate()
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.BankingEnabled) return;

            LastDefaultLoss = 0;
            LastContagions = 0;

            float avgWealth = EconomyEngine.AvgWealth;
            if (avgWealth <= 0f) return;

            float creditRate = cfg.CreditRate;
            bool isDepression = EconomyCycleModulator.CurrentPhase == EconomyPhase.Depression;
            float defaultRate = isDepression ? cfg.DefaultRateDepression : 0.02f;
            float contagionThreshold = cfg.CrisisContagionThreshold;

            var kingdoms = GameHelpers.KingdomSnapshot();
            foreach (var kingdom in kingdoms)
            {
                if (kingdom == null || kingdom.data == null) continue;
                if (kingdom.units == null || kingdom.units.Count == 0) continue;

                long kingdomId = kingdom.data.id;

                // 统计富人（财富 > 2×人均）
                _richPool.Clear();
                float richWealthTotal = 0f;
                int poorCount = 0;

                foreach (var actor in kingdom.units)
                {
                    if (actor == null || !actor.isAlive()) continue;
                    if (actor.asset == null || !actor.asset.civ) continue;
                    float w;
                    if (!GameHelpers.TryGetWealth(actor, out w)) continue;

                    if (w > avgWealth * 2f)
                    {
                        _richPool.Add(actor);
                        richWealthTotal += w;
                    }
                    else if (w < avgWealth * 0.5f)
                    {
                        poorCount++;
                    }
                }

                if (_richPool.Count == 0 || poorCount == 0) continue;

                // 信贷规模 = 富人财富 × 信贷率
                long creditAmount = (long)(richWealthTotal * creditRate);
                if (creditAmount <= 0) continue;

                // 违约：信贷规模 × 违约率
                long defaultAmount = (long)(creditAmount * defaultRate);
                if (defaultAmount > 0 && _richPool.Count > 0)
                {
                    long lossPerRich = defaultAmount / _richPool.Count;
                    foreach (var rich in _richPool)
                    {
                        if (rich == null || !rich.isAlive()) continue;
                        // long 直接 (int) 强转可能溢出为负 → 反而"加钱"；先钳制到 int 上限
                        try { rich.addMoney(-(int)Mathf.Min(lossPerRich, int.MaxValue)); } catch { }
                    }
                    LastDefaultLoss += defaultAmount;
                }

                // 危机传染：违约率超过阈值时，逆差贸易伙伴遭受损失
                if (defaultRate > contagionThreshold)
                {
                    var kingdomStats = EconomyEngine.KingdomStats;
                    foreach (var kvp in kingdomStats)
                    {
                        if (kvp.Key == kingdomId || kvp.Key == 0) continue;
                        if (kvp.Value.TradeBalance < 0) // 逆差王国 = 贸易伙伴
                        {
                            var partnerKingdom = GameHelpers.FindKingdom(kvp.Key);
                            if (partnerKingdom == null || partnerKingdom.units == null) continue;
                            long contagionLoss = (long)(defaultAmount * 0.3f);
                            if (contagionLoss > 0)
                            {
                                GameHelpers.DeductCoins(partnerKingdom.units, contagionLoss);
                                LastContagions++;
                            }
                        }
                    }
                }
            }

            if (LastDefaultLoss > 0)
            {
                string phase = isDepression ? "萧条期" : "";
                GameHelpers.Log($"[ClassicalEconomics] 银行系统{phase}违约：损失{LastDefaultLoss}金币" +
                                 (LastContagions > 0 ? $"，危机传染{LastContagions}国" : ""));
                if (isDepression && LastContagions > 0)
                    GameHelpers.Notify($"[经济] 银行危机！萧条违约蔓延{LastContagions}国，损失{LastDefaultLoss}金币");
                EventStreamService.Record(EventStreamService.TypeBanking, "", LastDefaultLoss);
            }
        }
    }
}
