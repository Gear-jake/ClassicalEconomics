# ClassicalEconomics v0.13.0 全面审计与修改总结报告

- 审计对象:`https://github.com/Gear-jake/ClassicalEconomics`(WorldBox 模组,NeoModLoader + C#,39 个源文件,约 470 KB)
- 审计范围:内存泄漏排查 / 全面纠错 / 性能优化(按冻结的执行计划,6 个模块全部完成)
- 修改规模:9 个文件,新增 204 行 / 删除 73 行;对外行为语义**零变化**

## 1. 内存泄漏排查结果(模块 memory-leak-audit)

全部 39 个文件逐一行审阅,重点核查事件订阅、静态缓存、跨世界引用、纹理生命周期、后台线程握手。

**确认并修复 4 处**(详见 `E:\code\new\evidence\leak\leak-checklist.md`):

| # | 位置 | 问题 | 严重度 |
|---|------|------|--------|
| F1 | `TradeSimulationWorker._edgeCache` | 城市毁灭后旧贸易边永久残留(原只按王国消亡清理),长局无界增长 | 中 |
| F2 | `TradeSimulationWorker._flowCityRefs` | 世界重置未清空,旧世界 City 引用滞留 | 低 |
| F3 | `DisasterEngine._prevKingdomPop/_prevKingdomCities` | 死王国条目只增不减 | 低 |
| F4 | `BiomeEconomy` 双缓存 + `DataCollector` 静态池 | 死王国缓存条目 / 旧世界 Actor 引用残留 | 低 |

**确认无泄漏 19 处**(历史环形缓冲、事件双环、图标/圆角纹理、UI 窗口体系、各引擎状态字典的剪除逻辑等,均有界或有正确清理,清单内逐项记录)。

## 2. 纠错结果(模块 bug-fix)

**确认并修复 4 处**(详见 `E:\code\new\evidence\bugfix\bugfix-checklist.md`):

| # | 位置 | 问题 | 严重度 |
|---|------|------|--------|
| B1 | `EventStreamService.GetMinorRecent/GetMajorRecent` | 环形读取起点错误:count<缓冲数时返回"最老 N 条"而非"最近 N 条"(v0.11 修了 HistoryService、漏了这两处) | 低(潜在) |
| B2 | `build_local.ps1` | 硬编码不存在的旧项目绝对路径 + 手写源文件列表 → **克隆者无法复现构建** | 中 |
| B3 | `DataCollector.ApplyWealthTax` | long→int 溢出为负 → 收税/补贴反向"加钱"(BankingEngine 同类已修,此处漏修) | 低 |
| B4 | `ConfigCallbacks.OnTradePowerEnabledChanged` | SWITCH 控件回调却只有 string 重载,设置窗口切换"贸易军力"开关不实时生效 | 中 |

**建议(未改动,供作者决策)**:`EconomyModMain.ClearPlayerLog()` 每次启动清空用户 Player.log,会抹掉游戏与其他模组的崩溃日志,建议改为仅在 `LogToWorldLog` 开启时执行或移除。

## 3. 性能优化结果(模块 perf-optimize)

**已应用 1 项**(详见 `E:\code\new\evidence\perf\perf-checklist.md`):

| # | 位置 | 优化 |
|---|------|------|
| P1 | `GameHelpers.FindKingdom` | 每周期重建王国 O(1) 索引(带世界引用校验防旧世界残留):年度多引擎 O(K²) 与寻路全量重算 O(K³) 线性查找 → O(1)/O(K²) |

**分析后不改动**(后台线程分配、已优化的采集主路径、节流与环形缓冲等,收益不抵风险,清单内逐项记录)。

## 4. 验证证据索引(模块 verify / deliver)

| 证据 | 路径 |
|------|------|
| 基线构建(修复前对照,exit 0,197,632 B) | `E:\code\new\evidence\baseline\baseline_build.log` |
| B2 RED(修复前脚本失败) | `E:\code\new\evidence\bugfix\B2_red.log` |
| B2 GREEN(修复后 BUILD_OK,exit 0) | `E:\code\new\evidence\bugfix\B2_green.log` |
| 最终构建(exit 0,200,192 B) | `E:\code\new\evidence\verify\final_build.log` |
| TDD 证据与冻结例外 | `E:\code\new\evidence\verify\tdd-evidence.md` |
| 泄漏清单 | `E:\code\new\evidence\leak\leak-checklist.md` |
| 纠错清单 | `E:\code\new\evidence\bugfix\bugfix-checklist.md` |
| 优化清单 | `E:\code\new\evidence\perf\perf-checklist.md` |
| 修复 diff 快照 | `E:\code\new\evidence\verify\repo-snapshot\fixes.patch` |

## 5. 修改文件清单(9 个)

- `Core/TradeSimulationWorker.cs`(F1、F2)
- `Core/DisasterEngine.cs`(F3)
- `Core/BiomeEconomy.cs`(F4a)
- `Core/DataCollector.cs`(F4b、B3、P1 挂钩)
- `Core/GameHelpers.cs`(P1)
- `EconomyModMain.cs`(F4b、P1 挂钩)
- `Services/EventStreamService.cs`(B1)
- `Services/ConfigCallbacks.cs`(B4)
- `build_local.ps1`(B2)

## 6. 诚实披露

- 冻结计划中的模块写范围字面为 `src\**`,但本仓库无 `src` 目录(源码位于根目录与 Core/Models/Services/UI)。所有改动均严格限定在模组仓库源码树内(`E:\code\new\ClassicalEconomics\**`),未触及任何模组外文件;此路径差异在此明示,未静默扩大范围。
- 无法在本环境实际运行 WorldBox 游戏做运行时内存 profile;泄漏结论基于静态分析 + 代码路径推导,修复后已通过完整编译验证。建议作者在长局(数百游戏年)中用真实 profile(如 dotMemory/Unity Profiler)复核。
- 审计过程中发现工具环境问题(编辑工具异步删除文件 2 次),已从 git 恢复并完成快照备份,最终 9 个文件完整且编译通过。
