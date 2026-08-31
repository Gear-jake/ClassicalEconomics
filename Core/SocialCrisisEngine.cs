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
        private static readonly List<Kingdom> _kingdomPool = new List<Kingdom>(3);

        // 战败结算：财富降序比较器（富人优先被扣 → 劫富济贫）
        private static readonly System.Comparison<Actor> _wealthDescCompare = (a, b) => WealthOf(b).CompareTo(WealthOf(a));

        /// <summary>每年评估一次（在 UnrestEngine.Evaluate 之后调用）。</summary>
        public static void Evaluate()
        {
            try
            {
                var cfg = UnrestConfig.Instance;
                if (!cfg.CycleEnabled) return; // 跟随经济周期开关
                if (World.world == null || World.world.units == null) return;

                WarPlunderCheck(cfg);
                RevolutionCheck(cfg);
            }
            finally
            {
                ClearWorldReferences();
            }
        }

        /// <summary>清空仅用于当前世界的 Actor 引用，保留战争 ID 跟踪状态。</summary>
        public static void ClearWorldReferences()
        {
            _actorPool.Clear();
            _actorPool2.Clear();
            _kingdomPool.Clear();
        }

        /// <summary>世界重置（新地图/新游戏）时清空战争跟踪。</summary>
        public static void Reset()
        {
            ClearWorldReferences();
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
                    // 反射返回的 boxed 枚举不能直接 (int) unbox（抛 InvalidCastException 被 catch 吞掉
                    // → 战争掠夺对所有战争静默失效），必须经 Convert.ToInt32 转换。
                    winner = System.Convert.ToInt32(_winnerField.GetValue(war));
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

        /// <summary>
        /// 战败结算（两个机制配合，直接降低基尼系数）：
        /// 1) 战争损耗：掠夺额的 <see cref="UnrestConfig.WarWasteRatio"/> 比例直接"蒸发"
        ///    （战乱破坏财富、不转移给任何人），从败方富人优先扣除 → 总财富减少、富人损失绝对额更大；
        /// 2) 劫富济贫：剩余掠夺额从败方富人优先抽取，均分给败方与胜方的贫困公民。
        /// </summary>
        private static void Plunder(long attackerId, long defenderId, int winner, UnrestConfig cfg)
        {
            long winnerId = winner == WinnerAttackers ? attackerId : defenderId;
            long loserId = winner == WinnerAttackers ? defenderId : attackerId;
            var winnerKingdom = GameHelpers.FindKingdom(winnerId);
            var loserKingdom = GameHelpers.FindKingdom(loserId);
            if (loserKingdom == null || loserKingdom.units == null || loserKingdom.units.Count == 0) return;
            if (winnerKingdom == null || winnerKingdom.units == null || winnerKingdom.units.Count == 0) return;
            // 中央银行家：本国（或参战方）被掠夺后，其世界建筑被摧毁（无赔偿，战争风险真实）
            NationEngine.OnKingdomPlundered(loserId);

            // 败方成员（复用缓冲），按财富降序 → 富人优先被扣（劫富）
            var loserUnits = SnapshotUnits(loserKingdom, _actorPool);
            loserUnits.Sort(_wealthDescCompare);

            long loot = 0;
            foreach (var a in loserUnits)
                if (a != null && a.isAlive()) loot += Mathf.Max(0, Mathf.RoundToInt(a.money));
            long steal = (long)(loot * cfg.WarPlunderRatio);
            if (steal <= 0) return;

            // 1) 战争损耗：直接蒸发（不转移），从败方（富人优先）扣除
            long evap = (long)(steal * Mathf.Clamp01(cfg.WarWasteRatio));
            if (evap > 0) GameHelpers.DeductCoins(loserUnits, evap);

            // 2) 劫富济贫：剩余部分从败方（富人优先）抽取，分给败方/胜方贫困公民
            long transfer = steal - evap;
            var winnerUnits = SnapshotUnits(winnerKingdom, _actorPool2);
            long actual = 0L;
            long given = 0L;
            if (transfer > 0 && HasPoorRecipients(loserUnits, winnerUnits))
            {
                actual = GameHelpers.DeductCoins(loserUnits, transfer);
                if (actual > 0) given = GiveToPoor(loserUnits, winnerUnits, actual);
            }

            GameHelpers.Log($"[ClassicalEconomics] 战争掠夺 {WinnerName(winner)} 胜 掠夺={steal}(损耗{evap}+济贫{given}) 来自<{GameHelpers.SafeKingdomName(loserKingdom)}>");
            EventStreamService.Record(EventStreamService.TypePlunder, GameHelpers.SafeKingdomName(loserKingdom), steal);
        }

        /// <summary>安全读取生物财富（半销毁对象可能读取失败，返回 0）。</summary>
        private static float WealthOf(Actor a)
        {
            try { return Mathf.Max(0f, Mathf.RoundToInt(a.money)); } catch (System.Exception) { return 0f; }
        }

        /// <summary>单位列表存活成员的平均财富。</summary>
        private static float AvgWealth(List<Actor> units)
        {
            long sum = 0; int n = 0;
            foreach (var a in units)
            {
                if (a == null || !a.isAlive()) continue;
                sum += Mathf.Max(0, Mathf.RoundToInt(a.money));
                n++;
            }
            return n > 0 ? (float)sum / n : 0f;
        }

        private static bool HasPoorRecipients(List<Actor> losers, List<Actor> winners)
        {
            float loserLine = AvgWealth(losers) * 0.8f;
            float winnerLine = AvgWealth(winners) * 0.8f;
            foreach (var actor in losers)
                if (actor != null && actor.isAlive() && WealthOf(actor) < loserLine) return true;
            foreach (var actor in winners)
                if (actor != null && actor.isAlive() && WealthOf(actor) < winnerLine) return true;
            return false;
        }

        /// <summary>
        /// 把金额均分给"败方 + 胜方"中低于各自王国人均×0.8 的贫困公民（余数补给第一个）。
        /// 财富从败方富人（已被优先抽取）流向两国的穷人 → 直接降低基尼系数。
        /// 返回实际发放金额。
        /// </summary>
        private static long GiveToPoor(List<Actor> losers, List<Actor> winners, long amount)
        {
            float lAvg = AvgWealth(losers);
            float wAvg = AvgWealth(winners);
            float lPoor = lAvg * 0.8f;
            float wPoor = wAvg * 0.8f;

            int poorCount = 0;
            foreach (var a in losers) if (a != null && a.isAlive() && WealthOf(a) < lPoor) poorCount++;
            foreach (var a in winners) if (a != null && a.isAlive() && WealthOf(a) < wPoor) poorCount++;
            if (poorCount <= 0) return 0L;

            long per = amount / poorCount;
            long remain = amount - per * poorCount;
            long given = 0L;
            bool first = true;
            foreach (var a in losers)
            {
                if (a == null || !a.isAlive() || WealthOf(a) >= lPoor) continue;
                long give = per + (first ? remain : 0L);
                try { GameHelpers.AddPositiveMoney(a, give); given += give; } catch (System.Exception) { }
                first = false;
            }
            foreach (var a in winners)
            {
                if (a == null || !a.isAlive() || WealthOf(a) >= wPoor) continue;
                long give = per + (first ? remain : 0L);
                try { GameHelpers.AddPositiveMoney(a, give); given += give; } catch (System.Exception) { }
                first = false;
            }
            return given;
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
                if (state != 2) continue;                          // 仅普通叛乱触发革命；街头起义（3）已是政权崩塌完整事件
                if (elapsed < cfg.RevolutionDelayYears) continue;  // 叛乱持续未满 N 年

                Revolution(kingdom, cfg);
            }
        }

        /// <summary>革命：击杀人口 + 处决富豪（杀富济贫）+ 移除叛乱特质 + 硬币重新分配（旧政权被推翻）。</summary>
        private static void Revolution(Kingdom kingdom, UnrestConfig cfg)
        {
            string name = GameHelpers.SafeKingdomName(kingdom);

            // 0. 推翻国王：政权彻底崩塌（起义军处决暴君；若已无王则跳过）
            if (kingdom.hasKing()) GameHelpers.TryRemoveKing(kingdom);

            // 1. 击杀王国部分人口（革命暴力）
            int killed = KillRatioOfKingdom(kingdom, cfg.RevolutionKillRatio);

            // 1.5 杀富济贫：处决王国最富 Top 富豪，财富分给最穷公民（双通道降基尼）
            int richKilled = 0;
            if (kingdom.units != null)
            {
                int civCount = 0;
                foreach (var a in kingdom.units)
                    if (GameHelpers.IsCivilizedActor(a)) civCount++;
                if (civCount >= 4)
                {
                    int richCount = Mathf.Max(1, Mathf.RoundToInt(civCount * cfg.KillRichRatio));
                    int poorCount = Mathf.Max(3, Mathf.RoundToInt(civCount * 0.15f));
                    richKilled = GameHelpers.KillRichGiveToPoor(kingdom, richCount, poorCount, cfg.KillRichRedistRatio);
                }
            }

            // 2. 移除叛乱特质并清除震荡状态（复用镇压逻辑）
            try { UnrestEngine.Suppress(kingdom); } catch (System.Exception) { }

            // 3. 硬币重新分配：抽取王国 50% 硬币，分给最穷的 3 个王国（王国间）
            long extracted = RedistributeWealth(kingdom);

            // 3.5 王国内部劫富济贫：旧政权被推翻，从该国富人抽税分给穷人（直接降低该国基尼）
            long internalRedist = GameHelpers.RedistributeWithinKingdom(kingdom, 5, 10, 0.40f, 2.5f);

            GameHelpers.Log($"[ClassicalEconomics] 革命爆发！<{name}> 旧政权被推翻 击杀{killed}人 处决富豪{richKilled}人 重分配硬币={extracted} 王国内济贫={internalRedist}");
            GameHelpers.NotifyLocalized("toast_revolution", name, killed, richKilled);
            EventStreamService.Record(EventStreamService.TypeRevolution, name, killed + richKilled);
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

            // 分给最穷的 3 个王国（均分）
            var others = _poorestPool;
            others.Clear();
            foreach (var ks in EconomyEngine.KingdomStats.Values)
            {
                if (ks.KingdomId == 0 || ks.KingdomId == kingdom.data.id) continue;
                others.Add(ks);
            }
            others.Sort((x, y) => x.GDP.CompareTo(y.GDP));
            var receiverKingdoms = _kingdomPool;
            receiverKingdoms.Clear();
            for (int i = 0; i < others.Count && receiverKingdoms.Count < 3; i++)
            {
                var target = GameHelpers.FindKingdom(others[i].KingdomId);
                if (target == null || target.units == null) continue;
                bool hasAlive = false;
                foreach (var actor in target.units)
                    if (actor != null && actor.isAlive()) { hasAlive = true; break; }
                if (hasAlive) receiverKingdoms.Add(target);
            }
            if (receiverKingdoms.Count == 0) return 0L;

            // 仅在确认接收方后从原王国实际扣款。
            long actualExtract = GameHelpers.DeductCoins(units, extract);
            if (actualExtract <= 0) return 0L;

            long perKingdom = actualExtract / receiverKingdoms.Count;
            long kingdomRemainder = actualExtract - perKingdom * receiverKingdoms.Count;
            for (int i = 0; i < receiverKingdoms.Count; i++)
            {
                var target = receiverKingdoms[i];
                var tu = SnapshotUnits(target, _actorPool2);
                int count = 0;
                foreach (var a in tu) if (a != null && a.isAlive()) count++;
                if (count == 0) continue;
                long kingdomGive = perKingdom + (i == 0 ? kingdomRemainder : 0L);
                long perActor = kingdomGive / count;
                long actorRemainder = kingdomGive - perActor * count;
                bool first = true;
                foreach (var a in tu)
                {
                    if (a == null || !a.isAlive()) continue;
                    long actorGive = perActor + (first ? actorRemainder : 0L);
                    try { GameHelpers.AddPositiveMoney(a, actorGive); } catch (System.Exception) { }
                    first = false;
                }
            }
            return actualExtract;
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
