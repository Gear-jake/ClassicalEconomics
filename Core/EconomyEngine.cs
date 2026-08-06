using System;
using System.Collections.Generic;
using EconomyMod.Models;

namespace EconomyMod.Core
{
    /// <summary>
    /// 经济统计结果持有器（主线程零计算）：
    /// 全部统计计算由 <see cref="TradeSimulationWorker"/> 在后台线程完成，
    /// 主线程通过 <see cref="PublishResult"/> 发布结果，本类仅存结果并提供查询。
    /// </summary>
    public static class EconomyEngine
    {
        /// <summary>全局 GDP = 所有存活 Actor 的 (coins + loot) 之和。</summary>
        public static float GlobalGDP { get; private set; }

        /// <summary>人均财富 = GlobalGDP / AliveActorCount。</summary>
        public static float AvgWealth { get; private set; }

        /// <summary>本次采集到的存活开智 Actor 数量（排除野兽动物）。</summary>
        public static int AliveActorCount { get; private set; }

        /// <summary>采集周期序号（每次周期自增）。</summary>
        public static int CycleIndex { get; private set; }

        /// <summary>基尼系数（0=完全均等，1=完全不均等）。</summary>
        public static float GiniCoefficient { get; private set; }

        /// <summary>全王国贸易出口总额（顺差之和，金流模拟量）。</summary>
        public static long TotalTradeVolume { get; private set; }

        /// <summary>按王国 ID 聚合的经济统计（id=0 表示无王国桶）。</summary>
        public static Dictionary<long, KingdomStats> KingdomStats { get; private set; } = new Dictionary<long, KingdomStats>();

        /// <summary>
        /// 重置周期序号（新地图/新游戏时调用，下一次采集从 #1 重新计数）。
        /// </summary>
        public static void ResetCycle() => CycleIndex = 0;

        /// <summary>
        /// 主线程发布后台计算结果（由 TradeSimulationWorker.TryConsume / 手动采集调用）。
        /// </summary>
        public static void PublishResult(TradeSimulationWorker.CycleResult res)
        {
            GlobalGDP = res.GlobalGDP;
            AvgWealth = res.AvgWealth;
            AliveActorCount = res.AliveActorCount;
            GiniCoefficient = res.GiniCoefficient;
            CycleIndex = res.CycleIndex;
            TotalTradeVolume = res.TotalTradeVolume;

            KingdomStats.Clear();
            foreach (var ks in res.Kingdoms)
            {
                KingdomStats[ks.KingdomId] = new KingdomStats
                {
                    KingdomId = ks.KingdomId,
                    KingdomName = ks.Name,
                    GDP = ks.GDP,
                    AvgWealth = ks.AvgWealth,
                    ActorCount = ks.ActorCount,
                    GiniCoefficient = ks.Gini,
                    Population = ks.Population,
                    PopulationCapacity = ks.Capacity,
                    FoodPerCapita = ks.FoodPerCapita,
                    TradeBalance = ks.TradeBalance,
                    Workers = ks.Workers,
                    Productivity = ks.Productivity
                };
            }
        }

        // 复用 TopKingdoms 的排序缓冲（调用方均为立即遍历消费，不跨周期持有）
        private static readonly List<KingdomStats> _topKingdomPool = new List<KingdomStats>();

        /// <summary>按 GDP 降序返回前 n 个王国统计（复用静态缓冲，调用方不可跨周期持有引用）。</summary>
        public static List<KingdomStats> TopKingdoms(int n)
        {
            var list = _topKingdomPool;
            list.Clear();
            list.AddRange(KingdomStats.Values);
            list.Sort((a, b) => b.GDP.CompareTo(a.GDP));
            if (n < 0) n = 0;
            if (n < list.Count)
                list.RemoveRange(n, list.Count - n);
            return list;
        }
    }
}
