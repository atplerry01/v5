# Recovery SLOs (scaffold)

**STATUS:** SCAFFOLD — every `Target` field is `TBD`.

Recovery SLOs measure how quickly the system **drains accumulated
backlog or failure state** after an upstream outage clears. They are
the operational complement to failure-rate SLOs: F-* tells you "are
we failing?", R-* tells you "how fast do we recover when we stop
failing?".

---

## R-1 — Outbox backlog drain time

| Field | Value |
|-------|-------|
| **Intent** | After Kafka recovers from an outage, the publisher drains the accumulated `outbox.depth` to a steady-state value within budget. |
| **Mapped instrument** | `outbox.depth` on meter `Whyce.Outbox` (`OutboxDepthSampler.cs:52`) |
| **Unit** | rows (ObservableGauge) |
| **Recovery measure** | Time from `outbox.depth` peak to `outbox.depth ≤ steady-state-baseline`. |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | The runbook `outbox-backlog.md` is the manual triage entry point for this SLO. The publisher's batch size (100, hardcoded) and poll interval (1s default) are the system's natural drain rate ceiling. |

## R-2 — Oldest pending row age

| Field | Value |
|-------|-------|
| **Intent** | The oldest unpublished row in the outbox does not exceed a freshness budget; this is the "tail latency" SLO for outbox publish. |
| **Mapped instrument** | `outbox.oldest_pending_age_seconds` on meter `Whyce.Outbox` (`OutboxDepthSampler.cs:54`) |
| **Unit** | s (ObservableGauge) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | This is more sensitive than R-1 because a single stuck row will pin the metric even when the rest of the table drains; pair it with R-1 to distinguish "global slow" from "single bad row". |

## R-3 — Dead-letter depth bounded

| Field | Value |
|-------|-------|
| **Intent** | The number of rows in `status='deadletter'` stays bounded over time. Operators are expected to drain DLQ via the recovery process; this SLO measures whether they keep up. |
| **Mapped instrument** | `outbox.deadletter_depth` on meter `Whyce.Outbox` (`OutboxDepthSampler.cs:58`) |
| **Unit** | rows (ObservableGauge) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | A growing `outbox.deadletter_depth` over time means recovery is not keeping up with promotion — refer to the `outbox-backlog.md` runbook's DLQ-drain section. |

## R-4 — Policy breaker recovery time

| Field | Value |
|-------|-------|
| **Intent** | Once OPA recovers from an outage, the policy breaker transitions Open → Closed within budget so command dispatch resumes. |
| **Mapped instrument** | **Composite** — derive from `policy.evaluate.breaker_open` falling to zero and `policy.evaluate.duration` resuming with `outcome="success"` (`OpaPolicyEvaluator.cs:41,45`) |
| **Recovery measure** | Time from last `policy.evaluate.breaker_open` increment to first `policy.evaluate.duration{outcome="success"}` increment. |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | The breaker config (`OpaOptions.BreakerWindowSeconds`) is the natural floor — the SLO target should be at least the configured window. |

## R-5 — Postgres pool failure decay

| Field | Value |
|-------|-------|
| **Intent** | After a Postgres-side incident clears, the rate of `postgres.pool.acquisition_failures` falls back to baseline within budget. |
| **Mapped instrument** | `postgres.pool.acquisition_failures` rate-of-change on meter `Whyce.Postgres` (`PostgresPoolMetrics.cs:39`) |
| **Recovery measure** | Time from peak failure rate to `failure_rate ≤ baseline`. |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Direct entry point for the `database-connection-issues.md` runbook. The HC-aggregator translation `postgres_acquisition_failures` is the health-check view of the same signal. |

## R-6 — Chain anchor failure decay

| Field | Value |
|-------|-------|
| **Intent** | After a chain-store incident clears, `chain.anchor.hold_ms{outcome != "ok"}` falls to zero within budget. |
| **Mapped instrument** | `chain.anchor.hold_ms` outcome tag on meter `Whyce.Chain` (`ChainAnchorService.cs:47`) |
| **Recovery measure** | Time from last non-`ok` outcome to a sustained run of `ok` outcomes. |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Per FR-5 invariants, events emitted during a chain outage land in the event store but NOT the outbox. R-6 closure does NOT automatically replay them — that is a separate replay-side recovery operation tracked in the `chain-failure.md` runbook. |

## R-7 — Projection consumer catch-up time

| Field | Value |
|-------|-------|
| **Intent** | After a projection consumer outage, `projection.lag_seconds` returns to steady state within budget. |
| **Mapped instrument** | `projection.lag_seconds` on meter `Whyce.Projection.Consumer` (`GenericKafkaProjectionConsumerWorker.cs:68`) |
| **Recovery measure** | Time from peak `projection.lag_seconds` to `lag ≤ steady-state-baseline`. |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Read-side only — the write path is unaffected by projection lag per CQRS. |
