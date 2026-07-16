---
type: insight
topic: ui-spec
sources:
  - raw/topics/ui-spec/session_summary_20260703_ListCondition.md
  - wiki/entities/UIListViewEasy.md
  - wiki/entities/ListBox.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/insight
  - topic/ui-spec
---

# Insight：ListCondition 为何标注为 ListViewEasy（2026-07-03）

## For Agent

答疑 `Menu_SilkRoadFormation.lua` 里 `---@field ListCondition ListViewEasy` 的类型命名困惑。结论：**`ListCondition` 存的是 `UIUtil.RegListViewEasy` 返回的 Lua 列表控制器（运行时类 [[UIListViewEasy]]）**；注释里的 `ListViewEasy` 是丝路模块内的简写；而 `ctrlData.m_ListCondition` 才是 [[ListBox]]。体现了 CtrlData 双层对象心智（见 [[ctrldata-bridge]]）。

## 双层对象

| 字段 | 前缀 | 类型 | 角色 |
|------|------|------|------|
| `ctrlData.m_ListCondition` | `m_` | `UnityEngine.UI.Extensions.ListBox` | Unity 控件 |
| `self.ListCondition` | 无 | [[UIListViewEasy]]（简写 `ListViewEasy`） | Lua 业务封装控制器 |

## 可复用经验

- 读 `---@field xxx ListViewEasy` 时，先查同文件是否有 `UIUtil.RegListViewEasy` 赋值，真实类型即 [[UIListViewEasy]]。
- CtrlData 字段（`m_` 前缀）通常是 Unity 控件；View 字段（无 `m_`）通常是 Lua 封装实例。
- SilkRoad 模块多用简写 `ListViewEasy`，其他模块用完整名 `UIListViewEasy`，二者等价；工程内无 `@alias`，严格补全建议统一为 `UIListViewEasy`。
- 标准列表模式：`self.xxxList = UIUtil.RegListViewEasy(ctrlData.m_ListXxx, self)` → `RegisterTemplateAndEvt` → `SetDataAndRefresh`。
