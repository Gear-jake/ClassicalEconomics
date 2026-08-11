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
        private class AliveRecord
        {
            public Actor Actor;      // 引用（兜底）
            public string Name;      // 存活时缓存的名字（死亡后对象清理则不可读）
            public float Money;      // 最近一次存活时读取
            public float Loot;
            public City City;        // 所属城市
            public List<long> ParentIds = new List<long>();  // 存活时缓存的亲属 id
            public long SpouseId;
            public List<long> ChildIds = new List<long>();
            public int CacheAge;     // 亲属缓存年龄（秒）
        }

        private static readonly Dictionary<long, AliveRecord> _records = new Dictionary<long, AliveRecord>();
        private static float _timer;

        // ===== 性能优化：复用每秒扫描缓冲，避免每帧 GC 分配 =====
        private static readonly Dictionary<long, Actor> _aliveMap = new Dictionary<long, Actor>();
        private static readonly HashSet<long> _seen = new HashSet<long>();
        private static readonly List<long> _deadIds = new List<long>();
        private static readonly List<Actor> _heirsPool = new List<Actor>();
        private static readonly List<KeyValuePair<long, float>> _damageCivPool =
            new List<KeyValuePair<long, float>>();

        /// <summary>世界重置（新地图/新游戏）时清空记录。</summary>
        public static void Reset()
        {
            _records.Clear();
            DamageTracker.Reset();
        }

        /// <summary>每帧调用；内部按 3 秒节流执行死亡检测（降低大世界每秒全量扫描开销）。</summary>
        public static void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer < 3f) return;
            _timer = 0f;

            // 无世界（主菜单/加载中）时清空记录，避免误判为全员死亡
            if (World.world == null || World.world.units == null)
            {
                _records.Clear();
                DamageTracker.Reset();
                return;
            }

            var aliveList = World.world.units.units_only_alive;
            if (aliveList == null)
            {
                _records.Clear();
                DamageTracker.Reset();
                return;
            }

            var aliveMap = _aliveMap;
            var seen = _seen;
            aliveMap.Clear();
            seen.Clear();
            foreach (var actor in aliveList)
            {
                if (actor == null) continue;
                long id;
                try { id = actor.id; }
                catch (System.Exception) { continue; } // 半销毁对象读取 id 可能抛异常，跳过
                // 顺带完成掉血检测（伤害追踪与遗产继承共用本次 3 秒全量遍历）
                DamageTracker.CheckActor(actor);
                seen.Add(id);
                aliveMap[id] = actor;

                if (_records.TryGetValue(id, out var rec))
                {
                    // 刷新最近存活状态（死亡瞬间可能被原生逻辑清空，用存活时快照更可靠）
                    try { rec.Name = actor.name; } catch (System.Exception) { }
                    try { rec.Money = actor.money; } catch (System.Exception) { }
                    try { rec.Loot = actor.loot; } catch (System.Exception) { }
                    try { rec.City = actor.city; } catch (System.Exception) { }
                    // 每 30 秒刷新一次亲属缓存（关系会随结婚/生育变化）
                    rec.CacheAge++;
                    if (rec.CacheAge >= 30)
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

            // 上次存活、本次消失 => 死亡，执行遗产分配
            var deadIds = _deadIds;
            deadIds.Clear();
            foreach (var kv in _records)
                if (!seen.Contains(kv.Key)) deadIds.Add(kv.Key);

            foreach (var id in deadIds)
            {
                var rec = _records[id];
                _records.Remove(id);
                HandleDeath(rec, aliveMap);
                // 死亡即清理伤害追踪记录（遗产分配在 HandleDeath 内已读取完毕），
                // 否则 _prevHealth/_damage 中自然死亡者的条目无限残留，导致游戏越跑越卡
                DamageTracker.Remove(id);
            }
        }

        /// <summary>
        /// 存活时缓存亲属 id（死亡后 Actor 对象可能被原生逻辑清理，无法再读取）。
        /// </summary>
        private static void CacheRelatives(Actor actor, AliveRecord rec)
        {
            // 复用记录内的列表（Clear+Add 代替反复 new List）
            try
            {
                rec.ParentIds.Clear();
                foreach (var p in actor.getParents())
                    if (p != null) rec.ParentIds.Add(p.id);
            }
            catch (System.Exception) { }

            try
            {
                rec.SpouseId = actor.lover != null ? actor.lover.id : 0L;
            }
            catch (System.Exception) { }

            try
            {
                rec.ChildIds.Clear();
                foreach (var k in actor.getChildren(true))
                    if (k != null) rec.ChildIds.Add(k.id);
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
                int total = money + loot;
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
                        Debug.Log($"[ClassicalEconomics] 遗产分配 {SafeName(rec)} 遗产={total} 继承比例40% 净得={net} " +
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
                if (!aliveMap.TryGetValue(kv.Key, out var a)) continue;
                if (a.asset == null || !a.asset.civ) continue;
                civ.Add(kv);
            }
            if (civ.Count == 0) return false;

            float totalDmg = 0f;
            foreach (var kv in civ) totalDmg += kv.Value;
            if (totalDmg <= 0f) return false;

            // 按伤害占比分配（取整后把差额补给伤害最高者）
            try
            {
                int assigned = 0;
                foreach (var kv in civ)
                {
                    int amount = Mathf.RoundToInt(total * (kv.Value / totalDmg));
                    if (amount < 1 && total >= civ.Count) amount = 1; // 保底 1（仅当金币足够覆盖全员，避免因保底超额）
                    aliveMap[kv.Key].addMoney(amount);
                    assigned += amount;
                }
                if (assigned != total)
                {
                    long topId = civ[0].Key;
                    float maxD = civ[0].Value;
                    foreach (var kv in civ)
                    {
                        if (kv.Value > maxD) { maxD = kv.Value; topId = kv.Key; }
                    }
                    int delta = total - assigned;
                    if (delta > 0)
                    {
                        // 正残差补给伤害最高者（Σ 严格 ≤ total）
                        aliveMap[topId].addMoney(delta);
                        assigned = total;
                    }
                    // delta < 0（保底取整导致的微小超额）不再扣回：
                    // 原代码 addMoney(负值) 会掠夺伤害最高者（M4 负补偿修复）
                }
            }
            catch (System.Exception) { return false; }

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
            foreach (var pid in rec.ParentIds)
            {
                if (aliveMap.TryGetValue(pid, out var p) && !heirs.Contains(p))
                    heirs.Add(p);
            }
            if (heirs.Count > 0) return heirs;

            if (rec.SpouseId != 0L && aliveMap.TryGetValue(rec.SpouseId, out var s) && !heirs.Contains(s))
                heirs.Add(s);
            if (heirs.Count > 0) return heirs;

            foreach (var cid in rec.ChildIds)
            {
                if (aliveMap.TryGetValue(cid, out var k) && !heirs.Contains(k))
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
    }
}
