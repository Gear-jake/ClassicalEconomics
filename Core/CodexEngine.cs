using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.Core
{
    /// <summary>
    /// 法典引擎（v1.2 国法体系）：每个王国一套「法律」（常设制度，5 档）+「国策」（施政方针，3 档），
    /// 全王国（含 AI 国）年度自动演变；玩家认领国由玩家控制、AI 只给建议。
    /// 效果全部经 <see cref="LawMods"/> 统一聚合（每国一份），各引擎只读聚合乘数——
    /// 禁止逐条散落硬编码（防 dead modifier，Test-CodexLaws 门禁强制每档至少改变一个乘数）。
    /// 状态经王国 data 的 rb_* 键写入存档（MapBox save/load 反射挂接）；失败回退本局记忆。
    /// </summary>
    public static class CodexEngine
    {
        // ===== 法律/国策条目 key 常量（数据表定义，供索引与门禁）=====

        // 法律（24 条，5 档：0=无 1=轻 2=中 3=重 4=极）
        public const int LawTiers = 5;
        // 经济 5
        public const string LawTradeFreedom   = "law_trade_freedom";   // 贸易自由
        public const string LawPropertyRights = "law_property_rights"; // 财产权保护
        public const string LawTaxSystem      = "law_tax_system";      // 税收制度
        public const string LawAntimonopoly   = "law_antimonopoly";    // 反垄断
        public const string LawLandReform     = "law_land_reform";     // 土地改革
        // 社会 6
        public const string LawEducation    = "law_education";    // 义务教育
        public const string LawHealthcare   = "law_healthcare";   // 公共医疗
        public const string LawMigrant      = "law_migrant";      // 移民政策
        public const string LawReligion     = "law_religion";     // 宗教政策
        public const string LawPress        = "law_press";        // 新闻监管
        public const string LawGunControl   = "law_gun_control";  // 武器管制
        // 军事 4
        public const string LawConscription = "law_conscription"; // 征兵制度
        public const string LawStandingArmy = "law_standing_army"; // 常备军
        public const string LawMilitarism   = "law_militarism";    // 军国主义
        public const string LawPacifism     = "law_pacifism";      // 和平主义
        // 司法 4
        public const string LawJudicial    = "law_judicial";    // 司法独立
        public const string LawCapitalPun  = "law_capital_pun"; // 死刑
        public const string LawAntiCorrupt = "law_ant_corrupt"; // 反腐
        public const string LawPrison      = "law_prison";      // 监狱改造
        // 环境 3
        public const string LawForest     = "law_forest";     // 森林保护
        public const string LawAnimal     = "law_animal";     // 动物保护
        public const string LawPollution  = "law_pollution";  // 污染治理
        // 意识形态 6
        public const string LawMonarchy  = "law_monarchy";  // 君主制度
        public const string LawParliament = "law_parliament"; // 议会民主
        public const string LawPlannedEconomy = "law_planned_economy"; // 计划经济
        public const string LawFreeMarket      = "law_free_market";      // 自由市场
        public const string LawStateReligion  = "law_state_religion";    // 国教
        public const string LawSecularism     = "law_secularism";        // 政教分离
        public static readonly string[] LawKeys =
        {
            LawTradeFreedom, LawPropertyRights, LawTaxSystem, LawAntimonopoly, LawLandReform,
            LawEducation, LawHealthcare, LawMigrant, LawReligion, LawPress, LawGunControl,
            LawConscription, LawStandingArmy, LawMilitarism, LawPacifism,
            LawJudicial, LawCapitalPun, LawAntiCorrupt, LawPrison,
            LawForest, LawAnimal, LawPollution,
            LawMonarchy, LawParliament, LawPlannedEconomy, LawFreeMarket, LawStateReligion, LawSecularism,
        };

        // 互斥组（组内同时最多一项 > 0）
        public static readonly string[][] MutexGroups =
        {
            new[] { LawConscription, LawStandingArmy },
            new[] { LawPlannedEconomy, LawFreeMarket },
            new[] { LawStateReligion, LawSecularism },
            new[] { LawMilitarism, LawPacifism },
            new[] { LawMonarchy, LawParliament },
        };

        // 法律分类（UI 分组与 AI 维度）
        public static readonly (string Category, string[] Keys)[] LawCategories =
        {
            ("economy", new[] { LawTradeFreedom, LawPropertyRights, LawTaxSystem, LawAntimonopoly, LawLandReform }),
            ("social", new[] { LawEducation, LawHealthcare, LawMigrant, LawReligion, LawPress, LawGunControl }),
            ("military", new[] { LawConscription, LawStandingArmy, LawMilitarism, LawPacifism }),
            ("judicial", new[] { LawJudicial, LawCapitalPun, LawAntiCorrupt, LawPrison }),
            ("environment", new[] { LawForest, LawAnimal, LawPollution }),
            ("ideology", new[] { LawMonarchy, LawParliament, LawPlannedEconomy, LawFreeMarket, LawStateReligion, LawSecularism }),
        };

        // 国策（16 条，3 档：0=无 1=有 2=强化）
        public const int PolicyTiers = 3;
        // 财税 4
        public const string PolicyLowTax      = "policy_low_tax";      // 轻税
        public const string PolicyAusterity   = "policy_austerity";    // 紧缩
        public const string PolicySubsidy     = "policy_subsidy";      // 产业补贴
        public const string PolicyTradeDeal   = "policy_trade_deal";   // 贸易协定
        // 民生 4
        public const string PolicyPoorRelief  = "policy_poor_relief";  // 济贫
        public const string PolicyPublicWork  = "policy_public_work";  // 公共工程
        public const string PolicyFamily      = "policy_family";       // 鼓励生育
        public const string PolicyFestival    = "policy_festival";     // 宣发庆典
        // 军事 4
        public const string PolicyWarFund     = "policy_war_fund";     // 战争基金
        public const string PolicyRecruit     = "policy_recruit";      // 招兵
        public const string PolicyFortify     = "policy_fortify";      // 筑防
        public const string PolicyBorderGuard = "policy_border_guard"; // 边境巡防
        // 外交 4
        public const string PolicyDiplomacy   = "policy_diplomacy";    // 折冲樽俎
        public const string PolicyIsolation   = "policy_isolation";    // 闭关
        public const string PolicyExpansion   = "policy_expansion";    // 扩张
        public const string PolicyReparations = "policy_reparations";  // 赎金外交
        public static readonly string[] PolicyKeys =
        {
            PolicyLowTax, PolicyAusterity, PolicySubsidy, PolicyTradeDeal,
            PolicyPoorRelief, PolicyPublicWork, PolicyFamily, PolicyFestival,
            PolicyWarFund, PolicyRecruit, PolicyFortify, PolicyBorderGuard,
            PolicyDiplomacy, PolicyIsolation, PolicyExpansion, PolicyReparations,
        };

        // 国家个性风格（0-5）
        public const int StyleCount = 6;
        public static readonly string[] StyleKeys =
        {
            "style_bellicose",   // 尚武好战
            "style_merchant",    // 重商开放
            "style_welfare",     // 仁政福利
            "style_legalist",    // 法理严明
            "style_isolationist",// 闭关自守
            "style_tech",        // 科技兴邦
        };

        // ===== LawMods 聚合乘数（引擎唯一读取面）=====
        /// <summary>每国年度聚合效果。所有乘数以 1f 为中性；无新增乘数不放（宁可少而真）。</summary>
        public struct LawMods
        {
            public float Productivity; // 生产函数乘数
            public float TaxRate;      // 国民税负/收入乘数
            public float GiniShift;    // 基尼平移（+ = 拉大）
            public float UnrestAccum;  // 动荡积累速度乘数
            public float TradeFlow;    // 贸易边流量乘数
            public float Price;        // 本地物价乘数
            public float Consumer;     // 消费额乘数
            public float DisasterResist; // 灾害财富蒸发乘数（<1 更抗）
            public float BuildCost;    // 建筑费乘数
            public float Wage;         // 工资乘数
            public float Military;     // 军力档位（改变顺逆差阈值/加成）
            public float Happiness;    // 幸福（动荡平息速度乘数，1=默认）
            public float Birth;        // 人口增长乘数（施于经济产出的人口项）

            public static LawMods Neutral => new LawMods
            {
                Productivity = 1f, TaxRate = 1f, GiniShift = 0f, UnrestAccum = 1f,
                TradeFlow = 1f, Price = 1f, Consumer = 1f, DisasterResist = 1f,
                BuildCost = 1f, Wage = 1f, Military = 0f, Happiness = 1f, Birth = 1f
            };
        }

        // ===== 每国状态 =====
        public class NationState
        {
            public int[] LawLevels = new int[LawKeys.Length];
            public int[] PolicyLevels = new int[PolicyKeys.Length];
            public int Style;
            public int LastEvalYear = -9999;
            public long LastGdp = -1;
            public float LastGini = -1f;
            public int MutexCooldownYear = -9999; // AI 做互斥切换后的冷却
            public int MutexGroupId = -1;
            // 存档键注入
            public bool SaveFailedWarned;

            public LawMods Mods = LawMods.Neutral;
        }

        private static readonly Dictionary<long, NationState> _states = new Dictionary<long, NationState>();

        public static NationState Get(long kingdomId)
        {
            NationState st;
            if (!_states.TryGetValue(kingdomId, out st))
            {
                st = new NationState();
                _states[kingdomId] = st;
            }
            return st;
        }

        // ===== 读取 API（引擎/UI 用）=====

        /// <summary>某王国当前聚合乘数（未登记＝中性）。</summary>
        public static LawMods GetMods(long kingdomId)
        {
            NationState st;
            return _states.TryGetValue(kingdomId, out st) ? st.Mods : LawMods.Neutral;
        }

        public static int GetLawLevel(long kingdomId, string key)
        {
            int i = IndexOf(LawKeys, key);
            NationState st;
            if (i < 0 || !_states.TryGetValue(kingdomId, out st)) return 0;
            return st.LawLevels[i];
        }

        public static int GetPolicyLevel(long kingdomId, string key)
        {
            int i = IndexOf(PolicyKeys, key);
            NationState st;
            if (i < 0 || !_states.TryGetValue(kingdomId, out st)) return 0;
            return st.PolicyLevels[i];
        }

        public static int GetStyle(long kingdomId)
        {
            NationState st;
            return _states.TryGetValue(kingdomId, out st) ? st.Style : 0;
        }

        public static int LastEvalYear(long kingdomId)
        {
            NationState st;
            return _states.TryGetValue(kingdomId, out st) ? st.LastEvalYear : -9999;
        }

        /// <summary>重置某国（换地图/读档回退）。</summary>
        public static void ResetNation(long kingdomId)
        {
            _states.Remove(kingdomId);
        }

        /// <summary>世界重置/换地图全清。</summary>
        public static void ResetAll()
        {
            _states.Clear();
        }

        // ===== 档位/互斥变更（玩家与 AI 共用路径；升档付费降档免费）=====

        /// <summary>法律升档改革费（GDP×比例×新档）。</summary>
        public static float LawUpgradeCost(Kingdom kingdom, int newLevel)
        {
            float gdp = GdpOf(kingdom);
            return gdp * 0.005f * newLevel;
        }

        /// <summary>国策升档费用（GDP×比例×新档）。</summary>
        public static float PolicyUpgradeCost(Kingdom kingdom, int newLevel)
        {
            float gdp = GdpOf(kingdom);
            return gdp * 0.004f * newLevel;
        }

        private static float GdpOf(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.data == null) return 0f;
            EconomyMod.Models.KingdomStats ks;
            return EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out ks) ? ks.GDP : 0f;
        }

        // ===== 玩家变更 API（升档付费、降档免费；互斥组切换会清掉组内其他项）=====

        /// <summary>玩家为某国设置法律档位。升档从金库（玩家国）/仓库税（AI 国）付费；降档免费。</summary>
        public static bool SetLawLevel(Kingdom kingdom, string key, int level, out string noteKey)
        {
            noteKey = "toast_codex_law_ok";
            int i = IndexOf(LawKeys, key);
            if (kingdom == null || kingdom.data == null || i < 0) return false;
            if (level < 0 || level >= LawTiers) return false;
            bool playerOwns = NationEngine.NationKingdomId == kingdom.data.id;
            var st = Get(kingdom.data.id);
            int cur = st.LawLevels[i];
            if (cur == level) return false;

            // 互斥：升至 >0 时清掉同组其他项
            if (level > 0)
            {
                foreach (var group in MutexGroups)
                {
                    if (System.Array.IndexOf(group, key) < 0) continue;
                    foreach (var other in group)
                    {
                        if (other == key) continue;
                        int oi = IndexOf(LawKeys, other);
                        if (oi >= 0) st.LawLevels[oi] = 0;
                    }
                }
            }

            if (level > cur)
            {
                long cost = (long)LawUpgradeCost(kingdom, level);
                if (cost > 0)
                {
                    if (playerOwns)
                    {
                        if (!NationEngine.TrySpend(cost)) { noteKey = "toast_nation_poor_treasury"; return false; }
                    }
                    else
                    {
                        if (!CollectAIFunds(kingdom, cost)) { noteKey = "toast_codex_ai_poor"; return false; }
                    }
                }
            }
            st.LawLevels[i] = level;
            RecomputeMods(kingdom.data.id, st);
            EventStreamService.Record(EventStreamService.TypeNationDiplomacy, GameHelpers.SafeKingdomName(kingdom), 6);
            return true;
        }

        /// <summary>玩家为某国设置国策档位（规则同法律）。</summary>
        public static bool SetPolicyLevel(Kingdom kingdom, string key, int level, out string noteKey)
        {
            noteKey = "toast_codex_policy_ok";
            int i = IndexOf(PolicyKeys, key);
            if (kingdom == null || kingdom.data == null || i < 0) return false;
            if (level < 0 || level >= PolicyTiers) return false;
            bool playerOwns = NationEngine.NationKingdomId == kingdom.data.id;
            var st = Get(kingdom.data.id);
            int cur = st.PolicyLevels[i];
            if (cur == level) return false;
            if (level > cur)
            {
                long cost = (long)PolicyUpgradeCost(kingdom, level);
                if (cost > 0)
                {
                    if (playerOwns)
                    {
                        if (!NationEngine.TrySpend(cost)) { noteKey = "toast_nation_poor_treasury"; return false; }
                    }
                    else
                    {
                        if (!CollectAIFunds(kingdom, cost)) { noteKey = "toast_codex_ai_poor"; return false; }
                    }
                }
            }
            st.PolicyLevels[i] = level;
            RecomputeMods(kingdom.data.id, st);
            return true;
        }

        public static void SetStyle(Kingdom kingdom, int style)
        {
            if (kingdom == null || kingdom.data == null) return;
            Get(kingdom.data.id).Style = System.Math.Max(0, System.Math.Min(StyleCount - 1, style));
        }

        /// <summary>AI 国家变法费用：从城市仓库 + 居民征收（守恒）；不足返回 false。</summary>
        private static bool CollectAIFunds(Kingdom kingdom, long cost)
        {
            long got = 0;
            try
            {
                var cities = kingdom.getCities();
                if (cities != null)
                {
                    foreach (var c in cities)
                    {
                        if (c == null || got >= cost) continue;
                        int gold;
                        try { gold = c.getResourcesAmount("gold"); } catch (System.Exception) { gold = 0; }
                        if (gold <= 0) continue;
                        int take = System.Math.Min(gold, (int)System.Math.Min(cost - got, int.MaxValue));
                        try { c.takeResource("gold", take); got += take; } catch (System.Exception) { }
                    }
                }
            }
            catch (System.Exception) { }
            if (got < cost && kingdom.units != null)
            {
                var units = new List<Actor>();
                try
                {
                    foreach (var a in kingdom.units) if (a != null && a.isAlive()) units.Add(a);
                }
                catch (System.Exception) { }
                got += GameHelpers.DeductCoins(units, cost - got);
            }
            return got >= cost;
        }

        /// <summary>AI 年度决策建议（玩家法典页展示 + AI 演变共用）：返回建议档位（cur 不变返回 -1）。</summary>
        public static int SuggestLawLevel(Kingdom kingdom, string key, NationState st)
        {
            var ks = EconomyEngine.KingdomStats.TryGetValue(kingdom.data.id, out var s) ? s : null;
            int i = IndexOf(LawKeys, key);
            if (i < 0 || ks == null) return -1;
            int cur = st.LawLevels[i];
            int want = cur;
            float gini = ks.GiniCoefficient;
            float pop = ks.Population;
            float gdp = ks.GDP;
            bool atWar = NationDiplomacy.IsAtWarWith(kingdom) || st.Mods.Military > 0.3f;

            switch (LawKeys[i])
            {
                case LawConscription:
                case LawStandingArmy:
                    want = atWar ? 3 : (cur > 0 ? cur - 1 : 0);
                    break;
                case LawMilitarism:
                    want = atWar ? 3 : (cur > 1 ? 1 : 0);
                    break;
                case LawPacifism:
                    want = (gini > 0.7f || atWar) ? 0 : 2;
                    break;
                case LawEducation:
                case LawHealthcare:
                    want = (gdp < 0f || st.LastGdp > gdp && pop < ks.Population) ? 0 : (pop > 3000 ? 3 : 2);
                    break;
                case LawTaxSystem:
                    want = (gini > 0.6f && cur < 4) ? cur + 1 : (gini < 0.35f ? 0 : 2);
                    break;
                case LawTradeFreedom:
                case LawFreeMarket:
                    want = gini > 0.65f ? 0 : 3;
                    break;
                case LawPlannedEconomy:
                    want = gini > 0.7f ? 3 : 0;
                    break;
                case LawLandReform:
                case LawAntimonopoly:
                    want = gini > 0.65f ? 3 : (cur > 0 ? cur - 1 : 0);
                    break;
                case LawPropertyRights:
                    want = gini < 0.45f ? 3 : (cur > 0 ? cur - 1 : 0);
                    break;
                case LawMigrant:
                    want = pop > 8000 ? 0 : 2;
                    break;
                case LawPress:
                case LawJudicial:
                case LawAntiCorrupt:
                case LawPrison:
                case LawCapitalPun:
                    want = (gini > 0.6f || st.Mods.Happiness < 0.9f) ? 3 : (cur > 0 ? cur - 1 : 1);
                    break;
                default:
                    want = cur;
                    break;
            }
            return want != cur ? want : -1;
        }

        // ===== 年度评估（B3 填充 AI 逻辑；本文件先落骨架）=====

        public static void RunAnnual(int year)
        {
            try
            {
                if (World.world == null || World.world.kingdoms == null) return;
                foreach (var kingdom in GameHelpers.KingdomSnapshot())
                {
                    if (kingdom == null || kingdom.data == null) continue;
                    long kid = kingdom.data.id;
                    var st = Get(kid);
                    if (st.Style == 0 && year > 1) st.Style = RollStyle(kingdom);
                    RecomputeMods(kid, st);
                    st.LastEvalYear = year;
                    // B3 接入 AI 决策（玩家国豁免）
                    if (NationEngine.NationKingdomId != kid)
                        CodexAi.TickNation(kingdom, st, year);
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>按种族权重随机初始风格（B3 细化，先按索引哈希）。</summary>
        private static int RollStyle(Kingdom kingdom)
        {
            try
            {
                var race = kingdom.getActorAsset();
                if (race != null)
                {
                    // 种族 id 哈希映射 0-5（稳定）
                    int h = 0;
                    var id = race.id ?? "x";
                    for (int i = 0; i < id.Length; i++) h = (h * 31 + id[i]) & 0x7fffffff;
                    return h % StyleCount;
                }
            }
            catch (System.Exception) { }
            return (int)(kingdom.data.id % StyleCount);
        }

        public static void RecomputeMods(long kingdomId, NationState st)
        {
            var m = LawMods.Neutral;
            for (int i = 0; i < LawKeys.Length; i++)
                if (st.LawLevels[i] > 0) ApplyLawMod(i, st.LawLevels[i], ref m);
            for (int i = 0; i < PolicyKeys.Length; i++)
                if (st.PolicyLevels[i] > 0) ApplyPolicyMod(i, st.PolicyLevels[i], ref m);
            st.Mods = m;
        }

        // ===== 数据表：每条法律的档位乘数（0 档无效果；非零档必须影响至少一个乘数）=====
        // 每档以 Layer 叠加：Mult 直接相乘，Shift 累加。

        private static void ApplyLawMod(int index, int level, ref LawMods m)
        {
            // 简化数据驱动：每键档位参数表（档位 1..4 的乘数/平移）见下方静态定义
            switch (LawKeys[index])
            {
                case LawTradeFreedom:
                    // 自由贸易：贸易+、消费+、价格-；保护主义反效果在低档（用档位衰减表达）
                    m.TradeFlow *= 1f + 0.08f * level;
                    m.Price *= 1f - 0.02f * level;
                    m.Consumer *= 1f + 0.03f * level;
                    break;
                case LawPropertyRights:
                    m.Productivity *= 1f + 0.025f * level;
                    m.GiniShift += 0.008f * level;
                    m.BuildCost *= 1f - 0.02f * level;
                    break;
                case LawTaxSystem:
                    m.TaxRate *= 1f + 0.06f * level;
                    m.Consumer *= 1f - 0.02f * level;
                    m.UnrestAccum *= 1f + 0.01f * level;
                    break;
                case LawAntimonopoly:
                    m.GiniShift -= 0.006f * level;
                    m.Productivity *= 1f + 0.015f * level;
                    m.TaxRate *= 1f - 0.01f * level;
                    break;
                case LawLandReform:
                    m.GiniShift -= 0.01f * level;
                    m.Productivity *= 1f - 0.01f * level;
                    m.Consumer *= 1f + 0.02f * level;
                    break;
                case LawEducation:
                    m.Productivity *= 1f + 0.03f * level;
                    m.Wage *= 1f + 0.02f * level;
                    m.TaxRate *= 1f + 0.02f * level;
                    break;
                case LawHealthcare:
                    m.Birth *= 1f + 0.025f * level;
                    m.DisasterResist *= 1f - 0.02f * level;
                    m.TaxRate *= 1f + 0.02f * level;
                    break;
                case LawMigrant:
                    m.Birth *= 1f + 0.02f * level;
                    m.Productivity *= 1f + 0.01f * level;
                    m.UnrestAccum *= 1f + 0.03f * level;
                    break;
                case LawReligion:
                    m.Happiness *= 1f + 0.015f * level;
                    m.Wage *= 1f - 0.005f * level;
                    m.TaxRate *= 1f + 0.01f * level;
                    break;
                case LawPress:
                    m.UnrestAccum *= 1f - 0.02f * level;
                    m.Happiness *= 1f - 0.005f * level;
                    break;
                case LawGunControl:
                    m.UnrestAccum *= 1f - 0.015f * level;
                    m.Military -= 0.2f * level;
                    m.Happiness *= 1f - 0.005f * level;
                    break;
                case LawConscription:
                    m.Military += 0.25f * level;
                    m.Productivity *= 1f - 0.015f * level;
                    m.Wage *= 1f - 0.01f * level;
                    break;
                case LawStandingArmy:
                    m.Military += 0.22f * level;
                    m.TaxRate *= 1f + 0.03f * level;
                    m.Productivity *= 1f - 0.005f * level;
                    break;
                case LawMilitarism:
                    m.Military += 0.3f * level;
                    m.UnrestAccum *= 1f + 0.02f * level;
                    m.Happiness *= 1f - 0.01f * level;
                    break;
                case LawPacifism:
                    m.Military -= 0.35f * level;
                    m.Happiness *= 1f + 0.02f * level;
                    m.UnrestAccum *= 1f - 0.01f * level;
                    break;
                case LawJudicial:
                    m.UnrestAccum *= 1f - 0.015f * level;
                    m.Price *= 1f + 0.005f * level;
                    m.Happiness *= 1f + 0.005f * level;
                    break;
                case LawCapitalPun:
                    m.UnrestAccum *= 1f - 0.018f * level;
                    m.Happiness *= 1f - 0.008f * level;
                    break;
                case LawAntiCorrupt:
                    m.TaxRate *= 1f - 0.02f * level;
                    m.Productivity *= 1f + 0.015f * level;
                    m.Happiness *= 1f + 0.005f * level;
                    break;
                case LawPrison:
                    m.Happiness *= 1f - 0.006f * level;
                    m.UnrestAccum *= 1f - 0.012f * level;
                    m.TaxRate *= 1f + 0.01f * level;
                    break;
                case LawForest:
                    m.DisasterResist *= 1f - 0.015f * level;
                    m.Productivity *= 1f - 0.005f * level;
                    m.TaxRate *= 1f + 0.005f * level;
                    break;
                case LawAnimal:
                    m.Happiness *= 1f + 0.008f * level;
                    m.Productivity *= 1f - 0.004f * level;
                    break;
                case LawPollution:
                    m.Productivity *= 1f - 0.01f * level;
                    m.DisasterResist *= 1f - 0.01f * level;
                    m.TaxRate *= 1f + 0.01f * level;
                    break;
                case LawMonarchy:
                    m.Military += 0.1f * level;
                    m.TaxRate *= 1f + 0.02f * level;
                    m.Happiness += 0f;
                    m.Happiness *= 1f - 0.004f * level;
                    break;
                case LawParliament:
                    m.Happiness *= 1f + 0.012f * level;
                    m.UnrestAccum *= 1f - 0.01f * level;
                    m.TaxRate *= 1f + 0.015f * level;
                    break;
                case LawPlannedEconomy:
                    m.Productivity *= 1f + 0.01f * level;
                    m.TradeFlow *= 1f - 0.025f * level;
                    m.GiniShift -= 0.008f * level;
                    break;
                case LawFreeMarket:
                    m.TradeFlow *= 1f + 0.03f * level;
                    m.GiniShift += 0.01f * level;
                    m.Productivity *= 1f + 0.015f * level;
                    break;
                case LawStateReligion:
                    m.Happiness *= 1f + 0.008f * level;
                    m.UnrestAccum *= 1f - 0.008f * level;
                    m.Military += 0.05f * level;
                    break;
                case LawSecularism:
                    m.Productivity *= 1f + 0.01f * level;
                    m.Happiness *= 1f - 0.004f * level;
                    break;
            }
        }

        private static void ApplyPolicyMod(int index, int level, ref LawMods m)
        {
            switch (PolicyKeys[index])
            {
                case PolicyLowTax:
                    m.TaxRate *= 1f - 0.08f * level;
                    m.Consumer *= 1f + 0.04f * level;
                    break;
                case PolicyAusterity:
                    m.TaxRate *= 1f + 0.06f * level;
                    m.Consumer *= 1f - 0.04f * level;
                    m.Price *= 1f - 0.015f * level;
                    break;
                case PolicySubsidy:
                    m.Productivity *= 1f + 0.03f * level;
                    m.TaxRate *= 1f + 0.02f * level;
                    break;
                case PolicyTradeDeal:
                    m.TradeFlow *= 1f + 0.05f * level;
                    m.Price *= 1f - 0.01f * level;
                    break;
                case PolicyPoorRelief:
                    m.GiniShift -= 0.008f * level;
                    m.Happiness *= 1f + 0.01f * level;
                    m.TaxRate *= 1f + 0.01f * level;
                    break;
                case PolicyPublicWork:
                    m.Productivity *= 1f + 0.02f * level;
                    m.BuildCost *= 1f - 0.03f * level;
                    m.TaxRate *= 1f + 0.01f * level;
                    break;
                case PolicyFamily:
                    m.Birth *= 1f + 0.03f * level;
                    m.Consumer *= 1f - 0.015f * level;
                    m.TaxRate *= 1f + 0.01f * level;
                    break;
                case PolicyFestival:
                    m.Happiness *= 1f + 0.02f * level;
                    m.Consumer *= 1f + 0.03f * level;
                    break;
                case PolicyWarFund:
                    m.Military += 0.2f * level;
                    m.TaxRate *= 1f + 0.04f * level;
                    break;
                case PolicyRecruit:
                    m.Military += 0.15f * level;
                    m.Productivity *= 1f - 0.01f * level;
                    break;
                case PolicyFortify:
                    m.DisasterResist *= 1f - 0.02f * level;
                    m.BuildCost *= 1f + 0.05f * level;
                    break;
                case PolicyBorderGuard:
                    m.UnrestAccum *= 1f - 0.012f * level;
                    m.Military += 0.05f * level;
                    m.TradeFlow *= 1f - 0.01f * level;
                    break;
                case PolicyDiplomacy:
                    m.TradeFlow *= 1f + 0.03f * level;
                    m.Price *= 1f - 0.008f * level;
                    m.UnrestAccum *= 1f - 0.005f * level;
                    break;
                case PolicyIsolation:
                    m.TradeFlow *= 1f - 0.05f * level;
                    m.Price *= 1f + 0.02f * level;
                    m.UnrestAccum *= 1f - 0.015f * level;
                    break;
                case PolicyExpansion:
                    m.Military += 0.2f * level;
                    m.TaxRate *= 1f + 0.02f * level;
                    m.Happiness *= 1f - 0.006f * level;
                    break;
                case PolicyReparations:
                    m.TaxRate *= 1f + 0.03f * level;
                    m.Consumer *= 1f - 0.012f * level;
                    m.Happiness *= 1f - 0.004f * level;
                    break;
            }
        }

        private static int IndexOf(string[] arr, string key)
        {
            for (int i = 0; i < arr.Length; i++) if (arr[i] == key) return i;
            return -1;
        }
    }
}
