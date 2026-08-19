# AOE3D 项目 UGUI 改造与 CtrlData 机制详解

> 本文说明 AOE3D 项目如何在 Unity 原生 UGUI（`com.unity.ugui` 包）基础上做深度改造，
> 新增了哪些能力，以及整套 UI 系统（含 CtrlData、Lua 桥接、容器组件、事件系统）是如何协同工作的。
>
> 参考来源：
> - `Packages/com.unity.ugui/Runtime/Ext/`（C# 改造源码）
> - `Assets/Scripts/CS/Util/UIUtil.cs`（CtrlData 构建）
> - `Assets/Scripts/.Lua/UI/Core/UIWidget.lua`、`UI/CtrlDataBase.lua`（Lua 桥接）
> - `.openviking/viking/default/resources/UI/`（知识库）

---

## 一、总体设计：三层结构

项目把「UI 表现层」和「UI 逻辑层」彻底分离，中间用一张自动生成的 **CtrlData 表** 作桥梁。整个系统可以分成三层：

```
+---------------------------------------------------------------+
|  第三层：Lua 业务逻辑层                                        |
|  UIManager / UIWidget / UIView / 各业务界面(Menu_/PopupBox_..) |
|  只通过 ctrlData.m_XXX 访问控件，不关心 Prefab 结构           |
+-----------------------------+---------------------------------+
                              |  CtrlData（桥）
                              |  UIUtil.NewCtrlData() 运行时构建
                              |  *_UGUI_CtrlData.lua 静态类型/常量
+-----------------------------v---------------------------------+
|  第二层：CtrlData 桥接层                                       |
|  RootPanel.m_CtrlList（编辑器序列化的控件列表）               |
|  CtrlDataBase 元表（__index 拦截 GetRootPanel/GetCtrl/Idx_）  |
+-----------------------------+---------------------------------+
                              |
+-----------------------------v---------------------------------+
|  第一层：改造后的 C# UGUI（UnityEngine.UI.Extensions）        |
|  UIBehaviour(改) -> Panel -> RootPanel -> CanvasPanel         |
|  ListBox / TemplateBox / ScrollPanel / RichText / ImageEx ... |
|  集中式 Update(UGUIUpdateMgr) / 屏幕适配 / 引导 / 图集合批    |
+---------------------------------------------------------------+
```

核心思想：**编辑器里美术摆好的 Prefab，被导出为一份「控件清单」序列化进 RootPanel；运行时 Lua 侧一次性把这份清单翻译成一张名字到控件的 Lua 表（ctrlData），逻辑代码就用 `self.ctrlData.m_BtnClose` 这样的名字直接拿到控件。**

---

## 二、第一层：C# 侧对原生 UGUI 的改造

Unity 官方 UGUI 源码被 fork 进 `Packages/com.unity.ugui/`，主要改造集中在 `Runtime/Ext/`（扩展）和对 `Runtime/EventSystem/UIBehaviour.cs`、`Runtime/UI/Core/` 的直接修改。

### 2.1 改造基类 `UIBehaviour`

原生 `UIBehaviour` 只是「有生命周期回调的 MonoBehaviour」。项目在其中加入了一大块 `#region FOR_AOE`，把它变成了整个扩展 UI 体系的根基类。新增能力：

**(1) 自维护的显隐 / 可用状态与层级传播**

```csharp
[SerializeField] protected bool m_VisibleSelf = true;   // 自身是否可见
[SerializeField] protected bool m_EnableSelf  = true;   // 自身是否可交互
[SerializeField] protected UIBehaviour m_Parent;        // 逻辑父节点（非 Transform 父）
protected bool m_VisibleInHierarchy = true;             // 结合父链后的可见性
protected bool m_EnableInHierarchy  = true;
```

- `SetVisible(bool)` / `SetSelfVisible` / `UpdateVisible`：可见性可沿 `m_Parent` 链传播（`visibleInHierarchy = m_VisibleSelf && parent.visibleInHierarchy`）。
- `SetEnable(bool, ignoreClickEvent)` / `UpdateEnable`：可用性（可交互）同理递归传播，内部走 `UGuiUtil.SetEnableRecursively`。
- `AddChild / DelChild / SetParent / GetParent`：维护逻辑父子关系，供 Lua 层挂载子 Widget 使用。

这套「逻辑父子 + 层级状态」是原生 UGUI 没有的，它让 Lua 层可以脱离 Unity Transform 层级单独管理 UI 组合关系。

**(2) 集中式 Update 分发（性能关键改造）**

原生 UGUI 每个组件想要 `Update` 就得自己实现 MonoBehaviour 的 `Update()`，成百上千个 UI 组件各自被 Unity 逐个回调，开销巨大且不可控。项目改为**集中注册**：

```csharp
protected void RegisterUpdate() {
    UGUIUpdateMgr.Instance.OnUpdate -= OnUpdate;
    UGUIUpdateMgr.Instance.OnUpdate += OnUpdate;   // 挂到单一分发器
}
```

- 组件调用 `SetNeedUpdate(true)` / `SetNeedLateUpdate(true)` 才会被注册。
- `UGUIUpdateMgr`（`Runtime/UI/Core/UGUIUpdateMgr.cs`）是一个 `DontDestroyOnLoad` 单例，只有它自己有真正的 `Update()` / `LateUpdate()`，然后统一 `OnUpdate?.Invoke()` 广播给已注册组件。
- 还带了全局开关（`s_ButtonUpdateRegister`、`s_EffectUpdateRegister`、`s_TextLateUpdateRegister` 等）和 Profiler 采样点（CpuSample 21/22），可整类关闭以降端机开销。

这是相较原生 UGUI 最重要的性能改造之一：**把 N 个 MonoBehaviour.Update 合并成 1 个**。

**(3) 手动生命周期（ClearListeners）**

```csharp
protected override void OnDestroy() {
    // 下面函数应该由 Lua 层来调用
    // 否则由于调用顺序和环境的原因，可能在 Editor 下造成 Crash
    // ClearListeners();
}
public virtual void ClearListeners() { }
```

事件的清理不放在 Unity 的 `OnDestroy`，而是由 Lua 框架在解绑（`UnbindView`）时显式驱动，规避了销毁顺序不确定导致的崩溃。

**(4) 新手引导（Guide）支持**

新增 `GID`（引导逻辑 ID）、`GuideEnable()`、`GuideValid(checkFlag, checkAlpha, checkScale)` 以及一组 `UIBehaviourGuideValidCheckMask_*`（检测 GameObject 激活、targetGraphic、interactable、alpha、scale、mask 等）。引导系统可以据此判断某个控件此刻是否「可被引导手指点亮」。

### 2.2 面板继承体系：Panel -> RootPanel -> CanvasPanel

```
UIBehaviour(改造版)
  +-- Panel            近乎空壳，仅作类型分层
       +-- RootPanel   每个 Prefab 根节点，持有控件清单 + CtrlData 引用
            +-- CanvasPanel   界面根节点，管 Canvas/GraphicRaycaster/动画/安全区
```

**RootPanel（`Ext/RootPanel.cs`）—— CtrlData 的宿主**

关键字段与方法：

```csharp
[SerializeField] protected List<UIBehaviour> m_CtrlList;  // 编辑器导出的控件清单（核心）
protected object m_CtrlData;                              // 运行时挂载的 Lua ctrlData 表
protected string m_VerCode;                               // 版本校验码
protected uGUIEvent m_OnShow, m_OnHide;                   // 显隐事件

public UIBehaviour GetCtrl(int index);      // 按下标取控件
public int GetCtrlCount();
public List<UIBehaviour> GetCtrlList();
public void SetCtrlData(object data);       // Lua 侧把 ctrlData 存回来
public object GetCtrlData();
public virtual void OnShow()/OnHide();      // 触发 onShow/onHide 事件
```

- `m_CtrlList` 是编辑器导出流程写进去的**有序控件数组**（下标 0 通常是根 Panel 本身）。这是「一个 Prefab 有哪些需要暴露给 Lua 的控件」的唯一真源。
- `SetVisible` 在这里被重写为**基于 CanvasGroup** 的显隐（`alpha/blocksRaycasts/interactable`），并额外提供 `SetVisibleUseAlpha`（只改 alpha 不 SetActive，适合频繁显隐）。
- `GuideID`、`m_AnimOffset/enterAnimFlag/exitAnimFlag`（入退场动画偏移）、`m_LayoutResolution`（该 Layout 的设计分辨率）都在此。

**CanvasPanel（`Ext/CanvasPanel.cs`）—— 界面根节点**

对应 Lua 层每个界面的 `m_RootPanel`。相比 RootPanel 增加：

- `m_AddCanvas` / `m_AddGraphicRaycast`：运行时按需 `AddComponent<Canvas>` / `<GraphicRaycaster>`，并追加 `TexCoord1/TexCoord2` shader 通道（供特效/合批用）。
- `m_AffectBySafeArea` + `FitSafeArea()`：刘海屏/异形屏安全区适配（读 `UGuiUtil.GetSafeAreaTBLR()` 调 `offsetMin/offsetMax`）。
- `m_CanvasPanelAnimation`（`AnimationForCanvasPanel`）：界面打开/关闭动画。`SetVisible` 被重写为「可见时先 SetVisible 再播入场动画」，并提供 `PlayHideAnimation / RewindShowAnimation / ReplayShowAnimation / GetHideAnimationFrameLength` 等给 Lua 的 UIManager 精确控制动画时长与栈切换时的动画进度保存/恢复。
- `onGetFocus` 焦点事件、显隐音效（`m_ShowAudioEvt/m_HideAudioEvt`）。
- `SetAlpha` 重写：alpha < 0.5 时自动关掉 Raycaster（透明时不吃点击）。

### 2.3 主要扩展控件一览

除了上面继承链，`Ext/Ext/` 下还有约 47 个扩展组件（知识库 `UI/Controls/README.md` 有完整 Wiki）。改造/新增的重点：

| 组件 | 基类 | 改造/新增点 |
|------|------|-------------|
| `ListBox` | `ScrollPanel` | 对象池虚拟列表、多模板、循环列表、四种流式方向、自动 FitSize（见 2.4） |
| `TemplateBox` | `UIBehaviour` | 固定槽位模板容器，`m_ControlFromLua` 决定由 Lua 还是 C# 管理（见 2.4） |
| `ScrollPanel` | `UIBehaviour` | 项目自研滚动容器（非原生 ScrollRect），未铺满时可禁止滚动等 |
| `ScrollRectEx` | `ScrollRect` | 原生 ScrollRect 的增强版 |
| `SimpleBox` | - | 轻量模板容器 |
| `RichText` | - | 图文混排、超链接 `<a href>`、内嵌 RichItem_Image/Button/Coordinate |
| `ImageEx` | `Image` | 支持远程 URL 异步加载（`m_Url` + `RemoteImageInfo` 引用计数） |
| `ImageArray` | - | 一个组件管理多张 Sprite，按 index 切换 |
| `BackImage` | `RawImage` | 背景模糊（blurIter/blurSpread），常用于弹窗背板 |
| `EmptyGraphic` | - | 不绘制但可接收射线的透明图形（做点击区/遮罩） |
| `UIEffect` | - | UI 内粒子/序列帧特效，`layerMask` 全局控制 |
| `UIAnim` / `UISampledAnimation` | - | UI 动画播放/采样动画 |
| `Swiper` / `SwiperDynamic` | - | 轮播翻页（SwiperDynamic 基于 ListBox） |
| `ChartBox` / `LineChart` | - | 图表可视化 |
| `ScreenAdapt/*Adapt` | `AdaptBase` | 横竖屏双套布局适配体系（见 2.5） |

### 2.4 容器组件：ListBox 与 TemplateBox

这是「动态数量 UI」的两种范式，也是改造的重头戏。

**ListBox（滚动虚拟列表）**

- 继承自项目自研 `ScrollPanel`，实现 `IOrientationHandling`。
- 对象池：滚动时复用不可见 item，只实例化「可视范围 + 少量缓冲」的 item。
- 多模板：一个 ListBox 可注册多种 `ListItem_*` 模板（`m_TemplateNames`），配合 `typeArr` 决定每行用哪个模板。
- 流式布局 `ListBoxFlowMode`：Horizontal / Vertical / HorizontalR2L / VerticalB2T。
- 循环列表 `m_IsCycled`、未铺满强制居中 `ListBoxForceMiddleType`、水平/垂直充满、自动 FitSize、item 出现动画等。
- Lua 侧**不直接用**，而是通过 `UIListViewEasy`（推荐）或 `UIListView` 封装（见第五节）。列表项根节点必须是 `ListBoxItem`。

**TemplateBox（固定槽位模板容器）**

- 编辑器预置若干位置节点 `m_PosNode`，运行时按数据往这些槽位填模板实例。适合「少量固定格子」（如 5 个兵种位、3 个英雄槽）。
- 关键开关 `m_ControlFromLua`（默认新建时 `Reset()` 里置 true）：
  - `true`：模板的实例化/销毁全部由 **Lua 层**（`UITemplateBox`）管理，C# 侧的 `Init/RefreshLayout/RefreshItem` 在 `AOE_APP` 下直接 return。这是当前推荐模式。
  - `false`：走 C# 内置对象池逻辑（`LoadTemplate` 用 `UGuiResMgr.LoadPrefabAndLoader`，`GetTemplate/NewTemplate` 复用/新建）。
- 事件 `onItemInit/onItemShow/onItemHide/onItemFree` 供逻辑填充。
- 为新手引导额外提供 `AddItemLoadBylua/RemoveItemLoadBylua/GetItemByGuideID`，让引导能找到 Lua 动态加载的 item。

### 2.5 屏幕适配（ScreenAdapt 体系）

`Ext/Ext/ScreenAdapt/` 是一套「同一 Prefab 支持横竖屏两套布局参数」的适配框架：

- `IAdaptBase` / `AdaptBase<TConfig, TComp>` / `AdaptBehaviour` / `AdaptManager`（`AdaptManager.IsLandscape()`）。
- 几乎每种控件都有对应 Adapt：`RectTransformAdapt`、`TextAdapt`、`ImageArrayAdapt`、`ListBoxAdapt`、`GridLayoutGroupAdapt`、`BackImageAdapt`、`AnimationAdapt`、`CameraAdapt`、`AnchoredToScreenEdgeAdapt`……
- 屏幕旋转时，`UIUtil.ForcePanelOrientation` 会遍历 `GetComponentsInChildren<IAdaptBase>()` 逐个 `ApplyConfig(isLandscape)`；Lua 侧 `UIWidget:OnOrientationChange` 也会触发 `CanvasPanel.FitSafeArea()`。

### 2.6 图集合批（Smash）

`Ext/Smash/`（`UISmashMgr` / `UISmashLua` / `UISmashAtlas`）是运行时动态图集/合批系统（`SmashRenderMode.Job_Collect_NextSync`，分块 tile 32），用于把大量小图 UI 合批渲染，降低 DrawCall。

---

## 三、第二层：CtrlData 机制（核心桥）

CtrlData 是整个体系的灵魂。它让 Lua 逻辑用 `self.ctrlData.m_BtnClose` 就能拿到 Prefab 里那个按钮，而完全不用写 `transform:Find("Pan_Top/Btn/BtnClose")` 这种脆弱路径。

### 3.1 数据从哪来：编辑器导出 `m_CtrlList`

美术在 Unity 编辑器里摆好 Prefab 后，导出工具（`Assets/Editor/City/CreatScriptModel.cs` + `UIEventInjectTools.cs` 等）会做两件事：

1. 遍历 Prefab，把需要暴露的控件按顺序 `RegCtrl(ui)` 进 `RootPanel.m_CtrlList`（并写入版本码 `m_VerCode`）。下标 0 固定是根 Panel 自身。
2. 生成一份 `*_UGUI_CtrlData.lua` 静态描述文件，并把 UI 注册进 `UICfg.lua` / `UIName.lua`（`Path`、`Prefab`、`Flag`、`Layer`）。

生成的 CtrlData 文件长这样（截取 `ListItem_ArmyMarchSoldierNum2_UGUI_CtrlData.lua`）：

```lua
-- Short Name CtrlData
-- This file is generated automatically by Unity Editor.
-- No manual modification is permitted.
---@class ListItem_ArmyMarchSoldierNum2_UGUI_CtrlData
---@field m_RootPanel UnityEngine.UI.Extensions.ListBoxItem
---@field m_BtnDele UnityEngine.UI.Button
---@field m_LabNum_1 UnityEngine.UI.Text
---@field m_PanHead UnityEngine.UI.Extensions.HeroCardTemplateBox 子控件模板: Pan_HeroQuadCard_CustomSize_UGUI_CtrlData
---@field m_ListTypeTag UnityEngine.UI.Extensions.ListBox 子控件模板列表: ListItem_HeroTalent_TypeTagSmall_UGUI_CtrlData
local cls = {
--Idx_m_RootPanel = 0,
--Idx_m_BtnDele = 3,
  Idx_m_eff_flash_light = 21,   -- 只有需要用下标的才取消注释
  ...
}
function cls:GetRootPanel() return self.m_RootPanel end
function cls:GetCtrl(index) return self.m_Ctrls[index] end
return cls
```

**注意：这个 `.lua` 文件本身几乎不参与运行时逻辑**——它主要是给 IDE 做 EmmyLua 类型提示（`---@field m_XXX 类型`）和给需要按下标访问的场景提供 `Idx_` 常量。运行时真正的 ctrlData 表是下面动态构建的。文件头部还标注了模板依赖关系（`子控件模板: xxx` / `子控件模板列表: xxx`），说明哪些控件是 TemplateBox/ListBox 及其对应的子模板。

### 3.2 运行时如何构建：`UIUtil.NewCtrlData`

`Assets/Scripts/CS/Util/UIUtil.cs` 的 `NewCtrlData(Transform)` 是构建的核心（C# 侧直接操作 Lua 表）：

```csharp
public static LuaTable NewCtrlData(Transform rootPanel) {
    LuaTable ctrlBase = GameCore.LuaMgr.GetByPath<LuaTable>("CtrlBase"); // 全局元表
    LuaTable o     = env.NewTable();     // 最终的 ctrlData
    LuaTable ctrls = env.NewTable();     // 下标<->名字 双向索引
    RootPanel panel = rootPanel.GetComponent<RootPanel>();

    o.SetMetaTable(ctrlBase);            // 挂 CtrlBase 元表
    o.Set("transform", rootPanel);
    o.Set("m_RootPanel", panel.GetCtrl(0));   // 下标 0 = 根 Panel
    o.Set("m_Ctrls", ctrls);
    panel.SetCtrlData(o);                // 反向存回 C# RootPanel

    int count = panel.GetCtrlCount();
    var ctrlList = panel.GetCtrlList();
    for (int i = 1; i < count; i++) {    // 从 1 开始，逐个控件
        UIBehaviour ub = ctrlList[i];
        string ctrlName = ub.name;
        if (ub is UIAnim)  ctrlName += "_Anim";                         // 名字冲突处理
        else if (ub is BackImage && o.ContainsKey(ctrlName)) ctrlName += "_BkImg";
        o.Set(ctrlName, ub);             // 关键：ctrlData.m_BtnClose = 那个 Button
        ctrls.Set(i, ub);                // ctrls[3]        = ub
        ctrls.Set(ctrlName, i);          // ctrls["m_Btn.."] = 3
    }
    return o;
}
```

产出的 `ctrlData` 表结构：

- `ctrlData.transform`：根 Transform。
- `ctrlData.m_RootPanel`：根 CanvasPanel/RootPanel（`m_CtrlList[0]`）。
- `ctrlData.m_BtnClose`、`ctrlData.m_LabTitle` …：**每个控件按名字直接可取**（rawset 在表上）。
- `ctrlData.m_Ctrls`：`{ [下标]=控件, [名字]=下标 }` 双向索引。
- 冲突处理：`UIAnim` 追加 `_Anim`；`BackImage` 与同名控件冲突时追加 `_BkImg`（对应 UI 组件说明文档里的 `m_XXX_Anim`、`m_XXX_BkImg`）。

### 3.3 CtrlBase 元表：`UI/CtrlDataBase.lua`

`Mgr.lua` 里 `CtrlBase = require "UI.CtrlDataBase"`。它只是一个 `__index` 元表：

```lua
local function GetRootPanel(t)    return t.m_RootPanel end
local function GetCtrl(t, index)  return t.m_Ctrls[index] end
local function index(t, k)
    if k == "GetRootPanel"      then return GetRootPanel
    elseif k == "GetCtrl"       then return GetCtrl
    elseif k == nil             then return nil
    else return t.m_Ctrls[k:sub(5, -1)] end   -- 剥掉 "Idx_" 前缀，返回下标
end
return { __index = index }
```

工作方式：

- 直接访问 `ctrlData.m_BtnClose` —— 命中表上 rawset 的值（3.2 里 `o.Set` 存的），**不走元表**，O(1)。
- 访问 `ctrlData:GetRootPanel()` / `ctrlData:GetCtrl(i)` —— 表上没有，走 `__index` 返回对应函数。
- 访问 `ctrlData.Idx_m_BtnClose` —— 表上没有，走 `__index` 的 else 分支：`k:sub(5,-1)` 把 `"Idx_m_BtnClose"` 去掉前 4 个字符变成 `"m_BtnClose"`，返回 `m_Ctrls["m_BtnClose"]`（即该控件下标）。这就对应了 CtrlData 文件里那些 `Idx_` 常量。

一句话：**CtrlBase 元表统一了所有界面的 ctrlData 行为，per-UI 的 CtrlData 文件只负责静态类型 + 下标常量。**

---

## 四、第三层：Lua 框架如何消费 CtrlData

### 4.1 绑定流程 `UIWidget:BindView`

`Assets/Scripts/.Lua/UI/Core/UIWidget.lua` 是所有 UI 的基类（`UIView` 继承它）。界面创建时：

```lua
function cls:BindView(view)
    self.view = view
    self:_OnViewBinded()          -- 构建 ctrlData
    FuncUtil.PCall(self.OnCreate, self)   -- 业务在 OnCreate 里绑事件/初始化
    self.isBinded = true
end

local NewCtrlData = CS.UIUtil.NewCtrlData
function cls:_OnViewBinded()
    self.gameObject = self.view.gameObject
    if self.prefabName ~= nil then
        self.ctrlData = NewCtrlData(self.view.transform)      -- 见 3.2
        local rootPanel = self.ctrlData:GetRootPanel()
        -- 把 C# 的 onShow/onHide 事件接到 Lua 的 Show/Hide
        UIUtil.AddUIEventListener(rootPanel, "onShow", self, cls.Show)
        UIUtil.AddUIEventListener(rootPanel, "onHide", self, cls.Hide)
    end
end
```

于是业务界面里就能直接写：

```lua
function cls:OnCreate()
    UIUtil.AddBtnListener(self.ctrlData.m_BtnClose, self, self.OnClickClose)
    self.ctrlData.m_LabTitle.text = mgr.text:GetText("Title_Key")
end
```

### 4.2 解绑与清理 `UnbindView / _OnViewUnbinded`

界面关闭时反向清理，避免内存泄漏：

```lua
function cls:_OnViewUnbinded(view)
    mgr.ui:ClearUIEventByObj(self)  -- 清所有事件监听
    self.gameObject = nil
    self.ctrlData   = nil           -- 断引用，ctrlData 可被 GC
    ObjUtil.UnregistAll(view)
    ObjUtil.UnregistAll(self)
    mgr.attr:UnRegisterAll(self)
end
```

`cls:IsNil()` 就是靠 `self.ctrlData == nil` 判断界面是否已销毁——协程/闭包回调里访问 UI 前需要 `if not self.ctrlData then return end`（这也是 lua 规范里反复强调的判活）。

### 4.3 子 Widget 挂载：AddChild / AddChildInPrefab

- `AddChild(child, {mountView=...})`：把一个独立实例化的子 Widget 挂到某个挂载点。若 `mountView` 是 `UIBehaviour` 走 `mountView:AddChild`；若挂到 `TemplateBox` 且 `IsControlFromLua`，还会 `AddItemLoadBylua`（供引导查找）。
- `AddChildInPrefab(ctrlCode, name)`：同一个 Prefab 内拆分逻辑用——多个子 Widget **共享同一份 `ctrlData`**（`childWidget.ctrlData = self.ctrlData`），各自负责一块区域（如 `Hud_Common` 拆成 `Hud_Common_Top`/`Bottom`）。

### 4.4 显隐传播 Show/Hide

`Show/Hide` 会递归通知所有子 Widget，并触发业务的 `OnShow(isCreating)/OnHide(isDestroying)`，同时做打点上报（`WindowReportID`）。C# 的 `CanvasPanel.OnShow/OnHide` 事件 -> Lua `Show/Hide` 的桥接在 4.1 里已建立。

---

## 五、事件系统与列表封装

### 5.1 自动生命周期的事件绑定（UIUtil / UIEventUtil）

项目统一用 `UIUtil.AddXXXListener` 绑事件（源码 `Assets/Scripts/.Lua/Util/UIEventUtil.lua`），特点是**只在 OnCreate 绑一次、界面销毁自动清理**，三层保障：

1. UI-Obj 映射表：`mgr.ui:RegisterObjEventUIMap(obj, ui)`，界面销毁时反查清理。
2. 协程绑定：回调被包在 `mgr.co:StartBinded(obj, ...)` 里，界面销毁协程自动停——所以 **UI 事件回调里可以直接用 A2S**（`mgr.net:RequestA2S`）而不必再 `AsyncBinded`。
3. C# 侧 `ClearListeners` 由解绑流程驱动。

常用接口：

| 控件 | 绑定方法 |
|------|----------|
| Button | `UIUtil.AddBtnListener(btn, self, func)` |
| Toggle | `UIUtil.AddToggleChangedListener(tog, self, func)` |
| Slider | `UIUtil.AddSliderValueChangedListener(slider, self, func)` |
| InputField | `UIUtil.AddEditChangedListener(input, self, func)` |
| ScrollPanel | `UIUtil.AddScrollValueChange(scroll, self, func)` |
| Swiper | `UIUtil.AddSwiperPageChangedListener(swiper, self, func)` |
| RichText 超链接 | `UIUtil.AddHyperlinkTextListener(rich, self, func)` |

清理：`UIUtil.ClearBtnListener(btn)`、`UIUtil.ClearBoxItemListener(listBox)`。

### 5.2 列表封装 UIListViewEasy（推荐）

ListBox 不直接用，`UIUtil.RegListViewEasy(ctrlData.m_ListBox, self)` 拿到封装对象：

```lua
function cls:OnCreate()
    self.listView = UIUtil.RegListViewEasy(self.ctrlData.m_ListBox, self)
    self.listView:RegisterTemplateAndEvt("ListItem_Example_UGUI",
        nil, self.OnItemShow, nil, nil)   -- init/show/hide/free 回调
end
function cls:OnItemShow(ctrlData, data, index)   -- 注意：item 也有自己的 ctrlData
    ctrlData.m_LabName.text = data.name
    UIUtil.AddBtnListener(ctrlData.m_BtnSelect, self, function() self:OnItemClick(data, index) end)
end
function cls:UpdateList(dataList)
    self.listView:SetDataAndRefresh(dataList)     -- 多模板可传 typeArr
end
```

要点：Lua 层索引从 1 开始，C# 层从 0 开始；增量刷新用 `RefreshShowItemByIndex(i)`，别在 OnTick 里全量 `RefreshItemAll`。

### 5.3 模板容器封装 UITemplateBox

固定槽位用 `UITemplateBox`（`m_ControlFromLua=true` 时）：`Refresh(dataList, offset)` / `RefreshByCount(count, offset)`，配 `m_OnItemInit/m_OnItemShow/m_OnItemHide/m_OnItemFree` 回调。

---

## 六、完整数据流时序

以打开一个界面为例，串起三层：

```
1. Lua: mgr.ui:ShowUI(UIName.Menu_HeroDetail, params)
2. Lua: 加载 Prefab -> require(UICfg.Path).New() -> UIView:Init
3. Lua: UIWidget:BindView(view=CanvasPanel)
4.        _OnViewBinded():
5.          C#: UIUtil.NewCtrlData(transform)
6.              读 RootPanel.m_CtrlList（编辑器导出的控件清单）
7.              逐个控件 o.Set(name, ub) -> 生成 ctrlData 表（挂 CtrlBase 元表）
8.              panel.SetCtrlData(o)   （C# 反向持有）
9.          AddUIEventListener(rootPanel, onShow/onHide -> Show/Hide)
10.       OnCreate(): 业务用 ctrlData.m_XXX 绑事件、填数据
11. Lua: Show() -> 递归子 Widget -> OnShow(isCreating=true)
12. C#: CanvasPanel.SetVisible(true) -> 播入场动画 -> OnShow 事件
--- 用户交互：Button 点击 -> UGUIUpdateMgr 分发 / EventSystem -> UIUtil 协程回调 ---
13. Lua: 关闭 -> UnbindView -> _OnViewUnbinded -> ctrlData=nil，清事件
14. C#: 由 Lua 驱动 ClearListeners（不在 OnDestroy 自动做）
```

---

## 七、相比原生 UGUI 的改造清单（速查）

| 维度 | 原生 UGUI | AOE3D 改造后 |
|------|-----------|--------------|
| 控件访问 | `Find`/`GetComponent` 手写路径 | **CtrlData 表**：`ctrlData.m_BtnClose` 名字直取，编辑器导出、运行时构建 |
| 表现/逻辑分离 | 混在 MonoBehaviour | C# 只做表现，逻辑全在 Lua，靠 CtrlData 桥接 |
| Update 分发 | 每组件各自 `Update()` | **集中式 `UGUIUpdateMgr`** 单点广播，可整类开关，带 Profiler |
| 显隐/可用 | `SetActive` / `CanvasGroup` 手动 | 基类内建 `SetVisible/SetEnable` + `m_Parent` 层级传播 + `SetVisibleUseAlpha` |
| 生命周期清理 | Unity `OnDestroy` | Lua 驱动 `ClearListeners`，规避销毁顺序崩溃 |
| 事件绑定 | `onClick.AddListener` 手动解绑 | `UIUtil.AddBtnListener` 自动清理 + 协程绑定 + A2S 直用 + 自动埋点 |
| 列表 | ScrollRect 自己管 | `ListBox`(对象池/多模板/循环/流式) + `UIListViewEasy` 封装 |
| 固定模板 | 手动 Instantiate | `TemplateBox` + `UITemplateBox`（ControlFromLua） |
| 界面动画 | 手写 Animator | `CanvasPanel + AnimationForCanvasPanel`，UIManager 管栈切换动画进度 |
| 横竖屏 | 无 | `ScreenAdapt` 双套布局 + `FitSafeArea` 安全区 |
| 引导 | 无 | `UIBehaviour.GuideValid/GID` + TemplateBox `GetItemByGuideID` |
| 图片 | Image/RawImage | `ImageEx`(远程异步) / `ImageArray` / `BackImage`(模糊) / `EmptyGraphic` |
| 富文本 | 无图文混排 | `RichText` + RichItem 图/按钮/坐标 + 超链接 |
| 合批 | SpriteAtlas | `Smash` 运行时动态图集/Job 合批 |

---

## 八、关键文件索引

C# 层（`Packages/com.unity.ugui/Runtime/`）：

- `EventSystem/UIBehaviour.cs` —— 改造基类（`#region FOR_AOE`）
- `UI/Core/UGUIUpdateMgr.cs` —— 集中式 Update 分发
- `Ext/Ext/RootPanel.cs` / `RootPanelProperties.cs` —— 控件清单宿主 + CtrlData
- `Ext/Ext/CanvasPanel.cs` —— 界面根节点（Canvas/动画/安全区）
- `Ext/Ext/Panel.cs` —— 分层空壳基类
- `Ext/Ext/ListBox.cs` / `ScrollPanel.cs` / `TemplateBox.cs` / `SimpleBox.cs` —— 容器
- `Ext/Ext/RichText.cs` / `ImageEx.cs` / `BackImage.cs` —— 图形扩展
- `Ext/Ext/ScreenAdapt/*` —— 横竖屏适配
- `Ext/Smash/*` —— 动态图集合批

桥接层：

- `Assets/Scripts/CS/Util/UIUtil.cs` -> `NewCtrlData`（运行时构建 ctrlData）
- `Assets/Scripts/.Lua/UI/CtrlDataBase.lua` -> `CtrlBase` 元表
- `Assets/Scripts/.Lua/UI/CtrlData/**/*_UGUI_CtrlData.lua` -> 各界面静态类型/下标常量（编辑器生成）
- `Assets/Editor/City/CreatScriptModel.cs` / `UIEventInjectTools.cs` -> 导出工具

Lua 框架层（`Assets/Scripts/.Lua/`）：

- `UI/Core/UIWidget.lua` / `UI/Core/UIView.lua` —— UI 基类（BindView 绑定 ctrlData）
- `Mgr/UI/UIManager.lua` —— UI 管理器（三轨管理/栈/冲突队列/动画）
- `Util/UIEventUtil.lua`（即 UIUtil）—— 事件绑定
- `UI/Core/UIListViewEasy.lua` / `UITemplateBox.lua` —— 列表/模板封装

知识库（`.openviking/viking/default/resources/UI/`）：

- `lua_ui_framework.md` —— Lua UI 框架总规格
- `Controls/README.md` + 各组件 md —— 47 个组件 Wiki
- `BindEvent/README.md` —— 事件绑定规范

---

## 九、一句话总结

AOE3D 把 Unity UGUI 从「一堆各自为政的 MonoBehaviour」改造成了「**表现在 C#、逻辑在 Lua、用 CtrlData 表做桥**」的分层框架：编辑器把 Prefab 控件导出成 `RootPanel.m_CtrlList`，运行时 `NewCtrlData` 一次性翻译成挂 `CtrlBase` 元表的 Lua 表，业务逻辑靠 `ctrlData.m_XXX` 名字直取控件；再叠加集中式 Update、自动生命周期事件、对象池列表、界面动画、横竖屏适配、引导与合批等一整套工业化能力。
