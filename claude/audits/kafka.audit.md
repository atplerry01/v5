# KAFKA AUDIT — WBSM v3

```
AUDIT ID:       KAFKA-AUDIT-v2
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit all Kafka usage across the system to ensure strict compliance with WBSM v3 event fabric rules. Kafka access must be runtime-only, follow the dual-topic pattern (internal + public), use governed topic naming, enforce the outbox pattern, and maintain strict producer/consumer placement rules.

This audit MUST detect:

* Kafka access outside runtime/infrastructure
* Missing dual-topic pattern
* Non-governed topic names
* Missing outbox pattern (direct produce)
* Direct producer/consumer in domain or engine layers
* Consumer group naming violations

---

## SCOPE

```
src/runtime/           -> Kafka dispatch and consumer orchestration
infrastructure/        -> Kafka adapter implementations
src/engines/           -> MUST NOT contain Kafka references
src/domain/            -> MUST NOT contain Kafka references
src/systems/           -> MUST NOT contain Kafka references
src/platform/          -> MUST NOT contain Kafka references
```

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Kafka in domain/engine, missing outbox, direct produce from domain | Blocks deployment |
| HIGH | Missing dual-topic, non-governed topic name, missing consumer group | Must fix before merge |
| MEDIUM | Consumer group naming deviation, missing DLQ topic | Fix within sprint |
| LOW | Missing topic documentation, non-standard partition count | Fix at convenience |

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

### KDIM-01: Runtime-Only Kafka Access

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | No `Confluent.Kafka` or Kafka client references in `src/domain/` | CRITICAL |
| CHECK-01.2 | No `Confluent.Kafka` or Kafka client references in `src/engines/` | CRITICAL |
| CHECK-01.3 | No `Confluent.Kafka` or Kafka client references in `src/systems/` | CRITICAL |
| CHECK-01.4 | No `Confluent.Kafka` or Kafka client references in `src/platform/` | HIGH |
| CHECK-01.5 | Kafka producer/consumer implementations exist only in `infrastructure/` | HIGH |
| CHECK-01.6 | Kafka orchestration (dispatch, subscribe) exists only in `src/runtime/` | HIGH |

### KDIM-02: Dual-Topic Pattern

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | Each BC that publishes events has an internal topic for domain events | HIGH |
| CHECK-02.2 | Each BC that publishes cross-BC events has a public topic for integration events | HIGH |
| CHECK-02.3 | Domain events are NEVER published to public topics | CRITICAL |
| CHECK-02.4 | Integration events are transformations of domain events (not raw domain events) | HIGH |
| CHECK-02.5 | Internal topics consumed only within the owning BC | HIGH |

### KDIM-03: Governed Topic Naming

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | Topic names follow `{context}.{domain}.{type}` pattern | HIGH |
| CHECK-03.2 | Internal topics use `.internal` suffix or prefix | HIGH |
| CHECK-03.3 | Public topics use `.public` or `.integration` suffix | HIGH |
| CHECK-03.4 | No ad-hoc or unnamed topics | MEDIUM |
| CHECK-03.5 | Topic names use lowercase with dot separators | MEDIUM |

### KDIM-04: Outbox Pattern Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Events written to outbox table in same transaction as aggregate state | CRITICAL |
| CHECK-04.2 | No direct `ProduceAsync` or `Produce` calls from domain or engine code | CRITICAL |
| CHECK-04.3 | Outbox relay worker exists and processes outbox entries | HIGH |
| CHECK-04.4 | Outbox table schema includes: `id`, `event_type`, `payload`, `created_at`, `processed_at`, `retry_count` | HIGH |
| CHECK-04.5 | Outbox relay uses `SKIP LOCKED` or equivalent for concurrency | HIGH |
| CHECK-04.6 | Outbox entries marked as processed after successful Kafka produce | HIGH |

### KDIM-05: Consumer Group Naming

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | Consumer groups follow `{service}.{context}.{purpose}` naming pattern | MEDIUM |
| CHECK-05.2 | Each projection has a dedicated consumer group | HIGH |
| CHECK-05.3 | Consumer group names are unique across the system | HIGH |
| CHECK-05.4 | No hardcoded consumer group names (use configuration) | MEDIUM |

### KDIM-06: Consumer Idempotency

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | Consumers implement idempotency check (e.g., `ExistsAsync` before processing) | HIGH |
| CHECK-06.2 | Consumers use `EnableIdempotence=true` for producer config | HIGH |
| CHECK-06.3 | Consumer offset commits are explicit (not auto-commit in critical paths) | MEDIUM |

### KDIM-07: Dead Letter Queue

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | DLQ topic configured for each consumer group | HIGH |
| CHECK-07.2 | Failed messages routed to DLQ after retry exhaustion | HIGH |
| CHECK-07.3 | DLQ messages include original topic, partition, offset metadata | MEDIUM |
| CHECK-07.4 | DLQ monitoring/alerting configured | LOW |

### KDIM-08: Kafka Configuration Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | `Acks=All` configured for producers in critical paths | HIGH |
| CHECK-08.2 | Partition key strategy defined (aggregate ID as partition key) | HIGH |
| CHECK-08.3 | Retention policies configured per topic | MEDIUM |
| CHECK-08.4 | Schema registry used for event serialization (if applicable) | LOW |

### KDIM-09: Projection Event Consumption

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | Projections subscribe only to EVENT topics | CRITICAL |
| CHECK-09.2 | No projection subscribes to command topics | CRITICAL |
| CHECK-09.3 | Projection consumers exist only in runtime or projection layer | HIGH |

### KDIM-10: Event-First Architecture (Phase 1 — EFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | All state changes published to Kafka originate from domain events | CRITICAL |
| CHECK-10.2 | No direct state mutation messages on Kafka topics — events only | CRITICAL |
| CHECK-10.3 | Events are persisted (outbox) before Kafka publication | CRITICAL |
| CHECK-10.4 | Kafka projections are driven by events only — no command topics consumed by projections | CRITICAL |

### KDIM-11: Execution Chain Integration (Phase 1 — ECIDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | Events published after persistence — no Kafka publish before outbox commit completes | CRITICAL |
| CHECK-11.2 | Topics follow canonical naming — all execution chain topics use `{classification}.{context}.{domain}.{type}` pattern | CRITICAL |
| CHECK-11.3 | Sandbox/todo events are published — execution chain sandbox and todo state transitions emit events to Kafka via outbox | CRITICAL |

---

## OUTPUT FORMAT

```yaml
audit: kafka
status: PASS | FAIL
score: {0-100}
scope: "Kafka event fabric compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: KDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "kafka"
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
