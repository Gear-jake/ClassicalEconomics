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

        /// <summary>特长 → 中文名称。</summary>
        private static readonly string[] SpecialtyNameZh =
        {
            "无", "矿产", "木材", "粮食", "贸易品"
        };

        /// <summary>特长 → 英文名称。</summary>
        private static readonly string[] SpecialtyNameEn =
        {
            "None", "Mining", "Wood", "Food", "Trade"
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

        /// <summary>特长产出加成系数。</summary>
        public static float GetBonus(BiomeSpecialty specialty)
        {
            int idx = (int)specialty;
            return idx < SpecialtyBonus.Length ? SpecialtyBonus[idx] : 0f;
        }

        /// <summary>特长名称（中/英文）。</summary>
        public static string GetName(BiomeSpecialty specialty, bool chinese)
        {
            int idx = (int)specialty;
            if (idx < 0 || idx >= SpecialtyNameZh.Length) idx = 0;
            return chinese ? SpecialtyNameZh[idx] : SpecialtyNameEn[idx];
        }

        /// <summary>两王国是否特长互补（不同特长 = 可贸易）。</summary>
        public static bool IsComplementary(long kingdomA, long kingdomB)
        {
            var sa = GetSpecialty(kingdomA);
            var sb = GetSpecialty(kingdomB);
            return sa != BiomeSpecialty.None && sb != BiomeSpecialty.None && sa != sb;
        }
    }
}
