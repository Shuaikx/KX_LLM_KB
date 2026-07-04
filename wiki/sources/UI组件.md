---
type: source
topic: ui-spec
sources:
  - raw/topics/ui-spec/UI组件.md
confidence: 0.9
date_updated: 2026-07-03
status: current
tags:
  - wiki/source
  - topic/ui-spec
---

## For Agent

AOE3D UI 组件形式的总览 source 页。从两个维度组织知识：Prefab 前缀形态（Menu_/PopupBox_/ListItem_ 等）和 Unity 容器控件（ListBox/TemplateBox）。选 UI 形式或查层级时先读此页，再链到 [[wiki/concepts/prefab-screen-forms]] 和 [[wiki/concepts/ui-containers]]。

# UI 组件形式说明（Source 摘要）

> 原始文件：`raw/topics/ui-spec/UI组件.md`
> last_verified: 2026-07-03

## 核心结论

项目 UI 从两层理解：

1. **Prefab 屏幕/模板形态** — 由文件名前缀决定（`Menu_`、`PopupBox_`、`ListItem_`、`Pan_`、`Hud_`、`Float_`）
2. **Unity 控件/容器类型** — 节点上的组件（`ListBox`、`TemplateBox`、`Panel` 等）

## Prefab 前缀速查

| 前缀 | 用途 | 管理层/层 |
|------|------|-----------|
| `Menu_` | 全屏主功能页 | `ShowStackUI`，FullScreen 栈 |
| `PopupBox_` | 居中弹窗 | `ShowUI`，PopupBox 层 |
| `PopupBox_HoverInfo_` | 悬停 Tips | PopupBox 子类 |
| `Pan_` | 侧滑/子模板面板 | 独立或 TemplateBox 内 |
| `ListItem_` | 列表项 | ListBox 内，不独立打开 |
| `Hud_` | 常驻 HUD | ConstantUIManager |
| `Float_` | 3D 世界悬浮 | floatRoot，UIFloatBase |

详见 [[wiki/concepts/prefab-screen-forms]]。

## 容器控件速查

| 控件 | 场景 | Lua 侧 |
|------|------|--------|
| [[wiki/entities/ListBox]] | 动态长列表 | `UIListViewEasy` + `OnListItemShow` |
| [[wiki/entities/TemplateBox]] | 固定 N 个格子 | `UIUtil.RegistTemplateBox` |

详见 [[wiki/concepts/ui-containers]]。

## 层级关系

```
UIView → Menu_* / PopupBox_* / Hud_*
子模板 → ListItem_* (ListBox) / Pan_* (TemplateBox)
Float_* → UIFloatBase
```

关键 UILayer：`LowerFloat` → `Float` → `Hud` → `FullScreen` → `PopupBox` → `MessageBox` → `Tips` → `Loading`

详见 [[wiki/concepts/ui-layer-stack]]。

## 选型表（节选）

| 场景 | 推荐 |
|------|------|
| 完整功能页、压栈返回 | `Menu_` |
| 长列表一行一项 | `ListItem_` + ListBox |
| 固定 N 格（如 5 英雄位） | Pan/ListItem + TemplateBox |
| 地图跟实体气泡 | `Float_` |

## 参考来源（原文）

- openviking UI framework / knowledge_base_spec
- Controls/ListBox.md、TemplateBox.md
- Module/Hud_Common/hud_common.md
