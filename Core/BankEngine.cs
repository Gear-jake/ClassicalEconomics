using System.Collections.Generic;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 银行引擎（v1.5.0）：真实金币流的城市账本 + 玩家央行操作台 + AI 全自动银行。
    /// - 玩家国：每城账本（储备 + 单笔贷款簿），三要素档位（基准利率/放贷额度/准备金率）
    ///   + 刺激/中性/抑制预设；设置与账本随存档持久化（rb_bank_*）。
    /// - AI 国：简化池（储备/在外本金/均率/到期批年），按国性自动选通道（重商→刺激、
    ///   法理→中性、闭关→抑制、仁政→低利率），统计式回收/违约。
    /// - 年度生命周期：存款（居民财富按比例真转储备）→ 放贷（低利率量大但违约率高）→
    ///   回收（到期本息，扣不动即违约核销）→ 风险档（违约率+准备金覆盖）→ 货币联动
    ///   （放贷净增 × bank_money_supply_factor 计入 MoneySupply → CPI/泡沫）。
    /// - 商业：市场建筑年产商业税入金库（特许经营权 +60%/低价法案 −30%/商路断绝减半），
    ///   特许经营权另加违约风险，两政策互斥。
    /// 全程金币守恒：存款/放贷/回收均为真实转移；违约核销不移动金币（本金已在借款人处）。
    /// </summary>
    internal static class BankEngine
    {
        // ===== 玩家三要素（0/1/2 档；含義見各档表）=====
        internal static int RateTier = 1;      // 0=高利率 1=中 2=低（低→量大+险高）
        internal static int QuotaTier = 1;     // 0=紧 1=中 2=松
        internal static int ReserveTier = 1;   // 0=高准备金 1=中 2=低
        internal static bool FranchiseOn;      // 特许经营权（商业税+60%，违约风险+）
        internal static bool FairPriceOn;      // 低价法案（民怨积累×0.8 由 UnrestEngine 消费，商业税−30%）

        private const int RateHigh = 120;   // ‰/年
        private const int RateMid = 80;
        private const int RateLow = 50;
        private static readonly float[] QuotaFrac = { 0.25f, 0.5f, 0.8f };   // 可放贷储备比例
        private static readonly float[] ReserveFrac = { 0.4f, 0.25f, 0.1f }; // 准备金要求比例

        // ===== 玩家城市账本 =====
        internal class LoanRecord
        {
            public long ActorId;
            public long KingdomId;
            public long Principal;
            public int RatePermille;
            public int DueYear;
            public bool Active;
        }

        internal class CityLedger
        {
            public long CityId;
            public long Reserves;
            public readonly List<LoanRecord> Loans = new List<LoanRecord>(16);
        }

        private static readonly Dictionary<long, CityLedger> _playerLedgers = new Dictionary<long, CityLedger>(16);

        // ===== AI 简化池 =====
        internal class AiBank
        {
            public long Reserves;
            public long Outstanding;
            public int RatePermille;
            public int DueYear;
        }

        private static readonly Dictionary<long, AiBank> _aiBanks = new Dictionary<long, AiBank>(32);

        // ===== 统计与商业 =====
        private static long _lastCommerceTax;
        private static int _commercePenaltyUntilYear = int.MinValue; // 商路断绝：商业税减半至此年
        private static int _lastDefaultCount;
        private static int _lastDueCount;

        /// <summary>挤兑风险档：0=安全 1=警戒 2=危险（事件条件/灯色消费）。</summary>
        public static int RiskTier { get; private set; }

        public static long PlayerReservesTotal
        {
            get
            {
                long s = 0;
                foreach (var l in _playerLedgers.Values) s += l.Reserves;
                return s;
            }
        }

        public static long PlayerOutstandingTotal
        {
            get
            {
                long s = 0;
                foreach (var l in _playerLedgers.Values)
                    foreach (var ln in l.Loans)
                        if (ln.Active) s += ln.Principal;
                return s;
            }
        }

        /// <summary>上年违约占比（0~1；无到期贷款为 0）。</summary>
        public static float LastDefaultRate
            => _lastDueCount > 0 ? (float)_lastDefaultCount / _lastDueCount : 0f;

        public static long LastCommerceTax => _lastCommerceTax;

        private static int RatePermilleOf()
        {
            switch (RateTier)
            {
                case 0: return RateHigh;
                case 2: return RateLow;
                default: return RateMid;
            }
        }

        /// <summary>商业税乘数：低价法案 ×0.7，商路断绝期 ×0.5，特许经营权 ×1.6。</summary>
        public static float CommerceTaxMult(int year)
        {
            float m = 1f;
            if (FairPriceOn) m *= 0.7f;
            if (year < _commercePenaltyUntilYear) m *= 0.5f;
            if (FranchiseOn) m *= 1.6f;
            return m;
        }

        public static void SetCommercePenalty(int untilYear)
        {
            if (untilYear > _commercePenaltyUntilYear) _commercePenaltyUntilYear = untilYear;
        }

        // ===== 商业政策开关（互斥；UI 调用）=====

        public static void SetFranchise(bool on)
        {
            FranchiseOn = on;
            if (on) FairPriceOn = false;
        }

        public static void SetFairPrice(bool on)
        {
            FairPriceOn = on;
            if (on) FranchiseOn = false;
        }

        // ===== 预设通道（UI/AI 调用）=====

        public static void ApplyChannel(int channel)
        {
            switch (channel)
            {
                case 0: // 刺激
                    RateTier = 2; QuotaTier = 2; ReserveTier = 2;
                    break;
                case 1: // 中性
                    RateTier = 1; QuotaTier = 1; ReserveTier = 1;
                    break;
                default: // 抑制
                    RateTier = 0; QuotaTier = 0; ReserveTier = 0;
                    break;
            }
        }

        // ===== 年度评估（AnnualStage.Bank）=====

        public static void Evaluate(int year)
        {
            var cfg = UnrestConfig.Instance;
            if (World.world == null || cfg == null || !cfg.BankEnabled) return;
            if (!cfg.NationPlayEnabled) return;

            float depositRatio = Mathf.Clamp(cfg.BankDepositRatioDefault / 100f, 0f, 0.3f);
            int maxLoansPerCity = Mathf.Max(1, cfg.BankMaxLoansPerCity);
            long netLoanGrowth = 0;
            _lastDefaultCount = 0;
            _lastDueCount = 0;

            // ---- 玩家国 ----
            long playerId = NationEngine.NationKingdomId;
            var playerKingdom = GameHelpers.FindKingdom(playerId);
            if (playerKingdom != null && playerKingdom.data != null)
            {
                netLoanGrowth += EvaluatePlayer(playerKingdom, year, depositRatio, maxLoansPerCity);
            }
            else
            {
                _playerLedgers.Clear(); // 未认领：无银行业务
            }

            // ---- AI 国（简化池）----
            int aiActive = 0;
            var kingdomList = GameHelpers.KingdomSnapshot();
            if (kingdomList != null)
            {
                foreach (var k in kingdomList)
                {
                    if (k == null || k.data == null || k.data.id == 0 || k.data.id == playerId) continue;
                    if (k.units == null) continue;
                    netLoanGrowth += EvaluateAi(k, year, depositRatio);
                    aiActive++;
                    if (aiActive >= 64) break; // 有界：最多统计 64 个 AI 王国银行
                }
            }

            // ---- 货币联动：放贷净增量计入货币供给（次年 CPI 生效）----
            if (netLoanGrowth > 0)
            {
                float factor = Mathf.Clamp(cfg.BankMoneySupplyFactor, 0f, 0.5f);
                EconomyCycleModulator.MoneySupply += netLoanGrowth * factor;
            }

            // ---- 风险档 ----
            float defRate = LastDefaultRate;
            float outstanding = PlayerOutstandingTotal;
            float coverage = outstanding > 0 ? (float)PlayerReservesTotal / outstanding : 1f;
            if (defRate > 0.25f || coverage < 0.15f) RiskTier = 2;
            else if (defRate > 0.12f || coverage < 0.3f) RiskTier = 1;
            else RiskTier = 0;
        }

        /// <summary>玩家国年度结算：存款 → 放贷 → 回收。返回放贷净增量。</summary>
        private static long EvaluatePlayer(Kingdom kingdom, int year, float depositRatio, int maxLoansPerCity)
        {
            long netGrowth = 0;
            long kid = kingdom.data.id;
            var stats = NationEngine.NationStats();
            float avg = stats?.AvgWealth ?? 0f;

            var cities = NationEngine.SnapshotCities(kingdom, new List<City>(8));
            var seen = new HashSet<long>();
            // 1) 存款 + 重建/更新账本（城市消失则账本一并移除，余额随城灭——与其他世界资产同规则）
            foreach (var city in cities)
            {
                if (city == null) continue;
                long cid;
                try { cid = city.id; } catch (System.Exception) { continue; }
                if (!seen.Add(cid)) continue;
                var ledger = GetOrCreateLedger(cid);

                // 存款：市民财富按比例转入储备（真实转移）
                if (city.units != null && depositRatio > 0f)
                {
                    foreach (var a in city.units)
                    {
                        if (a == null || !a.isAlive() || !GameHelpers.IsCivilizedActor(a)) continue;
                        float w;
                        if (!GameHelpers.TryGetWealth(a, out w) || w < 20f) continue;
                        long dep = (long)(w * depositRatio);
                        if (dep <= 0) continue;
                        if (!AddMoneySafe(a, -dep)) continue;
                        ledger.Reserves += dep;
                    }
                }
            }
            // 移除已消失城市的账本
            var deadCities = new List<long>();
            foreach (var kv in _playerLedgers)
                if (!seen.Contains(kv.Key)) deadCities.Add(kv.Key);
            foreach (var cid in deadCities) _playerLedgers.Remove(cid);

            int ratePermille = RatePermilleOf();

            // 2) 放贷：额度比例内贷给城中低收入居民（真实转移）
            foreach (var kv in _playerLedgers)
            {
                var ledger = kv.Value;
                var city = FindCityById(kingdom, kv.Key);
                if (city == null || city.units == null) continue;

                // 回收到期（在放贷前做，腾出额度）
                netGrowth += CollectDue(ledger, kingdom, year);

                long lendable = (long)(ledger.Reserves * QuotaFrac[QuotaTier])
                                - (long)(ledger.Reserves * ReserveFrac[ReserveTier]);
                if (lendable <= 0) continue;

                int activeCount = 0;
                foreach (var ln in ledger.Loans) if (ln.Active) activeCount++;
                int slots = maxLoansPerCity - activeCount;
                if (slots <= 0) continue;

                // 候选借款人：城中低收入文明单位
                _borrowerPool.Clear();
                foreach (var a in city.units)
                {
                    if (a == null || !a.isAlive() || !GameHelpers.IsCivilizedActor(a)) continue;
                    float w;
                    if (!GameHelpers.TryGetWealth(a, out w)) continue;
                    if (w < avg * 1.2f) _borrowerPool.Add(a);
                }
                if (_borrowerPool.Count == 0) continue;

                long principal = System.Math.Max(20L, System.Math.Min(400L, lendable / _borrowerPool.Count));
                int issued = 0;
                foreach (var a in _borrowerPool)
                {
                    if (issued >= slots || lendable < principal) break;
                    long aid;
                    try { aid = a.id; } catch (System.Exception) { continue; }
                    if (aid == 0) continue;
                    if (!AddMoneySafe(a, principal)) continue;
                    ledger.Reserves -= principal;
                    lendable -= principal;
                    ledger.Loans.Add(new LoanRecord
                    {
                        ActorId = aid,
                        KingdomId = kid,
                        Principal = principal,
                        RatePermille = ratePermille,
                        DueYear = year + 2,
                        Active = true
                    });
                    issued++;
                    netGrowth += principal;
                }
            }
            return netGrowth;
        }

        /// <summary>回收到期贷款：能扣则本息入储备，扣不动核销（金币不移动，违约计入风险）。</summary>
        private static long CollectDue(CityLedger ledger, Kingdom kingdom, int year)
        {
            long collected = 0;
            var units = kingdom != null && kingdom.units != null ? kingdom.units : null;
            for (int i = ledger.Loans.Count - 1; i >= 0; i--)
            {
                var ln = ledger.Loans[i];
                if (!ln.Active || ln.DueYear > year) continue;
                _lastDueCount++;
                long owe = ln.Principal + ln.Principal * ln.RatePermille / 1000;
                long got = 0;
                if (units != null)
                {
                    foreach (var a in units)
                    {
                        if (a == null) continue;
                        long aid;
                        try { aid = a.id; } catch (System.Exception) { continue; }
                        if (aid != ln.ActorId) continue;
                        if (!a.isAlive()) break;
                        float w;
                        if (GameHelpers.TryGetWealth(a, out w))
                        {
                            long payable = System.Math.Min(owe, (long)System.Math.Max(0f, w * 0.5f));
                            if (payable > 0)
                            {
                                if (AddMoneySafe(a, -payable)) got = payable;
                            }
                        }
                        break;
                    }
                }
                if (got >= owe)
                {
                    ledger.Reserves += owe;
                    collected += owe;
                    ln.Active = false;
                }
                else
                {
                    // 违约核销：不移动金币（本金已在借款人处），风险上升
                    ln.Active = false;
                    _lastDefaultCount++;
                }
                if (ledger.Loans.Count > 64 && !ln.Active) ledger.Loans.RemoveAt(i); // 账簿有界
            }
            return collected;
        }

        /// <summary>AI 国年度结算：按国性选通道，简化池统计式存贷收违约。返回放贷净增量。</summary>
        private static long EvaluateAi(Kingdom k, int year, float depositRatio)
        {
            long kid = k.data.id;
            var stats = NationEngine.NationStats();
            AiBank bank;
            if (!_aiBanks.TryGetValue(kid, out bank) || bank == null)
            {
                bank = new AiBank { RatePermille = 80, DueYear = year };
                _aiBanks[kid] = bank;
            }

            // 通道：每 5 年按国性重选
            if (year >= bank.DueYear)
            {
                int style = LawEngine.GetStyle(kid);
                int channel = style == 1 ? 0 : style == 4 ? 2 : 1; // 重商→刺激 闭关→抑制 其余中性
                switch (channel)
                {
                    case 0: bank.RatePermille = RateLow; break;
                    case 2: bank.RatePermille = RateHigh; break;
                    default: bank.RatePermille = RateMid; break;
                }
                bank.DueYear = year + 5;
            }

            long netGrowth = 0;
            // 存款（聚合）
            if (k.units != null && depositRatio > 0f)
            {
                foreach (var a in k.units)
                {
                    if (a == null || !a.isAlive() || !GameHelpers.IsCivilizedActor(a)) continue;
                    float w;
                    if (!GameHelpers.TryGetWealth(a, out w) || w < 20f) continue;
                    long dep = (long)(w * depositRatio);
                    if (dep <= 0) continue;
                    if (AddMoneySafe(a, -dep)) bank.Reserves += dep;
                }
            }

            // 放贷（按利率档：低利率放出更多真实金币给国民）
            float lendFrac = bank.RatePermille <= RateLow ? 0.6f : bank.RatePermille >= RateHigh ? 0.25f : 0.4f;
            long lend = (long)(bank.Reserves * lendFrac);
            if (lend > 0 && k.units != null)
            {
                lend = GameHelpers.DeductCoinsFromWealth(k.units, lend, 0.4f);
                if (lend > 0)
                {
                    bank.Reserves -= lend;
                    bank.Outstanding += lend;
                    bank.DueYear = year + 2;
                    netGrowth += lend;
                }
            }

            // 到期统计式回收：按经济阶段决定回收比例，缺口即违约
            if (bank.Outstanding > 0 && year >= bank.DueYear)
            {
                float phaseMul = 1f;
                switch (EconomyCycleModulator.CurrentPhase)
                {
                    case EconomyPhase.Depression: phaseMul = 0.45f; break;
                    case EconomyPhase.Recession: phaseMul = 0.7f; break;
                    case EconomyPhase.Boom: phaseMul = 0.95f; break;
                }
                long owe = bank.Outstanding + bank.Outstanding * bank.RatePermille / 1000;
                long target = (long)(owe * phaseMul);
                long got = 0;
                if (k.units != null) got = GameHelpers.DeductCoinsFromWealth(k.units, target, 0.5f);
                bank.Reserves += got;
                long defaulted = owe - got;
                bank.Outstanding = 0;
                if (defaulted > 0)
                {
                    // 危机传染钩子：违约损失记入既有传染账本（弱国承压）
                    BankingEngine.NoteExternalDefault(kid, defaulted);
                }
            }
            return netGrowth;
        }

        private static readonly List<Actor> _borrowerPool = new List<Actor>(64);

        /// <summary>actor.addMoney 只收 int：long 金额钳制后入账（防溢出为负）。</summary>
        private static bool AddMoneySafe(Actor a, long amount)
        {
            if (a == null || amount == 0) return amount == 0;
            int v = amount > int.MaxValue ? int.MaxValue : amount < int.MinValue ? int.MinValue : (int)amount;
            try { a.addMoney(v); return true; }
            catch (System.Exception) { return false; }
        }

        private static CityLedger GetOrCreateLedger(long cityId)
        {
            CityLedger l;
            if (!_playerLedgers.TryGetValue(cityId, out l))
            {
                l = new CityLedger { CityId = cityId };
                _playerLedgers[cityId] = l;
            }
            return l;
        }

        private static City FindCityById(Kingdom kingdom, long cityId)
        {
            try
            {
                var cities = kingdom.getCities();
                if (cities == null) return null;
                foreach (var c in cities)
                {
                    if (c == null) continue;
                    if (c.id == cityId) return c;
                }
            }
            catch (System.Exception) { }
            return null;
        }

        public static void Reset()
        {
            _playerLedgers.Clear();
            _aiBanks.Clear();
            RateTier = 1; QuotaTier = 1; ReserveTier = 1;
            FranchiseOn = FairPriceOn = false;
            _lastCommerceTax = 0;
            _commercePenaltyUntilYear = int.MinValue;
            _lastDefaultCount = _lastDueCount = 0;
            RiskTier = 0;
        }

        public static void ClearWorldReferences()
        {
            _borrowerPool.Clear();
        }

        // ===== 商业税（ NationEngine.RunAnnual 调用，返回本年商业税入金库）=====

        public static long CollectCommerceTax(int year)
        {
            var stats = NationEngine.NationStats();
            if (stats == null || stats.ActorCount <= 0) { _lastCommerceTax = 0; return 0; }
            int markets = 0;
            foreach (var kv in NationEngine._cityBuildings)
                if (kv.Value == (int)NationEngine.BuildingKind.Market) markets++;
            if (markets <= 0) { _lastCommerceTax = 0; return 0; }
            int counted = System.Math.Min(5, markets);
            float perCityPop = (float)stats.ActorCount / System.Math.Max(1, markets);
            long tax = (long)(counted * perCityPop * (stats.AvgWealth * 0.004f) * CommerceTaxMult(year));
            if (tax < 0) tax = 0;
            _lastCommerceTax = tax;
            return tax;
        }

        // ===== 存档（NationSave 调用）=====

        public static void Serialize(System.Action<string, string> write)
        {
            var sb = new System.Text.StringBuilder(128);
            sb.Append(RateTier).Append(',')
              .Append(QuotaTier).Append(',')
              .Append(ReserveTier).Append(',')
              .Append(FranchiseOn ? '1' : '0').Append(',')
              .Append(FairPriceOn ? '1' : '0').Append(',')
              .Append(_commercePenaltyUntilYear);
            write("rb_bank_set", sb.ToString());

            sb.Length = 0;
            foreach (var kv in _playerLedgers)
            {
                sb.Append(kv.Key).Append(':').Append(kv.Value.Reserves);
                foreach (var ln in kv.Value.Loans)
                {
                    if (!ln.Active) continue;
                    sb.Append('|').Append(ln.ActorId).Append(',')
                      .Append(ln.KingdomId).Append(',')
                      .Append(ln.Principal).Append(',')
                      .Append(ln.RatePermille).Append(',')
                      .Append(ln.DueYear);
                }
                sb.Append(';');
            }
            write("rb_bank_ledger", sb.ToString());

            sb.Length = 0;
            foreach (var kv in _aiBanks)
            {
                sb.Append(kv.Key).Append(':').Append(kv.Value.Reserves).Append(',')
                  .Append(kv.Value.Outstanding).Append(',')
                  .Append(kv.Value.RatePermille).Append(',')
                  .Append(kv.Value.DueYear).Append(';');
            }
            write("rb_bank_ai", sb.ToString());
        }

        public static void Restore(string settings, string ledger, string ai)
        {
            Reset();
            try
            {
                if (!string.IsNullOrEmpty(settings))
                {
                    var f = settings.Split(',');
                    int iv;
                    if (f.Length >= 3)
                    {
                        if (int.TryParse(f[0], out iv)) RateTier = Mathf.Clamp(iv, 0, 2);
                        if (int.TryParse(f[1], out iv)) QuotaTier = Mathf.Clamp(iv, 0, 2);
                        if (int.TryParse(f[2], out iv)) ReserveTier = Mathf.Clamp(iv, 0, 2);
                    }
                    if (f.Length >= 5)
                    {
                        FranchiseOn = f[3] == "1";
                        FairPriceOn = f[4] == "1";
                    }
                    if (f.Length >= 6 && int.TryParse(f[5], out iv))
                        _commercePenaltyUntilYear = iv;
                }
            }
            catch (System.Exception) { }

            try
            {
                if (!string.IsNullOrEmpty(ledger))
                {
                    foreach (var cityEntry in ledger.Split(';'))
                    {
                        if (string.IsNullOrEmpty(cityEntry)) continue;
                        var head = cityEntry.Split(':');
                        if (head.Length < 2) continue;
                        long cid;
                        long reserves;
                        if (!long.TryParse(head[0], out cid) || !long.TryParse(head[1], out reserves)) continue;
                        var ledger2 = GetOrCreateLedger(cid);
                        ledger2.Reserves = reserves;
                        if (head.Length < 3) continue;
                        foreach (var loanStr in head[2].Split('|'))
                        {
                            if (string.IsNullOrEmpty(loanStr)) continue;
                            var lf = loanStr.Split(',');
                            if (lf.Length < 5) continue;
                            var ln = new LoanRecord
                            {
                                Active = true,
                                Principal = 1,
                            };
                            long l1;
                            int l2, l3;
                            long l0;
                            if (!long.TryParse(lf[0], out l0)) continue;
                            ln.ActorId = l0;
                            if (!long.TryParse(lf[1], out l1)) continue;
                            ln.KingdomId = l1;
                            if (!long.TryParse(lf[2], out l1)) continue;
                            ln.Principal = l1;
                            if (!int.TryParse(lf[3], out l2)) continue;
                            ln.RatePermille = l2;
                            if (!int.TryParse(lf[4], out l3)) continue;
                            ln.DueYear = l3;
                            ledger2.Loans.Add(ln);
                        }
                    }
                }
            }
            catch (System.Exception) { }

            try
            {
                if (!string.IsNullOrEmpty(ai))
                {
                    foreach (var item in ai.Split(';'))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        var f = item.Split(':');
                        if (f.Length < 5) continue;
                        long kid;
                        if (!long.TryParse(f[0], out kid)) continue;
                        var vals = f[1].Split(',');
                        if (vals.Length < 4) continue;
                        long res;
                        long outstanding;
                        int rate;
                        int due;
                        if (!long.TryParse(vals[0], out res) || !long.TryParse(vals[1], out outstanding)
                            || !int.TryParse(vals[2], out rate) || !int.TryParse(vals[3], out due)) continue;
                        _aiBanks[kid] = new AiBank
                        {
                            Reserves = res,
                            Outstanding = outstanding,
                            RatePermille = rate,
                            DueYear = due
                        };
                    }
                }
            }
            catch (System.Exception) { }
        }
    }
}
