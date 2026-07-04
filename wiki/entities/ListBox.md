---
type: entity
topic: ui-spec
sources:
  - raw/topics/ui-spec/UI组件.md
confidence: 0.9
date_updated: 2026-07-03
status: current
tags:
  - wiki/entity
  - topic/ui-spec
---

## For Agent

ListBox 是滚动列表容器。动态数量、对象池、子模板 ListItem_。Lua 用 UIListViewEasy。

# ListBox

> last_verified: 2026-07-03

| 项 | 值 |
|----|-----|
| 组件 | `UnityEngine.UI.Extensions.ListBox` |
| 子模板 | `ListItem_*` |
| Lua | `UIListViewEasy` + `OnListItemShow` |

对比 [[wiki/entities/TemplateBox]]：ListBox 适合长列表。→ [[wiki/concepts/ui-containers]]
