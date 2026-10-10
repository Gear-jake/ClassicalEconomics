using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;
using Newtonsoft.Json;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>抉择事件：**后果执行**部分（W3.4 拆分自 DecisionEvents.cs）。
    /// 全部组合现有通道（金库/征税/济贫/外交好感/动荡/战争），金币守恒；
    /// 与核心判定分离后，改效果不会碰到抽签与冷却逻辑。</summary>
    public static partial class DecisionEvents
    {
        // ===== 后果执行（全部组合现有通道，金币守恒）=====

        private static void Execute(EventDef d, Kingdom k, int optIndex, int year, bool isPlayer)
        {
            if (d == null || optIndex < 0 || optIndex >= d.options.Count) return;
            if (k == null || k.data == null) return; // 王国已亡：事件自然落空（不退款——金库/条件本就属于已亡国）
            var o = d.options[optIndex];
            long goldMoved = 0;
            var stats = NationEngine.NationStats();
            float gdp = stats?.GDP ?? 0f;
            bool playerInvolved = isPlayer || k.data.id == NationEngine.NationKingdomId;

            // 1. 金库变动（GDP 比例；负 = 支出）
            if (o.treasuryGdpRatio != 0f && playerInvolved)
            {
                long amount = (long)(gdp * o.treasuryGdpRatio);
                if (amount > 0) { NationEngine.AddTreasury(amount); goldMoved = amount; }
                else if (amount < 0)
                {
                    long cost = -amount;
                    if (NationEngine.TrySpend(cost)) goldMoved = -cost;
                }
            }

            // 2. 居民征税（财富比例 → 金库；真实转移）
            if (o.residentsTaxRatio > 0f && playerInvolved)
            {
                long target = (long)((stats?.ActorCount ?? 0) * (stats?.AvgWealth ?? 0f) * o.residentsTaxRatio);
                long collected = NationEngine.CollectFromResidents(k, target);
                NationEngine.AddTreasury(collected);
                goldMoved += collected;
            }

            // 3. 济贫分发（金库 → 贫民；真实转移）
            if (o.poorReliefRatio > 0f && playerInvolved)
            {
                long fund = (long)(NationEngine.Treasury * o.poorReliefRatio);
                if (fund > 0 && NationEngine.TrySpend(fund))
                    NationEngine.DistributeToPoor(k, fund, stats);
            }

            // 4. 全局外交好感
            if (o.goodwillAll != 0)
                NationDiplomacy.AddGoodwillAll(o.goodwillAll);

            // 5. 动荡
            if (o.unrest)
            {
                try { UnrestEngine.Incite(k); } catch (System.Exception) { }
            }

            _readyYear[d.id] = year + System.Math.Max(0, d.cooldownYears);

            // 5.4 商路断绝：商业税减半至指定年（BankEngine 消费）
            if (o.commercePenaltyYears > 0)
                BankEngine.SetCommercePenalty(year + o.commercePenaltyYears);

            // 5.45 v1.8 效果库扩展：宣战/结盟/迁都/建筑升级/个体财富/世界大战
            if (o.declareWarTarget >= -1 && o.declareWarTarget != 0)
            {
                var t = NationDiplomacy.PickTarget(k, o.declareWarTarget, true);
                if (t != null)
                {
                    string msg;
                    NationDiplomacy.StartWarBetween(k, t, out msg);
                }
            }
            if (o.formAllianceTarget >= -1 && o.formAllianceTarget != 0)
            {
                var t = NationDiplomacy.PickTarget(k, o.formAllianceTarget, false);
                if (t != null)
                {
                    string msg;
                    NationDiplomacy.FormAllianceBetween(k, t, out msg);
                }
            }
            if (o.moveCapital) TryMoveCapital(k);
            if (o.upgradeBuildings > 0) TryUpgradeBuildings(k, o.upgradeBuildings);
            if (o.citizenWealthRatio != 0f) TryCitizenWealth(k, stats, o.citizenWealthRatio);
            if (o.worldWar) TryWorldWar(k);

            // 5.5 连锁：按选项把后续事件排入队列（跨年生成，绕过概率/冷却）
            if (!string.IsNullOrEmpty(d.chainNext) && FindDef(d.chainNext) != null
                && (d.chainAfterOption < 0 || d.chainAfterOption == optIndex)
                && k.data != null)
            {
                _chains.Add(new ChainSpawn
                {
                    DefId = d.chainNext,
                    KingdomId = k.data.id,
                    DueYear = year + System.Math.Max(1, d.chainDelay)
                });
            }

            // 6. 结果横幅（进史书级事件流，Detail=渲染后文本供事件窗展示）+ 玩家屏上通知。
            // Detail 先做名字代入：{king}/{kingdom} 在记录时即替换（AI 国无弹窗，渲染只能读 Detail）。
            string resKey = "ev_" + d.id + "_res" + (optIndex + 1);
            string resText = Contextualize(Services.LocalizationService.Get(resKey), k.data.id);
            EventStreamService.Record(TypeDecision, GameHelpers.SafeKingdomName(k), optIndex + 1, resText);
            if (isPlayer)
            {
                if (goldMoved != 0)
                    GameHelpers.Notify(string.Format(resText, NationEngine.FormatGold(System.Math.Abs(goldMoved))));
                else GameHelpers.Notify(resText);
            }
        }

        // ===== v1.8 效果执行辅助（全部静默失败，遵循 fail-closed 惯例）=====

        /// <summary>迁都：选王国第二大城市（按 GDP 优先级；无第二城不动）。</summary>
        private static void TryMoveCapital(Kingdom k)
        {
            try
            {
                if (k == null || k.data == null) return;
                _capitalPool.Clear();
                NationEngine.SnapshotCities(k, _capitalPool);
                if (_capitalPool.Count < 2) return;
                City best = null;
                long bestPop = -1;
                for (int i = 0; i < _capitalPool.Count; i++)
                {
                    var c = _capitalPool[i];
                    if (c == null) continue;
                    if (k.capital != null && c == k.capital) continue; // 跳过当前首都
                    long pop = 0;
                    try { pop = c.getPopulationPeople(); } catch (System.Exception) { }
                    if (pop > bestPop) { bestPop = pop; best = c; }
                }
                if (best != null) k.setCapital(best);
            }
            catch (System.Exception) { }
        }

        /// <summary>升级 N 座可升级建筑：逐城遍历，Building.upgradeBuilding() 反射一次；静默跳过不可升级/施工中。</summary>
        private static void TryUpgradeBuildings(Kingdom k, int n)
        {
            try
            {
                if (k == null || k.data == null || n <= 0) return;
                if (_upgradeMethod == null && !_upgradeProbed)
                {
                    _upgradeProbed = true;
                    try
                    {
                        _upgradeMethod = typeof(Building).GetMethod("upgradeBuilding",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance);
                    }
                    catch (System.Exception) { }
                }
                if (_upgradeMethod == null) return;

                _capitalPool.Clear();
                NationEngine.SnapshotCities(k, _capitalPool);
                int done = 0;
                for (int ci = 0; ci < _capitalPool.Count && done < n; ci++)
                {
                    var c = _capitalPool[ci];
                    if (c == null || c.buildings == null) continue;
                    for (int bi = 0; bi < c.buildings.Count && done < n; bi++)
                    {
                        var b = c.buildings[bi];
                        if (b == null) continue;
                        try
                        {
                            object r = _upgradeMethod.Invoke(b, null);
                            if (r is bool ok && ok) done++;
                        }
                        catch (System.Exception) { }
                    }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>个体财富：ratio>0 给 top 富豪发钱（分块 addMoney）；&lt;0 按人均比例随机扣钱（clamp 0 保底）。</summary>
        private static void TryCitizenWealth(Kingdom k, EconomyMod.Models.KingdomStats stats, float ratio)
        {
            try
            {
                if (k == null || k.units == null || ratio == 0f) return;
                long perCapita = (long)((stats?.AvgWealth ?? 0f) * System.Math.Abs(ratio));
                if (perCapita <= 0) return;
                var pool = _citizenPool;
                pool.Clear();
                foreach (var a in k.units)
                {
                    if (a == null || !a.isAlive()) continue;
                    if (!GameHelpers.IsCivilizedActor(a)) continue;
                    pool.Add(a);
                }
                if (ratio > 0)
                {
                    // 发财：按财富排序取 top 25%（至少 1 人）
                    pool.Sort((x, y) =>
                    {
                        try { return GameHelpers.TryGetWealth(x, out float wx) && GameHelpers.TryGetWealth(y, out float wy)
                            ? wy.CompareTo(wx) : 0; }
                        catch (System.Exception) { return 0; }
                    });
                    int count = System.Math.Max(1, pool.Count / 4);
                    for (int i = 0; i < count && i < pool.Count; i++)
                        GameHelpers.AddPositiveMoney(pool[i], perCapita);
                }
                else
                {
                    // 破产：随机扣池中 30%（至少 1 人），逐人扣到 0 为止
                    int count = System.Math.Max(1, pool.Count * 3 / 10);
                    for (int i = 0; i < count && pool.Count > 0; i++)
                    {
                        var a = pool[UnityEngine.Random.Range(0, pool.Count)];
                        pool.RemoveAt(pool.IndexOf(a));
                        try { if (GameHelpers.TryGetWealth(a, out float w) && w > 0) a.addMoney(-(int)System.Math.Min(int.MaxValue, System.Math.Min(perCapita, w))); }
                        catch (System.Exception) { }
                    }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>世界大战：public eventSpite（全世界攻事件国；单世界仅一场总战争，已存在则无效果）。</summary>
        private static void TryWorldWar(Kingdom k)
        {
            try
            {
                if (k == null || k.data == null) return;
                var d = World.world != null ? World.world.diplomacy : null;
                if (d == null) return;
                d.eventSpite(k);
            }
            catch (System.Exception) { }
        }

        // v1.8 效果辅助缓存（主线程年度路径；仅分配一次）
        private static readonly List<City> _capitalPool = new List<City>(8);
        private static readonly List<Actor> _citizenPool = new List<Actor>(64);
        private static System.Reflection.MethodInfo _upgradeMethod;
        private static bool _upgradeProbed;

    }
}
