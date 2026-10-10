using System;
using System.Reflection;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 【W1.5】把原「干预王国」工具按钮（打开国家选择列表）拆成两个**真正的权能**（GodPower）：
    ///   · 煽动 economy_incite   —— 选中后点击地图上的王国，立即引发叛乱
    ///   · 镇压 economy_suppress —— 选中后点击地图上的王国，平息其社会动荡
    /// 交互从「开列表 -> 找国家 -> 点小按钮」变成「选权能 -> 点地图」，与游戏其他权能一致。
    ///
    /// 图标：GodPower.getIconSprite() 内部拼 "ui/Icons/" + path_icon，所以 path_icon 只写图标名；
    /// 实际贴图来自 GameResources/ui/Icons/iconIncite.png 与 iconSuppress.png（模组资源随模组注册）。
    /// 标签按钮的图标另由 UI/EconomyUI 通过 IconLoader 传入自定义 Sprite。
    /// </summary>
    internal static class KingdomPowers
    {
        public const string InciteId = "economy_incite";
        public const string SuppressId = "economy_suppress";

        // City.kingdom 是 internal 字段（编译期不可见）——与 WorldIdentity 读 MapBox.map_stats 同款做法：
        // 反射一次并缓存 FieldInfo。
        private static FieldInfo _cityKingdomField;
        private static bool _probed;

        /// <summary>注册两个权能资产（幂等；失败只记日志，不影响模组加载）。</summary>
        public static void Register()
        {
            AddPower(InciteId, "iconIncite", OnIncite);
            AddPower(SuppressId, "iconSuppress", OnSuppress);
        }

        private static void AddPower(string id, string iconName, PowerActionWithID action)
        {
            try
            {
                var power = new GodPower
                {
                    id = id,
                    name = id,                      // 本地化键：Locales/*.json 的 id 与 id_description
                    path_icon = iconName,
                    type = PowerActionType.PowerSpecial,
                    click_action = action,
                    unselect_when_window = false,   // 点完不自动取消选中，便于连续对多个王国操作
                    allow_unit_selection = false,
                    map_modes_switch = false,
                    hold_action = false,
                    click_interval = 0f,
                    particle_interval = 0f
                };
                AssetManager.powers.add(power);
                Debug.Log("[ClassicalEconomics] 权能已注册: " + id);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 权能注册失败 " + id + ": " + e.Message);
            }
        }

        /// <summary>从点击的地块解析出王国；无城市/无王国返回 null。</summary>
        private static Kingdom KingdomAt(WorldTile pTile)
        {
            try
            {
                if (pTile == null) return null;
                var zone = pTile.zone;
                if (zone == null) return null;
                var city = zone.city;
                if (city == null) return null;
                if (!_probed)
                {
                    _probed = true;
                    _cityKingdomField = typeof(City).GetField("kingdom",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
                return _cityKingdomField != null ? _cityKingdomField.GetValue(city) as Kingdom : null;
            }
            catch (Exception) { return null; }
        }

        /// <summary>煽动权能：点击地图上的王国 -> 立即触发叛乱。</summary>
        private static bool OnIncite(WorldTile pTile, string pPowerID)
        {
            var kingdom = KingdomAt(pTile);
            if (kingdom == null || kingdom.data == null)
            {
                GameHelpers.NotifyLocalized("power_nokingdom");
                return false;
            }
            string name = GameHelpers.SafeKingdomName(kingdom);
            int n = UnrestEngine.Incite(kingdom);
            GameHelpers.NotifyLocalized(n > 0 ? "picker_done" : "picker_failed", name);
            if (n > 0) RefreshAfterManualAction();
            return n > 0;
        }

        /// <summary>镇压权能：点击地图上的王国 -> 平息其社会动荡。</summary>
        private static bool OnSuppress(WorldTile pTile, string pPowerID)
        {
            var kingdom = KingdomAt(pTile);
            if (kingdom == null || kingdom.data == null)
            {
                GameHelpers.NotifyLocalized("power_nokingdom");
                return false;
            }
            string name = GameHelpers.SafeKingdomName(kingdom);
            int n = UnrestEngine.Suppress(kingdom);
            GameHelpers.NotifyLocalized(n > 0 ? "picker_done_suppress" : "picker_failed_suppress", name);
            if (n > 0) RefreshAfterManualAction();
            return n > 0;
        }

        /// <summary>手动操作后同步刷新统计。
        /// S2 约束：仅当无在途周期时执行 —— 直接 Collect() 会投递后台周期但无人消费
        /// （_cyclePending 未置位），导致 _posting 永久滞留、年度周期停摆。与选择列表按钮同一处理。</summary>
        private static void RefreshAfterManualAction()
        {
            try
            {
                if (!TradeSimulationWorker.IsBusy())
                {
                    DataCollector.Collect(applySideEffects: false, postCycle: false);
                    TradeSimulationWorker.ComputeAndConsumeSync(advanceCycle: false);
                }
                EconomyMod.UI.EconomyUI.RefreshOverview();
            }
            catch (Exception) { }
        }
    }
}
