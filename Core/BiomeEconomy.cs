using System;
using System.Collections.Generic;
using System.Reflection;
using NeoModLoader.api.attributes;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 生物群系经济特长：每个王国根据其领地特征分配一种产出特长，
    /// 驱动比较优势贸易（特长王国出口，非特长王国进口）。
    /// M4：优先读取王国首都城市的真实 biome tile 类型（沙漠→矿产/森林→木材/草原→粮食/雪原→贸易品），
    /// 读取失败时回退为基于王国 ID 的确定性哈希分配。
    /// 结果缓存（王国特长变化极小，避免每周期反射开销）。
    /// </summary>
    public enum BiomeSpecialty
    {
        None = 0,    // 无特长
        Mining = 1,  // 矿产（沙漠）
        Wood = 2,    // 木材（森林）
        Food = 3,    // 粮食（草原）
        Trade = 4    // 贸易品（雪原/沿海）
    }

    public static class BiomeEconomy
    {
        /// <summary>特长 → 产出加成系数。</summary>
        private static readonly float[] SpecialtyBonus =
        {
            0f,    // None
            0.08f, // Mining
            0.07f, // Wood
            0.06f, // Food
            0.10f  // Trade
        };

        // 王国特长缓存（kingdomId → specialty；王国领土变化小，缓存避免每周期反射）
        private static readonly Dictionary<long, BiomeSpecialty> _cache = new Dictionary<long, BiomeSpecialty>(32);

        /// <summary>
        /// 获取王国产出特长：优先读取首都城市 tile 的真实 biome，失败回退 ID 哈希。
        /// </summary>
        [Hotfixable]
        public static BiomeSpecialty GetSpecialty(long kingdomId)
        {
            if (kingdomId == 0) return BiomeSpecialty.None;
            if (_cache.TryGetValue(kingdomId, out var cached)) return cached;

            var specialty = ReadCityBiome(kingdomId);
            if (specialty == BiomeSpecialty.None)
            {
                // 回退：确定性哈希分配
                int hash = (int)((kingdomId * 2654435761L) >> 32) & 0x7FFFFFFF;
                specialty = (BiomeSpecialty)(1 + (hash % 4));
            }
            _cache[kingdomId] = specialty;
            return specialty;
        }

        /// <summary>王国特长缓存失效（新地图/读档时调用）。</summary>
        public static void ClearCache()
        {
            _cache.Clear();
            _coordCache.Clear();
        }

        /// <summary>
        /// 读取王国首都城市的 biome tile 类型映射为特长。
        /// 通过反射获取 City.tile（WorldTile）的 type 字段，失败返回 None。
        /// </summary>
        private static BiomeSpecialty ReadCityBiome(long kingdomId)
        {
            try
            {
                var kingdom = GameHelpers.FindKingdom(kingdomId);
                if (kingdom == null) return BiomeSpecialty.None;
                var cities = kingdom.getCities();
                if (cities == null) return BiomeSpecialty.None;
                foreach (var city in cities)
                {
                    if (city == null) continue;
                    var biome = GetCityTileBiome(city);
                    if (biome != BiomeSpecialty.None) return biome;
                }
            }
            catch (System.Exception) { }
            return BiomeSpecialty.None;
        }

        private static PropertyInfo _cityTileProp;
        private static FieldInfo _tileTypeField;

        /// <summary>反射读取 City.tile.type → BiomeSpecialty；失败返回 None。</summary>
        private static BiomeSpecialty GetCityTileBiome(City city)
        {
            try
            {
                if (_cityTileProp == null)
                {
                    _cityTileProp = typeof(City).GetProperty("tile",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
                if (_cityTileProp == null) return BiomeSpecialty.None;
                var tile = _cityTileProp.GetValue(city);
                if (tile == null) return BiomeSpecialty.None;

                if (_tileTypeField == null)
                {
                    _tileTypeField = tile.GetType().GetField("type",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
                if (_tileTypeField == null) return BiomeSpecialty.None;
                string type = _tileTypeField.GetValue(tile) as string;
                if (string.IsNullOrEmpty(type)) return BiomeSpecialty.None;

                switch (type)
                {
                    case "desert":  return BiomeSpecialty.Mining; // 沙漠 → 矿产
                    case "forest":  return BiomeSpecialty.Wood;   // 森林 → 木材
                    case "grass":   return BiomeSpecialty.Food;   // 草原 → 粮食
                    case "snow":    return BiomeSpecialty.Trade;  // 雪原 → 贸易品
                    default:        return BiomeSpecialty.None;
                }
            }
            catch (System.Exception) { return BiomeSpecialty.None; }
        }

        // ===== 首都城市坐标（v0.9 地理贸易距离）=====
        // 坐标字段名在公开文档中不可考，采用多候选字段名动态探测（x/y/tileX/posX/gridX 等），
        // 每 tile 类型只解析一次并缓存 FieldInfo；int/float 字段均兼容。

        private static FieldInfo _tileXField;
        private static FieldInfo _tileYField;
        private static Type _tileCoordFieldsFor; // 已解析坐标字段的 tile 类型
        private static readonly string[] _coordCandidatesX = { "x", "tileX", "posX", "gridX" };
        private static readonly string[] _coordCandidatesY = { "y", "tileY", "posY", "gridY" };

        /// <summary>首都坐标缓存（kingdomId → 城市 tile 坐标；坐标变化极小，世界重置时 ClearCache 失效）。</summary>
        private static readonly Dictionary<long, Vector2> _coordCache = new Dictionary<long, Vector2>(32);

        /// <summary>
        /// 获取王国首都城市坐标（地理贸易距离计算用）。
        /// 遍历王国城市，取首个 tile 坐标读取成功的城市；反射失败返回 false，且 x/y 置 NaN 哨兵。
        /// 成功结果缓存（避免每周期反射）；失败不缓存，下周期可重试（王国建城后即可读取）。
        /// </summary>
        public static bool TryGetCapitalCoords(long kingdomId, out float x, out float y)
        {
            x = float.NaN;
            y = float.NaN;
            if (kingdomId == 0) return false;
            if (_coordCache.TryGetValue(kingdomId, out var cached))
            {
                x = cached.x; y = cached.y;
                return !float.IsNaN(cached.x);
            }
            try
            {
                var kingdom = GameHelpers.FindKingdom(kingdomId);
                if (kingdom == null) return false;
                var cities = kingdom.getCities();
                if (cities == null) return false;
                foreach (var city in cities)
                {
                    if (city == null) continue;
                    float tx, ty;
                    if (TryGetTileCoords(city, out tx, out ty))
                    {
                        _coordCache[kingdomId] = new Vector2(tx, ty);
                        x = tx; y = ty;
                        return true;
                    }
                }
            }
            catch (System.Exception) { }
            return false;
        }

        /// <summary>尝试读取城市所在 tile 的 x/y 坐标；字段名未知，动态探测候选并缓存 FieldInfo。</summary>
        private static bool TryGetTileCoords(City city, out float x, out float y)
        {
            x = float.NaN; y = float.NaN;
            try
            {
                if (_cityTileProp == null)
                {
                    _cityTileProp = typeof(City).GetProperty("tile",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
                if (_cityTileProp == null) return false;
                var tile = _cityTileProp.GetValue(city);
                if (tile == null) return false;

                var tileType = tile.GetType();
                if (_tileCoordFieldsFor != tileType)
                {
                    _tileXField = ResolveCoordField(tileType, _coordCandidatesX);
                    _tileYField = ResolveCoordField(tileType, _coordCandidatesY);
                    _tileCoordFieldsFor = tileType;
                }
                if (_tileXField == null || _tileYField == null) return false;
                object vx = _tileXField.GetValue(tile);
                object vy = _tileYField.GetValue(tile);
                if (vx == null || vy == null) return false;
                x = System.Convert.ToSingle(vx);
                y = System.Convert.ToSingle(vy);
                return true;
            }
            catch (System.Exception) { return false; }
        }

        private static FieldInfo ResolveCoordField(Type tileType, string[] candidates)
        {
            foreach (var name in candidates)
            {
                var fi = tileType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null) return fi;
            }
            return null;
        }

        /// <summary>特长产出加成系数。</summary>
        public static float GetBonus(BiomeSpecialty specialty)
        {
            int idx = (int)specialty;
            return idx < SpecialtyBonus.Length ? SpecialtyBonus[idx] : 0f;
        }
    }
}
