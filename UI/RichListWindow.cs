using UnityEngine;
using EconomyMod.Core;

namespace EconomyMod.UI
{
    /// <summary>
    /// 全球富豪榜悬浮窗：与 GDP 概览悬浮窗同款交互逻辑——非模态不暂停游戏、
    /// 标题栏可拖拽、四边/四角可缩放、右上角 × 关闭、Tab 皇冠按钮切换显隐；
    /// 点击空白处不会关闭。实时列出财富（money+loot）前 10 的存活开智生物。
    /// </summary>
    public class RichListWindow : FloatingWindow
    {
        private static RichListWindow _instance;

        private const float PanelWidth = 340f;
        private const float PanelHeight = 470f;

        private static readonly Color Bg           = new Color(0.02f, 0.02f, 0.05f, 0.92f);
        private static readonly Color TextColor    = Color.white;
        private static readonly Color HeaderColor  = new Color(1f, 0.85f, 0.35f);
        private static readonly Color SubColor     = new Color(0.75f, 0.75f, 0.8f);
        private static readonly Color GoldRow      = new Color(1f, 0.9f, 0.5f);
        private static readonly Color DividerColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

        public static RichListWindow Instance => _instance;

        // ===== FloatingWindow 配置 =====
        protected override string WindowName => "EconomyRichList";
        protected override float SortingOrder => 10000;
        protected override string TitleKey => "rich_title";
        protected override Vector2 AnchorMin => new Vector2(1f, 0.5f);
        protected override Vector2 AnchorMax => new Vector2(1f, 0.5f);
        protected override Vector2 Pivot => new Vector2(0f, 0.5f);
        protected override Vector2 AnchoredPosition => new Vector2(-(PanelWidth + 16f), 0f);
        protected override Vector2 Size => new Vector2(PanelWidth, PanelHeight);
        protected override Color BgColor => Bg;

        public static void Create()
        {
            if (_instance != null) return;
            _instance = CreateWindow<RichListWindow>("EconomyRichList");
        }

        /// <summary>重建内容（每次打开或每年周期刷新时调用，拉取实时数据）。</summary>
        public override void RefreshNow()
        {
            ClearContent();
            BuildList();
        }

        private void BuildList()
        {
            // 直接使用采集器已维护好的 Top10 缓存，避免每年再次全量遍历世界单位
            var entries = DataCollector.TopRich;

            // 副标题：当前年份 + 文明数
            int year = 0;
            try { year = EconomyModMain.GetCurrentGameYear(); } catch (System.Exception) { }
            AddLine(UIHelpers.Lf("rich_subtitle", year, EconomyEngine.AliveActorCount), SubColor, 12f);

            AddDivider(DividerColor);

            if (entries.Count == 0)
            {
                // 区分"未进入世界"与"世界内暂无文明"
                bool noWorld = false;
                try { noWorld = World.world == null || World.world.units == null; } catch (System.Exception) { noWorld = true; }
                AddLine(UIHelpers.L(noWorld ? "rich_noworld" : "rich_empty"), new Color(0.7f, 0.7f, 0.7f), 12f);
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                string kingdom = string.IsNullOrEmpty(e.Kingdom) ? UIHelpers.L("rich_nok") : e.Kingdom;
                // 第1名金色高亮，其余白色
                var color = i == 0 ? GoldRow : TextColor;
                AddLine(UIHelpers.Lf("rich_row", i + 1, e.Name, kingdom, e.Wealth.ToString("F0")), color, 12f);
            }
        }
    }
}
