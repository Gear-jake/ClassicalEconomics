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

        private static void Postfix(StatsWindow __instance)
        {
            var window = __instance;
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
        /// <summary>
        /// 找窗口内第一个"右上角小按钮"：锚定 (>=0.99, >=0.99) 且位于窗口顶部区域
        /// （y ∈ [-400, 0]）、宽高 24~72、带 Button 组件（关闭 X 或同列工具钮）。
        /// 排除本模组自己的入口钮，避免把自身当参照造成自我偏移。
        /// </summary>
        private static Transform FindCornerButton(Transform root)
        {
            if (root == null) return null;
            for (int i = 0; i < root.childCount; i++)
            {
                var c = root.GetChild(i);
                if (c.name == ButtonName) continue; // 跳过自己
                var crt = c as RectTransform;
                if (crt != null && crt.anchorMin.x >= 0.99f && crt.anchorMin.y >= 0.99f
                    && crt.anchoredPosition.y >= -400f && crt.anchoredPosition.y <= 0f
                    && crt.sizeDelta.x >= 24f && crt.sizeDelta.x <= 72f
                    && crt.sizeDelta.y >= 24f && crt.sizeDelta.y <= 72f
                    && c.GetComponent<Button>() != null)
                    return c;
                var r = FindCornerButton(c);
                if (r != null) return r;
            }
            return null;
        }

        /// <summary>兜底：直接锚窗口框架右上角（探测不到角落按钮时）。</summary>
        private static void FallbackAnchor(RectTransform rt)
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-12f, -64f);
        }

        private static void DumpChildren(Transform t, int depth, System.Text.StringBuilder sb, int maxDepth)
        {
            if (t == null || depth > maxDepth) return;
            var rt = t as RectTransform;
            string sz = rt != null ? " size=" + rt.sizeDelta.x + "x" + rt.sizeDelta.y
                + " anchor=" + rt.anchorMin.x + "," + rt.anchorMin.y + " pos=" + rt.anchoredPosition.x + "," + rt.anchoredPosition.y : "";
            sb.Append(new string(' ', depth * 2)).Append(t.name).Append(sz).Append('\n');
            for (int i = 0; i < t.childCount; i++)
                DumpChildren(t.GetChild(i), depth + 1, sb, maxDepth);
        }        private static bool InjectEntry(StatsWindow window)
        {
            try
            {
                var cfg = UnrestConfig.Instance;
                if (cfg == null || !cfg.NationPlayEnabled || window == null) return false;

                Transform background = window.transform.Find("Background");
                if (background == null) background = FindChild(window.transform, "Background");
                if (background == null) return false;
                // 幂等：入口钮宿主动态（原版按钮列容器），须全树查找防重复注入
                if (FindChild(window.transform, ButtonName) != null) return false;

                var btn = PowerButtonCreator.CreateSimpleButton(
                    ButtonName,
                    () => OnEntryClick(window),
                    UI.IconLoader.Get("ledger"),
                    window.transform).gameObject;

                // 运行时自适应：以窗口内第一个"右上角小按钮"（原版关闭 X 或同列工具钮）
                // 的底边中心为参照，把入口钮放到其正下方 8px 处。宿主固定为 window.transform
                // （稳定对象，不会被原版 create 的后续逻辑重建/清理）；位置用世界坐标换算，
                // 不依赖宿主坐标系，也不受布局组接管。
                var rt = btn.GetComponent<RectTransform>();
                bool placed = false;
                var cornerBtn = FindCornerButton(window.transform);
                if (cornerBtn != null && cornerBtn is RectTransform crt)
                {
                    try
                    {
                        var windowRt = (RectTransform)window.transform;
                        // 参照按钮底边中心的世界坐标 → 窗口局部坐标
                        Vector3 bottomCenterWorld = crt.TransformPoint(
                            new Vector3(crt.rect.center.x, crt.rect.yMin, 0f));
                        Vector3 local = windowRt.InverseTransformPoint(bottomCenterWorld);
                        rt.anchorMin = new Vector2(1f, 1f);
                        rt.anchorMax = new Vector2(1f, 1f);
                        rt.pivot = new Vector2(1f, 1f);
                        float px = local.x - windowRt.rect.xMax;              // 右缘对齐（负值=内侧）
                        float py = local.y - windowRt.rect.yMax - 8f;         // 参照按钮下方 8px
                        // 自我保护：偏移落在窗口右上区域内才采用，防止布局未就绪时换算跑飞
                        if (px >= -200f && px <= 20f && py >= -500f && py <= 0f)
                        {
                            rt.anchoredPosition = new Vector2(px, py);
                            rt.sizeDelta = new Vector2(38f, 38f);
                            placed = true;
                        }
                    }
                    catch (System.Exception) { }
                }
                if (!placed)
                {
                    FallbackAnchor(rt);
                    rt.sizeDelta = new Vector2(38f, 38f);
                }
                // 布局探测：每次注入时打印窗口子物体树（仅写日志不上屏），
                // 若按钮位置仍不理想，可凭日志一次定准锚点。
                try
                {
                    var log = new System.Text.StringBuilder();
                    DumpChildren(window.transform, 0, log, 3);
                    Debug.Log("[ClassicalEconomics][LayoutProbe]\n" + log.ToString());
                }
                catch (System.Exception) { }
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

        /// <summary>创建摘要文本对象（幂等）；锚定按钮左下方。字号/卡片尺寸随 ui_scale 缩放（上限防挤爆原版窗口）。</summary>
        private static void EnsureSummaryText(StatsWindow window, Transform background)
        {
            if (_summaryText != null) return;
            try
            {
                float s = UnrestConfig.Instance != null ? Mathf.Clamp(UnrestConfig.Instance.UiScale, 0.8f, 1.6f) : 1.2f;
                var go = new GameObject(SummaryName, typeof(RectTransform), typeof(Text));
                go.transform.SetParent(background, false); // 面板内右上（面板右缘在框架按钮列左侧，互不重叠）
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-12f, -56f);
                rt.sizeDelta = new Vector2(Mathf.Min(240f, 190f * s), Mathf.Min(96f, 72f * s));
                var t = go.GetComponent<Text>();
                t.font = LocalizedTextManager.current_font != null
                    ? LocalizedTextManager.current_font
                    : Resources.GetBuiltinResource<Font>("Arial.ttf");
                t.fontSize = Mathf.RoundToInt(11f * s);
                t.alignment = TextAnchor.UpperRight;
                t.color = new Color(0.95f, 0.88f, 0.6f, 0.95f);
                t.lineSpacing = 1.05f;
                _summaryText = t;
            }
            catch (System.Exception) { }
        }

        /// <summary>ui_scale 变更后重建摘要卡：销毁旧文本对象，为打开中的原版窗口立即重建。</summary>
        public static void RefreshSummaryScale()
        {
            if (_summaryText != null)
            {
                try { Object.Destroy(_summaryText.gameObject); } catch (System.Exception) { }
                _summaryText = null;
            }
            try
            {
                foreach (var w in Resources.FindObjectsOfTypeAll<StatsWindow>())
                {
                    Transform background = w != null ? w.transform.Find("Background") : null;
                    if (background == null && w != null) background = FindChild(w.transform, "Background");
                    if (background == null) continue;
                    EnsureSummaryText(w, background);
                    UpdateLawSummary(w, GetShownKingdom(w));
                }
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
