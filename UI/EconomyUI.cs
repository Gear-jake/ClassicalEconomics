using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;
using UnityEngine.UI;
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
        // 结算期按钮引用与状态（年度收尾在途时禁用，完成后恢复；由 EconomyTickRunner.Update 每帧驱动）
        private static PowerButton _btnCollect;
        private static PowerButton _btnCyclePhase;
        private static bool _settling;

        /// <summary>
        /// 判断当前界面语言是否为中文系（简/繁）。
        /// 跟随模组设置"界面语言"(ui_language) 与游戏语言解耦。
        /// </summary>
        private static bool IsChinese()
        {
            return Services.LocalizationService.IsChinese;
        }

        /// <summary>已注册的按钮 tooltip（标题/描述取自 Locales/*.json 的 id 与 id+"_description" 键，语言切换后重新注入）。</summary>
        private class TooltipInfo
        {
            public PowerButton Btn;
            public string Id;
        }

        private static readonly System.Collections.Generic.List<TooltipInfo> _tooltips =
            new System.Collections.Generic.List<TooltipInfo>();

        /// <summary>
        /// 注册并注入按钮 tooltip；文案来源为 Locales/*.json（键 id 与 id_description），
        /// 供 <see cref="ReapplyTooltips"/> 在语言切换后重新注入。
        /// </summary>
        private static void RegisterTooltip(PowerButton btn, string id)
        {
            if (btn == null) return;
            _tooltips.Add(new TooltipInfo { Btn = btn, Id = id });
            SetTooltip(btn, id);
        }

        /// <summary>
        /// 按当前设置语言重新注入全部按钮 tooltip 与 Tab 名称（设置"使用中文界面"切换时调用）。
        /// LM.AddToCurrentLocale 覆盖同名 key，重复注入即可切换语言。
        /// </summary>
        public static void ReapplyTooltips()
        {
            RegisterTabLocale();
            EventWindow.Instance?.InvalidateContent();
            foreach (var t in _tooltips)
            {
                if (t.Btn == null) continue;
                SetTooltip(t.Btn, t.Id);
            }
        }

        // ===== Tab 名称/描述本地化 =====
        // 工具栏 Tab 的名称/描述是 vanilla LocalizedTextManager 按 key 直接查找的
        // （getText → _localized_text.ContainsKey），缺失时打印 "missing text" 日志，
        // 并可能在工具提示路径引发空键异常。故必须在运行时用 LM.AddToCurrentLocale 注入。
        private const string TabNameKey = "Classical Economics";
        private const string TabDescKey = "Classical Economics Tab";
        // Tab 名称/描述在 Locales/*.json 中的键（LM 查找键与其分离，避免键名带空格进 json）
        private const string TabNameLocaleKey = "tab_economy_name";
        private const string TabDescLocaleKey = "tab_economy_desc";

        private static void RegisterTabLocale()
        {
            LM.AddToCurrentLocale(TabNameKey, Services.LocalizationService.Get(TabNameLocaleKey));
            LM.AddToCurrentLocale(TabDescKey, Services.LocalizationService.Get(TabDescLocaleKey));
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
        /// 文案来源：Locales/*.json 的 id 与 id_description 键（Test-LocalizationCoverage 门禁校验）。
        /// </summary>
        private static void SetTooltip(PowerButton btn, string id)
        {
            if (btn == null) return;
            string title = Services.LocalizationService.Get(id);
            string desc = Services.LocalizationService.Get(id + "_description");
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
            // 创建内阁面板（中央银行家：国家认领/金库/政策/法令/记录）
            CabinetWindow.Create();
            // 创建「选择扮演国家」选择器（Tab 国家按钮打开：列表点选即认领，见 NationPickWindow）
            NationPickWindow.Create();
            // 创建抉择事件小窗（非模态，快照尾按需弹出；S4-1：不在 RefreshOverview 里刷新）
            EventChoiceWindow.Create();


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
            RegisterTooltip(btn, "economy_toggle");
            PowerButtonCreator.AddButtonToTab(btn, _tab, null);

            // 【W1.5】原「干预王国」按钮（打开国家选择列表，再点国家内的小按钮）
            // 拆成两个**真正的权能**：选中后直接点击地图上的王国即可生效。
            // GodPower 资产在 Core/KingdomPowers.Register() 里注册，这里只创建标签按钮。
            KingdomPowers.Register();

            var btnIncite = PowerButtonCreator.CreateGodPowerButton(
                KingdomPowers.InciteId, IconLoader.Get("flame"));
            RegisterTooltip(btnIncite, KingdomPowers.InciteId);
            PowerButtonCreator.AddButtonToTab(btnIncite, _tab, null);

            var btnSuppress = PowerButtonCreator.CreateGodPowerButton(
                KingdomPowers.SuppressId, IconLoader.Get("shield"));
            RegisterTooltip(btnSuppress, KingdomPowers.SuppressId);
            PowerButtonCreator.AddButtonToTab(btnSuppress, _tab, null);

            // 创建"立即采集"工具按钮（带刷新图标）：手动执行一次数据采集与重算
            var btnCollect = PowerButtonCreator.CreateSimpleButton(
                "economy_collect",
                () =>
                {
                    EconomyModMain.ManualCollect(); // 采集 + 同步计算发布 + 富豪税 + 刷新
                },
                IconLoader.Get("collect"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnCollect, "economy_collect");
            PowerButtonCreator.AddButtonToTab(btnCollect, _tab, null);
            _btnCollect = btnCollect;

            // 创建"清除历史"工具按钮（带垃圾桶图标）：清空历史快照（内存 + history.json）
            var btnClear = PowerButtonCreator.CreateSimpleButton(
                "economy_clear",
                () =>
                {
                    HistoryService.ClearHistory();
                    RefreshOverview(true);
                },
                IconLoader.Get("trash"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnClear, "economy_clear");
            PowerButtonCreator.AddButtonToTab(btnClear, _tab, null);

            // 创建"全球富豪榜"工具按钮（带皇冠图标，与悬浮窗按钮并列），
            // 点击弹出工具框显示财富前 10 的存活开智生物
            var btnRich = PowerButtonCreator.CreateSimpleButton(
                "economy_rich",
                () => RichListWindow.Instance?.Toggle(),
                IconLoader.Get("crown"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnRich, "economy_rich");
            PowerButtonCreator.AddButtonToTab(btnRich, _tab, null);

            // 创建"选择扮演国家"按钮（带旗帜图标）：打开国家列表选择器，点一行即认领该国。
            // 关掉 nation_play_enabled 时不挂这个按钮（玩法关闭时不留死入口）。
            if (UnrestConfig.Instance == null || UnrestConfig.Instance.NationPlayEnabled)
            {
                var btnPick = PowerButtonCreator.CreateSimpleButton(
                    "economy_nation_pick",
                    () => NationPickWindow.Instance?.Toggle(),
                    IconLoader.Get("nation"),
                    _tab.transform, Vector2.zero);
                RegisterTooltip(btnPick, "economy_nation_pick");
                PowerButtonCreator.AddButtonToTab(btnPick, _tab, null);
            }

            // 创建"经济事件"工具按钮（带铃铛图标）：切换事件流悬浮窗显隐
            var btnEvents = PowerButtonCreator.CreateSimpleButton(
                "economy_events",
                () => EventWindow.Instance?.Toggle(),
                IconLoader.Get("bell"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnEvents, "economy_events");
            PowerButtonCreator.AddButtonToTab(btnEvents, _tab, null);

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
                    RefreshOverview(true);
                },
                IconLoader.Get("phase_boom"),
                _tab.transform, Vector2.zero);
            RegisterTooltip(btnCyclePhase, "economy_cycle_phase");
            PowerButtonCreator.AddButtonToTab(btnCyclePhase, _tab, null);
            _btnCyclePhase = btnCyclePhase;

            Debug.Log("[ClassicalEconomics] EconomyUI 初始化完成（悬浮HUD + 煽动/镇压权能 + 富豪榜工具 + 事件流窗口 + 阶段切换）");
        }

        /// <summary>
        /// 结算期 UI 状态：年度收尾在途时禁用"立即采集/手动切阶段"按钮并刷新面板标记，
        /// 完成后恢复。由 EconomyTickRunner.Update 每帧驱动（既有逐帧路径）。
        /// </summary>
        public static void ApplySettlingState(bool settling)
        {
            if (_settling == settling) return;
            _settling = settling;
            SetButtonInteractable(_btnCollect, settling);
            SetButtonInteractable(_btnCyclePhase, settling);
            if (EconomyHUD.Instance != null && EconomyHUD.Instance.IsVisible)
            {
                EconomyHUD.Instance.RefreshCurrentSection();
            }
        }

        private static void SetButtonInteractable(PowerButton btn, bool settling)
        {
            if (btn == null) return;
            var b = btn.GetComponent<Button>();
            if (b == null) return;
            if (settling) b.interactable = false;
            else b.interactable = true;
        }

        /// <summary>
        /// 刷新概览数据（采集周期或手动采集后调用）。
        /// 富豪榜悬浮窗若处于打开状态，同步刷新其数据。
        /// refreshCabinet=true 时内阁面板也整页重建：年度快照/手动采集/语言切换等低频路径使用；
        /// 每秒实时刷新传 false，避免整页重建（含 GDP 图）造成 GC 抖动。
        /// </summary>
        public static void RefreshOverview(bool refreshCabinet = false)
        {
            if (EconomyHUD.Instance != null && EconomyHUD.Instance.IsVisible)
            {
                EconomyHUD.Instance.RefreshCurrentSection();
            }
            if (RichListWindow.Instance != null && RichListWindow.Instance.IsVisible)
            {
                RichListWindow.Instance.RefreshNow();
            }
            // 国家选择器：只在可见时重建（隐藏零调用），列表随王国兴亡变化
            if (NationPickWindow.Instance != null && NationPickWindow.Instance.IsVisible)
            {
                NationPickWindow.Instance.RefreshNow();
            }
            // 事件窗不在此刷新（S4-1）：隐藏零调用；打开时由打开动作渲染一次，
            // 之后仅每个游戏年（WriteCycleSnapshot→OnYearBoundary）或用户操作重建。
            if (refreshCabinet && CabinetWindow.Instance != null && CabinetWindow.Instance.IsVisible)
            {
                CabinetWindow.Instance.RefreshNow();
            }
        }

        /// <summary>返回主菜单时释放所有窗口动态内容及其中的按钮委托。</summary>
        public static void OnWorldUnavailable()
        {
            EconomyHUD.Instance?.OnWorldUnavailable();
            RichListWindow.Instance?.OnWorldUnavailable();
            EventWindow.Instance?.OnWorldUnavailable();
            EventChoiceWindow.Instance?.OnWorldUnavailable();
            CabinetWindow.Instance?.OnWorldUnavailable();
            NationPickWindow.Instance?.OnWorldUnavailable();
        }
    }
}
