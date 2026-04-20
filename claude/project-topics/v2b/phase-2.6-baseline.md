# Phase 2.6 Content-System Realignment — Baseline (P2.6.CS.0)

Gate: P2.6.CS.0 (pre-flight)
Executed: 2026-04-20 09:01:07 UTC
Controlling baseline docs:
- CANONICAL_DECISIONS_FINAL.md (previous turn)
- PHASE_2_6_CONTENT_SYSTEM_CHECKLIST_FINAL.md (previous turn)
- MIGRATION_STRATEGY_FINAL.md (previous turn)

---

## 1. Git baseline

- Branch: `dev_wip`
- HEAD:   `3c1e01a97a6f2822117921fef26e8f9ef5aed06a`
- Working-tree status: pre-existing modifications on CLAUDE.md, several guards/
  audits, many deleted `.gitkeep` placeholders under content-system (already
  removed in an earlier pass — domains are live with real artifacts now).
- Untracked analysis docs (kept, treat as reference input):
  - `src/domain/content-system/context_features.md`
  - `src/domain/content-system/domain_mapping.md`
  - `claude/project-prompts/20260420-085037-runtime-r3-a-6-human-approval-wait-state.md`
  - `claude/project-topics/v2b/closure/`

## 2. Tree snapshot

Scope: `src/domain/content-system/**` (C# + README only)
- File count: 485
- SHA256(filelist): `27709d6031a44875312eba2a6646ae59e3142a7730b5d0edffcef0aaa601b12f`
- Filelist path: `/tmp/content-system-tree.txt` (ephemeral)

## 3. Guard load

All 4 canonical guard files present directly under `claude/guards/`:
- `constitutional.guard.md` (1594 lines)
- `runtime.guard.md`        (3702 lines)
- `domain.guard.md`         (1570 lines)
- `infrastructure.guard.md` (verified present)
- No subdirectories under `claude/guards/` — `GUARD-LAYER-MODEL-01` satisfied.

## 4. Call-site inventory

### 4.1 DomainRoute / CommandContext references
- `DomainRoute("content", ...)` literal: 0 code call-sites.
  Matches in docs only: `src/domain/content-system/README.md`,
  `claude/guards/domain.guard.md` (examples).
- `Classification = "content"` literal: 0 match in any file.
  VERDICT: content classification has no active runtime bindings today. This
  means the realignment's DomainRoute tuple stability rule (§CD-14) is
  trivially satisfied — no callers to break.

### 4.2 Namespace fragments to be renamed
C# files referencing the legacy groups: 235 files
- `ContentArtifact` (document + media)
- `CompanionArtifact` (media)
- `DeliveryArtifact` (streaming)
- `PersistenceAndObservability` (streaming)
These will be rewritten during gates CS.1, CS.5, CS.6, CS.9.

### 4.3 Path references in READMEs/docs
- 38 files reference the legacy kebab-case group names.
- Includes one stray doc `checker.md` at repo root (inspect later; non-blocking).
- All content-system READMEs will be rewritten Band-S in-gate.

## 5. Mirror-layer status (DS-R4)

Critical finding — **pre-existing drift, PRE-DATES P2.6.CS**:

- `src/engines/T2E/content/`                       — ABSENT
- `src/projections/content/`                        — ABSENT (note: projections
  previously existed under `infrastructure/data/postgres/projections/content/...`
  and are among the deleted files in working tree — separate concern)
- `src/systems/downstream/content/`                 — not checked yet (inspect
  if needed before a later gate)
- `infrastructure/policy/domain/content/`          — ABSENT
- `infrastructure/event-fabric/kafka/topics/content/` — ABSENT

IMPACT: DS-R4 mirroring cannot be satisfied by the P2.6.CS gates because the
mirror layers do not yet exist. This realignment focuses strictly on the
DOMAIN LAYER tree (which is what the target canonical tree addresses). Mirror
layer scaffolding is a separate future phase. Captured as informational
drift — not a blocker for CS gates 1–13.

Finding recorded in:
`claude/new-rules/20260420-090107-guards.md`

## 6. Deferred items blocking map (recap)

| Item  | Blocks      | Closes at        |
|-------|-------------|------------------|
| §DF-04 retention scope | Wave 1 (CS.2)  | decision before CS.2 |
| §DF-01 playback fate   | Wave 4 (CS.5)  | CS.4a |
| §DF-02 metrics home    | Wave 4 (CS.5)  | CS.5 prep |
| §DF-06 recording split | CS.6           | before CS.6 |
| §DF-03 media VO split  | Wave 5 (CS.8)  | by §CD-03 refinement (LOCKED) |

§DF-03 is now effectively closed by §CD-03 refinement (intrinsic→asset,
evaluative→quality) — pre-closed at plan time.
§DF-04 will be resolved inline during CS.2 (recommendation: OPT-A, per-context
retention in document/governance/retention; no shared promotion).

## 7. Gate exit criteria

| Criterion | Status |
|-----------|--------|
| Baseline snapshot written | ✅ this file |
| All 4 guards loadable     | ✅ |
| Call-site inventory done  | ✅ |
| Tree hash reproducible    | ✅ |
| Drift captured under `/claude/new-rules/` | ✅ (mirror-layer finding) |

**P2.6.CS.0 VERDICT: PASS.** Proceed to P2.6.CS.1.
