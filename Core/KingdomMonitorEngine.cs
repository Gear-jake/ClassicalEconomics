using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 王位继承监测：每年比较各王国的在位国王（kingdom.king）变化，
    /// 检测到新王即位时记录"王位继承"事件（原版国王死亡、改革失败退位后
    /// 自动产生的新王均会触发）。首次观察到某王国时只建立基线，不产生事件。
    /// </summary>
    public static class KingdomMonitorEngine
    {
        /// <summary>王国跟踪记录：KingId=最近一次观察到的国王 id（0=无王）。字典 key 存在即已建立基线。</summary>
        private struct KingTrack
        {
            public long KingId;
        }

        private static readonly Dictionary<long, KingTrack> _known = new Dictionary<long, KingTrack>();

        // ===== 性能优化：复用每年扫描缓冲，避免 GC 分配 =====
        private static readonly HashSet<long> _seen = new HashSet<long>();
        private static readonly List<long> _removeIds = new List<long>();

        /// <summary>世界重置（新地图/新游戏）时清空记录。</summary>
        public static void Reset()
        {
            _known.Clear();
        }

        /// <summary>
        /// 每年在 FinishCycle 中调用：快照所有王国，比较国王变化。
        /// 变化规则：
        /// - 首次观察到（未建基线）→ 仅记录，不发事件（避免"开国国王"误判为继承）；
        /// - 已有基线且现任国王 id 变化（含 无王→新王、旧王→新王）→ 记录"王位继承"事件；
        /// - 新王→无王（国王离任）→ 不记录（退位场景已由改革失败事件覆盖）。
        /// </summary>
        public static void Evaluate()
        {
            if (World.world == null || World.world.kingdoms == null) return;

            var kingdomList = GameHelpers.KingdomSnapshot();
            var seen = _seen;
            seen.Clear();

            foreach (var kingdom in kingdomList)
            {
                if (kingdom == null || kingdom.data == null) continue;
                long kid = kingdom.data.id;
                seen.Add(kid);

                long currentKingId = 0;
                var king = kingdom.king;
                if (king != null)
                {
                    try { currentKingId = king.id; }
                    catch (System.Exception) { currentKingId = 0; } // 半销毁对象读取 id 可能抛异常
                }

                bool firstObserve = !_known.TryGetValue(kid, out var track);
                if (!firstObserve && currentKingId != 0 && currentKingId != track.KingId)
                {
                    // 王位更迭：新王即位（含无王空窗后新王产生）
                    string kName = GameHelpers.SafeKingdomName(kingdom);
                    EventStreamService.Record(EventStreamService.TypeKingInherit, kName, 0);
                    if (UnrestConfig.Instance.LogToWorldLog)
                    {
                        Debug.Log($"[ClassicalEconomics] 王位继承 王国<{kName}> 新王即位");
                    }
                }
                _known[kid] = new KingTrack { KingId = currentKingId };
            }

            // 清理已消失王国的记录
            var removeIds = _removeIds;
            removeIds.Clear();
            foreach (var kv in _known)
                if (!seen.Contains(kv.Key)) removeIds.Add(kv.Key);
            foreach (var id in removeIds) _known.Remove(id);
        }
    }
}
