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
                string zhTwPath = System.IO.Path.Combine(dir, "zh_tw.json");
                string ruPath = System.IO.Path.Combine(dir, "ru.json");
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
                if (System.IO.File.Exists(zhTwPath))
                {
                    LM.LoadLocale("zh_tw", zhTwPath); // 繁体独立文件
                }
                if (System.IO.File.Exists(ruPath))
                {
                    LM.LoadLocale("ru", ruPath); // 俄语
                }
                LM.ApplyLocale(false);
                // 按模组界面语言把配置项标签注入当前 NML locale（设置窗口跟随 ui_language）
                RegisterConfigLocale();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[ClassicalEconomics] 注册本地化失败: {e.Message}");
            }
        }

        /// <summary>default_config.json 全部配置项 Id（用于注入设置窗口本地化）。</summary>
        private static readonly string[] AllConfigIds =
        {
            "ui_language", "unrest_enabled", "log_worldlog", "gini_threshold",
            "unrest_grace_years", "unrest_max_cities", "policy_enabled", "cycle_enabled",
            "cycle_gini_high", "cycle_gini_low", "cycle_gini_periods", "boom_stimulus_ratio",
            "boom_bubble_factor", "bubble_threshold", "boom_max_duration", "recession_max_duration",
            "depression_max_duration", "recovery_max_duration", "survival_line", "war_plunder_ratio",
            "war_waste_ratio", "revolution_delay_years", "revolution_kill_ratio",
            "uprising_gini_threshold", "uprising_delay_years", "kill_rich_ratio", "kill_rich_redist_ratio",
            "population_enabled", "population_overcrowd", "era_enabled",
            "era_duration_years", "collapse_drop_ratio", "collapse_duration_years",
            "flourish_military_ratio", "flourish_periods", "labor_enabled", "labor_wage_base",
            "real_time_refresh", "real_time_interval", "real_time_refresh_threshold", "real_time_refresh_budget", "money_velocity", "inflation_bubble_boost",
            "disaster_enabled", "disaster_wealth_loss", "disaster_mine_bonus", "banking_enabled",
            "credit_rate", "default_rate_depression", "crisis_contagion_threshold",
            "spending_cap_per_year", "banking_default_cap_per_year", "banking_contagion_cap_per_year",
            "frame_budget_ms", "cycle_window_ms",
            "inheritance_scan_per_frame",
            "perf_diagnostics_enabled", "cycle_alloc_budget",
            "memory_cleanup_enabled", "memory_cleanup_force_gc", "memory_cleanup_interval_seconds",
            "memory_cleanup_notify_enabled",
            "nation_play_enabled", "treasury_income_ratio", "policy_slots",
            "nation_claim_hotkey", "ui_scale",
        };

        /// <summary>
        /// 按模组界面语言（ui_language）把所有配置项标签注入 NML 当前 locale 字典，
        /// 使设置窗口分组/配置项名称/描述显示对应语言（zh/zh_tw/en/ru）。
        /// AddToCurrentLocale 写入的是 NML 当前 locale，与游戏语言码无关，
        /// 因此无论游戏语言如何设置，设置窗口都跟随模组界面语言。
        /// 语言切换后需重新调用。
        /// </summary>
        public static void RegisterConfigLocale()
        {
            try
            {
                // 分组名
                LM.AddToCurrentLocale(GroupId, LocalizationService.Get(GroupId));
                // 全部配置项（Id + Description）
                foreach (var id in AllConfigIds)
                {
                    LM.AddToCurrentLocale(id, LocalizationService.Get(id));
                    LM.AddToCurrentLocale(id + " Description", LocalizationService.Get(id + " Description"));
                }
                LM.ApplyLocale(false);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[ClassicalEconomics] 注入配置项本地化失败: {e.Message}");
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
                if (group.TryGetValue("ui_language", out var lang)) u.Language = NormalizeLanguage(lang.TextVal);
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
                // M6：批量导入同样保持 high > low 不变量（与 OnGiniHigh/LowChanged 单个回调路径一致），
                // 防止配置异常值（含 NaN/Infinity 被拒绝后走 fallback）破坏周期状态机
                if (u.CycleGiniHigh <= u.CycleGiniLow)
                {
                    u.CycleGiniHigh = UnityEngine.Mathf.Clamp(u.CycleGiniLow + 0.05f, 0.3f, 0.9f);
                    if (u.CycleGiniHigh > 0.9f) u.CycleGiniLow = u.CycleGiniHigh - 0.05f;
                }
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
                // 地理贸易网络（城市为节点 / 王国为聚合层）
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
                // 贸易军力（顺差/逆差 → 国民战斗加成）
                // 劳动分工
                if (group.TryGetValue("labor_enabled", out var le))       u.LaborEnabled = le.BoolVal;
                if (group.TryGetValue("labor_wage_base", out var lwb))    u.LaborWageBase = ParseFloat(lwb.TextVal, u.LaborWageBase, 0f, 5f);
                // 实时数据刷新
                if (group.TryGetValue("real_time_refresh", out var rtr))    u.RealTimeRefresh = rtr.BoolVal;
                if (group.TryGetValue("real_time_interval", out var rti))   u.RealTimeInterval = ParseFloat(rti.TextVal, u.RealTimeInterval, 1f, 60f);
                if (group.TryGetValue("real_time_refresh_threshold", out var rtt)) u.RealTimeRefreshThreshold = ParseInt(rtt.TextVal, u.RealTimeRefreshThreshold, 100, 100000);
                if (group.TryGetValue("real_time_refresh_budget", out var rtb))    u.RealTimeRefreshBudget = ParseInt(rtb.TextVal, u.RealTimeRefreshBudget, 100, 100000);
                // 货币供给与价格指数（CPI）
                if (group.TryGetValue("money_velocity", out var mv))         u.MoneyVelocity = ParseFloat(mv.TextVal, u.MoneyVelocity, 0.1f, 2f);
                if (group.TryGetValue("inflation_bubble_boost", out var ibb)) u.InflationBubbleBoost = ParseFloat(ibb.TextVal, u.InflationBubbleBoost, 0f, 0.5f);
                // 灾害经济冲击
                if (group.TryGetValue("disaster_enabled", out var den))        u.DisasterEnabled = den.BoolVal;
                if (group.TryGetValue("disaster_wealth_loss", out var dwl))     u.DisasterWealthLoss = ParseFloat(dwl.TextVal, u.DisasterWealthLoss, 0f, 0.8f);
                if (group.TryGetValue("disaster_mine_bonus", out var dmb))      u.DisasterMineBonus = ParseFloat(dmb.TextVal, u.DisasterMineBonus, 0f, 1f);
                // 银行信贷与危机传染
                if (group.TryGetValue("banking_enabled", out var ben))          u.BankingEnabled = ben.BoolVal;
                if (group.TryGetValue("credit_rate", out var cr))                 u.CreditRate = ParseFloat(cr.TextVal, u.CreditRate, 0.01f, 0.5f);
                if (group.TryGetValue("default_rate_depression", out var drd)) u.DefaultRateDepression = ParseFloat(drd.TextVal, u.DefaultRateDepression, 0.1f, 0.8f);
                if (group.TryGetValue("crisis_contagion_threshold", out var cct)) u.CrisisContagionThreshold = ParseFloat(cct.TextVal, u.CrisisContagionThreshold, 0.05f, 0.5f);
                // 年度操作上限（性能保护）
                if (group.TryGetValue("spending_cap_per_year", out var scp))          u.SpendingCapPerYear = ParseInt(scp.TextVal, u.SpendingCapPerYear, 1, 100000);
                if (group.TryGetValue("banking_default_cap_per_year", out var bdc))   u.BankingDefaultCapPerYear = ParseInt(bdc.TextVal, u.BankingDefaultCapPerYear, 1, 100000);
                if (group.TryGetValue("banking_contagion_cap_per_year", out var bcc)) u.BankingContagionCapPerYear = ParseInt(bcc.TextVal, u.BankingContagionCapPerYear, 1, 100000);
                if (group.TryGetValue("inheritance_scan_per_frame", out var ispf)) u.InheritanceScanPerFrame = ParseInt(ispf.TextVal, u.InheritanceScanPerFrame, 1, 100000);
                // 年度收尾分帧（计划任务 7）
                if (group.TryGetValue("frame_budget_ms", out var fbm)) u.FrameBudgetMs = ParseInt(fbm.TextVal, u.FrameBudgetMs, 1, 100);
                if (group.TryGetValue("cycle_window_ms", out var cwm)) u.CycleWindowMs = ParseInt(cwm.TextVal, u.CycleWindowMs, 100, 10000);
                // 年度收尾性能诊断（计划任务 1）
                if (group.TryGetValue("perf_diagnostics_enabled", out var pde)) u.PerfDiagnosticsEnabled = pde.BoolVal;
                if (group.TryGetValue("cycle_alloc_budget", out var cab)) u.CycleAllocBudget = ParseInt(cab.TextVal, u.CycleAllocBudget, 1, 1048576);
                // 自动内存清理（MemoryCleanupEngine）
                if (group.TryGetValue("memory_cleanup_enabled", out var mce)) u.MemoryCleanupEnabled = mce.BoolVal;
                if (group.TryGetValue("memory_cleanup_force_gc", out var mcfg)) u.MemoryCleanupForceGc = mcfg.BoolVal;
                if (group.TryGetValue("memory_cleanup_interval_seconds", out var mci)) u.MemoryCleanupIntervalSeconds = ParseInt(mci.TextVal, u.MemoryCleanupIntervalSeconds, 5, 300);
                if (group.TryGetValue("memory_cleanup_notify_enabled", out var mcn)) u.MemoryCleanupNotifyEnabled = mcn.BoolVal;
                // 中央银行家（NationEngine）
                if (group.TryGetValue("nation_play_enabled", out var npe)) u.NationPlayEnabled = npe.BoolVal;
                if (group.TryGetValue("treasury_income_ratio", out var tir)) u.TreasuryIncomeRatio = ParseInt(tir.TextVal, u.TreasuryIncomeRatio, 1, 20);
                if (group.TryGetValue("policy_slots", out var psl)) u.PolicySlots = ParseInt(psl.TextVal, u.PolicySlots, 1, 5);
                if (group.TryGetValue("nation_claim_hotkey", out var nch) && !string.IsNullOrWhiteSpace(nch.TextVal)) u.NationClaimHotkey = nch.TextVal.Trim().ToUpperInvariant();
                // UI 缩放（内阁字体/按钮）
                if (group.TryGetValue("ui_scale", out var us)) u.UiScale = ParseFloat(us.TextVal, u.UiScale, 0.8f, 1.6f);
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
            // M6：float.TryParse 对 "NaN"/"Infinity" 会成功，且 Mathf.Clamp(NaN,...) 原样返回 NaN，
            // 会污染所有下游数值计算；此处显式拒绝 NaN/Infinity，回退 fallback。
            if (float.TryParse(text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out v)
                && !float.IsNaN(v) && !float.IsInfinity(v))
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

        /// <summary>语言规范化：zh/zh_tw/en/ru，非法值回退 zh。</summary>
        public static string NormalizeLanguage(string lang)
        {
            switch (lang)
            {
                case "zh_tw": case "en": case "ru": return lang;
                default: return "zh"; // 空/未知统一回退简中
            }
        }

        public static void OnLanguageChanged(string pValue)
        {
            UnrestConfig.Instance.Language = NormalizeLanguage(pValue);
            // 语言切换后：刷新设置窗口标签 + 悬浮窗/内阁标题与静态文本 + 重新注入按钮 tooltip（4 语言）
            try { RegisterConfigLocale(); } catch (System.Exception) { }
            try { EconomyHUD.Instance?.RefreshAllTexts(); } catch (System.Exception) { }
            try { EventWindow.Instance?.RefreshAllTexts(); } catch (System.Exception) { }
            try { RichListWindow.Instance?.RefreshAllTexts(); } catch (System.Exception) { }
            try { CabinetWindow.Instance?.RefreshAllTexts(); } catch (System.Exception) { }
            try { EconomyUI.ReapplyTooltips(); } catch (System.Exception) { }
        }

        /// <summary>兼容旧配置（旧版 SWITCH 开关回调，保留以防旧 config.json 残留）。</summary>
        public static void OnLanguageChanged(bool pValue)
        {
            OnLanguageChanged(pValue ? "zh" : "en");
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
            var u = UnrestConfig.Instance;
            float v = ParseFloat(pValue, u.CycleGiniHigh, 0.3f, 0.9f);
            if (v <= u.CycleGiniLow) v = u.CycleGiniLow + 0.05f; // 不变量：high > low
            u.CycleGiniHigh = v;
        }

        public static void OnGiniLowChanged(string pValue)
        {
            var u = UnrestConfig.Instance;
            float v = ParseFloat(pValue, u.CycleGiniLow, 0.2f, 0.7f);
            if (v >= u.CycleGiniHigh) v = u.CycleGiniHigh - 0.05f; // 不变量：low < high
            u.CycleGiniLow = v;
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

        // ===== 地理贸易网络回调 =====











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

        // ===== 贸易军力回调 =====




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

        public static void OnRealTimeThresholdChanged(string pValue)
        {
            UnrestConfig.Instance.RealTimeRefreshThreshold = ParseInt(pValue, UnrestConfig.Instance.RealTimeRefreshThreshold, 100, 100000);
        }

        public static void OnRealTimeBudgetChanged(string pValue)
        {
            UnrestConfig.Instance.RealTimeRefreshBudget = ParseInt(pValue, UnrestConfig.Instance.RealTimeRefreshBudget, 100, 100000);
        }

        // ===== 货币供给与价格指数（CPI）回调 =====

        public static void OnMoneyVelocityChanged(string pValue)
        {
            UnrestConfig.Instance.MoneyVelocity = ParseFloat(pValue, UnrestConfig.Instance.MoneyVelocity, 0.1f, 2f);
        }

        public static void OnInflationBubbleBoostChanged(string pValue)
        {
            UnrestConfig.Instance.InflationBubbleBoost = ParseFloat(pValue, UnrestConfig.Instance.InflationBubbleBoost, 0f, 0.5f);
        }

        // ===== 灾害经济冲击（DisasterEngine）回调 =====

        public static void OnDisasterEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.DisasterEnabled = pValue;
        }

        public static void OnDisasterWealthLossChanged(string pValue)
        {
            UnrestConfig.Instance.DisasterWealthLoss = ParseFloat(pValue, UnrestConfig.Instance.DisasterWealthLoss, 0f, 0.8f);
        }

        public static void OnDisasterMineBonusChanged(string pValue)
        {
            UnrestConfig.Instance.DisasterMineBonus = ParseFloat(pValue, UnrestConfig.Instance.DisasterMineBonus, 0f, 1f);
        }

        // ===== 银行信贷与危机传染（BankingEngine）回调 =====

        public static void OnBankingEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.BankingEnabled = pValue;
        }

        public static void OnCreditRateChanged(string pValue)
        {
            UnrestConfig.Instance.CreditRate = ParseFloat(pValue, UnrestConfig.Instance.CreditRate, 0.01f, 0.5f);
        }

        public static void OnDefaultRateDepressionChanged(string pValue)
        {
            UnrestConfig.Instance.DefaultRateDepression = ParseFloat(pValue, UnrestConfig.Instance.DefaultRateDepression, 0.1f, 0.8f);
        }

        public static void OnCrisisContagionThresholdChanged(string pValue)
        {
            UnrestConfig.Instance.CrisisContagionThreshold = ParseFloat(pValue, UnrestConfig.Instance.CrisisContagionThreshold, 0.05f, 0.5f);
        }

        // ===== 年度操作上限（性能保护）回调 =====

        public static void OnSpendingCapPerYearChanged(string pValue)
        {
            UnrestConfig.Instance.SpendingCapPerYear = ParseInt(pValue, UnrestConfig.Instance.SpendingCapPerYear, 1, 100000);
        }

        public static void OnBankingDefaultCapPerYearChanged(string pValue)
        {
            UnrestConfig.Instance.BankingDefaultCapPerYear = ParseInt(pValue, UnrestConfig.Instance.BankingDefaultCapPerYear, 1, 100000);
        }

        public static void OnBankingContagionCapPerYearChanged(string pValue)
        {
            UnrestConfig.Instance.BankingContagionCapPerYear = ParseInt(pValue, UnrestConfig.Instance.BankingContagionCapPerYear, 1, 100000);
        }

        public static void OnInheritanceScanPerFrameChanged(string pValue)
        {
            UnrestConfig.Instance.InheritanceScanPerFrame = ParseInt(pValue, UnrestConfig.Instance.InheritanceScanPerFrame, 1, 100000);
        }

        // ===== 年度收尾分帧（计划任务 7）回调 =====

        public static void OnFrameBudgetChanged(string pValue)
        {
            UnrestConfig.Instance.FrameBudgetMs = ParseInt(pValue, UnrestConfig.Instance.FrameBudgetMs, 1, 100);
        }

        public static void OnCycleWindowChanged(string pValue)
        {
            UnrestConfig.Instance.CycleWindowMs = ParseInt(pValue, UnrestConfig.Instance.CycleWindowMs, 100, 10000);
        }

        // ===== 年度收尾性能诊断（计划任务 1）回调 =====

        public static void OnPerfDiagnosticsEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.PerfDiagnosticsEnabled = pValue;
        }

        public static void OnCycleAllocBudgetChanged(string pValue)
        {
            UnrestConfig.Instance.CycleAllocBudget = ParseInt(pValue, UnrestConfig.Instance.CycleAllocBudget, 1, 1048576);
        }

        // ===== 自动内存清理（MemoryCleanupEngine）回调 =====

        public static void OnMemoryCleanupEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.MemoryCleanupEnabled = pValue;
        }

        public static void OnMemoryCleanupForceGcChanged(bool pValue)
        {
            UnrestConfig.Instance.MemoryCleanupForceGc = pValue;
        }

        public static void OnMemoryCleanupIntervalChanged(string pValue)
        {
            UnrestConfig.Instance.MemoryCleanupIntervalSeconds = ParseInt(pValue, UnrestConfig.Instance.MemoryCleanupIntervalSeconds, 5, 300);
        }

        public static void OnMemoryCleanupNotifyEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.MemoryCleanupNotifyEnabled = pValue;
        }

        // ===== 中央银行家（NationEngine）回调 =====

        public static void OnNationPlayEnabledChanged(bool pValue)
        {
            UnrestConfig.Instance.NationPlayEnabled = pValue;
        }

        public static void OnTreasuryIncomeRatioChanged(string pValue)
        {
            UnrestConfig.Instance.TreasuryIncomeRatio = ParseInt(pValue, UnrestConfig.Instance.TreasuryIncomeRatio, 1, 20);
        }

        public static void OnPolicySlotsChanged(string pValue)
        {
            UnrestConfig.Instance.PolicySlots = ParseInt(pValue, UnrestConfig.Instance.PolicySlots, 1, 5);
        }


        public static void OnNationClaimHotkeyChanged(string pValue)
        {
            var v = (pValue ?? "").Trim();
            UnrestConfig.Instance.NationClaimHotkey = string.IsNullOrEmpty(v) ? "" : v.ToUpperInvariant();
        }

        /// <summary>UI 缩放：改后即时重建可见的内阁面板与原版窗口法典摘要卡。</summary>
        public static void OnUiScaleChanged(string pValue)
        {
            UnrestConfig.Instance.UiScale = ParseFloat(pValue, UnrestConfig.Instance.UiScale, 0.8f, 1.6f);
            try
            {
                var cab = CabinetWindow.Instance;
                if (cab != null && cab.IsVisible) cab.RebuildPanelFromScale();
            }
            catch (System.Exception) { }
            try { EconomyMod.Core.KingdomWindowIntegration.RefreshSummaryScale(); }
            catch (System.Exception) { }
        }
    }
}
