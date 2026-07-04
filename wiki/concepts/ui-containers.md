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

ListBox 与 TemplateBox 的选型概念页。核心区别：动态长列表 vs 固定少量槽位。

# UI 容器控件选型

> last_verified: 2026-07-03

## ListBox — 滚动列表

| 维度 | 说明 |
|------|------|
| 数量 | 动态，对象池 |
| 子模板 | `ListItem_*` |
| Lua | `UIListViewEasy` + `OnListItemShow` |
| 适合 | 英雄列表、聊天行、商店商品等长列表 |

→ [[wiki/entities/ListBox]]

## TemplateBox — 固定槽位

| 维度 | 说明 |
|------|------|
| 数量 | 编辑器预置 N 个节点 |
| 子模板 | `Pan_*` 或 `ListItem_*` |
| Lua | `UIUtil.RegistTemplateBox` |
| 适合 | 5 兵种位、3 英雄槽等固定格子 |

→ [[wiki/entities/TemplateBox]]

## 选型对照

| 场景 | 选择 |
|------|------|
| 一行数据一项、可滚动很多行 | ListItem_ + **ListBox** |
| 固定 N 个格子、不滚动 | Pan/ListItem_ + **TemplateBox** |
| 复杂卡片含子列表 | `Pan_` 子模板 |

## 相关

- [[wiki/concepts/prefab-screen-forms]]
- [[wiki/sources/UI组件]]
