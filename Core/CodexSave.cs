using System.Collections.Generic;
using EconomyMod.Models;
using HarmonyLib;

namespace EconomyMod.Core
{
    /// <summary>
    /// 法典存档持久化：把各国法律/国策/个性写入王国 data（rb_codex_* 键），
    /// 经 MapBox.saveSave（前缀写盘）与 loadSave（后缀读回）手动 Harmony 补丁——
    /// 注解补丁对预编译 DLL 不可靠（与 KingdomWindowIntegration 同教训），故首帧手动 Patch。
    /// 任一环节异常则回退"本局记忆"（日志警告一次，不阻塞游戏）。
    /// </summary>
    public static class CodexSave
    {
        private const string HarmonyId = "com.classicaleconomics.codexsave";
        private static bool _installed;
        private static bool _loadWarned;

        /// <summary>幂等安装；由 EconomyTickRunner 首帧调用。</summary>
        public static void TryInstall()
        {
            if (_installed) return;
            _installed = true;
            try
            {
                var save = AccessTools.Method(typeof(MapBox), "saveSave");
                var load = AccessTools.Method(typeof(MapBox), "loadSave");
                if (save == null || load == null)
                {
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 法典存档：MapBox saveSave/loadSave 未找到，回退本局记忆");
                    return;
                }
                var harmony = new Harmony(HarmonyId);
                harmony.Patch(save, prefix: new HarmonyMethod(typeof(CodexSave), nameof(SavePrefix)));
                harmony.Patch(load, postfix: new HarmonyMethod(typeof(CodexSave), nameof(LoadPostfix)));
                UnityEngine.Debug.Log("[ClassicalEconomics] 法典存档补丁已安装（saveSave/loadSave）");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[ClassicalEconomics] 法典存档补丁安装失败: " + e.Message);
            }
        }

        /// <summary>写盘前把内存法典状态同步进王国 data。</summary>
        private static void SavePrefix()
        {
            try
            {
                if (World.world == null) return;
                foreach (var k in GameHelpers.KingdomSnapshot())
                {
                    if (k == null || k.data == null) continue;
                    var st = CodexEngine.Get(k.data.id);
                    for (int i = 0; i < CodexEngine.LawKeys.Length; i++)
                        try { k.data.set("rb_codex_law_" + i, st.LawLevels[i].ToString()); } catch (System.Exception) { }
                    for (int i = 0; i < CodexEngine.PolicyKeys.Length; i++)
                        try { k.data.set("rb_codex_policy_" + i, st.PolicyLevels[i].ToString()); } catch (System.Exception) { }
                    try { k.data.set("rb_codex_style", st.Style.ToString()); } catch (System.Exception) { }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>读档后从王国 data 恢复法典状态（缺失＝默认，所有档位重算聚合）。</summary>
        private static void LoadPostfix()
        {
            try
            {
                if (World.world == null) return;
                foreach (var k in GameHelpers.KingdomSnapshot())
                {
                    if (k == null || k.data == null) continue;
                    var st = CodexEngine.Get(k.data.id);
                    bool any = false;
                    for (int i = 0; i < CodexEngine.LawKeys.Length; i++)
                    {
                        string v = null;
                        try { k.data.get("rb_codex_law_" + i, out v); } catch (System.Exception) { }
                        int level;
                        if (v != null && int.TryParse(v, out level))
                        {
                            st.LawLevels[i] = System.Math.Max(0, System.Math.Min(CodexEngine.LawTiers - 1, level));
                            any = true;
                        }
                    }
                    for (int i = 0; i < CodexEngine.PolicyKeys.Length; i++)
                    {
                        string v = null;
                        try { k.data.get("rb_codex_policy_" + i, out v); } catch (System.Exception) { }
                        int level;
                        if (v != null && int.TryParse(v, out level))
                        {
                            st.PolicyLevels[i] = System.Math.Max(0, System.Math.Min(CodexEngine.PolicyTiers - 1, level));
                            any = true;
                        }
                    }
                    string s = null;
                    try { k.data.get("rb_codex_style", out s); } catch (System.Exception) { }
                    int style;
                    if (s != null && int.TryParse(s, out style))
                        st.Style = System.Math.Max(0, System.Math.Min(CodexEngine.StyleCount - 1, style));
                    if (any) CodexEngine.RecomputeMods(k.data.id, st);
                }
            }
            catch (System.Exception)
            {
                if (!_loadWarned)
                {
                    _loadWarned = true;
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 法典存档恢复失败，本局按默认法典重新演化");
                }
            }
        }
    }
}
