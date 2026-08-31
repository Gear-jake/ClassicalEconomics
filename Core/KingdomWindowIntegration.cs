using EconomyMod.Models;
using HarmonyLib;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.Core
{
    /// <summary>
    /// 原版国家界面入口（中央银行家）：照 PowerBox 的成熟模式——
    /// 补丁 StatsWindow.create（CityWindow/KingdomWindow 均派生自 StatsWindow），
    /// 在窗口 Background 内注入一个账本图标按钮；点击读取窗口当前展示的国家：
    /// 已认领 → 打开内阁；未认领 → 认领该国并打开内阁。
    /// 全部编译期类型化（与 XaviiNationTypes/PowerBox 同方式），原版结构变化时构建期即暴露。
    /// </summary>
    public static class KingdomWindowIntegration
    {
        private const string HarmonyId = "com.classicaleconomics.kingdomui";
        private const string ButtonName = "ClassicalEconomicsCabinetEntry";
        private static bool _installAttempted;

        /// <summary>手动打补丁（注解对预编译 DLL 模组不保证被 NML 应用——OptimeCompatibility 同款教训）。
        /// 由 EconomyTickRunner 首帧调用，幂等。</summary>
        public static void TryInstall()
        {
            if (_installAttempted) return;
            _installAttempted = true;
            try
            {
                var create = AccessTools.Method(typeof(StatsWindow), "create");
                if (create == null)
                {
                    Debug.LogWarning("[ClassicalEconomics] 国家界面入口：StatsWindow.create 未找到，入口禁用");
                    return;
                }
                new Harmony(HarmonyId).Patch(create,
                    postfix: new HarmonyMethod(typeof(KingdomWindowIntegration), nameof(Postfix)));

                // 已存在的窗口实例（可能在模组首帧之前就已创建）补注入
                int existing = 0;
                foreach (var w in Resources.FindObjectsOfTypeAll<StatsWindow>())
                {
                    try { if (InjectEntry(w)) existing++; } catch (System.Exception) { }
                }
                Debug.Log("[ClassicalEconomics] 国家界面入口补丁已安装（StatsWindow.create, static=" + create.IsStatic
                    + "，存量窗口补注入 " + existing + " 个）");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 国家界面入口补丁安装失败: " + e.Message);
            }
        }

        private static void Postfix(StatsWindow __instance, StatsWindow __result)
        {
            var window = __instance != null ? __instance : __result;
            if (window == null) return;
            try
            {
                InjectEntry(window);
                UpdateLawSummary(window, GetShownKingdom(window));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 国家界面入口注入失败: " + e.Message);
            }
        }

        /// <summary>向窗口注入入口按钮（幂等）；返回是否执行了注入。</summary>
        private static bool InjectEntry(StatsWindow window)
        {
            try
            {
                var cfg = UnrestConfig.Instance;
                if (cfg == null || !cfg.NationPlayEnabled || window == null) return false;

                Transform background = window.transform.Find("Background");
                if (background == null) background = FindChild(window.transform, "Background");
                if (background == null) return false;
                if (background.Find(ButtonName) != null) return false; // 该窗口已注入

                var btn = PowerButtonCreator.CreateSimpleButton(
                    ButtonName,
                    () => OnEntryClick(window),
                    UI.IconLoader.Get("ledger"),
                    background).gameObject;

                // 定位在 Background 面板右上角（锚定角落，不随窗口内容长度变化）
                var rt = btn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-12f, -12f);
                rt.sizeDelta = new Vector2(38f, 38f);
                // 原版窗口按钮底图（与窗口内其他方形按钮同款），保持"和他们一样"的外观
                var img = btn.GetComponent<Image>();
                var vanillaBg = Resources.Load<Sprite>("ui/window_back_button_bg");
                if (vanillaBg != null)
                {
                    img.sprite = vanillaBg;
                    img.type = Image.Type.Sliced;
                }
                var icon = btn.transform.Find("Icon");
                if (icon != null)
                {
                    icon.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 28f);
                    icon.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                }
                btn.GetComponent<Button>().transition = Selectable.Transition.None;

                // 法典摘要块（非操控国也能看到该国法律/国策状态；按钮下方，随窗口每开刷新）
                EnsureSummaryText(window, background);
                Debug.Log("[ClassicalEconomics] 国家界面入口按钮已注入: " + window.GetType().Name);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 国家界面入口注入失败: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 大地图快捷键 C（RulerBox 的 K 同款轮询模式）：鼠标悬停城市所属国家 →
        /// 未认领则认领并打开内阁，已认领直接打开。仅当世界存在且 UI 空闲时生效。
        /// </summary>
        public static void TryHotkeyOpen()
        {
            try
            {
                if (World.world == null) return;
                if (World.world.isBusyWithUI()) return;
                var cfg = UnrestConfig.Instance;
                if (cfg == null || !cfg.NationPlayEnabled) return;

                var tile = World.world.getMouseTilePos();
                if (tile == null) return;
                var zone = tile.zone;
                if (zone == null || zone.city == null) return;
                var kingdom = GetCityKingdom(zone.city);
                if (kingdom == null || kingdom.data == null) return;

                long kid = kingdom.data.id;
                if (NationEngine.NationKingdomId != 0 && NationEngine.NationKingdomId == kid)
                {
                    if (UI.CabinetWindow.Instance != null) UI.CabinetWindow.Instance.Show();
                    return;
                }

                int year;
                try { year = EconomyModMain.GetCurrentGameYear(); } catch (System.Exception) { year = 0; }
                if (NationEngine.Claim(kingdom, year, out _))
                {
                    if (UI.CabinetWindow.Instance != null) UI.CabinetWindow.Instance.Show();
                }
            }
            catch (System.Exception) { }
        }

        // ===== 法典摘要（原版窗口内展示任意国家的法律/国策状态）=====

        private const string SummaryName = "ClassicalEconomicsLawSummary";
        private static Text _summaryText;

        /// <summary>创建摘要文本对象（幂等）；锚定按钮左下方。</summary>
        private static void EnsureSummaryText(StatsWindow window, Transform background)
        {
            if (_summaryText != null) return;
            try
            {
                var go = new GameObject(SummaryName, typeof(RectTransform), typeof(Text));
                go.transform.SetParent(background, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-12f, -56f);
                rt.sizeDelta = new Vector2(190f, 72f);
                var t = go.GetComponent<Text>();
                t.font = LocalizedTextManager.current_font != null
                    ? LocalizedTextManager.current_font
                    : Resources.GetBuiltinResource<Font>("Arial.ttf");
                t.fontSize = 11;
                t.alignment = TextAnchor.UpperRight;
                t.color = new Color(0.95f, 0.88f, 0.6f, 0.95f);
                t.lineSpacing = 1.05f;
                _summaryText = t;
            }
            catch (System.Exception) { }
        }

        /// <summary>刷新摘要文本：国性 + 生效法律/国策条数 + 最高档 2 条法律。</summary>
        private static void UpdateLawSummary(StatsWindow window, Kingdom kingdom)
        {
            if (_summaryText == null) return;
            if (kingdom == null || kingdom.data == null)
            {
                _summaryText.gameObject.SetActive(false);
                return;
            }
            try
            {
                long kid = kingdom.data.id;
                int style = LawEngine.GetStyle(kid);
                string styleName = style >= 0 && style < LawEngine.StyleKeys.Length
                    ? Services.LocalizationService.Get(LawEngine.StyleKeys[style]) : "?";

                int lawCount = 0, polCount = 0;
                var top = new System.Collections.Generic.List<string>();
                int topLv = 0;
                for (int i = 0; i < LawEngine.LawKeys.Length; i++)
                {
                    int lv = LawEngine.GetLawLevel(kid, LawEngine.LawKeys[i]);
                    if (lv > 0)
                    {
                        lawCount++;
                        if (lv > topLv) { topLv = lv; top.Clear(); top.Add(LawEngine.LawKeys[i]); }
                        else if (lv == topLv && top.Count < 2) top.Add(LawEngine.LawKeys[i]);
                    }
                }
                for (int i = 0; i < LawEngine.PolicyKeys.Length; i++)
                    if (LawEngine.GetPolicyLevel(kid, LawEngine.PolicyKeys[i]) > 0) polCount++;

                var sb = new System.Text.StringBuilder();
                sb.Append(Services.LocalizationService.Get("kingdom_law_header")).Append('：').Append(styleName)
                  .Append('\n')
                  .Append(string.Format(Services.LocalizationService.Get("kingdom_law_counts"), lawCount, polCount));
                for (int i = 0; i < top.Count; i++)
                {
                    sb.Append('\n');
                    sb.Append(Services.LocalizationService.Get(top[i]));
                    sb.Append('·');
                    sb.Append(Services.LocalizationService.Get("law_lv" + topLv));
                }
                _summaryText.text = sb.ToString();
                _summaryText.gameObject.SetActive(true);
            }
            catch (System.Exception)
            {
                _summaryText.gameObject.SetActive(false);
            }
        }

        private static void OnEntryClick(StatsWindow window)
        {
            try
            {
                var cfg = UnrestConfig.Instance;
                if (cfg == null || !cfg.NationPlayEnabled || window == null) return;

                Kingdom kingdom = GetShownKingdom(window);
                if (kingdom == null || kingdom.data == null) return;
                long kid = kingdom.data.id;

                if (NationEngine.NationKingdomId != 0 && NationEngine.NationKingdomId == kid)
                {
                    if (UI.CabinetWindow.Instance != null) UI.CabinetWindow.Instance.Show();
                    return;
                }

                int year;
                try { year = EconomyModMain.GetCurrentGameYear(); } catch (System.Exception) { year = 0; }
                if (NationEngine.Claim(kingdom, year, out _))
                {
                    if (UI.CabinetWindow.Instance != null) UI.CabinetWindow.Instance.Show();
                }
                Debug.Log("[ClassicalEconomics] 国家界面入口被点击");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 国家界面入口点击失败: " + e.Message);
            }
        }

        /// <summary>读取窗口当前展示的国家：meta_object 在原版为非公开成员，经反射读取
        /// （KingdomWindow 为 Kingdom，CityWindow 为 City；读取失败返回 null）。</summary>
        private static Kingdom GetShownKingdom(StatsWindow window)
        {
            object meta = GetMetaObject(window);
            if (meta is Kingdom k) return k;
            if (meta is City city) return GetCityKingdom(city);
            return null;
        }

        private static object GetMetaObject(StatsWindow window)
        {
            try
            {
                var t = window.GetType();
                const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var prop = t.GetProperty("meta_object", F);
                if (prop != null) return prop.GetValue(window);
                var field = t.GetField("meta_object", F);
                if (field != null) return field.GetValue(window);
            }
            catch (System.Exception) { }
            return null;
        }

        /// <summary>解析城市所属王国：反射探测常见成员，全部失败返回 null（点击时提示无国家）。</summary>
        private static Kingdom GetCityKingdom(City city)
        {
            try
            {
                var t = city.GetType();
                const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                foreach (var name in new string[] { "kingdom", "kingdomData", "mainKingdom" })
                {
                    var prop = t.GetProperty(name, F);
                    if (prop != null)
                    {
                        var v = prop.GetValue(city);
                        if (v is Kingdom k) return k;
                        if (v != null)
                        {
                            var idField = v.GetType().GetField("id", F);
                            long id = idField != null ? System.Convert.ToInt64(idField.GetValue(v)) : 0L;
                            if (id != 0) return GameHelpers.FindKingdom(id);
                        }
                    }
                    var field = t.GetField(name, F);
                    if (field != null)
                    {
                        var v = field.GetValue(city);
                        if (v is Kingdom k) return k;
                        if (v != null)
                        {
                            var idField = v.GetType().GetField("id", F);
                            long id = idField != null ? System.Convert.ToInt64(idField.GetValue(v)) : 0L;
                            if (id != 0) return GameHelpers.FindKingdom(id);
                        }
                    }
                }
            }
            catch (System.Exception) { }
            return null;
        }

        /// <summary>递归找子节点（Background 可能不是直接子级）。</summary>
        private static Transform FindChild(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                if (c.name == name) return c;
                var r = FindChild(c, name);
                if (r != null) return r;
            }
            return null;
        }
    }
}
