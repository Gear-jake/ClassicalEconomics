using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 社会动荡引擎（Phase 4）。
    /// 自动检测高基尼王国（基尼 ≥ 阈值），施加国家特质（rebellion）并
    /// 触发原生叛乱机制（DiplomacyHelpersRebellion.startRebellion）。
    /// 严格复用原生 Kingdom trait 与叛乱系统，不创建自定义 trait。
    /// </summary>
    public static class UnrestEngine
    {
        /// <summary>国家动荡特质 ID（标记王国处于社会震荡状态）。</summary>
        private const string UnrestTraitId = "rebellion";

        /// <summary>连续高基尼超过阈值多少年后开始暴动。</summary>
        public const int UnrestDelayYears = 10;

        /// <summary>
        /// 王国震荡状态：StartYear=高基尼起始年（-1=无），HasRebelled=是否已开始暴动，
        /// RebelYear=叛乱实际触发年份（-1=无；供革命时序门计算"叛乱持续年数"），
        /// UprisingStartYear=街头起义起始年（-1=无），HasUprising=是否已触发街头起义。
        /// 基尼 ≥ 阈值持续 UnrestDelayYears 年后触发暴动；
        /// 暴动后基尼仍 ≥ UprisingGiniThreshold 持续 UprisingDelayYears 年 → 街头起义（政权崩塌）。
        /// 基尼回落则状态清零、移除动荡特质。
        /// </summary>
        private class UnrestState
        {
            public int StartYear = -1;
            public bool HasRebelled;
            public int RebelYear = -1;
            public int UprisingStartYear = -1;
            public bool HasUprising;
        }

        private static readonly Dictionary<long, UnrestState> _states = new Dictionary<long, UnrestState>();

        /// <summary>重置（新地图/新游戏）：清空震荡状态与收复战争跟踪，避免旧世界残留泄漏进新地图（M7）。</summary>
        public static void Reset()
        {
            _states.Clear();
            _rebelWars.Clear();
            _sustainTimer = 0f;
        }

        // ===== 持续收复战争：原王国 id → 叛乱王国 id =====
        // 暴动后原王国必须持续与叛乱王国交战（不停战），直到收回城市（叛乱王国消失）。
        private static readonly Dictionary<long, long> _rebelWars = new Dictionary<long, long>();

        // ===== 性能优化：复用每年扫描缓冲，避免 GC 分配 =====
        private static readonly HashSet<long> _seen = new HashSet<long>();
        private static readonly List<long> _removeIds = new List<long>();
        private static readonly List<City> _cityPool = new List<City>();
        private static readonly List<Actor> _candidatePool = new List<Actor>();

        /// <summary>退路施加的原生动荡特质池（静态数组，避免每次调用分配）。</summary>
        private static readonly string[] TraitPool = { "hotheaded", "greedy", "paranoid" };

        /// <summary>
        /// 每周期自动评估所有王国：基尼连续超过阈值满 10 年 → 开始暴动（国家特质 + 原版叛乱）。
        /// 在 EconomyEngine.Recalculate 之后调用。
        /// </summary>
        public static void Evaluate()
        {
            var cfg = UnrestConfig.Instance;
            if (!cfg.Enabled) return;

            // 开局宽限期：世界前 N 年不触发动荡，让经济先发展（避免起步期大家都穷导致基尼虚高）
            int currentYear = EconomyModMain.GetCurrentGameYear();
            if (currentYear < cfg.MinUnrestStartYear) return;

            if (World.world == null || World.world.kingdoms == null) return;

            // 先做 List 快照，避免 addTrait/startRebellion 修改集合导致 foreach 异常
            var kingdomList = GameHelpers.KingdomSnapshot();
            var seen = _seen;
            seen.Clear();
            foreach (var kingdom in kingdomList)
            {
                if (kingdom == null || kingdom.data == null) continue;
                long kid = kingdom.data.id;
                seen.Add(kid);
                if (!EconomyEngine.KingdomStats.TryGetValue(kid, out var stats))
                    continue;

                if (stats.GiniCoefficient >= cfg.GiniThreshold)
                {
                    // 高基尼：进入/维持震荡累积状态
                    if (!_states.TryGetValue(kid, out var st))
                    {
                        st = new UnrestState();
                        _states[kid] = st;
                    }
                    if (st.StartYear < 0) st.StartYear = currentYear;

                    // 持续超阈值满 10 年 → 开始暴动（每持续期只触发一次）
                    int elapsed = currentYear - st.StartYear;
                    if (!st.HasRebelled && elapsed >= UnrestDelayYears)
                    {
                        int affected = TriggerUnrest(kingdom, stats);
                        if (affected > 0)
                        {
                            st.HasRebelled = true;
                            st.RebelYear = currentYear; // 记录叛乱触发年：革命延迟从此年起算（非高基尼起始年）
                            EventStreamService.Record(EventStreamService.TypeUnrest, stats.KingdomName, affected);
                            GameHelpers.Notify($"[动荡] <{stats.KingdomName}> 社会动荡爆发，{affected} 座城市暴动");
                            if (cfg.LogToWorldLog)
                            {
                                Debug.Log($"[ClassicalEconomics] 社会震荡爆发 王国<{stats.KingdomName}> " +
                                          $"持续高基尼{elapsed}年 基尼={stats.GiniCoefficient:F2} 影响={affected}");
                            }
                        }
                    }
                    // 暴动后基尼仍 ≥ 起义阈值持续 UprisingDelayYears 年 → 街头起义（政权崩塌：全城暴动+杀富济贫+推翻国王）
                    else if (st.HasRebelled && !st.HasUprising && stats.GiniCoefficient >= cfg.UprisingGiniThreshold)
                    {
                        if (st.UprisingStartYear < 0) st.UprisingStartYear = currentYear;
                        int uElapsed = currentYear - st.UprisingStartYear;
                        if (uElapsed >= cfg.UprisingDelayYears)
                        {
                            // 返回值 = 处决富豪数或全城暴动数（任一 > 0 即起义实质爆发）
                            int result = TriggerUprising(kingdom, stats);
                            if (result > 0)
                            {
                                st.HasUprising = true;
                                EventStreamService.Record(EventStreamService.TypeUprising, stats.KingdomName, result);
                                GameHelpers.Notify($"[起义] <{stats.KingdomName}> 街头起义爆发！{result} 名富豪被处决，国王被推翻");
                                if (cfg.LogToWorldLog)
                                {
                                    Debug.Log($"[ClassicalEconomics] 街头起义 王国<{stats.KingdomName}> " +
                                              $"叛乱后基尼仍超起义阈值{uElapsed}年 基尼={stats.GiniCoefficient:F2} 处决富豪={result}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // 基尼在动荡阈值以上但未达起义条件：若曾进入起义累积，重置计时
                        st.UprisingStartYear = -1;
                    }
                }
                else
                {
                    // 基尼回落：暴动中则移除动荡特质（平息），并清除状态
                    if (_states.TryGetValue(kid, out var st))
                    {
                        if (st.HasRebelled && kingdom.hasTrait(UnrestTraitId))
                        {
                            kingdom.removeTrait(UnrestTraitId);
                            if (cfg.LogToWorldLog)
                            {
                                Debug.Log($"[ClassicalEconomics] 社会震荡平息 王国<{stats.KingdomName}> 基尼回落至 {stats.GiniCoefficient:F2}");
                            }
                        }
                        _states.Remove(kid);
                    }
                }
            }

            // 清理已消失王国的状态
            var removeIds = _removeIds;
            removeIds.Clear();
            foreach (var kv in _states)
                if (!seen.Contains(kv.Key)) removeIds.Add(kv.Key);
            foreach (var id in removeIds) _states.Remove(id);
            // 收复战争由 EconomyTickRunner.Update 每帧维持（高频），此处不再每年调用
        }

        // 持续收复战争的高频维持节流（每帧由 Update 驱动，1 秒节流）
        private static float _sustainTimer;

        /// <summary>
        /// 维持收复战争：只要叛乱王国仍存在，原王国必须与其保持战争（不停战），
        /// 直到叛乱城市被收回（叛乱王国消失）。
        /// 每帧由 EconomyTickRunner.Update 调用（内部 1 秒节流）：
        /// 1. 检测"谋求和平"——取消针对收复战争的停战 plot（游戏 AI 靠 attacker_stop_war plot 和谈），
        ///    阻止和平，战争自然持续，避免"和谈→重宣"循环造成的卡顿；
        /// 2. 战争异常结束后兜底重新宣战（叛军仍在但战争丢失时）。
        /// </summary>
        public static void SustainRebelWars()
        {
            if (_rebelWars.Count == 0) return;
            _sustainTimer += Time.deltaTime;
            if (_sustainTimer < 1f) return;
            _sustainTimer = 0f;

            // 1. 阻止和平：取消针对收复战争的停战 plot（治本，和平意图消失后重宣战极少触发）
            BlockPeacePlots();

            var removeIds = _removeIds;
            removeIds.Clear();
            foreach (var kv in _rebelWars)
            {
                long origId = kv.Key, rebelId = kv.Value;
                Kingdom orig = GameHelpers.FindKingdom(origId);
                Kingdom rebel = GameHelpers.FindKingdom(rebelId);
                if (orig == null || rebel == null)
                {
                    removeIds.Add(origId); // 任一方消失（城市已收回/灭国）→ 停止跟踪
                    // 暴动解决：叛军王国消失（rebel==null）→ 同时平息原王国的暴动状态，
                    // 移除动荡特质并清除震荡记录，避免"战斗结束但国家仍显示暴动中"的残留。
                    if (orig != null && rebel == null) ResolveUnrest(orig, false); // 城市收回
                    continue;
                }
                bool warActive = false;
                try
                {
                    // 任意类型战争均可：rebellion 可能被游戏转为普通战争，同样视为维持中
                    foreach (var w in orig.getWars())
                    {
                        if (w == null) continue;
                        foreach (var d in w.getDefenders())
                        {
                            if (d != null && d.data.id == rebelId) { warActive = true; break; }
                        }
                        if (warActive) break;
                    }
                }
                catch (System.Exception) { }
                if (!warActive)
                {
                    // 兜底：战争意外消失（非正常收回）→ 重新宣战（不停战，直至收回城市）
                    if (StartWarViaReflection(orig, rebel))
                    {
                        GameHelpers.Log($"[ClassicalEconomics] 叛军仍在，重新宣战 原王国<{GameHelpers.SafeKingdomName(orig)}> → 叛军<{GameHelpers.SafeKingdomName(rebel)}>（收回城市战争持续）");
                    }
                    else
                    {
                        // 重宣战失败（和谈/停战已生效，战争无法再开）：视为和谈解决暴动。
                        // 独立消息 + 平息状态 + 停止跟踪，避免"和谈成功但国家仍标记暴动中"。
                        removeIds.Add(origId);
                        ResolveUnrest(orig, true); // 和谈解决
                    }
                }
            }
            foreach (var id in removeIds) _rebelWars.Remove(id);
        }

        /// <summary>
        /// 平息暴动状态：移除原王国的动荡特质并清除震荡记录，使国家恢复正常。
        /// 若贫富差距仍高，Evaluate 会重新累积、满 UnrestDelayYears 年后再次暴动（合理循环）。
        /// 按结束方式区分提示消息：viaPeace=false=城市收回（叛军消失），viaPeace=true=和谈解决。
        /// </summary>
        private static void ResolveUnrest(Kingdom kingdom, bool viaPeace)
        {
            try
            {
                if (kingdom.hasTrait(UnrestTraitId)) kingdom.removeTrait(UnrestTraitId);
            }
            catch (System.Exception) { }
            _states.Remove(kingdom.data.id);
            string kName = GameHelpers.SafeKingdomName(kingdom);
            if (viaPeace)
            {
                GameHelpers.Notify($"[动荡] <{kName}> 与叛军达成和谈，暴动结束，王国恢复正常");
                EventStreamService.Record(EventStreamService.TypeUnrestPeace, kName, 0);
            }
            else
            {
                GameHelpers.Notify($"[动荡] <{kName}> 暴动平息，城市已收回，王国恢复正常");
                EventStreamService.Record(EventStreamService.TypeUnrestResolved, kName, 0);
            }
            if (UnrestConfig.Instance.LogToWorldLog)
            {
                Debug.Log($"[ClassicalEconomics] 暴动平息 王国<{kName}>（{(viaPeace ? "和谈解决" : "叛军消失，城市收回")}）");
            }
        }

        /// <summary>
        /// 检测"谋求和平"：遍历世界 plots，取消正在执行的停战 plot
        /// （游戏 AI 通过 attacker_stop_war / stop_war 行动向敌人求和）
        /// 且目标战争是我们登记的收复战争。阻止 AI 和谈，让战争持续到收回城市。
        /// 常态下无此类 plot 时零干预；只在和平意图出现时取消，开销可忽略。
        /// </summary>
        private static void BlockPeacePlots()
        {
            try
            {
                if (World.world == null || World.world.plots == null) return;
                foreach (var plot in World.world.plots)
                {
                    if (plot == null || !plot.isActive()) continue;
                    var asset = plot.getAsset();
                    if (asset == null) continue;
                    string pid = asset.id;
                    if (pid != "stop_war" && pid != "attacker_stop_war") continue;
                    War tw = plot.target_war;
                    if (tw == null || tw.hasEnded()) continue;
                    if (!IsTrackedWar(tw)) continue;
                    World.world.plots.cancelPlot(plot);
                    GameHelpers.Log($"[ClassicalEconomics] 阻止和谈：取消停战 plot（原王国 vs 叛军，战争持续至收回城市）");
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>判断 War 是否为我们登记的某对收复战争（同时包含原王国与叛军任一方）。</summary>
        private static bool IsTrackedWar(War w)
        {
            if (_rebelWars.Count == 0) return false;
            foreach (var kv in _rebelWars)
            {
                long origId = kv.Key, rebelId = kv.Value;
                bool hasOrig = false, hasRebel = false;
                foreach (var a in w.getAttackers())
                {
                    if (a == null || a.data == null) continue;
                    if (a.data.id == origId) hasOrig = true;
                    if (a.data.id == rebelId) hasRebel = true;
                }
                foreach (var d in w.getDefenders())
                {
                    if (d == null || d.data == null) continue;
                    if (d.data.id == origId) hasOrig = true;
                    if (d.data.id == rebelId) hasRebel = true;
                }
                if (hasOrig && hasRebel) return true;
            }
            return false;
        }

        /// <summary>
        /// 反射调用 internal DiplomacyManager.startWar(Kingdom, Kingdom, WarTypeAsset) 发起叛乱战争。
        /// </summary>
        private static System.Reflection.MethodInfo _startWarMethod;

        private static bool StartWarViaReflection(Kingdom attacker, Kingdom defender)
        {
            try
            {
                if (_startWarMethod == null)
                {
                    _startWarMethod = typeof(DiplomacyManager).GetMethod("startWar",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                }
                if (_startWarMethod == null) return false;
                _startWarMethod.Invoke(World.world.diplomacy,
                    new object[] { attacker, defender, WarTypeLibrary.rebellion });
                return true;
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>
        /// 捕获刚产生的叛乱王国并登记（startRebellion 会以原王国为攻方、叛乱王国为守方创建 rebellion 战争）。
        /// </summary>
        private static void RecordRebelKingdom(Kingdom kingdom)
        {
            try
            {
                foreach (var w in kingdom.getWars())
                {
                    if (w == null || w.getAsset() != WarTypeLibrary.rebellion) continue;
                    foreach (var d in w.getDefenders())
                    {
                        if (d != null && d.data.id != kingdom.data.id)
                        {
                            _rebelWars[kingdom.data.id] = d.data.id;
                            return;
                        }
                    }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// 查询王国震荡状态：0=无状态，1=高基尼累积中（elapsedYears=已持续年数），
        /// 2=暴动中，3=街头起义中（政权崩塌，elapsedYears=起义持续年数）。
        /// 供 UI 展示"用状态表示"的震荡进度。
        /// </summary>
        public static int GetState(long kingdomId, out int elapsedYears)
        {
            elapsedYears = 0;
            if (_states.TryGetValue(kingdomId, out var st))
            {
                if (st.HasUprising)
                {
                    elapsedYears = st.UprisingStartYear >= 0 ? EconomyModMain.GetCurrentGameYear() - st.UprisingStartYear : 0;
                    return 3;
                }
                if (st.HasRebelled)
                {
                    // elapsedYears = 叛乱持续年数（自 RebelYear 起算，供革命时序门与 UI 展示）
                    elapsedYears = st.RebelYear >= 0 ? EconomyModMain.GetCurrentGameYear() - st.RebelYear : 0;
                    return 2;
                }
                if (st.StartYear >= 0)
                {
                    elapsedYears = EconomyModMain.GetCurrentGameYear() - st.StartYear;
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// 手动煽动：对指定王国立即施加国家特质 + 触发叛乱，并标记为暴动中。
        /// </summary>
        public static int Incite(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.units == null || kingdom.units.Count == 0) return 0;
            if (!EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out var stats))
            {
                stats = new KingdomStats { KingdomName = kingdom.data.name };
            }
            int affected = TriggerAndMark(kingdom, stats);
            if (affected > 0)
            {
                EventStreamService.Record(EventStreamService.TypeIncite, stats.KingdomName, affected);
                if (UnrestConfig.Instance.LogToWorldLog)
                {
                    Debug.Log($"[ClassicalEconomics] 手动煽动 王国<{kingdom.data.name}> 影响={affected}");
                }
            }
            return affected;
        }

        /// <summary>
        /// 触发内战：对指定王国立即引发叛乱（城市暴动、叛军与原王国交战，
        /// 由 SustainRebelWars 持续维持战争直到城市收回或和谈）。
        /// 供改革失败等场景复用；不记录"手动煽动"事件（由调用方自行记录语义）。
        /// 返回受影响城市数（0 = 无可用人口/城市，内战未爆发）。
        /// </summary>
        public static int TriggerCivilWar(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.units == null || kingdom.units.Count == 0) return 0;
            if (!EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out var stats))
            {
                stats = new KingdomStats { KingdomName = kingdom.data.name };
            }
            int affected = TriggerAndMark(kingdom, stats);
            if (affected > 0 && UnrestConfig.Instance.LogToWorldLog)
            {
                Debug.Log($"[ClassicalEconomics] 内战爆发 王国<{stats.KingdomName}> 影响={affected}");
            }
            return affected;
        }

        /// <summary>
        /// 触发叛乱并登记震荡状态（供手动煽动/内战复用；仅在实质爆发时登记）。
        /// 返回受影响城市数。
        /// </summary>
        private static int TriggerAndMark(Kingdom kingdom, KingdomStats stats)
        {
            int affected = TriggerUnrest(kingdom, stats);
            if (affected > 0)
            {
                if (!_states.TryGetValue(kingdom.data.id, out var st))
                {
                    st = new UnrestState();
                    _states[kingdom.data.id] = st;
                }
                st.StartYear = EconomyModMain.GetCurrentGameYear();
                st.RebelYear = EconomyModMain.GetCurrentGameYear();
                st.HasRebelled = true;
            }
            return affected;
        }

        /// <summary>
        /// 手动镇压：移除指定王国的动荡特质，平息社会震荡并清除状态。
        /// 镇压后执行王国内劫富济贫（从富人抽税分给穷人），直接降低该王国基尼系数，
        /// 避免"镇压平息但贫富差距依旧"的悖论。
        /// </summary>
        public static int Suppress(Kingdom kingdom)
        {
            if (kingdom == null) return 0;
            int removed = 0;
            if (kingdom.hasTrait(UnrestTraitId))
            {
                kingdom.removeTrait(UnrestTraitId);
                removed++;
            }
            _states.Remove(kingdom.data.id);
            _rebelWars.Remove(kingdom.data.id); // 玩家主动镇压：停止持续收复战争

            // 镇压后劫富济贫：平息社会不公（Top3 富 → Bottom5 穷，超出人均 3 倍部分征 30%）
            long redistributed = GameHelpers.RedistributeWithinKingdom(kingdom, 3, 5, 0.30f, 3f);

            if (removed > 0 || redistributed > 0)
            {
                string kName = GameHelpers.SafeKingdomName(kingdom);
                EventStreamService.Record(EventStreamService.TypeSuppress, kName, removed + (int)(redistributed / 10));
                if (UnrestConfig.Instance.LogToWorldLog)
                {
                    Debug.Log($"[ClassicalEconomics] 手动镇压 王国<{kName}> 已移除动荡特质，劫富济贫 {redistributed} 金币");
                }
            }
            return removed;
        }

        /// <summary>
        /// 计算本次暴动的城市数量：随机值，且受贫富差距影响——
        /// 基尼系数超过阈值越多，可暴动城市越多；差距越小（刚过阈值）则越少（最低 1 座）。
        /// 返回 [1, 按贫富差距放大后的上限] 之间的随机整数。
        /// </summary>
        private static int ComputeUnrestCityCount(float gini, UnrestConfig cfg, int cityCount)
        {
            float threshold = Mathf.Max(0.05f, cfg.GiniThreshold);
            // 基尼超出阈值的部分归一化到 0..1（阈值 → 1.0）
            float ratio = (gini - threshold) / Mathf.Max(0.01f, 1f - threshold);
            ratio = Mathf.Clamp01(ratio);
            // 暴动规模上限随城市数放大（大国暴动更多城、更难镇压，避免"3 城对大国无关痛痒"），
            // 最少 MaxAffectedPerKingdom；基尼极高时最多约 50% 城市。
            int byCity = Mathf.Max(1, Mathf.RoundToInt(cityCount * 0.5f));
            int max = Mathf.Max(cfg.MaxAffectedPerKingdom, byCity);
            int upper = 1 + Mathf.RoundToInt(ratio * (max - 1));
            return Random.Range(1, upper + 1); // [1, upper]
        }

        /// <summary>
        /// 街头起义（政权彻底崩塌）：在普通暴动的基础上彻底清算——
        /// 1. 杀富济贫：按人口比例处决王国最富 Top 富豪，其财富分给最穷公民；
        /// 2. 全城暴动：所有城市同时叛乱（不受 MaxAffectedPerKingdom 限制）；
        /// 3. 推翻国王：removeKing（政权崩塌，游戏稍后自动产生新王）。
        /// 返回处决的富豪数（0 = 无富余/无人口，起义未实质爆发）。
        /// </summary>
        private static int TriggerUprising(Kingdom kingdom, KingdomStats stats)
        {
            var cfg = UnrestConfig.Instance;

            // 1. 杀富济贫：处决 Top 富豪（比例按人口），财富分给最穷公民
            int civCount = 0;
            if (kingdom.units != null)
                foreach (var a in kingdom.units)
                    if (a != null && a.isAlive() && a.asset != null && a.asset.civ) civCount++;
            if (civCount < 4) return 0; // 人口过少不触发（避免误杀国王/唯一富户）

            int richCount = Mathf.Max(1, Mathf.RoundToInt(civCount * cfg.KillRichRatio));
            int poorCount = Mathf.Max(3, Mathf.RoundToInt(civCount * 0.15f));
            int killed = GameHelpers.KillRichGiveToPoor(kingdom, richCount, poorCount, cfg.KillRichRedistRatio);

            // 2. 全城暴动：全部城市叛乱（街头起义，不受数量上限限制）
            int affected = 0;
            var cities = kingdom.cities;
            if (cities != null)
            {
                var cityList = _cityPool;
                cityList.Clear();
                cityList.AddRange(cities);
                foreach (var city in cityList)
                {
                    if (city == null) continue;
                    Actor leader = PickCityActor(city, kingdom);
                    if (leader == null) continue;
                    try
                    {
                        var plot = new Plot();
                        DiplomacyHelpersRebellion.startRebellion(leader, plot, true);
                        RecordRebelKingdom(kingdom);
                        affected++;
                    }
                    catch (System.Exception) { }
                }
            }

            // 3. 推翻国王：政权崩塌（起义军处决暴君）
            if (kingdom.hasKing()) GameHelpers.TryRemoveKing(kingdom);

            // 4. 施加国家特质（保持暴动状态直至城市收回）
            kingdom.addTrait(UnrestTraitId, true);

            return killed > 0 ? killed : affected;
        }

        /// <summary>
        /// 对王国施加国家特质并触发原生叛乱。
        /// 暴乱城市数随机（受贫富差距影响：差距越大越多），
        /// 配合王国冷却实现"一座接一座"的渐次蔓延，避免一次性大范围分裂。
        /// </summary>
        private static int TriggerUnrest(Kingdom kingdom, KingdomStats stats)
        {
            var cfg = UnrestConfig.Instance;
            int affected = 0;

            // 1. 施加国家特质
            kingdom.addTrait(UnrestTraitId, true);

            // 1.5 暴乱起因是贫富不均：暴乱爆发即劫富济贫（Top3 富 → Bottom5 穷），降低该国基尼
            GameHelpers.RedistributeWithinKingdom(kingdom, 3, 5, 0.25f, 3f);

            // 2. 对王国城市触发原生叛乱（数量随机，随贫富差距增大而增多）
            var cities = kingdom.cities;
            if (cities != null)
            {
                var cityList = _cityPool;
                cityList.Clear();
                cityList.AddRange(cities);
                int maxAffect = ComputeUnrestCityCount(stats.GiniCoefficient, cfg, cityList.Count);
                foreach (var city in cityList)
                {
                    if (affected >= maxAffect) break;
                    if (city == null) continue;

                    Actor leader = PickCityActor(city, kingdom);
                    if (leader == null) continue;

                    try
                    {
                        var plot = new Plot();
                        DiplomacyHelpersRebellion.startRebellion(leader, plot, true);
                        RecordRebelKingdom(kingdom); // 登记叛乱王国，确保持续战争收回城市
                        affected++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[ClassicalEconomics] 叛乱触发失败 城市<{city.data?.name}>: {e.Message}");
                    }
                }
            }

            // 3. 退路：对王国 Actor 施加原生 trait
            if (affected == 0 && kingdom.units != null)
            {
                affected = ApplyActorTraits(kingdom);
            }

            // 4. 推送 WorldLog 事件（默认关闭，需在配置页开启 LogToWorldLog）
            if (affected > 0 && cfg.LogToWorldLog)
            {
                var firstActor = GameHelpers.FindFirstCivActor(kingdom);
                if (firstActor != null)
                {
                    WorldLog.logFavMurder(firstActor, null);
                }
            }

            // 5. 完全无效果（无城市/无成员可煽动）：撤销刚施加的国家特质，
            //    避免"特质残留但无暴动"的状态不一致（起义判据依据 HasRebelled 而非特质）。
            if (affected == 0)
            {
                try { if (kingdom.hasTrait(UnrestTraitId)) kingdom.removeTrait(UnrestTraitId); } catch (System.Exception) { }
            }

            return affected;
        }

        /// <summary>从城市中选取一个存活的 Actor 作为叛乱 leader。</summary>
        private static Actor PickCityActor(City city, Kingdom kingdom)
        {
            if (city == null) return null;
            var leader = city.leader;
            if (leader != null && leader.isAlive())
                return leader;
            return GameHelpers.FindFirstCivActor(kingdom);
        }

        /// <summary>对王国 Actor 施加原生 trait（hotheaded/greedy/paranoid）。</summary>
        private static int ApplyActorTraits(Kingdom kingdom)
        {
            var cfg = UnrestConfig.Instance;
            string[] pool = TraitPool;

            // 收集候选到复用缓冲
            var candidates = _candidatePool;
            candidates.Clear();
            if (kingdom.units != null)
            {
                foreach (var actor in kingdom.units)
                {
                    if (actor == null || !actor.isAlive()) continue;
                    if (actor.asset == null || !actor.asset.civ) continue;
                    candidates.Add(actor);
                }
            }
            if (candidates.Count == 0) return 0;

            GameHelpers.Shuffle(candidates); // Fisher-Yates（GameHelpers 公共实现）

            int max = cfg.MaxAffectedPerKingdom;
            int affected = 0;
            for (int i = 0; i < candidates.Count && affected < max; i++)
            {
                string traitId = pool[Random.Range(0, pool.Length)];
                candidates[i].addTrait(traitId, true);
                affected++;
            }
            return affected;
        }
    }
}
