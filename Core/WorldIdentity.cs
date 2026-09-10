using System;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 世界稳定 ID（v2.1.17 修复）：双通道持久化——
    /// ① MapStats.custom_data 的 "ce_world_id"（随存档序列化，玄门道界同款通道）；
    /// ② 王国 data 的 "rb_world_id"（与 rb_hist 同宿主、同 set/get API、同序列化路径）。
    /// 关键教训：CustomDataContainer 字段初始为 null，由官方 set() 懒创建——
    /// 直接索引 custom_data_string[key] 会 NRE 并被静默吞掉，键永远写不进存档
    /// （导致每次读档都被误判为"新世界"）。故一律走官方 set/get（null 安全）。
    /// 同一存档 = 同一世界 ID：读档读到原值，新世界无键 → 生成新值。
    /// </summary>
    public static class WorldIdentity
    {
        private const string Key = "ce_world_id";          // MapStats.custom_data 键
        private const string KingdomKey = "rb_world_id";   // 王国 data 键（第二通道）

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
        /// 当前 MapStats 对象引用（世界身份信号：读档/新世界都会替换该对象，同一世界运行中恒定）。
        /// 供 GameHelpers 的王国快照/索引缓存判定使用——防止 world/管理器引用与数量都相同
        /// 时跨世界复用旧缓存（串数据根因之一）。
        /// </summary>
        public static object GetMapStatsRef()
        {
            return GetMapStats();
        }

        /// <summary>
        /// 只读当前世界 ID：custom_data 键 → 王国 data 键 → null（不生成、不写缓存）。
        /// 有值 = 读档（ID 随存档反序列化回来）；null = 新世界（generateNewMap 里 new MapStats）
        /// 或旧版存档首次进入。
        /// </summary>
        public static string PeekWorldId()
        {
            try
            {
                var stats = GetMapStats();
                if (stats != null && stats.custom_data != null)
                {
                    string id = null;
                    try { stats.custom_data.get(Key, out id); } catch (System.Exception) { }
                    if (!string.IsNullOrEmpty(id)) return id;
                }
                return ReadKingdomWorldId();
            }
            catch (System.Exception) { return null; }
        }

        /// <summary>
        /// 读取（必要时生成并写入）当前世界 ID。
        /// 优先级：绑定通道（custom_data 键 / 王国 data 键）→ 会话缓存 → 新 UUID（双通道写入）。
        /// </summary>
        public static string GetOrCreateWorldId()
        {
            try
            {
                string id = PeekWorldId();
                if (!string.IsNullOrEmpty(id))
                {
                    _sessionId = id;
                    return id;
                }
                if (!string.IsNullOrEmpty(_sessionId)) return _sessionId; // 会话缓存
                _sessionId = Guid.NewGuid().ToString("N").Substring(0, 12);
                WriteWorldId(_sessionId);
                return _sessionId;
            }
            catch (System.Exception) { return _sessionId; }
        }

        /// <summary>
        /// 把世界 ID 写入两个持久化通道：MapStats.custom_data（官方 set，懒创建容器）
        /// 与王国 data 键（优先认领国、否则第一个王国）。保存前缀调用，确保随存档落盘。
        /// </summary>
        public static void WriteWorldId(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            try
            {
                var stats = GetMapStats();
                if (stats != null && stats.custom_data != null)
                    stats.custom_data.set(Key, id);
            }
            catch (System.Exception) { }

            try
            {
                Kingdom host = null;
                try { host = GameHelpers.FindKingdom(NationEngine._nationKingdomId); } catch (System.Exception) { }
                if (host == null || host.data == null)
                {
                    var snapshot = GameHelpers.KingdomSnapshot();
                    if (snapshot != null && snapshot.Count > 0) host = snapshot[0];
                }
                if (host != null && host.data != null) host.data.set(KingdomKey, id);
            }
            catch (System.Exception) { }
        }

        /// <summary>遍历王国 data 读取 rb_world_id（第二通道）。</summary>
        private static string ReadKingdomWorldId()
        {
            try
            {
                var snapshot = GameHelpers.KingdomSnapshot();
                if (snapshot == null) return null;
                for (int i = 0; i < snapshot.Count; i++)
                {
                    var k = snapshot[i];
                    if (k == null || k.data == null) continue;
                    string v = null;
                    try { k.data.get(KingdomKey, out v); } catch (System.Exception) { }
                    if (!string.IsNullOrEmpty(v)) return v;
                }
                return null;
            }
            catch (System.Exception) { return null; }
        }

        /// <summary>新世界/离开世界时清会话缓存（确保新世界拿到新 ID）。</summary>
        public static void ResetSession()
        {
            _sessionId = null;
        }
    }
}
