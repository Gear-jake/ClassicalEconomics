using System;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 【W2 PoC】本体皮肤：从游戏自带的窗口预制体（Resources/windows/&lt;id&gt;）抽取
    /// 底图 / 关闭按钮 / 标题样式的 Sprite，用于我们**自己的非暂停悬浮窗**。
    ///
    /// 为什么必须"借皮"而不是"用本体窗口"：
    ///   本体窗口全部派生自 ScrollWindow，而 MapBox.cs:1692
    ///     _is_paused = Config.paused || ScrollWindow.isWindowActive() || RewardedAds.isShowing();
    ///   —— 只要有任何 ScrollWindow 系窗口处于激活状态，游戏就会暂停。
    ///   所以只取它的视觉元素，容器仍用自建 Canvas（FloatingWindow）。
    ///
    /// 安全约定：
    ///   · 只读预制体资源上的组件，**从不 Instantiate** →
    ///     Awake/OnEnable 不执行，ScrollWindow 不会写入 _current_window，因此不会暂停；
    ///   · 因此不会触发本体窗口的 Awake/OnEnable，也就不会注册为激活窗口（不暂停）；
    ///   · 任何一步失败都返回 null，调用方回退现有外观 —— 皮肤永远不能让模组崩。
    /// </summary>
    internal static class VanillaSkin
    {
        /// <summary>用作视觉模板的本体窗口（kingdom 是标准列表型窗口，底板/标题/关闭齐全）。</summary>
        // 【回退到已验证良好的模板】turn 25 用户认可的那一版用的就是 kingdom；
        // 中途改成 list_kingdoms 引入了未验证的变量，一并撤回。
        private const string TemplateWindowId = "kingdom";

        private static bool _probed;
        private static Sprite _panelSprite;
        private static Image.Type _panelType = Image.Type.Sliced;
        private static Vector4 _panelBorder;
        private static float _panelArea;
        private static string _panelSource = "?";
        private static Sprite _innerSprite;   // windowInnerSliced：本体内嵌面板（卡片/小容器用）
        private static int _panelPrio;
        private static Sprite _closeSprite;
        private static Sprite _buttonSprite;
        private static Color _titleColor = Color.white;
        private static Color _bodyColor = new Color(0.90f, 0.90f, 0.86f);   // 兜底：米白
        private static bool _bodyFound;
        private static float _titleSize;

        /// <summary>面板底板 Sprite（9-slice）；不可用返回 null。</summary>
        public static Sprite PanelSprite { get { Probe(); return _panelSprite; } }
        public static Image.Type PanelType { get { Probe(); return _panelType; } }
        public static Vector4 PanelBorder { get { Probe(); return _panelBorder; } }
        /// <summary>底板 9-slice 四条边框中最大的一条（精灵像素）。用于按面板尺寸缩放边框厚度。</summary>
        public static float PanelMaxBorder { get { Probe(); return Mathf.Max(Mathf.Max(_panelBorder.x, _panelBorder.y), Mathf.Max(_panelBorder.z, _panelBorder.w)); } }
        /// <summary>本体内嵌面板 Sprite（windowInnerSliced）：用于卡片、小容器等内嵌块。</summary>
        public static Sprite InnerSprite { get { Probe(); return _innerSprite; } }
        /// <summary>本体边框缩放统一常量（实机对比定值，见 docs/ui-style-guide.md）。</summary>
        public const float PanelBorderMultiplier = 20f;

        /// <summary>【实机标定】multiplier=20 时，精灵边框像素在屏幕上约占 0.436 倍。
        /// 用于让标题/页签/内容避让本体窗口的厚边框（否则会压在边框上）。</summary>
        private const float BorderScreenRatio = 0.436f;
        public static float TopInset    { get { Probe(); return _panelBorder.w * BorderScreenRatio; } }
        public static float BottomInset { get { Probe(); return _panelBorder.y * BorderScreenRatio; } }
        public static float SideInset   { get { Probe(); return Mathf.Max(_panelBorder.x, _panelBorder.z) * BorderScreenRatio; } }
        /// <summary>关闭按钮 Sprite；不可用返回 null（调用方沿用原图标）。</summary>
        public static Sprite CloseSprite { get { Probe(); return _closeSprite; } }
        /// <summary>按钮底图 Sprite；不可用返回 null。</summary>
        public static Sprite ButtonSprite { get { Probe(); return _buttonSprite; } }
        public static Color TitleColor { get { Probe(); return _titleColor; } }
        /// <summary>正文文字色：取自本体窗口里**标题以外**的第一个 Text（本体 UI 的正文色）。</summary>
        public static Color BodyColor { get { Probe(); return _bodyColor; } }
        public static float TitleSize { get { Probe(); return _titleSize; } }


        // ===== 本体页签素材（本体用的是 SortButton 的那一对）=====
        private static Sprite _tabOn;
        private static Sprite _tabOff;
        private static bool _tabProbed;

        /// <summary>页签选中态 Sprite（本体 ui/tab_button_sort_selected）。</summary>
        public static Sprite TabOnSprite { get { ProbeTabs(); return _tabOn; } }
        /// <summary>页签未选中态 Sprite（本体 ui/tab_button_sort）。</summary>
        public static Sprite TabOffSprite { get { ProbeTabs(); return _tabOff; } }
        /// <summary>页签边框缩放：本体按钮素材原生就带 border，按原生尺寸渲染（实测定值）。</summary>
        public const float TabBorderMultiplier = 8.73f;

        private static void ProbeTabs()
        {
            if (_tabProbed) return;
            _tabProbed = true;
            try
            {
                _tabOn = SpriteTextureLoader.getSprite("ui/tab_button_sort_selected");
                _tabOff = SpriteTextureLoader.getSprite("ui/tab_button_sort");
                Debug.Log("[ClassicalEconomics] 本体页签素材: on=" + (_tabOn != null) + " off=" + (_tabOff != null));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 本体页签素材加载失败（回退自绘）: " + e.Message);
            }
        }


        // ===== 原版「World News」弹窗素材（报纸风：羊皮纸底 + 衬线字体 + 深色按钮）=====
        // 该弹窗在新版本体里存在，但反编译源码版本较旧查不到实现，故改为**运行时从
        // windows/news 预制体抽取**；抽不到就返回 null，调用方回退现有外观。
        private static Sprite _newsPaper;
        private static Sprite _newsButton;
        private static Font _newsFont;
        private static bool _newsProbed;

        /// <summary>报纸/羊皮纸底图（来自本体 news 窗口）；不可用返回 null。</summary>
        public static Sprite NewsPaperSprite { get { ProbeNews(); return _newsPaper; } }
        /// <summary>报纸弹窗底部的深色按钮图；不可用返回 null。</summary>
        public static Sprite NewsButtonSprite { get { ProbeNews(); return _newsButton; } }
        /// <summary>报纸弹窗使用的字体（通常为衬线体）；不可用返回 null。</summary>
        public static Font NewsFont { get { ProbeNews(); return _newsFont; } }

        private static void ProbeNews()
        {
            if (_newsProbed) return;
            _newsProbed = true;
            try
            {
                var prefab = Resources.Load("windows/news", typeof(GameObject)) as GameObject;
                if (prefab == null)
                {
                    Debug.Log("[ClassicalEconomics][NewsDiag] windows/news 预制体未找到（报纸风回退默认外观）");
                    return;
                }
                var images = prefab.GetComponentsInChildren<Image>(true);
                var sb = new System.Text.StringBuilder();
                sb.Append("[ClassicalEconomics][NewsDiag] windows/news 的 Image 候选 ").Append(images.Length).Append(" 个:");
                for (int i = 0; i < images.Length; i++)
                {
                    var im = images[i];
                    if (im == null) continue;
                    var r = im.rectTransform != null ? im.rectTransform.rect : new Rect();
                    var sp = im.sprite;
                    sb.Append(" | [").Append(im.gameObject.name).Append("] ")
                      .Append((int)r.width).Append("x").Append((int)r.height)
                      .Append(" type=").Append(im.type)
                      .Append(" sprite=").Append(sp != null ? sp.name : "(null)")
                      .Append(" color=").Append(im.color);
                    if (sp != null) sb.Append(" spriteRect=").Append((int)sp.rect.width).Append("x").Append((int)sp.rect.height).Append(" border=").Append(sp.border);
                }
                Debug.Log(sb.ToString());

                // 择优：面积最大的 Image 作为纸底；名字含 Button 的作为底部按钮
                float bestArea = 0f;
                for (int i = 0; i < images.Length; i++)
                {
                    var im = images[i];
                    if (im == null || im.sprite == null) continue;
                    string n = im.gameObject.name;
                    var rt = im.rectTransform;
                    float area = rt != null ? Mathf.Abs(rt.rect.width * rt.rect.height) : 0f;
                    if (n.IndexOf("Button", StringComparison.OrdinalIgnoreCase) >= 0 && _newsButton == null)
                    {
                        _newsButton = im.sprite;
                    }
                    else if (area > bestArea && n.IndexOf("Button", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        bestArea = area;
                        _newsPaper = im.sprite;
                    }
                }

                var texts = prefab.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    var tx = texts[i];
                    if (tx != null && tx.font != null) { _newsFont = tx.font; break; }
                }
                Debug.Log(string.Format("[ClassicalEconomics] 报纸素材已抽取: paper={0} button={1} font={2}",
                    _newsPaper != null, _newsButton != null, _newsFont != null ? _newsFont.name : "(null)"));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 报纸素材抽取失败: " + e.Message);
            }
        }

        /// <summary>探测一次：从模板窗口预制体抽取视觉元素（非激活实例化，用完即毁）。</summary>
        private static void Probe()
        {
            if (_probed) return;
            _probed = true;
            GameObject tmp = null;
            try
            {
                var prefab = Resources.Load("windows/" + TemplateWindowId, typeof(GameObject)) as GameObject;
                if (prefab == null)
                {
                    Debug.LogWarning("[ClassicalEconomics] 本体窗口模板未找到: windows/" + TemplateWindowId + "（皮肤回退为默认外观）");
                    return;
                }
                // 关键：**直接读预制体资源上的组件，绝不 Instantiate**。
                // 教训：Instantiate(prefab, parent, false) 的第三个参数是 instantiateInWorldSpace，
                // **不是**“是否激活”；那样会激活实例化，触发本体窗口组件的 Awake/OnEnable
                // （实测产生 41 次 NullReferenceException，且可能注册为激活窗口而暂停游戏）。
                // 读预制体资源的序列化引用不会产生任何实例或副作用。
                tmp = prefab;

                var images = tmp.GetComponentsInChildren<Image>(true);
                // 【诊断】先把预制体里所有 Image 的候选打进日志 —— 不靠猜。
                try
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append("[ClassicalEconomics][SkinDiag] Image 候选 ").Append(images.Length).Append(" 个:");
                    for (int k = 0; k < images.Length && k < 12; k++)
                    {
                        var im = images[k];
                        if (im == null) continue;
                        var rr = im.rectTransform != null ? im.rectTransform.rect : new Rect();
                        var sp = im.sprite;
                        sb.Append(" | #").Append(k).Append(" [").Append(im.gameObject.name).Append("]")
                          .Append(" rect=").Append((int)rr.width).Append("x").Append((int)rr.height)
                          .Append(" type=").Append(im.type)
                          .Append(" sprite=").Append(sp != null ? sp.name : "(null)");
                        if (sp != null)
                        {
                            sb.Append(" spriteRect=").Append((int)sp.rect.width).Append("x").Append((int)sp.rect.height)
                              .Append(" border=").Append(sp.border).Append(" ppu=").Append(sp.pixelsPerUnit);
                        }
                    }
                    Debug.Log(sb.ToString());
                }
                catch (System.Exception) { }


                // 【决定性诊断】列出所有名字含 Background 的候选及其确切参数 —— 用于判断
                // “哪个才是整窗底板”以及“它是否真的支持 9-slice（border 是否非 0）”。
                try
                {
                    var sb2 = new System.Text.StringBuilder();
                    sb2.Append("[ClassicalEconomics][SkinDiag2] Background 候选:");
                    for (int k = 0; k < images.Length; k++)
                    {
                        var im2 = images[k];
                        if (im2 == null || im2.sprite == null) continue;
                        if (im2.gameObject.name.IndexOf("Background", StringComparison.OrdinalIgnoreCase) < 0) continue;
                        var r2 = im2.rectTransform != null ? im2.rectTransform.rect : new Rect();
                        var s2 = im2.sprite;
                        sb2.Append(" | [").Append(im2.gameObject.name).Append("] rect=").Append((int)r2.width).Append("x").Append((int)r2.height)
                           .Append(" sprite=").Append(s2.name)
                           .Append(" spriteRect=").Append((int)s2.rect.width).Append("x").Append((int)s2.rect.height)
                           .Append(" border=").Append(s2.border)
                           .Append(" ppu=").Append(s2.pixelsPerUnit)
                           .Append(" type=").Append(im2.type);
                    }
                    Debug.Log(sb2.ToString());
                }
                catch (System.Exception) { }

                for (int i = 0; i < images.Length; i++)
                {
                    var img = images[i];
                    if (img == null || img.sprite == null) continue;
                    string n = img.gameObject.name;
                    // 【关键】必须先判定"这是不是一个候选底板"，否则下面的 else-if（Close/Button）会被吞掉。
                    bool looksLikePanel = n.IndexOf("Background", StringComparison.OrdinalIgnoreCase) >= 0
                        || img.sprite.name.IndexOf("Inner", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (looksLikePanel && img.sprite != null)
                    {
                        // 【择优规则】本体有两套底板：
                        //   windowBig        border≈(99,111,111,126) —— 真正的窗口外框（城堡垛口装饰），
                        //                    自带厚边框，直接套小面板会重叠 → 由调用方按面板尺寸缩放；
                        //   windowInnerSliced border≈(22,10,23,11) —— 内嵌填充层（平的），单独用会变扁平色块，仅兜底。
                        // 结论：用**真正的窗口框**（windowBig），并通过缩放边框厚度让它适配我们的面板尺寸。
                        // 优先级：windowBig（真正的窗口外框，有城堡垛口装饰）= 3；
                        //           其他 Background（最大面积）= 2；windowInnerSliced（内嵌填充层，平的）= 1。
                        // 教训：Inner 是"内嵌面板填充"，不是窗口框 —— 单独用它窗口会变成一个扁平色块（实机反馈"变丑"）。
                        var rt2 = img.rectTransform;
                        float area = rt2 != null ? Mathf.Abs(rt2.rect.width * rt2.rect.height) : 0f;
                        // 单独记一份内嵌面板素材（card/小容器用），与窗口底板择优互不干扰
                        bool isBigFrame = img.sprite.name.IndexOf("Big", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool isInner = img.sprite.name.IndexOf("Inner", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (isInner) _innerSprite = img.sprite;   // 单独留一份内嵌面板素材
                        bool isInnerSprite = isInner;
                        int prio = isBigFrame ? 3 : (isInner ? 1 : 2);
                        bool better = prio > _panelPrio || (prio == _panelPrio && area > _panelArea);
                        if (better && img.sprite.name.IndexOf("ScrollBar", StringComparison.OrdinalIgnoreCase) < 0
                                  && img.sprite.name.IndexOf("close", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            _panelPrio = prio;
                            _panelArea = area;
                            _panelSource = img.gameObject.name + "/" + img.sprite.name;
                            _panelSprite = img.sprite;
                            _panelType = img.type;
                            _panelBorder = new Vector4(img.sprite.border.x, img.sprite.border.y, img.sprite.border.z, img.sprite.border.w);
                        }
                    }
                    else if (_closeSprite == null && n.IndexOf("Close", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _closeSprite = img.sprite;
                    }
                    else if (_buttonSprite == null && n.IndexOf("Button", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _buttonSprite = img.sprite;
                    }
                }

                var texts = tmp.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    var tx = texts[i];
                    if (tx == null) continue;
                    if (tx.gameObject.name.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _titleColor = tx.color;
                        _titleSize = tx.fontSize;
                    }
                    else if (!_bodyFound && tx.color.a > 0.5f && tx.color.grayscale > 0.4f)
                    {
                        // 本体 UI 的正文色：取第一个不透明、偏亮的非标题文字
                        _bodyColor = tx.color;
                        _bodyFound = true;
                    }
                }

                Debug.Log(string.Format("[ClassicalEconomics] 本体皮肤已抽取: panel={0} src={5} area={6} border={7} ppu={8} close={1} button={2} titleColor={3} titleSize={4}",
                    _panelSprite != null, _closeSprite != null, _buttonSprite != null, _titleColor, _titleSize, _panelSource, _panelArea, _panelBorder, _panelSprite != null ? _panelSprite.pixelsPerUnit : 0f));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ClassicalEconomics] 本体皮肤抽取失败（回退默认外观）: " + e.Message);
            }
            finally
            {
                // tmp 是 Resources 预制体资源，不能销毁（Destroy 资源会导致后续加载失效）。
            }
        }
    }
}
