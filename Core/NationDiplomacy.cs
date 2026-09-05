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
            msgKey = "toast_dip_declare_ok";
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (UnrestConfig.Instance == null || !UnrestConfig.Instance.NationPlayEnabled) { msgKey = "toast_dip_no_nation"; return false; }
            if (IsAtWarWith(target)) { msgKey = "toast_dip_already_war"; return false; }

            try
            {
                // 共同联盟因背叛瓦解
                if (mine.hasAlliance() && target.hasAlliance() && mine.getAlliance() == target.getAlliance())
                {
                    try { World.world.alliances.dissolveAlliance(mine.getAlliance()); } catch (System.Exception) { }
                }

                var warAsset = ResolveWarAsset();
                var startWar = ResolveStartWar();
                if (warAsset == null || startWar == null)
                {
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 外交：宣战不可用 warAsset=" + (warAsset != null) + " startWar=" + (startWar != null));
                    msgKey = "toast_dip_unavailable";
                    return false;
                }
                startWar.Invoke(World.world.diplomacy, new object[] { mine, target, warAsset, true });
                EventStreamService.Record(EventStreamService.TypeNationDiplomacy, target.data.name, 1);
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
            msgKey = "toast_dip_alliance_ok";
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (IsAtWarWith(target)) { msgKey = "toast_dip_alliance_war"; return false; }
            if (mine.hasAlliance() && target.hasAlliance() && mine.getAlliance() == target.getAlliance()) { msgKey = "toast_dip_alliance_exists"; return false; }

            int score = GetRelationScore(target) + GetGoodwill(target.data.id);
            if (score < 0) { msgKey = "toast_dip_alliance_refused"; return false; }

            try
            {
                bool hasMine = mine.hasAlliance();
                bool hasTheirs = target.hasAlliance();
                if (!hasMine && !hasTheirs)
                {
                    World.world.alliances.newAlliance(mine, target);
                }
                else if (hasMine && !hasTheirs)
                {
                    mine.getAlliance().join(target);
                }
                else if (!hasMine && hasTheirs)
                {
                    target.getAlliance().join(mine);
                }
                else { msgKey = "toast_dip_alliance_both"; return false; }
                EventStreamService.Record(EventStreamService.TypeNationDiplomacy, target.data.name, 3);
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

        public static int PactCount => _pacts.Count;

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
