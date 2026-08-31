using System;
using System.Collections.Generic;
using System.Threading;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 多线程统计 + 地理贸易网络模拟引擎（主线程零计算）。
    /// 主线程仅采集（读取 Unity 对象 → 写入纯数据记录）与寻路（PathfinderTools 为 Unity API，
    /// 仅主线程调用，结果缓存为纯数据边）；全部统计计算与贸易量计算在后台线程完成，
    /// 结果由主线程轮询 <see cref="TryConsume"/> 消费后发布。后台线程绝不接触 Unity 对象。
    ///
    /// 贸易网络（地理驱动，Phase 6）：城市为节点 / 王国为聚合层，按 MaxEdges 硬上限构建确定性稀疏图。
    /// - 边：国内城市按坐标/id 排序，仅连接前后各最多两个近邻；每对邻国仅保留最近两条
    ///       不同城市边（陆路、成本=欧氏距离）；非邻国王国对 → 取"最近城市对"（欧氏距离最小），距离 ≤ MaxTradeRange 才
    ///       入寻路队列，PathfinderTools.raycast 确认可达（空路径 ⇒ 无贸易边）；
    ///       寻路路径海洋占比 &gt; 50% ⇒ 海路（受出口王国 Boats × 每船载量上限约束）。
    /// - 邻国判定：原版城市邻国数据（City.neighbours_kingdoms 等）为 internal 不可访问，
    ///       以王国几何距离为代理——Kingdom.distanceBetweenKingdom ≤ MaxTradeRange ⇒ 邻国。
    /// - 成本：cost = (陆路距离 + 海路距离 × SeaRoutePenalty) × (邻国 ? 1 : NonNeighborPenalty)
    ///       贸易量 = min(|gapA|,|gapB|) × 距离衰减 × 运输摩擦 × 套利加成 × TradeFlowRatio。
    /// - 供需缺口：城市 gap = gold − 仓库容量；容量优先用原版真实仓库容量
    ///       （ResourceLibrary.gold.storage_max，游戏原版自带仓库系统，用户确认方案），
    ///       不可用（=0）时回退到 建筑数 × TradeCityBaseCapacity × 50% 估算；
    ///       gap &gt; 0 盈余出口 / gap &lt; 0 缺口进口，按边互补结算。
    /// - 结算：出口城 addResourcesToRandomStockpile("gold", amt)，进口城 takeResource 兜底
    ///       DeductCoins（与原版缴税同渠道），全图净≈0。
    /// 寻路缓存三类失效：Reset() 世界重置清空 / 每 PathRecomputeEvery 周期全量重算 /
    /// 城市、王国或邻接拓扑变化时清空边与寻路队列，并按确定性候选集完整重建。
    ///
    /// v0.11：生产函数（Workers × Productivity × CapitalFactor）、区域价格指数
    /// （LocalPrice = 全局 CPI × 供需系数，clamp 0.5~2）、价格离散度（PriceDispersion=CV）；
    /// 距离衰减/运输成本/套利权重三个参数由游戏状态每周期自适应推导（EMA 平滑 + 单点 clamp，
    /// 无固定配置），并在 ComputeTrade 内真正参与流量计算。
    /// </summary>
    public static class TradeSimulationWorker
    {
        // ===== 纯数据采集记录（主线程填写 / 后台线程只读）=====

        public struct ActorRecord
        {
            public float Wealth;
            public long KingdomId;
            public byte JobCode; // 0=无业 1=farm 2=hunt 3=wood 4=miner 5=builder 6=其他
        }

        public struct KingdomFacts
        {
            public long Id;
            public string Name;
            public int Population;
            public int Capacity;
            public long Food;
            public int Cities;
            public int Boats;
            public int Specialty; // BiomeSpecialty（主线程采集阶段读取，后台线程只读，避免后台访问 Unity 对象）
            public float CityX;   // 首都城市 tile x 坐标（主线程反射读取，后台只读；NaN=未知）
            public float CityY;   // 首都城市 tile y 坐标
        }

        /// <summary>城市快照（主线程采集：Unity 对象 → 纯数据，后台线程只读）。</summary>
        public struct CitySnapshot
        {
            public long CityId;            // ((long)x << 32) | (uint)y（原版 City 无稳定 id，坐标唯一）
            public long KingdomId;
            public string Name;            // 城市名（主线程采集 SafeCityName，后台聚合贸易对时只读）
            public int Gold;               // amount_gold（属性）
            public int Buildings;          // countBuildings()：兜底容量基准 = Buildings × TradeCityBaseCapacity × 50%
            public int Boats;              // countBoats()
            public int StockpileCap;       // 原版仓库真实容量（ResourceLibrary.gold.storage_max）；0 = 不可用 → 兜底估算
            public int TileX;
            public int TileY;
        }

        /// <summary>贸易边（主线程寻路/建边缓存，纯数据）。routeType：0=陆路 1=海路。</summary>
        public struct TradeEdge
        {
            public long CityAId;
            public long CityBId;
            public long KingdomAId;
            public long KingdomBId;
            public float Cost;             // 综合成本（含海路惩罚与邻国加成）
            public byte RouteType;
        }

        /// <summary>单条贸易流（后台计算，主线程 ApplyTradeFlows 按边结算）。</summary>
        public struct TradeFlow
        {
            public long FromCityId;
            public long ToCityId;
            public long FromKingdomId;
            public long ToKingdomId;
            public long Amount;
            public bool Sea;               // 海路（出口受王国 Boats × 每船载量上限约束）
        }

        /// <summary>王国级模拟结果（后台计算，主线程只读消费）。</summary>
        public class KingdomSim
        {
            public long KingdomId;
            public string Name;
            public long GDP;
            public float AvgWealth;
            public float Gini;
            public int ActorCount;
            public int Population;
            public int Capacity;
            public float FoodPerCapita;
            public float Pressure;    // 人口/承载（超载 &gt;1）
            public long TradeBalance; // 净贸易顺差（正=出口盈余，负=逆差）
            public int Workers;       // 有职业人口
            public float Productivity; // 平均劳动生产率（职业倍率均值）
            public float Production;  // 年产出 = Workers × Productivity × CapitalFactor（生产函数）
            public int Specialty;     // BiomeSpecialty（主线程采集阶段读取的纯数据）
            public float LocalPrice;  // 区域价格指数（全局 CPI × 本地供需系数，1.0=基准）
            public float CityX;       // 首都城市 x 坐标（纯数据，供距离计算）
            public float CityY;       // 首都城市 y 坐标
        }

        /// <summary>一轮周期模拟结果。</summary>
        public class CycleResult
        {
            public float GlobalGDP;
            public float AvgWealth;
            public float GiniCoefficient;
            public int AliveActorCount;
            public int CycleIndex;
            public long TotalTradeVolume; // 全图出口总额（各边出口之和）
            public float TotalProduction; // 全球年总产出（生产函数供给侧）
            public float PriceDispersion; // 区域价格离散度（本地价格变异系数 CV，0=各地同价；套利权重推导输入）

            // ===== 地理贸易实际生效值（v0.11：自适应推导，供 ComputeTrade 使用）=====
            public float DistanceDecay;     // 实际生效的距离衰减系数（推导后 clamp）
            public float TransportCost;     // 实际生效的运输成本比例（推导后 clamp）
            public float PriceDiffWeight;   // 实际生效的价格差（套利）权重（推导后 clamp）
            internal float NextSmDecay;
            internal float NextSmTransport;
            internal float NextSmPriceW;

            public readonly List<KingdomSim> Kingdoms = new List<KingdomSim>(16);
            public readonly List<TradeFlow> TradeFlows = new List<TradeFlow>(128); // 本周期按边结算明细
            public readonly List<TradeBalance> CityBalances = new List<TradeBalance>(64);    // 按城市净贸易额（降序）
            public readonly List<TradeBalance> KingdomBalances = new List<TradeBalance>(16); // 按国家净贸易额（降序）
        }

        // ===== 职业代码 → 生产率倍率（纯数据，后台线程安全；与 LaborEngine.JobCodeOf 对应）=====
        private static readonly float[] JobProductivity =
        {
            0.4f, // 0 无业
            1.0f, // 1 farmer
            1.1f, // 2 hunter
            0.9f, // 3 woodcutter
            1.3f, // 4 miner / miner_deposit
            1.0f, // 5 builder
            0.8f  // 6 其他
        };

        /// <summary>职业代码 → 财富生产率倍率（供 LaborEngine 与后台聚合共用）。</summary>
        public static float ProductivityOf(byte code)
            => code < JobProductivity.Length ? JobProductivity[code] : JobProductivity[6];

        /// <summary>海路每船载量：海路贸易量上限 = 出口王国 Boats × 该值（内部常量，可参数化）。</summary>
        private const int SeaCapacityPerBoat = 10;

        // ===== 主线程采集缓冲（每周期 Clear 复用，避免 GC 分配）=====
        private static List<ActorRecord> _collectActors = new List<ActorRecord>(4096);
        private static List<KingdomFacts> _collectKingdoms = new List<KingdomFacts>(32);
        private static List<CitySnapshot> _collectCities = new List<CitySnapshot>(128);
        private static List<TradeEdge> _collectEdges = new List<TradeEdge>(512);
        private static readonly List<Actor> _unitPool = new List<Actor>(64); // 贸易逆差扣款兜底
        private static Dictionary<long, City> _flowCityRefs; // cityId → City（主线程采集期缓存，供 ApplyTradeFlows O(1) 反查）
        private static Dictionary<long, long> _residentOwedByKingdom = new Dictionary<long, long>(16);
        private static Dictionary<long, long> _residentPaidByKingdom = new Dictionary<long, long>(16);
        private static readonly List<long> _residentKingdomIds = new List<long>(16);
        private static readonly List<long> _unpaidByFlow = new List<long>(128);
        private static readonly List<City> _exportCityByFlow = new List<City>(128);

        // ===== 后台计算缓冲与握手 =====
        private static List<ActorRecord> _computeActors = new List<ActorRecord>(4096);
        private static List<KingdomFacts> _computeKingdoms = new List<KingdomFacts>(32);
        private static List<CitySnapshot> _computeCities = new List<CitySnapshot>(128);
        private static List<TradeEdge> _computeEdges = new List<TradeEdge>(512);
        private static volatile bool _posting;    // 主线程：周期已提交待消费
        private static volatile bool _computing;  // 后台线程：计算进行中
        private static volatile CycleResult _readyResult; // 后台完成的待消费结果
        private static volatile string _workerError;      // 后台线程异常信息（主线程消费时记录日志，避免后台线程调用 Unity API）
        private static int _cycleIndex;
        private static int _generation;           // 代际计数：防止过期后台任务写入结果
        private static int _activeWorkers;         // Reset 后仍在退出的旧任务；归零前禁止复用计算缓冲
        private static readonly object _lifecycleLock = new object();

        // ===== 自适应贸易参数 EMA 平滑状态（v0.11：后台单任务互斥，无竞态；Reset 归零）=====
        private static float _smDecay = 0.02f;     // EMA 距离衰减
        private static float _smTransport = 0.05f; // EMA 运输成本
        private static float _smPriceW = 0.3f;     // EMA 套利权重

        /// <summary>最近一次已消费的结果（供各引擎读取；主线程使用，不跨周期持有）。</summary>
        public static CycleResult LastResult { get; private set; }

        // ===== 寻路缓存（主线程维护，纯数据，跨周期保留）=====

        /// <summary>待寻路城市对（主线程队列，每周期限流消费）。</summary>
        private struct CityPair
        {
            public long A, B;   // 城市 id
            public long KA, KB; // 归属王国 id
        }

        private static Dictionary<(long, long), TradeEdge> _edgeCache =
            new Dictionary<(long, long), TradeEdge>(512);   // key = KeyOf(cityAId, cityBId)
        private static readonly Queue<CityPair> _pathfindQueue = new Queue<CityPair>(256);
        private static readonly HashSet<long> _knownKingdoms = new HashSet<long>(32);
        private static readonly HashSet<long> _knownCities = new HashSet<long>(128);
        private static readonly HashSet<long> _currentKingdoms = new HashSet<long>(32);
        private static readonly HashSet<long> _currentCities = new HashSet<long>(128);
        private static readonly HashSet<(long, long)> _knownNeighborPairs = new HashSet<(long, long)>();
        private static readonly HashSet<(long, long)> _currentNeighborPairs = new HashSet<(long, long)>();
        private static Dictionary<long, (long, int, int)> _knownCityTopology =
            new Dictionary<long, (long, int, int)>(128);
        private static readonly List<(long, long)> _staleEdgeKeys = new List<(long, long)>(16);
        private static readonly List<long> _kingdomIdPool = new List<long>(32);
        private static readonly Dictionary<long, List<CitySnapshot>> _kingdomCitiesScratch =
            new Dictionary<long, List<CitySnapshot>>(8);   // 王国 → 城市索引（周期复用）
        private static readonly List<List<CitySnapshot>> _kingdomCityListPool =
            new List<List<CitySnapshot>>(8);
        private static int _routeCycle;                     // PrepareRoutes 调用计数（全量重算节奏）
        private static int _routeMaxEdges = -1;

        // ===== 主线程采集入口 =====

        /// <summary>开始新一轮采集（清空纯数据缓冲）。</summary>
        public static void BeginCycle()
        {
            _collectActors.Clear();
            _collectKingdoms.Clear();
            _collectCities.Clear();
            _collectEdges.Clear();
        }

        public static void AddActor(float wealth, long kingdomId, byte jobCode)
        {
            _collectActors.Add(new ActorRecord { Wealth = wealth, KingdomId = kingdomId, JobCode = jobCode });
        }

        public static void AddKingdom(long id, string name, int population, int capacity,
            long food, int cities, int boats, int specialty, float cityX, float cityY)
        {
            _collectKingdoms.Add(new KingdomFacts
            {
                Id = id, Name = name, Population = population, Capacity = capacity,
                Food = food, Cities = cities, Boats = boats, Specialty = specialty,
                CityX = cityX, CityY = cityY
            });
        }

        /// <summary>采集一个城市快照（地理贸易网络节点）。stockpileCap = 原版仓库真实容量，0 = 兜底估算。</summary>
        public static void AddCitySnapshot(long cityId, long kingdomId, string name, int gold, int buildings,
            int boats, int stockpileCap, int tileX, int tileY)
        {
            _collectCities.Add(new CitySnapshot
            {
                CityId = cityId, KingdomId = kingdomId, Name = name, Gold = gold, Buildings = buildings,
                Boats = boats, StockpileCap = stockpileCap, TileX = tileX, TileY = tileY
            });
        }

        /// <summary>
        /// 主线程寻路限流采集（须在 PostCycle 之前、采集完城市快照后调用）：
        /// 更新寻路缓存（邻国建边 / 非邻国 raycast 确认），并把缓存复制到本周期边缓冲。
        /// cityRefs 为主线程持有的 cityId → City 映射（仅此处使用，后台线程不接触）。
        /// </summary>
        public static void PrepareRoutes(Dictionary<long, City> cityRefs)
        {
            // 缓存本周期 cityId→City 映射，供 ApplyTradeFlows 用 O(1) 反查替代逐王国遍历（性能优化）
            _flowCityRefs = cityRefs;
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.TradeEnabled)
            {
                _edgeCache.Clear();
                _pathfindQueue.Clear();
                _collectEdges.Clear();
                return;
            }

            _routeCycle++;
            bool fullRebuild = (_routeCycle % Mathf.Max(1, cfg.PathRecomputeEvery)) == 0;
            int maxEdges = Mathf.Max(0, cfg.MaxEdges);

            // 王国/城市生灭、归属和坐标变化检测。集合为主线程静态缓冲，不进入后台 Compute。
            _currentKingdoms.Clear();
            _currentCities.Clear();
            bool cityTopologyChanged = false;
            for (int i = 0; i < _collectCities.Count; i++)
            {
                var city = _collectCities[i];
                _currentKingdoms.Add(city.KingdomId);
                _currentCities.Add(city.CityId);
                if (!_knownCityTopology.TryGetValue(city.CityId, out var topology)
                    || topology.Item1 != city.KingdomId || topology.Item2 != city.TileX || topology.Item3 != city.TileY)
                    cityTopologyChanged = true;
            }

            bool kingdomsChanged = _currentKingdoms.Count != _knownKingdoms.Count;
            if (!kingdomsChanged)
            {
                foreach (var k in _currentKingdoms)
                    if (!_knownKingdoms.Contains(k)) { kingdomsChanged = true; break; }
            }

            bool citiesChanged = _currentCities.Count != _knownCities.Count;
            if (!citiesChanged)
            {
                foreach (var c in _currentCities)
                    if (!_knownCities.Contains(c)) { citiesChanged = true; break; }
            }
            citiesChanged |= cityTopologyChanged;

            _currentNeighborPairs.Clear();
            var kingdomIds = _kingdomIdPool;
            kingdomIds.Clear();
            foreach (var kingdomId in _currentKingdoms) kingdomIds.Add(kingdomId);
            kingdomIds.Sort();
            for (int i = 0; i < kingdomIds.Count; i++)
            {
                for (int j = i + 1; j < kingdomIds.Count; j++)
                {
                    if (AreNeighborKingdoms(kingdomIds[i], kingdomIds[j]))
                        _currentNeighborPairs.Add((kingdomIds[i], kingdomIds[j]));
                }
            }
            bool neighborsChanged = _currentNeighborPairs.Count != _knownNeighborPairs.Count;
            if (!neighborsChanged)
            {
                foreach (var pair in _currentNeighborPairs)
                    if (!_knownNeighborPairs.Contains(pair)) { neighborsChanged = true; break; }
            }

            if (kingdomsChanged || citiesChanged || neighborsChanged)
            {
                RemoveStaleEdges(_currentKingdoms, _currentCities);
                _knownKingdoms.Clear();
                foreach (var k in _currentKingdoms) _knownKingdoms.Add(k);
                _knownCities.Clear();
                foreach (var c in _currentCities) _knownCities.Add(c);
                _knownCityTopology.Clear();
                for (int i = 0; i < _collectCities.Count; i++)
                {
                    var city = _collectCities[i];
                    _knownCityTopology[city.CityId] = (city.KingdomId, city.TileX, city.TileY);
                }
                _knownNeighborPairs.Clear();
                foreach (var pair in _currentNeighborPairs) _knownNeighborPairs.Add(pair);
            }

            if (fullRebuild || kingdomsChanged || citiesChanged || neighborsChanged || _routeMaxEdges != maxEdges
                || _edgeCache.Count > maxEdges)
            {
                // 候选集整体失效：旧边即使仍引用有效城市，也不能泄漏到新的稀疏图。
                _edgeCache.Clear();
                _pathfindQueue.Clear();
                _routeMaxEdges = maxEdges;
                EnqueueCandidatePairs(cfg, maxEdges);
            }

            // 限流寻路（PathfinderTools 仅主线程可调；结果 upsert 缓存）
            int budget = Mathf.Max(1, cfg.MaxPathfindPairs);
            while (budget > 0 && _pathfindQueue.Count > 0 && _edgeCache.Count < maxEdges)
            {
                var pair = _pathfindQueue.Dequeue();
                budget--;
                var edge = TryPathfindEdge(cityRefs, pair, cfg);
                if (edge != null && _edgeCache.Count < maxEdges)
                    _edgeCache[KeyOf(pair.A, pair.B)] = edge.Value;
            }

            // 缓存快照 → 本周期边缓冲（纯值拷贝，后台线程只读）
            _collectEdges.Clear();
            if (_edgeCache.Count > _collectEdges.Capacity)
                _collectEdges.Capacity = _edgeCache.Count;
            foreach (var kv in _edgeCache) _collectEdges.Add(kv.Value);
            _collectEdges.Sort(CompareEdges);
        }

        /// <summary>删除已消失王国或城市涉及的全部边（缓存维护，主线程 O(N) 过滤）。</summary>
        private static void RemoveStaleEdges(HashSet<long> curKingdoms, HashSet<long> curCities)
        {
            _staleEdgeKeys.Clear();
            foreach (var kv in _edgeCache)
            {
                var e = kv.Value;
                if (!curKingdoms.Contains(e.KingdomAId) || !curKingdoms.Contains(e.KingdomBId)
                    || !curCities.Contains(e.CityAId) || !curCities.Contains(e.CityBId))
                    _staleEdgeKeys.Add(kv.Key);
            }
            for (int i = 0; i < _staleEdgeKeys.Count; i++) _edgeCache.Remove(_staleEdgeKeys[i]);
        }

        /// <summary>
        /// 重建候选队列（主线程，纯数据）：
        /// - 每个王国先保留一条国内骨架边，再加入邻国边，最后用额外国内近邻填充预算；
        /// - 跨王国对以 TileX 二分 + 双向固定窗口近邻候选，选最近两条不同城市边；
        /// - 非邻国王国对 → 取近邻候选中的最近城市对，≤ MaxTradeRange 才入寻路队列。
        /// 成功边由 MaxEdges 硬限制；待寻路王国对使用独立有界队列，失败会释放成功边预算。
        /// 跨王国城市候选复杂度 O(A log B + B log A)，窗口检查为常数。
        /// </summary>
        private static void EnqueueCandidatePairs(UnrestConfig cfg, int maxEdges)
        {
            // 王国 → 城市索引（静态 scratch + 列表池复用，周期路径零集合分配）
            var kingdomCities = _kingdomCitiesScratch;
            foreach (var list in kingdomCities.Values)
            {
                list.Clear();
                _kingdomCityListPool.Add(list);
            }
            kingdomCities.Clear();
            for (int i = 0; i < _collectCities.Count; i++)
            {
                var cs = _collectCities[i];
                if (!kingdomCities.TryGetValue(cs.KingdomId, out var list))
                {
                    list = RentKingdomCityList();
                    kingdomCities[cs.KingdomId] = list;
                }
                list.Add(cs);
            }

            var ids = _kingdomIdPool;
            ids.Clear();
            foreach (var kingdomId in kingdomCities.Keys) ids.Add(kingdomId);
            ids.Sort();
            for (int i = 0; i < ids.Count; i++)
                kingdomCities[ids[i]].Sort(CompareCities);

            // 王国级邻接：原版城市邻国数据（City.neighbours_kingdoms 等）为 internal 不可访问，
            // 以王国几何距离为公开 API 代理——distanceBetweenKingdom ≤ MaxTradeRange ⇒ 邻国

            // 每国先保留一条最小国内骨架边，防止小预算下国内贸易完全消失。
            for (int k = 0; k < ids.Count; k++)
            {
                var list = kingdomCities[ids[k]];
                if (list.Count > 1 && !TryAddDirectEdge(list[0], list[1], maxEdges)) return;
            }

            float maxRange = Mathf.Max(0f, cfg.MaxTradeRange);

            // 邻国优先占用预算，每对王国只添加确定性选出的最近两条城市边。
            for (int i = 0; i < ids.Count; i++)
            {
                for (int j = i + 1; j < ids.Count; j++)
                {
                    long k1 = ids[i], k2 = ids[j];
                    var c1 = kingdomCities[k1];
                    var c2 = kingdomCities[k2];
                    if (_currentNeighborPairs.Contains((k1, k2)))
                    {
                        FindNearestTwoPairs(c1, c2, out var a1, out var b1, out var a2, out var b2, out int found);
                        if (found > 0 && !TryAddDirectEdge(a1, b1, maxEdges)) return;
                        if (found > 1 && !TryAddDirectEdge(a2, b2, maxEdges)) return;
                    }
                }
            }

            // 用剩余预算补充国内排序近邻；每城最多连接前后各两个城市。
            for (int k = 0; k < ids.Count; k++)
            {
                var list = kingdomCities[ids[k]];
                for (int a = 0; a < list.Count; a++)
                {
                    int end = Mathf.Min(list.Count, a + 3);
                    for (int b = a + 1; b < end; b++)
                        if (!TryAddDirectEdge(list[a], list[b], maxEdges)) goto DirectEdgesFull;
                }
            }
        DirectEdgesFull:

            // 非邻国：每对王国仅将最近一对加入有界寻路队列。队列不预占成功边预算，
            // 因而失败候选不会让后续可达候选永久失去机会。
            int maxQueuedCandidates = Mathf.Max(maxEdges, Mathf.Max(1, cfg.MaxPathfindPairs) * 4);
            for (int i = 0; i < ids.Count; i++)
            {
                for (int j = i + 1; j < ids.Count; j++)
                {
                    long k1 = ids[i], k2 = ids[j];
                    if (_currentNeighborPairs.Contains((k1, k2))) continue;
                    var c1 = kingdomCities[k1];
                    var c2 = kingdomCities[k2];
                    FindNearestTwoPairs(c1, c2, out var ba, out var bb, out _, out _, out int found);
                    if (found > 0 && Dist(ba, bb) <= maxRange)
                    {
                        if (_pathfindQueue.Count >= maxQueuedCandidates) return;
                        _pathfindQueue.Enqueue(new CityPair
                        {
                            A = ba.CityId, B = bb.CityId, KA = ba.KingdomId, KB = bb.KingdomId
                        });
                    }
                }
            }
        }

        private static int CompareCities(CitySnapshot a, CitySnapshot b)
        {
            int cmp = a.TileX.CompareTo(b.TileX);
            if (cmp != 0) return cmp;
            cmp = a.TileY.CompareTo(b.TileY);
            return cmp != 0 ? cmp : a.CityId.CompareTo(b.CityId);
        }

        /// <summary>从池租用王国城市列表（池空时增长；按峰值王国数有界，周期路径无 new List）。</summary>
        private static List<CitySnapshot> RentKingdomCityList()
        {
            if (_kingdomCityListPool.Count > 0)
            {
                int last = _kingdomCityListPool.Count - 1;
                var list = _kingdomCityListPool[last];
                _kingdomCityListPool.RemoveAt(last);
                return list;
            }
            return new List<CitySnapshot>(8);
        }

        private static int CompareEdges(TradeEdge a, TradeEdge b)
        {
            var aKey = KeyOf(a.CityAId, a.CityBId);
            var bKey = KeyOf(b.CityAId, b.CityBId);
            int cmp = aKey.Item1.CompareTo(bKey.Item1);
            return cmp != 0 ? cmp : aKey.Item2.CompareTo(bKey.Item2);
        }

        private static bool TryAddDirectEdge(CitySnapshot a, CitySnapshot b, int maxEdges)
        {
            var key = KeyOf(a.CityId, b.CityId);
            if (_edgeCache.ContainsKey(key)) return true;
            if (_edgeCache.Count >= maxEdges) return false;
            _edgeCache[key] = new TradeEdge
            {
                CityAId = a.CityId, CityBId = b.CityId,
                KingdomAId = a.KingdomId, KingdomBId = b.KingdomId,
                Cost = Dist(a, b), RouteType = 0
            };
            return true;
        }

        /// <summary>
        /// 在两个按 TileX/TileY/CityId 排序的列表间，以 TileX 二分定位后向两侧扩展；
        /// 当两侧 x 距离下界都大于当前第二优距离时停止，保证精确结果并避免无效笛卡尔积。
        /// </summary>
        private static void FindNearestTwoPairs(List<CitySnapshot> first, List<CitySnapshot> second,
            out CitySnapshot a1, out CitySnapshot b1, out CitySnapshot a2, out CitySnapshot b2, out int found)
        {
            CitySnapshot bestA1 = default(CitySnapshot), bestB1 = default(CitySnapshot);
            CitySnapshot bestA2 = default(CitySnapshot), bestB2 = default(CitySnapshot);
            double d1 = double.MaxValue, d2 = double.MaxValue;
            int bestCount = 0;

            void Consider(CitySnapshot a, CitySnapshot b)
            {
                var key = KeyOf(a.CityId, b.CityId);
                if (bestCount > 0 && key == KeyOf(bestA1.CityId, bestB1.CityId)) return;
                if (bestCount > 1 && key == KeyOf(bestA2.CityId, bestB2.CityId)) return;

                double dx = (double)a.TileX - b.TileX;
                double dy = (double)a.TileY - b.TileY;
                double d = dx * dx + dy * dy;
                if (bestCount == 0 || ComparePair(d, a, b, d1, bestA1, bestB1) < 0)
                {
                    d2 = d1; bestA2 = bestA1; bestB2 = bestB1;
                    d1 = d; bestA1 = a; bestB1 = b;
                    if (bestCount < 2) bestCount++;
                }
                else if (bestCount == 1 || ComparePair(d, a, b, d2, bestA2, bestB2) < 0)
                {
                    d2 = d; bestA2 = a; bestB2 = b;
                    bestCount = 2;
                }
            }

            void Scan(List<CitySnapshot> source, List<CitySnapshot> target, bool swapped)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    int low = 0, high = target.Count;
                    while (low < high)
                    {
                        int mid = low + ((high - low) >> 1);
                        if (target[mid].TileX < source[i].TileX) low = mid + 1;
                        else high = mid;
                    }

                    int left = low - 1;
                    int right = low;
                    while (left >= 0 || right < target.Count)
                    {
                        double leftDx = left >= 0
                            ? (double)target[left].TileX - source[i].TileX : double.MaxValue;
                        double rightDx = right < target.Count
                            ? (double)target[right].TileX - source[i].TileX : double.MaxValue;
                        leftDx *= leftDx;
                        rightDx *= rightDx;
                        double bound = bestCount > 1 ? d2 : double.MaxValue;
                        if (leftDx > bound && rightDx > bound) break;

                        CitySnapshot candidate;
                        if (leftDx <= rightDx) candidate = target[left--];
                        else candidate = target[right++];
                        if (swapped) Consider(candidate, source[i]);
                        else Consider(source[i], candidate);
                    }
                }
            }

            if (first.Count <= second.Count)
            {
                Scan(first, second, false);
            }
            else
            {
                Scan(second, first, true);
            }

            a1 = bestA1; b1 = bestB1;
            a2 = bestA2; b2 = bestB2;
            found = bestCount;
        }

        private static int ComparePair(double distance, CitySnapshot a, CitySnapshot b,
            double otherDistance, CitySnapshot otherA, CitySnapshot otherB)
        {
            int cmp = distance.CompareTo(otherDistance);
            if (cmp != 0) return cmp;
            var key = KeyOf(a.CityId, b.CityId);
            var otherKey = KeyOf(otherA.CityId, otherB.CityId);
            cmp = key.Item1.CompareTo(otherKey.Item1);
            return cmp != 0 ? cmp : key.Item2.CompareTo(otherKey.Item2);
        }

        /// <summary>城市对欧氏距离（tile 格数）。</summary>
        private static float Dist(CitySnapshot a, CitySnapshot b)
        {
            float dx = a.TileX - b.TileX;
            float dy = a.TileY - b.TileY;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 邻国判定（仅主线程调用，可访问 Unity 对象）：
        /// 原版城市级邻国数据（City.neighbours_kingdoms / neighbours_cities_kingdom 等）均为
        /// internal（fdAssembly），模组程序集不可访问；以王国几何距离为公开 API 代理——
        /// Kingdom.distanceBetweenKingdom(a, b) ≤ MaxTradeRange ⇒ 视为邻国（直接建边、无惩罚）。
        /// 王国对象缺失（已消亡/野生）或调用异常时按非邻国处理。
        /// </summary>
        private static bool AreNeighborKingdoms(long k1, long k2)
        {
            if (k1 == k2) return false;
            var a = GameHelpers.FindKingdom(k1);
            var b = GameHelpers.FindKingdom(k2);
            if (a == null || b == null) return false;
            try
            {
                return Kingdom.distanceBetweenKingdom(a, b)
                    <= Mathf.Max(1f, UnrestConfig.Instance.MaxTradeRange);
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>城市对缓存键（无序对）。</summary>
        private static (long, long) KeyOf(long a, long b) => a <= b ? (a, b) : (b, a);

        /// <summary>
        /// 非邻国城市对寻路确认（主线程）：raycast 空路径 ⇒ 不可达 ⇒ 无贸易边。
        /// 路径海洋占比 &gt; 50% ⇒ 海路（cost = 欧氏距离 × SeaRoutePenalty），否则陆路。
        /// 非邻国加成 NonNeighborPenalty 乘在基础 cost 上。
        /// </summary>
        private static TradeEdge? TryPathfindEdge(Dictionary<long, City> cityRefs, CityPair pair, UnrestConfig cfg)
        {
            City ca, cb;
            if (!cityRefs.TryGetValue(pair.A, out ca) || !cityRefs.TryGetValue(pair.B, out cb))
                return null;
            try
            {
                var ta = ca.getTile(false);
                var tb = cb.getTile(false);
                if (ta == null || tb == null) return null;

                // 路径与成本：优先 A* 真实路径（绕山跨海的实际步数）；A* 失败时回退 raycast
                // 直线判定（连通性只增不减）。海陆判定统一为路径点海洋占比（抽样前 512 点）。
                List<WorldTile> path = TryGetAStarPath(ta, tb, cfg);
                bool astarUsed = path != null;
                if (path == null)
                {
                    path = PathfinderTools.raycast(ta, tb, 1f);
                    if (path == null || path.Count == 0) return null; // 不可达 ⇒ 无贸易边
                }

                // 海陆判定：路径点海洋占比（抽样前 512 点，防极端长路径主线程卡顿）
                // 注：WorldTile.IsOceanAround 为 internal 不可访问，用公开的 isWaterAround() 等价判定
                int scan = Mathf.Min(path.Count, 512);
                int sea = 0;
                for (int i = 0; i < scan; i++)
                {
                    var t = path[i];
                    if (t == null) continue;
                    try { if (t.isWaterAround()) sea++; } catch (System.Exception) { }
                }
                bool isSea = sea * 2 > scan; // 海洋占比 > 50%

                // 成本基准：A* 用真实路径长度（绕行山岭/海湾自然更贵）；raycast 回退用直线距离
                float length = astarUsed ? path.Count
                    : Mathf.Sqrt((ta.x - tb.x) * (ta.x - tb.x) + (ta.y - tb.y) * (ta.y - tb.y));
                length = Mathf.Min(length, 1024f); // 极端长路径封顶，避免单边成本畸高
                float baseCost = isSea ? length * Mathf.Max(1f, cfg.SeaRoutePenalty) : length;
                float cost = baseCost * Mathf.Max(1f, cfg.NonNeighborPenalty);

                return new TradeEdge
                {
                    CityAId = pair.A, CityBId = pair.B,
                    KingdomAId = pair.KA, KingdomBId = pair.KB,
                    Cost = cost, RouteType = (byte)(isSea ? 1 : 0)
                };
            }
            catch (System.Exception)
            {
                return null; // 寻路异常视为不可达
            }
        }

        // ===== A* 真实路径（陆路）=====
        // 原版入口 PathfinderTools.tryToGetSimplePath(WorldTile, WorldTile, List<WorldTile>, ActorAsset,
        // EpPathFinding.cs.AStarParam, Int32)：按 ActorAsset 移动规则做 A*。经静态缓存探测可用性，
        // 不可用（API 缺失/无陆行资产/异常）时返回 null，调用方回退 raycast，绝不因 A* 失败丢边。

        private static bool _astarProbed;
        private static bool _astarAvailable;
        private static ActorAsset _astarAsset;

        /// <summary>A* 陆路真实路径；返回 null 表示 A* 不可用或不可达（调用方回退 raycast）。</summary>
        private static List<WorldTile> TryGetAStarPath(WorldTile from, WorldTile to, UnrestConfig cfg)
        {
            if (!cfg.TradeAstarEnabled) return null;
            if (!_astarProbed)
            {
                _astarProbed = true;
                try
                {
                    // 陆行资产候选：任一存在即可（A* 仅用于距离度量，种族差异不影响通行地形大类）
                    foreach (var id in new string[] { "human", "elf", "orc", "dwarf" })
                    {
                        try { _astarAsset = AssetManager.actor_library.get(id); } catch (System.Exception) { }
                        if (_astarAsset != null) break;
                    }
                    _astarAvailable = _astarAsset != null;
                }
                catch (System.Exception) { _astarAvailable = false; }
            }
            if (!_astarAvailable) return null;

            try
            {
                var path = _astarPathPool;
                path.Clear();
                var param = new EpPathFinding.cs.AStarParam(); // 简单路径封装内部会补全网格/起终点
                if (!PathfinderTools.tryToGetSimplePath(from, to, path, _astarAsset, param, 2048))
                    return null;
                if (path.Count == 0) return null;
                return path;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static readonly List<WorldTile> _astarPathPool = new List<WorldTile>(256);

        /// <summary>
        /// 提交一轮周期：缓冲交换后交由后台线程计算，主线程轮询 <see cref="TryConsume"/> 消费。
        /// 若已有周期在途则拒绝（返回 false）。
        /// </summary>
        public static bool PostCycle()
        {
            if (_posting || _computing || System.Threading.Volatile.Read(ref _activeWorkers) > 0) return false; // 防御：仅允许一轮在途
            SwapBuffers();
            _generation++;
            int idx = _cycleIndex + 1;
            int gen = _generation;
            var actors = _computeActors;
            var kingdoms = _computeKingdoms;
            var cities = _computeCities;
            var edges = _computeEdges;
            _computing = true;
            _posting = true;
            System.Threading.Interlocked.Increment(ref _activeWorkers);
            try
            {
                bool queued = ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var r = Compute(actors, kingdoms, cities, edges, idx);
                        lock (_lifecycleLock)
                        {
                            if (gen == _generation) _readyResult = r;
                        }
                    }
                    catch (Exception e)
                    {
                        // 后台线程禁止调用 Unity API（Debug.Log 非线程安全，可能引发原生崩溃），
                        // 只记录异常文本，由主线程 TryConsume 消费时输出日志并兜底重算。
                        lock (_lifecycleLock)
                        {
                            if (gen == _generation)
                            {
                                _workerError = e.Message;
                                _readyResult = null;
                            }
                        }
                    }
                    finally
                    {
                        bool stale;
                        lock (_lifecycleLock)
                        {
                            stale = gen != _generation;
                            if (!stale) _computing = false;
                        }
                        if (stale)
                        {
                            actors.Clear();
                            kingdoms.Clear();
                            cities.Clear();
                            edges.Clear();
                        }
                        System.Threading.Interlocked.Decrement(ref _activeWorkers);
                    }
                });
                if (!queued) throw new InvalidOperationException("ThreadPool rejected work item");
                _cycleIndex = idx;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 后台线程提交失败: " + e.Message);
                System.Threading.Interlocked.Decrement(ref _activeWorkers);
                _computing = false;
                _posting = false;
                return false;
            }
        }

        /// <summary>
        /// 主线程轮询：后台结果就绪则发布到 <see cref="EconomyEngine"/>，返回 true 表示本周期收尾。
        /// 后台失败（异常）时主线程同步兜底重算，保证周期不丢。
        /// </summary>
        public static bool TryConsume()
        {
            if (!_posting || _computing || System.Threading.Volatile.Read(ref _activeWorkers) > 0) return false;
            // 后台线程异常：主线程在此输出日志（后台线程不允许调用 Unity API）
            var err = _workerError;
            _workerError = null;
            if (err != null) Debug.LogWarning("[ClassicalEconomics] 后台统计失败，主线程兜底重算: " + err);
            CycleResult res;
            lock (_lifecycleLock)
            {
                if (!_posting || _computing) return false;
                res = _readyResult;
                _readyResult = null;
                _posting = false;
            }
            if (res == null)
            {
                res = Compute(_computeActors, _computeKingdoms, _computeCities, _computeEdges, _cycleIndex); // 兜底：同步重算
            }
            Publish(res);
            return true;
        }

        /// <summary>是否已有周期在途（已提交未消费 / 后台计算中）。调用方在发起新周期或同步计算前应检查。</summary>
        public static bool IsBusy() => _posting || _computing || System.Threading.Volatile.Read(ref _activeWorkers) > 0;

        /// <summary>后台结果是否已就绪但尚未被消费（供周期驱动器自愈：_posting 遗留但无人消费时兜底置位）。</summary>
        public static bool HasPendingResult() => _posting && !_computing
            && System.Threading.Volatile.Read(ref _activeWorkers) == 0;

        /// <summary>进入主菜单时使旧世界任务失效，并解除所有可立即释放的世界数据。</summary>
        public static void ClearWorldReferences()
        {
            lock (_lifecycleLock)
            {
                _generation++;
                _posting = false;
                _computing = false;
                _readyResult = null;
                _workerError = null;
                LastResult = null;
                if (System.Threading.Volatile.Read(ref _activeWorkers) == 0)
                {
                    _computeActors.Clear();
                    _computeKingdoms.Clear();
                    _computeCities.Clear();
                    _computeEdges.Clear();
                }
            }
            _flowCityRefs = null;
            _unitPool.Clear();
            _exportCityByFlow.Clear();
            _unpaidByFlow.Clear();
            _residentOwedByKingdom.Clear();
            _residentPaidByKingdom.Clear();
            _residentKingdomIds.Clear();
            _currentKingdoms.Clear();
            _currentCities.Clear();
            _currentNeighborPairs.Clear();
            _staleEdgeKeys.Clear();
            _kingdomIdPool.Clear();
        }

        /// <summary>
        /// 手动采集/实时刷新：同步计算并立即发布（按钮触发，不等后台线程）。
        /// advanceCycle=false 用于实时刷新（不推进周期号，避免 HUD"周期 #N"暴涨）。
        /// 注意：调用前须先完成采集（DataCollector.Collect 内部已跑 PrepareRoutes + PostCycle），
        /// 此处复用已交换的缓冲。
        /// </summary>
        public static void ComputeAndConsumeSync(bool advanceCycle = true)
        {
            if (System.Threading.Volatile.Read(ref _activeWorkers) > 0)
                throw new InvalidOperationException("Cannot run synchronous trade computation while a worker is active.");
            if (!_posting)
            {
                SwapBuffers();
                if (advanceCycle) _cycleIndex++;
            }
            // 丢弃在途后台任务（若有），以同步结果为准
            lock (_lifecycleLock)
            {
                _generation++;
                _computing = false;
                _posting = false;
                _readyResult = null;
                _workerError = null;
            }
            Publish(Compute(_computeActors, _computeKingdoms, _computeCities, _computeEdges, _cycleIndex));
        }

        /// <summary>世界重置（新地图/新游戏）时清空在途周期、结果与寻路缓存。</summary>
        public static void Reset()
        {
            ClearWorldReferences();
            _cycleIndex = 0;
            _routeCycle = 0;
            _collectActors.Clear();
            _collectKingdoms.Clear();
            _collectCities.Clear();
            _collectEdges.Clear();
            _edgeCache.Clear();
            _pathfindQueue.Clear();
            _knownKingdoms.Clear();
            _knownCities.Clear();
            _currentKingdoms.Clear();
            _currentCities.Clear();
            _knownNeighborPairs.Clear();
            _currentNeighborPairs.Clear();
            _knownCityTopology.Clear();
            _staleEdgeKeys.Clear();
            _kingdomIdPool.Clear();
            _routeMaxEdges = -1;
            lock (_lifecycleLock)
            {
                _smDecay = 0.02f;
                _smTransport = 0.05f;
                _smPriceW = 0.3f;
            }
        }

        private static void SwapBuffers()
        {
            var ta = _computeActors; _computeActors = _collectActors; _collectActors = ta; _collectActors.Clear();
            var tk = _computeKingdoms; _computeKingdoms = _collectKingdoms; _collectKingdoms = tk; _collectKingdoms.Clear();
            var tc = _computeCities; _computeCities = _collectCities; _collectCities = tc; _collectCities.Clear();
            var te = _computeEdges; _computeEdges = _collectEdges; _collectEdges = te; _collectEdges.Clear();
        }

        private static void Publish(CycleResult res)
        {
            lock (_lifecycleLock)
            {
                _smDecay = res.NextSmDecay;
                _smTransport = res.NextSmTransport;
                _smPriceW = res.NextSmPriceW;
            }
            LastResult = res;
            EconomyEngine.PublishResult(res);
        }

        // ===== 后台线程计算（纯数据，零 Unity 对象访问）=====

        // internal：MemoryCleanupEngine 的 AccScratchForTrim 访问器需要在程序集内暴露该类型
        internal class Accum
        {
            public double Gdp;
            public int Count;
            public int Workers;
            public double ProdSum;
            public readonly List<float> Wealths = new List<float>(256);

            public void Clear()
            {
                Gdp = 0d;
                Count = 0;
                Workers = 0;
                ProdSum = 0d;
                Wealths.Clear();
            }
        }

        // Compute is single-flight. Reusing these buffers avoids rebuilding population-sized
        // arrays and dictionaries every accelerated game year.
        private static Dictionary<long, Accum> _accScratch = new Dictionary<long, Accum>(32);
        private static readonly List<Accum> _accPool = new List<Accum>(32);
        private static readonly List<float> _globalWealthScratch = new List<float>(4096);
        private static readonly List<int> _validKingdomScratch = new List<int>(32);
        private static Dictionary<long, int> _cityIndexScratch = new Dictionary<long, int>(128);
        private static Dictionary<long, int> _kingdomIndexScratch = new Dictionary<long, int>(32);
        private static Dictionary<long, int> _boatsScratch = new Dictionary<long, int>(32);
        private static Dictionary<long, decimal> _seaCapacityScratch = new Dictionary<long, decimal>(8);
        private static readonly List<TradeEdge> _usableEdgesScratch = new List<TradeEdge>(512);
        private static long[] _kingdomExportScratch = new long[32];
        private static long[] _kingdomImportScratch = new long[32];
        private static long[] _cityExportScratch = new long[128];
        private static long[] _cityImportScratch = new long[128];
        private static float[] _priceScratch = new float[32];

        private static CycleResult Compute(List<ActorRecord> actors, List<KingdomFacts> kingdoms,
            List<CitySnapshot> cities, List<TradeEdge> edges, int cycleIndex)
        {
            var res = new CycleResult { CycleIndex = cycleIndex };

            // --- 全局 + 王国聚合（单遍遍历）---
            double gdp = 0d;
            int count = 0;
            foreach (var old in _accScratch.Values)
            {
                old.Clear();
                _accPool.Add(old);
            }
            var acc = _accScratch;
            acc.Clear();
            var globalWealths = _globalWealthScratch;
            globalWealths.Clear();
            if (globalWealths.Capacity < actors.Count) globalWealths.Capacity = actors.Count;
            foreach (var r in actors)
            {
                double w = r.Wealth;
                gdp += w;
                count++;
                globalWealths.Add(r.Wealth);
                if (!acc.TryGetValue(r.KingdomId, out var a))
                {
                    if (_accPool.Count > 0)
                    {
                        int last = _accPool.Count - 1;
                        a = _accPool[last];
                        _accPool.RemoveAt(last);
                    }
                    else a = new Accum();
                    acc[r.KingdomId] = a;
                }
                a.Gdp += w;
                a.Count++;
                a.Wealths.Add(r.Wealth);
                if (r.JobCode != 0)
                {
                    a.Workers++;
                    a.ProdSum += ProductivityOf(r.JobCode);
                }
            }
            res.GlobalGDP = (float)gdp;
            res.AliveActorCount = count;
            res.AvgWealth = count > 0 ? (float)(gdp / count) : 0f;

            // 全局基尼（升序排序，O(N log N)）
            res.GiniCoefficient = ComputeGini(globalWealths, (float)gdp);

            // --- 王国结果（含生产函数：产出 = Workers × Productivity × CapitalFactor）---
            double totalProduction = 0d;
            foreach (var f in kingdoms)
            {
                var ks = new KingdomSim
                {
                    KingdomId = f.Id,
                    Name = f.Name,
                    Population = f.Population,
                    Capacity = f.Capacity,
                    FoodPerCapita = f.Population > 0 ? (float)f.Food / f.Population : 0f,
                    Pressure = f.Capacity > 0 ? (float)f.Population / f.Capacity : 0f,
                    Specialty = f.Specialty,
                    CityX = f.CityX, // 首都坐标（纯数据透传，供距离计算；NaN=未知）
                    CityY = f.CityY
                };
                if (acc.TryGetValue(f.Id, out var a))
                {
                    ks.GDP = (long)a.Gdp;
                    ks.ActorCount = a.Count;
                    ks.AvgWealth = a.Count > 0 ? (float)(a.Gdp / a.Count) : 0f;
                    ks.Gini = ComputeGini(a.Wealths, (float)a.Gdp);
                    ks.Workers = a.Workers;
                    ks.Productivity = a.Workers > 0 ? (float)(a.ProdSum / a.Workers) : 0f;
                    // 生产函数（差异版）：产出 = Workers × Productivity × CapitalFactor
                    // 降低基础设施权重 + 加入制度质量（低基尼→效率高）+ 规模不经济（大国打折）
                    float employmentRatio = a.Count > 0 ? (float)a.Workers / a.Count : 0f;
                    float governanceBonus = 1f + (1f - Mathf.Clamp01(ks.Gini)) * 0.3f; // 基尼越低→治理越好
                    float scaleEfficiency = 1f / (1f + a.Count * 0.0001f);               // 人口越大→管理成本越高
                    float capitalFactor = (1f + f.Cities * 0.02f + f.Boats * 0.01f)     // 基础设施权重（降低）
                                          * governanceBonus * scaleEfficiency
                                          + employmentRatio * 0.15f;
                    ks.Production = a.Workers * ks.Productivity * capitalFactor;
                    // 法典：生产函数乘数（教育/补贴/计划 vs 自由市场等聚合）
                    ks.Production *= CodexEngine.GetMods(ks.KingdomId).Productivity;
                    totalProduction += ks.Production;
                }
                res.Kingdoms.Add(ks);
            }
            // 无王国桶（id=0）也纳入统计
            if (acc.TryGetValue(0L, out var z))
            {
                var prod0 = z.Workers * (z.Workers > 0 ? (float)(z.ProdSum / z.Workers) : 0f);
                var ks0 = new KingdomSim
                {
                    KingdomId = 0,
                    Name = "无王国",
                    GDP = (long)z.Gdp,
                    ActorCount = z.Count,
                    AvgWealth = z.Count > 0 ? (float)(z.Gdp / z.Count) : 0f,
                    Gini = ComputeGini(z.Wealths, (float)z.Gdp),
                    Workers = z.Workers,
                    Productivity = z.Workers > 0 ? (float)(z.ProdSum / z.Workers) : 0f,
                    Production = prod0 // 无王国无资本加成
                };
                totalProduction += prod0;
                res.Kingdoms.Add(ks0);
            }
            res.TotalProduction = (float)totalProduction;

            // --- 区域价格指数（v0.9 地理贸易）：本地价格 = 上期全局 CPI × 本地供需系数 ---
            // 供给侧：人均产出相对全球均值（产出高→供给充足→本地价低）；
            // 需求侧：人口压力（超载→需求旺盛→本地价高）。无王国桶不参与（LocalPrice=基准 CPI）。
            float baseCPI = EconomyCycleModulator.CurrentCPI; // 上期价格指数（Evaluate 于发布后更新，此处读到即本期基准）
            float totalPop = 0f;
            foreach (var ks in res.Kingdoms)
                if (ks.KingdomId != 0) totalPop += ks.Population;
            float globalPerCapitaProd = totalPop > 0f ? (float)totalProduction / totalPop : 0f;

            float meanPrice = 0f;
            int priceCount = 0;
            foreach (var ks in res.Kingdoms)
            {
                if (ks.KingdomId == 0) { ks.LocalPrice = baseCPI; continue; }
                float localPerCapitaProd = ks.Population > 0f ? ks.Production / ks.Population : 0f;
                float supplyRatio = globalPerCapitaProd > 0f ? localPerCapitaProd / globalPerCapitaProd : 1f;
                float demandRatio = 1f + Mathf.Clamp(ks.Pressure - 1f, -0.5f, 0.5f) * 0.5f; // 超载/空置 ±25%
                float localFactor = (1f / Mathf.Max(supplyRatio, 0.05f)) * Mathf.Max(demandRatio, 0.5f);
                ks.LocalPrice = baseCPI * Mathf.Clamp(localFactor, 0.5f, 2f); // 限幅 0.5×~2× 防极端
                // 中央银行家·关税：本国物价上浮（限幅 0.5×~2.5×）
                if (NationEngine.TariffPriceMult(ks.KingdomId) != 1f)
                    ks.LocalPrice = Mathf.Clamp(ks.LocalPrice * NationEngine.TariffPriceMult(ks.KingdomId), 0.5f, 2.5f);
                // 法典：物价乘数（紧缩/贸易协定等聚合）
                float cp2 = CodexEngine.GetMods(ks.KingdomId).Price;
                if (cp2 != 1f) ks.LocalPrice = Mathf.Clamp(ks.LocalPrice * cp2, 0.5f, 3f);
                meanPrice += ks.LocalPrice;
                priceCount++;
            }
            if (priceCount > 0) meanPrice /= priceCount;

            // 价格离散度 = 本地价格变异系数 CV（标准差/均值，0=各地同价，反映区域套利空间）
            if (priceCount > 1 && meanPrice > 0f)
            {
                double variance = 0d;
                foreach (var ks in res.Kingdoms)
                {
                    if (ks.KingdomId == 0) continue;
                    double d = ks.LocalPrice - meanPrice;
                    variance += d * d;
                }
                variance /= priceCount;
                res.PriceDispersion = (float)(Math.Sqrt(variance) / meanPrice);
            }
            else res.PriceDispersion = 0f;

            // --- 自适应贸易参数推导（v0.11：由游戏状态每周期动态决定，无固定配置）---
            // 距离衰减：大图弱衰减，海路多/船队强 → 远程可行 → 更小衰减；
            // 运输成本：平均运距主导，船队/网络密度减免；套利权重：正比实测价格离散 CV。
            // EMA 平滑避免逐周期震荡，单点 clamp（消除原 [0.001,0.5]/[0,0.05]/max(0,) 三处不一致）。
            int kingdomCount = res.Kingdoms.Count;

            // 全局平均首都距离 D_avg（单遍 O(K²)，仅后台线程，开销可忽略）
            float D_avg = 60f;
            var validIdx = _validKingdomScratch;
            validIdx.Clear();
            for (int i = 0; i < kingdomCount; i++)
            {
                var ks = res.Kingdoms[i];
                if (ks.KingdomId != 0 && !float.IsNaN(ks.CityX) && !float.IsNaN(ks.CityY)) validIdx.Add(i);
            }
            if (validIdx.Count > 1)
            {
                double pairwiseSum = 0d;
                int pairwiseCnt = 0;
                for (int i = 0; i < validIdx.Count; i++)
                {
                    var a = res.Kingdoms[validIdx[i]];
                    for (int j = i + 1; j < validIdx.Count; j++)
                    {
                        var b = res.Kingdoms[validIdx[j]];
                        double dx = a.CityX - b.CityX;
                        double dy = a.CityY - b.CityY;
                        pairwiseSum += Math.Sqrt(dx * dx + dy * dy);
                        pairwiseCnt++;
                    }
                }
                if (pairwiseCnt > 0) D_avg = (float)(pairwiseSum / pairwiseCnt);
            }

            // 海路占比 / 平均边成本（单遍 edges，O(E)）
            float seaRatio = 0f;
            float avgEdge = D_avg;
            if (edges.Count > 0)
            {
                int seaCnt = 0;
                double costSum = 0d;
                for (int i = 0; i < edges.Count; i++)
                {
                    if (edges[i].RouteType == 1) seaCnt++;
                    costSum += edges[i].Cost;
                }
                seaRatio = (float)seaCnt / edges.Count;
                avgEdge = (float)(costSum / edges.Count);
            }

            // 平均船队规模（单遍 kingdoms，O(K)）
            float fleet = 0f;
            if (kingdoms.Count > 0)
            {
                int boatSum = 0;
                for (int i = 0; i < kingdoms.Count; i++) boatSum += kingdoms[i].Boats;
                fleet = (float)boatSum / kingdoms.Count;
            }
            int edgeCnt = edges.Count;

            float decayRaw = 0.03f / (1f + D_avg / 80f) * (1f - 0.25f * seaRatio) / (1f + 0.15f * fleet / 10f);
            float previousDecay, previousTransport, previousPriceW;
            lock (_lifecycleLock)
            {
                previousDecay = _smDecay;
                previousTransport = _smTransport;
                previousPriceW = _smPriceW;
            }
            float smDecay = Mathf.Lerp(previousDecay, decayRaw, 0.35f);
            res.NextSmDecay = smDecay;
            res.DistanceDecay = Mathf.Clamp(smDecay, 0.001f, 0.05f);

            float transRaw = 0.08f * (avgEdge / 80f) / (1f + 0.08f * fleet) / (1f + edgeCnt / 1200f);
            float smTransport = Mathf.Lerp(previousTransport, transRaw, 0.35f);
            res.NextSmTransport = smTransport;
            res.TransportCost = Mathf.Clamp(smTransport, 0.005f, 0.25f);

            float pdwRaw = Mathf.Clamp01(res.PriceDispersion * 2f) * 0.5f;
            float smPriceW = Mathf.Lerp(previousPriceW, pdwRaw, 0.45f);
            res.NextSmPriceW = smPriceW;
            res.PriceDiffWeight = Mathf.Clamp(smPriceW, 0.01f, 0.5f);

            // --- 地理贸易网络（城市供需缺口 → 按边流动，全图净≈0）---
            ComputeTrade(res, cities, edges, kingdoms);
            return res;
        }

        /// <summary>
        /// 按贸易边计算流量（后台线程，纯数据）：
        /// 城市 gap = gold − 仓库容量；仓库容量优先用原版真实容量（StockpileCap =
        /// ResourceLibrary.gold.storage_max，游戏原版自带仓库系统），=0 时回退到
        /// Buildings × TradeCityBaseCapacity × 50% 估算；
        /// 互补缺口（一盈余一缺口）→ flow = min(|gapA|,|gapB|) × 距离衰减 × 运输摩擦 × 套利加成 × TradeFlowRatio；
        /// 海路流量受出口王国 Boats × SeaCapacityPerBoat 上限约束（超限按比例缩放）。
        /// 所有临时结构均为本地变量（后台线程局部，无跨线程共享静态缓冲，杜绝竞态），
        /// 聚合出王国净余额 + 按王国/城市出口额（供份额趋势）。
        /// </summary>
        private static void ComputeTrade(CycleResult res, List<CitySnapshot> cities,
            List<TradeEdge> edges, List<KingdomFacts> kingdoms)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.TradeEnabled) return;
            if (cities.Count < 2 || edges.Count == 0) return;

            float flowRatio = Mathf.Clamp(cfg.TradeFlowRatio, 0f, 0.2f);
            if (flowRatio <= 0f) return;
            float decay = res.DistanceDecay;
            float transportCost = res.TransportCost;
            float priceDiffWeight = res.PriceDiffWeight;
            float baseCap = Mathf.Max(1f, cfg.TradeCityBaseCapacity);
            int maxEdges = Mathf.Max(1, cfg.MaxEdges);

            // 城市索引（cityId → 下标）
            var cityIndex = _cityIndexScratch;
            cityIndex.Clear();
            for (int i = 0; i < cities.Count; i++) cityIndex[cities[i].CityId] = i;

            // 王国索引（kingdomId → 下标）+ 本地价格（套利用）+ 船队（海路上限用）
            int kCount = res.Kingdoms.Count;
            var kingdomIndex = _kingdomIndexScratch;
            kingdomIndex.Clear();
            for (int i = 0; i < kCount; i++) kingdomIndex[res.Kingdoms[i].KingdomId] = i;
            if (_priceScratch.Length < kCount) _priceScratch = new float[kCount];
            var priceByK = _priceScratch;
            for (int i = 0; i < kCount; i++) priceByK[i] = res.Kingdoms[i].LocalPrice;
            var boats = _boatsScratch;
            boats.Clear();
            for (int i = 0; i < kingdoms.Count; i++) boats[kingdoms[i].Id] = kingdoms[i].Boats;

            // MaxEdges 截断：缓存超限时按 cost 升序保留最便宜的前 MaxEdges 条
            var usable = edges;
            int usableCount = edges.Count;
            if (edges.Count > maxEdges)
            {
                _usableEdgesScratch.Clear();
                _usableEdgesScratch.AddRange(edges);
                _usableEdgesScratch.Sort((x, y) => x.Cost.CompareTo(y.Cost));
                usable = _usableEdgesScratch;
                usableCount = maxEdges;
            }

            var flows = res.TradeFlows;
            flows.Clear();
            if (flows.Capacity < 256) flows.Capacity = 256;

            for (int i = 0; i < usableCount; i++)
            {
                var e = usable[i];
                int ia, ib;
                if (!cityIndex.TryGetValue(e.CityAId, out ia)) continue;
                if (!cityIndex.TryGetValue(e.CityBId, out ib)) continue;
                var ca = cities[ia];
                var cb = cities[ib];

                // 仓库容量：真实容量仅在合理范围（≤10 万）内采用，异常大（无上限哨兵值）回退建筑估算
                float capA = (ca.StockpileCap > 0 && ca.StockpileCap <= 100000) ? ca.StockpileCap : ca.Buildings * baseCap * 0.5f;
                float capB = (cb.StockpileCap > 0 && cb.StockpileCap <= 100000) ? cb.StockpileCap : cb.Buildings * baseCap * 0.5f;
                // 中央银行家·市场建筑：本国市场所在城市贸易容量 +20%（只读基础类型，无分配）
                if (NationEngine.IsMarketCity(ca.CityId)) capA *= NationEngine.MarketCapBonus;
                if (NationEngine.IsMarketCity(cb.CityId)) capB *= NationEngine.MarketCapBonus;
                float gapA = ca.Gold - capA;
                float gapB = cb.Gold - capB;
                if (gapA * gapB >= 0f) continue; // 同号或零：无互补缺口

                // 距离衰减 × 运输摩擦 × 套利加成（区域价格差越大套利越强）
                float weight = 1f / (1f + e.Cost * decay);
                float transportFactor = Mathf.Max(0.5f, 1f - transportCost);
                float priceGap = 0f;
                int kiA, kiB;
                if (kingdomIndex.TryGetValue(ca.KingdomId, out kiA) && kingdomIndex.TryGetValue(cb.KingdomId, out kiB))
                    priceGap = Mathf.Abs(priceByK[kiA] - priceByK[kiB]);
                float arbitrage = 1f + priceDiffWeight * Mathf.Min(1f, priceGap);
                float flow = Mathf.Min(Mathf.Abs(gapA), Mathf.Abs(gapB)) * weight * transportFactor * arbitrage * flowRatio;
                // 中央银行家：贸易协定（本国参与的边流量+）/ 关税（本国为进口方流量-）
                float pactMult = NationEngine.PactFlowMult(ca.KingdomId);
                if (pactMult == 1f) pactMult = NationEngine.PactFlowMult(cb.KingdomId);
                if (pactMult != 1f) flow *= pactMult;
                float tariffMult = gapA < 0f ? NationEngine.TariffImportMult(ca.KingdomId)
                    : (gapB < 0f ? NationEngine.TariffImportMult(cb.KingdomId) : 1f);
                if (tariffMult != 1f) flow *= tariffMult;
                // 中央银行家：双边贸易协定（边两端恰为本国↔协约国时流量+）
                float bilateral = NationDiplomacy.BilateralFlowMult(ca.KingdomId, cb.KingdomId);
                if (bilateral != 1f) flow *= bilateral;
                // 法典：贸易流量乘数（自由贸易/计划/闭关等聚合，取两端较高一档）
                float codexFlowA = CodexEngine.GetMods(ca.KingdomId).TradeFlow;
                float codexFlowB = CodexEngine.GetMods(cb.KingdomId).TradeFlow;
                float codexFlow = codexFlowA > codexFlowB ? codexFlowA : codexFlowB;
                if (codexFlow != 1f) flow *= codexFlow;
                if (flow < 1f) continue; // 太小忽略，避免琐碎结算

                bool aExports = gapA > 0f;
                flows.Add(new TradeFlow
                {
                    FromCityId = aExports ? ca.CityId : cb.CityId,
                    ToCityId = aExports ? cb.CityId : ca.CityId,
                    FromKingdomId = aExports ? ca.KingdomId : cb.KingdomId,
                    ToKingdomId = aExports ? cb.KingdomId : ca.KingdomId,
                    Amount = (long)flow,
                    Sea = e.RouteType == 1
                });
            }
            if (flows.Count == 0) return;

            // 海路 Boats 上限：出口王国海路总量 ≤ Boats × SeaCapacityPerBoat，超限按比例缩放
            ApplySeaCapacity(flows, boats);

            // 净贸易额聚合：城市/国家各算 Export 与 Import，Net = Export − Import。
            // 国家值自动 = 该国所有城市贸易额之和（同国城市对贸易在出口、进口两侧同时计入，净额抵消）。
            if (_kingdomExportScratch.Length < kCount) _kingdomExportScratch = new long[kCount];
            if (_kingdomImportScratch.Length < kCount) _kingdomImportScratch = new long[kCount];
            if (_cityExportScratch.Length < cities.Count) _cityExportScratch = new long[cities.Count];
            if (_cityImportScratch.Length < cities.Count) _cityImportScratch = new long[cities.Count];
            var kExport = _kingdomExportScratch;
            var kImport = _kingdomImportScratch;
            var cExport = _cityExportScratch;
            var cImport = _cityImportScratch;
            Array.Clear(kExport, 0, kCount);
            Array.Clear(kImport, 0, kCount);
            Array.Clear(cExport, 0, cities.Count);
            Array.Clear(cImport, 0, cities.Count);
            long totalExport = 0;

            for (int i = 0; i < flows.Count; i++)
            {
                var f = flows[i];
                if (f.Amount <= 0) continue;
                totalExport += f.Amount;

                int fk, tk;
                if (kingdomIndex.TryGetValue(f.FromKingdomId, out fk)) kExport[fk] += f.Amount;
                if (kingdomIndex.TryGetValue(f.ToKingdomId, out tk)) kImport[tk] += f.Amount;

                int fc, tc;
                if (cityIndex.TryGetValue(f.FromCityId, out fc)) cExport[fc] += f.Amount;
                if (cityIndex.TryGetValue(f.ToCityId, out tc)) cImport[tc] += f.Amount;
            }
            for (int i = 0; i < kCount; i++)
                res.Kingdoms[i].TradeBalance = kExport[i] - kImport[i];
            res.TotalTradeVolume = totalExport;

            // 城市净额排名（仅列有贸易的城市，按 Net 降序）
            res.CityBalances.Clear();
            for (int i = 0; i < cities.Count; i++)
            {
                if (cExport[i] <= 0 && cImport[i] <= 0) continue;
                res.CityBalances.Add(new TradeBalance
                {
                    Id = cities[i].CityId,
                    Name = cities[i].Name,
                    Export = cExport[i],
                    Import = cImport[i],
                    Net = cExport[i] - cImport[i]
                });
            }
            res.CityBalances.Sort((a, b) => b.Net.CompareTo(a.Net));

            // 国家净额排名（降序）
            res.KingdomBalances.Clear();
            for (int i = 0; i < kCount; i++)
            {
                if (kExport[i] <= 0 && kImport[i] <= 0) continue;
                res.KingdomBalances.Add(new TradeBalance
                {
                    Id = res.Kingdoms[i].KingdomId,
                    Name = res.Kingdoms[i].Name,
                    Export = kExport[i],
                    Import = kImport[i],
                    Net = kExport[i] - kImport[i]
                });
            }
            res.KingdomBalances.Sort((a, b) => b.Net.CompareTo(a.Net));
        }

        /// <summary>海路出口上限约束（超限按比例缩放，保持各边相对结构）。</summary>
        private static void ApplySeaCapacity(List<TradeFlow> flows, Dictionary<long, int> boats)
        {
            var seaByK = _seaCapacityScratch;
            seaByK.Clear();
            for (int i = 0; i < flows.Count; i++)
            {
                var f = flows[i];
                if (!f.Sea || f.Amount <= 0) continue;
                decimal v;
                seaByK.TryGetValue(f.FromKingdomId, out v);
                seaByK[f.FromKingdomId] = v + f.Amount;
            }
            foreach (var kv in seaByK)
            {
                int b = 0;
                boats.TryGetValue(kv.Key, out b);
                long cap = b > 0 ? (long)b * SeaCapacityPerBoat : 0L;
                if (kv.Value <= cap) continue; // 未超限 → 不动
                decimal cumulative = 0m;
                long allocated = 0L;
                for (int i = 0; i < flows.Count; i++)
                {
                    var f = flows[i];
                    if (!f.Sea || f.FromKingdomId != kv.Key) continue;
                    if (f.Amount <= 0)
                    {
                        f.Amount = 0;
                        continue;
                    }
                    cumulative += f.Amount;
                    long target = (long)decimal.Floor((cumulative / kv.Value) * cap);
                    if (target > cap) target = cap;
                    f.Amount = target - allocated;
                    flows[i] = f;
                    allocated = target;
                }
            }
        }

        /// <summary>
        /// 基尼系数（O(N log N)：升序排序后单趟线性累加）。
        /// 公式：Gini = Σ_i (2i - n - 1) * w_i / (n * Σw)，w 升序，i 从 1 开始。
        /// 样本不足或财富和为 0 时返回 0。
        /// </summary>
        private static float ComputeGini(List<float> wealths, float wealthSum)
        {
            int n = wealths.Count;
            if (n < 2 || wealthSum <= 0f) return 0f;
            wealths.Sort(); // 升序
            double numerator = 0d;
            for (int i = 0; i < n; i++)
            {
                numerator += (2d * (i + 1) - n - 1d) * wealths[i];
            }
            double gini = numerator / (n * (double)wealthSum);
            if (gini < 0d) gini = 0d;
            else if (gini > 1d) gini = 1d;
            return (float)gini;
        }

        // ===== 贸易金流应用（主线程：按边在城市仓库间结算，全图净≈0）=====

        /// <summary>
        /// 将本周期贸易流应用到世界：出口城 addResourcesToRandomStockpile("gold", amt)，
        /// 进口城先付款并立即入出口城；不足部分按进口王国聚合后从居民金币 DeductCoins 兜底，
        /// 再按原流顺序分配实际扣款（与原版缴税同渠道）。
        /// </summary>
        public static void ApplyTradeFlows()
        {
            var cityRefs = _flowCityRefs;
            _flowCityRefs = null;
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.TradeEnabled) return;
            var res = LastResult;
            if (res == null || res.TradeFlows.Count == 0) return;

            var owedByKingdom = _residentOwedByKingdom;
            var paidByKingdom = _residentPaidByKingdom;
            var kingdomIds = _residentKingdomIds;
            var unpaidByFlow = _unpaidByFlow;
            var exportCityByFlow = _exportCityByFlow;
            owedByKingdom.Clear();
            paidByKingdom.Clear();
            kingdomIds.Clear();
            unpaidByFlow.Clear();
            exportCityByFlow.Clear();
            if (unpaidByFlow.Capacity < res.TradeFlows.Count) unpaidByFlow.Capacity = res.TradeFlows.Count;
            if (exportCityByFlow.Capacity < res.TradeFlows.Count) exportCityByFlow.Capacity = res.TradeFlows.Count;

            long debited = 0;
            long credited = 0;
            int validFlows = 0;
            for (int i = 0; i < res.TradeFlows.Count; i++)
            {
                var f = res.TradeFlows[i];
                unpaidByFlow.Add(0L);
                exportCityByFlow.Add(null);
                if (f.Amount <= 0) continue;
                // O(1) 反查城市（cityId 全局唯一），替代原 FindCity 逐王国 getCities() 遍历（O(C)）
                City fromCity, toCity;
                if (cityRefs == null
                    || !cityRefs.TryGetValue(f.FromCityId, out fromCity)
                    || !cityRefs.TryGetValue(f.ToCityId, out toCity))
                    continue; // 城市本周期内已消亡 → 该边作废
                if (fromCity == null || toCity == null) continue;
                validFlows++;
                exportCityByFlow[i] = fromCity;

                // 城市付款逐流即时入出口城，使其能被原顺序中的后续进口流使用。
                long remaining = f.Amount;
                long cityPaid = 0;
                try
                {
                    int have = toCity.getResourcesAmount("gold");
                    if (have > 0)
                    {
                        int take = (int)Math.Min(remaining, (long)have);
                        toCity.takeResource("gold", take);
                        remaining -= take;
                        cityPaid = take;
                    }
                }
                catch (System.Exception) { }

                if (cityPaid > 0)
                {
                    debited = SaturatingAdd(debited, cityPaid);
                    credited = SaturatingAdd(credited, AddTradeGold(fromCity, cityPaid));
                }

                if (remaining > 0)
                {
                    unpaidByFlow[i] = remaining;
                    long owed;
                    if (!owedByKingdom.TryGetValue(f.ToKingdomId, out owed))
                    {
                        kingdomIds.Add(f.ToKingdomId);
                        owed = 0L;
                    }
                    owedByKingdom[f.ToKingdomId] = SaturatingAdd(owed, remaining);
                }
            }

            // 每个进口王国只复制一次居民列表并扣款一次。
            for (int i = 0; i < kingdomIds.Count; i++)
            {
                long kingdomId = kingdomIds[i];
                var units = _unitPool;
                units.Clear();
                var kingdom = GameHelpers.FindKingdom(kingdomId);
                if (kingdom != null && kingdom.units != null) units.AddRange(kingdom.units);
                long deducted = GameHelpers.DeductCoins(units, owedByKingdom[kingdomId]);
                paidByKingdom[kingdomId] = deducted;
                debited = SaturatingAdd(debited, deducted);
            }

            // 按原 TradeFlows 顺序分配各王国实际扣得的居民款。
            for (int i = 0; i < res.TradeFlows.Count; i++)
            {
                long unpaid = unpaidByFlow[i];
                City exportCity = exportCityByFlow[i];
                if (unpaid <= 0 || exportCity == null) continue;
                long available;
                long kingdomId = res.TradeFlows[i].ToKingdomId;
                if (!paidByKingdom.TryGetValue(kingdomId, out available) || available <= 0) continue;
                long allocated = Math.Min(unpaid, available);
                paidByKingdom[kingdomId] = available - allocated;
                credited = SaturatingAdd(credited, AddTradeGold(exportCity, allocated));
            }

            if (debited > 0)
            {
                long net = credited - debited;
                GameHelpers.Log($"[ClassicalEconomics] 地理贸易：{validFlows} 条有效流实际支付 {debited} 金币，结算净 {net:+0;-0}");
            }
        }

        private static long AddTradeGold(City city, long amount)
        {
            long added = 0L;
            while (amount > 0)
            {
                int chunk = (int)Math.Min(amount, (long)int.MaxValue);
                try { city.addResourcesToRandomStockpile("gold", chunk); }
                catch (System.Exception) { break; }
                added += chunk;
                amount -= chunk;
            }
            return added;
        }

        private static long SaturatingAdd(long value, long increment)
            => increment > long.MaxValue - value ? long.MaxValue : value + increment;

        // ===== 静态缓冲缩容（MemoryCleanupEngine 空闲期调用；只缩容，不清内容，语义不变）=====

        private static int TrimList<T>(List<T> list)
        {
            try
            {
                if (list.Capacity > 4096 && list.Capacity > list.Count * 4)
                {
                    list.TrimExcess();
                    return 1;
                }
            }
            catch (System.Exception) { }
            return 0;
        }

        /// <summary>对全部静态 List 缓冲/缓存执行 TrimExcess（含池化内层列表），返回实际收缩的列表数；
        /// 静态字典由 MemoryCleanupEngine 通过 ForTrim 访问器重建缩容。</summary>
        public static int TrimMemory()
        {
            int shrunk = 0;
            shrunk += TrimList(_collectActors);
            shrunk += TrimList(_collectKingdoms);
            shrunk += TrimList(_collectCities);
            shrunk += TrimList(_collectEdges);
            shrunk += TrimList(_unitPool);
            shrunk += TrimList(_residentKingdomIds);
            shrunk += TrimList(_unpaidByFlow);
            shrunk += TrimList(_exportCityByFlow);
            shrunk += TrimList(_computeActors);
            shrunk += TrimList(_computeKingdoms);
            shrunk += TrimList(_computeCities);
            shrunk += TrimList(_computeEdges);
            shrunk += TrimList(_staleEdgeKeys);
            shrunk += TrimList(_kingdomIdPool);
            shrunk += TrimList(_globalWealthScratch);
            shrunk += TrimList(_validKingdomScratch);
            shrunk += TrimList(_usableEdgesScratch);

            // 内层池化列表：池中空闲的王国城市列表与其承载的城市列表一并缩容
            try
            {
                for (int i = 0; i < _kingdomCityListPool.Count; i++)
                {
                    var inner = _kingdomCityListPool[i];
                    if (inner != null) shrunk += TrimList(inner);
                }
                shrunk += TrimList(_kingdomCityListPool);
            }
            catch (System.Exception) { }

            // 内层池化 Accum：池中空闲对象的 Wealths 列表一并缩容
            try
            {
                for (int i = 0; i < _accPool.Count; i++)
                {
                    var acc = _accPool[i];
                    if (acc != null) shrunk += TrimList(acc.Wealths);
                }
                shrunk += TrimList(_accPool);
            }
            catch (System.Exception) { }

            // 非 readonly 字典 _flowCityRefs 及其余静态字典（_residentOwedByKingdom /
            // _residentPaidByKingdom / _edgeCache / _knownCityTopology / _accScratch /
            // _cityIndexScratch / _kingdomIndexScratch / _boatsScratch / _seaCapacityScratch）
            // 由 MemoryCleanupEngine 通过下方 ForTrim 访问器重建缩容
            // （本文件受 Test-AllocHygiene 锚定，禁止运行时集合分配）。

            return shrunk;
        }

        /// <summary>供 MemoryCleanupEngine 重建缩容时读取当前引用（仅空闲期调用）。</summary>
        internal static Dictionary<long, City> FlowCityRefsForTrim => _flowCityRefs;

        /// <summary>将重建后的紧凑字典换回（仅 MemoryCleanupEngine 空闲期调用）。</summary>
        internal static void ReplaceFlowCityRefsForTrim(Dictionary<long, City> compact)
        {
            _flowCityRefs = compact;
        }

        // ===== 其余静态字典的缩容访问器（仅 MemoryCleanupEngine 空闲期调用；
        // ===== 重建分配发生在 MemoryCleanupEngine.cs，保持本文件零运行时分配不变量）=====

        internal static Dictionary<long, long> ResidentOwedForTrim => _residentOwedByKingdom;
        internal static void ReplaceResidentOwedForTrim(Dictionary<long, long> compact) { _residentOwedByKingdom = compact; }

        internal static Dictionary<long, long> ResidentPaidForTrim => _residentPaidByKingdom;
        internal static void ReplaceResidentPaidForTrim(Dictionary<long, long> compact) { _residentPaidByKingdom = compact; }

        internal static Dictionary<(long, long), TradeEdge> EdgeCacheForTrim => _edgeCache;
        internal static void ReplaceEdgeCacheForTrim(Dictionary<(long, long), TradeEdge> compact) { _edgeCache = compact; }

        internal static Dictionary<long, (long, int, int)> KnownCityTopologyForTrim => _knownCityTopology;
        internal static void ReplaceKnownCityTopologyForTrim(Dictionary<long, (long, int, int)> compact) { _knownCityTopology = compact; }

        internal static Dictionary<long, Accum> AccScratchForTrim => _accScratch;
        internal static void ReplaceAccScratchForTrim(Dictionary<long, Accum> compact) { _accScratch = compact; }

        internal static Dictionary<long, int> CityIndexScratchForTrim => _cityIndexScratch;
        internal static void ReplaceCityIndexScratchForTrim(Dictionary<long, int> compact) { _cityIndexScratch = compact; }

        internal static Dictionary<long, int> KingdomIndexScratchForTrim => _kingdomIndexScratch;
        internal static void ReplaceKingdomIndexScratchForTrim(Dictionary<long, int> compact) { _kingdomIndexScratch = compact; }

        internal static Dictionary<long, int> BoatsScratchForTrim => _boatsScratch;
        internal static void ReplaceBoatsScratchForTrim(Dictionary<long, int> compact) { _boatsScratch = compact; }

        internal static Dictionary<long, decimal> SeaCapacityScratchForTrim => _seaCapacityScratch;
        internal static void ReplaceSeaCapacityScratchForTrim(Dictionary<long, decimal> compact) { _seaCapacityScratch = compact; }
    }
}
