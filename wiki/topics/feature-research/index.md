---
type: synthesis
topic: feature-research
aliases:
  - 功能调研课题
sources: []
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/synthesis
  - topic/feature-research
---

# 功能调研 / 调试 — 课题综述

## For Agent

本课题收录工具改造与问题排查记录。当前主线是 Unity 编辑器工具 [[AttrViewer]]（属性系统查看器）的一系列升级，以及配置协议（proto/pb）解码失败的排查。核心可复用经验见各 insight 页。

## 来源

- [[AttrViewer-TreeView-ID冲突修复]] — TreeView 多选高亮 Bug 根因与修复

## 概念

[[unity-treeview-id]]

## 实体

[[AttrViewer]]

## 分析 / 调试记录（insights）

- [[attrviewer-upgrade]] — AttrViewer 三项升级（Bug 修复 + 自动刷新 + VS Code 风格 UI）
- [[skillconf-pbin-decode-fix]] — SkillConfData pbin 解码失败的 proto 修复
