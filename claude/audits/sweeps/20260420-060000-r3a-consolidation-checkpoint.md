# R3.A Consolidation Checkpoint — Workflow Runtime Reliability Subsystem Complete

**Date:** 2026-04-20
**Scope:** Mark-point for the R3.A (Workflow Runtime Reliability) work stream. Five closures shipped in sequence (R3.A.1 → R3.A.2 → R3.A.5 → R3.A.4 → R3.A.3) without an intervening commit; this audit-sweep snapshot establishes a coherent review unit before proceeding into R3.A.6 (Human-approval wait-state) or R3.B (External side-effect control).

**Test envelope at checkpoint:** 373 passing, 3 pre-existing unrelated failures (`CurrencyPairTests.Constructor_Rejects_WhenBaseCodeEmpty`, `…_WhenQuoteCodeEmpty`, `Engines_do_not_call_Resume_on_workflow_aggregate_directly`). Build clean — 0 warnings, 0 errors.

---

## 1. R3.A closures shipped this session

| Order | Slice | Rows flipped | Arch tests added |
|---|---|---|---|
| R3.A.1 | Workflow observability — `workflow.execution.duration` / `workflow.step.duration` / `workflow.execution.completed` on `Whycespace.Workflow` meter; 5-outcome vocabulary | §13 "Workflow observability" PARTIAL → PRESENT; §13 "Workflow timeout handling" PARTIAL → PRESENT (mislabeled in prior survey) | 2 |
| R3.A.2 | Step retry handling — bounded retry loop with exponential backoff; `workflow.step.retry_attempts` counter; `StepRetryMaxAttempts` / `StepRetryBaseBackoffMs` / `StepRetryMaxBackoffMs` options | §13 "Workflow retry handling" PARTIAL → PRESENT | 1 |
| R3.A.5 | Step-exception classification — `WorkflowStepFailureClassifier` maps BCL exception types to Retryable/Terminal; Terminal fast-fails; `workflow.step.terminal_failures` counter | §13 "Exception-path handling" PARTIAL → PRESENT | 2 |
| R3.A.4 | Cancellation lifecycle event — `WorkflowExecutionCancelledEvent` + `Cancelled` status (ordinal 4); engine emits before re-throwing OCE; schema + Apply + factory all wired | §13 "Workflow cancellation" PARTIAL → PRESENT | 2 |
| R3.A.3 | Suspend lifecycle event — `WorkflowExecutionSuspendedEvent` + `Suspended` status (ordinal 5, NON-terminal); factory with Running-only guard; `Resumed` guard extended to accept Failed OR Suspended | §13 "Workflow suspend / resume" PARTIAL → PRESENT | 2 |

**Totals across R3.A:**
- 6 §13 rows flipped PARTIAL → PRESENT (1 mislabel correction + 5 actual closures)
- 9 new architecture tests
- 17 new canonical guard rules (across 5 drift-rule files dated 2026-04-20)
- 2 status enum values appended to `WorkflowExecutionStatus` (append-only discipline preserved)
- 2 new domain event types (Cancelled, Suspended)
- 4 new meter signals on `Whycespace.Workflow` (execution.duration, step.duration, execution.completed, step.retry_attempts, step.terminal_failures)

**§13 post-checkpoint status:** 17 of 18 rows PRESENT; only Human-approval wait-state (ABSENT) remains — and R3.A.3 shipped the suspend infrastructure it requires.

---

## 2. Guard + audit rules promoted this session

All under "R3.A Workflow Runtime Reliability" section in `runtime.guard.md`:

**R3.A.1 (Observability) — 3 rules:**
- R-WORKFLOW-OBSERVABILITY-01
- R-WORKFLOW-OBSERVABILITY-COMPLETION-COUNTER-01
- R-WORKFLOW-OBSERVABILITY-DETERMINISM-NOTE-01

**R3.A.2 (Retry) — 4 rules:**
- R-WORKFLOW-STEP-RETRY-01
- R-WORKFLOW-STEP-RETRY-NON-RETRYABLE-EXCLUSION-01
- R-WORKFLOW-STEP-RETRY-OBSERVABILITY-01
- R-WORKFLOW-STEP-RETRY-REPLAY-DETERMINISM-01

**R3.A.5 (Classification) — 4 rules:**
- R-WORKFLOW-STEP-EXCEPTION-CLASSIFICATION-01
- R-WORKFLOW-STEP-EXCEPTION-ENGINE-WIRING-01
- R-WORKFLOW-STEP-EXCEPTION-COUNTER-01
- R-WORKFLOW-STEP-EXCEPTION-REPLAY-DETERMINISM-01

**R3.A.4 (Cancellation) — 5 rules:**
- R-WORKFLOW-CANCELLATION-EVENT-01
- R-WORKFLOW-CANCELLATION-FACTORY-01
- R-WORKFLOW-CANCELLATION-ENGINE-EMISSION-01
- R-WORKFLOW-CANCELLATION-SCHEMA-REGISTRATION-01
- R-WORKFLOW-CANCELLATION-REPLAY-DETERMINISM-01

**R3.A.3 (Suspend) — 6 rules:**
- R-WORKFLOW-SUSPEND-EVENT-01
- R-WORKFLOW-SUSPEND-FACTORY-01
- R-WORKFLOW-SUSPEND-RESUME-GUARD-01
- R-WORKFLOW-SUSPEND-SCHEMA-REGISTRATION-01
- R-WORKFLOW-SUSPEND-REPLAY-DETERMINISM-01

All rules have matching audit entries in `runtime.audit.md` R3.A section.

---

## 3. Workflow execution status state machine (post-R3.A)

```
             NotStarted
                 │ (StartedEvent)
                 ↓
             Running  ←───────────┐
             │  │                 │
  (Step)     │  │  (Suspended)    │ (Resumed — from Failed OR Suspended)
             │  │        │        │
             │  └─→ Suspended ────┤
             │                    │
  (Failed)   ├──→ Failed ─────────┘ (resumable, since pre-R3.A.3)
             │
  (Cancel)   ├──→ Cancelled   (TERMINAL — R3.A.4)
             │
  (Complete) └──→ Completed   (TERMINAL)
```

**Resumable states:** `Failed`, `Suspended`.
**Terminal states:** `Completed`, `Cancelled`.
**Append-only enum ordinals:** NotStarted=0, Running=1, Completed=2, Failed=3, Cancelled=4, Suspended=5.

---

## 4. Accumulated working-tree state (UNCOMMITTED)

Significant accumulation since the last commit (`7435287f update`). The consolidation checkpoint flags this explicitly rather than silently carrying a large diff into R3.A.6 / R3.B.

**Modified files in working tree:** ~80 (across `src/` + `claude/`).
**Untracked new `.cs` files:** 42 (all R1 / R2 / R3 new artifacts — RuntimeFailureCategory, IRetryExecutor, ICircuitBreaker, KafkaRetryEscalator, WorkflowStepFailureClassifier, WorkflowExecutionCancelledEvent, WorkflowExecutionSuspendedEvent, etc.).
**Untracked drift-rule sources:** 20 under `claude/new-rules/` (2026-04-19 + 2026-04-20 series).
**Untracked sweep outputs:** 2 pre-existing + this one.

**Commit discipline:** no commits have been made during this session. Per CLAUDE.md guidance, commits require explicit user authorization. The consolidation checkpoint is a review-ready mark-point; the user can choose to:
- Commit the current state as a coherent R3.A-complete checkpoint.
- Continue to R3.A.6 or R3.B and defer the commit.
- Request a staged commit split (e.g. R1 separate from R2 separate from R3).

---

## 5. Pre-existing issues NOT caused by R3.A

Two classes of issue observed during the session that are NOT from R3.A work:

1. **Content-system streaming aggregate references** — a handful of untracked `*Specification.cs` files in `src/domain/content-system/streaming/` reference `StreamAccessAggregate` / `StreamSessionAggregate` and cause intermittent build failures that clear with a domain-only rebuild. Appears to be a partial / in-progress migration from another workstream. Not touched by R3.A work; flagged here for the next consolidation session.

2. **3 pre-existing unrelated unit-test failures** — unchanged across the whole R3.A sequence:
   - `CurrencyPairTests.Constructor_Rejects_WhenBaseCodeEmpty`
   - `CurrencyPairTests.Constructor_Rejects_WhenQuoteCodeEmpty`
   - `Engines_do_not_call_Resume_on_workflow_aggregate_directly` — the "do not call .Resume()" pattern check is a known architectural-test defect that references the old aggregate Resume method (removed in phase1.6-S1.2); two `Resume` call sites in enforcement engine code (`ResumeSystemLockHandler`, `ResumeRestrictionHandler`) are the hits. Not related to R3.A suspend/resume — these are domain-specific `Resume` methods on different aggregates.

---

## 6. Next-step options

| Option | Scope | Effort |
|---|---|---|
| **R3.A.6 Human-approval wait-state** | Compose on R3.A.3 suspend: step-level marker / attribute that short-circuits to Suspended awaiting a named external signal. Closes the last §13 ABSENT row. | 1 session |
| **R3.B External side-effect control (§15)** | Second R3 subsystem. Duplicate side-effect prevention, outbox-based external calls, safe third-party timeout handling. Larger scope; needs its own planning sweep. | 4-6 sessions |
| **Commit checkpoint** | Stage + commit R1 / R2 / R3.A accumulated work in reviewable chunks. Requires explicit user authorization. | 1 session (if split) |
| **Content-system streaming cleanup** | Resolve the untracked `*Aggregate.cs` / `*Specification.cs` references causing intermittent build failures. Unrelated to runtime work; could be delegated. | 30 min - 1 session |

Recommended: R3.A.6 while the R3.A context is warm. Or a commit checkpoint to establish a reviewable baseline before R3.B.
