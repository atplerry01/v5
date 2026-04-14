# Runtime Guard (Canonical)

## Purpose

Enforce that `src/runtime/` is the single control plane for all command routing, event dispatch, middleware registration, projection triggering, composition loading, engine invocation, persistence, publishing, and chain anchoring. This canonical guard consolidates the runtime execution surface: runtime order, composition loading, engine purity (stateless, events only), projection read-models, replay determinism, IClock usage, deterministic IDs, SHA256 hashing, event emission, and Phase 1.5 runtime rules.

## Scope

All files under `src/runtime/`, `src/engines/`, `src/projections/`, `src/platform/host/`, `src/platform/host/adapters/`, and any component that interacts with these (platform, systems). Evaluated at compile time, CI, and architectural review.

## Source consolidation

This guard merges the following sources:
1. `runtime.guard.md` (prior version — preserved verbatim: rules 1–15 + GE-01..05 + all NEW-RULES integrations)
2. `runtime-order.guard.md` (merged into Runtime Order & Lifecycle section)
3. `phase1.5-runtime.guard.md` (merged into Phase 1.5 Runtime Rules section)
4. `engine.guard.md` (merged into Engine Purity section)
5. `composition-loader.guard.md` (merged into Composition & Loading section)
6. `program-composition.guard.md` (merged into Composition & Loading section)
7. `projection.guard.md` (merged into Projections section)
8. `replay-determinism.guard.md` (merged into Replay Determinism section)
9. `determinism.guard.md` (merged into Determinism Core section)
10. `deterministic-id.guard.md` (merged into Deterministic IDs section)
11. `hash-determinism.guard.md` (merged into Hash Determinism section)

**Dedups performed:**
- `GE-01` (deterministic execution) appeared in runtime.guard.md, engine.guard.md, projection.guard.md — consolidated to a single canonical block under "WBSM v3 Global Enforcement". All three source contexts preserved by cross-reference.
- `GE-02..GE-05` likewise deduplicated across runtime/engine/projection sources.
- Runtime rule 7 (persist/publish/anchor) and engine rule 13 (no persistence in engines) overlap in spirit; both preserved verbatim because they carry distinct severity tables and check procedures.
- R-WF-RESUME-01 appeared twice in runtime.guard.md (rule 11 sub and NEW RULES 2026-04-07 workflow resume) — both preserved as they have complementary severity/context.
- R-RT-06 (Phase 1.5) and Determinism GE-01 / DET-* rules overlap on `IClock`/`IIdGenerator` — both preserved because R-RT-06 adds the MI-1 owner-token shape exemption.

**Semantic conflicts flagged (not resolved):**
- Engine rule 7 was historically "NO ENGINE-TO-ENGINE IMPORTS" but was amended on 2026-04-13 to "NO CROSS-TIER ENGINE IMPORTS" with same-tier imports now permitted. Both wordings appear in source; the amended wording is canonical per the 2026-04-13 new-rules entry.
- R-WF-OBSERVER-01 was REVOKED 2026-04-07 by the workflow eventification refactor; preserved with revocation marker per source.

---

## Rules

### Section: Runtime Order & Lifecycle

#### R1 — RUNTIME IS SOLE COMMAND ROUTER
All command objects must be dispatched through runtime's command pipeline. The runtime command bus is the only entry point for command execution. No layer may instantiate a command handler and invoke it directly.
**Source:** runtime.guard.md rule 1

#### R2 — RUNTIME IS SOLE EVENT DISPATCHER
All domain events are collected by runtime after aggregate operations and dispatched through runtime's event pipeline. No component outside runtime may publish or broadcast domain events. Aggregates raise events; runtime dispatches them.
**Source:** runtime.guard.md rule 2

#### R3 — MIDDLEWARE REGISTERED IN RUNTIME ONLY
All cross-cutting concerns (authorization, validation, logging, telemetry, transaction management) are registered as runtime middleware. No middleware may be defined or registered in engines, systems, or platform layers.
**Source:** runtime.guard.md rule 3

#### R4 — PROJECTIONS TRIGGERED BY RUNTIME EVENTS
Read-model projections are triggered exclusively by events flowing through the runtime event pipeline. No projection may subscribe directly to an engine or domain event source. Runtime owns the projection trigger lifecycle.
**Source:** runtime.guard.md rule 4

#### R5 — NO DIRECT ENGINE INVOCATION FROM PLATFORM
Platform layer (API controllers, CLI handlers) must not call engine methods directly. Platform dispatches commands/queries to runtime; runtime routes to the appropriate engine. Platform never holds a reference to an engine type.
**Source:** runtime.guard.md rule 5

#### R6 — RUNTIME OWNS TRANSACTION SCOPE
Transaction boundaries (unit of work) are managed by runtime middleware, not by engines or domain services. Engines operate within the transaction context provided by runtime.
**Source:** runtime.guard.md rule 6

#### R7 — RUNTIME IS SOLE PERSIST / PUBLISH / ANCHOR AUTHORITY
Runtime is the ONLY layer permitted to:
- **Persist**: Commit aggregate state changes to durable storage. Engines emit events via `EngineContext.EmitEvents()` — the runtime-injected `IAggregateStore` performs the actual persistence. No engine, system, or platform layer may persist directly.
- **Publish**: Dispatch domain events to external consumers (Kafka, outbox, webhooks). Engines produce events; runtime publishes them.
- **Anchor**: Write `ChainBlock` entries to the WhyceChain immutable ledger. Only runtime links events to governance chain after successful execution.

T2E engines NEVER persist state. They ONLY emit events via `EngineContext.EmitEvents()`. Any persistence, publishing, or anchoring outside runtime is a CRITICAL violation.
**Source:** runtime.guard.md rule 7

#### R8 — RUNTIME OWNS RETRY AND CIRCUIT BREAKER
Retry policies, circuit breakers, and fault tolerance patterns are defined in runtime middleware. Engines must not implement their own retry logic. This ensures consistent resilience behavior.
**Source:** runtime.guard.md rule 8

#### R9 — RUNTIME PIPELINE IS LINEAR
Commands flow through a linear middleware pipeline: Platform -> Runtime -> [Middleware Chain] -> Engine -> Domain. No middleware may fork execution or invoke multiple engines in parallel outside an explicit saga/process manager.
**Source:** runtime.guard.md rule 9

#### R10 — RUNTIME CONTEXT PROPAGATION
Runtime is responsible for propagating correlation IDs, tenant context, and user context through the pipeline. No component should extract context from ambient sources (HttpContext, Thread-local) — context is passed through the runtime pipeline.
**Source:** runtime.guard.md rule 10

#### R11 — NO DOMAIN LOGIC IN RUNTIME
Runtime orchestrates but does not decide. Business rules, domain validation, and aggregate invariant enforcement stay in domain. Runtime must not contain if/else business conditions on domain state.

**11.R-DOM-01 (S1) — NO DOMAIN-NAMED SYMBOLS OR PATHS IN RUNTIME / HOST / ADAPTERS**

No file under `src/runtime/**`, `src/platform/host/**`, or `src/platform/host/adapters/**` may:
- Reference a concrete domain type via a `using` directive (`using Whycespace.Domain.*` for any classification/context/domain)
- Reference a concrete domain type via a **fully-qualified type expression** (`typeof(Whycespace.Domain.X.Y)`, cast `(Whycespace.Domain.X.Y)e`, parameter, generic argument, field type, or return type)
- Reference a concrete domain type via a **namespace alias** (`using <Alias> = Whycespace.Domain.<…>;`)
- Reference a concrete domain event/schema type by name in a `using`, parameter, generic argument, switch case, or string literal
- Contain folders nested by `{classification}/{context}/{domain}/` (e.g. `runtime/policies/operational-system/sandbox/todo/`)
- Hold static dictionaries, switch/case branches, or constructor dependencies keyed on a single domain
- Hardcode domain-specific topic names, consumer-group names, projection table names, or schema mapper bodies

The fully-qualified and alias clauses were added under Phase 1.5 §5.1.2 Step C-G after BPV-D01 exposed eleven live binding sites in `src/platform/host/composition/**` that bypassed the original `using` predicate.

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
- `src/runtime/event-fabric/domain-schemas/**` — the canonical schema-binding seam (Phase 1.5 §5.1.2 BPV-D01 remediation). This directory is the *only* permitted runtime location for typed `Whycespace.Domain.*` references. Per-domain `*SchemaModule.cs` files own the binding from domain CLR event types to `EventSchemaRegistry`, plus payload-mapper closures. Host composition modules consume this seam via `DomainSchemaCatalog.Register*` static dispatchers and never learn the underlying domain types.
- `src/runtime/dispatcher/RuntimeCommandDispatcher.cs` may import `Whycespace.Domain.SharedKernel.Primitives.Kernel` for primitive kernel types (`AggregateId` and similar). Per-context domain types remain forbidden.

**CHECK:** `grep -R "Todo\|<DomainName>" src/runtime/ src/platform/host/` MUST return zero matches for any registered domain *outside* the exempt paths above. Severity is S1 — block merge, fail CI, must resolve in current PR.

**STATUS: FULLY ENFORCED as of Phase 1.5 §5.1.2 Step C (2026-04-08).** `src/platform/host/composition/**` is **no longer** an implicit exempt zone — it was cleared of all eleven typed domain references by BPV-D01 remediation. The only runtime exemption is `src/runtime/event-fabric/domain-schemas/**`, listed explicitly above.

Verified by:
- `grep -R "Whycespace\.Domain\." src/platform/host/` → only the intent-comment in `composition/runtime/RuntimeComposition.cs:80`.
- `bash scripts/dependency-check.sh` → `Violations: 0`, `Status: PASS` (with the strengthened fully-qualified predicate active).

**Phase history:**
- **B1**: eliminated `src/runtime/projection/bridges/TodoProjectionBridge.cs` (`TodoProjectionHandler` now implements envelope-based `IProjectionHandler` directly) and `src/runtime/policies/operational-system/sandbox/todo/TodoPolicyDefinition.cs` (constants relocated to `src/shared/contracts/policy/TodoPolicyIds.cs`).
- **B2a**: introduced `IDomainBootstrapModule` contract; relocated all 10 Program.cs Todo wiring sites into `TodoBootstrap`. Registry factories now iterate `sp.GetServices<IDomainBootstrapModule>()` inside the factory closure, before `Lock()` — preserving the lock-after-build immutability guarantee.
- **B2b**: extended `EventSchemaEntry` with dual CLR types (`StoredEventType` for replay, `InboundEventType` for Kafka). Introduced `EventDeserializer`, `IPostgresProjectionWriter` / `PostgresProjectionWriter`, and a generic `GenericKafkaProjectionConsumerWorker` parameterized over (topic, consumer-group, deserializer, registry, writer). Deleted `KafkaProjectionConsumerWorker.cs` and `EventTypeResolver.cs`. `PostgresEventStoreAdapter` now deserializes via `EventDeserializer`. Introduced `BootstrapModuleCatalog` so Program.cs imports zero domain-named symbols.

**Source:** runtime.guard.md rule 11 + 11.R-DOM-01

#### R12 — RUNTIME MUST ENFORCE POLICY MIDDLEWARE
The runtime pipeline must include a policy evaluation middleware step that runs before command dispatch to engines. Every command must pass through policy evaluation. The policy middleware checks that a valid `PolicyDecision` exists for the command. Commands without policy decisions are rejected. No engine receives an unauthorized command.
**Source:** runtime.guard.md rule 12

#### R13 — RUNTIME MUST ANCHOR EVENTS TO CHAIN
After successful command execution, runtime must anchor the resulting domain events to the WhyceChain immutable ledger. Each event batch produces a `ChainBlock` linking: correlation ID, event hashes, policy decision hash, and actor. Events that are not chain-anchored are not governance-compliant.
**Source:** runtime.guard.md rule 13

#### R14 — OUTBOX IS MANDATORY PATH
All domain event publishing to external consumers (Kafka, webhooks, notifications) must use the outbox pattern. Runtime persists events to the outbox table within the same transaction as the aggregate state change. A background relay publishes outbox entries to Kafka. No direct external publishing within the command transaction.
**Source:** runtime.guard.md rule 14

#### R15 — NO ENGINE DIRECT INVOCATION OUTSIDE DISPATCHER
All engine invocation must flow through the runtime command/query dispatcher. No component may bypass the dispatcher to call engine methods directly. The dispatcher is the single entry point that ensures middleware (policy, validation, telemetry, transaction) is applied. Direct engine method calls from any layer are forbidden.
**Source:** runtime.guard.md rule 15

#### R-CTX-01 — Mutable PolicyDecisionHash
`CommandContext.PolicyDecisionHash` MUST be mutable (get; set;) so middleware mutations propagate through closure-captured pipeline. PolicyMiddleware MUST mutate in-place, not via "with { ... }".
**Source:** runtime.guard.md NEW RULES 2026-04-07

#### R-ORDER-01 — Middleware Order Locked
Middleware order is LOCKED: Tracing -> Metrics -> ContextGuard -> Validation -> Policy -> AuthorizationGuard -> Idempotency -> ExecutionGuard -> Execution. Idempotency MUST come AFTER Auth + Policy. RuntimeControlPlaneBuilder MUST reject pipelines violating this order.
**Source:** runtime.guard.md NEW RULES 2026-04-07

#### R-UOW-01 — Unit of Work Wrapping
EventStore.Append -> ChainAnchor -> Outbox in RuntimeCommandDispatcher.ExecuteEngineAsync MUST be wrapped in a unit-of-work / saga to prevent partial persistence on failure.
**Source:** runtime.guard.md NEW RULES 2026-04-07

#### R-WORKFLOW-PIPE-01 — Workflow Pipeline
ExecuteWorkflowAsync MUST explicitly invoke persist->chain->outbox for accumulated workflow events, matching the engine path.
**Source:** runtime.guard.md NEW RULES 2026-04-07

#### R-DOM-LEAK-01 — Runtime Projection Bridges Event-Type-Agnostic
(sub-clause of R11): Runtime projection bridges MUST be event-type-agnostic (dispatch by string event-type key against a registry). No "using" of concrete Whycespace.Domain.* types in src/runtime/projection/**. Allowlist: dispatcher infrastructure only.
**Source:** runtime.guard.md NEW RULES 2026-04-07

#### R-POLICY-PATH-01 — No Domain Folder Nesting in Policies
No classification/context/domain folder nesting under src/runtime/policies/**. Policy identifier constants belong in src/shared/contracts/.
**Source:** runtime.guard.md NEW RULES 2026-04-07

#### R-WF-OBSERVER-01 — (REVOKED 2026-04-07)
~~Runtime MAY persist workflow state mid-execution via shared-contract IWorkflowStepObserver.~~ **REVOKED 2026-04-07** by the workflow eventification refactor. Runtime no longer owns workflow state. Lifecycle transitions are now domain events emitted by the T1M `WorkflowLifecycleEventFactory` via `IDomainEventSink` on `WorkflowExecutionContext`, persisted by the runtime persist → chain → outbox pipeline, and projected to a read model in `src/projections/orchestration-system/workflow/`. The deprecated `IWorkflowStepObserver`, `IWorkflowStateRepository`, and `src/runtime/workflow-state/` directory have been deleted. **See R-WF-EVENTIFIED-01 below.**
**Source:** runtime.guard.md NEW RULES 2026-04-07

#### R-WF-EVENTIFIED-01 — Workflow Lifecycle as Domain Events
Workflow lifecycle (start / step-completed / completed / failed) MUST be expressed as domain events (`WorkflowExecutionStartedEvent`, `WorkflowStepCompletedEvent`, `WorkflowExecutionCompletedEvent`, `WorkflowExecutionFailedEvent`) under `src/domain/orchestration-system/workflow/execution/event/`. Runtime MUST NOT mutate, persist, or cache workflow state. Engines MUST emit lifecycle events via `IDomainEventSink` on `WorkflowExecutionContext`. Reading workflow execution state is exclusively a projection-side concern.
**Source:** runtime.guard.md NEW RULES 2026-04-07
**Severity:** S1

#### R-WF-RESUME-01 — Workflow Resume Replay Service
`WorkflowResumeCommand` is currently rejected by `RuntimeCommandDispatcher` with a structured failure. Resume requires an explicit `IWorkflowExecutionReplayService` that lives outside `src/runtime/**` (because the runtime layer must not reference `Whycespace.Domain.OrchestrationSystem.Workflow.Execution.*` per rule 11.R-DOM-01). Reintroducing resume requires (a) defining `IWorkflowExecutionReplayService` in `src/shared/contracts/runtime/`, (b) implementing it under `src/engines/T1M/lifecycle/` or `src/platform/host/`, (c) injecting it into the dispatcher. `RuntimeCommandDispatcher` MUST NOT reference `Whycespace.Domain.*` types in order to implement `WorkflowResumeCommand`. Workflow aggregate reconstruction from the event store MUST be delegated to `IWorkflowExecutionReplayService` (contract in `src/shared/contracts/runtime/`, implementation under `src/engines/T1M/lifecycle/` or `src/platform/host/adapters/`). Extends rule 11.R-DOM-01. Until the service exists, `WorkflowResumeCommand` returns a structured failure.
**Source:** runtime.guard.md NEW RULES 2026-04-07 (rule 11 sub + workflow resume replay service)
**Severity:** S2

#### R-POLICY-FIRST-01 — POLICY BEFORE EXECUTION
Runtime MUST enforce policy BEFORE any execution.

ENFORCEMENT:
- Order MUST be: Guard → Policy → Idempotency → Execution

**Source:** runtime.guard.md NEW RULES 2026-04-07 (NORMALIZATION)

#### R-CANONICAL-PIPELINE-01 — EXECUTION ORDER LOCK
Runtime MUST follow canonical execution order:

1. Validation
2. Identity
3. Policy
4. Idempotency
5. Execution
6. Persistence
7. Chain
8. Kafka Publish

**Source:** runtime.guard.md NEW RULES 2026-04-07 (NORMALIZATION)

#### POLICY-PIPELINE-INTEGRATION-01 — Policy Events Flow Through Fabric
Policy events MUST flow through the RuntimeControlPlane event fabric (persist → chain → publish → outbox). PolicyMiddleware MUST NOT call EventStore, ChainAnchorService, or KafkaProducer directly. Policy events are returned via CommandResult.EmittedEvents and processed by the fabric. Extends rule 7 (Chain/Publish authority).
**Source:** runtime.guard.md NEW RULES 2026-04-07 (policy eventification)
**Severity:** S0

#### R-WF-PAYLOAD-01 — Payload Persistence on Started / StepCompleted
`WorkflowExecutionStartedEvent` MUST persist the original `WorkflowStartCommand.Payload` and `WorkflowStepCompletedEvent` MUST persist step `Output`. Resume paths rely on these for correct reconstruction. H9 closed the persistence half.
**Source:** runtime.guard.md NEW RULES 2026-04-07 (workflow resume payload)
**Severity:** S2

#### R-WF-PAYLOAD-TYPED-01 — Typed Payload Registry
Because `Payload` / `Output` are statically typed as `object?` and Postgres-backed replay round-trips them as `JsonElement`, a payload-type registry (event-type → payload CLR type) MUST be consulted by `EventDeserializer` when materializing Payload/Output. Steps performing `(MyPayloadType)context.Payload` otherwise fail on Postgres-backed resume. Remediation pending.
**Source:** runtime.guard.md NEW RULES 2026-04-07 (workflow resume payload)
**Severity:** S2

#### R-EVENT-AUDIT-COLS-01 — Audit Envelope Fields as Columns
All persisted domain events MUST carry the audit envelope fields (`execution_hash`, `correlation_id`, `causation_id`, `policy_decision_hash`, `policy_version`) as first-class columns on the `events` table. Values present in API response / `outbox.payload` but absent from `events` = violation. DRIFT-3.
**Source:** runtime.guard.md NEW RULES 2026-04-08 (Phase 1 gate blockers)
**Severity:** S1

#### R-CHAIN-CORRELATION-01 — Chain Correlation ID Preservation
The `correlation_id` written to `whyce_chain` MUST equal the `correlationId` returned in the API `auditEmission` for the same command. No layer between the dispatcher and the chain anchor may rewrite correlation IDs. DRIFT-5.
**Source:** runtime.guard.md NEW RULES 2026-04-08 (Phase 1 gate blockers)
**Severity:** S2

#### RO-LOCKED-ORDER — Canonical Execution Stage List
Lock the WBSM v3 canonical execution order at the source level. Any change that reorders, removes, or makes optional any of the 11 ordered stages (8 middlewares + 3 fabric stages) is a critical violation.

**Scope:**
- `src/runtime/control-plane/RuntimeControlPlaneBuilder.cs` — owns middleware order
- `src/runtime/event-fabric/EventFabric.cs` — owns post-execution sequence

**Locked Order:**
```
REQUEST PATH (middleware pipeline)
  1. TracingMiddleware
  2. MetricsMiddleware
  3. ContextGuardMiddleware
  4. ValidationMiddleware
  5. PolicyMiddleware
  6. AuthorizationGuardMiddleware
  7. IdempotencyMiddleware
  8. ExecutionGuardMiddleware
→ RuntimeCommandDispatcher (terminal)

RESPONSE PATH (event fabric, only on success with events)
  9.  EventStoreService.AppendAsync
  10. ChainAnchorService.AnchorAsync
  11. OutboxService.EnqueueAsync
```
**Source:** runtime-order.guard.md

#### RO-1 — NO REORDERING
The list returned by `RuntimeControlPlaneBuilder.Build()` must keep the 8 middlewares in positions 1–8 as listed above. Any PR that swaps two entries, inserts a new entry without renumbering this guard, or conditionally omits an entry is a violation.
**Source:** runtime-order.guard.md rule 1
**Severity:** S0

#### RO-2 — NO OPTIONAL MIDDLEWARES
All 8 must remain mandatory in `ValidateMandatoryDependencies()`. A PR that converts a middleware to optional (removing its null-check) is a violation.
**Source:** runtime-order.guard.md rule 2
**Severity:** S0

#### RO-3 — NO PARALLEL FABRIC STAGES
`EventFabric.ProcessAsync` must keep `Persist → Chain → Outbox` as three sequential `await` statements. The following are forbidden:
- `Task.WhenAll(persistTask, chainTask, outboxTask)`
- `_ = anchorTask` (fire-and-forget on chain)
- Wrapping the three calls in a `Parallel.ForEachAsync`
- Reordering them so chain or outbox precede persist

**Source:** runtime-order.guard.md rule 3
**Severity:** S0

#### RO-4 — NO ALTERNATIVE ENTRY POINTS
Engines must only be reachable through the control plane. New code that calls `engine.ExecuteAsync(...)` outside `RuntimeCommandDispatcher` is a violation. Cross-reference with the runtime C# guards `RuntimeIsolationGuard.cs`, `ControlPlaneGuard.cs`, `FabricInvocationGuard.cs`.
**Source:** runtime-order.guard.md rule 4
**Severity:** S0

#### RO-5 — POLICY MUST BE BETWEEN PRE- AND POST-POLICY GUARDS
`PolicyMiddleware` (position 5) must remain strictly after `ContextGuardMiddleware` + `ValidationMiddleware` (positions 3, 4) and strictly before `AuthorizationGuardMiddleware` + `IdempotencyMiddleware` (positions 6, 7). A PR that moves Policy to position 1 or position 8 is a violation even if all 8 middlewares are still present.
**Source:** runtime-order.guard.md rule 5
**Severity:** S0

#### RO-6 — CHAIN MUST FOLLOW PERSIST
Chain anchoring may not begin until persistence has completed. The `await` on line 87 of `EventFabric.cs` must precede the `await` on line 90. A PR that hoists the chain anchor above the event store append is a violation.
**Source:** runtime-order.guard.md rule 6
**Severity:** S0

#### RO-7 — OUTBOX MUST FOLLOW CHAIN
Outbox enqueue may not begin until chain anchoring has completed. The `await` on line 90 must precede the `await` on line 93. A PR that publishes to Kafka before the chain block is committed is a violation — projections would observe events that are not yet anchored.
**Source:** runtime-order.guard.md rule 7
**Severity:** S0

#### RO-CANONICAL-11 — 11-Stage Canonical Order
The canonical execution order is the **11-stage** order (8 middlewares + 3 fabric stages), NOT the 7-step prompt summary. Any prompt or audit that collapses to 7 steps is treated as a human-readable summary, never normative. The 11 stages are encoded structurally in RuntimeControlPlaneBuilder and EventFabric and MUST match this guard verbatim.
**Source:** runtime-order.guard.md NEW RULES 2026-04-07

---

### Section: Phase 1.5 Runtime Rules

**STATUS: CANONICAL** — LOCKED 2026-04-09 per `phase1.5-final.audit.md` §7.

This section locks the architectural and runtime invariants established by Phase 1.5 §5.2.4 (Health, Readiness, Degraded Modes) and §5.2.5 MI-1 (Distributed Execution Safety Baseline).

#### R-RT-01 — RuntimeControlPlane is the single execution entry
`Whyce.Runtime.ControlPlane.RuntimeControlPlane.ExecuteAsync` is the only path that may invoke `ICommandDispatcher.DispatchAsync` or `IEventFabric.ProcessAsync` / `IEventFabric.ProcessAuditAsync`.

**Why**: Defense-in-depth. The control plane runs HSID prelude, degraded stamping, enforcement gate, the locked middleware pipeline, the policy guard, and the event fabric in a fixed order with non-bypassable invariants. Any direct call to the dispatcher or fabric bypasses these guards.

**How to apply**: New callers MUST resolve `IRuntimeControlPlane` (or `ISystemIntentDispatcher`, which forwards to it). Direct DI references to `ICommandDispatcher` or `IEventFabric` outside `RuntimeControlPlane` are forbidden.
**Source:** phase1.5-runtime.guard.md

#### R-RT-02 — `ExecuteAsync` pipeline order is locked
The body of `RuntimeControlPlane.ExecuteAsync` MUST execute the following stages in this exact order:

```
1. Acquire MI-1 distributed execution lock
2. (try {) HSID v2.1 prelude
3. Stamp CommandContext.DegradedMode (HC-7)
4. RuntimeEnforcementGate.Evaluate (HC-8) — block branches return BEFORE the pipeline
5. Locked 8-middleware pipeline (Tracing → Metrics → ContextGuard → Validation → Policy → AuthorizationGuard → Idempotency → ExecutionGuard)
6. DispatchWithPolicyGuard
7. ICommandDispatcher.DispatchAsync
8. EventFabric.ProcessAuditAsync
9. EventFabric.ProcessAsync
10. (} finally { Release MI-1 lock })
```

**Why**: Each stage establishes invariants the next stage depends on. The lock must wrap everything (so a thrown exception still surrenders the lease). The HSID stamp must precede the pipeline (downstream tracing reads it). The degraded stamp must precede the enforcement gate (the gate consumes it). The enforcement gate must precede the pipeline (block decisions skip validation/policy/idempotency/engine).

**How to apply**: Re-ordering, removing, or short-circuiting any stage is a guard violation. New stages may only be inserted with an explicit guard amendment.
**Source:** phase1.5-runtime.guard.md

#### R-RT-03 — Lock provider must be exception-free
`IExecutionLockProvider.TryAcquireAsync` and `IExecutionLockProvider.ReleaseAsync` MUST NOT throw on a transient store outage. The contract is exception-free; failures collapse to a deterministic `false` (acquire) or no-op (release).

**Why**: HC-9 closed this. The runtime control plane translates `false` into `execution_lock_unavailable` / `execution_cancelled` via `CommandResult.Failure(...)`. A thrown exception would surface as an unhandled 500 and break the canonical refusal vocabulary.

**How to apply**: Any new `IExecutionLockProvider` implementation MUST wrap underlying store calls in a catch-all. Verified by `ExecutionLockProviderTests.ProviderUnderRedisOutage_ReturnsFalse_NoThrow`.
**Source:** phase1.5-runtime.guard.md

#### R-RT-04 — All §5.2.4 / §5.2.5 failures use `CommandResult.Failure(reason)`
Failures in the maintenance, degraded, lock, and health surfaces MUST return a structured `CommandResult.Failure` with a low-cardinality canonical reason identifier from the locked vocabulary. Exceptions MUST NOT be used as control flow at this surface.

**Why**: HC-7 / HC-8 / HC-9 / MI-1 established this discipline. Operators and audit pipelines depend on a finite, snake_case, non-payloaded reason set.

**Locked vocabulary** (additions require an amendment):
- Hard-block: `execution_lock_unavailable`, `execution_cancelled`, `system_maintenance_mode`, `restricted_during_degraded_mode`
- NotReady reasons: `host_draining`, `critical_healthcheck_failed`, `redis_unhealthy`, `postgres_pool_exhausted`, `postgres_acquisition_failures`, `postgres_invalid_pool_config`, `worker_unhealthy`, `outbox_snapshot_stale`
- Degraded reasons (= `RuntimeDegradedMode.CanonicalReasons`): `postgres_high_wait`, `opa_breaker_open`, `chain_anchor_breaker_open`, `outbox_over_high_water_mark`, `noncritical_healthcheck_failed`, `redis_degraded_latency`

**How to apply**: New reasons MUST be added to the canonical set tests (`RuntimeDegradedModeTests.CanonicalReasonSet_MatchesSpec` for the degraded set) so any future widening is a deliberate, reviewed change.

**Note**: Pre-§5.2.4 typed-exception refusal handlers (`OutboxSaturatedException`, `PolicyEvaluationUnavailableException`, etc.) remain canonical edge-handler patterns. They are NOT runtime control flow — they bubble untouched from a single throw site to a single API edge handler.
**Source:** phase1.5-runtime.guard.md

#### R-RT-05 — Critical infrastructure dependencies must be health-checked
Every infrastructure dependency on the dispatch hot path MUST be exposed via an `IHealthCheck` and folded into `RuntimeStateAggregator` with a specific canonical reason. The set of currently-required dependencies is: postgres pool, workers, redis, opa, chain, outbox.

**Why**: HC-1..HC-9 closed each. The aggregator's rule chain depends on every check being present. A missing check would silently degrade the runtime's self-knowledge.

**How to apply**: Any new infrastructure dependency added on the dispatch hot path MUST come with (a) an `IHealthCheck`, (b) a specific canonical reason in the aggregator, and (c) an exclusion from the generic `critical_healthcheck_failed` and `noncritical_healthcheck_failed` scans to prevent double-counting.
**Source:** phase1.5-runtime.guard.md

#### R-RT-06 — Determinism rules ($9) apply to all new code
New code MUST NOT use `Guid.NewGuid()`, `DateTime.UtcNow`, `DateTime.Now`, `DateTimeOffset.UtcNow`, `Random`, or any other non-deterministic primitive. Time MUST flow through `IClock`. IDs MUST be deterministic via `IIdGenerator` or, where genuinely process-unique infrastructure tokens are required (e.g. lock owner tokens), MUST use `{MachineName}:{ProcessId}:{Interlocked counter}` shape.

**Why**: $9 is a project-wide canonical rule. Phase 1.5 added the MI-1 owner-token shape as the canonical alternative for non-replayable process-local uniqueness needs.

**How to apply**: Pre-execution guard $1a will fail any new file containing forbidden patterns. The exemption list is the canonical clock implementation (`SystemClock.cs`) and the determinism guard documentation (`DeterminismGuard.cs`).
**Source:** phase1.5-runtime.guard.md

#### R-RT-07 — Enforcement gate must run before the middleware pipeline
`RuntimeEnforcementGate.Evaluate` MUST be invoked AFTER the degraded stamp (HC-7) and BEFORE the middleware pipeline. Block decisions (`BlockMaintenance`, `BlockRestricted`) MUST return `CommandResult.Failure(...)` BEFORE any middleware runs.

**Why**: HC-8. Rejecting a maintenance request post-validation, post-policy, or post-idempotency would charge the system for work that should have been refused at the door.

**How to apply**: Any reordering that moves the enforcement gate inside or after the middleware pipeline is a guard violation. Rule order inside `RuntimeEnforcementGate.Evaluate` is also locked: maintenance dominates degraded; restricted-during-degraded dominates plain degraded.
**Source:** phase1.5-runtime.guard.md

#### R-RT-08 — `CommandContext` write-once fields stay write-once
`CommandContext.PolicyDecisionAllowed`, `PolicyDecisionHash`, `Hsid`, `IdentityId`, `TrustScore`, `DegradedMode` (HC-7), and `IsExecutionRestricted` (HC-8) MUST remain write-once. Adding a free-form metadata bag is forbidden.

**Why**: Write-once invariants are the cornerstone of replay determinism and audit traceability. A free-form bag would erase the typed-field discipline and reintroduce the drift that §5.1.x closed.

**How to apply**: Any new typed field on `CommandContext` MUST follow the existing write-once pattern (private backing + setter throws on second write). New consumers MUST NOT mutate existing write-once fields after they have been stamped.
**Source:** phase1.5-runtime.guard.md

#### R-RT-09 — Dependency graph respects DG-R5-EXCEPT-01
Runtime → Host edges remain forbidden. Contracts that the runtime control plane consumes (`IRuntimeStateAggregator`, `IRuntimeMaintenanceModeProvider`, `IExecutionLockProvider`) MUST live in `Whyce.Shared.Contracts`. Concrete implementations live in `Whyce.Platform.Host`.

**Why**: §5.1.1 D1/D4 closed this. HC-7 / HC-8 / HC-9 / MI-1 all preserved it by placing every new contract in `Whyce.Shared`.

**How to apply**: New runtime-side dependencies MUST follow the contract-in-shared / concrete-in-host pattern. `bash scripts/dependency-check.sh` MUST exit 0.
**Source:** phase1.5-runtime.guard.md

#### R-RT-10 — Catch + rethrow allowance (observability instrumentation)
A `catch` clause naming a typed exception that the WBSM rules require to "travel untouched" (currently `ConcurrencyConflictException`, extensible) is permitted ONLY when its body is a **pure rethrow**:

- the body contains a bare `throw;` (rethrows the same exception unmodified), AND
- the body contains no `return` statement, AND
- the body contains no `throw new` (transformed exception).

Side-effecting statements before the `throw;` (e.g. histogram outcome tagging, structured logging, metric increment) are explicitly allowed — they do not affect exception flow.

**Why**: A pure-rethrow catch is observability instrumentation, not control flow. The exception still travels untouched from the throw site to the canonical edge handler. Rejecting catch+rethrow forces authors to either (a) use exception filters (`when` clauses cannot record outcome state without branching) or (b) duplicate observation logic into a `finally` block, both of which are strictly worse than the canonical pattern in `PostgresEventStoreAdapter.cs:237`.

**Counter-examples (still violations)**:

```csharp
catch (ConcurrencyConflictException) { return CommandResult.Failure(...); }   // swallows
catch (ConcurrencyConflictException ex) { throw new SomethingElse(ex); }      // transforms
catch (ConcurrencyConflictException) { /* nothing */ }                         // swallows
```

**Allowed**:

```csharp
catch (ConcurrencyConflictException) when (outcome == "ok")
{
    outcome = "concurrency_conflict";
    throw;
}
```

**How to apply**: `WbsmArchitectureTests.No_upstream_layer_catches_ConcurrencyConflictException` enforces this via `IsPureRethrowCatchHit`. Any extension of the rule to additional exception types MUST use the same predicate so the allowance is uniform across the canonical "must travel untouched" exception family.
**Source:** phase1.5-runtime.guard.md

**Phase 1.5 Lock conditions:** LOCKED 2026-04-09. Any future workstream that needs to amend a rule above MUST:
1. Open a `claude/new-rules/{ts}-runtime.md` capture with `STATUS: PROPOSED`.
2. Reference the specific R-RT-* rule being amended.
3. Include a regression-coverage test that locks the new behavior.
4. Update this file in the same patch as the amendment.

---

### Section: Composition & Loading

#### G-COMPLOAD-01 — Registry Membership
FAIL IF any class implementing `ICompositionModule` is not listed in `src/platform/host/composition/registry/CompositionRegistry.cs`.
**Source:** composition-loader.guard.md
**Severity:** S1

#### G-COMPLOAD-02 — Explicit Order
FAIL IF any `ICompositionModule` implementation does not define a unique, non-negative integer `Order`. Duplicate or missing `Order` values are S1.
**Source:** composition-loader.guard.md
**Severity:** S1

#### G-COMPLOAD-03 — Locked Execution Sequence
FAIL IF the registry order deviates from:
`Core(0) → Runtime(1) → Infrastructure(2) → Projections(3) → Observability(4)`.
Adding a new module requires extending this sequence and updating this guard.
**Source:** composition-loader.guard.md
**Severity:** S1

#### G-COMPLOAD-04 — Loader-Only Composition
FAIL IF `Program.cs` re-introduces direct `Add*Composition(...)` calls instead of `builder.Services.LoadModules(builder.Configuration)`.
**Source:** composition-loader.guard.md
**Severity:** S1

#### G-COMPLOAD-05 — BootstrapModuleCatalog Preserved
FAIL IF the `BootstrapModuleCatalog.All` registration loop is removed from `Program.cs` or migrated into the composition loader. Domain bootstrap MUST remain a separate, explicit pass.
**Source:** composition-loader.guard.md
**Severity:** S1

#### G-COMPLOAD-06 — No Reflection Discovery
FAIL IF the loader, registry, or any composition module discovers types via reflection (`Assembly.GetTypes`, `Activator.CreateInstance`, attribute scans, etc.). Module enumeration is explicit list literals only.
**Source:** composition-loader.guard.md
**Severity:** S1

#### G-COMPLOAD-07 — Modules Are Orchestration-Only
FAIL IF any `ICompositionModule.Register` body contains anything beyond a single delegating call to its category `Add*Composition` extension. No `new`, no `services.AddSingleton<...>` calls inside modules themselves.
**Source:** composition-loader.guard.md
**Severity:** S1

#### G-PROGCOMP-01 — Composition Only
FAIL IF `Program.cs` contains any of:
- `builder.Services.AddSingleton<` / `AddTransient<` / `AddScoped<`
- `builder.Services.AddHostedService(`
- Direct `new` of any infrastructure adapter or middleware
- `Configuration.GetValue<` / `Configuration["..."]` reads

ALLOWED:
- `WebApplication.CreateBuilder` and `builder.Build()`
- Calls to `Add*Composition(...)` extension methods
- Calls to `LoadModules(...)` from `CompositionModuleLoader` (deterministic registry walk)
- Calls to bootstrap module `RegisterServices` from `BootstrapModuleCatalog`
- HTTP pipeline configuration (`app.Use*`, `app.Map*`)
- `app.Run()`

**Source:** program-composition.guard.md
**Severity:** S1

#### G-PROGCOMP-02 — Size Cap
`Program.cs` MUST NOT exceed 100 non-empty lines. Re-extract before crossing this threshold.
**Source:** program-composition.guard.md
**Severity:** S2

#### G-PROGCOMP-03 — Classification-Aligned Domain Wiring
Domain registration MUST flow through `IDomainBootstrapModule` instances listed in `BootstrapModuleCatalog`. No domain type may be referenced directly from `Program.cs` or from any non-domain composition module.
**Source:** program-composition.guard.md
**Severity:** S1

#### G-PROGCOMP-04 — No Inline Middleware Definition
`Program.cs` MUST NOT define new middleware classes inline or via lambdas that contain business logic. Middleware composition belongs in `composition/runtime/RuntimeComposition.cs`.
**Source:** program-composition.guard.md
**Severity:** S1

#### G-PROGCOMP-05 — Locked Pipeline Order
The HTTP pipeline order in `Program.cs` MUST remain:
`HttpMetricsMiddleware → UseRouting → UseSwagger → UseSwaggerUI → MapControllers → MapMetrics → Run`. The locked runtime middleware order inside `RuntimeComposition` is enforced by this guard's Runtime Order & Lifecycle section.
**Source:** program-composition.guard.md
**Severity:** S1

---

### Section: Engine Purity

#### E1 — TIER CLASSIFICATION
Every engine must be classified into exactly one tier:
- **T0U (Utility)**: Stateless utility operations. No domain imports. Pure computation, formatting, transformation. Examples: hashing, serialization helpers, ID generation.
- **T1M (Mediation)**: Workflow coordination. Imports domain types for routing decisions. Orchestrates command/query flow. Does not execute domain operations directly.
- **T2E (Execution)**: Domain operation execution. Imports and invokes domain aggregates, services, and specifications. The primary tier for business operation execution.
- **T3I (Integration)**: External system adapters. Wraps third-party APIs, file systems, external services. Translates external data to/from domain types.
- **T4A (Automation)**: Scheduled and background processing. Cron jobs, background workers, saga orchestration. Triggers operations on time or event schedules.

**Source:** engine.guard.md rule 1

#### E2 — T0U: NO DOMAIN IMPORTS
T0U engines must not reference any type from `src/domain/`. They operate on primitive types and shared kernel types only. T0U is the lowest tier with the fewest permissions.
**Source:** engine.guard.md rule 2

#### E3 — T1M: NO DIRECT DOMAIN MUTATION
T1M engines may reference domain types for routing and workflow decisions but must not call domain aggregate methods that mutate state. T1M reads domain state for orchestration; T2E executes mutations.
**Source:** engine.guard.md rule 3

#### E4 — T2E: DOMAIN EXECUTION ONLY
T2E engines invoke domain aggregates, apply commands, and return results. T2E must not perform external integration (that is T3I) or scheduling (that is T4A). T2E is the workhorse tier.
**Source:** engine.guard.md rule 4

#### E5 — T3I: EXTERNAL BOUNDARY
T3I engines wrap all external system interactions. They translate external DTOs to domain-compatible types. T3I must not contain domain business rules — only mapping and protocol translation.
**Source:** engine.guard.md rule 5

#### E6 — T4A: SCHEDULE AND TRIGGER
T4A engines define scheduled jobs, background workers, and process managers. They dispatch commands through runtime, not execute domain logic directly. T4A triggers T2E operations via runtime.
**Source:** engine.guard.md rule 6

#### E7 — NO CROSS-TIER ENGINE IMPORTS
Cross-tier engine imports are FORBIDDEN (T0U ↔ T1M ↔ T2E ↔ T3I ↔ T4A). Same-tier internal helper imports (e.g. T1M.WorkflowEngine importing T1M.Lifecycle, T1M.StepExecutor) are PERMITTED provided the imported namespace shares the tier prefix. Cross-engine coordination across tiers is achieved through runtime. Lint: `using Whyce.Engines.<TIER>.*;` is allowed only inside files under `src/engines/<TIER>/**`.
**Source:** engine.guard.md rule 7 (amended 2026-04-13)

#### E8 — ENGINES NEVER DEFINE DOMAIN MODELS
Engines must not define aggregate roots, entities, value objects, or domain events. These artifacts belong exclusively to `src/domain/`. Engines may define DTOs, result types, and handler-specific request/response types.
**Source:** engine.guard.md rule 8

#### E9 — ENGINES ARE STATELESS
Engine classes must not hold mutable instance state across invocations. No caching in engine fields, no accumulated state, no instance-level counters. Each invocation is independent. State is held by domain aggregates or external stores.
**Source:** engine.guard.md rule 9

#### E10 — ENGINE FOLDER STRUCTURE
Each engine must reside in a folder indicating its tier: `src/engines/{tier}/{engine-name}/`. The tier prefix (T0U, T1M, T2E, T3I, T4A) must be explicit in the folder path or engine class name.
**Source:** engine.guard.md rule 10

#### E11 — ENGINE INPUT/OUTPUT TYPES
Engines receive commands/queries as input and return result types as output. Engines must not accept or return raw infrastructure types (HttpRequest, DbDataReader). Input/output types are defined in the engine or shared layer.
**Source:** engine.guard.md rule 11

#### E12 — ENGINE TESTABILITY
Every engine must be independently testable. Dependencies must be injected via constructor. No hidden dependencies, no service locator pattern, no static factory calls for runtime services.
**Source:** engine.guard.md rule 12

#### E13 — NO PERSISTENCE IN ENGINES
Engines must not write to any database, file system, or durable store. No `DbContext.SaveChanges()`, `IRepository.Save()`, `File.Write()`, or storage SDK calls. Engines receive input, execute logic, and return results (including events). Persistence is the responsibility of the runtime pipeline and infrastructure adapters. T2E engines NEVER persist state — they ONLY emit events via `EngineContext.EmitEvents()`. Runtime is the ONLY layer allowed to persist, publish, and anchor.
**Source:** engine.guard.md rule 13

#### E14 — EVENT EMISSION ONLY OUTPUT
Engines return domain events as their primary output. The result of engine execution is a set of domain events representing what happened, not a persisted state change. Runtime is responsible for persisting events and dispatching them. Engines produce events; runtime commits them.
**Source:** engine.guard.md rule 14

#### E15 — ENGINECONTEXT SURFACE RESTRICTION
`EngineContext` is the sole interface engines use to interact with aggregate state. EngineContext exposes ONLY:
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
**Source:** engine.guard.md rule 15

#### E16 — POLICY PRE-CONDITION REQUIRED
Engines must assume that policy evaluation has already occurred before the command reaches them. Engines must not evaluate policies, check authorization, or verify actor permissions. If a command arrives at an engine, it is already authorized. Policy enforcement is a runtime middleware concern.
**Source:** engine.guard.md rule 16

#### E-WORKFLOW-01 — Workflow Orchestration Location
Workflow orchestration belongs ONLY in src/engines/T1M/. No WorkflowEngine or workflow execution logic in src/runtime/ or src/systems/. Runtime delegates to IWorkflowEngine (T1MWorkflowEngine).
**Source:** engine.guard.md NEW RULES 2026-04-07

#### E-STEP-01 — Step Implementations
All IWorkflowStep implementations MUST live under src/engines/T1M/steps/. Forbidden in src/systems/** and src/runtime/**.
**Source:** engine.guard.md NEW RULES 2026-04-07

#### E-VERSION-01 — Aggregate Version Increments
Aggregate event versions MUST strictly increment (newVersion = currentVersion + 1). Engines MUST load prior aggregate state before applying mutating commands. Persistence layer MUST persist supplied version, not default/reset to 0.
**Source:** engine.guard.md NEW RULES 2026-04-07

#### E-STEP-02 — No Re-entry via Intent Dispatcher
T1M steps SHOULD aggregate intent results, NOT dispatch new commands via ISystemIntentDispatcher. Re-entry from step->runtime->engine requires explicit architectural approval.
**Source:** engine.guard.md NEW RULES 2026-04-07

#### E-LIFECYCLE-FACTORY-01 — T1M Uses Lifecycle Event Factory
T1M MUST NOT call mutating methods on `WorkflowExecutionAggregate` (or any other aggregate) directly — that would violate E3 ("T1M: NO DIRECT DOMAIN MUTATION"). Lifecycle events are produced via `WorkflowLifecycleEventFactory` (located at `src/engines/T1M/lifecycle/`), which constructs the event records itself. The aggregate exists solely as the canonical replay target (its `Apply` method reconstructs state from those events).
**Source:** engine.guard.md NEW RULES 2026-04-07
**Severity:** S1

#### E-RESUME-01 — Resume via Replay Service
Workflow resume MUST be driven by `IWorkflowExecutionReplayService` (replay-from-events). Direct state restoration from any read model, projection, or snapshot is forbidden. The replay service lives at `src/engines/T1M/lifecycle/WorkflowExecutionReplayService.cs` and may reference `Whycespace.Domain.OrchestrationSystem.Workflow.Execution.*` because it lives in the engine layer; the runtime dispatcher consumes only the shared contract `IWorkflowExecutionReplayService` + `WorkflowExecutionReplayState` DTO.
**Source:** engine.guard.md NEW RULES 2026-04-07
**Severity:** S1

#### E-RESUME-02 — Deterministic Next-Step Cursor
Resume MUST continue from the deterministic next-step cursor — defined as the count of `WorkflowStepCompletedEvent` instances on the loaded event stream. Resume MUST NOT re-execute already-completed steps. The aggregate's `CurrentStepIndex` is INSUFFICIENT as a cursor on its own because it collapses "started, no steps done" and "step 0 completed" to the same value; the replay service exposes `NextStepIndex` which is unambiguous.
**Source:** engine.guard.md NEW RULES 2026-04-07
**Severity:** S1

#### E-RESUME-03 — Resume Through Engine.ExecuteAsync
Resume MUST go through `T1MWorkflowEngine.ExecuteAsync` with `WorkflowExecutionContext.CurrentStepIndex` pre-populated from the replayed cursor. Adding a parallel `IWorkflowEngine.ResumeAsync` is forbidden — it would duplicate the lifecycle-event emission contract enforced by E-LIFECYCLE-FACTORY-01. The engine's existing gate (`if (startIndex == 0) EmitEvent(Started)`) ensures a resumed run does NOT re-emit `WorkflowExecutionStartedEvent`.
**Source:** engine.guard.md NEW RULES 2026-04-07
**Severity:** S1

#### E-STATE-01 — Started Event Carries Payload
`WorkflowExecutionStartedEvent` MUST carry the original `Payload` (typed as `object?`, default null for back-compat). The T1M engine MUST pass `context.Payload` into `WorkflowLifecycleEventFactory.Started(...)`. Without this, replay cannot reconstruct the original input and `WorkflowResumeCommand` cannot resume payload-dependent steps.
**Source:** engine.guard.md NEW RULES 2026-04-08
**Severity:** S1

#### E-STATE-02 — Step Completed Carries Output
`WorkflowStepCompletedEvent` MUST carry the step's `Output` (typed as `object?`, default null for back-compat). The T1M engine MUST pass `stepResult.Output` into `WorkflowLifecycleEventFactory.StepCompleted(...)`. Without this, replay cannot reconstruct `WorkflowExecutionContext.StepOutputs` and downstream steps that read prior outputs cannot be resumed.
**Source:** engine.guard.md NEW RULES 2026-04-08
**Severity:** S1

#### E-STATE-03 — Replay Reconstructs Payload + StepOutputs
`IWorkflowExecutionReplayService.ReplayAsync` MUST reconstruct both `Payload` (from the started event) and `StepOutputs` (a dictionary keyed on `StepName`, populated from each `WorkflowStepCompletedEvent.Output`). The dispatcher resume path MUST populate `WorkflowExecutionContext.Payload` and copy each `state.StepOutputs` entry into `executionContext.StepOutputs` (init-only on the property, member-mutable).

CAVEAT (not yet a guard): `Payload`/`Output` are statically `object?`, so `PostgresEventStoreAdapter`'s `JsonSerializer.Serialize(evt, evt.GetType())` round-trips them as `JsonElement` on Postgres-backed replay. In-process / in-memory replay preserves the original CLR reference. A typed-payload registry is required for typed Postgres-backed resume — tracked in `claude/new-rules/20260407-230000-workflow-resume-payload-and-test-coverage.md`.
**Source:** engine.guard.md NEW RULES 2026-04-08
**Severity:** S1

#### ENG-PURITY-01 — ENGINE PURITY (T2E)
T2E engines MUST be pure execution units.

ENFORCEMENT:
- MUST NOT persist data
- MUST NOT call runtime
- MUST NOT call infrastructure directly
- MUST only emit events

**Source:** engine.guard.md NEW RULES 2026-04-07 (NORMALIZATION)

#### ENG-DOMAIN-ALIGN-01 — STRICT DOMAIN ALIGNMENT
Each engine MUST map to a single domain aggregate.

ENFORCEMENT:
- No cross-domain logic inside a single engine
- One engine = one domain responsibility

**Source:** engine.guard.md NEW RULES 2026-04-07 (NORMALIZATION)

#### E-TYPE-01 — Payload Types Registered
Any CLR type that flows through a workflow lifecycle event as `Payload` or step `Output` MUST be registered in `IPayloadTypeRegistry` via the owning `IDomainBootstrapModule.RegisterPayloadTypes`. Unregistered types round-trip as `JsonElement` on Postgres-backed replay (current legacy behavior, preserved for back-compat) and CANNOT be consumed by resumed steps that expect a typed object.
**Source:** engine.guard.md NEW RULES 2026-04-07 (H10 TYPE SAFETY)
**Severity:** S1

#### E-TYPE-02 — PayloadType / OutputType Discriminators
`WorkflowExecutionStartedEvent` and `WorkflowStepCompletedEvent` MUST carry `PayloadType` / `OutputType` discriminator strings stamped by `WorkflowLifecycleEventFactory` at write time when (a) the payload/output is non-null and (b) the type is registered in `IPayloadTypeRegistry`. The factory MUST consult the registry via `TryGetName` and emit a null discriminator on miss (back-compat), never throw.
**Source:** engine.guard.md NEW RULES 2026-04-07 (H10 TYPE SAFETY)
**Severity:** S1

#### E-TYPE-03 — Replay Rehydrates from Discriminator
`WorkflowExecutionReplayService.ReplayAsync` MUST rehydrate `Payload` and step `Output` values from `JsonElement` back into the registered CLR type when (a) the value is a `JsonElement` and (b) the corresponding `PayloadType` / `OutputType` discriminator is non-null. Resolution MUST go through `IPayloadTypeRegistry.Resolve` (strict — throws on unknown). The deserialization seam MUST live in the engine layer (`src/engines/T1M/lifecycle/`), NOT in `src/runtime/event-fabric/EventDeserializer` or `src/platform/host/adapters/PostgresEventStoreAdapter` (rule 11.R-DOM-01 forbids concrete domain references in those paths).
**Source:** engine.guard.md NEW RULES 2026-04-07 (H10 TYPE SAFETY)
**Severity:** S1

#### E-LIFECYCLE-FACTORY-CALL-SITE-01 — No Aggregate Command Methods in Engines
T1M engines (and any future stateless engine tier) MUST NOT invoke command methods on domain aggregates. Allowed aggregate interactions are limited to read-only query methods/properties and `LoadFromHistory()` for replay reconstruction. State transitions a T1M engine wishes to express MUST be produced by calling a lifecycle event factory method that returns the corresponding event record. PAIRING REQUIREMENT: for every public command method `Aggregate.X(...)`, there MUST exist a matching `LifecycleFactory.X(...)` returning the event that command would have raised. Adding a new aggregate command without adding the factory method is a guard violation. Static check: in `src/engines/**`, flag any method call whose receiver type derives from `AggregateRoot` outside the read-only allowlist. Architecture-test pairing assertion in `WbsmArchitectureTests`. Source: `_archives/20260408-103326-engines.md` (originating violation: `src/engines/T1M/lifecycle/WorkflowExecutionReplayService.cs:111` calling `aggregate.Resume()` because `WorkflowLifecycleEventFactory.Resumed()` was missing).
**Source:** engine.guard.md NEW RULES 2026-04-10
**Severity:** S1

---

### Section: Projections

Enforce the Dual Projection Architecture across WBSM v3. Two projection layers exist with strictly separated responsibilities:
1. **Runtime Projections** (`src/runtime/projection/`) — internal execution support ONLY
2. **Domain Projections** (`src/projections/`) — business-facing read models / CQRS query layer

Both layers are mandatory, isolated, non-overlapping, and strictly enforced.

#### PART A — SHARED PROJECTION RULES (Apply to BOTH layers)

#### P1 — PROJECTIONS ARE READ-ONLY
Projection handlers must only write to their own dedicated read-model store. They must not write to the domain's write-model store, modify aggregate state, or trigger domain side effects. A projection's output is a denormalized view optimized for queries.
**Source:** projection.guard.md rule 1

#### P2 — PROJECTIONS CONSUME EVENTS ONLY
Projection handlers must subscribe to domain events (past-tense). They must never consume commands, invoke command handlers, or listen for request types. The input to a projection is always `{Subject}{PastTenseVerb}Event`.
**Source:** projection.guard.md rule 2

#### P3 — NO WRITE OPERATIONS IN PROJECTIONS
Projection handlers must not:
- Call domain aggregate methods.
- Invoke command handlers or dispatch commands.
- Publish new domain events.
- Call repository `Save()`, `Update()`, `Delete()` on domain aggregates.
- Write to external systems (APIs, queues, files) as a side effect.
The only permitted write is upserting the projection's own read model.
**Source:** projection.guard.md rule 3

#### P4 — PROJECTIONS ARE EVENTUALLY CONSISTENT
Projections acknowledge that their data may be stale. No projection may be used as the authoritative source for write-side decisions. Read models must not be fed back into command validation. The write model (domain aggregates) is the single source of truth.
**Source:** projection.guard.md rule 4

#### P5 — PROJECTION IDEMPOTENCY
Projection handlers must be idempotent: processing the same event twice must produce the same read-model state. Projections must handle duplicate delivery gracefully. Use event sequence numbers or idempotency keys.
**Source:** projection.guard.md rule 5

#### P6 — PROJECTION REBUILDS
Projections must support full rebuild from the event stream. No projection may rely on state that is not derivable from the event history. If the read model is deleted, replaying all events must reconstruct it identically.
**Source:** projection.guard.md rule 6

#### P7 — ONE PROJECTION PER READ MODEL
Each projection handler maps to exactly one read model. A single handler must not update multiple unrelated read models. If multiple views need the same event, create separate projection handlers.
**Source:** projection.guard.md rule 7

#### P8 — NO DOMAIN LOGIC IN PROJECTIONS
Projections must not contain domain business rules. They flatten, denormalize, and aggregate event data — but they do not evaluate business conditions or enforce invariants. If a projection contains `if (status == Approved && amount > threshold)` to make a business decision, it is a violation.
**Source:** projection.guard.md rule 8

#### P9 — PROJECTION NAMING CONVENTION
Projection handlers must follow the naming pattern: `{ReadModel}Projection` or `{ReadModel}ProjectionHandler`. Read models must follow: `{Entity}{View}ReadModel` or `{Query}View`. Names must clearly indicate their read-model purpose.
**Source:** projection.guard.md rule 9

#### P10 — PROJECTION DATA IS NON-AUTHORITATIVE
Projection read models must never be treated as the source of truth for write-side decisions. No command handler, engine, or domain service may query a projection store to make a business decision. The event store and domain aggregates are the sole authoritative sources.
**Source:** projection.guard.md rule 10

#### P11 — STRICT CQRS SEPARATION
The write path (command > runtime > engine > domain > event store) and the read path (event > projection > read model > query) must be completely independent. No shared database tables, no shared connections, no shared transaction contexts between write and read paths.
**Source:** projection.guard.md rule 11

#### P12 — EVENT ORDERING GUARANTEE REQUIRED
Projections must process events in the order they were produced per aggregate. Out-of-order event processing produces corrupted read models. Projections must track the last processed event sequence number and reject or requeue events that arrive out of order.
**Source:** projection.guard.md rule 12

#### P13 — CONTEXT FIELDS REQUIRED
All projection handlers MUST include in their event processing context:
- `CorrelationId`
- `EventId`
- `IdempotencyKey`

**Source:** projection.guard.md rule 13

#### PART B — RUNTIME PROJECTION RULES (`src/runtime/projection/`)

#### P14 — RUNTIME PROJECTIONS ARE EXECUTION SUPPORT ONLY
Runtime projections serve internal execution needs: workflow state tracking, idempotency tracking, policy linking, runtime context views. They are NOT business-facing read models.
**Source:** projection.guard.md rule 14

#### P15 — RUNTIME PROJECTIONS NOT EXPOSED EXTERNALLY
Runtime projections must NOT be exposed via API endpoints, query handlers, or any external-facing interface. They are internal to the runtime control plane.
**Source:** projection.guard.md rule 15

#### P16 — RUNTIME PROJECTIONS MAY BE SYNCHRONOUS
Unlike domain projections, runtime projections may use synchronous event processing when required for execution support.
**Source:** projection.guard.md rule 16

#### P17 — RUNTIME PROJECTIONS MUST NOT WRITE TO REDIS
Runtime projections must NOT write to Redis or any shared read-model store. They use internal ephemeral state only (unless strictly internal ephemeral state is required).
**Source:** projection.guard.md rule 17

#### P18 — RUNTIME PROJECTIONS OWN INTERNAL STATE ONLY
Runtime projections may only access:
- Runtime internal modules
- Shared contracts (`src/shared/`)

**Source:** projection.guard.md rule 18

#### P19 — RUNTIME PROJECTIONS MUST NOT REFERENCE DOMAIN PROJECTIONS
`src/runtime/projection/` must NEVER reference `src/projections/`. No imports, no shared handlers, no cross-layer coupling.
**Source:** projection.guard.md rule 19

#### PART C — DOMAIN PROJECTION RULES (`src/projections/`)

#### P20 — DOMAIN PROJECTIONS ARE EVENT-DRIVEN ONLY
All domain projection handlers MUST consume events via Kafka/event fabric. No direct method invocation from runtime, systems, or any other layer. No synchronous event processing.
**Source:** projection.guard.md rule 20

#### P21 — DOMAIN PROJECTIONS MUST NOT REFERENCE RUNTIME
`src/projections/` must NEVER reference `src/runtime/`. No imports, no shared state, no cross-layer coupling.
**Source:** projection.guard.md rule 21

#### P22 — DOMAIN PROJECTIONS MUST NOT REFERENCE DOMAIN
`src/projections/` must NEVER reference `src/domain/`. Domain projections consume event schemas and shared contracts only.
**Source:** projection.guard.md rule 22

#### P23 — DOMAIN PROJECTIONS MUST NOT REFERENCE ENGINES
`src/projections/` must NEVER reference `src/engines/`. No engine imports or dependencies.
**Source:** projection.guard.md rule 23

#### P24 — DOMAIN PROJECTIONS ALLOWED DEPENDENCIES
`src/projections/` may ONLY reference:
- `src/shared/` (contracts, primitives)
- `infrastructure/` adapters (Redis clients, persistence adapters)
- Event schemas

**Source:** projection.guard.md rule 24

#### P25 — DOMAIN PROJECTIONS OWN REDIS/READ-STORE
Redis and materialized view writes MUST originate ONLY from `src/projections/`. No other layer may write to the domain read-model store.
**Source:** projection.guard.md rule 25

#### P26 — DOMAIN PROJECTIONS ARE THE ONLY QUERY SOURCE
All external query endpoints (API layer) MUST read from `src/projections/` read models. No API endpoint may query runtime projections directly.
**Source:** projection.guard.md rule 26

#### P27 — DOMAIN PROJECTIONS MUST SUPPORT REPLAY
Domain projections must be event-versioning safe. Replaying the full event stream must reconstruct the read model identically. Event reprocessing must be safe and idempotent.
**Source:** projection.guard.md rule 27

#### PART D — PROJECTION ISOLATION GUARD (S24)

#### P28 — DEPENDENCY ISOLATION
- `src/runtime/` MUST NOT reference `src/projections/`
- `src/projections/` MUST NOT reference `src/runtime/`
- `src/projections/` MUST NOT reference `src/domain/`
- `src/projections/` MUST NOT reference `src/engines/`

**Source:** projection.guard.md rule 28

#### P29 — EVENT-DRIVEN ENFORCEMENT
- All `src/projections/` handlers MUST consume events ONLY
- NO direct method invocation from runtime into domain projections

**Source:** projection.guard.md rule 29

#### P30 — STORAGE OWNERSHIP
- Redis/read-store writes ONLY from `src/projections/`
- Runtime projections MUST NOT write to Redis

**Source:** projection.guard.md rule 30

#### P31 — EXPOSURE RULES
- `src/runtime/projection/` MUST NOT be exposed via API
- `src/projections/` MUST be the ONLY query source for external consumers

**Source:** projection.guard.md rule 31

#### P32 — IDEMPOTENCY ENFORCEMENT
- All projection handlers MUST be idempotent
- Must support replay (event versioning safe)

**Source:** projection.guard.md rule 32

#### P-TYPE-ALIGN-01 — Projection Type Alignment
Projection bridges MUST match on the same event types the domain layer emits. Either (a) EventFabric maps domain events to schema types before dispatch, or (b) bridges match domain types directly. Type alignment between emit and consume MUST be verified at registration time. Silent type-mismatch drops are S1 violations.
**Source:** projection.guard.md NEW RULES 2026-04-07

#### P-AGNOSTIC-01 — Runtime Projection Bridge Agnostic
Runtime projection bridges MUST be event-type-agnostic — no "using" of concrete domain/schema types inside src/runtime/projection/bridges/**.
**Source:** projection.guard.md NEW RULES 2026-04-07

#### PROJ-READ-ONLY-01 — PROJECTIONS ARE READ ONLY
Projections MUST NOT modify domain state.

ENFORCEMENT:
- No write-back to domain
- No aggregate mutation

**Source:** projection.guard.md NEW RULES 2026-04-07 (NORMALIZATION)

#### PROJ-DOMAIN-ALIGN-01 — DOMAIN ALIGNED STRUCTURE
Projections MUST follow domain-aligned folder structure.
**Source:** projection.guard.md NEW RULES 2026-04-07 (NORMALIZATION)

#### PROJ-WF-EXEC-01 — WORKFLOW EXECUTION PROJECTION
The `src/projections/orchestration-system/workflow/` projection layer is the ONLY query source for workflow execution state. Runtime is forbidden from persisting, mutating, or caching workflow lifecycle state.

ENFORCEMENT:
- `WorkflowExecutionProjectionHandler` consumes the four lifecycle event schemas (`WorkflowExecutionStartedEventSchema`, `WorkflowStepCompletedEventSchema`, `WorkflowExecutionCompletedEventSchema`, `WorkflowExecutionFailedEventSchema`) defined in `src/shared/contracts/events/orchestration-system/workflow/`.
- The handler writes ONLY to `IWorkflowExecutionProjectionStore` (`src/shared/contracts/projections/orchestration-system/workflow/`).
- Production wiring uses `InMemoryWorkflowExecutionProjectionStore` as a PLACEHOLDER (T-PLACEHOLDER-01) until migration `scripts/migrations/002_create_workflow_execution_projection.sql` is applied and a Postgres-backed adapter is provided.
- Any runtime-side type that re-introduces a `WorkflowState*` mutator, observer, or in-memory store under `src/runtime/**` is an S1 violation.

**Source:** projection.guard.md NEW RULES 2026-04-07 (NORMALIZATION)

#### PROJ-REPLAY-SAFE-01 — Projection Handlers Replay-Safe
Projection handlers MUST be replay-safe. Lifecycle handlers (`WorkflowStepCompletedEventSchema`, `WorkflowExecutionCompletedEventSchema`, `WorkflowExecutionFailedEventSchema`, and all equivalents) MUST upsert on missing read-model rows rather than throwing `InvalidOperationException`. Alternatively, a documented out-of-order buffering mechanism MUST exist. Throwing on missing state crashes the consumer on dropped / out-of-order / partial-offset rebuild paths.
**Source:** projection.guard.md NEW RULES 2026-04-08
**Severity:** S2

#### PROJ-NO-INPLACE-MUTATION-01 — No In-Place Mutation
Projection handlers MUST NOT mutate state returned from a projection store in place (e.g. `existing.StepOutputs[e.StepName] = e.Output`). Either the store contract guarantees a fresh-copy return, OR handlers construct a new collection before mutation. `with` expressions do not clone dictionary/list members — they shallow-copy the record only.
**Source:** projection.guard.md NEW RULES 2026-04-08
**Severity:** S2

---

### Section: Determinism Core

This section consolidates WBSM v3 determinism rules covering `src/domain/**`, `src/engines/**`, `src/runtime/**`, `src/systems/**`, and `src/platform/host/adapters/**` (the platform adapter surface). Adapters are the persistence and event-fabric boundary; non-determinism here breaks event-sourcing replay guarantees, idempotency, deduplication, and chain anchoring even when domain/engine/runtime code is perfectly deterministic.

#### DET-BLOCKLIST — Block List
Within all in-scope paths, the following are FORBIDDEN:

- `Guid.NewGuid()`
- `Guid.NewGuid().ToString(...)`
- `DateTime.Now`
- `DateTime.UtcNow`
- `DateTimeOffset.Now`
- `DateTimeOffset.UtcNow`
- `Random` instantiation, `Random.Shared`, `RandomNumberGenerator.GetBytes(...)` for non-cryptographic identity/sequence generation
- `Environment.TickCount`, `Environment.TickCount64`
- `Stopwatch.GetTimestamp()` / `Stopwatch.GetElapsedTime()` used as an identity, event-stamp, or hash-input source

**Source:** determinism.guard.md

#### DET-REQUIRED-REPLACEMENTS — Required Replacements
- For identity: `IIdGenerator.Generate(seed)` from `src/shared/kernel/domain/IIdGenerator.cs`. The seed MUST be derived deterministically from the operation's coordinates — for example `$"{aggregateId}:{version}"` for an event store row id, or `$"{commandId}:{handlerName}"` for a command-derived child id. Random or wall-clock seeds defeat the purpose.
- For time: `IClock.UtcNow` from `src/shared/kernel/domain/IClock.cs`.

Both seams are DI-registered as singletons in `src/platform/host/Program.cs` (`SystemClock` → `IClock`, `DeterministicIdGenerator` → `IIdGenerator`). There is no excuse for a constructor to be missing them.
**Source:** determinism.guard.md

#### DET-EXCEPTIONS — Permitted Exception Surfaces
Exactly two surfaces are permitted to read the system clock or generate a non-derived id, and they form the boundary between deterministic application code and the underlying OS:

1. **The `IClock` implementation itself.** `SystemClock.UtcNow` in `src/platform/host/Program.cs` is the single permitted reader of `DateTimeOffset.UtcNow`. No other class.
2. **The `IIdGenerator` implementation itself.** Currently `DeterministicIdGenerator` in `src/platform/host/Program.cs`, which derives ids via `SHA256(seed)` and never reads the system clock or RNG. If a future implementation needs randomness, it must be confined to this single class.
3. **Stopwatch for observability instrumentation.** `Stopwatch.GetTimestamp()` / `Stopwatch.GetElapsedTime()` are PERMITTED solely for observability instrumentation (latency histograms, counters, traces). The resulting value MUST NOT flow into `ExecutionHash`, deterministic IDs, sequence seeds, chain block IDs, or any persisted event payload. Lint: any data flow from `Stopwatch` to a hash/id constructor is a DET violation.

SQL `NOW()` / `CURRENT_TIMESTAMP` inside SQL statements is permitted **only** for storage-layer operational timestamps (`created_at`, `projected_at`, `published_at`, `next_retry_at`) in infrastructure adapter SQL, provided:
- The timestamp is never consumed by domain logic, event replay, chain integrity, or deterministic ID generation
- Business-significant time is always supplied from the `IClock` seam via parameterized values
- The usage is confined to the platform/host adapter layer (never in domain or engine SQL)

If any new SQL timestamp is introduced that could affect replay correctness or business logic, it must use a parameterized timestamp from `IClock` instead of `NOW()`.
A SQL-clock value that flows back into an aggregate, projection key, event hash, or chain anchor is a violation.
**Source:** determinism.guard.md

#### DET-ADAPTER-01 — Adapter Block List Extended
Block list extended to src/platform/host/adapters/**. Forbidden: Guid.NewGuid(), DateTime.Now, DateTime.UtcNow, DateTimeOffset.Now, DateTimeOffset.UtcNow. Use IIdGenerator.Generate(seed) with deterministic seed derived from aggregate id/version/stream coordinate, and IClock.UtcNow.
**Source:** determinism.guard.md NEW RULES 2026-04-07

#### DET-EXCEPTION-01 — IClock Single Reader
The IClock implementation (SystemClock) is the ONLY permitted reader of DateTimeOffset.UtcNow. SQL NOW() / CURRENT_TIMESTAMP is permitted ONLY for audit columns the application does NOT read back into deterministic logic.
**Source:** determinism.guard.md NEW RULES 2026-04-07

#### DET-SEED-01 — Derived Seed for Adapter Rows
PostgresEventStoreAdapter row id MUST derive from "{aggregateId}:{version}" via IIdGenerator. Kafka projection envelopes MUST stamp Timestamp from IClock.UtcNow, not consume-moment wall clock.
**Source:** determinism.guard.md NEW RULES 2026-04-07

#### DET-DUAL-SEAM-01 — Two Deterministic Identity Seams
The "single permitted ID seam" wording is reconciled. TWO deterministic identity seams are now canonical with non-overlapping responsibilities:
(1) `IIdGenerator.Generate(seed)` — returns `Guid`, used for internal adapter/row/hash IDs, sole implementation `DeterministicIdGenerator` (SHA256 of seed → Guid).
(2) `IDeterministicIdEngine.Generate(...)` — returns compact string `PPP-LLLL-TTT-TOPOLOGY-SEQ` for external-facing correlation IDs, sole implementation `Whyce.Engines.T0U.Determinism.DeterministicIdEngine`. Both must remain free of `Guid.NewGuid`, `DateTime*.UtcNow`, `Random`, `Environment.Tick*`.
**Source:** determinism.guard.md NEW RULES 2026-04-07 (HSID v2.1 parallel seam)
**Severity:** S1

#### DET-HSID-CALLSITE-01 — HSID Call-site Restriction
`IDeterministicIdEngine.Generate(...)` MUST NOT be called outside `src/runtime/control-plane/` and `src/engines/T0U/determinism/`.
**Source:** determinism.guard.md NEW RULES 2026-04-07 (HSID v2.1 parallel seam)
**Severity:** S1

#### DET-SEED-DERIVATION-01 — Seed Composition
When invoking `IIdGenerator.Generate(seed)` (or any seam producing a deterministic identifier from a seed string), the seed MUST be composed exclusively of stable command coordinates (aggregate id, command type name, aggregate version, correlation/causation id, deterministic discriminators). FORBIDDEN seed components: `IClock.UtcNow`/`DateTime.*`/`Stopwatch.*`/`Ticks`, `Guid.NewGuid()`/`Random.*`/`RandomNumberGenerator.*`, process/thread/machine identifiers, env vars, or hashes thereof. Static check: search `IIdGenerator.Generate(` and flag any seed-string interpolation containing `Clock|Now|Ticks|Guid|Random`. Architecture-test enforcement under `tests/unit/architecture/WbsmArchitectureTests`. Rationale: non-deterministic seeds defeat the entire deterministic-id mechanism and silently break replay, projection idempotency, and chain integrity.
**Source:** determinism.guard.md NEW RULES 2026-04-10
**Severity:** S1

#### DET-IDCHECK-COVERAGE-01 — ID Check Coverage
`scripts/deterministic-id-check.sh` (or a sibling script) MUST scan `tests/**` and `scripts/validation/**` in addition to `src/**`. Test paths and validation harnesses are not exempt from determinism rules.
**Source:** determinism.guard.md NEW RULES 2026-04-10
**Severity:** S2

#### DET-STOPWATCH-OBSERVABILITY-01 — Stopwatch Observability Only
`Stopwatch.GetTimestamp()` / `GetElapsedTime()` are PERMITTED solely for observability instrumentation (latency histograms, counters, traces). The resulting value MUST NOT flow into `ExecutionHash`, deterministic IDs, sequence seeds, chain block IDs, or any persisted event payload. Lint: any data flow from `Stopwatch` to a hash/id constructor is a DET violation.
**Source:** determinism.guard.md NEW RULES 2026-04-13
**Severity:** S2

#### DET-SQL-NOW-ADDENDUM-01 — SQL NOW() Addendum
SQL `NOW()` / `CURRENT_TIMESTAMP` is acceptable in infrastructure adapter SQL for storage-layer operational timestamps (`created_at`, `projected_at`, `published_at`, `next_retry_at`) provided: (1) the timestamp is never consumed by domain logic, event replay, chain integrity, or deterministic ID generation; (2) business-significant time is always supplied from the `IClock` seam via parameterized values; (3) the usage is confined to the platform/host adapter layer (never in domain or engine SQL).
**Source:** determinism.guard.md NEW RULES 2026-04-13
**Severity:** S3

---

### Section: Deterministic IDs (HSID v2.1)

Locks the HSID v2.1 compact correlation-id format and its single source of truth. This section is the **second** deterministic identity guard alongside the Determinism Core section, and the two are intentionally non-overlapping:

| Seam | Output | Scope |
|------|--------|-------|
| `IIdGenerator.Generate(seed)` | `Guid` | Internal row ids, hash inputs, adapter envelopes |
| `IDeterministicIdEngine.Generate(...)` | compact `string` | External-facing correlation IDs of the form `PPP-LLLL-TTT-TOPOLOGY-SEQ` |

#### Locked Format

```
PPP-LLLL-TTT-TOPOLOGY-SEQ
```

| Segment | Width | Charset | Meaning |
|---------|-------|---------|---------|
| PPP | 3 | `[A-Z]` | `IdPrefix` enum name |
| LLLL | 4 | `[A-Z]` | `LocationCode` |
| TTT | 3 | `[A-Z0-9]` | Deterministic time bucket (SHA256 of seed) |
| TOPOLOGY | 12 | `[A-Z0-9]` | Cluster (3) + SubCluster (3) + SPV (6) |
| SEQ | 3 | `[A-Z0-9]` | Bounded sequence (`X3`, 0..0xFFF) |

Canonical regex:

```
^[A-Z]{3}-[A-Z]{4}-[A-Z0-9]{3}-[A-Z0-9]{12}-[A-Z0-9]{3}$
```

#### G1 — SINGLE ENGINE
All HSIDs MUST be produced by `Whyce.Engines.T0U.Determinism.DeterministicIdEngine`. No other class may construct an HSID literal or implement `IDeterministicIdEngine`.
**Source:** deterministic-id.guard.md

#### G2 — NO RANDOMNESS
The engine, the bucket provider, and the sequence resolver MUST NOT call `Guid.NewGuid`, `Random*`, `RandomNumberGenerator`, `DateTime*.UtcNow`, `DateTimeOffset*.UtcNow`, `Environment.Tick*`, or `Stopwatch.GetTimestamp`. The bucket is derived from the seed via SHA256.
**Source:** deterministic-id.guard.md

#### G3 — TOPOLOGY REQUIRED
Every HSID MUST encode a `TopologyCode` of exactly 12 characters: Cluster (3) + SubCluster (3) + SPV (6). A topology value with any other width is a violation.
**Source:** deterministic-id.guard.md

#### G4 — SEQUENCE BOUNDED + LAST
The sequence segment MUST be the LAST segment, MUST be exactly 3 hex chars (`X3`), and MUST be produced by an `ISequenceResolver` whose scope key includes BOTH the topology and the seed. Unbounded counters or sequences keyed only on time are forbidden.
**Source:** deterministic-id.guard.md

#### G5 — DOMAIN PURITY
No code under `src/domain/**` may inject or call `IDeterministicIdEngine`. The domain layer is forbidden from naming its own HSIDs.
**Source:** deterministic-id.guard.md

#### G6 — SINGLE STAMP POINT
`IDeterministicIdEngine.Generate(...)` may be called from EXACTLY two surfaces:
1. `src/runtime/control-plane/RuntimeControlPlane.cs` (the prelude that stamps `CommandContext.Hsid`).
2. `src/engines/T0U/determinism/**` (the engine itself, for self-tests).

A call from any other path is an architectural violation. The HSID is stamped before the locked 8-middleware pipeline runs and is write-once on `CommandContext.Hsid`.
**Source:** deterministic-id.guard.md

#### G7 — STRUCTURAL VALIDATION
Every produced HSID MUST pass `IDeterministicIdEngine.IsValid(id)` immediately after generation. The prelude in `RuntimeControlPlane` performs this check; new call sites MUST do the same.
**Source:** deterministic-id.guard.md

#### G8 — NO RUNTIME-ORDER MUTATION
This guard MUST NOT be used as a justification to add a 9th middleware to the locked pipeline. The HSID stamp lives in the control-plane prelude, out of band of the 8-stage pipeline.
**Source:** deterministic-id.guard.md

#### G12 — ENGINE REQUIRED (H2–H6 HARDENING)
`IDeterministicIdEngine`, `ISequenceResolver`, and `ITopologyResolver` MUST all be configured. The `RuntimeControlPlane` constructor throws on any null. There is no fallback, no nullable injection, no optional DI. NO ENGINE → NO EXECUTION.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

#### G13 — SEQUENCE SOURCE
The canonical sequence resolver is `PersistedSequenceResolver` backed by `ISequenceStore`. The previous `InMemorySequenceResolver` has been removed. Reintroducing an in-memory resolver in production composition is a violation; the test `InMemorySequenceStore` is permitted ONLY under `tests/integration/setup/`.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

#### G14 — TOPOLOGY TRUST
Topology MUST come from `ITopologyResolver` (via `IStructureRegistry`) for any command that implements `IHsidCommand`. Caller-supplied topology in a command body, request DTO, or HTTP header is forbidden. The fallback path (non-`IHsidCommand`) derives topology deterministically from `classification|context|domain` via SHA256 — this fallback is permitted but flagged by audit A14.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

#### G15 — PRELUDE ENFORCEMENT
HSID stamping MUST occur in the `RuntimeControlPlane` prelude, before the locked 8-middleware pipeline runs. No middleware may stamp or replace `CommandContext.Hsid`. The write-once setter on `CommandContext.Hsid` enforces this at runtime.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

#### G16 — SEQUENCE STORE
`ISequenceStore` MUST exist as a dedicated persistence contract. `IEventStore` MUST NOT be used for HSID sequence persistence. Cross-using the event store for sequence counters conflates two replay surfaces.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

#### G17 — HSID COMMAND INTERFACE
Commands MAY implement `IHsidCommand`. If implemented, `RuntimeControlPlane` MUST resolve topology via `ITopologyResolver`. A command that implements `IHsidCommand` but bypasses the resolver is a G14 violation.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

#### G18 — SEQUENCE WIDTH
Sequence segment MUST remain X3 (3 hex chars, 0..0xFFF). Any change to width MUST update this guard, the engine regex, and the audit regex in the SAME commit. Locked 2026-04-07.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

#### G19 — INFRASTRUCTURE READINESS
`ISequenceStore.HealthCheckAsync()` MUST be invoked at host bootstrap by `HsidInfrastructureValidator`. The runtime MUST NOT begin accepting traffic if the health check returns false or throws. Silent degradation is forbidden.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

#### G20 — MIGRATION REQUIRED
The `hsid_sequences` table MUST exist in every environment that runs the host. Its canonical migration lives at `infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql`. CI MUST run `scripts/hsid-infra-check.sh` against the target database before any deploy. VIOLATION = BLOCKER.
**Source:** deterministic-id.guard.md H2–H6 HARDENING 2026-04-07

---

### Section: Hash Determinism

Lock the inputs to `ExecutionHash` and `DecisionHash` to a deterministic, replay-stable set. Any change that introduces a timestamp, RNG value, unordered collection, or non-normalized field into a hash input is a critical violation. Hash determinism is the foundation of replay verification — if hashes drift, replay cannot prove anything.

**Scope:**
- `src/runtime/deterministic/ExecutionHash.cs`
- `src/runtime/deterministic/DeterministicHasher.cs` (if present)
- Any future `*Hash.cs` file under `src/runtime/deterministic/`
- The `DecisionHash` field/computation in policy evaluation (`src/engines/T0U/whycepolicy/` and the policy result type)

#### HASH-PERMITTED-INPUTS — Permitted Hash Inputs
Only the following classes of input may feed a hash:

1. **Stable identifiers** — `correlationId`, `commandId`, `aggregateId`, `policyId`, `identityId`, `tenantId`. These are deterministic per command and reproduced exactly on replay.
2. **Normalized identity context** — `roles` (joined in canonical sort order), `trustScore` (string-formatted with invariant culture), `policyVersion` (string).
3. **Policy decision artifacts** — `policyDecisionHash`, `policyDecisionAllowed` (as a stable string).
4. **Domain event content** — event type names + per-event payload hashes computed via `DeterministicHasher.ComputePayloadHash(...)`. Events must be hashed in their emission order, with the position index included so that two events of the same type at different positions produce different signatures.
5. **Counts** — `domainEvents.Count` as a string. Counts are derived from the event list and are themselves deterministic.

**Source:** hash-determinism.guard.md

#### HASH-FORBIDDEN-INPUTS — Forbidden Hash Inputs
The following are FORBIDDEN as direct or indirect inputs to any hash computed in scope:

- `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `DateTimeOffset.UtcNow` — wall clock readings vary per run.
- `IClock.UtcNow` — even though `IClock` is the canonical time seam, its value is not stable across replays. Time may be hashed by the *event payload* (because events carry their own deterministic stamps), but the hash function itself must not call into `IClock`.
- `Guid.NewGuid()` — non-deterministic by definition.
- `Random`, `Random.Shared`, `RandomNumberGenerator.GetBytes(...)` for hash salt — defeats determinism.
- `Environment.TickCount`, `Environment.TickCount64`, `Stopwatch.GetTimestamp()`.
- **Unordered collections** as inputs — `HashSet<T>`, `Dictionary<K,V>`, `ConcurrentDictionary<K,V>`, or any `IEnumerable<T>` that is not explicitly sorted before hashing. Iteration order of unordered collections is implementation-defined and may vary across runs and framework versions.
- **Non-normalized strings** — locale-sensitive `ToString()` calls (`(0.5).ToString()` rather than `(0.5).ToString(CultureInfo.InvariantCulture)`), case-sensitive comparisons of canonical identifiers without first applying a documented canonicalization, paths with mixed separators.
- **Reference equality** — hashing the result of `Object.GetHashCode()` or `RuntimeHelpers.GetHashCode()` couples the hash to memory layout.
- **Floating-point representations** — directly hashing `double`/`float` bit patterns. Use the canonical decimal string form via `InvariantCulture` round-trip format ("R" / "G17") only when absolutely necessary, and prefer `decimal` for any value that flows into a hash.

**Source:** hash-determinism.guard.md

#### HASH-REQUIRED-PATTERNS — Required Patterns
1. **Composite hashes** must be computed via `DeterministicHasher.ComputeCompositeHash(...)` (or equivalent). The composite function must concatenate inputs with a reserved separator that cannot appear in the inputs, so that `("ab", "c")` and `("a", "bc")` produce different hashes.
2. **Per-event signatures** must include the event's positional index, not just its type and payload. Two `TodoUpdatedEvent`s with the same payload at positions 0 and 1 must produce different per-event signatures.
3. **Sort before hash.** When hashing a collection, sort by a stable key first. The sort must be culture-invariant and document the comparator.
4. **Null sentinels.** When a field may be null, hash a fixed sentinel string (e.g. `"none"`, `"anonymous"`) rather than letting the null propagate. The sentinel must be unique enough that it cannot collide with a real value.

**Source:** hash-determinism.guard.md

#### HASH-CURRENT-COMPLIANCE — Current Compliance (capture date 2026-04-07)
`ExecutionHash.cs:23-61` is currently **compliant**. Inputs:

- `correlationId.ToString()`, `commandId.ToString()`, `aggregateId.ToString()` — stable ids
- `identityId ?? "anonymous"`, `roles` joined, `trustScore?.ToString() ?? "0"` — normalized identity
- `policyId`, `policyDecisionHash ?? "none"`, `policyDecisionAllowed?.ToString() ?? "false"`, `policyVersion ?? "none"` — policy artifacts with sentinels
- `eventSignatures` built as `$"{type}:{i}:{payloadHash}"` per event — position index included
- `domainEvents.Count.ToString()` — derived count

**Zero forbidden inputs.** No clock read, no RNG, no unordered collection, no locale-sensitive formatting. The file passes this guard by inspection.
**Source:** hash-determinism.guard.md

**Severity:** Any forbidden input reaching a hash function in scope is S0 — CRITICAL. Hash drift is silent and breaks replay verification without producing an error at the point of mistake.

---

### Section: Replay Determinism

Lock the design intent behind `EventReplayService.ReplayAsync` and protect the sentinel envelope fields it produces during projection rebuild. This section exists to prevent future passes from "fixing" sentinels that are intentional design markers.

**Scope:**
- `src/runtime/event-fabric/EventReplayService.cs`
- Any future `EventReplay*.cs` file under `src/runtime/event-fabric/`
- The audit document `claude/audits/replay-determinism.audit.md` which records the by-design rationale and is the source of truth for this guard.

**Background — Two Notions of Replay:**

- **Type A — Re-execution.** Run the same commands twice through the full RuntimeControlPlane → Engine → EventFabric pipeline. With a frozen `IClock` and the existing `DeterministicIdGenerator`, every envelope field including `ExecutionHash`, `PolicyHash`, and `Timestamp` is byte-equal between runs. This is the property protected by the Hash Determinism section above.

- **Type B — Projection rebuild.** Use `EventReplayService.ReplayAsync` to load events from the event store and dispatch them to projection handlers. This path **deliberately** sets sentinel values:
  - `PolicyHash = "replay"`
  - `ExecutionHash = "replay"`
  - `Timestamp = DateTimeOffset.MinValue`

  The sentinels signal to downstream consumers that the envelope is a rebuild artifact, not a fresh execution. They are a feature, not a bug.

#### REPLAY-SENTINEL-PROTECTED-01 — Sentinels are protected design artifacts
The three sentinel assignments in `EventReplayService.ReplayAsync` MUST remain in place. Any code change, prompt instruction, or audit finding that proposes replacing them with "real" envelope values is a violation of this guard and MUST be rejected at the guard-load stage.

**The protected statements are:**

```csharp
ExecutionHash = "replay",
PolicyHash    = "replay",
Timestamp     = DateTimeOffset.MinValue,
```

Located at `EventReplayService.cs:55-59`.

**Source:** replay-determinism.guard.md
**Severity:** S1 — HIGH (block merge)

#### REPLAY-SENTINEL-LIFT-01 — How to lift the protection
The protection is **not absolute**, but lifting it requires a documented design change, not a hardening fix. The path to changing the sentinel behavior is:

1. **First** update `claude/audits/replay-determinism.audit.md` to remove the by-design clause at lines 53-72 and record the new requirement that justifies the change. Without this update, no downstream change is permitted.
2. Extend `EventStoreService` (or its successor) to persist and return per-event envelope metadata (`PolicyHash`, `ExecutionHash`, `Timestamp`) at the time the events are appended to the store.
3. Modify `EventReplayService.ReplayAsync` to read those values from the store rather than reconstructing envelopes from raw events.

Steps 2 and 3 may not be performed in any commit that does not also contain step 1.

**Source:** replay-determinism.guard.md
**Severity:** S1 — HIGH (block merge)

#### REPLAY-A-vs-B-DISTINCTION-01 — Audits and prompts must respect the distinction
Any audit, prompt, or test that asserts envelope-field equality on replay MUST distinguish Type A (re-execution) from Type B (rebuild):

- For Type A: `ExecutionHash`, `PolicyHash`, and `Timestamp` MUST be byte-equal between runs under a frozen `IClock` and deterministic `IIdGenerator`. Failure here is a true determinism violation under the Hash Determinism / Determinism Core sections above.
- For Type B: `ExecutionHash`, `PolicyHash`, and `Timestamp` are sentinel values and MUST NOT be asserted equal to the originals. The fields that DO survive rebuild are `EventId`, `AggregateId`, `CorrelationId`, `EventType`, `Payload`, and `SequenceNumber`.

Asserting Type A equality on a Type B replay is a misclassification and a violation of this rule.

**Source:** replay-determinism.guard.md
**Severity:** S2 — MEDIUM (advisory in CI; block in audit)

#### POLICY-REPLAY-INTEGRITY-01 — No Policy Re-evaluation on Replay
`EventReplayService` MUST NOT re-evaluate policy during replay. Stored `PolicyEvaluatedEvent` / `PolicyDeniedEvent` records are the source of truth for replayed decisions. Re-evaluation would risk drift if policy versions or trust scores have changed since original evaluation.
**Source:** replay-determinism.guard.md NEW RULES 2026-04-07 (policy eventification)
**Severity:** S0

---

## WBSM v3 GLOBAL ENFORCEMENT

(Consolidated from runtime.guard.md, engine.guard.md, projection.guard.md — deduplicated)

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

## Consolidated Check Procedure

### Runtime Core
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

### Runtime Order
1. Open `RuntimeControlPlaneBuilder.cs`. Verify the body of `Build()` matches the 8-entry locked sequence.
2. Open `EventFabric.cs`. Verify `ProcessAsync` contains exactly three sequential `await` statements in the order persist → chain → outbox, with no `Task.WhenAll`, no discarded tasks, no conditional skips.
3. Grep `src/` for `engine.ExecuteAsync(` outside `src/runtime/dispatcher/`. Any hit is a violation.
4. Grep `src/` for direct calls to `IEventStore.AppendEventsAsync`, `IChainAnchor.AnchorAsync`, or `IOutbox.EnqueueAsync` outside `src/runtime/event-fabric/`.

### Engines
1. Enumerate all engine folders under `src/engines/` and verify tier classification.
2. For T0U engines: scan for any `using Domain.*` import — must be zero.
3. For T1M engines: scan for aggregate mutation calls (`.Apply()`, `.Create()`, `.Execute()` on aggregates).
4. For T2E engines: scan for HTTP client, external API, or file system calls — must be zero.
5. For T3I engines: scan for domain business rule logic (if/else on domain state for business decisions).
6. For T4A engines: scan for direct aggregate invocation — must dispatch via runtime commands.
7. Scan all engines for `using Engines.*` imports referencing engines of a different tier (cross-tier). Same-tier internal imports are permitted.
8. Scan all engines for aggregate, entity, value-object, or domain event class definitions.
9. Scan all engine classes for mutable instance fields (`private List<>`, `private int`, non-readonly fields).
10. Verify each engine class uses constructor injection only (no `IServiceProvider.GetService`).
11. Verify engine folder structure includes tier designation.
12. Scan all T2E engines for persistence calls — must use ONLY `context.EmitEvents()`.
13. Verify `EngineContext` class exposes only `LoadAggregate`, `EmitEvents`, `EmittedEvents`, and `ValidateAsync`.

### Projections
Runtime Projections (`src/runtime/projection/`):
1. Verify all projection handlers reside in `src/runtime/projection/`.
2. Verify NO runtime projection is exposed via API or query endpoint.
3. Verify NO runtime projection references `src/projections/`.
4. Verify NO runtime projection writes to Redis or shared read-model store.
5. Scan for command dispatch calls in runtime projection handlers.

Domain Projections (`src/projections/`):
1. Verify all domain projection handlers reside in `src/projections/`.
2. Verify NO domain projection references `src/domain/`, `src/runtime/`, or `src/engines/`.
3. Verify ALL domain projections consume events via Kafka/event fabric.
4. Verify ALL domain projection handlers include CorrelationId, EventId, IdempotencyKey.
5. Verify Redis/read-store writes originate ONLY from `src/projections/`.
6. Scan for command dispatch calls, aggregate mutations, and event publications.
7. Verify idempotency mechanisms (sequence tracking, upsert patterns).
8. Verify replay safety (event reprocessing produces identical state).

Cross-Layer Isolation:
1. Parse all `using` directives in `src/projections/` — reject any referencing Runtime, Domain, or Engines namespaces.
2. Parse all `using` directives in `src/runtime/projection/` — reject any referencing Projections namespace.
3. Verify `.csproj` project references: `src/projections/` may only reference `src/shared/`.
4. Verify no API controller queries runtime projections directly.

### Determinism
1. Ripgrep the union of `src/domain`, `src/engines`, `src/runtime`, `src/systems`, and `src/platform/host/adapters` for each blocked pattern.
2. For every hit, classify:
   - Is the file the `IClock` or `IIdGenerator` implementation? → exception.
   - Is the hit inside a comment or string literal? → not a violation.
   - Otherwise → S0 violation.
3. For each adapter constructor, verify it accepts `IClock` and/or `IIdGenerator` whenever it stamps timestamps or generates ids.

### Deterministic IDs (HSID)
1. Ripgrep `src/` for `Guid.NewGuid`, `DateTime*.UtcNow`, `Random`, `Environment.Tick*`, `Stopwatch.GetTimestamp` inside `src/engines/T0U/determinism/**` and `src/shared/kernel/determinism/**`. Any hit → S0.
2. Ripgrep `src/` for `IDeterministicIdEngine` references. Any reference outside `src/runtime/control-plane/` and `src/engines/T0U/determinism/` → S1 (G6).
3. Ripgrep `src/domain/` for `IDeterministicIdEngine` or `HSID`. Any hit → S0 (G5).
4. Open `RuntimeControlPlaneBuilder.Build()` and confirm the locked list still contains exactly 8 middlewares (G8 cross-check).
5. Confirm the regex in `DeterministicIdEngine.Format` matches the canonical regex in this document character-for-character.

### Hashing
1. Open every file in scope. Read the body of every `Compute*` / `Hash*` method.
2. For every input expression, classify as permitted or forbidden by the lists above.
3. Grep the scope for `DateTime`, `Guid.NewGuid`, `Random`, `HashSet<`, `Dictionary<`, `Environment.Tick`, `Stopwatch`. Each hit must be either absent from the hash inputs or justified by a documented sentinel pattern.
4. Verify per-event signatures include the position index when iterating a list of events.

### Replay
1. Open `EventReplayService.cs`. Verify the three sentinel assignments are present, in the order shown above, with the exact literal values (`"replay"`, `"replay"`, `DateTimeOffset.MinValue`).
2. Grep `src/runtime/event-fabric/EventReplay*.cs` for any code that reads `ExecutionHash`, `PolicyHash`, or `Timestamp` from a stored event metadata source.
3. Grep `tests/**` and `claude/audits/**` for assertions on `ExecutionHash` or `PolicyHash` equality across replays.

---

## Consolidated Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Engine persistence attempt | Engine writes to DB, file, or any durable store |
| **S0 — CRITICAL** | Engine publishing event externally | Engine calls `eventBus.Publish()`, `IEventPublisher`, Kafka produce |
| **S0 — CRITICAL** | Engine calling infra | Engine calls Redis, Kafka, HTTP, SQL, file I/O directly |
| **S0 — CRITICAL** | Runtime bypass | Any path invoking engine without RuntimeControlPlane pipeline |
| **S0 — CRITICAL** | Engine anchors to chain directly | T2E creates `ChainBlock` or calls chain anchor service |
| **S0 — CRITICAL** | Engine defines domain aggregate | `class Order : AggregateRoot` in engine |
| **S0 — CRITICAL** | Engine-to-engine direct import (cross-tier) | T2E imports T1M engine class |
| **S0 — CRITICAL** | EngineContext exposes persistence | `public Task Save<T>()` on EngineContext |
| **S0 — CRITICAL** | Projection mutates domain aggregate | `aggregate.Apply(event)` in projection handler |
| **S0 — CRITICAL** | Projection dispatches command | `_commandBus.Send(new CreateOrder())` in projection |
| **S0 — CRITICAL** | Projection publishes event | `_eventBus.Publish(newEvent)` in projection |
| **S0 — CRITICAL** | Domain projection references runtime/domain/engines | `using Whycespace.Runtime;` in `src/projections/` |
| **S0 — CRITICAL** | Runtime projection references domain projections | `using Whycespace.Projections;` in `src/runtime/projection/` |
| **S0 — CRITICAL** | Runtime projection exposed via API | API controller returns runtime projection data |
| **S0 — CRITICAL** | Any runtime-order violation | Reorder / drop any of 8 middlewares or 3 fabric stages |
| **S0 — CRITICAL** | Direct `Guid.NewGuid()` / `DateTime*.UtcNow` in any in-scope path | Adapter envelope with `Guid.NewGuid()` |
| **S0 — CRITICAL** | `IIdGenerator.Generate(seed)` called with non-deterministic seed | seed = `DateTimeOffset.UtcNow.Ticks` |
| **S0 — CRITICAL** | Random / clock primitive in HSID engine or sequence | Guid.NewGuid in `DeterministicIdEngine` |
| **S0 — CRITICAL** | Domain-layer code references `IDeterministicIdEngine` | HSID import in `src/domain/` |
| **S0 — CRITICAL** | Sequence segment width changed without syncing guard/regex | X3 → X4 without guard update |
| **S0 — CRITICAL** | Any forbidden input reaches hash function | `DateTime.UtcNow` in `ExecutionHash` |
| **S0 — CRITICAL** | Policy re-evaluated on replay | `EventReplayService` calls policy engine |
| **S0 — CRITICAL** | PolicyMiddleware calls fabric directly | `_eventStore.Append` in PolicyMiddleware |
| **S1 — HIGH** | Middleware in engine/platform | Authorization middleware in `src/engines/` |
| **S1 — HIGH** | Domain logic in runtime | `if (order.Status == Approved)` in runtime |
| **S1 — HIGH** | R-DOM-01: domain-named symbol in runtime/host | `using Whycespace.Domain.OperationalSystem.Sandbox.Todo` in `src/runtime/**` or `src/platform/host/**` |
| **S1 — HIGH** | Engine manages transaction | `new TransactionScope()` in engine |
| **S1 — HIGH** | T0U imports domain types | `using Domain.Economic.Capital;` in T0U |
| **S1 — HIGH** | T1M mutates domain state | T1M calling `aggregate.Apply(event)` |
| **S1 — HIGH** | T3I contains business rules | `if (order.Total > creditLimit)` in T3I |
| **S1 — HIGH** | Projection in wrong layer | Projection class in `src/domain/` or `src/systems/` |
| **S1 — HIGH** | Projection consumes command | Handler subscribes to `CreateOrderCommand` |
| **S1 — HIGH** | Projection contains business rules | `if (total > creditLimit) flag = true` |
| **S1 — HIGH** | Domain projection invoked synchronously | Direct method call instead of Kafka consumer |
| **S1 — HIGH** | Redis write from runtime projection | Runtime projection writing to shared Redis store |
| **S1 — HIGH** | G-COMPLOAD-* / G-PROGCOMP-01/03/04/05 | Composition violations |
| **S1 — HIGH** | Replay sentinel removed without lift procedure | `ExecutionHash = "replay"` changed |
| **S1 — HIGH** | `IDeterministicIdEngine.Generate(...)` outside two permitted surfaces | HSID called from engine middleware |
| **S1 — HIGH** | Second `IDeterministicIdEngine` implementation | Duplicate HSID engine class |
| **S1 — HIGH** | Non-deterministic seed component | Seed contains `Clock|Now|Ticks|Guid|Random` |
| **S1 — HIGH** | SQL clock value flowing back into hash/key/anchor | `NOW()` becomes hash input |
| **S1 — HIGH** | Workflow state observer/repository re-introduced | `WorkflowState*` class under `src/runtime/**` |
| **S2 — MEDIUM** | Projection bypasses runtime | Projection subscribes directly to Kafka topic |
| **S2 — MEDIUM** | Engine has retry logic | `Polly.Policy.Handle<>()` in engine |
| **S2 — MEDIUM** | T2E performs external call | `HttpClient.GetAsync()` in T2E engine |
| **S2 — MEDIUM** | Engine with mutable state | `private int _processedCount = 0;` |
| **S2 — MEDIUM** | Missing tier in folder path | Engine in `src/engines/myengine/` without tier |
| **S2 — MEDIUM** | Non-idempotent projection | Insert without upsert; fails on replay |
| **S2 — MEDIUM** | Projection updates multiple read models | Single handler writing to two different views |
| **S2 — MEDIUM** | Missing context fields | Handler lacks CorrelationId or IdempotencyKey |
| **S2 — MEDIUM** | New adapter added without `IClock`/`IIdGenerator` injection | Adapter constructs envelope with stale seeds |
| **S2 — MEDIUM** | Type B replay asserts envelope equality on protected fields | Test asserts `ExecutionHash == original` on Type B |
| **S2 — MEDIUM** | Topology derivation collapses below 12 chars | Topology = 8 chars |
| **S2 — MEDIUM** | `Stopwatch` value flows into hash/id/persisted payload | Stopwatch ticks in `ExecutionHash` |
| **S2 — MEDIUM** | Deterministic-id check script missing test/validation scan | `deterministic-id-check.sh` not covering `tests/**` |
| **S2 — MEDIUM** | G-PROGCOMP-02 (size cap) | Program.cs > 100 lines |
| **S2 — MEDIUM** | Projection handler throws on missing read-model row | `InvalidOperationException` instead of upsert |
| **S2 — MEDIUM** | In-place mutation of projection-store state | `existing.StepOutputs[e.StepName] = ...` |
| **S2 — MEDIUM** | R-WF-RESUME-01 / R-WF-PAYLOAD-01 | Resume/payload handling not routed through replay service |
| **S3 — LOW** | Context not propagated via runtime | Engine reads `HttpContext.Current` |
| **S3 — LOW** | Service locator usage | `_provider.GetService<IFoo>()` in engine |
| **S3 — LOW** | Engine returns infrastructure type | Handler returning `HttpResponseMessage` |
| **S3 — LOW** | Naming convention violation | `OrderHandler` instead of `OrderProjection` |
| **S3 — LOW** | Missing rebuild support | Projection depends on non-event external state |
| **S3 — LOW** | SQL `NOW()` in adapter audit columns not consumed by deterministic logic | `created_at DEFAULT NOW()` |

## Enforcement Action

- **S0**: Block merge. Fail CI. Architectural remediation required immediately. Phase 2 lock condition fails until resolved.
- **S1**: Block merge. Fail CI. Must resolve in current PR.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track as tech debt.

All violations produce a structured report:
```
RUNTIME_GUARD_VIOLATION:
  section: <runtime-order | engine | projection | determinism | deterministic-id | hash | replay | composition | phase1.5>
  file: <path>
  rule: <rule id>
  severity: <S0-S3>
  violation: <description>
  expected: <correct behavior>
  actual: <detected behavior>
  remediation: <fix instruction>
```

---

## Relationship to Other Guards

This consolidated guard does not replace `behavioral.guard.md` or `domain.guard.md` GE-01 sections. It supplements them by widening the enforcement surface to runtime, engines, projections, composition, and platform adapters. Where this guard and an existing guard overlap, the stricter rule wins.

## Locked-Status

All sections are canonical and non-regressible. Any future workstream that needs to amend a rule above MUST:
1. Open a `claude/new-rules/{ts}-runtime.md` capture with `STATUS: PROPOSED`.
2. Reference the specific rule being amended.
3. Include a regression-coverage test that locks the new behavior.
4. Update this file in the same patch as the amendment.
