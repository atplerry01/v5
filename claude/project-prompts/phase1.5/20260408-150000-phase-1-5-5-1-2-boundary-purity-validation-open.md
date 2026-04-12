# Phase 1.5 §5.1.2 — Boundary Purity Validation (Workstream Opening Pack)

## TITLE
Phase 1.5 §5.1.2 Boundary Purity Validation — canonical workstream opening pack.

## CLASSIFICATION
system / governance / boundary-control

## CONTEXT
Phase 1.5 §5.1.1 Dependency Graph Remediation closed PASS on 2026-04-08
(see [20260408-143500-phase-1-5-5-1-1-pass-closure.md](20260408-143500-phase-1-5-5-1-1-pass-closure.md)).
Dependency-graph correctness is necessary but insufficient: a reference
edge being permitted does not prove that the code on either side of that
edge is doing only what its layer is canonically allowed to do. §5.1.2
is the next mandatory structural hardening item before Phase 2 and must
be opened as a fully tracked, auditable workstream in the same style and
rigor as §5.1.1.

This artifact is the **opening pack only**. No remediation work is
performed here. No guard, audit, source, or script file is modified.
The workstream is created in `OPEN` state and handed off for execution
in subsequent prompts.

---

## 1. EXECUTIVE SUMMARY

§5.1.2 Boundary Purity Validation verifies that every Whycespace layer
remains within its canonical responsibilities, even where the dependency
graph technically permits an edge. Where §5.1.1 asked *“is this edge
allowed?”*, §5.1.2 asks *“is the code on this edge doing only what its
layer is allowed to do?”*. The workstream covers platform/api vs
platform/host separation, runtime ownership, systems midstream/downstream
separation, T1M vs T2E purity, projections boundaries, persistence
ownership, composition-root purity, and orchestration drift against
WBSM v3.5 canon. Output is a boundary-purity audit report, a remediation
patch list, and (if necessary) guard updates — promoted only after
governance review. Initial status: **OPEN**.

---

## 2. WORKSTREAM DEFINITION

### 2.1 Purpose
Validate that the implementation honors WBSM v3.5 canonical layer
ownership boundaries beyond the dependency-graph level — i.e. that no
layer silently absorbs responsibilities belonging to another, even when
the project reference graph is clean.

### 2.2 Objective
Produce a complete, evidence-backed determination, per canonical
boundary, of whether the current codebase is **PURE**, **DRIFTED**, or
**VIOLATING**, and convert every non-PURE finding into a tracked
remediation item with severity, owner, and acceptance test.

### 2.3 Why This Matters Before Phase 2
- Phase 2 expands the runtime, projections, and systems surfaces. Any
  latent boundary drift will be amplified and become structurally
  expensive (or impossible) to reverse once new code is layered on top.
- §5.1.1 only proved that edges are legal. Boundary purity is the
  semantic counterpart: it proves that the code on those edges is
  legal. Phase 2 promotion is gated on both.
- WBSM v3.5 canonical layering is the contract that all later operational
  certifications (5.2 runtime hardening, 5.3 observability, 5.4 security,
  5.5 governance) build on. Drift here invalidates downstream gates.
- Boundary drift is the most common origin of silent business-rule
  migration into the wrong layer, which is an S1 violation under the
  WBSM v3 canonical execution rules ($16).

### 2.4 Known Risk Areas To Inspect
- **R1** — `platform/api` vs `platform/host` separation: API surface
  contamination by host concerns (DI, hosting, adapter wiring).
- **R2** — `runtime/` ownership: persistence, publishing, anchoring must
  live only in runtime; verify no engine or projection performs them.
- **R3** — `systems/midstream` vs `systems/downstream` separation:
  midstream must not adopt downstream responsibilities and vice versa.
- **R4** — **T1M vs T2E** purity: T1M (machine) and T2E (event) tiers
  must remain non-bypassable. Any direct T2E side-effect from T1M code
  paths is a canonical violation.
- **R5** — Projections boundary: projections may consume events only;
  must not call domain, runtime, or engine entry points directly.
- **R6** — Persistence ownership: only runtime adapters may persist;
  engines must remain stateless and event-only ($7 layer purity).
- **R7** — Composition root purity: `platform/host` may wire dependencies
  but must not execute business logic, route commands, or own state.
- **R8** — Runtime bypass from platform: no platform code may invoke
  runtime internals that bypass the canonical execution flow ($1).
- **R9** — Engine-owned persistence: any engine touching `DbContext`,
  outbox tables, Kafka producers, or anchoring directly is S0.
- **R10** — Silent business-rule migration: business invariants drifting
  into projections, host adapters, or systems integrations.
- **R11** — Orchestration drift: any orchestration outside the runtime
  canonical orchestrator violates WBSM v3.5.

### 2.5 Scope
- All source under `src/platform/`, `src/runtime/`, `src/engines/`,
  `src/projections/`, `src/systems/`, `src/domain/`, `src/shared/`.
- All composition-root wiring inside `src/platform/host/`.
- All T1M and T2E execution paths reachable from platform entry points.
- Existing guard files relevant to boundary purity:
  `behavioral.guard.md`, `domain.guard.md`, `engine.guard.md`,
  `policy.guard.md`, `projection.guard.md`, `runtime.guard.md`,
  `structural.guard.md`, `systems.guard.md`.
- Existing audit files relevant to boundary purity:
  `behavioral.audit.md`, `domain.audit.md`, `engine.audit.md`,
  `infrastructure.audit.md`, `projection.audit.md`, `runtime.audit.md`,
  `structural.audit.md`, `systems.audit.md`.

### 2.6 Non-Scope
- Dependency graph remediation (closed under §5.1.1).
- Canonical documentation alignment (handled in §5.1.3).
- Runtime infrastructure hardening (handled in §5.2.x).
- Performance, observability, security, or governance items
  (§5.3–§5.5).
- Refactoring `platform/host` adapters into a new `src/infrastructure/**`
  project (Phase 2 candidate; explicitly out of scope per
  DG-R5-EXCEPT-01).
- Any change to DG-R5-EXCEPT-01 or DG-R5-HOST-DOMAIN-FORBIDDEN.

### 2.7 Remediation Strategy
1. **Inventory** — enumerate every canonical boundary from WBSM v3.5
   canon and map it to the source paths that own it.
2. **Probe** — for each boundary, run targeted code probes (grep,
   symbol search, type-shape inspection) to detect responsibility
   leakage.
3. **Classify** — every probe result is classified as `PURE`,
   `DRIFTED` (recoverable, no behavior change), or `VIOLATING`
   (requires code or guard remediation).
4. **Triage** — assign severity per WBSM v3 §16: S0 system-breaking,
   S1 architectural, S2 structural, S3 formatting.
5. **Patch list** — non-PURE findings become a remediation patch list
   with explicit acceptance tests; no inline code changes during the
   audit pass itself.
6. **Guard reinforcement** — where a violation indicates a missing
   guard rule, capture it under `claude/new-rules/` per $1c, do not
   silently extend an existing guard.
7. **Promote** — execution and remediation occur in a follow-up prompt;
   this opening pack ends at the patch-list handoff.

### 2.8 Task Breakdown
- **T-A** Boundary inventory — extract canonical boundaries from
  WBSM v3.5 canon and write the inventory table.
- **T-B** Probe matrix — define one or more probes per boundary
  (R1–R11), each with an exact command or grep predicate.
- **T-C** Probe execution — run the probe matrix against the current
  tree; capture raw output as evidence.
- **T-D** Classification — assign `PURE` / `DRIFTED` / `VIOLATING`
  to every probe result with one-line justification.
- **T-E** Severity triage — assign S0/S1/S2/S3 to every non-PURE
  finding.
- **T-F** Patch list — produce remediation patch list with file paths,
  diff intent, and acceptance test per item.
- **T-G** New-rules capture — write any newly discovered guard rules
  to `claude/new-rules/{YYYYMMDD-HHMMSS}-guards.md` per $1c.
- **T-H** Final boundary-purity audit report — single artifact bundling
  inventory, probe matrix, raw evidence, classifications, severities,
  patch list, and PASS/FAIL determination.

### 2.9 Acceptance Criteria
1. Every WBSM v3.5 canonical boundary in scope (§2.5) is enumerated in
   the inventory and probed at least once.
2. Every probe has reproducible evidence (command + raw output) stored
   alongside the audit report.
3. Every probe result is classified as PURE / DRIFTED / VIOLATING with
   a one-line justification.
4. Every non-PURE finding has S0–S3 severity assigned per $16.
5. Every non-PURE finding has a remediation patch list entry with file
   path, intended change, and acceptance test.
6. No remediation patch is applied during the audit pass; opening-pack
   discipline is preserved until §5.1.2 advances out of audit phase.
7. Any newly discovered guard rule is captured under `claude/new-rules/`
   with the canonical 5-field shape (CLASSIFICATION, SOURCE,
   DESCRIPTION, PROPOSED_RULE, SEVERITY) per $1c.
8. Final report explicitly returns one of: `PASS`, `FAIL`, `PARTIAL`,
   `BLOCKED`, `WAIVED`, with the reason recorded.
9. The §5.1.2 row in the README §6.0 master tracking table is updated
   only when the workstream actually advances state — not by the
   opening pack itself.

### 2.10 Evidence Required
- Boundary inventory table.
- Probe matrix (probe ID, boundary ID, command/predicate, expected
  shape of a PURE result).
- Raw probe output for every probe (verbatim).
- Classification table (probe ID → PURE/DRIFTED/VIOLATING + reason).
- Severity table (finding ID → S0/S1/S2/S3).
- Remediation patch list.
- New-rules capture file (if any).
- Final boundary-purity audit report with explicit terminal status.

---

## 3. TRACKING TABLE

| Field | Value |
|---|---|
| **ID** | 5.1.2 |
| **Topic** | Boundary Purity Validation |
| **Objective** | Validate that every layer remains within its canonical WBSM v3.5 responsibility boundaries, beyond the dependency-graph level. |
| **Tasks** | T-A Inventory · T-B Probe matrix · T-C Probe execution · T-D Classification · T-E Severity triage · T-F Patch list · T-G New-rules capture · T-H Final audit report |
| **Deliverables** | Boundary inventory · Probe matrix · Raw probe evidence · Classification table · Severity table · Remediation patch list · New-rules capture (if any) · Final boundary-purity audit report |
| **Evidence Required** | Reproducible probe commands and raw output for every boundary in §2.5; classification + severity for every finding; explicit terminal status (PASS/FAIL/PARTIAL/BLOCKED/WAIVED) |
| **Status** | OPEN (NOT STARTED — workstream defined, no execution yet) |
| **Risk** | MEDIUM — boundary drift is common in pre-Phase-2 codebases and is the primary origin of silent business-rule migration; latent S0/S1 findings are plausible. |
| **Blockers** | None known. §5.1.1 PASS prerequisite satisfied 2026-04-08. |
| **Owner** | Whycespace governance / structural hardening track |
| **Notes** | Opening pack only. No remediation in this prompt. Promotion of state in README §6.0 master tracking table occurs only when execution advances. Any new guard rules must be captured under `claude/new-rules/` per $1c, not silently merged into existing guards. Continuity with §5.1.1 closure (2026-04-08) preserved. |

**Status legend:** NOT STARTED · IN PROGRESS · PARTIAL · BLOCKED · PASS · FAIL · WAIVED.

---

## 4. ACCEPTANCE CRITERIA
(See §2.9 above. Reproduced here for tracking convenience.)

1. Every in-scope canonical boundary enumerated and probed.
2. Every probe has reproducible evidence.
3. Every probe result classified PURE / DRIFTED / VIOLATING with reason.
4. Every non-PURE finding has S0–S3 severity.
5. Every non-PURE finding has a remediation patch list entry with
   acceptance test.
6. No remediation applied during audit pass.
7. Any newly discovered guard rule captured under `claude/new-rules/`.
8. Final report returns explicit terminal status.
9. README §6.0 row 5.1.2 updated only on real state change.

---

## 5. REQUIRED ARTIFACTS

- `claude/project-prompts/20260408-150000-phase-1-5-5-1-2-boundary-purity-validation-open.md`
  — this opening pack.
- `claude/audits/boundary-purity.audit.md` — to be created during T-H
  (final audit report). Not created by this opening pack.
- `claude/new-rules/{YYYYMMDD-HHMMSS}-guards.md` — to be created during
  T-G if and only if newly discovered guard rules emerge.
- README §5.1.2 (existing) — unchanged by this opening pack; the
  workstream definition is anchored here, but state promotion is gated
  on real execution.
- README §6.0 master tracking table row 5.1.2 — unchanged by this
  opening pack.

---

## 6. CLAUDE EXECUTION PROMPT

> **Use this prompt to execute §5.1.2 in a follow-up session. Do not
> execute it as part of this opening pack.**

```
Phase 1.5 §5.1.2 — Boundary Purity Validation (Execution Pass)

CLASSIFICATION: system / governance / boundary-control
CONTEXT: §5.1.1 PASS (2026-04-08). Opening pack:
  claude/project-prompts/20260408-150000-phase-1-5-5-1-2-boundary-purity-validation-open.md
OBJECTIVE: Execute T-A through T-H of §5.1.2 as defined in the opening
  pack. Produce claude/audits/boundary-purity.audit.md as the single
  consolidated deliverable. Do not modify source, guards, scripts, or
  README outside the audit artifact and (if needed) a single
  claude/new-rules/ capture file.

CONSTRAINTS:
  - WBSM v3 canonical execution rules ($1–$16) apply in full.
  - Pre-execution: load every guard in claude/guards/ ($1a). No skip,
    no cache, no summary.
  - Post-execution: run every audit in claude/audits/ ($1b). Inline-fix
    any drift discovered against the audit artifact itself before
    completion.
  - Anti-drift ($5): no architecture changes, no renames, no file
    moves, no inference of missing components.
  - File system ($6): only operate in /src, /infrastructure, /tests,
    /docs, /scripts, /claude.
  - Layer purity ($7), Policy ($8), Determinism ($9), Events ($10),
    Audit ($11), Failure ($12) all apply.
  - No remediation patches applied; produce the patch list only.
  - Any newly discovered guard rule → claude/new-rules/ per $1c (do
    not silently extend an existing guard).
  - Boundary scope: R1–R11 from §2.4 of the opening pack.

EXECUTION STEPS:
  1. T-A Inventory — enumerate every canonical boundary in §2.5 scope
     and write the inventory table.
  2. T-B Probe matrix — define probes (commands or grep predicates) for
     each boundary R1–R11. Each probe must declare its expected PURE
     shape.
  3. T-C Probe execution — run every probe; capture verbatim raw
     output.
  4. T-D Classification — PURE / DRIFTED / VIOLATING per probe with a
     one-line justification.
  5. T-E Severity triage — S0/S1/S2/S3 for every non-PURE finding per
     $16.
  6. T-F Patch list — file path + diff intent + acceptance test per
     non-PURE finding. No code changes applied.
  7. T-G New-rules capture — if any guard rule is newly discovered,
     write claude/new-rules/{YYYYMMDD-HHMMSS}-guards.md per $1c with
     CLASSIFICATION, SOURCE, DESCRIPTION, PROPOSED_RULE, SEVERITY.
  8. T-H Final report — write claude/audits/boundary-purity.audit.md
     bundling inventory, probe matrix, raw evidence, classifications,
     severities, patch list, and explicit terminal status.

OUTPUT FORMAT:
  - Single audit artifact: claude/audits/boundary-purity.audit.md
  - Optional: claude/new-rules/{YYYYMMDD-HHMMSS}-guards.md
  - Structured failure report on any halt ($12: STATUS / STAGE /
    REASON / ACTION_REQUIRED).

VALIDATION CRITERIA:
  - All nine acceptance criteria from §2.9 / §4 satisfied.
  - Terminal status one of: PASS / FAIL / PARTIAL / BLOCKED / WAIVED.
  - Audit sweep ($1b) clean against the produced artifact.
  - No source/guard/script/README modification outside the explicitly
    named artifacts.
```

---

## 7. INITIAL STATUS

**OPEN** — workstream defined, tracked, and ready for execution. No
remediation performed. No source, guard, audit, script, or README file
modified by this opening pack. §5.1.2 enters the Phase 1.5 work queue
as the next mandatory structural hardening item after §5.1.1 PASS
(2026-04-08).
