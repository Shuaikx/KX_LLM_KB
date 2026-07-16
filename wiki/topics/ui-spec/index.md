---
type: synthesis
topic: ui-spec
aliases:
  - UI规范课题
sources: []
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/synthesis
  - topic/ui-spec
---

# UI 规范 — 课题综述

## For Agent

本课题汇总 AOE3D 项目的 UI 体系知识：从**表现层的 Prefab 屏幕形态**（[[prefab-screen-forms]]）、**层级栈**（[[ui-layer-stack]]）、**容器控件**（[[ui-containers]]），到**表现/逻辑分离的 CtrlData 桥**（[[ctrldata-bridge]]）、**集中式更新**（[[centralized-ui-update]]），再到**统一交互规范**（[[unified-ui-interaction]]）与**渲染适配疑难**（[[ui-rendering-pitfalls]]）。核心心智：能统一就走通用组件、能数据驱动就不硬编码、重活下沉到 C#。

## 来源

- [[UI组件]] — UI 组件形态与容器控件总览
- [[iwiki-4013196232]] — AOE3D UI 基建文档树总结（22+ 篇）
- [[UGUI-CtrlData机制]] — UGUI 改造与 CtrlData 三层结构详解
- [[listcondition-listvieweasy]] — ListCondition 字段类型答疑（insight）

## 概念

[[prefab-screen-forms]] · [[ui-layer-stack]] · [[ui-containers]] · [[ctrldata-bridge]] · [[centralized-ui-update]] · [[unified-ui-interaction]] · [[ui-rendering-pitfalls]]

## 实体

[[ListBox]] · [[TemplateBox]] · [[RootPanel]] · [[CanvasPanel]] · [[UIListViewEasy]]
