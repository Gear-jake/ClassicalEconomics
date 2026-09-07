using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;
using Newtonsoft.Json;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 王国抉择事件系统（v1.4.0）：带选项与后果的王国事件，填补中央银行家的互动性空洞。
    /// - 数据：模组根 events.json（结构），文本在 Locales/*.json 四语键（ev_&lt;id&gt;_*），JsonConvert 类型化反序列化；
    ///   加载失败 fail-open（空池 + 警告一次），绝不拖垮经济模拟。
    /// - 触发：AnnualPipeline 的 Events 阶段每年调用 EvaluateYear——条件过滤 + 概率抽签；
    ///   每国每年最多 1 个；同事件按 cooldownYears 冷却；全局冷却 event_cooldown_years。
    /// - 呈现：玩家国事件进挂起池 → 弹非模态抉择小窗（EventChoiceWindow）+ 内阁待办区，
    ///   超时（timeoutYears）自动执行 fallback 选项；AI 国按国性权重（styleWeights）立即决策，结果只进事件流。
    /// - 后果（Q4-A 零新引擎字段，全部组合现有通道）：金库收支（GDP 比例）、居民征税、济贫分发、
    ///   全局外交好感、动荡（UnrestEngine.Incite）。金币全程守恒。
    /// - 存档：rb_ev_pending / rb_ev_cooldown / rb_ev_lastGlobal（由 NationSave 统一读写）。
    /// </summary>
    public static class DecisionEvents
    {
        public const string TypeDecision = "ev_decision"; // 抉择事件（AI 决策结果/玩家选择结果进事件流，史书级）

        // ===== 数据模型（events.json 直接反序列化目标）=====

        private class EventsFile
        {
            public List<EventDef> events;
        }

        public class EventDef
        {
            public string id;
            public string family;        // finance|disaster|court|military|civil|diplomacy
            public int minYear;          // 最早可触发年（0 = 无限制）
            public int timeoutYears;     // 挂起超时（年，1~3）
            public int fallback;         // 超时执行的选项序号
            public int cooldownYears;    // 同事件再触发冷却
            public float treasuryRatioMax = -1f; // 条件：金库/GDP ≤ 该值（-1 = 不检查；仅玩家国可判）
            public float giniMin = -1f;          // 条件：基尼 ≥
            public float giniMax = -1f;          // 条件：基尼 ≤
            public int atWar = -1;               // 条件：1=仅交战国 0=仅和平国 -1=不限
            public bool onlyPlayer;              // 条件：仅玩家认领国可触发（宫廷/权谋类）
            public int bankRiskMin = -1;         // 条件：挤兑风险档 ≥（0安全/1警戒/2危险；仅玩家国有意义）
            public int minPop = -1;              // 条件：人口 ≥
            public int maxPop = -1;              // 条件：人口 ≤
            public int phase = -1;               // 条件：经济阶段 0繁荣 1衰退 2萧条 3复苏（-1 不限）
            public float treasuryRatioMin = -1f; // 条件：金库/GDP ≥（仅玩家国可判）
            public string variantGroup;           // 变体互斥组：同组事件每局仅部分启用（种子决定，同局稳定）
            public string chainNext;             // 连锁：结算后触发的后续事件 id（null=无）
            public int chainDelay = 1;           // 连锁：后续事件延迟年数
            public int chainAfterOption = -1;    // 连锁：仅该选项序号触发（-1=任意选项）
            public List<EventOption> options;
        }

        public class EventOption
        {
            public string key;                   // 本地化键后缀（ev_<id>_<key>）
            public float treasuryGdpRatio;       // 金库变动（GDP 比例，负=支出）
            public float residentsTaxRatio;      // 向居民征税（财富比例 → 金库）
            public float poorReliefRatio;        // 金库 → 贫民分发（占金库比例）
            public int goodwillAll;              // 对所有其他王国外交好感增量
            public bool unrest;                  // 触发动荡（UnrestEngine.Incite）
            public int commercePenaltyYears;     // 商路断绝：商业税减半持续年数（0=无）
            public Dictionary<string, float> styleWeights; // AI 国性 → 权重（缺省 1）
        }

        /// <summary>挂起中的玩家国抉择（跨年等待玩家选择）。</summary>
        public class PendingEvent
        {
            public EventDef Def;
            public long KingdomId;
            public string KingdomName;
            public int ElapsedYears;
        }

        // ===== 运行时状态 =====

        private static List<EventDef> _defs = new List<EventDef>(16);
        private static bool _loadWarned;
        private static readonly List<PendingEvent> _pending = new List<PendingEvent>(8);
        private const int MaxPending = 8;
        private static readonly Dictionary<string, int> _readyYear = new Dictionary<string, int>(16);
        private static int _lastGlobalYear = int.MinValue;
        private static bool _popupQueued;

        // ===== 每局事件池（v1.7.0 种子化）=====
        // 每局按世界种子（MapBox.current_world_seed_id）确定性筛池，同 seed 同池：
        // 链/变体组为整体启用，单事件保留率 68%，每族保底，onlyPlayer 保底。
        // 读档后世界种子不变 → 池不变，存档兼容零改动。
        private static readonly HashSet<string> _activeIds = new HashSet<string>(64);
        private static bool _poolBuilt;
        private static int _worldSeed = 1;                          // 失败降级 1（确定性优先）
        private static readonly Dictionary<string, float> _familyBias = new Dictionary<string, float>(8);
        private const float PoolRetainRatio = 0.68f;                // 单事件保留率
        private const float FamilyBiasMin = 0.65f, FamilyBiasMax = 1.4f;

        /// <summary>事件是否在本局事件池中（EvaluateYear 候选过滤用）。</summary>
        private static bool PoolActive(EventDef d)
        {
            return _activeIds.Contains(d.id);
        }

        /// <summary>族倾向权重（每局固定；池未建时回退 1）。</summary>
        private static float FamilyWeight(string family)
        {
            if (!_poolBuilt) return 1f;
            string fam = string.IsNullOrEmpty(family) ? "civil" : family;
            return _familyBias.TryGetValue(fam, out float w) ? w : 1f;
        }

        // 加权抽样候选复用缓冲（EvaluateYear 主线程年度路径，不跨周期持有）
        private static readonly List<EventDef> _candidatePool = new List<EventDef>(32);

        /// <summary>连锁队列：到年限期直接生成（绕过概率/冷却），跨存档持久化。</summary>
        private class ChainSpawn
        {
            public string DefId;
            public long KingdomId;
            public int DueYear;
        }
        private static readonly List<ChainSpawn> _chains = new List<ChainSpawn>(8);

        /// <summary>挂起事件数（内阁待办区显示）。</summary>
        public static int PendingCount => _pending.Count;

        /// <summary>最早的挂起事件（抉择小窗/待办区消费；无则 null）。</summary>
        public static PendingEvent FirstPending => _pending.Count > 0 ? _pending[0] : null;

        /// <summary>挂起事件只读遍历（UI 待办列表；调用方不可修改）。</summary>
        public static IReadOnlyList<PendingEvent> Pending => _pending;

        /// <summary>弹窗排队标记（Events 阶段置位，快照尾消费——不在管线中途造 UI）。</summary>
        public static bool PopupQueued => _popupQueued;
        public static void ClearPopupQueued() { _popupQueued = false; }

        /// <summary>解析 events.json 候选路径：模组根（FolderPath）优先，其次 Locales 目录本身与其上级。</summary>
        private static string ResolveEventsPath()
        {
            var candidates = new List<string>(3);
            try
            {
                var main = EconomyModMain.Instance;
                var decl = main?.GetDeclaration();
                string folder = decl != null ? decl.FolderPath : null;
                if (!string.IsNullOrEmpty(folder))
                    candidates.Add(System.IO.Path.Combine(folder, "events.json"));
                string locDir = main != null && decl != null ? main.GetLocaleFilesDirectory(decl) : null;
                if (!string.IsNullOrEmpty(locDir))
                {
                    candidates.Add(System.IO.Path.Combine(locDir, "events.json"));
                    candidates.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(locDir), "events.json"));
                }
            }
            catch (System.Exception) { }
            for (int i = 0; i < candidates.Count; i++)
                if (System.IO.File.Exists(candidates[i])) return candidates[i];
            return null;
        }

        /// <summary>模组加载时解析 events.json（一次）；失败 fail-open。</summary>
        public static void Load()
        {
            if (_defs.Count > 0 || _loadWarned) return;
            try
            {
                string path = ResolveEventsPath();
                if (path == null)
                {
                    WarnOnce("events.json 未找到（已尝试 模组根/Locales/上级），抉择事件系统以空池运行");
                    return;
                }
                Debug.Log("[ClassicalEconomics] 抉择事件：events.json 路径=" + path);
                var file = JsonConvert.DeserializeObject<EventsFile>(System.IO.File.ReadAllText(path));
                if (file == null || file.events == null)
                {
                    WarnOnce("events.json 解析结果为空，抉择事件系统以空池运行");
                    return;
                }
                foreach (var d in file.events)
                {
                    if (d == null || string.IsNullOrEmpty(d.id) || d.options == null || d.options.Count < 2) continue;
                    if (string.IsNullOrEmpty(d.family)) d.family = "civil";
                    if (d.timeoutYears < 1) d.timeoutYears = 1;
                    if (d.timeoutYears > 3) d.timeoutYears = 3;
                    if (d.fallback < 0 || d.fallback >= d.options.Count) d.fallback = 0;
                    if (d.cooldownYears < 0) d.cooldownYears = 0;
                    _defs.Add(d);
                }
                Debug.Log($"[ClassicalEconomics] 抉择事件系统已加载 {_defs.Count} 个事件");
            }
            catch (System.Exception e)
            {
                _defs.Clear();
                WarnOnce("events.json 加载失败（" + e.Message + "），抉择事件系统以空池运行");
            }
        }

        private static void WarnOnce(string msg)
        {
            if (_loadWarned) return;
            _loadWarned = true;
            Debug.LogWarning("[ClassicalEconomics] " + msg);
        }

        /// <summary>新地图/新游戏：清空运行时状态（事件定义保留，事件池待下届世界重建）。</summary>
        public static void Reset()
        {
            _pending.Clear();
            _readyYear.Clear();
            _chains.Clear();
            _lastGlobalYear = int.MinValue;
            _popupQueued = false;
            _activeIds.Clear();
            _familyBias.Clear();
            _poolBuilt = false;
        }

        // ===== 每局事件池构建（种子确定性；首次 EvaluateYear 惰性执行）=====

        /// <summary>读世界种子：MapBox.current_world_seed_id（静态 int，新地图递增，读档同局不变）。</summary>
        private static int ReadWorldSeed()
        {
            try
            {
                var t = typeof(MapBox);
                var f = t.GetField("current_world_seed_id",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.NonPublic);
                if (f != null)
                {
                    int s = System.Convert.ToInt32(f.GetValue(null));
                    if (s != 0) return s; // 0=世界未就绪，保持降级
                }
            }
            catch (System.Exception) { }
            return 1;
        }

        /// <summary>FNV-1a 64→32 位字符串哈希（与种子混合，确定性，仅主线程构建时用）。</summary>
        private static uint HashMix(string s, int seed)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)seed) * 16777619u;
                for (int i = 0; i < s.Length; i++)
                {
                    h = (h ^ (uint)s[i]) * 16777619u;
                }
                return h;
            }
        }

        /// <summary>
        /// 惰性构建：首次 EvaluateYear 时执行一次（此时 World 已就绪、种子可读）。
        /// 规则：链整链启用/禁用；变体组整组启用、组内种子序取 ceil(n×0.6) 个；
        /// 单事件按保留率；每族保底 max(5, 族数×0.45)；onlyPlayer 保底 6。
        /// </summary>
        private static void BuildWorldPool()
        {
            if (_poolBuilt || _defs.Count == 0) return;
            _poolBuilt = true;
            _worldSeed = ReadWorldSeed();
            _activeIds.Clear();
            _familyBias.Clear();

            for (int i = 0; i < _defs.Count; i++)
            {
                var d = _defs[i];
                if (!string.IsNullOrEmpty(d.family) && !_familyBias.ContainsKey(d.family))
                {
                    // 族倾向：0.65~1.4 均匀映射，营造"此局天灾频仍、彼局宫廷喧哗"的氛围差
                    float t = HashMix("bias:" + d.family, _worldSeed) / 4294967295f;
                    _familyBias[d.family] = FamilyBiasMin + t * (FamilyBiasMax - FamilyBiasMin);
                }
            }

            // 单元化：链以"链尾"为单元 key（沿 chainNext 走到尽头），整链同生共死，
            // 防止链头启用而链尾被单抽漏掉（断链）；变体组以组为单元；其余单事件各自抽签。
            var chainIds = new HashSet<string>(32);          // 所有链上成员 id（含头/中/尾）
            foreach (var d in _defs)
            {
                if (string.IsNullOrEmpty(d.chainNext)) continue;
                chainIds.Add(d.id);
                chainIds.Add(d.chainNext);
            }

            var groups = new Dictionary<string, List<EventDef>>(8);
            var singles = new List<EventDef>(_defs.Count);
            foreach (var d in _defs)
            {
                if (string.IsNullOrEmpty(d.id)) continue;
                if (chainIds.Contains(d.id)) continue; // 链成员归链单元
                if (!string.IsNullOrEmpty(d.variantGroup))
                {
                    if (!groups.TryGetValue(d.variantGroup, out var g))
                    {
                        g = new List<EventDef>(4);
                        groups[d.variantGroup] = g;
                    }
                    g.Add(d);
                }
                else singles.Add(d);
            }

            // 1) 变体组：整组启用，组内按种子序取 ceil(n×0.6) 个（互斥：同组只出部分版本）
            foreach (var g in groups.Values)
            {
                g.Sort((a, b) => HashMix(a.id, _worldSeed + 7).CompareTo(HashMix(b.id, _worldSeed + 7)));
                int keep = System.Math.Max(1, (g.Count * 3 + 4) / 5);
                for (int i = 0; i < keep && i < g.Count; i++)
                    _activeIds.Add(g[i].id);
            }

            // 2) 链单元：以链尾为 key 归一化成员，链尾哈希抽签决定整链启用
            var chainTailOf = new Dictionary<string, string>(16); // 成员 id → 链尾 id
            foreach (var id in chainIds)
            {
                string cur = id;
                int guard = 0;
                while (guard++ < 16)
                {
                    var curDef = FindDef(cur);
                    if (curDef == null || string.IsNullOrEmpty(curDef.chainNext)) break;
                    cur = curDef.chainNext;
                }
                chainTailOf[id] = cur;
            }
            var tailActive = new HashSet<string>(8);
            foreach (var kv in chainTailOf)
            {
                if (tailActive.Contains(kv.Value)) continue;
                uint h = HashMix("chain:" + kv.Value, _worldSeed + 13);
                bool on = h / 4294967295f < PoolRetainRatio;
                if (on) tailActive.Add(kv.Value);
            }
            foreach (var kv in chainTailOf)
                if (tailActive.Contains(kv.Value)) _activeIds.Add(kv.Key);

            // 3) 单事件（不在链、不在组）：保留率
            for (int i = 0; i < singles.Count; i++)
            {
                var d = singles[i];
                uint h = HashMix(d.id, _worldSeed + 29);
                bool on = h / 4294967295f < PoolRetainRatio;
                if (on) _activeIds.Add(d.id);
            }

            // 5) 保底：每族至少 max(5, 族数×0.45)，onlyPlayer 至少 6（种子序补首）
            var familyCount = new Dictionary<string, int>(8);
            var familyActiveCount = new Dictionary<string, int>(8);
            int onlyPlayerTotal = 0, onlyPlayerActive = 0;
            for (int i = 0; i < _defs.Count; i++)
            {
                var d = _defs[i];
                if (string.IsNullOrEmpty(d.id)) continue;
                string fam = string.IsNullOrEmpty(d.family) ? "civil" : d.family;
                familyCount.TryGetValue(fam, out int c); familyCount[fam] = c + 1;
                if (_activeIds.Contains(d.id)) { familyActiveCount.TryGetValue(fam, out int a); familyActiveCount[fam] = a + 1; }
                if (d.onlyPlayer) { onlyPlayerTotal++; if (_activeIds.Contains(d.id)) onlyPlayerActive++; }
            }
            foreach (var kv in familyCount)
            {
                int min = System.Math.Max(5, (int)(kv.Value * 0.45f));
                familyActiveCount.TryGetValue(kv.Key, out int act);
                if (act >= min) continue;
                // 按种子序补足
                var candidates = new List<EventDef>(8);
                for (int i = 0; i < _defs.Count; i++)
                {
                    var d = _defs[i];
                    string fam = string.IsNullOrEmpty(d.family) ? "civil" : d.family;
                    if (fam != kv.Key || _activeIds.Contains(d.id)) continue;
                    candidates.Add(d);
                }
                candidates.Sort((a, b) => HashMix(a.id, _worldSeed + 41).CompareTo(HashMix(b.id, _worldSeed + 41)));
                for (int i = 0; i < candidates.Count && act < min; i++)
                {
                    _activeIds.Add(candidates[i].id);
                    act++;
                }
            }
            if (onlyPlayerTotal > 0 && onlyPlayerActive < 6)
            {
                var candidates = new List<EventDef>(8);
                for (int i = 0; i < _defs.Count; i++)
                {
                    var d = _defs[i];
                    if (!d.onlyPlayer || _activeIds.Contains(d.id)) continue;
                    candidates.Add(d);
                }
                candidates.Sort((a, b) => HashMix(a.id, _worldSeed + 53).CompareTo(HashMix(b.id, _worldSeed + 53)));
                for (int i = 0; i < candidates.Count && onlyPlayerActive < 6; i++)
                {
                    _activeIds.Add(candidates[i].id);
                    onlyPlayerActive++;
                }
            }

            Debug.Log($"[ClassicalEconomics] 每局事件池已构建（seed={_worldSeed}）：" +
                      $"启用 {_activeIds.Count}/{_defs.Count}，族倾向 " + string.Join(",", _familyBias.Keys));
        }

        // ===== 条件评估（纯数据 + 王国 API，主线程年度调用）=====

        private static bool IsAtWar(Kingdom k)
        {
            if (k == null) return false;
            try
            {
                var kingdoms = World.world != null ? World.world.kingdoms : null;
                if (kingdoms == null) return false;
                foreach (var o in kingdoms)
                {
                    if (o == null || o == k) continue;
                    if (k.isEnemy(o)) return true;
                }
            }
            catch (System.Exception) { }
            return false;
        }

        private static bool ConditionsOk(EventDef d, Kingdom k, KingdomStats stats, int year, bool isPlayer)
        {
            if (d.minYear > 0 && year < d.minYear) return false;
            if (d.giniMin >= 0f && (stats == null || stats.GiniCoefficient < d.giniMin)) return false;
            if (d.giniMax >= 0f && (stats == null || stats.GiniCoefficient > d.giniMax)) return false;
            if (d.treasuryRatioMax >= 0f)
            {
                if (!isPlayer) return false; // AI 国无王室金库，金库类条件只对玩家国有意义
                float gdp = stats?.GDP ?? 0f;
                if (gdp <= 0f || (float)NationEngine.Treasury / gdp > d.treasuryRatioMax) return false;
            }
            if (d.treasuryRatioMin >= 0f)
            {
                if (!isPlayer) return false;
                float gdp = stats?.GDP ?? 0f;
                if (gdp <= 0f || (float)NationEngine.Treasury / gdp < d.treasuryRatioMin) return false;
            }
            if (d.minPop >= 0 && (stats == null || stats.Population < d.minPop)) return false;
            if (d.maxPop >= 0 && (stats != null && stats.Population > d.maxPop)) return false;
            if (d.phase >= 0 && (int)EconomyCycleModulator.CurrentPhase != d.phase) return false;
            if (d.bankRiskMin >= 0)
            {
                if (!isPlayer || BankEngine.RiskTier < d.bankRiskMin) return false;
            }
            if (d.atWar >= 0)
            {
                bool war = IsAtWar(k);
                if (d.atWar == 1 && !war) return false;
                if (d.atWar == 0 && war) return false;
            }
            return true;
        }

        private static int ReadyYearOf(EventDef d)
        {
            return _readyYear.TryGetValue(d.id, out int y) ? y : int.MinValue;
        }

        // ===== 年度评估（AnnualPipeline.Events 阶段调用，主线程）=====

        public static void EvaluateYear(int year)
        {
            if (_defs.Count == 0) return;
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled) return;

            // 0. 每局事件池惰性构建（世界种子此时已就绪；同 seed 跨读档稳定）
            BuildWorldPool();

            // 1. 到期结算：玩家国挂起事件超时 → fallback
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var p = _pending[i];
                p.ElapsedYears++;
                if (p.ElapsedYears < p.Def.timeoutYears) continue;
                _pending.RemoveAt(i);
                Execute(p.Def, GameHelpers.FindKingdom(p.KingdomId), p.Def.fallback, year, true);
            }

            // 1.5 连锁结算：到期的后续事件直接生成（绕过概率/冷却/条件——剧情既定）
            for (int i = _chains.Count - 1; i >= 0; i--)
            {
                var c = _chains[i];
                if (year < c.DueYear) continue;
                var def = FindDef(c.DefId);
                if (def == null) { _chains.RemoveAt(i); continue; }
                var chainKingdom = GameHelpers.FindKingdom(c.KingdomId);
                if (chainKingdom == null || chainKingdom.data == null) { _chains.RemoveAt(i); continue; }
                bool chainIsPlayer = c.KingdomId == NationEngine.NationKingdomId;
                if (chainIsPlayer && _pending.Count >= MaxPending) { c.DueYear = year + 1; continue; } // 满则顺延
                _chains.RemoveAt(i);
                SpawnFor(def, chainKingdom, chainIsPlayer, year, true);
            }

            // 2. 全局冷却只约束"玩家国弹窗事件"（AI 事件不弹窗，仅进事件流，另设年度上限）
            bool playerBlocked = _lastGlobalYear != int.MinValue
                && year - _lastGlobalYear < System.Math.Max(1, cfg.EventCooldownYears);

            var kingdomList = GameHelpers.KingdomSnapshot();
            if (kingdomList == null) return;
            long playerId = NationEngine.NationKingdomId;
            int aiSpawnedThisYear = 0;
            const int MaxAiEventsPerYear = 2; // 列国故事每年限量，防事件流刷屏

            // 3. 每国抽签：概率 → 候选（条件+冷却）→ 均匀取一
            for (int ki = 0; ki < kingdomList.Count; ki++)
            {
                var k = kingdomList[ki];
                if (k == null || k.data == null) continue;
                long kid = k.data.id;
                if (kid == 0) continue;
                bool isPlayer = kid == playerId;
                if (isPlayer && playerBlocked) continue;
                if (!isPlayer && aiSpawnedThisYear >= MaxAiEventsPerYear) continue;

                // 玩家国已有挂起未决 → 不再压入新事件
                if (isPlayer && _pending.Count > 0) continue;

                float chance = isPlayer ? cfg.EventChancePlayer : cfg.EventChanceAi;
                if (chance <= 0f) continue;
                if (Random.value > chance) continue;

                EconomyMod.Models.KingdomStats stats;
                EconomyEngine.KingdomStats.TryGetValue(kid, out stats);
                EventDef picked = null;
                float totalWeight = 0f;
                _candidatePool.Clear();
                for (int di = 0; di < _defs.Count; di++)
                {
                    var d = _defs[di];
                    if (!PoolActive(d)) continue; // 本局未启用（种子池）
                    if (d.onlyPlayer && !isPlayer) continue; // 宫廷/权谋剧情专属玩家国
                    if (year < ReadyYearOf(d)) continue;
                    if (!ConditionsOk(d, k, stats, year, isPlayer)) continue;
                    _candidatePool.Add(d);
                    totalWeight += FamilyWeight(d.family);
                }
                if (totalWeight > 0f)
                {
                    // 加权抽样（族倾向：此局天灾频仍、彼局宫廷喧哗）
                    float roll = Random.value * totalWeight;
                    for (int ci = 0; ci < _candidatePool.Count; ci++)
                    {
                        roll -= FamilyWeight(_candidatePool[ci].family);
                        if (roll <= 0f) { picked = _candidatePool[ci]; break; }
                    }
                    if (picked == null && _candidatePool.Count > 0) picked = _candidatePool[_candidatePool.Count - 1];
                }
                if (picked == null) continue;
                SpawnFor(picked, k, isPlayer, year, false);
                if (isPlayer) _lastGlobalYear = year; // 只有玩家事件推进全局冷却
                else aiSpawnedThisYear++;
                _readyYear[picked.id] = year + System.Math.Max(0, picked.cooldownYears);
            }
        }

        /// <summary>把事件送达目标国：玩家国入挂起池+弹窗排队，AI 国按国性立即决策。</summary>
        private static void SpawnFor(EventDef def, Kingdom k, bool isPlayer, int year, bool fromChain)
        {
            if (isPlayer)
            {
                if (_pending.Count >= MaxPending) return;
                _pending.Add(new PendingEvent
                {
                    Def = def,
                    KingdomId = k.data.id,
                    KingdomName = GameHelpers.SafeKingdomName(k),
                    ElapsedYears = 0
                });
                _popupQueued = true;
                GameHelpers.NotifyLocalized(fromChain ? "toast_event_chain" : "toast_event_pending");
            }
            else
            {
                int opt = AiChoose(def, k.data.id);
                Execute(def, k, opt, year, false);
            }
        }

        /// <summary>AI 决策：各选项按国性权重（缺省 1）加权随机。</summary>
        private static int AiChoose(EventDef d, long kingdomId)
        {
            var weights = new float[d.options.Count];
            float total = 0f;
            string style = LawEngine.StyleKeys[LawEngine.GetStyle(kingdomId)];
            for (int i = 0; i < d.options.Count; i++)
            {
                float w = 1f;
                var sw = d.options[i].styleWeights;
                if (sw != null && style != null && sw.TryGetValue(style, out float v) && v > 0f) w = v;
                w = System.Math.Max(0.05f, w + RulerEngine.OptionBias(kingdomId, d.options[i]));
                weights[i] = w;
                total += w;
            }
            float roll = Random.value * total;
            for (int i = 0; i < weights.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return i;
            }
            return d.options.Count - 1;
        }

        // ===== 玩家选择入口（抉择小窗调用；optIndex 已由 UI 保证合法/可负担）=====

        public static bool CanAfford(EventDef def, int optIndex)
        {
            if (def == null || optIndex < 0 || optIndex >= def.options.Count) return false;
            float cost = -def.options[optIndex].treasuryGdpRatio; // 负收益 = 支出
            if (cost <= 0f) return true;
            var stats = NationEngine.NationStats();
            float gdp = stats?.GDP ?? 0f;
            return NationEngine.Treasury >= (long)(gdp * cost);
        }

        public static void Choose(int optIndex)
        {
            var p = FirstPending;
            if (p == null || optIndex < 0 || optIndex >= p.Def.options.Count) return;
            if (!CanAfford(p.Def, optIndex)) { GameHelpers.NotifyLocalized("toast_nation_poor_treasury"); return; }
            _pending.RemoveAt(0);
            Execute(p.Def, GameHelpers.FindKingdom(p.KingdomId), optIndex, SafeYear(), true);
        }

        private static int SafeYear()
        {
            try { return EconomyModMain.GetCurrentGameYear(); }
            catch (System.Exception) { return 0; }
        }

        // ===== 后果执行（全部组合现有通道，金币守恒）=====

        private static void Execute(EventDef d, Kingdom k, int optIndex, int year, bool isPlayer)
        {
            if (d == null || optIndex < 0 || optIndex >= d.options.Count) return;
            if (k == null || k.data == null) return; // 王国已亡：事件自然落空（不退款——金库/条件本就属于已亡国）
            var o = d.options[optIndex];
            long goldMoved = 0;
            var stats = NationEngine.NationStats();
            float gdp = stats?.GDP ?? 0f;
            bool playerInvolved = isPlayer || k.data.id == NationEngine.NationKingdomId;

            // 1. 金库变动（GDP 比例；负 = 支出）
            if (o.treasuryGdpRatio != 0f && playerInvolved)
            {
                long amount = (long)(gdp * o.treasuryGdpRatio);
                if (amount > 0) { NationEngine.AddTreasury(amount); goldMoved = amount; }
                else if (amount < 0)
                {
                    long cost = -amount;
                    if (NationEngine.TrySpend(cost)) goldMoved = -cost;
                }
            }

            // 2. 居民征税（财富比例 → 金库；真实转移）
            if (o.residentsTaxRatio > 0f && playerInvolved)
            {
                long target = (long)((stats?.ActorCount ?? 0) * (stats?.AvgWealth ?? 0f) * o.residentsTaxRatio);
                long collected = NationEngine.CollectFromResidents(k, target);
                NationEngine.AddTreasury(collected);
                goldMoved += collected;
            }

            // 3. 济贫分发（金库 → 贫民；真实转移）
            if (o.poorReliefRatio > 0f && playerInvolved)
            {
                long fund = (long)(NationEngine.Treasury * o.poorReliefRatio);
                if (fund > 0 && NationEngine.TrySpend(fund))
                    NationEngine.DistributeToPoor(k, fund, stats);
            }

            // 4. 全局外交好感
            if (o.goodwillAll != 0)
                NationDiplomacy.AddGoodwillAll(o.goodwillAll);

            // 5. 动荡
            if (o.unrest)
            {
                try { UnrestEngine.Incite(k); } catch (System.Exception) { }
            }

            _readyYear[d.id] = year + System.Math.Max(0, d.cooldownYears);

            // 5.4 商路断绝：商业税减半至指定年（BankEngine 消费）
            if (o.commercePenaltyYears > 0)
                BankEngine.SetCommercePenalty(year + o.commercePenaltyYears);

            // 5.5 连锁：按选项把后续事件排入队列（跨年生成，绕过概率/冷却）
            if (!string.IsNullOrEmpty(d.chainNext) && FindDef(d.chainNext) != null
                && (d.chainAfterOption < 0 || d.chainAfterOption == optIndex)
                && k.data != null)
            {
                _chains.Add(new ChainSpawn
                {
                    DefId = d.chainNext,
                    KingdomId = k.data.id,
                    DueYear = year + System.Math.Max(1, d.chainDelay)
                });
            }

            // 6. 结果横幅（进史书级事件流，Detail=渲染后文本供事件窗展示）+ 玩家屏上通知。
            // Detail 先做名字代入：{king}/{kingdom} 在记录时即替换（AI 国无弹窗，渲染只能读 Detail）。
            string resKey = "ev_" + d.id + "_res" + (optIndex + 1);
            string resText = Contextualize(Services.LocalizationService.Get(resKey), k.data.id);
            EventStreamService.Record(TypeDecision, GameHelpers.SafeKingdomName(k), optIndex + 1, resText);
            if (isPlayer)
            {
                if (goldMoved != 0)
                    GameHelpers.Notify(string.Format(resText, NationEngine.FormatGold(System.Math.Abs(goldMoved))));
                else GameHelpers.Notify(resText);
            }
        }

        // ===== 存档（NationSave 调用；rb_ev_* 三键）=====

        /// <summary>序列化挂起/冷却/全局冷却（culture 不变式）。</summary>
        public static void Serialize(System.Action<string, string> write)
        {
            var sb = new System.Text.StringBuilder(128);
            for (int i = 0; i < _pending.Count; i++)
            {
                var p = _pending[i];
                sb.Append(p.Def.id).Append('|').Append(p.KingdomId).Append('|').Append(p.ElapsedYears).Append(';');
            }
            write("rb_ev_pending", sb.ToString());

            sb.Length = 0;
            foreach (var kv in _readyYear)
                sb.Append(kv.Key).Append(':').Append(kv.Value).Append(';');
            write("rb_ev_cooldown", sb.ToString());

            write("rb_ev_lastGlobal", _lastGlobalYear == int.MinValue
                ? ""
                : _lastGlobalYear.ToString(System.Globalization.CultureInfo.InvariantCulture));

            var csb = new System.Text.StringBuilder(64);
            for (int i = 0; i < _chains.Count; i++)
            {
                var c = _chains[i];
                csb.Append(c.DefId).Append('|').Append(c.KingdomId).Append('|').Append(c.DueYear).Append(';');
            }
            write("rb_ev_chains", csb.ToString());
        }

        /// <summary>读档恢复（缺失/解析失败回退本局记忆）。</summary>
        public static void Restore(string pending, string cooldown, string lastGlobal, string chains)
        {
            _pending.Clear();
            try
            {
                if (!string.IsNullOrEmpty(pending))
                {
                    foreach (var item in pending.Split(';'))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        var f = item.Split('|');
                        if (f.Length < 3) continue;
                        var def = FindDef(f[0]);
                        if (def == null) continue; // 事件已从 JSON 移除 → 丢弃
                        if (!long.TryParse(f[1], out long kid)) continue;
                        if (!int.TryParse(f[2], out int elapsed)) continue;
                        if (_pending.Count >= MaxPending) break;
                        var kingdom = GameHelpers.FindKingdom(kid);
                        _pending.Add(new PendingEvent
                        {
                            Def = def,
                            KingdomId = kid,
                            KingdomName = kingdom != null ? GameHelpers.SafeKingdomName(kingdom) : "",
                            ElapsedYears = System.Math.Max(0, elapsed)
                        });
                    }
                }
            }
            catch (System.Exception) { }

            _readyYear.Clear();
            try
            {
                if (!string.IsNullOrEmpty(cooldown))
                {
                    foreach (var item in cooldown.Split(';'))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        var f = item.Split(':');
                        if (f.Length < 2) continue;
                        if (FindDef(f[0]) == null) continue;
                        if (int.TryParse(f[1], out int y)) _readyYear[f[0]] = y;
                    }
                }
            }
            catch (System.Exception) { }

            _lastGlobalYear = int.MinValue;
            try
            {
                if (!string.IsNullOrEmpty(lastGlobal) && int.TryParse(lastGlobal, out int y)) _lastGlobalYear = y;
            }
            catch (System.Exception) { }

            _chains.Clear();
            try
            {
                if (!string.IsNullOrEmpty(chains))
                {
                    foreach (var item in chains.Split(';'))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        var f = item.Split('|');
                        if (f.Length < 3) continue;
                        if (FindDef(f[0]) == null) continue;
                        if (!long.TryParse(f[1], out long kid)) continue;
                        if (!int.TryParse(f[2], out int due)) continue;
                        _chains.Add(new ChainSpawn { DefId = f[0], KingdomId = kid, DueYear = due });
                    }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>名字代入：{king}=在位国王名（无王则国名），{kingdom}=国名。抉择弹窗渲染用。</summary>
        public static string Contextualize(string text, long kingdomId)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var k = GameHelpers.FindKingdom(kingdomId);
            if (k == null || k.data == null) return text;
            string kName = GameHelpers.SafeKingdomName(k);
            string king = null;
            try { if (k.king != null) king = GameHelpers.SafeName(k.king); } catch (System.Exception) { }
            if (string.IsNullOrEmpty(king)) king = kName;
            return text.Replace("{king}", king).Replace("{kingdom}", kName);
        }

        private static EventDef FindDef(string id)
        {
            for (int i = 0; i < _defs.Count; i++)
                if (_defs[i].id == id) return _defs[i];
            return null;
        }
    }
}
