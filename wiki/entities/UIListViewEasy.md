---
type: entity
topic: ui-spec
sources:
  - wiki/sources/UGUI-CtrlData机制.md
  - wiki/sources/iwiki-4013196232.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/entity
  - topic/ui-spec
---

# 实体：UIListViewEasy

## For Agent

`UIListViewEasy`（`UI/Core/UIListViewEasy.lua`）—— [[ListBox]] 的推荐 **Lua 封装列表控制器**。业务不直接操作 ListBox 控件，而是拿这个封装对象来注册模板、刷新数据。注释里常简写为 `ListViewEasy`（同一类，见 [[listcondition-listvieweasy]]）。

## 用法

```lua
self.listView = UIUtil.RegListViewEasy(self.ctrlData.m_ListBox, self)
self.listView:RegisterTemplateAndEvt("ListItem_Example_UGUI", nil, self.OnItemShow, nil, nil)
self.listView:SetDataAndRefresh(dataList)  -- 多模板可传 typeArr
```

## 要点

- item 有自己的 `ctrlData`（`OnItemShow(ctrlData, data, index)`）。
- Lua 层索引从 1 开始，C# 层从 0 开始。
- 增量刷新用 `RefreshShowItemByIndex(i)`，别在 OnTick 里全量 `RefreshItemAll`。
- 复杂可复用场景可用底层 `UIListView`；固定槽位用 UITemplateBox（配 [[TemplateBox]]）。

## 关联

- 底层控件 [[ListBox]]；封装原因与 CtrlData 双层对象见 [[listcondition-listvieweasy]]。
