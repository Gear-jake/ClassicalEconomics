using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EconomyMod.Core;
using EconomyMod.Models;

namespace EconomyMod.UI
{
    /// <summary>
    /// 「选择扮演国家」选择器悬浮窗（Tab 上的国家按钮打开）：
    /// 列出世界上所有王国（按 GDP 降序，最多 <see cref="MaxRows"/> 行），点一行即认领该国，
    /// 认领成功后直接打开内阁面板；已扮演的国家标出并改为「打开内阁」。
    ///
    /// 设计约束（与既有两个入口共用同一套规则，绝不绕过）：
    /// · 认领**只能**经 <see cref="NationEngine.Claim"/>（换国冷却 10 年、启动资金守恒、
    ///   结算法典、事件流记录都在那里面）；本窗不得直接改 `_nationKingdomId`。
    /// · 关闭 `nation_play_enabled` 时按钮不出现（见 EconomyUI），窗口自身也再兜一道。
    /// · 每帧零开销：只在打开（Toggle→RefreshNow）与语言切换时重建；
    ///   行按钮按 W4-UI 计数规范统计创建/销毁。
    /// </summary>
    public class NationPickWindow : FloatingWindow
    {
        private static NationPickWindow _instance;
        public static NationPickWindow Instance => _instance;

        private const int MaxRows = 60;

        private static readonly Color Bg        = UIStyles.PanelBg;
        private static readonly Color RowColor  = UIStyles.CardBgAlt;
        private static readonly Color MineColor = UIStyles.RowRank1;
        private static readonly Color SubColor  = UIStyles.TextSecondary;
        private static readonly Color Muted     = UIStyles.TextMuted;

        /// <summary>本窗自建的行按钮（ClearContent 只管 AddLine 的行，这些要自己回收）。</summary>
        private static readonly List<GameObject> _rows = new List<GameObject>();
        private static readonly List<Kingdom> _sortPool = new List<Kingdom>(256);

        // ===== FloatingWindow 配置 =====
        protected override string WindowName => "EconomyNationPick";
        protected override float SortingOrder => 10000;
        protected override string TitleKey => "nation_pick_title";
        protected override Vector2 AnchorMin => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchorMax => new Vector2(0.5f, 0.5f);
        protected override Vector2 Pivot => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchoredPosition => Vector2.zero;
        protected override Vector2 Size => new Vector2(UIStyles.ListWidth, UIStyles.ListHeight);
        protected override Color BgColor => Bg;
        /// <summary>长列表（世界可能有上百国）→ 固定高度 + 滚动，不做内容自适应。</summary>
        protected override bool AutoFitHeight => false;
        protected override bool UseVanillaSkin => true;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<NationPickWindow>("EconomyNationPick");
        }

        public override void RefreshNow()
        {
            ClearContent();
            ClearRows();
            Build();
        }

        /// <summary>语言切换：标题走基类；可见时立即按键控文案重建。</summary>
        public override void RefreshAllTexts()
        {
            base.RefreshAllTexts();
            if (_visible && _panelRoot != null) RefreshNow();
        }

        /// <summary>退出世界：清空行（避免残留上一局的国家名）。</summary>
        public override void OnWorldUnavailable()
        {
            base.OnWorldUnavailable();
            ClearRows();
        }

        private void ClearRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                var go = _rows[i];
                if (go == null) continue;
                PerfCounters.AddUiDestroyed(1);
                Destroy(go);
            }
            _rows.Clear();
        }

        private void Build()
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled)
            {
                AddLine(UIHelpers.L("nation_pick_disabled"), Muted, 12f);
                return;
            }

            if (World.world == null || World.world.kingdoms == null)
            {
                AddLine(UIHelpers.L("picker_empty"), Muted, 12f);
                return;
            }

            long mineId = NationEngine.NationKingdomId;
            if (mineId != 0)
            {
                var mine = GameHelpers.FindKingdom(mineId);
                AddLine(UIHelpers.Lf("nation_pick_current",
                    mine != null ? GameHelpers.SafeKingdomName(mine) : "?"), UIStyles.AccentGold, 12f);
                AddRowButton(UIHelpers.L("nation_pick_open_cabinet"), RowColor,
                    () => { if (CabinetWindow.Instance != null) CabinetWindow.Instance.Show(); });
            }
            else
            {
                AddLine(UIHelpers.L("nation_pick_none"), SubColor, 12f);
            }

            AddDivider(UIStyles.Divider);

            // 按 GDP 降序（与经济面板/外交页同一排序口径），无统计的国家排后
            _sortPool.Clear();
            foreach (var k in World.world.kingdoms)
            {
                if (k == null || k.data == null) continue;
                _sortPool.Add(k);
            }
            _sortPool.Sort((a, b) =>
            {
                long ga = 0, gb = 0;
                EconomyMod.Models.KingdomStats ka, kb;
                if (EconomyEngine.KingdomStats.TryGetValue(a.data.id, out ka) && ka != null) ga = (long)ka.GDP;
                if (EconomyEngine.KingdomStats.TryGetValue(b.data.id, out kb) && kb != null) gb = (long)kb.GDP;
                return gb.CompareTo(ga);
            });

            int shown = 0;
            for (int i = 0; i < _sortPool.Count && shown < MaxRows; i++)
            {
                var k = _sortPool[i];
                if (k == null || k.data == null) continue;
                long kid = k.data.id;
                bool isMine = kid == mineId;

                EconomyMod.Models.KingdomStats ks;
                string gdpTxt = "?";
                string popTxt = "?";
                if (EconomyEngine.KingdomStats.TryGetValue(kid, out ks) && ks != null)
                {
                    gdpTxt = UIHelpers.FormatCompact((long)ks.GDP);
                    popTxt = UIHelpers.FormatCompact(ks.Population);
                }
                string label = UIHelpers.Lf("nation_pick_row", GameHelpers.SafeKingdomName(k), gdpTxt, popTxt);
                if (isMine) label = UIHelpers.L("nation_pick_mine") + " " + label;

                var captured = k;
                AddRowButton(label, isMine ? MineColor : RowColor, () => Pick(captured));
                shown++;
            }

            if (shown == 0)
            {
                AddLine(UIHelpers.L("picker_empty"), Muted, 12f);
                return;
            }

            AddLine(UIHelpers.Lf("nation_pick_hint", NationEngine.SwitchCooldownYears), Muted, 11f);
        }

        /// <summary>点一行：认领（或打开已扮演国家的内阁）。失败时按原因给提示。</summary>
        private void Pick(Kingdom kingdom)
        {
            try
            {
                if (kingdom == null || kingdom.data == null) return;
                if (NationEngine.NationKingdomId == kingdom.data.id)
                {
                    if (CabinetWindow.Instance != null) CabinetWindow.Instance.Show();
                    return;
                }

                var cfg = UnrestConfig.Instance;
                if (cfg == null || !cfg.NationPlayEnabled)
                {
                    GameHelpers.NotifyLocalized("nation_pick_disabled");
                    return;
                }

                int year;
                try { year = EconomyModMain.GetCurrentGameYear(); } catch (System.Exception) { year = 0; }

                string noteKey;
                if (NationEngine.Claim(kingdom, year, out noteKey))
                {
                    // 成功提示由 NationEngine.Claim 内部发出（toast_nation_claim），此处不重复
                    RefreshNow();
                    if (CabinetWindow.Instance != null) CabinetWindow.Instance.Show();
                    return;
                }

                // 失败：区分换国冷却与其它原因（冷却时不静默，否则玩家会以为按钮坏了）
                if (NationEngine.NationKingdomId != 0
                    && year - NationEngine.LastSwitchYear < NationEngine.SwitchCooldownYears)
                    GameHelpers.NotifyLocalized("nation_pick_cooldown", NationEngine.SwitchCooldownYears);
                else
                    GameHelpers.NotifyLocalized("picker_failed", GameHelpers.SafeKingdomName(kingdom));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 选择扮演国家失败: " + e.Message);
            }
        }

        /// <summary>整行按钮（宽度撑满内容区；行由本窗自行回收）。</summary>
        private void AddRowButton(string label, Color bg, System.Action onClick)
        {
            var btn = UIHelpers.CreateButton(label, _content.transform, -1, Fs(30f), _gameFont, bg, Fs(12f));
            if (btn == null) return;
            var le = btn.GetComponent<LayoutElement>();
            if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = Fs(30f);
            le.minHeight = Fs(24f);
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            PerfCounters.AddUiCreated(1);
            _rows.Add(btn.gameObject);
        }
    }
}
