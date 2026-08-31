using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.Core
{
    /// <summary>
    /// 外交（经济+外交大臣 v0.98）：以本国（NationEngine 认领国）名义对目标国家执行
    /// 宣战 / 求和 / 结盟 / 外交赠礼 / 双边贸易协定。全部动作实时生效（不等待年度结算），
    /// 经原版 DiplomacyManager / WarManager / AllianceManager 反射调用（与 RulerBox 的
    /// DiplomacyActionsWindow 同源 API），任一 API 缺失/异常时 fail-closed 并提示。
    /// 赠礼产生本模组"外交好感"（原版无公开好感写入 API），与结盟门槛共同构成赠礼的实际意义。
    /// </summary>
    public static class NationDiplomacy
    {
        // 双边贸易协定：targetKingdomId → tier（少/中/大 = 0/1/2），上限 2 个
        private static readonly Dictionary<long, int> _pacts = new Dictionary<long, int>();
        public const int MaxPacts = 2;
        public const float PactFlowPerTier = 0.10f; // 每档 +10% 边流量
        public const float PactAnnualCostRatio = 0.003f; // 年费 = GDP×0.3%×档

        // 外交好感（赠礼累计）：targetKingdomId → goodwill；结盟门槛 = 原版好感 + 本值 ≥ 0
        private static readonly Dictionary<long, int> _goodwill = new Dictionary<long, int>();
        public const int GiftAmount = 500;          // 赠礼金额（金库金币）
        public const int GiftGoodwill = 25;         // 每次赠礼好感
        public const int GoodwillCap = 200;         // 好感上限

        // ===== 反射缓存（全部 fail-closed）=====
        private static System.Reflection.MethodInfo _startWarMethod;   // DiplomacyManager.startWar(Kingdom, Kingdom, WarTypeAsset, bool)
        private static System.Reflection.MethodInfo _endWarMethod;     // WarManager.endWar(War, WarWinner)
        private static System.Reflection.MethodInfo _getWarsMethod;    // WarManager.getWars(Kingdom)
        private static System.Reflection.MethodInfo _newAllianceMethod;  // AllianceManager.newAlliance(Kingdom, Kingdom)
        private static System.Reflection.MethodInfo _joinAllianceMethod; // Alliance.join(Kingdom)
        private static System.Reflection.MethodInfo _dissolveAllianceMethod; // AllianceManager.dissolveAlliance(Alliance)
        private static System.Reflection.MethodInfo _getRelationMethod; // DiplomacyManager.getRelation(Kingdom, Kingdom)
        private static System.Reflection.MethodInfo _getOpinionMethod;  // DiplomacyRelation.getOpinion(Kingdom, Kingdom)
        private static System.Reflection.MethodInfo _hasAllianceMethod; // Kingdom.hasAlliance()
        private static System.Reflection.MethodInfo _getAllianceMethod; // Kingdom.getAlliance()
        private static System.Reflection.MethodInfo _isEnemyMethod;     // Kingdom.isEnemy(Kingdom)
        private static System.Reflection.FieldInfo _opinionTotalField;  // Opinion.total
        private static object _warWinnerPeace;                          // WarWinner.Peace 枚举值

        private static bool _probed;

        /// <summary>首次使用时探测全部反射成员（只探测一次，结果缓存）。</summary>
        private static void EnsureProbed()
        {
            if (_probed) return;
            _probed = true;
            try
            {
                const System.Reflection.BindingFlags F =
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                const System.Reflection.BindingFlags FS = F | System.Reflection.BindingFlags.Static;

                _startWarMethod = typeof(DiplomacyManager).GetMethod("startWar", F);
                _getRelationMethod = typeof(DiplomacyManager).GetMethod("getRelation", F);

                var worldType = typeof(World);
                var warsField = worldType.GetField("wars", FS) ?? (System.Reflection.MemberInfo)worldType.GetProperty("wars", FS);
                var warsType = warsField is System.Reflection.FieldInfo fw ? fw.FieldType : ((System.Reflection.PropertyInfo)warsField).PropertyType;
                _endWarMethod = warsType.GetMethod("endWar", F);
                _getWarsMethod = warsType.GetMethod("getWars", F);

                var alliancesMember = worldType.GetField("alliances", FS) ?? (System.Reflection.MemberInfo)worldType.GetProperty("alliances", FS);
                var alliancesType = alliancesMember is System.Reflection.FieldInfo fa ? fa.FieldType : ((System.Reflection.PropertyInfo)alliancesMember).PropertyType;
                _newAllianceMethod = alliancesType.GetMethod("newAlliance", F);
                _dissolveAllianceMethod = alliancesType.GetMethod("dissolveAlliance", F);

                var kingdomType = typeof(Kingdom);
                _hasAllianceMethod = kingdomType.GetMethod("hasAlliance", F);
                _getAllianceMethod = kingdomType.GetMethod("getAlliance", F);
                _isEnemyMethod = kingdomType.GetMethod("isEnemy", F);
                if (_getAllianceMethod != null)
                {
                    var allianceType = _getAllianceMethod.ReturnType;
                    _joinAllianceMethod = allianceType.GetMethod("join", F);
                }

                if (_getRelationMethod != null)
                {
                    var relationType = _getRelationMethod.ReturnType;
                    _getOpinionMethod = relationType.GetMethod("getOpinion", F);
                    if (_getOpinionMethod != null)
                        _opinionTotalField = _getOpinionMethod.ReturnType.GetField("total", F);
                }

                // WarWinner.Peace：枚举值经运行时定位（本程序集构建期不可见）
                foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    System.Type[] types;
                    try { types = asm.GetTypes(); } catch (System.Exception) { continue; }
                    foreach (var t in types)
                    {
                        if (t != null && t.Name == "WarWinner" && t.IsEnum)
                        {
                            try { _warWinnerPeace = System.Enum.Parse(t, "Peace"); } catch (System.Exception) { }
                            break;
                        }
                    }
                    if (_warWinnerPeace != null) break;
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>外交可用性（所有关键反射成员就位）。</summary>
        private static bool DiplomacyAvailable
        {
            get
            {
                EnsureProbed();
                return _startWarMethod != null && _endWarMethod != null && _getWarsMethod != null
                    && _newAllianceMethod != null;
            }
        }

        private static Kingdom Mine()
        {
            long id = NationEngine.NationKingdomId;
            return id != 0 ? GameHelpers.FindKingdom(id) : null;
        }

        /// <summary>目标国与本国的原版好感（不可用时为 0）。</summary>
        public static int GetRelationScore(Kingdom target)
        {
            var mine = Mine();
            if (mine == null || target == null || target.data == null) return 0;
            EnsureProbed();
            try
            {
                if (_getRelationMethod == null) return 0;
                var relation = _getRelationMethod.Invoke(World.world.diplomacy, new object[] { mine, target });
                if (relation == null || _getOpinionMethod == null) return 0;
                var opinion = _getOpinionMethod.Invoke(relation, new object[] { target, mine });
                if (opinion == null || _opinionTotalField == null) return 0;
                return System.Convert.ToInt32(_opinionTotalField.GetValue(opinion));
            }
            catch (System.Exception) { return 0; }
        }

        /// <summary>赠礼累计好感。</summary>
        public static int GetGoodwill(long kingdomId)
        {
            int g;
            return _goodwill.TryGetValue(kingdomId, out g) ? g : 0;
        }

        /// <summary>是否与目标国处于战争。</summary>
        public static bool IsAtWarWith(Kingdom target)
        {
            var mine = Mine();
            if (mine == null || target == null) return false;
            EnsureProbed();
            if (_isEnemyMethod != null)
            {
                try { return (bool)_isEnemyMethod.Invoke(mine, new object[] { target }); }
                catch (System.Exception) { }
            }
            // 回退：遍历战争列表
            var wars = GetActiveWars(mine);
            foreach (var w in wars)
            {
                try
                {
                    if (IsWarBetween(w, mine, target)) return true;
                }
                catch (System.Exception) { }
            }
            return false;
        }

        /// <summary>宣战：解散共同联盟（背叛）+ startWar(whisper_of_war)。</summary>
        public static bool DeclareWar(Kingdom target, out string msgKey)
        {
            msgKey = "toast_dip_declare_ok";
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (UnrestConfig.Instance == null || !UnrestConfig.Instance.NationPlayEnabled) { msgKey = "toast_dip_no_nation"; return false; }
            if (!DiplomacyAvailable) { msgKey = "toast_dip_unavailable"; return false; }
            if (IsAtWarWith(target)) { msgKey = "toast_dip_already_war"; return false; }

            try
            {
                // 共同联盟因背叛瓦解
                if (HasAlliance(mine) && HasAlliance(target) && SameAlliance(mine, target))
                {
                    var alliance = GetAlliance(mine);
                    if (alliance != null && _dissolveAllianceMethod != null)
                    {
                        try { _dissolveAllianceMethod.Invoke(World.world.alliances, new object[] { alliance }); } catch (System.Exception) { }
                    }
                }

                var warAsset = AssetManager.war_types_library.get("whisper_of_war");
                if (warAsset == null) { msgKey = "toast_dip_unavailable"; return false; }
                _startWarMethod.Invoke(World.world.diplomacy, new object[] { mine, target, warAsset, true });
                EventStreamService.Record(EventStreamService.TypeNationDiplomacy, target.data.name, 1);
                return true;
            }
            catch (System.Exception) { msgKey = "toast_dip_failed"; return false; }
        }

        /// <summary>求和：我方军力 ≥ 对方 → 免费和谈；否则需按军力差支付赎金（金库）。</summary>
        public static bool SueForPeace(Kingdom target, out string msgKey)
        {
            msgKey = "toast_dip_peace_ok";
            var mine = Mine();
            if (mine == null || target == null || target.data == null) { msgKey = "toast_dip_no_nation"; return false; }
            if (!DiplomacyAvailable) { msgKey = "toast_dip_unavailable"; return false; }
            if (!IsAtWarWith(target)) { msgKey = "toast_dip_not_war"; return false; }

            try
            {
                var wars = GetActiveWars(mine);
                object activeWar = null;
                foreach (var w in wars)
                {
                    if (IsWarBetween(w, mine, target)) { activeWar = w; break; }
                }
                if (activeWar == null || _warWinnerPeace == null) { msgKey = "toast_dip_failed"; return false; }

                int myPower = mine.countTotalWarriors();
                int theirPower = target.countTotalWarriors();
                if (myPower >= theirPower)
                {
                    _endWarMethod.Invoke(World.world.wars, new object[] { activeWar, _warWinnerPeace });
                }
                else
                {
                    long ransom = System.Math.Min(5000L, (theirPower - myPower) * 5L);
                    if (ransom <= 0) ransom = 1;
                    if (NationEngine.Treasury < ransom) { msgKey = "toast_dip_peace_poor"; return false; }
                    if (!NationEngine.TrySpend(ransom)) { msgKey = "toast_dip_peace_poor"; return false; }
                    _endWarMethod.Invoke(World.world.wars, new object[] { activeWar, _warWinnerPeace });
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
            if (!DiplomacyAvailable) { msgKey = "toast_dip_unavailable"; return false; }
            if (IsAtWarWith(target)) { msgKey = "toast_dip_alliance_war"; return false; }
            if (HasAlliance(mine) && HasAlliance(target) && SameAlliance(mine, target)) { msgKey = "toast_dip_alliance_exists"; return false; }

            int score = GetRelationScore(target) + GetGoodwill(target.data.id);
            if (score < 0) { msgKey = "toast_dip_alliance_refused"; return false; }

            try
            {
                bool hasMine = HasAlliance(mine);
                bool hasTheirs = HasAlliance(target);
                if (!hasMine && !hasTheirs)
                {
                    _newAllianceMethod.Invoke(World.world.alliances, new object[] { mine, target });
                }
                else if (hasMine && !hasTheirs)
                {
                    var mineAlliance = GetAlliance(mine);
                    if (mineAlliance == null || _joinAllianceMethod == null) { msgKey = "toast_dip_failed"; return false; }
                    _joinAllianceMethod.Invoke(mineAlliance, new object[] { target });
                }
                else if (!hasMine && hasTheirs)
                {
                    var theirAlliance = GetAlliance(target);
                    if (theirAlliance == null || _joinAllianceMethod == null) { msgKey = "toast_dip_failed"; return false; }
                    _joinAllianceMethod.Invoke(theirAlliance, new object[] { mine });
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

        // ===== 双边贸易协定 =====

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
        public static bool CancelPact(Kingdom target, out string msgKey)
        {
            msgKey = "toast_dip_pact_cancel";
            if (target == null || target.data == null) return false;
            if (_pacts.Remove(target.data.id))
            {
                EventStreamService.Record(EventStreamService.TypeNationDiplomacy, target.data.name, 6);
                return true;
            }
            msgKey = "toast_dip_failed";
            return false;
        }

        /// <summary>双边协定边流量乘数：边两端恰为 本国↔协约国 时生效。</summary>
        public static float BilateralFlowMult(long kingdomA, long kingdomB)
        {
            long mine = NationEngine.NationKingdomId;
            if (mine == 0 || _pacts.Count == 0) return 1f;
            long other = kingdomA == mine ? kingdomB : (kingdomB == mine ? kingdomA : 0L);
            if (other == 0L) return 1f;
            int tier = PactTier(other);
            return tier >= 0 ? 1f + PactFlowPerTier * (tier + 1) : 1f;
        }

        /// <summary>年度管线：双边协定年费（金库 → 消耗）。</summary>
        public static void RunAnnual(int year)
        {
            if (_pacts.Count == 0) return;
            var stats = NationEngine.NationStats();
            float gdp = stats != null ? stats.GDP : 0f;
            var expire = new List<long>();
            foreach (var kv in _pacts)
            {
                long fee = (long)(gdp * PactAnnualCostRatio * NationEngine.TierMult(kv.Value));
                if (fee > 0 && !NationEngine.TrySpend(fee)) expire.Add(kv.Key); // 金库不足 → 协定自动解除
            }
            for (int i = 0; i < expire.Count; i++) _pacts.Remove(expire[i]);
        }

        /// <summary>世界重置/换地图时清空。</summary>
        public static void Reset()
        {
            _pacts.Clear();
            _goodwill.Clear();
        }

        // ===== 反射辅助 =====

        private static List<object> GetActiveWars(Kingdom kingdom)
        {
            var result = new List<object>();
            EnsureProbed();
            if (_getWarsMethod == null) return result;
            try
            {
                var wars = _getWarsMethod.Invoke(World.world.wars, new object[] { kingdom }) as System.Collections.IEnumerable;
                if (wars == null) return result;
                foreach (var w in wars) result.Add(w);
            }
            catch (System.Exception) { }
            return result;
        }

        private static bool IsWarBetween(object war, Kingdom a, Kingdom b)
        {
            if (war == null) return false;
            var t = war.GetType();
            const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            foreach (var name in new string[] { "isAttacker", "isDefender" })
            {
                var m = t.GetMethod(name, F);
                if (m == null) continue;
                bool hitA = false, hitB = false;
                try { hitA = (bool)m.Invoke(war, new object[] { a }); } catch (System.Exception) { }
                try { hitB = (bool)m.Invoke(war, new object[] { b }); } catch (System.Exception) { }
                if (hitA && hitB) return true;
            }
            return false;
        }

        private static bool HasAlliance(Kingdom k)
        {
            EnsureProbed();
            if (_hasAllianceMethod == null || k == null) return false;
            try { return (bool)_hasAllianceMethod.Invoke(k, null); } catch (System.Exception) { return false; }
        }

        private static object GetAlliance(Kingdom k)
        {
            EnsureProbed();
            if (_getAllianceMethod == null || k == null) return null;
            try { return _getAllianceMethod.Invoke(k, null); } catch (System.Exception) { return null; }
        }

        private static bool SameAlliance(Kingdom a, Kingdom b)
        {
            var aa = GetAlliance(a);
            var ab = GetAlliance(b);
            return aa != null && aa == ab;
        }
    }
}
