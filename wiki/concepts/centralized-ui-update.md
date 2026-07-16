---
type: concept
topic: ui-spec
sources:
  - wiki/sources/UGUI-CtrlData机制.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

# 概念：集中式 UI Update（UGUIUpdateMgr）

## For Agent

AOE3D 对原生 UGUI 最重要的性能改造之一：把成百上千 UI 组件各自的 `MonoBehaviour.Update()` 合并成**单一分发器**统一广播，避免 Unity 逐个回调的巨大开销。

## 机制

- 组件调用 `SetNeedUpdate(true)`/`SetNeedLateUpdate(true)` 才注册。
- `RegisterUpdate()` 把 `OnUpdate` 挂到单例 `UGUIUpdateMgr.Instance.OnUpdate`。
- `UGUIUpdateMgr`（`Runtime/UI/Core/UGUIUpdateMgr.cs`）是 `DontDestroyOnLoad` 单例，只有它自己有真正的 `Update()/LateUpdate()`，再 `OnUpdate?.Invoke()` 广播。
- 全局开关（`s_ButtonUpdateRegister`/`s_EffectUpdateRegister`/`s_TextLateUpdateRegister` 等）可整类关闭以降端机开销，带 Profiler 采样点（CpuSample 21/22）。

## 关联

- 属于 [[UGUI-CtrlData机制|UGUI 改造]] 的一部分，与 [[ctrldata-bridge]] 同源。
- 对照：原生 UGUI 每组件各自 `Update()`；改造后单点广播、可整类开关、带 Profiler。
