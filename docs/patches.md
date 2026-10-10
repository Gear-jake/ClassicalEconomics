# 补丁清单（Harmony Patches）

> 【W3 结构收敛】本模组的全部 Harmony 补丁统一由 `Core/PatchInstaller.cs` 安装。
> 本文件是**每个补丁的说明书**；`PatchInstaller.Registry` 是**代码侧登记表**。
> 两处必须一致 —— `tools/Test-PatchesManifest.ps1` 会校验。

## 为什么用手动安装而不是 `[HarmonyPatch]` 注解

注解方式对**预编译 DLL 模组**不保证被 NML 应用（`OptimeCompatibility` 的实战教训）。
所以全部改为在首帧（`EconomyTickRunner.Update`）由 `PatchInstaller.InstallAll()` 手动安装，**幂等**。

## 安装时机

`EconomyModMain` → `EconomyTickRunner.Update()` 首帧（`_optimeGuardChecked` 守卫）：

~~~csharp
OnSaveLoaded += AlignYearAfterLoad;
PatchInstaller.InstallAll();
~~~

选择"首帧"而非模组加载期，是因为此时**所有模组都已加载**，才能可靠检测 Optime 等第三方模组。

## 失败隔离

每个补丁独立 try/catch，**单个失败不影响其余**，也不会让整个模组挂掉。
结束时统一输出一行：`补丁安装完成: 成功 N / 失败 M / 共 4`。

---

## 补丁列表

### 1. OptimeCompatibility

| 项 | 内容 |
|---|---|
| 文件 | `Services/OptimeCompatibility.cs` |
| HarmonyId | `ClassicalEconomics.OptimeCompatibility` |
| 目标 | 由 Optime 模组引起的重复注入/异常（检测到 Optime 程序集时才安装） |
| 类型 | Transpiler + Finalizer 兜底 |
| 用途 | 与 Optime 共存，避免两者对同一目标方法重复打补丁导致行为异常 |
| 风险 | 低。**未检测到 Optime 时完全不安装**，不影响任何原版行为 |

### 2. KingdomWindowIntegration

| 项 | 内容 |
|---|---|
| 文件 | `Core/KingdomWindowIntegration.cs` |
| HarmonyId | `com.classicaleconomics.kingdomui` |
| 目标 | `StatsWindow.create` |
| 类型 | Postfix |
| 用途 | 在原版国家信息窗口注入"内阁"入口按钮（中央银行家玩法的入口） |
| 风险 | 低。仅追加按钮；`create` 方法找不到时打印警告并禁用入口，不抛异常 |

### 3. LawSave

| 项 | 内容 |
|---|---|
| 文件 | `Core/LawSave.cs` |
| HarmonyId | `com.classicaleconomics.codexsave` |
| 目标 | `SaveManager.saveWorldToDirectory`（存）/ `MapBox.finishingUpLoading`（读） |
| 类型 | Prefix（存）+ Postfix（读） |
| 用途 | 法典等级数据随存档持久化 |
| 风险 | **中**。挂在存档链路上；仅读写自家 `kingdom.data` 键，失败时只记日志不影响存档本身 |

> 读档恢复**必须**挂 `MapBox.finishingUpLoading` 而不是 `SaveManager.loadWorld` 的 postfix：
> `loadData` 只把任务塞进 SmoothLoader 队列就返回（分帧异步），postfix 时刻王国列表尚未恢复。

### 4. NationSave

| 项 | 内容 |
|---|---|
| 文件 | `Core/NationSave.cs` |
| HarmonyId | `com.classicaleconomics.nationsave` |
| 目标 | `SaveManager.saveWorldToDirectory`（存 Prefix + Postfix Sidecar）/ `MapBox.finishingUpLoading`（读） |
| 类型 | Prefix + Postfix |
| 用途 | 中央银行家数据（认领国家/金库/政策/法令/政绩）随存档持久化，Sidecar 作为兜底通道 |
| 风险 | **中**。同 LawSave；Sidecar 失败时降级为主通道，不阻断存档 |

---

## 新增补丁的步骤

1. 在新文件里写 `TryInstall()`（幂等：`_installed` 守卫 + try/catch）；
2. 在 `Core/PatchInstaller.cs` 的 `Registry` 追加一行；
3. 在**本文件**追加对应章节（含 HarmonyId / 目标 / 类型 / 用途 / 风险）；
4. 运行 `tools/Test-PatchesManifest.ps1` 确认两处一致。

