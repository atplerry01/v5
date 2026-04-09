# Latency SLOs (scaffold)

**STATUS:** SCAFFOLD — every `Target` field is `TBD`.

Latency SLOs measure how long the system takes to perform a single
unit of work. They are typically expressed as percentile budgets
(`p50 / p95 / p99`) over a rolling window.

| Field | Meaning |
|-------|---------|
| **SLO ID** | Stable identifier for cross-references. |
| **Intent** | Plain-language statement of what the SLO promises. |
| **Mapped instrument** | Canonical meter + instrument name that measures the value. |
| **Unit** | Reported unit on the instrument. |
| **Target** | `TBD` until baseline measurement is in hand. |
| **Window** | Rolling evaluation window — `TBD`. |
| **Owner** | Team / role responsible — `TBD`. |
| **Notes** | Drift / interpretation caveats. |

---

## L-1 — Policy evaluation duration

| Field | Value |
|-------|-------|
| **Intent** | OPA-side policy evaluation completes within an operationally acceptable budget so the dispatch hot path is not held up by policy I/O. |
| **Mapped instrument** | `policy.evaluate.duration` on meter `Whyce.Policy` (`OpaPolicyEvaluator.cs:41`) |
| **Unit** | ms (Histogram&lt;double&gt;) |
| **Tags** | `policy_id`, `outcome` (`success`, `http_status`, `timeout`, etc.) |
| **Target** | `TBD` (p50 / p95 / p99 to be set after baseline) |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Timeout budget is gated by `OpaOptions.RequestTimeoutMs`. SLO target should be ≤ that ceiling minus a safety margin. |

## L-2 — Chain anchor critical-section hold time

| Field | Value |
|-------|-------|
| **Intent** | The single-permit chain anchor critical section completes quickly enough that contention does not cascade upstream. |
| **Mapped instrument** | `chain.anchor.hold_ms` on meter `Whyce.Chain` (`ChainAnchorService.cs:47`) |
| **Unit** | ms (Histogram&lt;double&gt;) |
| **Tags** | `outcome` (`ok`, `engine_failed`, `exception`) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Companion gauge `chain.anchor.wait_ms` measures upstream queue depth — both are needed to triage saturation. |

## L-3 — Chain anchor wait time (queue depth proxy)

| Field | Value |
|-------|-------|
| **Intent** | Time spent waiting for the chain anchor's single permit before entering the critical section is bounded. |
| **Mapped instrument** | `chain.anchor.wait_ms` on meter `Whyce.Chain` (`ChainAnchorService.cs:45`) |
| **Unit** | ms (Histogram&lt;double&gt;) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | When wait_ms approaches `ChainAnchorOptions.WaitTimeoutMs`, callers begin throwing `ChainAnchorWaitTimeoutException` — that boundary is the natural SLO ceiling. |

## L-4 — Event store append hold time

| Field | Value |
|-------|-------|
| **Intent** | Per-aggregate event-store append (advisory lock + INSERT + commit) completes within budget; degradation indicates lock contention or pool saturation. |
| **Mapped instrument** | `event_store.append.hold_ms` on meter `Whyce.EventStore` (`PostgresEventStoreAdapter.cs:51`) |
| **Unit** | ms (Histogram&lt;double&gt;) |
| **Tags** | `outcome` (`ok`, `concurrency_conflict`, `exception`) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | The `concurrency_conflict` outcome is expected at low rates and should NOT count against the SLO numerator — see `failure-rate-slos.md` F-3 for the conflict-rate SLO. |

## L-5 — Event store advisory-lock wait

| Field | Value |
|-------|-------|
| **Intent** | Time waiting for the per-aggregate advisory lock before append is bounded. Spikes indicate hot aggregates. |
| **Mapped instrument** | `event_store.append.advisory_lock_wait_ms` on meter `Whyce.EventStore` (`PostgresEventStoreAdapter.cs:49`) |
| **Unit** | ms (Histogram&lt;double&gt;) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Hot-aggregate detection — pair with append hold time to distinguish lock contention from slow disk. |

## L-6 — Projection consumer lag

| Field | Value |
|-------|-------|
| **Intent** | Read-model projections stay close to the event-fabric publish edge, bounding the staleness window for query-path consumers. |
| **Mapped instrument** | `projection.lag_seconds` on meter `Whyce.Projection.Consumer` (`GenericKafkaProjectionConsumerWorker.cs:68`) |
| **Unit** | s (Histogram&lt;double&gt;) |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Per CQRS rules ($GE-05), the write path must never gate on this — the SLO is consumer-side only. |

## L-7 — End-to-end command dispatch latency

| Field | Value |
|-------|-------|
| **Intent** | Time from `RuntimeControlPlane.ExecuteAsync` entry to `CommandResult` return is bounded. |
| **Mapped instrument** | **UNMAPPED** — no end-to-end histogram exists today. The closest existing signal is the sum of L-1 + L-2 + L-4 plus dispatcher overhead, which is not a single instrument. |
| **Target** | `TBD` |
| **Window** | `TBD` |
| **Owner** | `TBD` |
| **Notes** | Adding this SLO requires a new histogram on the runtime control plane. Out of scope for the §5.4 scaffold pass — recorded here so the gap is visible. |
