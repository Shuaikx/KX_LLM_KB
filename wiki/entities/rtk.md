---
type: entity
topic: agent-tooling
sources:
  - wiki/sources/Agent-Tool-Hook-MCP-rtk.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/entity
  - topic/agent-tooling
---

# 实体：rtk（Rust Token Killer）

## For Agent

单文件 Rust CLI，把常见开发命令（`ls`/`cat`/`grep`/`git`/`cargo test`/`docker ps`…）的冗长输出过滤、去噪、压缩，回灌给 LLM 的内容少 60–90% token。是理解 [[agent-tool-hook-mcp|Hook vs MCP]] 的真实案例：rtk 走 **Hook** 而非 MCP。

## 事实

- 接入方式：注册 `PreToolUse` Hook，把 Agent 想跑的 `X` 改写成 `rtk X`，对 Agent 透明。
- 两套 hook：
  - Claude：`~/.claude/settings.json`，命令 `rtk hook claude`，matcher `Bash`，Allow/Ask 都改写。
  - Cursor：`~/.cursor/hooks.json`，命令 `rtk hook cursor`，matcher `Shell`，仅 Allow 改写（规则来自 `~/.cursor/cli-config.json`）。
- 协议形状：从 `/tool_input/command` 读命令，改写时输出 `hookSpecificOutput.updatedInput.command`；无等价命令时输出 `{}`（不干预）。
- 本机实测：Cursor 的 `cli-config.json` 不存在 → Cursor 路径全返回 `{}` 空转，真正省 token 的是 Claude 路径。

## 参考

- 仓库 <https://github.com/rtk-ai/rtk>；源码 `src/hooks/hook_cmd.rs`、`permissions.rs`；用量 `rtk gain --history`。
