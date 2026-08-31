using System.Collections.Generic;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 跨引擎共享的公共辅助方法：查找王国、读取安全名称、日志输出、集合快照。
    /// 消除 SocialCrisisEngine / UnrestEngine / SpendingEngine / EconomyCycleModulator 中的重复实现。
    /// </summary>
    internal static class GameHelpers
    {
        // ===== 王国查找（SocialCrisisEngine、EconomyCycleModulator 中均有重复实现）=====

        private static readonly Dictionary<long, Kingdom> _kingdomById = new Dictionary<long, Kingdom>();
        private static object _kingdomIndexWorld;
        private static object _kingdomIndexSource;
        private static int _kingdomIndexCount = -1;

        /// <summary>按王国 ID 在 World.world.kingdoms 中查找原生 Kingdom 对象；不存在返回 null。</summary>
        public static Kingdom FindKingdom(long kingdomId)
        {
            if (kingdomId == 0) return null;

            var world = World.world;
            var kingdoms = world != null ? world.kingdoms : null;
            if (kingdoms == null)
            {
                ClearKingdomIndex();
                return null;
            }

            bool rebuilt = false;
            if (!ReferenceEquals(_kingdomIndexWorld, world)
                || !ReferenceEquals(_kingdomIndexSource, kingdoms)
                || _kingdomIndexCount != kingdoms.Count)
            {
                RebuildKingdomIndex();
                rebuilt = true;
            }

            Kingdom kingdom;
            if (_kingdomById.TryGetValue(kingdomId, out kingdom)) return kingdom;

            // Count 不变的替换无法由轻量标记识别；首次 miss 时补一次重建。
            if (!rebuilt)
            {
                RebuildKingdomIndex();
                if (_kingdomById.TryGetValue(kingdomId, out kingdom)) return kingdom;
            }
            return null;
        }

        /// <summary>采集周期开始时刷新索引，覆盖王国数量不变但对象已替换的情况。</summary>
        public static void RefreshKingdomIndex()
        {
            RebuildKingdomIndex();
        }

        private static void RebuildKingdomIndex()
        {
            _kingdomById.Clear();
            var world = World.world;
            var kingdoms = world != null ? world.kingdoms : null;
            if (kingdoms == null)
            {
                _kingdomIndexWorld = null;
                _kingdomIndexSource = null;
                _kingdomIndexCount = -1;
                return;
            }

            foreach (var kingdom in kingdoms)
            {
                if (kingdom != null && kingdom.data != null && kingdom.data.id != 0)
                    _kingdomById[kingdom.data.id] = kingdom;
            }
            _kingdomIndexWorld = world;
            _kingdomIndexSource = kingdoms;
            _kingdomIndexCount = kingdoms.Count;
        }

        private static void ClearKingdomIndex()
        {
            _kingdomById.Clear();
            _kingdomIndexWorld = null;
            _kingdomIndexSource = null;
            _kingdomIndexCount = -1;
        }

        // ===== 安全名称读取（多引擎均有重复实现）=====

        /// <summary>安全读取 Actor.name（半销毁对象可能抛异常），失败返回 "?"。</summary>
        public static string SafeName(Actor a)
        {
            if (a == null) return "?";
            try
            {
                var n = a.name;
                return string.IsNullOrEmpty(n) ? "?" : n;
            }
            catch (System.Exception) { return "?"; }
        }

        /// <summary>安全读取 Kingdom.data.name；失败返回 "?"。</summary>
        public static string SafeKingdomName(Kingdom k)
        {
            try
            {
                if (k != null && k.data != null && k.data.name != null)
                    return k.data.name;
            }
            catch (System.Exception) { }
            return "?";
        }

        /// <summary>安全读取 City.name；失败返回 "?"。</summary>
        public static string SafeCityName(City c)
        {
            if (c == null) return "?";
            try
            {
                var n = c.name;
                return string.IsNullOrEmpty(n) ? "?" : n;
            }
            catch (System.Exception) { return "?"; }
        }

        // ===== 日志（仅在配置开启时输出，多引擎均有重复实现）=====

        /// <summary>当 UnrestConfig.LogToWorldLog 开启时输出一条 Debug.Log。</summary>
        public static void Log(string msg)
        {
            if (UnrestConfig.Instance.LogToWorldLog)
                Debug.Log(msg);
        }

        // ===== 游戏内醒目提示（重大事件：经济崩溃/泡沫破裂/革命/动荡/时代开启）=====

        /// <summary>
        /// 在游戏界面顶部显示醒目提示（WorldTip 顶部横幅）。
        /// 与日志解耦：始终显示（不依赖 LogToWorldLog），失败静默（不崩溃）。
        /// 仅可在主线程调用（事件均在主线程年度评估中触发）。
        /// </summary>
        public static void Notify(string msg)
        {
            try { WorldTip.showNowTop(msg, false); } catch (System.Exception) { }
        }

        /// <summary>
        /// 本地化版顶部横幅：按 Locales/*.json 的键取当前语言文案并格式化。
        /// 全部引擎的玩家可见横幅必须走本方法（Test-LocalizationCoverage 禁止
        /// Notify 直接携带硬编码 CJK 文案）。
        /// </summary>
        public static void NotifyLocalized(string key, params object[] args)
        {
            string text;
            try { text = args != null && args.Length > 0 ? string.Format(Services.LocalizationService.Get(key), args) : Services.LocalizationService.Get(key); }
            catch (System.Exception) { text = key; }
            Notify(text);
        }

        // ===== 集合快照（避免 foreach 时原生系统修改集合抛异常）=====

        /// <summary>将 kingdoms 拷贝到复用的静态缓冲，返回该缓冲；不分配新对象。</summary>
        private static readonly List<Kingdom> _kingdomSnapshot = new List<Kingdom>();

        /// <summary>进入无世界状态时解除复用缓冲对旧世界对象的引用。</summary>
        public static void ClearWorldReferences()
        {
            _kingdomSnapshot.Clear();
            ClearKingdomIndex();
            _redistRich.Clear();
            _redistPoor.Clear();
        }

        /// <summary>获取 kingdoms 列表的复用快照（每年评估时使用，避免 GC 分配）。</summary>
        public static List<Kingdom> KingdomSnapshot()
        {
            var list = _kingdomSnapshot;
            list.Clear();
            _kingdomById.Clear();

            var world = World.world;
            var kingdoms = world != null ? world.kingdoms : null;
            if (kingdoms == null)
            {
                _kingdomIndexWorld = null;
                _kingdomIndexSource = null;
                _kingdomIndexCount = -1;
                return list;
            }

            foreach (var kingdom in kingdoms)
            {
                list.Add(kingdom);
                if (kingdom != null && kingdom.data != null && kingdom.data.id != 0)
                    _kingdomById[kingdom.data.id] = kingdom;
            }
            _kingdomIndexWorld = world;
            _kingdomIndexSource = kingdoms;
            _kingdomIndexCount = kingdoms.Count;
            return list;
        }

        // ===== 智慧生物遍历辅助（UnrestEngine / SocialCrisisEngine 原重复实现合并）=====

        /// <summary>从 kingdom.units 中取第一个存活的智慧（civ）Actor；无则返回 null。</summary>
        public static Actor FindFirstCivActor(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.units == null) return null;
            foreach (var a in kingdom.units)
            {
                if (!IsCivilizedActor(a)) continue;
                return a;
            }
            return null;
        }

        // ===== Actor.data 反射（SpendingEngine / DamageTracker 原重复实现合并）=====

        private static System.Reflection.FieldInfo _actorDataField;

        /// <summary>反射读取 Actor 的私有 data 字段（编译期不可见）；失败返回 null。</summary>
        public static object GetActorData(Actor actor)
        {
            try
            {
                if (_actorDataField == null)
                {
                    _actorDataField = typeof(Actor).GetField("data",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                }
                return _actorDataField != null ? _actorDataField.GetValue(actor) : null;
            }
            catch (System.Exception) { return null; }
        }

        // ===== 安全财富读取（多引擎重复实现合并）=====

        /// <summary>安全读取 Actor 的（money + loot）总额；失败返回 false 且 out 置 0。</summary>
        public static bool TryGetWealth(Actor a, out float wealth)
        {
            wealth = 0f;
            if (a == null) return false;
            try
            {
                wealth = a.money + a.loot;
                if (float.IsNaN(wealth) || float.IsInfinity(wealth))
                {
                    wealth = 0f;
                    return false;
                }
                return true;
            }
            catch (System.Exception) { return false; }
        }

        public static bool IsCivilizedActor(Actor actor)
        {
            if (actor == null) return false;
            try
            {
                if (!actor.isAlive()) return false;
                if (actor.city != null) return true;
                return actor.hasKingdom() && actor.kingdom != null;
            }
            catch (System.Exception) { return false; }
        }

        // ===== 安全死亡调用（SocialCrisisEngine / PopulationEngine 共用）=====

        private static System.Reflection.MethodInfo _dieMethod;

        /// <summary>反射调用 Actor.die（原生私有方法），失败时静默跳过；成功返回 true。</summary>
        public static bool TryDieActor(Actor actor, AttackType type)
        {
            try
            {
                if (_dieMethod == null)
                {
                    _dieMethod = typeof(Actor).GetMethod("die",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                }
                if (_dieMethod == null) return false;
                _dieMethod.Invoke(actor, new object[] { true, type, false, false });
                return true;
            }
            catch (System.Exception) { return false; }
        }

        // ===== 国王退位（PolicyEngine 国家政策失败后果）=====

        private static System.Reflection.MethodInfo _removeKingMethod;

        /// <summary>反射调用 Kingdom.removeKing（internal）：国王转回平民，游戏稍后自动产生新王；失败返回 false。</summary>
        public static bool TryRemoveKing(Kingdom kingdom)
        {
            try
            {
                if (_removeKingMethod == null)
                {
                    _removeKingMethod = typeof(Kingdom).GetMethod("removeKing",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                }
                if (_removeKingMethod == null) return false;
                _removeKingMethod.Invoke(kingdom, null);
                return true;
            }
            catch (System.Exception) { return false; }
        }

        // ===== 集合洗牌（EraEngine / UnrestEngine 原 Fisher-Yates 重复实现合并）=====

        /// <summary>Fisher-Yates 原地洗牌（UnityEngine.Random）；null 或元素不足 2 时静默返回。</summary>
        public static void Shuffle<T>(List<T> list)
        {
            if (list == null || list.Count < 2) return;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }

        // ===== 批量扣款（SocialCrisisEngine 战争掠夺/革命抽取原重复实现合并）=====
        /// <summary>从 units 快照中按上限逐人扣减金币，返回实际扣除总额。</summary>
        public static long DeductCoins(List<Actor> units, long limit)
        {
            if (units == null || limit <= 0) return 0L;
            long toDeduct = limit;
            foreach (var a in units)
            {
                if (a == null || !a.isAlive() || toDeduct <= 0) continue;
                try
                {
                    int coins = Mathf.Max(0, Mathf.RoundToInt(a.money));
                    if (coins <= 0) continue;
                    int take = System.Math.Min(coins, (int)System.Math.Min(toDeduct, (long)int.MaxValue));
                    a.addMoney(-take);
                    toDeduct -= take;
                }
                catch (System.Exception) { }
            }
            return limit - toDeduct;
        }

        /// <summary>向 Actor 分块增加正数金币，避免 long 转 int 溢出。</summary>
        internal static void AddPositiveMoney(Actor actor, long amount)
        {
            while (actor != null && amount > 0)
            {
                int chunk = (int)System.Math.Min(amount, int.MaxValue);
                actor.addMoney(chunk);
                amount -= chunk;
            }
        }

        // ===== 王国内劫富济贫（镇压/暴乱/革命后调用，事件驱动，常态零开销）=====

        private static readonly List<Actor> _redistRich = new List<Actor>(8);
        private static readonly List<Actor> _redistPoor = new List<Actor>(12);

        /// <summary>
        /// 王国内部劫富济贫：单遍遍历王国 units，找出最富 richCount 与最穷 poorCount 名智慧成员，
        /// 对富人征收"超出 全王国人均×capMult 部分 × taxRatio"的财富（每人上限为其财富的 50%），
        /// 均分给最穷成员。财富仅转移、总量守恒 → 直接降低该王国基尼系数。
        /// 返回实际抽取总额；无富余/无对象时返回 0。事件驱动（镇压/暴乱/革命后调用），非常态运行。
        /// </summary>
        public static long RedistributeWithinKingdom(Kingdom kingdom,
            int richCount, int poorCount, float taxRatio, float capMult)
        {
            if (kingdom == null || kingdom.units == null || kingdom.units.Count == 0) return 0L;
            if (richCount <= 0 || poorCount <= 0) return 0L;

            var rich = _redistRich; rich.Clear();
            var poor = _redistPoor; poor.Clear();
            float totalWealth = 0f;
            int count = 0;
            float richEdge = 0f, poorEdge = 0f; // 候选池边界缓存

            foreach (var a in kingdom.units)
            {
                if (!IsCivilizedActor(a)) continue;
                float w;
                if (!TryGetWealth(a, out w)) continue;
                totalWealth += w;
                count++;
                UpdateTopN(rich, ref richEdge, a, w, richCount, true);   // 最富（越界替换）
                UpdateTopN(poor, ref poorEdge, a, w, poorCount, false);  // 最穷（越界替换）
            }
            if (count == 0) return 0L;

            float avg = totalWealth / count;
            float taxLine = Mathf.Max(1f, avg * capMult);

            // 逐富人扣税，并只累计实际成功扣除额。
            long totalTax = 0;
            for (int i = 0; i < rich.Count; i++)
            {
                float w;
                if (!TryGetWealth(rich[i], out w)) continue;
                if (w <= taxLine) continue;
                long tax = (long)Mathf.Min((w - taxLine) * taxRatio, w * 0.5f);
                if (tax <= 0) continue;
                int charged = (int)System.Math.Min(tax, int.MaxValue);
                try { rich[i].addMoney(-charged); totalTax += charged; } catch (System.Exception) { }
            }
            if (totalTax <= 0) return 0L;

            // 均分给最穷成员（余数补第一名）
            if (poor.Count == 0) return 0L; // 防御：无穷人候选时避免除零（正常路径 count>0 时池必非空）
            long per = totalTax / poor.Count;
            for (int i = 0; i < poor.Count; i++)
            {
                try { AddPositiveMoney(poor[i], per + (i == 0 ? totalTax - per * poor.Count : 0)); }
                catch (System.Exception) { }
            }
            return totalTax;
        }

        /// <summary>
        /// 王国内杀富济贫（街头起义/革命后调用，事件驱动，常态零开销）：
        /// 找出王国最富 richCount 名智慧成员，按 redistRatio 抽取其财富（抽完后处决），
        /// 抽出的财富均分给最穷 poorCount 名成员。
        /// "杀富"（富人绝对损失 + 人口消失）+ "济贫"（穷人获得财富）双通道直接降低基尼系数。
        /// 返回处决的富人数；无对象/无富余时返回 0。
        /// </summary>
        public static int KillRichGiveToPoor(Kingdom kingdom,
            int richCount, int poorCount, float redistRatio)
        {
            if (kingdom == null || kingdom.units == null || kingdom.units.Count == 0) return 0;
            if (richCount <= 0 || poorCount <= 0) return 0;

            var rich = _redistRich; rich.Clear();
            var poor = _redistPoor; poor.Clear();
            float richEdge = 0f, poorEdge = 0f;

            foreach (var a in kingdom.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (!IsCivilizedActor(a)) continue;
                float w;
                if (!TryGetWealth(a, out w)) continue;
                UpdateTopN(rich, ref richEdge, a, w, richCount, true);   // 最富（越界替换）
                UpdateTopN(poor, ref poorEdge, a, w, poorCount, false);  // 最穷（越界替换）
            }
            if (rich.Count == 0 || poor.Count == 0) return 0;

            // 第一遍：抽取富人财富（只取部分比例，避免把王国财富全部抽干）
            long totalLoot = 0;
            for (int i = 0; i < rich.Count; i++)
            {
                float w;
                if (!TryGetWealth(rich[i], out w)) continue;
                if (w <= 0) continue;
                long loot = (long)(w * redistRatio);
                if (loot <= 0) continue;
                int charged = (int)System.Math.Min(loot, int.MaxValue);
                try { rich[i].addMoney(-charged); } catch (System.Exception) { continue; }
                totalLoot += charged;
            }
            if (totalLoot <= 0) return 0;

            // 第二遍：均分给最穷成员（余数补第一名）
            long per = totalLoot / poor.Count;
            for (int i = 0; i < poor.Count; i++)
            {
                try { AddPositiveMoney(poor[i], per + (i == 0 ? totalLoot - per * poor.Count : 0)); }
                catch (System.Exception) { }
            }

            // 第三遍：处决最富成员（杀富）——跳过与最穷池重叠者（人口极少时避免误杀刚分到钱的穷人）
            int killed = 0;
            for (int i = 0; i < rich.Count; i++)
            {
                if (rich[i] == null || !rich[i].isAlive()) continue;
                bool overlaps = false;
                for (int j = 0; j < poor.Count; j++)
                    if (poor[j] == rich[i]) { overlaps = true; break; }
                if (overlaps) continue;
                if (TryDieActor(rich[i], AttackType.Other)) killed++;
            }
            return killed;
        }

        /// <summary>
        /// 维护定长 TopN 候选集合（带边界缓存）：
        /// 容量未满直接加入并更新边界；已满时 O(1) 与边界比较快速淘汰，
        /// 仅更富/更穷时替换边界成员并重算边界（替换次数 ≤ cap，成本可忽略）。
        /// 调用方需提供边界缓存变量（最富集合=最小值 / 最穷集合=最大值），遍历前应初始化为 0。
        /// 由 RedistributeWithinKingdom 使用。
        /// </summary>
        /// <summary>
        /// 外交赠礼：把金币均分给目标国存活国民（守恒，余数补第一人），返回实际发放金额。
        /// 无国民可领时返回 0（调用方应退回金库）。
        /// </summary>
        public static long GiveToKingdomMembers(Kingdom kingdom, long amount)
        {
            if (kingdom == null || kingdom.units == null || amount <= 0) return 0L;
            var list = _redistPoor;
            list.Clear();
            try
            {
                foreach (var a in kingdom.units)
                    if (a != null && a.isAlive()) list.Add(a);
            }
            catch (System.Exception) { }
            if (list.Count == 0) return 0L;
            long per = amount / list.Count;
            long given = 0;
            for (int i = 0; i < list.Count; i++)
            {
                long share = per + (i == 0 ? amount - per * list.Count : 0);
                if (share > 0) { AddPositiveMoney(list[i], share); given += share; }
            }
            return given;
        }

        internal static void UpdateTopN(List<Actor> pool, ref float edge,
            Actor a, float w, int cap, bool richest)
        {
            if (pool.Count < cap)
            {
                pool.Add(a);
                // 池未满：edge 更新为当前集合极值
                if (pool.Count == 1) edge = w;
                else edge = richest ? Mathf.Min(edge, w) : Mathf.Max(edge, w);
                return;
            }
            // O(1) 快速淘汰：未超过当前边界
            if (richest ? w <= edge : w >= edge) return;

            // 替换边界成员（最富集合的最小者 / 最穷集合的最大者）
            int edgeIdx = 0;
            float cur = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                float wi;
                if (!TryGetWealth(pool[i], out wi)) continue;
                if (i == 0) { cur = wi; edgeIdx = 0; continue; }
                if (richest ? wi < cur : wi > cur) { cur = wi; edgeIdx = i; }
            }
            pool[edgeIdx] = a;
            // 重算替换后的真实边界；不能沿用被替换成员的旧边界。
            edge = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                float wi;
                if (!TryGetWealth(pool[i], out wi)) continue;
                if (i == 0) edge = wi;
                else edge = richest ? Mathf.Min(edge, wi) : Mathf.Max(edge, wi);
            }
        }
    }
}
