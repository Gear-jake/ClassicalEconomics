# Optime 上游缺陷反馈：ActorJobFlatten 空引用崩溃

> 整理人：Classical Economics 模组作者（Jake）
> 日期：2026-08-11
> 目标：Optime (ApexLite) — Steam 创意工坊 ID `3387086485`，本机版本 0.4.0beta

## 一、问题描述

安装 Optime 后开启 `ActorJobFlatten` 优化功能，游戏在**生物大规模死亡后**随机崩溃，抛出 `NullReferenceException`。

## 二、崩溃堆栈（游戏 0.51.2 + NML + Optime 0.4.0beta）

```
NullReferenceException: Object reference not set to an instance of an object
Actor.u1_checkInside (System.Single pElapsed) (at /github/workspace/Assets/Scripts/base/actors/ActorUpdaters.cs:89)
Optime.Optimizations.ActorJobFlatten.BatchActors_u4_deadCheck_Prefix (BatchActors& __instance) (at Optimizations/ActorJobFlatten.cs:99)
(wrapper dynamic-method) BatchActors.DMD<BatchActors::u4_deadCheck>(BatchActors)
Batch`1[T].runUpdater (Job`1[T] pObj) (at /github/workspace/Assets/Scripts/base/updaters/core/Batch.cs:120)
Batch`1[T].updateJobsPost (System.Single pElapsed) (at /github/workspace/Assets/Scripts/base/updaters/core/Batch.cs:109)
JobManagerBase`2[TBatch,T].updateBaseJobsPost (System.Single pElapsed) (at /github/workspace/Assets/Scripts/base/updaters/core/JobManagerBase.cs:114)
...
```

## 三、根因分析

`ActorJobFlatten` 用 `[HarmonyPrefix]` 将原版 `BatchActors.u4_deadCheck` 替换为扁平循环：

```csharp
Actor[] actors = __instance._array;
int count = __instance._count;
float elapsed = __instance._elapsed;

for (int i = 0; i < count; i++) {
    Actor actor = actors[i];
    actor.u1_checkInside(elapsed);   // ← 此处 NRE
    actor.u4_deadCheck(elapsed);
    ...
}
```

**缺陷**：`BatchActors` 底层是共享对象池数组（`Batch._array`）。actor 死亡/移除后，其槽位会被回收但**可能保留 null 引用**（Unity 对象延迟销毁）。原版 `Batch.runUpdater` 的批处理内部遍历会跳过 null，而扁平化重写直接解引用，一旦遍历到空槽位即抛 NRE。

**为什么与本模组（Classical Economics）联动后更容易触发**：本模组显著提高生物死亡率（饥荒经济冲击、杀富济贫革命、战争掠夺蒸发等），死亡/移除的 actor 槽位暴增，命中 null 槽位的概率随之升高。崩溃堆栈中完全没有本模组帧，属于 Optime 自身的健壮性缺陷。

## 四、补丁建议（最小改动）

在扁平循环体内加空引用防御，与原版批处理语义保持一致：

```csharp
for (int i = 0; i < count; i++) {
    Actor actor = actors[i];
    // 共享数组池在 actor 死亡/移除后槽位可能为 null，
    // 原版批处理内部会跳过 null，扁平化重写必须同样防御。
    if (actor == null) {
        continue;
    }
    actor.u1_checkInside(elapsed);
    actor.u4_deadCheck(elapsed);
    ...
}
```

## 五、复现建议

1. 安装 Optime（开启 `ActorJobFlatten`）+ 任意提高死亡率的模组（或游戏内大规模饥荒/战争）
2. 长时间运行观察；空槽位需要 actor 死亡/移除与数组复用碰撞才会出现，属概率性触发
3. 崩溃日志特征：`error_*.log` 中堆栈命中 `Optime.Optimizations.ActorJobFlatten.BatchActors_u4_deadCheck_Prefix`

## 六、本侧兼容兜底（供参考）

Classical Economics v0.8.1 内置 `OptimeCompatibility` 兼容层：

- 启动首帧检测 Optime 程序集，命中后对 `BatchActors.u4_deadCheck` 挂 Harmony Finalizer
- Finalizer 仅当异常堆栈包含 `ActorJobFlatten` 时吞掉该帧异常（跳过本帧批次更新），其余异常照常抛出
- 正常路径零开销（Finalizer 仅在异常时执行）

该兜底不影响 Optime 修复上游缺陷，仅作为未升级用户的防崩溃保障。
