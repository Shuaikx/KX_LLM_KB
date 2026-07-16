---
type: concept
topic: ui-spec
sources:
  - wiki/sources/iwiki-4013196232.md
confidence: 0.85
date_updated: 2026-07-13
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

# 概念：统一交互 / 弹窗 / 提示体系

## For Agent

AOE3D UI 基建的核心规范：**Tips / MsgBox / 提示条 / 弹窗都走统一入口**，不要各写各的历史散装接口。目标是标准化交互、数据驱动配置、降低维护成本。

## 统一入口清单

| 类别 | 统一做法 |
|------|----------|
| Tips/Hover/确认/飘字 | `ShowTips1~4`、`ShowConfirmDialog`、`ShowYes/YesNo/YesNoWithCounter/...`（Tips 查询文档为总索引） |
| 消息文本拼接 | 系统消息 Format 规则：`DisplayPara` 各类型（数字/坐标/玩家名/道具名/时间戳…）→ 展示文本 |
| 弹窗 | 统一新版 `ShowPopupMsgBox` 体系（全量/单按钮/双按钮/左取消右确认） |
| 提示条 | 统一接口按参数区分跑马灯/错误/中间提示 |
| 改名/输入 | 通用改名弹窗 `PopupBox_CommonChangeNameParamData`（敏感词+长度+消耗+二次确认） |
| 功能说明 | 复用配表 `T_图文引导`，传功能 ID |
| 触发式弹窗 | `mgr.ui:ShowStackUIOnSceneEmpty()` 统一打开时机与优先级 |
| 栈多实例 | `MultiInStack` Flag：同名 prefab 保留多份状态（`OnMultiModeSameUIShow`/`Restore`） |

## 底层沉降规范

- 倒计时：文本拼接放 C#，统一注册（别在 Lua 每秒 `setText`）。
- 时间点触发：用服务端时间戳回调机制，替代本地 `scheduler`。
- 多 EditBox 校验：`UIFormatView` 统一管特殊字符/繁体/长度/敏感词。

## 一句话准则

先找有没有统一组件/统一弹窗/统一调度，再决定要不要自己写。渲染类疑难见 [[ui-rendering-pitfalls]]。
