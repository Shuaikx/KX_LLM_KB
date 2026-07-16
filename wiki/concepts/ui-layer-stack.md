---
type: concept
topic: ui-spec
sources:
  - wiki/sources/UI组件.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

# 概念：UI 层级栈（UILayer）

## For Agent

AOE3D 用约 20 层 `UILayer` 组织 UI 的前后关系，不同 [[prefab-screen-forms|屏幕形态]] 归属不同层。全屏 `Menu_` 走「栈」管理（压栈/出栈），弹窗走冲突队列，HUD/Float 常驻。

## 关键层（低 → 高）

`LowerFloat` → `Float` → `Hud` → `FullScreen`(栈) → `PopupBox` → `MessageBox` → `Tips` → `Loading` …

## 层级 ↔ 形态 对应

- `LowerFloat`/`Float`（1~2 层）：`Float_*` 3D 悬浮 UI。
- `Hud`（第 3 层）：`Hud_*` 常驻。
- `FullScreen`（栈）：`Menu_*`，`ShowStackUI`/`HideStackUI`，打开新页隐藏上一个。
- `PopupBox`（9~10）：`PopupBox_*`，遮罩 + 冲突队列。
- `MessageBox`：`PopupBox_MsgBox` 二次确认。
- `Tips`：`Hud_Msg`/`Hud_StrongToast` 轻/强提示。

## 关联

- 逻辑父子/显隐传播由改造后的 UIBehaviour 提供，见 [[ctrldata-bridge]] 来源 [[UGUI-CtrlData机制]]。
- 触发式弹窗打开时机统一管理见 [[unified-ui-interaction]]。
