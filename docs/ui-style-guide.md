# ClassicalEconomics · UI 风格与实现规范

> 适用范围：本模组全部自建 UI（悬浮窗、按钮、行、图表容器）。
> 核心原则：**一切视觉元素取自游戏本体，不自绘。**

---

## 一、核心原则

| 原则 | 做法 | 反例（禁止） |
|---|---|---|
| 视觉取自本体 | 从本体窗口预制体 `Resources/windows/<id>` 抽 Sprite | 自己画"石板金边"9-slice |
| 容器自建但不暂停 | 用 `FloatingWindow`（自建 Canvas） | 直接用本体 `ScrollWindow`（会暂停游戏） |
| 缺失即回退 | 任何抽取失败都返回 null，调用方沿用旧外观 | 抽取失败就抛异常 / 显示空白 |
| 不猜，要验证 | 视觉改动必须**实际截图确认**（DPI-aware + PrintWindow） | 凭代码推理断言"应该好看" |

---

## 二、为什么不能直接用本体窗口

`MapBox.cs:1692`：

```csharp
_is_paused = Config.paused || ScrollWindow.isWindowActive() || RewardedAds.isShowing();
```

**只要有任何 `ScrollWindow` 系窗口处于激活状态，游戏就暂停。**
所以本体窗口一个都不能用 —— 只能"借皮"：把它的 Sprite 抽出来，装到我们自己的悬浮容器上。

---

## 三、本体皮肤管线（`UI/VanillaSkin.cs`）

### 3.1 抽取

```csharp
var prefab = Resources.Load("windows/" + TemplateWindowId, typeof(GameObject)) as GameObject;
var images = prefab.GetComponentsInChildren<Image>(true);   // 直接在资源上读组件
```

模板窗口：`kingdom`（标准列表型窗口，底板/标题/关闭齐全）。

### 3.2 ⚠️ 绝对不要实例化（血泪教训）

```csharp
// ❌ 错误：第三个参数是 instantiateInWorldSpace，**不是"是否激活"**！
Object.Instantiate(prefab, null, false);
```

上面这行会**激活实例化** → 本体窗口组件的 `Awake/OnEnable` 全部执行
→ 实测产生 **41 次 NullReferenceException**，且有注册为激活窗口（暂停游戏）的风险。

**正确做法：只读预制体资源上的组件，从不 Instantiate。** 读序列化引用零副作用。

### 3.3 底板择优

| 精灵 | border | 用途 | 优先级 |
|---|---|---|---|
| `windowBig` | (99,111,111,126) | **真正的窗口外框**（城堡垛口装饰） | 3 |
| 其他 `Background` | — | 一般底板（按面积取最大） | 2 |
| `windowInnerSliced` | (22,10,23,11) | **内嵌填充层（平的）** | 1（仅兜底） |

> 教训：单独用 `windowInnerSliced` 会让窗口退化成一个**扁平色块**（实机反馈"变丑"）。

---

## 四、边框缩放（关键参数）

本体 UI 精灵的 `pixelsPerUnit = 1`，而我们 Canvas 用默认 `referencePixelsPerUnit = 100`。
**不补偿的话 9-slice 边框会被放大到远超面板尺寸，四边互相重叠**（实机表现为几层错位色块）。

### 4.1 实测定值

通过实机截图对比（同一台机器、2560×1600 物理分辨率、360×500 面板）：

| multiplier | 边框观感 | 结论 |
|---|---|---|
| 100 | 约 12 UI 单位 | 装饰丢失，窗口退化成扁平色块 |
| 40 | 约 55 UI 单位 | 垛口/角饰完整 |
| **20** | — | **选定**：装饰完整且不挤压内容 |
| 12 / 10 | 更厚 | 边框占比偏大 |
| 5 | 很厚 | 内容区被挤扁 |

固化于 `FloatingWindow.DefaultBorderMultiplier = 20f`。

> 调试期可用 `<persistentDataPath>/ce_ui_tune.txt` 覆盖（内容为数字）。**发布前移除该钩子。**

---

## 五、分层约定

### 5.1 窗口基类

`FloatingWindow`（抽象）：Canvas + 拖拽标题栏 + 四边缩放 + 关闭按钮 + 滚动内容区。
子类只提供配置（`WindowName / Anchors / Size`）与 `RefreshNow()`。

### 5.2 巨型窗口一律用 `partial` 拆分

| 文件 | 行数 | 职责 |
|---|---|---|
| `EconomyHUD.cs` | 426 | 壳：创建、面板、页签、刷新 |
| `EconomyHUD.Charts.cs` | 600 | 图表：GDP 多国折线、基尼趋势、悬停、图例 |
| `EconomyHUD.Overview.cs` | 147 | 概览：周期徽章、统计卡、格式化 |
| `CabinetWindow.cs` | 292 | 壳 + 行/按钮小构件 |
| `CabinetWindow.Pages.cs` | 390 | 法典/认领/央行/财政/政策/法令 |
| `CabinetWindow.Diplomacy.cs` | 364 | 外交列表与详情 |
| `CabinetWindow.Chart.cs` | 135 | GDP 图表 + 记录区 |

**拆分后必须同步更新门禁的形状锚点**（改为扫描 `<Window>*.cs` 分层），
并记入 `docs/门禁分层说明.md` 的契约变更记录。契约**只换锚点，不删断言**。

---

## 六、池化约束（强制）

`FloatingWindow` 的行对象**必须走对象池**：

```csharp
ClearContent()        // 回收：SetActive(false) + 入池，不 Destroy
RentLine(name)        // 取用：按名字从池里取
AddLine/AddDivider    // 取到则复用，取不到才 Create
```

- 池上限 `LinePoolCap = 512`
- ⚠️ **复用时必须 `SetAsLastSibling()`**：`VerticalLayoutGroup` 按 sibling index 排序，不复位会错序
- 新写的窗口内容代码同样受此约束（这是 W2 对 W4-UI 池化项的落实方式）

---

## 七、配色与字号

正文色与标题色**从本体窗口抽取**（`VanillaSkin.BodyColor / TitleColor`），
`UIStyles.TextPrimary` / `UIStyles.Gold` 已改为向本体取值并带兜底。

本体皮肤下**不再自绘**的装饰（会与本体边框打架 / 跑出框外）：
- 标题栏金色下划线
- 面板内金色描边
- 内容区暗色蒙版

---

## 八、验证方法（UI 改动必做）

本项目的 UI 无法靠代码推理判断，**必须实机截图**：

1. **DPI-aware 截取**：进程 DPI-unaware 时 `GetWindowRect` 返回逻辑坐标，
   屏幕是 2560×1600 而你会拿到 1280×800 —— **只截到左上角 1/4**。
   先调 `SetProcessDpiAwarenessContext(-4)`。
2. **穿透遮挡**：`PrintWindow(hwnd, hdc, 2)` 可在窗口被遮挡时截取；
   或 `AttachThreadInput` + `SetForegroundWindow` 前置后整屏截。
3. 工具已备：`工具/临时产物/game_capture.ps1`、`dpicap.cs`、`fa.cs`。

> ⚠️ **前置窗口会抢用户屏幕**。机器正在使用时不要这样做，先询问。

---

## 八·补、布局时序（Unity 特有大坑）

**不要在创建时测量一个宽度尚未确定的元素。**

行高自适应（`FitRowHeightToText`）必须**先给行确定宽度再测量**：

```csharp
rowRt.sizeDelta = new Vector2(knownWidth, baseHeight);   // 先定宽
LayoutRebuilder.ForceRebuildLayoutImmediate(rowRt);      // 再测
float h = txt.preferredHeight;                           // 此时才准
```

若此时宽度还是 0（父级布局尚未分配），文本会被挤成很多行，
`preferredHeight` 会**虚高数倍** —— 实机表现是"每条之间空隙巨大"。
（本项目真实事故：事件流每条空隙 3 倍。）

**保险**：宽度未知时**直接跳过测量**，交回布局器按自然尺寸处理。
宁可少修一处，也不要写进去一个错误的固定高度。

## 九、禁忌清单（本项目的真实教训）

| # | 禁忌 | 后果 |
|---|---|---|
| 1 | `Instantiate(prefab, parent, false)` 想当然当"非激活" | 41 次空引用 + 可能暂停游戏 |
| 2 | 用 `windowInnerSliced` 当窗口框 | 窗口变扁平色块 |
| 3 | 把 `windowBig` 边框压缩到 25px | 垛口/角饰被压扁，装饰全失 |
| 4 | 图层条件化时把声明写进 `if` 块却删掉了声明 | 编译失败 |
| 5 | 面板分支写成 `if (img.sprite != null)` | 吞掉后续 else-if，close/button 抽不到 |
| 6 | 在帧循环里读文件（调参热重载） | **每帧一次文件 I/O** |
| 7 | 只看局部截图就下结论 | 连续 3 轮改错方向 |
| 8 | 用 `tools.edit` 改含中文的 `.ps1` 不检查 BOM | PowerShell 按 ANSI 解析，语法错乱 |
