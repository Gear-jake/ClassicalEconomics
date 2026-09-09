using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 波次 0 性能采集台（W0-B）：帧耗时分位数 + 热点计数 + JSON 落盘 + 黄金行为哈希。
    ///
    /// 设计约束（与优化方案的硬约束一致）：
    /// 1. 复用 PerfDiagnostics.IsEnabled 同一个配置开关（perf_diagnostics_enabled），**零新增配置项**；
    /// 2. 关闭时每个入口只做一次布尔判断即返回，零字符串格式化、零分配、零 Stopwatch 读取；
    /// 3. 只用 GC.GetTotalMemory(false) 采样托管内存，从不强制回收，也从不修改回收器设置；
    /// 4. 本文件独立于 PerfDiagnostics.cs，避免破坏 Test-PerfDiagnostics 的三处 mutation 锚点。
    ///
    /// 落盘位置：模组程序集所在目录（WorldBox/Mods/EconomyMod/），文件名 perf-capture.json 与 perf-parity.json。
    /// </summary>
    public static class PerfCounters
    {
        private const int FrameRingCapacity = 1024;
        private const string CaptureFileName = "perf-capture.json";
        private const string ParityFileName = "perf-parity.json";

        private static readonly float[] _frameMs = new float[FrameRingCapacity];
        private static readonly float[] _frameSort = new float[FrameRingCapacity];
        private static readonly StringBuilder _sb = new StringBuilder(2048);

        private static int _frameHead;
        private static int _frameCount;

        private static long _year;
        private static long _yearStartTicks;
        private static long _yearStartBytes;

        private static long _findEquipTargetCalls;
        private static long _findEquipTargetUnits;
        private static long _fullActorScans;
        private static long _fullActorScanUnits;
        private static long _uiGoCreated;
        private static long _uiGoDestroyed;
        private static long _finishCycleCalls;
        private static long _yearTotalMs;
        private static long _yearTotalBytes;
        private static int _overBudgetStages;

        /// <summary>是否启用（与 PerfDiagnostics 共用开关，默认关闭）。</summary>
        public static bool IsEnabled => PerfDiagnostics.IsEnabled;

        /// <summary>年度采集开始：清零全部计数器并重置帧环形缓冲。</summary>
        public static void BeginYear(int year)
        {
            if (!IsEnabled) return;
            _year = year;
            _yearStartTicks = Stopwatch.GetTimestamp();
            _yearStartBytes = GC.GetTotalMemory(false);
            _frameHead = 0;
            _frameCount = 0;
            _findEquipTargetCalls = 0L;
            _findEquipTargetUnits = 0L;
            _fullActorScans = 0L;
            _fullActorScanUnits = 0L;
            _uiGoCreated = 0L;
            _uiGoDestroyed = 0L;
            _finishCycleCalls = 0L;
            _overBudgetStages = 0;
        }

        /// <summary>年度采集结束：写入 perf-capture.json 与 perf-parity.json（各一次/年）。</summary>
        public static void EndYear()
        {
            if (!IsEnabled) return;
            _yearTotalMs = ElapsedMs(_yearStartTicks);
            _yearTotalBytes = GC.GetTotalMemory(false) - _yearStartBytes;
            WriteCapture();
            WriteParity();
        }

        /// <summary>每帧采样一次帧耗时（毫秒）。关闭时仅一次布尔判断。</summary>
        public static void SampleFrame(float deltaTime)
        {
            if (!IsEnabled) return;
            float ms = deltaTime * 1000f;
            _frameMs[_frameHead] = ms;
            _frameHead = (_frameHead + 1) % FrameRingCapacity;
            if (_frameCount < FrameRingCapacity) _frameCount++;
        }

        /// <summary>记录一次装备目标寻址（W2-B 验收：调用次数与累计扫描单位数）。</summary>
        public static void AddFindEquipTarget(int unitsScanned)
        {
            if (!IsEnabled) return;
            _findEquipTargetCalls++;
            _findEquipTargetUnits += unitsScanned;
        }

        /// <summary>记录一次全量 actor 扫描（W2-A 验收：每年应从 6~8 次降到 1 次）。</summary>
        public static void MarkFullActorScan(int units)
        {
            if (!IsEnabled) return;
            _fullActorScans++;
            _fullActorScanUnits += units;
        }

        /// <summary>记录 UI 创建的对象数（W4 验收：应降到 0）。</summary>
        public static void AddUiCreated(int count)
        {
            if (!IsEnabled) return;
            _uiGoCreated += count;
        }

        /// <summary>记录 UI 销毁的对象数（W4 验收：应降到 0）。</summary>
        public static void AddUiDestroyed(int count)
        {
            if (!IsEnabled) return;
            _uiGoDestroyed += count;
        }

        /// <summary>记录年度收尾完成次数（W3-A 验收：必须恰好 1 次/年）。</summary>
        public static void MarkFinishCycle()
        {
            if (!IsEnabled) return;
            _finishCycleCalls++;
        }

        /// <summary>
        /// 阶段结束：阶段耗时超过帧预算时累计（W2-E 验收）。
        /// 由 AnnualPipeline 传入阶段起点与当帧预算，避免在 PerfDiagnostics 内部追加逻辑。
        /// </summary>
        public static void EndStage(long startTicks, int budgetMs)
        {
            if (!IsEnabled) return;
            if (ElapsedMs(startTicks) > budgetMs) _overBudgetStages++;
        }

        // ===== 落盘 =====

        private static void WriteCapture()
        {
            try
            {
                float p50 = 0f, p95 = 0f, max = 0f;
                ComputeFrameStats(out p50, out p95, out max);
                _sb.Length = 0;
                _sb.Append("{\n  \"Schema\": \"perf-capture/1\",\n");
                AppendLong("Year", _year, ",");
                AppendLong("YearTotalMs", _yearTotalMs, ",");
                AppendLong("YearTotalBytes", _yearTotalBytes, ",");
                AppendInt("OverBudgetStages", _overBudgetStages, ",");
                AppendLong("FrameCount", _frameCount, ",");
                AppendFloat("FrameP50Ms", p50, ",");
                AppendFloat("FrameP95Ms", p95, ",");
                AppendFloat("FrameMaxMs", max, ",");
                AppendLong("FindEquipTargetCalls", _findEquipTargetCalls, ",");
                AppendLong("FindEquipTargetUnits", _findEquipTargetUnits, ",");
                AppendLong("FullActorScans", _fullActorScans, ",");
                AppendLong("FullActorScanUnits", _fullActorScanUnits, ",");
                AppendLong("UiGoCreated", _uiGoCreated, ",");
                AppendLong("UiGoDestroyed", _uiGoDestroyed, ",");
                AppendLong("FinishCycleCalls", _finishCycleCalls, "");
                _sb.Append("\n}\n");
                Write(OutputPath(CaptureFileName), _sb.ToString());
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[PerfCounters] capture write failed: " + e.Message);
            }
        }

        /// <summary>
        /// 黄金行为哈希（W0-C）：固定存档 + 固定配置跑固定年数后，本文件可作为行为等价的判据。
        /// 整数项（金币总量、各类计数）必须逐位相等；浮点项按容差比较。
        /// </summary>
        private static void WriteParity()
        {
            try
            {
                long moneyTotal = 0L;
                long actorCount = 0L;
                long civilizedCount = 0L;
                long traitGolden = 0L, traitRevival = 0L, traitFlourish = 0L, traitCollapse = 0L;
                long traitEdu = 0L, traitWelfare = 0L, traitMil = 0L, traitAusterity = 0L;

                var aliveList = World.world != null && World.world.units != null
                    ? World.world.units.units_only_alive : null;
                if (aliveList != null)
                {
                    foreach (var a in aliveList)
                    {
                        if (a == null || !a.isAlive()) continue;
                        float w;
                        if (!GameHelpers.TryGetWealth(a, out w)) continue;
                        civilizedCount++;
                        moneyTotal += (long)Mathf.RoundToInt(w);
                    }
                    actorCount = aliveList.Count;
                    foreach (var a in aliveList)
                    {
                        if (a == null || !a.isAlive()) continue;
                        if (!GameHelpers.IsCivilizedActor(a)) continue;
                        if (HasTraitSafe(a, EraEngine.ActorTraitGolden)) traitGolden++;
                        if (HasTraitSafe(a, EraEngine.ActorTraitRevival)) traitRevival++;
                        if (HasTraitSafe(a, EraEngine.ActorTraitFlourish)) traitFlourish++;
                        if (HasTraitSafe(a, EraEngine.ActorTraitCollapse)) traitCollapse++;
                        if (HasTraitSafe(a, LawEngine.LawTraitEdu)) traitEdu++;
                        if (HasTraitSafe(a, LawEngine.LawTraitWelfare)) traitWelfare++;
                        if (HasTraitSafe(a, LawEngine.LawTraitMil)) traitMil++;
                        if (HasTraitSafe(a, LawEngine.LawTraitAusterity)) traitAusterity++;
                    }
                }

                _sb.Length = 0;
                _sb.Append("{\n  \"Schema\": \"perf-parity/1\",\n");
                AppendLong("Year", _year, ",");
                AppendLong("CycleIndex", EconomyEngine.CycleIndex, ",");
                AppendLong("MoneyTotal", moneyTotal, ",");
                AppendLong("ActorCount", actorCount, ",");
                AppendLong("CivilizedCount", civilizedCount, ",");
                AppendLong("TraitGolden", traitGolden, ",");
                AppendLong("TraitRevival", traitRevival, ",");
                AppendLong("TraitFlourish", traitFlourish, ",");
                AppendLong("TraitCollapse", traitCollapse, ",");
                AppendLong("TraitEdu", traitEdu, ",");
                AppendLong("TraitWelfare", traitWelfare, ",");
                AppendLong("TraitMil", traitMil, ",");
                AppendLong("TraitAusterity", traitAusterity, ",");
                AppendFloat("GlobalGDP", EconomyEngine.GlobalGDP, ",");
                AppendFloat("AvgWealth", EconomyEngine.AvgWealth, ",");
                AppendFloat("GiniCoefficient", EconomyEngine.GiniCoefficient, "");
                _sb.Append("\n}\n");
                Write(OutputPath(ParityFileName), _sb.ToString());
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[PerfCounters] parity write failed: " + e.Message);
            }
        }

        private static bool HasTraitSafe(Actor a, string traitId)
        {
            try { return a.hasTrait(traitId); }
            catch (System.Exception) { return false; }
        }

        private static void ComputeFrameStats(out float p50, out float p95, out float max)
        {
            p50 = 0f; p95 = 0f; max = 0f;
            int n = _frameCount;
            if (n <= 0) return;
            for (int i = 0; i < n; i++) _frameSort[i] = _frameMs[i];
            Array.Sort(_frameSort, 0, n);
            max = _frameSort[n - 1];
            p50 = _frameSort[Mathf.Clamp((int)(n * 0.50f), 0, n - 1)];
            p95 = _frameSort[Mathf.Clamp((int)(n * 0.95f), 0, n - 1)];
        }

        private static void AppendLong(string key, long value, string suffix)
        {
            _sb.Append("  \"").Append(key).Append("\": ")
               .Append(value.ToString(CultureInfo.InvariantCulture)).Append(suffix).Append('\n');
        }

        private static void AppendInt(string key, int value, string suffix)
        {
            _sb.Append("  \"").Append(key).Append("\": ")
               .Append(value.ToString(CultureInfo.InvariantCulture)).Append(suffix).Append('\n');
        }

        private static void AppendFloat(string key, float value, string suffix)
        {
            _sb.Append("  \"").Append(key).Append("\": ")
               .Append(value.ToString("F4", CultureInfo.InvariantCulture)).Append(suffix).Append('\n');
        }

        private static void Write(string path, string text)
        {
            System.IO.File.WriteAllText(path, text);
            UnityEngine.Debug.LogWarning("[PerfCounters] wrote " + path);
        }

        private static string OutputPath(string fileName)
        {
            string dir = null;
            try
            {
                var asm = typeof(PerfCounters).Assembly;
                if (asm != null && !string.IsNullOrEmpty(asm.Location))
                    dir = System.IO.Path.GetDirectoryName(asm.Location);
            }
            catch (System.Exception) { }
            if (string.IsNullOrEmpty(dir)) dir = ".";
            return System.IO.Path.Combine(dir, fileName);
        }

        private static long ElapsedMs(long startTicks)
        {
            return (Stopwatch.GetTimestamp() - startTicks) * 1000L / Stopwatch.Frequency;
        }
    }
}
