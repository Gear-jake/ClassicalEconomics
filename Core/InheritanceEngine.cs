using System.Collections.Generic;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 遗产继承机制：智慧生物死亡时，其金币（coins+loot）按
    /// 父母 → 配偶 → 子嗣 的顺序转移；城市（村长）收取遗产税；
    /// 若没有任何继承者，全部归村长所有。
    /// 通过轮询存活列表检测死亡（死亡后从 units_only_alive 消失）。
    /// </summary>
    public static class InheritanceEngine
    {
        // internal：MemoryCleanupEngine 的 RecordsForTrim 访问器需要在程序集内暴露该类型
        internal class AliveRecord
        {
            public Actor Actor;      // 引用（兜底）
            public string Name;      // 存活时缓存的名字（死亡后对象清理则不可读）
            public float Money;      // 最近一次存活时读取
            public float Loot;
            public City City;        // 所属城市
            public long ParentId1;
            public long ParentId2;
            public long SpouseId;
            public long[] ChildIds;  // 定长缓冲（压缩：无 List 对象头与容量冗余；ChildCount 为实际数量）
            public int ChildCount;
            public int CacheAge;     // 亲属缓存年龄（3 秒扫描次数）
        }

        private static Dictionary<long, AliveRecord> _records = new Dictionary<long, AliveRecord>();
        private static float _timer;
        private static bool _scanActive;  // 扫描窗口进行中（跨帧分摊 3 秒全量扫描）
        private static int _scanCursor;   // 当前窗口已扫描到的存活列表索引

        // ===== 性能优化：复用每秒扫描缓冲，避免每帧 GC 分配 =====
        private static Dictionary<long, Actor> _aliveMap = new Dictionary<long, Actor>();
        private static readonly List<long> _deadIds = new List<long>();
        private static readonly List<long> _staleIds = new List<long>();
        private static readonly List<Actor> _heirsPool = new List<Actor>();
        private static readonly List<KeyValuePair<long, float>> _damageCivPool =
            new List<KeyValuePair<long, float>>();
        private struct DamageShare
        {
            public long AttackerId;
            public int Amount;
            public decimal Remainder;
        }
        private static readonly List<DamageShare> _damageSharePool = new List<DamageShare>(16);

        /// <summary>世界重置（新地图/新游戏）时清空记录。</summary>
        public static void Reset()
        {
            ClearWorldReferences();
        }

        /// <summary>离开世界时清除所有世界对象引用及扫描状态。</summary>
        public static void ClearWorldReferences()
        {
            _records.Clear();
            _aliveMap.Clear();
            _deadIds.Clear();
            _staleIds.Clear();
            _heirsPool.Clear();
            _damageCivPool.Clear();
            _damageSharePool.Clear();
            _timer = 0f;
            _scanActive = false;
            _scanCursor = 0;
            DamageTracker.Reset();
        }

        /// <summary>每帧调用；按 3 秒节流开启扫描窗口，窗口内跨帧分摊全量扫描（每帧最多 2000 个存活单位）。</summary>
        public static void Tick(float deltaTime)
        {
            _timer += deltaTime;

            // 无世界（主菜单/加载中）时清空记录，避免误判为全员死亡
            if (World.world == null || World.world.units == null)
            {
                ClearWorldReferences();
                return;
            }

            var aliveList = World.world.units.units_only_alive;
            if (aliveList == null)
            {
                ClearWorldReferences();
                return;
            }

            // 3 秒节流：开启一次扫描窗口（旧版在单帧完成全量扫描，现跨帧分摊）
            if (!_scanActive)
            {
                if (_timer < 3f) return;
                _timer = 0f;
                _aliveMap.Clear();
                _scanCursor = 0;
                _scanActive = true;
            }

            // 普通帧最多扫描 cap 个条目；窗口累计满 3 秒时本帧扫完剩余部分，
            // 保留旧版"3 秒内完成一次全量扫描"的截止语义
            int cap = Mathf.Clamp(UnrestConfig.Instance.InheritanceScanPerFrame, 1, 100000);
            bool deadline = _timer >= 3f;
            int scanned = 0;
            while (_scanCursor < aliveList.Count && (scanned < cap || deadline))
            {
                ScanActor(aliveList[_scanCursor]);
                _scanCursor++;
                scanned++;
            }

            if (_scanCursor >= aliveList.Count)
            {
                CompleteWindow(aliveList);
            }
        }

        /// <summary>扫描单个存活单位：civ 过滤、掉血检测、索引登记与记录刷新/创建（与旧版逐单位体语义一致）。</summary>
        private static void ScanActor(Actor actor)
        {
            if (actor == null) return;
            // 动物无遗产、不参与灾害经济冲击，跳过（DataCollector 同款 civ 过滤），
            // 避免每 3 秒对全部动物（往往占总单位数相当比例）做无意义扫描。
            if (!GameHelpers.IsCivilizedActor(actor)) return;
            long id;
            try { id = actor.id; }
            catch (System.Exception) { return; } // 半销毁对象读取 id 可能抛异常，跳过
            // 顺带完成掉血检测（伤害追踪与遗产继承共用本次 3 秒全量遍历）
            DamageTracker.CheckActor(actor);
            _aliveMap[id] = actor;

            if (_records.TryGetValue(id, out var rec))
            {
                // 刷新最近存活状态（死亡瞬间可能被原生逻辑清空，用存活时快照更可靠）
                try { rec.Name = actor.name; } catch (System.Exception) { }
                try { rec.Money = actor.money; } catch (System.Exception) { }
                try { rec.Loot = actor.loot; } catch (System.Exception) { }
                try { rec.City = actor.city; } catch (System.Exception) { }
                // 每 30 秒刷新一次亲属缓存（关系会随结婚/生育变化）
                rec.CacheAge++;
                if (rec.CacheAge >= 10)
                {
                    rec.CacheAge = 0;
                    CacheRelatives(actor, rec);
                }
            }
            else
            {
                var newRec = new AliveRecord { Actor = actor };
                try { newRec.Name = actor.name; } catch (System.Exception) { }
                try { newRec.Money = actor.money; } catch (System.Exception) { }
                try { newRec.Loot = actor.loot; } catch (System.Exception) { }
                try { newRec.City = actor.city; } catch (System.Exception) { }
                CacheRelatives(actor, newRec);
                _records[id] = newRec;
            }
        }

        /// <summary>
        /// 扫描窗口收尾：窗口跨帧期间存活列表可能增删（死亡移除/新出生），
        /// 先补扫所有仍在存活列表但未登记的单位，再执行不变的死亡判定与遗产分配。
        /// </summary>
        private static void CompleteWindow(List<Actor> aliveList)
        {
            // 补扫：窗口期间被游标跳过或新出现的存活单位，避免被误判为死亡
            foreach (var actor in aliveList)
            {
                if (actor == null) continue;
                long id;
                try { id = actor.id; }
                catch (System.Exception) { continue; } // 半销毁对象读取 id 可能抛异常，跳过
                if (_aliveMap.ContainsKey(id)) continue;
                ScanActor(actor);
            }

            // 清理陈旧条目：窗口期间已死亡或不再满足开智判定的单位从 aliveMap 移除，
            // 使其立即进入下方死亡判定（否则会滞留为继承者或推迟死亡处理）
            var staleIds = _staleIds;
            staleIds.Clear();
            foreach (var kv in _aliveMap)
            {
                var a = kv.Value;
                if (a == null || !GameHelpers.IsCivilizedActor(a)) staleIds.Add(kv.Key);
            }
            foreach (var id in staleIds) _aliveMap.Remove(id);

            // 上次存活、本次消失 => 死亡，执行遗产分配
            var aliveMap = _aliveMap;
            var deadIds = _deadIds;
            deadIds.Clear();
            foreach (var kv in _records)
                if (!aliveMap.ContainsKey(kv.Key)) deadIds.Add(kv.Key);

            foreach (var id in deadIds)
            {
                var rec = _records[id];
                _records.Remove(id);
                HandleDeath(rec, aliveMap);
                // 死亡即清理伤害追踪记录（遗产分配在 HandleDeath 内已读取完毕），
                // 否则 _prevHealth/_damage 中自然死亡者的条目无限残留，导致游戏越跑越卡
                DamageTracker.Remove(id);
            }

            _scanActive = false;
            _scanCursor = 0;
        }

        /// <summary>
        /// 存活时缓存亲属 id（死亡后 Actor 对象可能被原生逻辑清理，无法再读取）。
        /// </summary>
        private static void CacheRelatives(Actor actor, AliveRecord rec)
        {
            // 复用记录内的列表（Clear+Add 代替反复 new List）
            try
            {
                rec.ParentId1 = 0L;
                rec.ParentId2 = 0L;
                foreach (var p in actor.getParents())
                {
                    if (p == null) continue;
                    if (rec.ParentId1 == 0L) rec.ParentId1 = p.id;
                    else if (rec.ParentId2 == 0L) { rec.ParentId2 = p.id; break; }
                }
            }
            catch (System.Exception) { }

            try
            {
                rec.SpouseId = actor.lover != null ? actor.lover.id : 0L;
            }
            catch (System.Exception) { }

            try
            {
                rec.ChildCount = 0;
                foreach (var k in actor.getChildren(true))
                {
                    if (k == null) continue;
                    if (rec.ChildIds == null) rec.ChildIds = new long[2];
                    if (rec.ChildCount == rec.ChildIds.Length)
                    {
                        var grown = new long[rec.ChildIds.Length * 2];
                        System.Array.Copy(rec.ChildIds, grown, rec.ChildIds.Length);
                        rec.ChildIds = grown;
                    }
                    rec.ChildIds[rec.ChildCount++] = k.id;
                }
            }
            catch (System.Exception) { }
        }

        private static void HandleDeath(AliveRecord rec, Dictionary<long, Actor> aliveMap)
        {
            if (rec == null || rec.Actor == null) return;

            try
            {
                int money = Mathf.Max(0, Mathf.RoundToInt(rec.Money));
                int loot = Mathf.Max(0, Mathf.RoundToInt(rec.Loot));
                int total = (int)System.Math.Min((long)money + loot, int.MaxValue);
                if (total <= 0) return; // 无遗产

                // 被击杀：金币按伤害比例分给对其造成过伤害的存活智慧生物（优先于继承链）
                if (TryDistributeByDamage(rec, aliveMap, total))
                {
                    return;
                }

                City city = rec.City;
                Actor leader = city != null ? city.leader : null;

                // 继承顺序：父母 → 配偶 → 子嗣（按缓存 id 从当前存活列表解析）
                var heirs = ResolveHeirs(rec, aliveMap);

                if (heirs.Count > 0)
                {
                    // 继承制：遗产只传递 40% 给继承者，其余 60% 消散
                    // （削减代际财富累积，防止富人财富世袭滚雪球，间接降低基尼）
                    int net = Mathf.RoundToInt(total * 0.4f);
                    int each = heirs.Count > 0 ? net / heirs.Count : 0;
                    int remainder = net - each * heirs.Count;
                    for (int i = 0; i < heirs.Count; i++)
                    {
                        if (heirs[i] == null) continue;
                        heirs[i].addMoney(each + (i == 0 ? remainder : 0));
                    }

                    if (UnrestConfig.Instance.LogToWorldLog)
                    {
                        Debug.Log($"[ClassicalEconomics] 遗产分配 {SafeName(rec)} 遗产={total} 继承比例80% 净得={net} " +
                                  $"继承者={heirs.Count}人 城市={(city != null ? GameHelpers.SafeCityName(city) : "无")}");
                    }
                }
                else if (leader != null)
                {
                    leader.addMoney(total);
                    if (UnrestConfig.Instance.LogToWorldLog)
                    {
                        Debug.Log($"[ClassicalEconomics] 遗产归村长 {SafeName(rec)} 遗产={total} 村长={GameHelpers.SafeName(leader)}");
                    }
                }
                else
                {
                    if (UnrestConfig.Instance.LogToWorldLog)
                    {
                        Debug.Log($"[ClassicalEconomics] 遗产无人继承 {SafeName(rec)} 遗产={total}（无亲属无城市，消散）");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ClassicalEconomics] 遗产处理异常: {e.Message}");
            }
        }

        /// <summary>
        /// 被击杀分配：若死者有伤害记录且存在存活的智慧攻击者，
        /// 将其全部金币按各攻击者造成的伤害比例分配；否则返回 false 走原继承链。
        /// </summary>
        private static bool TryDistributeByDamage(AliveRecord rec, Dictionary<long, Actor> aliveMap, int total)
        {
            if (rec.Actor == null || total <= 0) return false;
            long victimId;
            try { victimId = rec.Actor.id; }
            catch (System.Exception) { return false; } // 对象已销毁则回退原继承
            if (!DamageTracker.TryGetDamage(victimId, out var dmgMap)) return false;

            // 过滤：只分给仍存活的开智攻击者（复用缓冲避免每次分配）
            var civ = _damageCivPool;
            civ.Clear();
            foreach (var kv in dmgMap)
            {
                if (kv.Value <= 0f || float.IsNaN(kv.Value) || float.IsInfinity(kv.Value)) continue;
                if (!aliveMap.TryGetValue(kv.Key, out var a) || a == null) continue;
                if (!GameHelpers.IsCivilizedActor(a)) continue;
                civ.Add(kv);
            }
            if (civ.Count == 0) return false;

            // 先按伤害确定最高贡献者；同伤害时 id 小者优先，避免字典遍历顺序影响结果。
            civ.Sort((left, right) =>
            {
                int damageOrder = right.Value.CompareTo(left.Value);
                return damageOrder != 0 ? damageOrder : left.Key.CompareTo(right.Key);
            });

            // 以最大伤害归一化后转 decimal，既保留比例又避免累计伤害及 total*damage 溢出。
            double maxDamage = civ[0].Value;
            var shares = _damageSharePool;
            shares.Clear();
            decimal damageSum = 0m;
            foreach (var kv in civ)
                damageSum += (decimal)(kv.Value / maxDamage);

            long assigned = 0L;
            foreach (var kv in civ)
            {
                decimal quota = total * (decimal)(kv.Value / maxDamage) / damageSum;
                int amount = (int)decimal.Floor(quota);
                shares.Add(new DamageShare
                {
                    AttackerId = kv.Key,
                    Amount = amount,
                    Remainder = quota - amount
                });
                assigned += amount;
            }

            shares.Sort((left, right) =>
            {
                int remainderOrder = right.Remainder.CompareTo(left.Remainder);
                return remainderOrder != 0 ? remainderOrder : left.AttackerId.CompareTo(right.AttackerId);
            });
            int remaining = (int)((long)total - assigned);
            for (int i = 0; i < remaining; i++)
            {
                DamageShare share = shares[i];
                share.Amount++;
                shares[i] = share;
            }

            // 从此处起已确认存在有效伤害来源；单个付款失败时该份额消散，不得回退继承链。
            foreach (var share in shares)
            {
                if (share.Amount <= 0) continue;
                try { aliveMap[share.AttackerId].addMoney(share.Amount); }
                catch (System.Exception) { }
            }

            if (UnrestConfig.Instance.LogToWorldLog)
            {
                Debug.Log($"[ClassicalEconomics] 战利品分配 {SafeName(rec)} 金币={total} 按伤害分给 {civ.Count} 名智慧攻击者");
            }
            return true;
        }

        /// <summary>
        /// 按缓存亲属 id 从当前存活列表解析继承者（父母 → 配偶 → 子嗣）。
        /// </summary>
        private static List<Actor> ResolveHeirs(AliveRecord rec, Dictionary<long, Actor> aliveMap)
        {
            // 复用静态缓冲（HandleDeath 单线程顺序执行，无嵌套调用）
            var heirs = _heirsPool;
            heirs.Clear();
            Actor p;
            if (rec.ParentId1 != 0L && aliveMap.TryGetValue(rec.ParentId1, out p)) heirs.Add(p);
            if (rec.ParentId2 != 0L && aliveMap.TryGetValue(rec.ParentId2, out p) && !heirs.Contains(p)) heirs.Add(p);
            if (heirs.Count > 0) return heirs;

            if (rec.SpouseId != 0L && aliveMap.TryGetValue(rec.SpouseId, out var s) && !heirs.Contains(s))
                heirs.Add(s);
            if (heirs.Count > 0) return heirs;

            for (int i = 0; i < rec.ChildCount && rec.ChildIds != null; i++)
            {
                if (aliveMap.TryGetValue(rec.ChildIds[i], out var k) && !heirs.Contains(k))
                    heirs.Add(k);
            }
            return heirs;
        }

        private static string SafeName(AliveRecord rec)
        {
            if (rec == null) return "?";
            if (!string.IsNullOrEmpty(rec.Name)) return rec.Name;
            return GameHelpers.SafeName(rec.Actor);
        }

        // ===== 静态缓冲缩容（MemoryCleanupEngine 空闲期调用；只缩容，不清内容，语义不变）=====

        private static int TrimList<T>(List<T> list)
        {
            try
            {
                if (list.Capacity > 4096 && list.Capacity > list.Count * 4)
                {
                    list.TrimExcess();
                    return 1;
                }
            }
            catch (System.Exception) { }
            return 0;
        }

        /// <summary>对全部静态 List 缓冲/缓存执行 TrimExcess，返回实际收缩的列表数；
        /// _records / _aliveMap 由 MemoryCleanupEngine 通过 ForTrim 访问器重建缩容。</summary>
        public static int TrimMemory()
        {
            int shrunk = 0;
            shrunk += TrimList(_deadIds);
            shrunk += TrimList(_staleIds);
            shrunk += TrimList(_heirsPool);
            shrunk += TrimList(_damageCivPool);
            shrunk += TrimList(_damageSharePool);
            return shrunk;
        }

        /// <summary>供 MemoryCleanupEngine 重建缩容时读取当前引用（仅空闲期调用，绝不与扫描窗口并发）。</summary>
        internal static Dictionary<long, AliveRecord> RecordsForTrim => _records;
        internal static Dictionary<long, Actor> AliveMapForTrim => _aliveMap;

        /// <summary>将重建后的紧凑字典换回（仅 MemoryCleanupEngine 空闲期调用）。</summary>
        internal static void ReplaceRecordsForTrim(Dictionary<long, AliveRecord> compact) { _records = compact; }
        internal static void ReplaceAliveMapForTrim(Dictionary<long, Actor> compact) { _aliveMap = compact; }
    }
}
