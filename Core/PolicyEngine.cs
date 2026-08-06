using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 国家政策引擎：高基尼王国每年以一定概率尝试"贫富调节政策"来降低基尼指数。
    /// 政策有概率失败：失败时统治者退位（removeKing，游戏 5-20 年后自动产生新王）、
    /// 死亡（die）或王国陷入内战（原版叛乱机制，持续至城市收回/和谈）；
    /// 成功则王国内劫富济贫（财富再分配），基尼指数直接下降。
    /// 概率设计：基尼越高改革越激进、越容易失败；失败后该王国进入冷却，缓冲后再次尝试。
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

                if (RollSuccess(stats.GiniCoefficient))
                {
                    ApplyPolicy(kingdom, stats);
                }
                else
                {
                    FailPolicy(kingdom, stats);
                    _cooldown[kid] = currentYear + CooldownYears; // 失败进入冷却，到期自动解除
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

        /// <summary>成功概率：基尼越高成功率越低（改革越激进越易失败）。</summary>
        private static bool RollSuccess(float gini)
        {
            // gini=阈值 → 0.65；gini=1.0 → 0.35
            float success = Mathf.Lerp(BaseSuccessChance, 0.35f,
                Mathf.Clamp01((gini - PolicyGiniThreshold) / (1f - PolicyGiniThreshold)));
            return Random.value < success;
        }

        /// <summary>政策成功：王国内劫富济贫（Top5 富 → Bottom10 穷），直接降低基尼。</summary>
        private static void ApplyPolicy(Kingdom kingdom, KingdomStats stats)
        {
            GameHelpers.RedistributeWithinKingdom(kingdom, 5, 10, 0.40f, 2f);
            string kName = GameHelpers.SafeKingdomName(kingdom);
            GameHelpers.Notify($"[政策] <{kName}> 推行贫富调节政策，财富再分配，贫富差距下降");
            EventStreamService.Record(EventStreamService.TypePolicy, kName, 1);
            if (UnrestConfig.Instance.LogToWorldLog)
            {
                Debug.Log($"[ClassicalEconomics] 政策成功 王国<{kName}> 基尼={stats.GiniCoefficient:F2}（财富再分配）");
            }
        }

        /// <summary>政策失败：随机选择统治者退位、死亡或王国陷入内战（40% / 30% / 30%）。</summary>
        private static void FailPolicy(Kingdom kingdom, KingdomStats stats)
        {
            string kName = GameHelpers.SafeKingdomName(kingdom);
            float roll = Random.value;

            // 1) 退位（40%）：国王转回平民，游戏 5-20 年后自动产生新王（removeKing 为 internal，由 GameHelpers 反射调用）
            if (roll < 0.4f)
            {
                if (kingdom.hasKing() && GameHelpers.TryRemoveKing(kingdom))
                {
                    GameHelpers.Notify($"[政策] <{kName}> 改革失败！国王退位");
                    EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 1); // 1=退位
                    if (UnrestConfig.Instance.LogToWorldLog)
                    {
                        Debug.Log($"[ClassicalEconomics] 政策失败 王国<{kName}> 国王退位（基尼={stats.GiniCoefficient:F2}）");
                    }
                }
                return;
            }

            // 2) 驾崩（30%）：国王死亡（dieAndDestroy 为 public）
            if (roll < 0.7f)
            {
                var king = kingdom.king;
                if (king != null && king.isAlive() && !king.hasDied())
                {
                    try
                    {
                        king.dieAndDestroy(AttackType.Other);
                    }
                    catch (System.Exception) { }
                    GameHelpers.Notify($"[政策] <{kName}> 改革失败！国王驾崩");
                    EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 2); // 2=驾崩
                    if (UnrestConfig.Instance.LogToWorldLog)
                    {
                        Debug.Log($"[ClassicalEconomics] 政策失败 王国<{kName}> 国王死亡（基尼={stats.GiniCoefficient:F2}）");
                    }
                }
                return;
            }

            // 3) 内战（30%）：改革失败引发城市叛乱，王国陷入内战（原版叛乱机制：叛军与原王国交战，
            //    由 SustainRebelWars 持续维持，直到城市收回或和谈）。若王国已暴动中或内战无法触发
            //    （无可用人口/城市），回退为退位。
            if (UnrestEngine.GetState(kingdom.data.id, out _) == 2 || UnrestEngine.TriggerCivilWar(kingdom) == 0)
            {
                if (kingdom.hasKing() && GameHelpers.TryRemoveKing(kingdom))
                {
                    GameHelpers.Notify($"[政策] <{kName}> 改革失败！国王退位");
                    EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 1); // 1=退位
                    if (UnrestConfig.Instance.LogToWorldLog)
                    {
                        Debug.Log($"[ClassicalEconomics] 政策失败 王国<{kName}> 国王退位（内战触发失败回退，基尼={stats.GiniCoefficient:F2}）");
                    }
                }
                return;
            }
            GameHelpers.Notify($"[政策] <{kName}> 改革失败！王国陷入内战");
            EventStreamService.Record(EventStreamService.TypePolicyFail, kName, 3); // 3=内战
            if (UnrestConfig.Instance.LogToWorldLog)
            {
                Debug.Log($"[ClassicalEconomics] 政策失败 王国<{kName}> 内战爆发（基尼={stats.GiniCoefficient:F2}）");
            }
        }
    }
}
