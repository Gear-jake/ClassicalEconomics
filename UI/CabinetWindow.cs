using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 内阁面板（中央银行家）：玩家认领国家的治理台。
    /// RulerBox 式枢纽-页面布局：顶部 Tab 切换 4 个页面——财税 / 政策 / 法令·建设 / 外交，
    /// 每个页面获得整幅内容宽度（不再挤在一个小页里）；窗口 600×760。
    /// 未认领时显示国家列表（认领页）。
    /// </summary>
    public partial class CabinetWindow : FloatingWindow
    {
        private static CabinetWindow _instance;
        public static CabinetWindow Instance => _instance;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<CabinetWindow>("EconomyCabinet");
        }

        protected override string WindowName => "EconomyCabinet";
        protected override float SortingOrder => 10003f;
        protected override string TitleKey => "cabinet_title";
        protected override Vector2 AnchorMin => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchorMax => new Vector2(0.5f, 0.5f);
        protected override Vector2 Pivot => new Vector2(0.5f, 0.5f);
        protected override Vector2 AnchoredPosition => new Vector2(560f, 0f);
        protected override Vector2 Size => new Vector2(600f, 760f);
        protected override Color BgColor => new Color(0.12f, 0.13f, 0.16f, 0.97f);
        /// <summary>【W2】套用本体窗口皮肤（不注册为 ScrollWindow，游戏不暂停）。</summary>
        protected override bool UseVanillaSkin => true;

        private enum CabinetPage { Finance = 0, Policy, Decree, Diplomacy, Law, Bank }
        private const int PageCount = 6;
        private static readonly string[] PageKeys = { "cabinet_tab_finance", "cabinet_tab_policy", "cabinet_tab_decree", "cabinet_tab_diplomacy", "cabinet_tab_codex", "cabinet_tab_bank" };

        /// <summary>「退出国家扮演」两步确认：首次点击进入待确认态（避免误触清空金库）。</summary>
        private bool _leaveArmed;

        private static readonly Color Muted = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color DividerColor = new Color(0.35f, 0.35f, 0.4f, 0.6f);
        private static readonly Color BtnColor = new Color(0.35f, 0.35f, 0.4f, 0.85f);
        private static readonly Color BtnGood = new Color(0.25f, 0.42f, 0.3f, 0.9f);
        private static readonly Color BtnBad = new Color(0.45f, 0.28f, 0.28f, 0.9f);
        private static readonly Color TabOn = new Color(0.25f, 0.42f, 0.3f, 0.95f);
        private static readonly Color TabOff = new Color(0.28f, 0.29f, 0.34f, 0.9f);

        private RectTransform _contentRect;
        private Button[] _tabButtons = new Button[PageCount];
        private readonly GameObject[] _pages = new GameObject[PageCount];
        private readonly List<GameObject>[] _pageLines = new List<GameObject>[PageCount];
        private CabinetPage _page = CabinetPage.Finance;
        private long _dipTargetId; // 外交详情页目标国（0 = 列表模式）
        private bool _refreshing;  // 重入保护：年度刷新恰逢按钮触发时防止双重 Destroy

        private List<GameObject> CurLines => _pageLines[(int)_page];
        private GameObject CurPage => _pages[(int)_page];

        protected override void BuildPanel()
        {
            var canvas = GetComponent<Canvas>();
            UIHelpers.SetupCanvas(canvas, SortingOrder);

            _panelRect = UIHelpers.CreatePanelRoot(transform, WindowName + "Panel",
                AnchorMin, AnchorMax, Pivot, AnchoredPosition, Size, BgColor);
            _panelRoot = _panelRect.gameObject;

            UIHelpers.CreateDragArea(_panelRect, _panelRect, Padding + 36);
            _titleText = UIHelpers.CreateWindowTitle(_panelRect, UIHelpers.L(TitleKey), _gameFont,
                UIStyles.Gold, TitleFontSize, Padding, TitleLineHeight);
            UIHelpers.CreateResizeHandles(_panelRect, OnPanelResized);
            UIHelpers.CreateCloseButton(_panelRect, _gameFont, Hide);

            // Tab 栏（RulerBox 式页面切换）
            var tabBar = new GameObject("CabinetTabBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabBar.transform.SetParent(_panelRoot.transform, false);
            var tabRt = tabBar.GetComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0, 1); tabRt.anchorMax = new Vector2(1, 1);
            tabRt.pivot = new Vector2(0.5f, 1f);
            // 【W2】本体皮肤下需避让厚边框，否则页签会压在边框上（实机反馈"按钮偏上，出框了"）
            tabRt.anchoredPosition = new Vector2(0, -BelowTitleOffset);
            tabRt.sizeDelta = new Vector2(-Padding * 2, Fs(30f));
            var hlg = tabBar.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true; hlg.childControlHeight = true;

            for (int i = 0; i < PageCount; i++)
            {
                int page = i;
                var btn = UIHelpers.CreateButton(UIHelpers.L(PageKeys[i]), tabBar.transform, -1, Fs(28f), _gameFont, TabOff, Fs(12f));
                btn.onClick.AddListener(() => SwitchPage((CabinetPage)page));
                _tabButtons[i] = btn;
            }

            // 滚动内容区（页面容器挂这里）
            var scrollGo = UIHelpers.CreateScrollContent(_panelRect, Padding, BelowTitleOffset + Fs(30f) + 6f, false, SkinBottomInset);
            _content = scrollGo.gameObject;
            _contentRect = scrollGo;

            // 4 个页面容器（SetActive 切换）
            for (int i = 0; i < PageCount; i++)
            {
                var go = new GameObject("CabinetPage" + i, typeof(RectTransform), typeof(VerticalLayoutGroup));
                go.transform.SetParent(_content.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = Vector2.zero;
                var vhl = go.GetComponent<VerticalLayoutGroup>();
                vhl.spacing = 4;
                vhl.padding = new RectOffset(2, 2, 2, 2);
                vhl.childControlWidth = true; vhl.childControlHeight = false;
                vhl.childForceExpandWidth = true; vhl.childForceExpandHeight = false;
                _pages[i] = go;
                _pageLines[i] = new List<GameObject>();
            }

            UpdateTabHighlights();
            SwitchPage(CabinetPage.Finance);
        }

        private void SwitchPage(CabinetPage page)
        {
            _page = page;
            _leaveArmed = false;   // 切页复位"退出扮演"的待确认态
            for (int i = 0; i < PageCount; i++)
                _pages[i].SetActive(i == (int)page);
            UpdateTabHighlights(); // 高亮随打开的页面（曾漏调：点击 Tab 后永远停在第一个）
            RefreshNow();
        }

        private void UpdateTabHighlights()
        {
            for (int i = 0; i < PageCount; i++)
            {
                if (_tabButtons[i] == null) continue;
                var img = _tabButtons[i].GetComponent<Image>();
                if (img != null) img.color = i == (int)_page ? TabOn : TabOff;
            }
        }

        public override void RefreshNow()
        {
            if (_refreshing) return; // 重入保护（年度刷新与按钮点击同帧交错时跳过本次）
            _refreshing = true;
            try
            {
                DoRefreshNow();
            }
            finally
            {
                _refreshing = false;
            }
        }

        private void DoRefreshNow()
        {
            // 整页彻底重建：销毁当前页所有子物体（防止 ChartCard 等因注册遗漏累积成黑块）
            PerfCounters.AddUiDestroyed(CurPage.transform.childCount);
            for (int i = CurPage.transform.childCount - 1; i >= 0; i--)
                Destroy(CurPage.transform.GetChild(i).gameObject);
            CurLines.Clear();

            var cfg = UnrestConfig.Instance;
            if (cfg == null) return;

            if (AnnualPipeline.IsSettling)
            {
                AddLine(UIHelpers.L("settling_marker"), UIStyles.Warning, 12f);
            }

            if (!cfg.NationPlayEnabled)
            {
                AddLine(UIHelpers.L("cabinet_disabled"), Muted, 12f);
                return;
            }

            if (NationEngine.NationKingdomId == 0) BuildClaimSection();
            else BuildPage((int)_page);
        }

        private void BuildPage(int page)
        {
            switch ((CabinetPage)page)
            {
                case CabinetPage.Finance: BuildFinancePage(); break;
                case CabinetPage.Policy: BuildPolicyPage(); break;
                case CabinetPage.Decree: BuildDecreePage(); break;
                default: BuildDiplomacyPage(); break;
                case CabinetPage.Law: BuildLawPage(null); break;
                case CabinetPage.Bank: BuildBankPage(); break;
            }
        }


        // ===== 行/按钮小构件（挂当前页） =====

        /// <summary>向当前页添加一行文本。</summary>
        private void AddLine(string text, Color color, float size)
        {
            var go = UIHelpers.CreateText(text, CurPage.transform, Fs(size), color, _gameFont, Fs(22f));
            CurLines.Add(go);
        }

        private void AddDivider(Color color)
        {
            CurLines.Add(UIHelpers.CreateDivider(CurPage.transform, color));
        }

        private GameObject NewRow(int expectButtons, float height = 28f, bool fill = false)
        {
            var row = new GameObject("CabinetRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(CurPage.transform, false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = Fs(height);
            le.flexibleWidth = 1f;
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            // fill=true：子按钮均分整行（随窗口拖拽缩放自适应）；
            // fill=false：紧凑——按钮按固定宽排布，不拉满整行
            hlg.childForceExpandWidth = fill;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            CurLines.Add(row);
            return row;
        }

        private void AddRowButton(GameObject row, string label, Color bg, System.Action onClick, float width = 120f, bool fill = false)
        {
            var btn = UIHelpers.CreateButton(label, row.transform, Fs(width), Fs(28f), _gameFont, bg, Fs(12f));
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = Fs(width);
            le.flexibleWidth = fill ? 1f : 0f; // fill=true 均分剩余（随窗口缩放）
            le.preferredHeight = Fs(28f);
            btn.onClick.AddListener(() => onClick());
        }

        private void AddRowSpacer(GameObject row)
        {
            var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(row.transform, false);
            var le = go.GetComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.preferredWidth = 1f;
            le.flexibleHeight = 1f;
        }

        private static int SafeYear()
        {
            try { return EconomyModMain.GetCurrentGameYear(); }
            catch (System.Exception) { return 0; }
        }

        public override void RefreshAllTexts()
        {
            base.RefreshAllTexts();
            for (int i = 0; i < PageCount; i++)
            {
                if (_tabButtons[i] == null) continue;
                var t = _tabButtons[i].GetComponentInChildren<Text>();
                if (t != null) t.text = UIHelpers.L(PageKeys[i]);
            }
            if (_visible && _panelRoot != null) RefreshNow();
        }

        /// <summary>窗口内容区随改变缩放重建（页面结构不变，仅刷新当前页）。</summary>
        protected override void OnPanelResized()
        {
            if (_visible) RefreshNow();
        }

        /// <summary>UI 缩放配置变更后整窗重建（Tab 栏高度/滚动区偏移都依赖 Fs，仅刷当前页不够）。</summary>
        public void RebuildPanelFromScale()
        {
            if (_panelRoot != null) Destroy(_panelRoot);
            for (int i = 0; i < PageCount; i++)
            {
                _pages[i] = null;
                _pageLines[i] = new List<GameObject>();
            }
            BuildPanel();
            if (_visible)
            {
                _panelRoot.SetActive(true);
                RefreshNow();
            }
        }
    }
}
