# v0.92 发布说明（可安装候选包已生成）

> 状态说明：内存清理提示 + 清理面扩全已完成，43/43 门禁（42 项既有 + 新增 Test-MemoryCleanupNotify）+ performance_audit + 构建全绿；可安装候选包 `release/ClassicalEconomics-0.92.zip` 已生成。F3 实机验收由用户安装到游戏后亲自执行。

## 构建与哈希验证

- `build_local.ps1` 全量构建通过（exit 0，`EconomyMod.dll` 231424 字节，VS18 Roslyn csc，42 个源文件）。
- 暂存包 `release\ClassicalEconomics-0.92\EconomyMod\` 内全部文件与项目当前状态逐字节一致；暂存 DLL 与包内 DLL SHA-256 一致（哈希见打包记录，与 `bin\EconomyMod.dll` 相同口径）。
- 注：Roslyn 每次编译生成随机模块 MVID，同一源码连续两次构建字节哈希必然不同；哈希门禁以「暂存与包内逐字节一致」为准。

## 本版本新增：清理提示（三通道）

1. **顶部横幅**（`memory_cleanup_notify_enabled` 新配置，默认开启）：自动清理释放量有意义（估算 ≥0.5 MB 或执行了强制 GC）时弹 `WorldTip` 顶部横幅"已自动清理内存：释放 X MB（收缩 N 个缓冲）"；无事发生不弹，避免每 30 秒扰民。文案四语言本地化（`memory_cleanup_toast`）。
2. **经济面板内存状态行**（概览页，清理开启时常显）：上次清理时间/释放量/收缩缓冲数 + 当前内存分项——"托管堆 X MB｜Unity 已用 Y MB（保留 Z MB）"（`hud_mem_cleanup` / `hud_mem_cleanup_pending` / `hud_mem_usage`，四语言）。
   - **期望管理**：托管堆是模组与游戏本体共享的 Mono GC 口径；Unity 已用/保留是游戏原生资源（贴图/网格/音频）口径。全库审计结论：模组自身无无界泄漏，长局内存增长的大头通常来自游戏本体原生内存与 Mono 不向系统归还已释放页（GC.Collect 不改变任务管理器数字）。若状态行显示托管堆稳定而 Unity 已用持续上涨，即证明增长来自游戏本体。
3. **日志**：每次清理一条 `Debug.Log`（进 player.log），含收缩数、托管堆前后值、精确/估算标记、Unity 内存快照。

## 本版本新增：清理面扩全（只缩容量、绝不清数据）

- **字典重建缩容**（.NET Framework 无 `Dictionary.TrimExcess`，重建+换引用，条目 ≥4096 才触发）：TradeSimulationWorker 10 个静态字典（`_flowCityRefs` + `_residentOwed/PaidByKingdom` + `_edgeCache` + `_knownCityTopology` + `_accScratch` + `_cityIndexScratch` + `_kingdomIndexScratch` + `_boatsScratch` + `_seaCapacityScratch`）、InheritanceEngine `_records`/`_aliveMap`、DataCollector `_cityRefs`、DamageTracker 3 个字典。全部仅在 `!IsBusy() && !IsSettling` 空闲期执行，与在途周期/后台计算零并发。
- **忙碌重试**：清理间隔到达但系统忙碌时按 5 秒短延迟重试，不再把机会推迟到完整间隔之后。
- **EraEngine 死国清扫兜底**：`Tick` 每年清扫已消失王国的 `_prevAvg/_flourishStreak/_kingdomTrait/_startYears` 记录（原清扫挂在 `EraEnabled` 开关下，关闭时残留）。
- **读档清缓存**：检测到读档时 `BiomeEconomy.ClearCache()`（王国 ID 可能属于另一存档，biome/坐标缓存必须失效）。
- **审计确认不动的项**：HistoryService 50 快照环形缓冲（图表数据）、EventStreamService 事件流环形缓冲、TopRich——属产品数据或已有硬上限。
- 审计修订说明：规划阶段曾记录"civilized→非 civilized 降级 actor 的 DamageTracker 记录滞留"，复核代码后确认同窗口 dead 路径已调用 `DamageTracker.Remove`，无残留，未做改动（避免破坏遗产分配语义）。

## 门禁与兼容

- 新增 `tools/Test-MemoryCleanupNotify.ps1`：配置链路五方一致（json/UnrestConfig/Callbacks/四语言/README）、提示三通道存在性与门控、GC.Collect 唯一且行内引用 `MemoryCleanupForceGc`（与 performance_audit 10a 一致）、16 个 ForTrim 访问器声明且唯一调用方为 MemoryCleanupEngine、EraEngine Tick 清扫、读档清缓存、HUD 状态行。
- `tools/Test-ConfigDocs.ps1` 键表纳入 `memory_cleanup_notify_enabled`；README（中/英）性能章节同步。
- 嵌套类型 `AliveRecord`/`Accum` 由 private 放宽为 internal（缩容访问器需要，程序集内可见，不改语义）。
- 42/42 既有门禁 + 新门禁 + performance_audit（42 源文件）+ 构建全绿；`GCSettings` 全项目禁令保持绝对。

## 暂缓任务

- 无。F3 实机验收由用户安装后亲自执行：观察清理横幅与经济面板内存状态行；若托管堆稳定而 Unity 已用持续上涨，增长来自游戏本体而非模组。
