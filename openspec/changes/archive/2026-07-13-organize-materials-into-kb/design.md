## Context

仓库已完成 Phase 0 脚手架与部分 Phase 1：`AGENTS.md` schema、`wiki/` 种子页（index/log/hot/overview、ui-spec 与 feature-research 课题 index、`UI组件` 已 ingest 出 source + 3 concept + 2 entity）均已就绪。但根目录仍有一批未编译资料游离在管线外：

- `iwiki_4013196232_子文档总结.md`（UI 规范类）
- `AttrViewer-TreeView-ID冲突修复.md`（功能调研/调试）
- `UGUI改造与CtrlData机制详解.md`（客户端 UI 机制）
- `Agent-MCP-Hook-概念解析-rtk案例.md`（Agent/工具链）
- `agent_summary/*.md`（3 篇 episodic 会话总结）

约束来自 `AGENTS.md`：raw 只读、不删 wiki 页（只标 superseded）、每次 INGEST 必更新 index 与 log、矛盾显式标注。

## Goals / Non-Goals

**Goals:**
- 把上述根目录资料系统性归类进 `raw/`，并逐篇 INGEST 成合规 wiki 页。
- 按需新增课题（Agent/MCP、UGUI 客户端），保持单 vault 统一检索。
- 保证归类后 Obsidian 打开即可浏览、link 无断裂、index/log 一致。

**Non-Goals:**
- 不迁移 `KX_Resume_docs/`（自带 CLAUDE.md 的独立子库）。
- 不引入 qmd / 向量检索（留待 Phase 3，index 超 ~80 条再评估）。
- 不改动 `Karpathy_LLM_KB.md`、`KB_BUILD_PLAN.md` 及 `raw/` 原文语义。
- 不做 QUERY/insight 回填（本次聚焦 intake + ingestion，QUERY 另行触发）。

## Decisions

**D1：先复制到 raw，再 INGEST，而非原地编译。**
根目录原文件保留不动，在 `raw/topics/<slug>/` 建副本作为只读来源。理由：符合 `AGENTS.md` 三层模型与"raw 只读"，副本可追溯，原文件后续可自由删除或另作他用。备选（原地引用根文件）被否，因会让 raw 层边界模糊、破坏 Karpathy 目录约定。

**D2：新增两个候选课题 `agent-tooling` 与 `client-ui`（名称可在实现时定稿）。**
`Agent-MCP-Hook-概念解析-rtk案例.md` 归 `agent-tooling`；`UGUI改造与CtrlData机制详解.md` 视其与 `ui-spec` 的重叠度，或并入 `ui-spec`，或独立为 `client-ui`。理由：避免把不相关主题硬塞进现有课题稀释检索质量。备选（全塞进 ui-spec）被否。

**D3：`agent_summary/*.md` 作为 episodic 材料 ingest，倾向落到 `wiki/insights/` 而非 sources。**
它们是会话式总结/决策记录，性质更接近 insight。理由：与 `KB_BUILD_PLAN.md` 迁移清单一致。

**D4：逐篇顺序 INGEST 而非批量。**
每篇走完整 INGEST 步骤（source→concept/entity→topic index→global index→log）再进入下一篇，便于矛盾标注与增量校验。

## Risks / Trade-offs

- **[概念/实体重复]** 多篇资料提及同一实体（如 `ListBox`、AttrViewer）易生成重复页 → 每次 INGEST 前先查 `wiki/entities/`、`wiki/concepts/`，命中则增量更新并补 source 链接。
- **[课题划分反复]** 新课题命名/归属早期可能改动 → 先定 slug 再建目录，改名时同步更新 `AGENTS.md`、`wiki/index.md` 与已建页 frontmatter 的 `topic` 字段。
- **[矛盾静默覆盖]** 新旧结论冲突若直接改写会丢历史 → 强制走"标注 + superseded/stale"流程，不删旧页。
- **[index 漂移]** 手工维护 index 易与实有页不一致 → 每篇 INGEST 收尾即同步，收官跑一次一致性检查。

## Migration Plan

1. 盘点根目录待迁移资料，逐篇判定 topic（沿用迁移清单）。
2. 新建所需课题目录并登记。
3. 复制资料到 `raw/topics/<slug>/` 或 `raw/inbox/`（inbox 兜底）。
4. 逐篇 INGEST，更新 wiki 页与 index/log。
5. 收官一致性检查（断链、orphan、index 对齐）。
- **回滚**：wiki 为新增/可标 superseded 的派生产物，raw 副本可删；根目录原文件全程未动，删除 change 与新增 wiki 页即可回到当前状态。

## Open Questions

- `UGUI改造与CtrlData机制详解.md` 并入 `ui-spec` 还是独立 `client-ui`？（实现时据内容重叠度定）
- `agent_summary/session_summary_*.md` 逐篇成页，还是合并为一篇会话 insight？
- 新课题最终 slug 命名（`agent-tooling` vs `agent-mcp`）。
