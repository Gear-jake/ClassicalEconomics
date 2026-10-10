# 与「帝国盒子 EmpireCraft」的共存分析

> ## ⛔ 结论：适配已取消，代码已全部回退（2026-10-10）
>
> 用户决定**不做**帝国盒子适配（投入产出不划算），本文档**只作为资料留存**，其中的 P0/P1/P2 方案
> 与门禁设计**均未保留在代码里**。已回退的内容：
>
> | 已删除 | 已回退 |
> |---|---|
> | `Core/Compat/EmpireCraftCompat.cs`（感知层） | `Core/UnrestEngine.cs` 法则读取回退为 `isEnabled()` |
> | `Core/Compat/EmpireCraftPopulation.cs`（城市口径聚合） | `Core/EconomyEngine.cs` / `Core/DataCollector.cs` 城市口径覆盖 |
> | `tools/Test-EmpireCraftCompat.ps1`（含 `run_all_tests.ps1` 登记） | `EconomyModMain.cs` / `UI/EconomyHUD.Overview.cs` / `UI/CabinetWindow.Diplomacy.cs` / `Core/NationSave.cs` |
> | 配置键 `empirecraft_aware`（`UnrestConfig` / `ConfigCallbacks` / `default_config.json` / 6 语言 / README ×2 / `Test-ConfigDocs`） | 门禁总数 **60 → 59**（Contract 46 → 45） |
>
> 回退后实测：`BUILD_OK: 387584`、0 `error CS`、6 条既有告警、门禁 **59/59 全绿**。
> 保留的知识（仍然有效）：补丁目标零交集、`BatchActors.u4_deadCheck` 是唯一条件重叠、
> 无小人模式会让"实体单位口径"失真、以及它按 id 重注册原版法则资产这件事。
>
> ---

> 日期：2026-10-10 · 对象：`Mods\EMPIRECRAFT(帝国盒子)` 版本 **0.4.4Beta9**（作者 长明火_CHM）
> 方法：**静态源码分析 + 一次同跑日志观察**。全文区分三种证据等级：
> `[已核实]` 源码/反编译/日志里直接读到 · `[推断]` 由证据推理 · `[未验证]` 没做过实验。
> 本轮**没有**在"两模组同开 + 无小人模式"下实测过我们的输出。

---

## 0. 一句话结论

**补丁层几乎零碰撞（可以直接共存）**；真正的适配点是**数据口径**与**玩法让位**：
帝国盒子的「无小人／虚拟人口」模式会让我们的人口—财富—基尼口径**静默失真**（不报错，只是数字不再代表全民）；
它按 id **重注册原版世界法则资产**，会让我们的法则读取吃到**陈旧缓存**。
建议做 3 件 P0 事 + 1 道契约门禁，其余靠文档与开关。

---

## 1. 对方是什么（实测）

| 项 | 事实 | 证据 |
|---|---|---|
| 版本 / 形态 | 0.4.4Beta9；**源码模组**（466 个 `.cs` / 5.4 MB，NML 启动时编译） | `mod.json.close`、`Scripts/` 统计 |
| 编译产物 | `长明火_CHM_EMPIRECRAFT_帝国盒子_.dll` **2.75 MB**（19:31:46 那次启动编译） | `CompiledMods\` |
| 补丁机制 | **手动 Harmony**：`new Harmony(nameof(x)).Patch(AccessTools.Method(typeof(T), nameof(T.m)), prefix/postfix: ...)`，`GamePatch` 基类；**全仓 0 个 `[HarmonyPatch]` 特性** | `GamePatches/CityPatch.cs:66+` |
| 补丁规模 | CityPatch 56 处 · ActorPatch 32 · NoCommonersPatch 29 · BabyHelperPatch 22 · KingdomPatch 20 · WarPatch 14 · OptimizationPatch 13 · TechnologyPatch 11 · SaveManagerPatch 10 | `Select-String '.Patch('` 计数 |
| 自带系统 | 帝国/正统/法理 · 派系（`FixedFaction` 27 KB） · 官制（`OfficeSystem` 19 KB） · 政体（`RegimeSystem` 16 KB） · 诉求 · 法律/犯罪/腐败 · 战争占领 · 图层/铭牌 | `Scripts/` 目录 |
| 自带工具 | 性能采样日志、虚拟人口+多核后台经济、存档完整性校验、Bug 上报、更新检查、`EmpireCraft Editor.exe`（71 MB 政体编辑器） | `Diagnostics/`、根目录 |
| 兼容方法论 | `Scripts/Compatibility/README.md`（对 AncientWarfare3）：反射软探测 + ownership guard + **"不要给 EC 的 Harmony 回调再打补丁：Mono 可能发不出嵌套包装，导致整个模组加载失败"** + 外来对象回退 pre-EC 实现 + 自带 `*.Tests.ps1` | 该文件 1–88 行 |

**它和我们是同一种工程文化**：手动装补丁、幂等、try/catch 隔离、反射探测第三方、自带验证脚本、区分"已验证/未验证"。
这一点决定了共存的**基本盘是好的**。

---

## 2. 冲突面矩阵

| 面 | 帝国盒子 | 古典经济学 | 结论 |
|---|---|---|---|
| **Harmony 补丁** | `SaveManager.saveMapData/loadActors/loadData`、`MapBox.startTheGame/lastGC`、大量 City/Actor/Kingdom 热路径 | 仅 4 个：`StatsWindow.create`、`SaveManager.saveWorldToDirectory`、`MapBox.finishingUpLoading`、Optime 条件层 | **零交集** ✓ |
| **存档键** | `wb_political_legitimacy`、`empirecraft_*`、`aw_*`、`custom_data_bool` | `rb_*`（单一真源 `Core/SaveSchema.cs`） | **不重叠** ✓ |
| **存档校验** | `SaveIntegrityChecker` 只校验自家 empire/claim/parliament/membership | — | 不会因我们的键报警 ✓ |
| **UI 注入** | 国家窗口：销毁 tabs 的 `space (1)`、追加 `regime`/`technology` 页签；**Prefix 替换 `KingdomWindow.showStatsRows`（`return false`）** | `StatsWindow.create` 后置注入内阁入口按钮 + 法典摘要卡 | 同窗口，**需实测布局**（低风险，见 2.3） |
| **数据口径** | 「无小人」模式：背景人口虚拟化，补丁 `MapBox.getCivWorldPopulation`、`Kingdom.countTotalWarriors` | 只读 `units_only_alive` 实体单位 | **最硬的适配点**（见 2.4） |
| **世界法则** | **按 id 重注册原版法则资产**（remove + new） | 读 `WorldLawLibrary.world_law_rebellions.isEnabled()` | 陈旧缓存风险（见 2.5） |
| **玩法** | 派系叛乱、诉求、税制/岁币、政体、经济（土地/市场/人口） | 动荡→起义→革命、法典/国策、税负/财富税、GDP/基尼/周期/银行 | **两套并行**，需"让位"策略（见 2.6） |
| **性能** | 自采样：`updateCities 26.8~38.4 ms/帧`、`updateActors 16.7~26.9`、最慢帧 980 ms；已做多核后台（4 线程/64 待办） | 4 补丁中 3 个在存档/UI 链，**不进逐帧热路径**；后台 `TradeSimulationWorker` | 不冲突，但**抢 CPU**（见 2.7） |

---

### 2.1 补丁层：零碰撞 `[已核实]`

我们补丁目标（`Core/PatchInstaller.cs:25-31` Registry + `docs/patches.md`）：

1. `OptimeCompatibility` — 仅在检测到 Optime 程序集时安装
2. `KingdomWindowIntegration` — Postfix `StatsWindow.create`
3. `LawSave` — Prefix/Postfix `SaveManager.saveWorldToDirectory` + Postfix `MapBox.finishingUpLoading`
4. `NationSave` — 同上 + Sidecar 兜底

帝国盒子对这些方法的使用情况（全仓 grep）：

| 目标 | 帝国盒子命中 |
|---|---|
| `finishingUpLoading` | **0 处** |
| `saveWorldToDirectory` | **0 处** |
| `StatsWindow.create` | **0 处**（它只碰 `StatsWindow.tryToShowMetaReligion`） |
| `SaveManager`（类） | 它碰 `saveMapData` / `loadActors` / `loadData` → **不同方法** |
| `BatchActors.u4_deadCheck` | **有**（`NativeWorldOptimizationPatch` 的计时 `Profile`）—— 我们也碰它，但**只在检测到 Optime 时**（`Services/OptimeCompatibility.cs` 的 Finalizer）⇒ 见下 |

⇒ 两边都用手动安装 + 幂等 + 单点失败隔离，**不会互相顶掉**，也不会出现"两个 Transpiler 抢同一个方法"的情况。

> **唯一的条件重叠**：`BatchActors.u4_deadCheck` —— 帝国盒子给它挂计时 `Profile`；
> 我们的 `OptimeCompatibility` 给它挂 Finalizer（**仅当检测到 Optime 时才安装**，且我们的 Transpiler 打的是
> *Optime 自己的* `ActorJobFlatten.BatchActors_u4_deadCheck_Prefix` 方法，不是这个原版方法）。
> Harmony 会正常组合"对方的 Prefix + 我们的 Finalizer"，但**这三方同台（Optime + 帝国盒子 + 我们）是套件里最需要实测的一处**。
> 门禁 `Test-EmpireCraftCompat` 把这条写成白名单：**无条件补丁必须零交集，唯一允许的重叠必须落在 Optime 条件层里**。

> 附带发现（对我们有利）：它把读档恢复挂在 `SaveManager.loadData` 的 Postfix 上，而我们的 `docs/patches.md:65-66` 明确记过
> 「`loadData` 只把任务塞进 SmoothLoader 队列就返回（分帧异步），此刻王国列表尚未恢复」。
> 我们的 `MapBox.finishingUpLoading` 选择更稳。**这解释了为什么联合测试必须做"两种加载顺序"**——两边的恢复时机不同。

### 2.2 存档键：不重叠 `[已核实]`

- 我们：`rb_*` 全部收敛到 `Core/SaveSchema.cs`（18 固定键 + 3 参数化前缀 + `rb_schema_ver`）
- 它：`wb_political_legitimacy`、`AncientWarfareRules.XiaizationLevelKey`（`aw_*`）、
  `CustomDataType.empirecraft_history_record`、family `custom_data_bool["has_city_pre"]`
- 它的 `SaveIntegrityChecker` 只遍历自家 Empire/Claim/Parliament/Membership，**不扫描未知键** ⇒ 我们的键安全

### 2.3 UI：同窗口注入，需实测 `[已核实 + 未验证布局]`

帝国盒子 `GamePatches/KingdomWindowPatch.cs`：

- `Prefix KingdomWindow.OnEnable`：**销毁 tabs 下的 `space (1)`**，然后按需追加两个页签 `regime`（政体）、`technology`（科技）
- `Prefix KingdomWindow.showStatsRows` **`return false`** —— **整段替换**原版国家信息行，自己重新 `showStatRow(...)`（含 `main_title`、`ruler_money`、`tribute`、`national_power` 等）

我们：`Postfix StatsWindow.create` → 注入 `ClassicalEconomicsCabinetEntry` 按钮（锚定窗口右上角"角落按钮"）+ 法典摘要卡。

原版调用链 `[已核实]`（`decomp/base/StatsWindow.cs:50/61/75`）：

```
StatsWindow.create()  →  base.create()  →  updateStats()  →  showStatsRows() [virtual]  +  showMetaRows()
```

- 我们在 `create` 的 Postfix 注入 ⇒ **在 `showStatsRows` 之后**，**不会被它的 `return false` 跳过** ✓
- 它的页签容器（`tabs`）与我们的按钮父级（窗口 Background）不同层 ⇒ 不会互相删 ✓
- `[未验证]` 它替换行内容后**行数/行高与"原版行数"不同**，我们的摘要卡是追加在窗口里的，位置/滚动高度可能偏；它的 `OnEnable` 前缀**每次启用都跑**（我们是幂等注入）⇒ 只需实测一次确认

### 2.4 数据口径：**最硬的适配点**（P0）`[已核实 + 推断]`

**它的「无小人／虚拟人口」模式**：

- 触发：EC 注册的世界法则 `empirecraft_law_no_commoners`（`Scripts/GameLibrary/EmpireCraftWorldLawLibrary.cs:28`）+ 性能开关 `performance_high_population_toggle`（`default_config.json`，回调 `ConfigActions:HighPopulationPerformanceCallBack`）
- 它做了什么 `[已核实：NoCommonersPatch.cs 顶部注释 + 补丁目标]`：
  - 城里的人**不睡觉**（拦 `Actor.addStatusEffect` 的睡眠状态）
  - **兵额按军制**（只有军镇有兵）
  - **没有实体单位的背景人口也计入城市人口与已占用住房**（这样原版不会因"空房很多"而无限制生育）
  - 补丁目标含：`MapBox.getCivWorldPopulation`、`Kingdom.countTotalWarriors`、`City.updateCityStatus`、`City.recalculateMaxHouses`、`City.tryToMakeWarrior`、`WorldBehaviourActions.updateUnitSpawn`、`BabyHelper.isMetaLimitsReached`、`NameplateText.showTextClanCity` …
- 它自己的负载日志（19:56 那次运行）`[已核实]`：
  `实体单位 2309；城市 489；人口组 1443；城内背景人口 46630；建筑 6602`

**我们这边的口径** `[已核实]`：

- `Core/DataCollector.cs:94` 只读 `World.world.units.units_only_alive`（实体单位）
- 人均 = 财富 / 实体数（活日志：`周期#22 财富=522 人均=0.16 Actor=3188` → 522/3188 ≈ 0.16，**分母是实体数**）
- 依赖实体单位的还有：富豪榜（`UpdateTopRich`）、贫困税定向（`WealthyPool` / 贫困公民）、遗产分配（`InheritanceEngine` 轮询存活表）、银行放贷、`PerfCounters.MarkFullActorScan`

⇒ `[推断]` 在该模式下，实体单位只剩**名人 / 劳动者 / 将领 / 征召兵**（46630 名背景人口没有 Actor），
我们统计的**人均、基尼、富豪榜、遗产、贫困税、银行放贷**会**静默变成"仅实体单位口径"**：
数字不报错、但不再代表全民，而我们的政策/事件（`政策失败(财政) 王国<…> 基尼=0.96` 这类）会据此下判断。

`[未验证]` 我们**没有**在该模式下实测过；也没有把我们的输出与"模式开关"对齐比对过。

### 2.5 世界法则读取：陈旧缓存风险（P0）`[已核实]`

帝国盒子会**按 id 重注册原版法则资产** `[已核实：EmpireCraftWorldLawLibrary.init()]`：

```csharp
AssetManager.world_laws_library.list.RemoveAll(w => w.id == "world_law_civ_limit_population_100");
AssetManager.world_laws_library.dict.Remove("world_law_civ_limit_population_100");
AssetManager.world_laws_library.add(WorldLawLibrary.world_law_civ_limit_population_100 = new WorldLawAsset() { ... });
```

我们**已经在读原版法则** `[已核实]`，而且**全仓只有一处**（这是好消息：只需硬化一个点）：

- `Core/UnrestEngine.cs:478` — `VanillaRebellionsEnabled()`：
  `return WorldLawLibrary.world_law_rebellions == null || WorldLawLibrary.world_law_rebellions.isEnabled();`
- `Core/PolicyEngine.cs:258` — 调用上面的助手（尊重玩家关掉"外交→叛乱"），不自己读法则

原版语义 `[已核实：decomp/base/WorldLawAsset.cs]`：

- `isEnabled()` → 返回**缓存** `_cached_enabled`（由 `WorldLaws.updateCaches()` 刷新）
- `isEnabledRaw()` → `getOption().boolVal` → 直查 `World.world.world_laws.dict[id]`（**无缓存**）

⇒ `[推断]` 第三方换掉资产对象后，新对象的 `_cached_enabled` 要等下一次 `updateCaches()` 才准；
我们若在这一窗口内读 `isEnabled()`，可能拿到**陈旧值**（表现为"玩家明明关了叛乱法则，我们的动荡系统仍按开处理"或反之）。

### 2.6 玩法叠加：需要"让位"策略（P1）`[已核实：双方系统都在]`

| 领域 | 帝国盒子 | 古典经济学 | 叠加后果 |
|---|---|---|---|
| 叛乱/政权更替 | 派系叛乱、诉求推进、暴君值、`ImperialLegitimacyChallengeService` | 动荡→起义→革命、政策失败内战 | **两套叛乱源**：同时开会出现"莫名其妙的内战/起义"归属不清 |
| 税制 | `EmpireCraftKingdomBehCheckTax`、岁币/朝贡 | 王室税负、财富税、商业税 | 钱双向往返（各自守恒，但总量口径互相干扰） |
| 政体 | `RegimeSystem` + 政体编辑器 | 统治者性格 + 政策槽 | 不同轴，可共存 |
| 法律 | 诉求（削藩/税制/宗教政策/官制调整）、`City.hasReachedWorldLawLimit` | 法典 28×5 档 + 国策 16×3 | 不同轴，可共存 |
| 经济 | `LandEconomySystem`、`MarketSystem`、`PopulationEconomySystem` + 多核后台经济 | GDP/基尼/周期/银行 | **两套经济模型同时改钱**：我们的"经济体检"数字要说明是谁的功劳 |

好消息：我们的政权更替开关**默认就是关**（W1 的 `regime_change_enabled` / `gini_collapse_enabled`），
所以默认状态下不会出现"两套叛乱同时跑"。**只需把这件事写进文档 + 检测到 EC 时在日志里提示一次。**

### 2.7 性能：不冲突但会叠加（P1）`[已核实]`

- 它自己的采样：`原版·updateCities 26.8~38.4 ms/帧`、`原版·updateActors 16.7~26.9 ms/帧`、单次峰值 119~221 ms、最慢帧 980 ms
  ⇒ **瓶颈在"它 + 原版"**，不在我们（我们 4 个补丁 3 个在存档/UI 链，不进逐帧热路径）
- 它已经做了多核后台（理念/经济/举人排名，4 线程 / 64 待办，主线程只写回），我们在 `TradeSimulationWorker` 也后台
- 风险：**同一帧抢 CPU** 会让"最慢帧"更糟；建议实测同开/单开对比（它的 `[每帧耗时]` vs 我们的 `PerfCounters`）
- 另一个（不在本模组范围）：它自带 `OptimizationPatch` / `NativeWorldOptimizationPatch`，与**性能类模组**（VanillaPerf 等）可能重复优化同一批原版内部方法——这才是套件里风险最高的一对

---

## 3. 适配方案

> **实施状态（2026-10-10 当晚）**：P0-1 / P0-2 / P0-3 **已落地并自证** ——
> 编译 `BUILD_OK: 389632`、0 `error CS`、6 条既有告警未增加；门禁 **60/60 全绿**（新增 `Test-EmpireCraftCompat`，6 组变异测试全部 RED）。
> 改动文件：新增 `Core/Compat/EmpireCraftCompat.cs`、`tools/Test-EmpireCraftCompat.ps1`；
> 修改 `Core/UnrestEngine.cs`（法则读取硬化）、`EconomyModMain.cs`（周期日志口径标注 + 首次提示）、
> `UI/EconomyHUD.Overview.cs` 与 `UI/CabinetWindow.Diplomacy.cs`（口径标注行）、6 个 `Locales/*.json`（新增 `virtual_pop_note`）。
> P1/P2 仍待做（P2 需要实机，按用户要求暂不启动游戏）。

### P0-1 无小人模式感知（`Core/Compat/EmpireCraftCompat.cs`）✅ 已实施

```csharp
// 软探测：不引用 EC 程序集，缺了就是 false（与我们对 Optime / 它对我们 的做法一致）
private static bool EcPresent() =>
    AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.GetType("EmpireCraft.Scripts.ModClass", false) != null);   // [已核实] namespace EmpireCraft.Scripts

// 模式判定：世界法则 id 由 EC 注册；键不存在 = 没开（无硬依赖）
public static bool NoCommonersMode()
{
    var lib = AssetManager.world_laws_library;              // [已核实] public static WorldLawLibrary
    var law = lib?.get("empirecraft_law_no_commoners");     // [已核实] AssetLibrary<T>.get(id) → dict 查询
    return law != null && law.isEnabledRaw();               // 用 Raw，避开陈旧缓存
}
```

> `[已核实]` 访问器来源：`decomp/base/AssetLibrary.cs:15/27/50` — `public Dictionary<string,T> dict`、`get(pID)`、`has(pID)`；
> `decomp/base/WorldLawAsset.cs` — `isEnabled()`（缓存）/ `isEnabledRaw()`（直查玩家选项）。

行为（**保守优先：不改玩法，只把口径说清楚**）：

1. 采集时**打一次**日志（`PerfCounters` 里加标志位，不刷屏）
2. HUD / 内阁的「人均」「基尼」旁标注 `（虚拟人口模式：仅实体单位口径）`
3. 依赖"全民分布"的功能（贫困税定向、银行小额放贷）**照旧运行但在日志里标注口径**；是否要"该模式下自动关闭"做成配置项，默认**不自动关**

### P0-2 世界法则读取硬化 ✅ 已实施

- `Core/UnrestEngine.cs` 的 `VanillaRebellionsEnabled()`（全仓**唯一**读法则的地方）改为：
  以 **`isEnabledRaw()`**（无缓存、直查玩家选项）为准；`isEnabledRaw()` 抛异常（选项字典未建立）时退回 `isEnabled()`；
  两者不一致时打一条诊断日志（**这正是"有模组重注册过该法则资产"的信号**）；读不到一律返回 `true`（= 按原版默认，不擅自扩大模组干涉范围）

### P0-3 契约门禁 `tools/Test-EmpireCraftCompat.ps1` ✅ 已实施

| # | 断言 | 变异测试（必须变 RED） |
|---|---|---|
| 1 | 我们的**无条件**补丁目标 ∩ EC 的 180 条目标 = ∅；允许的重叠只能来自 `Services/OptimeCompatibility.cs`（条件层） | 加一条 `Actor.updateStats`；或把重叠目标的归属文件改成非条件层 |
| 2 | 世界法则读取必须用 `isEnabledRaw()`（源码里不得出现 `WorldLawLibrary.*.isEnabled()`） | 把 `isEnabledRaw` 改回 `isEnabled` |
| 3 | EC 探测必须是反射：`EmpireCraft` 只许出现在字符串字面量里（不得 `using EmpireCraft`） | 加一行 `using EmpireCraft.Scripts;` |
| 4 | 无小人模式必须被感知（`empirecraft_law_no_commoners` 恰好出现一次）且**被调用**（`NoCommonersMode()` ≥3 处调用点） | 删掉法则 id 字符串 |
| 5 | 口径标注必须本地化（ch/en/zh_tw/ru 都有 `virtual_pop_note` 且被 `L("...")` 引用） | 删掉某个语言的键 |
| 6 | 门禁自身非空转（EC 目标清单 ≥ 30 条） | 清空清单 |

### P1 文档与开关

- `README.md` / `README_en.md` 增「与帝国盒子共存」章节：两套叛乱/两套经济、无小人模式口径说明、加载顺序无关
- 可选配置 `empirecraft_aware`（`auto` / `off`），默认 `auto`

### P2 联合验收清单（**待用户发话才做**）

1. 两种加载顺序各一次（EC 先 / 我们后 与 反之）
2. 旧存档：EC 已有帝国数据 + 我们 `rb_*` 键 → 读档后两边数据都在
3. **无小人模式 开 / 关 各看一次**我们的 HUD：`Actor=` 计数、人均、基尼（这是 P0-1 的验收点）
4. `logs\error_*.log` 无 `error CS`；两边的"补丁安装完成"日志行都在
5. 它的 `[每帧耗时]` 与我们的 `PerfCounters` 同帧对比（同开/单开）

---

## 4. 明确没做的事 / 没验证的部分

- 本轮**只做静态分析**：读 EC 源码 466 个文件中的相关部分、读反编译原版、读一次同跑日志
- `[未验证]` 我们的模组在**无小人模式**下的实际输出（人均/基尼/政策判定会变成什么样）
- `[未验证]` 国家窗口里"我们的入口按钮 + 法典摘要卡"与 EC 替换后的行布局是否真的不打架
- `[未验证]` 两边恢复时机不同（`loadData` vs `finishingUpLoading`）在**旧存档**上的联合表现
- `[未验证]` 性能同开/单开的量化对比
- **没有**做"让 EC 侧适配我们"（它的 `Compatibility/README.md` 给了 `Owns` / `OwnsObject` / `BlocksEmpireFormation` 这类入口，但需要对方改代码）

---

## 5. 附：怎么快速复核本文结论

```powershell
$m = 'D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods\EMPIRECRAFT(帝国盒子)\Scripts'

# 它有没有碰我们的补丁目标（应全部为 0）
Get-ChildItem $m -Recurse -File -Filter *.cs |
  Select-String -Pattern 'finishingUpLoading|saveWorldToDirectory|StatsWindow\.create'

# 它的无小人模式
Get-ChildItem $m -Recurse -File -Filter *.cs |
  Select-String -Pattern 'empirecraft_law_no_commoners'

# 它重注册原版法则资产
Select-String -Path (Get-ChildItem $m -Recurse -File -Filter 'EmpireCraftWorldLawLibrary.cs').FullName `
  -Pattern 'world_laws_library\.(list\.RemoveAll|dict\.Remove|add)'

# 它替换国家窗口行
Get-Content (Get-ChildItem $m -Recurse -File -Filter 'KingdomWindowPatch.cs').FullName | Select-String 'return false'
```
