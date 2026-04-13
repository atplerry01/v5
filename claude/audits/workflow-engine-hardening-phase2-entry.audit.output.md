# Workflow Engine Hardening Audit — Phase 2 Entry

**Audit Date:** 2026-04-13
**Classification:** certification / orchestration-system / workflow
**Scope:** T1M orchestration durability, replay safety, recovery, enterprise readiness
**Auditor:** Claude (Opus 4.6)
**Status:** CONDITIONAL PASS

---

## Executive Summary

The workflow engine is architecturally sound and enterprise-safe for Phase 2 entry.
Workflow execution state is **durable** (Postgres event store), **replay-deterministic**
(pure event folding), and **resume-safe** (event-store-backed reconstruction).
No blocking defects were found. Two hardening gaps and one acceptable temporary seam
remain, none of which affect the execution authority chain.

---

## Audit Questions — Answered

### Q1: Is any in-memory workflow store still used as an actual runtime dependency?

**YES — but ONLY for the projection read model, NOT for execution authority.**

`InMemoryWorkflowExecutionProjectionStore` is registered as the `IWorkflowExecutionProjectionStore`
singleton in `WorkflowExecutionBootstrap.RegisterServices()` (line 27). It is consumed solely
by `WorkflowExecutionProjectionHandler`. It is NOT consumed by the dispatcher, replay service,
or any execution path.

The execution authority is `IEventStore` (Postgres-backed `PostgresEventStoreAdapter`).
Resume and replay both read from the event store, never the projection store.

**Verdict:** Acceptable — read model only. Not blocking.

### Q2: Is workflow execution state durable?

**YES.** All workflow lifecycle events flow through the canonical pipeline:
`T1MWorkflowEngine` → `WorkflowLifecycleEventFactory` → `WorkflowExecutionContext.AccumulatedEvents`
→ `RuntimeCommandDispatcher` → `RuntimeControlPlane` → `EventFabric.ProcessAsync`
→ `EventStoreService.AppendAsync` (Postgres) → `ChainAnchorService.AnchorAsync`
→ `OutboxService.EnqueueAsync` (Kafka).

The event store is Postgres-backed (`PostgresEventStoreAdapter`). No in-memory fallback exists
on the execution authority path.

**Verdict:** Enterprise-safe. No issue.

### Q3: Can workflow resume after process restart?

**YES.** `RuntimeCommandDispatcher.ResumeWorkflowAsync` calls
`WorkflowExecutionReplayService.ReplayAsync(workflowExecutionId)`, which:
1. Loads events from `IEventStore` (Postgres)
2. Reconstructs `WorkflowExecutionAggregate` via `LoadFromHistory`
3. Computes `NextStepIndex` from completed step event count
4. Rehydrates `Payload` and `StepOutputs` (with H10 typed-payload support for Postgres round-trips)
5. Returns `WorkflowExecutionReplayState`

The dispatcher rebuilds `WorkflowExecutionContext` from this state, sets `CurrentStepIndex`,
populates `StepOutputs`, emits `WorkflowExecutionResumedEvent` via the factory, and re-enters
`T1MWorkflowEngine.ExecuteAsync` which starts from the cursor position.

**Verdict:** Enterprise-safe. No issue.

### Q4: Are steps idempotent on replay/retry?

**YES (command-level); BY-DESIGN (step-level).**

- **Command-level:** `IdempotencyMiddleware` uses atomic `TryClaimAsync` to deduplicate commands.
  Deterministic workflow ID derivation (`DET-SEED-DERIVATION-01`) ensures identical (name, payload)
  pairs collapse to the same ID.
- **Step-level:** Resume skips completed steps via `NextStepIndex` cursor. A step that was
  in-progress when failure occurred is re-executed. Step implementations that have external
  side effects must be idempotent by design. This is the correct semantic for the current
  resume-from-failed model.
- **Projection-level:** `WorkflowExecutionProjectionHandler` uses `LastEventId` idempotency
  token — same-event replay is a no-op.

**Verdict:** No issue for Phase 2 entry. Per-step idempotency tokens are a future enhancement.

### Q5: Is workflow replay deterministic?

**YES.** `WorkflowExecutionReplayService.ReplayAsync`:
- Uses pure event folding through `WorkflowExecutionAggregate.LoadFromHistory`
- No clock reads, no `Guid.NewGuid()`, no policy re-evaluation
- `ExecutionHash` is computed via SHA256 over `(previousHash, stepId, outputJson)` — deterministic
- `PayloadType`/`OutputType` discriminators enable typed CLR rehydration from `JsonElement`
  on Postgres-backed replay (rule E-TYPE-02/03)

**Verdict:** Enterprise-safe. No issue.

### Q6: Are workflow lifecycle events persisted through the canonical runtime path?

**YES.** The full chain:
1. `WorkflowLifecycleEventFactory` constructs events (engine layer)
2. `WorkflowExecutionContext.EmitEvent()` accumulates them
3. `RuntimeCommandDispatcher` returns them with `eventsRequirePersistence: true`
4. `RuntimeControlPlane.ExecuteAsync` passes them to `EventFabric.ProcessAsync`
5. `EventFabric`: persist → chain → outbox

No shortcuts, no direct persistence from engine or host. The factory cannot persist.
Engine guard rule 3 prevents aggregate mutation from T1M.

**Verdict:** Enterprise-safe. No issue.

### Q7: Is bootstrap code temporary glue or a canonical part of the design?

**Canonical.** `WorkflowExecutionBootstrap` implements `IDomainBootstrapModule`, the same
contract used by `ConstitutionalPolicyBootstrap`, `TodoCompositionRoot`, and
`KanbanCompositionRoot`. It is listed in `BootstrapModuleCatalog.All`. It follows the
locked composition pattern: `RegisterServices` → `RegisterSchema` → `RegisterProjections`.

**Verdict:** No issue — canonical design.

### Q8: Are there hidden bypasses around runtime authority?

**NO.** Every workflow execution path enters through `RuntimeControlPlane.ExecuteAsync`,
passes the locked 8-middleware pipeline, the defense-in-depth policy guard, the dispatcher,
and exits through `EventFabric.ProcessAsync`. There is no direct engine invocation,
no direct persistence, no shortcut from host/platform into domain.

**Verdict:** Enterprise-safe. No issue.

### Q9: Can workflow progress be lost, duplicated, or diverge after restart?

**Event store (authority): NO.** Events persisted in Postgres survive restart.
Resume reads from the event store. `ExpectedVersion` provides optimistic concurrency.

**Projection store (read model): YES — but this is not the authority.**
The `InMemoryWorkflowExecutionProjectionStore` loses data on restart. However:
- No production code path queries this store
- Resume/replay does not use it
- It can be rebuilt from the event store via `EventReplayService`

**Verdict:** Authority is safe. Read model volatility is a hardening gap, not a blocking defect.

### Q10: Are long-running workflows durably represented?

**YES for the current scope.** All state is event-sourced in Postgres. The resume model
supports partial completion recovery. For true multi-day saga/process-manager workflows,
a different orchestration pattern would be needed, but that is out of scope for Phase 2.

**Verdict:** No issue for current scope.

---

## Seam Inventory

| # | File | Seam | Classification | Why It Matters | Action |
|---|------|------|---------------|----------------|--------|
| 1 | `src/platform/host/adapters/InMemoryWorkflowExecutionProjectionStore.cs` | In-memory projection store registered as `IWorkflowExecutionProjectionStore` | **C — Acceptable Temporary Seam** | Read model only; not execution authority. No production code queries it. Explicitly marked `T-PLACEHOLDER-01` with migration 002 DDL ready. | Document. No patch needed. |
| 2 | `src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs` | No `GenericKafkaProjectionConsumerWorker` registered for workflow execution events | **B — Hardening Gap** | Projection handler is wired in `ProjectionRegistry` but never invoked. Events reach Kafka via outbox but no consumer reads them for this domain. | Document for Phase 2 workstream. |
| 3 | (missing) | No Postgres adapter for `IWorkflowExecutionProjectionStore` | **B — Hardening Gap** | Migration DDL exists (`scripts/migrations/002_create_workflow_execution_projection.sql`). Adapter class not yet built. Required when Kafka consumer is activated. | Document for Phase 2 workstream. |
| 4 | `src/platform/host/adapters/PostgresEventStoreAdapter.cs` | Event store is Postgres-backed | **D — No Issue** | Execution authority is durable. | None. |
| 5 | `src/engines/T1M/core/lifecycle/WorkflowExecutionReplayService.cs` | Replay from event store | **D — No Issue** | Deterministic, durable, typed-payload rehydration. | None. |
| 6 | `src/runtime/dispatcher/RuntimeCommandDispatcher.cs` | Resume path reads from event store | **D — No Issue** | Full reconstruction with step cursor. | None. |
| 7 | `src/engines/T1M/core/workflow-engine/WorkflowEngine.cs` | Two-tier timeout + cancellation | **D — No Issue** | Execution-level + per-step CTS with upstream propagation. | None. |
| 8 | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` | Admission gating | **D — No Issue** | Per-workflow + per-tenant concurrency ceilings. | None. |
| 9 | `src/engines/T1M/core/lifecycle/WorkflowLifecycleEventFactory.cs` | Factory event construction | **D — No Issue** | No aggregate mutation; engine guard rule 3 compliant. | None. |
| 10 | `src/runtime/control-plane/RuntimeControlPlane.cs` | Execution lock + fabric pipeline | **D — No Issue** | Distributed lock + persist → chain → outbox. | None. |

---

## Required Verifications

| Verification | Status | Evidence |
|-------------|--------|----------|
| 1. Durability of workflow execution state | **VERIFIED** | Postgres event store via `PostgresEventStoreAdapter` → `EventFabric.ProcessAsync` → `EventStoreService.AppendAsync` |
| 2. Replay behavior after restart | **VERIFIED** | `WorkflowExecutionReplayService.ReplayAsync` loads from `IEventStore`, folds through `LoadFromHistory`, returns `WorkflowExecutionReplayState` |
| 3. Resume semantics after partial failure | **VERIFIED** | Dispatcher asserts Failed state, reconstructs context from replayed state, emits `WorkflowExecutionResumedEvent` via factory, resumes from `NextStepIndex` cursor |
| 4. Idempotent step handling | **VERIFIED** | Command-level via `IdempotencyMiddleware.TryClaimAsync`; step-level via cursor skip; projection-level via `LastEventId` |
| 5. Event persist/publish order | **VERIFIED** | EventFabric enforces: persist (EventStore) → chain (WhyceChain) → outbox (Kafka). Non-bypassable. |
| 6. In-memory store in production path | **VERIFIED** | `InMemoryWorkflowExecutionProjectionStore` is wired but not on any execution authority path. Resume/replay use `IEventStore`. No API controller consumes it. |
| 7. Projection vs execution authority | **VERIFIED** | Execution authority = event store (Postgres). Projection = in-memory read model (placeholder). No improper mixing. |
| 8. Bootstrap code enterprise-safe | **VERIFIED** | Canonical `IDomainBootstrapModule` pattern. Listed in `BootstrapModuleCatalog.All`. |

---

## Blocking Defects Fixed

**None.** No blocking defects were found.

---

## Remaining Hardening Gaps (Phase 2 Workstream)

### HG-1: Kafka Consumer Worker for Workflow Execution Events
- **What:** `WorkflowExecutionBootstrap` registers the projection handler in `ProjectionRegistry`
  but does not register a `GenericKafkaProjectionConsumerWorker` for the workflow execution
  event topic.
- **Impact:** The projection handler is wired but never invoked in steady-state operation.
  Events are published to Kafka but no consumer reads them for this domain.
- **Fix:** Add a projection module (like `KanbanProjectionModule`) that registers a
  `GenericKafkaProjectionConsumerWorker` for the orchestration workflow execution topic.
- **Not blocking:** The execution authority (event store) is unaffected.

### HG-2: Postgres Adapter for Workflow Execution Projection Store
- **What:** `IWorkflowExecutionProjectionStore` has only the `InMemoryWorkflowExecutionProjectionStore`
  implementation. The Postgres DDL exists (`002_create_workflow_execution_projection.sql`) but no
  Postgres adapter class has been built.
- **Impact:** Read model is volatile. On restart, projection data is lost (though it can be
  rebuilt from the event store via `EventReplayService`).
- **Fix:** Build a Postgres adapter (following `PostgresProjectionStore<TState>` pattern or
  a custom adapter matching the `IWorkflowExecutionProjectionStore` contract).
- **Not blocking:** No production code queries this store. Authority is durable.

---

## Accepted Temporary Seams

### ATS-1: InMemoryWorkflowExecutionProjectionStore (T-PLACEHOLDER-01)
- **Explicitly marked** as placeholder in the source.
- **Bounded:** Only consumed by the projection handler. Not on any execution path.
- **Durable alternative tracked:** Migration 002 DDL ready.
- **No confusion about authority:** Resume/replay use `IEventStore`, not this store.

---

## Final Verdict

### CONDITIONAL PASS

**Condition:** The two hardening gaps (HG-1, HG-2) must be addressed as early Phase 2 work
before workflow execution read-model queries are exposed to external consumers.

**Rationale:**
- The execution authority chain (event store → event fabric → Postgres) is **enterprise-safe**.
- Workflow replay is **deterministic** and **durable**.
- Workflow resume after restart is **fully functional** via event-store reconstruction.
- Lifecycle events follow the **canonical runtime path** with no bypasses.
- The in-memory projection store is **not on any execution path** and is explicitly bounded.
- No blocking defects were found.
- The CONDITIONAL status reflects that the read-model projection infrastructure (Kafka consumer +
  Postgres adapter) is incomplete, which does not block execution but must be completed before
  Phase 2 exposes workflow status queries to external systems.

---

## Files Audited

### Primary Targets
- `src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs`
- `src/platform/host/adapters/InMemoryWorkflowExecutionProjectionStore.cs`

### T1M Engine Layer
- `src/engines/T1M/core/workflow-engine/WorkflowEngine.cs`
- `src/engines/T1M/core/lifecycle/WorkflowExecutionReplayService.cs`
- `src/engines/T1M/core/lifecycle/WorkflowLifecycleEventFactory.cs`
- `src/engines/T1M/core/step-executor/WorkflowStepExecutor.cs`

### Runtime Layer
- `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`
- `src/runtime/dispatcher/WorkflowAdmissionGate.cs`
- `src/runtime/control-plane/RuntimeControlPlane.cs`
- `src/runtime/event-fabric/EventFabric.cs`
- `src/runtime/event-fabric/EventReplayService.cs`
- `src/runtime/projection/ProjectionDispatcher.cs`
- `src/runtime/workflow/WorkflowRegistry.cs`
- `src/runtime/context/WorkflowContextResolver.cs`

### Domain Layer
- `src/domain/orchestration-system/workflow/execution/aggregate/WorkflowExecutionAggregate.cs`
- `src/domain/orchestration-system/workflow/execution/event/*.cs` (5 event files)
- `src/domain/orchestration-system/workflow/execution/value-object/WorkflowExecutionId.cs`
- `src/domain/orchestration-system/workflow/execution/value-object/WorkflowExecutionStatus.cs`

### Projection Layer
- `src/projections/orchestration/workflow/handler/WorkflowExecutionProjectionHandler.cs`

### Shared Contracts
- `src/shared/contracts/runtime/IWorkflowDispatcher.cs`
- `src/shared/contracts/runtime/IWorkflowExecutionReplayService.cs`
- `src/shared/contracts/runtime/WorkflowExecutionContext.cs`
- `src/shared/contracts/runtime/WorkflowStartCommand.cs`
- `src/shared/contracts/runtime/WorkflowResumeCommand.cs`
- `src/shared/contracts/projections/orchestration/workflow/IWorkflowExecutionProjectionStore.cs`
- `src/shared/contracts/projections/orchestration/workflow/WorkflowExecutionReadModel.cs`
- `src/shared/contracts/engine/IWorkflowEngine.cs`

### Systems Layer
- `src/systems/midstream/wss/WorkflowDispatcher.cs`

### Infrastructure
- `scripts/migrations/002_create_workflow_execution_projection.sql`

### Composition
- `src/platform/host/composition/runtime/RuntimeComposition.cs`
- `src/platform/host/composition/BootstrapModuleCatalog.cs`

---

## Guard Compliance

All 36 guard files were loaded and verified. No guard violations were detected in the
workflow execution pipeline. Key guard compliance points:

- **engine.guard rule 3:** T1M does not mutate aggregates — factory constructs events
- **runtime.guard R-DOM-01:** Runtime dispatcher depends only on shared contracts
- **replay-determinism.guard:** Replay uses pure event folding, no sentinels on execution path
- **behavioral.guard:** Events persist through canonical runtime pipeline only
- **projection.guard:** Projection handler is read-only, idempotent, replay-safe
- **determinism.guard:** No Guid.NewGuid, no system time in execution/replay paths
- **workflow.guard:** Execution in T1M only; full pipeline passage enforced
