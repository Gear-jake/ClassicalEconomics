using System.Collections.Generic;
using EconomyMod.Models;

namespace EconomyMod.Services
{
    /// <summary>
    /// 历史快照内存层：环形数组缓冲（固定 ≤ 容量）。
    /// 每次启动都会 ClearHistory（历史仅本局有效），因此不落盘——
    /// 消除文件 IO 与后台线程，避免 Unity 主线程 GC 停顿。
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
    }
}
