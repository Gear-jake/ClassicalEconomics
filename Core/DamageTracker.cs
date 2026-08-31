using System.Collections.Generic;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 伤害追踪（无 Harmony 环境下的轮询方案）：
    /// 由 InheritanceEngine 的 3 秒存活扫描顺带驱动（避免两次独立全量遍历），
    /// 对每个存活 Actor 对比 health，发现掉血即反射读取其 main_attacker
    /// （游戏在受击时更新该字段），记录 victimId -> (attackerId -> 累计伤害)。
    /// 用于"被击杀的智慧生物按伤害比例分配金币"。
    /// 若反射不可用或读取不到攻击者，则不影响游戏（死亡时回退原继承）。
    /// </summary>
    public static class DamageTracker
    {
        private const int MaxAttackersPerVictim = 16;
        private const int MaxInactiveScans = 10;

        private static Dictionary<long, Dictionary<long, float>> _damage =
            new Dictionary<long, Dictionary<long, float>>();
        private static Dictionary<long, int> _prevHealth = new Dictionary<long, int>();
        private static Dictionary<long, int> _inactiveScans = new Dictionary<long, int>();

        private static System.Reflection.PropertyInfo _mainAttackerProp;

        /// <summary>逐单位掉血检测（由 InheritanceEngine 存活扫描循环调用，不再独立全量遍历）。</summary>
        public static void CheckActor(Actor actor)
        {
            if (actor == null) return;
            long id;
            try { id = actor.id; }
            catch (System.Exception) { return; } // 半销毁对象读取 id 可能抛异常，跳过

            int cur = GetHealth(actor);
            if (_prevHealth.TryGetValue(id, out int prev))
            {
                if (cur < prev)
                {
                    Actor attacker = GetMainAttacker(actor);
                    if (attacker != null)
                    {
                        long aid;
                        try { aid = attacker.id; } catch (System.Exception) { aid = 0L; }
                        if (aid != 0L && aid != id && GameHelpers.IsCivilizedActor(attacker))
                        {
                            AddDamage(id, aid, prev - cur);
                            _inactiveScans[id] = 0;
                        }
                    }
                }
                else if (_damage.ContainsKey(id))
                {
                    int scans;
                    _inactiveScans.TryGetValue(id, out scans);
                    scans++;
                    if (scans >= MaxInactiveScans)
                    {
                        _damage.Remove(id);
                        _inactiveScans.Remove(id);
                    }
                    else
                    {
                        _inactiveScans[id] = scans;
                    }
                }
            }
            _prevHealth[id] = cur;
        }

        private static int GetHealth(Actor actor)
        {
            try { return actor.getHealth(); }
            catch (System.Exception) { return 0; }
        }

        /// <summary>反射读取 actor.data.main_attacker（Actor.data 编译期不可见）。</summary>
        private static Actor GetMainAttacker(Actor actor)
        {
            try
            {
                var d = GameHelpers.GetActorData(actor);
                if (d != null && _mainAttackerProp == null)
                {
                    _mainAttackerProp = d.GetType().GetProperty("main_attacker");
                }
                if (_mainAttackerProp != null)
                {
                    var v = _mainAttackerProp.GetValue(d);
                    if (v is Actor a && a != actor) return a;
                }
            }
            catch (System.Exception) { }
            return null;
        }

        private static void AddDamage(long victimId, long attackerId, float dmg)
        {
            if (!_damage.TryGetValue(victimId, out var map))
            {
                map = new Dictionary<long, float>();
                _damage[victimId] = map;
            }

            float currentDamage;
            if (map.TryGetValue(attackerId, out currentDamage))
            {
                map[attackerId] = currentDamage + dmg;
                return;
            }

            if (map.Count >= MaxAttackersPerVictim)
            {
                long weakestAttackerId = 0L;
                float weakestDamage = float.MaxValue;
                foreach (var entry in map)
                {
                    if (entry.Value < weakestDamage
                        || (entry.Value == weakestDamage && entry.Key > weakestAttackerId))
                    {
                        weakestAttackerId = entry.Key;
                        weakestDamage = entry.Value;
                    }
                }
                if (dmg <= weakestDamage) return;
                map.Remove(weakestAttackerId);
            }

            map[attackerId] = dmg;
        }

        /// <summary>获取受害者的伤害来源记录（attackerId -> 伤害值）。</summary>
        public static bool TryGetDamage(long victimId, out Dictionary<long, float> map)
        {
            if (_damage.TryGetValue(victimId, out map) && map.Count > 0) return true;
            map = null;
            return false;
        }

        /// <summary>清理已分配受害者的记录。</summary>
        public static void Remove(long victimId)
        {
            _damage.Remove(victimId);
            _prevHealth.Remove(victimId);
            _inactiveScans.Remove(victimId);
        }

        /// <summary>世界重置（新地图/新游戏）或离开世界时清空。</summary>
        public static void Reset()
        {
            _damage.Clear();
            _prevHealth.Clear();
            _inactiveScans.Clear();
            _mainAttackerProp = null;
        }

        /// <summary>供 MemoryCleanupEngine 重建缩容时读取当前引用（仅空闲期调用，绝不与存活扫描并发）。</summary>
        internal static Dictionary<long, Dictionary<long, float>> DamageForTrim => _damage;
        internal static Dictionary<long, int> PrevHealthForTrim => _prevHealth;
        internal static Dictionary<long, int> InactiveScansForTrim => _inactiveScans;

        /// <summary>将重建后的紧凑字典换回（仅 MemoryCleanupEngine 空闲期调用）。</summary>
        internal static void ReplaceDamageForTrim(Dictionary<long, Dictionary<long, float>> compact) { _damage = compact; }
        internal static void ReplacePrevHealthForTrim(Dictionary<long, int> compact) { _prevHealth = compact; }
        internal static void ReplaceInactiveScansForTrim(Dictionary<long, int> compact) { _inactiveScans = compact; }
    }
}
