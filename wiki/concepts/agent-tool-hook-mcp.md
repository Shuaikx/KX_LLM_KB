---
type: concept
topic: agent-tooling
sources:
  - wiki/sources/Agent-Tool-Hook-MCP-rtk.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/agent-tooling
---

# 概念：Agent · Tool · Hook · MCP

## For Agent

四个常被混淆的 LLM 工具生态概念。核心心智：**Hook 与 MCP 是两条正交扩展轴** —— Hook 管「怎么调」（拦截/改写已有调用），MCP 管「能调什么」（新增工具/数据源）。案例见 [[rtk]]。

## 四概念

| 概念 | 比喻 | 一句话 | 交互形式 |
|------|------|--------|----------|
| **Agent** | 大脑 | LLM 决策循环：观察→推理→调工具→再观察 | — |
| **Tool** | 手 | Agent 能发起的原子调用，宿主执行 | 结构化 tool call |
| **Hook** | 门口安检 | 在「决定调用」与「真正执行」之间拦截，可放行/拒绝/改写 | stdin/stdout JSON |
| **MCP** | 通用插座 | 标准协议，Server 暴露 Tools/Resources/Prompts | Client↔Server JSON-RPC |

## 正交关系

```
用户 → [ Agent 决策 ] → (Hook 拦截/改写) → [ Tool 执行 ] → 结果 → Agent
                          ⟂
                   [ MCP Server 提供新的 Tool/Resource ]
```

- Hook 是横切（cross-cutting）中间件，对 Agent 透明，一个 hook 可覆盖所有 shell 命令。
- MCP 是纵向能力扩展，需 Agent「主动改口」调用新工具。
- 二者可同时存在、互不冲突。

## 选型准则

- 改造已有调用（拦截/改写/审计/脱敏/限流/压缩）→ **Hook**。
- 新增一类能力/数据源并跨 Agent 复用（数据库/Jira/浏览器/内部 API）→ **MCP**。
