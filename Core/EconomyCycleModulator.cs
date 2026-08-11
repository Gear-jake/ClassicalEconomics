using EconomyMod.Models;
using EconomyMod.Services;
using NeoModLoader.api.attributes;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>经济周期阶段。</summary>
    public enum EconomyPhase
    {
        Boom,       // 繁荣
        Recession,  // 衰退
        Depression, // 萧条
        Recovery    // 复苏
    }

    /// <summary>
    /// 经济周期调制器（Phase 4）。
    /// 在原生税收系统之上叠加周期调制，不替代原生流转：
    /// 1. 四阶段状态机（繁荣/衰退/萧条/复苏），阶段持续时长与是否转移**由全球贫富差距（基尼指数）决定**：
    ///    - 基尼 ≥ 危险线：经济失衡（繁荣破裂转衰退、衰退转萧条）
    ///    - 基尼 ≤ 健康线：财富均等（萧条/复苏转繁荣）
    ///    - 基尼须连续 N 期越线才转移（防抖，节奏可配置）；
    /// 2. 繁荣期信用扩张（按 GDP 比例向全体文明注入硬币）+ 泡沫累积，破裂时蒸发硬币；
    /// 3. 动态修改王国税率特质：繁荣低税率刺激、衰退/萧条/复苏恢复默认（萧条不再主动集中财富，
    ///    避免"基尼高→萧条→更集中→基尼更高"死循环，基尼回落交由累进税等再分配机制）。
    /// </summary>
    public static class EconomyCycleModulator
    {
        // ===== 阶段状态（供 UI 读取）=====

        /// <summary>当前经济阶段。</summary>
        public static EconomyPhase CurrentPhase { get; private set; } = EconomyPhase.Boom;

        /// <summary>当前阶段已持续期数。</summary>
        public static int PhaseDuration { get; private set; }

        /// <summary>繁荣期累积泡沫值（= 累计注入量 × 泡沫系数）。</summary>
        public static float BubbleValue { get; private set; }

        /// <summary>本期 GDP 增长率（仅展示用，不再作为阶段转移判据）。</summary>
        public static float GrowthRate { get; private set; }

        /// <summary>当前价格指数 CPI（= 货币供给 / 总产出×流通速度，1.0=基准）。</summary>
        public static float CurrentCPI { get; private set; } = 1f;

        /// <summary>当前货币供给 M（繁荣注金+/泡沫蒸发- 追踪）。</summary>
        public static float MoneySupply { get; private set; }

        /// <summary>玩家手动设置经济阶段（立即生效，重置持续期与泡沫值，并应用对应政策）。</summary>
        public static void SetPhaseManual(EconomyPhase phase)
        {
            if (phase == CurrentPhase) return;
            CurrentPhase = phase;
            PhaseDuration = 1;
            BubbleValue = 0f;
            _highGiniStreak = 0;
            _lowGiniStreak = 0;
            string logName;
            switch (phase)
            {
                case EconomyPhase.Boom:       logName = "经济繁荣"; break;
                case EconomyPhase.Recession:  logName = "经济衰退"; break;
                case EconomyPhase.Depression: logName = "经济萧条"; break;
                default:                      logName = "经济复苏"; break;
            }
            GameHelpers.Log($"[ClassicalEconomics] 玩家手动切换经济阶段 → {logName}");
            try { ApplyPhasePolicies(UnrestConfig.Instance); }
            catch (System.Exception) { }
        }

        // ===== 内部状态 =====

        private static float _prevGDP;
        private static bool _initialized;   // 首期无上期数据，跳过阶段判定
        private static int _highGiniStreak; // 基尼 ≥ 危险线的连续期数
        private static int _lowGiniStreak;  // 基尼 ≤ 健康线的连续期数

        // 游戏原生王国税率特质 id
        private const string TaxLocalLow = "tax_rate_local_low";       // 低税率 20%
        private const string TaxLocalHigh = "tax_rate_local_high";     // 高税率 70%
        private const string TaxTributeHigh = "tax_rate_tribute_high"; // 高供奉 70%

        // 调制范围：只对财富前 N 的王国施加税率特质，避免干扰小国
        private const int ModulateKingdoms = 5;

        /// <summary>世界重置（新地图/新游戏）时清空周期状态。</summary>
        public static void Reset()
        {
            CurrentPhase = EconomyPhase.Boom;
            PhaseDuration = 0;
            BubbleValue = 0f;
            GrowthRate = 0f;
            _prevGDP = 0f;
            _initialized = false;
            _highGiniStreak = 0;
            _lowGiniStreak = 0;
            MoneySupply = 0f;
            CurrentCPI = 1f;
        }

        /// <summary>
        /// 每个采集周期调用一次（在 EconomyEngine.Recalculate() 之后）。
        /// 顺序：更新增长率与基尼趋势 → 阶段转移（基尼驱动）→ 执行该阶段政策（注入/泡沫/税率）。
        /// </summary>
        [Hotfixable]
        public static void Evaluate()
        {
            var cfg = UnrestConfig.Instance;
            if (!cfg.CycleEnabled) return;

            float gdp = EconomyEngine.GlobalGDP;
            if (!_initialized)
            {
                _initialized = true;
                _prevGDP = gdp;
                GrowthRate = 0f;
                return; // 首期仅记录基线
            }

            GrowthRate = _prevGDP > 0f ? (gdp - _prevGDP) / _prevGDP : 0f;
            _prevGDP = gdp;

            // 基尼趋势：越危险线/健康线分别累积连续期数，中间区间清零
            float gini = EconomyEngine.GiniCoefficient;
            if (gini >= cfg.CycleGiniHigh) { _highGiniStreak++; _lowGiniStreak = 0; }
            else if (gini <= cfg.CycleGiniLow) { _lowGiniStreak++; _highGiniStreak = 0; }
            else { _highGiniStreak = 0; _lowGiniStreak = 0; }

            PhaseDuration++;

            AdvancePhase(cfg);
            // 计算价格指数 CPI = 货币供给 / (总产出 × 流通速度)
            float production = EconomyEngine.TotalProduction;
            float velocity = cfg.MoneyVelocity;
            CurrentCPI = production > 0f && velocity > 0f ? MoneySupply / (production * velocity) : 1f;
            ApplyPhasePolicies(cfg);
        }

        // ===== 阶段转移（由全球贫富差距决定）=====

        [Hotfixable]
        private static void AdvancePhase(UnrestConfig cfg)
        {
            switch (CurrentPhase)
            {
                case EconomyPhase.Boom:
                    // 繁荣结束：贫富差距越危险线持续 N 期（泡沫破裂）、泡沫超阈值、或繁荣超绝对上限
                    // 泡沫阈值自适应：大图用绝对值 BubbleThreshold，小图用 GDP 的 10%（避免小图永不破裂）
                    float gdp = EconomyEngine.GlobalGDP;
                    float bubbleThreshold = gdp > cfg.BubbleThreshold
                        ? cfg.BubbleThreshold
                        : gdp * 0.1f;
                    if (_highGiniStreak >= cfg.CycleGiniPeriods ||
                        BubbleValue >= bubbleThreshold ||
                        PhaseDuration > cfg.BoomMaxDuration)
                    {
                        TriggerBubbleBurst();
                        SetPhase(EconomyPhase.Recession, "经济衰退");
                    }
                    break;

                case EconomyPhase.Recession:
                    // 衰退 → 萧条：贫富差距仍高，或持续超过衰退最大期数；
                    // 衰退 → 复苏：贫富差距回落到健康线
                    if (_highGiniStreak >= cfg.CycleGiniPeriods ||
                        PhaseDuration > cfg.RecessionMaxDuration)
                        SetPhase(EconomyPhase.Depression, "经济萧条");
                    else if (_lowGiniStreak >= cfg.CycleGiniPeriods)
                        SetPhase(EconomyPhase.Recovery, "经济复苏");
                    break;

                case EconomyPhase.Depression:
                    // 萧条持续到贫富差距回落到健康线以下，或超过萧条最大期数（强制回暖）
                    if (_lowGiniStreak >= cfg.CycleGiniPeriods ||
                        PhaseDuration > cfg.DepressionMaxDuration)
                        SetPhase(EconomyPhase.Recovery, "经济复苏");
                    break;

                case EconomyPhase.Recovery:
                    // 复苏：贫富差距保持健康线以下持续 N 期 → 繁荣；超期强制回暖为繁荣
                    if (_lowGiniStreak >= cfg.CycleGiniPeriods ||
                        PhaseDuration > cfg.RecoveryMaxDuration)
                        SetPhase(EconomyPhase.Boom, "经济繁荣");
                    break;
            }
        }

        private static void SetPhase(EconomyPhase next, string logName)
        {
            if (next == CurrentPhase) return;
            CurrentPhase = next;
            PhaseDuration = 1;
            BubbleValue = 0f;
            // 重置基尼连续期数：否则切阶段后旧 streak 立即满足下一跳条件，造成阶段双跳（M1 修复）
            _highGiniStreak = 0;
            _lowGiniStreak = 0;
            GameHelpers.Log($"[ClassicalEconomics] 经济周期 → {logName}（贫富差距 {EconomyEngine.GiniCoefficient:F2}，增长率 {GrowthRate.ToString("+0.0%;-0.0%")}，人均 {EconomyEngine.AvgWealth:F1}）");
        }

        // ===== 阶段政策 =====

        private static void ApplyPhasePolicies(UnrestConfig cfg)
        {
            switch (CurrentPhase)
            {
                case EconomyPhase.Boom:
                    ApplyBoomPolicy(cfg);
                    break;
                case EconomyPhase.Recession:
                    ApplyDefaultTaxPolicy(); // 衰退：恢复默认税率（原生 50%）
                    break;
                case EconomyPhase.Depression:
                    ApplyDefaultTaxPolicy(); // 萧条：不再加高税率集中财富（避免"基尼高→萧条→更集中→基尼更高"死循环，配合累进税让基尼可回落）
                    break;
                case EconomyPhase.Recovery:
                    ApplyDefaultTaxPolicy(); // 复苏：恢复默认税率
                    break;
            }
        }

        /// <summary>繁荣期：向全体文明注入硬币（信用扩张）+ 泡沫累积 + 低税率刺激。</summary>
        [Hotfixable]
        private static void ApplyBoomPolicy(UnrestConfig cfg)
        {
            // 1. 信用扩张：按 GDP 比例折算成人均注入，均匀发给全体文明
            float stimulus = EconomyEngine.GlobalGDP * cfg.BoomStimulusRatio;
            int perActor = EconomyEngine.AliveActorCount > 0
                ? Mathf.Max(1, Mathf.RoundToInt(stimulus / EconomyEngine.AliveActorCount))
                : 0;
            if (perActor > 0)
            {
                int count = InjectCoinsToAllCiv(perActor);
                if (count > 0)
                    GameHelpers.Log($"[ClassicalEconomics] 繁荣期刺激 人均+{perActor} 覆盖{count}人");
            }

            // 货币供给追踪：注金增加 M
            MoneySupply += stimulus;

            // 2. 泡沫累积：注入规模 × 泡沫系数 + 通胀加速（CPI>1 时泡沫累积更快）
            float inflationBoost = CurrentCPI > 1f ? (CurrentCPI - 1f) * cfg.InflationBubbleBoost * stimulus : 0f;
            BubbleValue += stimulus * cfg.BoomBubbleFactor + inflationBoost;

            // 3. 低税率刺激消费（繁荣期政策）
            ApplyTaxPolicy(localLow: true, localHigh: false, tributeHigh: false);
        }

        /// <summary>泡沫破裂：全体文明硬币按比例蒸发（最多 50%），推日志并清零泡沫。
        /// 单遍遍历同时完成蒸发与 WorldLog 事件选人（原 PushBubbleEvent 独立遍历已合并）。</summary>
        private static void TriggerBubbleBurst()
        {
            float gdp = EconomyEngine.GlobalGDP;
            float crashRatio = gdp > 0f ? Mathf.Min(BubbleValue / gdp, 0.5f) : 0f;
            int victims = 0;
            float totalEvaporated = 0f;
            Actor bubbleVictim = null;
            var aliveList = World.world != null && World.world.units != null
                ? World.world.units.units_only_alive : null;
            if (aliveList != null)
            {
                foreach (var actor in aliveList)
                {
                    if (actor == null || !actor.isAlive()) continue;
                    if (actor.asset == null || !actor.asset.civ) continue;
                    if (bubbleVictim == null) bubbleVictim = actor;
                    if (crashRatio <= 0.01f) continue;
                    int coins = Mathf.Max(0, Mathf.RoundToInt(actor.money));
                    if (coins <= 0) continue;
                    try
                    {
                        // M9：保底 1 金币仅对持有 ≥10 金币者生效；小额持有者按比例（可能为 0），
                        // 避免最穷者被 1 金币保底按 100% 比例蒸发（比富人受害更重）。
                        int evap = Mathf.RoundToInt(coins * crashRatio);
                        if (evap < 1 && coins >= 10) evap = 1;
                        actor.addMoney(-evap);
                        victims++;
                        totalEvaporated += evap;
                    }
                    catch (System.Exception) { }
                }
            }

            // 货币供给追踪：蒸发减少 M（通缩压力，下期 CPI 下降）
            // M2：钳制下限为 0，避免蒸发（含 M9 保底取整）导致 MoneySupply 变负
            MoneySupply = Mathf.Max(0f, MoneySupply - totalEvaporated);

            GameHelpers.Log($"[ClassicalEconomics] 经济泡沫破裂！蒸发比例 {crashRatio.ToString("P0")}，波及 {victims} 人（泡沫值 {BubbleValue:F0}，CPI 将下降）");
            GameHelpers.Notify($"[经济] 泡沫破裂！{crashRatio.ToString("P0")} 财富蒸发，波及 {victims} 人");
            EventStreamService.Record(EventStreamService.TypeBubbleBurst, "", Mathf.RoundToInt(crashRatio * 100f));
            try { if (bubbleVictim != null) WorldLog.logFavMurder(bubbleVictim, null); } catch (System.Exception) { }
            BubbleValue = 0f;
        }

        /// <summary>衰退/复苏/萧条期：清除全部税率特质，回到原生默认 50%（萧条不主动集中财富）。</summary>
        private static void ApplyDefaultTaxPolicy()
        {
            ApplyTaxPolicy(localLow: false, localHigh: false, tributeHigh: false);
        }

        /// <summary>对财富前 N 的王国统一设置税率特质（先移除旧的再添加新的，幂等）。</summary>
        private static void ApplyTaxPolicy(bool localLow, bool localHigh, bool tributeHigh)
        {
            var top = EconomyEngine.TopKingdoms(ModulateKingdoms);
            foreach (var stats in top)
            {
                if (stats.KingdomId == 0) continue; // 跳过"无王国"桶
                var kingdom = GameHelpers.FindKingdom(stats.KingdomId);
                if (kingdom == null) continue;
                try
                {
                    SetTrait(kingdom, TaxLocalLow, localLow);
                    SetTrait(kingdom, TaxLocalHigh, localHigh);
                    SetTrait(kingdom, TaxTributeHigh, tributeHigh);
                }
                catch (System.Exception) { }
            }
        }

        private static void SetTrait(Kingdom kingdom, string traitId, bool on)
        {
            bool has = kingdom.hasTrait(traitId);
            if (on && !has) kingdom.addTrait(traitId, true);
            else if (!on && has) kingdom.removeTrait(traitId);
        }

        /// <summary>向全体存活开智文明单位注入 coinsPerActor 枚硬币，返回受影响的单位数。</summary>
        private static int InjectCoinsToAllCiv(int coinsPerActor)
        {
            var aliveList = World.world != null && World.world.units != null
                ? World.world.units.units_only_alive : null;
            if (aliveList == null) return 0;
            int count = 0;
            foreach (var actor in aliveList)
            {
                if (actor == null || !actor.isAlive()) continue;
                if (actor.asset == null || !actor.asset.civ) continue;
                try { actor.addMoney(coinsPerActor); count++; }
                catch (System.Exception) { }
            }
            return count;
        }
    }
}
