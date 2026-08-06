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
        public List<KingdomStats> Kingdoms = new List<KingdomStats>();
    }
}
