---
type: concept
topic: ui-spec
sources:
  - wiki/sources/UI组件.md
  - wiki/sources/iwiki-4013196232.md
  - wiki/sources/UGUI-CtrlData机制.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

# 概念：UI 容器控件（动态数量 UI 的两种范式）

## For Agent

AOE3D 处理「动态数量 UI」有两种容器范式：[[ListBox]]（滚动虚拟列表，对象池）与 [[TemplateBox]]（固定槽位模板）。Lua 侧不直接操作它们，而是通过封装 [[UIListViewEasy]] / UITemplateBox 使用。选型看「不定长滚动」还是「少量固定格」。

## 两种范式对比

| 维度 | [[ListBox]] | [[TemplateBox]] |
|------|-------------|------------------|
| 场景 | 长列表、不定数量、可滚动 | 少量固定格子（5 兵种/3 英雄槽） |
| 机制 | 对象池虚拟列表、多模板、循环、流式 | 编辑器预置 `m_PosNode` 槽位填充 |
| 子模板 | `ListItem_*` | `Pan_*` / `ListItem_*` |
| Lua 封装 | [[UIListViewEasy]]（推荐）/ UIListView | UITemplateBox（`m_ControlFromLua=true`） |

## 要点

- ListBox 继承项目自研 `ScrollPanel`；列表项根节点须是 `ListBoxItem`。
- TemplateBox `m_ControlFromLua=true`（推荐）时实例化/销毁由 Lua 管理，C# 侧 `Init/RefreshLayout` 直接 return。
- 裸写方式灵活但维护成本高，简单场景优先 `UIListViewEasy`/UITemplateBox，复杂可复用用 UIListView。
- 大数据量列表配合 ListView 分页拉取（`SplitPage*`）。
- 其他逻辑分组容器：`Panel`/`CanvasPanel`（见 [[CanvasPanel]]）。
