# §5.6 Scenario 2 — Postgres Outage During Load (EVIDENCE)

**Scenario:** Postgres connection drops mid-batch while the runtime is appending event-store / outbox rows under load.
**Date:** 2026-04-09
**Test file:** [tests/integration/failure-recovery/PostgresFailureRecoveryTest.cs](../../../../../tests/integration/failure-recovery/PostgresFailureRecoveryTest.cs)
**Stack:** real `whyce-postgres` (5432).
**Tests run:**
- `Connection_Drop_Mid_Batch_Rollbacks_To_Zero_Rows` — **PASSED [21 ms]**
- `Recovery_After_Rollback_Reinserts_Exactly_Once` — **PASSED [29 ms]**

## Refusal seam exercised
- §5.2.1 PC-4 declared `Postgres.Pools.EventStore` envelope
- §5.2.2 KC-5 advisory-lock wait observability (`Whyce.EventStore`)
- §5.2.3 TC-5 cancellation token threading through every adapter

## Behavior verified
- **Connection-drop path:** an `Execute*Async` failure mid-transaction
  rolls back atomically. `CountByCorrelationAsync` reports **0 rows**
  for the failed correlation id — no partial commit, no orphaned rows.
- **Recovery path:** after the connection is restored, re-issuing the
  same batch under the same correlation id reinserts exactly the
  expected `rowCount` rows. No duplication relative to a parallel
  successful run; the rolled-back transaction left zero residue.

## Snapshots
- **Before:** count(corr) = 0
- **During (drop):** transaction aborted, count(corr) = 0
- **After (recovery + retry):** count(corr) = `rowCount`

## Acceptance
| F1 | F2 | F3 | F4 | F5 | F6 |
|---|---|---|---|---|---|
| PASS | PASS | PASS — automatic via Npgsql reconnect | PASS — TC-5 CT propagation, PC-4 envelope | PASS — sub-30 ms recovery in test composition | PASS |
