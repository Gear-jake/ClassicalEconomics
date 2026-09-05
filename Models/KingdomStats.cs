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

        // ===== 区域价格指数（统计展示）=====
        public float LocalPrice;        // 本地价格指数（全局 CPI × 本地供需系数，1.0=基准）

        // ===== 人口约束（马尔萨斯）=====
        public int Population;          // 原版人口（Kingdom.getPopulationTotal）
    }
}
