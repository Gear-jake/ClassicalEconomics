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

        // ===== 人口约束（马尔萨斯）=====
        public int Population;          // 原版人口（Kingdom.getPopulationTotal）
        public int PopulationCapacity;  // 原版承载（Kingdom.getPopulationTotalPossible）
        public float FoodPerCapita;     // 人均食物

        // ===== 劳动分工 =====
        public int Workers;             // 有职业人口
        public float Productivity;      // 平均劳动生产率（职业倍率均值）
    }
}
