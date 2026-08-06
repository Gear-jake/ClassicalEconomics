using System.Collections.Generic;
using System.Reflection;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 社会危机引擎（Phase 5）。复用原生机制，不新增自定义系统：
    /// 1. 战争掠夺：检测原生战争（DiplomacyHelpers.wars），战争结束胜方掠夺败方王国 WarPlunderRatio 比例的硬币；
    /// 2. 革命：叛乱持续满 N 年 → 王国被推翻（击杀人口 + 移除叛乱特质 + 硬币重新分配）。
    /// 注：饥荒已移除——游戏自带饮食系统处理人口饿死，模组不再自行饿死人口。
    /// </summary>
    public static class SocialCrisisEngine
    {
        // ===== 战争掠夺：反射缓存 =====
        private static PropertyInfo _warsProp;
        private static PropertyInfo _attackerProp;
        private static PropertyInfo _defenderProp;
        private static FieldInfo _winnerField;
        private static readonly Dictionary<string, int> _warWinnerCache = new Dictionary<string, int>();

        // WarWinner 枚举值（与游戏一致）
        private const int WinnerNobody = 0;    // 进行中/无结果
        private const int WinnerAttackers = 1; // 攻方胜利
        private const int WinnerDefenders = 2; // 守方胜利

        // ===== 性能优化：复用每年扫描缓冲，避免 GC 分配 =====
        private static readonly List<Actor> _actorPool = new List<Actor>();
        private static readonly List<Actor> _actorPool2 = new List<Actor>();
        private static readonly HashSet<string> _currentWarKeys = new HashSet<string>();
        private static readonly List<string> _staleWarKeys = new List<string>();
        private static readonly List<KingdomStats> _poorestPool = new List<KingdomStats>();

        /// <summary>每年评估一次（在 UnrestEngine.Evaluate 之后调用）。</summary>
        public static void Evaluate()
        {
            var cfg = UnrestConfig.Instance;
            if (!cfg.CycleEnabled) return; // 跟随经济周期开关
            if (World.world == null || World.world.units == null) return;

            WarPlunderCheck(cfg);
            RevolutionCheck(cfg);
        }

        /// <summary>世界重置（新地图/新游戏）时清空战争跟踪。</summary>
        public static void Reset()
        {
            _warWinnerCache.Clear();
        }

        // ===== 1. 战争掠夺 =====

        private static void WarPlunderCheck(UnrestConfig cfg)
        {
            var wars = GetWars();
            if (wars == null) return;
            EnsureWarReflection();

            var currentKeys = _currentWarKeys;
            currentKeys.Clear();
            foreach (var war in wars)
            {
                if (war == null) continue;
                long attackerId = 0L, defenderId = 0L;
                int winner = WinnerNobody;
                try
                {
                    attackerId = (long)_attackerProp.GetValue(war);
                    defenderId = (long)_defenderProp.GetValue(war);
                    winner = (int)_winnerField.GetValue(war);
                }
                catch (System.Exception) { continue; }
                if (attackerId == 0L || defenderId == 0L) continue;

                string key = attackerId + "_" + defenderId;
                currentKeys.Add(key);

                // 战争刚结束（winner 由"进行中"变为有结果）→ 执行掠夺
                if (winner != WinnerNobody)
                {
                    int prev = _warWinnerCache.TryGetValue(key, out int p) ? p : WinnerNobody;
                    if (prev == WinnerNobody)
                        Plunder(attackerId, defenderId, winner, cfg);
                }
                _warWinnerCache[key] = winner;
            }

            // 清理已结束并从战争列表移除的记录
            var stale = _staleWarKeys;
            stale.Clear();
            foreach (var k in _warWinnerCache.Keys)
                if (!currentKeys.Contains(k)) stale.Add(k);
            foreach (var k in stale) _warWinnerCache.Remove(k);
        }

        /// <summary>胜方掠夺败方王国 WarPlunderRatio 比例的硬币，平分给胜方成员。</summary>
        private static void Plunder(long attackerId, long defenderId, int winner, UnrestConfig cfg)
        {
            long winnerId = winner == WinnerAttackers ? attackerId : defenderId;
            long loserId = winner == WinnerAttackers ? defenderId : attackerId;
            var winnerKingdom = GameHelpers.FindKingdom(winnerId);
            var loserKingdom = GameHelpers.FindKingdom(loserId);
            if (loserKingdom == null || loserKingdom.units == null || loserKingdom.units.Count == 0) return;
            if (winnerKingdom == null || winnerKingdom.units == null || winnerKingdom.units.Count == 0) return;

            // 计算败方总硬币与掠夺额（用复用缓冲，避免 new List）
            var loserUnits = SnapshotUnits(loserKingdom, _actorPool);
            long loot = 0;
            foreach (var a in loserUnits)
            {
                if (a == null || !a.isAlive()) continue;
                try { loot += Mathf.Max(0, Mathf.RoundToInt(a.money)); } catch (System.Exception) { }
            }
            long steal = (long)(loot * cfg.WarPlunderRatio);
            if (steal <= 0) return;

            // 从败方逐人扣款
            GameHelpers.DeductCoins(loserUnits, steal);

            // 平分给胜方存活成员
            var winnerUnits = SnapshotUnits(winnerKingdom, _actorPool2);
            int receivers = 0;
            foreach (var a in winnerUnits)
                if (a != null && a.isAlive()) receivers++;
            if (receivers == 0) return;
            int share = (int)(steal / receivers);
            if (share > 0)
            {
                foreach (var a in winnerUnits)
                {
                    if (a == null || !a.isAlive()) continue;
                    try { a.addMoney(share); } catch (System.Exception) { }
                }
            }

            GameHelpers.Log($"[ClassicalEconomics] 战争掠夺 {WinnerName(winner)} 胜 掠夺={steal} 来自<{GameHelpers.SafeKingdomName(loserKingdom)}>");
            EventStreamService.Record(EventStreamService.TypePlunder, GameHelpers.SafeKingdomName(loserKingdom), steal);
        }

        // ===== 2. 革命与政权更迭 =====

        private static void RevolutionCheck(UnrestConfig cfg)
        {
            if (World.world.kingdoms == null) return;
            var kingdomList = GameHelpers.KingdomSnapshot();
            foreach (var kingdom in kingdomList)
            {
                if (kingdom == null || kingdom.data == null) continue;
                int state = UnrestEngine.GetState(kingdom.data.id, out int elapsed);
                if (state != 2) continue;                          // 非暴动中
                if (elapsed < cfg.RevolutionDelayYears) continue;  // 叛乱持续未满 N 年

                Revolution(kingdom, cfg);
            }
        }

        /// <summary>革命：击杀人口 + 移除叛乱特质 + 硬币重新分配（旧政权被推翻）。</summary>
        private static void Revolution(Kingdom kingdom, UnrestConfig cfg)
        {
            string name = GameHelpers.SafeKingdomName(kingdom);

            // 1. 击杀王国部分人口（革命暴力）
            int killed = KillRatioOfKingdom(kingdom, cfg.RevolutionKillRatio);

            // 2. 移除叛乱特质并清除震荡状态（复用镇压逻辑）
            try { UnrestEngine.Suppress(kingdom); } catch (System.Exception) { }

            // 3. 硬币重新分配：抽取王国 50% 硬币，分给最穷的 3 个王国（王国间）
            long extracted = RedistributeWealth(kingdom);

            // 3.5 王国内部劫富济贫：旧政权被推翻，从该国富人抽税分给穷人（直接降低该国基尼）
            long internalRedist = GameHelpers.RedistributeWithinKingdom(kingdom, 5, 10, 0.40f, 2.5f);

            GameHelpers.Log($"[ClassicalEconomics] 革命爆发！<{name}> 旧政权被推翻 击杀{killed}人 重分配硬币={extracted} 王国内济贫={internalRedist}");
            GameHelpers.Notify($"[革命] <{name}> 旧政权被推翻！击杀 {killed} 人");
            EventStreamService.Record(EventStreamService.TypeRevolution, name, killed);
            try
            {
                var any = GameHelpers.FindFirstCivActor(kingdom);
                if (any != null) WorldLog.logFavMurder(any, null);
            }
            catch (System.Exception) { }
        }

        private static int KillRatioOfKingdom(Kingdom kingdom, float ratio)
        {
            if (kingdom.units == null) return 0;
            var units = SnapshotUnits(kingdom, _actorPool);
            int target = (int)(units.Count * ratio);
            int killed = 0;
            foreach (var a in units)
            {
                if (killed >= target) break;
                if (a == null || !a.isAlive()) continue;
                if (GameHelpers.TryDieActor(a, AttackType.Weapon)) killed++;
            }
            return killed;
        }

        /// <summary>抽取王国 50% 硬币，分给财富最低的 3 个王国；返回抽取总额。</summary>
        private static long RedistributeWealth(Kingdom kingdom)
        {
            if (kingdom.units == null) return 0L;
            var units = SnapshotUnits(kingdom, _actorPool);
            long total = 0;
            foreach (var a in units)
            {
                if (a == null || !a.isAlive()) continue;
                try { total += Mathf.Max(0, Mathf.RoundToInt(a.money)); } catch (System.Exception) { }
            }
            long extract = total / 2;
            if (extract <= 0) return 0L;

            // 从王国逐人抽取
            GameHelpers.DeductCoins(units, extract);

            // 分给最穷的 3 个王国（均分）
            var others = _poorestPool;
            others.Clear();
            foreach (var ks in EconomyEngine.KingdomStats.Values)
            {
                if (ks.KingdomId == 0 || ks.KingdomId == kingdom.data.id) continue;
                others.Add(ks);
            }
            others.Sort((x, y) => x.GDP.CompareTo(y.GDP));
            int receivers = Mathf.Min(3, others.Count);
            long per = receivers > 0 ? extract / receivers : 0L;
            for (int i = 0; i < receivers; i++)
            {
                var target = GameHelpers.FindKingdom(others[i].KingdomId);
                if (target == null || target.units == null) continue;
                var tu = SnapshotUnits(target, _actorPool2);
                int count = 0;
                foreach (var a in tu) if (a != null && a.isAlive()) count++;
                if (count == 0) continue;
                int share = (int)(per / count);
                if (share <= 0) continue;
                foreach (var a in tu)
                {
                    if (a == null || !a.isAlive()) continue;
                    try { a.addMoney(share); } catch (System.Exception) { }
                }
            }
            return extract;
        }

        // ===== 反射辅助（战争检测）=====

        private static System.Collections.IEnumerable GetWars()
        {
            try
            {
                if (_warsProp == null)
                {
                    _warsProp = typeof(DiplomacyHelpers).GetProperty("wars",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }
                return _warsProp?.GetValue(null) as System.Collections.IEnumerable;
            }
            catch (System.Exception) { return null; }
        }

        private static void EnsureWarReflection()
        {
            if (_attackerProp != null) return;
            var t = typeof(WarData);
            _attackerProp = t.GetProperty("main_attacker");
            _defenderProp = t.GetProperty("main_defender");
            _winnerField = t.GetField("winner");
        }

        // ===== 通用辅助 =====

        /// <summary>从 kingdom.units 取出存活 Actor 到指定复用缓冲（不分配新 List）。</summary>
        private static List<Actor> SnapshotUnits(Kingdom kingdom, List<Actor> pool)
        {
            pool.Clear();
            if (kingdom.units == null) return pool;
            pool.AddRange(kingdom.units);
            return pool;
        }

        private static string WinnerName(int winner)
            => winner == WinnerAttackers ? "攻方" : "守方";
    }
}
