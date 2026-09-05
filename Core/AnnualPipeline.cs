using System.Diagnostics;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 年度收尾分帧状态机（计划任务 7）：
    /// 后台周期统计消费后启动，评估/效果按帧预算推进，全部阶段完成才写快照/刷 UI。
    /// 帧预算 FrameBudgetMs（默认 4ms/帧）；整个收尾窗口 CycleWindowMs（默认 2000ms），
    /// 超窗口先延窗至 5000ms 硬上限，仍超时按 消费→银行→其他 削减（税收永不削减）。
    /// </summary>
    public enum AnnualStage
    {
        WealthTax,
        CycleModulator,
        Unrest,
        Policy,
        KingdomMonitor,
        SocialCrisis,
        Population,
        Spending,
        EraEvaluate,
        EraTick,
        Disaster,
        Banking,
        Nation,
        Events,
        Snapshot,
        Done
    }

    /// <summary>年度收尾管线：帧预算驱动的阶段状态机。</summary>
    public static class AnnualPipeline
    {
        private const long HardWindowMs = 5000L; // 窗口延窗硬上限（先延窗 ≤5s，再削减）

        private static AnnualStage _cursor = AnnualStage.Done;
        private static int _year;
        private static long _windowStartTicks;
        private static bool _windowExtended;
        private static bool _reduced;

        /// <summary>收尾是否在途（还有阶段未完成）。</summary>
        public static bool IsSettling => _cursor != AnnualStage.Done;

        /// <summary>
        /// 启动年度收尾：后台统计已消费（TryConsume 成功），从富豪税阶段开始分帧推进。
        /// </summary>
        public static void Start(int year)
        {
            _year = year;
            _cursor = AnnualStage.WealthTax;
            _windowStartTicks = Stopwatch.GetTimestamp();
            _windowExtended = false;
            _reduced = false;
            PerfDiagnostics.BeginYear(year);
        }

        /// <summary>
        /// 中止在途收尾（世界失效/年份回退）：清空状态，避免旧世界数据污染下一周期。
        /// </summary>
        public static void Abort()
        {
            _cursor = AnnualStage.Done;
            _year = 0;
            _windowStartTicks = 0L;
            _windowExtended = false;
            _reduced = false;
        }

        /// <summary>
        /// 每帧推进一次：先处理窗口超时（延窗→削减），再在帧预算内执行阶段。
        /// 快照阶段豁免帧切片：全部经济阶段完成后同帧完成快照/UI（原子收尾）。
        /// </summary>
        public static void Tick()
        {
            if (_cursor == AnnualStage.Done) return;

            var cfg = UnrestConfig.Instance;
            long totalMs = ElapsedMs(_windowStartTicks);
            // 超窗口先延窗（≤5000ms 硬上限）；仍超时按 消费→银行→其他 削减，税收永不削减
            if (totalMs > cfg.CycleWindowMs && !_windowExtended)
            {
                _windowExtended = true;
                UnityEngine.Debug.LogWarning($"[ClassicalEconomics] 年度收尾超过窗口 {cfg.CycleWindowMs}ms，已延窗至 {HardWindowMs}ms 硬上限");
            }
            if (totalMs > HardWindowMs && !_reduced)
            {
                _reduced = true;
                UnityEngine.Debug.LogWarning("[ClassicalEconomics] 年度收尾超过硬上限，按 消费→银行→其他 削减（税收永不削减）");
            }

            long frameStart = Stopwatch.GetTimestamp();
            int budgetMs = cfg.FrameBudgetMs > 0 ? cfg.FrameBudgetMs : 1;
            while (_cursor != AnnualStage.Done)
            {
                if (_cursor != AnnualStage.Snapshot && ElapsedMs(frameStart) >= budgetMs)
                    break; // 帧预算耗尽，剩余阶段下一帧继续
                PerfDiagnostics.BeginStage();
                RunStage(_cursor);
                PerfDiagnostics.EndStage(PerfDiagnostics.IsEnabled ? _cursor.ToString() : null);
                _cursor = (AnnualStage)((int)_cursor + 1);
            }
            if (_cursor == AnnualStage.Done)
            {
                PerfDiagnostics.EndYear();
            }
        }

        private static void RunStage(AnnualStage stage)
        {
            switch (stage)
            {
                case AnnualStage.WealthTax:
                    // 年度富豪税（依赖本周期全球人均，须在统计消费后）
                    DataCollector.ApplyWealthTax();
                    break;
                case AnnualStage.CycleModulator:
                    EconomyCycleModulator.Evaluate();
                    break;
                case AnnualStage.Unrest:
                    UnrestEngine.Evaluate();
                    break;
                case AnnualStage.Policy:
                    PolicyEngine.Evaluate(); // 高基尼王国尝试贫富调节政策（失败则统治者退位/死亡）
                    break;
                case AnnualStage.KingdomMonitor:
                    KingdomMonitorEngine.Evaluate(); // 王位继承监测（新王即位事件）
                    break;
                case AnnualStage.SocialCrisis:
                    SocialCrisisEngine.Evaluate();
                    break;
                case AnnualStage.Population:
                    PopulationEngine.Evaluate();
                    break;
                case AnnualStage.Spending:
                    if (!_reduced) SpendingEngine.RunOncePerYear(); // 超预算兜底削减：先砍消费
                    break;
                case AnnualStage.EraEvaluate:
                    // 时代事件：先自动评估触发（含花钱触发的状态），再同步国民特质与到期移除
                    EraEngine.Evaluate();
                    break;
                case AnnualStage.EraTick:
                    EraEngine.Tick(_year);
                    break;
                case AnnualStage.Disaster:
                    // 灾害经济冲击：检测城市人口骤降，施加财富蒸发（火山矿产加成）
                    DisasterEngine.Evaluate();
                    break;
                case AnnualStage.Banking:
                    if (!_reduced) BankingEngine.Evaluate(); // 超预算兜底削减：再砍银行
                    break;
                case AnnualStage.Nation:
                    // 中央银行家：金库税负 + 持续政策 + 政绩记录回填（财政路径，不参与削减）
                    NationEngine.RunAnnual(_year);
                    // 法典：全王国法律/国策年度演变（AI 国自动；玩家国 B3 接建议）＋乘数聚合
                    LawEngine.RunAnnual(_year);
                    break;
                case AnnualStage.Events:
                    // 抉择事件：到期结算 + 条件抽签（O(K×E)，无分配；弹窗延后到快照尾）
                    DecisionEvents.EvaluateYear(_year);
                    break;
                case AnnualStage.Snapshot:
                    // 全部经济阶段完成后才写快照/刷 UI（唯一完成钩子）
                    EconomyMod.EconomyModMain.WriteCycleSnapshot(_year);
                    break;
            }
        }

        private static long ElapsedMs(long startTicks)
        {
            return (Stopwatch.GetTimestamp() - startTicks) * 1000L / Stopwatch.Frequency;
        }
    }
}