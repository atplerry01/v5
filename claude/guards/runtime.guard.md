# Runtime Guard (Canonical)

## Purpose

Enforce that `src/runtime/` is the single control plane for all command routing, event dispatch, middleware registration, projection triggering, composition loading, engine invocation, persistence, publishing, and chain anchoring. This canonical guard consolidates the runtime execution surface: runtime order, composition loading, engine purity (stateless, events only), projection read-models, replay determinism, IClock usage, deterministic IDs, SHA256 hashing, event emission, and Phase 1.5 runtime rules.

## Scope

All files under `src/runtime/`, `src/engines/`, `src/projections/`, `src/platform/host/`, `src/platform/host/adapters/`, and any component that interacts with these (platform, systems). Evaluated at compile time, CI, and architectural review.

## Source consolidation

This guard merges the following sources (aligned to GUARD-LAYER-MODEL-01 4-layer model, LOCKED 2026-04-14):
1. `runtime.guard.md` (prior version — preserved verbatim: rules 1–15 + GE-01..05 + all NEW-RULES integrations)
2. `runtime-order.guard.md` (merged into Runtime Order & Lifecycle section)
3. `phase1.5-runtime.guard.md` (merged into Phase 1.5 Runtime Rules section)
4. `engine.guard.md` (merged into Engine Purity section)
5. `projection.guard.md` (merged into Projections section)
6. `prompt-container.guard.md` (merged into Prompt Container section)
7. `dependency-graph.guard.md` (merged into Dependency Graph & Layer Boundaries section)
8. `contracts-boundary.guard.md` (merged into Contracts Boundary section)
9. `clean-code.guard.md` (merged into Code Quality Enforcement > Clean Code subsection)
10. `no-dead-code.guard.md` (merged into Code Quality Enforcement > Dead Code Elimination subsection)
11. `stub-detection.guard.md` (merged into Code Quality Enforcement > Stub Detection subsection)
12. `tests.guard.md` (merged into Test & E2E Validation > Test Architecture subsection)
13. `e2e-validation.guard.md` (merged into Test & E2E Validation > E2E Validation subsection)

**Relocated to other canonical guards per GUARD-LAYER-MODEL-01:**
- `determinism.guard.md`, `deterministic-id.guard.md`, `hash-determinism.guard.md`, `replay-determinism.guard.md` → `constitutional.guard.md` (constitutional determinism primitives).
- `composition-loader.guard.md`, `program-composition.guard.md` → `infrastructure.guard.md` (host-process assembly).

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

#### POLICY-INPUT-ENVELOPE-01 — Canonical OPA Input Envelope (S1)
Every `IPolicyEvaluator` implementation must post, and every rego policy file under `infrastructure/policy/**` must consume, the SAME canonical OPA `input` envelope:

```
input:
  policy_id        — canonical dotted id
  action           — "<domain>.<verb>"
  subject          — { role, ...attrs }
  resource         — { classification, context, domain, aggregate_id?, state | null }
  command          — typed command record, snake-case serialised (optional — omitted by pre-B6 callers)
  command_type     — command class name (populated together with `command`; never one without the other)
  correlation_id, tenant_id, actor_id
  now              — ISO 8601 UTC (optional; populated together with `now_ns`)
  now_ns           — epoch nanoseconds (rego-friendly numeric comparison)
```

Discipline:

1. **Field names are snake-case only** (`JsonNamingPolicy.SnakeCaseLower`).
2. **`command` and `command_type`** are populated together or both omitted — never one without the other.
3. **`resource.state`** is populated with an explicit JSON `null` when no per-command loader is registered or the aggregate does not yet exist. NEVER omit the `state` key when `command` is populated — rego needs the explicit `not input.resource.state` branch to distinguish "no aggregate" from "aggregate with no attributes".
4. **`now` and `now_ns`** are populated together, both from the SAME `IClock.UtcNow` read; never from `DateTime.UtcNow`, never from a DB `NOW()`, never re-read inside the evaluator.
5. **`aggregate_id`** is emitted iff the command implements `IHasAggregateId` — `Guid.Empty` is never emitted.

A drift in any field name / shape silently mis-evaluates policies because rego key lookups return `undefined` (which defaults to the deny-branch skip, i.e. silent allow for the backward-compat `not input.x` guards).

**Source:** new-rules 2026-04-17 (Phase 8 B6 — policy middleware input enrichment).
**Severity:** S1

#### POLICY-STATE-SOURCE-EVENT-STORE-01 — Aggregate State Loader Must Source From Event Store (S1)
`IAggregateStateLoader` implementations that hydrate `input.resource.state` for policy evaluation MUST source the snapshot from the authoritative write-side: `IEventStore.LoadEventsAsync` → aggregate-type-specific reducer → policy-visible DTO. They MUST NOT read from projection stores, read models, or any Kafka-driven view.

**Rationale:** Projection lag is the exact drift B6 closes. A projection-backed loader can deny based on a stale "Sanction is Active" row after the engine has already processed a Revoke, producing audit disagreement with committed state.

**Null contract:** If the aggregate does not exist yet (factory-style commands), implementations MUST return `null`. They MUST NOT return a default-initialised DTO — rego's backward-compat branch (`not input.resource.state`) keys on the null signal to skip state-aware deny rules.

**Registration precedence:** The default registration is `NullAggregateStateLoader` (returns null for every input). Per-aggregate implementations register later in composition and override the default. Only one implementation may be resolved at runtime; multiple registrations must throw at startup.

**Audit probe:** Grep every `IAggregateStateLoader` implementation under `src/platform/host/adapters/**` and `src/runtime/**`; its constructor dependency graph MUST include `IEventStore` (direct or transitive). Presence of a dependency on `I*ProjectionStore`, `I*ReadModel`, or a Postgres projection connection string = S1 fail.

**Source:** new-rules 2026-04-17 (Phase 8 B6 — policy middleware input enrichment).
**Severity:** S1

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

#### EVENT-STORE-HOLDS-MAPPED-PAYLOAD-01 — Event Store Persists the Schema-Registry Mapping Output (S1)
**Scope:** every `IEventStore` implementation under `src/**` and `tests/**`, every test that inspects `IEventStore.AllEvents(...)` (or equivalent write-side getters), and the single canonical write path in `src/runtime/event-fabric/EventFabric.cs`.

**Rule:**

1. **Write side (canonical).** `EventFabric.ProcessAsync` MUST set `EventEnvelope.Payload = _schemaRegistry.MapPayload(eventTypeName, domainEvent)`. No other payload transformation is permitted on the write path. The mapper's output is the authoritative on-disk / in-memory payload shape.

2. **Mapper semantics.** `EventSchemaRegistry.MapPayload` returns:
   - the **schema type** produced by the registered mapper when one exists for `eventTypeName`;
   - the **domain event unchanged** when no mapper is registered.
   The choice between schema and domain is schema-registry-driven, not a per-adapter decision.

3. **In-memory adapters.** `InMemoryEventStore` and any equivalent test double MUST store `envelope.Payload` as-is and return it unchanged from `AllEvents` / `LoadEventsAsync`. A test double that deserialises, re-maps, or otherwise transforms the payload silently violates the canonical contract and is rejected.

4. **Postgres / durable adapters.** The write side serialises `envelope.Payload` to JSON via its declared CLR type. The read side (`LoadEventsAsync`) MUST deserialise using `EventSchemaRegistry.Resolve(eventType).StoredEventType` (the domain type), restoring the pre-mapping form so aggregate `Apply(object)` pattern-matches on domain types at replay. This asymmetry is intentional — it lets aggregates remain ignorant of schema types without forcing the write side to carry both representations.

5. **Test assertions.** Any test that inspects write-side store contents (`AllEvents`, `Versions`, or equivalent) MUST assert on the mapper's output type:
   - **If a mapper is wired** for the event type in the active `EventSchemaRegistry` → assert on the schema type.
   - **If no mapper is wired** → assert on the domain type.
   - Tests that exercise the replay path (`LoadEventsAsync` → aggregate reconstruction) MUST assert on domain types; replay is the read-side transformation covered by rule (4).

**Audit probes:**
- Static check 1: grep every assertion on `Assert.IsType<*Event>` / `Assert.IsType<*EventSchema>` near an `IEventStore` getter call (`AllEvents`, `Stream`, `Versions`). For each, determine whether a mapper is registered for that event type in the active schema module. Mismatch = S1 fail.
- Static check 2: grep every `IEventStore` implementation's `AppendEventsAsync` / `AllEvents` / `LoadEventsAsync`. Confirm it does not transform `Payload`. Any transformation = S1 fail.
- Static check 3: confirm `EventFabric.ProcessAsync` is the SOLE call site of `EventSchemaRegistry.MapPayload` on the write path.

**Rationale:** pre-promotion, the `WorkflowResumedEventFabricRoundTripTest` survived for multiple cycles in two contradictory states — line 108 asserted `MapPayload` produces a schema, while lines 131–134 asserted the event store holds domain types. The contradiction persisted because the "event store holds the mapper's output" contract was implicit in `EventFabric.cs:117` but never canonicalised. This rule makes the contract explicit so the same drift cannot re-surface silently.

**Source:** Phase 10 B1 decision (`claude/certification/phase10-b1-decision.md`); captured under `claude/new-rules/_archives/20260417-150000-guards.md`; promoted 2026-04-17.
**Severity:** S1

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

#### RT-API-CORRELATION-ECHO-01 / R-CHAIN-CORRELATION-SURFACE-01 — API Envelope Correlation ID (S2)
Every API response envelope returned to a client MUST carry the runtime-stamped correlation id in `meta.correlationId`. Source priority:
1. **Command paths** (POST/PUT/DELETE that flow through `ISystemIntentDispatcher`): use `result.CorrelationId` from the returned `CommandResult` — the same value persisted to `events.correlation_id`, `whyce_chain.correlation_id`, and the Kafka `correlation-id` header.
2. **Read paths** (GET that bypass the runtime): use the inbound `X-Correlation-Id` request header if present and parseable as a Guid; otherwise `Guid.Empty` is acceptable as a read-path sentinel.
3. A zero `Guid.Empty` value on a **successful command response** is a surface-layer violation of `R-CHAIN-CORRELATION-01` — persistence/Kafka carry the real value while the API surface serves zero.

Sub-rule: the `ApiResponse.Ok(T data, DateTimeOffset timestamp)` two-arg overload at `src/shared/contracts/common/ApiResponse.cs` is a structural footgun — it silently drops correlation. Every callsite that uses it in a command-path controller is a latent bug. Static check: grep `src/platform/api/controllers/**/*.cs` for `ApiResponse\.Ok\([^,]+,\s*[A-Z]\w+\.UtcNow\)` and flag every hit in a command controller; allow only when an explicit comment justifies the dropped correlation. Complements INV-501 (Mandatory Telemetry Emission).
**Source:** new-rules 2026-04-16 (validation-infra D10; audit finding 5).
**Severity:** S2

#### R-RT-CMD-AGGID-01 — Commands MUST Implement IHasAggregateId Explicitly (S1)
Every command type routed through `ISystemIntentDispatcher` MUST implement `IHasAggregateId`. The reflective property-name fallback in `SystemIntentDispatcher.ResolveAggregateId` is DEPRECATED and MUST be removed once all existing commands are migrated — relying on it couples the dispatcher to a fixed set of identifier-naming conventions and makes every new domain one forgotten-property-rename away from an HTTP 500 at first dispatch.

Migration path (non-breaking): (1) add `IHasAggregateId` implementation to every `*Command` record; (2) add an architecture test under `tests/unit/architecture/` asserting every type assignable from a known `ICommand` marker (or matching `*Command` under `src/shared/contracts/**`) implements `IHasAggregateId`; (3) once CI is green, delete the `AggregateIdPropertyCandidates` fallback from `SystemIntentDispatcher`. Static check: grep `src/shared/contracts/**` for `record.*Command(` and confirm the declaration includes `: IHasAggregateId` or that the base marker interface does.
**Source:** new-rules 2026-04-16 (Stage L compliance/audit certification).
**Severity:** S1

#### RT-OUTBOX-AGGID-FROM-ENVELOPE-01 — Outbox AggregateId Sourced From Envelope (S0)
The outbox row's `aggregate_id` column and every Kafka header / message key derived from it MUST be sourced from `IEventEnvelope.AggregateId`. Concretely:
- The `IOutbox.EnqueueAsync` contract MUST accept `aggregateId` as an explicit parameter alongside `correlationId`. Callers (only `EventFabric.OutboxService`) MUST pass the envelope's authoritative value.
- Reflection-by-property-name (the legacy `ExtractAggregateId` / `IdentityPropertyNames` allowlist pattern) is FORBIDDEN in any outbox implementation.
- The outbox adapter MUST throw `InvalidOperationException` (not silently emit `Guid.Empty`) when `aggregateId == Guid.Empty && events.Count > 0`. Pairs with the `K-AGGREGATE-ID-HEADER-01` fence on `EventEnvelope` (producer-side mirror).
- Static check: grep `src/platform/host/adapters/**` for `IdentityPropertyNames`, `ExtractAggregateId`, or reflective `GetProperty("...Id")` patterns inside `IOutbox` implementations — any hit is an S0 fail.

**Rationale:** payload types are domain-owned; the runtime must not depend on a per-type identity-property convention. Every new aggregate whose payload identity property is `<X>Id` for an `<X>` not in a hard-coded allowlist silently corrupts its own Kafka partition routing and consumer reconstruction. Complements infrastructure R-K-11 (partition key alignment) and R-K-15 (order guarantee by aggregate id).
**Source:** new-rules 2026-04-16 (revenue-domain validation D9).
**Severity:** S0

#### RT-BACKGROUND-IDENTITY-EXPLICIT-01 — Background Workers Declare System Identity (S1)
Any code path that invokes `ISystemIntentDispatcher.DispatchAsync` (or any contract that internally invokes `ICallerIdentityAccessor.GetActorId/GetTenantId/GetRoles`) OUTSIDE an HTTP request scope MUST establish an explicit, declared system identity before dispatching. Concretely:
- A `SystemIdentityScope` (or equivalent) seam MUST exist alongside the HTTP-bound `ICallerIdentityAccessor`. The scope MUST:
  - Be opt-in per call site (no global default — HTTP requests stay fail-closed per WP-1).
  - Be `AsyncLocal`-bound so it cannot leak across unrelated background tasks.
  - Carry a non-empty actor identifier prefixed `system/<purpose>` (e.g. `system/workflow-trigger`, `system/integration-bridge`). Empty-string actors are forbidden.
- The HTTP-bound `ICallerIdentityAccessor` implementation MUST consult the scope FIRST and only fall through to HTTP context when no scope is active. The HTTP fall-through's deny-by-default behaviour MUST remain unchanged.
- Every `BackgroundService` / `IHostedService` / Kafka consumer worker that dispatches commands MUST wrap its dispatch in the scope:
  ```csharp
  using (SystemIdentityScope.Begin("system/<worker-name>", "system", "system"))
  {
      await _dispatcher.DispatchAsync(command, route, ct);
  }
  ```
- Direct invocation of `IHttpContextAccessor.HttpContext` from a background worker is FORBIDDEN — the dependency must flow through `ICallerIdentityAccessor` so the scope mechanism applies uniformly.

Static check: enumerate every `BackgroundService` / `IHostedService` under `src/platform/host/adapters/**`; for each, confirm any runtime-entry-point invocation is dominated by a `using (SystemIdentityScope.Begin(...))` on the call path. Bare dispatch without scope = S1 fail. Complements constitutional WP-1 (HTTP fail-closed) and INV-202 (No Anonymous Execution).
**Source:** new-rules 2026-04-16 (revenue-domain validation D11).
**Severity:** S1

#### RT-POLL-SCHEDULER-DET-01 — Deterministic Polling-Scheduler Contract (S1)
Every timer-driven `BackgroundService` in `src/platform/host/adapters/**` that dispatches commands via `ISystemIntentDispatcher` MUST:

1. **Clock discipline.** Source `now` exclusively from injected `IClock.UtcNow`; pass `now` as an explicit parameter into every read-side query so the DB's `NOW()` / `CURRENT_TIMESTAMP` is never consulted. A single `now` read per iteration seeds both the scan and every idempotency key derived from that iteration's candidates.

2. **Deterministic idempotency key shape.** The scheduler-side `IIdempotencyStore` key for each dispatched candidate MUST be `{worker-prefix}:{aggregate-id:N}:{schedule-timestamp.UtcTicks}` — a pure function of (aggregate, scheduled time). Restart-replay and multi-replica concurrent scans MUST produce byte-identical keys for the same logical candidate so the atomic claim picks exactly one winner.

3. **Two-layer idempotency.** The scheduler-side claim (layer 1) MUST be paired with an aggregate-level state guard (layer 2, e.g. `Status != Active` → error) so a stale or bypassed claim table still cannot produce duplicate terminal transitions.

4. **Failure-release contract (KC-2 parity).** A dispatch that throws MUST `ReleaseAsync` the claim so the next tick can retry. A dispatch that returns a failed `CommandResult` MUST leave the claim in place (terminal resolution — no retry).

5. **Identity scope (RT-BACKGROUND-IDENTITY-EXPLICIT-01 extension).** Each per-candidate dispatch MUST be dominated by `using (SystemIdentityScope.Begin("system/<worker-name>", "system", "system"))`, identical to the Kafka-subscriber variant.

6. **Worker liveness (HC-5 extension).** The worker MUST stamp `IWorkerLivenessRegistry.RecordSuccess(<worker-name>, _clock.UtcNow)` on the **success path only** — never inside a catch block. A persistently-failing projection scan MUST age the scheduler out of Ready on the configured `MaxSilenceSeconds`.

7. **Fail-safe query posture.** The query abstraction consumed by the scheduler MUST return an empty candidate list on transport / query failure (NOT throw, NOT return a sentinel). Missed scheduler ticks are bounded-delay, not safety-critical — this is deliberately opposite to `ILockStateQuery`'s fail-closed posture.

Static check: enumerate every `BackgroundService` in `src/platform/host/adapters/**` whose `ExecuteAsync` contains `PeriodicTimer` or `Task.Delay(TimeSpan.From...)` but no Kafka `consumer.Subscribe(...)` — treat each as a polling scheduler and verify items (1)–(7) against its source. Pair with existing `RT-BACKGROUND-IDENTITY-EXPLICIT-01`, `R-PLAT-11 / PLAT-DET-01`, `HC-5 / WORKER-LIVENESS-01`, `KC-2 / IDEMPOTENCY-COALESCE-01` which cover subsets of this rule for the Kafka-subscriber variant.

**Source:** new-rules 2026-04-17 (Phase 8 B5 — sanction / system-lock expiry schedulers; first timer-driven polling scheduler pattern to ship in the repo).
**Severity:** S1

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

**STATUS: CANONICAL** — LOCKED 2026-04-09 per `claude/audits/phase1.5/phase1.5-final.audit.output.md` §7.

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

#### P-IDEMPOTENCY-KEY-NOT-NULL-01 — Idempotency Key Populated On Upsert (S2)
Projection handlers under `src/projections/**` MUST populate `idempotency_key` on every upsert, derived from `{event_id}` or `{aggregate_id}:{version}`. A projection row with `idempotency_key IS NULL` defeats the UNIQUE constraint that enforces duplicate-suppression, silently violating P5. Audit probe: `SELECT count(*) FROM <projection>_read_model WHERE idempotency_key IS NULL` MUST equal `0` on every environment after at least one event has flowed.
**Source:** infra-validation audit 2026-04-16 — Finding 2. Extends P5 / P13.
**Severity:** S2

#### P-VERSION-MONOTONE-01 — Projection current_version Monotone (S2)
Projection upserts MUST set `current_version = eventEnvelope.Version` (the aggregate stream version of the applied event). Rows where `current_version = 0 AND last_event_id IS NOT NULL` are evidence that out-of-order protection is absent in practice. An out-of-order event whose `Version <= current_version` MUST be skipped or requeued. Audit probe: `SELECT count(*) FROM <projection>_read_model WHERE current_version = 0 AND last_event_id IS NOT NULL` MUST equal `0`.
**Source:** infra-validation audit 2026-04-16 — Finding 6. Extends P12.
**Severity:** S2

#### P-JSONB-KEY-CASE-01 — JSONB Key Casing Consistency (S2)
Projection state JSONB MUST use a single documented casing convention per read model — typically `camelCase` to match controller DTOs. Reducers writing one casing (e.g. `PascalCase`) into `state` while indexes or query extractors use the other casing (`state->>'camelCase'`) is a contract violation: GET-by-key endpoints return empty for valid queries while the rows are present. Audit probe: for every projection table, for every index that extracts a state key, assert the key is present under that exact casing in at least one stored row.
**Source:** infra-validation audit 2026-04-16 — Finding 4. Extends P7 / P9 and DTO-R naming consistency.
**Severity:** S2

#### P-EVENT-TIMESTAMP-STAMP-01 — Projection Reducers Must Stamp Temporal Fields (S2)
Projection reducers that map a "registered at" / "created at" / "effective at" temporal field from a domain event MUST stamp that field from `EventEnvelope.Timestamp` (or, during replay, from the persisted envelope metadata — respecting `REPLAY-SENTINEL-PROTECTED-01` for sentinel cases). A read-model row carrying `default(DateTimeOffset)` (`0001-01-01T00:00:00+00:00`) for such a field when the envelope has a real timestamp is a reducer-integrity defect. Static check: enumerate reducer handler methods; for each temporal read-model column, confirm the assignment reads `envelope.Timestamp` or a payload field populated from it.
**Source:** infra-validation audit 2026-04-16 — Finding 3.
**Severity:** S2

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


### Section: Prompt Container

Source: prompt-container.guard.md (absorbed verbatim 2026-04-14 per GUARD-LAYER-MODEL-01).


##### Purpose

Enforce canonical prompt formatting across all AI-assisted prompts used in the WBSM v3 system. Every prompt must use the markdown container format, declare mandatory sections, avoid broken fencing, support batch execution, and be registered in the prompt registry.

##### Scope

All prompt files (`.prompt.md`, `.prompt.json`, or prompt templates) across the repository, including `claude/` directory prompts, CI prompts, and any prompt used for code generation, auditing, or governance. Evaluated at CI and prompt review.

##### Rules

1. **MARKDOWN CONTAINER FORMAT** — All prompts must use the standard markdown container structure. Each prompt is a self-contained markdown document with clearly delineated sections using level-2 headings (`##`). No free-form text prompts. No inline prompt strings embedded in code. Every prompt is a file.

2. **MANDATORY SECTIONS** — Every prompt must declare these five sections:
   - `## Role` — Defines the AI's persona, expertise, and constraints for this prompt.
   - `## Objective` — States the specific goal of the prompt in one to three sentences.
   - `## Rules` — Numbered list of behavioral rules the AI must follow during execution.
   - `## Output` — Defines the expected output format, structure, and delivery method.
   - `## Failure` — Defines what constitutes failure, how to detect it, and what to do on failure.
   Missing any section makes the prompt non-compliant.

3. **NO BROKEN NESTED CODE FENCING** — Prompts that contain code examples must use proper fencing. Triple backticks inside a prompt must not break the outer markdown structure. Use different fence lengths (````` vs ```) or indent-based code blocks when nesting. A prompt with broken fencing is unparseable and therefore invalid.

4. **PROMPTS ARE BATCH-SAFE** — Every prompt must be executable in batch mode (non-interactive). Prompts must not require mid-execution user input, confirmations, or interactive decisions. All parameters must be declared upfront in a `## Parameters` section (optional but required if the prompt takes input). Batch-safe means: given inputs, the prompt runs to completion autonomously.

5. **PROMPTS REGISTERED IN PROMPT REGISTRY** — Every prompt file must have an entry in `prompt.registry.json` (or equivalent registry file). The registry entry includes: prompt ID, file path, category, version, and last-validated date. Unregistered prompts are not executable by CI or automated systems.

6. **PROMPT VERSIONING** — Each prompt must declare its version in a YAML frontmatter block or metadata section. Version follows semver: `major.minor.patch`. Breaking changes to prompt structure increment major. Output format changes increment minor. Clarifications increment patch.

7. **PROMPT CATEGORIZATION** — Prompts must be categorized:
   - **audit**: Prompts that validate code or architecture.
   - **generate**: Prompts that produce code, configs, or artifacts.
   - **review**: Prompts that evaluate PRs, diffs, or changes.
   - **enforce**: Prompts that check compliance against guards.
   - **report**: Prompts that produce summary or status reports.
   The category must be declared in the prompt metadata and registry entry.

8. **NO PROMPT INJECTION VECTORS** — Prompts must not contain user-controllable interpolation without sanitization. If a prompt accepts parameters, parameter values must be enclosed in delimited blocks (e.g., `<parameter>value</parameter>`) to prevent prompt injection. No raw string concatenation of user input into prompt text.

9. **DETERMINISTIC OUTPUT SPECIFICATION** — The `## Output` section must define the exact output format (JSON schema, markdown template, structured report format). Outputs must be machine-parseable when consumed by CI. Free-form prose output is permitted only for human-targeted prompts explicitly marked as such.

10. **PROMPT DEPENDENCY DECLARATION** — If a prompt depends on output from another prompt (chained execution), the dependency must be declared in a `## Dependencies` section. The dependency graph must be acyclic. Circular prompt dependencies are forbidden.

11. **PROMPT IDEMPOTENCY** — Prompts must be idempotent: running the same prompt with the same inputs on the same codebase state must produce equivalent output. Non-deterministic prompts (e.g., creative generation) must be explicitly marked as `idempotent: false` in metadata.

12. **MAXIMUM PROMPT LENGTH** — Individual prompts must not exceed 4000 words. Prompts exceeding this limit must be decomposed into chained sub-prompts with explicit dependency declarations. Overly long prompts degrade AI performance and are harder to audit.

13. **WRITING BLOCK REQUIRED FOR LONG PROMPTS** — Prompts that generate or modify more than 5 files, or produce output exceeding 2000 lines, must include a `## Writing Block` section. The writing block declares: target files, expected changes, rollback strategy, and validation criteria. This ensures large-scale prompt executions are auditable and reversible.

14. **NO BROKEN CONTAINERS (CRITICAL)** — A prompt container must be syntactically complete. Every opened section must be closed. Every code fence must be balanced. Every parameter placeholder must have a corresponding declaration. A broken container is a prompt that cannot be parsed to completion. Broken containers must fail CI immediately with S0 severity. No partial prompt execution is permitted.

15. **PROMPT MUST DECLARE EXECUTION MODE** — Every prompt must declare its execution mode in metadata or frontmatter:
    - `mode: autonomous` — prompt runs to completion without human intervention
    - `mode: supervised` — prompt pauses at checkpoints for human review
    - `mode: dry-run` — prompt simulates execution and reports what would change
    Prompts without a declared execution mode default to `supervised` and must be flagged for metadata completion.

---

##### WBSM v3 GLOBAL ENFORCEMENT

##### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

##### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

##### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

##### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

##### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

---

##### Check Procedure

1. Enumerate all prompt files matching `*.prompt.md`, `*.prompt.json`, or files in prompt directories.
2. For each prompt file, verify presence of all five mandatory sections: Role, Objective, Rules, Output, Failure.
3. Parse markdown fencing and verify no broken nested code blocks (fence depth tracking).
4. Verify no prompt requires mid-execution user interaction (scan for interactive markers).
5. Verify each prompt has an entry in `prompt.registry.json`.
6. Verify version declaration in frontmatter or metadata for each prompt.
7. Verify category assignment for each prompt.
8. Scan for raw string interpolation patterns (e.g., `${userInput}`, `{0}`) without delimiters.
9. Verify `## Output` section defines structured format (JSON schema, template, or format spec).
10. Build prompt dependency graph and check for cycles.
11. Verify prompt word count does not exceed 4000 words.
12. Cross-reference registry entries against actual prompt files (detect orphaned registrations or unregistered prompts).

##### Pass Criteria

- All prompts use markdown container format.
- All prompts have all five mandatory sections.
- No broken code fencing in any prompt.
- All prompts are batch-safe (no interactive requirements).
- All prompts registered in prompt registry.
- All prompts have version declarations.
- All prompts are categorized.
- No prompt injection vectors detected.
- All output specifications are structured.
- Prompt dependency graph is acyclic.
- All prompts under 4000 words.

##### Fail Criteria

- Prompt missing mandatory section (Role, Objective, Rules, Output, or Failure).
- Broken nested code fencing.
- Prompt requires interactive input mid-execution.
- Prompt not registered in prompt registry.
- Missing version declaration.
- Missing category assignment.
- Raw user input interpolation without sanitization.
- Unstructured output specification for CI-consumed prompt.
- Circular prompt dependency.
- Prompt exceeds 4000 words without decomposition.
- Orphaned registry entry (prompt file deleted but registry entry remains).

##### Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Prompt injection vector | `${userInput}` concatenated into role section |
| **S0 — CRITICAL** | Missing mandatory section | Prompt without `## Failure` section |
| **S1 — HIGH** | Unregistered prompt | Prompt file exists but not in registry |
| **S1 — HIGH** | Broken code fencing | Triple backticks break outer markdown |
| **S1 — HIGH** | Interactive prompt in CI pipeline | Prompt asks "Continue? (y/n)" mid-execution |
| **S2 — MEDIUM** | Missing version | No version in frontmatter or metadata |
| **S2 — MEDIUM** | Circular dependency | Prompt A depends on B, B depends on A |
| **S2 — MEDIUM** | Unstructured output | Output section says "describe the findings" |
| **S3 — LOW** | Missing category | Prompt without category assignment |
| **S3 — LOW** | Prompt over 4000 words | Long prompt without decomposition |
| **S3 — LOW** | Orphaned registry entry | Registry points to deleted prompt file |

##### Enforcement Action

- **S0**: Block merge. Fail CI. Prompt must be fixed or removed immediately.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
PROMPT_CONTAINER_GUARD_VIOLATION:
  prompt: <prompt file path>
  registry_id: <registry ID if applicable>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  section: <which section is affected>
  expected: <correct format>
  actual: <detected issue>
  remediation: <fix instruction>
```

##### NEW RULES INTEGRATED — 2026-04-07 (prompt reconciliation pre-execution)

- **PROMPT-RECONCILE-01** (S2): Pasted prompts MUST be reconciled against existing repository surface
  area BEFORE code emission. For every type / interface / method / path / topic named in the prompt,
  verify the canonical equivalent in the codebase. If the prompt literal differs from the canonical
  name (e.g. `IKafkaConsumer` vs `GenericKafkaProjectionConsumerWorker`, `t1m/` vs `T1M/`,
  `whyce.workflow.execution.events` vs `whyce.orchestration-system.workflow.events`,
  `IEventStore.LoadAsync` vs `LoadEventsAsync`), the canonical name MUST be used and the divergence
  recorded inline in the project-prompt file under a RECONCILIATION section. Literal execution of an
  unreconciled prompt is a $5 anti-drift violation.
- Source: `claude/new-rules/_archives/20260407-220000-prompt-reconciliation-pre-execution.md`.

---

### Section: Dependency Graph & Layer Boundaries

Source: dependency-graph.guard.md (absorbed verbatim 2026-04-14 per GUARD-LAYER-MODEL-01).


##### CLASSIFICATION
system / governance / dependency-control

##### PRIORITY
S0 — Architectural Safety. Violations HALT execution.

##### SCOPE
Entire repository. Loaded fresh on every prompt execution per CLAUDE.md $1a.

---

##### CANONICAL LAYER ORDER (TOP → BOTTOM)

```
platform
  ↓
systems
  ↓
runtime
  ↓
engines
  ↓
domain
  ↓
shared
```

A layer MAY only reference layers strictly below it, subject to the rules
below. Upward references are FORBIDDEN. Cycles are FORBIDDEN.

---

##### RULES

##### R1 — DOMAIN PURITY
- Path: `src/domain/**`
- Allowed references: `shared` only
- Forbidden: `engines`, `runtime`, `systems`, `platform`, `infrastructure`,
  `projections`
- Failure → S0 halt

##### R2 — ENGINE PURITY
- Path: `src/engines/**`
- Allowed references: `domain`, `shared`
- Forbidden: `runtime`, `systems`, `platform`, `infrastructure`, `projections`

##### R3 — RUNTIME AUTHORITY
- Path: `src/runtime/**`
- Allowed references: `engines`, `domain`, `shared`
- Forbidden: `systems`, `platform`, `projections`

##### R4 — SYSTEMS BOUNDARY
- Path: `src/systems/**`
- Allowed references: `runtime` (contracts/interfaces ONLY), `shared`
- Forbidden: `engines` (direct), `infrastructure`, `platform`, `projections`

##### R5 — PLATFORM ISOLATION
- Path: `src/platform/**`
- Default rule: `src/platform/api` may reference `systems` and `shared`
  only. Forbidden in `platform/api`: `runtime`, `engines`, `domain`,
  `projections`.
- **Composition-root exception:** `src/platform/host` is the composition
  root and is governed by **DG-R5-EXCEPT-01** (lines below). It MAY
  reference `runtime`, `engines`, `systems`, `projections`, and
  infrastructure adapters for DI registration purposes only. It MUST
  NOT reference `Whycespace.Domain` (see **DG-R5-HOST-DOMAIN-FORBIDDEN**).
- See DG-R5-EXCEPT-01 and DG-R5-HOST-DOMAIN-FORBIDDEN in the EXCEPTIONS
  section below for the canonical wording. Authoritative mechanical
  enforcement is `scripts/dependency-check.sh`.

##### R6 — INFRASTRUCTURE RULE
- Path: `src/infrastructure/**` (when present)
- Implements `runtime` interfaces ONLY
- Forbidden: referenced by `domain` or `engines`

##### R7 — PROJECTION RULE
- Path: `src/projections/**`
- Allowed references: `domain` (events only), `shared`
- Forbidden: `engines`, `runtime`, `systems`, `platform`
- Projections MUST NOT contain domain logic

---

##### ENFORCEMENT MAPPING

| Layer          | Path              | Allowed Project Refs                          |
|----------------|-------------------|-----------------------------------------------|
| shared         | src/shared        | (none)                                        |
| domain         | src/domain        | shared                                        |
| engines        | src/engines       | domain, shared                                |
| runtime        | src/runtime       | engines, domain, shared                       |
| systems        | src/systems       | runtime (contracts), shared                   |
| projections    | src/projections   | domain, shared                                |
| platform/api   | src/platform/api  | systems, shared                               |
| platform/host  | src/platform/host | systems, shared, api, runtime, engines, projections (DI-only, DG-R5-EXCEPT-01; **NOT domain** per DG-R5-HOST-DOMAIN-FORBIDDEN) |

Any `<ProjectReference>` outside this matrix = VIOLATION.

---

##### CODE-LEVEL CHECKS

**Authoritative enforcement:** `scripts/dependency-check.sh`. The
script is the single source of truth for the mechanical predicates;
the patterns below are illustrative summaries that must agree with
the script and with DG-R5-EXCEPT-01 / DG-R5-HOST-DOMAIN-FORBIDDEN.

Illustrative predicates run on every execution:

```
grep -r "using .*Runtime"        src/domain
grep -r "using .*Infrastructure" src/engines
grep -r "using .*Engines"        src/systems
grep -r "using .*Runtime"        src/platform/api      # platform/api only
grep -r "using .*Engines"        src/projections
grep -r "using .*Runtime"        src/projections
grep -r "Whycespace\.Domain\."   src/platform/host     # DG-R5-HOST-DOMAIN-FORBIDDEN
```

`src/platform/host` is intentionally omitted from the runtime/engines
grep above: composition-root usings are permitted under
DG-R5-EXCEPT-01. Only `Whycespace.Domain.*` references in
`src/platform/host/**` are forbidden, and the script enforces all
three forms (using directive, fully-qualified expression, namespace
alias) per the §5.1.2 Step C-G strengthening.

Any hit = VIOLATION (subject to documented exceptions below).

---

##### ADAPTER LEAKAGE

Adapters are permitted ONLY in:
- `src/platform/host/adapters/**`
- `src/infrastructure/**`

Any type/file matching `*Adapter*` outside those paths = VIOLATION.

---

##### SHARED KERNEL

`src/shared/**` MUST contain only:
- primitives
- contracts
- interfaces

FORBIDDEN inside shared:
- runtime logic
- infrastructure logic
- engine logic

---

##### FAILURE ACTION

On any violation:
1. HALT execution (CLAUDE.md $12)
2. Emit structured failure: STATUS / STAGE=GUARD / REASON / ACTION_REQUIRED
3. Do NOT auto-fix architecture (CLAUDE.md $5). Report and require explicit
   prompt for remediation.

---

##### LOCK CONDITIONS

Guard is LOCKED only if:
1. All rules R1–R7 pass, **and** all DG-* additions pass:
   `DG-R5-EXCEPT-01` (composition-root permission, narrowed 2026-04-08),
   `DG-R5-HOST-DOMAIN-FORBIDDEN` (host→domain prohibition, strengthened
   §5.1.2 Step C-G), and `DG-R7-01` (projections→runtime, remediated
   2026-04-07).
2. No illegal project references
3. No circular dependencies
4. CI runs `scripts/dependency-check.sh`
5. `claude/audits/canonical/runtime.audit.md` §Dependency Graph & Layer Boundaries reports FULL PASS (historical baseline at `claude/audits/dependency-graph.audit.output.md`)
---

##### NEW RULES INTEGRATED — 2026-04-07

- **DG-R7-01**: ~~Pre-existing violation tracked: Whycespace.Projections.csproj references Whycespace.Runtime.csproj~~ → **REMEDIATED 2026-04-07**. Resolution: introduced shared `IEnvelopeProjectionHandler` and `IEventEnvelope` contracts under `src/shared/contracts/projection/` and `src/shared/contracts/event-fabric/`. Runtime `EventEnvelope` record now implements `IEventEnvelope`; projection handlers in `src/projections/**` consume the shared contracts only. The runtime project reference has been removed from `Whycespace.Projections.csproj`. Verified by `dotnet build` green across host, unit, and integration projects.
- **DG-R5-01**: ~~Pre-existing violation tracked~~ → **CONVERTED TO DOCUMENTED EXCEPTION (2026-04-07)**. See DG-R5-EXCEPT-01 below.

##### EXCEPTIONS (documented and granted)

##### DG-R5-EXCEPT-01 — Composition Root references (2026-04-07; narrowed 2026-04-08)

`src/platform/host/Whycespace.Host.csproj` MAY reference `Whycespace.Runtime`,
`Whycespace.Engines`, `Whycespace.Systems`, `Whycespace.Projections`, and
infrastructure adapters **for DI registration purposes only**.

`Whycespace.Domain` is **NOT** in the permitted list. Per Phase 1.5 §5.1.1
Step C (2026-04-08), the sole residual host → domain typed usage in
`src/platform/host/adapters/PostgresOutboxAdapter.cs` was replaced with a
reflection-based `.Value` unwrap, and the `<ProjectReference>` to
`Whycespace.Domain.csproj` was removed from `Whycespace.Host.csproj`.
Re-introducing either the csproj reference or any `using Whycespace.Domain.*`
inside `src/platform/host/**` is a fresh S0 violation under R5 and
**DG-R5-HOST-DOMAIN-FORBIDDEN** (see below).

**Authority:** This exception aligns with the already-canonical composition-root
permission in [platform.guard.md G-PLATFORM-07](platform.guard.md):

> "Host (`Program.cs`) is the composition root and MAY reference runtime,
> engines, systems, domain, and infrastructure for DI registration purposes
> only."

The prior R5 wording ("Allowed: systems only") was inconsistent with
G-PLATFORM-07 and produced a perpetually-tracked violation that was
not actually a violation under the canonical platform guard. This
exception entry resolves the inconsistency by recording the DI-only
permission explicitly inside dependency-graph.guard.md.

**Constraints on the exception:**
1. The references are permitted **only** in `Whycespace.Host.csproj` itself.
   No other project under `src/platform/**` may use this exception.
2. The references must be **DI registration only**. Per
   [program-composition.guard.md G-PROGCOMP-01 / G-PROGCOMP-03](program-composition.guard.md),
   `Program.cs` must not contain `AddSingleton<...>` calls keyed on
   concrete domain types — domain wiring flows through
   `IDomainBootstrapModule` and `BootstrapModuleCatalog`, and category
   wiring flows through the literal-list `CompositionModuleLoader`.
3. Per [runtime.guard.md rule 11.R-DOM-01](runtime.guard.md), the host
   may not contain folders nested by `{classification}/{context}/{domain}/`
   nor hold static dictionaries keyed on a single domain.
4. Removal of any of these direct references is permitted only after
   the corresponding wiring has been migrated into a bootstrap module
   listed in `BootstrapModuleCatalog` or a category composition module.

**LOCK status:** With this exception logged, R5 is no longer suspended
on `Whycespace.Host.csproj` references. DG-R7-01 (projections → runtime)
was remediated 2026-04-07. Phase 1.5 §5.1.1 Step C (2026-04-08) removed
the `host → domain` csproj edge. No outstanding tracked violations remain
under this guard pending verification by a clean full-solution build and
a green `scripts/dependency-check.sh` run.

---

##### DG-R5-HOST-DOMAIN-FORBIDDEN — host may not depend on domain (2026-04-08)

**Rule:** `src/platform/host/**` may not, under any circumstance:
1. Declare `<ProjectReference Include="..\..\domain\Whycespace.Domain.csproj" />`
   in `Whycespace.Host.csproj`.
2. Contain `using Whycespace.Domain.*;` in any `*.cs` file.
3. Contain a **fully-qualified type expression** referencing
   `Whycespace.Domain.*` — including but not limited to
   `typeof(Whycespace.Domain.X.Y)`, parameter types, generic arguments,
   cast expressions `(Whycespace.Domain.X.Y)e`, and field/property type
   declarations.
4. Contain a **namespace alias** of the form
   `using <Alias> = Whycespace.Domain.<…>;` (e.g.
   `using DomainEvents = Whycespace.Domain.OrchestrationSystem.Workflow.Execution;`).
5. Re-introduce a typed dependency on any `Whycespace.Domain.SharedKernel.*`
   primitive. Adapters that need to inspect domain event shapes MUST do so
   via reflection or via shared contracts under `src/shared/contracts/**`.

Clauses 3 and 4 were added under Phase 1.5 §5.1.2 Step C-G after BPV-D01
exposed eleven live binding sites that bypassed clause 2 by using
fully-qualified or aliased forms. The strengthened predicate is enforced
mechanically by `scripts/dependency-check.sh` (see the `host_fq_hits`
block immediately following the C2 scan).

**Severity:** S0. Violations HALT execution and fail
`scripts/dependency-check.sh`.

**Authority:** Phase 1.5 §5.1.1 Step C remediation. The composition root
must wire modules and own infrastructure adapters; it must not import
domain primitives directly. Domain reachability remains available
transitively via `runtime → domain` for type identity at runtime, but
no host source file may bind to a domain symbol at compile time.

##### NEW RULES INTEGRATED — 2026-04-07 (baseline scan addendum)

- **DG-BASELINE-01** (S0): Dual violations logged — (R7) Projections →
  Runtime and (R5 ×4) Platform/Host fan-in to Runtime/Engines/Projections/
  Domain. LOCK blocked until remediated (invert dependencies via shared/
  domain contracts and route Host composition through systems facades) OR
  narrow exception documented inline. See
  `claude/new-rules/_archives/20260407-160000-dependency-graph.md`.

##### NEW RULES INTEGRATED — 2026-04-10 (promoted from new-rules backlog)

- **DG-SCRIPT-HYGIENE-01** (S3, tooling): `scripts/dependency-check.sh` MUST exclude `/obj/` and `/bin/` from all scans (build artifacts under e.g. `src/shared/obj/*.json` are not source). The adapter-leakage check MUST detect adapters by interface-implementation markers (`: IEventStore`, `: IProjectionWriter`, `: IRepository<>`, `: IOutbox*`, `: IChainAnchor*`) rather than by filename `*Adapter*.cs` — the latter false-positives domain aggregates that legitimately model the *Adapter* concept (e.g., `src/domain/business-system/integration/adapter/**`). The script MUST have unit-test fixtures with known-good and known-bad inputs. Source: `_archives/20260408-132840-audits.md`.
- **DG-COMPOSITION-ROOT-01** (S1, scoping): The `src/platform/host/composition/**` subtree is the composition root and is NOT exempt from R-DOM-01 host→domain prohibition (see canonical interpretation in `_archives/20260408-180000-guards.md`). However, composition modules legitimately reference Runtime, Engines, Projections, and Systems. `dependency-check.sh` MUST treat `src/platform/host/composition/**` as classification `composition` (distinct from `platform`) so that references to those layers are not flagged as R5 platform leaks. Adapter files (`src/platform/host/adapters/*.cs`) are NOT exempted under this rule and remain subject to standard layer checks; their long-term home is runtime or a documented justification. Source: `_archives/20260408-145000-validation-live-execution.md` Finding 6.

---

### Section: Contracts Boundary

Source: contracts-boundary.guard.md (absorbed verbatim 2026-04-14 per GUARD-LAYER-MODEL-01).

name: contracts-boundary
type: structural
severity: S1
locked: true
---

##### Contracts Boundary Enforcement Guard

##### Purpose
Preserve strict separation between business language (domain contracts) and
system mechanics (infrastructure/runtime contracts) within `src/shared/contracts/`.

##### Rules

##### G-CONTRACTS-01 — Domain Contracts Location
FAIL IF any domain-specific contract (command, query, intent, DTO, read model,
policy ID) exists outside the canonical path:
`src/shared/contracts/{classification}/{context}/{domain}/`

Domain classifications: `operational`, `economic`, `governance`, and future
classifications as they are onboarded.

##### G-CONTRACTS-02 — System Contracts Location
FAIL IF any system-level contract exists inside a domain classification path.
System contracts MUST live under one of:
- `contracts/events/{classification}/{context}/{domain}/` (event schemas only)
- `contracts/infrastructure/` (persistence, messaging, health, policy, projection, chain, admission)
- `contracts/runtime/` (command dispatch, workflow execution, control plane)
- `contracts/common/` (API envelope, standard response models)
- `contracts/engine/` (generic engine contracts)
- `contracts/event-fabric/` (event infrastructure)
- `contracts/identity/` (identity/auth)
- `contracts/chain/` (chain integrity/sequencing)
- `contracts/policy/` (system-wide policy evaluation contracts)
- `contracts/projection/` (projection infrastructure)
- `contracts/projections/{classification}/{context}/{domain}/` (cross-domain projection read models)

##### G-CONTRACTS-03 — Prohibited Top-Level Directories
FAIL IF any of the following directories are introduced under `src/shared/contracts/`:
- `contracts/domain/`
- `contracts/business/`
- `contracts/core/`

These generic groupings violate the classification-based domain structure.

##### G-CONTRACTS-04 — No Domain/System Mixing
FAIL IF a single directory contains both domain-specific contracts and
system-level contracts. Domain and system concerns occupy separate directory
subtrees with no overlap.

##### G-CONTRACTS-05 — Event Schema Location
FAIL IF event schemas exist outside `contracts/events/{classification}/{context}/{domain}/`.
Event schemas follow the domain three-level nesting but live under the `events/`
subtree, not alongside commands/queries in the domain contract path.

##### G-CONTRACTS-06 — Namespace Alignment
FAIL IF a contract file's declared namespace does not align with its directory
path. Domain contracts use `Whyce.Shared.Contracts.{Classification}.{Context}.{Domain}`.
System contracts use `Whyce.Shared.Contracts.{Category}` (e.g., `Infrastructure.Health`,
`Runtime`, `Common`). Event schemas use
`Whyce.Shared.Contracts.Events.{Classification}.{Context}.{Domain}`.

##### Severity
- All G-CONTRACTS-* rules: **S1 — HIGH** (block merge)

---

### Section: Code Quality Enforcement

Subsystem of Runtime Enforcement per GUARD-LAYER-MODEL-01 — consolidates clean-code, no-dead-code, stub-detection.

#### Clean Code


##### CLASSIFICATION

governance / clean-code / enforcement

##### MODE

MANDATORY — BLOCKING

---

##### PRINCIPLE

Clean code in Whycespace is defined as:

> Deterministic, readable, domain-aligned, non-over-engineered, and structurally consistent code that is easy to understand, test, and modify.

---

##### ENFORCEMENT RULES

###### CCG-01 — READABILITY (MANDATORY)

* All variables, methods, classes MUST use descriptive, intention-revealing names
* Single-letter variables are FORBIDDEN (except loop counters)
* Abbreviations are FORBIDDEN unless domain-standard

BLOCK IF:

* ambiguous naming detected
* non-semantic identifiers used

---

###### CCG-02 — FUNCTION SIZE & FOCUS

* Functions MUST do ONE thing only
* Max recommended length: 20–30 lines
* Nested depth MUST NOT exceed 3 levels

BLOCK IF:

* multiple responsibilities detected
* deep nesting (>3)
* large monolithic methods

---

###### CCG-03 — NO SPAGHETTI LOGIC

* Deep nested conditionals MUST be flattened
* Early returns MUST be preferred
* Flow MUST be linear and predictable

BLOCK IF:

* nested if/else chains > 3 levels
* branching chaos without clear flow

---

###### CCG-04 — NO OVER-ENGINEERING

* Do NOT introduce abstractions without clear necessity
* Avoid premature generalization
* Avoid unused interfaces, factories, patterns

BLOCK IF:

* unused abstractions
* speculative architecture
* indirection without value

---

###### CCG-05 — DOMAIN PURITY (CRITICAL)

* Business logic MUST exist ONLY in domain aggregates/entities
* No business logic in:

  * controllers
  * runtime
  * engines (outside orchestration role)

BLOCK IF:

* domain logic leakage detected outside domain layer

---

###### CCG-06 — LAYER ISOLATION

STRICT enforcement:

| Layer    | Allowed Access   |
| -------- | ---------------- |
| Platform | Systems only     |
| Systems  | Runtime only     |
| Runtime  | Engines only     |
| Engines  | Domain only      |
| Domain   | NOTHING external |

BLOCK IF:

* any cross-layer violation occurs

---

###### CCG-07 — DETERMINISM (CRITICAL)

FORBIDDEN:

* Guid.NewGuid()
* DateTime.UtcNow
* Random()

REQUIRED:

* DeterministicIdHelper
* Injected IClock

BLOCK IF:

* non-deterministic behavior detected

---

###### CCG-08 — SELF-DOCUMENTING CODE

* Code MUST express intent without comments
* Comments ONLY allowed for:

  * WHY, not WHAT

BLOCK IF:

* excessive comments explaining obvious logic
* unclear logic requiring explanation

---

###### CCG-09 — CONSISTENCY

* Naming conventions MUST be uniform
* Folder structure MUST follow canonical rules
* DDD structure MUST be complete

BLOCK IF:

* inconsistent naming
* structural deviations

---

###### CCG-10 — TESTABILITY

* Code MUST be:

  * deterministic
  * side-effect controlled
  * dependency-injected

BLOCK IF:

* hidden dependencies
* untestable logic

---

###### CCG-FILE-NAME-MATCHES-TYPE-01 — Source File Name Must Match Contained Public Type

For every file `src/**/*.cs` that contains exactly one public top-level type (class, record, struct, interface, enum, or delegate), the file name (without `.cs`) MUST equal the simple name of that type.

Exceptions:
1. Partial classes spread across multiple files MAY use the convention `{TypeName}.{Aspect}.cs` (e.g. `FooAggregate.Apply.cs`).
2. Files containing multiple public top-level types are out of scope (tracked separately under a `CCG-ONE-TYPE-PER-FILE-01` capture if introduced).
3. Auto-generated `.g.cs` / `.designer.cs` files are exempt.

**Severity:** S3 (formatting) when the contained type IS referenced by its real name elsewhere (silent drift — build is green, grep fails, README-as-evidence audit silently misses the file). Promote to **S2 (structural)** when the misleading file name contradicts a domain README boundary statement (e.g. `FxExecutedEvent.cs` containing `FxPairRegisteredEvent` in a domain whose README bans "Executed" semantics).

**Scan:** for each `*.cs` under `src/**`, parse public top-level type declarations; if exactly one and its simple name ≠ file basename, flag.

**Rationale:** filename–type alignment is the fastest visual contract between filesystem and code. Drift here defeats grep, defeats README-as-evidence audits, and lets boundary-violating names slip past code review.
**Source:** new-rules 2026-04-15 (exchange/fx post-fix sweep).

---

##### ENFORCEMENT MODE

* PRE-COMMIT: WARNING
* CI/CD: HARD BLOCK
* RUNTIME: NOT APPLICABLE

---

##### OUTPUT

Violation MUST return:

```
CLEAN_CODE_VIOLATION
Rule: <RULE_ID>
File: <path>
Reason: <description>
Fix: <required correction>
```

---

##### LOCK STATUS

LOCKED — CANONICAL

#### Dead Code Elimination


**Status:** ACTIVE
**Severity baseline:** S0 = must remove immediately; S1 = should remove.
**Owner:** WBSM v3 structural integrity.

##### Objective

Ensure the codebase contains only executable, referenced, and purposeful code.
Eliminate misleading, unused, or placeholder artifacts that introduce ambiguity.

---

##### Core Rule

A file MUST NOT exist in the repository if it has no runtime or compile-time impact,
unless it is explicitly allowed under the Exceptions section.

---

##### Definitions

###### Dead Code

Code is considered DEAD if ANY of the following are true:

1. It is not referenced anywhere in the codebase
2. It is not part of the build output
3. It is not used by runtime execution (API → Runtime → Engine → Domain → Projection)
4. It is not used by tests
5. It exists only as:

   * a placeholder
   * a redirect stub
   * commented-out logic
   * legacy artifact

---

##### Prohibited Patterns

The following MUST NOT exist:

###### 1. Redirect Stubs

Example:

```csharp
// Moved to ...
public class X {}
```

###### 2. Empty Classes

```csharp
public class TodoService {}
```

###### 3. Unused Interfaces / Implementations

* Interfaces with zero callers
* Implementations not registered or invoked

###### 4. Duplicate Execution Paths

* Old consumers alongside new ones
* Parallel patterns doing the same job

###### 5. Commented-Out Code Blocks

```csharp
// var x = ...
```

---

##### Allowed Exceptions

The following are allowed:

###### 1. Structural Placeholders

Used to preserve folder structure:

* `.gitkeep`

###### 2. Scaffolding for Future Layers

ONLY if empty and intentional:

* `T3I/`
* `T4A/`

Must contain:

* no logic
* no fake implementations

###### 3. Documentation (Outside Runtime Path)

* `/docs/`
* `/claude/`

NOT allowed inside:

* `/src/`

###### 4. Contracts in Active Use

* DTOs, Commands, Queries even if indirectly referenced

---

##### Enforcement Rules

###### R1 — Reference Check

Every class must have at least one of:

* direct reference
* DI registration
* runtime invocation
* test usage

###### R2 — Registration Check

If a class is meant to execute:

* it MUST be registered (DI / engine registry / workflow registry)

###### R3 — Projection Consumption

* No custom consumers if a generic worker exists
* All projections must be reachable via registry

###### R4 — Single Pattern Rule

* Only ONE valid implementation pattern per concern

---

##### Violation Severity

| Severity | Description                                      |
| -------- | ------------------------------------------------ |
| S0       | Dead code affecting runtime clarity or execution |
| S1       | Unused but harmless code                         |
| S2       | Cosmetic / formatting                            |

---

##### Action on Violation

* S0 → MUST be removed immediately
* S1 → SHOULD be removed
* S2 → optional cleanup

---

##### Example (Correct)

✔ Used handler registered in engine registry
✔ Projection handler registered in projection registry

##### Example (Violation)

✘ Old event consumer not used
✘ Empty service class
✘ Stub file with comment "moved to ..."

---

##### Canonical Principle

> If a developer cannot trace a file to execution, it must not exist.

---

##### Scope

Applies to:

* src/domain
* src/engines
* src/runtime
* src/systems
* src/projections
* src/platform
* src/shared

---

##### Summary

Dead code is not neutral—it creates false architecture.

This guard ensures:

* clarity
* determinism
* maintainability
* correct developer understanding

#### Stub Detection


**Status:** ACTIVE
**Severity baseline:** S0 fails build; S1 requires explicit registry entry.
**Owner:** WBSM v3 structural integrity.

##### SCOPE
All files under `src/`. Tests are out of scope.

##### RULES

###### STUB-R1 — Zero NotImplementedException on production path (S0)
`throw new NotImplementedException(...)` is FORBIDDEN in:
- `src/domain/**`
- `src/engines/**`
- `src/runtime/**`
- `src/platform/api/**`
- Anywhere on the Todo E2E path: API → Runtime → Engines → Persistence → Kafka → Response

If a method must be unimplemented, throw a structured domain exception with explicit reason, OR remove the method.

###### STUB-R2 — Zero TODO/FIXME/HACK on production path (S1)
Comments containing `TODO`, `FIXME`, `HACK`, `XXX` are forbidden in production code. Convert to GitHub issues or new-rules entries instead.

###### STUB-R3 — Placeholder implementations must be registered (S1)
A class is a "tracked placeholder" only if:
1. Class name begins with `InMemory` OR file/class XML doc contains the literal token `PLACEHOLDER (T-PLACEHOLDER-NN)`
2. There is a corresponding registry entry in `claude/registry/placeholders.json` (or equivalent) with:
   - `id` (matching `T-PLACEHOLDER-NN`)
   - `file`
   - `replacement_target` (e.g., the migration script or canonical implementation path)
   - `phase_gate` (which phase must replace it)
3. Architecture test enforces 1:1 between marker and registry.

Untracked placeholders are S1 violations.

###### STUB-R4 — No silent exception swallowing (S2)
Forbidden:
```csharp
catch { }
catch (Exception) { }
```
Allowed:
- `catch (OperationCanceledException) { return; }` in shutdown paths
- `catch (SpecificException ex) { _logger.LogDebug(ex, "..."); /* known recoverable */ }`

###### STUB-R5 — No empty interface implementations without doc (S2)
An empty `void` method implementing an interface contract requires an XML doc comment explaining why it is intentionally a no-op (e.g., "schema-only module owns no engines").

###### STUB-R6 — No hardcoded placeholder return values (S2)
`return true;`, `return 0;`, `return "ok";`, `return new List<T>();` as the entire method body is forbidden unless the method's contract permits it (verified by name like `IsAlwaysTrue` or interface explicitly states "returns empty when …").

##### CI ENFORCEMENT
1. **Architecture test:** grep `src/{domain,engines,runtime,platform/api}/**/*.cs` for `NotImplementedException` — fail.
2. **Architecture test:** grep `src/**/*.cs` for `\bTODO\b|\bFIXME\b|\bHACK\b|\bXXX\b` outside `// XML doc` — fail.
3. **Architecture test:** for every class matching `^InMemory.*` or comment `PLACEHOLDER \(T-PLACEHOLDER-\d+\)`, assert a registry entry exists in `claude/registry/placeholders.json`.
4. **Architecture test:** scan for `catch\s*\{\s*\}` — fail.

##### CURRENTLY TRACKED PLACEHOLDERS
- `T-PLACEHOLDER-01` — InMemoryWorkflowExecutionProjectionStore (replaced by Postgres impl after migration 002)
- `T-PLACEHOLDER-02` — InMemoryStructureRegistry (replaced by canonical constitutional registry)

(Both must be added to `claude/registry/placeholders.json` when that registry is created — see new-rules entry 20260408-132840-stub-detection.md.)

---

### Section: Test & E2E Validation

Subsystem of Runtime Enforcement per GUARD-LAYER-MODEL-01 — consolidates tests, e2e-validation.

#### Test Architecture


##### Purpose

Enforce test architecture rules across the WBSM v3 system. Tests must mirror the domain structure, respect layer boundaries, cover the full execution pipeline, and ensure that simulation capabilities exist for policy and chain validation. Tests are a constitutional enforcement layer — not optional, not advisory.

##### Scope

All test files across `tests/`. Applies to unit tests, integration tests, end-to-end tests, and simulation tests. Evaluated at CI, code review, and governance audit.

##### Rules

1. **UNIT TESTS MUST MIRROR DOMAIN** — The unit test folder structure must mirror `src/domain/` exactly. For each bounded context at `src/domain/{system}/{context}/{domain}/`, a corresponding test folder must exist at `tests/unit/{system}/{context}/{domain}/`. Each aggregate, entity, value object, service, and specification must have a corresponding test class following the `{ClassName}Tests` naming pattern.

2. **INTEGRATION TESTS MUST USE RUNTIME** — Integration tests must exercise the full runtime pipeline: command dispatch → middleware → engine → domain → event → projection. Integration tests must not call domain aggregates or engines directly. They must dispatch commands through the runtime command bus and verify results through the query/projection path. This validates the complete execution flow.

3. **E2E TESTS MUST COVER FULL PIPELINE** — End-to-end tests must cover the complete pipeline from platform entry point to persistence and projection. E2E tests verify: API request → platform → systems → runtime → engine → domain → event store → outbox → Kafka → projection. E2E tests use real infrastructure (or containerized equivalents) — no mocks at this level.

4. **NO INFRASTRUCTURE IN UNIT TESTS** — Unit tests must not reference any infrastructure type: no `DbContext`, no `HttpClient`, no Kafka types, no file system calls, no external service clients. Unit tests operate on pure domain logic with in-memory fakes or stubs for repository interfaces. Any infrastructure import in a unit test is a structural violation.

5. **SIMULATION TESTS MUST EXIST** — Policy simulation tests and chain verification tests must exist. Simulation tests validate:
   - Policy evaluation produces correct `DecisionHash` for known inputs
   - Chain anchoring produces valid `ChainBlock` entries
   - Policy simulation mode correctly reports what would happen without enforcing
   - Event replay produces consistent aggregate state
   Systems without simulation tests cannot pass governance audit.

---

##### WBSM v3 GLOBAL ENFORCEMENT

###### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

###### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

###### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

###### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

###### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

---

##### Check Procedure

1. Enumerate all BCs at D2 activation level and verify corresponding test folders exist in `tests/unit/`.
2. Verify each D2 aggregate has a corresponding `{Aggregate}Tests` class.
3. Scan integration test files for direct aggregate or engine instantiation (must use runtime pipeline).
4. Verify integration tests dispatch commands through `ICommandBus` or runtime mediator.
5. Verify E2E tests exercise the full platform-to-projection pipeline.
6. Scan unit test files for infrastructure imports (`DbContext`, `HttpClient`, `Confluent.Kafka`, etc.).
7. Verify simulation test files exist for policy evaluation and chain anchoring.
8. Verify simulation tests cover: DecisionHash generation, ChainBlock creation, policy simulation mode, event replay consistency.
9. Verify test naming follows `{ClassName}Tests` pattern.
10. Verify test folder structure mirrors domain structure.

##### Pass Criteria

- All D2-level BCs have mirrored unit test folders.
- All aggregates, services, and specifications at D2 have test classes.
- Integration tests use runtime pipeline exclusively.
- E2E tests cover full pipeline.
- Zero infrastructure imports in unit tests.
- Simulation tests exist for policy and chain.
- Test folder structure mirrors domain structure.

##### Fail Criteria

- D2-level BC without corresponding test folder.
- Aggregate without test class.
- Integration test calling domain/engine directly (bypassing runtime).
- E2E test missing pipeline stages.
- Infrastructure import in unit test.
- Missing simulation tests for policy or chain.
- Test folder structure does not mirror domain.

##### Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Integration test bypasses runtime | Test calls `aggregate.Apply()` directly instead of dispatching command |
| **S0 — CRITICAL** | Missing simulation tests | No policy simulation or chain verification tests exist |
| **S1 — HIGH** | Infrastructure in unit test | `using Microsoft.EntityFrameworkCore;` in unit test |
| **S1 — HIGH** | D2 BC without test folder | `economic-system/capital/vault/` has no test mirror |
| **S2 — MEDIUM** | Missing aggregate test | `VaultAggregate` has no `VaultAggregateTests` class |
| **S2 — MEDIUM** | E2E test missing stages | E2E test skips outbox/Kafka stage |
| **S3 — LOW** | Test naming violation | `VaultTest` instead of `VaultAggregateTests` |
| **S3 — LOW** | Test folder structure drift | Test folder doesn't match domain folder hierarchy |

##### Enforcement Action

- **S0**: Block merge. Fail CI. Immediate remediation required.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
TESTS_GUARD_VIOLATION:
  test_type: <unit|integration|e2e|simulation>
  bc: <classification/context/domain>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct test structure>
  actual: <detected issue>
  remediation: <fix instruction>
```

---

##### NEW RULES INTEGRATED — 2026-04-07

- **T-BUILD-01**: tests/integration/ MUST compile on every CI run. A red integration project halts merge. The CI gate fails when "dotnet build tests/integration/Whycespace.Tests.Integration.csproj" fails.
- **T-DOUBLES-01**: All in-memory test doubles (InMemoryChainAnchor, InMemoryOutbox, InMemoryEventStore, etc.) MUST take IClock and IIdGenerator constructor parameters. No Guid.NewGuid() / DateTimeOffset.UtcNow inside test doubles.
- **T-PLACEHOLDER-01**: In-memory repository implementations used in production composition MUST be clearly marked as placeholders AND have a corresponding migration script in scripts/migrations/ ready for swap.

##### NEW RULES INTEGRATED — 2026-04-07 (workflow resume test coverage)

- **T1M-RESUME-TEST-COVERAGE-01** (S2): A `T1MWorkflowHarness` test fixture MUST exist under
  `tests/integration/orchestration-system/workflow/` wiring `T1MWorkflowEngine`, `WorkflowStepExecutor`,
  in-memory `IWorkflowRegistry`, and a real `IEventStore`. Required scenarios: resume midway
  (cursor = 2 of 4 → executes steps 2,3); resume completed → fails with "not in resumable state";
  resume after failure → re-runs the failed step per chosen policy. `NoOpWorkflowEngine` stubs do not
  satisfy this rule.
- Source: `claude/new-rules/_archives/20260407-230000-workflow-resume-payload-and-test-coverage.md`.

##### NEW RULES INTEGRATED — 2026-04-10 (promoted from new-rules backlog)

- **ACT-FABRIC-ROUNDTRIP-TEST-01** (S0): Every domain event whose schema is registered with `IEventSchemaRegistry` MUST have at least one integration test that constructs the event, persists it via the real `IEventFabric` (not a double), reloads it from the real event store, and asserts payload integrity round-trip. Tests that bypass the real fabric MUST be tagged `[Trait("Fabric","Bypass")]`. CI gate enumerates registered events and fails the build on any uncovered registration. Rationale: prevents the `WorkflowExecutionResumedEvent` class of drift where in-memory doubles green-light a path the real fabric does not know about. Source: `_archives/20260408-103326-activation.md`.
- **G-E2E-010 — Untested = FAIL** (S1): Validation, audit, and e2e prompts MUST treat any test case that is unreachable, un-runnable, or skipped as a FAILURE, never a SKIP. Reports listing SKIP in place of executable evidence are non-compliant. Applies cross-cutting to all test/validation/audit prompts. Source: `_archives/20260408-142631-validation.md` Finding 1.
- **T-POLICY-001** (S1): Test fixtures touching the command path MUST assert non-null `decision_hash` and `policy.version` on the resulting event/command-result. Production code is covered by `policy-binding.guard.md`; this closes the test-side gap. Source: `_archives/20260408-142631-validation.md` Finding 3.
- **T-BUILD-01 STRENGTHENING** (S1): A commit that changes a production interface signature (constructor args or interface members) MUST update the corresponding test doubles in `tests/integration/setup/` in the SAME commit. Orphaned test doubles are an S1 architectural violation. The executable enforcement is `claude/audits/canonical/runtime.audit.md` §Test & E2E Validation (T-BUILD-01). Source: `_archives/20260409-120500-infrastructure-tests-integration-baseline-drift.md`.

#### E2E Validation


**Classification:** validation
**Scope:** Phase 1.5 certification gate. Loaded before any prompt that claims to validate, certify, or smoke-test the Whycespace system end-to-end.

##### RULES

###### G-E2E-001 — No PASS without execution evidence
Any test entry in `/docs/validation/*.md` marked `STATUS: PASS` MUST carry an `EVIDENCE:` block containing at minimum: command run, exit code, captured event_id(s), kafka offset, and timestamp of execution. Missing evidence = `STATUS: FAIL — NOT EXECUTED`.

###### G-E2E-002 — Layer coverage is mandatory
Every E2E test MUST exercise: API → Runtime → Engine → Event Store → Kafka → Projection → Read API. Skipping any layer = S1 violation. Single-layer unit assertions are NOT E2E.

###### G-E2E-003 — Determinism in fixtures
Test fixtures MUST NOT embed `Guid.NewGuid()`, `DateTime.UtcNow`, `new Random()`, or wall-clock-derived IDs. All IDs derived via `IDeterministicIdGenerator`; all clocks via `IClock`. Violations = S1 ($9).

###### G-E2E-004 — Policy decision required
Every command-side test MUST capture `policy.decision`, `policy.decision_hash`, `policy.version`. Absence = S0 ($8 — no operation may bypass WHYCEPOLICY).

###### G-E2E-005 — Chain anchor required
Every event-emitting test MUST capture `chain.block_id` and `chain.hash`. Hash MUST be reproducible across two runs of the same fixture. Non-reproducibility = S1 ($9).

###### G-E2E-006 — DLQ before commit
Failure-injection tests MUST assert that on engine/projection/consumer failure the message lands on the DLQ topic BEFORE the source offset is committed. Commit-then-DLQ = S0 (data loss risk).

###### G-E2E-007 — Replay equivalence
Every aggregate touched by an E2E test MUST be replayable from the event store and produce a byte-equal projection state. Divergence = S1.

###### G-E2E-008 — No test self-cleanup that hides failures
Tests MUST NOT delete event-store rows, kafka topics, or projection state on failure. Cleanup runs ONLY on PASS, after evidence capture.

###### G-E2E-009 — Severity ladder
Failures classified per source prompt §14: CRITICAL (blocks Phase 1.5) / HIGH / MEDIUM / LOW. CRITICAL is reserved for: data loss, policy bypass, chain break, replay divergence, DLQ-after-commit.

###### G-E2E-011 — Static checks are STAGE 0
`scripts/validation/run-e2e.sh` MUST invoke every `scripts/*-check.sh` as STAGE 0 before any HTTP/Kafka call. Any non-zero exit aborts the run with status `FAIL — STAGE 0`. Rationale: cheap signal catches config and dependency bugs before expensive live execution.

###### G-E2E-010 — Untested = FAIL
Per source §15, any case the harness cannot execute (missing service, missing fixture, environmental gap) is recorded as `FAIL — NOT EXECUTED` with `severity: CRITICAL` if it sits on the gate path. Silent skips are forbidden.

##### INTEGRATION
- Loaded by `$1a` pre-execution stage for any prompt classified `validation` or `phase1.5-gate`.
- Audited by `claude/audits/canonical/runtime.audit.md` §Test & E2E Validation (G-E2E-001..011).

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

Per GUARD-LAYER-MODEL-01 (4-layer canonical model, LOCKED 2026-04-14):
- **Determinism, Deterministic Identifiers (HSID), Hash Determinism, Replay Determinism** live in `constitutional.guard.md`. Rule IDs DET-*, G1..G8/G12..G20, HASH-*, REPLAY-*, POLICY-REPLAY-INTEGRITY-01 are owned there, not here.
- **Composition Loader (G-COMPLOAD-*) and Program Composition (G-PROGCOMP-*)** live in `infrastructure.guard.md` (host-process assembly).
- **Domain purity, structural, behavioral, DTO naming, classification-suffix, and domain-aligned (economic/governance/identity/observability/workflow)** live in `domain.guard.md`.
- This guard (`runtime.guard.md`) is authoritative for the Runtime enforcement surface: execution order, engine purity, projections, prompt container, dependency graph & layer boundaries, contracts boundary, code quality enforcement (Runtime subsystem), and test/E2E validation (Runtime subsystem).

Where this guard and another canonical guard overlap in subject matter (e.g., GE-01 deterministic execution appears in all four canonical files as the shared WBSM v3 baseline), the stricter rule wins; the GE-01..05 block appears exactly once within each canonical file as a shared appendix.

## Locked-Status

All sections are canonical and non-regressible. Any future workstream that needs to amend a rule above MUST:
1. Open a `claude/new-rules/{ts}-runtime.md` capture with `STATUS: PROPOSED`.
2. Reference the specific rule being amended.
3. Include a regression-coverage test that locks the new behavior.
4. Update this file in the same patch as the amendment.

---

## Rules Promoted from new-rules/ (2026-04-18)

Rules below were captured in `claude/new-rules/` per CLAUDE.md $1c and promoted into this guard on 2026-04-18. Rule IDs are indexed in `claude/audits/runtime.audit.md`.

### RUNTIME-LAYER-PURITY-01 — Engine → Runtime Namespace Isolation

Definition:
Files under `src/engines/**` MUST NOT reference the `Whycespace.Runtime.*` namespace. Metrics, tracing, logging, and persistence are runtime concerns invoked *by* the host around engine execution, never *from* inside engine code. Strengthens E7 (no cross-tier engine imports) by forbidding engine → runtime specifically.

Enforcement:
`dotnet build src/engines/Whycespace.Engines.csproj` MUST succeed without any `Whycespace.Runtime.*` project reference or `using` directive present under `src/engines/**`.

Severity:
S0

References:
- E7 (no cross-tier engine imports)
- DG-R2 (engine isolation edge)
- Source: `claude/new-rules/20260417-100926-audits.md`

### SYSTEM-ORIGIN-BYPASS-01 — System-Origin Bypass Flag for Compensation

Definition:
`CommandContext.IsSystem` is an init-only flag set exclusively by `ISystemIntentDispatcher.DispatchSystemAsync`. `EnforcementGuard.RequireNotRestricted` bypasses restriction checks when `isSystem=true`. Both forward-progress and compensation commands route through this path. Two architecture tests pin the invariant: `IsSystem_flag_is_only_set_by_SystemIntentDispatcher` and `Api_controllers_do_not_reference_IsSystem`.

Severity:
S2

References:
- R-SYS-14 (`ISystemIntentDispatcher` entry point)
- Source: `claude/new-rules/20260417-105914-audits.md`

### STEP-EXCEPTION-CONTRACT-01 — Workflow Step Exception Uniformity

Definition:
Either (a) `IWorkflowStep.ExecuteAsync` callers MUST explicitly `try`/`catch` known infrastructure exceptions (resolver/store failures) and translate them to `WorkflowStepResult.Failure` with the original message, OR (b) the workflow runtime MUST guarantee uniform translation of unhandled step exceptions to `WorkflowStepResult.Failure` with the exception message preserved. Either contract is acceptable; current state assumes (b) without a test proving it.

Severity:
S3

References:
- E-STEP-01 / E-STEP-02 (engine step constraints)
- Source: `claude/new-rules/20260417-120050-economic-system-phase3-6-final-residual.md` (Finding 11)

### ESCAPE-HATCH-COMMITMENT-01 — Escape-Hatch Shelf-Life Deadline

Definition:
Code retained as an "escape-hatch shell" MUST carry a tracked decision deadline (e.g., `// ESCAPE-HATCH-DEADLINE: 2026-Q3 — remove or extend dispatcher contract`). Without a deadline, escape-hatch code accumulates as dead-code rot.

Severity:
S3

References:
- Dead Code R1..R4 (reference check / registration / consumption / single-pattern)
- STUB-R5 (stub registry discipline)
- Source: `claude/new-rules/20260417-122320-economic-system-phase4-5-final-residual.md`

### R-RT-USING-RUNTIME-01 — Runtime Contract Using Requirement

Definition:
Step and handler files under `src/engines/**/steps/` and `tests/**/*Step*Tests.cs` that reference `IWorkflowStep`, `WorkflowExecutionContext`, `WorkflowStepResult`, `IHasAggregateId`, or `CommandResult` MUST carry `using Whycespace.Shared.Contracts.Runtime;`. CI grep enforcement required.

Severity:
S2

References:
- G-CONTRACTS-01..06 (contracts boundary)
- Source: `claude/new-rules/20260417-123910-audits.md`

### R-TEST-PATH-01 — Absolute Path Literals Forbidden in Tests

Definition:
Test files MUST resolve repo-relative paths via `AppContext.BaseDirectory` / git-root discovery. No absolute path literals (`C:\…`, `/home/…`, `/Users/…`) anywhere under `tests/**`. CI grep pattern: `["']([A-Z]:\\|/home/|/Users/)` in `.cs` under `tests/**` = S2 fail.

Severity:
S2

References:
- Test Architecture rules 1–5 (isolation, determinism)
- Source: `claude/new-rules/20260417-125532-audits.md`

### R-TEST-PROJREF-01 — Test ProjectReference Alignment

Definition:
Any `.cs` under `tests/<project>/**` that transitively uses a top-level project MUST have the matching `ProjectReference` in `tests/<project>/Whycespace.Tests.<Project>.csproj`. CI roslyn analyzer: build MUST succeed without relying on `<Compile Remove>` shims.

Severity:
S2

References:
- T-BUILD-01 (build gate)
- Source: `claude/new-rules/20260417-125532-audits.md`

### PIPELINE-CANONICAL-ANCHOR-01 — Pipeline File Presence Gate

Definition:
Any prompt invoking `/pipeline/*` integration MUST first verify presence of referenced files. If a referenced file is missing, execution halts per CLAUDE.md $12 with `ACTION_REQUIRED = (a) restore pipeline files, OR (b) declare §pipeline clause superseded in an updated prompt`. Partial execution is forbidden.

Severity:
S0

References:
- CLAUDE.md $12 (failure semantics)
- GUARD-PIPELINE-TEMPLATE-01 (`/pipeline/*.md` templates)
- Source: `claude/new-rules/20260418-093419-audits.md`
