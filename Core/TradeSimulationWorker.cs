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
    /// 贸易网络（地理驱动，Phase 6）：城市为节点 / 王国为聚合层。
    /// - 边：邻国王国对 → 全部城市对直接建边（陆路、成本=欧氏距离，成本最低）；
    ///       非邻国王国对 → 取"最近城市对"（欧氏距离最小），距离 ≤ MaxTradeRange 才
    ///       入寻路队列，PathfinderTools.raycast 确认可达（空路径 ⇒ 无贸易边）；
    ///       寻路路径海洋占比 &gt; 50% ⇒ 海路（受出口王国 Boats × 每船载量上限约束）。
    /// - 邻国判定：原版城市邻国数据（City.neighbours_kingdoms 等）为 internal 不可访问，
    ///       以王国几何距离为代理——Kingdom.distanceBetweenKingdom ≤ MaxTradeRange ⇒ 邻国。
    /// - 成本：cost = (陆路距离 + 海路距离 × SeaRoutePenalty) × (邻国 ? 1 : NonNeighborPenalty)
    ///       贸易量 ∝ 1 / (1 + cost × DistanceDecay) × 供需缺口 × TradeFlowRatio。
    /// - 供需缺口：城市 gap = gold − 仓库容量；容量优先用原版真实仓库容量
    ///       （ResourceLibrary.gold.storage_max，游戏原版自带仓库系统，用户确认方案），
    ///       不可用（=0）时回退到 建筑数 × TradeCityBaseCapacity × 50% 估算；
    ///       gap &gt; 0 盈余出口 / gap &lt; 0 缺口进口，按边互补结算。
    /// - 结算：出口城 addResourcesToRandomStockpile("gold", amt)，进口城 takeResource 兜底
    ///       DeductCoins（与原版缴税同渠道），全图净≈0。
    /// 寻路缓存三类失效：Reset() 世界重置清空 / 每 PathRecomputeEvery 周期全量重算 /
    /// 王国生灭增量增删。
    ///
    /// v0.9.1 兼容：生产函数（Workers × Productivity × CapitalFactor）、区域价格指数
    /// （LocalPrice = 全局 CPI × 供需系数，clamp 0.5~2）、价格离散度（PriceDispersion=CV）、
    /// 平均距离衰减因子（AvgDistanceFactor）等统计字段仍在此计算并发布，供 HUD 展示。
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
        public class TradeFlow
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
            public float PriceDispersion; // 区域价格离散度（本地价格变异系数 CV，0=各地同价）

            // ===== 地理贸易实际生效值（v0.9.1：暴露给 HUD 展示）=====
            public float AvgDistanceFactor; // 参与贸易王国的平均距离衰减因子（1=无衰减，越低远程贸易越弱）
            public float DistanceDecay;     // 实际生效的距离衰减系数（clamp 后）
            public float TransportCost;     // 实际生效的运输成本比例（clamp 后）
            public float PriceDiffWeight;   // 实际生效的价格差（套利）权重（clamp 后）

            public readonly List<KingdomSim> Kingdoms = new List<KingdomSim>(16);
            public readonly List<TradeFlow> TradeFlows = new List<TradeFlow>(128); // 本周期按边结算明细
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

        /// <summary>最近一次已消费的结果（供各引擎读取；主线程使用，不跨周期持有）。</summary>
        public static CycleResult LastResult { get; private set; }

        // ===== 寻路缓存（主线程维护，纯数据，跨周期保留）=====

        /// <summary>待寻路城市对（主线程队列，每周期限流消费）。</summary>
        private struct CityPair
        {
            public long A, B;   // 城市 id
            public long KA, KB; // 归属王国 id
        }

        private static readonly Dictionary<(long, long), TradeEdge> _edgeCache =
            new Dictionary<(long, long), TradeEdge>(512);   // key = KeyOf(cityAId, cityBId)
        private static readonly Queue<CityPair> _pathfindQueue = new Queue<CityPair>(256);
        private static readonly HashSet<long> _knownKingdoms = new HashSet<long>(32);
        private static int _routeCycle;                     // PrepareRoutes 调用计数（全量重算节奏）

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
        public static void AddCitySnapshot(long cityId, long kingdomId, int gold, int buildings,
            int boats, int stockpileCap, int tileX, int tileY)
        {
            _collectCities.Add(new CitySnapshot
            {
                CityId = cityId, KingdomId = kingdomId, Gold = gold, Buildings = buildings,
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
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.TradeEnabled) return; // 未启用贸易：不跑寻路，后台无边
            if (_collectCities.Count < 2) return;

            _routeCycle++;
            bool fullRebuild = (_routeCycle % Mathf.Max(1, cfg.PathRecomputeEvery)) == 0;

            // 王国生灭检测：消失王国 → 删除相关边；新增王国 → 触发重建候选
            var curKingdoms = new HashSet<long>(_collectCities.Count / 2);
            for (int i = 0; i < _collectCities.Count; i++) curKingdoms.Add(_collectCities[i].KingdomId);

            bool kingdomsChanged = false;
            if (curKingdoms.Count != _knownKingdoms.Count) kingdomsChanged = true;
            else
            {
                foreach (var k in curKingdoms)
                    if (!_knownKingdoms.Contains(k)) { kingdomsChanged = true; break; }
            }

            if (kingdomsChanged)
            {
                RemoveDeadKingdomEdges(curKingdoms);
                _knownKingdoms.Clear();
                foreach (var k in curKingdoms) _knownKingdoms.Add(k);
            }

            if (fullRebuild || kingdomsChanged)
            {
                _pathfindQueue.Clear();
                EnqueueCandidatePairs(cfg);
            }

            // 限流寻路（PathfinderTools 仅主线程可调；结果 upsert 缓存）
            int budget = Mathf.Max(1, cfg.MaxPathfindPairs);
            while (budget > 0 && _pathfindQueue.Count > 0)
            {
                var pair = _pathfindQueue.Dequeue();
                budget--;
                var edge = TryPathfindEdge(cityRefs, pair, cfg);
                if (edge != null)
                    _edgeCache[KeyOf(pair.A, pair.B)] = edge.Value;
            }

            // 缓存快照 → 本周期边缓冲（纯值拷贝，后台线程只读）
            _collectEdges.Clear();
            if (_edgeCache.Count > _collectEdges.Capacity)
                _collectEdges.Capacity = _edgeCache.Count;
            foreach (var kv in _edgeCache) _collectEdges.Add(kv.Value);
        }

        /// <summary>删除已消失王国涉及的全部边（缓存维护，主线程 O(N) 过滤）。</summary>
        private static void RemoveDeadKingdomEdges(HashSet<long> curKingdoms)
        {
            if (_knownKingdoms.Count == 0) return;
            var keysToRemove = new List<(long, long)>(8);
            foreach (var kv in _edgeCache)
            {
                var e = kv.Value;
                if (!curKingdoms.Contains(e.KingdomAId) || !curKingdoms.Contains(e.KingdomBId))
                    keysToRemove.Add(kv.Key);
            }
            for (int i = 0; i < keysToRemove.Count; i++) _edgeCache.Remove(keysToRemove[i]);
        }

        /// <summary>
        /// 重建候选队列（主线程，纯数据）：
        /// - 邻国王国对 → 全部城市对直接建边（陆路、成本=欧氏距离；邻国成本最低，无需寻路）；
        /// - 非邻国王国对 → 取最近城市对（欧氏距离最小），≤ MaxTradeRange 才入寻路队列。
        /// </summary>
        private static void EnqueueCandidatePairs(UnrestConfig cfg)
        {
            // 王国 → 城市索引
            var kingdomCities = new Dictionary<long, List<CitySnapshot>>(8);
            for (int i = 0; i < _collectCities.Count; i++)
            {
                var cs = _collectCities[i];
                if (!kingdomCities.TryGetValue(cs.KingdomId, out var list))
                {
                    list = new List<CitySnapshot>(8);
                    kingdomCities[cs.KingdomId] = list;
                }
                list.Add(cs);
            }

            // 王国级邻接：原版城市邻国数据（City.neighbours_kingdoms 等）为 internal 不可访问，
            // 以王国几何距离为公开 API 代理——distanceBetweenKingdom ≤ MaxTradeRange ⇒ 邻国

            float maxRange = Mathf.Max(0f, cfg.MaxTradeRange);
            var ids = new List<long>(kingdomCities.Keys);
            for (int i = 0; i < ids.Count; i++)
            {
                for (int j = i + 1; j < ids.Count; j++)
                {
                    long k1 = ids[i], k2 = ids[j];
                    var c1 = kingdomCities[k1];
                    var c2 = kingdomCities[k2];
                    bool neighbor = AreNeighborKingdoms(k1, k2);

                    if (neighbor)
                    {
                        // 邻国：全部城市对直接建边（成本最低：欧氏距离，无惩罚，陆路）
                        for (int a = 0; a < c1.Count; a++)
                        {
                            for (int b = 0; b < c2.Count; b++)
                            {
                                var ca = c1[a]; var cb = c2[b];
                                _edgeCache[KeyOf(ca.CityId, cb.CityId)] = new TradeEdge
                                {
                                    CityAId = ca.CityId, CityBId = cb.CityId,
                                    KingdomAId = ca.KingdomId, KingdomBId = cb.KingdomId,
                                    Cost = Dist(ca, cb), RouteType = 0
                                };
                            }
                        }
                    }
                    else
                    {
                        // 非邻国：最近城市对 ≤ MaxTradeRange → 入队寻路（跨海/远距必须寻路确认）
                        float best = float.MaxValue;
                        bool found = false;
                        long ba = 0, bb = 0, bka = 0, bkb = 0;
                        for (int a = 0; a < c1.Count; a++)
                        {
                            for (int b = 0; b < c2.Count; b++)
                            {
                                float d = Dist(c1[a], c2[b]);
                                if (d < best) { best = d; ba = c1[a].CityId; bb = c2[b].CityId; bka = c1[a].KingdomId; bkb = c2[b].KingdomId; found = true; }
                            }
                        }
                        if (found && best <= maxRange)
                            _pathfindQueue.Enqueue(new CityPair { A = ba, B = bb, KA = bka, KB = bkb });
                    }
                }
            }
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
                var path = PathfinderTools.raycast(ta, tb, 1f);
                if (path == null || path.Count == 0) return null; // 不可达 ⇒ 无贸易边

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

                float euclid = Mathf.Sqrt(
                    (ta.x - tb.x) * (ta.x - tb.x) + (ta.y - tb.y) * (ta.y - tb.y));
                float baseCost = isSea ? euclid * Mathf.Max(1f, cfg.SeaRoutePenalty) : euclid;
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

        /// <summary>
        /// 提交一轮周期：缓冲交换后交由后台线程计算，主线程轮询 <see cref="TryConsume"/> 消费。
        /// 若已有周期在途则拒绝（返回 false）。
        /// </summary>
        public static bool PostCycle()
        {
            if (_posting || _computing) return false; // 防御：仅允许一轮在途
            SwapBuffers();
            _cycleIndex++;
            _generation++;
            int idx = _cycleIndex;
            int gen = _generation;
            var actors = _computeActors;
            var kingdoms = _computeKingdoms;
            var cities = _computeCities;
            var edges = _computeEdges;
            _computing = true;
            _posting = true;
            try
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var r = Compute(actors, kingdoms, cities, edges, idx);
                        if (gen == _generation) _readyResult = r; // 过期任务不写结果
                    }
                    catch (Exception e)
                    {
                        // 后台线程禁止调用 Unity API（Debug.Log 非线程安全，可能引发原生崩溃），
                        // 只记录异常文本，由主线程 TryConsume 消费时输出日志并兜底重算。
                        if (gen == _generation)
                        {
                            _workerError = e.Message;
                            _readyResult = null;
                        }
                    }
                    finally
                    {
                        if (gen == _generation) _computing = false; // volatile 写：发布对 _readyResult 的写入
                    }
                });
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 后台线程提交失败: " + e.Message);
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
            if (!_posting) return false;
            if (_computing) return false;
            // 后台线程异常：主线程在此输出日志（后台线程不允许调用 Unity API）
            var err = _workerError;
            _workerError = null;
            if (err != null) Debug.LogWarning("[ClassicalEconomics] 后台统计失败，主线程兜底重算: " + err);
            var res = _readyResult;
            if (res == null)
            {
                res = Compute(_computeActors, _computeKingdoms, _computeCities, _computeEdges, _cycleIndex); // 兜底：同步重算
            }
            _readyResult = null;
            _posting = false;
            Publish(res);
            return true;
        }

        /// <summary>是否已有周期在途（已提交未消费 / 后台计算中）。调用方在发起新周期或同步计算前应检查。</summary>
        public static bool IsBusy() => _posting || _computing;

        /// <summary>后台结果是否已就绪但尚未被消费（供周期驱动器自愈：_posting 遗留但无人消费时兜底置位）。</summary>
        public static bool HasPendingResult() => _posting && !_computing;

        /// <summary>
        /// 手动采集/实时刷新：同步计算并立即发布（按钮触发，不等后台线程）。
        /// advanceCycle=false 用于实时刷新（不推进周期号，避免 HUD"周期 #N"暴涨）。
        /// 注意：调用前须先完成采集（DataCollector.Collect 内部已跑 PrepareRoutes + PostCycle），
        /// 此处复用已交换的缓冲。
        /// </summary>
        public static void ComputeAndConsumeSync(bool advanceCycle = true)
        {
            if (!_posting)
            {
                SwapBuffers();
                if (advanceCycle) _cycleIndex++;
            }
            // 丢弃在途后台任务（若有），以同步结果为准
            _generation++; // 使在途后台任务过期
            _computing = false;
            _posting = false;
            _readyResult = null;
            _workerError = null;
            Publish(Compute(_computeActors, _computeKingdoms, _computeCities, _computeEdges, _cycleIndex));
        }

        /// <summary>世界重置（新地图/新游戏）时清空在途周期、结果与寻路缓存。</summary>
        public static void Reset()
        {
            _generation++; // 使在途后台任务过期
            _posting = false;
            _computing = false;
            _readyResult = null;
            _workerError = null;
            LastResult = null;
            _cycleIndex = 0;
            _routeCycle = 0;
            _collectActors.Clear();
            _collectKingdoms.Clear();
            _collectCities.Clear();
            _collectEdges.Clear();
            _edgeCache.Clear();
            _pathfindQueue.Clear();
            _knownKingdoms.Clear();
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
            LastResult = res;
            EconomyEngine.PublishResult(res);
        }

        // ===== 后台线程计算（纯数据，零 Unity 对象访问）=====

        private class Accum
        {
            public double Gdp;
            public int Count;
            public int Workers;
            public double ProdSum;
            public readonly List<float> Wealths = new List<float>(256);
        }

        private static CycleResult Compute(List<ActorRecord> actors, List<KingdomFacts> kingdoms,
            List<CitySnapshot> cities, List<TradeEdge> edges, int cycleIndex)
        {
            var res = new CycleResult { CycleIndex = cycleIndex };

            // --- 全局 + 王国聚合（单遍遍历）---
            double gdp = 0d;
            int count = 0;
            var acc = new Dictionary<long, Accum>(32);
            var globalWealths = new List<float>(actors.Count); // 全局基尼样本（同遍收集，避免二次遍历）
            foreach (var r in actors)
            {
                double w = r.Wealth;
                gdp += w;
                count++;
                globalWealths.Add(r.Wealth);
                if (!acc.TryGetValue(r.KingdomId, out var a))
                {
                    a = new Accum();
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
            var cfg = UnrestConfig.Instance; // 后台只读纯数据配置（非 Unity 对象，与 BiomeEconomy.GetBonus 同模式）
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

            // --- 距离矩阵（v0.9 地理贸易）：王国首都间欧氏距离 → 平均距离 → 衰减因子 ---
            // 坐标无效（NaN）的王国不参与距离计算，因子回退 1（不惩罚）。O(m²) 仅后台线程，开销可忽略。
            int kingdomCount = res.Kingdoms.Count;
            var distanceFactor = new float[kingdomCount];
            for (int i = 0; i < kingdomCount; i++) distanceFactor[i] = 1f;
            var validIdx = new List<int>(kingdomCount);
            for (int i = 0; i < kingdomCount; i++)
            {
                var ks = res.Kingdoms[i];
                if (ks.KingdomId != 0 && !float.IsNaN(ks.CityX) && !float.IsNaN(ks.CityY)) validIdx.Add(i);
            }
            float distanceDecay = cfg != null ? Mathf.Clamp(cfg.DistanceDecay, 0f, 0.05f) : 0f;
            if (validIdx.Count > 1 && distanceDecay > 0f)
            {
                var xs = new float[validIdx.Count];
                var ys = new float[validIdx.Count];
                for (int i = 0; i < validIdx.Count; i++)
                {
                    var ks = res.Kingdoms[validIdx[i]];
                    xs[i] = ks.CityX; ys[i] = ks.CityY;
                }
                for (int i = 0; i < validIdx.Count; i++)
                {
                    double sum = 0d;
                    int cnt = 0;
                    for (int j = 0; j < validIdx.Count; j++)
                    {
                        if (j == i) continue;
                        double dx = xs[i] - xs[j];
                        double dy = ys[i] - ys[j];
                        sum += Math.Sqrt(dx * dx + dy * dy);
                        cnt++;
                    }
                    float avg = cnt > 0 ? (float)(sum / cnt) : 0f;
                    distanceFactor[validIdx[i]] = 1f / (1f + avg * distanceDecay);
                }
            }

            // --- 地理贸易特征汇总（v0.9.1：供 HUD 概览页展示实际生效值）---
            // 平均距离衰减因子 = 所有参与贸易王国距离因子的均值（无王国/无坐标时回退 1=无衰减）
            float factorSum = 0f;
            int factorCount = 0;
            for (int i = 0; i < kingdomCount; i++)
            {
                if (res.Kingdoms[i].KingdomId == 0) continue;
                factorSum += distanceFactor[i];
                factorCount++;
            }
            res.AvgDistanceFactor = factorCount > 0 ? factorSum / factorCount : 1f;
            res.DistanceDecay = distanceDecay;
            float transportCost = cfg != null ? Mathf.Clamp(cfg.TransportCost, 0f, 0.3f) : 0f;
            float priceDiffWeight = cfg != null ? Mathf.Clamp(cfg.PriceDiffWeight, 0f, 1f) : 0f;
            res.TransportCost = transportCost;
            res.PriceDiffWeight = priceDiffWeight;

            // --- 地理贸易网络（城市供需缺口 → 按边流动，全图净≈0）---
            ComputeTrade(res, cities, edges, kingdoms);
            return res;
        }

        /// <summary>
        /// 按贸易边计算流量（后台线程，纯数据）：
        /// 城市 gap = gold − 仓库容量；仓库容量优先用原版真实容量（StockpileCap =
        /// ResourceLibrary.gold.storage_max，游戏原版自带仓库系统），=0 时回退到
        /// Buildings × TradeCityBaseCapacity × 50% 估算；
        /// 互补缺口（一盈余一缺口）→ flow = min(|gapA|,|gapB|) × 1/(1+cost×DistanceDecay) × TradeFlowRatio；
        /// 海路流量受出口王国 Boats × SeaCapacityPerBoat 上限约束（超限按比例缩放）。
        /// </summary>
        private static void ComputeTrade(CycleResult res, List<CitySnapshot> cities,
            List<TradeEdge> edges, List<KingdomFacts> kingdoms)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.TradeEnabled) return;
            if (cities.Count < 2 || edges.Count == 0) return;

            float flowRatio = Mathf.Clamp(cfg.TradeFlowRatio, 0f, 0.2f);
            if (flowRatio <= 0f) return;
            float decay = Mathf.Max(0f, cfg.DistanceDecay);
            float baseCap = Mathf.Max(1f, cfg.TradeCityBaseCapacity);
            int maxEdges = Mathf.Max(1, cfg.MaxEdges);

            // 城市索引（cityId → 快照）
            var cityMap = new Dictionary<long, CitySnapshot>(cities.Count);
            for (int i = 0; i < cities.Count; i++) cityMap[cities[i].CityId] = cities[i];

            // 王国 Boats（海路上限用）
            var boats = new Dictionary<long, int>(kingdoms.Count);
            for (int i = 0; i < kingdoms.Count; i++) boats[kingdoms[i].Id] = kingdoms[i].Boats;

            // MaxEdges 截断：缓存超限时按 cost 升序保留最便宜的前 MaxEdges 条
            var usable = edges;
            if (edges.Count > maxEdges)
            {
                var sorted = new List<TradeEdge>(edges);
                sorted.Sort((x, y) => x.Cost.CompareTo(y.Cost));
                usable = sorted;
                if (sorted.Count > maxEdges) usable = sorted.GetRange(0, maxEdges);
            }

            var flows = res.TradeFlows;
            flows.Clear();
            if (flows.Capacity < 256) flows.Capacity = 256;

            for (int i = 0; i < usable.Count; i++)
            {
                var e = usable[i];
                if (!cityMap.TryGetValue(e.CityAId, out var ca)) continue;
                if (!cityMap.TryGetValue(e.CityBId, out var cb)) continue;
                if (ca.KingdomId == cb.KingdomId) continue; // 同王国不成边

                float capA = ca.StockpileCap > 0 ? ca.StockpileCap : ca.Buildings * baseCap * 0.5f;
                float capB = cb.StockpileCap > 0 ? cb.StockpileCap : cb.Buildings * baseCap * 0.5f;
                float gapA = ca.Gold - capA;
                float gapB = cb.Gold - capB;
                if (gapA * gapB >= 0f) continue; // 同号或零：无互补缺口

                float weight = 1f / (1f + e.Cost * decay);
                float flow = Mathf.Min(Mathf.Abs(gapA), Mathf.Abs(gapB)) * weight * flowRatio;
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

            // 王国净余额 + 全图出口总额
            var balance = new Dictionary<long, long>(16);
            long totalExport = 0;
            for (int i = 0; i < flows.Count; i++)
            {
                var f = flows[i];
                totalExport += f.Amount;
                if (f.FromKingdomId == f.ToKingdomId) continue;
                long v;
                balance.TryGetValue(f.FromKingdomId, out v); balance[f.FromKingdomId] = v + f.Amount;
                balance.TryGetValue(f.ToKingdomId, out v);   balance[f.ToKingdomId] = v - f.Amount;
            }
            for (int i = 0; i < res.Kingdoms.Count; i++)
            {
                long net;
                if (balance.TryGetValue(res.Kingdoms[i].KingdomId, out net))
                    res.Kingdoms[i].TradeBalance = net;
            }
            res.TotalTradeVolume = totalExport;
        }

        /// <summary>海路出口上限约束（超限按比例缩放，保持各边相对结构）。</summary>
        private static void ApplySeaCapacity(List<TradeFlow> flows, Dictionary<long, int> boats)
        {
            var seaByK = new Dictionary<long, long>(8);
            for (int i = 0; i < flows.Count; i++)
            {
                var f = flows[i];
                if (!f.Sea) continue;
                long v;
                seaByK.TryGetValue(f.FromKingdomId, out v);
                seaByK[f.FromKingdomId] = v + f.Amount;
            }
            foreach (var kv in seaByK)
            {
                int b = 0;
                boats.TryGetValue(kv.Key, out b);
                long cap = (long)b * SeaCapacityPerBoat;
                if (kv.Value <= cap) continue; // 未超限 → 不动
                if (cap <= 0)
                {
                    // 无船 → 海路清零（Amount=0，下游 ApplyTradeFlows 会跳过零额边）
                    for (int i = 0; i < flows.Count; i++)
                    {
                        var f = flows[i];
                        if (!f.Sea || f.FromKingdomId != kv.Key) continue;
                        f.Amount = 0;
                    }
                    continue;
                }
                double scale = (double)cap / kv.Value;
                for (int i = 0; i < flows.Count; i++)
                {
                    var f = flows[i];
                    if (!f.Sea || f.FromKingdomId != kv.Key) continue;
                    long scaled = (long)(f.Amount * scale);
                    f.Amount = scaled > 0 ? scaled : 1L;
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
        /// 进口城 takeResource("gold", amt)，不足部分从进口王国成员金币 DeductCoins 兜底
        /// （与原版缴税同渠道；取款 = 入库，总量守恒，净≈0）。
        /// </summary>
        public static void ApplyTradeFlows()
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.TradeEnabled) return;
            var res = LastResult;
            if (res == null || res.TradeFlows.Count == 0) return;

            long gross = 0;
            long net = 0;
            foreach (var f in res.TradeFlows)
            {
                if (f.Amount <= 0) continue;
                var fromCity = FindCity(f.FromKingdomId, f.FromCityId);
                var toCity = FindCity(f.ToKingdomId, f.ToCityId);
                if (fromCity == null || toCity == null) continue;
                gross += f.Amount;

                // 进口城先付款（取款不足部分 → 居民金币 DeductCoins 兜底），
                // 出口城只收到实际支付额 —— 支付 = 入库，全图严格净≈0，无凭空创造
                long remaining = f.Amount;
                long paid = 0;
                try
                {
                    int have = toCity.getResourcesAmount("gold");
                    if (have > 0)
                    {
                        int take = (int)Mathf.Min(remaining, have);
                        toCity.takeResource("gold", take);
                        remaining -= take;
                        paid += take;
                    }
                }
                catch (System.Exception) { }

                if (remaining > 0)
                {
                    var units = _unitPool;
                    units.Clear();
                    var kingdom = GameHelpers.FindKingdom(f.ToKingdomId);
                    if (kingdom != null && kingdom.units != null) units.AddRange(kingdom.units);
                    long deducted = GameHelpers.DeductCoins(units, remaining);
                    remaining -= deducted;
                    paid += deducted;
                }

                // 出口城入库（只收实际支付额）
                if (paid > 0)
                {
                    try { fromCity.addResourcesToRandomStockpile("gold", (int)paid); } catch (System.Exception) { }
                }
                net += paid - f.Amount; // 入库(+) − 应付款(−)，未付清部分不结算（净≤0，无凭空创造）
            }
            if (gross > 0)
            {
                GameHelpers.Log($"[ClassicalEconomics] 地理贸易：{res.TradeFlows.Count} 条边共 {gross} 金币流动，净 {net:+0;-0}（按边城市仓库结算）");
            }
        }

        /// <summary>按 (王国, 城市坐标 id) 反查 City 对象（主线程；cityId = (x&lt;&lt;32)|y）。</summary>
        private static City FindCity(long kingdomId, long cityId)
        {
            var kingdom = GameHelpers.FindKingdom(kingdomId);
            if (kingdom == null) return null;
            int tx = (int)(cityId >> 32);
            int ty = (int)(cityId & 0xFFFFFFFFL);
            try
            {
                foreach (var c in kingdom.getCities())
                {
                    if (c == null) continue;
                    var t = c.getTile(false);
                    if (t != null && t.x == tx && t.y == ty) return c;
                }
            }
            catch (System.Exception) { }
            return null;
        }
    }
}
