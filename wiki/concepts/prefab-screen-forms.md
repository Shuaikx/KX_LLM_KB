---
type: concept
topic: ui-spec
sources:
  - raw/topics/ui-spec/UI组件.md
  - wiki/sources/UI组件.md
confidence: 0.9
date_updated: 2026-07-03
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

## For Agent

Prefab 文件名前缀决定 UI 的打开方式、管理层和 UILayer。做新界面命名或判断该用 Menu 还是 PopupBox 时读此页。与 [[wiki/concepts/ui-layer-stack]]、[[wiki/concepts/ui-containers]] 配合使用。

# Prefab 屏幕形态（前缀体系）

> last_verified: 2026-07-03

## 可独立打开的 UIView 形态

### Menu_ — 全屏菜单

- `panel_type`: `fullscreen_menu`
- 管理：`ShowStackUI` / `HideStackUI`，**UI 栈**（FullScreen 层）
- 骨架：`Pan_Top` / `Pan_Middle` / `Pan_Left` / `Pan_Right` / `Pan_Bottom`
- 示例：`Menu_HeroDetail`、`Menu_SilkRoadFormation`

### PopupBox_ — 标准弹窗

- `panel_type`: `popupbox`
- 管理：`ShowUI` / `HideStackUI`，PopupBox 层（9~10）
- 走弹窗冲突队列 `UIPopupboxConflictQueue`
- 示例：`PopupBox_MsgBox`、`PopupBox_CommonRule`

### PopupBox_HoverInfo_ — 悬停说明

- PopupBox 子类，轻量 Tips，跟锚点位置

### Hud_ — 常驻 HUD

- `ConstantUIManager`，`UILayerCfg.Hud`（第 3 层）
- 不走 Stack/Popup，禁止重逻辑 OnUpdate
- 示例：`Hud_Common`、`Hud_Msg`

### Float_ — 3D 悬浮

- 基类 `UIFloatBase`，`GetFloatUI` / `PutFloatUI`
- 层：Float / LowerFloat（1~2）
- 示例：`Float_EntityBubble`、`Float_MapBuildingHpBar`

## 子模板（不独立打开）

### ListItem_

- 挂在 [[wiki/entities/ListBox]] 内，对象池复用
- `UIListViewEasy` / `OnListItemShow` 驱动
- CtrlData 在 `CtrlData/Template/`

### Pan_

两种角色：
- **独立屏幕**：侧滑 `side_popup_*`、底部 `bottom_float`
- **子模板**：Menu/PopupBox 内，经 [[wiki/entities/TemplateBox]] 实例化

## 相关

- [[wiki/concepts/ui-layer-stack]]
- [[wiki/concepts/ui-containers]]
- [[wiki/sources/UI组件]]
