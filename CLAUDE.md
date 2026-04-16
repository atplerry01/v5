# WBSM v3 Canonical Execution Rules (v1.0 LOCKED)

## Execution Flow ($1)
Policy Binding -> Guard Loading -> Integrity Validation -> Guard Validation -> Execution -> Audit Sweep -> Drift Capture -> Output Validation. Any stage failure = halt.

## Pre-Execution Guard Loading ($1a)
Before executing ANY prompt, read ALL guard files from `/claude/guards/`. Every `.guard.md` file MUST be loaded and its rules applied as validation constraints for the execution. Guard violations detected before or during execution = halt. Guards are non-negotiable and are never skipped, cached from memory, or summarized — they are read fresh each execution.

**Canonical 4-layer guard model (LOCKED 2026-04-14, rule GUARD-LAYER-MODEL-01):**

The guard system has exactly four canonical layers, realised as four files directly under `claude/guards/`:

1. `constitutional.guard.md` — WHYCEPOLICY authority, determinism, deterministic identifiers (HSID), hash determinism, replay determinism.
2. `runtime.guard.md` — execution pipeline & ordering, engine, projection, prompt-container, dependency graph & layer boundaries, contracts boundary, **code quality enforcement** (clean-code, no-dead-code, stub-detection), **test & E2E validation**.
3. `domain.guard.md` — business truth: layer purity, domain structure, classification-suffix, DTO naming, behavioral, structural, and inlined domain-aligned guards (economic, governance, identity, observability, workflow).
4. `infrastructure.guard.md` — platform, systems, kafka, config safety, composition loader, program composition.

**Subsystem note:** Quality enforcement and test/E2E validation are subsystems of Runtime enforcement — they are NOT separate top-level guards. No fifth canonical guard file may exist. No subdirectories (including any reintroduction of `domain-aligned/`) may exist under `claude/guards/`.

Guard discovery directive:
- Load every `*.guard.md` file under `claude/guards/` (four canonical files only). Presence of any additional `.guard.md` file or any subdirectory is drift per `GUARD-LAYER-MODEL-01` and MUST be captured under `/claude/new-rules/` per $1c.

## Post-Execution Audit Sweep ($1b)
After executing ANY prompt, execute the audit prompts from `/claude/audits/` against the output and any modified files. The audit sweep checks for drift, violations, and structural integrity. Any drift or violation found MUST be fixed inline before the execution is considered complete.

Audit discovery directive:
- Run every `*.audit.md` file directly under `claude/audits/**`. Directory contents are authoritative.
- `*.audit.output.md` files and the `claude/audits/phase1-evidence/` and `claude/audits/sweeps/` subdirectories are HISTORICAL BASELINE / ARCHIVAL RECORD and are NOT re-run by the post-execution sweep — they are frozen evidence from prior passes.

## New Rules Capture ($1c)
When the audit sweep or guard validation discovers NEW guard errors or drift rules not already covered by existing guard/audit files, capture them in: `/claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md` where `{type}` is one of: the domain classification, `guards`, or `audits`. These files serve as a backlog for promoting into canonical guards/audits. Format: structured finding with CLASSIFICATION, SOURCE, DESCRIPTION, PROPOSED_RULE, SEVERITY.

## Prompt Storage ($2)
`/claude/project-prompts/{YYYYMMDD-HHMMSS}-{classification}-{topic}.md`. Never skip, never overwrite, never store elsewhere.

## Prompt Structure ($3)
Mandatory sections: TITLE, CONTEXT, OBJECTIVE, CONSTRAINTS, EXECUTION STEPS, OUTPUT FORMAT, VALIDATION CRITERIA.

## Classification ($4)
Every prompt declares classification + context + domain. Unclear classification = STOP and ask.

## Anti-Drift ($5)
No architecture changes, no new patterns, no renaming, no file moves, no inference of missing components.

## File System ($6)
Only operate in: /src, /infrastructure, /tests, /docs, /scripts, /claude.

## Layer Purity ($7)
Domain = zero dependencies. Engine = stateless, events only. Runtime = only layer that persists/publishes/anchors.

## Policy ($8)
ALL operations through WHYCEPOLICY. No bypass. NoPolicyFlag = anomaly.

## Determinism ($9)
No Guid.NewGuid, no system time (use IClock), deterministic IDs, SHA256 hashing.

## Events ($10)
All state changes emit events. Naming: {Domain}{Action}Event.

## Audit ($11)
Every execution produces: Execution Trace, Policy Decision, Guard Result, Chain Anchor.

## Failure ($12)
STOP, no partial completion, structured failure report (STATUS/STAGE/REASON/ACTION_REQUIRED).

## Violations ($16)
S0=system-breaking, S1=architectural, S2=structural, S3=formatting.

## Priority ($15)
WHYCEPOLICY > WBSM v3 Architecture > This Rule Set > Prompt Instructions.

## Phase 2-4 Pipeline Execution ($17) — LOCKED 2026-04-15, rule PIPELINE-EXEC-01
All Phase 2, Phase 3, and Phase 4 prompt executions run from `/pipeline/**`. This is the canonical execution pipeline for these phases and is non-negotiable.

**Canonical pipeline contract:**
0. **Guard Pre-Load (MANDATORY)** — Before executing ANY file under `/pipeline/**`, load and read every `*.guard.md` file under `/claude/guards/**` per $1a. If any instruction in the pipeline file (context, prompt, audit, or fix) contradicts a guard rule, **PAUSE immediately and ask the user for correction** — do not attempt to reconcile silently, do not proceed, do not pick a side. Resume only after the user resolves the contradiction.
1. **Input** — Read execution input from `/pipeline/execution_context.md`. This is the authoritative input; never substitute another source.
2. **Execute** — Run the generic execution prompt at `/pipeline/generic-prompt.md` against that context.
3. **Pipeline Audit** — After execution completes, run the audit defined in `/pipeline/generic-audit.md` against the output and modified files.
4. **Pipeline Fix** — For every pipeline audit finding, run `/pipeline/generic-audit-fix-prompt.md` inline until the pipeline audit is clean.
5. **System Audit Sweep (MANDATORY)** — Once the pipeline audit is clean, run the full system audit sweep at `/claude/audits/**` per $1b against the output and modified files. Any drift or violation from the system sweep MUST also be fixed inline before the execution is considered complete.

Any deviation (skipping a stage, swapping prompt files, partial audit, unresolved findings, skipping guard pre-load, skipping the post-fix system audit) = halt per $12. This pipeline is additive to $1/$1a/$1b — guard loading ($1a) and the global audit sweep ($1b) are mandatory entry and exit points of every pipeline run.

---

# Project Architecture

Source architecture under `src/`:
- `domain/` — Pure DDD bounded contexts, three-level nesting: `{classification}/{context}/{domain}/`
- `engines/` — Engine layer
- `platform/` — Platform APIs
- `projections/` — CQRS read models (consumes events, produces read models)
- `runtime/` — Runtime execution support (includes internal projection for routing/orchestration/indexing)
- `shared/` — Shared kernel
- `systems/` — System integrations

Claude orchestration under `claude/`:
- `guards/` — Pre-execution guard rules (loaded before every prompt execution per $1a). **Canonical 4-layer model (LOCKED):** `constitutional.guard.md`, `runtime.guard.md`, `domain.guard.md`, `infrastructure.guard.md`. No subdirectories; domain-aligned guards are inlined as subsections of `domain.guard.md`.
- `audits/` — Post-execution audit definitions (run after every prompt execution per $1b). Frozen `*.audit.output.md` files and the `phase1-evidence/` and `sweeps/` subdirectories are HISTORICAL BASELINE.
- `new-rules/` — Captured new guard errors and drift rules discovered during execution per $1c. The `_archives/` subdirectory holds promoted/closed captures.
- `project-prompts/` — **Canonical prompt storage per $2.** This is the only canonical location for prompts; the `phase1/` and `phase2/` subdirectories hold per-phase prompts.
- `project-topics/`, `traceability/` — Supporting tracking artifacts.
- `agents/`, `prompts/`, `registry/`, `templates/`, `workflows/` — Supporting orchestration assets. Note: `prompts/` is a legacy supporting directory and is NOT the canonical prompt store; per $2, all new prompts go to `project-prompts/`.

All domain work must respect the classification > context > domain three-level nesting. Domain layer has zero external dependencies. Projections are strictly separated from domain.
