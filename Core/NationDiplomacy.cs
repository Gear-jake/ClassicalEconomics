using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.Core
{
    /// <summary>
    /// 外交（经济+外交大臣）：以本国（NationEngine 认领国）名义对目标国家执行
    /// 宣战 / 求和 / 结盟 / 外交赠礼 / 双边经济协定。全部动作实时生效（不等待年度结算）。
    /// 直接编译期调用原版 DiplomacyManager / WarManager / AllianceManager（与 RulerBox 的
    /// DiplomacyActionsWindow 同源 API，均为公开成员），异常时 fail-closed 并提示，不再依赖反射探测。
    /// 赠礼产生本模组"外交好感"（原版无公开好感写入 API），与结盟门槛共同构成赠礼的实际意义。
    /// （v1.3.0：双边协定自"贸易协定"改义为"经济协定"——按对方 GDP 比例向本国金库纳贡，流量加成随贸易模拟移除。）
    /// </summary>
    public static class NationDiplomacy
    {
        // 双边经济协定：targetKingdomId → tier（少/中/大 = 0/1/2），上限 2 个
        internal static readonly Dictionary<long, int> _pacts = new Dictionary<long, int>();
        public const int MaxPacts = 2;
        public const float PactIncomeRatio = 0.003f; // 协定收入 = 对方 GDP×0.3%×(档+1)，从对方居民征收（守恒）
        public const float PactAnnualCostRatio = 0.003f; // 年费（维护成本）= 本国 GDP×0.3%×档

        // 外交好感（赠礼累计）：targetKingdomId → goodwill；结盟门槛 = 原版好感 + 本值 ≥ 0
        internal static readonly Dictionary<long, int> _goodwill = new Dictionary<long, int>();
        public const int GiftAmount = 500;          // 赠礼金额（金库金币）
        public const int GiftGoodwill = 25;         // 每次赠礼好感
        public const int GoodwillCap = 200;         // 好感上限

        // startWar 在编译期引用 DLL 中不存在（运行时有，与 RulerBox 运行时编译不同）：
        // 运行时反射定位一次并缓存；拿不到则宣战 fail-closed。
        private static System.Reflection.MethodInfo _startWarMethod;
        private static bool _startWarProbed;

        private static System.Reflection.MethodInfo ResolveStartWar()
        {
            if (_startWarProbed) return _startWarMethod;
            _startWarProbed = true;
            try
            {
                _startWarMethod = typeof(DiplomacyManager).GetMethod("startWar",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (_startWarMethod == null)
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 外交：DiplomacyManager.startWar 运行时未找到");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[ClassicalEconomics] 外交：startWar 定位失败 " + e.Message);
            }
            return _startWarMethod;
        }

        /// <summary>解析宣战战争资产：首选 whisper_of_war，缺失时尝试常见候选（游戏版本差异兜底）。</summary>
        private static WarTypeAsset ResolveWarAsset()
        {
            foreach (var id in new string[] { "whisper_of_war", "war", "rebellion", "invasion" })
            {
                try
                {
                    var a = AssetManager.war_types_library.get(id);
                    if (a != null) return a;
                }
                catch (System.Exception) { }
            }
            return null;
        }

        private static Kingdom Mine()
        {
            long id = NationEngine.NationKingdomId;
            return id != 0 ? GameHelpers.FindKingdom(id) : null;
        }

        // ===== 抉择事件效果：目标王国选择（按事件字段 mode 选）=====

        /// <summary>
        /// 按 mode 挑选另一王国作为宣战/结盟目标（不含 a 自身与已亡国）：
        /// 宣战 mode：-1=最强邻国(GDP最高非本国) 0=随机 1=最弱邻国(有文明人口) 2=当前交战国（无交战则回退最强邻国）。
        /// 结盟 mode：-1=关系最好 0=随机 1=国力最强。
        /// 选择失败（王国数不足等）返回 null，调用方静默跳过（事件效果落空不报错）。
        /// </summary>
        internal static Kingdom PickTarget(Kingdom a, int mode, bool forWar)
        {
            if (a == null) return null;
            try
            {
                var ks = GameHelpers.KingdomSnapshot();
                if (ks == null || ks.Count < 2) return null;

                // 交战国优先
                if (forWar && mode == 2)
                {
                    Kingdom atWar = null;
                    foreach (var w in GetActiveWars(a))
                    {
                        if (w.hasEnded()) continue;
                        foreach (var o in ks)
                        {
                            if (o == null || o == a || o.data == null) continue;
                            if (w.isAttacker(o) || w.isDefender(o)) { atWar = o; break; }
                        }
                        if (atWar != null) break;
                    }
                    if (atWar != null) return atWar;
                }

                var pool = new List<Kingdom>(System.Math.Min(ks.Count, 48));
                for (int i = 0; i < ks.Count; i++)
                {
                    var o = ks[i];
                    if (o == null || o == a || o.data == null) continue;
                    if (o.data.id == a.data.id) continue;
                    pool.Add(o);
                }
                if (pool.Count == 0) return null;

                if (mode == 0) return pool[UnityEngine.Random.Range(0, pool.Count)];

                // 排序选择：宣战 -1=GDP高 → 1=人类少（弱）优先；结盟 -1=好 -1/1 类似逻辑走位
                if (forWar)
                {
                    if (mode == -1)
                    {
                        Kingdom best = null; long bestGdp = -1;
                        for (int i = 0; i < pool.Count; i++)
                        {
                            EconomyEngine.KingdomStats.TryGetValue(pool[i].data.id, out var st);
                            long g = st?.GDP ?? 0;
                            if (g > bestGdp) { bestGdp = g; best = pool[i]; }
                        }
                        return best;
                    }
                    // mode == 1：最弱（GDP 最小但 actor 数 >0）
                    {
                        Kingdom weak = null; long weakGdp = long.MaxValue;
                        for (int i = 0; i < pool.Count; i++)
                        {
                            EconomyEngine.KingdomStats.TryGetValue(pool[i].data.id, out var st);
                            if (st == null || st.ActorCount <= 0) continue;
                            if (st.GDP < weakGdp) { weakGdp = st.GDP; weak = pool[i]; }
                        }
                        return weak ?? pool[UnityEngine.Random.Range(0, pool.Count)];
                    }
                }
                else
                {
                    if (mode == -1)
                    {
                        Kingdom best = null; int bestScore = int.MinValue;
                        for (int i = 0; i < pool.Count; i++)
                        {
                            int s = GetRelationScore(pool[i]) + GetGoodwill(pool[i].data.id);
                            if (s > bestScore) { bestScore = s; best = pool[i]; }
                        }
                        return best ?? pool[0];
                    }
                    // mode == 1：GDP 最强
                    {
                        Kingdom strongest = null; long bestGdp = -1;
                        for (int i = 0; i < pool.Count; i++)
                        {
                            EconomyEngine.KingdomStats.TryGetValue(pool[i].data.id, out var st);
                            long g = st?.GDP ?? 0;
                            if (g > bestGdp) { bestGdp = g; strongest = pool[i]; }
                        }
                        return strongest ?? pool[0];
                    }
                }
            }
            catch (System.Exception) { return null; }
        }

        /// <summary>目标国与本国的原版好感（异常时为 0）。</summary>
        public static int GetRelationScore(Kingdom target)
        {
            var mine = Mine();
            if (mine == null || target == null || target.data == null) return 0;
            try
            {
                var relation = World.world.diplomacy.getRelation(mine, target);
                if (relation == null) return 0;
                var opinion = relation.getOpinion(target, mine);
                return opinion != null ? opinion.total : 0;
            }
            catch (System.Exception) { return 0; }
        }

        /// <summary>赠礼累计好感。</summary>
        public static int GetGoodwill(long kingdomId)
        {
            int g;
            return _goodwill.TryGetValue(kingdomId, out g) ? g : 0;
        }

        /// <summary>是否与目标国处于战争（isEnemy；异常时回退遍历战争列表）。</summary>
        public static bool IsAtWarWith(Kingdom target)
        {
            var mine = Mine();
            if (mine == null || target == null) return false;
            try
            {
                if (mine.isEnemy(target)) return true;
                foreach (var w in GetActiveWars(mine))
                {
                    if (!w.hasEnded() && (w.isAttacker(target) || w.isDefender(target))) return true;
                }
                return false;
            }
            catch (System.Exception) { return false; }
        }

        /// <summary>宣战：解散共同联盟（背叛）+ startWar(whisper_of_war)。</summary>
        public static bool DeclareWar(Kingdom target, out string msgKey)
        {
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (UnrestConfig.Instance == null || !UnrestConfig.Instance.NationPlayEnabled) { msgKey = "toast_dip_no_nation"; return false; }
            return StartWarBetween(mine, target, out msgKey);
        }

        /// <summary>
        /// 通用宣战（抉择事件效果用）：任意王国 a 向 b 开战，逻辑与 DeclareWar 一致。
        /// 事件国为主语（AI 国事件 → AI 国宣战；玩家国事件 → 认领国视角）。
        /// </summary>
        internal static bool StartWarBetween(Kingdom a, Kingdom b, out string msgKey)
        {
            msgKey = "toast_dip_declare_ok";
            if (a == null || b == null || a.data == null || b.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            try
            {
                if (a.isEnemy(b)) { msgKey = "toast_dip_already_war"; return false; }
                foreach (var w in GetActiveWars(a))
                    if (!w.hasEnded() && (w.isAttacker(b) || w.isDefender(b))) { msgKey = "toast_dip_already_war"; return false; }

                // 共同联盟因背叛瓦解
                if (a.hasAlliance() && b.hasAlliance() && a.getAlliance() == b.getAlliance())
                {
                    try { World.world.alliances.dissolveAlliance(a.getAlliance()); } catch (System.Exception) { }
                }

                var warAsset = ResolveWarAsset();
                var startWar = ResolveStartWar();
                if (warAsset == null || startWar == null)
                {
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 外交：宣战不可用 warAsset=" + (warAsset != null) + " startWar=" + (startWar != null));
                    msgKey = "toast_dip_unavailable";
                    return false;
                }
                startWar.Invoke(World.world.diplomacy, new object[] { a, b, warAsset, true });
                EventStreamService.Record(EventStreamService.TypeNationDiplomacy, b.data.name, 1);
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[ClassicalEconomics] 外交：宣战失败 " + e.Message);
                msgKey = "toast_dip_failed";
                return false;
            }
        }

        /// <summary>求和：我方军力 ≥ 对方 → 免费和谈；否则需按军力差支付赎金（金库）。</summary>
        public static bool SueForPeace(Kingdom target, out string msgKey)
        {
            msgKey = "toast_dip_peace_ok";
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (!IsAtWarWith(target)) { msgKey = "toast_dip_not_war"; return false; }

            try
            {
                var wars = World.world.wars.getWars(mine);
                War activeWar = null;
                if (wars != null)
                {
                    foreach (var w in wars)
                    {
                        if (!w.hasEnded() && (w.isAttacker(target) || w.isDefender(target))) { activeWar = w; break; }
                    }
                }
                if (activeWar == null) { msgKey = "toast_dip_failed"; return false; }

                int myPower = mine.countTotalWarriors();
                int theirPower = target.countTotalWarriors();
                if (myPower >= theirPower)
                {
                    World.world.wars.endWar(activeWar, WarWinner.Peace);
                }
                else
                {
                    long ransom = System.Math.Min(5000L, (theirPower - myPower) * 5L);
                    if (ransom <= 0) ransom = 1;
                    if (!NationEngine.TrySpend(ransom)) { msgKey = "toast_dip_peace_poor"; return false; }
                    World.world.wars.endWar(activeWar, WarWinner.Peace);
                }
                EventStreamService.Record(EventStreamService.TypeNationDiplomacy, target.data.name, 2);
                return true;
            }
            catch (System.Exception) { msgKey = "toast_dip_failed"; return false; }
        }

        /// <summary>结盟：无战争 + (原版好感 + 赠礼好感) ≥ 0；双方均无联盟 → 新建，单方有 → 加入。</summary>
        public static bool FormAlliance(Kingdom target, out string msgKey)
        {
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            return FormAllianceBetween(mine, target, out msgKey);
        }

        /// <summary>通用结盟（抉择事件效果用）：a 与 b 结盟，逻辑与 FormAlliance 一致。</summary>
        internal static bool FormAllianceBetween(Kingdom a, Kingdom b, out string msgKey)
        {
            msgKey = "toast_dip_alliance_ok";
            if (a == null || b == null || a.data == null || b.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (a.isEnemy(b)) { msgKey = "toast_dip_alliance_war"; return false; }
            if (a.hasAlliance() && b.hasAlliance() && a.getAlliance() == b.getAlliance()) { msgKey = "toast_dip_alliance_exists"; return false; }

            int score = GetRelationScore(b) + GetGoodwill(b.data.id);
            if (score < 0) { msgKey = "toast_dip_alliance_refused"; return false; }

            try
            {
                bool hasMine = a.hasAlliance();
                bool hasTheirs = b.hasAlliance();
                if (!hasMine && !hasTheirs)
                {
                    World.world.alliances.newAlliance(a, b);
                }
                else if (hasMine && !hasTheirs)
                {
                    a.getAlliance().join(b);
                }
                else if (!hasMine && hasTheirs)
                {
                    b.getAlliance().join(a);
                }
                else { msgKey = "toast_dip_alliance_both"; return false; }
                EventStreamService.Record(EventStreamService.TypeNationDiplomacy, b.data.name, 3);
                return true;
            }
            catch (System.Exception) { msgKey = "toast_dip_failed"; return false; }
        }

        /// <summary>外交赠礼：金库支付固定金额转给目标国国民（守恒）+ 累计好感。</summary>
        public static bool GiveGift(Kingdom target, out string msgKey)
        {
            msgKey = "toast_dip_gift_ok";
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (!NationEngine.TrySpend(GiftAmount)) { msgKey = "toast_nation_poor_treasury"; return false; }

            // 资金转给目标国国民（真实转移，守恒；无国民可领则退回金库）
            long given = GameHelpers.GiveToKingdomMembers(target, GiftAmount);
            if (given <= 0) { NationEngine.TrySpend(-GiftAmount); msgKey = "toast_dip_failed"; return false; }
            if (given < GiftAmount) NationEngine.TrySpend(-(GiftAmount - given)); // 未发出部分退回

            long tid = target.data.id;
            int g = GetGoodwill(tid) + GiftGoodwill;
            _goodwill[tid] = System.Math.Min(g, GoodwillCap);
            EventStreamService.Record(EventStreamService.TypeNationDiplomacy, target.data.name, 4);
            return true;
        }

        // ===== 双边经济协定 =====

        /// <summary>协定价档（-1 = 未签）。</summary>
        public static int PactTier(long kingdomId)
        {
            int t;
            return _pacts.TryGetValue(kingdomId, out t) ? t : -1;
        }


        /// <summary>签署/升档协定：无战争 + 好感 ≥ 0；槽位上限 2（同国升档不占新槽）。</summary>
        public static bool SignPact(Kingdom target, int tier, out string msgKey)
        {
            msgKey = "toast_dip_pact_ok";
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (tier < 0 || tier >= NationEngine.TierCount) { msgKey = "toast_dip_failed"; return false; }
            if (IsAtWarWith(target)) { msgKey = "toast_dip_pact_war"; return false; }
            if (GetRelationScore(target) + GetGoodwill(target.data.id) < 0) { msgKey = "toast_dip_alliance_refused"; return false; }

            long tid = target.data.id;
            int existing = PactTier(tid);
            if (!_pacts.ContainsKey(tid) && _pacts.Count >= MaxPacts) { msgKey = "toast_dip_pact_full"; return false; }
            if (existing == tier) { msgKey = "toast_dip_pact_same"; return false; }

            _pacts[tid] = tier;
            EventStreamService.Record(EventStreamService.TypeNationDiplomacy, target.data.name, 5);
            return true;
        }

        /// <summary>取消协定（免费）。</summary>


        /// <summary>对除本国外全部王国增减外交好感（抉择事件通道；clamp 到好感上限内）。</summary>
        public static void AddGoodwillAll(int delta)
        {
            if (delta == 0) return;
            var kingdoms = World.world != null ? World.world.kingdoms : null;
            if (kingdoms == null) return;
            long mine = NationEngine.NationKingdomId;
            foreach (var k in kingdoms)
            {
                if (k == null || k.data == null || k.data.id == 0 || k.data.id == mine) continue;
                int v = GetGoodwill(k.data.id) + delta;
                if (v > GoodwillCap) v = GoodwillCap;
                if (v < -GoodwillCap) v = -GoodwillCap;
                _goodwill[k.data.id] = v;
            }
        }

        /// <summary>年度管线：双边经济协定年费（金库 → 消耗）+ 协约国纳贡收入（对方居民 → 本国金库）。</summary>
        public static void RunAnnual(int year)
        {
            if (_pacts.Count == 0) return;
            var stats = NationEngine.NationStats();
            float gdp = stats != null ? stats.GDP : 0f;
            var expire = new List<long>();
            foreach (var kv in _pacts)
            {
                long fee = (long)(gdp * PactAnnualCostRatio * NationEngine.TierMult(kv.Value));
                if (fee > 0 && !NationEngine.TrySpend(fee)) { expire.Add(kv.Key); continue; } // 金库不足 → 协定自动解除

                // 纳贡收入：从协约国居民征收 对方GDP×0.3%×(档+1) 进本国金库（真实转移，守恒）
                var partner = GameHelpers.FindKingdom(kv.Key);
                if (partner == null || partner.units == null) continue;
                float partnerGdp = 0f;
                if (EconomyEngine.KingdomStats.TryGetValue(kv.Key, out var ps)) partnerGdp = ps.GDP;
                long target = (long)(partnerGdp * PactIncomeRatio * (kv.Value + 1));
                if (target <= 0) continue;
                long collected = GameHelpers.DeductCoins(partner.units, target);
                if (collected > 0) NationEngine.AddTreasury(collected);
            }
            for (int i = 0; i < expire.Count; i++) _pacts.Remove(expire[i]);
        }

        /// <summary>世界重置/换地图时清空。</summary>
        public static void Reset()
        {
            _pacts.Clear();
            _goodwill.Clear();
        }

        // ===== 辅助 =====

        private static List<War> GetActiveWars(Kingdom kingdom)
        {
            var result = new List<War>();
            try
            {
                var wars = World.world.wars.getWars(kingdom);
                if (wars == null) return result;
                foreach (var w in wars) result.Add(w);
            }
            catch (System.Exception) { }
            return result;
        }
    }
}
