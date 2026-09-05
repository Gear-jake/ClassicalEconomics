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
    /// 购买/打造/批发武器：v1.4.1 起不再调用原版锻造 API，而是按装备强度分层
    /// （弱/中/强，按 equipment_value 在同类装备内分位）直接凭空生成游戏内已有
    /// 的武器/盔甲/头盔/靴子/戒指/护符并给本国单位穿戴——强度越高，概率越低、
    /// 金钱需求越高（弱 30 / 中 80 / 强 200），消费越多可购件数越多。
    /// </summary>
    public static class SpendingEngine
    {
        /// <summary>富裕判定阈值：金币超过该值的智慧生物才会消费（调低以扩大消费覆盖面）。</summary>
        public const int WealthyThreshold = 40;

        private static readonly System.Random _rng = new System.Random();
        private static BuildingAsset _towerAsset;

        // ===== 性能优化：复用消费缓冲，避免 GC 分配 =====
        private static readonly List<Actor> _cityPool = new List<Actor>();
        private static readonly Dictionary<long, bool> _kingdomWarCache = new Dictionary<long, bool>();
        private static readonly Dictionary<City, Actor> _poorestCityActorCache = new Dictionary<City, Actor>();
        private const int BuildCooldownCycles = 50;
        private static readonly Dictionary<long, int> _lastBuildCycle = new Dictionary<long, int>();
        private static readonly List<long> _expiredCityIds = new List<long>();

        /// <summary>每年调用一次：富裕智慧生物按财富比例消费。
        /// 富人列表由 DataCollector.Collect 在单遍遍历中顺带收集（WealthyPool），
        /// 本方法不再全量扫描，仅打乱 + 逐个消费。</summary>
        public static void RunOncePerYear()
        {
            ClearWorldReferences();
            PruneExpiredCityActions(EconomyEngine.CycleIndex);
            try
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
                // 年度消费操作上限：每年最多处理 cap 个富裕生物（默认 5000，与现状同量级，可配置调低限流）
                int cap = UnrestConfig.Instance.SpendingCapPerYear;
                int processed = 0;
                foreach (var actor in actors)
                {
                    if (++processed > cap) break;
                    int money;
                    try { money = Mathf.Max(0, Mathf.RoundToInt(actor.money)); }
                    catch (System.Exception) { continue; } // 半销毁对象可能读取失败
                    if (money <= WealthyThreshold) continue;

                    // 花掉超出阈值部分的三分之二（扩大个人消费），单次上限提高到 400（支撑大额消费如武器批发）
                    int spend = Mathf.Clamp((money - WealthyThreshold) * 2 / 3, 10, 400);
                    // 法典：消费乘数（低税/庆典放大，紧缩/赎金外交收缩）
                    float consumeMult = actor.kingdom != null && actor.kingdom.data != null
                        ? LawEngine.GetMods(actor.kingdom.data.id).Consumer : 1f;
                    if (consumeMult != 1f) spend = Mathf.Clamp((int)(spend * consumeMult), 10, 600);

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
            finally
            {
                ClearWorldReferences();
            }
        }

        /// <summary>清除年度消费过程中持有的世界对象引用和查询结果。</summary>
        public static void ClearWorldReferences()
        {
            _cityPool.Clear();
            _poorestCityActorCache.Clear();
            _kingdomWarCache.Clear();
        }

        public static void Reset()
        {
            ClearWorldReferences();
            _lastBuildCycle.Clear();
            _expiredCityIds.Clear();
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
        private static readonly object[] _hasNegativeStatusArgs = { "war" };

        /// <summary>反射检测王国是否处于战争状态（国王有"war"负面状态）；失败返回 false。</summary>
        private static bool IsKingdomAtWar(Kingdom kingdom)
        {
            long kingdomId = SafeKingdomId(kingdom);
            if (_kingdomWarCache.TryGetValue(kingdomId, out bool cached)) return cached;

            bool atWar = false;
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
                atWar = (bool)_hasNegativeStatusMethod.Invoke(person, _hasNegativeStatusArgs);
            }
            catch (System.Exception) { }
            finally
            {
                _kingdomWarCache[kingdomId] = atWar;
            }
            return atWar;
        }

        /// <summary>购买武器：金币先转给城市领袖（军械收入），再按强度分层购入一件真装备
        /// （强 200 / 中 80 / 弱 30，预算越多越可能买到高档）。</summary>
        private static bool TryBuyWeapon(Actor actor, int spend, out string note)
        {
            note = "购买武器";
            if (!TransferToCity(actor, spend)) return false;
            int got = CraftTiered(actor, spend, 1);
            note = got > 0 ? "购买武器（真装备上身）" : "购买武器（军械入城）";
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
            try { if (actor.city != null) _poorestCityActorCache.Remove(actor.city); }
            catch (System.Exception) { }
            note = "慈善施舍（接济穷人）";
            return true;
        }

        /// <summary>建造投资：先扣款（50% 转城主材料人工 + 50% 材料损耗），再放置瞭望塔建筑。
        /// 先付款后动工，杜绝"免费造楼"；建楼失败不退款（投资失败，资金已投）。</summary>
        private static bool TryBuildInvestment(Actor actor, int spend, out string note)
        {
            note = "建造投资";
            if (actor.city == null || actor.current_tile == null) return false;
            spend = Mathf.Clamp(spend, 40, 200);
            try
            {
                if (_towerAsset == null) _towerAsset = AssetManager.buildings.get("watch_tower_human");
                if (_towerAsset == null) return false;
                long cityId;
                if (!CanUseCityAction(actor, _lastBuildCycle, BuildCooldownCycles, out cityId)) return false;
                int toCity = spend / 2;
                if (!TransferToCity(actor, toCity)) return false;
                actor.addMoney(-(spend - toCity));
                CommitCityAction(_lastBuildCycle, cityId);
                // addBuilding 为 internal 方法，用反射调用
                Building b = AddBuildingViaReflection(_towerAsset, actor.current_tile);
                if (b == null)
                {
                    note = "建造投资（工程失败，资金已投）";
                    return true; // 资金已扣，消费成立（不回退造成双重扣款）
                }
                if (actor.kingdom != null) b.setKingdom(actor.kingdom);
                EventStreamService.Record(EventStreamService.TypeBuildInv, GameHelpers.SafeKingdomName(actor.kingdom), spend);
                note = "建造投资（矗立瞭望塔）";
                return true;
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>打造军械：先扣款（30% 转城主军械费 + 70% 锻造消耗），再批量锻造 3-5 件武器。
        /// 先付款后交货，杜绝"免费武器"；锻造失败不退款（钱已成交）。</summary>
        private static bool TryCraftArsenal(Actor actor, int spend, out string note)
        {
            note = "打造军械";
            if (actor.city == null) return false;
            spend = Mathf.Clamp(spend, 60, 300);
            try
            {
                if (!TransferToCity(actor, Mathf.RoundToInt(spend * 0.3f))) return false;
                actor.addMoney(-Mathf.RoundToInt(spend * 0.7f));
            }
            catch (System.Exception) { return false; }
            int success = CraftTiered(actor, Mathf.RoundToInt(spend * 0.7f), 5);
            EventStreamService.Record(EventStreamService.TypeCraftArsenal, GameHelpers.SafeKingdomName(actor.kingdom), success);
            note = "打造军械（" + success + "件装备入城）";
            return true;
        }

        /// <summary>武器批发：先扣款（20% 转城主 + 80% 锻造消耗），再批量锻造 6-10 件武器。
        /// 先付款后交货，杜绝"免费武器"；锻造失败不退款（钱已成交）。</summary>
        private static bool TryWholesaleWeapons(Actor actor, int spend, out string note)
        {
            note = "武器批发";
            if (actor.city == null) return false;
            spend = Mathf.Clamp(spend, 120, 500);
            try
            {
                if (!TransferToCity(actor, Mathf.RoundToInt(spend * 0.2f))) return false;
                actor.addMoney(-Mathf.RoundToInt(spend * 0.8f));
            }
            catch (System.Exception) { return false; }
            int success = CraftTiered(actor, Mathf.RoundToInt(spend * 0.8f), 10);
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
                try { EraEngine.Start(kingdom, kingdomTrait, EconomyModMain.GetCurrentGameYear()); }
                catch (System.Exception)
                {
                    // 资金已扣但时代未开启：消费成立（投资失败不退款，避免回退缴税双重扣款）
                    note = "时代赞助（资金已投，活动未举办）";
                    return true;
                }
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
            City city;
            try { city = actor.city; }
            catch (System.Exception) { return null; }
            if (city == null) return null;

            if (_poorestCityActorCache.TryGetValue(city, out var cached))
            {
                if (cached == null || SafeId(cached) != SafeId(actor)) return cached;
            }

            var candidates = _cityPool;
            candidates.Clear();
            try
            {
                if (city.units == null) return null;
                candidates.AddRange(city.units);
            }
            catch (System.Exception) { return null; }

            Actor poorest = null;
            float minMoney = float.MaxValue;
            foreach (var c in candidates)
            {
                if (c == null) continue;
                if (!GameHelpers.IsCivilizedActor(c)) continue;
                try
                {
                    if (c.money < minMoney) { minMoney = c.money; poorest = c; }
                }
                catch (System.Exception) { }
            }

            if (!_poorestCityActorCache.ContainsKey(city))
                _poorestCityActorCache[city] = poorest;
            if (SafeId(poorest) != SafeId(actor)) return poorest;

            poorest = null;
            minMoney = float.MaxValue;
            foreach (var c in candidates)
            {
                if (c == null || SafeId(c) == SafeId(actor)) continue;
                if (!GameHelpers.IsCivilizedActor(c)) continue;
                try
                {
                    if (c.money < minMoney) { minMoney = c.money; poorest = c; }
                }
                catch (System.Exception) { }
            }
            return poorest;
        }


        // ===== 分层装备生成（v1.4.1）：凭空生成游戏内已有装备并给本国单位穿戴 =====

        /// <summary>各档价格：强度越高越贵（金钱需求）。</summary>
        private const int WeakTierPrice = 30;
        private const int MidTierPrice = 80;
        private const int StrongTierPrice = 200;

        private static List<EquipmentAsset> _weakPool;
        private static List<EquipmentAsset> _midPool;
        private static List<EquipmentAsset> _strongPool;

        /// <summary>按 equipment_value 在全部装备内分三档（弱 0-40% / 中 40-80% / 强 80-100% 分位），加载一次缓存。</summary>
        private static void EnsureItemPools()
        {
            if (_weakPool != null) return;
            _weakPool = new List<EquipmentAsset>(64);
            _midPool = new List<EquipmentAsset>(48);
            _strongPool = new List<EquipmentAsset>(24);
            var all = new List<EquipmentAsset>(AssetManager.items.list);
            all.RemoveAll(a => a == null || a.equipment_value <= 0);
            all.Sort((x, y) => x.equipment_value.CompareTo(y.equipment_value));
            int n = all.Count;
            if (n == 0) return;
            for (int i = 0; i < n; i++)
            {
                float p = (float)i / n;
                if (p < 0.4f) _weakPool.Add(all[i]);
                else if (p < 0.8f) _midPool.Add(all[i]);
                else _strongPool.Add(all[i]);
            }
        }

        /// <summary>
        /// 以 budget 预算最多 maxRolls 次购置装备：每次按强度掷档（强 8% / 中 32% / 弱 60%，
        /// 预算不足的档位权重归零），扣对应档价后凭空生成该档随机装备并 equip 到
        /// 同城/同国的随机存活文明单位（旧装备回城市仓库）。返回成功件数。
        /// </summary>
        private static int CraftTiered(Actor actor, int budget, int maxRolls)
        {
            EnsureItemPools();
            if (actor == null || actor.kingdom == null || _weakPool.Count == 0) return 0;
            int rolls = 0;
            int success = 0;
            while (budget >= WeakTierPrice && rolls < maxRolls)
            {
                rolls++;
                int tier = RollTier(budget);
                int price = tier == 2 ? StrongTierPrice : tier == 1 ? MidTierPrice : WeakTierPrice;
                if (budget < price) break;
                budget -= price;
                var pool = tier == 2 ? _strongPool : tier == 1 ? _midPool : _weakPool;
                var asset = pool[_rng.Next(pool.Count)];
                if (asset == null) continue;
                if (GenerateAndEquip(actor, asset)) success++;
            }
            return success;
        }

        /// <summary>掷档：强度越高概率越低；买不起的档位不参与。</summary>
        private static int RollTier(int budget)
        {
            float wStrong = budget >= StrongTierPrice ? 8f : 0f;
            float wMid = budget >= MidTierPrice ? 32f : 0f;
            float wWeak = 60f;
            float total = wStrong + wMid + wWeak;
            float r = (float)_rng.NextDouble() * total;
            if ((r -= wStrong) < 0f) return 2;
            if ((r -= wMid) < 0f) return 1;
            return 0;
        }

        /// <summary>凭空生成装备并给同城（优先）或同国随机存活文明单位穿戴；旧装备回城市仓库。</summary>
        private static bool GenerateAndEquip(Actor crafter, EquipmentAsset asset)
        {
            try
            {
                var target = FindEquipTarget(crafter);
                if (target == null) return false;
                var item = World.world.items.generateItem(asset, target.kingdom ?? crafter.kingdom,
                    "armory", 3, target, 0, false);
                if (item == null) return false;
                var slot = target.equipment.getSlot(asset.equipment_type);
                if (slot == null) return false;
                if (slot.isEmpty())
                {
                    SlotSetItem(slot, item, target);
                }
                else
                {
                    Item old = slot.getItem();
                    slot.takeAwayItem();
                    if (target.city != null) target.city.tryToPutItem(old);
                    SlotSetItem(slot, item, target);
                }
                return true;
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>穿戴目标：优先同城市民，城市为空则全王国随机存活文明单位。</summary>
        private static Actor FindEquipTarget(Actor crafter)
        {
            _equipTargetPool.Clear();
            if (crafter.city != null)
            {
                try
                {
                    if (crafter.city.units != null)
                        foreach (var a in crafter.city.units)
                            if (a != null && a.isAlive() && GameHelpers.IsCivilizedActor(a)) _equipTargetPool.Add(a);
                }
                catch (System.Exception) { }
            }
            if (_equipTargetPool.Count == 0 && crafter.kingdom != null && crafter.kingdom.units != null)
            {
                foreach (var a in crafter.kingdom.units)
                    if (a != null && a.isAlive() && GameHelpers.IsCivilizedActor(a)) _equipTargetPool.Add(a);
            }
            if (_equipTargetPool.Count == 0) return null;
            return _equipTargetPool[_rng.Next(_equipTargetPool.Count)];
        }

        private static readonly List<Actor> _equipTargetPool = new List<Actor>(32);

        // ActorEquipmentSlot.setItem 在编译期程序集中非 public（getItem/isEmpty/takeAwayItem 均公开），
        // 反射缓存调用；签名 setItem(Item, Actor)。
        private static System.Reflection.MethodInfo _slotSetItemMethod;

        private static bool SlotSetItem(ActorEquipmentSlot slot, Item item, Actor owner)
        {
            try
            {
                if (slot == null) return false;
                if (_slotSetItemMethod == null)
                {
                    _slotSetItemMethod = slot.GetType().GetMethod("setItem",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                }
                if (_slotSetItemMethod == null) return false;
                _slotSetItemMethod.Invoke(slot, new object[] { item, owner });
                return true;
            }
            catch (System.Exception) { return false; }
        }

        private static bool CanUseCityAction(Actor actor, Dictionary<long, int> cooldowns,
            int cooldownCycles, out long cityId)
        {
            cityId = 0L;
            if (actor == null || actor.city == null) return false;
            WorldTile tile;
            try { tile = actor.city.getTile(false); }
            catch (System.Exception) { return false; }
            if (tile == null) return false;

            cityId = ((long)tile.x << 32) | (uint)tile.y;
            int cycle = EconomyEngine.CycleIndex;
            int last;
            return !cooldowns.TryGetValue(cityId, out last) || cycle - last >= cooldownCycles;
        }

        private static void CommitCityAction(Dictionary<long, int> cooldowns, long cityId)
        {
            cooldowns[cityId] = EconomyEngine.CycleIndex;
        }

        private static void PruneExpiredCityActions(int cycle)
        {
            PruneCooldowns(_lastBuildCycle, cycle, BuildCooldownCycles);
        }

        private static void PruneCooldowns(Dictionary<long, int> cooldowns, int cycle, int cooldownCycles)
        {
            _expiredCityIds.Clear();
            foreach (var entry in cooldowns)
                if (cycle - entry.Value >= cooldownCycles) _expiredCityIds.Add(entry.Key);
            for (int i = 0; i < _expiredCityIds.Count; i++) cooldowns.Remove(_expiredCityIds[i]);
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
