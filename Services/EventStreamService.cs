using System.Collections.Generic;

namespace EconomyMod.Services
{
    /// <summary>
    /// 经济事件流（Phase 6.5）：各引擎（动荡/社会危机）记录饥荒、掠夺、革命、动荡等事件，
    /// 供 HUD「事件」标签页按类型统计（类似主页人口/死亡数据行）+ 时间线展示。
    /// v0.8.3 双环改造：低频重大事件（革命/起义/泡沫破裂/灾害/银行危机/时代/崩溃/改革失败/王位/掠夺）
    /// 走独立「史书级」环形缓冲（MajorCapacity=100，防覆盖）；高频消费类事件
    /// （建造投资/军械/批发/动荡/政策等）走普通环形缓冲（Capacity=60）。
    /// 解决 v0.8.2 问题：SpendingEngine 每年几十条高频事件把革命这类低频大事件挤出窗口。
    /// 内存环形数组缓冲（固定 ≤ 容量），随游戏进程存在，不落盘；新地图/新游戏时清空。
    /// 算法优化：用环形数组替代 List.RemoveAt(0)，追加 O(1)（原 O(N) 搬移）。
    /// </summary>
    public static class EventStreamService
    {
        /// <summary>普通事件容量（高频消费类）。</summary>
        public const int Capacity = 60;

        /// <summary>重大事件容量（史书级，防覆盖）。</summary>
        public const int MajorCapacity = 100;

        // ===== 事件类型本地化键 =====
        public const string TypeUnrest     = "ev_unrest";      // 自动触发的社会动荡
        public const string TypeIncite     = "ev_incite";      // 手动煽动
        public const string TypeSuppress   = "ev_suppress";    // 手动镇压
        public const string TypePlunder    = "ev_plunder";     // 战争掠夺
        public const string TypeRevolution = "ev_revolution";  // 革命爆发
        public const string TypeUprising   = "ev_uprising";    // 街头起义（政权崩塌）
        public const string TypeBuildInv     = "ev_build_inv";      // 建造投资
        public const string TypeCraftArsenal = "ev_craft_arsenal";  // 打造军械
        public const string TypeWholesale    = "ev_wholesale";      // 武器批发
        public const string TypeEraGolden    = "ev_era_golden";     // 盛世
        public const string TypeEraRevival   = "ev_era_revival";    // 复兴
        public const string TypeEraFlourish  = "ev_era_flourish";   // 强盛期
        public const string TypeCollapse     = "ev_collapse";       // 经济崩溃
        public const string TypePolicy       = "ev_policy";         // 国家政策（降基尼）
        public const string TypeUnrestPeace  = "ev_unrest_peace";   // 暴动和谈
        public const string TypeUnrestResolved = "ev_unrest_resolved"; // 暴动平定（城市收回）
        public const string TypePolicyFail   = "ev_policy_fail";    // 改革失败（1=退位 2=驾崩）
        public const string TypeKingInherit  = "ev_king_inherit";   // 王位继承（新王即位）
        public const string TypeDisaster     = "ev_disaster";       // 灾害经济冲击（P2）
        public const string TypeBanking      = "ev_banking";        // 银行信贷违约/危机传染（P2）
        public const string TypeBubbleBurst  = "ev_bubble_burst";   // 经济泡沫破裂（P1）
        public const string TypeNationClaim    = "ev_nation_claim";    // 中央银行家：认领国家
        public const string TypeNationPolicy   = "ev_nation_policy";   // 中央银行家：持续政策启用/取消
        public const string TypeNationRelief   = "ev_nation_relief";   // 中央银行家：紧急救济
        public const string TypeNationFestival = "ev_nation_festival"; // 中央银行家：国庆庆典
        public const string TypeNationBuild    = "ev_nation_build";    // 中央银行家：兴建建筑
        public const string TypeNationDiplomacy = "ev_nation_diplomacy"; // 中央银行家：外交动作（1宣战2求和3结盟4赠礼5协定6解约）
        public const string TypeLawReform = "ev_law_reform"; // 法典：AI 国家变法（1一般 2重大互斥）

        /// <summary>
        /// 是否为重大事件（史书级）：低频高价值，走独立环形缓冲防被高频事件覆盖。
        /// </summary>
        public static bool IsMajorType(string typeKey)
        {
            switch (typeKey)
            {
                case TypeRevolution:
                case TypeUprising:
                case TypeBubbleBurst:
                case TypeDisaster:
                case TypeBanking:
                case TypeEraGolden:
                case TypeEraRevival:
                case TypeEraFlourish:
                case TypeCollapse:
                case TypePolicyFail:
                case TypeKingInherit:
                case TypePlunder:
                    return true;
                default:
                    return false;
            }
        }

        public class EventEntry
        {
            public int GameYear;
            public string TypeKey;
            public string KingdomName; // 可为空（如全球性饥荒）
            public long Value;
        }

        // 普通事件环形数组缓冲：_head 指向下一个写入位置，_count 为当前条数（≤Capacity）
        private static readonly EventEntry[] _events = new EventEntry[Capacity];
        private static int _head;
        private static int _count;

        // 重大事件环形数组缓冲（史书级，防覆盖）：_majorHead/_majorCount
        private static readonly EventEntry[] _majorEvents = new EventEntry[MajorCapacity];
        private static int _majorHead;
        private static int _majorCount;

        // 类型累计计数（历史总数，环形覆盖不减——保持原语义：UI 显示"累计发生 X 次"）
        private static readonly Dictionary<string, int> _typeCounts = new Dictionary<string, int>();

        // 复用的 GetMinorRecent / GetMajorRecent 输出缓冲（调用方立即消费，不跨周期持有）
        private static readonly List<EventEntry> _recentPool = new List<EventEntry>(Capacity);
        private static readonly List<EventEntry> _majorRecentPool = new List<EventEntry>(MajorCapacity);

        // 事件条目对象池：Clear() 后供下一世界复用；环满后直接原位改写槽位对象。
        private static readonly List<EventEntry> _entryPool = new List<EventEntry>(Capacity + MajorCapacity);

        /// <summary>事件流变更版本，供 UI 跳过无变化的重复重建。</summary>
        public static int Version { get; private set; }

        /// <summary>记录一条事件：按类型分流到重大/普通环形缓冲。</summary>
        public static void Record(string typeKey, string kingdomName, long value)
        {
            if (!IsKnownType(typeKey)) return;

            bool major = IsMajorType(typeKey);
            int slot = major ? _majorHead : _head;
            var entry = major ? _majorEvents[slot] : _events[slot];
            if (entry == null) entry = RentEntry();
            entry.GameYear = EconomyModMain.GetCurrentGameYear();
            entry.TypeKey = typeKey;
            entry.KingdomName = string.IsNullOrEmpty(kingdomName) ? "" : kingdomName;
            entry.Value = value;
            // 环形写入：满环直接复用被覆盖槽位的对象，稳定期零 EventEntry 分配。
            if (major)
            {
                _majorEvents[slot] = entry;
                _majorHead = (_majorHead + 1) % MajorCapacity;
                if (_majorCount < MajorCapacity) _majorCount++;
            }
            else
            {
                _events[slot] = entry;
                _head = (_head + 1) % Capacity;
                if (_count < Capacity) _count++;
            }

            _typeCounts.TryGetValue(typeKey, out int c);
            _typeCounts[typeKey] = c < int.MaxValue ? c + 1 : int.MaxValue;
            unchecked { Version++; }
        }

        private static bool IsKnownType(string typeKey)
        {
            switch (typeKey)
            {
                case TypeUnrest: case TypeIncite: case TypeSuppress: case TypePlunder:
                case TypeRevolution: case TypeUprising: case TypeBuildInv: case TypeCraftArsenal:
                case TypeWholesale: case TypeEraGolden: case TypeEraRevival: case TypeEraFlourish:
                case TypeCollapse: case TypePolicy: case TypeUnrestPeace: case TypeUnrestResolved:
                case TypePolicyFail: case TypeKingInherit: case TypeDisaster: case TypeBanking:
                case TypeBubbleBurst:
                case TypeNationClaim: case TypeNationPolicy: case TypeNationRelief:
                case TypeNationFestival: case TypeNationBuild: case TypeNationDiplomacy:
                case TypeLawReform:
                    return true;
                default:
                    return false;
            }
        }

        private static EventEntry RentEntry()
        {
            if (_entryPool.Count > 0)
            {
                var e = _entryPool[_entryPool.Count - 1];
                _entryPool.RemoveAt(_entryPool.Count - 1);
                return e;
            }
            return new EventEntry();
        }

        /// <summary>取最近 count 条普通事件（时间正序）。返回复用缓冲，调用方不可跨周期持有引用。</summary>
        public static List<EventEntry> GetMinorRecent(int count)
        {
            var result = _recentPool;
            result.Clear();
            int take = count > _count ? _count : count;
            if (take <= 0) return result;

            int start = (_head - take + Capacity) % Capacity;
            for (int i = 0; i < take; i++)
            {
                int idx = (start + i) % Capacity;
                result.Add(_events[idx]);
            }
            return result;
        }

        /// <summary>取最近 count 条重大事件（时间正序）。返回复用缓冲，调用方不可跨周期持有引用。</summary>
        public static List<EventEntry> GetMajorRecent(int count)
        {
            var result = _majorRecentPool;
            result.Clear();
            int take = count > _majorCount ? _majorCount : count;
            if (take <= 0) return result;

            int start = (_majorHead - take + MajorCapacity) % MajorCapacity;
            for (int i = 0; i < take; i++)
            {
                int idx = (start + i) % MajorCapacity;
                result.Add(_majorEvents[idx]);
            }
            return result;
        }

        /// <summary>某类型累计发生次数（历史总数，环形覆盖不减）。</summary>
        public static int GetTypeCount(string typeKey)
        {
            return _typeCounts.TryGetValue(typeKey, out int c) ? c : 0;
        }

        /// <summary>事件总次数（重大 + 普通）。</summary>
        public static int TotalCount
        {
            get
            {
                int t = 0;
                foreach (var v in _typeCounts.Values) t += v;
                return t;
            }
        }

        /// <summary>普通事件当前条数。</summary>
        public static int Count => _count;

        /// <summary>重大事件当前条数。</summary>
        public static int MajorCount => _majorCount;

        /// <summary>清空事件流（新地图/新游戏时调用）。</summary>
        public static void Clear()
        {
            // 回收普通缓冲条目
            int start = (_head - _count + Capacity) % Capacity;
            for (int i = 0; i < _count; i++)
            {
                int idx = (start + i) % Capacity;
                var e = _events[idx];
                _events[idx] = null;
                ReturnEntry(e);
            }
            _head = 0;
            _count = 0;

            // 回收重大缓冲条目
            int mstart = (_majorHead - _majorCount + MajorCapacity) % MajorCapacity;
            for (int i = 0; i < _majorCount; i++)
            {
                int idx = (mstart + i) % MajorCapacity;
                var e = _majorEvents[idx];
                _majorEvents[idx] = null;
                ReturnEntry(e);
            }
            _majorHead = 0;
            _majorCount = 0;

            _typeCounts.Clear();
            _recentPool.Clear();
            _majorRecentPool.Clear();
            unchecked { Version++; }
        }

        private static void ReturnEntry(EventEntry entry)
        {
            if (entry == null) return;
            entry.GameYear = 0;
            entry.TypeKey = null;
            entry.KingdomName = null;
            entry.Value = 0L;
            if (_entryPool.Count < Capacity + MajorCapacity) _entryPool.Add(entry);
        }
    }
}
