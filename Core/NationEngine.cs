using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.Core
{
    /// <summary>
    /// 中央银行家（v0.95 玩家参与度）：国家绑定 + 王室财政金库 + 持续政策/一次性法令 + 政绩记录。
    /// 玩家认领一个王国作为"本国"，所有动作经金库真实金币支付（全程守恒：城市仓库 → 金库 → 国民/消耗），
    /// 本国仍照常受 AI 引擎治理，玩家动作是叠加层不是接管。
    /// 年度管线在 Banking 之后、Snapshot 之前执行 RunAnnual（既有阶段相对顺序不变）。
    /// 金库只在年度阶段与玩家操作时变更（主线程）；后台统计不接触任何 NationEngine 状态。
    /// </summary>
    public static class NationEngine
    {
        // ===== 档位（少/中/大 = ×1/×2/×4）=====
        public const int TierCount = 3;
        public static float TierMult(int tier)
        {
            return tier <= 0 ? 1f : tier == 1 ? 2f : 4f;
        }

        /// <summary>持续政策类型。</summary>
        public enum PolicyKind { TaxCut = 0, TaxUp, PoorRelief, Propaganda, TradePact, Tariff }
        public const int PolicyKindCount = 6;

        /// <summary>建筑类型（世界内经济建筑）。</summary>
        public enum BuildingKind { Market = 0, Granary }
        private const int BuildingKindCount = 2;
        public const float MarketTaxBaseBonus = 0.10f; // 市场所在城市居民税基 +10%/座（v1.3.0 改义：原贸易容量加成随贸易模拟移除）
        public const float GranaryLossFactor = 0.7f; // 粮仓所在城市灾害财富蒸发 ×0.7

        private const int MaxSlots = 5;
        private const int SwitchCooldownYears = 10;
        internal const int RecordCapacity = 50;

        // ===== 绑定状态 =====
        internal static long _nationKingdomId;   // 0 = 未认领
        internal static string _nationName;
        internal static int _lastSwitchYear = int.MinValue;

        // ===== 金库 =====
        internal static long _treasury;
        internal static long _lastIncome;   // 上一期金库总收入（税负+政策收入）
        internal static long _lastExpense;  // 上一期金库总支出

        // ===== 持续政策槽位 =====
        internal class PolicySlot
        {
            public PolicyKind Kind;
            public int Tier;
            public int StartYear;
            public long TotalSpent; // 累计净支出（收入型政策为负值计入）
        }
        internal static readonly List<PolicySlot> _slots = new List<PolicySlot>(MaxSlots);

        // ===== 一次性法令冷却（年）=====
        internal static int _reliefReadyYear = int.MinValue;
        internal static int _festivalReadyYear = int.MinValue;
        public const int ReliefCooldownYears = 5;
        public const int FestivalCooldownYears = 3;
        public const int BuildCooldownYears = 10;

        // ===== 世界建筑（cityId → kind；仅年度阶段主线程写，后台只读且时序不重叠）=====
        internal static readonly Dictionary<long, int> _cityBuildings = new Dictionary<long, int>();
        private static readonly Dictionary<long, int> _buildReadyYear = new Dictionary<long, int>();

        // ===== 政绩记录（环形，UI 读取）=====
        public class NationRecord
        {
            public int Year;
            public string Key;       // 本地化键（policy/decree 名）
            public long Amount;      // 金额（正=支出，负=收入）
            public float GiniBefore, AvgBefore, PriceBefore;
            public float GiniAfter, AvgAfter, PriceAfter; // 下一年 RunAnnual 回填
            public bool Closed;
        }
        internal static readonly NationRecord[] _records = new NationRecord[RecordCapacity];
        internal static int _recordHead;
        internal static int _recordCount;

        // ===== 复用缓冲（年度路径，避免年度内重复分配）=====
        private static readonly List<Actor> _actorPool = new List<Actor>(256);
        private static readonly List<City> _cityPool = new List<City>(32);

        // ===== 绑定 =====

        public static long NationKingdomId => _nationKingdomId;
        public static string NationName => _nationName;
        public static long Treasury => _treasury;
        public static long LastIncome => _lastIncome;
        public static long LastExpense => _lastExpense;
        public static int LastSwitchYear => _lastSwitchYear;
        public static int ReliefReadyYear => _reliefReadyYear;
        public static int FestivalReadyYear => _festivalReadyYear;

        /// <summary>认领（或切换到）一个王国。切换有冷却；从新国家城市仓库转入 20% 金币作为启动资金。</summary>
        public static bool Claim(Kingdom kingdom, int currentYear, out string noteKey)
        {
            noteKey = null;
            if (kingdom == null || kingdom.data == null) return false;
            if (UnrestConfig.Instance == null || !UnrestConfig.Instance.NationPlayEnabled) return false;
            if (_nationKingdomId != 0 && currentYear - _lastSwitchYear < SwitchCooldownYears) return false;

            long newId = kingdom.data.id;
            if (newId == _nationKingdomId) return false;

            // 旧国家解绑（保留金库余额——钱随大臣走，认领新国家时作为启动资金的一部分带入）
            _nationKingdomId = newId;
            _nationName = GameHelpers.SafeKingdomName(kingdom);
            _lastSwitchYear = currentYear;
            _slots.Clear();
            _reliefReadyYear = currentYear; // 换国后法令冷却重新起算
            _festivalReadyYear = currentYear;

            // 启动资金：城市仓库金币 20% + 开国拨款（从居民财富征 0.5%）——
            // 城市仓库往往为空（金币主要在居民口袋），只抽仓库会出现金库 0 动不了的局面。
            long start = 0;
            var cities = SnapshotCities(kingdom, _cityPool);
            for (int i = 0; i < cities.Count; i++)
            {
                var c = cities[i];
                if (c == null) continue;
                try
                {
                    long gold = c.getResourcesAmount("gold");
                    long take = gold * 20 / 100;
                    if (take > 0)
                    {
                        c.takeResource("gold", (int)System.Math.Min(take, int.MaxValue));
                        start += take;
                    }
                }
                catch (System.Exception) { }
            }
            // 开国拨款：人口×人均×0.5%（从居民征收，守恒；无居民可征则为 0）
            var stats0 = NationStats();
            long grant = (long)(System.Math.Max(1, stats0?.ActorCount ?? 0) * System.Math.Max(0f, stats0?.AvgWealth ?? 0f) * 0.005f);
            start += CollectFromResidents(kingdom, grant);

            _treasury += start;

            EventStreamService.Record(EventStreamService.TypeNationClaim, _nationName, _treasury > int.MaxValue ? int.MaxValue : (int)_treasury);
            GameHelpers.NotifyLocalized("toast_nation_claim", _nationName, FormatGold(_treasury));
            AddRecord(currentYear, "nation_claim", start); // 记录实际转入的启动资金
            noteKey = "nation_claim";
            return true;
        }

        /// <summary>国家灭亡/世界失效时自动解绑（金库随人而空——清零，防止跨局套利）。</summary>
        public static void Unbind(string reasonKey)
        {
            if (_nationKingdomId == 0) return;
            string name = _nationName;
            _nationKingdomId = 0;
            _nationName = null;
            _slots.Clear();
            _cityBuildings.Clear();
            _buildReadyYear.Clear();
            _treasury = 0;
            _lastIncome = 0;
            _lastExpense = 0;
            GameHelpers.NotifyLocalized(reasonKey, name);
        }

        /// <summary>世界重置/换地图时全量清空。</summary>
        public static void Reset()
        {
            _nationKingdomId = 0;
            _nationName = null;
            _lastSwitchYear = int.MinValue;
            _treasury = 0;
            _lastIncome = 0;
            _lastExpense = 0;
            _slots.Clear();
            _cityBuildings.Clear();
            _buildReadyYear.Clear();
            _reliefReadyYear = int.MinValue;
            _festivalReadyYear = int.MinValue;
            _recordHead = 0;
            _recordCount = 0;
            for (int i = 0; i < RecordCapacity; i++) _records[i] = null;
            _nativeBuildId = null;
            _nativeBuildName = null;
            NationDiplomacy.Reset();
        }

        // ===== 持续政策 =====

        /// <summary>当前槽位占用（UI 显示 N/M）。</summary>
        public static int SlotCount => _slots.Count;

        /// <summary>查询某政策是否启用（返回槽位档位；未启用返回 -1）。</summary>
        public static int GetPolicyTier(PolicyKind kind)
        {
            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].Kind == kind) return _slots[i].Tier;
            return -1;
        }

        /// <summary>只读槽位快照（UI 用；返回内部引用列表，仅主线程 UI 调用）。</summary>



        /// <summary>启用（或改档）持续政策：占一个槽位；改档按改制约收费（一档年费）。</summary>
        public static bool EnablePolicy(PolicyKind kind, int tier, int currentYear)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled || _nationKingdomId == 0) return false;
            if (tier < 0 || tier >= TierCount) return false;
            if (AnnualPipeline.IsSettling) return false; // 结算期禁止操作（防并发污染周期状态）

            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].Kind != kind) continue;
                if (_slots[i].Tier == tier) return false; // 原档重复启用
                // 改档：付一档年费作改制约
                long reformFee = (long)PolicyAnnualCost(kind, _slots[i].Tier);
                if (!TryPay(reformFee)) { GameHelpers.NotifyLocalized("toast_nation_poor_treasury"); return false; }
                _slots[i].Tier = tier;
                _slots[i].TotalSpent += reformFee;
                ApplyPolicySideState(kind, tier);
                EventStreamService.Record(EventStreamService.TypeNationPolicy, _nationName, tier + 1);
                GameHelpers.NotifyLocalized("toast_nation_policy_on", PolicyName(kind), _nationName, tier + 1);
                return true;
            }

            if (_slots.Count >= cfg.PolicySlots) { GameHelpers.NotifyLocalized("toast_nation_no_slot", cfg.PolicySlots); return false; }

            // 新启用：预扣首年年费
            long firstYear = (long)PolicyAnnualCost(kind, tier);
            if (firstYear > 0 && !TryPay(firstYear)) { GameHelpers.NotifyLocalized("toast_nation_poor_treasury"); return false; }
            _slots.Add(new PolicySlot { Kind = kind, Tier = tier, StartYear = currentYear, TotalSpent = firstYear });
            ApplyPolicySideState(kind, tier);
            AddRecord(currentYear, PolicyKey(kind), firstYear);
            EventStreamService.Record(EventStreamService.TypeNationPolicy, _nationName, tier + 1);
            GameHelpers.NotifyLocalized("toast_nation_policy_on", PolicyName(kind), _nationName, tier + 1);
            return true;
        }

        /// <summary>取消政策（免费，特质随之移除）。</summary>
        public static bool DisablePolicy(PolicyKind kind)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].Kind != kind) continue;
                _slots.RemoveAt(i);
                RemovePolicySideState(kind);
                EventStreamService.Record(EventStreamService.TypeNationPolicy, _nationName, 0);
                GameHelpers.NotifyLocalized("toast_nation_policy_off", PolicyName(kind), _nationName);
                return true;
            }
            return false;
        }

        public static string PolicyKey(PolicyKind kind)
        {
            switch (kind)
            {
                case PolicyKind.TaxCut: return "nation_policy_taxcut";
                case PolicyKind.TaxUp: return "nation_policy_taxup";
                case PolicyKind.PoorRelief: return "nation_policy_poorrelief";
                case PolicyKind.Propaganda: return "nation_policy_propaganda";
                case PolicyKind.TradePact: return "nation_policy_tradepact";
                default: return "nation_policy_tariff";
            }
        }

        public static string PolicyName(PolicyKind kind)
        {
            return Services.LocalizationService.Get(PolicyKey(kind));
        }

        /// <summary>政策年费（档位基准；收入型政策返回的是收入基准，由调用方按方向处理）。</summary>
        public static float PolicyAnnualCost(PolicyKind kind, int tier)
        {
            float mult = TierMult(tier);
            var stats = NationStats();
            float gdp = stats?.GDP ?? 0f;
            float pop = stats?.ActorCount ?? 0;
            float avg = stats?.AvgWealth ?? 0f;
            switch (kind)
            {
                case PolicyKind.TaxCut: return gdp * 0.005f * mult;
                case PolicyKind.TaxUp: return gdp * 0.005f * mult;    // 收入
                case PolicyKind.PoorRelief: return pop * avg * 0.001f * mult;
                case PolicyKind.Propaganda: return pop * 0.02f * mult;
                case PolicyKind.TradePact: return gdp * 0.003f * mult;
                default: return gdp * 0.004f * mult;                   // 关税：收入
            }
        }

        /// <summary>政策的世界侧状态（王国税率特质）。启用时施加，取消/清空时移除。</summary>
        private static void ApplyPolicySideState(PolicyKind kind, int tier)
        {
            var k = GameHelpers.FindKingdom(_nationKingdomId);
            if (k == null) return;
            if (kind == PolicyKind.TaxCut) SetTaxTrait(k, "tax_rate_local_low", true);
            else if (kind == PolicyKind.TaxUp) SetTaxTrait(k, "tax_rate_local_high", true);
        }

        private static void RemovePolicySideState(PolicyKind kind)
        {
            var k = GameHelpers.FindKingdom(_nationKingdomId);
            if (k == null) return;
            if (kind == PolicyKind.TaxCut) SetTaxTrait(k, "tax_rate_local_low", false);
            else if (kind == PolicyKind.TaxUp) SetTaxTrait(k, "tax_rate_local_high", false);
        }

        /// <summary>幂等开关王国税率特质（与经济周期调制器的特质通道一致）。</summary>
        private static void SetTaxTrait(Kingdom k, string traitId, bool add)
        {
            try
            {
                if (add) { if (!k.hasTrait(traitId)) k.addTrait(traitId, true); }
                else { if (k.hasTrait(traitId)) k.removeTrait(traitId); }
            }
            catch (System.Exception) { }
        }

        // ===== 一次性法令 =====

        /// <summary>紧急救济：大额转移给最穷 20% 国民（守恒），冷却 5 年。</summary>
        public static bool TryEmergencyRelief(int currentYear)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled || _nationKingdomId == 0) return false;
            if (AnnualPipeline.IsSettling) return false;
            if (currentYear < _reliefReadyYear) { GameHelpers.NotifyLocalized("toast_nation_cooldown"); return false; }
            var stats = NationStats();
            var kingdom = GameHelpers.FindKingdom(_nationKingdomId);
            if (stats == null || kingdom == null || kingdom.units == null) return false;

            long cost = (long)(stats.ActorCount * stats.AvgWealth * 0.02f);
            if (cost <= 0) return false;
            if (!TryPay(cost)) { GameHelpers.NotifyLocalized("toast_nation_poor_treasury"); return false; }

            // 最穷 20%（按财富升序取前 20%）
            var units = SnapshotActors(kingdom, _actorPool);
            if (units.Count == 0) { _treasury += cost; return false; } // 无国民可领，退回金库
            var poor = _poorPool;
            poor.Clear();
            for (int i = 0; i < units.Count; i++)
            {
                float w;
                if (GameHelpers.TryGetWealth(units[i], out w)) poor.Add(units[i]);
            }
            poor.Sort((a, b) => WealthOf(a).CompareTo(WealthOf(b)));
            int count = System.Math.Max(1, poor.Count * 20 / 100);
            long per = cost / count;
            long given = 0;
            for (int i = 0; i < count && i < poor.Count; i++)
            {
                long share = per + (i == 0 ? cost - per * count : 0); // 余数补第一个，精确守恒
                if (share > 0) { GameHelpers.AddPositiveMoney(poor[i], share); given += share; }
            }
            if (given < cost) _treasury += cost - given; // 未能发出部分退回金库

            _reliefReadyYear = currentYear + ReliefCooldownYears;
            AddRecord(currentYear, "nation_decree_relief", given);
            EventStreamService.Record(EventStreamService.TypeNationRelief, _nationName, given > int.MaxValue ? int.MaxValue : (int)given);
            GameHelpers.NotifyLocalized("toast_nation_relief", _nationName, FormatGold(given));
            return true;
        }

        /// <summary>国庆庆典：清除本国动荡特质与积累 + 庆典消耗与红包，冷却 3 年。</summary>
        public static bool TryFestival(int currentYear)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled || _nationKingdomId == 0) return false;
            if (AnnualPipeline.IsSettling) return false;
            if (currentYear < _festivalReadyYear) { GameHelpers.NotifyLocalized("toast_nation_cooldown"); return false; }
            var kingdom = GameHelpers.FindKingdom(_nationKingdomId);
            if (kingdom == null) return false;
            var stats = NationStats();

            long cost = (long)(System.Math.Max(1, stats?.ActorCount ?? 0) * 0.5f);
            if (!TryPay(cost)) { GameHelpers.NotifyLocalized("toast_nation_poor_treasury"); return false; }

            // 资金去向：50% 庆典消耗回流城市仓库（消费真实入账），50% 红包均分国民
            long toCities = cost / 2;
            var cities = SnapshotCities(kingdom, _cityPool);
            long deposited = 0;
            for (int i = 0; i < cities.Count && toCities - deposited > 0; i++)
            {
                long give = (toCities - deposited) / (cities.Count - i) + ((toCities - deposited) % (cities.Count - i) == 0 ? 0 : 1);
                if (give <= 0) break;
                try { cities[i].addResourcesToRandomStockpile("gold", (int)System.Math.Min(give, int.MaxValue)); deposited += give; }
                catch (System.Exception) { }
            }
            long gifts = cost - deposited;
            var units = SnapshotActors(kingdom, _actorPool);
            if (units.Count > 0)
            {
                long per = gifts / units.Count;
                long given = 0;
                for (int i = 0; i < units.Count; i++)
                {
                    long share = per + (i == 0 ? gifts - per * units.Count : 0);
                    if (share > 0) { GameHelpers.AddPositiveMoney(units[i], share); given += share; }
                }
                if (given < gifts)
                {
                    // 未能发出部分回流城市仓库（守恒兜底）
                    if (cities.Count > 0)
                        try { cities[0].addResourcesToRandomStockpile("gold", (int)System.Math.Min(gifts - given, int.MaxValue)); } catch (System.Exception) { }
                }
            }
            else
            {
                if (cities.Count > 0)
                    try { cities[0].addResourcesToRandomStockpile("gold", (int)System.Math.Min(gifts, int.MaxValue)); } catch (System.Exception) { }
            }

            if (UnrestEngine.TryFestivalClear(kingdom))
            {
                // 动荡已清除（含特质）；若在暴动战争中则不强制停战（战争由收复战争机制管理）
            }

            _festivalReadyYear = currentYear + FestivalCooldownYears;
            AddRecord(currentYear, "nation_decree_festival", cost);
            EventStreamService.Record(EventStreamService.TypeNationFestival, _nationName, cost > int.MaxValue ? int.MaxValue : (int)cost);
            GameHelpers.NotifyLocalized("toast_nation_festival", _nationName, FormatGold(cost));
            return true;
        }

        // ===== 世界建筑 =====

        /// <summary>市场建筑记录查询（v1.3.0 改义：每座市场使该国居民税基 +10%，仅主线程年度税负路径使用）。</summary>
        public static bool IsMarketCity(long cityId) { return _nationKingdomId != 0 && _cityBuildings.TryGetValue(cityId, out int k) && k == (int)BuildingKind.Market; }
        public static bool IsGranaryCity(long cityId) { return _nationKingdomId != 0 && _cityBuildings.TryGetValue(cityId, out int k) && k == (int)BuildingKind.Granary; }

        /// <summary>兴建建筑（本国城市，同类每城一座，按城市 10 年冷却）。</summary>
        public static bool TryBuild(Kingdom kingdom, City city, BuildingKind kind, int currentYear)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled || _nationKingdomId == 0) return false;
            if (AnnualPipeline.IsSettling) return false;
            if (kingdom == null || city == null || kingdom.data == null) return false;
            if (kingdom.data.id != _nationKingdomId) return false;
            long cityId;
            try { cityId = city.id; } catch (System.Exception) { return false; }
            if (_cityBuildings.ContainsKey(cityId)) { GameHelpers.NotifyLocalized("toast_nation_built_already"); return false; }
            int ready;
            if (_buildReadyYear.TryGetValue(cityId, out ready) && currentYear < ready) { GameHelpers.NotifyLocalized("toast_nation_cooldown"); return false; }

            var stats = NationStats();
            float avg = stats?.AvgWealth ?? 0f;
            // 城市人口估算：王国人口 / 城市数（City 无公开人口读取，改用均值口径，费用仍随规模缩放）
            var cities0 = SnapshotCities(kingdom, _cityPool);
            int pop = cities0.Count > 0 ? System.Math.Max(1, (stats?.ActorCount ?? 0) / cities0.Count) : 1;
            long cost = (long)(System.Math.Max(1, pop) * System.Math.Max(1f, avg) * (kind == BuildingKind.Market ? 1.5f : 1.2f));
            if (!TryPay(cost)) { GameHelpers.NotifyLocalized("toast_nation_poor_treasury"); return false; }

            var asset = ResolveBuildingAsset(kind);
            if (asset == null)
            {
                _treasury += cost; // 无可用建筑资源（原版缺该建筑资产）：fail-closed 退回并禁用
                GameHelpers.NotifyLocalized("toast_nation_build_unavailable");
                return false;
            }

            var building = PlaceBuilding(city, asset, kingdom);
            if (building == null)
            {
                _treasury += cost; // 工程失败退回（资金不消失）
                GameHelpers.NotifyLocalized("toast_nation_build_failed");
                return false;
            }

            _cityBuildings[cityId] = (int)kind;
            _buildReadyYear[cityId] = currentYear + BuildCooldownYears;
            AddRecord(currentYear, kind == BuildingKind.Market ? "nation_build_market" : "nation_build_granary", cost);
            EventStreamService.Record(EventStreamService.TypeNationBuild, _nationName, cost > int.MaxValue ? int.MaxValue : (int)cost);
            GameHelpers.NotifyLocalized("toast_nation_built", _nationName, FormatGold(cost));
            return true;
        }

        /// <summary>灾害/战争掠夺摧毁建筑（无赔偿——风险真实）。返回是否摧毁了东西。</summary>
        public static bool DestroyCityBuildings(long cityId, string reasonKey)
        {
            if (_cityBuildings.Remove(cityId))
            {
                GameHelpers.NotifyLocalized(reasonKey, _nationName);
                return true;
            }
            return false;
        }

        /// <summary>战争掠夺后：摧毁该王国全部建筑（掠夺者不给赔偿）。</summary>
        public static void OnKingdomPlundered(long kingdomId)
        {
            if (_nationKingdomId == 0 || kingdomId != _nationKingdomId) return;
            var k = GameHelpers.FindKingdom(kingdomId);
            if (k == null) return;
            var cities = SnapshotCities(k, _cityPool);
            var ids = _plunderedIds;
            ids.Clear();
            for (int i = 0; i < cities.Count; i++)
            {
                try { ids.Add(cities[i].id); } catch (System.Exception) { }
            }
            for (int i = 0; i < ids.Count; i++) DestroyCityBuildings(ids[i], "toast_nation_destroyed_war");
        }

        private static readonly List<long> _plunderedIds = new List<long>(16);

        private static BuildingAsset ResolveBuildingAsset(BuildingKind kind)
        {
            string[] candidates = kind == BuildingKind.Market
                ? new string[] { "market", "marketplace", "trade_post" }
                : new string[] { "granary", "food_storage", "storehouse" };
            for (int i = 0; i < candidates.Length; i++)
            {
                BuildingAsset a = null;
                try { a = AssetManager.buildings.get(candidates[i]); } catch (System.Exception) { }
                if (a != null) return a;
            }
            return null;
        }

        private static System.Reflection.MethodInfo _addBuildingMethod;

        /// <summary>反射调用 internal BuildingManager.addBuilding(BuildingAsset, WorldTile, bool) 放置建筑（与 SpendingEngine 同模式）。</summary>
        private static Building PlaceBuilding(City city, BuildingAsset asset, Kingdom kingdom)
        {
            try
            {
                WorldTile tile = city.getTile(false);
                if (tile == null) return null;
                if (_addBuildingMethod == null)
                {
                    _addBuildingMethod = typeof(BuildingManager).GetMethod("addBuilding",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                }
                if (_addBuildingMethod == null) return null;
                var b = _addBuildingMethod.Invoke(null, new object[] { asset, tile, true }) as Building;
                if (b != null && kingdom != null) b.setKingdom(kingdom);
                return b;
            }
            catch (System.Exception) { return null; }
        }

        // ===== 年度管线（Banking 之后、Snapshot 之前）=====

        /// <summary>年度结算：国家存续校验 → 金库税负收入 → 持续政策扣费/收入与效果 → 回填政绩记录。</summary>
        public static void RunAnnual(int year)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled) return;
            if (_nationKingdomId == 0) return;

            // 1. 国家存续校验（灭亡自动解绑）
            var kingdom = GameHelpers.FindKingdom(_nationKingdomId);
            if (kingdom == null)
            {
                Unbind("toast_nation_unbind");
                return;
            }

            long income = 0;
            long expense = 0;
            var stats = NationStats();

            // 2. 金库税负 = 城市仓库×配置比例 + 居民税（人口×人均×0.1%）；
            //    总上限 = 人口×人均×0.1（居民税保证城市仓库为空时国库仍有收入）
            var cities = SnapshotCities(kingdom, _cityPool);
            long ratio = System.Math.Max(1, System.Math.Min(20, cfg.TreasuryIncomeRatio));
            long cap = (long)((stats?.ActorCount ?? 0) * (stats?.AvgWealth ?? 0f) * 0.1f);
            for (int i = 0; i < cities.Count; i++)
            {
                var c = cities[i];
                if (c == null) continue;
                try
                {
                    long gold = c.getResourcesAmount("gold");
                    long take = gold * ratio / 100;
                    if (cap > 0 && income + take > cap) take = cap - income;
                    if (take <= 0) continue;
                    c.takeResource("gold", (int)System.Math.Min(take, int.MaxValue));
                    income += take;
                }
                catch (System.Exception) { }
            }
            // 居民税：人口×人均×0.1%（受总上限约束；仓库已征满则居民税为 0）。
            // 铸币政策：居民税 +8%/档；市场建筑：每座居民税基 +10%（v1.3.0 改义）
            int mintTier = GetPolicyTier(PolicyKind.TradePact);
            float residentMult = 1f + (mintTier >= 0 ? 0.08f * (mintTier + 1) : 0f);
            int marketCount = 0;
            foreach (var kv in _cityBuildings)
                if (kv.Value == (int)BuildingKind.Market) marketCount++;
            residentMult += MarketTaxBaseBonus * marketCount;
            long residentTarget = (long)((stats?.ActorCount ?? 0) * (stats?.AvgWealth ?? 0f) * 0.001f * residentMult);
            long remaining = cap > 0 ? System.Math.Max(0, cap - income) : residentTarget;
            long residentIncome = CollectFromResidents(kingdom, System.Math.Min(residentTarget, remaining));
            income += residentIncome;
            _treasury += income;

            // 3. 持续政策：扣费/收税 + 效果
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                var slot = _slots[i];
                float costF = PolicyAnnualCost(slot.Kind, slot.Tier);
                if (slot.Kind == PolicyKind.TaxUp)
                {
                    // 加税充实：从本国居民征收（真实转移，守恒）
                    long target = (long)costF;
                    long collected = CollectFromResidents(kingdom, target);
                    income += collected;
                    _treasury += collected;
                    slot.TotalSpent -= collected;
                    continue;
                }
                if (slot.Kind == PolicyKind.Tariff)
                {
                    // 专卖：从本国居民征收消费税进金库（真实转移，守恒；v1.3.0 改义自关税）
                    long target = (long)costF;
                    long collected = CollectFromResidents(kingdom, target);
                    income += collected;
                    _treasury += collected;
                    slot.TotalSpent -= collected;
                    continue;
                }

                long fee = (long)costF;
                if (fee > 0 && !TryPay(fee))
                {
                    // 金库不足：政策自动暂停（横幅通知），不取消槽位（余额够时下年自动恢复）
                    GameHelpers.NotifyLocalized("toast_nation_policy_suspended", PolicyName(slot.Kind));
                    RemovePolicySideState(slot.Kind);
                    continue;
                }
                expense += fee;
                slot.TotalSpent += fee;

                if (slot.Kind == PolicyKind.PoorRelief && fee > 0)
                    DistributeToPoor(kingdom, fee, stats);
            }

            // 4. 回填上一年记录的"事后指标"
            for (int i = 0; i < _recordCount; i++)
            {
                var r = _records[(_recordHead - 1 - i + RecordCapacity) % RecordCapacity];
                if (r == null || r.Closed) continue;
                r.GiniAfter = stats?.GiniCoefficient ?? r.GiniBefore;
                r.AvgAfter = stats?.AvgWealth ?? r.AvgBefore;
                r.PriceAfter = stats?.LocalPrice ?? r.PriceBefore;
                r.Closed = true;
            }

            _lastIncome = income;
            _lastExpense = expense;
            NationDiplomacy.RunAnnual(year); // 双边经济协定年费（金库不足自动解除）
        }

        /// <summary>从本国居民征收金币（关税/加税口径；真实转移，上限为居民财富的 10%）。</summary>
        private static long CollectFromResidents(Kingdom kingdom, long target)
        {
            if (target <= 0 || kingdom == null || kingdom.units == null) return 0L;
            var units = SnapshotActors(kingdom, _actorPool);
            if (units.Count == 0) return 0L;
            long cap = 0;
            for (int i = 0; i < units.Count; i++)
            {
                float w;
                if (GameHelpers.TryGetWealth(units[i], out w)) cap += (long)System.Math.Max(0f, w);
            }
            cap = cap / 10; // 居民总财富 10% 上限，防止征收致贫
            long amount = System.Math.Min(target, cap);
            if (amount <= 0) return 0L;
            return GameHelpers.DeductCoins(units, amount);
        }

        /// <summary>把钱分给本国贫困线（人均×0.8）以下国民（余数补第一人，精确守恒）。</summary>
        private static void DistributeToPoor(Kingdom kingdom, long amount, EconomyMod.Models.KingdomStats stats)
        {
            if (amount <= 0 || kingdom == null || kingdom.units == null) return;
            float line = (stats?.AvgWealth ?? 0f) * 0.8f;
            var units = SnapshotActors(kingdom, _actorPool);
            var poor = _poorPool;
            poor.Clear();
            for (int i = 0; i < units.Count; i++)
            {
                float w;
                if (GameHelpers.TryGetWealth(units[i], out w) && w < line) poor.Add(units[i]);
            }
            if (poor.Count == 0) return; // 无穷人可领：钱留在金库（下期再试）
            long per = amount / poor.Count;
            for (int i = 0; i < poor.Count; i++)
            {
                long share = per + (i == 0 ? amount - per * poor.Count : 0);
                if (share > 0) GameHelpers.AddPositiveMoney(poor[i], share);
            }
        }

        /// <summary>金库扣款（公开给外交模块用）；不足返回 false（不部分扣款）。
        /// 金额为负数时按退款处理（外交赠礼未发出部分的退回）。</summary>
        public static bool TrySpend(long amount)
        {
            return TryPay(amount);
        }

        /// <summary>金库入账（公开给外交模块用：双边经济协定纳贡收入）。</summary>
        public static void AddTreasury(long amount)
        {
            if (amount > 0) _treasury += amount;
        }

        /// <summary>金库扣款；不足返回 false（不部分扣款）。</summary>
        private static bool TryPay(long amount)
        {
            if (amount <= 0) return true;
            if (_treasury < amount) return false;
            _treasury -= amount;
            return true;
        }

        /// <summary>本国的当前统计快照（无统计或未认领时为 null；外交模块复用）。</summary>
        public static EconomyMod.Models.KingdomStats NationStats()
        {
            EconomyMod.Models.KingdomStats ks;
            return EconomyEngine.KingdomStats.TryGetValue(_nationKingdomId, out ks) ? ks : null;
        }

        // ===== 政策查询 =====

        /// <summary>宣传：本国动荡积累暂停。</summary>
        public static bool PropagandaActive(long kingdomId)
        {
            if (_nationKingdomId == 0 || kingdomId != _nationKingdomId) return false;
            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].Kind == PolicyKind.Propaganda) return true;
            return false;
        }

        // ===== 政绩记录 =====

        private static void AddRecord(int year, string key, long amount)
        {
            var stats = NationStats();
            var r = new NationRecord
            {
                Year = year,
                Key = key,
                Amount = amount,
                GiniBefore = stats?.GiniCoefficient ?? 0f,
                AvgBefore = stats?.AvgWealth ?? 0f,
                PriceBefore = stats?.LocalPrice ?? 0f
            };
            _records[_recordHead] = r;
            _recordHead = (_recordHead + 1) % RecordCapacity;
            if (_recordCount < RecordCapacity) _recordCount++;
        }

        /// <summary>最近 count 条记录（时间正序；返回复用缓冲，仅 UI 主线程调用）。</summary>
        public static IReadOnlyList<NationRecord> GetRecentRecords(int count)
        {
            var list = new List<NationRecord>(System.Math.Min(count, _recordCount));
            for (int i = _recordCount - 1; i >= 0 && list.Count < count; i--)
                list.Add(_records[(_recordHead - 1 - i + RecordCapacity) % RecordCapacity]);
            list.Reverse();
            return list;
        }

        // ===== 工具 =====

        public static string FormatGold(long amount)
        {
            return amount.ToString("N0");
        }

        private static float WealthOf(Actor a)
        {
            float w;
            return GameHelpers.TryGetWealth(a, out w) ? w : 0f;
        }

        private static List<Actor> SnapshotActors(Kingdom kingdom, List<Actor> pool)
        {
            pool.Clear();
            try
            {
                if (kingdom.units != null)
                    foreach (var a in kingdom.units)
                        if (a != null && a.isAlive()) pool.Add(a);
            }
            catch (System.Exception) { }
            return pool;
        }

        private static List<City> SnapshotCities(Kingdom kingdom, List<City> pool)
        {
            pool.Clear();
            try
            {
                var cities = kingdom.getCities();
                if (cities != null)
                    foreach (City c in cities)
                        if (c != null) pool.Add(c);
            }
            catch (System.Exception) { }
            return pool;
        }

        private static readonly List<Actor> _poorPool = new List<Actor>(64);        // ===== 原版建筑放置（RulerBox BuildingPlacementTool 模式）：建设页选建筑 →
        // ===== 鼠标点击地图（本国领土、非海洋）→ 金库扣费 → World.world.buildings.addBuilding。
        // ===== 右键取消；放置成功可继续放置（连续建造），再次右键退出。

        private static string _nativeBuildId;
        private static string _nativeBuildName;

        /// <summary>是否处于原版建筑放置模式。</summary>
        public static bool IsNativePlacing => _nativeBuildId != null;

        /// <summary>当前放置中的建筑名（UI 提示用）。</summary>
        public static string NativeBuildName => _nativeBuildName;

        /// <summary>按建筑资产解析金库费用：gold 成本 + 资源成本折半，保底 100 金币。</summary>
        public static long NativeFee(BuildingAsset asset)
        {
            if (asset == null || asset.cost == null) return 100L;
            var c = asset.cost;
            long fee = c.gold + (c.wood + c.stone + c.common_metals) / 2;
            // 法典：建筑费乘数（公共工程折扣 / 筑防加价）
            float bc = _nationKingdomId != 0 ? LawEngine.GetMods(_nationKingdomId).BuildCost : 1f;
            if (bc != 1f) fee = System.Math.Max(50L, (long)(fee * bc));
            return System.Math.Max(100L, fee);
        }

        /// <summary>进入放置模式；建筑不存在或未认领时返回 false（fail-closed）。</summary>
        public static bool BeginNativePlacement(string assetId, string displayName)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled || _nationKingdomId == 0) return false;
            if (AnnualPipeline.IsSettling) return false;
            BuildingAsset asset = null;
            try { asset = AssetManager.buildings.get(assetId); } catch (System.Exception) { }
            if (asset == null)
            {
                GameHelpers.NotifyLocalized("toast_nation_build_unavailable", displayName != null ? displayName : assetId);
                return false;
            }
            _nativeBuildId = assetId;
            _nativeBuildName = assetId;
            GameHelpers.NotifyLocalized("toast_nation_place_mode", displayName != null ? displayName : assetId);
            return true;
        }

        /// <summary>取消放置模式。</summary>
        public static void CancelNativePlacement()
        {
            _nativeBuildId = null;
            _nativeBuildName = null;
        }

        /// <summary>每帧由 EconomyTickRunner 调用：右键取消；左键（非 UI 上）尝试放置。</summary>
        public static void TickNativePlacement()
        {
            if (!IsNativePlacing) return;
            if (World.world == null) { CancelNativePlacement(); return; }

            try
            {
                if (UnityEngine.Input.GetMouseButtonDown(1))
                {
                    CancelNativePlacement();
                    GameHelpers.NotifyLocalized("toast_nation_place_cancelled");
                    return;
                }
                if (!UnityEngine.Input.GetMouseButtonDown(0)) return;
                if (UnityEngine.EventSystems.EventSystem.current != null
                    && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return; // 点在 UI 上不放

                var tile = World.world.getMouseTilePos();
                if (tile == null) return;
                if (tile.Type != null && (tile.Type.ocean || tile.Type.liquid))
                {
                    GameHelpers.NotifyLocalized("toast_nation_place_land");
                    return;
                }
                var city = tile.zone != null ? tile.zone.city : null;
                var kingdom = city != null ? GameHelpers.GetKingdomOfCity(city) : null;
                if (kingdom == null || kingdom.data == null || kingdom.data.id != _nationKingdomId)
                {
                    GameHelpers.NotifyLocalized("toast_nation_place_territory");
                    return;
                }

                BuildingAsset asset = null;
                try { asset = AssetManager.buildings.get(_nativeBuildId); } catch (System.Exception) { }
                if (asset == null) { CancelNativePlacement(); return; }

                long fee = NativeFee(asset);
                if (!TrySpend(fee))
                {
                    GameHelpers.NotifyLocalized("toast_nation_poor_treasury");
                    return;
                }

                if (!NativeAddBuildingTried(asset, tile))
                {
                    _treasury += fee; // 放置失败退款（fail-closed）
                    GameHelpers.NotifyLocalized("toast_nation_build_failed");
                    return;
                }
                AddRecord(SafeYearNow(), "nation_build_native", fee);
                EventStreamService.Record(EventStreamService.TypeNationBuild, _nationName,
                    fee > int.MaxValue ? int.MaxValue : (int)fee);
                GameHelpers.NotifyLocalized("toast_nation_built", _nationName, FormatGold(fee));
                // 连续放置：不退出模式（右键结束）
            }
            catch (System.Exception) { CancelNativePlacement(); }
        }

        // BuildingManager.addBuilding(string, WorldTile) 为运行时成员（编译期 DLL 缺失，与 startWar 同理）：
        // 运行时反射定位一次并缓存；失败返回 false（调用方退款）。
        private static System.Reflection.MethodInfo _addBuildingByIdMethod;

        private static bool NativeAddBuildingTried(BuildingAsset asset, WorldTile tile)
        {
            try
            {
                if (_addBuildingByIdMethod == null)
                {
                    _addBuildingByIdMethod = typeof(BuildingManager).GetMethod("addBuilding",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                        null, new System.Type[] { typeof(string), typeof(WorldTile) }, null);
                }
                if (_addBuildingByIdMethod == null || _buildingManagerInstance == null)
                {
                    _buildingManagerInstance = World.world.buildings;
                }
                if (_addBuildingByIdMethod == null || _buildingManagerInstance == null) return false;
                _addBuildingByIdMethod.Invoke(_buildingManagerInstance, new object[] { asset.id, tile });
                return true;
            }
            catch (System.Exception) { return false; }
        }

        private static BuildingManager _buildingManagerInstance;

        private static int SafeYearNow()
        {
            try { return EconomyModMain.GetCurrentGameYear(); } catch (System.Exception) { return 0; }
        }
    }
}