# v0.10.0 发布说明（2026-08-13）

## 城市级地理贸易网络

贸易引擎从"王国间抽象金流"重构为真实的**城市级地理网络**——城市为节点、王国为聚合层，金流沿真实地图路径结算。

### 城市为节点
- 城市以 tile 坐标唯一标识（`cityId = ((long)x << 32) | (uint)y`，原版 City 无稳定 id）
- 供需缺口 = 城市金币 − 仓库容量，盈余出口 / 缺口进口，按边互补零和结算
- 出口城 `addResourcesToRandomStockpile` 入金库，进口城 `takeResource` 兜底 `DeductCoins`（与原版缴税同渠道），全图净≈0

### 真实仓库容量
- 优先使用**原版真实仓库容量**（`ResourceLibrary.gold.storage_max`，游戏原版自带仓库系统）
- 原版 API 不可用（=0）时回退到 `建筑数 × TradeCityBaseCapacity × 50%` 估算
- 开关 `trade_use_real_stockpiles` 可切换

### 寻路建边
- 邻国王国对 → 全部城市对直接建边（陆路，成本=欧氏距离）
- 非邻国王国对 → 取"最近城市对"，距离 ≤ MaxTradeRange 才入寻路队列，`PathfinderTools.raycast` 确认可达
- 寻路路径海洋占比 > 50% ⇒ 海路
- 邻国判定代理：`Kingdom.distanceBetweenKingdom ≤ MaxTradeRange`（`City.neighbours_kingdoms` 为 internal 不可访问）

### 成本与衰减
- 边成本 = (陆路距离 + 海路距离 × SeaRoutePenalty) × (邻国 ? 1 : NonNeighborPenalty)
- 贸易量 ∝ 1 / (1 + cost × DistanceDecay) × 供需缺口 × TradeFlowRatio

### 海路容量
- 海路贸易量上限 = 出口王国 Boats × SeaCapacityPerBoat(10)，超限比例缩放
- 无船海路贸易额清零

### 寻路缓存
- 三类失效：`Reset()` 世界重置清空 / 每 PathRecomputeEvery 周期全量重算 / 王国生灭增量增删
- 主线程限流：每周期最多 MaxPathfindPairs 对寻路（`PathfinderTools.raycast` 仅主线程调用），剩余下一周期补齐

### v0.9.1 兼容
- 生产函数（Workers × Productivity × CapitalFactor）
- 区域价格指数（LocalPrice = CPI × 供需系数，clamp 0.5~2）
- 价格离散度（PriceDispersion = CV）
- 平均距离衰减因子（AvgDistanceFactor）、DistanceDecay / TransportCost / PriceDiffWeight HUD 展示字段
- `postCycle` 按钮同步路径、`IsBusy` / `HasPendingResult` 防护、`ApplyWealthTax` 的 `per<=0` 金币守恒修复

### 新增配置（四语言本地化）
| 配置 | 范围 | 默认 | 说明 |
|------|------|------|------|
| 每周期寻路对数 | 1~500 | 100 | 主线程每周期最多寻路的城市对（限流） |
| 寻路重算周期 | 5~100 | 20 | 地形变化后全量重算寻路缓存间隔 |
| 贸易最大距离 | 10~500 | 120 | 非邻国城市对距离超限不建边 |
| 海路惩罚系数 | 1~10 | 3.0 | 海路成本 = 陆路 + 海路 × 系数 |
| 非邻国惩罚 | 1~10 | 2.0 | 非邻国城市对成本乘系数 |
| 贸易边数上限 | 100~50000 | 8000 | 寻路缓存最大边数 |
| 城市容量基准 | 10~500 | 50 | 兜底容量 = 建筑数 × 值 × 50% |
| 使用原版真实仓库 | 开关 | 开 | 城市缺口用原版金库容量 |

## 性能
- 全部统计与贸易量计算在后台线程，主线程仅采集 + 寻路（限流）
- 城市对按邻国/最近对策略收敛，避免 O(n²) 城市对爆炸
