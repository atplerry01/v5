# WBSM v3 Canonical Execution Rules (v1.0 LOCKED)

## Execution Flow ($1)
Policy Binding -> Guard Loading -> Integrity Validation -> Guard Validation -> Execution -> Audit Sweep -> Drift Capture -> Output Validation. Any stage failure = halt.

## Pre-Execution Guard Loading ($1a)
Before executing ANY prompt, read ALL guard files from `/claude/guards/`. Every `.guard.md` file MUST be loaded and its rules applied as validation constraints for the execution. Guard violations detected before or during execution = halt. Guards are non-negotiable and are never skipped, cached from memory, or summarized — they are read fresh each execution.

Guard discovery directive:
- Load every `*.guard.md` file under `claude/guards/**`, including the `claude/guards/domain-aligned/**` subtree.
- The directive form is canonical because the guard inventory grows over time (Phase 1.5 §5.1.x added several). Any static list in this document is an aid, not a limit — directory contents are authoritative.

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
- `guards/` — Pre-execution guard rules (loaded before every prompt execution per $1a; includes the `domain-aligned/` subtree).
- `audits/` — Post-execution audit definitions (run after every prompt execution per $1b). Frozen `*.audit.output.md` files and the `phase1-evidence/` and `sweeps/` subdirectories are HISTORICAL BASELINE.
- `new-rules/` — Captured new guard errors and drift rules discovered during execution per $1c. The `_archives/` subdirectory holds promoted/closed captures.
- `project-prompts/` — **Canonical prompt storage per $2.** This is the only canonical location for prompts; the `phase1/` and `phase2/` subdirectories hold per-phase prompts.
- `project-topics/`, `traceability/` — Supporting tracking artifacts.
- `agents/`, `prompts/`, `registry/`, `templates/`, `workflows/` — Supporting orchestration assets. Note: `prompts/` is a legacy supporting directory and is NOT the canonical prompt store; per $2, all new prompts go to `project-prompts/`.

All domain work must respect the classification > context > domain three-level nesting. Domain layer has zero external dependencies. Projections are strictly separated from domain.
