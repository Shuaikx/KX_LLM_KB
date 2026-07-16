## 1. 盘点与课题规划

- [x] 1.1 列出根目录待迁移资料清单，排除 `KX_Resume_docs/`、`Karpathy_LLM_KB.md`、`KB_BUILD_PLAN.md`
- [x] 1.2 为每篇资料判定 topic（ui-spec / feature-research / 新课题 / inbox 兜底）
- [x] 1.3 定稿新课题 slug（`agent-tooling`；UGUI 并入 ui-spec，未启用 client-ui），在 `raw/topics/` 与 `wiki/topics/` 建目录
- [x] 1.4 在 `AGENTS.md` 课题表与 `wiki/index.md` 登记新课题

## 2. 资料归类到 raw（只读来源层）

- [x] 2.1 复制 `iwiki_4013196232_子文档总结.md` → `raw/topics/ui-spec/`
- [x] 2.2 复制 `AttrViewer-TreeView-ID冲突修复.md` → `raw/topics/feature-research/`
- [x] 2.3 复制 `UGUI改造与CtrlData机制详解.md` → `raw/topics/ui-spec/`
- [x] 2.4 复制 `Agent-MCP-Hook-概念解析-rtk案例.md` → `raw/topics/agent-tooling/`
- [x] 2.5 复制 `agent_summary/*.md` 到对应 raw 目录，标记为 episodic 材料
- [x] 2.6 核对：根目录原文件与 raw 原有文件均未被改写或删除（`Agents_概念.md` 空文件留 inbox，未 ingest）

## 3. 逐篇 INGEST 编译 wiki

- [x] 3.1 对每篇 raw 生成/更新 `wiki/sources/` source 页（5 篇，含合规 frontmatter + `## For Agent`）
- [x] 3.2 抽取 concept 写入 `wiki/concepts/`（9 篇）
- [x] 3.3 抽取 entity 写入 `wiki/entities/`（7 篇：ListBox/TemplateBox/RootPanel/CanvasPanel/UIListViewEasy/AttrViewer/rtk）
- [x] 3.4 `agent_summary/*` 作为 insight 落到 `wiki/insights/`（3 篇）
- [x] 3.5 新旧结论冲突时显式标注（本次全新重建，无新旧冲突，不适用）

## 4. 导航与索引维护

- [x] 4.1 更新 `wiki/topics/<slug>/index.md`（3 个课题综述，含 aliases）
- [x] 4.2 更新全局 `wiki/index.md`（链接 + 一行摘要）
- [x] 4.3 向 `wiki/log.md` append 本次 INGEST 记录
- [x] 4.4 更新 `wiki/hot.md` 记录当前课题与未完成项

## 5. 完整性校验与收官

- [x] 5.1 检查 `[[wikilinks]]` 无断链（脚本校验：0 断链）
- [x] 5.2 检查 orphan 页（脚本校验：0 orphan）
- [x] 5.3 校验 `index.md` 与实有 wiki 页面一致（31 md 全覆盖）
- [x] 5.4 在 Obsidian 打开仓库根确认呈现（结构已校验；建议用户在 Obsidian 视觉复核关系图）
