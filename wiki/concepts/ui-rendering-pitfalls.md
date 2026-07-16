---
type: concept
topic: ui-spec
sources:
  - wiki/sources/iwiki-4013196232.md
confidence: 0.8
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

# 概念：UI 渲染 / 适配疑难排查

## For Agent

AOE3D UI 显示异常的排查心智：先看**空间与渲染链路** —— Alpha 是否被改坏？RT/Mask/Blend 顺序？是否混用了两种坐标空间？顶点数据是否没刷新？下面是几类典型问题。

## 典型问题与根因

| 问题 | 根因 | 修复 |
|------|------|------|
| 3D Canvas 透明 UI「透贴」 | 3D 与 2D Canvas 渲染顺序不同，Mask 更新了 RT 的 Alpha | 让 `Pic_Mask` 不更新 Alpha 通道（Color Mask） |
| UGUI 动态 mask FadeTexture 不显示 | 从 none 切到有 FadeTexture 时顶点里的 `fadeIndex` 未更新 → 采样 alpha=0（全透明） | 设顶点脏标记，强制刷新顶点数据 |
| 动态分辨率下属性界面被遮挡 | 混用像素空间坐标与 canvas 变化前空间坐标做边界判断 | 统一到 rect 局部空间再比较 |
| Android 多窗口/小窗 16:9 | 华为等 ROM 限制改 Window 尺寸 | 约束 `UnityPlayer` 视图尺寸做 letterbox，而非硬改 Window |

## 排查工具

- RenderDoc / frame debug / 顶点数据检查（动态 mask 案例讲得最清楚）。
- 一句话：**内容层 letterbox 比窗口层硬改尺寸更稳；先统一坐标空间再谈逻辑。**
