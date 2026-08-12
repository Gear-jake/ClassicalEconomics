using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EconomyMod.Models;
using EconomyMod.Services;
using NeoModLoader.api.attributes;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 灾害经济冲击引擎（E2 增强版）：
    /// 检测三类灾害信号——(A) 王国人口骤降 >30% (B) 王国城市数骤降 >25% (C) 城市 tile 为灾害类型（火山/陨石坑），
    /// 对受灾王国的城市施加经济冲击——城市仓库财富按 DisasterWealthLoss 蒸发。
    /// 火山短期提振矿产（繁荣期受灾区域产出额外加成）。
    /// 效果检测 + tile 反射检测，无需 hook 原版灾害事件，兼容所有灾害类型。
    /// </summary>
    public static class DisasterEngine
    {
        // 上周期王国人口快照（kingdomId → population）
        private static readonly Dictionary<long, int> _prevKingdomPop = new Dictionary<long, int>(32);

        // 上周期王国城市数快照（kingdomId → cities）
        private static readonly Dictionary<long, int> _prevKingdomCities = new Dictionary<long, int>(32);

        /// <summary>本期受灾城市数（供快照/UI读取）。</summary>
        public static int LastDisasterCityCount { get; private set; }

        /// <summary>本期灾害蒸发的财富总量。</summary>
        public static long LastWealthLost { get; private set; }

        /// <summary>世界重置（新地图/新游戏）时清空快照。</summary>
        public static void Reset()
        {
            _prevKingdomPop.Clear();
            _prevKingdomCities.Clear();
            LastDisasterCityCount = 0;
            LastWealthLost = 0;
        }

        /// <summary>
        /// 每个采集周期调用一次：遍历所有王国，检测三类灾害信号（人口骤降/城市数骤降/灾害tile），
        /// 对受灾王国的城市施加财富蒸发冲击。繁荣期受灾区域获得矿产产出加成（模拟火山矿产）。
        /// </summary>
        [Hotfixable]
        public static void Evaluate()
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.DisasterEnabled) return;

            LastDisasterCityCount = 0;
            LastWealthLost = 0;

            var kingdoms = World.world?.kingdoms;
            if (kingdoms == null) return;

            bool isBoom = EconomyCycleModulator.CurrentPhase == EconomyPhase.Boom;
            float lossRatio = cfg.DisasterWealthLoss;
            float mineBonus = cfg.DisasterMineBonus;

            foreach (var k in kingdoms)
            {
                if (k == null || k.data == null) continue;
                long kingdomId = k.data.id;
                int curPop;
                int curCities = 0;
                try { curPop = k.getPopulationTotal(); } catch { continue; }
                try
                {
                    var cs = k.getCities();
                    if (cs != null) curCities = cs.Count();
                }
                catch { }

                // 三类灾害信号
                bool popCrash = _prevKingdomPop.TryGetValue(kingdomId, out int prevPop)
                                && prevPop > 10 && curPop < prevPop * 0.7f;        // A: 人口骤降>30%
                bool cityCrash = _prevKingdomCities.TryGetValue(kingdomId, out int prevCities)
                                 && prevCities > 1 && curCities < prevCities * 0.75f; // B: 城市数骤降>25%
                bool hazardTile = HasHazardTile(k);                                  // C: 灾害tile

                if (popCrash || cityCrash || hazardTile)
                {
                    // 灾害冲击：遍历该王国城市，蒸发城市仓库财富
                    var cities = k.getCities();
                    if (cities != null)
                    {
                        foreach (var city in cities)
                        {
                            if (city == null) continue;
                            int gold;
                            try { gold = city.getResourcesAmount("gold"); } catch { gold = 0; }
                            int loss = Mathf.RoundToInt(gold * lossRatio);
                            if (loss > 0)
                            {
                                try { city.takeResource("gold", loss); } catch { }
                                LastWealthLost += loss;
                                LastDisasterCityCount++;

                                // 繁荣期受灾区域矿产产出加成（火山矿产刺激）
                                if (isBoom && mineBonus > 0f)
                                {
                                    int bonus = Mathf.Max(1, Mathf.RoundToInt(loss * mineBonus));
                                    try { city.addResourcesToRandomStockpile("gold", bonus); } catch { }
                                }
                            }
                        }
                    }
                }
                _prevKingdomPop[kingdomId] = curPop;
                _prevKingdomCities[kingdomId] = curCities;
            }

            if (LastDisasterCityCount > 0)
            {
                GameHelpers.Log($"[ClassicalEconomics] 灾害经济冲击：{LastDisasterCityCount}座城市受灾，财富蒸发{LastWealthLost}金币" +
                                 (isBoom ? $"，火山矿产刺激+{Mathf.RoundToInt(LastWealthLost * mineBonus)}金币" : ""));
                GameHelpers.Notify($"[经济] 灾害冲击！{LastDisasterCityCount}座城市受灾，财富蒸发{LastWealthLost}金币");
                EventStreamService.Record(EventStreamService.TypeDisaster, "", LastDisasterCityCount);
            }
        }

        // ===== 灾害 tile 检测（反射读取 City.tile.type，火山/陨石坑判定为灾害）=====

        private static PropertyInfo _cityTileProp;
        private static FieldInfo _tileTypeField;

        /// <summary>检测王国任意城市是否位于灾害 tile（火山/陨石坑）。失败返回 false（不误报）。</summary>
        private static bool HasHazardTile(Kingdom kingdom)
        {
            try
            {
                var cities = kingdom.getCities();
                if (cities == null) return false;
                foreach (var city in cities)
                {
                    if (city == null) continue;
                    if (_cityTileProp == null)
                    {
                        _cityTileProp = typeof(City).GetProperty("tile",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    }
                    if (_cityTileProp == null) return false;
                    var tile = _cityTileProp.GetValue(city);
                    if (tile == null) continue;
                    if (_tileTypeField == null)
                    {
                        _tileTypeField = tile.GetType().GetField("type",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    }
                    if (_tileTypeField == null) return false;
                    // 字段类型可能是 string 或 enum：`as string` 在非 string 时恒 null → 检测静默失效，
                    // 统一经 ToString 取值（enum 得到枚举名，string 得到原文）。
                    object typeObj = _tileTypeField.GetValue(tile);
                    string type = typeObj == null ? null : typeObj.ToString();
                    if (string.IsNullOrEmpty(type)) continue;
                    // 灾害/资源地形：火山、陨石坑、燃烧地面
                    if (type.Contains("volcano") || type.Contains("meteor") || type.Contains("burn"))
                        return true;
                }
            }
            catch (System.Exception) { }
            return false;
        }
    }
}
