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

        // ===== 贸易金流（TradeSimulationWorker 后台模拟）=====
        public long TradeBalance;       // 净贸易顺差（正=出口盈余，负=逆差）

        // ===== 区域价格指数（v0.9 地理贸易）=====
        public float LocalPrice;        // 本地价格指数（全局 CPI × 本地供需系数，1.0=基准）

        // ===== 人口约束（马尔萨斯）=====
        public int Population;          // 原版人口（Kingdom.getPopulationTotal）
    }
}
