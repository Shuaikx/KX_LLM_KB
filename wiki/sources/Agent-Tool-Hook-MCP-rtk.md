---
type: source
topic: agent-tooling
sources:
  - raw/topics/agent-tooling/Agent-MCP-Hook-概念解析-rtk案例.md
confidence: 0.9
date_updated: 2026-07-13
status: current
tags:
  - wiki/source
  - topic/agent-tooling
---

# Source：Agent · Tool · Hook · MCP 概念解析（以 rtk 为例）

## For Agent

用 [[rtk]]（把 shell 命令输出压缩 60–90% 的 CLI）作案例，讲清四个常被混淆的概念，是 [[agent-tool-hook-mcp]] 概念页的主要来源。核心结论：**Hook 与 MCP 正交** —— Hook 拦截/改写「已有工具调用」，MCP 给 Agent「新增工具与数据源」；rtk 走 Hook 而非 MCP。

## 关键事实

### 四概念定位
- **Agent**：LLM 驱动的决策循环（观察→推理→调用工具→再观察）。
- **Tool**：Agent 能发起的结构化调用（`Shell`/`Read`/`Edit`），真正执行由宿主运行时完成。
- **Hook**：工具生命周期回调点（如 `PreToolUse`），stdin/stdout 收发 JSON，可 allow/deny/ask + `updatedInput` 改写参数，对 Agent 透明。
- **MCP**：开放协议（Client↔Server, JSON-RPC），Server 暴露 Tools/Resources/Prompts，即插即用扩展能力。

### rtk 案例
- rtk = Rust Token Killer，单文件 Rust CLI，把 `ls/cat/grep/git/...` 冗长输出过滤压缩。
- 接入方式是 `PreToolUse` Hook：把 `X` 改写成 `rtk X`。
- 两套 hook：Claude（`~/.claude/settings.json`，`rtk hook claude`，matcher `Bash`，Allow/Ask 都改写）与 Cursor（`~/.cursor/hooks.json`，`rtk hook cursor`，matcher `Shell`，仅 Allow 改写，规则来自 `~/.cursor/cli-config.json`）。
- 本机实测：Cursor 的 `cli-config.json` 不存在 → Cursor 路径全返回 `{}`（空转），真正省 token 的是 Claude 路径。

### 选型准则
- 改造已有调用（拦截/改写/审计/压缩）→ 用 **Hook**。
- 新增一类能力/数据源并跨 Agent 复用 → 用 **MCP**。

## 参考

- rtk 仓库 <https://github.com/rtk-ai/rtk>；MCP 规范 <https://modelcontextprotocol.io>（`last_verified: 2026-07-04`，来自原文）
