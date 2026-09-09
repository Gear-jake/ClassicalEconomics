using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EconomyMod.Models;

namespace EconomyMod.Services
{
    /// <summary>
    /// 历史快照内存层：环形数组缓冲（固定 ≤ 容量）。
    /// 每次启动都会 ClearHistory（历史仅本局有效），因此不落盘——
    /// 消除文件 IO 与后台线程，避免 Unity 主线程 GC 停顿。
    /// （v1.3.0：新增 Serialize/Restore——GDP 折线历史随王国存档键 rb_hist 落盘，
    /// 读档后恢复折线图并消除“读另一存档串档”。）
    /// </summary>
    public static class HistoryService
    {
        // 只保留最近 50 期（图表/份额窗均只消费最近 50 条），超限覆盖最老条目即"删除"，
        // 内存固定 ≤ 50 × EconomySnapshot，杜绝无界增长。
        private const int Capacity = 50;

        // 环形数组缓冲：_head 指向下一个写入位置，_count 为当前条数（≤Capacity）
        private static readonly EconomySnapshot[] _buffer = new EconomySnapshot[Capacity];
        private static int _head;
        private static int _count;

        // 复用的 GetRecent 输出缓冲（调用方立即消费，不跨周期持有）
        private static readonly List<EconomySnapshot> _recentPool = new List<EconomySnapshot>(Capacity);

        /// <summary>追加一条快照到环形缓冲，O(1)。</summary>
        public static void AppendSnapshot(EconomySnapshot snapshot)
        {
            _buffer[_head] = snapshot;
            _head = (_head + 1) % Capacity;
            if (_count < Capacity) _count++;
        }

        /// <summary>
        /// 取最近 count 条快照（按时间正序）。返回复用缓冲，调用方不可跨周期持有引用。
        /// </summary>
        public static List<EconomySnapshot> GetRecent(int count)
        {
            var result = _recentPool;
            result.Clear();
            int take = count > _count ? _count : count;
            if (take <= 0) return result;

            // 环形读取：取最近 take 条，起点 = 最近 take 条中的最老位置（_head - take）。
            // 注意不能用 (_head - _count) —— 那是最老条目，会在数据超 take 后永远返回最老的 take 条
            // 导致图表"卡住"不再滚动（v0.11 修复）。
            int start = (_head - take + Capacity) % Capacity;
            for (int i = 0; i < take; i++)
            {
                int idx = (start + i) % Capacity;
                result.Add(_buffer[idx]);
            }
            return result;
        }

        public static void ClearHistory()
        {
            // 环形清空：真实条目位于 (start+i)%Capacity，按线性下标置空会残留旧快照引用
            int start = (_head - _count + Capacity) % Capacity;
            for (int i = 0; i < _count; i++) _buffer[(start + i) % Capacity] = null;
            _head = 0;
            _count = 0;
            _recentPool.Clear();
        }

        /// <summary>
        /// 序列化最近 50 条快照为紧凑字符串（按时间正序）：
        /// "year:gdp:avg:gini:phase:cpi;..."，float 用 InvariantCulture 防文化小数点差异。
        /// </summary>
        public static string Serialize()
        {
            var sb = new StringBuilder(1024);
            int start = (_head - _count + Capacity) % Capacity;
            var inv = CultureInfo.InvariantCulture;
            for (int i = 0; i < _count; i++)
            {
                var s = _buffer[(start + i) % Capacity];
                if (s == null) continue;
                sb.Append(s.GameYear).Append(':')
                  .Append(s.GlobalGDP).Append(':')
                  .Append(s.AvgWealth.ToString("F2", inv)).Append(':')
                  .Append(s.GiniCoefficient.ToString("F4", inv)).Append(':')
                  .Append(s.Phase).Append(':')
                  .Append(s.PriceIndex.ToString("F3", inv)).Append(';');
            }
            return sb.ToString();
        }

        /// <summary>从 Serialize 生成的字符串恢复环形缓冲（解析失败条目跳过；空串=清空）。</summary>
        public static void Restore(string data)
        {
            ClearHistory();
            if (string.IsNullOrEmpty(data)) return;
            var inv = CultureInfo.InvariantCulture;
            try
            {
                string[] entries = data.Split(';');
                for (int i = 0; i < entries.Length; i++)
                {
                    if (string.IsNullOrEmpty(entries[i])) continue;
                    string[] f = entries[i].Split(':');
                    if (f.Length < 6) continue;
                    var snap = new EconomySnapshot();
                    int iv;
                    long lv;
                    float fv;
                    if (!int.TryParse(f[0], out iv)) continue;
                    snap.GameYear = iv;
                    if (!long.TryParse(f[1], out lv)) continue;
                    snap.GlobalGDP = lv;
                    if (!float.TryParse(f[2], NumberStyles.Float, inv, out fv)) continue;
                    snap.AvgWealth = fv;
                    if (!float.TryParse(f[3], NumberStyles.Float, inv, out fv)) continue;
                    snap.GiniCoefficient = fv;
                    if (!int.TryParse(f[4], out iv)) continue;
                    snap.Phase = iv;
                    if (!float.TryParse(f[5], NumberStyles.Float, inv, out fv)) continue;
                    snap.PriceIndex = fv;
                    AppendSnapshot(snap);
                }
            }
            catch (System.Exception e)
            {
                ClearHistory();
                UnityEngine.Debug.LogWarning("[ClassicalEconomics] 旁挂历史恢复解析失败: " + e.Message
                    + " (bytes=" + (data != null ? data.Length : 0) + ")");
            }
        }

        // ===== v2.1.12 世界 id 历史库（与存档目录解耦，同 EventStreamService）=====
        // <persistentDataPath>\ClassicalEconomicsWorlds\history_<seed>.txt

        /// <summary>历史库目录名。</summary>
        public const string WorldStoreDirName = "ClassicalEconomicsWorlds";

        /// <summary>历史文件前缀（实际文件 history_&lt;seed&gt;.txt）。</summary>
        public const string WorldStoreFilePrefix = "history_";

        /// <summary>历史文件后缀。</summary>
        public const string WorldStoreFileSuffix = ".txt";

        private static string WorldStoreDir()
        {
            return System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, WorldStoreDirName);
        }

        /// <summary>按世界 id 写历史库（IO 失败静默）。</summary>
        public static void SaveToWorldStore(int seed)
        {
            try
            {
                if (seed <= 0) return;
                string dir = WorldStoreDir();
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(dir, WorldStoreFilePrefix + seed + WorldStoreFileSuffix),
                    Serialize());
            }
            catch (System.Exception) { }
        }

        /// <summary>按世界 id 读历史库；文件缺失返回 false（调用方按新世界从零处理）。</summary>
        public static bool LoadFromWorldStore(int seed)
        {
            try
            {
                if (seed <= 0) return false;
                string path = System.IO.Path.Combine(
                    WorldStoreDir(), WorldStoreFilePrefix + seed + WorldStoreFileSuffix);
                if (!System.IO.File.Exists(path)) return false;
                Restore(System.IO.File.ReadAllText(path));
                return true;
            }
            catch (System.Exception) { return false; }
        }

        // ===== v2.0.2 旧旁挂（保留：v2.1.12 前存档目录内 txt 仍可读，新逻辑不再写）=====
        // 不依赖王国 data 键与读档钩子时序：保存时由 NationSave 的 saveWorldToDirectory
        // Postfix 把历史写成存档目录旁的独立文件；读档/切世界时由 EconomyModMain 跟踪
        // SaveManager.currentSavePath 变化懒加载。任何 IO 异常静默吞掉（不阻断原版）。

        /// <summary>旁挂文件名（存档目录下）。</summary>
        public const string SidecarFileName = "ClassicalEconomics_history.txt";

        /// <summary>旁挂世界身份头（首行）：`#CE_SEED <seed>`——恢复时校验世界种子，防跨档串数据。</summary>
        private const string SidecarSeedPrefix = "#CE_SEED ";

        /// <summary>把当前历史写进存档目录（无目录/IO 失败静默）。</summary>
        public static void SaveToFile(string saveDir)
        {
            try
            {
                if (string.IsNullOrEmpty(saveDir)) return;
                System.IO.Directory.CreateDirectory(saveDir);
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(saveDir, SidecarFileName),
                    SidecarSeedPrefix + EconomyMod.Core.GameHelpers.ReadWorldSeed() + "\n" + Serialize());
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// 从存档目录读旁挂历史：校验世界种子（`#CE_SEED <n>` 首行）与当前世界一致才恢复；
        /// 文件缺失/种子不符/旧格式（无头）静默跳过，保留内存现状——防跨档串数据（v2.1.3）。
        /// </summary>
        public static void LoadFromFile(string saveDir)
        {
            try
            {
                if (string.IsNullOrEmpty(saveDir)) return;
                string path = System.IO.Path.Combine(saveDir, SidecarFileName);
                if (!System.IO.File.Exists(path)) return;
                string content = System.IO.File.ReadAllText(path);
                int nl = content.IndexOf('\n');
                string head = nl >= 0 ? content.Substring(0, nl) : content;
                if (!head.StartsWith(SidecarSeedPrefix, System.StringComparison.Ordinal)) return; // 旧格式无头
                int seed;
                if (!int.TryParse(head.Substring(SidecarSeedPrefix.Length), out seed)) return;
                if (seed != EconomyMod.Core.GameHelpers.ReadWorldSeed()) return; // 世界不符：拒绝恢复
                Restore(nl >= 0 ? content.Substring(nl + 1) : "");
            }
            catch (System.Exception) { }
        }
    }
}
