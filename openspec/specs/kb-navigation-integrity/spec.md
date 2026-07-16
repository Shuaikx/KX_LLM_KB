# kb-navigation-integrity

## Purpose

规范知识库的导航与完整性：全局/课题 index 与实有页面一致、操作日志与会话热缓存维护、wikilink 无断链，保证整仓在 Obsidian 中可浏览、可 QUERY。

## Requirements

### Requirement: 索引与实有页面一致
每次 INGEST 后，系统 SHALL 更新全局 `wiki/index.md`（每条：链接 + 一行摘要）与对应 `wiki/topics/<slug>/index.md`，使索引条目与 `wiki/` 下实有页面保持一致，无遗漏、无指向不存在页的条目。

#### Scenario: 新增页面同步索引
- **WHEN** INGEST 新建了 source/concept/entity 页
- **THEN** 系统在全局 index 与课题 index 中新增对应链接与一行摘要

### Requirement: 操作日志与会话热缓存
系统 SHALL 在每次 INGEST 后向 `wiki/log.md` 追加一条记录（含 source、created、updated、notes），并在会话结束时更新 `wiki/hot.md` 记录当前课题与未完成事项。

#### Scenario: 追加 log 记录
- **WHEN** 完成一次 INGEST
- **THEN** 系统向 `wiki/log.md` append 一条带时间戳的 INGEST 记录，不覆盖历史

### Requirement: wikilink 完整性
系统 SHALL 保证 wiki 内 `[[wikilinks]]` 无断链，使整仓在 Obsidian 中打开即可正确跳转与呈现关系图；发现断链或 orphan 页时 SHALL 报告或自动修复明显错误。

#### Scenario: 检测断链
- **WHEN** 对 wiki 执行完整性检查
- **THEN** 系统列出断链 `[[wikilinks]]` 与无入链 orphan 页，并修复可明确判定的错误链接
