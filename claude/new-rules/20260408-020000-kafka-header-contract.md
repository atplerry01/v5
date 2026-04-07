---
CLASSIFICATION: kafka / infrastructure
SOURCE: Phase 1 gate end-to-end verification (S0b follow-up to a3987e4)
SEVERITY: S1 (HIGH) — silent data loss in projection pipeline
STATUS: workaround in place; proper fix pending
---

# HEADER-CONTRACT-01 — Outbox publisher must emit event-id + aggregate-id Kafka headers

## DESCRIPTION

`src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs` (lines 90-99)
requires every consumed Kafka message to carry two headers:

- `event-id`     — parseable as `Guid`
- `aggregate-id` — parseable as `Guid`

When either header is missing or unparseable, the worker logs
`[KAFKA] Missing event-id/aggregate-id headers ... skipping` and **commits the
offset anyway**, advancing past the message without invoking any projection
handler. This is silent data loss: Kafka offsets advance, the outbox row stays
`status='published'`, but the projection table never reflects the event.

Before commit S0b, `KafkaOutboxPublisher` only emitted `event-type` and
`correlation-id` headers. Every message produced by the publisher hit the
"missing headers" branch on the consumer side. Phase 1 gate end-to-end runs
showed:

- Outbox: 56 rows `published`
- Kafka: 26 todo events delivered
- Consumer lag: 0
- Projection: 0 new rows

The pipeline appeared healthy on every individual layer's health metric while
losing 100% of new events.

## CURRENT WORKAROUND (commit S0b)

`KafkaOutboxPublisher` now derives both headers at publish time:

- `event-id`     ← `outbox.id` (the outbox row primary key, a UUID)
- `aggregate-id` ← parsed from `payload->'AggregateId'->>'Value'` (or
                   `payload->>'AggregateId'` if scalar) via `JsonDocument.Parse`

Verified end-to-end after the fix: POST → outbox → Kafka → consumer →
projection → API GET returns the materialized row in ~8 seconds.

## WHY THE WORKAROUND IS NOT THE RIGHT FIX

1. **Identity collapse**: `outbox.id` is the outbox row's primary key, not the
   domain event's identity. They are conceptually distinct. Downstream
   consumers that key on `event-id` for idempotency / dedup will treat outbox
   row identity as event identity, which is wrong if the same domain event
   ever gets enqueued twice (e.g. retry scenarios).

2. **Schema-shape coupling**: The publisher now hard-codes a JSON parse path
   (`AggregateId.Value` nested object). Any event whose payload uses a
   different shape (`AggregateId` as scalar string, or under a different key
   like `AggregateRootId`) will fall through to `Guid.Empty`, which the
   consumer will accept as "valid" and project under a zero-id row. The
   tolerance for the two shapes in `TryExtractAggregateId` is itself a smell
   — the outbox should not be guessing.

3. **Performance**: every batch publish now does a `JsonDocument.Parse` per
   row. Negligible at current volume but proportional to event size.

## PROPOSED PROPER FIX (separate PR)

### Schema change

Add to `infrastructure/data/postgres/outbox/migrations/004_outbox_event_identity.sql`:

```sql
ALTER TABLE outbox ADD COLUMN event_id     uuid NOT NULL;
ALTER TABLE outbox ADD COLUMN aggregate_id uuid NOT NULL;

CREATE INDEX idx_outbox_aggregate_id ON outbox (aggregate_id);
```

### Enqueue site change

Wherever the runtime persists outbox rows (currently in
`RuntimeCommandDispatcher` or its outbox adapter), populate both columns from
the canonical event envelope:

- `event_id`     ← `EventEnvelope.EventId`
- `aggregate_id` ← `EventEnvelope.AggregateId`

### Publisher change

Replace the `TryExtractAggregateId` JSON-parse path with a direct column read.
Delete the helper method.

### Consumer hardening

`GenericKafkaProjectionConsumerWorker` should NOT silently commit offsets when
required headers are missing. Either:

- Route to deadletter topic (preferred — aligns with K-DLQ-PUBLISH-01), OR
- Halt the worker with a structured failure (forces operator attention)

Silent skip + offset commit is the worst possible failure mode and should be
removed entirely.

## PROPOSED RULE

**K-HEADER-CONTRACT-01**: All Kafka messages produced by the WBSM event fabric
MUST carry the following headers, populated from the canonical
`EventEnvelope`:

| Header           | Source                       | Type   |
|------------------|------------------------------|--------|
| `event-id`       | `EventEnvelope.EventId`      | UUID   |
| `aggregate-id`   | `EventEnvelope.AggregateId`  | UUID   |
| `event-type`     | `EventEnvelope.EventType`    | string |
| `correlation-id` | `EventEnvelope.CorrelationId`| UUID   |

Producers MUST NOT derive these from payload bodies. Consumers MUST NOT
silently skip messages with missing headers — missing headers indicate a
producer-side contract violation and must be surfaced (deadletter or halt),
never absorbed.

## IMPACTED FILES

- `src/platform/host/adapters/KafkaOutboxPublisher.cs` (workaround in place)
- `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs` (silent-skip behavior)
- `infrastructure/data/postgres/outbox/migrations/` (schema needs new migration 004)
- Outbox enqueue site (likely `src/runtime/dispatcher/RuntimeCommandDispatcher.cs` or its adapter)

## SUGGESTED ENFORCEMENT POINT

Promote into `claude/audits/kafka.audit.md` as `CHECK-K-HEADER-CONTRACT-01`
under KDIM-04 (Outbox Pattern Enforcement). Severity: HIGH.

Promote into `claude/guards/kafka.guard.md` as a sub-clause under rule 6
(OUTBOX PATTERN MANDATORY): "Outbox-to-Kafka producers MUST emit
event-id/aggregate-id/event-type/correlation-id headers populated from
discrete outbox columns, not derived from payload bodies."
