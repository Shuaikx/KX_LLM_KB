# ArmyMarch 模块重构分析报告

## 一、重构背景与原因

### 1.1 重构前的问题

重构前项目中存在两个独立的编队界面文件：

| 文件 | 行数 | 绑定 Prefab |
|------|------|-------------|
| `Menu_ArmyMarch2.lua` (大地图编队) | ~2065 行 | `Menu_ArmyMarch2_UGUI` |
| `Menu_SilkRoadFormation.lua` (丝路编队) | ~1659 行 | `Menu_SilkRoadFormation_UGUI` |

**核心痛点**：

1. **大量代码重复** -- 两个文件在生命周期管理（OnShow/OnHide/OnDestroy/CleanUp）、编队切换逻辑（SetAndRefreshIndex/RefreshData/RefreshUI）、英雄/3D阵型/羁绊/分享/打点等区块上高度重复。
2. **维护成本高** -- 修一个通用 Bug 需要在两个文件同步修改，容易遗漏。
3. **扩展困难** -- 如果未来新增第三种编队场景（如华夏对战编队），又要复制一份代码。
4. **单文件过大** -- 2000+ 行的单文件难以阅读和定位问题。

### 1.2 重构目标

- 抽出 `Menu_ArmyMarch_Base` 承载**共享逻辑**，两个子类继承之
- 保持运行时行为、UI 表现、子控件回调契约完全不变
- 差异点通过**模板方法 + 钩子**隔离，子类实现最小化
- 不改 Prefab / CtrlData / UIName 键

### 1.3 重构效果

| 文件 | 行数 | 职责 |
|------|------|------|
| `Menu_ArmyMarch_Base.lua` | ~1352 行 | 通用骨架 + 共享逻辑 |
| `Menu_ArmyMarch_Global.lua` | ~1025 行 | 大地图专属逻辑 |
| `Menu_ArmyMarch_SilkRoad.lua` | ~536 行 | 丝路专属逻辑 |

**总行数对比**：重构前 ~3724 行 -> 重构后 ~2913 行，减少约 22%，且职责边界清晰。

---

## 二、架构设计

### 2.1 继承关系

```
UIView (框架基类)
  |
  +-- Menu_ArmyMarch_Base (抽象基类，不注册 UICfg)
        |
        +-- Menu_ArmyMarch_Global (大地图编队，注册 UIName.Menu_ArmyMarch)
        |
        +-- Menu_ArmyMarch_SilkRoad (丝路编队，注册 UIName.Menu_SilkRoadFormation)
```

### 2.2 模板方法模式

Base 定义统一流程骨架，子类通过覆盖钩子注入差异：

```
InitUIWithParam(params)
    |-- ParseInitParam(params)          <-- 子类覆盖：解析入参
    |-- InitData()
    |       |-- InitDataExtra()         <-- 子类覆盖：额外数据初始化
    |-- InitUI()
    |-- AfterInitUIWithParam(params)    <-- 子类覆盖：收尾逻辑

RefreshUI(...)
    |-- RefreshUIExtraBefore(...)       <-- 子类覆盖：刷新前置逻辑
    |-- RefreshHeroPanel(...)
    |-- RefreshSoldierNumPanel(...)
    |       |-- AfterRefreshSoldierNumPanel()  <-- 子类覆盖
    |-- Refresh3DFormationUI(...)
    |-- RefreshFormationPanel(...)
    |-- RefreshArmyInfoPanel()          <-- 子类各自实现
    |-- RefreshUIExtraAfter(...)        <-- 子类覆盖：刷新后置逻辑
```

---

## 三、三文件关系详解

### 3.1 模块职责总览

```
+--------------------------------------------------+
|           Menu_ArmyMarch_Base (基类)              |
|                                                  |
|  [生命周期] OnCreate骨架 / OnDestroy / CleanUp   |
|  [初始化]   InitUIWithParam / InitData / InitUI  |
|  [通用按钮] BindCommonButtons (14个通用按钮)      |
|  [编队切换] SetAndRefreshIndex / RefreshIndex    |
|  [英雄管理] InitHeroData/UI / RefreshHeroPanel   |
|  [3D阵型]  Init3DFormation / Refresh3DFormation  |
|  [阵型面板] RefreshFormationPanel / 阵型格子渲染  |
|  [羁绊系统] GetHeroFetters / RefreshHeroFetters  |
|  [关闭动画] Close / OnClickClose                 |
|  [Fake数据] GetFakeBattleHeroId 等回调接口       |
|  [分兵基础] RefreshSoldierNum / 分兵数据计算      |
|  [分享基础] GetTeamIdByTypeAndIndex_Main         |
+--------------------------------------------------+
          |                          |
          v                          v
+------------------------+  +---------------------------+
| Menu_ArmyMarch_Global  |  | Menu_ArmyMarch_SilkRoad   |
|                        |  |                           |
| [分兵完整实现]          |  | [敌方信息面板]             |
|  - 滑条/键盘输入       |  |  - 敌方总兵力              |
|  - 一键分兵算法        |  |  - 敌方英雄头像            |
|  - 确认下发            |  | [星级条件列表]             |
|  - 城内预备兵额度      |  |  - RefreshConditionList   |
| [部队信息面板]          |  | [援军英雄]                |
|  - 士气条与惩罚        |  |  - 章节援军数据            |
|  - 动员兵              |  | [丝路专属入口]             |
|  - 练兵等级跳转        |  |  - 挑战按钮               |
| [华夏统帅附身]          |  |  - 查看敌方部队            |
|  - RefreshLeaderFollow |  |  - 配将台(阵容同步)        |
|  - 统帅下阵            |  | [入参]                    |
| [大地图专属]            |  |  - YPTT_SilkRoadArmy      |
|  - 征兵侧栏隐藏        |  |  - chapterId/levelId      |
|  - 兵种克制额外入口    |  | [分享] 已停用(no-op)       |
|  - 补给规则            |  +---------------------------+
|  - 回城自动补兵        |
| [入参]                 |
|  - YPTT_Army (默认)    |
| [分享] 完整实现         |
+------------------------+
```

### 3.2 共用模块详细列表 (Base 提供)

| 功能区块 | 主要方法 | 说明 |
|----------|----------|------|
| 生命周期 | `OnDestroy`, `CleanUp`, `OnCleanUp` | 事件反注册、音效卸载、拖拽清理 |
| 通用按钮绑定 | `BindCommonButtons()` | 分享/返回/羁绊/阵型编辑/兵种编辑/编辑英雄/换位/总览/配将台/阵型说明/克制表等 14 个按钮 |
| 初始化骨架 | `InitUIWithParam`, `InitData`, `InitUI` | 统一入参解析 -> 数据 -> UI 流程 |
| 编队切换 | `SetAndRefreshIndex`, `RefreshIndex`, `SetIndex` | 索引管理 + 数据/UI 联动刷新 |
| 侧边队伍列表 | `InitTeamListData/UI`, `RefreshTeamListUI/Data` | 委托给 `teamListCtrl` |
| 英雄管理 | `InitHeroData/UI`, `RefreshHeroPanel/Data/List` | 委托给 `heroCtrl` |
| 3D 阵型 | `Init3DFormationData/UI`, `Refresh3DFormationUI` | 委托给 `threeDFormationCtrl` |
| 阵型格子 | `RefreshAllFormationOutHH`, `RefreshFormationName` | 格子高亮 + 阵型名同步 |
| 羁绊系统 | `GetHeroFetters`, `RefreshHeroFetters`, `PlayFetterBurstEffect` | 羁绊计算 + 动效 |
| 信息弹窗 | `OnClickBtnInfo_1/2`, `OnClickBtnInfoA_2/3` | 阵型/兵种/征兵/补给规则说明 |
| 关闭流程 | `Close`, `OnClickClose` | 动画 + 事件通知 + 层级关闭 |
| Fake 数据接口 | `Get/SetFakeBattleHeroTeamId/Id`, `Get/SetFakeDismissHeroId` | 供子控件回调 |
| 分兵数据计算 | `RefreshSoldierNum`, `GetSoldierNumberInCity`, `GetMaxSoldierInCity` | 城内预备兵额度基础计算 |
| 阵型更新 | `UpdateFormation` | 修改阵型类型 + 英雄槽位映射 |
| 分享基础 | `GetTeamIdByTypeAndIndex_Main`, `RefreshShareBtn` | 分享按钮可见性 |
| 打点 | `ReportPlayerClick` | 埋点上报 |
| 屏幕旋转 | `OnOrientationChange` | 刷新 3D 阵型 |

### 3.3 Global 专有模块

| 功能区块 | 主要方法 | 说明 |
|----------|----------|------|
| 分兵完整 UI | `OnClickBtnGO`, `OnClickCloseSoldierAssign`, `RefreshSoldierAssignPanel` | 分兵面板开合 |
| 分兵滑条 | `OnSoldierAssignSliderChanged`, `OnClickBtnInputSoldierAssign`, `SetSliderValueByIndex` | 每个英雄的分兵滑条 + 数字键盘 |
| 一键分兵 | `QuickDistribute` | 均匀分配算法（按最低血量优先填充） |
| 确认分兵 | `ConfirmDistribute`, `ClickConfirm`, `OnClickBtnConfirm/Cancel` | 网络请求 + UI 动画 |
| 部队信息面板 | `RefreshArmyInfoPanel` | 部队名 + 士气条(含惩罚) + 动员兵 + 总兵力 + 练兵等级 |
| 华夏统帅 | `RefreshLeaderFollow`, `OnClickBtnDown`, `OnBtnHuaxia` | 统帅附身状态/下阵/技能界面 |
| 征兵侧栏 | `HideRecruitmentPanel` | 隐藏共享 Prefab 中的征兵面板 |
| 补给规则 | `OnClickBtnHint4` | 打开 `PopupBox_HoverInfo_ArmyMarchRule` |
| 回城自动补兵 | `OnReturnHomeRecoverHPChanged` | Toggle 监听 |
| 兵种克制额外入口 | `m_BtnSoldierType3`, `m_BtnHint_4` | 大地图额外的克制表/专精 tips 入口 |
| 属性监听 | `RegisterSceneReadyAttrs` | 城内预备兵数量变化 |
| 分享 | `OnClickBtnShare` | 完整分享逻辑（含天赋数据） |

### 3.4 SilkRoad 专有模块

| 功能区块 | 主要方法 | 说明 |
|----------|----------|------|
| 敌方信息 | `RefreshUIExtraBefore`, `_RefreshEnemyHeroAvatars` | 敌方总兵力 + 英雄头像 |
| 星级条件 | `RefreshConditionList`, `OnListConditionShow` | 三星条件列表渲染 |
| 援军英雄 | `RefreshReinforcementHeroDataList` | 章节援军数据供 heroCtrl 使用 |
| 挑战 | `OnClickBtnChallenge` | 发起丝路关卡挑战 |
| 查看敌方 | `OnClickBtnCheck` | 打开守军详情面板 |
| 配将台 | `OnClickBtnHelper` | 阵容同步弹窗（从大地图队伍复制到丝路） |
| 部队总览 | `OnClickBtnTroop` | 跳转 `Menu_HeroLineUp_Main`（丝路策略） |
| 换位 | `OnClickBtnExPosition` | 传入 levelId/isElite 参数 |
| 入参解析 | `ParseInitParam` | teamType/chapterId/isElite/levelId |
| 收尾逻辑 | `AfterInitUIWithParam` | 每次打开刷新星级条件 |
| 隐藏士气面板 | `AfterRefreshSoldierNumPanel` -> `_HideStaminaPanels` | 丝路不展示士气/动员/练兵 |
| 分享 | `OnClickBtnShare` | 已停用 (no-op，`do return end`) |
| 部队信息 | `RefreshArmyInfoPanel` | 仅部队名 + 我方总兵力（无士气/动员） |
| 数据校验 | `_ValidateSilkRoadTeamData` | 丝路编队数据合法性检查 |

---

## 四、钩子覆盖对照表

| 钩子方法 | Base 默认行为 | Global 覆盖 | SilkRoad 覆盖 |
|----------|--------------|-------------|---------------|
| `GetCurPresetTeamType()` | 返回 `teamListCtrl.presetTeamType` | 继承默认 | 返回 `self._teamType` (固定丝路) |
| `GetTeamDictForCache()` | `GetTeamDict()` | 继承默认 | `GetTeamDictByType(self._teamType)` |
| `ParseInitParam(params)` | no-op | 设置 `presetTeamType` | 设置 teamType/chapterId/isElite/levelId |
| `AfterInitUIWithParam(params)` | no-op | 不覆盖 | 刷新星级条件列表 |
| `InitDataExtra()` | no-op | 不覆盖 | 初始化敌方头像/援军数据容器 |
| `RefreshDataExtra()` | no-op | 不覆盖 | 刷新援军英雄数据 |
| `RefreshUIExtraBefore(...)` | no-op | 不覆盖 | 刷新敌方总兵力 + 头像 |
| `RefreshUIExtraAfter(...)` | no-op | 刷新华夏统帅 | 不覆盖 |
| `AfterRefreshSoldierNumPanel()` | no-op | 不覆盖 | 隐藏士气面板 |
| `RefreshArmyInfoPanel()` | (子类必须实现) | 士气+动员+练兵+部队名 | 仅部队名+总兵力 |
| `OnClickBtnShare()` | (子类实现) | 完整分享 | do return end |
| `OnClickBtnEdit()` | (子类实现) | 检查解锁+出征状态后打开 | 继承默认 |
| `OnClickBtnTroop()` | (子类实现) | 打开 PopupBox_ArmyMarchTroop | 打开 Menu_HeroLineUp_Main |
| `OnClickBtnHelper()` | (子类实现) | 打开 Menu_HeroLineUp_Main | 打开阵容同步弹窗 |
| `OnClickBtnExPosition()` | (子类实现) | 传 presetTeamType | 传 levelId/isElite |

---

## 五、子控件共享关系

两个子类在 `OnCreate` 中挂载**完全相同的四个子控件**：

```lua
self:AddChildInPrefab("Outside.UI.March.ArmyMarch.Outside.Menu_ArmyMarch_TeamList", "teamListCtrl")
self:AddChildInPrefab("Outside.UI.March.ArmyMarch.Outside.ListItem_ArmyMarchSoldierNum2", "soldierNumListCtrl")
self:AddChildInPrefab("Outside.UI.March.ArmyMarch.Outside.Menu_ArmyMarch_Hero2", "heroCtrl")
self:AddChildInPrefab("Outside.UI.March.ArmyMarch.Outside.Menu_ArmyMarch_3DFormation", "threeDFormationCtrl")
```

这些子控件通过 `self.parent` 反向调用父级方法，Base 保证了所有被回调的方法都有默认实现。

---

## 六、数据流与初始化流程

```
外部调用 ShowStackUI(UIName.Menu_ArmyMarch, params)
    |
    v
[OnCreate] (子类)
    |-- AddChildInPrefab x4 (挂载子控件)
    |-- BindCommonButtons() (Base: 14个通用按钮)
    |-- 子类专属按钮绑定
    |
    v
[InitUIWithParam(params)] (Base 骨架)
    |-- ParseInitParam(params)     --> 子类解析入参
    |-- if not hasInit:
    |       |-- InitData()         --> 动画/英雄/3D阵型/额外数据
    |       |-- InitUI(teamIndex)  --> 队伍列表/英雄/3D阵型 UI
    |       |-- hasInit = true
    |-- netCtrl = require(...)
    |-- AfterInitUIWithParam()     --> 子类收尾
    |
    v
[SetAndRefreshIndex] (编队切换触发)
    |-- RefreshData()
    |       |-- RefreshDataExtra()
    |-- RefreshUI()
            |-- RefreshUIExtraBefore()
            |-- RefreshHeroPanel()
            |-- RefreshSoldierNumPanel()
            |       |-- AfterRefreshSoldierNumPanel()
            |-- Refresh3DFormationUI()
            |-- RefreshFormationPanel()
            |-- RefreshArmyInfoPanel()  --> 子类各自实现
            |-- RefreshHeroFetters()
            |-- RefreshShareBtn()
            |-- RefreshUIExtraAfter()
```

---

## 七、设计决策总结

| 决策 | 选择 | 否决方案 | 理由 |
|------|------|----------|------|
| 代码复用方式 | 继承 | 组合/Mixin | 现有代码大量用 self/生命周期/cls.super，继承改动最小 |
| 差异隔离方式 | 模板方法 + 钩子 | if-else 分支 | 遵循开闭原则，新增场景只需新建子类 |
| Prefab 处理 | 不动 | 合并两个 Prefab | 项目约束：不触碰 Unity Prefab |
| 旧文件处理 | 保留不删 | 直接删除 | 项目红线：AI 不执行删除 |
| Base 注册 | 不注册 UICfg | 注册为抽象 UI | Base 无 Prefab，不应被直接打开 |

---

## 八、扩展性

重构后如需新增第三种编队场景（如华夏对战编队），只需：

1. 新建 `Menu_ArmyMarch_Huaxia.lua`，继承 `Menu_ArmyMarch_Base`
2. 覆盖必要钩子（`GetCurPresetTeamType`, `ParseInitParam`, `RefreshArmyInfoPanel` 等）
3. 在 `UICfg` 注册新 UIName
4. 无需修改 Base 或其他子类的任何代码

---

*参考来源: openspec/changes/archive/2026-07-23-refactor-army-march-base/*
