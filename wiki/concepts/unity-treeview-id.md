---
type: concept
topic: feature-research
sources:
  - wiki/sources/AttrViewer-TreeView-ID冲突修复.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/feature-research
---

# 概念：Unity TreeView 整数 id 选中机制

## For Agent

Unity `TreeView` 用**整数 id** 唯一标识每个节点，选中状态绑定在 id 上——界面上所有共享同一 id 的可见行会一起高亮。因此为节点分配 id 时必须保证唯一，否则会出现「选一行、同名行全亮」的联动 Bug（[[AttrViewer]] 案例）。

## 要点

- 选中/展开/滚动状态都以 id 为键（`selectedIDs`/`expandedIDs`）。
- 错误做法：用 `displayName.GetHashCode()` 生成 id → 同名同值节点 id 相同 → 选中联动。
- 正确做法：用**全局递增 id**，每次重建（`Reload`/`RefreshTable`）从固定值起重新分配，保证同一次构建内唯一。
- 副作用：id 每次重建会变，依赖 id 恢复的选中/展开状态需改用稳定的 `fullPath` 之类的键。

## 关联

- 实例修复见 [[AttrViewer-TreeView-ID冲突修复]] 与 insight [[attrviewer-upgrade]]。
