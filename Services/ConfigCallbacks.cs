using EconomyMod.Models;
using EconomyMod.UI;
using NeoModLoader.General;

namespace EconomyMod.Services
{
    /// <summary>
    /// NML 模组设置（default_config.json）回调桥：
    /// NML 在设置窗口关闭时调用回调方法，此处将值写入 UnrestConfig 运行时单例。
    /// 类名必须与 default_config.json 中 "Callback" 的类名一致（NML 按简单类名跨程序集解析）。
    /// </summary>
    public static class EconomyConfigCallbacks
    {
        private const string GroupId = "economy_general";

        /// <summary>
        /// 将本模组 Locales/*.json 注册到 NML 多语言系统，
        /// 使模组设置窗口中的分组/配置项标签显示中文。
        /// zh/ch/cz 均注册中文（cz 为本机当前游戏语言码），en 注册英文。
        /// Mod 界面语言仍由 use_chinese_ui 独立控制。
        /// </summary>
        public static void RegisterLocales()
        {
            try
            {
                var main = EconomyModMain.Instance;
                if (main == null) return;
                var decl = main.GetDeclaration();
                if (decl == null) return;
                string dir = main.GetLocaleFilesDirectory(decl);
                string chPath = System.IO.Path.Combine(dir, "ch.json");
                string enPath = System.IO.Path.Combine(dir, "en.json");
                if (System.IO.File.Exists(chPath))
                {
                    LM.LoadLocale("zh", chPath);
                    LM.LoadLocale("ch", chPath); // 繁体中文语言码
                    LM.LoadLocale("cz", chPath); // 简体中文语言码 → 同样汉化
                }
                if (System.IO.File.Exists(enPath))
                {
                    LM.LoadLocale("en", enPath);
                }
                LM.ApplyLocale(false);
                // 验证标签是否解析为中文（写入日志便于部署检查）
                UnityEngine.Debug.Log($"[ClassicalEconomics] 设置标签示例: economy_general = {LM.Get("economy_general")} / gini_threshold = {LM.Get("gini_threshold")}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[ClassicalEconomics] 注册本地化失败: {e.Message}");
            }
        }

        /// <summary>
        /// 模组加载时从 NML ModConfig 拉取一次全部配置值，确保运行值与设置窗口一致
        /// （回调在窗口关闭时触发，加载期需主动同步一次）。
        /// 数值型配置项均为 TEXT（输入框），值以字符串存储，此处解析并限幅写入。
        /// </summary>
        public static void SyncFromModConfig()
        {
            try
            {
                var cfg = EconomyModMain.Instance?.GetConfig();
                if (cfg == null) return;
                var group = cfg[GroupId];
                if (group == null) return;

                var u = UnrestConfig.Instance;
                if (group.TryGetValue("use_chinese_ui", out var lang)) u.Language = lang.BoolVal ? "zh" : "en";
                if (group.TryGetValue("unrest_enabled", out var on))   u.Enabled = on.BoolVal;
                if (group.TryGetValue("log_worldlog", out var log))    u.LogToWorldLog = log.BoolVal;
                if (group.TryGetValue("gini_threshold", out var g))    u.GiniThreshold = ParseFloat(g.TextVal, u.GiniThreshold, 0.1f, 1.0f);
                if (group.TryGetValue("unrest_grace_years", out var gr)) u.MinUnrestStartYear = ParseInt(gr.TextVal, u.MinUnrestStartYear, 0, 50);
                if (group.TryGetValue("unrest_max_cities", out var mc))  u.MaxAffectedPerKingdom = ParseInt(mc.TextVal, u.MaxAffectedPerKingdom, 1, 5);
                // 国家政策（降基尼）
                if (group.TryGetValue("policy_enabled", out var pe2))     u.PolicyEnabled = pe2.BoolVal;
                if (group.TryGetValue("cycle_enabled", out var cy))      u.CycleEnabled = cy.BoolVal;
                if (group.TryGetValue("cycle_gini_high", out var gh))    u.CycleGiniHigh = ParseFloat(gh.TextVal, u.CycleGiniHigh, 0.3f, 0.9f);
                if (group.TryGetValue("cycle_gini_low", out var gl))     u.CycleGiniLow = ParseFloat(gl.TextVal, u.CycleGiniLow, 0.2f, 0.7f);
                if (group.TryGetValue("cycle_gini_periods", out var gp)) u.CycleGiniPeriods = ParseInt(gp.TextVal, u.CycleGiniPeriods, 1, 5);
                if (group.TryGetValue("boom_stimulus_ratio", out var bs)) u.BoomStimulusRatio = ParseFloat(bs.TextVal, u.BoomStimulusRatio, 0f, 0.1f);
                if (group.TryGetValue("boom_bubble_factor", out var bf)) u.BoomBubbleFactor = ParseFloat(bf.TextVal, u.BoomBubbleFactor, 0.05f, 0.5f);
                if (group.TryGetValue("bubble_threshold", out var bt))   u.BubbleThreshold = ParseFloat(bt.TextVal, u.BubbleThreshold, 1000f, 50000f);
                if (group.TryGetValue("boom_max_duration", out var bm))  u.BoomMaxDuration = ParseInt(bm.TextVal, u.BoomMaxDuration, 2, 20);
                if (group.TryGetValue("recession_max_duration", out var rem)) u.RecessionMaxDuration = ParseInt(rem.TextVal, u.RecessionMaxDuration, 1, 20);
                if (group.TryGetValue("depression_max_duration", out var dem)) u.DepressionMaxDuration = ParseInt(dem.TextVal, u.DepressionMaxDuration, 1, 20);
                if (group.TryGetValue("recovery_max_duration", out var rcm)) u.RecoveryMaxDuration = ParseInt(rcm.TextVal, u.RecoveryMaxDuration, 1, 20);
                if (group.TryGetValue("survival_line", out var sl))      u.SurvivalLine = ParseFloat(sl.TextVal, u.SurvivalLine, 0.5f, 10f);
                if (group.TryGetValue("war_plunder_ratio", out var wp))  u.WarPlunderRatio = ParseFloat(wp.TextVal, u.WarPlunderRatio, 0f, 0.5f);
                if (group.TryGetValue("war_waste_ratio", out var ww))    u.WarWasteRatio = ParseFloat(ww.TextVal, u.WarWasteRatio, 0f, 1f);
                if (group.TryGetValue("revolution_delay_years", out var rd)) u.RevolutionDelayYears = ParseInt(rd.TextVal, u.RevolutionDelayYears, 1, 10);
                if (group.TryGetValue("revolution_kill_ratio", out var rk)) u.RevolutionKillRatio = ParseFloat(rk.TextVal, u.RevolutionKillRatio, 0.1f, 0.8f);
                // 街头起义（政权崩塌）
                if (group.TryGetValue("uprising_gini_threshold", out var ug)) u.UprisingGiniThreshold = ParseFloat(ug.TextVal, u.UprisingGiniThreshold, 0.7f, 1f);
                if (group.TryGetValue("uprising_delay_years", out var uy))    u.UprisingDelayYears = ParseInt(uy.TextVal, u.UprisingDelayYears, 1, 10);
                if (group.TryGetValue("kill_rich_ratio", out var kr))         u.KillRichRatio = ParseFloat(kr.TextVal, u.KillRichRatio, 0.01f, 0.3f);
                if (group.TryGetValue("kill_rich_redist_ratio", out var krr)) u.KillRichRedistRatio = ParseFloat(krr.TextVal, u.KillRichRedistRatio, 0.1f, 1f);
                // 年度累进税
                if (group.TryGetValue("wealth_tax_enabled", out var wt))       u.WealthTaxEnabled = wt.BoolVal;
                if (group.TryGetValue("wealth_tax_ratio", out var wtr))        u.WealthTaxRatio = ParseFloat(wtr.TextVal, u.WealthTaxRatio, 0f, 0.5f);
                if (group.TryGetValue("wealth_tax_line", out var wtl))         u.WealthTaxLineMult = ParseFloat(wtl.TextVal, u.WealthTaxLineMult, 1f, 3f);
                // 王国贸易金流
                if (group.TryGetValue("trade_enabled", out var te))       u.TradeEnabled = te.BoolVal;
                if (group.TryGetValue("trade_flow_ratio", out var tfr))    u.TradeFlowRatio = ParseFloat(tfr.TextVal, u.TradeFlowRatio, 0f, 0.2f);
                // 人口约束（马尔萨斯）
                if (group.TryGetValue("population_enabled", out var pe))   u.PopulationEnabled = pe.BoolVal;
                if (group.TryGetValue("population_overcrowd", out var po)) u.OvercrowdRatio = ParseFloat(po.TextVal, u.OvercrowdRatio, 0.5f, 1.0f);
                // 王国时代事件（盛世/复兴/强盛期/经济崩溃）
                if (group.TryGetValue("era_enabled", out var ee))              u.EraEnabled = ee.BoolVal;
                if (group.TryGetValue("era_duration_years", out var ed))       u.EraDurationYears = ParseInt(ed.TextVal, u.EraDurationYears, 1, 50);
                if (group.TryGetValue("collapse_drop_ratio", out var cdr))     u.CollapseDropRatio = ParseFloat(cdr.TextVal, u.CollapseDropRatio, 0.05f, 0.8f);
                if (group.TryGetValue("collapse_duration_years", out var cdy)) u.CollapseDurationYears = ParseInt(cdy.TextVal, u.CollapseDurationYears, 1, 20);
                if (group.TryGetValue("flourish_military_ratio", out var fmr)) u.FlourishMilitaryRatio = ParseFloat(fmr.TextVal, u.FlourishMilitaryRatio, 0.05f, 0.9f);
                if (group.TryGetValue("flourish_periods", out var fp))        u.FlourishPeriods = ParseInt(fp.TextVal, u.FlourishPeriods, 1, 10);
                // 劳动分工
                if (group.TryGetValue("labor_enabled", out var le))       u.LaborEnabled = le.BoolVal;
                if (group.TryGetValue("labor_wage_base", out var lwb))    u.LaborWageBase = ParseFloat(lwb.TextVal, u.LaborWageBase, 0f, 5f);
                // 实时数据刷新
                if (group.TryGetValue("real_time_refresh", out var rtr))    u.RealTimeRefresh = rtr.BoolVal;
                if (group.TryGetValue("real_time_interval", out var rti))   u.RealTimeInterval = ParseFloat(rti.TextVal, u.RealTimeInterval, 1f, 60f);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[ClassicalEconomics] 从 ModConfig 同步配置失败: {e.Message}");
            }
        }

        // ===== 解析辅助（TEXT 输入框存字符串，解析失败保留原值）=====

        private static float ParseFloat(string text, float fallback, float min, float max)
        {
            float v;
            if (float.TryParse(text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out v))
                return UnityEngine.Mathf.Clamp(v, min, max);
            return fallback;
        }

        private static int ParseInt(string text, int fallback, int min, int max)
        {
            int v;
            if (int.TryParse(text, out v))
                return UnityEngine.Mathf.Clamp(v, min, max);
            return fallback;
        }

        // ===== NML 设置回调（TEXT 输入框的回调参数为 string，需解析后写入）=====

        public static void OnLanguageChanged(bool pValue)
        {
            UnrestConfig.Instance.Language = pValue ? "zh" : "en";
            // 语言切换后刷新悬浮窗所有静态文本 + 重新注入按钮 tooltip（中/英）
            try { EconomyHUD.Instance?.RefreshAllTexts(); } catch (System.Exception) { }
            try { EconomyUI.ReapplyTooltips(); } catch (System.Exception) { }
        }

        public static void OnEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.Enabled = pValue;
        }

        public static void OnLogChanged(bool pValue)
        {
            UnrestConfig.Instance.LogToWorldLog = pValue;
        }

        public static void OnGiniChanged(string pValue)
        {
            UnrestConfig.Instance.GiniThreshold = ParseFloat(pValue, UnrestConfig.Instance.GiniThreshold, 0.1f, 1.0f);
        }

        public static void OnGraceChanged(string pValue)
        {
            UnrestConfig.Instance.MinUnrestStartYear = ParseInt(pValue, UnrestConfig.Instance.MinUnrestStartYear, 0, 50);
        }

        public static void OnMaxAffectedChanged(string pValue)
        {
            UnrestConfig.Instance.MaxAffectedPerKingdom = ParseInt(pValue, UnrestConfig.Instance.MaxAffectedPerKingdom, 1, 5);
        }

        public static void OnPolicyEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.PolicyEnabled = pValue;
        }

        // ===== 经济周期调制器（Phase 4）回调 =====

        public static void OnCycleEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.CycleEnabled = pValue;
        }

        public static void OnGiniHighChanged(string pValue)
        {
            UnrestConfig.Instance.CycleGiniHigh = ParseFloat(pValue, UnrestConfig.Instance.CycleGiniHigh, 0.3f, 0.9f);
        }

        public static void OnGiniLowChanged(string pValue)
        {
            UnrestConfig.Instance.CycleGiniLow = ParseFloat(pValue, UnrestConfig.Instance.CycleGiniLow, 0.2f, 0.7f);
        }

        public static void OnGiniPeriodsChanged(string pValue)
        {
            UnrestConfig.Instance.CycleGiniPeriods = ParseInt(pValue, UnrestConfig.Instance.CycleGiniPeriods, 1, 5);
        }

        public static void OnBoomStimulusChanged(string pValue)
        {
            UnrestConfig.Instance.BoomStimulusRatio = ParseFloat(pValue, UnrestConfig.Instance.BoomStimulusRatio, 0f, 0.1f);
        }

        public static void OnBubbleFactorChanged(string pValue)
        {
            UnrestConfig.Instance.BoomBubbleFactor = ParseFloat(pValue, UnrestConfig.Instance.BoomBubbleFactor, 0.05f, 0.5f);
        }

        public static void OnBubbleThresholdChanged(string pValue)
        {
            UnrestConfig.Instance.BubbleThreshold = ParseFloat(pValue, UnrestConfig.Instance.BubbleThreshold, 1000f, 50000f);
        }

        public static void OnBoomMaxDurationChanged(string pValue)
        {
            UnrestConfig.Instance.BoomMaxDuration = ParseInt(pValue, UnrestConfig.Instance.BoomMaxDuration, 2, 20);
        }

        public static void OnRecessionMaxDurationChanged(string pValue)
        {
            UnrestConfig.Instance.RecessionMaxDuration = ParseInt(pValue, UnrestConfig.Instance.RecessionMaxDuration, 1, 20);
        }

        public static void OnDepressionMaxDurationChanged(string pValue)
        {
            UnrestConfig.Instance.DepressionMaxDuration = ParseInt(pValue, UnrestConfig.Instance.DepressionMaxDuration, 1, 20);
        }

        public static void OnRecoveryMaxDurationChanged(string pValue)
        {
            UnrestConfig.Instance.RecoveryMaxDuration = ParseInt(pValue, UnrestConfig.Instance.RecoveryMaxDuration, 1, 20);
        }

        public static void OnSurvivalLineChanged(string pValue)
        {
            UnrestConfig.Instance.SurvivalLine = ParseFloat(pValue, UnrestConfig.Instance.SurvivalLine, 0.5f, 10f);
        }

        // ===== 社会危机引擎（Phase 5）回调 =====

        public static void OnWarPlunderRatioChanged(string pValue)
        {
            UnrestConfig.Instance.WarPlunderRatio = ParseFloat(pValue, UnrestConfig.Instance.WarPlunderRatio, 0f, 0.5f);
        }

        public static void OnWarWasteRatioChanged(string pValue)
        {
            UnrestConfig.Instance.WarWasteRatio = ParseFloat(pValue, UnrestConfig.Instance.WarWasteRatio, 0f, 1f);
        }

        public static void OnRevolutionDelayChanged(string pValue)
        {
            UnrestConfig.Instance.RevolutionDelayYears = ParseInt(pValue, UnrestConfig.Instance.RevolutionDelayYears, 1, 10);
        }

        public static void OnRevolutionKillRatioChanged(string pValue)
        {
            UnrestConfig.Instance.RevolutionKillRatio = ParseFloat(pValue, UnrestConfig.Instance.RevolutionKillRatio, 0.1f, 0.8f);
        }

        public static void OnUprisingGiniThresholdChanged(string pValue)
        {
            UnrestConfig.Instance.UprisingGiniThreshold = ParseFloat(pValue, UnrestConfig.Instance.UprisingGiniThreshold, 0.7f, 1f);
        }

        public static void OnUprisingDelayYearsChanged(string pValue)
        {
            UnrestConfig.Instance.UprisingDelayYears = ParseInt(pValue, UnrestConfig.Instance.UprisingDelayYears, 1, 10);
        }

        public static void OnKillRichRatioChanged(string pValue)
        {
            UnrestConfig.Instance.KillRichRatio = ParseFloat(pValue, UnrestConfig.Instance.KillRichRatio, 0.01f, 0.3f);
        }

        public static void OnKillRichRedistRatioChanged(string pValue)
        {
            UnrestConfig.Instance.KillRichRedistRatio = ParseFloat(pValue, UnrestConfig.Instance.KillRichRedistRatio, 0.1f, 1f);
        }

        // ===== 年度累进税回调 =====

        public static void OnWealthTaxEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.WealthTaxEnabled = pValue;
        }

        public static void OnWealthTaxRatioChanged(string pValue)
        {
            UnrestConfig.Instance.WealthTaxRatio = ParseFloat(pValue, UnrestConfig.Instance.WealthTaxRatio, 0f, 0.5f);
        }

        public static void OnWealthTaxLineChanged(string pValue)
        {
            UnrestConfig.Instance.WealthTaxLineMult = ParseFloat(pValue, UnrestConfig.Instance.WealthTaxLineMult, 1f, 3f);
        }

        // ===== 王国贸易金流回调 =====

        public static void OnTradeEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.TradeEnabled = pValue;
        }

        public static void OnTradeFlowRatioChanged(string pValue)
        {
            UnrestConfig.Instance.TradeFlowRatio = ParseFloat(pValue, UnrestConfig.Instance.TradeFlowRatio, 0f, 0.2f);
        }

        // ===== 人口约束（马尔萨斯）回调 =====

        public static void OnPopulationEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.PopulationEnabled = pValue;
        }

        public static void OnOvercrowdChanged(string pValue)
        {
            UnrestConfig.Instance.OvercrowdRatio = ParseFloat(pValue, UnrestConfig.Instance.OvercrowdRatio, 0.5f, 1.0f);
        }

        // ===== 王国时代事件（EraEngine）回调 =====

        public static void OnEraEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.EraEnabled = pValue;
        }

        public static void OnEraDurationYearsChanged(string pValue)
        {
            UnrestConfig.Instance.EraDurationYears = ParseInt(pValue, UnrestConfig.Instance.EraDurationYears, 1, 50);
        }

        public static void OnCollapseDropRatioChanged(string pValue)
        {
            UnrestConfig.Instance.CollapseDropRatio = ParseFloat(pValue, UnrestConfig.Instance.CollapseDropRatio, 0.05f, 0.8f);
        }

        public static void OnCollapseDurationYearsChanged(string pValue)
        {
            UnrestConfig.Instance.CollapseDurationYears = ParseInt(pValue, UnrestConfig.Instance.CollapseDurationYears, 1, 20);
        }

        public static void OnFlourishMilitaryRatioChanged(string pValue)
        {
            UnrestConfig.Instance.FlourishMilitaryRatio = ParseFloat(pValue, UnrestConfig.Instance.FlourishMilitaryRatio, 0.05f, 0.9f);
        }

        public static void OnFlourishPeriodsChanged(string pValue)
        {
            UnrestConfig.Instance.FlourishPeriods = ParseInt(pValue, UnrestConfig.Instance.FlourishPeriods, 1, 10);
        }

        // ===== 劳动分工回调 =====

        public static void OnLaborEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.LaborEnabled = pValue;
        }

        public static void OnWageBaseChanged(string pValue)
        {
            UnrestConfig.Instance.LaborWageBase = ParseFloat(pValue, UnrestConfig.Instance.LaborWageBase, 0f, 5f);
        }

        // ===== 实时数据刷新回调 =====

        public static void OnRealTimeRefreshChanged(bool pValue)
        {
            UnrestConfig.Instance.RealTimeRefresh = pValue;
        }

        public static void OnRealTimeIntervalChanged(string pValue)
        {
            UnrestConfig.Instance.RealTimeInterval = ParseFloat(pValue, UnrestConfig.Instance.RealTimeInterval, 1f, 60f);
        }
    }
}
