# OpenSpec 全流程记录 — organize-materials-into-kb

> 本文件记录本次通过 `/openspec-propose` 提案流程的关键节点、context 组成、调用的 skill 与 openspec CLI 工具，以及它们各自的功能与角色。
> 仅作概要，非逐字记录。最后更新：2026-07-10

---

## 0. 本次任务

- **目标**：依据 `Karpathy_LLM_KB.md`（方法论）与 `KB_BUILD_PLAN.md`（实施方案），把本仓库根目录下零散资料整理进 Obsidian 驱动的个人知识库。
- **入口命令**：`/openspec-propose`（Cursor slash command）。
- **产物**：一个名为 `organize-materials-into-kb` 的 OpenSpec change，含 proposal / design / specs / tasks。

---

## 1. Context 的组成（本次调用喂给 Agent 的上下文来源）

| 来源 | 内容 | 角色 |
|------|------|------|
| 用户 query | 目标 + 两个 @ 引用文档 + 记录要求 | 意图 |
| `@Karpathy_LLM_KB.md` | Karpathy 的 LLM Wiki 方法论（ingest/query/lint/output） | 方法论依据 |
| `@KB_BUILD_PLAN.md` | 三层架构、目录结构、topic、frontmatter、三操作、路线图、迁移清单 | 实施蓝图 |
| workspace rules（always applied） | `CLAUDE.md`、`AGENTS.md` — KB 维护规范 | 硬约束 |
| manually attached skill | `openspec-propose` SKILL.md 全文内联 | 流程脚本 |
| `openspec/config.yaml` | `schema: spec-driven`，无自定义 context/rules | schema 选择 |
| `openspec status/instructions --json` | 每个 artifact 的 template/instruction/依赖/输出路径 | 逐步指令 |
| 环境信息 | OS win32、PowerShell、git repo、日期 | 执行环境 |

**要点**：`openspec/config.yaml` 未填 `context:` 与 `rules:`，因此 instructions JSON 里没有额外项目级约束，artifact 的写作规范完全来自各自的 `instruction` + `template`。

---

## 2. 调用的 Skill

| Skill | 路径 | 功能 / 角色 |
|-------|------|------------|
| `openspec-propose` | `.cursor/skills/openspec-propose/SKILL.md` | 主流程脚本：定义"一步创建 change 并生成全部 artifacts"的步骤、CLI 调用顺序、artifact 写作守则（context/rules 仅约束不写入文件） |

> 相关但本次未触发：`openspec-apply`（实现 tasks）、`openspec-archive`（归档）、`openspec-sync-specs`（同步 delta spec）、`openspec-explore`（探索模式）。

---

## 3. 调用的 OpenSpec CLI 工具

| 命令 | 功能 | 在本流程中的角色 |
|------|------|------------------|
| `openspec --version` / `--help` | 版本(1.4.1)与命令总览 | 确认 CLI 可用 |
| `openspec new change "<name>"` | 按 schema 脚手架化一个 change 目录 | 创建 `openspec/changes/organize-materials-into-kb/` + `.openspec.yaml` |
| `openspec status --change <name> --json` | 输出 artifact 完成状态、依赖图、`applyRequires`、路径与 actionContext | 得到构建顺序：proposal → design+specs → tasks |
| `openspec instructions <artifact> --change <name> --json` | 输出该 artifact 的 instruction / template / dependencies / resolvedOutputPath | 每个 artifact 写作前取指令 |
| `openspec status --change <name>`（非 json） | 人类可读的最终状态 | 收尾展示 |

**依赖顺序（来自 status JSON）**：
- `proposal` — ready（无依赖）
- `design` — 需 proposal
- `specs` — 需 proposal
- `tasks` — 需 design + specs（`applyRequires: ["tasks"]`）

---

## 4. 关键节点时间线

1. 读取两份参考文档 + config.yaml，确认 schema=spec-driven。
2. 探测 CLI 可用性与子命令帮助（`--version`=1.4.1、`new/status/instructions --help`）。
3. `openspec new change "organize-materials-into-kb"` 创建脚手架。
4. `openspec status --json` 取依赖图与 `applyRequires: ["tasks"]`。
5. 依序取指令并创建 artifact：proposal → design + specs（3 个 capability）→ tasks。
6. `openspec status` 显示 4/4 complete；`openspec validate --strict` 通过。

---

## 5. 产出的 change 结构

```
openspec/changes/organize-materials-into-kb/
├── .openspec.yaml
├── proposal.md
├── design.md
├── specs/<capability>/spec.md
└── tasks.md
```

（各 artifact 完成后补充摘要于下方）

### Artifact 摘要

| Artifact | 角色（回答什么） | 本次要点 |
|----------|------------------|----------|
| `proposal.md` | WHY + WHAT | 落地被延后的资料迁移：根目录零散 md 归类→ingest 进 wiki；定义 3 个新 capability；Non-goals 排除 `KX_Resume_docs/`、qmd/RAG |
| `design.md` | HOW（架构与决策） | D1 先复制到 raw 再 INGEST；D2 新增 `agent-tooling`/`client-ui` 候选课题；D3 `agent_summary` 归 insights；D4 逐篇顺序 ingest；含风险与回滚 |
| `specs/*/spec.md` | WHAT（可测需求） | 三份 delta spec：`material-intake`（归类/inbox/只读）、`wiki-ingestion`（source/concept/entity/矛盾标注）、`kb-navigation-integrity`（index/log/hot/wikilink 完整性） |
| `tasks.md` | 实现清单（可勾选） | 5 组任务：盘点规划 → raw 归类 → 逐篇 INGEST → 索引维护 → 完整性校验；`applyRequires` 所需 |

**校验结果**：`openspec status` → 4/4 complete；`openspec validate --strict` → valid。

---

## 6. 结论：各角色一句话总结

- **Skill `openspec-propose`**：把"提案"标准化为一条可复现流水线，规定先脚手架、再按依赖图逐个产出 artifact，并约束 context/rules 只用于指导、不写进产物。
- **CLI `new`**：按 schema 生成 change 骨架与 `.openspec.yaml`。
- **CLI `status --json`**：唯一权威的"下一步该做什么"来源（依赖图 + applyRequires + 解析后路径）。
- **CLI `instructions --json`**：每个 artifact 的"写作说明书"（instruction + template + 依赖），保证产物结构统一。
- **CLI `validate`**：交付前的结构校验闸门。
- **四类 artifact**：proposal 定"为什么/做什么"，specs 定"可测的什么"，design 定"怎么做/为何这样选"，tasks 定"按序可勾选的执行清单"。

## 7. 下一步（propose 阶段视角）

- 运行 `/opsx:apply`（或让我实现）按 `tasks.md` 执行真正的归类与 INGEST。
- 实现完成后用 `/opsx:archive` 归档，把 delta specs 合入主 specs。

---

# 第二部分：Apply 阶段全流程（2026-07-13）

> 承接上文 propose 产物，通过 `/openspec-propose`→`/opsx-apply` 把 change 落地为真实的 wiki 页面。

## A1. Context 的组成（apply 调用）

| 来源 | 内容 | 角色 |
|------|------|------|
| 用户 query | 「实现 @change」+ 记录全流程要求 | 意图 |
| `contextFiles`（来自 CLI） | proposal / design / 3 份 specs / tasks 的实际路径 | 实现依据 |
| always-applied rules | `AGENTS.md`（frontmatter/For Agent/三操作/只读约束）、`kb.mdc` | 硬约束 |
| manually attached skill | `openspec-apply-change` / `opsx-apply` 命令脚本 | 流程脚本 |
| 待迁移 raw 资料 | 根目录 8 篇 md + `agent_summary/*`（逐篇 Read） | 编译输入 |
| 磁盘实况 | git 显示 `wiki/`、`raw/` 已被删除（未提交） | 触发决策点 |

**关键节点**：apply 前发现 `wiki/`、`raw/` 脚手架已从磁盘删除（git HEAD 仍有）。用 AskQuestion 询问用户 → 用户选择**从零重建**，忽略旧种子页。

## A2. 调用的 Skill

| Skill | 功能 / 角色 |
|-------|------------|
| `openspec-apply`（命令 `/opsx-apply`） | apply 主脚本：选 change→读 status/apply 指令→读 contextFiles→逐个 task 实现→勾选→收尾 |

## A3. 调用的 OpenSpec CLI 工具（apply）

| 命令 | 功能 | 角色 |
|------|------|------|
| `openspec status --change <n> --json` | 查 schema、`applyRequires`、artifact 状态、`actionContext`（编辑范围） | 确认可 apply、repo-local 编辑边界 |
| `openspec instructions apply --change <n> --json` | 输出 `contextFiles`、进度、task 列表、动态 instruction、`state` | 实现指令 + 进度真源（ready→all_done） |

## A4. 本次实现产物

- **归类**：8 篇 raw 复制进 `raw/topics/{ui-spec,feature-research,agent-tooling}/`；空文件 `Agents_概念.md` 留 `raw/inbox/` 未 ingest。
- **新增课题**：`agent-tooling`（登记 `AGENTS.md` + 各 index）；UGUI 并入 `ui-spec`（design 的 Open Question 定稿）。
- **编译**：5 source + 9 concept + 7 entity + 3 insight + 3 topic index + 4 全局页（index/log/hot/overview）= **31 篇 wiki**。
- **完整性**：脚本校验 **0 断链、0 orphan**；用 frontmatter `aliases` 消解多个 `index.md` 的 wikilink 歧义。
- **task 进度**：`instructions apply` → `state: all_done`，23/23。

## A5. Apply 阶段各角色一句话总结

- **Skill `openspec-apply`**：把「实现」标准化为读 context→按 task 顺序改文件→即时勾选→到 all_done 的循环，并在 blocker 处暂停问人。
- **CLI `status --json`**：给出编辑范围（`actionContext`/`allowedEditRoots`）与 apply 前置条件。
- **CLI `instructions apply --json`**：apply 的「施工图」——contextFiles 要读哪些、task 清单、当前 `state`。
- **决策点价值**：apply 不是盲目执行；发现磁盘与计划假设不符时，用 AskQuestion 让用户拍板（重建 vs 恢复）。

## A6. 下一步（apply 阶段视角）

- 可执行首个 `QUERY`（如「ListBox 与 TemplateBox 区别」）验证 wiki 检索。
- 满意后用 `/opsx-archive` 归档 change，把 delta specs 合入主 specs（`openspec/specs/`）。

---

# 第三部分：Archive 阶段全流程（2026-07-13）

> 通过 `/opsx-archive` 把已完成的 change 同步进主 specs 并移入归档区。

## B1. Context 的组成（archive 调用）

| 来源 | 内容 | 角色 |
|------|------|------|
| 用户 query | 「归档 @change」+ 记录要求 | 意图 |
| `opsx-archive` 命令脚本 | 归档流程（校验→sync 评估→移动） | 流程脚本 |
| `openspec status --json` | artifact 状态、`existingOutputPaths`（3 份 delta spec）、`actionContext` | 归档前置校验 |
| `openspec list --json` | 活跃 change 及完成度（23/23 complete） | 确认可归档 |
| 3 份 delta spec + 空的 `openspec/specs/` | 同步输入与目标 | sync 依据 |
| `openspec-sync-specs` skill | delta→主 spec 的智能合并规则 | sync 脚本 |

**关键节点**：主 specs 目录为空 → 用 AskQuestion 询问是否先 sync → 用户选择**先同步再归档**。

## B2. 调用的 Skill

| Skill | 功能 / 角色 |
|-------|------------|
| `openspec-archive`（命令 `/opsx-archive`） | 归档主脚本：校验完成度→评估 delta/主 spec 差异→（可选）sync→移入 `archive/YYYY-MM-DD-<name>` |
| `openspec-sync-specs` | agent 驱动的智能合并：读 delta + 主 spec，按 ADDED/MODIFIED/REMOVED/RENAMED 应用到 `openspec/specs/<capability>/spec.md` |

## B3. 调用的 OpenSpec CLI 工具（archive）

| 命令 | 功能 | 角色 |
|------|------|------|
| `openspec status --change <n> --json` | artifact 状态 + delta spec 路径 + planningHome/changesDir | 归档前校验 + 路径解析 |
| `openspec list --json` | 列活跃 change 及任务完成度 | 确认目标、归档后核验（→ `[]`） |
| `openspec spec list`（已弃用，建议 `openspec show`/`validate --specs`） | 列主 specs | 验证 sync 结果（3 份已注册） |

（移动目录用 shell `Move-Item`；`.openspec.yaml` 随目录一起迁移。）

## B4. 本次归档产物

- **Sync**：因 `openspec/specs/` 为空，3 份 delta（全 ADDED）→ 新建 3 份常驻主 spec：
  - `openspec/specs/material-intake/spec.md`（3 需求）
  - `openspec/specs/wiki-ingestion/spec.md`（3 需求）
  - `openspec/specs/kb-navigation-integrity/spec.md`（3 需求）
  - 每份加了 `## Purpose` 段（delta 无 Purpose，主 spec 需要）。
- **Archive**：`openspec/changes/organize-materials-into-kb` → `openspec/changes/archive/2026-07-13-organize-materials-into-kb`。
- **核验**：`openspec list` → 无活跃 change；`openspec spec list` → 3 份主 spec 已注册。

## B5. Archive 阶段各角色一句话总结

- **Skill `openspec-archive`**：把「收尾」标准化为完成度校验 + delta/主 spec 差异评估 + 移档，警告不阻断但需确认。
- **Skill `openspec-sync-specs`**：把 change 里的 delta spec「固化」成常驻主 specs，支持部分/智能合并而非整体覆盖。
- **CLI `status --json`**：归档同样以它为权威（delta 路径、changesDir、编辑范围）。
- **主 specs（`openspec/specs/`）的意义**：change 是「一次变更」，归档后消失；主 specs 是「系统当前应有的行为」，长期沉淀——本次即把 KB 的三类能力固化为常驻规范。

## B6. 三阶段总览（propose → apply → archive）

```
propose  →  new/status/instructions  →  proposal+design+specs+tasks（planning）
apply    →  status/instructions apply →  真实 wiki 页面 + tasks 全勾选（all_done）
archive  →  status/list + sync-specs  →  delta 固化为主 specs + change 移入 archive
```

一句话：**propose 定计划、apply 落地实现、archive 沉淀规范并封存变更**。
