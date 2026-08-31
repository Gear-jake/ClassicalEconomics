using System.Collections.Generic;
using System.Linq;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>政策类型（M3 政策工具箱）：按经济情境选择，失败后果分级。</summary>
    public enum PolicyKind
    {
        Redistribution, // 贫富调节（高基尼，激进，失败后果严重）
        Fiscal,         // 财政政策（繁荣减税刺激/萧条增税补财政，失败后果轻微）
        Trade           // 贸易政策（关税调节，失败后果中度）
    }

    /// <summary>
    /// 国家政策引擎：高基尼王国每年以一定概率尝试"贫富调节政策"来降低基尼指数。
    /// M3 扩展为政策工具箱：按经济情境选择政策（贫富调节/财政/贸易），失败后果分级——
    /// 贫富调节失败最严重（退位/驾崩/内战），财政失败轻微（仅降低支持），贸易失败中度（贸易伙伴报复）。
    /// </summary>
    public static class PolicyEngine
    {
        /// <summary>基尼 ≥ 该值才考虑采取政策。</summary>
        private const float PolicyGiniThreshold = 0.6f;

        /// <summary>每年尝试政策的概率。</summary>
        private const float AttemptChance = 0.35f;

        /// <summary>低基尼时的基础成功率（随基尼升高线性降到 0.35）。</summary>
        private const float BaseSuccessChance = 0.65f;

        /// <summary>失败后的冷却期（年）：改革失败后 N 年内不重复尝试，避免连续换王/死王/内战。</summary>
        private const int CooldownYears = 6;

        /// <summary>王国 id → 冷却到期年份（改革失败后进入；到期自动解除，保证大国仍会定期尝试改革）。</summary>
        private static readonly Dictionary<long, int> _cooldown = new Dictionary<long, int>();

        // 冷却清理复用缓冲（避免每年分配）
        private static readonly List<long> _cooldownExpired = new List<long>();
        private static readonly List<City> _cityPool = new List<City>();

        /// <summary>重置（新地图/新游戏）：清空改革冷却记录，避免旧世界冷却泄漏进新地图（M7）。</summary>
        public static void Reset()
        {
            _cooldown.Clear();
            _cityPool.Clear();
        }

        /// <summary>
        /// 每年在 UnrestEngine.Evaluate 之后调用：对所有基尼超阈值且不在冷却的王国，
        /// 按概率尝试一次政策；成功则财富再分配降基尼，失败则统治者退位/驾崩或陷入内战
        /// 并进入 CooldownYears 年冷却（到期自动解除，避免永久性"高基尼却不改革"）。
        /// </summary>
        public static void Evaluate()
        {
            var cfg = UnrestConfig.Instance;
            if (!cfg.PolicyEnabled) return;
            if (World.world == null || World.world.kingdoms == null) return;
            int currentYear = EconomyModMain.GetCurrentGameYear();

            var kingdomList = GameHelpers.KingdomSnapshot();
            foreach (var kingdom in kingdomList)
            {
                if (kingdom == null || kingdom.data == null) continue;
                long kid = kingdom.data.id;
                if (!EconomyEngine.KingdomStats.TryGetValue(kid, out var stats)) continue;
                if (stats.GiniCoefficient < PolicyGiniThreshold) continue;
                if (_cooldown.TryGetValue(kid, out int until) && currentYear < until) continue; // 冷却中：到期前不重复尝试

                // 每年按概率尝试政策
                if (Random.value > AttemptChance) continue;

                // M3：按经济情境选择政策类型
                PolicyKind kind = PickPolicyKind(stats);

                if (RollSuccess(stats.GiniCoefficient, kind))
                {
                    ApplyPolicy(kingdom, stats, kind);
                }
                else
                {
                    FailPolicy(kingdom, stats, kind);
                    // 财政失败后果轻微，不进入冷却（可每年轻试）；贫富调节/贸易失败进入冷却
                    if (kind != PolicyKind.Fiscal)
                        _cooldown[kid] = currentYear + CooldownYears;
                }
            }

            // 清理：移除冷却已到期或王国已消失的条目，防止无限膨胀
            if (_cooldown.Count > 0)
            {
                var expired = _cooldownExpired;
                expired.Clear();
                foreach (var kv in _cooldown)
                {
                    if (currentYear >= kv.Value || GameHelpers.FindKingdom(kv.Key) == null)
                        expired.Add(kv.Key);
                }
                foreach (var id in expired) _cooldown.Remove(id);
            }
        }

        /// <summary>按经济情境选择政策类型：高基尼→贫富调节；繁荣→减税(财政)；萧条→增税(财政)；贸易逆差大→关税(贸易)。</summary>
        private static PolicyKind PickPolicyKind(KingdomStats stats)
        {
            var phase = EconomyCycleModulator.CurrentPhase;
            // 基尼极高 → 贫富调节（最激进）
            if (stats.GiniCoefficient > 0.75f) return PolicyKind.Redistribution;
            // 萧条 → 增税补财政
            if (phase == EconomyPhase.Depression) return PolicyKind.Fiscal;
            // 贸易逆差大 → 关税调节
            if (stats.TradeBalance < -stats.GDP * 0.03f) return PolicyKind.Trade;
            // 繁荣 → 减税刺激
            if (phase == EconomyPhase.Boom) return PolicyKind.Fiscal;
            // 默认贫富调节
            return PolicyKind.Redistribution;
        }

        /// <summary>成功概率：基尼越高成功率越低（改革越激进越易失败）；财政政策基础成功率更高。</summary>
        private static bool RollSuccess(float gini, PolicyKind kind)
        {
            float baseChance = kind == PolicyKind.Fiscal ? 0.75f : BaseSuccessChance;
            // gini=阈值 → base；gini=1.0 → 0.35
            float success = Mathf.Lerp(baseChance, 0.35f,
                Mathf.Clamp01((gini - PolicyGiniThreshold) / (1f - PolicyGiniThreshold)));
            return Random.value < success;
        }

        /// <summary>政策成功：按类型分派（贫富调节=劫富济贫；财政=税率调整；贸易=关税调节）。</summary>
        private static void ApplyPolicy(Kingdom kingdom, KingdomStats stats, PolicyKind kind)
        {
            switch (kind)
            {
                case PolicyKind.Redistribution:
                    // 劫富济贫规模按人口比例：大国动更多人，基尼才真正下降（原来固定 5 富 10 穷对大国无效）
                    int pop = Mathf.Max(1, stats.ActorCount);
                    int richCount = Mathf.Max(5, Mathf.RoundToInt(pop * 0.01f));   // 1% 富人（最少 5）
                    int poorCount = Mathf.Max(10, Mathf.RoundToInt(pop * 0.02f));  // 2% 穷人（最少 10）
                    GameHelpers.RedistributeWithinKingdom(kingdom, richCount, poorCount, 0.40f, 2f);
                    string kName = GameHelpers.SafeKingdomName(kingdom);
                    GameHelpers.NotifyLocalized("toast_policy_redistribute", kName);
                    EventStreamService.Record(EventStreamService.TypePolicy, kName, 1);
                    if (UnrestConfig.Instance.LogToWorldLog)
                        Debug.Log($"[ClassicalEconomics] 政策成功(贫富调节) 王国<{kName}> 基尼={stats.GiniCoefficient:F2}");
                    break;

                case PolicyKind.Fiscal:
                    ApplyFiscalPolicy(kingdom, stats);
                    break;

                case PolicyKind.Trade:
                    ApplyTradePolicy(kingdom, stats);
                    break;
            }
        }

        /// <summary>财政政策：繁荣期低税率刺激消费；萧条期高税率补充财政（对财富前5王国调整税率特质）。</summary>
        private static void ApplyFiscalPolicy(Kingdom kingdom, KingdomStats stats)
        {
            string kName = GameHelpers.SafeKingdomName(kingdom);
            bool isBoom = EconomyCycleModulator.CurrentPhase == EconomyPhase.Boom;
            try
            {
                // 繁荣减税（低税率特质）/ 萧条增税（高税率特质）
                string taxTrait = isBoom ? "tax_rate_local_low" : "tax_rate_local_high";
                if (kingdom.hasTrait(taxTrait)) return;
                if (isBoom)
                {
                    if (kingdom.hasTrait("tax_rate_local_high")) kingdom.removeTrait("tax_rate_local_high");
                }
                else
                {
                    if (kingdom.hasTrait("tax_rate_local_low")) kingdom.removeTrait("tax_rate_local_low");
                }
                kingdom.addTrait(taxTrait, true);
                GameHelpers.NotifyLocalized(isBoom ? "toast_policy_tax_boom" : "toast_policy_tax_austerity", kName);
                EventStreamService.Record(EventStreamService.TypePolicy, kName, isBoom ? 2 : 3);
                if (UnrestConfig.Instance.LogToWorldLog)
                    Debug.Log($"[ClassicalEconomics] 政策成功(财政) 王国<{kName}> {(isBoom ? "减税" : "增税")}");
            }
            catch (System.Exception) { }
        }

        /// <summary>贸易政策：逆差王国向居民征收关税并转入城市仓库，金币真实转移。</summary>
        private static void ApplyTradePolicy(Kingdom kingdom, KingdomStats stats)
        {
            string kName = GameHelpers.SafeKingdomName(kingdom);
            long tariffTarget = stats.TradeBalance < 0 ? (long)(-stats.TradeBalance * 0.1f) : 0L;
            long tariff = 0L;
            var cityPool = _cityPool;
            cityPool.Clear();
            try
            {
                var cities = kingdom.getCities();
                if (cities != null)
                {
                    foreach (var city in cities) if (city != null) cityPool.Add(city);
                }
                if (tariffTarget > 0 && cityPool.Count > 0 && kingdom.units != null)
                {
                    tariff = CollectTariff(kingdom.units, cityPool, tariffTarget);
                }
            }
            finally
            {
                cityPool.Clear();
            }
            GameHelpers.NotifyLocalized("toast_policy_tariff", kName, tariff);
            EventStreamService.Record(EventStreamService.TypePolicy, kName, 4);
            if (UnrestConfig.Instance.LogToWorldLog)
                Debug.Log($"[ClassicalEconomics] 政策成功(贸易) 王国<{kName}> 关税收入{tariff}");
        }

        private static long AddGoldToCity(City city, long amount)
        {
            long added = 0L;
            while (city != null && amount > 0)
            {
                int give = (int)System.Math.Min(amount, (long)int.MaxValue);
                try { city.addResourcesToRandomStockpile("gold", give); }
                catch (System.Exception) { break; }
                added += give;
                amount -= give;
            }
            return added;
        }

        private static long CollectTariff(List<Actor> units, List<City> cities, long target)
        {
            long remaining = target;
            long collected = 0L;
            int nextCity = 0;
            foreach (var actor in units)
            {
                if (actor == null || !actor.isAlive() || remaining <= 0) continue;
                int coins;
                try { coins = Mathf.Max(0, Mathf.RoundToInt(actor.money)); }
                catch (System.Exception) { continue; }
                if (coins <= 0) continue;

                int charge = System.Math.Min(coins, (int)System.Math.Min(remaining, (long)int.MaxValue));
                try { actor.addMoney(-charge); }
                catch (System.Exception) { continue; }

                long deposited = 0L;
                for (int offset = 0; offset < cities.Count && deposited < charge; offset++)
                {
                    int cityIndex = (nextCity + offset) % cities.Count;
                    deposited += AddGoldToCity(cities[cityIndex], charge - deposited);
                }
                nextCity = (nextCity + 1) % cities.Count;
                if (deposited < charge) GameHelpers.AddPositiveMoney(actor, charge - deposited);
                collected += deposited;
                remaining -= deposited;
            }
            return collected;
        }

        /// <summary>政策失败：后果分级——贫富调节最严重（退位/死亡/内战），贸易中度（贸易伙伴报复），财政轻微（仅支持下降）。</summary>
        private static void FailPolicy(Kingdom kingdom, KingdomStats stats, PolicyKind kind)
        {
            string kName = GameHelpers.SafeKingdomName(kingdom);

            // 财政失败：轻微后果（仅通知，不触发政权变动）
            if (kind == PolicyKind.Fiscal)
            {
                GameHelpers.NotifyLocalized("toast_policy_fiscal_fail", kName);
                EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 4); // 4=财政失败
                if (UnrestConfig.Instance.LogToWorldLog)
                    Debug.Log($"[ClassicalEconomics] 政策失败(财政) 王国<{kName}> 基尼={stats.GiniCoefficient:F2}");
                return;
            }

            // 贸易失败：中度后果（贸易伙伴报复，损失部分城市财富）
            if (kind == PolicyKind.Trade)
            {
                long loss = Mathf.Max(0, Mathf.RoundToInt(stats.GDP * 0.01f));
                if (loss > 0)
                {
                    var cities = kingdom.getCities();
                    if (cities != null)
                    {
                        foreach (var city in cities)
                        {
                            if (city == null) continue;
                            int gold;
                            try { gold = city.getResourcesAmount("gold"); } catch { gold = 0; }
                            int take = Mathf.Min(gold, Mathf.RoundToInt(loss / Mathf.Max(1, cities.Count())));
                            if (take > 0) { try { city.takeResource("gold", take); } catch { } }
                        }
                    }
                }
                GameHelpers.NotifyLocalized("toast_policy_tariff_fail", kName, loss);
                EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 5); // 5=贸易失败
                if (UnrestConfig.Instance.LogToWorldLog)
                    Debug.Log($"[ClassicalEconomics] 政策失败(贸易) 王国<{kName}> 损失{loss}");
                return;
            }

            // 贫富调节失败：严重后果（随机选择统治者退位、死亡或王国陷入内战：40% / 30% / 30%）
            float roll = Random.value;

            // 1) 退位（40%）
            if (roll < 0.4f)
            {
                if (kingdom.hasKing() && GameHelpers.TryRemoveKing(kingdom))
                {
                    GameHelpers.NotifyLocalized("toast_policy_fail_abdicate", kName);
                    EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 1);
                    if (UnrestConfig.Instance.LogToWorldLog)
                        Debug.Log($"[ClassicalEconomics] 政策失败 王国<{kName}> 国王退位（基尼={stats.GiniCoefficient:F2}）");
                }
                return;
            }

            // 2) 驾崩（30%）
            if (roll < 0.7f)
            {
                var king = kingdom.king;
                if (king != null && king.isAlive() && !king.hasDied())
                {
                    try { king.dieAndDestroy(AttackType.Other); } catch (System.Exception) { }
                    GameHelpers.NotifyLocalized("toast_policy_fail_death", kName);
                    EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 2);
                    if (UnrestConfig.Instance.LogToWorldLog)
                        Debug.Log($"[ClassicalEconomics] 政策失败 王国<{kName}> 国王死亡（基尼={stats.GiniCoefficient:F2}）");
                }
                return;
            }

            // 3) 内战（30%）：王国尚未在内战中 → 触发内战；已在内战或内战无法爆发（无人口/城市）→ 回退国王退位。
            // 注：显式拆开判断，避免原 `A || B == 0` 的短路歧义（A 为真时 B 完全不执行）。
            bool alreadyRebelling = UnrestEngine.GetState(kingdom.data.id, out _) == 2;
            if (!alreadyRebelling && UnrestEngine.TriggerCivilWar(kingdom) > 0)
            {
                GameHelpers.NotifyLocalized("toast_policy_fail_civilwar", kName);
                EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 3);
                if (UnrestConfig.Instance.LogToWorldLog)
                    Debug.Log($"[ClassicalEconomics] 政策失败 王国<{kName}> 内战爆发（基尼={stats.GiniCoefficient:F2}）");
                return;
            }
            if (kingdom.hasKing() && GameHelpers.TryRemoveKing(kingdom))
            {
                GameHelpers.NotifyLocalized("toast_policy_fail_abdicate", kName);
                EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 1);
                if (UnrestConfig.Instance.LogToWorldLog)
                    Debug.Log($"[ClassicalEconomics] 政策失败 王国<{kName}> 国王退位（内战触发失败回退，基尼={stats.GiniCoefficient:F2}）");
            }
        }
    }
}
