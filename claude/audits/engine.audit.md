# ENGINE AUDIT — WBSM v3

```
AUDIT ID:       ENGINE-AUDIT-v2
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit the engine layer to ensure strict compliance with WBSM v3 tiered engine architecture. Engines are stateless execution units organized into five tiers (T0U through T4A). They must import from domain, never define domain models, maintain strict inter-tier isolation, and contain no persistence logic.

This audit MUST detect:

* Tier misclassification
* Statefulness in engines
* Persistence logic in engines
* Engine-to-engine imports
* Domain model definition in engines (must import only)
* Incorrect tier placement of functionality

---

## SCOPE

```
src/engines/T0U/  -> Utility/Policy engine tier
src/engines/T1M/  -> Middleware/Workflow engine tier
src/engines/T2E/  -> Execution/Domain engine tier
src/engines/T3I/  -> Intelligence engine tier
src/engines/T4A/  -> Access/Adapter engine tier
```

Excluded: `src/domain/`, `src/runtime/`, `src/systems/`, `src/platform/`, `infrastructure/`

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Engine persistence attempt, engine publishing event externally, engine calling infra, runtime bypass, engine-to-engine import, domain model definition | Blocks deployment |
| HIGH | Stateful engine, tier misplacement, missing domain import | Must fix before merge |
| MEDIUM | Non-standard naming, missing interface contract | Fix within sprint |
| LOW | Missing documentation, cosmetic deviation | Fix at convenience |

---

## ENGINE TIER REFERENCE

| Tier | Name | Responsibility | Invoked By |
|------|------|----------------|------------|
| T0U | Utility/Policy | Policy evaluation, validation, pre-flight checks | Runtime (before T1M/T2E) |
| T1M | Middleware/Workflow | Workflow orchestration, step execution, saga coordination | Runtime (after T0U) |
| T2E | Execution/Domain | Domain command execution, aggregate operations | Runtime (via T1M or direct) |
| T3I | Intelligence | Analytics, ML inference, recommendation | Runtime (async) |
| T4A | Access/Adapter | External system integration, API gateway | Runtime (outbound) |

---

## GLOBAL RULE: PROJECTION LAYER AUTHORITY

* `src/projections/` = DOMAIN PROJECTION LAYER (CQRS READ MODELS)
* `src/runtime/projection/` = INTERNAL EXECUTION SUPPORT ONLY

MANDATORY:

* Domain projections:
  * consume EVENTS only
  * produce READ MODELS only
  * exposed via platform APIs
* Runtime projections:
  * NOT exposed externally
  * support execution only (routing, orchestration, indexing)

---

## AUDIT DIMENSIONS

### EDIM-01: Tier Classification

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | Only T0U/T1M/T2E/T3I/T4A directories exist under `src/engines/` | CRITICAL |
| CHECK-01.2 | No engine code exists outside tier directories | CRITICAL |
| CHECK-01.3 | Policy evaluation logic resides only in T0U | HIGH |
| CHECK-01.4 | Workflow orchestration logic resides only in T1M | HIGH |
| CHECK-01.5 | Domain execution logic resides only in T2E | HIGH |
| CHECK-01.6 | Intelligence/analytics logic resides only in T3I | HIGH |
| CHECK-01.7 | External adapter logic resides only in T4A | HIGH |

### EDIM-02: Statelessness

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | No static mutable fields in engine classes | CRITICAL |
| CHECK-02.2 | No instance-level state carried between invocations | HIGH |
| CHECK-02.3 | Engine classes are registered as transient or scoped (not singleton with state) | HIGH |
| CHECK-02.4 | No in-memory caching of domain state in engines | HIGH |

### EDIM-03: No Persistence / No Infra in Engines (S0 CRITICAL — Emit-Only Rule)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | No `DbContext`, `IDbConnection`, or ORM references in engines | CRITICAL |
| CHECK-03.2 | No direct SQL queries in engine code | CRITICAL |
| CHECK-03.3 | No file system read/write operations in engines | CRITICAL |
| CHECK-03.4 | No direct Redis/cache/Kafka/HTTP/infra calls in engines | CRITICAL |
| CHECK-03.5 | T2E engines use ONLY `context.EmitEvents()` — no `context.Save()`, `_repository.Save()`, `SaveChanges()`, or any direct persistence call | CRITICAL |
| CHECK-03.6 | T2E engines do NOT inject or hold `IAggregateStore`, `IRepository`, or any persistence interface directly — aggregate access is exclusively through `EngineContext.LoadAggregate()` and `EngineContext.EmitEvents()` | CRITICAL |
| CHECK-03.7 | T2E engines do NOT publish events directly — no `IEventBus.Publish()`, `IEventPublisher`, Kafka produce, or outbox writes. Runtime is the sole publisher | CRITICAL |
| CHECK-03.8 | T2E engines do NOT anchor to WhyceChain — no `ChainBlock` creation or chain service calls. Runtime is the sole anchor | CRITICAL |
| CHECK-03.9 | Engines do NOT call any infrastructure adapter — no Redis, Kafka, HTTP, gRPC, SMTP, or cloud SDK calls. All infra interaction is runtime's responsibility | CRITICAL |

### EDIM-04: Engine-to-Engine Isolation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | T0U has no imports from T1M/T2E/T3I/T4A | CRITICAL |
| CHECK-04.2 | T1M has no imports from T0U/T2E/T3I/T4A (except via runtime dispatch) | CRITICAL |
| CHECK-04.3 | T2E has no imports from T0U/T1M/T3I/T4A | CRITICAL |
| CHECK-04.4 | T3I has no imports from T0U/T1M/T2E/T4A | CRITICAL |
| CHECK-04.5 | T4A has no imports from T0U/T1M/T2E/T3I | CRITICAL |
| CHECK-04.6 | No shared mutable state between engine tiers | CRITICAL |

### EDIM-05: Domain Import Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | Engines import domain aggregates from `src/domain/` | HIGH |
| CHECK-05.2 | Engines import domain events from `src/domain/` | HIGH |
| CHECK-05.3 | Engines import domain value objects from `src/domain/` | HIGH |
| CHECK-05.4 | Engines NEVER define their own aggregate classes | CRITICAL |
| CHECK-05.5 | Engines NEVER define their own domain event classes | CRITICAL |
| CHECK-05.6 | Engines NEVER define their own domain value objects | CRITICAL |
| CHECK-05.7 | Engine-specific DTOs are clearly separated from domain models | MEDIUM |

### EDIM-06: Correct Tier Placement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | WHYCEPOLICY bindings are in T0U only | HIGH |
| CHECK-06.2 | Saga/workflow step handlers are in T1M only | HIGH |
| CHECK-06.3 | Aggregate command handlers are in T2E only | HIGH |
| CHECK-06.4 | ML model invocations are in T3I only | HIGH |
| CHECK-06.5 | External HTTP/gRPC clients are in T4A only | HIGH |

### EDIM-07: Engine Contract Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | Each engine tier exposes a defined interface contract | HIGH |
| CHECK-07.2 | Engine interfaces are defined in shared layer, not in engine tier | HIGH |
| CHECK-07.3 | Engine implementations are internal (not publicly accessible except via interface) | MEDIUM |
| CHECK-07.4 | Engine methods accept commands/queries and return results (no side-channel communication) | HIGH |

### EDIM-08: EngineContext Surface Validation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | `EngineContext` exposes only `LoadAggregate<T>()`, `EmitEvents<T>()`, `EmittedEvents`, and `ValidateAsync()` as public methods/properties | CRITICAL |
| CHECK-08.2 | `EngineContext` does NOT expose `Save<T>()`, `SaveChanges()`, or any persistence-named method | CRITICAL |
| CHECK-08.3 | `EngineContext` does NOT expose `DbContext`, `IRepository`, `IAggregateStore`, or any storage interface on its public API | CRITICAL |
| CHECK-08.4 | `EngineContext` does NOT expose SQL, raw connection strings, or data access objects | CRITICAL |
| CHECK-08.5 | `EmitEvents<T>()` constraint requires `IEventSource` — aggregates must implement the event bridge interface | HIGH |
| CHECK-08.6 | `EmitEvents<T>()` collects pending events from the aggregate and clears them after collection | HIGH |
| CHECK-08.7 | `IAggregateStore` is held as a private field in `EngineContext` — not accessible to engines | CRITICAL |

### EDIM-09: Projection Isolation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | Engines do not reference `src/projections/` | CRITICAL |
| CHECK-09.2 | Engines do not trigger projection handlers directly | CRITICAL |

### EDIM-10: Determinism Enforcement (Phase 1 — DETDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | No `DateTime.UtcNow` or non-deterministic time usage in engine code | CRITICAL |
| CHECK-10.2 | No `Guid.NewGuid()` outside deterministic helper in engines | CRITICAL |
| CHECK-10.3 | IDs generated via `DeterministicIdHelper` only | CRITICAL |
| CHECK-10.4 | Event ordering deterministic per aggregate within engine execution | HIGH |
| CHECK-10.5 | Replay of engine execution produces identical results | HIGH |

### EDIM-11: Event-First Architecture (Phase 1 — EFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | All state changes originate from domain events emitted by engines | CRITICAL |
| CHECK-11.2 | No direct state mutation outside aggregate methods in engine handlers | CRITICAL |
| CHECK-11.3 | Engines emit events via `EngineContext.EmitEvents()` — events are the sole output | CRITICAL |
| CHECK-11.4 | Engine execution produces events that drive projections downstream | CRITICAL |

### EDIM-12: Lifecycle + Workflow Validation (Phase 1 — LWFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-12.1 | T2E handlers support lifecycle transitions (created → active → completed) | CRITICAL |
| CHECK-12.2 | T2E lifecycle transitions enforce aggregate invariants | CRITICAL |
| CHECK-12.3 | T1M handlers support at least one workflow execution path | CRITICAL |
| CHECK-12.4 | T1M workflow uses WSS (midstream) orchestration pattern | CRITICAL |
| CHECK-12.5 | Workflow and lifecycle execution produces observable events | HIGH |

---

## OUTPUT FORMAT

```yaml
audit: engine
status: PASS | FAIL
score: {0-100}
scope: "Engine layer compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: EDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "engine"
approval: GRANTED | BLOCKED
blocking_violations: {count of CRITICAL/HIGH}
```

---

## SCORING

| Start Score | 100 |
|-------------|-----|
| CRITICAL violation | -10 per occurrence |
| HIGH violation | -5 per occurrence |
| MEDIUM violation | -2 per occurrence |
| LOW violation | -1 per occurrence |
| Floor | 0 |
| Pass threshold | >= 80 |

---

## NEW CHECKS INTEGRATED — 2026-04-07

- **CHECK-E-WORKFLOW-01**: grep src/runtime/ src/systems/ for "WorkflowEngine" / IWorkflowStep impls. Any hit outside src/engines/T1M/ = S1.
- **CHECK-E-VERSION-01**: Inspect every engine update path. Confirm aggregate state load before mutation and that (newVersion = currentVersion + 1) is computed. Verify Postgres adapter does not coerce version to 0/default.
- **CHECK-E-STEP-02**: grep src/engines/T1M/steps/ for ISystemIntentDispatcher. Each hit must carry an explicit "// architectural-decision:" comment or fails.

### CHECK: ENG-PURITY-01
Verify engines:
- do not persist
- do not call runtime
- only emit events

### CHECK: E-LIFECYCLE-FACTORY-01 (NEW 2026-04-07)
- FAIL if any file under `src/engines/T1M/**` calls `WorkflowExecutionAggregate.Start`, `.CompleteStep`, `.Complete`, or `.Fail` (engine.guard rule 3 — T1M MUST NOT mutate domain).
- PASS requires `T1MWorkflowEngine` to construct lifecycle events via injected `WorkflowLifecycleEventFactory` and emit them through `IDomainEventSink.EmitEvent` on `WorkflowExecutionContext`.
- The aggregate remains the canonical replay target — its `Apply` method is exercised by event-store replay only, not by T1M execution.

### CHECK-E-RESUME-01 (NEW 2026-04-07)
- FAIL if `RuntimeCommandDispatcher` (or any other handler of `WorkflowResumeCommand`) reconstructs workflow state from anything other than `IWorkflowExecutionReplayService.ReplayAsync`.
- FAIL if `WorkflowExecutionReplayService.ReplayAsync` reads from a projection store, read model, or snapshot instead of `IEventStore.LoadEventsAsync`.
- PASS requires the resume path to: replay → guard `Status == "Running"` → reconstruct definition via `IWorkflowRegistry.Resolve` → call `T1MWorkflowEngine.ExecuteAsync` with a context whose `CurrentStepIndex` equals the replayed `NextStepIndex`.

### CHECK-E-RESUME-02 (NEW 2026-04-07)
- FAIL if `WorkflowExecutionReplayState.NextStepIndex` is derived from `aggregate.CurrentStepIndex` instead of the count of `WorkflowStepCompletedEvent` instances on the loaded stream (the aggregate field is ambiguous between "started, no steps done" and "step 0 completed").
- FAIL if any resume path calls a `ResumeAsync`-style method on `IWorkflowEngine` (the engine has no such method by E-RESUME-03; resume MUST go through the existing `ExecuteAsync` with a pre-populated cursor).
- FAIL if a resumed run re-emits `WorkflowExecutionStartedEvent` (covered by `T1MWorkflowEngine`'s `if (startIndex == 0)` gate; any new resume path must preserve it).

### CHECK-E-STATE-01 (NEW 2026-04-08)
- FAIL if `WorkflowExecutionStartedEvent` does not carry a `Payload` parameter.
- FAIL if `T1MWorkflowEngine` does not pass `context.Payload` into `WorkflowLifecycleEventFactory.Started(...)`.
- FAIL if `WorkflowExecutionBootstrap`'s `WorkflowExecutionStartedEvent` payload-mapper drops `Payload` when constructing `WorkflowExecutionStartedEventSchema`.

### CHECK-E-STATE-02 (NEW 2026-04-08)
- FAIL if `WorkflowStepCompletedEvent` does not carry an `Output` parameter.
- FAIL if `T1MWorkflowEngine` does not pass `stepResult.Output` into `WorkflowLifecycleEventFactory.StepCompleted(...)`.
- FAIL if `WorkflowExecutionBootstrap`'s `WorkflowStepCompletedEvent` payload-mapper drops `Output` when constructing `WorkflowStepCompletedEventSchema`.

### CHECK-E-STATE-03 (NEW 2026-04-08)
- FAIL if `WorkflowExecutionReplayService.ReplayAsync` does not extract `Payload` from `WorkflowExecutionStartedEvent` into the returned `WorkflowExecutionReplayState.Payload`.
- FAIL if `WorkflowExecutionReplayService.ReplayAsync` does not populate `WorkflowExecutionReplayState.StepOutputs` from each `WorkflowStepCompletedEvent.Output` keyed on `StepName`.
- FAIL if `RuntimeCommandDispatcher.ResumeWorkflowAsync` initializes `executionContext.Payload = new object()` instead of `state.Payload ?? new object()`.
- FAIL if `RuntimeCommandDispatcher.ResumeWorkflowAsync` does not copy `state.StepOutputs` into `executionContext.StepOutputs`.

### CHECK-E-TYPE-01 (NEW 2026-04-07)
- FAIL if `IPayloadTypeRegistry` is missing from `src/shared/contracts/event-fabric/`.
- FAIL if `PayloadTypeRegistry` impl is missing from `src/runtime/event-fabric/` or holds a hard-coded list of concrete domain types instead of being populated by `IDomainBootstrapModule.RegisterPayloadTypes`.
- FAIL if `IDomainBootstrapModule` does not declare `RegisterPayloadTypes(IPayloadTypeRegistry)` (default no-op permitted for back-compat).

### CHECK-E-TYPE-02 (NEW 2026-04-07)
- FAIL if `WorkflowExecutionStartedEvent` does not carry a `PayloadType` field.
- FAIL if `WorkflowStepCompletedEvent` does not carry an `OutputType` field.
- FAIL if `WorkflowLifecycleEventFactory` constructs lifecycle events without consulting `IPayloadTypeRegistry.TryGetName` to populate the discriminator.
- FAIL if the factory throws when the payload type is unregistered (write side MUST be permissive — emit null discriminator and let replay round-trip as JsonElement).

### CHECK-E-TYPE-03 (NEW 2026-04-07)
- FAIL if `WorkflowExecutionReplayService.Rehydrate` (or equivalent extraction loop) does not check for `JsonElement` + non-null discriminator and call `IPayloadTypeRegistry.Resolve` to deserialize.
- FAIL if the typed-payload reconstruction code lives anywhere under `src/runtime/event-fabric/` or `src/platform/host/adapters/` — runtime.guard 11.R-DOM-01 forbids concrete domain references in those paths; the seam MUST be in the engine layer.
- FAIL if `IPayloadTypeRegistry.Resolve` is called with a non-strict fallback (e.g. swallowing the unknown-type exception) — read side MUST fail fast.

## TRACEABILITY REFERENCE — 2026-04-07

MAP: see claude/traceability/guard-traceability.map.md
- Each CHECK in this audit maps to a Guard Rule ID, Enforcement Point, and Evidence as defined in the master traceability map.
