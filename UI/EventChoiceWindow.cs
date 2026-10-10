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
        // ===== 报纸风（参考 World News 报纸弹窗）=====
        // 底色走自带素材 GameResources/ui/panels/paper_9slice.png（由 tools/gen_paper_art.py 生成），
        // 因此不再套本体石板皮肤 —— 报纸底 + 石板边框两种风格会打架。
        protected override Color BgColor => PaperBg;
        /// <summary>【布局修复】模态卡片应完整显示所有选项。</summary>
        protected override bool AutoFitHeight => true;
        protected override bool UseVanillaSkin => false;
        protected override Sprite CustomPanelSprite => UIArt.Get("paper_9slice");
        protected override Color TitleColor => Headline;   // 报头：暗红
        protected override Color TitleRuleColor => new Color(0.46f, 0.10f, 0.08f, 0.65f);   // 报头分隔线

        // 报纸配色：暖米纸 + 墨色字 + 暗红报头
        private static readonly Color PaperBg  = new Color(0.91f, 0.86f, 0.75f, 1f);
        internal static readonly Color Ink      = new Color(0.20f, 0.16f, 0.11f);
        internal static readonly Color InkSoft  = new Color(0.40f, 0.34f, 0.24f);
        internal static readonly Color Headline = new Color(0.46f, 0.10f, 0.08f);
        internal static readonly Color Caption  = new Color(0.44f, 0.37f, 0.26f);
        internal static readonly Color RuleLine = new Color(0.45f, 0.38f, 0.26f, 0.55f);

        private static readonly Color Muted = InkSoft;
        private static readonly Color BtnGood = new Color(0.20f, 0.22f, 0.19f, 1f);   // 可选：近墨色
        private static readonly Color BtnColor = new Color(0.32f, 0.30f, 0.26f, 1f);   // 不可选：灰墨
        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.45f);

        private GameObject _backdrop;  // 全屏遮罩（拦截点击；随窗口显隐）
        private int _index;            // 当前展示的挂起事件下标（多件时循环切换）

        // ===== 遮罩随窗口显隐（基类 Show/Hide/Toggle 虚化后经 OnVisibilityChanged 钩子；
        // ===== 关闭按钮/OnWorldUnavailable 等全部路径统一走此钩子，杜绝遮罩残留）=====

        protected override void OnVisibilityChanged(bool visible)
        {
            SetBackdrop(visible);
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
            base.OnWorldUnavailable(); // 基类 Hide → 钩子关遮罩 + 清内容
        }

        /// <summary>弹出并展示最早挂起事件（无挂起则空态）。</summary>
        public void ShowPending()
        {
            _index = 0;
            Show();
        }

        /// <summary>年度边界：挂起事件倒计时已递增，可见时刷新（"剩余 X 年"逐年递减）。</summary>
        public void OnYearBoundary()
        {
            if (!_visible) return;
            if (DecisionEvents.PendingCount == 0) return;
            RefreshNow();
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

            // 事件卡头：族别（族别色）+ 事件名 + 国名 + 当前年（多件时附计数）；{king}/{kingdom} 名字代入
            // 报纸版式：分类小字（如 "AFFAIRS OF STATE - YEAR 1"）→ 大标题 → 正文
            AddLine(UIHelpers.L("event_family_" + def.family).ToUpperInvariant(), Caption, 11f);
            var familyColor = FamilyColor(def.family);
            string header = pending.Count > 1
                ? UIHelpers.Lf("event_choice_header", UIHelpers.L("ev_" + def.id), _index + 1, pending.Count)
                : UIHelpers.L("ev_" + def.id);
            header = Core.DecisionEvents.Contextualize(header, p.KingdomId);
            AddLine(header, Headline, 18f);   // 大标题：暗红，报纸头条
            AddLine(Core.DecisionEvents.Contextualize(
                UIHelpers.Lf("event_choice_kingdom", p.KingdomName), p.KingdomId), Muted, 12f);
            try { AddLine(UIHelpers.Lf("event_choice_year", EconomyModMain.GetCurrentGameYear()), Muted, 11f); }
            catch (System.Exception) { }

            // 描述正文
            AddLine(Core.DecisionEvents.Contextualize(UIHelpers.L("ev_" + def.id + "_desc"), p.KingdomId),
                Ink, 13f);

            // 剩余年
            int left = System.Math.Max(0, def.timeoutYears - p.ElapsedYears);
            AddLine(UIHelpers.Lf("event_choice_countdown", left), Headline, 12f);   // 报纸风：暗红，与报头呼应
            AddDivider(RuleLine);

            // 选项（全宽卡式按钮：选项名 + 后果摘要行）
            var stats = Core.NationEngine.NationStats();
            float gdp = stats?.GDP ?? 0f;
            for (int i = 0; i < def.options.Count; i++)
            {
                var opt = def.options[i];
                bool canAfford = Core.DecisionEvents.CanAfford(def, i);
                string optName = Core.DecisionEvents.Contextualize(
                    UIHelpers.L("ev_" + def.id + "_" + opt.key), p.KingdomId);
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
                // 报纸风：选项按钮改用深色石板素材（参考图底部那种）
                var paperBtn = UIArt.Get("paper_button_9slice");
                if (paperBtn != null)
                {
                    var bi = btn.GetComponent<Image>();
                    if (bi != null) { bi.sprite = paperBtn; bi.type = Image.Type.Sliced; bi.color = Color.white; bi.pixelsPerUnitMultiplier = 1f; }
                    var bl = btn.GetComponentInChildren<Text>();
                    if (bl != null) bl.color = new Color(0.96f, 0.94f, 0.88f);   // 深底上近白字
                }
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
                btnLe.preferredHeight = 34f * s;   // 显式行高：不让布局器猜

                string optDesc = Core.DecisionEvents.Contextualize(
                    UIHelpers.L("ev_" + def.id + "_" + opt.key + "_desc"), p.KingdomId);
                if (optDesc != "ev_" + def.id + "_" + opt.key + "_desc")
                {
                    var dGo = UIHelpers.CreateText(optDesc, row.transform, 11f * s, InkSoft, _gameFont, 18f * s);
                    var dLe = dGo.AddComponent<LayoutElement>();
                    dLe.flexibleWidth = 1f; dLe.preferredHeight = 18f * s;
                }
                string summary = OptionSummary(def, i, gdp);
                if (!string.IsNullOrEmpty(summary))
                {
                    var sGo = UIHelpers.CreateText(summary, row.transform, 10f * s, Muted, _gameFont, 16f * s);
                    var sLe = sGo.AddComponent<LayoutElement>();
                    sLe.flexibleWidth = 1f; sLe.preferredHeight = 16f * s;
                }
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

            FitPanelToContent();
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
            if (o.commercePenaltyYears > 0)
                parts.Add(UIHelpers.Lf("event_choice_commerce", o.commercePenaltyYears));
            if (o.declareWarTarget != 0)
                parts.Add(UIHelpers.L("event_choice_declare_war"));
            if (o.formAllianceTarget != 0)
                parts.Add(UIHelpers.L("event_choice_alliance"));
            if (o.moveCapital)
                parts.Add(UIHelpers.L("event_choice_move_capital"));
            if (o.upgradeBuildings > 0)
                parts.Add(UIHelpers.Lf("event_choice_upgrade", o.upgradeBuildings));
            if (o.citizenWealthRatio > 0f)
                parts.Add(UIHelpers.L("event_choice_citizen_gain"));
            else if (o.citizenWealthRatio < 0f)
                parts.Add(UIHelpers.L("event_choice_citizen_loss"));
            if (o.worldWar)
                parts.Add(UIHelpers.L("event_choice_worldwar"));
            return parts.Count > 0 ? string.Join("  ", parts.ToArray()) : null;
        }

        /// <summary>族别色（事件卡头/事件流过滤同源语义）。</summary>
        private static Color FamilyColor(string family)
        {
            switch (family)
            {
                case "finance": return UIStyles.Gold;
                case "disaster": return UIStyles.EvDisaster;
                case "court": return new Color(0.85f, 0.6f, 0.95f);
                case "military": return UIStyles.EvPlunder;
                case "civil": return UIStyles.Info;
                case "diplomacy": return UIStyles.Positive;
                default: return UIStyles.TextPrimary;
            }
        }

        private static float Scale()
        {
            var cfg = UnrestConfig.Instance;
            float s = cfg != null ? cfg.UiScale : 1.2f;
            return Mathf.Clamp(s, 0.8f, 1.6f);
        }
    }
}
