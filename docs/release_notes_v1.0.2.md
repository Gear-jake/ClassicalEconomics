# v1.0.2 发布说明（可安装候选包已生成）

> 状态说明：修复宣战/结盟显示"外交接口不可用"；45/45 门禁 + performance_audit + 构建全绿。

## 修复：外交接口不可用

**症状**：内阁外交区点击"宣战/结盟"弹出"原版外交接口不可用"。

**根因**（两处，均已修复）：
1. **startWar 方法**：编译期引用的游戏 DLL 中 `DiplomacyManager` 并没有 `startWar` 成员（该方法是运行时才有—— 等模组之所以能用是因为源码随 NML 运行时编译）。改为**运行时反射定位一次并缓存**，找不到才报不可用（并写诊断日志）。
2. **战争类型资产**：`whisper_of_war` 在部分游戏版本/翻译环境下取不到。改为**候选 ID 兜底**（whisper_of_war → war → rebellion → invasion），全部失败才报不可用。

其余外交 API（endWar/getWars/newAlliance/join/isEnemy/getOpinion 等）经编译验证在公开面，保持编译期调用。

## 门禁与兼容

- 45/45 门禁 + performance_audit（46 源文件）+ 构建全绿；DLL 哈希包内一致（见打包记录）。

## 暂缓任务

- 无。安装后请重点试宣战与结盟；若仍报错，发我 player.log 中 `[ClassicalEconomics] 外交` 开头的行。
