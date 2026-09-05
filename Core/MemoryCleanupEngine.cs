using System.Collections.Generic;
using System.Diagnostics;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.Core
{
    /// <summary>
    /// 自动内存清理引擎：按配置间隔（默认 30 秒）在游戏空闲时对静态 scratch/缓存集合
    /// 执行缩容（List TrimExcess + 字典重建换引用，只缩容量、绝不清数据），并可选择
    /// 触发一次 System.GC.Collect（全项目唯一 GC 入口）。仅在
    /// !TradeSimulationWorker.IsBusy() && !AnnualPipeline.IsSettling 时执行，
    /// 保证清理绝不与在途周期/后台计算并发。
    ///
    /// 结果可见性（三通道）：
    /// 1. 顶部横幅 GameHelpers.Notify（memory_cleanup_notify_enabled 门控，且仅在
    ///    估算释放 ≥0.5 MB 或执行了强制 GC 时弹出，无事发生不扰民）；
    /// 2. 经济面板概览的内存状态行（LastCleanupRealtime/LastFreedBytes/ManagedHeapBytes/
    ///    UnityUsedBytes/UnityReservedBytes，把"模组托管堆"与"游戏本体 Unity 内存"分开显示）；
    /// 3. 每次清理一条 Debug.Log（进 player.log 便于事后排查）。
    /// </summary>
    public static class MemoryCleanupEngine
    {
        /// <summary>字典重建缩容的最小条目数（与 List 缩容的容量门槛一致，避免小额重建 garbage）。</summary>
        private const int MinCompactEntries = 4096;
        /// <summary>估算释放达到该字节数（0.5 MB）才弹顶部横幅。</summary>
        private const long MinToastBytes = 512 * 1024;
        /// <summary>清理间隔到达但系统忙碌时的短延迟重试（秒），不把机会推迟到完整间隔之后。</summary>
        private const double BusyRetryDelaySeconds = 5;

        private static long _lastCleanupTimestamp = Stopwatch.GetTimestamp();

        /// <summary>上次清理的托管堆释放字节数（强制 GC 时为精确值，否则为估算，可能为负）。</summary>
        public static long LastFreedBytes { get; private set; }

        /// <summary>上次清理收缩的缓冲/字典个数。</summary>
        public static int LastShrunkCount { get; private set; }

        /// <summary>上次清理发生时的 Time.realtimeSinceStartup（秒）；尚未清理过为 -1。</summary>
        public static float LastCleanupRealtime { get; private set; } = -1f;

        /// <summary>当前托管堆字节数（Mono GC 口径；模组与游戏本体共享同一托管堆）。</summary>
        public static long ManagedHeapBytes => System.GC.GetTotalMemory(false);

        /// <summary>Unity 已分配内存（原生资源：贴图/网格/音频等；读取失败为 -1）。</summary>
        public static long UnityUsedBytes
        {
            get { try { return UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(); } catch (System.Exception) { return -1; } }
        }

        /// <summary>Unity 保留内存（已向系统申请的总量，任务管理器数字的主体；读取失败为 -1）。</summary>
        public static long UnityReservedBytes
        {
            get { try { return UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong(); } catch (System.Exception) { return -1; } }
        }

        /// <summary>字节数格式化为 MB（一位小数，负值原样显示供日志诊断）。</summary>
        public static string FormatMb(long bytes)
        {
            return (bytes / 1048576d).ToString("F1");
        }

        /// <summary>每帧调用；到达清理间隔且系统空闲时执行一次内存清理。</summary>
        public static void Tick(float deltaTime)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.MemoryCleanupEnabled) return;

            long now = Stopwatch.GetTimestamp();
            double elapsedSeconds = (double)(now - _lastCleanupTimestamp) / Stopwatch.Frequency;
            if (elapsedSeconds < cfg.MemoryCleanupIntervalSeconds) return;

            if (AnnualPipeline.IsSettling || TradeSimulationWorker.IsBusy())
            {
                // 忙碌时按短延迟顺延重试（默认 5 秒），而不是把这次机会推迟到完整间隔之后
                double retryDelay = System.Math.Max(0d, cfg.MemoryCleanupIntervalSeconds - BusyRetryDelaySeconds);
                _lastCleanupTimestamp = now - (long)(retryDelay * Stopwatch.Frequency);
                return;
            }
            _lastCleanupTimestamp = now;

            long heapBefore = System.GC.GetTotalMemory(false);
            int shrunk = 0;

            try { shrunk += TradeSimulationWorker.TrimMemory(); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("[ClassicalEconomics] 贸易模拟缓冲缩容失败: " + e.Message); }

            try { shrunk += DataCollector.TrimMemory(); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("[ClassicalEconomics] 数据采集缓冲缩容失败: " + e.Message); }

            try { shrunk += InheritanceEngine.TrimMemory(); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("[ClassicalEconomics] 遗产继承缓冲缩容失败: " + e.Message); }

            try { shrunk += CompactTradeDictionaries(); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("[ClassicalEconomics] 贸易模拟字典缩容失败: " + e.Message); }

            try { shrunk += CompactCollectorAndTrackerDictionaries(); }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("[ClassicalEconomics] 采集/追踪字典缩容失败: " + e.Message); }

            bool forced = cfg.MemoryCleanupForceGc;
            // 全项目唯一 GC 入口：本行受 performance_audit 10a allowlist 约束
            // （行内必须引用 MemoryCleanupForceGc），不得拆分到局部变量后再调用。
            if (cfg.MemoryCleanupForceGc) System.GC.Collect();

            long heapAfter = System.GC.GetTotalMemory(false);
            LastFreedBytes = heapBefore - heapAfter;
            LastShrunkCount = shrunk;
            LastCleanupRealtime = UnityEngine.Time.realtimeSinceStartup;

            UnityEngine.Debug.Log(
                $"[ClassicalEconomics] 自动内存清理：收缩 {shrunk} 个缓冲，托管堆 " +
                $"{FormatMb(heapBefore)}→{FormatMb(heapAfter)} MB（{(forced ? "精确" : "估算")}释放 {FormatMb(LastFreedBytes)} MB），" +
                $"Unity 已用 {FormatMb(UnityUsedBytes)} MB / 保留 {FormatMb(UnityReservedBytes)} MB");

            // 横幅：通知开关门控 + 有意义才弹（估算释放 ≥0.5 MB，或执行了强制 GC）。
            long freedForToast = System.Math.Max(0L, LastFreedBytes);
            if (cfg.MemoryCleanupNotifyEnabled && (freedForToast >= MinToastBytes || forced))
            {
                GameHelpers.Notify(string.Format(LocalizationService.Get("memory_cleanup_toast"),
                    FormatMb(freedForToast), shrunk));
            }
        }

        // ===== 字典重建缩容（.NET Framework 无 Dictionary.TrimExcess；重建+换引用，
        // ===== 保留全部内容与语义；仅在空闲期调用，绝不与周期/后台计算并发）=====

        /// <summary>TradeSimulationWorker 的静态字典：_accScratch（统计聚合 scratch）。</summary>
        private static int CompactTradeDictionaries()
        {
            int n = 0;
            n += CompactDict(TradeSimulationWorker.AccScratchForTrim, TradeSimulationWorker.ReplaceAccScratchForTrim);
            return n;
        }

        /// <summary>DamageTracker 的 3 个字典。</summary>
        private static int CompactCollectorAndTrackerDictionaries()
        {
            int n = 0;
            n += CompactDict(DamageTracker.DamageForTrim, DamageTracker.ReplaceDamageForTrim);
            n += CompactDict(DamageTracker.PrevHealthForTrim, DamageTracker.ReplacePrevHealthForTrim);
            n += CompactDict(DamageTracker.InactiveScansForTrim, DamageTracker.ReplaceInactiveScansForTrim);
            return n;
        }

        /// <summary>条目数达到门槛时重建紧凑字典并换回；返回 1 表示发生了重建。</summary>
        private static int CompactDict<K, V>(Dictionary<K, V> source, System.Action<Dictionary<K, V>> replace)
        {
            if (source == null || source.Count < MinCompactEntries) return 0;
            var compact = new Dictionary<K, V>(source.Count);
            foreach (var kv in source) compact[kv.Key] = kv.Value;
            replace(compact);
            return 1;
        }
    }
}
