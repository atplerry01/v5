# DOMAIN INTEGRITY AUDIT — OUTPUT

**Date:** 2026-04-08
**Sweep ID:** 20260408-132840
**Scope:** src/ (excluding tests/)
**Verdict:** PASS (2 × S2, 0 × S0/S1)

---

## FINDINGS

### DI-S2-01 — Duplicate Todo validation in engine step
- **File:** src/engines/T1M/steps/todo/ValidateIntentStep.cs:14-29
- **Severity:** S2
- **Description:** ValidateIntentStep checks `Title` and `UserId` required ("Title is required", "UserId is required"). Identical invariant already enforced in TodoAggregate.Create() (src/domain/operational-system/sandbox/todo/aggregate/TodoAggregate.cs:15,24) via TodoErrors (src/domain/operational-system/sandbox/todo/error/TodoErrors.cs:5).
- **Risk:** Maintenance drift — two sources of truth for the same invariant.
- **Fix:** Remove ValidateIntentStep or convert to a pass-through. Domain aggregate is authoritative.

### DI-S2-02 — Status recalculated in API controller
- **File:** src/platform/api/controllers/TodoController.cs:89
- **Severity:** S2
- **Description:** Controller computes `lastEventType == "TodoCompletedEvent" ? "completed" : "active"`. Same calc lives in TodoProjectionHandler (src/projections/operational-system/sandbox/todo/TodoProjectionHandler.cs:63,77).
- **Risk:** Projection is authoritative read model; controller bypassing it defeats CQRS separation.
- **Fix:** Read the projection's stored `Status` field directly; remove the inline calc.

---

## CLEAN AREAS (verified)

- Domain layer: zero infra references (System.IO, EF, Kafka, ILogger, Npgsql)
- Domain layer: zero `DateTime.UtcNow` / `Guid.NewGuid()` (all via IClock / IIdGenerator)
- Three-level nesting (classification > context > domain) intact
- All aggregates enforce invariants in constructors / state-change methods
- Events past-tense, immutable, sealed records
- Prior domain.audit.output.md findings remain resolved (WorkflowExecution D2, Policy Decision events)

## CROSS-REFERENCE
- claude/audits/domain.audit.output.md — prior baseline, no regressions