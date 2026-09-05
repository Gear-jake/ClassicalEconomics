using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 抉择事件弹窗（v1.4.0，钢铁雄心4 式）：事件触发时正面弹出的事件卡——
    /// 全屏半透明遮罩（拦截点击，世界照常流动但不可操作）+ 居中事件卡
    /// （事件名/国名/剩余年 + 描述 + 全宽选项按钮，每个选项带后果摘要行）。
    /// 多件待决时逐件切换；点 × 关闭仅隐藏（世界恢复可操作），待办保留——
    /// 可从内阁财税页"待决事件"行或下一年度弹窗重新打开。
    /// 弹出时机：快照尾（WriteCycleSnapshot）消费 Events 阶段的排队标记。
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
        protected override Vector2 AnchoredPosition => Vector2.zero;
        protected override Vector2 Size => new Vector2(520f, 440f);
        protected override Color BgColor => new Color(0.10f, 0.11f, 0.14f, 0.98f);

        private static readonly Color Muted = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color BtnGood = new Color(0.25f, 0.42f, 0.3f, 0.9f);
        private static readonly Color BtnColor = new Color(0.35f, 0.35f, 0.4f, 0.85f);
        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.55f);

        private GameObject _backdrop;  // 全屏遮罩（拦截点击；随窗口显隐）
        private int _index;            // 当前展示的挂起事件下标（多件时循环切换）

        // ===== 遮罩随窗口显隐（Show/Hide/Toggle 三口全覆盖）=====

        public new void Show()
        {
            base.Show();
            SetBackdrop(true);
        }

        public new void Hide()
        {
            base.Hide();
            SetBackdrop(false);
        }

        public new void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        private void SetBackdrop(bool on)
        {
            if (transform == null) return;
            if (_backdrop == null)
            {
                var go = new GameObject("EventChoiceBackdrop", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false); // 挂 Canvas 根（全屏），不在 _panelRoot 下
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                var img = go.GetComponent<Image>();
                img.color = BackdropColor;
                img.raycastTarget = true; // 拦截点击：遮罩存续期间世界不可点
                go.transform.SetAsFirstSibling(); // 垫在事件卡之下、其余 Canvas 之上
                _backdrop = go;
            }
            _backdrop.SetActive(on);
        }

        public override void OnWorldUnavailable()
        {
            Hide(); // 我们的 Hide 负责遮罩；基类再做窗口隐藏与内容清理
            base.OnWorldUnavailable();
        }

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

            // 事件卡头：事件名 + 国名（多件时附计数）
            string header = pending.Count > 1
                ? UIHelpers.Lf("event_choice_header", UIHelpers.L("ev_" + def.id), _index + 1, pending.Count)
                : UIHelpers.L("ev_" + def.id);
            AddLine(header, UIStyles.Gold, 16f);
            AddLine(UIHelpers.Lf("event_choice_kingdom", p.KingdomName), Muted, 12f);

            // 描述正文
            AddLine(UIHelpers.L("ev_" + def.id + "_desc"), UIStyles.TextPrimary, 13f);

            // 剩余年
            int left = System.Math.Max(0, def.timeoutYears - p.ElapsedYears);
            AddLine(UIHelpers.Lf("event_choice_countdown", left), UIStyles.Warning, 11f);
            AddDivider(new Color(0.35f, 0.35f, 0.4f, 0.6f));

            // 选项（全宽卡式按钮：选项名 + 后果摘要行）
            var stats = Core.NationEngine.NationStats();
            float gdp = stats?.GDP ?? 0f;
            for (int i = 0; i < def.options.Count; i++)
            {
                var opt = def.options[i];
                bool canAfford = Core.DecisionEvents.CanAfford(def, i);
                string optName = UIHelpers.L("ev_" + def.id + "_" + opt.key);
                var row = new GameObject("OptCard" + i, typeof(RectTransform), typeof(VerticalLayoutGroup));
                row.transform.SetParent(_content.transform, false);
                var le = row.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                var vlg = row.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 1;
                vlg.childForceExpandWidth = true;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                _lines.Add(row);

                var btn = UIHelpers.CreateButton(optName, row.transform, -1, 34f * s, _gameFont,
                    canAfford ? BtnGood : BtnColor, 13f * s);
                btn.interactable = canAfford;
                int idx = i;
                btn.onClick.AddListener(() =>
                {
                    Core.DecisionEvents.Choose(idx);
                    _index = 0;
                    if (DecisionEvents.PendingCount > 0) RefreshNow(); // 下一件待决
                    else Hide();
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

        /// <summary>选项费用/效果摘要（一行：花费/入库/征税/济贫/好感/动荡）。</summary>
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
