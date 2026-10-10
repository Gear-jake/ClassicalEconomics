using System;
using System.Diagnostics;
using System.Text;
using EconomyMod.Models;

namespace EconomyMod.Core
{
    /// <summary>
    /// 年度收尾性能诊断（计划任务 1，配置门控，默认关闭）：
    /// Stopwatch 记录各阶段耗时，GC.GetTotalMemory(false) 记录托管内存增量。
    /// 关闭时 BeginYear/BeginStage/EndStage/EndYear 全部立即返回，零字符串格式化。
    /// 仅记录超预算阶段（耗时超单帧预算或分配超单阶段阈值），每年结束时输出一条汇总，
    /// 汇总按 CycleAllocBudget（KB）判断整年分配是否超预算。
    /// 除 GetTotalMemory(false) 外不使用任何垃圾回收 API（不强制回收、不修改回收器设置）。
    /// </summary>
    public static class PerfDiagnostics
    {
        private const long StageAllocThresholdBytes = 262144L; // 单阶段分配阈值（256KB）
        private const long MsPerSecond = 1000L;

        private static bool _yearActive;
        private static int _year;
        private static long _yearStartTicks;
        private static long _yearStartBytes;
        private static long _yearTotalMs;
        private static long _yearTotalBytes;
        private static int _overStageCount;
        private static long _stageStartTicks;
        private static long _stageStartBytes;
        private static readonly StringBuilder _overStages = new StringBuilder(128);

        /// <summary>分阶段累计统计：阶段名 -> [累计ms, 累计字节, 次数, 单次最大ms]。
        /// 用于把"哪个阶段最慢"写进 perf-capture.json —— 只有超预算**计数**不足以定位优化点。
        /// 仅诊断开启时有内容（关闭时 EndStage 立即返回，不会触碰此字典）。</summary>
        private static readonly System.Collections.Generic.Dictionary<string, long[]> _stageStats
            = new System.Collections.Generic.Dictionary<string, long[]>(32);

        /// <summary>分阶段累计统计（供 PerfCounters 落盘）。</summary>
        public static System.Collections.Generic.Dictionary<string, long[]> StageStats => _stageStats;

        /// <summary>是否启用性能诊断（读配置门控）。</summary>
        public static bool IsEnabled => UnrestConfig.Instance.PerfDiagnosticsEnabled;

        /// <summary>开始一个年度收尾周期：记录起点时间与托管内存基数。</summary>
        public static void BeginYear(int year)
        {
            if (!IsEnabled) return;
            _year = year;
            _yearActive = true;
            _yearStartTicks = Stopwatch.GetTimestamp();
            _yearStartBytes = GC.GetTotalMemory(false);
            _yearTotalMs = 0L;
            _yearTotalBytes = 0L;
            _overStageCount = 0;
            _overStages.Length = 0;
        }

        /// <summary>开始一个阶段：记录阶段起点时间与托管内存基数。</summary>
        public static void BeginStage()
        {
            if (!IsEnabled) return;
            if (!_yearActive) return;
            _stageStartTicks = Stopwatch.GetTimestamp();
            _stageStartBytes = GC.GetTotalMemory(false);
        }

        /// <summary>结束一个阶段：累计耗时/分配增量，仅记录超预算阶段。</summary>
        public static void EndStage(string stageName)
        {
            if (!IsEnabled) return;
            if (!_yearActive) return;
            long ms = ElapsedMs(_stageStartTicks);
            long bytes = GC.GetTotalMemory(false) - _stageStartBytes;
            _yearTotalMs += ms;
            _yearTotalBytes += bytes;
            AccumulateStage(stageName, ms, bytes);
            int frameBudgetMs = UnrestConfig.Instance.FrameBudgetMs > 0
                ? UnrestConfig.Instance.FrameBudgetMs : 1;
            if (ms > frameBudgetMs || bytes > StageAllocThresholdBytes)
            {
                _overStageCount++;
                if (_overStages.Length > 0) _overStages.Append(", ");
                _overStages.Append(stageName);
                UnityEngine.Debug.LogWarning("[PerfDiagnostics] Stage " + stageName +
                    " over budget: " + ms + "ms, +" + bytes + " bytes");
            }
        }

        /// <summary>结束年度收尾：每年输出一条汇总，整年分配超 CycleAllocBudget 时标注。</summary>
        public static void EndYear()
        {
            if (!IsEnabled) return;
            if (!_yearActive) return;
            _yearActive = false;
            int allocBudgetKb = UnrestConfig.Instance.CycleAllocBudget;
            bool overAlloc = _yearTotalBytes > (long)allocBudgetKb * 1024L;
            bool overTime = _yearTotalMs > (long)UnrestConfig.Instance.CycleWindowMs;
            UnityEngine.Debug.LogWarning("[PerfDiagnostics] Year " + _year + " summary: " +
                _yearTotalMs + "ms, +" + _yearTotalBytes + " bytes" +
                (overAlloc ? ", over cycle allocation budget " + allocBudgetKb + "KB" : "") +
                (overTime ? ", over cycle time window" : "") +
                ", over-budget stages: " + _overStageCount +
                (_overStageCount > 0 ? " (" + _overStages + ")" : ""));
        }

        /// <summary>累计单个阶段的耗时/分配（字典查找 + 就地累加，无每帧分配）。</summary>
        private static void AccumulateStage(string name, long ms, long bytes)
        {
            if (string.IsNullOrEmpty(name)) return;
            long[] s;
            if (!_stageStats.TryGetValue(name, out s))
            {
                s = new long[4];
                _stageStats[name] = s;
            }
            s[0] += ms;
            s[1] += bytes;
            s[2] += 1;
            if (ms > s[3]) s[3] = ms;
        }

        private static long ElapsedMs(long startTicks)
        {
            return (Stopwatch.GetTimestamp() - startTicks) * MsPerSecond / Stopwatch.Frequency;
        }
    }
}