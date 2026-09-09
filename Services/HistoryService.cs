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
            catch (System.Exception) { ClearHistory(); }
        }

        // ===== v2.0.2 旁挂文件（诡秘之主-宿命之环同款方案）=====
        // 不依赖王国 data 键与读档钩子时序：保存时由 NationSave 的 saveWorldToDirectory
        // Postfix 把历史写成存档目录旁的独立文件；读档/切世界时由 EconomyModMain 跟踪
        // SaveManager.currentSavePath 变化懒加载。任何 IO 异常静默吞掉（不阻断原版）。

        /// <summary>旁挂文件名（存档目录下）。</summary>
        public const string SidecarFileName = "ClassicalEconomics_history.txt";

        /// <summary>把当前历史写进存档目录（无目录/IO 失败静默）。</summary>
        public static void SaveToFile(string saveDir)
        {
            try
            {
                if (string.IsNullOrEmpty(saveDir)) return;
                System.IO.Directory.CreateDirectory(saveDir);
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(saveDir, SidecarFileName), Serialize());
            }
            catch (System.Exception) { }
        }

        /// <summary>从存档目录读旁挂历史（文件缺失/坏文件静默跳过，保留内存现状）。</summary>
        public static void LoadFromFile(string saveDir)
        {
            try
            {
                if (string.IsNullOrEmpty(saveDir)) return;
                string path = System.IO.Path.Combine(saveDir, SidecarFileName);
                if (!System.IO.File.Exists(path)) return;
                Restore(System.IO.File.ReadAllText(path));
            }
            catch (System.Exception) { }
        }
    }
}
