---
type: insight
topic: feature-research
sources:
  - raw/topics/feature-research/session_summary_20260702_SkillConf.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/insight
  - topic/feature-research
---

# Insight：SkillConfData pbin 解码失败修复（2026-07-02）

## For Agent

排查 `Cfg load error, decode fail! ... SkillConfData_9.pbin` 报错。**真因不在 pbin 数据，而在缺失的 `ResBattleSkill.pb` 描述文件**导致运行时类型 `table_SkillConf` 未注册。修复走 proto 标签补齐 + 重新生成 pb。可复用的调试经验记录。

## 根因链

1. 表面：`SkillConfData_9.pbin` decode fail（堆栈指向 `CfgMgr.lua:861`）。
2. 日志真因：`type 'com.tencent.nk.xlsRes.table_SkillConf' does not exists` + `ResBattleSkill.pb` 不存在。
3. 生成 pb 时 protoc 报 `SkillBuffFactionType is not defined`。
4. proto 不一致：客户端 message 引用了仅 `//@useSvr` 的枚举，未导出到 client proto。

## 修复

- 在源 `common/excel/proto/ResKeywords.proto` 为 `SkillBuffFactionType` 补 `//@useCli`。
- 跑 `client_generate_protocol.bat`（或定向 `genCsPb.py` + `ClientCfgGen`），生成 `ResBattleSkill.pb` 与 `ResKeywords.pb`。
- 建议一并提交两个 pb，避免团队协议不一致。

## 可复用经验

- `decode fail! .pbin` + `type 'xxx' does not exists` → 查 `protobuf/cfg/pb/XXX.pb` 是否存在、是否成功 load，别先怀疑 pbin 数据。
- `SkillConfData*` 依赖 `ResBattleSkill.pb`（映射 `CfgPbDecodeMap.lua` 的 `table_SkillConf`）。
- 客户端 message 若 `//@useCli` 引用某枚举，该枚举也须 `//@useCli`。
- Windows 跑 bat 用 `cmd /c`；`.pbin`=导表数据、`.pb`=schema，职责不同。
- 改 proto 标准顺序：改 `common/excel/proto/` → 生成协议 → （如需）导表 → 重启验证。
