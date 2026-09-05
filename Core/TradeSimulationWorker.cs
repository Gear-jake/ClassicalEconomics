using System;
using System.Collections.Generic;
using System.Threading;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 多线程经济统计引擎（主线程零计算）。
    /// 主线程仅采集（读取 Unity 对象 → 写入纯数据记录）；全部统计计算在后台线程完成，
    /// 结果由主线程轮询 <see cref="TryConsume"/> 消费后发布。后台线程绝不接触 Unity 对象。
    ///
    /// 统计内容（v1.3.0 起贸易模拟已移除）：
    /// - 全球 GDP / 人均财富 / 基尼系数（O(N log N) 升序排序单趟累加）；
    /// - 王国级聚合：GDP / 人均 / 基尼 / 就业 / 生产函数（产出 = Workers × Productivity × CapitalFactor，
    ///   资本因子含基础设施权重 × 治理加成 × 规模不经济，并乘法典 Productivity 快照）；
    /// - 区域价格指数（LocalPrice = 上期全局 CPI × 本地供需系数，clamp 0.5~2，
    ///   法典 Price 乘数叠加；仅统计展示，无跨城市金流）。
    /// 主线程采集时把法典聚合快照（LawMods）复制进 KingdomFacts，后台只读该拷贝，零字典访问。
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
            public LawMods LawMods; // 法典聚合快照（主线程采集时读一次；后台只读该拷贝，杜绝跨线程读写共享字典）
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
            public int Workers;       // 有职业人口
            public float Productivity; // 平均劳动生产率（职业倍率均值）
            public float Production;  // 年产出 = Workers × Productivity × CapitalFactor（生产函数）
            public int Specialty;     // BiomeSpecialty（主线程采集阶段读取的纯数据）
            public float LocalPrice;  // 区域价格指数（全局 CPI × 本地供需系数，1.0=基准）
        }

        /// <summary>一轮周期模拟结果。</summary>
        public class CycleResult
        {
            public float GlobalGDP;
            public float AvgWealth;
            public float GiniCoefficient;
            public int AliveActorCount;
            public int CycleIndex;
            public float TotalProduction; // 全球年总产出（生产函数供给侧）

            public readonly List<KingdomSim> Kingdoms = new List<KingdomSim>(16);
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

        // ===== 主线程采集缓冲（每周期 Clear 复用，避免 GC 分配）=====
        private static List<ActorRecord> _collectActors = new List<ActorRecord>(4096);
        private static List<KingdomFacts> _collectKingdoms = new List<KingdomFacts>(32);

        // ===== 后台计算缓冲与握手 =====
        private static List<ActorRecord> _computeActors = new List<ActorRecord>(4096);
        private static List<KingdomFacts> _computeKingdoms = new List<KingdomFacts>(32);
        private static volatile bool _posting;    // 主线程：周期已提交待消费
        private static volatile bool _computing;  // 后台线程：计算进行中
        private static volatile CycleResult _readyResult; // 后台完成的待消费结果
        private static volatile string _workerError;      // 后台线程异常信息（主线程消费时记录日志，避免后台线程调用 Unity API）
        private static int _cycleIndex;
        private static int _generation;           // 代际计数：防止过期后台任务写入结果
        private static int _activeWorkers;         // Reset 后仍在退出的旧任务；归零前禁止复用计算缓冲
        private static readonly object _lifecycleLock = new object();

        /// <summary>最近一次已消费的结果（供各引擎读取；主线程使用，不跨周期持有）。</summary>
        public static CycleResult LastResult { get; private set; }


        // ===== 主线程采集入口 =====

        /// <summary>开始新一轮采集（清空纯数据缓冲）。</summary>
        public static void BeginCycle()
        {
            _collectActors.Clear();
            _collectKingdoms.Clear();
        }

        public static void AddActor(float wealth, long kingdomId, byte jobCode)
        {
            _collectActors.Add(new ActorRecord { Wealth = wealth, KingdomId = kingdomId, JobCode = jobCode });
        }

        public static void AddKingdom(long id, string name, int population, int capacity,
            long food, int cities, int boats, int specialty, float cityX, float cityY, LawMods mods = default(LawMods))
        {
            _collectKingdoms.Add(new KingdomFacts
            {
                Id = id, Name = name, Population = population, Capacity = capacity,
                Food = food, Cities = cities, Boats = boats, Specialty = specialty,
                CityX = cityX, CityY = cityY, LawMods = mods
            });
        }

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
            _computing = true;
            _posting = true;
            System.Threading.Interlocked.Increment(ref _activeWorkers);
            try
            {
                bool queued = ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var r = Compute(actors, kingdoms, idx);
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
                res = Compute(_computeActors, _computeKingdoms, _cycleIndex); // 兜底：同步重算
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
                }
            }
        }

        /// <summary>
        /// 手动采集/实时刷新：同步计算并立即发布（按钮触发，不等后台线程）。
        /// advanceCycle=false 用于实时刷新（不推进周期号，避免 HUD"周期 #N"暴涨）。
        /// 调用前须先完成采集（DataCollector.Collect 已完成缓冲交换）。
        /// </summary>
        public static void ComputeAndConsumeSync(bool advanceCycle = true)
        {
            if (System.Threading.Volatile.Read(ref _activeWorkers) > 0)
                throw new InvalidOperationException("Cannot run synchronous computation while a worker is active.");
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
            Publish(Compute(_computeActors, _computeKingdoms, _cycleIndex));
        }

        /// <summary>世界重置（新地图/新游戏）时清空在途周期与结果。</summary>
        public static void Reset()
        {
            ClearWorldReferences();
            _cycleIndex = 0;
            _collectActors.Clear();
            _collectKingdoms.Clear();
        }

        private static void SwapBuffers()
        {
            var ta = _computeActors; _computeActors = _collectActors; _collectActors = ta; _collectActors.Clear();
            var tk = _computeKingdoms; _computeKingdoms = _collectKingdoms; _collectKingdoms = tk; _collectKingdoms.Clear();
        }

        private static void Publish(CycleResult res)
        {
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

        private static CycleResult Compute(List<ActorRecord> actors, List<KingdomFacts> kingdoms, int cycleIndex)
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
                    Specialty = f.Specialty
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
                    // 法典：生产函数乘数（教育/补贴/计划 vs 自由市场等聚合；读采集时快照，后台零字典访问）
                    ks.Production *= f.LawMods.Productivity;
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


            return res;
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
        /// 静态字典 _accScratch 由 MemoryCleanupEngine 通过 ForTrim 访问器重建缩容。</summary>
        public static int TrimMemory()
        {
            int shrunk = 0;
            shrunk += TrimList(_collectActors);
            shrunk += TrimList(_collectKingdoms);
            shrunk += TrimList(_computeActors);
            shrunk += TrimList(_computeKingdoms);
            shrunk += TrimList(_globalWealthScratch);
            shrunk += TrimList(_validKingdomScratch);

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

            // 非 readonly 字典 _accScratch 由 MemoryCleanupEngine 通过下方 ForTrim 访问器重建缩容
            // （本文件受 Test-AllocHygiene 锚定，禁止运行时集合分配）。

            return shrunk;
        }

        /// <summary>供 MemoryCleanupEngine 重建缩容时读取当前引用（仅空闲期调用）。</summary>
        internal static Dictionary<long, Accum> AccScratchForTrim => _accScratch;

        /// <summary>将重建后的紧凑字典换回（仅 MemoryCleanupEngine 空闲期调用）。</summary>
        internal static void ReplaceAccScratchForTrim(Dictionary<long, Accum> compact) { _accScratch = compact; }
    }
}
