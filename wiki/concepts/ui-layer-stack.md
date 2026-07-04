---
type: concept
topic: ui-spec
sources:
  - raw/topics/ui-spec/UI组件.md
confidence: 0.85
date_updated: 2026-07-03
status: current
tags:
  - wiki/concept
  - topic/ui-spec
---

## For Agent

AOE3D UI 层级与继承关系。判断界面应走 Stack、Popup 还是 Hud 时参考。

# UI 层级与继承关系

> last_verified: 2026-07-03

## 类继承树

```
Entity → UIWidget → UIView (Menu_/PopupBox_/Hud_)
                  → UIFloatBase → Float_*
子模板: ListItem_* (ListBox) / Pan_* (TemplateBox)
```

## UILayer 关键层（低→高）

`LowerFloat` → `Float` → `Hud` → `FullScreen`(栈) → `PopupBox` → `MessageBox` → `Tips` → `Loading`

## 管理 API

| 形态 | API |
|------|-----|
| Menu_ | `ShowStackUI` / `HideStackUI` |
| PopupBox_ | `ShowUI` / `HideUI` |
| Hud_ | `ConstantUIManager` |
| Float_ | `GetFloatUI` / `PutFloatUI` |

## 相关

- [[wiki/concepts/prefab-screen-forms]]
- [[wiki/sources/UI组件]]
