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

        /// <summary>新地图/新游戏：清空运行时状态（事件定义保留）。</summary>
        public static void Reset()
        {
            _pending.Clear();
            _readyYear.Clear();
            _lastGlobalYear = int.MinValue;
            _popupQueued = false;
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

            // 1. 到期结算：玩家国挂起事件超时 → fallback
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var p = _pending[i];
                p.ElapsedYears++;
                if (p.ElapsedYears < p.Def.timeoutYears) continue;
                _pending.RemoveAt(i);
                Execute(p.Def, GameHelpers.FindKingdom(p.KingdomId), p.Def.fallback, year, true);
            }

            // 2. 全局冷却：冷却期内不产生新事件（到期结算不受影响）
            if (_lastGlobalYear != int.MinValue && year - _lastGlobalYear < System.Math.Max(1, cfg.EventCooldownYears)) return;

            var kingdomList = GameHelpers.KingdomSnapshot();
            if (kingdomList == null) return;
            long playerId = NationEngine.NationKingdomId;

            // 3. 每国抽签：概率 → 候选（条件+冷却）→ 均匀取一
            for (int ki = 0; ki < kingdomList.Count; ki++)
            {
                var k = kingdomList[ki];
                if (k == null || k.data == null) continue;
                long kid = k.data.id;
                if (kid == 0) continue;
                bool isPlayer = kid == playerId;

                // 玩家国已有挂起未决 → 不再压入新事件
                if (isPlayer && _pending.Count > 0) continue;

                float chance = isPlayer ? cfg.EventChancePlayer : cfg.EventChanceAi;
                if (chance <= 0f) continue;
                if (Random.value > chance) continue;

                EconomyMod.Models.KingdomStats stats;
                EconomyEngine.KingdomStats.TryGetValue(kid, out stats);
                EventDef picked = null;
                int candidates = 0;
                for (int di = 0; di < _defs.Count; di++)
                {
                    var d = _defs[di];
                    if (year < ReadyYearOf(d)) continue;
                    if (!ConditionsOk(d, k, stats, year, isPlayer)) continue;
                    candidates++;
                    if (Random.Range(0, candidates) == 0) picked = d; // 蓄水池抽样（等权取一）
                }
                if (picked == null) continue;

                if (isPlayer)
                {
                    if (_pending.Count >= MaxPending) continue;
                    _pending.Add(new PendingEvent
                    {
                        Def = picked,
                        KingdomId = kid,
                        KingdomName = GameHelpers.SafeKingdomName(k),
                        ElapsedYears = 0
                    });
                    _popupQueued = true;
                    GameHelpers.NotifyLocalized("toast_event_pending");
                }
                else
                {
                    // AI 国：按国性加权立即决策（无弹窗，结果进事件流）
                    int opt = AiChoose(picked, kid);
                    Execute(picked, k, opt, year, false);
                }
                _lastGlobalYear = year;
                _readyYear[picked.id] = year + System.Math.Max(0, picked.cooldownYears);
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

            // 6. 结果横幅（进史书级事件流，Detail=结果键供事件窗渲染）+ 玩家屏上通知
            EventStreamService.Record(TypeDecision, GameHelpers.SafeKingdomName(k), optIndex + 1,
                "ev_" + d.id + "_res" + (optIndex + 1));
            if (isPlayer)
            {
                string key = "ev_" + d.id + "_res" + (optIndex + 1);
                if (goldMoved != 0) GameHelpers.NotifyLocalized(key, NationEngine.FormatGold(System.Math.Abs(goldMoved)));
                else GameHelpers.NotifyLocalized(key);
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
        }

        /// <summary>读档恢复（缺失/解析失败回退本局记忆）。</summary>
        public static void Restore(string pending, string cooldown, string lastGlobal)
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
        }

        private static EventDef FindDef(string id)
        {
            for (int i = 0; i < _defs.Count; i++)
                if (_defs[i].id == id) return _defs[i];
            return null;
        }
    }
}
