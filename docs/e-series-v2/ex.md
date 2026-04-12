# WBSM v3.5 -- E-SERIES v2 (CANONICAL LIST + IMPLEMENTATION GUIDE)

## CLASSIFICATION: system / development / execution-framework

## VERSION: 2.0 (Post-Kanban Validation)

## SOURCE: Lessons from end-to-end Kanban domain implementation (E1-E8 + live validation)

---

## DOMAIN FOUNDATION

**E1 -- Domain Model**

* Aggregate (single root per domain, sealed class, private ctor, static Create factory)
* Entities (sealed class, internal methods, Entity base)
* Value Objects (readonly record struct for IDs, enums for states)
* Domain Events (sealed record, past tense, extends DomainEvent)
* Errors (static class with const string messages)
* Guard-based invariant enforcement (Guard.Against)

---

**E2 -- Contracts & Event Definitions**

* Command contracts (sealed record, Id as FIRST positional parameter = aggregate ID)
* Query contracts (sealed record)
* Intent contracts (sealed record, carries UserId for identity)
* Event schemas (sealed record, raw primitive types -- Guid not value objects, int not position structs)
* DTOs (sealed record with init properties)
* Versioning structure (EventVersion)

---

**E3 -- Persistence & Infrastructure Binding**

* Event store (Postgres, runtime-owned)
* Outbox pattern (Postgres, deterministic IDs)
* Aggregate loading via reflection (private ctor + LoadFromHistory)
* NO repository abstraction in domain -- runtime handles all persistence

---

**E4 -- Determinism & Integrity**

* Deterministic ID generation (IIdGenerator.Generate with stable seed strings)
* IClock enforcement (never DateTime.UtcNow)
* Idempotency rules (CommandId-based, not AggregateId-based)
* Replay safety guarantees (sentinel values: ExecutionHash="replay", PolicyHash="replay")

---

## EXECUTION & EXPOSURE

**E5 -- Engine Implementation (T2E)**

* One handler per command (sealed class implements IEngine)
* Stateless execution -- load aggregate, call domain method, emit events
* NO persistence, NO publishing, NO infrastructure
* Create commands: construct aggregate directly
* Mutate commands: LoadAggregateAsync then call domain method

---

**E6 -- Runtime Integration**

* Bootstrap module (IDomainBootstrapModule) -- single wiring point
* Engine registration via IEngineRegistry
* Schema registration via DomainSchemaCatalog + ISchemaModule
* OPA policy creation (rego file per domain)
* Kafka topic creation (events + deadletter + retry)
* Projection DB schema creation (migration SQL)
* API controller with request DTOs (never bind directly to command records)

---

**E7 -- Event Fabric & Kafka**

* Topic definition JSON (4 topics: commands, events, retry, deadletter)
* Schema module with payload mappers (domain event -> schema record)
* Header enforcement (event-id, aggregate-id, event-type, correlation-id)
* Outbox aggregate ID extraction via reflection on AggregateId property
* FlexibleGuidConverter + FlexibleIntConverter for value object deserialization
* Consumer hosted service registration (AddSingleton<IHostedService>, NOT AddHostedService)

---

**E8 -- Projections / Read Models**

* Read model records with JsonPropertyName attributes
* IEnvelopeProjectionHandler implementation
* Load -> Apply -> Upsert pattern (JSONB state column)
* Idempotency via last_event_id (WHERE IS DISTINCT FROM)
* Projection DB migration (schema + table + indexes)
* Registration in ProjectionRegistry for all event types

---

## GOVERNANCE & ENFORCEMENT

**E9 -- Policy Integration (WHYCEPOLICY)**

* OPA rego file per domain with action rules
* Action mapping: domain.{verb} derived from command type name
* Policy middleware evaluates before execution
* PolicyDecisionAllowed write-once field on CommandContext

---

**E10 -- Guards**

* All 32+ guard files loaded fresh per execution
* Domain purity, engine purity, determinism, dependency graph
* Classification suffix guard (-system in domain folders only)

---

**E11 -- Chain Anchoring (WHYCECHAIN)**

* Persist -> Chain -> Outbox (locked order)
* Audit emissions use separate aggregate stream (ExpectedVersion = -1)
* Chain blocks linked via previous_block_hash

---

**E12 -- Full Enforcement Pipeline**

```
Tracing -> Metrics -> ContextGuard -> Validation -> Policy ->
AuthorizationGuard -> Idempotency -> ExecutionGuard ->
Engine -> EventFabric(Persist -> Chain -> Outbox)
```

---

## OBSERVABILITY & INTELLIGENCE PREP

**E13 -- Observability**

* CorrelationId end-to-end propagation
* Prometheus metrics (pool acquisitions, consumer consumed, projection lag)
* Structured error types (DomainInvariantViolation, ConcurrencyConflict, PolicyDenied)

---

**E14 -- Governance Assist**

* Outbox depth monitoring with high-water-mark admission control
* Worker liveness registry
* DLQ routing with structured reason headers

---

**E15 -- Optimization**

* Connection pool instrumentation
* Outbox batched publishing with FOR UPDATE SKIP LOCKED
* Exponential backoff retry (2^n seconds, 300s cap)

---

**E16 -- Identity Integration (WHYCEID)**

* ActorId/TenantId on CommandContext
* Role enforcement via OPA policy rules
* Identity resolution before policy evaluation

---

## SYSTEM INTEGRATION

**E17 -- Economic Integration**
**E18 -- Structural Integration**
**E19 -- SPV Integration**
**E20 -- CWG / Workforce Integration**

---

## ACTIVATION

**EX -- Full System Activation**

* End-to-end domain activation with live infrastructure verification
* All stores verified (EventStore, WhyceChain, Outbox, Kafka, Projection)
* API GET returns correct hierarchical state
* Idempotency, concurrency, and load tests pass

---

## CANONICAL RULE

```
E1 - E4   = Define truth (domain, contracts, determinism)
E5 - E8   = Make executable (engine, runtime, fabric, projection)
E9 - E12  = Enforce governance (policy, guards, chain, pipeline)
E13 - E16 = Observe & prepare intelligence
E17 - E20 = Integrate into full system
EX        = Activate and prove
```
