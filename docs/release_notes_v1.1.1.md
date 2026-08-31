# v1.1.1 发布说明（可安装候选包已生成）

> 状态说明：兴建建筑改为放置**游戏原版建筑**（房子/兵营/水井等，RulerBox 式点击地图放置）；45/45 门禁 + performance_audit + 构建全绿。

## 原版建筑放置（RulerBox BuildingPlacementTool 同款交互）

- **用法**：内阁 →「法令·建设」页 →「兴建原版建筑」区选一种建筑（基础房屋/标准房屋/豪华住宅/兵营/瞭望塔/水井/矿井/纪念碑/神庙/篝火）→ 进入放置模式 → **鼠标点击本国领土**即建造（连续放置，无需反复进页）→ **右键取消**。
- **建筑目录**：读取原版 `BuildingAsset`（含 cost 字段）与库；显示 10 种常用建筑；房屋按种族拼接 ID（`house_human_3`，与 RulerBox 的 GetRaceBuild 同规则），通用建筑（水井/矿井/纪念碑/篝火）直接使用原版 ID；原版无对应资产时 fail-closed 提示。
- **费用**：按建筑成本折算金库支付 —— `cost.gold + (wood+stone+metals)/2`，保底 100 金币；余额不足提示、**放置失败自动退款**（不出现钱扣了建筑没了）。
- **放置校验**：仅限陆地区域 + 本国领土（`GetKingdomOfCity` 反射校验，与城市窗口入口同款）；点击 UI 时不会误放置（EventSystem 判定）。
- **落账**：成功计入政绩记录（nation_build_native）与事件流；放置 API `BuildingManager.addBuilding(string, WorldTile)` 经运行时反射调用（编译期 DLL 无此成员，与 startWar 同理）。

## 说明

- 原「模组经济建筑」（市场/粮仓）保留在建设页下方，两者并存。
- `Test-NationPlay` 增补放置模式断言（入口/每帧 tick/反射 addBuilding/领土校验/UI 按钮）。

## 暂缓任务

- 无。安装后体验：选建筑 → 放置模式 → 点击地图连放；反馈放置手感/费用是否合适。
