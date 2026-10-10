using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 【W3 结构收敛】补丁安装总入口。
    ///
    /// 为什么需要它：本模组有 4 处 Harmony 补丁，原先散落在 4 个文件、
    /// 由 EconomyTickRunner 首帧依次手工调用，各自的 try/catch 与日志格式也不统一
    /// —— 想回答"到底装了哪些补丁、有没有装失败"需要翻 4 个文件。
    ///
    /// 现在：**一处登记、一处安装、一处报告**。
    /// 每个补丁的目标方法/类型/用途与风险，见 docs/patches.md。
    ///
    /// 约定：
    ///   · 新增补丁 → 在 Registry 追加一行，并在 docs/patches.md 补对应条目
    ///     （tools/Test-PatchesManifest.ps1 会校验两处一致）；
    ///   · 单个补丁失败**不得**影响其余补丁（补丁是可选增强，不能因它挂掉整个模组）。
    /// </summary>
    internal static class PatchInstaller
    {
        private static bool _installed;

        /// <summary>补丁登记表：(标识, 用途, 安装委托)。</summary>
        private static readonly (string Id, string Purpose, System.Action Install)[] Registry =
        {
            ("OptimeCompatibility",       "Optime 程序集防御层（Transpiler + Finalizer 兜底）", () => Services.OptimeCompatibility.TryInstall()),
            ("KingdomWindowIntegration",  "原版国家窗口注入内阁入口（Postfix StatsWindow.create）", () => KingdomWindowIntegration.TryInstall()),
            ("LawSave",                   "法典数据随存档持久化（SaveManager 存 / MapBox 读）",     () => LawSave.TryInstall()),
            ("NationSave",                "中央银行家数据随存档持久化（含 Sidecar 兜底）",         () => NationSave.TryInstall()),
        };

        /// <summary>已尝试安装（幂等标志）。</summary>
        public static bool Installed => _installed;

        /// <summary>补丁数量（供门禁/诊断）。</summary>
        public static int Count => Registry.Length;

        /// <summary>补丁标识清单（供诊断/文档一致性校验）。</summary>
        public static string[] Ids()
        {
            var ids = new string[Registry.Length];
            for (int i = 0; i < Registry.Length; i++) ids[i] = Registry[i].Id;
            return ids;
        }

        /// <summary>安装全部补丁（幂等；单点失败不影响其余）。</summary>
        public static void InstallAll()
        {
            if (_installed) return;
            _installed = true;
            int ok = 0, fail = 0;
            for (int i = 0; i < Registry.Length; i++)
            {
                var p = Registry[i];
                try
                {
                    p.Install();
                    ok++;
                }
                catch (System.Exception e)
                {
                    fail++;
                    Debug.LogError("[ClassicalEconomics] 补丁安装失败 [" + p.Id + "] " + p.Purpose + " :: " + e.Message);
                }
            }
            Debug.Log("[ClassicalEconomics] 补丁安装完成: 成功 " + ok + " / 失败 " + fail + " / 共 " + Registry.Length);
        }
    }
}
