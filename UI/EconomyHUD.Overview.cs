using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;

namespace EconomyMod.UI
{
    /// <summary>【W2 分层】EconomyHUD 的概览层：周期/阶段徽章、关键统计卡片、会话时长与阶段名格式化。</summary>
    public partial class EconomyHUD : FloatingWindow
    {
        private void BuildOverview()
        {
            float contentW = Mathf.Max(150f, _panelRect.rect.width - Padding * 2f - 16f);
            var phase = EconomyCycleModulator.CurrentPhase;

            // 周期 + 阶段徽章
            AddLine(UIHelpers.Lf("overview_cycle", EconomyEngine.CycleIndex), true);
            _lines.Add(UIComponents.CreatePhaseBadge(_content.transform, phase,
                PhaseName(phase), _gameFont, contentW * 0.5f, Fs(1f)));
            AddLine(UIHelpers.Lf("cycle_detail",
                EconomyCycleModulator.PhaseDuration,
                EconomyCycleModulator.GrowthRate.ToString("+0.0%;-0.0%"),
                UIHelpers.FormatCompact((long)EconomyCycleModulator.BubbleValue)),
                color: new Color(0.8f, 0.9f, 0.7f));
            AddLine("");

            // 核心指标卡（单行，窗口缩放保证不溢出；v0.9.1 恢复单行布局）
            // 不显示人口：配合人口倍数类模组时单位数会被放大，数字失真且出戏（v0.94 移除）
            var stats = new (string, string, Color)[]
            {
                ("GDP", UIHelpers.FormatCompact((long)EconomyEngine.GlobalGDP), UIStyles.Gold),
                (UIHelpers.L("col_avg"), EconomyEngine.AvgWealth.ToString("F1"), UIStyles.Info),
                (UIHelpers.L("col_gini"), EconomyEngine.GiniCoefficient.ToString("F3"), GiniColor(EconomyEngine.GiniCoefficient)),
                (UIHelpers.L("stat_bubble"), UIHelpers.FormatCompact((long)EconomyCycleModulator.BubbleValue), UIStyles.Warning)
            };
            _lines.Add(UIComponents.CreateStatGrid(_content.transform, stats, _gameFont, contentW, Fs(1f)));

            // 内存状态行（清理开启时显示）：把"模组+游戏共享的托管堆"与"游戏本体 Unity 原生内存"
            // 分项展示，用于判断内存增长来源；上次清理信息来自 MemoryCleanupEngine
            var mcCfg = UnrestConfig.Instance;
            if (mcCfg != null && mcCfg.MemoryCleanupEnabled)
            {
                string cleanupLine = MemoryCleanupEngine.LastCleanupRealtime < 0f
                    ? UIHelpers.L("hud_mem_cleanup_pending")
                    : UIHelpers.Lf("hud_mem_cleanup", FormatSessionTime(MemoryCleanupEngine.LastCleanupRealtime),
                        MemoryCleanupEngine.FormatMb(System.Math.Max(0L, MemoryCleanupEngine.LastFreedBytes)),
                        MemoryCleanupEngine.LastShrunkCount.ToString());
                _lines.Add(UIComponents.CreateStatusRow(_content.transform, cleanupLine,
                    UIStyles.TextMuted, _gameFont, contentW, Fs(1f)));

                long unityUsed = MemoryCleanupEngine.UnityUsedBytes;
                long unityReserved = MemoryCleanupEngine.UnityReservedBytes;
                _lines.Add(UIComponents.CreateStatusRow(_content.transform,
                    UIHelpers.Lf("hud_mem_usage",
                        MemoryCleanupEngine.FormatMb(MemoryCleanupEngine.ManagedHeapBytes),
                        unityUsed < 0 ? UIHelpers.L("hud_mem_na") : MemoryCleanupEngine.FormatMb(unityUsed),
                        unityReserved < 0 ? UIHelpers.L("hud_mem_na") : MemoryCleanupEngine.FormatMb(unityReserved)),
                    UIStyles.TextMuted, _gameFont, contentW, Fs(1f)));
                AddLine("");
            }

            // 王国排行
            _lines.Add(UIComponents.CreateSectionHeader(_content.transform,
                UIHelpers.L("overview_kingdoms"), _gameFont, contentW, Fs(1f)));
            _lines.Add(UIComponents.CreateKingdomHeader(_content.transform, _gameFont, contentW, Fs(1f)));
            var top = EconomyEngine.TopKingdoms(8);
            if (top.Count == 0)
            {
                AddLine(UIHelpers.L("overview_no_kingdom"), color: UIStyles.TextMuted);
            }
            int rank = 1;
            foreach (var k in top)
            {
                _lines.Add(UIComponents.CreateKingdomRow(_content.transform, rank, k.KingdomName,
                    UIHelpers.FormatCompact(k.GDP), k.AvgWealth.ToString("F1"), k.GiniCoefficient.ToString("F2"),
                    _gameFont, contentW, rank == 1, Fs(1f)));
                rank++;
            }

            // 社会动荡状态
            _lines.Add(UIComponents.CreateSectionHeader(_content.transform,
                UIHelpers.L("unrest_state_title"), _gameFont, contentW, Fs(1f)));
            int stateCount = 0;
            foreach (var k in top)
            {
                int st = UnrestEngine.GetState(k.KingdomId, out int elapsed);
                if (st == 1)
                {
                    _lines.Add(UIComponents.CreateStatusRow(_content.transform,
                        UIHelpers.Lf("unrest_state_accum", k.KingdomName, elapsed),
                        UIStyles.Warning, _gameFont, contentW, Fs(1f)));
                    stateCount++;
                }
                else if (st == 2)
                {
                    _lines.Add(UIComponents.CreateStatusRow(_content.transform,
                        UIHelpers.Lf("unrest_state_active", k.KingdomName),
                        UIStyles.Danger, _gameFont, contentW, Fs(1f)));
                    stateCount++;
                }
                else if (st == 3)
                {
                    _lines.Add(UIComponents.CreateStatusRow(_content.transform,
                        UIHelpers.Lf("unrest_state_uprising", k.KingdomName),
                        UIStyles.Negative, _gameFont, contentW, Fs(1f)));
                    stateCount++;
                }
            }
            if (stateCount == 0)
            {
                AddLine(UIHelpers.L("unrest_state_none"), color: UIStyles.TextMuted);
            }
            AddLine(UIHelpers.Lf("unrest_state_threshold", UnrestConfig.Instance.GiniThreshold.ToString("F3")),
                color: new Color(0.7f, 0.85f, 0.7f));
        }

        /// <summary>基尼语义色（≥0.7 红 / ≥0.55 琥珀 / 其他弱色）。</summary>
        private static Color GiniColor(float gini)
        {
            return gini >= 0.7f ? UIStyles.Danger : gini >= 0.55f ? UIStyles.Warning : UIStyles.TextSecondary;
        }

        /// <summary>会话运行时长（Time.realtimeSinceStartup 秒）格式化为 HH:mm:ss 或 mm:ss。</summary>
        private static string FormatSessionTime(float realtimeSeconds)
        {
            int t = Mathf.Max(0, (int)realtimeSeconds);
            int h = t / 3600, m = (t % 3600) / 60, s = t % 60;
            return h > 0 ? $"{h:D2}:{m:D2}:{s:D2}" : $"{m:D2}:{s:D2}";
        }

        /// <summary>经济周期阶段名（本地化）。</summary>
        private static string PhaseName(EconomyPhase p)
        {
            switch (p)
            {
                case EconomyPhase.Boom:       return UIHelpers.L("cycle_phase_boom");
                case EconomyPhase.Recession:  return UIHelpers.L("cycle_phase_recession");
                case EconomyPhase.Depression: return UIHelpers.L("cycle_phase_depression");
                default:                      return UIHelpers.L("cycle_phase_recovery");
            }
        }
    }
}
