# 操作日志（append-only）

## 2026-07-13 — INGEST（批量，via OpenSpec change organize-materials-into-kb / apply）

- **背景**：用户选择「从零重建」；上次会话的 wiki/raw 种子页已从磁盘删除，本次不恢复旧种子，按当前根目录资料全新生成。
- **归类到 raw**（只读副本，根目录原文件未改动）：
  - `raw/topics/ui-spec/`：UI组件.md、iwiki_4013196232_子文档总结.md、UGUI改造与CtrlData机制详解.md、session_summary_20260703_ListCondition.md
  - `raw/topics/feature-research/`：AttrViewer-TreeView-ID冲突修复.md、AttrViewer_升级总结_20260706.md、session_summary_20260702_SkillConf.md
  - `raw/topics/agent-tooling/`：Agent-MCP-Hook-概念解析-rtk案例.md
  - `raw/inbox/`：Agents_概念.md（空文件，未 ingest）
- **新增课题**：`agent-tooling`（登记于 [[overview]]、[[index]]、各 topic index）。
- **created — sources**：[[UI组件]]、[[iwiki-4013196232]]、[[UGUI-CtrlData机制]]、[[AttrViewer-TreeView-ID冲突修复]]、[[Agent-Tool-Hook-MCP-rtk]]
- **created — concepts**：[[prefab-screen-forms]]、[[ui-layer-stack]]、[[ui-containers]]、[[ctrldata-bridge]]、[[centralized-ui-update]]、[[unified-ui-interaction]]、[[ui-rendering-pitfalls]]、[[agent-tool-hook-mcp]]、[[unity-treeview-id]]
- **created — entities**：[[ListBox]]、[[TemplateBox]]、[[RootPanel]]、[[CanvasPanel]]、[[UIListViewEasy]]、[[AttrViewer]]、[[rtk]]
- **created — insights**：[[attrviewer-upgrade]]、[[skillconf-pbin-decode-fix]]、[[listcondition-listvieweasy]]
- **updated**：[[overview]]、[[index]]、[[hot]]、topics/{ui-spec,feature-research,agent-tooling}/index
- **notes**：因全新重建，无新旧 claim 冲突（任务 3.5 不适用）。topic index 用 frontmatter `aliases` 消解多个 `index.md` 的 wikilink 歧义。
