# §5.6 Scenario 1 — Kafka Failure During Load (EVIDENCE)

**Scenario:** Kafka broker unavailable while the runtime is publishing outbox rows.
**Date:** 2026-04-09
**Test file:** [tests/integration/failure-recovery/OutboxKafkaOutageRecoveryTest.cs](../../../../../tests/integration/failure-recovery/OutboxKafkaOutageRecoveryTest.cs)
**Stack:** real `whyce-postgres` (5432) + real `KafkaOutboxPublisher` + stub `IProducer` simulating outage.
**Tests run:**
- `Kafka_Outage_Promotes_Row_To_Deadletter_After_Retry_Budget_Exhausted` — **PASSED [6 s]**
- `Kafka_Recovery_Mid_Retry_Allows_Row_To_Reach_Published_State` — **PASSED [7 s]**

## Refusal seam exercised
- §5.2.1 PC-3 outbox saturation seam (declared)
- §5.2.2 KC-3 DLQ promotion + `outbox.deadletter_depth` gauge

## Behavior verified
- **Outage path:** under sustained `IProducer` failure, the row's
  `failed_count` advances each retry, then promotes to `status='deadletter'`
  exactly once after `OutboxOptions.MaxRetry` is exhausted. No
  duplicate promotion. No row lost.
- **Recovery path:** if the broker recovers before retry budget
  exhausts, the row transitions `pending → published` exactly once;
  `published_at` is set; `failed_count` retains the in-flight history;
  no duplicate Kafka publish was observed downstream.

## Snapshots
- **Before:** `pending=N, failed=0, deadletter=0, published=0`
- **During (outage):** `pending<N, failed=N, deadletter=0, published=0`
- **After (outage):** `pending=0, failed=0, deadletter=N, published=0`
- **After (recovery):** `pending=0, failed=0, deadletter=0, published=N`

## Acceptance
| F1 no data loss | F2 no duplicate | F3 unattended recovery | F4 refusal semantics | F5 bounded recovery time | F6 reproducible |
|---|---|---|---|---|---|
| PASS | PASS | PASS | PASS — DLQ promotion via PC-3/KC-3 | PASS — bounded by `MaxRetry × pollInterval` | PASS — gated test |
