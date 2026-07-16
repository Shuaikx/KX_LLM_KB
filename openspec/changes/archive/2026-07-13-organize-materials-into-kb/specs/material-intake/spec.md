## ADDED Requirements

### Requirement: 根目录资料归类到 raw 层
系统 SHALL 将仓库根目录下的零散 `.md` 资料按内容归类，复制到 `raw/topics/<slug>/` 对应课题目录；无法立即判定课题的资料 SHALL 先复制到 `raw/inbox/` 暂存。归类过程 MUST NOT 修改任一 `raw/` 下文件的内容语义，MUST NOT 删除根目录原始文件。

#### Scenario: 明确课题的资料直接归类
- **WHEN** 一篇根目录资料能明确对应某个已存在课题（如 `iwiki_4013196232_子文档总结.md` → `ui-spec`）
- **THEN** 系统在 `raw/topics/<slug>/` 下创建同名副本，并保留根目录原文件不变

#### Scenario: 课题不明的资料入 inbox
- **WHEN** 一篇资料无法立即判定归属课题
- **THEN** 系统将其复制到 `raw/inbox/`，并在后续 INGEST 前再确定 topic

### Requirement: 按需新增课题
当资料不属于任何现有课题时，系统 SHALL 新增课题：在 `raw/topics/<新slug>/` 与 `wiki/topics/<新slug>/` 下创建同名目录，并把新课题登记到 `AGENTS.md` 课题表与 `wiki/index.md`。

#### Scenario: 新增 Agent/MCP 课题
- **WHEN** 归类 `Agent-MCP-Hook-概念解析-rtk案例.md` 且无匹配课题
- **THEN** 系统新建对应课题目录，并在 `AGENTS.md` 与 `wiki/index.md` 登记该课题

### Requirement: raw 层只读约束
系统 SHALL 将 `raw/` 视为只读来源层。除新增归类副本外，MUST NOT 编辑、重写或删除已存在的 raw 文件；`KX_Resume_docs/`、`Karpathy_LLM_KB.md`、`KB_BUILD_PLAN.md` MUST 排除在本次归类之外。

#### Scenario: 排除独立子库与参考文档
- **WHEN** 扫描根目录待归类资料
- **THEN** 系统跳过 `KX_Resume_docs/` 及元参考文档，不将其纳入 raw 归类
