using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 抉择事件小窗（v1.4.0）：非模态、可拖拽、不暂停游戏。
    /// 展示最早一条挂起事件（标题/描述/剩余年/选项按钮竖排含费用与效果摘要），
    /// 多个挂起事件用"还有 N 件待决"循环切换；选择后执行后果并刷新下一件。
    /// 由快照尾（WriteCycleSnapshot）在 DecisionEvents.PopupQueued 时弹出；
    /// 也可从内阁待办区打开。无挂起事件时显示空态。
    /// </summary>
    public class EventChoiceWindow : FloatingWindow
    {
        private static EventChoiceWindow _instance;
        public static EventChoiceWindow Instance => _instance;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<EventChoiceWindow>("EconomyEventChoice");
        }

        protected override string WindowName => "EconomyEventChoice";
        protected override float SortingOrder => 10004f;
        protected override string TitleKey => "event_choice_title";
        protected override Vector2 AnchorMin => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchorMax => new Vector2(0.5f, 0.5f);
        protected override Vector2 Pivot => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchoredPosition => new Vector2(0f, 120f);
        protected override Vector2 Size => new Vector2(420f, 380f);
        protected override Color BgColor => new Color(0.12f, 0.13f, 0.16f, 0.97f);

        private static readonly Color Muted = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color BtnColor = new Color(0.35f, 0.35f, 0.4f, 0.85f);
        private static readonly Color BtnGood = new Color(0.25f, 0.42f, 0.3f, 0.9f);
        private int _index; // 当前展示的挂起事件下标（多件时循环切换）

        /// <summary>弹出并展示最早挂起事件（无挂起则空态）。</summary>
        public void ShowPending()
        {
            _index = 0;
            Show();
        }

        public override void RefreshNow()
        {
            if (_content == null) return;
            for (int i = _content.transform.childCount - 1; i >= 0; i--)
                Destroy(_content.transform.GetChild(i).gameObject);
            _lines.Clear();

            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.NationPlayEnabled)
            {
                AddLine(UIHelpers.L("cabinet_disabled"), Muted, 12f);
                return;
            }

            var pending = DecisionEvents.Pending;
            if (pending == null || pending.Count == 0)
            {
                AddLine(UIHelpers.L("event_choice_none"), Muted, 12f);
                return;
            }
            if (_index >= pending.Count) _index = 0;
            var p = pending[_index];
            var def = p.Def;
            float s = Scale();

            // 头行：国名 + 件数切换
            string head = UIHelpers.Lf("event_choice_header", p.KingdomName, _index + 1, pending.Count);
            AddLine(head, UIStyles.Gold, 13f);

            // 描述
            AddLine(UIHelpers.L("ev_" + def.id + "_desc"), UIStyles.TextPrimary, 12f);

            // 剩余年
            int left = System.Math.Max(0, def.timeoutYears - p.ElapsedYears);
            AddLine(UIHelpers.Lf("event_choice_countdown", left), UIStyles.Warning, 11f);
            AddDivider(new Color(0.35f, 0.35f, 0.4f, 0.6f));

            // 选项按钮（竖排）：名称 + 费用/效果摘要行
            var stats = Core.NationEngine.NationStats();
            float gdp = stats?.GDP ?? 0f;
            for (int i = 0; i < def.options.Count; i++)
            {
                var opt = def.options[i];
                bool canAfford = Core.DecisionEvents.CanAfford(def, i);
                string optName = UIHelpers.L("ev_" + def.id + "_" + opt.key);
                var row = new GameObject("OptRow" + i, typeof(RectTransform), typeof(VerticalLayoutGroup));
                row.transform.SetParent(_content.transform, false);
                var le = row.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                var vlg = row.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 1;
                vlg.childForceExpandWidth = true;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                _lines.Add(row);

                var btn = UIHelpers.CreateButton(optName, row.transform, -1, 28f * s, _gameFont,
                    canAfford ? BtnGood : BtnColor, 12f * s);
                btn.interactable = canAfford;
                int idx = i;
                btn.onClick.AddListener(() =>
                {
                    Core.DecisionEvents.Choose(idx);
                    _index = 0;
                    RefreshNow();
                });
                var btnLe = btn.gameObject.AddComponent<LayoutElement>();
                btnLe.flexibleWidth = 1f;

                string summary = OptionSummary(def, i, gdp);
                if (!string.IsNullOrEmpty(summary))
                    UIHelpers.CreateText(summary, row.transform, 10f * s, Muted, _gameFont, 16f * s);
            }

            // 多件切换
            if (pending.Count > 1)
            {
                var more = UIHelpers.CreateButton(
                    UIHelpers.Lf("event_choice_next", pending.Count - 1),
                    _content.transform, -1, 26f * s, _gameFont, BtnColor, 11f * s);
                more.onClick.AddListener(() =>
                {
                    _index = (_index + 1) % DecisionEvents.Pending.Count;
                    RefreshNow();
                });
                var moreLe = more.gameObject.AddComponent<LayoutElement>();
                moreLe.flexibleWidth = 1f;
                _lines.Add(more.gameObject);
            }
        }

        /// <summary>选项费用/效果摘要（一行：花费/征税/济贫/好感/动荡）。</summary>
        private string OptionSummary(DecisionEvents.EventDef def, int i, float gdp)
        {
            var o = def.options[i];
            var parts = new List<string>();
            if (o.treasuryGdpRatio < 0f && gdp > 0f)
                parts.Add(UIHelpers.Lf("event_choice_cost", Core.NationEngine.FormatGold((long)(gdp * -o.treasuryGdpRatio))));
            else if (o.treasuryGdpRatio > 0f && gdp > 0f)
                parts.Add(UIHelpers.Lf("event_choice_gain", Core.NationEngine.FormatGold((long)(gdp * o.treasuryGdpRatio))));
            if (o.residentsTaxRatio > 0f)
                parts.Add(UIHelpers.L("event_choice_tax"));
            if (o.poorReliefRatio > 0f)
                parts.Add(UIHelpers.L("event_choice_relief"));
            if (o.goodwillAll != 0)
                parts.Add(UIHelpers.Lf("event_choice_goodwill", o.goodwillAll > 0 ? "+" : "", o.goodwillAll));
            if (o.unrest)
                parts.Add(UIHelpers.L("event_choice_unrest"));
            return parts.Count > 0 ? string.Join("  ", parts.ToArray()) : null;
        }

        private static float Scale()
        {
            var cfg = UnrestConfig.Instance;
            float s = cfg != null ? cfg.UiScale : 1.2f;
            return Mathf.Clamp(s, 0.8f, 1.6f);
        }
    }
}
