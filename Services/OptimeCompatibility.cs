using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EconomyMod.Services
{
    /// <summary>
    /// Optime 兼容层（v0.8.1）：
    /// 第三方优化模组 Optime (ApexLite) 的 ActorJobFlatten 功能用扁平循环重写
    /// BatchActors.u4_deadCheck，但其循环未跳过数组中的 null 槽位
    /// （actor 死亡/移除后 BatchActors 的共享数组池保留 null），在死亡高发期
    /// （战争/饥荒/经济危机）会抛 NullReferenceException 直接崩溃游戏。
    /// 本兼容层在检测到 Optime 已加载时，给 BatchActors.u4_deadCheck 挂一个
    /// Harmony Finalizer：仅吞掉来自 ActorJobFlatten 帧的 NRE（该帧该批次
    /// actor 更新跳过、下一帧 BatchActors 重试），其余异常照常抛出。
    /// 正常路径零开销（Finalizer 仅在异常时执行）。
    /// </summary>
    public static class OptimeCompatibility
    {
        private static bool _checked;
        private static bool _installed;

        /// <summary>
        /// 检测 Optime 是否已加载并安装兜底 Finalizer（幂等，应在所有模组加载完成后调用，
        /// 例如经济 TickRunner 的首帧 Update）。
        /// </summary>
        public static void TryInstall()
        {
            if (_checked) return;
            _checked = true;
            try
            {
                // 检测 Optime 程序集（源码模组编译产物 APEXLITE_OPTIME.dll，程序集名含 Optime）
                bool optimeLoaded = AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => a.GetName().Name != null &&
                              a.GetName().Name.IndexOf("Optime", StringComparison.OrdinalIgnoreCase) >= 0);
                if (!optimeLoaded)
                {
                    Debug.Log("[ClassicalEconomics] 未检测到 Optime，跳过兼容层安装");
                    return;
                }

                // BatchActors 在全局命名空间（Optime 源码同样直接引用，无需 using）
                var target = AccessTools.Method(typeof(BatchActors), "u4_deadCheck");
                if (target == null)
                {
                    Debug.LogWarning("[ClassicalEconomics] 反射 BatchActors.u4_deadCheck 失败，兼容层跳过");
                    return;
                }

                var harmony = new Harmony("ClassicalEconomics.OptimeCompatibility");
                harmony.Patch(target, finalizer: new HarmonyMethod(
                    typeof(OptimeCompatibility).GetMethod(nameof(GuardFinalizer),
                        BindingFlags.Static | BindingFlags.NonPublic)));
                _installed = true;
                Debug.Log("[ClassicalEconomics] Optime 兼容兜底已安装（BatchActors.u4_deadCheck Finalizer）");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] Optime 兼容层安装失败: " + e.Message);
            }
        }

        /// <summary>
        /// Harmony Finalizer：仅吞掉 ActorJobFlatten 扁平循环抛出的 NRE，其余异常照常抛出。
        /// 返回 null = 异常已处理（不抛出）；返回非 null = 抛出该异常。
        /// </summary>
        private static Exception GuardFinalizer(Exception __exception)
        {
            if (__exception == null) return null;
            if (__exception is NullReferenceException &&
                __exception.StackTrace != null &&
                __exception.StackTrace.IndexOf("ActorJobFlatten", StringComparison.Ordinal) >= 0)
            {
                Debug.LogWarning("[ClassicalEconomics] 捕获 Optime ActorJobFlatten NRE（已兜底，本帧批次更新跳过）");
                return null;
            }
            return __exception;
        }
    }
}
