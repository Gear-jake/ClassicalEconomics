namespace EconomyMod.Models
{
    [System.Serializable]
    public class KingdomStats
    {
        public long KingdomId;
        public string KingdomName;
        public long GDP;
        public float AvgWealth;
        public int ActorCount;
        public float GiniCoefficient;

        // ===== 人口约束（马尔萨斯）=====
        public int Population;          // 原版人口（Kingdom.getPopulationTotal）
    }
}
