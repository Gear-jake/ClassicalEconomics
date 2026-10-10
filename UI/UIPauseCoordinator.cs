using UnityEngine;
using EconomyMod.Models;

namespace EconomyMod.UI
{
    /// <summary>
    /// 【W2.8】「打开面板时暂停游戏」协调器（配置键 `pause_on_panel_open`，**默认关**）。
    ///
    /// 为什么需要协调器而不是每个窗口自己暂停：
    ///   · 本模组同时可能有多个悬浮窗（内阁 / 事件流 / 抉择窗 / 富豪榜）；
    ///     任一窗口关闭就恢复时间，会让"还开着另一个窗口"时游戏偷偷跑起来。
    ///     ⇒ 用**可见窗口计数**：0 → 1 时暂停，1 → 0 时恢复。
    ///   · **绝不与玩家抢暂停**：只在"当前未暂停"时接管（`_ownsPause`），
    ///     玩家自己按的暂停我们一律不碰；玩家在我们暂停后再手动恢复，我们也不回抢。
    ///
    /// 与本体的一致性（反编译源码依据）：
    ///   · `Config.paused` 是纯静态属性（`Config.cs:263-273`），setter 只写 `_paused`，无副作用；
    ///   · `MapBox.cs:1692`：`_is_paused = Config.paused || ScrollWindow.isWindowActive() || ...`
    ///     ⇒ 置位 `Config.paused` 就等于按了本体的暂停键；
    ///   · `PauseButton.Update()`（`PauseButton.cs:17-45`）每帧按 `Config.paused` 刷新图标
    ///     ⇒ 我们改状态后本体暂停按钮图标会自动同步，不会出现"图标与状态不符"。
    /// </summary>
    internal static class UIPauseCoordinator
    {
        private static int _visibleWindows;
        private static bool _ownsPause;   // 暂停是**我们**造成的（只有我们造成的才由我们恢复）

        /// <summary>某个悬浮窗可见性变化（由 FloatingWindow 的显示/隐藏路径统一调用）。</summary>
        public static void SetVisible(bool visible)
        {
            if (visible)
            {
                _visibleWindows++;
                if (_visibleWindows == 1) TryPause();
            }
            else
            {
                if (_visibleWindows > 0) _visibleWindows--;
                if (_visibleWindows == 0) RestorePause();
            }
        }

        /// <summary>世界退出 / 模组卸载：无条件归还暂停，避免把玩家永久停在暂停态。</summary>
        public static void ResetAll()
        {
            _visibleWindows = 0;
            RestorePause();
        }

        /// <summary>当前是否由本协调器持有暂停（诊断用）。</summary>
        public static bool OwnsPause { get { return _ownsPause; } }

        /// <summary>设置项「打开面板暂停游戏」被改动时调用：即时生效。
        /// · 关掉开关：若暂停由我们持有 → 立刻恢复；玩家自己的暂停不动。
        /// · 打开开关：若当前已有面板可见 → 立刻接管暂停。</summary>
        public static void OnSwitchChanged(bool enabled)
        {
            if (!enabled) { RestorePause(); return; }
            if (_visibleWindows > 0) TryPause();
        }

        private static void TryPause()
        {
            try
            {
                var cfg = UnrestConfig.Instance;
                if (cfg == null || !cfg.PauseOnPanelOpen) return;
                // 世界加载中不抢暂停（加载流程对暂停状态敏感）
                if (Config.worldLoading) return;
                // 玩家自己已经暂停 → 不接管，关闭时也就不会误帮玩家恢复
                if (Config.paused) return;
                Config.paused = true;
                _ownsPause = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 面板暂停失败（忽略）: " + e.Message);
            }
        }

        private static void RestorePause()
        {
            try
            {
                if (!_ownsPause) return;
                _ownsPause = false;
                Config.paused = false;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 面板恢复时间失败（忽略）: " + e.Message);
            }
        }
    }
}
