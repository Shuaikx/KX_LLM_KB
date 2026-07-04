# AttrViewer TreeView 多选高亮 Bug 修复总结

## 基本信息

| 项目 | 内容 |
|------|------|
| 工具 | Unity Editor - AttrViewer（属性系统查看器） |
| 菜单入口 | AoE -> Lua -> 查看属性系统（快捷键 Alt+C） |
| 涉及文件 | `Assets/Editor/AttrViewer/AttrViewerWindow.cs` |
| 修复日期 | 2026-07-03 |

---

## 问题现象

在属性系统查看器中，当多个 Lua table 下存在**显示文本相同**的叶子字段时（例如多个 `presetTeamData` 条目下都有 `commanderToPresetTeamId = {number} 0`），点击选中其中一行，**所有相同显示文本的行会同时被高亮**，表现为"选中一个字段，类似的字段都会被选中"。

---

## Bug 原因

### 1. Unity TreeView 的选中机制

Unity `TreeView` 使用**整数 id** 唯一标识每个节点。用户点击某一行时，选中状态绑定在该 id 上；界面上所有**共享同一 id** 的可见行都会一起高亮。

### 2. 叶子节点 id 生成方式错误

修复前，非 `LuaTable` 的叶子节点创建代码如下：

```csharp
Id++;
this.AddChild(new TreeViewItem(GetStableId(itemName, null), 0, itemName));
```

`GetStableId` 在 `table == null` 时仅对 `displayName` 做 hash：

```csharp
private static int GetStableId(string name, LuaTable table)
{
    if (table != null)
    {
        return (name + table.GetHashCode()).GetHashCode();
    }
    return name.GetHashCode();  // 仅依赖显示文本
}
```

因此，凡是 `displayName` 相同（字段名 + 类型 + 值都相同）的叶子节点，会得到**完全相同的 id**。

### 3. LuaTable 节点也存在 id 参数未生效的问题

`LuaTableTreeViewItem` 构造函数虽然接收了递增的 `id` 参数，但 `base` 调用使用的是 `GetStableId(name, table)`，传入的 `Id` 被忽略：

```csharp
// 修复前
public LuaTableTreeViewItem(int id, string name, LuaTable table) : base(GetStableId(name, table), 0, name)
```

虽然 LuaTable 节点因包含 `table.GetHashCode()` 碰撞概率较低，但 id 分配策略不一致，且静态递增 `Id` 形同虚设。

### 4. 触发场景示例

以 `yzPresetTeam.presetTeamData` 为例，索引 `1`、`1001`、`2`、`3` 下若均存在：

```
commanderToPresetTeamId = {number} 0
```

四条记录的 `displayName` 完全一致 -> 四条记录的 TreeView id 完全一致 -> 点击任意一条，四条同时高亮。

---

## 解决方法

统一改为使用**全局递增 id**（`LuaTableTreeViewItem.Id`），保证每个树节点 id 唯一。

### 修改 1：LuaTable 节点使用传入的递增 id

```csharp
// 修复后
public LuaTableTreeViewItem(int id, string name, LuaTable table) : base(id, 0, name)
```

### 修改 2：叶子节点使用递增 id

```csharp
// 修复后
Id++;
this.AddChild(new TreeViewItem(Id, 0, itemName));
```

### 修改 3：移除已无用的 GetStableId 方法

`GetStableId` 不再被任何代码引用，已删除，避免后续误用。

### id 分配规则（修复后）

| 节点类型 | id 来源 |
|----------|---------|
| 根节点 root | 固定为 0 |
| LuaTable 子节点 | 每次 `Id++` 后传入构造函数 |
| 叶子节点（string/number/boolean 等） | 每次 `Id++` 后直接作为 TreeViewItem id |

每次刷新属性树时，`RefreshTable()` 会将 `LuaTableTreeViewItem.Id` 重置为 1，然后整棵树重建，id 从 1 起重新递增分配。

---

## 修复后产生的影响

### 正面影响

1. **选中行为正确**：点击任意叶子字段，仅高亮当前行，不再误选其他 table 下的同名字段。
2. **id 策略一致**：LuaTable 节点与叶子节点均使用同一套递增 id，逻辑清晰、可维护。
3. **监视列表不受影响**：监视项刷新时通过 `item.id` 重建节点，id 唯一后行为更可靠。

### 无影响 / 可忽略的变化

1. **刷新后 id 会重新分配**：每次点"刷新"或重新打开窗口，节点 id 数值会变，但 TreeView 内部使用，对用户不可见，不影响功能。
2. **展开/滚动/选中状态保留逻辑不变**：`RefreshTable()` 仍通过 `expandedIDs`、`selectedIDs`、`scrollPos` 尝试恢复 UI 状态；因 id 每次重建会变化，刷新后**原先记住的选中项可能无法精确恢复**（此行为修复前即存在，非本次引入）。

### 不涉及的范围

- 未改动属性数据的读取逻辑（仍从 `mgr.userAttr.root` 获取）。
- 未改动搜索、监视列表、双击跳转 VS Code 等功能。
- 未改动 `LuaAttrParser.cs` 的类型映射逻辑。
- `SaveTable()` 仍为未实现状态，与本次修复无关。

---

## 验证建议

1. 进入 Play 模式并登录，打开 **AoE -> Lua -> 查看属性系统**。
2. 展开存在重复字段名的结构（如 `yzPresetTeam -> presetTeamData -> 1/2/3...`）。
3. 点击其中一条 `commanderToPresetTeamId = {number} 0`，确认**仅当前行高亮**。
4. 分别点击不同索引下的同名字段，确认每次只选中一行。
5. 点击"刷新"，确认树可正常重建，选中行为仍正确。

---

## 相关代码位置

```
Assets/Editor/AttrViewer/AttrViewerWindow.cs
  - LuaTableTreeViewItem 构造函数（约第 402 行）
  - 叶子节点 AddChild（约第 440-441 行）
  - RefreshTable() 中 LuaTableTreeViewItem.Id = 1 重置（约第 145 行）
```
