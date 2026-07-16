---
type: concept
topic: ui-spec
sources:
  - wiki/sources/UGUI-CtrlData机制.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

# 概念：CtrlData 桥（表现/逻辑分离的核心）

## For Agent

CtrlData 是 AOE3D UI 体系的灵魂：让 Lua 逻辑用 `self.ctrlData.m_BtnClose` 名字直取控件，而不写脆弱的 `transform:Find(...)` 路径。它把「编辑器摆好的 Prefab 控件清单」在运行时翻译成一张 Lua 表。宿主实体是 [[RootPanel]]/[[CanvasPanel]]。

## 数据流（三步）

1. **编辑器导出**：导出工具遍历 Prefab，把控件按序 `RegCtrl` 进 `RootPanel.m_CtrlList`（下标 0 = 根 Panel），并生成 `*_UGUI_CtrlData.lua`（仅供 EmmyLua 类型提示 + `Idx_` 下标常量，几乎不参与运行时）。
2. **运行时构建**：`UIUtil.NewCtrlData(transform)` 遍历清单，`o.Set(name, ub)` 把每个控件按名字 rawset 进表，挂 `CtrlBase` 元表，`m_Ctrls` 存 `{下标↔名字}` 双向索引。
3. **消费**：`UIWidget:BindView` → `_OnViewBinded` 构建 ctrlData → 业务在 `OnCreate` 里 `ctrlData.m_XXX` 绑事件/填数据。

## 关键点

- `CtrlBase` 元表 `__index`：命中 rawset 值走 O(1)；`GetRootPanel`/`GetCtrl` 走函数；`Idx_xxx` 剥前缀返回下标。
- 名字冲突：`UIAnim`→`_Anim`，`BackImage`→`_BkImg`。
- 判活：`UnbindView` 置 `ctrlData=nil`，`IsNil()` 据此判断界面已销毁（协程回调前需 `if not self.ctrlData then return end`）。
- 同 Prefab 拆分：`AddChildInPrefab` 让多个子 Widget 共享同一 ctrlData（如 `Hud_Common_Top/Bottom`）。

## 关联

- 表现层控件的集中更新见 [[centralized-ui-update]]；容器封装见 [[ui-containers]]、[[UIListViewEasy]]。
