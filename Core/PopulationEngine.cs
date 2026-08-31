using System.Collections.Generic;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 人口约束引擎（马尔萨斯）：人口超承载 → 移民压力（不满特质，驱动原版 AI 迁徙/流失）。
    /// 承载数据来自原版 Kingdom.getPopulationTotal / getPopulationTotalPossible（住房承载）。
    /// 需在后台统计消费后调用（读取 TradeSimulationWorker.LastResult 的压力值）。
    ///
    /// 注：原设计的"饥饿→饿死人口"已移除——游戏自带饮食系统处理人口饿死，模组不再自行饿死人口。
    /// </summary>
    public static class PopulationEngine
    {
        private static readonly List<Actor> _candidates = new List<Actor>(64);

        /// <summary>每周期评估一次（需在统计消费后调用）。</summary>
        public static void Evaluate()
        {
            try
            {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.PopulationEnabled) return;
            var res = TradeSimulationWorker.LastResult;
            if (res == null || res.Kingdoms.Count == 0) return;

            foreach (var ks in res.Kingdoms)
            {
                if (ks.KingdomId == 0 || ks.Population <= 0) continue;
                if (ks.Pressure < cfg.OvercrowdRatio) continue; // 未超载

                var kingdom = GameHelpers.FindKingdom(ks.KingdomId);
                if (kingdom == null || kingdom.units == null) continue;

                // 移民约束：人口压力 → 成员不满（hotheaded），原生 AI 自行迁徙/流失
                int pressureAffected = ApplyEmigrationPressure(kingdom, ks, cfg);
                if (pressureAffected > 0 && cfg.LogToWorldLog)
                {
                    GameHelpers.Log($"[ClassicalEconomics] 人口超载·移民压力 <{ks.Name}> 压力={ks.Pressure:P0} 受影响={pressureAffected}人");
                }
            }
            }
            finally
            {
                ClearWorldReferences();
            }
        }

        /// <summary>清空仅用于当前世界的 Actor 引用。</summary>
        public static void ClearWorldReferences()
        {
            _candidates.Clear();
        }

        /// <summary>重置（新地图/新游戏）。</summary>
        public static void Reset()
        {
            ClearWorldReferences();
        }

        /// <summary>对超载王国随机成员施加不满特质（hotheaded），驱动原生移民/流失。返回受影响人数。</summary>
        private static int ApplyEmigrationPressure(Kingdom kingdom, TradeSimulationWorker.KingdomSim ks, UnrestConfig cfg)
        {
            // 超载程度越高，受影响人口越多（超载 10% → 2% 人口，上限 30 人）
            int max = Mathf.Clamp(Mathf.RoundToInt(ks.Population * (ks.Pressure - 1f) * 0.2f), 1, 30);

            var pool = _candidates;
            pool.Clear();
            if (kingdom.units != null)
            {
                foreach (var a in kingdom.units)
                {
                    if (a == null || !a.isAlive()) continue;
                    if (!GameHelpers.IsCivilizedActor(a)) continue;
                    pool.Add(a);
                    if (pool.Count >= max * 3) break; // 候选池足够即可
                }
            }
            if (pool.Count == 0) return 0;

            // Fisher-Yates 洗牌，随机选取受影响成员
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
            }

            int affected = 0;
            int limit = Mathf.Min(max, pool.Count);
            for (int i = 0; i < limit; i++)
            {
                try
                {
                    pool[i].addTrait("hotheaded", true);
                    affected++;
                }
                catch (System.Exception) { }
            }
            return affected;
        }
    }
}
