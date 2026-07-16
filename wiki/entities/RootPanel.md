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

# 实体：RootPanel

## For Agent

`RootPanel`（`Ext/Ext/RootPanel.cs`）—— 每个 Prefab 根节点，是 [[ctrldata-bridge|CtrlData]] 的**宿主**：持有编辑器导出的控件清单 `m_CtrlList` 和运行时 ctrlData 引用。继承链 `UIBehaviour(改) → Panel → RootPanel → [[CanvasPanel]]`。

## 事实

- `m_CtrlList`（`List<UIBehaviour>`）：编辑器导出的**有序控件数组**，下标 0 固定是根 Panel 本身，是「哪些控件暴露给 Lua」的唯一真源。
- `m_CtrlData`：运行时 Lua ctrlData 表；`SetCtrlData`/`GetCtrlData` 反向存取。
- `m_VerCode`：版本校验码。
- `GetCtrl(index)`/`GetCtrlCount()`/`GetCtrlList()`：按下标/数量取控件。
- `SetVisible` 重写为基于 `CanvasGroup`（alpha/blocksRaycasts/interactable），另有 `SetVisibleUseAlpha`（只改 alpha 不 SetActive，适合频繁显隐）。
- 含 `GuideID`、入退场动画偏移、`m_LayoutResolution` 设计分辨率。

## 关联

- 界面根节点 [[CanvasPanel]] 继承它；构建流程见 [[ctrldata-bridge]]。
