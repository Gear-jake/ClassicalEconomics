# ClassicalEconomics 内存、性能与代码框架审计报告

**审计日期**：2026-08-20  
**对象**：`E:\code\new\ClassicalEconomics` 当前工作树  
**目标运行环境**：WorldBox 0.51.x + NeoModLoader  
**结论状态**：代码与可执行门禁已完成；真实 WorldBox 长局复测仍需人工加载固定存档后执行。

## 执行摘要

本轮审计没有把“进程内存上涨”直接等同于“ClassicalEconomics 托管内存泄漏”。Unity 官方将内存区分为托管堆、C# 非托管内存和 Unity 原生内存；现有 CSV 只记录 `worldbox.exe` 的进程私有内存、工作集、句柄、线程和 CPU，无法单独归因到模组。[Source 4][Source 5]

本轮确认并修复了三组可证明问题，并关闭了一条不可行方案：

1. **关闭无效卸载方案**：旧 NML 教程描述了 `IUnloadable.OnUnload()`，但当前 NML 源码已将该接口标记为 obsolete 且“没有实际效果”。此外，销毁 `PowersTab` 不会自动清除 NML 的 Tab entry、PowerTabAsset 和内部注册表。审计过程中曾实现该方案，严格复审后已完整撤销，避免制造虚假安全感。[Source 1][Source 2][Source 8]
2. **继承扫描维护重复人口索引**：`InheritanceEngine` 每 3 秒同时维护 `_seen: HashSet<long>` 和 `_aliveMap: Dictionary<long, Actor>`，两者键集合完全相同。现已删除 `_seen`，死亡检测直接复用 `_aliveMap.ContainsKey`。
3. **继承优化的行为漂移**：当前工作树曾把子女缓存截断为 32 人，可能漏掉仍存活的后序子女并改变遗产归属。现已取消硬上限，保留懒分配但恢复“全部子女参与继承”。
4. **测试代码污染发布程序集及构建竞争**：C# 基准一度被 `build_local.ps1` 当成生产源码编入 DLL；并发构建还会争用固定的诊断文件和 DLL。现已排除 `tools/`，并用命名 Mutex 串行化构建。

独立 C# 基准在 20,000 人口、每 7 人移除 1 人、100 次迭代下得到：死亡 ID 数与顺序完全一致；哈希写入从 **3,462,886** 降至 **1,731,443**，正好减半；最终单次验证计时从 **857,115 ticks** 降至 **212,970 ticks**。计时仅证明该数据结构变更在独立算法表面更快，不代表游戏整体帧率会按相同比例提升。

## 1. 方法与证据等级

| 等级 | 含义 | 本报告中的使用方式 |
|---|---|---|
| MEASURED | 工具实际执行产生的数据 | WorldBox 进程 CSV、独立 C# 基准、编译产物 |
| OBSERVED | 当前源码、官方文档、调用链可直接确认 | 生命周期、复杂度、资源所有权、清理路径 |
| DERIVED | 从测量或源码推导，公式明确 | O(N)、O(N log N)、每小时 OLS 斜率 |
| UNRESOLVED | 当前环境无法可靠证明 | Unity 托管堆对象归属、帧时间 p95/p99、长局 retained objects |

研究覆盖：39 个生产 C# 文件、Core/Services/UI/Models 四层、构建与内存工具、6 组历史进程采样、NML 中文教程、官方 ModExample、Unity 与 Microsoft 内存文档。

## 2. 框架与调用链分析

### 2.1 模块结构

```text
EconomyModMain / EconomyTickRunner
  -> DataCollector（主线程读取 Unity 世界对象）
  -> TradeSimulationWorker（纯数据双缓冲 + ThreadPool 计算）
  -> EconomyEngine（发布当前周期统计）
  -> 年度引擎链（周期、动荡、政策、人口、消费、时代、贸易军力、灾害、银行）
  -> HistoryService / EventStreamService（固定容量环形存储）
  -> EconomyUI（可见窗口按需刷新）
```

整体方向是合理的：Unity 对象读取与寻路留在主线程，后台只接收值类型/纯数据；结果通过单周期握手发布。`TradeSimulationWorker` 的 generation、active-worker 与生命周期锁用于阻止旧世界任务覆盖新世界状态。

### 2.2 主要框架风险

`TradeSimulationWorker.cs` 已超过 1,500 行，同时承担采集缓冲、路由图、后台统计、贸易计算、结算和生命周期握手。它是当前最深但也最拥挤的模块。由于缺少行为级测试，本轮没有做大规模拆分；贸然拆分会放大线程与所有权风险。

`EconomyModMain.cs` 是世界切换、热重载和年度周期的生命周期汇合点。旧教程明确描述过 `IUnloadable.OnUnload()`，但当前 NML 官方源码将其标为无实际效果；禁用路径只记录期望状态，并不调用 `OnUnload()` 或销毁已加载模组对象。因此，不能依靠该接口解决当前版本的进程内禁用/再启用清理。[Source 1][Source 2][Source 8]

## 3. 内存泄漏与资源生命周期

### 3.1 当前 NML 下不可实现的进程内卸载

严格复审确认，旧教程与当前 NML 源码发生冲突：当前 `IUnloadable` 没有实际效果。即使强行调用清理方法，`TabManager.CreateTab` 还会创建独立的入口按钮、`PowerTabAsset` 和多组内部注册表，而公开 API 没有完整 unregister 操作。只销毁 `PowersTab` 会留下悬空按钮/回调；随后销毁图标纹理还可能让残留 UI 引用已销毁资源。

因此本轮**没有保留**不可达的 `OnUnload()` 实现，也不声称已经解决“禁用后不重启游戏即可完整卸载”。当前可靠生命周期仍是：

- 返回主菜单/世界不可用：清除所有世界对象引用、事件动态内容和后台结果。
- 新地图：重置全部经济状态与历史。
- 热重载：按现有幂等保护重置引擎，不重复创建 TickRunner/UI。
- 进程退出：由 Unity/NML 释放进程级对象。

### 3.2 已有且有效的有界结构

- `HistoryService`：50 条固定环形缓冲，清空时逐槽置 null。
- `EventStreamService`：普通 60 + 重大 100 双环，覆盖时复用 `EventEntry`，回池前清字符串。
- `DamageTracker`：每个受害者最多 16 个攻击者，10 次无新伤害后过期，死亡/世界退出清理。
- `InheritanceEngine`：每个存活文明 Actor 一条记录；子女 ID 懒分配，不再设置 32 人硬上限；世界退出清空。
- `TradeSimulationWorker`：边数、寻路队列和历史排名受配置/常量约束；世界退出使旧 generation 失效。

### 3.3 不能由进程 CSV 证明的事项

Unity 官方说明进程内存包含托管、C# 非托管和 Unity 原生层；进程私有内存上涨可能来自纹理、场景、图形驱动、原生分配器保留或托管存活对象。[Source 4] Microsoft 进一步说明静态字段、线程栈和 GC handle 都是 GC Root，但只有对象级快照才能判断具体保留链。[Source 5]

因此，必须使用 Unity Memory Profiler 在固定存档中比较：预热后、500 周期后、回主菜单 10 秒后、20 次世界切换后。当前报告不声称“进程斜率低于 1 MiB/h”已经达标。

## 4. CPU、GC 与算法热点

### 4.1 已优化：继承/伤害共享扫描

`InheritanceEngine.Tick` 每 3 秒扫描文明 Actor，同时驱动 `DamageTracker.CheckActor`。原实现对每个有效 Actor 做：

```text
seen.Add(id) + aliveMap[id] = actor
```

随后只用 `seen.Contains(id)` 检测死亡。`aliveMap` 已包含完全相同的 key，因此 `_seen` 是冗余的人口级哈希表。

优化后：

```text
aliveMap[id] = actor
dead iff !aliveMap.ContainsKey(recordId)
```

**MEASURED**：

| 指标 | 旧算法 | 新算法 | 变化 |
|---|---:|---:|---:|
| 死亡 ID 数 | 2,857 | 2,857 | 一致 |
| 哈希写入 | 3,462,886 | 1,731,443 | -50% |
| 100 次计时 | 857,115 ticks | 212,970 ticks | -75.2% |

### 4.2 保留：精确基尼排序

全局基尼和每王国基尼采用精确升序排序：复杂度约为 `O(N log N + Σ n_k log n_k)`。列表/累加器已复用，年度后台计算的 GC 压力较低。近似分桶或采样会改变经济周期、政策、时代与价格等输出，因此本轮拒绝近似算法。

大地图实时刷新已在 3,000 存活单位处熔断，只刷新 UI；默认实时刷新关闭。这一策略比改变基尼语义更稳妥。

### 4.3 待 Profiler 决策：路由图与 UI 重建

- `PrepareRoutes` 每次采集执行王国对邻接检查，最坏 `O(K²)`；完整候选重建受拓扑变化、周期和 MaxEdges 限制。
- 可见窗口刷新采用“销毁动态内容并重建”；图表会创建数组、GameObject 与文本。不可见窗口不会刷新，Tooltip 布局也已按数据索引节流。

两者都可能造成主线程尖峰，但在没有 Unity Profiler marker/GC.Alloc 的情况下，不应做高风险重写。

## 5. 历史 WorldBox 进程采样

![六组 WorldBox 进程私有内存轨迹](../.omo/ulw-research/20260820-141556/memory-captures.png)

六组采样经重新分析后，使用 OLS 而不是仅看首尾值，并剔除线程数跌至初始值一半以下的进程退出段。

| 场景 | 有效时长 | 末 5 分钟 Private 斜率 | 说明 |
|---|---:|---:|---|
| 禁用模组 fresh world | 20.06 分钟 | +44,666.9 MiB/h | 单次对照 |
| 启用模组 fresh world | 20.07 分钟 | +60,981.8 MiB/h | 单次实验 |
| 修复版运行 | 18.13 分钟 | -4,181.3 MiB/h | 有效段末期发生大规模回收 |

两组 20 分钟样本不是同一进程，也没有达到“每组至少 5 次”的验收标准。差值不能作为模组泄漏量。修复版末期斜率为负也不能证明完全无泄漏，只能证明该进程并非一直单调增长。

完整窗口结果见 `E:\code\new\.omo\ulw-research\20260820-141556\memory-window-summary.csv`。

## 6. 验证证据

### 6.1 RED -> GREEN

| 场景 | RED | GREEN |
|---|---|---|
| 继承扫描 | 2 个缺口：仍有 `_seen` 且未复用 aliveMap | `PERFORMANCE_AUDIT_OK` |
| 子女继承语义 | 1 个缺口：只缓存前 32 名子女 | 取消硬上限后 GREEN |
| 构建边界 | 1 个缺口：tools 被编入 DLL | 排除 tools 后 GREEN |
| 并发构建 | 双构建争用共享文件并失败 | `CONCURRENT_BUILD_GREEN`；abandoned owner 可恢复 |

### 6.2 最新命令结果

```text
INHERITANCE_SCAN_EQUIVALENT: dead=2857
INHERITANCE_SCAN_HASH_WRITES: old=3462886 new=1731443
INHERITANCE_SCAN_TIMING_TICKS: old=857115 new=212970
```

```text
DAMAGE_SHARE_INVARIANTS_GREEN: 4 scenarios passed
```

```text
CONCURRENT_BUILD_GREEN: 2 serialized builds passed
ABANDONED_BUILD_MUTEX_GREEN: abandoned owner recovered
```

```text
BUILD_OK: E:\code\new\ClassicalEconomics\bin\EconomyMod.dll (212480 bytes)
PERFORMANCE_AUDIT_OK: 39 source files, build and invariants passed
```

C# LSP 已配置但未安装，因此没有 LSP 诊断证据；Roslyn `csc.exe` 完整生产编译为当前类型/语法门禁。

## 7. 剩余风险与下一轮实机计划

### 必须人工执行的 P0 场景

1. 固定 WorldBox/NML/模组集合、图形设置、时间倍率和存档。
2. 对照组与实验组各运行至少 5 次，每次预热 10 周期并采集 100 周期以上。
3. Unity Profiler 记录主线程 p50/p95/p99、年度尖峰、`GC.Alloc`、Gen0/1 与暂停。
4. Memory Profiler 抓取 4 个时点，检查旧世界 Actor/City/Kingdom 的 GC Root。
5. 当前 NML 不支持真实 `IUnloadable`；若未来 NML 恢复卸载，应先获得完整 Tab unregister 接口，再测试禁用/再启用与注册表基数。
6. 两个存档往返 20 次，确认 `_activeWorkers` 回到 0、旧世界对象不累积。

### 暂不建议的优化

- 近似基尼系数：会改变经济语义。
- 复用 `KingdomStats` 对象：历史快照浅拷贝引用，复用会篡改历史。
- 仅按王国 ID 缓存邻国关系：领土几何会变化。
- 池化 `CycleResult`：当前 `LastResult` 跨多个主线程消费者，所有权不够清晰。
- 全面重写 UI 为 retained rows：缺少可见窗口的 Profiler 证据与视觉自动化环境。

## 8. 结论

当前代码已在不改变死亡检测结果的前提下减少继承扫描的主线程哈希工作和峰值容器容量，取消了会改变遗产归属的 32 子女硬上限，并修复了工具代码污染发布程序集与并发构建争用。生产编译、静态不变量和独立算法等价/守恒基准均通过。

“模组禁用时完整卸载”在当前 NML 中不是可达能力。本报告明确拒绝用旧教程覆盖当前源码事实，也没有保留一条无法被加载器调用、且无法完整注销 Tab 注册表的清理路径。

对“游戏整体是否达到长期内存稳定、帧时间改善多少”的结论仍保持保守：已有进程采样不足以分离 Unity/WorldBox 原生内存与模组托管对象，必须完成固定场景的多次 Unity Profiler 与 Memory Profiler 复测后才能关闭该项。

## 来源

1. **NML 中文教程：模组接口**，`IUnloadable.OnUnload()` 契约。https://worldboxopenmods.gitbook.io/mod-tutorial-zh/ji-chu-gai-nian/modinterface.md （访问：2026-08-20）
2. **WorldBoxOpenMods ModExample**，入口实现 `IReloadable, IUnloadable` 并在卸载时移除 Trait。https://github.com/WorldBoxOpenMods/ModExample/blob/master/ExampleModCode.cs （访问：2026-08-20）
3. **NML 中文教程：事件监听**，说明 Listen-Handler 用于降低多模组 Patch 冲突与 Prefix/Postfix 开销。https://worldboxopenmods.gitbook.io/mod-tutorial-zh/qi-ta-ji-shu/listenhandle.md （访问：2026-08-20）
4. **Unity Manual：Memory in Unity introduction**，托管、C# 非托管、Unity 原生三层内存。https://docs.unity3d.com/Manual/performance-memory-overview.html （访问：2026-08-20）
5. **Microsoft Learn：Fundamentals of garbage collection**，静态字段/线程栈/GC handle 作为 GC Root。https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals （访问：2026-08-20）
6. **Unity Manual：Optimizing your code for managed memory**，减少频繁分配、复用对象以降低 GC。https://docs.unity3d.com/Manual/performance-optimizing-code-managed-memory.html （访问：2026-08-20）
7. **NML 中文教程索引**。https://worldboxopenmods.gitbook.io/mod-tutorial-zh/llms.txt （访问：2026-08-20）
8. **NeoModLoader 当前源码**，`IUnloadable` obsolete 标记及当前禁用/Tab 注册实现。https://github.com/WorldBoxOpenMods/ModLoader （访问：2026-08-20）
