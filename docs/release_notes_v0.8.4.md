# 古典经济学 Classical Economics v0.8.4

WorldBox 古典经济学模组 · 全面优化精简版

## v0.8.4 变更（本次）

### 死代码废除（15 处，行为零影响，全经跨文件验证）
- `TradeSimulationWorker`: `Accum.Id`、`KingdomSim.Food/Boats` 只写字段删除
- `KingdomMonitorEngine`: `KingTrack.Seen` 只写字段删除
- `BiomeEconomy`: `GetName`/`IsComplementary` 死方法 + 双语名称数组删除
- `BankingEngine`: `_kingdomCredit`/`LastDefaults` 只写状态删除
- `EraEngine`: `HasActive` 零消费属性删除；清理消亡王国时移除 `flourishStreak` 残留
- `EconomyModMain`: `_tickRunner` 只写字段删除
- `EventStreamService`: `GetRecent` 兼容入口删除；对象池注释与实现对齐
- `KingdomStats`: `PopulationCapacity`/`FoodPerCapita`/`Workers`/`Productivity` 只写字段删除
- `UnrestConfig`: `Instance` setter 降为 private

### 缺陷修复（10 处）
- **战争掠夺静默失效**：反射读取 boxed 枚举直接 unbox int 抛 `InvalidCastException` 被吞，导致战争胜利掠夺从未生效 → 改 `System.Convert.ToInt32`，v0.8.4 起战争掠夺真正生效
- **革命延迟门恒不可达**：`GetState(state=2)` 的 elapsed 含叛乱前累积年数，使 `RevolutionDelayYears` 门失效 → `UnrestState` 新增 `RebelYear`，按实际叛乱年份计算持续年数
- **起义判据缺失**：`killed>0` 未含 `affected>0` 主事件 → 改为 `result>0`
- **特质残留**：`affected==0`（无城市无成员）时撤销动荡特质
- **金币溢出**：`rich.addMoney(-(int)lossPerRich)` long→int 溢出为负反而加钱 → `Mathf.Min` 钳制
- **tile 反射软失败**：enum `as string` 恒 null → `ToString()` 取值
- **FailPolicy 短路歧义**：分支 3 的 `||` 隐式合并 → 显式判断已叛乱状态
- **免费武器/造楼/双重扣款**：SpendingEngine 四类消费统一先扣款后回报
- **金币守恒**：`ApplyWealthTax` per==0 时金币静默销毁 → 余数全数补给首穷人
- **环形清理缺陷**：`HistoryService.ClearHistory` 线性清空残留旧快照 → 环形清空

### 精简重构（4 项）
- `EconomyCycleModulator`: `ApplyTaxPolicy` 删除恒 false 参数（`TaxLocalHigh`/`TaxTributeHigh` 废弃常量删除），`MoneySupply` 降 private
- `EconomyModMain`: 抽取 `ResetAllEngines(bool full)`，Reload 与新地图分支共用
- `GameHelpers`: `Shuffle<T>` Fisher-Yates 公共化，`EraEngine`/`UnrestEngine` 复用
- `TradeSimulationWorker`: `globalWealths` 二次遍历合并进单遍聚合

## v0.8.3 变更（补录）
- 事件流双环改造：重大事件（革命/起义/泡沫破裂/灾害/银行/时代/崩溃/改革失败/王位/掠夺，容量 100）与普通事件（60）双环形缓冲，`IsMajorType` 分流
- 事件窗口顶部类型统计行 + 重大事件区块在上，修复高频消费事件挤掉低频大事件的问题

## v0.8.2-hotfix 变更（补录）
- Optime 兼容层升级：**零 Optime 修改**，`Services/OptimeCompatibility.cs` 双防线（Harmony Transpiler 注入 actor==null 跳过 + Finalizer 兜底）
- 修复 Transpiler 运行时 `InvalidCastException`（stloc operand 必须用 `ExtractLocalIndex()` 提取）

## 兼容性
- 目标游戏：WorldBox 0.51.2+
- 依赖：NeoModLoader (NML)
- 兼容 Optime（零修改方案）、四语言界面（简体中文/繁體中文/English/Русский）

## 仓库内容
- `ClassicalEconomics/` 全部 C# 源码
- `README.md` + `README_zh_tw.md` + `README_en.md` + `README_ru.md` 四语言说明
- `STEAM_WORKSHOP_DESC_*.txt` 四语言创意工坊描述（纯 BBCode）
- `docs/` 设计文档与兼容性反馈
