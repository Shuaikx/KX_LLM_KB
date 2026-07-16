## ADDED Requirements

### Requirement: 将 raw 文件编译为 source 页
对每篇已归类的 raw 文件，系统 SHALL 在 `wiki/sources/` 生成或更新一篇 source 页，slug 与 raw 文件名对应，且包含合规 frontmatter（`type: source`、`topic`、`sources`、`confidence`、`date_updated`、`status`）与 `## For Agent` 摘要段。

#### Scenario: 新资料生成 source 页
- **WHEN** 对 `raw/topics/ui-spec/iwiki_4013196232_子文档总结.md` 执行 INGEST
- **THEN** 系统在 `wiki/sources/` 创建对应 source 页，含 frontmatter 与事实摘要

### Requirement: 抽取 concept 与 entity
INGEST 过程中，系统 SHALL 从 source 内容抽取抽象概念写入 `wiki/concepts/`、具体实体写入 `wiki/entities/`，并以 `[[wikilinks]]` 相互链接；已存在的 concept/entity 页 SHALL 增量更新而非重复创建。

#### Scenario: 复用已有实体页
- **WHEN** 新 source 再次提及已存在的实体 `[[ListBox]]`
- **THEN** 系统更新既有 `wiki/entities/ListBox.md` 并补充来源链接，而非新建重复页

### Requirement: 矛盾显式标注
当新 ingest 的内容与现有 wiki 页冲突时，系统 SHALL 显式标注矛盾，MUST NOT 静默覆盖旧内容；新 claim 页可标 `status: current`，被取代的旧页标 `status: superseded` 或 `stale`。

#### Scenario: 新旧结论冲突
- **WHEN** 新资料给出与现有 concept 页相反的结论
- **THEN** 系统在两页标注冲突关系并更新 status，保留旧内容可追溯
