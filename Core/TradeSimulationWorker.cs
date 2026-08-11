using System;
using System.Collections.Generic;
using System.Threading;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 多线程统计 + 王国贸易金流模拟引擎（主线程零计算）。
    /// 主线程仅采集（读取 Unity 对象 → 写入纯数据记录），全部统计计算
    /// （全局/王国 GDP、基尼、贸易金流、人口压力、劳动生产率）在后台线程完成，
    /// 结果由主线程轮询 <see cref="TryConsume"/> 消费后发布。
    /// 后台线程绝不接触 Unity 对象，只处理纯值类型/字符串记录 → 线程安全。
    ///
    /// 贸易金流（原版机制优先）：金币经城市仓库（gold 资源，与原版缴税同渠道）
    /// 在王国间零和结算——人均财富高于全球均值的王国为贸易顺差（获得金币），
    /// 低于者为逆差（支付金币），全王国顺差总额 = 逆差总额（总和为零，不凭空造币）。
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
            public long Food;
            public float FoodPerCapita;
            public float Pressure;    // 人口/承载（超载 &gt;1）
            public int Boats;
            public long TradeBalance; // 净贸易顺差（正=出口盈余，负=逆差）
            public int Workers;       // 有职业人口
            public float Productivity; // 平均劳动生产率（职业倍率均值）
            public float Production;  // 年产出 = Workers × Productivity × CapitalFactor（生产函数）
        }

        /// <summary>一轮周期模拟结果。</summary>
        public class CycleResult
        {
            public float GlobalGDP;
            public float AvgWealth;
            public float GiniCoefficient;
            public int AliveActorCount;
            public int CycleIndex;
            public long TotalTradeVolume; // 全王国出口总额（顺差之和）
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
        private static readonly List<Actor> _unitPool = new List<Actor>(64); // 贸易逆差扣款兜底
        private static readonly List<City> _cityPool = new List<City>(16);   // 贸易金流城市缓冲

        // ===== 后台计算缓冲与握手 =====
        private static List<ActorRecord> _computeActors = new List<ActorRecord>(4096);
        private static List<KingdomFacts> _computeKingdoms = new List<KingdomFacts>(32);
        private static volatile bool _posting;    // 主线程：周期已提交待消费
        private static volatile bool _computing;  // 后台线程：计算进行中
        private static volatile CycleResult _readyResult; // 后台完成的待消费结果
        private static volatile string _workerError;      // 后台线程异常信息（主线程消费时记录日志，避免后台线程调用 Unity API）
        private static int _cycleIndex;
        private static int _generation;           // 代际计数：防止过期后台任务写入结果

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
            long food, int cities, int boats)
        {
            _collectKingdoms.Add(new KingdomFacts
            {
                Id = id, Name = name, Population = population, Capacity = capacity,
                Food = food, Cities = cities, Boats = boats
            });
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
            _computing = true;
            _posting = true;
            try
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var r = Compute(actors, kingdoms, idx);
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
                res = Compute(_computeActors, _computeKingdoms, _cycleIndex); // 兜底：同步重算
            }
            _readyResult = null;
            _posting = false;
            Publish(res);
            return true;
        }

        /// <summary>
        /// 手动采集/实时刷新：同步计算并立即发布（按钮触发，不等后台线程）。
        /// advanceCycle=false 用于实时刷新（不推进周期号，避免 HUD"周期 #N"暴涨）。
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
            Publish(Compute(_computeActors, _computeKingdoms, _cycleIndex));
        }

        /// <summary>世界重置（新地图/新游戏）时清空在途周期与结果。</summary>
        public static void Reset()
        {
            _generation++; // 使在途后台任务过期
            _posting = false;
            _computing = false;
            _readyResult = null;
            _workerError = null;
            LastResult = null;
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

        private class Accum
        {
            public long Id;
            public double Gdp;
            public int Count;
            public int Workers;
            public double ProdSum;
            public readonly List<float> Wealths = new List<float>(256);
        }

        private static CycleResult Compute(List<ActorRecord> actors, List<KingdomFacts> kingdoms, int cycleIndex)
        {
            var res = new CycleResult { CycleIndex = cycleIndex };

            // --- 全局 + 王国聚合（单遍遍历）---
            double gdp = 0d;
            int count = 0;
            var acc = new Dictionary<long, Accum>(32);
            foreach (var r in actors)
            {
                double w = r.Wealth;
                gdp += w;
                count++;
                if (!acc.TryGetValue(r.KingdomId, out var a))
                {
                    a = new Accum { Id = r.KingdomId };
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
            var globalWealths = new List<float>(count);
            foreach (var r in actors) globalWealths.Add(r.Wealth);
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
                    Food = f.Food,
                    Boats = f.Boats,
                    FoodPerCapita = f.Population > 0 ? (float)f.Food / f.Population : 0f,
                    Pressure = f.Capacity > 0 ? (float)f.Population / f.Capacity : 0f
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

            // --- 贸易金流模拟（零和：顺差总额 = 逆差总额）---
            // --- 贸易金流模拟（比较优势：特长王国出口，非特长王国进口）---
            // 有 biome 特长（矿产/木材/粮食/贸易品）的王国生产特长产品 → 贸易顺差（金币流入）；
            // 无特长或特长弱的王国需要进口 → 贸易逆差（金币流出）。按王国 GDP ±10% 限幅防极端。
            double totalExport = 0d;
            foreach (var ks in res.Kingdoms)
            {
                if (ks.KingdomId == 0 || ks.ActorCount <= 0) continue;
                var specialty = BiomeEconomy.GetSpecialty(ks.KingdomId);
                float bonus = BiomeEconomy.GetBonus(specialty);
                // 贸易余额 = 特长产出加成 - 基础需求（3%）
                double balance = (bonus - 0.03d) * ks.GDP;
                double cap = Math.Abs(ks.GDP) * 0.10d;
                if (balance > cap) balance = cap;
                else if (balance < -cap) balance = -cap;
                ks.TradeBalance = (long)balance;
                if (balance > 0) totalExport += balance;
            }
            res.TotalTradeVolume = (long)totalExport;
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

        // ===== 贸易金流应用（主线程：金币经城市仓库结算）=====

        /// <summary>
        /// 将本周期贸易金流应用到世界：顺差王国经城市仓库获得金币，
        /// 逆差王国从城市仓库支付（不足时从成员金币兜底扣除）。比例由配置 TradeFlowRatio 控制。
        /// </summary>
        public static void ApplyTradeFlows()
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.TradeEnabled) return;
            var res = LastResult;
            if (res == null || res.Kingdoms.Count == 0) return;
            float ratio = Mathf.Clamp(cfg.TradeFlowRatio, 0f, 0.2f);
            if (ratio <= 0f) return;

            // 残差分配法：先计算每个王国精确浮点贸易额并截断为整数，
            // 再将截断残差按 |精确值| 大小分配给最大王国，保证 Σ=0 严格零和。
            var entries = new List<(Kingdom kingdom, long floored, double precise)>(res.Kingdoms.Count);
            double totalPrecise = 0d;
            long totalFloored = 0;

            foreach (var ks in res.Kingdoms)
            {
                if (ks.KingdomId == 0 || ks.TradeBalance == 0) continue;
                var kingdom = GameHelpers.FindKingdom(ks.KingdomId);
                if (kingdom == null) continue;
                double precise = ks.TradeBalance * ratio;
                long floored = (long)precise; // 向零截断
                if (floored == 0) continue;
                entries.Add((kingdom, floored, precise));
                totalPrecise += precise;
                totalFloored += floored;
            }

            // 残差 = 四舍五入(精确总和) - 截断总和，分配给 |精确值| 最大的王国
            long targetTotal = (long)Math.Round(totalPrecise);
            long residual = targetTotal - totalFloored;
            if (residual != 0 && entries.Count > 0)
            {
                entries.Sort((a, b) => Math.Abs(b.precise).CompareTo(Math.Abs(a.precise)));
                for (int i = 0; i < entries.Count && residual != 0; i++)
                {
                    var e = entries[i];
                    if (residual > 0)
                    {
                        entries[i] = (e.kingdom, e.floored + 1, e.precise);
                        residual--;
                    }
                    else
                    {
                        entries[i] = (e.kingdom, e.floored - 1, e.precise);
                        residual++;
                    }
                }
            }

            // 应用贸易金流（此时 Σ amount = targetTotal ≈ 0 严格成立）
            long net = 0;
            foreach (var (kingdom, amount, _) in entries)
            {
                if (amount > 0) CreditKingdom(kingdom, amount);
                else if (amount < 0) DebitKingdom(kingdom, -amount);
                net += amount;
            }

            if (net != 0)
            {
                GameHelpers.Log($"[ClassicalEconomics] 王国贸易金流：残差分配后净流动 {net:+0;-0}（应为0，非零表示异常）");
            }
        }

        /// <summary>向王国各城市仓库分发金币（与原版缴税入库同渠道）。单遍遍历，城市先入复用缓冲。</summary>
        private static void CreditKingdom(Kingdom kingdom, long amount)
        {
            var cities = _cityPool;
            cities.Clear();
            try { cities.AddRange(kingdom.getCities()); } catch (System.Exception) { return; }
            if (cities.Count == 0) return;
            long per = amount / cities.Count;
            if (per <= 0) return;
            foreach (var c in cities)
            {
                if (c == null) continue;
                try { c.addResourcesToRandomStockpile("gold", (int)per); } catch (System.Exception) { }
            }
        }

        /// <summary>从王国各城市仓库收取金币，不足部分从成员金币兜底扣除。</summary>
        private static void DebitKingdom(Kingdom kingdom, long amount)
        {
            long remaining = amount;
            var cities = _cityPool;
            cities.Clear();
            try { cities.AddRange(kingdom.getCities()); } catch (System.Exception) { return; }
            foreach (var c in cities)
            {
                if (c == null || remaining <= 0) continue;
                int have;
                try { have = c.getResourcesAmount("gold"); } catch (System.Exception) { continue; }
                if (have <= 0) continue;
                int take = (int)Math.Min(remaining, have);
                try { c.takeResource("gold", take); } catch (System.Exception) { }
                remaining -= take;
            }
            if (remaining > 0)
            {
                // 兜底：从王国成员金币扣除（复用批量扣款）
                var units = _unitPool;
                units.Clear();
                if (kingdom.units != null) units.AddRange(kingdom.units);
                GameHelpers.DeductCoins(units, remaining);
            }
        }
    }
}
