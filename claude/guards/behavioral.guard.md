# Behavioral Guard

## Purpose

Enforce runtime behavior rules that govern how components interact at execution time. Structural correctness alone is insufficient — this guard ensures that the system behaves according to WBSM v3 doctrine during operation: correct flow direction, proper isolation, and deterministic execution paths.

## Scope

All executable code under `src/`. Applies to method bodies, constructor logic, event handlers, and service implementations. Evaluated at code review, CI, and integration test phases.

## Rules

1. **NO DIRECT DB FROM DOMAIN** — Domain layer must never perform direct database access. No `DbContext`, `IDbConnection`, `SqlCommand`, repository implementation, or ORM calls in `src/domain/`. Domain defines repository interfaces only; implementations live in `infrastructure/`.

2. **NO ENGINE-TO-ENGINE CALLS** — Engines must not invoke other engines directly. An engine at any tier (T0U-T4A) must not hold a reference to or call methods on another engine. Cross-engine coordination flows through runtime only.

3. **NO DOMAIN MUTATION FROM SYSTEMS** — The systems layer must never modify domain state. Systems compose and orchestrate but do not create, update, or delete domain aggregates. All state changes originate from engines invoked through runtime.

4. **NO BYPASS OF RUNTIME CONTROL PLANE** — All command and event routing must pass through the runtime layer. No component may short-circuit the control plane by directly invoking domain services or engine operations outside the runtime pipeline.

5. **COMMANDS FLOW THROUGH RUNTIME ONLY** — Command objects must be dispatched exclusively through runtime command handlers. No direct instantiation and execution of command handlers outside the runtime pipeline. Platform dispatches commands to runtime; runtime routes to engines.

6. **EVENTS ARE IMMUTABLE AFTER PUBLISH** — Once a domain event is published (raised by an aggregate and dispatched by runtime), its properties must not be modified. Event classes must be immutable (readonly properties, no setters). No post-publish mutation is permitted.

7. **NO SYNCHRONOUS CROSS-BC CALLS** — Bounded contexts must not call each other synchronously. Cross-BC communication must use domain events (asynchronous). No direct method invocation between aggregates in different BCs. Integration events bridge BC boundaries.

8. **DETERMINISTIC COMMAND HANDLING** — Command handlers must be deterministic: same input produces same side effects. No reliance on ambient state, current time (must be injected), or random values (must be seeded/injected). System clock access must use an injectable `ITimeProvider`.

9. **NO SIDE EFFECTS IN QUERIES** — Query handlers and read operations must not produce side effects. No state mutation, no event publishing, no command dispatching from within a query handler or projection read.

10. **AGGREGATE TRANSACTION BOUNDARY** — Each command targets exactly one aggregate. A single transaction must not span multiple aggregates. Cross-aggregate consistency is achieved through eventual consistency via domain events.

11. **NO STATIC MUTABLE STATE** — No static mutable fields or properties in domain, engine, or runtime code. Static state breaks isolation, testability, and multi-tenancy. Only immutable constants and pure static methods are permitted.

12. **EXCEPTION BOUNDARY ENFORCEMENT** — Domain exceptions must not leak to platform. Engines catch domain exceptions and translate to result types. Runtime translates engine results to platform-consumable responses. No raw exception propagation across layers.

13. **POLICY DECISION REQUIRED BEFORE EXECUTION** — Every command must include a `PolicyDecision` in its execution context before reaching the engine. Commands without an attached, evaluated policy decision must be rejected at the runtime pipeline. No engine may execute a command that lacks a policy decision artifact.

14. **CHAIN LINK REQUIRED** — Every execution that mutates domain state must produce a `ChainBlock` entry linking the action to the immutable ledger. The chain link includes: action hash, policy decision hash, actor, timestamp, and correlation ID. Executions without chain anchoring are governance violations.

15. **EVENT MUST FOLLOW EVERY STATE CHANGE** — No domain mutation may occur without emitting at least one domain event. Silent state changes (direct field assignment without event) are forbidden. Every `Apply()`, `Create()`, `Update()`, or state transition must raise a corresponding past-tense domain event.

16. **NO CLOCK OR RANDOM ACCESS** — All temporal and identity generation must use injected providers (`ITimeProvider`, `IIdGenerator`). Direct use of `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `Guid.NewGuid()`, `Random`, or `Environment.TickCount` in domain, engine, or runtime code is forbidden.

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

1. Scan `src/domain/` for any database-related types (`DbContext`, `IDbConnection`, `SqlCommand`, `EntityFrameworkCore`, `Dapper`, `NpgsqlConnection`).
2. Scan `src/engines/` for imports or instantiations of other engine types.
3. Scan `src/systems/` for aggregate method calls that mutate state (`.Create()`, `.Update()`, `.Apply()`, `.RaiseEvent()`).
4. Scan for command handler invocations outside `src/runtime/` pipeline.
5. Verify all domain event classes have only `{ get; }` or `{ get; init; }` properties — no `{ get; set; }`.
6. Scan for direct method calls between aggregates in different BC namespaces.
7. Verify command handlers do not call `DateTime.Now`, `DateTime.UtcNow`, `Guid.NewGuid()` directly — must use injected providers.
8. Scan query handlers for write operations, event publishing, or command dispatching.
9. Verify each command handler targets a single aggregate repository.
10. Scan for `static` fields with mutable types (`static List<>`, `static Dictionary<>`, `static int`, etc.) in domain/engine/runtime.
11. Verify domain exception types are caught at engine or runtime boundary.

## Pass Criteria

- Zero forbidden patterns detected across all scans.
- All domain events are immutable.
- All command handlers are deterministic (no ambient state access).
- All cross-BC communication uses async events.
- No engine-to-engine direct invocation found.
- No domain mutation found in systems layer.
- All commands routed through runtime.

## Fail Criteria

- Domain file contains direct database access code.
- Engine file instantiates or calls another engine.
- Systems file mutates domain aggregate state.
- Command handler invoked outside runtime pipeline.
- Domain event class has mutable property (`{ get; set; }`).
- Synchronous method call crosses BC boundary.
- Command handler uses `DateTime.Now` or `Guid.NewGuid()` directly.
- Query handler produces side effects.
- Single transaction spans multiple aggregates.
- Static mutable state detected in domain/engine/runtime.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Direct DB access in domain | `_dbContext.SaveChanges()` in aggregate |
| **S0 — CRITICAL** | Domain mutation from systems | `aggregate.Apply(event)` in systems layer |
| **S0 — CRITICAL** | Mutable domain event | `public string Name { get; set; }` on event |
| **S0 — CRITICAL** | Engine persistence attempt | `context.Save()`, `_repository.Save()`, `DbContext.SaveChanges()` in engine |
| **S0 — CRITICAL** | Engine publishing event externally | Engine calls `eventBus.Publish()`, `IEventPublisher`, Kafka produce |
| **S0 — CRITICAL** | Engine calling infra | Engine calls Redis, Kafka, HTTP, SQL, file I/O directly |
| **S0 — CRITICAL** | Runtime bypass | Any path invoking engine without RuntimeControlPlane middleware pipeline |
| **S1 — HIGH** | Engine-to-engine call | T2E engine instantiating T1M engine |
| **S1 — HIGH** | Synchronous cross-BC call | `identityService.GetUser()` from billing aggregate |
| **S2 — MEDIUM** | Non-deterministic handler | `DateTime.UtcNow` in command handler |
| **S2 — MEDIUM** | Multi-aggregate transaction | Two repository saves in one handler |
| **S3 — LOW** | Static mutable state | `static int counter` in engine |
| **S3 — LOW** | Exception leakage | Domain exception reaching platform unhandled |

## Enforcement Action

- **S0**: Block merge. Fail CI immediately. Mandatory fix before any further review.
- **S1**: Block merge. Fail CI. Must be resolved in current PR.
- **S2**: Warn in CI. Create tracking issue. Must be resolved within sprint.
- **S3**: Advisory warning. Log for tech debt review.

All violations produce a structured report:
```
BEHAVIORAL_GUARD_VIOLATION:
  file: <path>
  method: <method name>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  pattern_detected: <code snippet>
  remediation: <fix instruction>
```
