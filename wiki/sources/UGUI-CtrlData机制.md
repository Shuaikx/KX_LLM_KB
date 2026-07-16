---
type: source
topic: ui-spec
sources:
  - raw/topics/ui-spec/UGUI改造与CtrlData机制详解.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/source
  - topic/ui-spec
---

# Source：AOE3D UGUI 改造与 CtrlData 机制详解

## For Agent

详解 AOE3D 如何在 Unity 原生 UGUI 上做深度改造，形成「**表现在 C#、逻辑在 Lua、用 CtrlData 表做桥**」的三层框架。是 [[ctrldata-bridge]]、[[centralized-ui-update]] 概念及 [[RootPanel]]、[[CanvasPanel]] 实体的主要来源，并补充 [[ListBox]]/[[TemplateBox]] 的 C# 侧实现。

## 关键事实

### 三层结构
1. Lua 业务逻辑层：只通过 `ctrlData.m_XXX` 访问控件。
2. CtrlData 桥接层：`RootPanel.m_CtrlList`（编辑器序列化控件清单）+ `CtrlDataBase` 元表。
3. 改造后 C# UGUI：`UIBehaviour(改) → Panel → [[RootPanel]] → [[CanvasPanel]]`，含 [[ListBox]]/[[TemplateBox]] 等约 47 个扩展控件。

### C# 侧改造（UIBehaviour `#region FOR_AOE`）
- 自维护显隐/可用状态 + 沿 `m_Parent` 逻辑父链传播（`SetVisible/SetEnable`）。
- **集中式 Update**：`SetNeedUpdate` 注册到单例 [[centralized-ui-update|UGUIUpdateMgr]]，把 N 个 `MonoBehaviour.Update` 合并成 1 个。
- 手动生命周期 `ClearListeners`（由 Lua 解绑驱动，规避销毁顺序崩溃）。
- 新手引导 `GID`/`GuideValid`。

### CtrlData 机制（核心桥）
- 编辑器导出工具把控件按序 `RegCtrl` 进 `RootPanel.m_CtrlList`（下标 0 = 根 Panel），并生成 `*_UGUI_CtrlData.lua`（仅供 EmmyLua 类型提示 + `Idx_` 下标常量，几乎不参与运行时）。
- 运行时 `UIUtil.NewCtrlData(transform)` 遍历清单，`o.Set(name, ub)` 生成 ctrlData 表并挂 `CtrlBase` 元表；名字冲突时 `UIAnim`→`_Anim`、`BackImage`→`_BkImg`。
- `CtrlBase` 元表 `__index`：`GetRootPanel`/`GetCtrl`，`Idx_xxx` 剥前缀返回下标。
- 详见 [[ctrldata-bridge]]。

### Lua 消费
- `UIWidget:BindView` → `_OnViewBinded` 构建 ctrlData → `OnCreate` 绑事件。
- `UnbindView` 置 `ctrlData=nil`（`IsNil` 判活依据）。
- `AddChildInPrefab` 多子 Widget 共享同一 ctrlData（如 `Hud_Common_Top/Bottom`）。
- 列表用 [[UIListViewEasy]] 封装，模板用 UITemplateBox。

### 其他能力
- `ScreenAdapt` 横竖屏双套布局 + `FitSafeArea` 安全区；`Smash` 运行时动态图集合批。

## 关键文件索引

- `Packages/com.unity.ugui/Runtime/EventSystem/UIBehaviour.cs`、`UI/Core/UGUIUpdateMgr.cs`、`Ext/Ext/RootPanel.cs`/`CanvasPanel.cs`/`ListBox.cs`/`TemplateBox.cs`
- `Assets/Scripts/CS/Util/UIUtil.cs`（`NewCtrlData`）、`.Lua/UI/CtrlDataBase.lua`、`UI/Core/UIWidget.lua`
