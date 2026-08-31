using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;
using EconomyMod.UI;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using UnityEngine;

namespace EconomyMod
{
    public class EconomyModMain : BasicMod<EconomyModMain>, IReloadable
    {
        protected override void OnModLoad()
        {
            Debug.Log("[ClassicalEconomics] Economy Mod 已加载");
            // 配置全部由 NML 模组设置管理：先注册本地化（设置窗口标签）+ 同步初始值，再初始化 UI
            Services.EconomyConfigCallbacks.RegisterLocales();
            Services.EconomyConfigCallbacks.SyncFromModConfig();

            // 注册时代事件国民特质（盛世/复兴/强盛期/经济崩溃），供 EraEngine 国民加成使用
            RegisterEraTraits();

            if (gameObject.GetComponent<EconomyTickRunner>() == null)
                gameObject.AddComponent<EconomyTickRunner>();
            // 每次打开游戏都清空历史，图表从本局从头绘制（避免残留上局数据）
            HistoryService.ClearHistory();
            EconomyUI.Initialize();
        }

        /// <summary>
        /// NML 热重载：模组列表点击重载按钮后调用（需 NML editor 模式）。
        /// 函数热更新已完成，此处重置经济状态 + 重新同步配置，使修改立即生效。
        /// </summary>
        [Hotfixable]
        public void Reload()
        {
            Debug.Log("[ClassicalEconomics] === 开始热重载 ===");
            // 重置全部经济引擎状态（新代码从干净状态运行）
            ResetAllEngines(full: false);
            // 重新同步配置（可能修改了默认值）
            Services.EconomyConfigCallbacks.SyncFromModConfig();
            // 刷新 UI
            EconomyUI.RefreshOverview();
            Debug.Log("[ClassicalEconomics] === 热重载完成 ===");
        }

        /// <summary>
        /// 重置全部经济引擎状态（热重载 / 新地图共用序列）。
        /// full=true 时额外执行新地图专属清理：清 biome 缓存。
        /// </summary>
        private static void ResetAllEngines(bool full)
        {
            TradeSimulationWorker.Reset();
            EconomyEngine.ResetCycle();
            EconomyCycleModulator.Reset();
            SocialCrisisEngine.Reset();
            EraEngine.Reset();
            TradePowerEngine.Reset();
            UnrestEngine.Reset();   // M7：清空震荡状态与收复战争跟踪
            PolicyEngine.Reset();   // M7：清空改革冷却
            KingdomMonitorEngine.Reset();
            InheritanceEngine.Reset();
            DisasterEngine.Reset();
            BankingEngine.Reset();
            PopulationEngine.Reset();
            SpendingEngine.Reset();
            NationEngine.Reset(); // 中央银行家：清空国家绑定/金库/政策（新地图从零开始）
            HistoryService.ClearHistory();
            EventStreamService.Clear();
            if (full)
            {
BiomeEconomy.ClearCache();
            }
        }

        /// <summary>
        /// 年度收尾完成钩子（AnnualPipeline 最后一个阶段调用）：
        /// 全部经济阶段完成后才执行周期日志 + 写快照 + 刷新 UI（快照时机不变量）。
        /// </summary>
        public static void WriteCycleSnapshot(int year)
        {
            // 周期/王国检测日志：默认关闭，需在配置页开启 LogToWorldLog
            bool logOut = UnrestConfig.Instance.LogToWorldLog;
            if (logOut)
            {
                Debug.Log($"[ClassicalEconomics] 周期#{EconomyEngine.CycleIndex} " +
                          $"财富={EconomyEngine.GlobalGDP:F0} " +
                          $"人均={EconomyEngine.AvgWealth:F2} " +
                          $"Actor={EconomyEngine.AliveActorCount} " +
                          $"贫富差距={EconomyEngine.GiniCoefficient:F2} " +
                          $"贸易额={EconomyEngine.TotalTradeVolume:F0}");

                var topKingdoms = EconomyEngine.TopKingdoms(3);
                foreach (var k in topKingdoms)
                {
                    Debug.Log($"[ClassicalEconomics]   王国<{k.KingdomName}> 财富={k.GDP} 人均={k.AvgWealth:F2} 贫富差距={k.GiniCoefficient:F2}");
                }
            }

            var snapshot = new EconomySnapshot
            {
                CycleIndex = EconomyEngine.CycleIndex,
                GameYear = year,
                GlobalGDP = (long)EconomyEngine.GlobalGDP,
                AvgWealth = EconomyEngine.AvgWealth,
                AliveActorCount = EconomyEngine.AliveActorCount,
                GiniCoefficient = EconomyEngine.GiniCoefficient,
                Phase = (int)EconomyCycleModulator.CurrentPhase,
                TotalProduction = EconomyEngine.TotalProduction,
                PriceIndex = EconomyCycleModulator.CurrentCPI
            };
            snapshot.Kingdoms = new List<KingdomStats>(EconomyEngine.KingdomStats.Values);
            // 贸易净额排名（v0.13）：直接引用后台已聚合的城市/国家净额列表（本周期只读，零拷贝）
            snapshot.TotalExport = EconomyEngine.TotalTradeVolume;
            var last = TradeSimulationWorker.LastResult;
            snapshot.CityBalances = CopyTopBalances(last != null ? last.CityBalances : null, 40);
            snapshot.KingdomBalances = CopyTopBalances(last != null ? last.KingdomBalances : null, 40);
            HistoryService.AppendSnapshot(snapshot);
            EconomyUI.RefreshOverview();
        }

        private static List<TradeBalance> CopyTopBalances(List<TradeBalance> source, int limit)
        {
            int count = source == null ? 0 : System.Math.Min(source.Count, limit);
            var copy = new List<TradeBalance>(count);
            for (int i = 0; i < count; i++) copy.Add(source[i]);
            return copy;
        }

        // ===== 时代事件国民特质注册（EraEngine 国民加成用，替换原 cultural_awakening）=====
        // 注册方式参照社区标准模组（如 avbsMedalTraits）：
        // 1) 先注册特质分组 trait_group；2) 每个特质配独立图标（GameResources/ui/Icons/*.png，
        //    path_icon 指向真实存在的资源）；3) base_stats 用 new BaseStats() + 索引器（等价 set()）；
        // 4) 只 AssetManager.traits.add，不调用 unlock()（避免进度系统副作用）。

        private static void RegisterEraTraits()
        {
            try
            {
                AssetManager.trait_groups.add(new ActorTraitGroupAsset
                {
                    id = "ClassicalEra",
                    name = "trait_group_classical_era",
                    color = "#D4AF37"
                });
                // 分组名/特质名/描述都要注册进游戏本地化表（LocalizedTextManager），
                // 否则游戏按 id 显示占位符。add 写入当前语言表，随游戏语言注册对应文本。
                // 注意：LocalizedTextManager.instance 可能尚未初始化（模组加载早期），
                // 必须判空 + 整体 try-catch，否则 NRE 会直接炸掉 OnModLoad 导致游戏崩溃。
                if (LocalizedTextManager.instance != null)
                {
                    LocalizedTextManager.add("trait_group_classical_era",
                        IsChinese ? "古典时代" : "Classical Era", true, "ClassicalEconomics", false);
                }

                if (IsChinese)
                {
                    RegisterTrait(Core.EraEngine.ActorTraitGolden,   "ui/Icons/iconEraGolden",    30f, 0f, 0f, 10, "盛世", "进入盛世：国民幸福 +30、生育 +10");
                    RegisterTrait(Core.EraEngine.ActorTraitRevival,  "ui/Icons/iconEraRevival",   35f, 10f, 0f, 10, "复兴", "迎来复兴：国民幸福 +35、伤害 +10、生育 +10");
                    RegisterTrait(Core.EraEngine.ActorTraitFlourish, "ui/Icons/iconEraFlourish",  20f, 5f, 5f, 0, "强盛期", "强盛期：国民幸福 +20、伤害 +5、护甲 +5");
                    RegisterTrait(Core.EraEngine.ActorTraitCollapse, "ui/Icons/iconEraCollapse",  -15f, 30f, 20f, 0, "经济崩溃", "经济崩溃：国民幸福 -15、伤害 +30、护甲 +20");
                    RegisterTrait(Core.TradePowerEngine.ActorTraitSurplus, "ui/Icons/iconEraFlourish", 0f, 20f, 10f, 0, "贸易顺差", "贸易顺差：国民伤害 +20、护甲 +10");
                    RegisterTrait(Core.TradePowerEngine.ActorTraitDeficit, "ui/Icons/iconEraCollapse", 0f, -20f, 0f, 0, "贸易逆差", "贸易逆差：国民伤害 -20");
                }
                else
                {
                    RegisterTrait(Core.EraEngine.ActorTraitGolden,   "ui/Icons/iconEraGolden",    30f, 0f, 0f, 10, "Golden Age", "Golden Age: happiness +30, birth rate +10");
                    RegisterTrait(Core.EraEngine.ActorTraitRevival,  "ui/Icons/iconEraRevival",   35f, 10f, 0f, 10, "Revival", "Revival: happiness +35, damage +10, birth rate +10");
                    RegisterTrait(Core.EraEngine.ActorTraitFlourish, "ui/Icons/iconEraFlourish",  20f, 5f, 5f, 0, "Flourishing", "Flourishing: happiness +20, damage +5, armor +5");
                    RegisterTrait(Core.EraEngine.ActorTraitCollapse, "ui/Icons/iconEraCollapse",  -15f, 30f, 20f, 0, "Economic Collapse", "Economic Collapse: happiness -15, damage +30, armor +20");
                    RegisterTrait(Core.TradePowerEngine.ActorTraitSurplus, "ui/Icons/iconEraFlourish", 0f, 20f, 10f, 0, "Trade Surplus", "Trade Surplus: damage +20, armor +10");
                    RegisterTrait(Core.TradePowerEngine.ActorTraitDeficit, "ui/Icons/iconEraCollapse", 0f, -20f, 0f, 0, "Trade Deficit", "Trade Deficit: damage -20");
                }
            }
            catch (System.Exception e)
            {
                // 注册失败仅记录日志，绝不让模组加载崩溃
                Debug.LogWarning("[ClassicalEconomics] 时代特质注册异常: " + e);
            }
        }

        /// <summary>是否中文游戏语言（WorldBox 语言 id 含 zh/cn）。</summary>
        private static bool IsChinese
        {
            get
            {
                try
                {
                    var lang = LocalizedTextManager.current_language;
                    if (lang != null && !string.IsNullOrEmpty(lang.id))
                    {
                        string id = lang.id.ToLowerInvariant();
                        return id.Contains("zh") || id.Contains("cn");
                    }
                }
                catch (System.Exception) { }
                return true; // 取不到时默认中文
            }
        }

        private static void RegisterTrait(string id, string iconPath,
            float happiness, float damage, float armor, int rateBirth,
            string displayName, string description)
        {
            try
            {
                var t = new ActorTrait
                {
                    id = id,
                    group_id = "ClassicalEra",
                    // 图标路径指向模组 GameResources/ui/Icons/ 下的真实 PNG。
                    // 必须有效：Actor 说话/展示特质时调用 trait.getSprite() →
                    // SpriteTextureLoader.getSprite(path_icon)，path_icon 为 null 会触发
                    // Dictionary 空键 ArgumentNullException，每次社交行为反复崩溃。
                    path_icon = iconPath,
                    rate_birth = rateBirth,
                    rate_inherit = 0,
                    needs_to_be_explored = false,
                    unlocked_with_achievement = false,
                    // 名称/描述走游戏本地化表：has_localized_id=true 后 getLocaleID()
                    // 返回 special_locale_id；has_description_1=true 后 getDescriptionID()
                    // 返回 special_locale_description。文本由下方 add() 注册。
                    has_localized_id = true,
                    special_locale_id = id,
                    has_description_1 = true,
                    special_locale_description = id + "_info"
                };
                if (t.base_stats == null) t.base_stats = new BaseStats();
                if (happiness != 0f) t.base_stats["happiness"] = happiness;
                if (damage != 0f) t.base_stats["damage"] = damage;
                if (armor != 0f) t.base_stats["armor"] = armor;
                // 注册名称与描述文本（replace=true 覆盖旧值；key 无空格，Underscore 不影响）
                LocalizedTextManager.add(id, displayName, true, "ClassicalEconomics", false);
                LocalizedTextManager.add(id + "_info", description, true, "ClassicalEconomics", false);
                AssetManager.traits.add(t);
                Debug.Log("[ClassicalEconomics] 时代特质已注册: " + id);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[ClassicalEconomics] 时代特质注册失败 (" + id + "): " + ex.Message);
            }
        }

        // ===== 反射访问游戏真实年份 =====
        private static System.Reflection.FieldInfo _mapStatsField;
        private static System.Reflection.MethodInfo _getYearMethod;
        private static bool _reflectionReady;

        /// <summary>
        /// 通过反射读取 MapBox.map_stats.get_year() 获取真实游戏年份（Int32）。
        /// map_stats 为 internal 字段，编译期不可见，故用反射。
        /// </summary>
        public static int GetCurrentGameYear()
        {
            if (!_reflectionReady)
            {
                try
                {
                    _mapStatsField = typeof(MapBox).GetField("map_stats",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (_mapStatsField != null)
                    {
                        _getYearMethod = _mapStatsField.FieldType.GetMethod("get_year",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    }
                    _reflectionReady = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ClassicalEconomics] 年份反射初始化失败: {e.Message}");
                    _reflectionReady = true;
                }
            }

            try
            {
                if (_mapStatsField != null && _getYearMethod != null)
                {
                    var ms = _mapStatsField.GetValue(MapBox.instance);
                    if (ms != null)
                    {
                        return (int)_getYearMethod.Invoke(ms, null);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ClassicalEconomics] 获取年份失败: {e.Message}");
            }
            return 0;
        }

        /// <summary>
        /// 手动采集（工具按钮触发）：同步计算并立即刷新，不等后台线程。
        /// </summary>
        public static void ManualCollect()
        {
            try
            {
                // 在途周期/分帧收尾存在时跳过同步计算（否则 _generation++ 会作废在途年度周期，S2 防护）
                if (!TradeSimulationWorker.IsBusy() && !AnnualPipeline.IsSettling)
                {
                    DataCollector.Collect(postCycle: false);              // 采集纯数据（含年度副作用，不投后台）
                    TradeSimulationWorker.ComputeAndConsumeSync();        // 同步计算并发布（推进周期号）
                    DataCollector.ApplyWealthTax();
                }
                EconomyUI.RefreshOverview();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 手动采集失败: " + e.Message);
            }
        }

        /// <summary>
        /// 实时轻量刷新（配置开启时按秒调用）：
        /// 跳过工资/富豪税等年度副作用（applySideEffects=false），也不推进周期号（advanceCycle=false），
        /// 只重算统计并刷新 HUD，让经济面板数据"活"起来，不影响年度周期节奏。
        /// </summary>
        public static void RealTimeRefresh()
        {
            // 熔断：较大地图时跳过实时全量重算（主线程同步 O(n log n) 基尼排序 + 全量采集）。
            // 阈值默认 2000（实测 5000 人口已出现可感卡顿，2000 更稳），并纳入配置 real_time_refresh_threshold。
            var cfg = UnrestConfig.Instance;
            var aliveList = World.world != null && World.world.units != null
                ? World.world.units.units_only_alive : null;
            if (aliveList != null && aliveList.Count >= cfg.RealTimeRefreshThreshold)
            {
                EconomyUI.RefreshOverview(); // 仅刷新UI，不重算
                return;
            }
            // 在途周期/分帧收尾存在时跳过（同步计算会作废在途任务；年度周期收尾后自会刷新 UI，S2 防护）
            if (TradeSimulationWorker.IsBusy() || AnnualPipeline.IsSettling) return;
            // 阈值下保持同步全量重算；单次处理上限由 real_time_refresh_budget 预算约束（默认 2000，阈值下不截断）
            DataCollector.Collect(applySideEffects: false, postCycle: false, maxUnits: cfg.RealTimeRefreshBudget);
            TradeSimulationWorker.ComputeAndConsumeSync(advanceCycle: false);
            EconomyUI.RefreshOverview();
        }

        /// <summary>
        /// 基于游戏年份的采集周期驱动器。
        /// 年份（map_stats.get_year()）变更时触发一次采集（主线程仅采集纯数据，
        /// 统计在后台线程计算），结果就绪后消费并执行周期收尾。
        /// </summary>
        private class EconomyTickRunner : MonoBehaviour
        {
            private int _lastCollectedYear = -1;
            private float _yearCheckTimer;   // 反射读取年份的节流计时（年份粒度为年，无需每帧）
            private float _realtimeTimer;    // 实时刷新节流计时（配置开启时按秒轻量刷新 HUD 数据）
            private bool _cyclePending;      // 后台统计进行中/待消费
            private int _pendingYear = -1;   // 提交周期对应的游戏年份，避免后台耗时跨年后错标快照
            private bool _optimeGuardChecked; // 首帧执行一次 Optime 兼容兜底安装
            private bool _worldReferencesCleared;

            private void Update()
            {
                // Optime 兼容兜底：首帧安装（此时所有模组已加载，能可靠检测到 Optime）
                if (!_optimeGuardChecked)
                {
                    _optimeGuardChecked = true;
                    Services.OptimeCompatibility.TryInstall();
                    KingdomWindowIntegration.TryInstall(); // 中央银行家：原版界面入口（手动补丁，幂等）;
                }

                // 大地图快捷键（默认 G，可配置）：鼠标悬停国家 → 认领/打开内阁（RulerBox K 键同款）
                try
                {
                    var hotkeyCfg = UnrestConfig.Instance;
                    if (hotkeyCfg == null) return;
                    string keyName = hotkeyCfg.NationClaimHotkey;
                    if (!string.IsNullOrWhiteSpace(keyName))
                    {
                        UnityEngine.KeyCode key;
                        if (System.Enum.TryParse(keyName, true, out key))
                        {
                            if (UnityEngine.Input.GetKeyDown(key)) KingdomWindowIntegration.TryHotkeyOpen();
                        }
                    }
                }
                catch (System.Exception) { }

                InheritanceEngine.Tick(Time.deltaTime);
                // 每帧维持收复战争（内部 1 秒节流）：和谈后立即重新宣战，直到收回叛乱城市
                UnrestEngine.SustainRebelWars();

                // 后台统计完成 → 消费并执行周期收尾（评估/效果/快照/UI）
                if (_cyclePending && TradeSimulationWorker.TryConsume())
                {
                    _cyclePending = false;
                    FinishCycle();
                }
                // 自愈：后台结果已就绪但 _cyclePending 未置位（历史遗留/工具按钮路径遗漏的 PostCycle），
                // 兜底置位走正常消费分支，避免 _posting 永久滞留导致年度周期停摆（S2）。
                else if (!_cyclePending && TradeSimulationWorker.HasPendingResult())
                {
                    _cyclePending = true;
                }

                // 分帧收尾推进：管线在途时每帧推进，直到全部阶段完成（快照/UI 最后才写）
                if (AnnualPipeline.IsSettling)
                {
                    AnnualPipeline.Tick();
                }
                // 自动内存清理（空闲期缩容静态 scratch/缓存；内部按配置间隔节流）
                MemoryCleanupEngine.Tick(Time.deltaTime);
                // 结算期 UI 状态：面板"结算中…"标记 + 禁用 立即采集/手动切阶段（完成后恢复）
                EconomyUI.ApplySettlingState(AnnualPipeline.IsSettling);

                // 实时数据感：配置开启且无年度周期在途时，按秒做轻量采集+同步计算+刷 HUD
                // （跳过工资/税收等年度副作用，也不推进周期号），让经济面板"活"起来
                var cfg = UnrestConfig.Instance;
                // 认领国家后自动实时刷新（中央治国的"时间实时进行"体验），开关仍可强关
                bool realtime = cfg.RealTimeRefresh || NationEngine.NationKingdomId != 0;
                if (realtime && !_cyclePending && !AnnualPipeline.IsSettling && World.world != null)
                {
                    _realtimeTimer += Time.deltaTime;
                    if (_realtimeTimer >= cfg.RealTimeInterval)
                    {
                        _realtimeTimer = 0f;
                        try { RealTimeRefresh(); }
                        catch (System.Exception e) { Debug.LogWarning("[ClassicalEconomics] 实时刷新失败: " + e.Message); }
                    }
                }

                // 年份反射调用节流：每 0.5 秒最多检查一次，避免每帧 Invoke 开销
                _yearCheckTimer += Time.deltaTime;
                if (_yearCheckTimer < 0.5f) return;
                _yearCheckTimer = 0f;

                // 无世界（主菜单/加载中）：不检测年份也不重置状态，
                // 保证"回主菜单再读档"时历史与周期状态不被误清
if (World.world == null)
                {
                    _cyclePending = false;
                    _pendingYear = -1;
                    AnnualPipeline.Abort(); // 在途分帧收尾随世界失效立即终止（避免污染下一局）
                    if (!_worldReferencesCleared)
                    {
                        _worldReferencesCleared = true;
                        DataCollector.ClearWorldReferences();
                        TradeSimulationWorker.ClearWorldReferences();
                        GameHelpers.ClearWorldReferences();
                        InheritanceEngine.ClearWorldReferences();
                        SpendingEngine.Reset();
                        EraEngine.ClearWorldReferences();
                        SocialCrisisEngine.ClearWorldReferences();
                        UnrestEngine.ClearWorldReferences();
                        TradePowerEngine.ClearWorldReferences();
                        PopulationEngine.ClearWorldReferences();
                        BankingEngine.ClearWorldReferences();
                        EventStreamService.Clear();
                        EconomyUI.OnWorldUnavailable();
                    }
                    return;
                }
                _worldReferencesCleared = false;

                int currentYear = GetCurrentGameYear();
                // 年份回退：可能是新地图/新游戏（年份归零），也可能是读档（存档年份 < 上次运行年份）
                if (currentYear < _lastCollectedYear)
                {
                    _lastCollectedYear = currentYear;
                    _cyclePending = false;
                    AnnualPipeline.Abort(); // 世界数据已失效，终止在途分帧收尾
                    TradeSimulationWorker.Reset(); // 在途后台周期无条件丢弃（世界数据已失效）
                    if (currentYear <= 1)
                    {
                        // 新地图/新游戏：年份归零，全部状态重置
                        ResetAllEngines(full: true);
                        Debug.Log("[ClassicalEconomics] 检测到新地图/新游戏，历史已清空，周期从 #1 重新开始");
                    }
                    else
                    {
                        // 读档：保留历史快照/周期/时代/动荡状态，仅重建失效引用并继续运行
                        InheritanceEngine.Reset();
                        // 读档后的王国 ID 可能属于另一存档，biome/坐标缓存按 ID 缓存必须失效
                        BiomeEconomy.ClearCache();
                        Debug.Log($"[ClassicalEconomics] 检测到读档（年份 {currentYear}），保留历史与周期状态，继续运行");
                    }
                }
                if (currentYear != _lastCollectedYear)
                {
                    if (RunOneCycle(currentYear)) _lastCollectedYear = currentYear;
                }
            }

private bool RunOneCycle(int year)
            {
                if (_cyclePending) return false; // 防御：上一周期尚未消费完成
                if (AnnualPipeline.IsSettling) return false; // 防御：上一周期仍在分帧收尾（避免新结果发布打断在途收尾）
                // Collect 内部完成采集 + 提交后台统计（主线程零计算），返回提交是否成功。
                // 提交失败（在途周期残留等）则下个检查点重试，避免 _cyclePending 被置位后
                // 永远等不到结果（周期永久卡死）。
                _cyclePending = DataCollector.Collect();
                if (_cyclePending) _pendingYear = year;
                if (!_cyclePending)
                {
                    Debug.LogWarning("[ClassicalEconomics] 周期提交失败，将在下次年份检查时重试");
                }
                return _cyclePending;
            }

            private void FinishCycle()
            {
int year = _pendingYear >= 0 ? _pendingYear : GetCurrentGameYear();
                _pendingYear = -1;
                // 启动分帧收尾管线：全部经济阶段按帧预算推进（超预算兜底削减顺序见下），
                // 全部阶段完成后才写快照/刷新 UI（快照时机不变量，S4）。
                // 超预算兜底削减顺序（年度操作上限生效点）：consumption(spending) -> banking -> other。
                // over-budget fallback reduction order: spending caps -> banking caps -> other stages
                // remain unchanged; the wealth-tax/redistribution path is never reduced (tax conservation).
                AnnualPipeline.Start(year);
            }

}
    }
}
