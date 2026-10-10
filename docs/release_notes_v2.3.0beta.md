# v2.3.0beta 发布说明

> 基线：v2.2.0（2026-09-10 打包）→ 本版。本轮为 **W0–W4 重构收口 + 一个新功能**。
> 验证口径：编译 `BUILD_OK` / 门禁 **59/59 全绿** / 游戏内实机三信号（`CompiledMods` 时间戳 · `Player.log` 的 `[NML]` 行 · `error_*.log` 无 `error CS`）。

---

## 一、新增功能

### 1. Tab 上的「选择扮演国家」按钮（旗帜图标）

- 打开国家列表：按 **GDP 降序**、最多 60 行，每行显示 `国名 · GDP · 人口`，**点一行即认领该国**并自动打开内阁。
- 顶部显示「当前扮演：X」+「打开内阁」；当前国家在列表里标 **★当前**。
- 未认领 / 冷却中 / 玩法开关关闭，都有明确提示，不静默失败。
- **与另两个入口共用同一套规则**（地图悬停按 `G`、原版国家窗口右上角账本钮）：只经 `NationEngine.Claim` 认领——换国冷却 10 年、启动资金守恒（城市仓库 20% + 开国拨款）、法典与事件流记录一致。
- 文案 6 语言 ×11 键；新增门禁 9 条断言（不得绕过 `Claim`、不得直写绑定字段、开关关闭不留死入口、必须显示冷却、行按钮按 W4-UI 计数回收、退世界清理）。

---

## 二、W0 · 门禁分层（本次重构的地基）

- `run_all_tests.ps1` 三层输出（**Contract / Consistency / Infra**）+ `-Layer` / `-AllowShapeDrift`；默认行为与改前一致。
- `docs/门禁分层说明.md`：契约登记表 + 判据 + **维护规则（契约只换锚点，不删断言）**。
- 构建可移植化：`build_local.ps1`（libs 自动定位）、`tools/pack_release.py`、`tools/pack_test.py` 的 ROOT 由 `__file__` 推导，仓库里不再有机器特定路径。

## 三、W1 · 紧急修复（社区反馈"禁了一切还在无穷夺权叛乱"）

| # | 改动 | 效果 |
|---|---|---|
| 1 | 政策引擎接入**动荡总开关** | 关掉「社会动荡」后政策引擎整体停摆 |
| 2 | 新增 `regime_change_enabled`（**默认关**） | 改革失败只降支持度，**绝不改变政权** |
| 3 | 新增 `gini_collapse_enabled`（**默认关**） | 基尼再高也不会「街头起义推翻国王」 |
| 4 | **尊重原版世界法则** | 玩家关掉 外交→叛乱 时，模组不得制造暴动/内战 |
| 5 | 降频（配置驱动） | 改革尝试从约 2.9 年一次降到约 6.7 年一次 |
| 6 | 失败后果分级 | 40% 未果 / 25% 退位 / 20% 驾崩 / 15% 内战 |

## 四、W2 · UI 重构

- **本体皮肤**：`UI/VanillaSkin.cs` 从游戏自带窗口预制体抽取底板 9-slice + 关闭按钮图标（含 pixelsPerUnit 补偿，避免边框放大 100 倍的老问题）。
- **配色取自本体**：`UI/VanillaChartPalette.cs` —— 颜色全部来自本体 `Toolbox`，轴标签按取值上色对齐 `GraphHelpers.verticalFormat`，面积填充改为本体同款 0.4→0.1 垂直渐变。
- **图表宿主常驻**：`EnsureGdpHost/EnsureGiniHost` 只创建一次，刷新只走 `SetChartData`；`ChartMeshGraphic` 增加 `RebuildCount/SkipCount` 计数器，让"脏标记是否真的命中"可观测。
- **行池化**：HUD 纯文本行走 `_textPool`（原每次刷新新建约 40 个 `Text`）。
- **窗口拆分**：`EconomyHUD` → 壳 / Overview / Charts；`CabinetWindow` → 壳 / Pages / Diplomacy / Chart。
- **打开面板暂停**（`pause_on_panel_open`，**默认关**）：`UI/UIPauseCoordinator.cs` 只恢复自己造成的暂停，玩家自己按的暂停不受影响。

## 五、W3 · 结构收敛

- **W3.2 存档键单一真源**：新增 `Core/SaveSchema.cs`（18 个固定键常量 + 3 个参数化构造器 + `Version`），把 5 个文件里的 **41 处键值字面量**全部收口；保存时写新键 `rb_schema_ver`（纯追加，旧档读作 v0）。
- **新门禁 `Test-SaveSchema`**：键值字面量只许出现在 SaveSchema；**历史键值逐字冻结**（改键值 = 旧存档数据静默丢失，编译不报错但门禁 RED）；读写点数量对称；含 5 组变异测试。
- **W3.4 拆巨型文件**：`Core/DecisionEvents.cs` 1122 → **776 行**，拆出 `DecisionEvents.Effects.cs`（255）与 `DecisionEvents.Save.cs`（121）；4 道相关门禁**只换锚点**（改为合并扫描 `DecisionEvents*.cs`），断言一条未删。

## 六、W4 · 性能收口（2026-10-07 结论 + 本版复核）

- 事件 id 字典 / 战争对缓存、语言按需加载、时代成员扫描跳过：**确认早已实现**。
- `_rtCyclePending` 解耦 + `PostCycle(advanceCycle:false)` 异步化：已落地并复核（`ComputeGini` 只在后台线程）。
- Snapshot 切片：经代码级评估判定为**无效工作**（王国统计复制只值微秒级，真正尖峰来自 UI 重建）→ 转移到 W2 用池化解决。

## 七、工具链与工程规范

| 工具 | 作用 |
|---|---|
| `tools/release_dev.ps1` | 一键：编译 → 门禁 → 打包 → 部署 → 清 NML 缓存，任一步失败立即中止 |
| `tools/deploy_source.ps1` | 源码部署：逐文件哈希校验、保留玩家 `config.json`、清 NML 编译缓存、写 `_dev-build.txt` |
| `tools/Test-Ps1Bom.ps1` + `tools/Fix-Ps1Bom.ps1` | **真实事故**沉淀：无 BOM 的 `.ps1` 会被 PowerShell 5.1 按 ANSI 解码，中文注释吞换行 → 断言静默失效。已为全仓 8 个同类脚本补 BOM，并加门禁防复发 |

**门禁总数 59**（Contract 45 / Consistency 8 / Infra 6），`run_all_tests.ps1 -Layer All` 全绿。

## 八、界面与文案

- 6 语言持续同步（简中 / 繁中 / English / Русский / 日本語 / Deutsch），本版新增 11 键。
- 参考文档新增：`docs/架构说明.md`、`docs/架构评估与重构建议.md`、`docs/重构总纲_v3.0.md`、`docs/门禁分层说明.md`、`docs/patches.md`、`docs/ui-style-guide.md`、`docs/参考资料-0.22经济新生与帝国盒子信息机制.md`。

## 九、取消的工作

- **帝国盒子（EmpireCraft）适配已取消并全部回退**（用户决定）。分析文档 `docs/兼容-帝国盒子EmpireCraft.md` 保留作资料；结论仍然有效：补丁目标零交集，但它的「无小人/虚拟人口」模式会让"实体单位口径"的人均/基尼失真。

## 十、已知未实机确认（诚实标注）

- 打开面板暂停（`pause_on_panel_open`）与图表配色/渐变：编译 + 门禁 + 契约级证据齐备，**未有实机截图/日志证据**。
- 内阁窗口内的图表仍随页重建（HUD 侧已常驻化）。
