using System.Collections.Generic;
using System.Reflection;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>消费方式类型（统一分派与回退，新增消费只需加枚举值 + 实现 Try* 方法）。</summary>
    public enum SpendKind
    {
        BuyWeapon, BuildInvestment, CraftArsenal,
        WholesaleWeapons, EraEvent, Charity, PayTax
    }

    /// <summary>
    /// 智慧生物消费系统：每年让富裕的智慧生物大量消费金币，缓解"硬币过多"。
    /// 金币不凭空消失——全部真实转移给城市领袖（购买武器 / 缴纳市税）
    /// 或同城最穷的智慧生物（慈善施舍），并让生物获得真实回报：
    /// 购买武器通过反射调用 ActorData.tryToCraftRandomWeapon 获得真实武器。
    /// </summary>
    public static class SpendingEngine
    {
        /// <summary>富裕判定阈值：金币超过该值的智慧生物才会消费（调低以扩大消费覆盖面）。</summary>
        public const int WealthyThreshold = 40;

        private static readonly System.Random _rng = new System.Random();
        private static MethodInfo _craftWeaponMethod;
        private static BuildingAsset _towerAsset;

        // ===== 性能优化：复用消费缓冲，避免 GC 分配 =====
        private static readonly List<Actor> _cityPool = new List<Actor>();

        /// <summary>每年调用一次：富裕智慧生物按财富比例消费。
        /// 富人列表由 DataCollector.Collect 在单遍遍历中顺带收集（WealthyPool），
        /// 本方法不再全量扫描，仅打乱 + 逐个消费。</summary>
        public static void RunOncePerYear()
        {
            if (World.world == null) return;
            var actors = DataCollector.WealthyPool;
            if (actors.Count == 0) return;

            // 打乱富人列表，避免每轮都优先处理同一批
            for (int i = actors.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var tmp = actors[i]; actors[i] = actors[j]; actors[j] = tmp;
            }

            bool log = UnrestConfig.Instance.LogToWorldLog;
            foreach (var actor in actors)
            {
                int money;
                try { money = Mathf.Max(0, Mathf.RoundToInt(actor.money)); }
                catch (System.Exception) { continue; } // 半销毁对象可能读取失败
                if (money <= WealthyThreshold) continue;

                // 花掉超出阈值部分的三分之二（扩大个人消费），单次上限提高到 400（支撑大额消费如武器批发）
                int spend = Mathf.Clamp((money - WealthyThreshold) * 2 / 3, 10, 400);

                // 七类消费按情境权重选择（战时多买武器、和平多投资、高基尼多施舍）
                SpendKind kind = PickSpendKindByContext(actor);
                bool spent = false;
                string note = "";
                spent = kind switch
                {
                    SpendKind.BuyWeapon         => TryBuyWeapon(actor, spend, out note),
                    SpendKind.BuildInvestment   => TryBuildInvestment(actor, spend, out note),
                    SpendKind.CraftArsenal      => TryCraftArsenal(actor, spend, out note),
                    SpendKind.WholesaleWeapons  => TryWholesaleWeapons(actor, spend, out note),
                    SpendKind.EraEvent           => TryEraEvent(actor, spend, out note),
                    SpendKind.Charity            => TryCharity(actor, spend, out note),
                    _                           => TryPayTax(actor, spend, out note),
                };
                if (!spent && kind != SpendKind.PayTax)
                {
                    spent = TryPayTax(actor, spend, out note);
                }

                if (spent && log)
                {
                    Debug.Log($"[ClassicalEconomics] 消费 {GameHelpers.SafeName(actor)} 花费={spend} {note}");
                }
            }
        }

        /// <summary>
        /// 按情境权重选择消费方式：
        /// 战争中王国→武器类权重×3；高基尼(>0.7)→慈善×2；繁荣期→建造投资×2；
        /// 萧条期→慈善×1.5、缴税减半；其他默认权重1。
        /// </summary>
        private static SpendKind PickSpendKindByContext(Actor actor)
        {
            // 默认权重
            float wWeapon = 1f, wBuild = 1f, wCraft = 1f, wWholesale = 1f;
            float wEra = 1f, wCharity = 1f, wTax = 1f;

            // 王国情境
            Kingdom kingdom = null;
            float kingdomGini = 0f;
            bool atWar = false;
            try
            {
                kingdom = actor.kingdom;
                if (kingdom != null)
                {
                    // curKing 为游戏 internal 属性，编译期不可见，用反射安全读取
                    atWar = IsKingdomAtWar(kingdom);
                }
            }
            catch { }

            // 经济阶段情境
            var phase = EconomyCycleModulator.CurrentPhase;

            // 战时：武器类权重 ×3
            if (atWar)
            {
                wWeapon *= 3f; wCraft *= 2f; wWholesale *= 2f;
            }

            // 高基尼：慈善施舍 ×2
            if (EconomyEngine.KingdomStats.TryGetValue(SafeKingdomId(kingdom), out var ks))
                kingdomGini = ks.GiniCoefficient;
            if (kingdomGini > 0.7f) wCharity *= 2f;

            // 繁荣期：建造投资 ×2
            if (phase == EconomyPhase.Boom) wBuild *= 2f;

            // 萧条期：慈善 ×1.5，缴税减半（穷人已穷，少收税）
            if (phase == EconomyPhase.Depression) { wCharity *= 1.5f; wTax *= 0.5f; }

            // 按权重随机选择
            float total = wWeapon + wBuild + wCraft + wWholesale + wEra + wCharity + wTax;
            float r = (float)_rng.NextDouble() * total;
            if ((r -= wWeapon) < 0) return SpendKind.BuyWeapon;
            if ((r -= wBuild) < 0) return SpendKind.BuildInvestment;
            if ((r -= wCraft) < 0) return SpendKind.CraftArsenal;
            if ((r -= wWholesale) < 0) return SpendKind.WholesaleWeapons;
            if ((r -= wEra) < 0) return SpendKind.EraEvent;
            if ((r -= wCharity) < 0) return SpendKind.Charity;
            return SpendKind.PayTax;
        }

        private static long SafeKingdomId(Kingdom k)
        {
            if (k == null || k.data == null) return 0L;
            try { return k.data.id; } catch { return 0L; }
        }

        // ===== 王国战争状态检测（curKing/hasNegativeStatus 为游戏 internal，反射访问）=====

        private static System.Reflection.PropertyInfo _curKingProp;
        private static System.Reflection.PropertyInfo _personProp;
        private static System.Reflection.MethodInfo _hasNegativeStatusMethod;

        /// <summary>反射检测王国是否处于战争状态（国王有"war"负面状态）；失败返回 false。</summary>
        private static bool IsKingdomAtWar(Kingdom kingdom)
        {
            try
            {
                if (_curKingProp == null)
                {
                    _curKingProp = typeof(Kingdom).GetProperty("curKing",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                }
                if (_curKingProp == null) return false;
                var curKing = _curKingProp.GetValue(kingdom);
                if (curKing == null) return false;

                if (_personProp == null)
                {
                    _personProp = curKing.GetType().GetProperty("person",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                }
                if (_personProp == null) return false;
                var person = _personProp.GetValue(curKing);
                if (person == null) return false;

                if (_hasNegativeStatusMethod == null)
                {
                    _hasNegativeStatusMethod = person.GetType().GetMethod("hasNegativeStatus",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                }
                if (_hasNegativeStatusMethod == null) return false;
                return (bool)_hasNegativeStatusMethod.Invoke(person, new object[] { "war" });
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>购买武器：反射造真实武器，金币转给城市领袖（军械收入）。</summary>
        private static bool TryBuyWeapon(Actor actor, int spend, out string note)
        {
            note = "购买武器";
            if (!CraftRandomWeapon(actor)) return false;
            if (!TransferToCity(actor, spend)) return false;
            note = "购买武器（军械入城）";
            return true;
        }

        /// <summary>缴纳市税：金币转给城市领袖。</summary>
        private static bool TryPayTax(Actor actor, int spend, out string note)
        {
            note = "缴纳市税";
            if (!TransferToCity(actor, spend)) return false;
            note = "缴纳市税（市政基金）";
            return true;
        }

        /// <summary>慈善施舍：金币转给同城最穷的智慧生物。</summary>
        private static bool TryCharity(Actor actor, int spend, out string note)
        {
            note = "慈善施舍";
            var target = FindPoorestCityActor(actor);
            long aId = SafeId(actor);
            long tId = SafeId(target);
            if (target == null || tId == 0L || tId == aId) return false;
            if (!DeductAndGive(actor, target, spend)) return false;
            note = "慈善施舍（接济穷人）";
            return true;
        }

        /// <summary>建造投资：放置瞭望塔建筑；混合流向 50% 消耗（材料费）+ 50% 转移城主。</summary>
        private static bool TryBuildInvestment(Actor actor, int spend, out string note)
        {
            note = "建造投资";
            if (actor.city == null || actor.current_tile == null) return false;
            spend = Mathf.Clamp(spend, 40, 200);
            try
            {
                if (_towerAsset == null) _towerAsset = AssetManager.buildings.get("watch_tower_human");
                if (_towerAsset == null) return false;
                // addBuilding 为 internal 方法，用反射调用
                Building b = AddBuildingViaReflection(_towerAsset, actor.current_tile);
                if (b == null) return false;
                if (actor.kingdom != null) b.setKingdom(actor.kingdom);
                // 混合流向：一半转移城主（材料人工），一半消耗（材料损耗）
                int toCity = spend / 2;
                if (!TransferToCity(actor, toCity)) return false;
                actor.addMoney(-(spend - toCity));
                EventStreamService.Record(EventStreamService.TypeBuildInv, GameHelpers.SafeKingdomName(actor.kingdom), spend);
                note = "建造投资（矗立瞭望塔）";
                return true;
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>打造军械：批量锻造 3-5 件武器；30% 转城主军械费 + 70% 锻造消耗。</summary>
        private static bool TryCraftArsenal(Actor actor, int spend, out string note)
        {
            note = "打造军械";
            if (actor.city == null) return false;
            spend = Mathf.Clamp(spend, 60, 300);
            int craftCount = Mathf.Clamp(spend / 30, 3, 5);
            int success = 0;
            for (int i = 0; i < craftCount; i++)
            {
                if (CraftRandomWeapon(actor)) success++;
            }
            if (success == 0) return false;
            try
            {
                if (!TransferToCity(actor, Mathf.RoundToInt(spend * 0.3f))) return false;
                actor.addMoney(-Mathf.RoundToInt(spend * 0.7f));
            }
            catch (System.Exception) { return false; }
            EventStreamService.Record(EventStreamService.TypeCraftArsenal, GameHelpers.SafeKingdomName(actor.kingdom), success);
            note = "打造军械（" + success + "件装备入城）";
            return true;
        }

        /// <summary>武器批发：大量锻造 6-10 件武器；20% 转城主 + 80% 锻造消耗。</summary>
        private static bool TryWholesaleWeapons(Actor actor, int spend, out string note)
        {
            note = "武器批发";
            if (actor.city == null) return false;
            spend = Mathf.Clamp(spend, 120, 500);
            int craftCount = Mathf.Clamp(spend / 40, 6, 10);
            int success = 0;
            for (int i = 0; i < craftCount; i++)
            {
                if (CraftRandomWeapon(actor)) success++;
            }
            if (success == 0) return false;
            try
            {
                if (!TransferToCity(actor, Mathf.RoundToInt(spend * 0.2f))) return false;
                actor.addMoney(-Mathf.RoundToInt(spend * 0.8f));
            }
            catch (System.Exception) { return false; }
            EventStreamService.Record(EventStreamService.TypeWholesale, GameHelpers.SafeKingdomName(actor.kingdom), success);
            note = "武器批发（" + success + "件装备入城）";
            return true;
        }

        /// <summary>时代事件赞助：按当前周期阶段选择盛世/复兴/强盛期，花钱为王国开启时代特质（国民加成）。</summary>
        private static bool TryEraEvent(Actor actor, int spend, out string note)
        {
            note = "时代赞助";
            Kingdom kingdom = actor.kingdom;
            if (kingdom == null) return false;
            // 按阶段选择可赞助的时代（萧条/衰退或条件不足返回 null → 回退缴税）
            string kingdomTrait = EraEngine.PickSpendEra(kingdom);
            if (kingdomTrait == null) return false;
            if (kingdom.hasTrait(kingdomTrait)) return false;
            spend = Mathf.Clamp(spend, 50, 250);
            try
            {
                actor.addMoney(-spend);
                EraEngine.Start(kingdom, kingdomTrait, EconomyModMain.GetCurrentGameYear());
                note = "时代赞助（" + EraEngine.EventName(kingdomTrait) + "，国民加成生效）";
                return true;
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>生物扣款、接收者收款（真实转移）。</summary>
        private static bool DeductAndGive(Actor payer, Actor receiver, int amount)
        {
            try
            {
                payer.addMoney(-amount);
                receiver.addMoney(amount);
                return true;
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>生物扣款，城市领袖收款；无城市则失败。</summary>
        private static bool TransferToCity(Actor actor, int amount)
        {
            Actor leader = null;
            try
            {
                var city = actor.city;
                leader = city != null ? city.leader : null;
            }
            catch (System.Exception) { return false; }
            if (leader == null || leader.id == actor.id) return false;
            return DeductAndGive(actor, leader, amount);
        }

        /// <summary>寻找同城金币最少的智慧生物（使用复用缓冲）。</summary>
        private static Actor FindPoorestCityActor(Actor actor)
        {
            var candidates = _cityPool;
            candidates.Clear();
            try
            {
                var city = actor.city;
                if (city == null || city.units == null) return null;
                candidates.AddRange(city.units);
            }
            catch (System.Exception) { return null; }

            Actor poorest = null;
            float minMoney = float.MaxValue;
            foreach (var c in candidates)
            {
                if (c == null || c.id == actor.id) continue;
                if (c.asset == null || !c.asset.civ) continue;
                try
                {
                    if (c.money < minMoney) { minMoney = c.money; poorest = c; }
                }
                catch (System.Exception) { }
            }
            return poorest;
        }

        /// <summary>反射调用 ActorData.tryToCraftRandomWeapon 制造真实武器。</summary>
        private static bool CraftRandomWeapon(Actor actor)
        {
            try
            {
                var d = GameHelpers.GetActorData(actor);
                if (d == null) return false;
                if (_craftWeaponMethod == null)
                {
                    _craftWeaponMethod = d.GetType().GetMethod("tryToCraftRandomWeapon",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
                if (_craftWeaponMethod == null) return false;
                _craftWeaponMethod.Invoke(d, null);
                return true;
            }
            catch (System.Exception) { return false; }
        }

        private static MethodInfo _addBuildingMethod;

        /// <summary>反射调用 internal BuildingManager.addBuilding(BuildingAsset, WorldTile, bool) 放置建筑。</summary>
        private static Building AddBuildingViaReflection(BuildingAsset asset, WorldTile tile)
        {
            try
            {
                if (_addBuildingMethod == null)
                {
                    _addBuildingMethod = typeof(BuildingManager).GetMethod("addBuilding",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                }
                if (_addBuildingMethod == null) return null;
                return _addBuildingMethod.Invoke(World.world.buildings,
                    new object[] { asset, tile, false }) as Building;
            }
            catch (System.Exception) { return null; }
        }

        private static long SafeId(Actor a)
        {
            if (a == null) return 0L;
            try { return a.id; }
            catch (System.Exception) { return 0L; }
        }
    }
}
