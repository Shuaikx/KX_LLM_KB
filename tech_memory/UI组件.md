# AOE3D 项目 UI 组件形式说明

本项目 UI 组件可以从两个层面理解：**Prefab 屏幕/模板形态**（`Menu_`、`ListItem_` 等前缀）和 **Unity 控件/容器类型**（`ListBox`、`TemplateBox` 等）。下面按层次说明。

---

## 一、Prefab 屏幕形态（按文件名前缀）

这是业务上最常说的「UI 形式」，决定界面层级、打开方式和骨架结构。

### 1. `Menu_` — 全屏菜单

| 属性 | 说明 |
|------|------|
| **panel_type** | `fullscreen_menu` |
| **尺寸** | 约 1920x800 ~ 1920x1080 |
| **管理层** | `ShowStackUI` / `HideStackUI`，走 **UI 栈**（`FullScreen` 层） |
| **骨架分区** | `Pan_Top` / `Pan_Middle` / `Pan_Left` / `Pan_Right` / `Pan_Bottom` |
| **典型用途** | 英雄详情、出征、商店、科技树、战报等主功能页 |

特点：占满屏幕，可压栈（打开新 Menu 时隐藏上一个），有入场/退场动画，常带 `Pan_Anim` 动画壳。

示例：`Menu_ArmyMarch2`、`Menu_HeroDetail`、`Menu_SilkRoadFormation`

---

### 2. `PopupBox_` — 标准弹窗

| 属性 | 说明 |
|------|------|
| **panel_type** | `popupbox` |
| **尺寸** | 约 1080x680 ~ 1800x800 |
| **管理层** | `ShowUI` / `HideUI`，`PopupBox` 层（9~10） |
| **内容区** | 主要放在 `Pan_Content` |
| **典型用途** | 二次确认、规则说明、奖励预览、筛选、恭喜获得 |

特点：居中浮层，有遮罩，走弹窗冲突队列（`UIPopupboxConflictQueue`），可配置 `DefaultPopupBox` 等 Flag。

示例：`PopupBox_MsgBox`、`PopupBox_CommonRule`、`PopupBox_GetItem2`

---

### 3. `PopupBox_HoverInfo_` — Hover / Tips 提示

| 属性 | 说明 |
|------|------|
| **本质** | PopupBox 的子类 |
| **典型用途** | 鼠标悬停、长按、点击信息图标时的轻量说明 |
| **特点** | 无标题或极简标题，内容紧凑，常跟锚点位置显示 |

示例：`PopupBox_HoverInfo`、`PopupBox_HoverInfo_BattleReport`、`PopupBox_HoverInfo_SilkRoadFormation`

---

### 4. `Pan_` — 面板（两种角色）

`Pan_` 有两种用法，需要看上下文：

**A. 独立屏幕（侧滑/底部浮层）**

| panel_type | 说明 |
|------------|------|
| `side_popup_left` / `side_popup_right` | 左右侧滑面板，约 600~800 宽 |
| `bottom_float` | 底部浮层，高度约 200~600 |

**B. 子模板/子面板（更常见）**

挂在 Menu / PopupBox 内部，通过 `TemplateBox` 动态实例化，或作为 Tab 子页。

示例：
- 独立：`Pan_BattleReport_Page1`（战报分页）
- 子模板：`Pan_HeroCircleCard`、`Pan_ArmyMarchHeroCell`、`Pan_RedPoint_Counter`

特点：比 ListItem 更大、更完整，可含 ListBox 和多个交互区；CtrlData 多在 `CtrlData/Template/` 下。

---

### 5. `ListItem_` — 列表项模板

| 属性 | 说明 |
|------|------|
| **容器** | 挂在 `ListBox` 里 |
| **数量** | 动态（对象池复用） |
| **CtrlData** | 统一在 `CtrlData/Template/` |
| **典型用途** | 英雄卡片、聊天行、商店商品、部队索引格 |

特点：
- 不单独打开，由父界面的 `UIListViewEasy` / `OnListItemShow` 驱动
- 支持**多模板**（同一 ListBox 可注册多种 ListItem）
- 一行数据对应一个 item，适合滚动长列表

示例：`ListItem_ArmyMarchSoldierNum2`、`ListItem_HudCommonChat`、`ListItem_HeroList_Card`

---

### 6. `Hud_` — 常驻 HUD

| 属性 | 说明 |
|------|------|
| **管理层** | `ConstantUIManager`，场景切换时自动显示/隐藏 |
| **层级** | `UILayerCfg.Hud`（第 3 层） |
| **特点** | 常驻、高性能约束（禁止重逻辑 OnUpdate） |

示例：
- `Hud_Common` — 主界面（资源条、按钮组、聊天等）
- `Hud_Msg` — 通用信息提示
- `Hud_Common_Army` — 右侧部队队列

特点：不走 Stack/Popup 打开流程，与 3D 场景共存，子组件用 `UIWidget` 拆分（如 `Hud_Common_Top`、`Hud_Common_Bottom`）。

---

### 7. `Float_` — 3D 世界悬浮 UI

| 属性 | 说明 |
|------|------|
| **基类** | `UIFloatBase` |
| **管理层** | `floatRoot`，`GetFloatUI` / `PutFloatUI` |
| **层级** | `Float` / `LowerFloat`（1~2 层） |
| **典型用途** | 地图气泡、建筑名、血条、对话气泡、实体 CD |

特点：跟随 3D 坐标或实体，World Space / Screen Space 混合，不阻塞主界面交互。

示例：`Float_EntityBubble`、`Float_DialogBubble`、`Float_InfiniteZoomName`、`Float_MapBuildingHpBar`

---

### 8. 其他常见变体

| 前缀/名称 | 特点 |
|-----------|------|
| `PopupBox_Dialog_*` | 剧情对话全屏/半屏 |
| `PopupBox_MsgBox` | 二次确认（MessageBox 层） |
| `Hud_Msg` / `Hud_StrongToast` | 轻提示、强提示（Tips 层） |
| `Menu_MarchFloat_*` | 出征相关全屏变体（带 Float 交互逻辑） |

---

## 二、层级关系总览

```
Entity
  UIWidget                    -- 子组件基类
    UIView                    -- 可打开的界面
      Menu_*                  -- 全屏栈 UI
      PopupBox_*              -- 弹窗
      Hud_*                   -- 常驻 HUD
      UIFloatBase
        Float_*               -- 3D 悬浮

  子模板（不独立打开）:
    ListItem_*  -> ListBox 内
    Pan_*       -> TemplateBox 内 或 作为子页
```

**20 层 UILayer 关键层**（从低到高）：

`LowerFloat` -> `Float` -> `Hud` -> `FullScreen`(栈) -> `PopupBox` -> `MessageBox` -> `Tips` -> `Loading` ...

---

## 三、容器控件形式（Prefab 内部）

除了 Prefab 前缀，节点上还常用这些**容器组件**：

### `ListBox` — 滚动列表容器

- 动态数量、对象池、可滚动
- Lua 侧用 `UIListViewEasy` + `OnListItemShow`
- 子模板：`ListItem_*`

```lua
---@field m_ListArmyMarchTeam UnityEngine.UI.Extensions.ListBox
---    子控件模板列表: ListItem_ArmyMarchTeamIndex_UGUI_CtrlData
```

### `TemplateBox` — 固定槽位模板容器

- 编辑器预置位置节点，运行时按数据填充
- 适合**少量固定格子**（如 5 个兵种位、3 个英雄槽）
- Lua 侧用 `UIUtil.RegistTemplateBox`
- 子模板：`Pan_*` 或 `ListItem_*`

```lua
---@field m_PanSoldierNum_01 UnityEngine.UI.Extensions.TemplateBox
---    子控件模板: ListItem_ArmyMarchSoldierNum2_UGUI_CtrlData
```

### `Panel` / `CanvasPanel` — 逻辑分组容器

- `SetVisible` 控制显隐
- `CanvasPanel` 通常是界面根节点（`m_RootPanel`）

### 基础控件

| 前缀 | 组件 | 用途 |
|------|------|------|
| `m_Btn` | Button | 按钮 |
| `m_Lab` | Text | 文本 |
| `m_Pic` | Image | 图片 |
| `m_Tog` | Toggle | 开关/Tab |
| `m_Input` | InputField | 输入框 |
| `m_Slider` | Slider | 滑动条 |
| `m_List` | ListBox | 列表 |
| `m_Pan` | Panel/LayoutGroup | 布局分区 |

---

## 四、如何选择形式

| 场景 | 推荐形式 |
|------|----------|
| 完整功能页、需压栈返回 | `Menu_` |
| 居中确认/规则/奖励 | `PopupBox_` |
| 悬停说明、轻提示 | `PopupBox_HoverInfo_` |
| 侧滑详情、底部条 | 独立 `Pan_` |
| 长列表一行一项 | `ListItem_` + `ListBox` |
| 固定 N 个格子（如 5 英雄位） | `Pan_`/`ListItem_` + `TemplateBox` |
| 主界面常驻元素 | `Hud_` |
| 地图上跟实体走的气泡 | `Float_` |
| 复杂卡片（含子列表） | `Pan_` 子模板 |

---

## 五、典型组合示例（丝路编队 / 出征）

- **`Menu_SilkRoadFormation`** — 全屏 Menu 壳
- **`ListItem_ArmyMarchSoldierNum2`** — ListBox/TemplateBox 里的列表项
- **`Pan_ArmyMarchHeroCell`** — 英雄格子子面板
- **`PopupBox_HoverInfo_SilkRoadFormation`** — 悬停 Tips

---

## 参考来源

- `.openviking/viking/default/resources/UI/lua_ui_framework.md`
- `.openviking/viking/default/resources/UI/LayoutWorkflow/schemas_docs/knowledge_base_spec.md`
- `.openviking/viking/default/resources/UI/Controls/ListBox.md`、`TemplateBox.md`
- `.openviking/viking/default/resources/UI/Module/Hud_Common/hud_common.md`
