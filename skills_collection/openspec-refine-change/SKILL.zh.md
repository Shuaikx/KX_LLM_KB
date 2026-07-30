---
name: openspec-refine-change
description: 根据用户对实现结果的自然语言反馈，修订某个处于活跃状态（未归档）的 OpenSpec change 的规划产物。把每一条反馈分派到 delta spec、design.md 或 tasks.md，先跟用户确认分派结果，再改产物并做校验。适用场景：用户跑完 /opsx:apply 后对结果不满意、想在 change 中途调整需求或方案，或者针对 openspec/changes/ 下某个 change 覆盖的工作给出类似"结果不是我想要的"、"改成 X"、"效果不对"、"改成…"这样的反馈。
---

# 依据反馈修订活跃的 OpenSpec change

把"我不喜欢现在这个结果"转化成对 change 产物的正确修改，让计划始终是唯一事实来源，并保证归档前 spec 是诚实可信的。

## 仅适用于活跃的 change

本 skill 针对的是仍然存在于 `openspec/changes/<name>/` 下的 change，不论 `/opsx:apply` 是否已经跑过。

以下情况不要使用本 skill：

- **change 已归档**（已经位于 `openspec/changes/archive/` 下）。它的 delta 已经合并进 `openspec/specs/`，属于历史记录。此时应引导用户走 `/opsx:propose`，新建一个针对主 spec 携带 `## MODIFIED Requirements` 的 change。
- **没有任何 change 覆盖这项工作**。引导用户走 `/opsx:propose`。

在本 skill 中永远不要修改 `openspec/specs/`。把 delta 合并进主 spec 是 `/opsx:sync` 和 `/opsx:archive` 的职责。

## 工作流

### 1. 确定是哪个 change

如果用户点明了，就用它。否则执行 `openspec list --json` 并**主动询问**这条反馈针对的是哪个 change。绝不猜测、绝不自动选择。确定后宣告选择："正在修订 change：`<name>`"。

### 2. 读取当前状态

```bash
openspec status --change "<name>" --json
```

使用输出中的 `schemaName`、`artifactPaths` 和 `changeRoot`，不要假设仓库内的路径。

**如果 `schemaName` 不是 `spec-driven`**，说明产物集合是自定义的。对每一个你打算改动的产物，执行 `openspec instructions <artifact-id> --change "<name>" --json`，并遵循其 `instruction`、`rules`、`template` 字段。这些字段的优先级高于下面针对 spec-driven 的指引。但分派原则依然成立：可观测行为归入行为类产物，实现机制归入设计类产物，工作项归入被追踪的清单。

然后读取产物本身。把反馈和 delta spec 中已经写好的 scenario 逐条对比，这个对比过程正是分派的依据。

### 3. 先分派，确认后再动手改

对每一条反馈做归类。务必先把分派结果呈现给用户并取得确认，再写任何内容。

| 这条反馈实际上是… | 归到哪里 |
|---|---|
| 期望的可观测行为和 spec 写的不一致 | delta spec |
| 行为是对的，但方案或结构不对 | 仅 `design.md` |
| spec 是对的，只是代码还没做到或者做错了 | 仅 `tasks.md`，这是 bug，不是需求变更 |
| 本 change 中原计划的某个能力应当砍掉 | delta spec **以及** proposal 的 Capabilities 小节 |

第三行是最需要守住的。把 delta spec 的 scenario 逐条走一遍：如果某个 scenario 已经描述了用户想要的东西，那么 spec 没问题，只是实现落后了。把这种情况错分成需求变更，会在归档时污染主 spec。

同时还要检查相关的 requirement 是否根本不存在。如果 delta 里没有任何内容覆盖用户提到的这一点，说明 spec 有缺口，应当**新增**一条 requirement，而不是去改一条无关的。

按这个格式呈现，然后等待用户回应：

```
<change-name> 的反馈分派：

1. "<引用用户的原话>"
   -> delta spec (specs/<capability>/spec.md)：行为变更
   -> Requirement "<名称>"，scenario "<名称>"

2. "<引用用户的原话>"
   -> 仅 design.md：行为不变，方案不同

3. "<引用用户的原话>"
   -> 仅 tasks.md：spec 已覆盖，实现落后

按以上方案修改吗？
```

### 4. 自上而下地施加修改

按依赖顺序推进：proposal -> specs -> design -> tasks。一次 spec 修改并不算完成，除非你已经检查过 `design.md` 的决策是否依然成立、以及 `tasks.md` 是否需要新增工作项。留下三份互相矛盾的产物是这里最常见的失败。

遵循下面的 delta spec 规则和 tasks 规则。

### 5. 校验

```bash
openspec validate "<name>" --strict
```

delta spec 的格式错误是静默失败的：一个 scenario 写成三个 `#` 而不是四个，就直接不会被解析。修掉所有报出的问题并重跑，直到干净通过。

### 6. 收手

不要写实现代码，也不要跑 apply。汇报改了什么，然后交还给用户：

```
已修订 <change-name>：
- specs/<capability>/spec.md 改动内容：<what changed>
- design.md 改动内容：<what changed>
- tasks.md 新增 N 条任务（M.1 … M.N）

校验：通过

执行 /opsx:apply 来实现这些新任务。
```

## delta spec 规则

**ADDED 还是 MODIFIED，取决于这条 requirement 当前所在的位置。** 在一个未归档的 change 中，delta 尚未被合并，所以：

- 这条 requirement 位于本 change 的 delta 的 `## ADDED Requirements` 下 -> **就地修改那个块**。不要再为它添加 `## MODIFIED Requirements` 条目。
- 这条 requirement 存在于 `openspec/specs/<capability>/spec.md` 中 -> 使用 `## MODIFIED Requirements`。
- 两处都没有 -> 加到 `## ADDED Requirements` 下面。

为本 change 自己新增的 requirement 写 MODIFIED 会破坏合并流程：sync 步骤处理 ADDED 的方式是把它加进主 spec，处理 MODIFIED 的方式是去主 spec 里查找它。同一条 requirement 同时出现在两个小节，就会产生依赖顺序的结果，而且查找目标可能还不存在。

**当 MODIFIED 确实正确时**，把整个 requirement 块从主 spec 完整拷过来，从 `### Requirement:` 那行一直到它的每一个 scenario，然后再改。不完整的 MODIFIED 会在归档时静默丢掉被省略的 scenario。requirement 标题必须与主 spec 完全一致。

**格式**，不容商量，因为违反后是静默失败：

- `### Requirement: <名称>`，随后是使用 SHALL/MUST 的描述
- `#### Scenario: <名称>`，必须恰好四个 `#`，采用 WHEN/THEN 形式
- 每条 requirement 至少携带一个 scenario

**保持 spec 是行为层面的。** 只写可观测行为、输入、输出、错误状态。如果这个改动换一种实现方式也不会带来任何用户可见的差异，那它属于 `design.md`。

**砍掉一个能力**意味着从 delta 中删掉它的块，**并且**从 proposal 的 Capabilities 小节中移除它。那个小节是 proposal 与 spec 之间的契约，留下过期条目会破坏这份契约。

## tasks.md 规则

**追加，不要重写。** 在末尾添加新的未勾选条目，编号挂在一个新的或已有的 `## N.` 标题下。已勾选的条目是实现日志，而 `/opsx:apply` 是从第一个未勾选的复选框开始续跑的，把中间某项取消勾选会让它把后面所有内容重做一遍。

只有当某个已完成任务的产出要被整体丢弃时，才取消它的勾选，并且在文字里说明：`- [ ] 3.2 重建主题切换按钮，此前的实现已废弃`。

每条任务都必须使用 `- [ ] N.M 描述` 的形式，否则不会被追踪。并且每条都要可验证。

## 示例

> 用户：我 apply 完了，暗色模式能切，但切换按钮跑到页脚去了，而且刷新之后又变回浅色。

分派：

1. 按钮位置：delta spec 里没有任何关于这个控件放在哪里的 requirement。spec 存在缺口，而位置属于用户可见的，所以在 `## ADDED Requirements` 下新增一条 requirement 并附带对应 scenario，然后追加一条任务。
2. 偏好设置无法在刷新后保留：delta 里已经有 "Requirement: Theme preference persists across sessions" 以及匹配的 scenario。spec 是正确的，只是实现落后了，所以仅改 `tasks.md`，不动 spec。

两条都确认，改完，校验，交还给 `/opsx:apply`。
