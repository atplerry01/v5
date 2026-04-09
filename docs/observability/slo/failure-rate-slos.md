# Failure-Rate SLOs (scaffold)

**STATUS:** SCAFFOLD — every `Target` field is `TBD`.

Failure-rate SLOs cap the proportion of operations that end in a
recognized failure state. Each SLO is expressed as a ratio
(numerator / denominator) over a rolling window. **Do not invent
thresholds** — every `Target` field is `TBD` until baseline data is
collected.

---

## F-1 — Outbox publish failure rate

| Field | Value |
|-------|-------|
| **Intent** | The proportion of outbox rows whose first publish attempt fails (transient broker errors, ProduceException) stays below a budgeted ceiling. |
| **Numerator** | `outbox.failed` on meter `Whyce.Outbox` (`KafkaOutboxPublisher.cs:29`) |
| **Denominator** | `outbox.published` + `outbox.failed` on meter `Whyce.Outbox` |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | A single `outbox.failed` increment does NOT mean data loss — the row is retried until `MaxRetry` is exhausted. Pair with F-2 for the loss-class SLO. |

## F-2 — Outbox dead-letter promotion rate

| Field | Value |
|-------|-------|
| **Intent** | The proportion of outbox rows that exhaust their retry budget and are promoted to `deadletter` stays below a strict ceiling. This is the "loss-equivalent" SLO. |
| **Numerator** | `outbox.deadlettered` on meter `Whyce.Outbox` (`KafkaOutboxPublisher.cs:30`) |
| **Denominator** | `outbox.published` + `outbox.deadlettered` |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Any non-zero rate over a sustained window indicates an upstream outage; this SLO is the canonical alert source for "events are not making it to consumers". |

## F-3 — Event store concurrency conflict rate

| Field | Value |
|-------|-------|
| **Intent** | Optimistic-concurrency conflicts on `AppendEventsAsync` stay within a healthy contention budget. |
| **Numerator** | `event_store.append.hold_ms` count where `outcome="concurrency_conflict"` (`PostgresEventStoreAdapter.cs:51`) |
| **Denominator** | total `event_store.append.hold_ms` count (all outcomes) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Conflicts are an expected back-pressure signal, not a system error. The SLO is "this rate should not GROW", not "this rate should be zero". |

## F-4 — Postgres pool acquisition failure rate

| Field | Value |
|-------|-------|
| **Intent** | Connection acquisition failures (pool exhaustion, transport, auth) on the `event-store` pool stay near zero. |
| **Numerator** | `postgres.pool.acquisition_failures` on meter `Whyce.Postgres` (`PostgresPoolMetrics.cs:39`) |
| **Denominator** | `postgres.pool.acquisitions` on meter `Whyce.Postgres` (`PostgresPoolMetrics.cs:36`) |
| **Tags on numerator** | `pool` (=`event-store`), `reason` (exception type) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | A spike maps to runbook `database-connection-issues.md`. The aggregator's `postgres_pool_exhausted` and `postgres_acquisition_failures` reasons are the canonical health-check translations of this signal. |

## F-5 — Policy evaluation timeout rate

| Field | Value |
|-------|-------|
| **Intent** | OPA timeouts stay within budget; sustained timeouts indicate a misconfigured `OpaOptions.RequestTimeoutMs` or an OPA-side incident. |
| **Numerator** | `policy.evaluate.timeout` on meter `Whyce.Policy` (`OpaPolicyEvaluator.cs:43`) |
| **Denominator** | total `policy.evaluate.duration` count (`OpaPolicyEvaluator.cs:41`) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Drives the `policy-failure-spike.md` runbook entry path. |

## F-6 — Policy breaker-open rate

| Field | Value |
|-------|-------|
| **Intent** | The fraction of policy calls refused while the OPA breaker is Open stays bounded; sustained breaker-open is an outage class signal. |
| **Numerator** | `policy.evaluate.breaker_open` on meter `Whyce.Policy` (`OpaPolicyEvaluator.cs:45`) |
| **Denominator** | total `policy.evaluate.duration` count |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | This SLO is fail-closed by design — every breaker-open call is a refused command, observable via FR-4 invariants. |

## F-7 — Policy generic failure rate

| Field | Value |
|-------|-------|
| **Intent** | All other policy failures (HTTP non-2xx, transport) stay within a small budget. |
| **Numerator** | `policy.evaluate.failure` on meter `Whyce.Policy` (`OpaPolicyEvaluator.cs:47`) |
| **Denominator** | total `policy.evaluate.duration` count |
| **Tags on numerator** | `policy_id`, `reason` (`http_status`, `transport`, …) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Distinct from timeout (F-5) and breaker-open (F-6) so triage can route on `reason`. |

## F-8 — Chain anchor failure rate

| Field | Value |
|-------|-------|
| **Intent** | The chain anchor critical section completes successfully; failures (`engine_failed` / `exception`) stay near zero. |
| **Numerator** | `chain.anchor.hold_ms` count where `outcome != "ok"` (`ChainAnchorService.cs:47`) |
| **Denominator** | total `chain.anchor.hold_ms` count |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Per FR-5 invariants, a chain failure leaves the event in the source of truth but skips outbox enqueue — sustained failures cause silent backlog of unanchored events that need replay-side recovery. Pair with the `chain-failure.md` runbook. |

## F-9 — Workflow admission rejection rate

| Field | Value |
|-------|-------|
| **Intent** | Workflow admission rejects stay within a small budget; sustained rejections indicate per-tenant or global ceiling exhaustion. |
| **Numerator** | `workflow.rejected` on meter `Whyce.Workflow` (`WorkflowAdmissionGate.cs:42`) |
| **Denominator** | `workflow.admitted` + `workflow.rejected` |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | The corresponding gate config (`WorkflowOptions`) is the lever that tunes this SLO; do not lower the SLO target without first reviewing the option ceilings. |
