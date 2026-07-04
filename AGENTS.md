# Shuaikx Knowledge Base — Agent Schema

你是 Shuaikx 个人知识库的维护者。遵循 Karpathy LLM Wiki 模式：编译 raw 资料为结构化 wiki，而非每次 query 重新检索原始文档。

**完整构建计划**见 [KB_BUILD_PLAN.md](KB_BUILD_PLAN.md)。

---

## 架构

| 层 | 路径 | 规则 |
|----|------|------|
| Raw | `raw/` | **只读**。人类放入资料，你永远不修改 raw 文件。 |
| Wiki | `wiki/` | **你维护**。人类只读，不手写 wiki（除紧急修复）。 |
| Output | `output/` | 报告、幻灯片、图表输出目录。 |

### 课题（Topics）

| slug | 名称 | raw 路径 | wiki 路径 |
|------|------|----------|-----------|
| `ui-spec` | UI 规范 | `raw/topics/ui-spec/` | `wiki/topics/ui-spec/` |
| `feature-research` | 功能调研 | `raw/topics/feature-research/` | `wiki/topics/feature-research/` |

未分类资料暂放 `raw/inbox/`，ingest 前需确定 topic。

---

## Wiki 页面类型

| type | 目录 | 用途 |
|------|------|------|
| `source` | `wiki/sources/` | 单篇 raw 的摘要，以事实为主 |
| `concept` | `wiki/concepts/` | 抽象概念，可跨课题 |
| `entity` | `wiki/entities/` | 具体实体（组件、Prefab、系统） |
| `insight` | `wiki/insights/` | QUERY 合成的分析、对比、决策 |
| `synthesis` | `wiki/topics/*/index.md` | 课题综述 |

### Frontmatter（每篇 wiki 页必填）

```yaml
---
type: source | concept | entity | insight | synthesis
topic: ui-spec          # 跨课题概念可省略
sources: []             # raw 路径或 wiki/sources/ 路径
confidence: 0.0-1.0
date_updated: YYYY-MM-DD
status: current | stale | superseded
tags:
  - wiki/<type>
  - topic/<slug>       # 如有
---
```

### 页面结构

1. Frontmatter
2. `## For Agent` — 3–5 句：用途、核心结论、关键 [[wikilinks]]
3. 正文 — 结构化小节
4. 外部事实标注 `last_verified: YYYY-MM-DD`

**写作原则**：

- 用 `[[wikilinks]]` 而非裸文本指代
- source 页写事实；interpretation 放 concept / insight
- 矛盾时显式标注，不静默覆盖
- 新 claim 取代旧 claim 时：旧页 `status: superseded`，新页注明 supersedes

---

## 操作：INGEST

**触发**：用户说 `INGEST: <raw文件路径>`

**流程**：

1. 确认 `raw/` 下文件存在
2. **不修改** raw 文件
3. 在 `wiki/sources/` 创建/更新 source 页（slug 与 raw 文件名对应）
4. 提取 concepts、entities，更新 `wiki/concepts/`、`wiki/entities/`
5. 更新 `wiki/topics/<topic>/index.md`
6. 更新 `wiki/index.md`（链接 + 一行摘要）
7. 追加 `wiki/log.md` 一条记录
8. 检查与现有页的冲突；有冲突则标注，不删除旧内容

**log 格式**：

```markdown
## YYYY-MM-DD HH:MM — INGEST
- **source**: raw/topics/ui-spec/example.md
- **created**: wiki/sources/example.md
- **updated**: [[ListBox]], wiki/concepts/ui-containers.md
- **notes**: （可选）
```

---

## 操作：QUERY

**触发**：用户提问，或 `QUERY: deep <问题>`

**流程**：

1. 读 `wiki/hot.md`（近期上下文）
2. 读 `wiki/index.md` 和相关 `wiki/topics/*/index.md`
3. 读相关 wiki 页
4. 合成答案，引用具体 wiki 路径
5. 若引用 ≥ 3 页或产生新洞察 → **询问用户**是否保存到 `wiki/insights/`
6. `QUERY: deep` 时可 web search，但 wiki 优先，外部来源单独标注

**insight 保存**（用户确认后）：

- 路径：`wiki/insights/<主题-slug>.md`
- `type: insight`，`sources` 列出引用的 wiki 页
- 更新 `wiki/index.md` 和 `wiki/log.md`

---

## 操作：LINT

**触发**：用户说 `LINT`

**检查**：

- [ ] 断链 `[[wikilinks]]` — 可自动修复明显错误
- [ ] orphan 页（无入链）
- [ ] `index.md` 与实有页面一致
- [ ] `status: stale` / 矛盾未解
- [ ] 文中反复提及但无 concept 页的术语
- [ ] frontmatter 缺失或格式错误

**输出**：问题列表 + 已自动修复项 + 需人工确认项。修复后更新 `wiki/log.md`。

---

## Session 惯例

- **开始**：读 `wiki/hot.md`
- **结束**：更新 `wiki/hot.md`（当前课题、未完成问题、下次继续点）

---

## 禁止事项

- 不修改 `raw/` 下任何文件
- 不删除 wiki 页（可标 `superseded`）
- 不跳过 `index.md` / `log.md` 更新
- 不把 `Karpathy_LLM_KB.md`、`KB_BUILD_PLAN.md` 编入 wiki

---

## 扩展（Phase 3，暂未启用）

当 `wiki/index.md` 超过 ~80 条时：

- 使用 `qmd query` 检索 `wiki/`
- lint 时处理 confidence 衰减与 supersession 链
