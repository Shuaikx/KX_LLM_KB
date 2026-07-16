---
type: synthesis
aliases:
  - 全局目录
sources: []
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/synthesis
---

# 全局目录

## For Agent

知识库全局导航。按课题与页面类型索引所有 wiki 页，每条一行摘要。门户见 [[overview]]，会话上下文见 [[hot]]，操作日志见 [[log]]。

## 课题综述

- [[UI规范课题]] — AOE3D UI 组件形态、UGUI 改造、CtrlData、统一交互
- [[功能调研课题]] — AttrViewer 工具与配置解码等调试记录
- [[Agent工具链课题]] — Agent/Tool/Hook/MCP 概念与工具生态

## Sources（原文摘要）

- [[UI组件]] — AOE3D UI 组件形态（前缀）与容器控件总览
- [[iwiki-4013196232]] — iWiki UI 基建文档树（22+ 篇）总结
- [[UGUI-CtrlData机制]] — UGUI 改造与 CtrlData 三层结构详解
- [[AttrViewer-TreeView-ID冲突修复]] — TreeView 多选高亮 Bug 修复
- [[Agent-Tool-Hook-MCP-rtk]] — 用 rtk 案例解析 Agent/Tool/Hook/MCP

## Concepts（概念）

- [[prefab-screen-forms]] — Prefab 屏幕形态（Menu/PopupBox/Pan/ListItem/Hud/Float 前缀）
- [[ui-layer-stack]] — UI 层级栈（约 20 层 UILayer）
- [[ui-containers]] — 容器控件两范式（ListBox vs TemplateBox）
- [[ctrldata-bridge]] — CtrlData 桥（表现/逻辑分离核心）
- [[centralized-ui-update]] — 集中式 UI Update（UGUIUpdateMgr）
- [[unified-ui-interaction]] — 统一交互/弹窗/提示体系
- [[ui-rendering-pitfalls]] — UI 渲染/适配疑难排查
- [[agent-tool-hook-mcp]] — Agent·Tool·Hook·MCP 四概念
- [[unity-treeview-id]] — Unity TreeView 整数 id 选中机制

## Entities（实体）

- [[ListBox]] — 滚动虚拟列表容器
- [[TemplateBox]] — 固定槽位模板容器
- [[RootPanel]] — Prefab 根节点，CtrlData 宿主
- [[CanvasPanel]] — 界面根节点（Canvas/动画/安全区）
- [[UIListViewEasy]] — ListBox 的 Lua 封装列表控制器
- [[AttrViewer]] — Unity 属性系统查看器
- [[rtk]] — Rust Token Killer（PreToolUse Hook 压缩工具）

## Insights（分析 / 记录）

- [[attrviewer-upgrade]] — AttrViewer 三项升级（Bug 修复 + 自动刷新 + UI 改造）
- [[skillconf-pbin-decode-fix]] — SkillConfData pbin 解码失败的 proto 修复
- [[listcondition-listvieweasy]] — ListCondition 字段为何是 ListViewEasy

## 待办

- `Agents_概念.md`（空文件）暂留根目录，待补充内容后 ingest。
