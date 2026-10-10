using System.Globalization;

namespace EconomyMod.Core
{
    /// <summary>
    /// 【W3.2】存档键单一真源：本模组写进游戏存档的**全部键位**集中定义在此，
    /// 其它文件一律不得再写 `"rb_*"` 字面量（由 `tools/Test-SaveSchema.ps1` 强制）。
    ///
    /// 为什么必须收敛（改前实况）：
    ///   同一批键散落在 `NationSave` / `LawSave` / `DecisionEvents` / `BankEngine` / `WorldIdentity`
    ///   五个文件、共 41 处字面量，**写侧与读侧各写一遍**。任何一次手滑（少个下划线、大小写变化）
    ///   都不会报错，只会静默表现为"读档后某些数据没了"——最难查的一类 bug。
    ///
    /// 兼容性铁律（**改键值 = 玩家旧存档数据丢失**）：
    ///   1. 已发布键的**字符串值不可修改**，只能新增；
    ///   2. 废弃键保留常量（并在注释里标注废弃版本），不得删除；
    ///   3. 参数化键（下标/序号）必须走本类的构造函数，不要自己拼字符串；
    ///   4. 存档格式变更请递增 <see cref="Version"/> 并写进 `rb_schema_ver`，读档侧据此迁移。
    /// </summary>
    public static class SaveSchema
    {
        /// <summary>全部键的统一前缀（本模组的键都带它，便于在存档里辨认归属）。</summary>
        public const string Prefix = "rb_";

        /// <summary>存档结构版本：新增/改变键语义时递增。旧档缺该键 = 0（legacy）。</summary>
        public const int Version = 1;

        /// <summary>版本键（v3.0 新增；写入是纯追加，旧版本读到时忽略，不影响旧档）。</summary>
        public const string SchemaVersion = "rb_schema_ver";

        // ===== 世界身份（双通道之二：王国 data 键；另一通道是 MapStats.custom_data 的 ce_world_id）=====
        public const string WorldId = "rb_world_id";

        // ===== 历史 / 事件流（全局数据，挂认领国或第一个王国的 data）=====
        public const string History = "rb_hist";
        public const string EventStream = "rb_ev_stream";

        // ===== 认领国（中央银行家）=====
        public const string NationKingdom = "rb_nat_kingdom";
        public const string NationTreasury = "rb_nat_treasury";
        public const string NationSlotPrefix = "rb_nat_slot_";
        public const string NationBuildings = "rb_nat_buildings";
        public const string NationRecords = "rb_nat_records";
        public const string NationPacts = "rb_nat_pacts";
        public const string NationGoodwill = "rb_nat_goodwill";

        // ===== 抉择事件 =====
        public const string EventPending = "rb_ev_pending";
        public const string EventCooldown = "rb_ev_cooldown";
        public const string EventLastGlobal = "rb_ev_lastGlobal";
        public const string EventChains = "rb_ev_chains";

        // ===== 银行与商业 =====
        public const string BankSettings = "rb_bank_set";
        public const string BankLedger = "rb_bank_ledger";
        public const string BankAi = "rb_bank_ai";

        // ===== 法典 =====
        public const string LawLevelPrefix = "rb_law_law_";
        public const string LawPolicyPrefix = "rb_law_policy_";
        public const string LawStyle = "rb_law_style";

        // ===== 参数化键构造（禁止在调用点自己拼字符串）=====

        /// <summary>政策槽键（下标 0..MaxSlotKeys-1；历史格式：满槽位写空串覆盖旧值）。</summary>
        public static string NationSlot(int index)
        {
            return NationSlotPrefix + index.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>法典条目档位键（下标 = 法条序号）。</summary>
        public static string LawLevel(int index)
        {
            return LawLevelPrefix + index.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>国策条目档位键（下标 = 国策序号）。</summary>
        public static string LawPolicy(int index)
        {
            return LawPolicyPrefix + index.ToString(CultureInfo.InvariantCulture);
        }

        // ===== 键清单（供门禁/审计：新增键必须同时登记，否则门禁 RED）=====

        /// <summary>全部**固定键**。</summary>
        public static readonly string[] FixedKeys =
        {
            SchemaVersion,
            WorldId,
            History,
            EventStream,
            NationKingdom,
            NationTreasury,
            NationBuildings,
            NationRecords,
            NationPacts,
            NationGoodwill,
            EventPending,
            EventCooldown,
            EventLastGlobal,
            EventChains,
            BankSettings,
            BankLedger,
            BankAi,
            LawStyle,
        };

        /// <summary>全部**参数化键前缀**。</summary>
        public static readonly string[] KeyPrefixes =
        {
            NationSlotPrefix,
            LawLevelPrefix,
            LawPolicyPrefix,
        };
    }
}
