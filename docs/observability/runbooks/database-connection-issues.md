# Runbook — Database connection issues

**STATUS:** TEMPLATE (resolution playbook is placeholder)

## Symptom

One or more of:

- `postgres.pool.acquisition_failures` rising (`Whyce.Postgres`),
  tagged by `pool` and `reason`.
- `event_store.append.advisory_lock_wait_ms` p99 rising.
- Health aggregator surfacing one of the canonical reasons:
  - `postgres_pool_exhausted`
  - `postgres_acquisition_failures`
  - `postgres_invalid_pool_config`
- API edge intermittently returning 503 from event-store / outbox /
  idempotency / sequence store paths.

## Linked SLO(s)

- [F-4 — Postgres pool acquisition failure rate](../slo/failure-rate-slos.md#f-4--postgres-pool-acquisition-failure-rate)
- [R-5 — Postgres pool failure decay](../slo/recovery-slos.md#r-5--postgres-pool-failure-decay)
- [L-4 — Event store append hold time](../slo/latency-slos.md#l-4--event-store-append-hold-time)
- [L-5 — Event store advisory-lock wait](../slo/latency-slos.md#l-5--event-store-advisory-lock-wait)

## Severity guide (TEMPLATE)

| Severity | Condition | Routing |
|----------|-----------|---------|
| S0 | `TBD` (e.g. failure rate ≥ X% over Y minutes) | `TBD` |
| S1 | `TBD` | `TBD` |
| S2 | `TBD` | `TBD` |
| S3 | `TBD` | `TBD` |

## Triage (real and actionable today)

1. **Identify which pool is affected.** `postgres.pool.acquisition_failures`
   is tagged by `pool`. Today the only declared pool is `event-store`
   (per `EventStoreDataSource.PoolName`). A failure on this pool
   blocks the entire event store + outbox enqueue + idempotency +
   sequence store path.
2. **Classify the failure reason.** The `reason` tag captures the
   exception type. Common patterns:
   - `Npgsql.NpgsqlException`             → broker-side: db down,
                                             network drop, auth.
   - `System.TimeoutException`            → pool exhausted; raise
                                             pool size or shed load.
   - `System.InvalidOperationException`   → config drift; refer to
                                             `postgres_invalid_pool_config`.
3. **Confirm health-check translation.** The `RuntimeStateAggregator`
   translates pool failures into one of three canonical reasons:
   - `postgres_pool_exhausted`             — pool size cap hit.
   - `postgres_acquisition_failures`       — repeated transient
                                             acquisition errors.
   - `postgres_invalid_pool_config`        — pool failed to construct.
4. **Cross-check append-side latency.** When acquisitions are slow,
   `event_store.append.advisory_lock_wait_ms` rises in lockstep —
   this is the downstream impact. If wait_ms is high but failures
   are zero, the pool is healthy and contention is the issue.
5. **Cross-check outbox-side metrics.** Pool failure during
   `KafkaOutboxPublisher.PublishBatchAsync` results in zero rows
   processed for that poll cycle. The publisher does NOT crash —
   per its outer-loop catch — but `outbox.depth` will rise. This is
   the FR-2 / FR-3 invariant: connection failure leaves the table in
   a consistent state, recovery is automatic when the pool heals.

## Mitigation (TEMPLATE)

- `TBD` — short-term steps:
  - Raise pool max size (config push).
  - Drain affected runtime instances.
  - Failover to standby database (if one exists).

## Resolution (TEMPLATE)

- `TBD` — root cause analysis (db logs, slow queries, replication
  state, OS-level connection limits).

## Post-incident (TEMPLATE)

- `TBD` — verify:
  - `postgres.pool.acquisition_failures` rate returned to baseline.
  - `event_store.append.advisory_lock_wait_ms` p99 returned to
    baseline.
  - `outbox.depth` drained back to steady state.
  - Aggregator is no longer surfacing any of the three canonical
    Postgres reasons.

## References

- Code: [src/platform/host/adapters/PostgresPoolMetrics.cs](../../../src/platform/host/adapters/PostgresPoolMetrics.cs)
- Code: [src/platform/host/adapters/EventStoreDataSource.cs](../../../src/platform/host/adapters/EventStoreDataSource.cs)
- Code: [src/platform/host/adapters/PostgresEventStoreAdapter.cs](../../../src/platform/host/adapters/PostgresEventStoreAdapter.cs)
- Tests: [tests/integration/failure-recovery/PostgresFailureRecoveryTest.cs](../../../tests/integration/failure-recovery/PostgresFailureRecoveryTest.cs) (FR-2)
- Tests: [tests/integration/failure-recovery/RuntimeCrashRecoveryTest.cs](../../../tests/integration/failure-recovery/RuntimeCrashRecoveryTest.cs) (FR-3)
- SLO map: [docs/observability/slo/metric-mapping.md](../slo/metric-mapping.md)
