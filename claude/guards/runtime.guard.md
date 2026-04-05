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
