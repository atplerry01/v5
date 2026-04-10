# Kafka Guard

## Purpose

Enforce Kafka governance rules across the WBSM v3 architecture. Kafka must be accessed exclusively through the runtime layer using the dual-topic pattern (commands + events), with strict topic naming conventions and mandatory outbox pattern for event publishing.

## Scope

All files across `src/` and `infrastructure/` that reference Kafka, message brokers, or event streaming. Evaluated at CI, code review, and architectural audit.

## Rules

1. **KAFKA USED ONLY THROUGH RUNTIME** — All Kafka producer and consumer interactions must be encapsulated within `src/runtime/` or `infrastructure/` adapters invoked by runtime. No direct Kafka client usage in `src/domain/`, `src/engines/`, `src/systems/`, or `src/platform/`. Domain and engines must be Kafka-agnostic.

2. **DUAL-TOPIC PATTERN** — Every bounded context uses two Kafka topic types:
   - **Command topic**: Carries command messages for the BC. Pattern: `{classification}.{context}.{domain}.commands`
   - **Event topic**: Carries domain events from the BC. Pattern: `{classification}.{context}.{domain}.events`
   No mixed-purpose topics. Commands and events must never share a topic.

3. **TOPIC NAMING CONVENTION** — All Kafka topics must follow the naming pattern: `{classification}.{context}.{domain}.{type}` where type is `commands`, `events`, `deadletter`, or `retry`. Examples: `economic.capital.vault.commands`, `governance.decision.voting.events`. No ad-hoc topic names. No camelCase or PascalCase — use lowercase with dots.

4. **NO DIRECT KAFKA IN DOMAIN** — Domain layer must have zero references to Kafka types. No `IProducer<>`, `IConsumer<>`, `KafkaConfig`, `ProducerMessage`, or any Kafka SDK type in `src/domain/`. Domain events are raised by aggregates; Kafka is an infrastructure concern.

5. **NO DIRECT KAFKA IN ENGINES** — Engines must not reference Kafka SDK types. Engines receive commands through runtime (which may source them from Kafka) and return results to runtime (which may publish to Kafka). Engines are transport-agnostic.

6. **OUTBOX PATTERN MANDATORY** — All domain event publishing to Kafka must use the outbox pattern. Events are first persisted to an outbox table within the same transaction as the aggregate state change, then a background process (relay) publishes them to Kafka. No direct Kafka publish within the command handling transaction.

7. **CONSUMER GROUP NAMING** — Kafka consumer groups must follow: `{system-name}.{classification}.{context}.{domain}.{purpose}`. Examples: `wbsm.economic.capital.vault.projection`, `wbsm.governance.decision.voting.saga`. Consumer group names must be globally unique and self-documenting.

8. **DEAD LETTER TOPIC REQUIRED** — Every command and event topic must have a corresponding dead-letter topic: `{classification}.{context}.{domain}.{type}.deadletter`. Failed messages are routed to dead letter after retry exhaustion. No silent message drops.

9. **SCHEMA GOVERNANCE** — All Kafka messages must have a registered schema (Avro, Protobuf, or JSON Schema). Schema registry must validate message compatibility. No untyped or ad-hoc JSON payloads on Kafka topics. Schema evolution must be backward-compatible.

10. **NO KAFKA CONFIGURATION IN DOMAIN OR ENGINES** — Kafka connection strings, topic configurations, consumer group settings, and broker addresses must not appear in domain or engine configuration. Kafka config lives in `infrastructure/` or runtime configuration only.

11. **PARTITION KEY ALIGNMENT** — Kafka partition keys must align with aggregate IDs to ensure ordered processing per aggregate. Commands for the same aggregate must land on the same partition. Partition key = aggregate ID (or a deterministic derivative).

12. **EXACTLY-ONCE SEMANTICS** — Kafka consumers must implement idempotent processing. Combined with the outbox pattern on the producer side, the system achieves effectively exactly-once delivery. Consumers must track processed message offsets or use idempotency keys.

13. **RUNTIME OUTBOX ONLY** — All Kafka event publishing must flow through the runtime outbox pattern. No direct `producer.ProduceAsync()` or `producer.Send()` calls outside the outbox relay process. The outbox relay is the sole Kafka producer in the system. Any component publishing directly to Kafka bypasses transactional guarantees and is a critical violation.

14. **EVENT SCHEMA VERSIONING REQUIRED** — All events published to Kafka must include a schema version identifier. Schema evolution must be backward-compatible (consumers on older schema versions must still function). Schema registry must enforce compatibility checks on publish. Breaking schema changes require a new topic version.

15. **ORDER GUARANTEE BY AGGREGATE ID** — Events for the same aggregate must be delivered and processed in order. Kafka partition key must equal aggregate ID (or a deterministic derivative). Consumer processing must respect partition ordering. Out-of-order processing for the same aggregate is a critical violation that breaks event sourcing consistency.

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

1. Scan `src/domain/` for Kafka-related imports: `Confluent.Kafka`, `KafkaNet`, `IProducer`, `IConsumer`, `KafkaConfig`.
2. Scan `src/engines/` for the same Kafka-related imports.
3. Scan `src/systems/` and `src/platform/` for direct Kafka producer/consumer instantiation.
4. Verify all Kafka producer/consumer code resides in `src/runtime/` or `infrastructure/`.
5. Enumerate all topic names and validate against the naming pattern `{classification}.{context}.{domain}.{type}`.
6. Verify dual-topic pattern: for each BC, confirm both `.commands` and `.events` topics exist.
7. Verify outbox table and relay exist: scan for outbox entity and background relay service.
8. Verify dead-letter topics exist for each command and event topic.
9. Verify consumer group names follow the naming convention.
10. Scan for direct `producer.ProduceAsync()` or `producer.Send()` within command handlers (must use outbox instead).
11. Verify partition key configuration uses aggregate ID.
12. Verify schema registry references for all topic configurations.

## Pass Criteria

- Zero Kafka imports in domain or engine layers.
- All topics follow naming convention.
- Dual-topic pattern applied for all active BCs.
- Outbox pattern in use for all event publishing.
- Dead-letter topics configured for all topics.
- Consumer groups follow naming convention.
- Schema governance in place.
- Partition keys aligned with aggregate IDs.

## Fail Criteria

- Kafka import in domain or engine file.
- Direct Kafka producer/consumer in platform or systems layer.
- Topic name violating naming convention.
- Missing dual-topic (commands or events topic absent for active BC).
- Event published directly to Kafka without outbox.
- Missing dead-letter topic.
- Consumer group with ad-hoc name.
- Untyped Kafka message without schema.
- Kafka configuration in domain or engine config.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Kafka import in domain | `using Confluent.Kafka;` in aggregate file |
| **S0 — CRITICAL** | Direct publish without outbox | `producer.ProduceAsync(topic, msg)` in command handler |
| **S1 — HIGH** | Kafka import in engine | `IProducer<string, byte[]>` in T2E engine |
| **S1 — HIGH** | Topic naming violation | Topic named `VaultCommands` instead of `economic.capital.vault.commands` |
| **S1 — HIGH** | Missing dead-letter topic | No `.deadletter` topic for an active BC |
| **S2 — MEDIUM** | Direct Kafka in platform | Controller creating Kafka producer |
| **S2 — MEDIUM** | Missing dual-topic | BC has event topic but no command topic |
| **S2 — MEDIUM** | Ad-hoc consumer group name | Consumer group named `myConsumer1` |
| **S3 — LOW** | Missing schema registration | Topic without schema validation |
| **S3 — LOW** | Partition key not aggregate ID | Random or hash-based partition key |

## Enforcement Action

- **S0**: Block merge. Fail CI. Immediate remediation.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for tech debt.

All violations produce a structured report:
```
KAFKA_GUARD_VIOLATION:
  file: <path>
  topic: <topic name if applicable>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct Kafka usage>
  actual: <detected violation>
  remediation: <fix instruction>
```

---

## NEW RULES INTEGRATED — 2026-04-07

- **K-TOPIC-01**: Outbox publishers MUST route events to domain-aligned Kafka topics derived from event metadata: whyce.{classification}.{context}.{domain}.events. Hardcoded default topics (e.g. whyce.events) are FORBIDDEN.
- **K-TOPIC-02**: Every bounded context that emits events MUST declare its topics in infrastructure/event-fabric/kafka/create-topics.sh. Missing topic declarations fail bootstrap verification.

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: K-TOPIC-CANONICAL-01 — NAMING STANDARD
Kafka topics MUST follow canonical format:

whyce.{cluster}.{context}.{event}

ENFORCEMENT:
- 4–5 segments ONLY
- lowercase ONLY
- event MUST be past tense

---

### RULE: K-DETERMINISTIC-01 — PARTITION RESOLUTION
Kafka partition MUST be deterministic.

ENFORCEMENT:
- PartitionResolver must use deterministic hash (FNV-1a or equivalent)

## NEW RULES INTEGRATED — 2026-04-08 (Phase 1 gate blockers)

- **K-TOPIC-COVERAGE-01** (S0): Every event type ever written to `outbox.topic` MUST have a
  corresponding topic provisioned in `infrastructure/event-fabric/kafka/create-topics.sh`. A
  startup-time guard MUST diff the set of distinct `outbox.topic` values against the broker topic
  list and fail fast on mismatch. DRIFT-1.
- **K-OUTBOX-ISOLATION-01** (S0): Outbox publishers MUST catch produce exceptions per row, mark the
  row `status='failed'` with an error message column (or move it to a dead-letter table), and
  continue. A single bad row MUST NOT halt the publish loop or crash the host. DRIFT-2.
- **K-TOPIC-DOC-CONSISTENCY-01** (S3): Any topic name written in `claude/` docs / prompts MUST match
  an existing topic in `create-topics.sh`. DRIFT-6.

## NEW RULES INTEGRATED — 2026-04-08 (DLQ publish)

- **K-DLQ-PUBLISH-01** (S2): Sub-clause of rule 8 (Dead Letter Topic Required). When an outbox row
  exhausts its retry budget, the publisher MUST attempt to produce the message body to the canonical
  deadletter topic `{classification}.{context}.{domain}.deadletter` BEFORE updating the row to
  `status='deadletter'`. Headers MUST include original topic, `event-type`, `correlation-id`, and
  `dlq-reason` (last error). "Configured" in rule 8 means "actively published to," not merely "exists
  in create-topics.sh." Source:
  `claude/new-rules/_archives/20260408-010000-kafka-dlq-publish.md`.

## NEW RULES INTEGRATED — 2026-04-08 (Kafka header contract)

- **K-HEADER-CONTRACT-01** (S1): Sub-clause of rule 6 (OUTBOX PATTERN MANDATORY). All Kafka messages
  produced by the WBSM event fabric MUST carry the following headers populated from the canonical
  `EventEnvelope` discrete columns (NOT derived from payload bodies):
  `event-id` (UUID, from `EventEnvelope.EventId`), `aggregate-id` (UUID, from
  `EventEnvelope.AggregateId`), `event-type` (string), `correlation-id` (UUID). Consumers MUST NOT
  silently skip messages with missing headers — missing headers indicate a producer-side contract
  violation and must be surfaced (deadletter or halt), never absorbed. Source:
  `claude/new-rules/_archives/20260408-020000-kafka-header-contract.md`.

- Phase 1 gate source: `claude/new-rules/_archives/20260408-000000-phase1-gate-blockers.md`.

## NEW RULES INTEGRATED — 2026-04-10 (promoted from new-rules backlog)

- **K-DLQ-001 — DLQ route precedes source-offset commit** (S0, data-loss risk): On the consumer failure path, a poisoned message MUST be successfully produced to its DLQ topic (and its DLQ produce acknowledged) BEFORE the source partition offset is committed. Committing the source offset prior to a confirmed DLQ write is a guard violation: a process death between the two acts permanently loses the message. The consumer wrapper MUST express this as: `await ProduceToDlqAsync(msg); await CommitSourceOffsetAsync(msg);` — never the reverse, never in parallel without an awaited DLQ ack. Static check: grep consumer wrappers for `Commit*` calls and assert each is dominated by an awaited DLQ produce on the failure branch. Source: `_archives/20260408-142631-validation.md` Finding 2.
