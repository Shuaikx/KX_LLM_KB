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

TemplateBox 是固定槽位容器。Lua 用 UIUtil.RegistTemplateBox。

# TemplateBox

> last_verified: 2026-07-03

| 项 | 值 |
|----|-----|
| 组件 | `UnityEngine.UI.Extensions.TemplateBox` |
| 子模板 | `Pan_*` 或 `ListItem_*` |
| Lua | `UIUtil.RegistTemplateBox` |

对比 [[wiki/entities/ListBox]]：TemplateBox 适合固定 N 格。→ [[wiki/concepts/ui-containers]]
