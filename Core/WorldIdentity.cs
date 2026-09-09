using System;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 世界稳定 ID（v2.1.13，按用户流程：存档有世界ID，用它开数据库键）。
    /// 实现：MapStats.custom_data（随存档持久化的字符串容器）中写入/读出 "ce_world_id"——
    /// 首次遇到某世界生成 UUID 写入，此后同局读档永远读到同一值；新世界无此键 → 新 UUID，
    /// 天然零冲突。比 MapBox.current_world_seed_id（进程内递增计数器，读档/新会话会变）
    /// 稳定得多，与"存档有ID"语义一致（同一存档=同一世界ID）。
    /// </summary>
    public static class WorldIdentity
    {
        private const string Key = "ce_world_id";

        // MapBox.map_stats 为 internal 字段（编译期不可见），以反射读取 MapStats
        private static System.Reflection.FieldInfo _mapStatsField;
        private static bool _mapStatsProbed;

        private static MapStats GetMapStats()
        {
            try
            {
                if (World.world == null) return null;
                if (!_mapStatsProbed)
                {
                    _mapStatsProbed = true;
                    _mapStatsField = typeof(MapBox).GetField("map_stats",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance);
                }
                return _mapStatsField != null ? _mapStatsField.GetValue(World.world) as MapStats : null;
            }
            catch (System.Exception) { return null; }
        }

        /// <summary>
        /// 读取（必要时生成并写入）当前世界 ID。无世界/读取失败返回 null（调用方降级）。
        /// 写入 custom_data_string 即随存档序列化：首次生成 UUID，同局读档恒同、新世界必新。
        /// </summary>
        public static string GetOrCreateWorldId()
        {
            try
            {
                var stats = GetMapStats();
                if (stats == null || stats.custom_data == null) return null;
                string id;
                if (stats.custom_data.custom_data_string.TryGetValue(Key, out id) && !string.IsNullOrEmpty(id))
                    return id;
                id = Guid.NewGuid().ToString("N").Substring(0, 12);
                try { stats.custom_data.custom_data_string[Key] = id; } catch (System.Exception) { }
                return id;
            }
            catch (System.Exception) { return null; }
        }
    }
}
