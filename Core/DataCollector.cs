using System.Collections.Generic;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 数据采集层：按周期遍历 World.world.units，读取每个存活 Actor 的
    /// 原生 money/loot 字段（只读，不写入原生数据）。
    /// 仅维护富豪榜 Top10 与年度富豪税候选池，不保留全量快照
    /// （统计计算由 EconomyEngine 在单遍遍历中顺带完成）。
    /// </summary>
    public static class DataCollector
    {
        /// <summary>富豪榜数据行（采集时顺带构建，供富豪榜直接使用，避免再次全量遍历）。</summary>
        public class RichEntryData
        {
            public long Id;
            public string Name;
            public string Kingdom;
            public float Wealth;
        }

        /// <summary>最近一次采集得到的财富 Top10（降序；未采集时为空）。</summary>
        public static readonly List<RichEntryData> TopRich = new List<RichEntryData>(10);

        /// <summary>地理贸易网络：本周期 cityId → City 引用映射（仅主线程寻路用，每周期复用防 GC）。</summary>
        private static readonly Dictionary<long, City> _cityRefs = new Dictionary<long, City>(128);

        /// <summary>
        /// 采集时顺带收集的"富裕"智慧生物（wealth &gt; SpendingEngine.WealthyThreshold），
        /// 供 SpendingEngine.RunOncePerYear 直接消费，避免每年重复全量遍历。
        /// </summary>
        public static readonly List<Actor> WealthyPool = new List<Actor>();

        /// <summary>富豪税"贫困线以下"公民缓冲（收税单遍顺带收集，避免 ApplyWealthTax 再全量遍历两遍）。</summary>
        private static readonly List<Actor> _poorPool = new List<Actor>(256);

        // 富豪榜条目对象池：每年采集最多新建 0 个对象（复用池中条目）
        private static readonly List<RichEntryData> _entryPool = new List<RichEntryData>(10);

        /// <summary>进入主菜单或切换世界时释放 Unity 世界对象引用，不影响经济历史。</summary>
        public static void ClearWorldReferences()
        {
            for (int i = 0; i < TopRich.Count; i++) ReturnEntry(TopRich[i]);
            TopRich.Clear();
            WealthyPool.Clear();
            _poorPool.Clear();
            _cityRefs.Clear();
        }

        /// <summary>
        /// 执行一次采集。遍历 ActorManager.units_only_alive（public List&lt;Actor&gt;），
        /// 读取原生 money/loot 字段（只读），并顺带读取公民职业。
        /// 主线程仅"采集"：将纯数据（财富/王国/职业）写入 TradeSimulationWorker 缓冲，
        /// 全部统计计算在后台线程完成；富豪榜/富池等需 Actor 引用的池仍在主线程维护。
        /// 仅采集开智文明种族（actor.asset.civ == true），排除野兽动物。
        /// applySideEffects=false 用于实时刷新（跳过工资发放等年度副作用，只做统计）。
        /// postCycle=false 用于按钮同步路径（采集后由调用方 ComputeAndConsumeSync 同步计算，不投递后台任务）。
        /// 返回后台统计是否提交成功（false = 已有周期在途，调用方应稍后重试）。
        /// </summary>
        public static bool Collect(bool applySideEffects = true, bool postCycle = true)
        {
            // 在途周期存在时拒绝本次采集：PostCycle 会被拒（数据滞留缓冲被下轮 BeginCycle 清空），
            // 且调用方随后的同步计算会以 _generation++ 作废在途任务，破坏年度周期（S2 根因防护）。
            if (postCycle && TradeSimulationWorker.IsBusy()) return false;
            // 将上一轮富豪榜条目归还对象池，避免每年新建
            for (int i = 0; i < TopRich.Count; i++) ReturnEntry(TopRich[i]);
            TopRich.Clear();
            WealthyPool.Clear();

            TradeSimulationWorker.BeginCycle();

            var aliveList = World.world != null && World.world.units != null
                ? World.world.units.units_only_alive : null;
            if (aliveList != null)
            {
                foreach (var actor in aliveList)
                {
                    if (actor == null || !actor.isAlive())
                        continue;
                    if (actor.asset == null || !actor.asset.civ)
                        continue;

                    float wealth;
                    if (!GameHelpers.TryGetWealth(actor, out wealth))
                        continue;

                    long kid = 0L;
                    try
                    {
                        if (actor.hasKingdom() && actor.kingdom != null && actor.kingdom.data != null)
                            kid = actor.kingdom.data.id;
                    }
                    catch (System.Exception) { }

                    byte jobCode = LaborEngine.JobCodeOf(actor);
                    // 纯数据记录 → 后台统计
                    TradeSimulationWorker.AddActor(wealth, kid, jobCode);

                    // 富豪榜 Top10（仅上榜竞争时插入，成本 O(1)~O(10)）
                    UpdateTopRich(actor, wealth);

                    // 富裕生物供 SpendingEngine 消费（避免其再次全量遍历）
                    if (wealth > SpendingEngine.WealthyThreshold) WealthyPool.Add(actor);

                    // 劳动分工：按职业发放工资（劳动创造财富）。实时刷新跳过此副作用。
                    if (applySideEffects) LaborEngine.PayWage(actor, jobCode);
                }
            }

            // 王国事实（人口/承载/食物/城市/船只/首都坐标）→ 纯数据，供后台人口/贸易模拟
            var kingdoms = World.world != null ? World.world.kingdoms : null;
            if (kingdoms != null)
            {
                foreach (var k in kingdoms)
                {
                    if (k == null || k.data == null) continue;
                    int pop = 0, cap = 0, cities = 0, boats = 0;
                    long food = 0L;
                    float cx = float.NaN, cy = float.NaN; // 首都坐标（反射读取失败保持 NaN 哨兵，后台据此跳过距离计算）
                    try { pop = k.getPopulationTotal(); } catch (System.Exception) { }
                    try { cap = k.getPopulationTotalPossible(); } catch (System.Exception) { }
                    try { food = k.countTotalFood(); } catch (System.Exception) { }
                    try { foreach (var c in k.getCities()) cities++; } catch (System.Exception) { }
                    try { boats = k.countBoats(); } catch (System.Exception) { }
                    try { BiomeEconomy.TryGetCapitalCoords(k.data.id, out cx, out cy); } catch (System.Exception) { }
                    TradeSimulationWorker.AddKingdom(k.data.id, GameHelpers.SafeKingdomName(k),
                        pop, cap, food, cities, boats, (int)BiomeEconomy.GetSpecialty(k.data.id), cx, cy);
                }
            }

            // 城市快照（地理贸易网络节点）→ 纯数据，供后台按边贸易计算；
            // 同时维护 cityId → City 引用映射（仅主线程寻路用，每周期复用防 GC）
            _cityRefs.Clear();
            if (kingdoms != null && UnrestConfig.Instance.TradeEnabled)
            {
                foreach (var k in kingdoms)
                {
                    if (k == null || k.data == null) continue;
                    long kid = k.data.id;
                    System.Collections.Generic.IEnumerable<City> cityList = null;
                    try { cityList = k.getCities(); } catch (System.Exception) { }
                    if (cityList == null) continue;
                    foreach (var c in cityList)
                    {
                        if (c == null) continue;
                        WorldTile tile = null;
                        try { tile = c.getTile(false); } catch (System.Exception) { }
                        if (tile == null) continue; // 无 tile 的城市无法定位，跳过
                        int tx = tile.x, ty = tile.y;
                        long cityId = ((long)tx << 32) | (uint)ty;
                        int gold = 0, buildings = 0, boats = 0, cap = 0;
                        // amount_gold 为属性（访问器 get_amount_gold，CS0571 不可显式调用）
                        try { gold = c.amount_gold; } catch (System.Exception) { }
                        try { buildings = c.countBuildings(); } catch (System.Exception) { }
                        try { boats = c.countBoats(); } catch (System.Exception) { }
                        // 原版仓库真实容量（游戏原版自带仓库系统）：ResourceLibrary.gold 为
                        // public static 资源资产（全局命名空间），storage_max 为公开 int 字段。
                        // 实测 storage_max 可能是「无上限」哨兵值（≈6 亿，接近 int 上限），
                        // 直接用作 gap 基准会让 gap=gold−6亿 恒为负、所有城市都是缺口 → 无贸易。
                        // 故加合理性检查：超过 10 万金币视为无效，回退到建筑数估算。
                        if (UnrestConfig.Instance.TradeUseRealStockpiles)
                        {
                            try
                            {
                                var goldAsset = ResourceLibrary.gold;
                                if (goldAsset != null)
                                {
                                    cap = goldAsset.storage_max;
                                    if (cap > 100000) cap = 0; // 哨兵值/无上限 → 回退建筑估算
                                }
                            }
                            catch (System.Exception) { cap = 0; }
                        }
                        // 邻国王国（City.neighbours_kingdoms 为 internal 不可访问）：
                        // 改由 PrepareRoutes 用王国几何距离（Kingdom.distanceBetweenKingdom）判定
                        TradeSimulationWorker.AddCitySnapshot(cityId, kid, GameHelpers.SafeCityName(c),
                            gold, buildings, boats, cap, tx, ty);
                        _cityRefs[cityId] = c;
                    }
                }
            }

            // 主线程寻路限流（更新缓存并复制到本周期边缓冲）——必须在 PostCycle 之前
            TradeSimulationWorker.PrepareRoutes(_cityRefs);

            // 提交后台计算（主线程零计算；结果由 EconomyTickRunner 轮询消费）。
            // postCycle=false（按钮同步路径）时不投递后台任务，交由调用方同步计算。
            if (!postCycle) return true;
            return TradeSimulationWorker.PostCycle();
        }

        /// <summary>
        /// 年度累进税（全员再分配）：对全球所有"财富 &gt; 人均×税线"的公民按
        /// "超出部分×税率"征税（每人单次上限自身财富 MaxRatio），均分给所有
        /// "财富 &lt; 人均×贫困线"的公民。覆盖全人口而非 TopN，规模与劳动工资
        /// 同量级，直接、持续地降低全球基尼系数。
        /// 每年一次全量遍历（读 money 只读 + 少量 addMoney），低频可接受。
        /// 依赖本周期全球人均（EconomyEngine.AvgWealth），须在后台统计消费后调用。
        /// </summary>
        public static void ApplyWealthTax()
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.WealthTaxEnabled) return;

            float avg = EconomyEngine.AvgWealth;
            if (avg <= 0f) return;
            float ratio = Mathf.Clamp(cfg.WealthTaxRatio, 0f, 0.5f);
            if (ratio <= 0f) return;

            float taxLine = avg * Mathf.Max(1f, cfg.WealthTaxLineMult);
            const float PoorLineMult = 0.8f;  // 贫困线：人均 × 0.8（内部固定，简化配置）
            const float MaxRatio = 0.5f;      // 单人单次扣税上限占其财富比例（内部固定）
            float poorLine = avg * PoorLineMult;

            var aliveList = World.world != null && World.world.units != null
                ? World.world.units.units_only_alive : null;
            if (aliveList == null) return;

            // 单遍：对超税线者收税（立即扣款）+ 顺带收集贫困线以下公民到复用缓冲。
            // 原来分三遍遍历（收税/统计穷人/发钱），现合并为一遍全量 + 一遍穷人数（远小于全量）。
            long totalTax = 0;
            var poor = _poorPool;
            poor.Clear();
            foreach (var actor in aliveList)
            {
                if (actor == null || !actor.isAlive()) continue;
                if (actor.asset == null || !actor.asset.civ) continue;
                float w;
                if (!GameHelpers.TryGetWealth(actor, out w)) continue;
                if (w > taxLine)
                {
                    long tax = (long)Mathf.Min((w - taxLine) * ratio, w * MaxRatio);
                    if (tax > 0)
                    {
                        try { actor.addMoney(-(int)tax); totalTax += tax; } catch (System.Exception) { }
                    }
                }
                else if (w < poorLine)
                {
                    poor.Add(actor); // 收集穷人，第二遍只遍历缓冲
                }
            }
            if (totalTax <= 0) return;

            int poorCount = poor.Count;
            if (poorCount <= 0) return;

            // 第二遍：税款均分给贫困线以下公民（只遍历穷人缓冲，余数补给第一个）。
            // 注意：per==0（税款总额 < 贫困人口数）时仍须分发——余数=totalTax 全部补给第一个穷人，
            // 保证收上来的税款绝不凭空消失（金币守恒）。
            long per = totalTax / poorCount;
            long remainder = totalTax - per * poorCount;
            for (int i = 0; i < poorCount; i++)
            {
                var actor = poor[i];
                if (actor == null || !actor.isAlive()) continue;
                long give = per + (i == 0 ? remainder : 0);
                if (give <= 0) continue;
                try { actor.addMoney((int)give); } catch (System.Exception) { }
            }

            GameHelpers.Log($"[ClassicalEconomics] 年度累进税：征税 {totalTax} 金币 → {poorCount} 名贫困公民（人均+{per}）");
        }

        /// <summary>将单位按财富降序插入 TopRich（最多保留 10 条）。</summary>
        private static void UpdateTopRich(Actor actor, float wealth)
        {
            if (TopRich.Count == 10 && wealth <= TopRich[TopRich.Count - 1].Wealth)
                return; // 快速淘汰：未超过当前末位

            string kingdomName = "";
            try
            {
                if (actor.hasKingdom() && actor.kingdom != null &&
                    actor.kingdom.data != null && actor.kingdom.data.name != null)
                    kingdomName = actor.kingdom.data.name;
            }
            catch (System.Exception) { }

            // 从对象池取条目，避免每年为 Top10 新建对象
            var entry = RentEntry();
            entry.Id = actor.id;
            entry.Name = GameHelpers.SafeName(actor);
            entry.Kingdom = kingdomName;
            entry.Wealth = wealth;

            int idx = 0;
            while (idx < TopRich.Count && TopRich[idx].Wealth >= wealth) idx++;
            TopRich.Insert(idx, entry);
            if (TopRich.Count > 10)
            {
                ReturnEntry(TopRich[10]);
                TopRich.RemoveAt(10);
            }
        }

        private static RichEntryData RentEntry()
        {
            if (_entryPool.Count > 0)
            {
                var e = _entryPool[_entryPool.Count - 1];
                _entryPool.RemoveAt(_entryPool.Count - 1);
                return e;
            }
            return new RichEntryData();
        }

        private static void ReturnEntry(RichEntryData e)
        {
            if (_entryPool.Count < 16) _entryPool.Add(e);
        }
    }
}
