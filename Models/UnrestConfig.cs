using UnityEngine;

namespace EconomyMod.Models
{
    /// <summary>
    /// 社会动荡+采集配置的运行时单例。
    /// 配置项由 NML 模组设置（default_config.json）管理：NML 在设置窗口关闭时调用
    /// <see cref="EconomyMod.Services.EconomyConfigCallbacks"/> 回调写入本单例；
    /// 模组加载时由 SyncFromModConfig() 拉取一次初始值。持久化由 NML 负责，本类不再写文件。
    /// </summary>
    public class UnrestConfig
    {
        /// <summary>是否启用自动动荡检测。</summary>
        public bool Enabled = true;

        /// <summary>是否将经济事件推送到游戏 WorldLog（默认关闭，可在模组设置开启）。</summary>
        public bool LogToWorldLog = false;

        /// <summary>基尼系数触发阈值（≥ 则判定高基尼动荡）。</summary>
        public float GiniThreshold = 0.90f;

        /// <summary>每次动荡每王国最多暴乱的城市数上限（实际数量随机，随贫富差距增大而增多）。</summary>
        public int MaxAffectedPerKingdom = 3;

        /// <summary>开局宽限期（年）：世界前 N 年不触发动荡，让经济先发展。</summary>
        public int MinUnrestStartYear = 5;

        /// <summary>Mod 界面语言："zh" 中文 / "en" English（由模组设置切换，与游戏语言解耦）。</summary>
        public string Language = "zh";

        /// <summary>是否启用国家政策（高基尼王国自动尝试贫富调节，失败则统治者退位/驾崩或陷入内战）。</summary>
        public bool PolicyEnabled = true;

        // ===== 经济周期调制器（Phase 4）=====

        /// <summary>是否启用经济周期调制（四阶段状态机 + 泡沫 + 税率调制）。</summary>
        public bool CycleEnabled = true;

        /// <summary>贫富差距危险线：基尼 ≥ 该值视为经济失衡（繁荣转衰退、衰退转萧条的判据）。</summary>
        public float CycleGiniHigh = 0.60f;

        /// <summary>贫富差距健康线：基尼 ≤ 该值视为财富均等（萧条/复苏转繁荣的判据）。</summary>
        public float CycleGiniLow = 0.40f;

        /// <summary>基尼越线持续期数：连续 N 期满足条件才转移阶段（防抖，决定阶段过渡节奏）。</summary>
        public int CycleGiniPeriods = 2;

        /// <summary>繁荣期每期注入硬币占 GDP 比例（信用扩张）。</summary>
        public float BoomStimulusRatio = 0.03f;

        /// <summary>繁荣期泡沫累积系数（注入量 × 系数计入泡沫值）。</summary>
        public float BoomBubbleFactor = 0.15f;

        /// <summary>泡沫破裂阈值：累积泡沫值超过该值自动破裂。</summary>
        public float BubbleThreshold = 5000f;

        /// <summary>繁荣期最大持续期数：超过自动转向衰退。</summary>
        public int BoomMaxDuration = 8;

        /// <summary>衰退期最大持续期数：超过自动转向萧条。</summary>
        public int RecessionMaxDuration = 5;

        /// <summary>萧条期最大持续期数：超过自动转向复苏。</summary>
        public int DepressionMaxDuration = 6;

        /// <summary>复苏期最大持续期数：超过自动转向繁荣。</summary>
        public int RecoveryMaxDuration = 4;

        /// <summary>人均财富生存线：低于该值判定经济萧条。</summary>
        public float SurvivalLine = 3.0f;

        // ===== 社会危机引擎（Phase 5）=====

        /// <summary>战争掠夺比例：胜方掠夺败方王国硬币比例。</summary>
        public float WarPlunderRatio = 0.20f;

        /// <summary>战争损耗比例：掠夺额中直接"蒸发"（战乱破坏财富、不转移给胜方）的比例。
        /// 损耗从败方富人优先扣除且不进入任何人口袋 → 总财富减少、直接降低基尼系数。</summary>
        public float WarWasteRatio = 0.50f;

        /// <summary>叛乱持续满 N 年触发革命（王国被推翻）。</summary>
        public int RevolutionDelayYears = 3;

        /// <summary>革命时击杀王国人口比例。</summary>
        public float RevolutionKillRatio = 0.30f;

        // ===== 街头起义（政权彻底崩塌）=====

        /// <summary>街头起义触发阈值：叛乱后基尼仍 ≥ 该值持续满 UprisingDelayYears 年 → 街头起义。
        /// 起义 = 全城暴动 + 杀富济贫 + 推翻国王（比普通叛乱更彻底）。</summary>
        public float UprisingGiniThreshold = 0.95f;

        /// <summary>起义延迟（年）：叛乱后基尼持续超起义阈值满 N 年触发街头起义。</summary>
        public int UprisingDelayYears = 3;

        /// <summary>杀富比例：起义/革命时处决王国最富人口的 Top 比例（Top1% 表示 0.01）。</summary>
        public float KillRichRatio = 0.05f;

        /// <summary>杀富再分配比例：被处决富人的财富按该比例分给最穷公民（其余蒸发/掉落）。</summary>
        public float KillRichRedistRatio = 0.60f;

        // ===== 年度累进税（全员再分配，直接降低全球基尼）=====

        /// <summary>是否启用年度累进税（全球超税线公民纳税，补贴贫困公民）。</summary>
        public bool WealthTaxEnabled = true;

        /// <summary>税率：对"超出 全球人均×税线 部分"的征税比例（0~0.5）。</summary>
        public float WealthTaxRatio = 0.3f;

        /// <summary>税线倍数：财富超过全球人均×该值的公民需纳税（1.0~3.0）。</summary>
        public float WealthTaxLineMult = 1.5f;

        // ===== 王国贸易金流（TradeSimulationWorker 后台模拟）=====

        /// <summary>是否启用王国贸易金流（人均财富→顺差/逆差，金币经城市仓库零和结算）。</summary>
        public bool TradeEnabled = true;

        /// <summary>贸易流动比例：每年实际流动的贸易顺差/逆差占模拟余额的比例（0~0.2）。</summary>
        public float TradeFlowRatio = 0.05f;

        // ===== 人口约束（马尔萨斯，PopulationEngine）=====

        /// <summary>是否启用人口约束（超承载→饥饿/移民压力）。</summary>
        public bool PopulationEnabled = true;

        /// <summary>人口超载阈值：人口/承载 ≥ 该值判定超载（0.5~1.0）。</summary>
        public float OvercrowdRatio = 0.9f;

        // ===== 王国时代事件（EraEngine：盛世/复兴/强盛期/经济崩溃）=====

        /// <summary>是否启用时代事件（双通道：富豪花钱 + 周期自动）。</summary>
        public bool EraEnabled = true;

        /// <summary>正面时代（盛世/复兴/强盛期）持续年数。</summary>
        public int EraDurationYears = 15;

        /// <summary>经济崩溃触发阈值：萧条期人均财富同比下滑 ≥ 该比例。</summary>
        public float CollapseDropRatio = 0.2f;

        /// <summary>经济崩溃持续年数。</summary>
        public int CollapseDurationYears = 5;

        /// <summary>强盛期自动触发：战士占比阈值（战士数/人口）。</summary>
        public float FlourishMilitaryRatio = 0.3f;

        /// <summary>强盛期防抖：连续满足条件期数。</summary>
        public int FlourishPeriods = 2;

        // ===== 劳动分工（LaborEngine）=====

        /// <summary>是否启用劳动分工（职业人口按职业生产率创造工资）。</summary>
        public bool LaborEnabled = true;

        /// <summary>基础工资：职业年工资 = 基础工资 × 职业生产率倍率（0~5）。</summary>
        public float LaborWageBase = 1.0f;

        // ===== 实时数据刷新（HUD 实时感）=====

        /// <summary>是否启用实时轻量刷新（按秒重算统计并刷新 HUD，不影响年度周期）。</summary>
        public bool RealTimeRefresh = false;

        /// <summary>实时刷新间隔（秒）：两次轻量采集之间至少相隔的秒数。</summary>
        public float RealTimeInterval = 5f;

        private static UnrestConfig _instance;

        /// <summary>全局单例。</summary>
        public static UnrestConfig Instance
        {
            get
            {
                if (_instance == null) _instance = new UnrestConfig();
                return _instance;
            }
            set { _instance = value; }
        }
    }
}
