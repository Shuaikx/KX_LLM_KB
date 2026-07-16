## Why

仓库根目录仍散落着多篇未编译的技术资料（`iwiki_4013196232_子文档总结.md`、`AttrViewer-TreeView-ID冲突修复.md`、`UGUI改造与CtrlData机制详解.md`、`Agent-MCP-Hook-概念解析-rtk案例.md`、`agent_summary/*.md` 等），它们游离在 Karpathy 式 raw→wiki 管线之外，无法在 Obsidian 中被链接、检索或参与 QUERY。`KB_BUILD_PLAN.md` 已把这批迁移标记为"延后执行"，现在需要落地：将散落资料系统性归类、ingest 进 `wiki/`，让整仓成为一个可被 LLM 维护、可在 Obsidian 浏览的知识库。

## What Changes

- 建立**资料归类规则**：把根目录零散 `.md` 按内容映射到 `raw/topics/<slug>/`，无法立即判定的先入 `raw/inbox/`；`raw/` 保持只读、不改写原文语义。
- 按需**新增 topic**：为不属于现有 `ui-spec` / `feature-research` 的资料（如 Agent/MCP、UGUI 客户端）建立新课题目录并登记到 `AGENTS.md` 与 `wiki/index.md`。
- 对每篇已归类 raw 文件执行 **INGEST**：生成/更新 `wiki/sources/`，抽取 concept/entity，更新课题 index、全局 `wiki/index.md`、`wiki/log.md`。
- 维护**导航与完整性**：保证 `index.md` 与实有页一致、`hot.md` 记录会话上下文、`[[wikilinks]]` 无断链，使 Obsidian 打开即可浏览图谱。
- **Non-goals（本次不做）**：不迁移 `KX_Resume_docs/`（独立子库）；不引入 qmd / 向量检索（Phase 3）；不改动 `Karpathy_LLM_KB.md`、`KB_BUILD_PLAN.md`；不删除源文件。

## Capabilities

### New Capabilities
- `material-intake`: 根目录零散资料到 raw 层的归类与暂存规则，含 topic 判定、inbox 兜底、raw 只读约束、新 topic 登记。
- `wiki-ingestion`: 把已归类 raw 文件编译为 wiki 页（source/concept/entity）并更新课题 index 的 INGEST 契约。
- `kb-navigation-integrity`: 全局/课题 index、log、hot 与 wikilink 完整性维护，保证 Obsidian 可浏览、可 QUERY。

### Modified Capabilities
<!-- 无现有 specs（openspec/specs/ 为空），本次全部为新建 capability。 -->

## Impact

- **新增文件**：`raw/topics/<新旧slug>/*.md`（迁移副本）、`wiki/sources/`、`wiki/concepts/`、`wiki/entities/` 下多篇页面。
- **更新文件**：`wiki/index.md`、`wiki/log.md`、`wiki/hot.md`、各 `wiki/topics/*/index.md`、`AGENTS.md`（topic 表）。
- **依赖**：沿用现有 Obsidian 仓库结构与 `AGENTS.md` schema，无新增外部依赖。
- **只读**：`raw/` 原文、根目录参考文档、`KX_Resume_docs/` 不受影响。
