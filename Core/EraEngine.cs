using System.Collections.Generic;
using System.Linq;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 王国时代事件引擎（取代"文化觉醒"）：
    /// - 三个正面时代事件（盛世/复兴/强盛期）+ 一个负面事件（经济崩溃），双通道触发：
    ///   富豪花钱（SpendingEngine.TryEraEvent）+ 周期自动（<see cref="Evaluate"/>）；
    /// - 国民加成（方案 A）：时代状态完全由本模组字典内部维护（不写原版王国特质，
    ///   避免与 vanilla 特质系统的未注册特质交互风险），<see cref="Tick"/> 每年把对应
    ///   国民 Actor 特质（携带 happiness/damage/armor/rate_birth 修正）同步给存活文明成员，
    ///   事件到期 removeTrait 干净复原；
    /// - 经济崩溃：萧条期 + 人均财富同比大幅下滑 → 负面特质 + 移民压力（人口流失）。
    /// 性能：无活跃事件时 Tick O(1) 早退；触发评估只读后台线程已算好的纯数据，主线程仅查表比对。
    /// </summary>
    internal static class EraEngine
    {
        // ===== 事件特质 ID（王国标记 / 国民加成）=====
        public const string KingdomTraitGolden   = "era_prosperous";
        public const string KingdomTraitRevival  = "era_revival";
        public const string KingdomTraitFlourish = "era_flourishing";
        public const string KingdomTraitCollapse = "era_collapse";

        public const string ActorTraitGolden  = "trait_golden_age";
        public const string ActorTraitRevival = "trait_revival";
        public const string ActorTraitFlourish = "trait_flourishing";
        public const string ActorTraitCollapse = "trait_collapse";

        /// <summary>王国特质 → 国民 Actor 特质（静态映射，无分配）。</summary>
        private static readonly Dictionary<string, string> KingdomToActor = new Dictionary<string, string>
        {
            { KingdomTraitGolden,   ActorTraitGolden },
            { KingdomTraitRevival,  ActorTraitRevival },
            { KingdomTraitFlourish, ActorTraitFlourish },
            { KingdomTraitCollapse, ActorTraitCollapse }
        };

        // ===== 状态 =====
        private static readonly Dictionary<long, string> _kingdomTrait = new Dictionary<long, string>(); // 王国 id → 当前王国特质
        private static readonly Dictionary<long, int> _startYears = new Dictionary<long, int>();         // 王国 id → 事件起始年
        private static readonly Dictionary<long, float> _prevAvg = new Dictionary<long, float>();       // 王国 id → 上一周期人均财富
        private static readonly Dictionary<long, int> _flourishStreak = new Dictionary<long, int>();    // 强盛期防抖连续期数

        // ===== 复用缓冲（避免 GC）=====
        private static readonly List<long> _expired = new List<long>();
        private static readonly List<long> _stale = new List<long>();
        private static readonly HashSet<long> _curIds = new HashSet<long>();
        private static readonly Dictionary<long, Kingdom> _kingdomById = new Dictionary<long, Kingdom>(32);
        private static readonly List<Actor> _memberPool = new List<Actor>(64);

        /// <summary>当前是否有活跃时代事件（Tick 的 O(1) 早退依据）。</summary>
        public static bool HasActive => _kingdomTrait.Count > 0;

        /// <summary>触发时代事件（SpendingEngine 花钱与 Evaluate 自动触发共用）。</summary>
        public static void Start(Kingdom kingdom, string kingdomTrait, int year)
        {
            if (kingdom == null || kingdom.data == null) return;
            long kid = kingdom.data.id;
            if (_kingdomTrait.ContainsKey(kid)) return; // 已有事件，不重复触发
            _kingdomTrait[kid] = kingdomTrait;
            _startYears[kid] = year;
            // M2 时代事件经济深度：触发时施加对应经济效果（财政盈余/救济重建/军费扩张）
            ApplyEraEconomicEffect(kingdom, kingdomTrait);
            // 注意：不写原版王国特质（era_* 未在 KingdomTraitLibrary 注册，addTrait 静默无效），
            // 时代标记仅存于本模组字典，国民加成走已注册的 Actor 特质（真实生效）。
            GameHelpers.Log($"[ClassicalEconomics] {EventName(kingdomTrait)} <{GameHelpers.SafeKingdomName(kingdom)}> 时代开启（国民加成生效）");
            GameHelpers.Notify($"[时代] {EventName(kingdomTrait)} <{GameHelpers.SafeKingdomName(kingdom)}> 开启");
            EventStreamService.Record(EventTypeOf(kingdomTrait), GameHelpers.SafeKingdomName(kingdom), 0);
        }

        /// <summary>
        /// M2 时代事件经济深度：触发时向王国注入对应经济效果——
        /// 盛世 = GDP×2% 财政盈余（城市仓库）；复兴 = GDP×1% 救济金（均分最穷成员）；强盛期 = GDP×3% 军费（城市仓库）。
        /// </summary>
        private static void ApplyEraEconomicEffect(Kingdom kingdom, string kingdomTrait)
        {
            if (kingdom == null || kingdom.data == null) return;
            long kid = kingdom.data.id;
            if (!EconomyEngine.KingdomStats.TryGetValue(kid, out var ks)) return;
            long gdp = ks.GDP;
            if (gdp <= 0) return;

            long amount;
            switch (kingdomTrait)
            {
                case KingdomTraitGolden:   amount = (long)(gdp * 0.02f); break; // 盛世财政盈余
                case KingdomTraitFlourish: amount = (long)(gdp * 0.03f); break; // 强盛期军费
                default:                   amount = 0L; break; // 复兴走救济通道
            }

            if (amount > 0)
            {
                var cities = kingdom.getCities();
                if (cities != null && cities.Count() > 0)
                {
                    long per = amount / cities.Count();
                    if (per > 0)
                    {
                        foreach (var city in cities)
                        {
                            if (city == null) continue;
                            try { city.addResourcesToRandomStockpile("gold", (int)per); } catch { }
                        }
                    }
                }
            }

            // 复兴救济金：均分给王国最穷的 8 名成员（经济重建均化，降低基尼）
            if (kingdomTrait == KingdomTraitRevival && kingdom.units != null && kingdom.units.Count > 0)
            {
                long relief = (long)(gdp * 0.01f);
                if (relief > 0)
                {
                    var poorest = new List<Actor>(8);
                    float edge = float.MaxValue;
                    foreach (var a in kingdom.units)
                    {
                        if (a == null || !a.isAlive()) continue;
                        if (a.asset == null || !a.asset.civ) continue;
                        float w;
                        if (!GameHelpers.TryGetWealth(a, out w)) continue;
                        GameHelpers.UpdateTopN(poorest, ref edge, a, w, 8, false);
                    }
                    if (poorest.Count > 0)
                    {
                        long per = relief / poorest.Count;
                        if (per > 0)
                        {
                            foreach (var a in poorest)
                            {
                                try { a.addMoney((int)per); } catch { }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>按当前周期阶段与王国条件选择可赞助的时代（无合适时代返回 null；供 SpendingEngine 花钱触发）。</summary>
        public static string PickSpendEra(Kingdom kingdom)
        {
            var phase = EconomyCycleModulator.CurrentPhase;
            if (phase == EconomyPhase.Boom) return KingdomTraitGolden;
            if (phase == EconomyPhase.Recovery) return KingdomTraitRevival;
            if (phase != EconomyPhase.Depression)
            {
                // 稳定期：基尼健康 + 军强
                if (kingdom == null || kingdom.data == null) return null;
                if (EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out var ks)
                    && ks.GiniCoefficient < UnrestConfig.Instance.CycleGiniLow
                    && WarriorRatio(kingdom) >= UnrestConfig.Instance.FlourishMilitaryRatio)
                    return KingdomTraitFlourish;
            }
            return null; // 萧条/衰退或条件不足
        }

        /// <summary>王国特质 → 事件流类型键。</summary>
        public static string EventTypeOf(string kingdomTrait)
        {
            switch (kingdomTrait)
            {
                case KingdomTraitGolden:  return EventStreamService.TypeEraGolden;
                case KingdomTraitRevival: return EventStreamService.TypeEraRevival;
                case KingdomTraitFlourish: return EventStreamService.TypeEraFlourish;
                default:                  return EventStreamService.TypeCollapse;
            }
        }

        /// <summary>王国特质 → 中文事件名（日志用）。</summary>
        public static string EventName(string kingdomTrait)
        {
            switch (kingdomTrait)
            {
                case KingdomTraitGolden:  return "盛世";
                case KingdomTraitRevival: return "复兴";
                case KingdomTraitFlourish: return "强盛期";
                default:                  return "经济崩溃";
            }
        }

        /// <summary>
        /// 每年评估一次（在后台统计消费后调用）：按周期阶段自动触发时代事件。
        /// 只读 TradeSimulationWorker.LastResult（后台已算好的纯数据），主线程零重计算。
        /// </summary>
        public static void Evaluate()
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.EraEnabled) return;
            var res = TradeSimulationWorker.LastResult;
            if (res == null || World.world == null) return;
            int year = EconomyModMain.GetCurrentGameYear();
            var phase = EconomyCycleModulator.CurrentPhase;
            bool isDepression = phase == EconomyPhase.Depression;

            // 构建 王国 id → Kingdom 查找表（复用缓冲，O(K)），避免每个王国多次 FindKingdom（O(K)）扫描
            var byId = _kingdomById;
            byId.Clear();
            if (World.world != null && World.world.kingdoms != null)
            {
                foreach (var k in World.world.kingdoms)
                    if (k != null && k.data != null) byId[k.data.id] = k;
            }

            foreach (var ks in res.Kingdoms)
            {
                if (ks.KingdomId == 0 || ks.ActorCount <= 0) continue;
                long kid = ks.KingdomId;
                float prev = _prevAvg.TryGetValue(kid, out float p) ? p : 0f;

                if (!_kingdomTrait.ContainsKey(kid))
                {
                    bool triggered = false;
                    // 盛世：繁荣期 + 人均 ≥ 全球均值
                    if (phase == EconomyPhase.Boom && ks.AvgWealth >= res.AvgWealth)
                    {
                        if (byId.TryGetValue(kid, out var k1))
                        {
                            Start(k1, KingdomTraitGolden, year);
                            triggered = true;
                        }
                    }
                    // 复兴：复苏期 + 人均环比回升
                    // M5：要求上一周期有正基线（prev > 0），否则首轮评估 prev=0 恒满足 >=，引发第一年假复苏
                    else if (phase == EconomyPhase.Recovery && prev > 0f && ks.AvgWealth >= prev)
                    {
                        if (byId.TryGetValue(kid, out var k2))
                        {
                            Start(k2, KingdomTraitRevival, year);
                            triggered = true;
                        }
                    }
                    // 强盛期：非萧条期 + 基尼健康 + 军强 + 连续防抖（盛世/复兴已触发则跳过）
                    if (!triggered && !isDepression && ks.Gini < cfg.CycleGiniLow
                        && byId.TryGetValue(kid, out var k3))
                    {
                        if (WarriorRatio(k3) >= cfg.FlourishMilitaryRatio)
                        {
                            int streak = (_flourishStreak.TryGetValue(kid, out int s) ? s : 0) + 1;
                            if (streak >= cfg.FlourishPeriods)
                            {
                                _flourishStreak.Remove(kid);
                                Start(k3, KingdomTraitFlourish, year);
                                triggered = true;
                            }
                            else _flourishStreak[kid] = streak;
                        }
                        else _flourishStreak.Remove(kid);
                    }
                    // 经济崩溃：萧条期 + 人均财富同比下滑 ≥ 比例
                    if (!triggered && isDepression && prev > 0f
                        && ks.AvgWealth < prev * (1f - cfg.CollapseDropRatio)
                        && byId.TryGetValue(kid, out var k4))
                    {
                        Start(k4, KingdomTraitCollapse, year);
                        ApplyEmigrationPressure(k4, ks);
                    }
                }
                _prevAvg[kid] = ks.AvgWealth;
            }

            // 清理已消失王国的上一周期记录（复用缓冲，O(K) 而非 O(K²)）
            var curIds = _curIds;
            curIds.Clear();
            foreach (var ks in res.Kingdoms) curIds.Add(ks.KingdomId);
            _stale.Clear();
            foreach (var kv in _prevAvg)
                if (!curIds.Contains(kv.Key)) _stale.Add(kv.Key);
            foreach (long id in _stale) _prevAvg.Remove(id);
        }

        /// <summary>
        /// 每年同步（在 Evaluate 之后调用）：
        /// 1. 到期事件移除（王国特质 + 国民特质 + 状态）；2. 活跃事件国民特质同步（无事件 O(1) 早退）。
        /// </summary>
        public static void Tick(int currentYear)
        {
            var cfg = UnrestConfig.Instance;

            // 1. 到期移除
            if (_startYears.Count > 0)
            {
                _expired.Clear();
                foreach (var kv in _startYears)
                {
                    string trait = _kingdomTrait[kv.Key];
                    int duration = trait == KingdomTraitCollapse
                        ? cfg.CollapseDurationYears : cfg.EraDurationYears;
                    if (currentYear - kv.Value >= duration) _expired.Add(kv.Key);
                }
                foreach (long kid in _expired)
                {
                    string trait = _kingdomTrait[kid];
                    var k = GameHelpers.FindKingdom(kid);
                    if (k != null) RemoveActorTraitFromMembers(k, KingdomToActor[trait]);
                    _kingdomTrait.Remove(kid);
                    _startYears.Remove(kid);
                }
            }

            // 2. 活跃事件国民特质同步（无事件 O(1) 早退）
            if (_kingdomTrait.Count == 0) return;
            foreach (var kv in _kingdomTrait)
            {
                var k = GameHelpers.FindKingdom(kv.Key);
                if (k == null) continue;
                AddActorTraitToMembers(k, KingdomToActor[kv.Value]);
            }
        }

        /// <summary>重置（新地图/新游戏）。</summary>
        public static void Reset()
        {
            _kingdomTrait.Clear();
            _startYears.Clear();
            _prevAvg.Clear();
            _flourishStreak.Clear();
        }

        // ===== 内部辅助 =====

        /// <summary>战士占比 = 战士数 / 人口（原版 countTotalWarriors / getPopulationTotal）。</summary>
        internal static float WarriorRatio(Kingdom kingdom)
        {
            try
            {
                int pop = kingdom.getPopulationTotal();
                if (pop <= 0) return 0f;
                return (float)kingdom.countTotalWarriors() / pop;
            }
            catch (System.Exception) { return 0f; }
        }

        /// <summary>给王国存活文明成员补国民特质（已有则跳过）。</summary>
        private static void AddActorTraitToMembers(Kingdom kingdom, string actorTrait)
        {
            if (kingdom == null || kingdom.units == null) return;
            var pool = _memberPool;
            pool.Clear();
            foreach (var a in kingdom.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (a.asset == null || !a.asset.civ) continue;
                pool.Add(a);
            }
            for (int i = 0; i < pool.Count; i++)
            {
                try
                {
                    if (!pool[i].hasTrait(actorTrait)) pool[i].addTrait(actorTrait, true);
                }
                catch (System.Exception) { }
            }
        }

        /// <summary>给王国存活文明成员移除国民特质（事件到期复原）。</summary>
        private static void RemoveActorTraitFromMembers(Kingdom kingdom, string actorTrait)
        {
            if (kingdom == null || kingdom.units == null) return;
            var pool = _memberPool;
            pool.Clear();
            foreach (var a in kingdom.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (a.asset == null || !a.asset.civ) continue;
                pool.Add(a);
            }
            for (int i = 0; i < pool.Count; i++)
            {
                try
                {
                    if (pool[i].hasTrait(actorTrait)) pool[i].removeTrait(actorTrait);
                }
                catch (System.Exception) { }
            }
        }

        /// <summary>经济崩溃移民压力：对崩溃王国随机成员施加不满特质（hotheaded），驱动人口流失。</summary>
        private static void ApplyEmigrationPressure(Kingdom kingdom, TradeSimulationWorker.KingdomSim ks)
        {
            if (kingdom == null || kingdom.units == null) return;
            int max = Mathf.Clamp(Mathf.RoundToInt(ks.Population * 0.05f), 1, 20);
            var pool = _memberPool;
            pool.Clear();
            foreach (var a in kingdom.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (a.asset == null || !a.asset.civ) continue;
                pool.Add(a);
                if (pool.Count >= max * 3) break;
            }
            if (pool.Count == 0) return;

            for (int i = pool.Count - 1; i > 0; i--) // Fisher-Yates
            {
                int j = Random.Range(0, i + 1);
                var tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
            }
            int limit = Mathf.Min(max, pool.Count);
            for (int i = 0; i < limit; i++)
            {
                try { pool[i].addTrait("hotheaded", true); } catch (System.Exception) { }
            }
        }
    }
}
