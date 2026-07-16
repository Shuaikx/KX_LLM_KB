---
type: entity
topic: ui-spec
sources:
  - wiki/sources/UI组件.md
  - wiki/sources/UGUI-CtrlData机制.md
  - wiki/sources/iwiki-4013196232.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/entity
  - topic/ui-spec
---

# 实体：ListBox

## For Agent

`UnityEngine.UI.Extensions.ListBox` —— AOE3D 自研的**滚动虚拟列表容器**，动态数量 UI 的两种范式之一（另一是 [[TemplateBox]]，对比见 [[ui-containers]]）。Lua 侧不直接用，通过 [[UIListViewEasy]] 封装。

## 事实

- 继承项目自研 `ScrollPanel`，实现 `IOrientationHandling`。
- 对象池：滚动时复用不可见 item，只实例化「可视范围 + 缓冲」。
- 多模板：一个 ListBox 可注册多种 `ListItem_*`（`m_TemplateNames` + `typeArr`）。
- 流式布局 `ListBoxFlowMode`：Horizontal / Vertical / HorizontalR2L / VerticalB2T。
- 支持循环列表 `m_IsCycled`、未铺满强制居中、自动 FitSize、item 出现动画。
- 子模板 `ListItem_*` 根节点须是 `ListBoxItem`。
- CtrlData 字段类型标注为 `UnityEngine.UI.Extensions.ListBox`（见 [[listcondition-listvieweasy]]）。

## 关联

- Lua 封装 [[UIListViewEasy]]；C# 源码 `Ext/Ext/ListBox.cs`。
- 大数据量配合 ListView 分页拉取（`SplitPage*`）。
