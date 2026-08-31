# v0.99 发布说明（可安装候选包已生成）

> 状态说明：大地图快捷键 C 打开内阁面板完成；45/45 门禁 + performance_audit（46 源文件）+ 构建全绿（新增 UnityEngine.InputLegacyModule 引用）。F3 实机验收由用户安装到游戏后亲自执行。

## 新快捷键：C（RulerBox K 键同款）

- **大地图上把鼠标悬停到任意国家/城市，按 C**：未认领 → 自动认领该国并打开内阁面板；已认领 → 直接打开内阁。
- 实现为每帧轮询 `Input.GetKeyDown(KeyCode.C)`（与 RulerBox `Main.Update` 的 K 键完全同款），经鼠标 tile 反查城市/王国（`World.world.getMouseTilePos`）——无需先点开国家窗口，鼠标指着选就行。
- 触发条件：世界中、非 UI 忙碌时生效；入口按钮（账本图标）保留作为补充方式。
- `build_local.ps1` 新增 `UnityEngine.InputLegacyModule.dll` 引用（Input API 所在程序集）。

## 用法速览

1. 大地图鼠标悬停某国（城市上方）→ 按 C → 认领 + 内阁打开（也可点国家窗口右上角账本按钮）
2. 内阁面板：国库 / 政策 / 法令 / 建筑 / **外交**（宣战·求和·结盟·协定·赠礼）/ 政绩记录
3. 认领后 HUD 实时刷新（5 秒节流），外交与法令全部即时生效

## 门禁与兼容

- `Test-NationPlay` 增补 C 键断言（TryHotkeyOpen 存在、每帧轮询 KeyCode.C）。
- 45/45 门禁 + performance_audit + 构建全绿；DLL 哈希包内一致（见打包记录）。

## 暂缓任务

- 无。F3 实机验收由用户安装后亲自执行：大地图悬停国家按 C 验证认领/打开内阁。
