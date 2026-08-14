using UnityEngine;
using UnityEngine.UI;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    /// <summary>
    /// 贸易净额悬浮窗（v0.13）：两张净贸易额排名表——
    /// 城市表：单个城市 净额 = 总出口 − 总进口，降序；
    /// 国家表：单个国家 净额 = 其所有城市（村镇）贸易额之和（总出口 − 总进口），降序。
    /// 数据来自 HistoryService 最新快照（后台已聚合 + 降序 + 含真实名字）。
    /// </summary>
    public class TradeShareWindow : FloatingWindow
    {
        private static TradeShareWindow _instance;

        private RectTransform _contentRect;
        private Button _btnKingdom, _btnCity;
        private Image _btnKingdomImg, _btnCityImg;
        private Text _titleText;
        private bool _showCity; // false=国家 true=城市

        private const int TopN = 40; // 每表最多显示条数

        private static readonly Color Bg           = UIStyles.PanelBg;
        private static readonly Color BtnNormal    = UIStyles.CardBgAlt;
        private static readonly Color BtnActive    = UIStyles.Gold;
        private static readonly Color TextColor    = UIStyles.TextPrimary;
        private static readonly Color HeaderColor  = UIStyles.Gold;
        private static readonly Color Muted        = UIStyles.TextMuted;

        private const float PanelWidth  = UIStyles.ListWidth;
        private const float PanelHeight = UIStyles.ListHeight;
        private const float HeaderSize  = 14f;
        private const float TextSize    = 12f;
        private const float BtnHeight   = 28f;
        private const float RowHeight   = 22f;

        // 列宽（排名 / 名称弹性 / 出口 / 进口 / 净额）
        private const float RankW = 28f;
        private const float NumW  = 64f;
        private const float Gap   = 6f;

        public static TradeShareWindow Instance => _instance;

        // ===== FloatingWindow 配置（与富豪榜同逻辑：右中锚点，拖拽/缩放不飘）=====
        protected override string WindowName => "EconomyTradeShare";
        protected override float SortingOrder => 10002;
        protected override string TitleKey => "trade_share_title";
        protected override Vector2 AnchorMin => new Vector2(1f, 0.5f);
        protected override Vector2 AnchorMax => new Vector2(1f, 0.5f);
        protected override Vector2 Pivot => new Vector2(0f, 0.5f);
        protected override Vector2 AnchoredPosition => new Vector2(-(PanelWidth + 16f), -(PanelHeight + 24f));
        protected override Vector2 Size => new Vector2(PanelWidth, PanelHeight);
        protected override Color BgColor => Bg;
        protected override float Padding => 16f;
        protected override float TitleFontSize => 14f;
        protected override float TitleLineHeight => 28f;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<TradeShareWindow>("EconomyTradeShare");
        }

        protected override void BuildPanel()
        {
            var canvas = GetComponent<Canvas>();
            UIHelpers.SetupCanvas(canvas, SortingOrder);
            _panelRect = UIHelpers.CreatePanelRoot(transform, WindowName + "Panel",
                AnchorMin, AnchorMax, Pivot, AnchoredPosition, Size, BgColor);
            _panelRoot = _panelRect.gameObject;
            UIHelpers.CreateDragArea(_panelRect, _panelRect, Padding + 36);
            _titleText = UIHelpers.CreateWindowTitle(_panelRect, UIHelpers.L(TitleKey),
                _gameFont, HeaderColor, TitleFontSize, Padding, TitleLineHeight);
            UIHelpers.CreateResizeHandles(_panelRect, OnPanelResized);
            UIHelpers.CreateCloseButton(_panelRect, _gameFont, Hide);

            // TabBar（国家/城市维度切换）
            var tabBar = new GameObject("TabBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabBar.transform.SetParent(_panelRoot.transform, false);
            var tabBarRt = tabBar.GetComponent<RectTransform>();
            tabBarRt.anchorMin = new Vector2(0, 1); tabBarRt.anchorMax = new Vector2(1, 1);
            tabBarRt.pivot = new Vector2(0.5f, 1f);
            tabBarRt.anchoredPosition = new Vector2(0, -(Padding + TitleLineHeight));
            tabBarRt.sizeDelta = new Vector2(-Padding * 2, BtnHeight);
            var hlg = tabBar.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true; hlg.childControlHeight = true;

            _btnKingdom = CreateTabButton(UIHelpers.L("trade_share_dim_kingdom"), tabBar.transform, out _btnKingdomImg);
            _btnCity = CreateTabButton(UIHelpers.L("trade_share_dim_city"), tabBar.transform, out _btnCityImg);
            _btnKingdom.onClick.AddListener(() => SwitchDimension(false));
            _btnCity.onClick.AddListener(() => SwitchDimension(true));

            // 滚动内容区（topInset 留出标题 + TabBar 空间）
            float topInset = Padding + TitleLineHeight + BtnHeight + 12;
            _contentRect = UIHelpers.CreateScrollContent(_panelRect, Padding, topInset);
            _content = _contentRect.gameObject;

            UpdateTabButtons();
        }

        protected override void OnPanelResized()
        {
            if (!_visible) return;
            ClearContent();
            BuildTable();
        }

        public override void RefreshNow()
        {
            ClearContent();
            BuildTable();
        }

        public void RefreshAllTexts()
        {
            if (_panelRoot == null) return;
            if (_titleText != null) _titleText.text = UIHelpers.L(TitleKey);
            if (_btnKingdom != null) SetButtonText(_btnKingdom, UIHelpers.L("trade_share_dim_kingdom"));
            if (_btnCity != null) SetButtonText(_btnCity, UIHelpers.L("trade_share_dim_city"));
            UpdateTabButtons();
            if (_visible) { ClearContent(); BuildTable(); }
        }

        private static void SetButtonText(Button btn, string text)
        {
            var t = btn.GetComponentInChildren<Text>();
            if (t != null) t.text = text;
        }

        private Button CreateTabButton(string label, Transform parent, out Image img)
        {
            var btn = UIHelpers.CreateButton(label, parent, -1, BtnHeight, _gameFont, BtnNormal, HeaderSize);
            img = btn.GetComponent<Image>();
            img.color = BtnNormal;
            img.sprite = UIHelpers.RoundedSprite();
            img.type = Image.Type.Sliced;
            var txt = btn.GetComponentInChildren<Text>();
            txt.font = _gameFont;
            txt.fontSize = Mathf.RoundToInt(HeaderSize);
            txt.color = TextColor;
            txt.alignment = TextAnchor.MiddleCenter;
            return btn;
        }

        private void SwitchDimension(bool city)
        {
            _showCity = city;
            UpdateTabButtons();
            if (_visible) { ClearContent(); BuildTable(); }
        }

        private void UpdateTabButtons()
        {
            if (_btnKingdomImg == null || _btnCityImg == null) return;
            _btnKingdomImg.color = !_showCity ? BtnActive : BtnNormal;
            _btnCityImg.color = _showCity ? BtnActive : BtnNormal;
            var kt = _btnKingdom.GetComponentInChildren<Text>();
            var ct = _btnCity.GetComponentInChildren<Text>();
            kt.color = !_showCity ? Color.black : TextColor;
            ct.color = _showCity ? Color.black : TextColor;
        }

        private void BuildTable()
        {
            var snaps = HistoryService.GetRecent(1);
            if (snaps == null || snaps.Count == 0)
            {
                AddLine(UIHelpers.L("trade_share_empty"), Muted, TextSize);
                return;
            }
            var snap = snaps[snaps.Count - 1];
            var list = _showCity ? snap.CityBalances : snap.KingdomBalances;
            if (list == null || list.Count == 0)
            {
                AddLine(UIHelpers.L("trade_share_empty"), Muted, TextSize);
                return;
            }

            float contentW = Mathf.Max(200f, _panelRect.rect.width - Padding * 2f - 16f);

            // 摘要行
            AddLine(UIHelpers.Lf("trade_share_summary",
                snap.TotalExport.ToString("F0"), list.Count), HeaderColor, HeaderSize);

            // 表头
            AddHeader(contentW);

            // 数据行（后台已按净额降序）
            int take = list.Count < TopN ? list.Count : TopN;
            for (int i = 0; i < take; i++)
            {
                AddBalanceRow(i + 1, list[i], contentW);
            }
        }

        private float NameWidth(float width)
            => width - RankW - NumW * 3 - Gap * 4;

        private void AddHeader(float width)
        {
            var row = new GameObject("BalHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(_content.transform, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = Gap;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 18f);
            row.AddComponent<LayoutElement>().preferredHeight = 18f;
            _lines.Add(row);

            var rankGo = UIHelpers.CreateText(UIHelpers.L("col_rank"), row.transform, 10f, Muted, _gameFont, 18f, "Rank");
            rankGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            rankGo.GetComponent<RectTransform>().sizeDelta = new Vector2(RankW, 18f);
            rankGo.AddComponent<LayoutElement>().preferredWidth = RankW;

            var nameGo = UIHelpers.CreateText(_showCity ? UIHelpers.L("col_city") : UIHelpers.L("col_kingdom"),
                row.transform, 10f, Muted, _gameFont, 18f, "Name");
            nameGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            float nameW = NameWidth(width);
            nameGo.GetComponent<RectTransform>().sizeDelta = new Vector2(nameW, 18f);
            nameGo.AddComponent<LayoutElement>().preferredWidth = nameW;

            var expGo = UIHelpers.CreateText(UIHelpers.L("col_export"), row.transform, 10f, Muted, _gameFont, 18f, "Exp");
            expGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            expGo.GetComponent<RectTransform>().sizeDelta = new Vector2(NumW, 18f);
            expGo.AddComponent<LayoutElement>().preferredWidth = NumW;

            var impGo = UIHelpers.CreateText(UIHelpers.L("col_import"), row.transform, 10f, Muted, _gameFont, 18f, "Imp");
            impGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            impGo.GetComponent<RectTransform>().sizeDelta = new Vector2(NumW, 18f);
            impGo.AddComponent<LayoutElement>().preferredWidth = NumW;

            var netGo = UIHelpers.CreateText(UIHelpers.L("col_net"), row.transform, 10f, Muted, _gameFont, 18f, "Net");
            netGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            netGo.GetComponent<RectTransform>().sizeDelta = new Vector2(NumW, 18f);
            netGo.AddComponent<LayoutElement>().preferredWidth = NumW;
        }

        private void AddBalanceRow(int rank, TradeBalance b, float width)
        {
            var row = new GameObject("BalRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(_content.transform, false);
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = Gap;
            hlg.childControlWidth = false; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(width, RowHeight);
            row.AddComponent<LayoutElement>().preferredHeight = RowHeight;
            _lines.Add(row);

            // 排名（前三金银铜）
            Color rankColor = rank == 1 ? UIStyles.Gold : rank == 2 ? UIStyles.Silver
                : rank == 3 ? UIStyles.Bronze : Muted;
            var rankGo = UIHelpers.CreateText(rank.ToString(), row.transform, TextSize, rankColor, _gameFont, RowHeight, "Rank");
            rankGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            rankGo.GetComponent<Text>().fontStyle = FontStyle.Bold;
            rankGo.GetComponent<RectTransform>().sizeDelta = new Vector2(RankW, RowHeight);
            rankGo.AddComponent<LayoutElement>().preferredWidth = RankW;

            // 名称
            var nameGo = UIHelpers.CreateText(b.Name, row.transform, TextSize, TextColor, _gameFont, RowHeight, "Name");
            nameGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            nameGo.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            float nameW = NameWidth(width);
            nameGo.GetComponent<RectTransform>().sizeDelta = new Vector2(nameW, RowHeight);
            nameGo.AddComponent<LayoutElement>().preferredWidth = nameW;

            // 出口
            var expGo = UIHelpers.CreateText(b.Export.ToString("F0"), row.transform, TextSize,
                UIStyles.TextSecondary, _gameFont, RowHeight, "Exp");
            expGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            expGo.GetComponent<RectTransform>().sizeDelta = new Vector2(NumW, RowHeight);
            expGo.AddComponent<LayoutElement>().preferredWidth = NumW;

            // 进口
            var impGo = UIHelpers.CreateText(b.Import.ToString("F0"), row.transform, TextSize,
                UIStyles.TextSecondary, _gameFont, RowHeight, "Imp");
            impGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            impGo.GetComponent<RectTransform>().sizeDelta = new Vector2(NumW, RowHeight);
            impGo.AddComponent<LayoutElement>().preferredWidth = NumW;

            // 净额（顺差绿 +，逆差红 −，零弱色）
            Color netColor = b.Net > 0 ? UIStyles.Positive : b.Net < 0 ? UIStyles.Negative : Muted;
            string netStr = b.Net > 0 ? "+" + b.Net.ToString("F0") : b.Net.ToString("F0");
            var netGo = UIHelpers.CreateText(netStr, row.transform, TextSize, netColor, _gameFont, RowHeight, "Net");
            netGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
            netGo.GetComponent<Text>().fontStyle = FontStyle.Bold;
            netGo.GetComponent<RectTransform>().sizeDelta = new Vector2(NumW, RowHeight);
            netGo.AddComponent<LayoutElement>().preferredWidth = NumW;
        }
    }
}
