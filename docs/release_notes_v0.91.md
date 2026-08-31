# v0.91 发布说明（可安装候选包已生成）

> 状态说明：计划任务 1-12 已全部完成，42/42 门禁 + performance_audit + 构建全绿；可安装候选包 `release/ClassicalEconomics-0.91.zip` 已生成。F1/F2/F4 最终验证通过；F3 实机 QA 由用户安装到游戏后亲自执行。

## 构建与哈希验证

- `build_local.ps1` 全量构建通过（exit 0，`EconomyMod.dll` 224768 字节，VS18 Roslyn csc，42 个源文件）。
- 本候选以当前源码（含自动内存清理 + 门禁更新）重新全量构建，并整体刷新暂存包：`release\ClassicalEconomics-0.91\EconomyMod\` 内全部文件（DLL、mod.json、default_config.json、icon.png、README 四语言、Locales 四语言、Icons、GameResources、release notes）与项目当前状态逐字节一致。
- 暂存 DLL（`bin\EconomyMod.dll`）与包内 DLL（`release\ClassicalEconomics-0.91\EconomyMod\EconomyMod.dll`）SHA-256 完全一致：`1718A88FB6C7FFB0DE251417EE83BC2115548A33E4DFD5CD0E2E82513091D0FD`（224768 字节）。该哈希为全部门禁（42 项 Test-*.ps1 + performance_audit.ps1 -SkipBuild）跑完后重新构建的最终产物。
- 注：Roslyn 每次编译生成随机模块 MVID，同一源码连续两次构建字节哈希必然不同（已验证），故哈希门禁以「暂存与包内逐字节一致」为准，而非固定发布哈希。

## 已完成的年度性能治理（本候选包含）

- 自动内存清理（MemoryCleanupEngine）：按配置间隔（默认 30 秒）在游戏空闲时（`!TradeSimulationWorker.IsBusy()` && `!AnnualPipeline.IsSettling`）对三大引擎静态 scratch/缓存集合执行 TrimExcess 缩容，并可按配置强制执行一次 `System.GC.Collect`（全项目唯一 GC 入口，`memory_cleanup_force_gc` 默认关闭）。新增 `memory_cleanup_enabled` / `memory_cleanup_force_gc` / `memory_cleanup_interval_seconds` 三项配置，四语言本地化；`performance_audit.ps1` 10a 门禁同步放宽为仅允许 `Core\MemoryCleanupEngine.cs` 中同时引用 `MemoryCleanupForceGc` 的那一行调用 GC.Collect，`GCSettings` 禁令保持绝对。
- 周期分配卫生：`PrepareRoutes` 等周期路径集合改用静态 scratch + 池复用，`Test-AllocHygiene` 区域扫描通过。
- 实时刷新熔断：阈值 3000→2000 配置化，阈值下保持同步采集，`Test-RealtimeBreaker` / `Test-RealtimeConfig` 通过。
- 年度分帧状态机（计划任务 7）：`frame_budget_ms` / `cycle_window_ms` 配置化，收尾按帧预算推进，超时先延窗再按 消费→银行→其他 削减，税收永不削减。
- 继承分片扫描（计划任务 9）：`inheritance_scan_per_frame` 配置化，3 秒窗口内每帧限流扫描。
- 消费/银行年度上限（计划任务 4）：`spending_cap_per_year` / `banking_default_cap_per_year` / `banking_contagion_cap_per_year` 配置化限流。
- 年度收尾性能诊断（计划任务 1）：`perf_diagnostics_enabled` / `cycle_alloc_budget` 配置化，关闭时零开销。
- 文档与配置同步（计划任务 11）：`tools/Test-ConfigDocs.ps1` fail-closed 门禁覆盖全部性能键（含本次三项内存清理键）在 default_config.json、UnrestConfig、ConfigCallbacks（AllConfigIds + 有界 ParseInt + 回调）、四语言 locale（键 + Description）与 README（中/英）中的一致性；README 性能章节、性能验收清单与本文档同步更新。
- 静态回归确认：42/42 门禁 + performance_audit -SkipBuild 全绿（本次重跑结果）。

## 暂缓任务

- 无。计划任务 1-12 已全部完成；本候选不包含任何暂缓实现项。
- F1/F2/F4 最终验证通过（构建、哈希、包内容核对）；F3 实机 QA 由用户安装后亲自执行，结果待用户在游戏中确认。

## 兼容与验证

- 本候选与 v0.90 保持同一经济语义与既有门禁基线；`GC.Collect` 仅存在于 `Core\MemoryCleanupEngine.cs` 中由 `memory_cleanup_force_gc` 门控的唯一一行，`GCSettings` 保持全项目禁用。
- 可安装候选包已生成（`release/ClassicalEconomics-0.91.zip`），DLL 哈希 `1718A88FB6C7FFB0DE251417EE83BC2115548A33E4DFD5CD0E2E82513091D0FD` 已记录；F3 实机验收由用户安装后亲自执行。