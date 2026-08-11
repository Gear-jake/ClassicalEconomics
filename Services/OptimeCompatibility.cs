using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace EconomyMod.Services
{
    /// <summary>
    /// Optime 兼容层（v0.8.2）：
    /// 第三方优化模组 Optime (ApexLite) 的 ActorJobFlatten 功能用扁平循环重写
    /// BatchActors.u4_deadCheck（将死亡/饥饿/冻结/寻敌/任务/AI 等 12 项逻辑内联为
    /// 单循环），但其循环未跳过共享数组池中的 null 槽位（actor 死亡/移除后槽位
    /// 保留 null），在死亡高发期（战争/饥荒/经济危机）会抛 NullReferenceException
    /// 直接崩溃游戏。
    ///
    /// 本层【不修改 Optime 任何文件】，纯运行时防御（产品级兼容）：
    /// 1) Transpiler（主防线）：对 Optime 已编译的
    ///    ActorJobFlatten.BatchActors_u4_deadCheck_Prefix 方法体注入 null 防御——
    ///    在循环体 actor 加载后插入「actor 为 null 则跳过本次迭代」的分支。
    ///    Optime 逻辑 100% 原样执行（优化效果保留），仅补齐其缺失的空值防御；
    ///    若 Optime 更新导致 IL 模式不匹配，Transpiler 安全回退（不修改任何指令）。
    /// 2) Finalizer（第二道防线）：给 BatchActors.u4_deadCheck 挂 Harmony Finalizer，
    ///    仅吞掉来自 ActorJobFlatten 帧的 NRE（该帧批次跳过、下一帧重试），其余
    ///    异常照常抛出；日志 30 秒限频防刷屏。
    /// 3) 未装 Optime 时零开销（不挂任何 patch，原版批处理照常运行）。
    /// </summary>
    public static class OptimeCompatibility
    {
        private static bool _checked;
        private static float _lastGuardLogTime = -999f;

        /// <summary>
        /// 检测 Optime 是否已加载并安装防御层（幂等，应在所有模组加载完成后调用，
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
                    Debug.Log("[ClassicalEconomics] 未检测到 Optime，跳过兼容层安装（原版批处理正常运行）");
                    return;
                }

                var harmony = new Harmony("ClassicalEconomics.OptimeCompatibility");

                // 1) Transpiler 主防线：给 Optime 的扁平循环 Prefix 注入 null 防御
                bool transpilerInstalled = false;
                try
                {
                    var optimeType = AccessTools.TypeByName("Optime.Optimizations.ActorJobFlatten");
                    if (optimeType != null)
                    {
                        var prefixMethod = AccessTools.Method(optimeType, "BatchActors_u4_deadCheck_Prefix",
                            new[] { typeof(BatchActors).MakeByRefType() });
                        if (prefixMethod != null)
                        {
                            harmony.Patch(prefixMethod, transpiler: new HarmonyMethod(
                                typeof(OptimeCompatibility).GetMethod(nameof(NullGuardTranspiler),
                                    BindingFlags.Static | BindingFlags.NonPublic)));
                            transpilerInstalled = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[ClassicalEconomics] Optime Transpiler 安装失败，仅 Finalizer 兜底: " + e.Message);
                }

                // 2) Finalizer 第二道防线：Optime Prefix 抛 NRE 时吞掉防崩溃
                bool finalizerInstalled = false;
                try
                {
                    // BatchActors 在全局命名空间（Optime 源码同样直接引用，无需 using）
                    var target = AccessTools.Method(typeof(BatchActors), "u4_deadCheck");
                    if (target != null)
                    {
                        harmony.Patch(target, finalizer: new HarmonyMethod(
                            typeof(OptimeCompatibility).GetMethod(nameof(GuardFinalizer),
                                BindingFlags.Static | BindingFlags.NonPublic)));
                        finalizerInstalled = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[ClassicalEconomics] Optime Finalizer 安装失败: " + e.Message);
                }

                Debug.Log("[ClassicalEconomics] Optime 兼容层已安装（Transpiler null 防御=" +
                          transpilerInstalled + ", Finalizer 兜底=" + finalizerInstalled + "）");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] Optime 兼容层安装失败: " + e.Message);
            }
        }

        /// <summary>
        /// Transpiler：在 Optime 扁平循环的「actor 加载」后插入空值检查——
        ///   actor = actors[i];
        ///   if (actor == null) goto 循环迭代尾;   ← 注入
        ///   actor.u1_checkInside(elapsed);
        ///   ...
        /// 模式：ldelem.ref → stloc(actor) 之后；迭代尾 = ldloc i; ldc.i4.1; add; stloc i
        /// （即 for 循环的 i++ 增量序列）。
        /// 任何匹配失败均返回原指令（安全回退，由 Finalizer 兜底）。
        /// </summary>
        private static IEnumerable<CodeInstruction> NullGuardTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            try
            {
                // 找 actors[i] -> actor 的序列：ldelem.ref + stloc(任意变体)
                var matcher = new CodeMatcher(codes);
                matcher.MatchForward(false, new CodeMatch(OpCodes.Ldelem_Ref));
                if (!matcher.IsValid) return codes;

                matcher.Advance(1);
                var stlocOp = matcher.Opcode;
                int actorVar;
                if (stlocOp == OpCodes.Stloc || stlocOp == OpCodes.Stloc_S)
                {
                    actorVar = (int)matcher.Operand;
                }
                else if (stlocOp == OpCodes.Stloc_0) actorVar = 0;
                else if (stlocOp == OpCodes.Stloc_1) actorVar = 1;
                else if (stlocOp == OpCodes.Stloc_2) actorVar = 2;
                else if (stlocOp == OpCodes.Stloc_3) actorVar = 3;
                else return codes; // 不是 stloc，模式不匹配

                // 从 stloc 之后找 for 循环迭代尾（i++ 增量序列），确保定位正确：
                //   ldloc i; ldc.i4.1; add; stloc i; ldloc i; ldloc count; blt 循环头
                int stlocPos = matcher.Pos;
                matcher.MatchForward(false,
                    new CodeMatch(IsLdloc),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(IsStloc),
                    new CodeMatch(IsLdloc),
                    new CodeMatch(IsLdloc),
                    new CodeMatch(c => c.opcode == OpCodes.Blt || c.opcode == OpCodes.Blt_S));
                if (!matcher.IsValid) return codes;

                // 在循环尾增量序列第一条指令处建标签（null 时跳过本次迭代）
                // 注意：必须在插入指令前创建（位置基于未插入的指令列表）
                Label tailLabel;
                matcher.CreateLabelAt(matcher.Pos, out tailLabel);

                // 回到 stloc(actor) 之后插入：ldloc actorVar; brfalse tailLabel
                matcher.Start();
                matcher.Advance(stlocPos + 1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, actorVar) { labels = matcher.Labels },
                    new CodeInstruction(OpCodes.Brfalse, tailLabel));

                Debug.Log("[ClassicalEconomics] Optime Transpiler 已注入 null 防御（actor 局部变量 #" + actorVar + "）");
                return matcher.Instructions();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] Optime Transpiler 失败，退回 Finalizer 兜底: " + e.Message);
                return codes;
            }
        }

        private static bool IsLdloc(CodeInstruction c)
        {
            byte v = (byte)c.opcode.Value;
            return c.opcode == OpCodes.Ldloc || c.opcode == OpCodes.Ldloc_S ||
                   (v >= OpCodes.Ldloc_0.Value && v <= OpCodes.Ldloc_3.Value);
        }

        private static bool IsStloc(CodeInstruction c)
        {
            byte v = (byte)c.opcode.Value;
            return c.opcode == OpCodes.Stloc || c.opcode == OpCodes.Stloc_S ||
                   (v >= OpCodes.Stloc_0.Value && v <= OpCodes.Stloc_3.Value);
        }

        /// <summary>
        /// Harmony Finalizer（第二道防线）：仅吞掉 ActorJobFlatten 扁平循环抛出的 NRE，
        /// 其余异常照常抛出。返回 null = 异常已处理（不抛出）；返回非 null = 抛出该异常。
        /// 正常路径零开销（Finalizer 仅在异常时执行）；异常时日志 30 秒限频防刷屏。
        /// </summary>
        private static Exception GuardFinalizer(Exception __exception)
        {
            if (__exception == null) return null;
            if (__exception is NullReferenceException &&
                __exception.StackTrace != null &&
                __exception.StackTrace.IndexOf("ActorJobFlatten", StringComparison.Ordinal) >= 0)
            {
                if (Time.unscaledTime - _lastGuardLogTime > 30f)
                {
                    _lastGuardLogTime = Time.unscaledTime;
                    Debug.LogWarning("[ClassicalEconomics] 捕获 Optime ActorJobFlatten NRE（Finalizer 兜底生效，本帧批次跳过）");
                }
                return null;
            }
            return __exception;
        }
    }
}
