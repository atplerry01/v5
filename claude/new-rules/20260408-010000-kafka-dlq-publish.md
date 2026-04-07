---
CLASSIFICATION: kafka / infrastructure
SOURCE: targeted post-execution mini-sweep, KafkaOutboxPublisher S0 patch (Phase 1 gate unblock)
SEVERITY: S2 (MEDIUM)
STATUS: open
---

# DLQ-PUBLISH-01 — Outbox publisher must publish to deadletter topic, not only mark row

## DESCRIPTION

`src/platform/host/adapters/KafkaOutboxPublisher.cs` (after the Phase 1 S0 patch)
promotes outbox rows to `status='deadletter'` once `retry_count >= maxRetryCount`,
but it does **not** produce the failed message to the corresponding
`{classification}.{context}.{domain}.deadletter` Kafka topic.

This satisfies the *pipeline-halt* failure mode (no batch crash, no host stop, no
silent loss at the row level) but leaves a compliance gap against
`claude/audits/kafka.audit.md` CHECK-07.2:

> CHECK-07.2 — Failed messages routed to DLQ after retry exhaustion — HIGH

The current behavior is "row marked deadletter in Postgres"; the audit expects
"message produced to .deadletter Kafka topic so downstream DLQ consumers /
alerting / replay tooling can act on it."

## PROPOSED RULE

**K-DLQ-PUBLISH-01**: When an outbox row exhausts its retry budget, the publisher
MUST attempt to produce the message body to the canonical deadletter topic
(`{classification}.{context}.{domain}.deadletter`) **before** updating the row
to `status='deadletter'`. The deadletter produce attempt is best-effort; if it
also fails, the row stays as `status='failed'` with the latest `last_error` and
will be retried on the next poll cycle (i.e. the deadletter publish itself is
inside the retry budget, not separate). Headers MUST include the original topic,
the original `event-type`, the original `correlation-id`, and a
`dlq-reason` header carrying the last error string.

## IMPACTED FILE

- `src/platform/host/adapters/KafkaOutboxPublisher.cs`

## SUGGESTED ENFORCEMENT POINT

Promote into `claude/audits/kafka.audit.md` as `CHECK-K-DLQ-PUBLISH-01`
(MEDIUM) and into `claude/guards/kafka.guard.md` under the existing rule 8
(Dead Letter Topic Required) as a sub-clause clarifying that "configured" means
"actively published to," not "exists in create-topics.sh."

## NOT FIXED IN THIS PATCH BECAUSE

The Phase 1 gate prompt's acceptance criteria are:
- ✔ Publisher does not crash on bad row
- ✔ Failed rows marked correctly
- ✔ Valid rows still publish
- ✔ Kafka contains events
- ✔ Projection updates correctly

DLQ-side publication is a follow-up correctness/observability improvement, not a
gate-blocking requirement. Implementing it correctly requires a separate produce
path with its own error handling and is non-trivial enough to warrant its own PR.
