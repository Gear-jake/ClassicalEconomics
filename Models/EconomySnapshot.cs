using System.Collections.Generic;

namespace EconomyMod.Models
{
    /// <summary>
    /// 单个实体（城市 / 国家）的本周期净贸易额：Export − Import。
    /// 国家值 = 其所有城市（村镇）贸易额之和（同国城市对贸易在国家层面互相抵消，净额不变）。
    /// 存真实名字（主线程采集时解析，避免渲染时再查 Unity 对象）。
    /// </summary>
    [System.Serializable]
    public struct TradeBalance
    {
        public long Id;          // 城市 id（((long)x<<32)|(uint)y）或王国 id
        public string Name;      // 城市名 / 王国名
        public long Export;      // 本周期总出口额
        public long Import;      // 本周期总进口额
        public long Net;         // 净额 = Export − Import（顺差&gt;0，逆差&lt;0）
    }

    [System.Serializable]
    public class EconomySnapshot
    {
        public long CycleIndex;
        public int GameYear;
        public long GlobalGDP;
        public float AvgWealth;
        public int AliveActorCount;
        public float GiniCoefficient;
        public int Phase; // 采集时的经济周期阶段（EconomyPhase 枚举值，供趋势图阶段色带）
        public float TotalProduction; // 全球年总产出（生产函数）
        public float PriceIndex; // 价格指数 CPI（货币供给 / 总产出×流通速度）
        public List<KingdomStats> Kingdoms = new List<KingdomStats>();

        // ===== 贸易净额排名（v0.13：城市 + 国家）=====
        public long TotalExport;                                    // 全图出口总额（各边出口之和）
        public List<TradeBalance> CityBalances = new List<TradeBalance>();    // 按城市净额
        public List<TradeBalance> KingdomBalances = new List<TradeBalance>(); // 按国家净额
    }
}
