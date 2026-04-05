# BEHAVIORAL AUDIT — WBSM v3

```
AUDIT ID:       BEHAV-AUDIT-v2
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit runtime behavior across all layers to ensure strict compliance with WBSM v3 execution model. This audit validates command flow, event immutability, cross-BC isolation, engine invocation governance, projection read-only enforcement, outbox pattern usage, and Kafka dual-topic compliance.

This audit MUST detect:

* Command flow violations (commands bypassing runtime)
* Event mutability
* Synchronous cross-BC calls
* Engine-to-engine coupling
* Domain mutation from systems layer
* Projection write operations
* Missing outbox pattern
* Kafka dual-topic non-compliance

---

## SCOPE

```
src/engines/         -> engine execution behavior
src/runtime/         -> control plane behavior
src/systems/         -> composition behavior
infrastructure/      -> adapter behavior
```

Excluded: `src/domain/` (pure model, no behavior), `src/shared/`, `bin/`, `obj/`, `.git/`

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Engine persistence attempt, engine publishing event externally, engine calling infra, runtime bypass, command flow bypass, event mutation, cross-engine coupling | Blocks deployment |
| HIGH | Missing outbox, sync cross-BC call, projection write | Must fix before merge |
| MEDIUM | Non-standard event naming, missing idempotency guard | Fix within sprint |
| LOW | Missing observability hook, non-standard consumer group | Fix at convenience |

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

### BDIM-01: Command Flow Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | All commands route through runtime control plane (IRuntimeControlPlane) | CRITICAL |
| CHECK-01.2 | No direct engine invocation from platform layer | CRITICAL |
| CHECK-01.3 | No direct engine invocation from systems layer | CRITICAL |
| CHECK-01.4 | Command dispatch follows platform > systems > runtime > engine flow | CRITICAL |

### BDIM-02: Event Immutability

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | Domain events have no public setters | CRITICAL |
| CHECK-02.2 | Domain events are sealed or record types | HIGH |
| CHECK-02.3 | Event properties are init-only or readonly | CRITICAL |
| CHECK-02.4 | No event modification after creation (no mutating methods) | CRITICAL |

### BDIM-03: Cross-BC Isolation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | No synchronous cross-BC method calls | CRITICAL |
| CHECK-03.2 | Cross-BC communication is event-driven only | HIGH |
| CHECK-03.3 | No shared mutable state between BCs | CRITICAL |
| CHECK-03.4 | Integration events used for cross-BC messaging | HIGH |

### BDIM-04: Engine-to-Engine Isolation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | No engine imports another engine's namespace | CRITICAL |
| CHECK-04.2 | No direct method calls between engine tiers | CRITICAL |
| CHECK-04.3 | Engine communication goes through runtime only | CRITICAL |

### BDIM-05: Domain Mutation from Systems

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | Systems layer has no write access to domain aggregates | CRITICAL |
| CHECK-05.2 | Systems layer performs composition only (no new aggregate creation) | CRITICAL |
| CHECK-05.3 | Systems layer does not call domain services directly | HIGH |

### BDIM-06: Projection Read-Only Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | Projections do not issue commands | CRITICAL |
| CHECK-06.2 | Projections consume events only | HIGH |
| CHECK-06.3 | Projection handlers have no write operations to source aggregates | CRITICAL |
| CHECK-06.4 | Projection data is materialized views only | HIGH |

### BDIM-07: Outbox Pattern Usage

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | Events published via outbox table, not direct Kafka produce | CRITICAL |
| CHECK-07.2 | Outbox entries written in same transaction as aggregate state | CRITICAL |
| CHECK-07.3 | Outbox relay processes entries with at-least-once delivery | HIGH |
| CHECK-07.4 | Outbox table has `processed_at` and `retry_count` columns | HIGH |

### BDIM-08: Kafka Dual-Topic Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | Internal topic used for domain events within BC | HIGH |
| CHECK-08.2 | Public topic used for integration events across BCs | HIGH |
| CHECK-08.3 | No domain events published to public topic | CRITICAL |
| CHECK-08.4 | Topic naming follows `{context}.{domain}.{type}` pattern | MEDIUM |

### BDIM-09: Idempotency

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | Command handlers are idempotent (duplicate detection) | HIGH |
| CHECK-09.2 | Event consumers implement idempotency checks | HIGH |
| CHECK-09.3 | Outbox relay uses `SKIP LOCKED` or equivalent concurrency control | HIGH |

### BDIM-10: Determinism

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | No `DateTime.Now` or `DateTime.UtcNow` in engines (use injected clock) | HIGH |
| CHECK-10.2 | No `Random` usage in engines without seed injection | MEDIUM |
| CHECK-10.3 | No `Guid.NewGuid()` in domain layer (use ID generation service) | MEDIUM |

### BDIM-11: CQRS Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | Command handlers return void or Result (no query data) | HIGH |
| CHECK-11.2 | Query handlers do not modify state | CRITICAL |
| CHECK-11.3 | Read models separated from write models | HIGH |

### BDIM-12: Workflow Orchestration

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-12.1 | Workflows defined in WSS (midstream) only | HIGH |
| CHECK-12.2 | Workflow execution in T1M engine only | CRITICAL |
| CHECK-12.3 | Workflow state transitions are event-sourced | HIGH |

### BDIM-13: Retry and Timeout

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-13.1 | Retry policies defined at runtime level | HIGH |
| CHECK-13.2 | Timeout values are configurable, not hardcoded | MEDIUM |
| CHECK-13.3 | Dead letter queue (DLQ) configured for failed messages | HIGH |

### BDIM-14: Event Sourcing Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-14.1 | Aggregate state rebuilt from event stream | HIGH |
| CHECK-14.2 | No direct state mutation outside Apply methods | CRITICAL |
| CHECK-14.3 | Event store append-only (no updates or deletes) | CRITICAL |
| CHECK-14.4 | Snapshot strategy defined for high-event aggregates | MEDIUM |

### BDIM-15: Adapter Behavioral Purity

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-15.1 | Infrastructure adapters contain no branching business logic | HIGH |
| CHECK-15.2 | Adapters perform translation only (no domain decisions) | CRITICAL |
| CHECK-15.3 | Adapters do not transform domain events | HIGH |

### BDIM-16: S0 Violation Classification (CRITICAL — ZERO TOLERANCE)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-16.1 | Engine persistence attempt — engine writes to DB, file, or any durable store | CRITICAL |
| CHECK-16.2 | Engine publishing event externally — engine calls `IEventPublisher`, `eventBus.Publish()`, Kafka produce | CRITICAL |
| CHECK-16.3 | Engine calling infra — engine calls Redis, Kafka, HTTP, SQL, file I/O, or any infra adapter | CRITICAL |
| CHECK-16.4 | Runtime bypass — any path invoking engine without RuntimeControlPlane middleware pipeline | CRITICAL |

### BDIM-17: Policy Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-16.1 | T0U evaluates policy before T1M/T2E execution | CRITICAL |
| CHECK-16.2 | Policy results are immutable and auditable | HIGH |
| CHECK-16.3 | No policy bypass paths exist | CRITICAL |

### BDIM-18: Observability

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-17.1 | Correlation IDs propagated across all layers | HIGH |
| CHECK-17.2 | Structured logging in runtime middleware | MEDIUM |
| CHECK-17.3 | Metrics collection for command/event throughput | LOW |

### BDIM-19: Failure Handling

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-18.1 | Domain errors are typed (not generic exceptions) | HIGH |
| CHECK-18.2 | Engine failures escalate to runtime | HIGH |
| CHECK-18.3 | No swallowed exceptions in event handlers | CRITICAL |

### BDIM-20: Runtime Control Plane Assertion

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-19.1 | Runtime is the SOLE entry point for engine invocation | CRITICAL |
| CHECK-19.2 | Runtime manages all cross-cutting concerns (auth, logging, tracing) | HIGH |
| CHECK-19.3 | Runtime configuration is environment-driven | MEDIUM |

### BDIM-21: Simulation Mode

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-20.1 | Simulation mode flag available at runtime level | MEDIUM |
| CHECK-20.2 | Simulation does not produce real side effects | HIGH |
| CHECK-20.3 | Simulation results are distinguishable from production | MEDIUM |

### BDIM-22: Projection Behavioral Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-22.1 | Projections consume events only (no command consumption) | CRITICAL |
| CHECK-22.2 | Projections do not dispatch commands | CRITICAL |
| CHECK-22.3 | Projections do not call domain aggregates | CRITICAL |
| CHECK-22.4 | Projections do not call engines | CRITICAL |
| CHECK-22.5 | Projection writes limited to read models only | HIGH |

### BDIM-23: E2E Execution Integrity (Phase 1 — E2EDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-23.1 | Platform → Systems → Runtime → Engines → Domain path exists and is enforced at runtime | CRITICAL |
| CHECK-23.2 | No direct Platform → Runtime or Platform → Engine calls observed in execution behavior | CRITICAL |
| CHECK-23.3 | Systems dispatch to runtime only via approved contracts at runtime | CRITICAL |
| CHECK-23.4 | Runtime is the ONLY execution entry point — no alternative invocation paths | CRITICAL |
| CHECK-23.5 | End-to-end execution produces domain events, persistence, and projections | CRITICAL |

### BDIM-24: Determinism Enforcement — Phase 1 Upgrade (Phase 1 — DETDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-24.1 | No `DateTime.UtcNow` or non-deterministic time usage in domain or engines | CRITICAL |
| CHECK-24.2 | No `Guid.NewGuid()` outside deterministic helper | CRITICAL |
| CHECK-24.3 | IDs generated via `DeterministicIdHelper` only | CRITICAL |
| CHECK-24.4 | Event ordering deterministic per aggregate | HIGH |
| CHECK-24.5 | Replay produces identical results | HIGH |

### BDIM-25: Policy & Guard Enforcement — Phase 1 Upgrade (Phase 1 — PGDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-25.1 | Policy middleware exists in runtime pipeline | CRITICAL |
| CHECK-25.2 | Policy evaluation occurs before execution | CRITICAL |
| CHECK-25.3 | Guard middleware exists (pre-execution + post-execution) | CRITICAL |
| CHECK-25.4 | No execution bypasses policy layer | CRITICAL |
| CHECK-25.5 | Policy decision is traceable (DecisionHash) | HIGH |

### BDIM-26: Event-First Architecture (Phase 1 — EFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-26.1 | All state changes originate from domain events | CRITICAL |
| CHECK-26.2 | No direct state mutation outside aggregates | CRITICAL |
| CHECK-26.3 | Events are persisted before publication | CRITICAL |
| CHECK-26.4 | Projections are driven by events only | CRITICAL |

### BDIM-27: Lifecycle + Workflow Validation (Phase 1 — LWFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-27.1 | At least one lifecycle process implemented (created → active → completed) | CRITICAL |
| CHECK-27.2 | Lifecycle transitions enforce invariants | CRITICAL |
| CHECK-27.3 | At least one workflow execution path exists | CRITICAL |
| CHECK-27.4 | Workflow uses WSS (midstream) orchestration pattern | CRITICAL |
| CHECK-27.5 | Workflow execution produces observable events and projections | HIGH |

### BDIM-28: Sandbox/Todo Mandatory (Phase 1 — SBDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-28.1 | Sandbox bounded context exists and is executable end-to-end | CRITICAL |
| CHECK-28.2 | Todo bounded context exists and is executable end-to-end | CRITICAL |
| CHECK-28.3 | Both demonstrate full vertical slice (domain events → persistence → projections) | CRITICAL |

---

## OUTPUT FORMAT

```yaml
audit: behavioral
status: PASS | FAIL
score: {0-100}
scope: "Runtime behavior compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: BDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "behavioral"
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
