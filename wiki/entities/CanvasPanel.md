---
type: entity
topic: ui-spec
sources:
  - wiki/sources/UGUI-CtrlData机制.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/entity
  - topic/ui-spec
---

# 实体：CanvasPanel

## For Agent

`CanvasPanel`（`Ext/Ext/CanvasPanel.cs`）—— **界面根节点**，对应 Lua 每个界面的 `m_RootPanel`。继承 [[RootPanel]]，额外管 Canvas、动画、安全区、焦点与显隐音效。

## 事实

- `m_AddCanvas`/`m_AddGraphicRaycast`：运行时按需 `AddComponent<Canvas>`/`<GraphicRaycaster>`，追加 `TexCoord1/2` shader 通道（特效/合批用）。
- `m_AffectBySafeArea` + `FitSafeArea()`：刘海/异形屏安全区适配（读 `UGuiUtil.GetSafeAreaTBLR()`）。
- `m_CanvasPanelAnimation`（`AnimationForCanvasPanel`）：打开/关闭动画；`SetVisible` 重写为「可见时先 SetVisible 再播入场动画」；提供 `PlayHideAnimation`/`RewindShowAnimation`/`ReplayShowAnimation`/`GetHideAnimationFrameLength` 供 UIManager 栈切换时精确控制动画进度。
- `onGetFocus` 焦点事件、显隐音效（`m_ShowAudioEvt`/`m_HideAudioEvt`）。
- `SetAlpha` 重写：alpha < 0.5 自动关 Raycaster（透明不吃点击）。

## 关联

- 父类 [[RootPanel]] 持有 CtrlData（见 [[ctrldata-bridge]]）；C#↔Lua 显隐桥接 `OnShow/OnHide`。
