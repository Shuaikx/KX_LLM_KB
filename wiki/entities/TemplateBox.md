---
type: entity
topic: ui-spec
sources:
  - wiki/sources/UI组件.md
  - wiki/sources/UGUI-CtrlData机制.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/entity
  - topic/ui-spec
---

# 实体：TemplateBox

## For Agent

`UnityEngine.UI.Extensions.TemplateBox` —— **固定槽位模板容器**，适合少量固定格子（5 兵种位、3 英雄槽）。与滚动列表 [[ListBox]] 并列为两种容器范式（见 [[ui-containers]]）。Lua 侧用 UITemplateBox 封装。

## 事实

- 编辑器预置若干位置节点 `m_PosNode`，运行时按数据往槽位填模板实例。
- 关键开关 `m_ControlFromLua`（新建 `Reset()` 默认 true）：
  - `true`（推荐）：实例化/销毁由 Lua（UITemplateBox）管理，C# 侧 `Init/RefreshLayout/RefreshItem` 在 `AOE_APP` 下 return。
  - `false`：走 C# 内置对象池（`LoadTemplate`/`GetTemplate`/`NewTemplate`）。
- 事件 `onItemInit/onItemShow/onItemHide/onItemFree`。
- 新手引导：`AddItemLoadBylua`/`GetItemByGuideID` 让引导找到 Lua 动态加载的 item。
- 子模板可为 `Pan_*` 或 `ListItem_*`。

## 关联

- Lua 侧 `UIUtil.RegistTemplateBox`；C# 源码 `Ext/Ext/TemplateBox.cs`。
- 变体：`HeroCardTemplateBox` 等（见 CtrlData 文件模板依赖标注）。
