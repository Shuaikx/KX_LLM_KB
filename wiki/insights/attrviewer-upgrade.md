---
type: insight
topic: feature-research
sources:
  - raw/topics/feature-research/AttrViewer_升级总结_20260706.md
  - wiki/sources/AttrViewer-TreeView-ID冲突修复.md
  - wiki/entities/AttrViewer.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/insight
  - topic/feature-research
---

# Insight：AttrViewer 三项升级（2026-07-06）

## For Agent

[[AttrViewer]] 一次性完成三类升级：**Bug 修复**（选中联动高亮）、**自动刷新**、**VS Code 风格 UI 改造**。本页是会话式升级记录（episodic），基于 session_summary 143500/143600/143604 合并。

## 三项升级

### 1. Bug 修复：选中联动高亮
- 现象：`yzPresetTeam.presetTeamData` 多索引下同名同值字段被一起高亮。
- 根因/修复：TreeView 整数 id 唯一性问题，改全局递增 id。详见 [[unity-treeview-id]] 与 [[AttrViewer-TreeView-ID冲突修复]]。

### 2. 自动刷新
- 架构：属性推送 → DirtyTable → 顶层 dirty → `AttrViewerEditorUtil`(Lua) 回调 → C# `AttrRefreshCall` delegate → 帧级 debounce → `RefreshTable()` + `TreeView.Reload()`。
- 关键点：`rootAttr` 18 个顶层字段注册监听；listener key `{ __cname = "AttrViewerEditorUtil" }`（满足 Profiler）；缓存 delegate 避免 GC；`Launcher.OnCleanUpByCS` 清理。

### 3. VS Code 风格 UI
- 从 `GUILayout` 堆叠 → 基于 `Rect` 的精确分区；深色 `#252526`。
- 三区：**监视 / 路径 / 变量**；路径显示 `root.user.xxx.field`；分界线可拖拽；面板高度序列化持久化。
- 技术：`Styles` 静态类、`DrawSectionHeader`、`DrawHorizontalSplitter`；路径查询下沉到 `LuaTableTreeView` 子类。

## 涉及文件

- `Assets/Editor/AttrViewer/AttrViewerWindow.cs`（核心修改）
- `Assets/Scripts/.Lua/Util/AttrViewerEditorUtil.lua`（新增）
- `Assets/Scripts/.Lua/Launcher.lua`（退出清理钩子）

## 已知限制

刷新后 id 变化依赖 `fullPath` 恢复；高频推送仅帧级 debounce；`SaveTable()` 未实现。
