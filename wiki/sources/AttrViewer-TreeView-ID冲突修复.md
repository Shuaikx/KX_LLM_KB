---
type: source
topic: feature-research
sources:
  - raw/topics/feature-research/AttrViewer-TreeView-ID冲突修复.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/source
  - topic/feature-research
---

# Source：AttrViewer TreeView 多选高亮 Bug 修复总结

## For Agent

记录 Unity 编辑器工具 [[AttrViewer]]（`AoE → Lua → 查看属性系统`，Alt+C）的一个 Bug 修复：显示文本相同的叶子字段被点选时会同时高亮。根因是 [[unity-treeview-id]]（TreeView 用整数 id 标识节点，同 id 共享选中）。是 [[attrviewer-upgrade]] insight 的事实底稿之一。

## 关键事实

### 问题现象
多个 table 下显示文本相同的叶子（如多个 `commanderToPresetTeamId = {number} 0`），点选一行 → 所有同文本行一起高亮。

### 根因
- Unity `TreeView` 用整数 id 唯一标识节点，界面上共享同一 id 的行一起高亮。
- 修复前叶子节点 id = `displayName.GetHashCode()`，同名同值 → 同 id。
- `LuaTableTreeViewItem` 虽传入递增 `Id`，但 `base` 仍用 `GetStableId(name, table)`，递增 id 形同虚设。

### 修复
- 全树统一用全局递增 `LuaTableTreeViewItem.Id`。
- LuaTable 节点构造函数改为 `base(id, 0, name)`；叶子节点 `Id++` 后直接作 `TreeViewItem` id。
- 删除已无用的 `GetStableId`。
- `RefreshTable()` 每次重建时 `Id` 重置为 1。

### 影响
- 正面：选中行为正确、id 策略一致。
- 可忽略：刷新后 id 会变（内部使用，对用户不可见）；因 id 重建，刷新后原选中项可能无法精确恢复（修复前即存在）。
- 不涉及：属性读取、搜索、监视、双击跳转、`LuaAttrParser` 类型映射、`SaveTable()`（仍未实现）。

## 相关代码

`Assets/Editor/AttrViewer/AttrViewerWindow.cs`：`LuaTableTreeViewItem` 构造（约 402 行）、叶子 `AddChild`（约 440–441 行）、`RefreshTable()` 重置（约 145 行）。
