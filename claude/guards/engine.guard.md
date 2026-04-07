# Engine Guard

## Purpose

Enforce the tiered engine topology (T0U through T4A) in `src/engines/`. Each engine tier has specific responsibilities, permitted dependencies, and behavioral constraints. Engines are stateless processors that import domain models but never define them.

## Scope

All files under `src/engines/`. Applies to every engine class, handler, and configuration. Evaluated at CI, code review, and architectural audit.

## Rules

1. **TIER CLASSIFICATION** — Every engine must be classified into exactly one tier:
   - **T0U (Utility)**: Stateless utility operations. No domain imports. Pure computation, formatting, transformation. Examples: hashing, serialization helpers, ID generation.
   - **T1M (Mediation)**: Workflow coordination. Imports domain types for routing decisions. Orchestrates command/query flow. Does not execute domain operations directly.
   - **T2E (Execution)**: Domain operation execution. Imports and invokes domain aggregates, services, and specifications. The primary tier for business operation execution.
   - **T3I (Integration)**: External system adapters. Wraps third-party APIs, file systems, external services. Translates external data to/from domain types.
   - **T4A (Automation)**: Scheduled and background processing. Cron jobs, background workers, saga orchestration. Triggers operations on time or event schedules.

2. **T0U: NO DOMAIN IMPORTS** — T0U engines must not reference any type from `src/domain/`. They operate on primitive types and shared kernel types only. T0U is the lowest tier with the fewest permissions.

3. **T1M: NO DIRECT DOMAIN MUTATION** — T1M engines may reference domain types for routing and workflow decisions but must not call domain aggregate methods that mutate state. T1M reads domain state for orchestration; T2E executes mutations.

4. **T2E: DOMAIN EXECUTION ONLY** — T2E engines invoke domain aggregates, apply commands, and return results. T2E must not perform external integration (that is T3I) or scheduling (that is T4A). T2E is the workhorse tier.

5. **T3I: EXTERNAL BOUNDARY** — T3I engines wrap all external system interactions. They translate external DTOs to domain-compatible types. T3I must not contain domain business rules — only mapping and protocol translation.

6. **T4A: SCHEDULE AND TRIGGER** — T4A engines define scheduled jobs, background workers, and process managers. They dispatch commands through runtime, not execute domain logic directly. T4A triggers T2E operations via runtime.

7. **NO ENGINE-TO-ENGINE IMPORTS** — No engine may import or reference another engine directly. Cross-engine coordination is achieved through runtime. Engine A must not hold a reference to Engine B regardless of tier.

8. **ENGINES NEVER DEFINE DOMAIN MODELS** — Engines must not define aggregate roots, entities, value objects, or domain events. These artifacts belong exclusively to `src/domain/`. Engines may define DTOs, result types, and handler-specific request/response types.

9. **ENGINES ARE STATELESS** — Engine classes must not hold mutable instance state across invocations. No caching in engine fields, no accumulated state, no instance-level counters. Each invocation is independent. State is held by domain aggregates or external stores.

10. **ENGINE FOLDER STRUCTURE** — Each engine must reside in a folder indicating its tier: `src/engines/{tier}/{engine-name}/`. The tier prefix (T0U, T1M, T2E, T3I, T4A) must be explicit in the folder path or engine class name.

11. **ENGINE INPUT/OUTPUT TYPES** — Engines receive commands/queries as input and return result types as output. Engines must not accept or return raw infrastructure types (HttpRequest, DbDataReader). Input/output types are defined in the engine or shared layer.

12. **ENGINE TESTABILITY** — Every engine must be independently testable. Dependencies must be injected via constructor. No hidden dependencies, no service locator pattern, no static factory calls for runtime services.

13. **NO PERSISTENCE IN ENGINES** — Engines must not write to any database, file system, or durable store. No `DbContext.SaveChanges()`, `IRepository.Save()`, `File.Write()`, or storage SDK calls. Engines receive input, execute logic, and return results (including events). Persistence is the responsibility of the runtime pipeline and infrastructure adapters. T2E engines NEVER persist state — they ONLY emit events via `EngineContext.EmitEvents()`. Runtime is the ONLY layer allowed to persist, publish, and anchor.

14. **EVENT EMISSION ONLY OUTPUT** — Engines return domain events as their primary output. The result of engine execution is a set of domain events representing what happened, not a persisted state change. Runtime is responsible for persisting events and dispatching them. Engines produce events; runtime commits them.

15. **ENGINECONTEXT SURFACE RESTRICTION** — `EngineContext` is the sole interface engines use to interact with aggregate state. EngineContext exposes ONLY:
    - `LoadAggregate<T>(id)` — read path (loads aggregate)
    - `EmitEvents<T>(aggregate)` — write path (collects domain events + delegates persistence to runtime-injected store)
    - `EmittedEvents` — readonly collection of events emitted this cycle
    - `ValidateAsync(entityId)` — T0U validation gate

    EngineContext must NOT expose:
    - `SaveChanges()` or any direct persistence method
    - `DbContext` or any ORM context
    - `IRepository` or any repository interface
    - SQL, raw database connections, or storage SDK calls

    Any method on EngineContext that exposes a direct persistence concept (Save, Commit, Flush, Persist) is a CRITICAL violation.

16. **POLICY PRE-CONDITION REQUIRED** — Engines must assume that policy evaluation has already occurred before the command reaches them. Engines must not evaluate policies, check authorization, or verify actor permissions. If a command arrives at an engine, it is already authorized. Policy enforcement is a runtime middleware concern.

---

## WBSM v3 GLOBAL ENFORCEMENT

### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

---

## Check Procedure

1. Enumerate all engine folders under `src/engines/` and verify tier classification.
2. For T0U engines: scan for any `using Domain.*` import — must be zero.
3. For T1M engines: scan for aggregate mutation calls (`.Apply()`, `.Create()`, `.Execute()` on aggregates).
4. For T2E engines: scan for HTTP client, external API, or file system calls — must be zero.
5. For T3I engines: scan for domain business rule logic (if/else on domain state for business decisions).
6. For T4A engines: scan for direct aggregate invocation — must dispatch via runtime commands.
7. Scan all engines for `using Engines.*` imports referencing other engines.
8. Scan all engines for aggregate, entity, value-object, or domain event class definitions.
9. Scan all engine classes for mutable instance fields (`private List<>`, `private int`, non-readonly fields).
10. Verify each engine class uses constructor injection only (no `IServiceProvider.GetService`).
11. Verify engine folder structure includes tier designation.
12. Scan all T2E engines for persistence calls — must use ONLY `context.EmitEvents()`, never `context.Save()`, `_repository.Save()`, `DbContext.SaveChanges()`, or any direct store access.
13. Verify `EngineContext` class exposes only `LoadAggregate`, `EmitEvents`, `EmittedEvents`, and `ValidateAsync` — no `Save`, `SaveChanges`, `DbContext`, or repository on the public surface.

## Pass Criteria

- All engines correctly classified by tier.
- T0U engines have zero domain imports.
- T1M engines have zero domain mutation calls.
- No engine-to-engine imports.
- No domain model definitions in engines.
- All engine classes are stateless.
- All engines use constructor injection.
- Folder structure reflects tier classification.

## Fail Criteria

- Engine without clear tier classification.
- T0U engine importing domain types.
- T1M engine mutating domain aggregates.
- T2E engine performing external integration.
- T3I engine containing domain business rules.
- T4A engine directly invoking domain aggregates.
- Engine importing another engine.
- Engine defining domain aggregate/entity/value-object/event.
- Engine with mutable instance state.
- Engine using service locator or static factory for dependencies.
- T2E engine persisting state directly (bypassing `EngineContext.EmitEvents()`).
- `EngineContext` exposing `Save()`, `SaveChanges()`, `DbContext`, repository, or SQL on its public API.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Engine defines domain aggregate | `class Order : AggregateRoot` in engine |
| **S0 — CRITICAL** | Engine-to-engine direct import | T2E imports T1M engine class |
| **S0 — CRITICAL** | Engine persistence attempt | `_repository.Save()`, `DbContext.SaveChanges()`, `context.Save()`, or any direct store write in engine |
| **S0 — CRITICAL** | Engine publishing event externally | Engine calls `eventBus.Publish()`, `IEventPublisher.PublishAsync()`, Kafka produce, or any external event dispatch |
| **S0 — CRITICAL** | Engine calling infra | Engine calls Redis, Kafka, HTTP, SQL, file I/O, or any infrastructure adapter directly |
| **S0 — CRITICAL** | Runtime bypass | Any path that invokes an engine without passing through the RuntimeControlPlane middleware pipeline |
| **S0 — CRITICAL** | EngineContext exposes persistence | `public Task Save<T>()` on EngineContext |
| **S1 — HIGH** | T0U imports domain types | `using Domain.Economic.Capital;` in T0U |
| **S1 — HIGH** | T1M mutates domain state | T1M calling `aggregate.Apply(event)` |
| **S1 — HIGH** | T3I contains business rules | `if (order.Total > creditLimit)` in T3I |
| **S2 — MEDIUM** | T2E performs external call | `HttpClient.GetAsync()` in T2E engine |
| **S2 — MEDIUM** | Engine with mutable state | `private int _processedCount = 0;` |
| **S2 — MEDIUM** | Missing tier in folder path | Engine in `src/engines/myengine/` without tier |
| **S3 — LOW** | Service locator usage | `_provider.GetService<IFoo>()` in engine |
| **S3 — LOW** | Engine returns infrastructure type | Handler returning `HttpResponseMessage` |

## Enforcement Action

- **S0**: Block merge. Fail CI. Requires architectural remediation.
- **S1**: Block merge. Fail CI. Must fix before merge.
- **S2**: Warn in CI. Must fix within sprint.
- **S3**: Advisory. Log for review.

All violations produce a structured report:
```
ENGINE_GUARD_VIOLATION:
  engine: <engine name>
  tier: <T0U|T1M|T2E|T3I|T4A>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  permitted: <what the tier allows>
  detected: <what was found>
  remediation: <fix instruction>
```

---

## NEW RULES INTEGRATED — 2026-04-07 (from claude/new-rules)

- **E-WORKFLOW-01**: Workflow orchestration belongs ONLY in src/engines/T1M/. No WorkflowEngine or workflow execution logic in src/runtime/ or src/systems/. Runtime delegates to IWorkflowEngine (T1MWorkflowEngine).
- **E-STEP-01**: All IWorkflowStep implementations MUST live under src/engines/T1M/steps/. Forbidden in src/systems/** and src/runtime/**.
- **E-VERSION-01**: Aggregate event versions MUST strictly increment (newVersion = currentVersion + 1). Engines MUST load prior aggregate state before applying mutating commands. Persistence layer MUST persist supplied version, not default/reset to 0.
- **E-STEP-02**: T1M steps SHOULD aggregate intent results, NOT dispatch new commands via ISystemIntentDispatcher. Re-entry from step->runtime->engine requires explicit architectural approval.

- **E-LIFECYCLE-FACTORY-01** (NEW 2026-04-07): T1M MUST NOT call mutating methods on `WorkflowExecutionAggregate` (or any other aggregate) directly — that would violate rule 3 ("T1M: NO DIRECT DOMAIN MUTATION"). Lifecycle events are produced via `WorkflowLifecycleEventFactory` (located at `src/engines/T1M/lifecycle/`), which constructs the event records itself. The aggregate exists solely as the canonical replay target (its `Apply` method reconstructs state from those events). Severity S1.

- **E-RESUME-01** (NEW 2026-04-07): Workflow resume MUST be driven by `IWorkflowExecutionReplayService` (replay-from-events). Direct state restoration from any read model, projection, or snapshot is forbidden. The replay service lives at `src/engines/T1M/lifecycle/WorkflowExecutionReplayService.cs` and may reference `Whycespace.Domain.OrchestrationSystem.Workflow.Execution.*` because it lives in the engine layer; the runtime dispatcher consumes only the shared contract `IWorkflowExecutionReplayService` + `WorkflowExecutionReplayState` DTO. Severity S1.

- **E-RESUME-02** (NEW 2026-04-07): Resume MUST continue from the deterministic next-step cursor — defined as the count of `WorkflowStepCompletedEvent` instances on the loaded event stream. Resume MUST NOT re-execute already-completed steps. The aggregate's `CurrentStepIndex` is INSUFFICIENT as a cursor on its own because it collapses "started, no steps done" and "step 0 completed" to the same value; the replay service exposes `NextStepIndex` which is unambiguous. Severity S1.

- **E-RESUME-03** (NEW 2026-04-07): Resume MUST go through `T1MWorkflowEngine.ExecuteAsync` with `WorkflowExecutionContext.CurrentStepIndex` pre-populated from the replayed cursor. Adding a parallel `IWorkflowEngine.ResumeAsync` is forbidden — it would duplicate the lifecycle-event emission contract enforced by E-LIFECYCLE-FACTORY-01. The engine's existing gate (`if (startIndex == 0) EmitEvent(Started)`) ensures a resumed run does NOT re-emit `WorkflowExecutionStartedEvent`. Severity S1.

- **E-STATE-01** (NEW 2026-04-08): `WorkflowExecutionStartedEvent` MUST carry the original `Payload` (typed as `object?`, default null for back-compat). The T1M engine MUST pass `context.Payload` into `WorkflowLifecycleEventFactory.Started(...)`. Without this, replay cannot reconstruct the original input and `WorkflowResumeCommand` cannot resume payload-dependent steps. Severity S1.

- **E-STATE-02** (NEW 2026-04-08): `WorkflowStepCompletedEvent` MUST carry the step's `Output` (typed as `object?`, default null for back-compat). The T1M engine MUST pass `stepResult.Output` into `WorkflowLifecycleEventFactory.StepCompleted(...)`. Without this, replay cannot reconstruct `WorkflowExecutionContext.StepOutputs` and downstream steps that read prior outputs cannot be resumed. Severity S1.

- **E-STATE-03** (NEW 2026-04-08): `IWorkflowExecutionReplayService.ReplayAsync` MUST reconstruct both `Payload` (from the started event) and `StepOutputs` (a dictionary keyed on `StepName`, populated from each `WorkflowStepCompletedEvent.Output`). The dispatcher resume path MUST populate `WorkflowExecutionContext.Payload` and copy each `state.StepOutputs` entry into `executionContext.StepOutputs` (init-only on the property, member-mutable). Severity S1.

  CAVEAT (not yet a guard): `Payload`/`Output` are statically `object?`, so `PostgresEventStoreAdapter`'s `JsonSerializer.Serialize(evt, evt.GetType())` round-trips them as `JsonElement` on Postgres-backed replay. In-process / in-memory replay preserves the original CLR reference. A typed-payload registry is required for typed Postgres-backed resume — tracked in `claude/new-rules/20260407-230000-workflow-resume-payload-and-test-coverage.md`.

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: ENG-PURITY-01 — ENGINE PURITY (T2E)
T2E engines MUST be pure execution units.

ENFORCEMENT:
- MUST NOT persist data
- MUST NOT call runtime
- MUST NOT call infrastructure directly
- MUST only emit events

---

### RULE: ENG-DOMAIN-ALIGN-01 — STRICT DOMAIN ALIGNMENT
Each engine MUST map to a single domain aggregate.

ENFORCEMENT:
- No cross-domain logic inside a single engine
- One engine = one domain responsibility

---

## NEW RULES INTEGRATED — 2026-04-07 (H10 TYPE SAFETY)

- **E-TYPE-01** (NEW 2026-04-07): Any CLR type that flows through a workflow lifecycle event as `Payload` or step `Output` MUST be registered in `IPayloadTypeRegistry` via the owning `IDomainBootstrapModule.RegisterPayloadTypes`. Unregistered types round-trip as `JsonElement` on Postgres-backed replay (current legacy behavior, preserved for back-compat) and CANNOT be consumed by resumed steps that expect a typed object. Severity S1.

- **E-TYPE-02** (NEW 2026-04-07): `WorkflowExecutionStartedEvent` and `WorkflowStepCompletedEvent` MUST carry `PayloadType` / `OutputType` discriminator strings stamped by `WorkflowLifecycleEventFactory` at write time when (a) the payload/output is non-null and (b) the type is registered in `IPayloadTypeRegistry`. The factory MUST consult the registry via `TryGetName` and emit a null discriminator on miss (back-compat), never throw. Severity S1.

- **E-TYPE-03** (NEW 2026-04-07): `WorkflowExecutionReplayService.ReplayAsync` MUST rehydrate `Payload` and step `Output` values from `JsonElement` back into the registered CLR type when (a) the value is a `JsonElement` and (b) the corresponding `PayloadType` / `OutputType` discriminator is non-null. Resolution MUST go through `IPayloadTypeRegistry.Resolve` (strict — throws on unknown). The deserialization seam MUST live in the engine layer (`src/engines/T1M/lifecycle/`), NOT in `src/runtime/event-fabric/EventDeserializer` or `src/platform/host/adapters/PostgresEventStoreAdapter` (runtime.guard 11.R-DOM-01 forbids concrete domain references in those paths). Severity S1.
