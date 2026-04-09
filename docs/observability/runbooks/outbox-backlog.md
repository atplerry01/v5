# Runbook — Outbox backlog

**STATUS:** TEMPLATE (resolution playbook is placeholder)

## Symptom

One or more of:

- `outbox.depth` (gauge, `Whyce.Outbox`) growing without returning to
  baseline.
- `outbox.oldest_pending_age_seconds` rising.
- `outbox.deadletter_depth` growing.
- Downstream consumers reporting missing or delayed events.

## Linked SLO(s)

- [R-1 — Outbox backlog drain time](../slo/recovery-slos.md#r-1--outbox-backlog-drain-time)
- [R-2 — Oldest pending row age](../slo/recovery-slos.md#r-2--oldest-pending-row-age)
- [R-3 — Dead-letter depth bounded](../slo/recovery-slos.md#r-3--dead-letter-depth-bounded)
- [F-1 — Outbox publish failure rate](../slo/failure-rate-slos.md#f-1--outbox-publish-failure-rate)
- [F-2 — Outbox dead-letter promotion rate](../slo/failure-rate-slos.md#f-2--outbox-dead-letter-promotion-rate)

## Severity guide (TEMPLATE)

| Severity | Condition | Routing |
|----------|-----------|---------|
| S0 | `TBD` (e.g. `outbox.depth` exceeds X over Y minutes) | `TBD` |
| S1 | `TBD` | `TBD` |
| S2 | `TBD` | `TBD` |
| S3 | `TBD` | `TBD` |

## Triage (real and actionable today)

1. **Confirm the symptom shape.** Query the three gauges together —
   `outbox.depth`, `outbox.oldest_pending_age_seconds`,
   `outbox.deadletter_depth`. The pattern tells you which sub-runbook
   path to take:
   - Depth growing + oldest age stable    → publisher is keeping up but
                                             input rate is up
                                             (capacity, not failure).
   - Depth growing + oldest age rising    → publisher is NOT keeping
                                             up — go to step 2.
   - Depth flat + DLQ depth rising        → individual rows are
                                             failing all retries — go
                                             to step 4.
2. **Check publisher liveness.** The publisher reports liveness via
   `IWorkerLivenessRegistry` after every successful poll cycle. If
   the worker is not reporting, the runtime aggregator surfaces
   `worker_unhealthy` as a not-ready reason. Inspect host health.
3. **Inspect publish failure rate.** If `outbox.failed` is climbing,
   the publisher is reaching the broker but `ProduceAsync` is
   throwing. Cross-reference with Kafka broker health. The publisher
   logs the failure reason via `last_error` on each row — query the
   `outbox` table for the most recent rows in `status='failed'` and
   read `last_error`.
   ```sql
   SELECT id, retry_count, last_error, next_retry_at
   FROM outbox
   WHERE status = 'failed'
   ORDER BY next_retry_at DESC
   LIMIT 20;
   ```
4. **Inspect DLQ promotions.** If rows are reaching `status='deadletter'`
   they have exhausted `OutboxOptions.MaxRetry` attempts. Query:
   ```sql
   SELECT id, retry_count, last_error, created_at
   FROM outbox
   WHERE status = 'deadletter'
   ORDER BY created_at DESC
   LIMIT 20;
   ```
   The `last_error` column tells you what failed. Common patterns:
   - Same Kafka broker error on every row → broker outage; cross-link
     to broker runbook.
   - Different errors per row              → likely a payload-shape
                                             contract violation; refer
                                             to schema-registry alerts.

## Mitigation (TEMPLATE)

- `TBD` — define short-term steps once thresholds exist (scale
  publisher, drain DLQ, manually re-enqueue, etc.).

## Resolution (TEMPLATE)

- `TBD` — root-cause analysis steps and the irreversible-fix
  checklist.

## Post-incident (TEMPLATE)

- `TBD` — verification steps:
  - Confirm `outbox.depth` returned to baseline.
  - Confirm `outbox.deadletter_depth` is not growing.
  - Confirm `outbox.failed` rate is back at baseline.

## References

- Code: [src/platform/host/adapters/KafkaOutboxPublisher.cs](../../../src/platform/host/adapters/KafkaOutboxPublisher.cs)
- Code: [src/platform/host/adapters/OutboxDepthSampler.cs](../../../src/platform/host/adapters/OutboxDepthSampler.cs)
- Tests: [tests/integration/failure-recovery/OutboxKafkaOutageRecoveryTest.cs](../../../tests/integration/failure-recovery/OutboxKafkaOutageRecoveryTest.cs) (FR-1)
- Tests: [tests/integration/failure-recovery/PostgresFailureRecoveryTest.cs](../../../tests/integration/failure-recovery/PostgresFailureRecoveryTest.cs) (FR-2)
- Tests: [tests/integration/failure-recovery/RuntimeCrashRecoveryTest.cs](../../../tests/integration/failure-recovery/RuntimeCrashRecoveryTest.cs) (FR-3)
- Tests: [tests/integration/platform/host/adapters/OutboxMultiInstanceSafetyTest.cs](../../../tests/integration/platform/host/adapters/OutboxMultiInstanceSafetyTest.cs) (MI-2)
- SLO map: [docs/observability/slo/metric-mapping.md](../slo/metric-mapping.md)
