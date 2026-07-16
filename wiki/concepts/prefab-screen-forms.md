---
type: concept
topic: ui-spec
sources:
  - wiki/sources/UI组件.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

# 概念：Prefab 屏幕形态（按前缀）

## For Agent

AOE3D 用 Prefab 文件名前缀区分 UI 的「屏幕形态」，决定其层级、打开方式与骨架。这是业务上最常说的「UI 形式」。可打开界面（`Menu_`/`PopupBox_`/`Hud_`/`Float_`）与不独立打开的子模板（`ListItem_`/`Pan_`）是两大类。层级归属见 [[ui-layer-stack]]，容器承载见 [[ui-containers]]。

## 前缀速查

| 前缀 | 形态 | 管理/层 | 打开方式 |
|------|------|---------|----------|
| `Menu_` | 全屏菜单 | UI 栈，FullScreen 层 | `ShowStackUI`（可压栈） |
| `PopupBox_` | 标准弹窗 | PopupBox 层 | `ShowUI`，走冲突队列 |
| `PopupBox_HoverInfo_` | Hover/Tips | PopupBox 子类 | 跟锚点显示 |
| `Pan_` | 独立浮层 或 子模板 | 视用途 | 侧滑/底部，或挂 [[TemplateBox]] |
| `ListItem_` | 列表项模板 | 挂 [[ListBox]] | 由 `UIListViewEasy` 驱动 |
| `Hud_` | 常驻 HUD | `ConstantUIManager`, Hud 层 | 场景切换自动显隐 |
| `Float_` | 3D 悬浮 | `floatRoot`, Float 层 | 跟随 3D 坐标 |

## 要点

- `Menu_` 有入场/退场动画（`Pan_Anim`），骨架分区 `Pan_Top/Middle/Left/Right/Bottom`。
- `PopupBox_` 走 `UIPopupboxConflictQueue`，内容在 `Pan_Content`。
- `ListItem_` 与 `Pan_` 子模板不单独打开，由父界面驱动；`Hud_` 禁止重逻辑 OnUpdate。
- 选择准则：功能页压栈→`Menu_`；居中确认→`PopupBox_`；长列表→`ListItem_`+[[ListBox]]；固定 N 格→`Pan_`/`ListItem_`+[[TemplateBox]]；地图气泡→`Float_`。
