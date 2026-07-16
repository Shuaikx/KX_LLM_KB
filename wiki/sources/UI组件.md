---
type: source
topic: ui-spec
sources:
  - raw/topics/ui-spec/UI组件.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/source
  - topic/ui-spec
---

# Source：AOE3D UI 组件形式说明

## For Agent

原文从两个层面讲 AOE3D 的 UI 组件：**Prefab 屏幕形态**（按文件名前缀 `Menu_`/`PopupBox_`/`Pan_`/`ListItem_`/`Hud_`/`Float_`）和 **Unity 容器控件**（[[ListBox]]、[[TemplateBox]]、Panel）。是 [[prefab-screen-forms]]、[[ui-layer-stack]]、[[ui-containers]] 三个概念的主要事实来源。

## 关键事实

### Prefab 屏幕形态（前缀）
- `Menu_`：全屏菜单，走 UI 栈（`ShowStackUI`/`HideStackUI`，FullScreen 层），骨架 `Pan_Top/Middle/Left/Right/Bottom`。
- `PopupBox_`：标准弹窗，走弹窗冲突队列 `UIPopupboxConflictQueue`，内容在 `Pan_Content`。
- `PopupBox_HoverInfo_`：PopupBox 子类，用于 Hover/Tips 轻提示。
- `Pan_`：两种角色 —— 独立屏幕（`side_popup_*`/`bottom_float`）或子模板/子面板（挂在 [[TemplateBox]] 内）。
- `ListItem_`：列表项模板，挂在 [[ListBox]]，由 `UIListViewEasy`/`OnListItemShow` 驱动，对象池复用，支持多模板。
- `Hud_`：常驻 HUD，`ConstantUIManager` 管理，层 `UILayerCfg.Hud`（第 3 层），禁止重逻辑 OnUpdate。
- `Float_`：3D 世界悬浮 UI，基类 `UIFloatBase`，`floatRoot` 管理，跟随 3D 坐标。

### 容器控件
- [[ListBox]]：滚动列表容器，动态数量、对象池、可滚动，Lua 侧用 `UIListViewEasy`。
- [[TemplateBox]]：固定槽位模板容器，适合少量固定格子，Lua 侧 `UIUtil.RegistTemplateBox`。
- `Panel`/`CanvasPanel`：逻辑分组容器，`CanvasPanel` 通常是界面根节点 `m_RootPanel`。
- 基础控件前缀：`m_Btn`/`m_Lab`/`m_Pic`/`m_Tog`/`m_Input`/`m_Slider`/`m_List`/`m_Pan`。

### 层级栈（关键层，低→高）
`LowerFloat` → `Float` → `Hud` → `FullScreen`(栈) → `PopupBox` → `MessageBox` → `Tips` → `Loading`（详见 [[ui-layer-stack]]）。

## 参考来源

- `.openviking/viking/default/resources/UI/lua_ui_framework.md`
- `.openviking/.../UI/Controls/ListBox.md`、`TemplateBox.md`
