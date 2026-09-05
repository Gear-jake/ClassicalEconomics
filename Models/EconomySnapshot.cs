using System.Collections.Generic;

namespace EconomyMod.Models
{
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
    }
}
