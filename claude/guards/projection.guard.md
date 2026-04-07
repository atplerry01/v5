# Projection Guard — Dual Projection Architecture

## Purpose

Enforce the Dual Projection Architecture across WBSM v3. Two projection layers exist with strictly separated responsibilities:

1. **Runtime Projections** (`src/runtime/projection/`) — internal execution support ONLY
2. **Domain Projections** (`src/projections/`) — business-facing read models / CQRS query layer

Both layers are mandatory, isolated, non-overlapping, and strictly enforced.

## Scope

All projection handler classes, read model definitions, and projection configuration across `src/runtime/projection/` and `src/projections/`. Evaluated at CI, code review, and architectural audit.

---

## PART A — SHARED PROJECTION RULES (Apply to BOTH layers)

1. **PROJECTIONS ARE READ-ONLY** — Projection handlers must only write to their own dedicated read-model store. They must not write to the domain's write-model store, modify aggregate state, or trigger domain side effects. A projection's output is a denormalized view optimized for queries.

2. **PROJECTIONS CONSUME EVENTS ONLY** — Projection handlers must subscribe to domain events (past-tense). They must never consume commands, invoke command handlers, or listen for request types. The input to a projection is always `{Subject}{PastTenseVerb}Event`.

3. **NO WRITE OPERATIONS IN PROJECTIONS** — Projection handlers must not:
   - Call domain aggregate methods.
   - Invoke command handlers or dispatch commands.
   - Publish new domain events.
   - Call repository `Save()`, `Update()`, `Delete()` on domain aggregates.
   - Write to external systems (APIs, queues, files) as a side effect.
   The only permitted write is upserting the projection's own read model.

4. **PROJECTIONS ARE EVENTUALLY CONSISTENT** — Projections acknowledge that their data may be stale. No projection may be used as the authoritative source for write-side decisions. Read models must not be fed back into command validation. The write model (domain aggregates) is the single source of truth.

5. **PROJECTION IDEMPOTENCY** — Projection handlers must be idempotent: processing the same event twice must produce the same read-model state. Projections must handle duplicate delivery gracefully. Use event sequence numbers or idempotency keys.

6. **PROJECTION REBUILDS** — Projections must support full rebuild from the event stream. No projection may rely on state that is not derivable from the event history. If the read model is deleted, replaying all events must reconstruct it identically.

7. **ONE PROJECTION PER READ MODEL** — Each projection handler maps to exactly one read model. A single handler must not update multiple unrelated read models. If multiple views need the same event, create separate projection handlers.

8. **NO DOMAIN LOGIC IN PROJECTIONS** — Projections must not contain domain business rules. They flatten, denormalize, and aggregate event data — but they do not evaluate business conditions or enforce invariants. If a projection contains `if (status == Approved && amount > threshold)` to make a business decision, it is a violation.

9. **PROJECTION NAMING CONVENTION** — Projection handlers must follow the naming pattern: `{ReadModel}Projection` or `{ReadModel}ProjectionHandler`. Read models must follow: `{Entity}{View}ReadModel` or `{Query}View`. Names must clearly indicate their read-model purpose.

10. **PROJECTION DATA IS NON-AUTHORITATIVE** — Projection read models must never be treated as the source of truth for write-side decisions. No command handler, engine, or domain service may query a projection store to make a business decision. The event store and domain aggregates are the sole authoritative sources.

11. **STRICT CQRS SEPARATION** — The write path (command > runtime > engine > domain > event store) and the read path (event > projection > read model > query) must be completely independent. No shared database tables, no shared connections, no shared transaction contexts between write and read paths.

12. **EVENT ORDERING GUARANTEE REQUIRED** — Projections must process events in the order they were produced per aggregate. Out-of-order event processing produces corrupted read models. Projections must track the last processed event sequence number and reject or requeue events that arrive out of order.

13. **CONTEXT FIELDS REQUIRED** — All projection handlers MUST include in their event processing context:
    - `CorrelationId`
    - `EventId`
    - `IdempotencyKey`

---

## PART B — RUNTIME PROJECTION RULES (`src/runtime/projection/`)

14. **RUNTIME PROJECTIONS ARE EXECUTION SUPPORT ONLY** — Runtime projections serve internal execution needs: workflow state tracking, idempotency tracking, policy linking, runtime context views. They are NOT business-facing read models.

15. **RUNTIME PROJECTIONS NOT EXPOSED EXTERNALLY** — Runtime projections must NOT be exposed via API endpoints, query handlers, or any external-facing interface. They are internal to the runtime control plane.

16. **RUNTIME PROJECTIONS MAY BE SYNCHRONOUS** — Unlike domain projections, runtime projections may use synchronous event processing when required for execution support.

17. **RUNTIME PROJECTIONS MUST NOT WRITE TO REDIS** — Runtime projections must NOT write to Redis or any shared read-model store. They use internal ephemeral state only (unless strictly internal ephemeral state is required).

18. **RUNTIME PROJECTIONS OWN INTERNAL STATE ONLY** — Runtime projections may only access:
    - Runtime internal modules
    - Shared contracts (`src/shared/`)

19. **RUNTIME PROJECTIONS MUST NOT REFERENCE DOMAIN PROJECTIONS** — `src/runtime/projection/` must NEVER reference `src/projections/`. No imports, no shared handlers, no cross-layer coupling.

---

## PART C — DOMAIN PROJECTION RULES (`src/projections/`)

20. **DOMAIN PROJECTIONS ARE EVENT-DRIVEN ONLY** — All domain projection handlers MUST consume events via Kafka/event fabric. No direct method invocation from runtime, systems, or any other layer. No synchronous event processing.

21. **DOMAIN PROJECTIONS MUST NOT REFERENCE RUNTIME** — `src/projections/` must NEVER reference `src/runtime/`. No imports, no shared state, no cross-layer coupling.

22. **DOMAIN PROJECTIONS MUST NOT REFERENCE DOMAIN** — `src/projections/` must NEVER reference `src/domain/`. Domain projections consume event schemas and shared contracts only.

23. **DOMAIN PROJECTIONS MUST NOT REFERENCE ENGINES** — `src/projections/` must NEVER reference `src/engines/`. No engine imports or dependencies.

24. **DOMAIN PROJECTIONS ALLOWED DEPENDENCIES** — `src/projections/` may ONLY reference:
    - `src/shared/` (contracts, primitives)
    - `infrastructure/` adapters (Redis clients, persistence adapters)
    - Event schemas

25. **DOMAIN PROJECTIONS OWN REDIS/READ-STORE** — Redis and materialized view writes MUST originate ONLY from `src/projections/`. No other layer may write to the domain read-model store.

26. **DOMAIN PROJECTIONS ARE THE ONLY QUERY SOURCE** — All external query endpoints (API layer) MUST read from `src/projections/` read models. No API endpoint may query runtime projections directly.

27. **DOMAIN PROJECTIONS MUST SUPPORT REPLAY** — Domain projections must be event-versioning safe. Replaying the full event stream must reconstruct the read model identically. Event reprocessing must be safe and idempotent.

---

## PART D — PROJECTION ISOLATION GUARD (S24)

28. **DEPENDENCY ISOLATION** —
    - `src/runtime/` MUST NOT reference `src/projections/`
    - `src/projections/` MUST NOT reference `src/runtime/`
    - `src/projections/` MUST NOT reference `src/domain/`
    - `src/projections/` MUST NOT reference `src/engines/`

29. **EVENT-DRIVEN ENFORCEMENT** —
    - All `src/projections/` handlers MUST consume events ONLY
    - NO direct method invocation from runtime into domain projections

30. **STORAGE OWNERSHIP** —
    - Redis/read-store writes ONLY from `src/projections/`
    - Runtime projections MUST NOT write to Redis

31. **EXPOSURE RULES** —
    - `src/runtime/projection/` MUST NOT be exposed via API
    - `src/projections/` MUST be the ONLY query source for external consumers

32. **IDEMPOTENCY ENFORCEMENT** —
    - All projection handlers MUST be idempotent
    - Must support replay (event versioning safe)

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

### Runtime Projections (`src/runtime/projection/`)
1. Verify all projection handlers reside in `src/runtime/projection/` (not scattered in other runtime folders).
2. Verify NO runtime projection is exposed via API or query endpoint.
3. Verify NO runtime projection references `src/projections/`.
4. Verify NO runtime projection writes to Redis or shared read-model store.
5. Scan for command dispatch calls in runtime projection handlers.

### Domain Projections (`src/projections/`)
1. Verify all domain projection handlers reside in `src/projections/`.
2. Verify NO domain projection references `src/domain/`, `src/runtime/`, or `src/engines/`.
3. Verify ALL domain projections consume events via Kafka/event fabric (no synchronous invocation).
4. Verify ALL domain projection handlers include CorrelationId, EventId, IdempotencyKey.
5. Verify Redis/read-store writes originate ONLY from `src/projections/`.
6. Scan for command dispatch calls, aggregate mutations, and event publications.
7. Verify idempotency mechanisms (sequence tracking, upsert patterns).
8. Verify replay safety (event reprocessing produces identical state).

### Cross-Layer Isolation
1. Parse all `using` directives in `src/projections/` — reject any referencing Runtime, Domain, or Engines namespaces.
2. Parse all `using` directives in `src/runtime/projection/` — reject any referencing Projections namespace.
3. Verify `.csproj` project references: `src/projections/` may only reference `src/shared/`.
4. Verify no API controller queries runtime projections directly.

## Pass Criteria

- All projections consume events only (no commands).
- Runtime projections in `src/runtime/projection/` ONLY — not exposed externally.
- Domain projections in `src/projections/` ONLY — event-driven, no runtime/domain/engine deps.
- Zero cross-layer dependency violations between the two projection layers.
- Zero command dispatches from projections.
- Zero domain aggregate mutations from projections.
- Zero event publications from projections.
- All projections are idempotent.
- All projections support full rebuild.
- No domain business logic in projections.
- Redis writes originate only from `src/projections/`.

## Fail Criteria

- Projection handler consumes a command type.
- Projection handler dispatches a command.
- Projection handler mutates a domain aggregate.
- Projection handler publishes a domain event.
- Domain projection references runtime, domain, or engines.
- Runtime projection references domain projections.
- Runtime projection exposed via API.
- Domain projection invoked synchronously (not via event fabric).
- Redis write from runtime projection layer.
- API endpoint queries runtime projection directly.
- Projection file located in domain, engine, systems, or platform layer.
- Projection contains domain business rule logic.
- Projection is not idempotent.
- Projection handler missing CorrelationId, EventId, or IdempotencyKey.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Projection mutates domain aggregate | `aggregate.Apply(event)` in projection handler |
| **S0 — CRITICAL** | Projection dispatches command | `_commandBus.Send(new CreateOrder())` in projection |
| **S0 — CRITICAL** | Projection publishes event | `_eventBus.Publish(newEvent)` in projection |
| **S0 — CRITICAL** | Domain projection references runtime/domain/engines | `using Whycespace.Runtime;` in `src/projections/` |
| **S0 — CRITICAL** | Runtime projection references domain projections | `using Whycespace.Projections;` in `src/runtime/projection/` |
| **S0 — CRITICAL** | Runtime projection exposed via API | API controller returns runtime projection data |
| **S1 — HIGH** | Projection in wrong layer | Projection class in `src/domain/` or `src/systems/` |
| **S1 — HIGH** | Projection consumes command | Handler subscribes to `CreateOrderCommand` |
| **S1 — HIGH** | Projection contains business rules | `if (total > creditLimit) flag = true` |
| **S1 — HIGH** | Domain projection invoked synchronously | Direct method call instead of Kafka consumer |
| **S1 — HIGH** | Redis write from runtime projection | Runtime projection writing to shared Redis store |
| **S2 — MEDIUM** | Non-idempotent projection | Insert without upsert; fails on replay |
| **S2 — MEDIUM** | Projection updates multiple read models | Single handler writing to two different views |
| **S2 — MEDIUM** | Missing context fields | Handler lacks CorrelationId or IdempotencyKey |
| **S3 — LOW** | Naming convention violation | `OrderHandler` instead of `OrderProjection` |
| **S3 — LOW** | Missing rebuild support | Projection depends on non-event external state |

## Enforcement Action

- **S0**: Block merge. Fail CI. Immediate remediation.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
PROJECTION_GUARD_VIOLATION:
  layer: <runtime_projection | domain_projection>
  projection: <projection name>
  read_model: <target read model>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct behavior>
  actual: <detected behavior>
  remediation: <fix instruction>
```

---

## NEW RULES INTEGRATED — 2026-04-07

- **P-TYPE-ALIGN-01**: Projection bridges MUST match on the same event types the domain layer emits. Either (a) EventFabric maps domain events to schema types before dispatch, or (b) bridges match domain types directly. Type alignment between emit and consume MUST be verified at registration time. Silent type-mismatch drops are S1 violations.
- **P-AGNOSTIC-01**: Runtime projection bridges MUST be event-type-agnostic — no "using" of concrete domain/schema types inside src/runtime/projection/bridges/**.

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: PROJ-READ-ONLY-01 — PROJECTIONS ARE READ ONLY
Projections MUST NOT modify domain state.

ENFORCEMENT:
- No write-back to domain
- No aggregate mutation

---

### RULE: PROJ-DOMAIN-ALIGN-01 — DOMAIN ALIGNED STRUCTURE
Projections MUST follow domain-aligned folder structure.
