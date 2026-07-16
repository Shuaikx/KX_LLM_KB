---
type: synthesis
sources: []
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/synthesis
---

# Session 热缓存

## For Agent

记录最近会话上下文与未完成事项，Session 开始时先读此页。

## 当前状态（2026-07-13）

- **本次工作**：通过 OpenSpec change `organize-materials-into-kb` 的 apply 阶段，把根目录零散资料重建/编译进 `wiki/`。
- **已完成**：8 篇 raw 资料归类进 `raw/topics/*`，编译出 5 篇 source、9 篇 concept、7 篇 entity、3 篇 insight，新增课题 `agent-tooling`。
- **未完成 / 待办**：
  - `Agents_概念.md` 为空文件，暂留根目录，待补充内容后 ingest。
  - 可执行首个 `QUERY`（如「ListBox 与 TemplateBox 区别」）验证检索。
  - 后续新资料走 `raw/inbox/` → 归类 → INGEST。

## 关键上下文

- UI 课题的核心桥梁概念是 [[ctrldata-bridge]]；容器双范式见 [[ui-containers]]。
- AttrViewer 相关分散在 [[AttrViewer]] 实体与 [[attrviewer-upgrade]] insight。
