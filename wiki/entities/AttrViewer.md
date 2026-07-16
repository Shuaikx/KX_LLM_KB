---
type: entity
topic: feature-research
sources:
  - wiki/sources/AttrViewer-TreeView-ID冲突修复.md
  - raw/topics/feature-research/AttrViewer_升级总结_20260706.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/entity
  - topic/feature-research
---

# 实体：AttrViewer（属性系统查看器）

## For Agent

Unity Editor 中的运行时 Lua 属性调试工具。入口 `AoE → Lua → 查看属性系统`（Alt+C），Play 模式且已登录时从 `mgr.userAttr.root` 拉属性树，结合 `attr_*_proto.lua` 做类型映射。核心文件 `Assets/Editor/AttrViewer/AttrViewerWindow.cs`。升级全貌见 insight [[attrviewer-upgrade]]。

## 能力

- 属性树浏览（TreeView）、搜索、监视列表、双击跳转 VS Code。
- 自动刷新（勾选后运行时属性变化自动更新，帧级 debounce）。
- VS Code Debug 风格三区 UI：**监视 / 路径 / 变量**，可拖拽分界线，深色主题。

## 已知限制

- 刷新后 TreeView id 重置（依赖 `fullPath` 恢复展开/选中，见 [[unity-treeview-id]]）。
- `SaveTable()` 仍未实现（属性编辑需单独设计）。

## 关联

- Bug 修复来源 [[AttrViewer-TreeView-ID冲突修复]]；机制 [[unity-treeview-id]]；升级记录 [[attrviewer-upgrade]]。
- Lua 桥接 `Assets/Scripts/.Lua/Util/AttrViewerEditorUtil.lua`。
