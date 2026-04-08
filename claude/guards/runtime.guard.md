# Runtime Guard

## Purpose

Enforce that `src/runtime/` is the single control plane for all command routing, event dispatch, middleware registration, and projection triggering. No component may bypass runtime to invoke engines, publish events, or execute domain operations directly.

## Scope

All files under `src/runtime/`, and all files that interact with runtime (platform, systems, engines). Evaluated at compile time, CI, and architectural review.

## Rules

1. **RUNTIME IS SOLE COMMAND ROUTER** — All command objects must be dispatched through runtime's command pipeline. The runtime command bus is the only entry point for command execution. No layer may instantiate a command handler and invoke it directly.

2. **RUNTIME IS SOLE EVENT DISPATCHER** — All domain events are collected by runtime after aggregate operations and dispatched through runtime's event pipeline. No component outside runtime may publish or broadcast domain events. Aggregates raise events; runtime dispatches them.

3. **MIDDLEWARE REGISTERED IN RUNTIME ONLY** — All cross-cutting concerns (authorization, validation, logging, telemetry, transaction management) are registered as runtime middleware. No middleware may be defined or registered in engines, systems, or platform layers.

4. **PROJECTIONS TRIGGERED BY RUNTIME EVENTS** — Read-model projections are triggered exclusively by events flowing through the runtime event pipeline. No projection may subscribe directly to an engine or domain event source. Runtime owns the projection trigger lifecycle.

5. **NO DIRECT ENGINE INVOCATION FROM PLATFORM** — Platform layer (API controllers, CLI handlers) must not call engine methods directly. Platform dispatches commands/queries to runtime; runtime routes to the appropriate engine. Platform never holds a reference to an engine type.

6. **RUNTIME OWNS TRANSACTION SCOPE** — Transaction boundaries (unit of work) are managed by runtime middleware, not by engines or domain services. Engines operate within the transaction context provided by runtime.

7. **RUNTIME IS SOLE PERSIST / PUBLISH / ANCHOR AUTHORITY** — Runtime is the ONLY layer permitted to:
   - **Persist**: Commit aggregate state changes to durable storage. Engines emit events via `EngineContext.EmitEvents()` — the runtime-injected `IAggregateStore` performs the actual persistence. No engine, system, or platform layer may persist directly.
   - **Publish**: Dispatch domain events to external consumers (Kafka, outbox, webhooks). Engines produce events; runtime publishes them.
   - **Anchor**: Write `ChainBlock` entries to the WhyceChain immutable ledger. Only runtime links events to governance chain after successful execution.
   
   T2E engines NEVER persist state. They ONLY emit events via `EngineContext.EmitEvents()`. Any persistence, publishing, or anchoring outside runtime is a CRITICAL violation.

8. **RUNTIME OWNS RETRY AND CIRCUIT BREAKER** — Retry policies, circuit breakers, and fault tolerance patterns are defined in runtime middleware. Engines must not implement their own retry logic. This ensures consistent resilience behavior.

9. **RUNTIME PIPELINE IS LINEAR** — Commands flow through a linear middleware pipeline: Platform -> Runtime -> [Middleware Chain] -> Engine -> Domain. No middleware may fork execution or invoke multiple engines in parallel outside an explicit saga/process manager.

10. **RUNTIME CONTEXT PROPAGATION** — Runtime is responsible for propagating correlation IDs, tenant context, and user context through the pipeline. No component should extract context from ambient sources (HttpContext, Thread-local) — context is passed through the runtime pipeline.

11. **NO DOMAIN LOGIC IN RUNTIME** — Runtime orchestrates but does not decide. Business rules, domain validation, and aggregate invariant enforcement stay in domain. Runtime must not contain if/else business conditions on domain state.

    **11.R-DOM-01 (S1) — NO DOMAIN-NAMED SYMBOLS OR PATHS IN RUNTIME / HOST / ADAPTERS**

    No file under `src/runtime/**`, `src/platform/host/**`, or `src/platform/host/adapters/**` may:
    - Reference a concrete domain type via a `using` directive
      (`using Whycespace.Domain.*` for any classification/context/domain)
    - Reference a concrete domain type via a **fully-qualified type
      expression** (`typeof(Whycespace.Domain.X.Y)`, cast
      `(Whycespace.Domain.X.Y)e`, parameter, generic argument, field
      type, or return type)
    - Reference a concrete domain type via a **namespace alias**
      (`using <Alias> = Whycespace.Domain.<…>;`)
    - Reference a concrete domain event/schema type by name in a `using`, parameter, generic argument, switch case, or string literal
    - Contain folders nested by `{classification}/{context}/{domain}/` (e.g. `runtime/policies/operational-system/sandbox/todo/`)
    - Hold static dictionaries, switch/case branches, or constructor dependencies keyed on a single domain
    - Hardcode domain-specific topic names, consumer-group names, projection table names, or schema mapper bodies

    The fully-qualified and alias clauses were added under Phase 1.5
    §5.1.2 Step C-G after BPV-D01 exposed eleven live binding sites in
    `src/platform/host/composition/**` that bypassed the original `using`
    predicate.

    **ALLOWED PATTERNS** in runtime/host/adapters:
    - Generic registries (`EventHandlerRegistry`, `ProjectionHandlerRegistry`, `EventSchemaRegistry`) keyed by string event-type
    - Generic bridges/workers parameterized over (topic, handler-resolver, schema-registry, table-resolver)
    - Reflection/registry-driven dispatch via the existing `Whyce.Shared.Contracts.Infrastructure.Projection.IProjectionHandler` contract

    **EXEMPT PATHS** (allowed to hold concrete domain references):
    - `src/domain/**`
    - `src/engines/T2E/**`
    - `src/projections/**`
    - `src/platform/api/**` (controllers and request DTOs are domain-shaped by design)
    - `src/systems/**` (per-domain bootstrap modules and intent handlers)
    - `src/shared/contracts/**` (cross-layer contracts may carry domain identifiers)
    - `src/runtime/event-fabric/domain-schemas/**` — the canonical
      schema-binding seam (Phase 1.5 §5.1.2 BPV-D01 remediation). This
      directory is the *only* permitted runtime location for typed
      `Whycespace.Domain.*` references. Per-domain `*SchemaModule.cs`
      files own the binding from domain CLR event types to
      `EventSchemaRegistry`, plus payload-mapper closures. Host
      composition modules consume this seam via
      `DomainSchemaCatalog.Register*` static dispatchers and never
      learn the underlying domain types.
    - `src/runtime/dispatcher/RuntimeCommandDispatcher.cs` may import
      `Whycespace.Domain.SharedKernel.Primitives.Kernel` for primitive
      kernel types (`AggregateId` and similar). Per-context domain types
      remain forbidden.

    **CHECK:** `grep -R "Todo\|<DomainName>" src/runtime/ src/platform/host/` MUST return zero matches for any registered domain *outside* the exempt paths above. Severity is S1 — block merge, fail CI, must resolve in current PR.

    **STATUS: FULLY ENFORCED as of Phase 1.5 §5.1.2 Step C (2026-04-08).**
    `src/platform/host/composition/**` is **no longer** an implicit
    exempt zone — it was cleared of all eleven typed domain references
    by BPV-D01 remediation. The only runtime exemption is
    `src/runtime/event-fabric/domain-schemas/**`, listed explicitly above.
    Verified by:
    - `grep -R "Whycespace\.Domain\." src/platform/host/` → only the
      intent-comment in `composition/runtime/RuntimeComposition.cs:80`.
    - `bash scripts/dependency-check.sh` → `Violations: 0`, `Status: PASS`
      (with the strengthened fully-qualified predicate active).

    **Phase history:**
    - **B1**: eliminated `src/runtime/projection/bridges/TodoProjectionBridge.cs` (`TodoProjectionHandler` now implements envelope-based `IProjectionHandler` directly) and `src/runtime/policies/operational-system/sandbox/todo/TodoPolicyDefinition.cs` (constants relocated to `src/shared/contracts/policy/TodoPolicyIds.cs`).
    - **B2a**: introduced `IDomainBootstrapModule` contract; relocated all 10 Program.cs Todo wiring sites into `TodoBootstrap`. Registry factories now iterate `sp.GetServices<IDomainBootstrapModule>()` inside the factory closure, before `Lock()` — preserving the lock-after-build immutability guarantee.
    - **B2b**: extended `EventSchemaEntry` with dual CLR types (`StoredEventType` for replay, `InboundEventType` for Kafka). Introduced `EventDeserializer`, `IPostgresProjectionWriter` / `PostgresProjectionWriter`, and a generic `GenericKafkaProjectionConsumerWorker` parameterized over (topic, consumer-group, deserializer, registry, writer). Deleted `KafkaProjectionConsumerWorker.cs` and `EventTypeResolver.cs`. `PostgresEventStoreAdapter` now deserializes via `EventDeserializer`. Introduced `BootstrapModuleCatalog` so Program.cs imports zero domain-named symbols.

12. **RUNTIME MUST ENFORCE POLICY MIDDLEWARE** — The runtime pipeline must include a policy evaluation middleware step that runs before command dispatch to engines. Every command must pass through policy evaluation. The policy middleware checks that a valid `PolicyDecision` exists for the command. Commands without policy decisions are rejected. No engine receives an unauthorized command.

13. **RUNTIME MUST ANCHOR EVENTS TO CHAIN** — After successful command execution, runtime must anchor the resulting domain events to the WhyceChain immutable ledger. Each event batch produces a `ChainBlock` linking: correlation ID, event hashes, policy decision hash, and actor. Events that are not chain-anchored are not governance-compliant.

14. **OUTBOX IS MANDATORY PATH** — All domain event publishing to external consumers (Kafka, webhooks, notifications) must use the outbox pattern. Runtime persists events to the outbox table within the same transaction as the aggregate state change. A background relay publishes outbox entries to Kafka. No direct external publishing within the command transaction.

15. **NO ENGINE DIRECT INVOCATION OUTSIDE DISPATCHER** — All engine invocation must flow through the runtime command/query dispatcher. No component may bypass the dispatcher to call engine methods directly. The dispatcher is the single entry point that ensures middleware (policy, validation, telemetry, transaction) is applied. Direct engine method calls from any layer are forbidden.

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

1. Scan `src/platform/` for any direct `using` or reference to `src/engines/` namespaces.
2. Scan `src/platform/` for command handler instantiation or direct invocation.
3. Scan `src/engines/` and `src/domain/` for event publishing calls (e.g., `IEventBus.Publish`, `IMediator.Publish`) — only runtime should publish.
4. Scan `src/engines/` and `src/systems/` for middleware class definitions or registrations.
5. Verify all projection handlers are wired through runtime event subscriptions, not engine events.
6. Scan `src/engines/` for `IDbTransaction`, `TransactionScope`, or unit-of-work instantiation.
7. Scan `src/engines/` for retry-related types (`Polly`, `RetryPolicy`, manual retry loops).
8. Verify runtime pipeline configuration exists and all middleware is registered there.
9. Scan `src/runtime/` for business rule conditionals on domain aggregate state.
10. Verify correlation ID / tenant context originates from runtime, not from platform or engine.
11. Scan `src/engines/` for any direct persistence calls (`SaveAsync`, `SaveChanges`, `IRepository.Save`, `DbContext`) — engines must use only `EngineContext.EmitEvents()`.
12. Verify `EngineContext` injects `IAggregateStore` privately and does not expose it on its public API — runtime owns the store, not engines.
13. Scan `src/engines/` and `src/systems/` for direct Kafka/outbox/event-bus publish calls — only runtime publishes.
14. Scan `src/engines/` for `ChainBlock` creation or chain-anchoring calls — only runtime anchors.

## Pass Criteria

- All commands routed exclusively through runtime command bus.
- All events dispatched exclusively through runtime event pipeline.
- All middleware defined and registered in runtime layer only.
- All projections triggered by runtime event subscriptions.
- Platform layer has zero engine references.
- Runtime contains zero domain business logic.
- Transaction management is runtime-owned.
- Retry/resilience policies are runtime-owned.

## Fail Criteria

- Platform imports or references engine namespace directly.
- Command handler instantiated or invoked outside runtime pipeline.
- Event published from engine or domain layer directly to external bus.
- Middleware class defined in engine, systems, or platform layer.
- Projection subscribes to non-runtime event source.
- Engine manages its own transaction scope.
- Engine implements retry/circuit-breaker logic.
- Runtime contains domain business rule (if/else on aggregate state for business purpose).
- Engine persists state directly (bypassing `EngineContext.EmitEvents()` and runtime pipeline).
- Engine publishes events directly to Kafka, outbox, or external bus.
- Engine anchors to WhyceChain directly.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Engine persistence attempt | Engine writes to DB, file, or any durable store (`_repository.Save()`, `DbContext.SaveChanges()`, `context.Save()`) |
| **S0 — CRITICAL** | Engine publishing event externally | Engine calls `eventBus.Publish()`, `IEventPublisher`, Kafka produce, or any external event dispatch |
| **S0 — CRITICAL** | Engine calling infra | Engine calls Redis, Kafka, HTTP, SQL, file I/O, or any infrastructure adapter directly |
| **S0 — CRITICAL** | Runtime bypass | Any path invoking engine without RuntimeControlPlane middleware pipeline (platform/systems calling engine directly) |
| **S0 — CRITICAL** | Engine anchors to chain directly | T2E creates `ChainBlock` or calls chain anchor service |
| **S1 — HIGH** | Middleware in engine/platform | Authorization middleware in `src/engines/` |
| **S1 — HIGH** | Domain logic in runtime | `if (order.Status == Approved)` in runtime |
| **S1 — HIGH** | R-DOM-01: domain-named symbol in runtime/host | `using Whycespace.Domain.OperationalSystem.Sandbox.Todo` in `src/runtime/**` or `src/platform/host/**` |
| **S1 — HIGH** | Engine manages transaction | `new TransactionScope()` in engine |
| **S2 — MEDIUM** | Projection bypasses runtime | Projection subscribes directly to Kafka topic |
| **S2 — MEDIUM** | Engine has retry logic | `Polly.Policy.Handle<>()` in engine |
| **S3 — LOW** | Context not propagated via runtime | Engine reads `HttpContext.Current` |

## Enforcement Action

- **S0**: Block merge. Fail CI. Architectural remediation required immediately.
- **S1**: Block merge. Fail CI. Must resolve in current PR.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track as tech debt.

All violations produce a structured report:
```
RUNTIME_GUARD_VIOLATION:
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <runtime-routed behavior>
  actual: <detected bypass>
  remediation: <fix instruction>
```

---

## NEW RULES INTEGRATED — 2026-04-07

- **R-CTX-01**: CommandContext.PolicyDecisionHash MUST be mutable (get; set;) so middleware mutations propagate through closure-captured pipeline. PolicyMiddleware MUST mutate in-place, not via "with { ... }".
- **R-ORDER-01**: Middleware order is LOCKED: Tracing -> Metrics -> ContextGuard -> Validation -> Policy -> AuthorizationGuard -> Idempotency -> ExecutionGuard -> Execution. Idempotency MUST come AFTER Auth + Policy. RuntimeControlPlaneBuilder MUST reject pipelines violating this order.
- **R-UOW-01**: EventStore.Append -> ChainAnchor -> Outbox in RuntimeCommandDispatcher.ExecuteEngineAsync MUST be wrapped in a unit-of-work / saga to prevent partial persistence on failure.
- **R-WORKFLOW-PIPE-01**: ExecuteWorkflowAsync MUST explicitly invoke persist->chain->outbox for accumulated workflow events, matching the engine path.
- **R-DOM-LEAK-01** (sub-clause of rule 11): Runtime projection bridges MUST be event-type-agnostic (dispatch by string event-type key against a registry). No "using" of concrete Whycespace.Domain.* types in src/runtime/projection/**. Allowlist: dispatcher infrastructure only.
- **R-POLICY-PATH-01**: No classification/context/domain folder nesting under src/runtime/policies/**. Policy identifier constants belong in src/shared/contracts/.
- **R-WF-OBSERVER-01**: ~~Runtime MAY persist workflow state mid-execution via shared-contract IWorkflowStepObserver.~~ **REVOKED 2026-04-07** by the workflow eventification refactor. Runtime no longer owns workflow state. Lifecycle transitions are now domain events emitted by the T1M `WorkflowLifecycleEventFactory` via `IDomainEventSink` on `WorkflowExecutionContext`, persisted by the runtime persist → chain → outbox pipeline, and projected to a read model in `src/projections/orchestration-system/workflow/`. The deprecated `IWorkflowStepObserver`, `IWorkflowStateRepository`, and `src/runtime/workflow-state/` directory have been deleted. **See R-WF-EVENTIFIED-01 below.**

- **R-WF-EVENTIFIED-01** (NEW 2026-04-07): Workflow lifecycle (start / step-completed / completed / failed) MUST be expressed as domain events (`WorkflowExecutionStartedEvent`, `WorkflowStepCompletedEvent`, `WorkflowExecutionCompletedEvent`, `WorkflowExecutionFailedEvent`) under `src/domain/orchestration-system/workflow/execution/event/`. Runtime MUST NOT mutate, persist, or cache workflow state. Engines MUST emit lifecycle events via `IDomainEventSink` on `WorkflowExecutionContext`. Reading workflow execution state is exclusively a projection-side concern. Severity S1.

- **R-WF-RESUME-01** (NEW 2026-04-07): `WorkflowResumeCommand` is currently rejected by `RuntimeCommandDispatcher` with a structured failure. Resume requires an explicit `IWorkflowExecutionReplayService` that lives outside `src/runtime/**` (because the runtime layer must not reference `Whycespace.Domain.OrchestrationSystem.Workflow.Execution.*` per rule 11.R-DOM-01). Reintroducing resume requires (a) defining `IWorkflowExecutionReplayService` in `src/shared/contracts/runtime/`, (b) implementing it under `src/engines/T1M/lifecycle/` or `src/platform/host/`, (c) injecting it into the dispatcher.

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: R-POLICY-FIRST-01 — POLICY BEFORE EXECUTION
Runtime MUST enforce policy BEFORE any execution.

ENFORCEMENT:
- Order MUST be: Guard → Policy → Idempotency → Execution

---

### RULE: R-CANONICAL-PIPELINE-01 — EXECUTION ORDER LOCK
Runtime MUST follow canonical execution order:

1. Validation
2. Identity
3. Policy
4. Idempotency
5. Execution
6. Persistence
7. Chain
8. Kafka Publish

## NEW RULES INTEGRATED — 2026-04-07 (policy eventification)

- **POLICY-PIPELINE-INTEGRATION-01** (S0): Policy events MUST flow through the RuntimeControlPlane event
  fabric (persist → chain → publish → outbox). PolicyMiddleware MUST NOT call EventStore, ChainAnchorService,
  or KafkaProducer directly. Policy events are returned via CommandResult.EmittedEvents and processed by the
  fabric. Extends rule 7 (Chain/Publish authority).
- Source: `claude/new-rules/_archives/20260407-190000-policy-eventification.md`.

## NEW RULES INTEGRATED — 2026-04-07 (workflow resume replay service)

- **R-WF-RESUME-01** (S2): `RuntimeCommandDispatcher` MUST NOT reference `Whycespace.Domain.*` types in
  order to implement `WorkflowResumeCommand`. Workflow aggregate reconstruction from the event store MUST be
  delegated to `IWorkflowExecutionReplayService` (contract in `src/shared/contracts/runtime/`, implementation
  under `src/engines/T1M/lifecycle/` or `src/platform/host/adapters/`). Extends rule 11.R-DOM-01. Until the
  service exists, `WorkflowResumeCommand` returns a structured failure.
- Source: `claude/new-rules/_archives/20260407-210000-workflow-resume-replay-service.md`.

## NEW RULES INTEGRATED — 2026-04-07 (workflow resume payload)

- **R-WF-PAYLOAD-01** (S2): `WorkflowExecutionStartedEvent` MUST persist the original
  `WorkflowStartCommand.Payload` and `WorkflowStepCompletedEvent` MUST persist step `Output`. Resume
  paths rely on these for correct reconstruction. H9 closed the persistence half.
- **R-WF-PAYLOAD-TYPED-01** (S2): Because `Payload` / `Output` are statically typed as `object?` and
  Postgres-backed replay round-trips them as `JsonElement`, a payload-type registry (event-type →
  payload CLR type) MUST be consulted by `EventDeserializer` when materializing Payload/Output.
  Steps performing `(MyPayloadType)context.Payload` otherwise fail on Postgres-backed resume. Remediation
  pending.
- Source: `claude/new-rules/_archives/20260407-230000-workflow-resume-payload-and-test-coverage.md`.

## NEW RULES INTEGRATED — 2026-04-08 (Phase 1 gate blockers)

- **R-EVENT-AUDIT-COLS-01** (S1): All persisted domain events MUST carry the audit envelope fields
  (`execution_hash`, `correlation_id`, `causation_id`, `policy_decision_hash`, `policy_version`) as
  first-class columns on the `events` table. Values present in API response / `outbox.payload` but
  absent from `events` = violation. DRIFT-3.
- **R-CHAIN-CORRELATION-01** (S2): The `correlation_id` written to `whyce_chain` MUST equal the
  `correlationId` returned in the API `auditEmission` for the same command. No layer between the
  dispatcher and the chain anchor may rewrite correlation IDs. DRIFT-5.
- Source: `claude/new-rules/_archives/20260408-000000-phase1-gate-blockers.md`.
