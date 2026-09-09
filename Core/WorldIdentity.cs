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

        // 会话级缓存：读档缺键时生成后记下，防"同档连续读档/读档后未保存"期间每次生成新值
        private static string _sessionId;

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
        /// 读取（必要时生成并写入）当前世界 ID。
        /// 优先级：custom_data 键（随档持久）→ 会话缓存（同会话稳定）→ 新 UUID（写入两步源）。
        /// 保证同档读档/读档后未保存期间 ID 恒定；保存前缀会把它落进 custom_data 随档持久。
        /// </summary>
        public static string GetOrCreateWorldId()
        {
            try
            {
                var stats = GetMapStats();
                if (stats != null && stats.custom_data != null)
                {
                    string id;
                    if (stats.custom_data.custom_data_string.TryGetValue(Key, out id) && !string.IsNullOrEmpty(id))
                    {
                        _sessionId = id;
                        return id;
                    }
                }
                if (!string.IsNullOrEmpty(_sessionId)) return _sessionId; // 会话缓存
                _sessionId = Guid.NewGuid().ToString("N").Substring(0, 12);
                if (stats != null && stats.custom_data != null)
                {
                    try { stats.custom_data.custom_data_string[Key] = _sessionId; } catch (System.Exception) { }
                }
                return _sessionId;
            }
            catch (System.Exception) { return _sessionId; }
        }

        /// <summary>新世界/离开世界时清会话缓存（确保新世界拿到新 ID）。</summary>
        public static void ResetSession()
        {
            _sessionId = null;
        }
    }
}
