using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    /// <summary>
    /// 经济 Mod UI 入口：在底部工具栏创建 Tab + 切换按钮，
    /// 点击按钮切换悬浮 HUD 面板的显示/隐藏（不暂停游戏）。
    /// </summary>
    public static class EconomyUI
    {
        private static PowersTab _tab;
        private static bool _initialized;

        /// <summary>
        /// 判断当前界面语言是否为中文系（简/繁）。
        /// 跟随模组设置"界面语言"(ui_language) 与游戏语言解耦。
        /// </summary>
        private static bool IsChinese()
        {
            return Services.LocalizationService.IsChinese;
        }

        /// <summary>已注册的按钮 tooltip 定义（用于语言切换时重新注入）。</summary>
        private class TooltipInfo
        {
            public PowerButton Btn;
            public string Id;
            public string ZhTitle, ZhDesc;
            public string ZhTwTitle, ZhTwDesc;
            public string EnTitle, EnDesc;
            public string RuTitle, RuDesc;
        }

        private static readonly System.Collections.Generic.List<TooltipInfo> _tooltips =
            new System.Collections.Generic.List<TooltipInfo>();

        /// <summary>
        /// 注册并注入按钮 tooltip；记录文案，供 <see cref="ReapplyTooltips"/> 在语言切换后重新注入。
        /// 支持 4 语言：简中 / 繁中 / 英文 / 俄文。
        /// </summary>
        private static void RegisterTooltip(PowerButton btn, string id,
            string zhTitle, string zhDesc,
            string zhTwTitle, string zhTwDesc,
            string enTitle, string enDesc,
            string ruTitle, string ruDesc)
        {
            if (btn == null) return;
            _tooltips.Add(new TooltipInfo
            {
                Btn = btn, Id = id,
                ZhTitle = zhTitle, ZhDesc = zhDesc,
                ZhTwTitle = zhTwTitle, ZhTwDesc = zhTwDesc,
                EnTitle = enTitle, EnDesc = enDesc,
                RuTitle = ruTitle, RuDesc = ruDesc
            });
            SetTooltip(btn, id, zhTitle, zhDesc, zhTwTitle, zhTwDesc, enTitle, enDesc, ruTitle, ruDesc);
        }

        /// <summary>
        /// 按当前设置语言重新注入全部按钮 tooltip 与 Tab 名称（设置"使用中文界面"切换时调用）。
        /// LM.AddToCurrentLocale 覆盖同名 key，重复注入即可切换语言。
        /// </summary>
        public static void ReapplyTooltips()
        {
            RegisterTabLocale();
            foreach (var t in _tooltips)
            {
                if (t.Btn == null) continue;
                SetTooltip(t.Btn, t.Id, t.ZhTitle, t.ZhDesc, t.ZhTwTitle, t.ZhTwDesc,
                    t.EnTitle, t.EnDesc, t.RuTitle, t.RuDesc);
            }
        }

        // ===== Tab 名称/描述本地化 =====
        // 工具栏 Tab 的名称/描述是 vanilla LocalizedTextManager 按 key 直接查找的
        // （getText → _localized_text.ContainsKey），缺失时打印 "missing text" 日志，
        // 并可能在工具提示路径引发空键异常。故必须在运行时用 LM.AddToCurrentLocale 注入。
        private const string TabNameKey = "Classical Economics";
        private const string TabDescKey = "Classical Economics Tab";

        private static void RegisterTabLocale()
        {
            string lang = Services.LocalizationService.CurrentLanguage;
            string name = lang == "zh_tw" ? "古典經濟學"
                : lang == "ru" ? "Классическая экономика"
                : lang == "en" ? "Classical Economics" : "古典经济学";
            string desc = lang == "zh_tw" ? "古典經濟學工具欄"
                : lang == "ru" ? "Панель классической экономики"
                : lang == "en" ? "Classical Economics Toolbar" : "古典经济学工具栏";
            LM.AddToCurrentLocale(TabNameKey, name);
            LM.AddToCurrentLocale(TabDescKey, desc);
            LM.ApplyLocale(false);
        }

        /// <summary>
        /// 设置 PowerButton 的提示信息。
        /// 双保险：
        /// 1) 通过 LM.AddToCurrentLocale 直接注入本地化文本，确保 key 一定存在
        ///    （不依赖 locale 文件语言代码是否匹配，如 zh.json vs ch.json）；
        /// 2) 设置 TipButton.textOnClick/textOnClickDescription，
        ///    CreateSimpleButton 创建的 Library 类型按钮 godPower 为 null，
        ///    反射修改 GodPower.name 无效，必须走 TipButton。
        /// </summary>
        private static void SetTooltip(PowerButton btn, string id,
            string zhTitle, string zhDesc,
            string zhTwTitle, string zhTwDesc,
            string enTitle, string enDesc,
            string ruTitle, string ruDesc)
        {
            if (btn == null) return;
            // 按当前语言选择文案（zh/zh_tw/en/ru）
            string lang = Services.LocalizationService.CurrentLanguage;
            string title = lang == "zh_tw" ? zhTwTitle
                : lang == "ru" ? ruTitle
                : lang == "en" ? enTitle : zhTitle;
            string desc = lang == "zh_tw" ? zhTwDesc
                : lang == "ru" ? ruDesc
                : lang == "en" ? enDesc : zhDesc;
            // 直接写入当前语言的本地化字典（同时注册到 locales[language]，
            // 语言切换后 NML 的 ApplyLocale 会重新应用，保持持久）
            LM.AddToCurrentLocale(id, title);
            LM.AddToCurrentLocale(id + "_description", desc);
            // 设置 TipButton（若 prefab 未自带则运行时添加）
            var tip = btn.GetComponent<TipButton>();
            if (tip == null)
            {
                tip = btn.gameObject.AddComponent<TipButton>();
            }
            tip.textOnClick = id;
            tip.textOnClickDescription = id + "_description";
            tip.text_description_2 = "";
        }

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            _tooltips.Clear();
            // 创建悬浮 HUD（Canvas 上的非模态面板）
            EconomyHUD.Create();
            // 创建富豪榜"工具框"（点击皇冠按钮弹出的轻量弹窗）
            RichListWindow.Create();
            // 创建事件流悬浮窗（独立于经济窗口，点击铃铛按钮切换显隐）
            EventWindow.Create();
            // 创建贸易份额趋势悬浮窗（各国各城市出口份额趋势，独立窗口）
            TradeShareWindow.Create();

            // 创建底部工具栏 Tab（带金币图标）；
            // 先注入 Tab 名称/描述本地化键（vanilla LTM 按 key 查找，缺失会打印 missing text 日志）
            RegisterTabLocale();
            _tab = TabManager.CreateTab("economy", TabNameKey, TabDescKey,
                IconLoader.Get("coin"), "");

            // 创建切换 HUD 显示的按钮（带账本图标），挂到 Tab 上
            var btn = PowerButtonCreator.CreateSimpleButton(
                "economy_toggle",
                () => EconomyHUD.Instance?.Toggle(),
                IconLoader.Get("ledger"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btn, "economy_toggle",
                "经济概览", "切换经济主面板显隐",
                "經濟概覽", "切換經濟主面板顯隱",
                "Economy Overview", "Toggle the main economy panel",
                "Экономика", "Переключить главную панель экономики");
            PowerButtonCreator.AddButtonToTab(btn, _tab, null);

            // 创建"干预王国"工具按钮（合并原煽动+镇压）：打开国家选择列表，
            // 每个王国内部已有煽动(红)+镇压(蓝)两个操作按钮，无需工具栏重复
            var btnIntervene = PowerButtonCreator.CreateSimpleButton(
                "economy_intervene",
                () => EconomyHUD.Instance?.ShowKingdomPicker(),
                IconLoader.Get("flame"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnIntervene, "economy_intervene",
                "干预王国", "选择王国进行煽动或镇压",
                "干預王國", "選擇王國進行煽動或鎮壓",
                "Intervene", "Select a kingdom to incite or suppress",
                "Вмешательство", "Выберите королевство для подстрекательства или подавления");
            PowerButtonCreator.AddButtonToTab(btnIntervene, _tab, null);

            // 创建"立即采集"工具按钮（带刷新图标）：手动执行一次数据采集与重算
            var btnCollect = PowerButtonCreator.CreateSimpleButton(
                "economy_collect",
                () =>
                {
                    EconomyModMain.ManualCollect(); // 采集 + 同步计算发布 + 富豪税 + 刷新
                },
                IconLoader.Get("collect"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnCollect, "economy_collect",
                "立即采集", "手动执行一次数据采集与经济重算",
                "立即採集", "手動執行一次數據採集與經濟重算",
                "Collect Now", "Manually run data collection and recalculation",
                "Собрать", "Запустить сбор данных и пересчёт экономики");
            PowerButtonCreator.AddButtonToTab(btnCollect, _tab, null);

            // 创建"清除历史"工具按钮（带垃圾桶图标）：清空历史快照（内存 + history.json）
            var btnClear = PowerButtonCreator.CreateSimpleButton(
                "economy_clear",
                () =>
                {
                    HistoryService.ClearHistory();
                    RefreshOverview();
                },
                IconLoader.Get("trash"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnClear, "economy_clear",
                "清除历史", "清空所有历史快照数据",
                "清除歷史", "清空所有歷史快照數據",
                "Clear History", "Wipe all historical snapshot data",
                "Очистить", "Стереть все исторические данные");
            PowerButtonCreator.AddButtonToTab(btnClear, _tab, null);

            // 创建"全球富豪榜"工具按钮（带皇冠图标，与悬浮窗按钮并列），
            // 点击弹出工具框显示财富前 10 的存活开智生物
            var btnRich = PowerButtonCreator.CreateSimpleButton(
                "economy_rich",
                () => RichListWindow.Instance?.Toggle(),
                IconLoader.Get("crown"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnRich, "economy_rich",
                "全球富豪榜", "查看财富排行",
                "全球富豪榜", "查看財富排行",
                "Rich List", "View the wealthiest actors",
                "Богатейшие", "Просмотр самых богатых");
            PowerButtonCreator.AddButtonToTab(btnRich, _tab, null);

            // 创建"经济事件"工具按钮（带铃铛图标）：切换事件流悬浮窗显隐
            var btnEvents = PowerButtonCreator.CreateSimpleButton(
                "economy_events",
                () => EventWindow.Instance?.Toggle(),
                IconLoader.Get("bell"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnEvents, "economy_events",
                "经济事件", "切换事件流悬浮窗",
                "經濟事件", "切換事件流懸浮窗",
                "Economy Events", "Toggle the event stream window",
                "События", "Переключить окно событий");
            PowerButtonCreator.AddButtonToTab(btnEvents, _tab, null);

            // 创建"贸易净额"工具按钮（复用金币图标）：切换贸易净额排名悬浮窗显隐
            var btnShare = PowerButtonCreator.CreateSimpleButton(
                "economy_trade_share",
                () => TradeShareWindow.Instance?.Toggle(),
                IconLoader.Get("coin"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnShare, "economy_trade_share",
                "贸易净额", "查看各城市/国家净贸易额（出口−进口）排名",
                "貿易淨額", "查看各城市/國家淨貿易額（出口−進口）排名",
                "Trade Balance", "Net trade balance (exports − imports) by city/kingdom",
                "Торговый баланс", "Чистый баланс (экспорт − импорт) по городам/странам");
            PowerButtonCreator.AddButtonToTab(btnShare, _tab, null);

            // 创建"切换经济阶段"按钮（循环切换：繁荣→衰退→萧条→复苏→繁荣）
            // 合并原4个阶段按钮，降低工具栏认知负荷（11→8按钮）
            var btnCyclePhase = PowerButtonCreator.CreateSimpleButton(
                "economy_cycle_phase",
                () =>
                {
                    EconomyPhase next;
                    switch (EconomyCycleModulator.CurrentPhase)
                    {
                        case EconomyPhase.Boom:       next = EconomyPhase.Recession; break;
                        case EconomyPhase.Recession:  next = EconomyPhase.Depression; break;
                        case EconomyPhase.Depression: next = EconomyPhase.Recovery; break;
                        default:                      next = EconomyPhase.Boom; break;
                    }
                    EconomyCycleModulator.SetPhaseManual(next);
                    RefreshOverview();
                },
                IconLoader.Get("phase_boom"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnCyclePhase, "economy_cycle_phase",
                "切换阶段", "循环切换经济周期阶段（繁荣→衰退→萧条→复苏）",
                "切換階段", "循環切換經濟週期階段（繁榮→衰退→蕭條→復甦）",
                "Cycle Phase", "Cycle through economic phases (Boom→Recession→Depression→Recovery)",
                "Сменить фазу", "Цикл фаз экономики (Бум→Спад→Депрессия→Восстановление)");
            PowerButtonCreator.AddButtonToTab(btnCyclePhase, _tab, null);

            Debug.Log("[ClassicalEconomics] EconomyUI 初始化完成（悬浮HUD + 煽动工具 + 富豪榜工具 + 事件流窗口 + 阶段切换）");
        }

        /// <summary>
        /// 刷新概览数据（采集周期或手动采集后调用）。
        /// 富豪榜悬浮窗若处于打开状态，同步刷新其数据。
        /// </summary>
        public static void RefreshOverview()
        {
            if (EconomyHUD.Instance != null && EconomyHUD.Instance.IsVisible)
            {
                EconomyHUD.Instance.RefreshCurrentSection();
            }
            if (RichListWindow.Instance != null && RichListWindow.Instance.IsVisible)
            {
                RichListWindow.Instance.RefreshNow();
            }
            if (EventWindow.Instance != null && EventWindow.Instance.IsVisible)
            {
                EventWindow.Instance.RefreshNow();
            }
            if (TradeShareWindow.Instance != null && TradeShareWindow.Instance.IsVisible)
            {
                TradeShareWindow.Instance.RefreshNow();
            }
        }
    }
}
