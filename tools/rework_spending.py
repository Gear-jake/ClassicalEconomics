# -*- coding: utf-8 -*-
import io, re
t = io.open('Core/SpendingEngine.cs', encoding='utf-8').read()

# BuyWeapon
old = """        /// <summary>购买武器：金币先转给城市领袖（军械收入），再反射造真实武器（先付款后交货，杜绝免费武器）。</summary>
        private static bool TryBuyWeapon(Actor actor, int spend, out string note)
        {
            note = "购买武器";
            if (!TransferToCity(actor, spend)) return false;
            CraftRandomWeapon(actor); // 锻造失败不退款（交易已成交，钱已入城）
            note = "购买武器（军械入城）";
            return true;
        }"""
new = """        /// <summary>购买武器：金币先转给城市领袖（军械收入），再按强度分层购入一件真装备
        /// （强 200 / 中 80 / 弱 30，预算越多越可能买到高档）。</summary>
        private static bool TryBuyWeapon(Actor actor, int spend, out string note)
        {
            note = "购买武器";
            if (!TransferToCity(actor, spend)) return false;
            int got = CraftTiered(actor, spend, 1);
            note = got > 0 ? "购买武器（真装备上身）" : "购买武器（军械入城）";
            return true;
        }"""
assert old in t, 'buyweapon'
t = t.replace(old, new, 1)

old = """            int craftCount = Mathf.Clamp(spend / 30, 3, 5);
            int success = 0;
            for (int i = 0; i < craftCount; i++)
                if (CraftRandomWeapon(actor)) success++;
            EventStreamService.Record(EventStreamService.TypeCraftArsenal, GameHelpers.SafeKingdomName(actor.kingdom), success);"""
new = """            int success = CraftTiered(actor, Mathf.RoundToInt(spend * 0.7f), 5);
            EventStreamService.Record(EventStreamService.TypeCraftArsenal, GameHelpers.SafeKingdomName(actor.kingdom), success);"""
assert old in t, 'arsenal'
t = t.replace(old, new, 1)

old = """            int craftCount = Mathf.Clamp(spend / 40, 6, 10);
            int success = 0;
            for (int i = 0; i < craftCount; i++)
                if (CraftRandomWeapon(actor)) success++;
            EventStreamService.Record(EventStreamService.TypeWholesale, GameHelpers.SafeKingdomName(actor.kingdom), success);"""
new = """            int success = CraftTiered(actor, Mathf.RoundToInt(spend * 0.8f), 10);
            EventStreamService.Record(EventStreamService.TypeWholesale, GameHelpers.SafeKingdomName(actor.kingdom), success);"""
assert old in t, 'wholesale'
t = t.replace(old, new, 1)

# CraftRandomWeapon block: line-based removal
lines = t.splitlines(True)
start = next(i for i, l in enumerate(lines) if '/// <summary>反射调用 ActorData.tryToCraftRandomWeapon' in l)
end = next(i for i, l in enumerate(lines) if 'private static bool CanUseCityAction' in l)
cut = end
while cut > start and lines[cut - 1].strip() == '':
    cut -= 1
print('removing lines', start + 1, 'to', cut)
del lines[start:cut]
t = ''.join(lines)

new_block = """        // ===== 分层装备生成（v1.4.1）：凭空生成游戏内已有装备并给本国单位穿戴 =====

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
                    slot.setItem(item, target);
                }
                else
                {
                    Item old = slot.getItem();
                    slot.takeAwayItem();
                    if (target.city != null) target.city.tryToPutItem(old);
                    slot.setItem(item, target);
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
                    foreach (var a in crafter.city.getActors())
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

"""
anchor = '        private static bool CanUseCityAction'
idx = t.index(anchor)
t = t[:idx] + new_block + t[idx:]

io.open('Core/SpendingEngine.cs', 'w', encoding='utf-8', newline='').write(t)
print('SpendingEngine reworked')
