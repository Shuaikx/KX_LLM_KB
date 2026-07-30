---
name: openspec-refine-change
description: Revises the planning artifacts of an active, un-archived OpenSpec change based on natural-language feedback about the implemented result. Routes each piece of feedback to the delta spec, design.md, or tasks.md, confirms the routing with the user, edits the artifacts, and validates. Use when the user has run /opsx:apply and is unhappy with the outcome, wants to adjust requirements or approach mid-change, or gives feedback like "the result isn't what I wanted", "change it to X", "效果不对", "改成…" about work covered by a change under openspec/changes/.
---

# Refining an active OpenSpec change from feedback

Turns "I don't like how this turned out" into correct edits of the change's artifacts, so the plan stays the source of truth and the specs stay honest before archive.

## Applies only to active changes

This skill covers a change that still lives in `openspec/changes/<name>/`, whether or not `/opsx:apply` has run.

Do not use it when:

- **The change is archived** (already under `openspec/changes/archive/`). Its delta was merged into `openspec/specs/` and is now history. Direct the user to `/opsx:propose` for a new change that carries `## MODIFIED Requirements` against the main spec.
- **No change covers this work.** Direct the user to `/opsx:propose`.

Never edit `openspec/specs/` from this skill. Merging deltas into the main specs is `/opsx:sync` and `/opsx:archive`'s job.

## Workflow

### 1. Identify the change

If the user named one, use it. Otherwise run `openspec list --json` and **ask** which change the feedback is about. Never guess or auto-select. Announce the choice: "Refining change: `<name>`".

### 2. Read the current state

```bash
openspec status --change "<name>" --json
```

Use `schemaName`, `artifactPaths`, and `changeRoot` from the output rather than assuming repo-local paths.

**If `schemaName` is not `spec-driven`**, the artifact set is custom. For each artifact you intend to touch, run `openspec instructions <artifact-id> --change "<name>" --json` and follow its `instruction`, `rules`, and `template` fields. They override the spec-driven guidance below. The triage principle still holds: observable behavior belongs in the behavior artifact, mechanism in the design artifact, work items in the tracked checklist.

Then read the artifacts themselves. Compare the feedback against the scenarios already written in the delta spec — that comparison is what drives triage.

### 3. Triage, then confirm before editing

Classify every piece of feedback. Always present the triage and get the user's confirmation before writing anything.

| Feedback is really… | Goes to |
|---|---|
| Expected observable behavior differs from what the spec says | delta spec |
| Behavior is right, the approach or structure is not | `design.md` only |
| Spec is right, the code just doesn't do it yet or does it wrong | `tasks.md` only — this is a bug, not a requirement change |
| A capability planned in this change should be dropped | delta spec **and** the proposal's Capabilities section |

The third row is the one to defend. Walk the delta spec's scenarios: if a scenario already describes what the user wants, the spec is fine and only the implementation lags. Misfiling this as a requirement change pollutes the main spec at archive time.

Also check whether the relevant requirement exists at all. If nothing in the delta covers the user's point, the spec has a gap — add a requirement rather than editing an unrelated one.

Present it like this and wait:

```
Feedback triage for <change-name>:

1. "<user's point, quoted>"
   → delta spec (specs/<capability>/spec.md): behavior change
   → Requirement "<name>", scenario "<name>"

2. "<user's point, quoted>"
   → design.md only: same behavior, different approach

3. "<user's point, quoted>"
   → tasks.md only: spec already covers this, implementation lags

Proceed with these edits?
```

### 4. Apply the edits, top down

Work in dependency order: proposal → specs → design → tasks. A spec edit is not finished until you have checked whether `design.md` decisions still hold and whether `tasks.md` needs new work items. Leaving three artifacts that contradict each other is the most common failure here.

Follow the delta spec rules and tasks rules below.

### 5. Validate

```bash
openspec validate "<name>" --strict
```

Delta spec format errors fail silently — a scenario written with three `#` instead of four is simply not parsed. Fix anything reported and re-run until clean.

### 6. Stop

Do not write implementation code and do not run apply. Report what changed and hand back:

```
Revised <change-name>:
- specs/<capability>/spec.md — <what changed>
- design.md — <what changed>
- tasks.md — added N tasks (M.1 … M.N)

Validation: passed

Run /opsx:apply to implement the new tasks.
```

## Delta spec rules

**Decide ADDED vs MODIFIED by where the requirement currently lives.** In an un-archived change the delta has not been merged, so:

- The requirement is under `## ADDED Requirements` in this change's delta → **edit that block in place.** Do not add a `## MODIFIED Requirements` entry for it.
- The requirement exists in `openspec/specs/<capability>/spec.md` → use `## MODIFIED Requirements`.
- Neither → add it under `## ADDED Requirements`.

Writing MODIFIED for a requirement this change itself added breaks the merge: the sync step resolves ADDED by adding it to the main spec and resolves MODIFIED by looking it up in the main spec, so the same requirement in both sections produces an order-dependent result against a target that may not exist yet.

**When MODIFIED is correct**, copy the entire requirement block from the main spec — the `### Requirement:` line through every scenario — then edit. A partial MODIFIED silently drops the omitted scenarios at archive time. The requirement title must match the main spec exactly.

**Format**, non-negotiable because failures are silent:

- `### Requirement: <name>`, then a description using SHALL/MUST
- `#### Scenario: <name>` with exactly four `#`, in WHEN/THEN form
- Every requirement carries at least one scenario

**Keep specs behavioral.** Observable behavior, inputs, outputs, error states. If the change could be implemented a different way without any user-visible difference, it belongs in `design.md`.

**Dropping a capability** means deleting its block from the delta *and* removing it from the proposal's Capabilities section. That section is the contract between the proposal and the specs; leaving a stale entry breaks it.

## tasks.md rules

**Append, don't rewrite.** Add new unchecked items at the end, numbered under a new or existing `## N.` heading. Checked items are the implementation log, and `/opsx:apply` resumes from the first unchecked box — unchecking something in the middle makes it redo everything after it.

Only uncheck a completed task when its output is being discarded wholesale, and say so in the text: `- [ ] 3.2 Rebuild the theme toggle — previous implementation discarded`.

Every task must use the `- [ ] N.M description` form or it is not tracked. Make each one verifiable.

## Example

> User: 我 apply 完了，暗色模式能切，但切换按钮跑到页脚去了，而且刷新之后又变回浅色。

Triage:

1. Button placement — the delta spec has no requirement about where the control lives. The spec has a gap, and placement is user-visible, so add a requirement under `## ADDED Requirements` with a scenario for it, then append a task.
2. Preference not surviving reload — the delta already has "Requirement: Theme preference persists across sessions" with a matching scenario. The spec is correct and the implementation lags, so `tasks.md` only. No spec edit.

Confirm both, edit, validate, hand back to `/opsx:apply`.
