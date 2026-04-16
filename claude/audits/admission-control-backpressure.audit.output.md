# Phase 1.5 §5.2.1 — Admission Control and Backpressure (Final Audit)

**Status: PASS (2026-04-08)**

## Executive Summary

Phase 1.5 §5.2.1 closed PASS on 2026-04-08. The workstream established
declared, observable, and bounded overload control at the runtime HTTP
edge and at every internal chokepoint that Step A's surface inventory
identified as a load-bearing risk. All three S0 risks identified at
Step B (R-01 HTTP intake, R-02 outbox table buffer, R-06 OPA
evaluator), the escalated S0 chain-anchor finding (R-03), and the
leading S1/S2 observability gaps (R-04/R-10 Postgres pool, R-06 Kafka
half, R-07 projection lag) were converted from `UNBOUNDED-OPEN` /
`UNBOUNDED-IMPLICIT` / `BOUNDED-OPAQUE` shapes to `BOUNDED-OBSERVABLE`
without architectural redesign.

The runtime now has three canonical RETRYABLE REFUSAL edges (HTTP 429
intake, HTTP 503 policy unavailability, HTTP 503 outbox saturation),
each backed by a typed exception (or limiter `OnRejected` callback)
mapped through the existing `IExceptionHandler` chain, each carrying
`Retry-After`, and each emitting metrics on its own canonical meter.
Five new declared configuration blocks (`Intake.*`, `Opa.*`,
`Outbox.*`, `Postgres.Pools.*`, `KafkaConsumer.*`) replace the
pre-§5.2.1 incidental library defaults across the entire runtime hot
path.

Step A inventoried 20 overload surfaces. Step B's eight B-Narrow
probes returned ABSENT for every signal sought and additionally
surfaced two hot-path I/O findings (OPA HTTP, idempotency-store 2×
connection amplification) that escalated R-06 to S0 and added R-10.
Step C delivered seven mechanical patches (PC-1..PC-7), each green
on `dotnet build` against `src/platform/host/Whycespace.Host.csproj`,
each preserving query / transaction / commit / ordering semantics
verbatim, and each closing exactly the risks it claimed to close.

The §5.2.1 patch list is exhausted. Residual items (projections-pool
refactor, DLQ depth bounds, `LoadEventsAsync` streaming, native
Npgsql/Confluent client counter bridging, structural restructuring of
the `ChainAnchorService` global semaphore) are explicitly outside the
§5.2.1 acceptance gate and queued for follow-on §5.2.x workstreams.

---

## Scope

In-scope surfaces (per opening pack §2.5):

- HTTP/Kestrel intake (`Program.cs`, controllers under
  `src/platform/api/controllers/**`).
- Runtime command intake and dispatch (`SystemIntentDispatcher`,
  `RuntimeCommandDispatcher`, `RuntimeControlPlane`, middleware
  pipeline).
- Workflow intake path (`WorkflowExecutionBootstrap`,
  `T1MWorkflowEngine`).
- Policy evaluation path (`PolicyMiddleware`, `OpaPolicyEvaluator`).
- Event-store append path (`PostgresEventStoreAdapter`).
- Outbox enqueue / publish / drain
  (`PostgresOutboxAdapter`, `KafkaOutboxPublisher`).
- Kafka consumer / projection worker
  (`GenericKafkaProjectionConsumerWorker`).
- Projection writer path (`PostgresProjectionWriter`,
  `IPostgresProjectionWriter`).
- DLQ / retry path (consumer-side and outbox-side).
- In-process channels, queues, buffers, and fire-and-forget seams.
- Bootstrap / config surfaces influencing concurrency, buffering, or
  throughput.
- ChainAnchorService global semaphore.

Out-of-scope (explicitly per opening pack §2.6): generic performance
tuning, capacity planning, hardware sizing, autoscaling policy,
domain-layer changes, engine-layer changes beyond confirming
statelessness, controller redesign, and re-litigation of any locked
DG-R / R-DOM / runtime guard rule.

---

## Step A — Runtime Overload Surface Inventory (Summary)

Step A walked the live runtime path end-to-end and classified 20
surfaces under the canonical 4-way model
(`BOUNDED-OBSERVABLE` / `BOUNDED-OPAQUE` / `UNBOUNDED-IMPLICIT` /
`UNBOUNDED-OPEN`). Initial distribution:

| Class | Count |
|---|---|
| BOUNDED-OBSERVABLE | 0 |
| BOUNDED-OPAQUE | 2 |
| UNBOUNDED-IMPLICIT | 5 |
| UNBOUNDED-OPEN | 4 |

Notable findings:

- **S-01 HTTP intake** — `Program.cs` registered no rate limiter, no
  `Kestrel.Limits` override. UNBOUNDED-OPEN.
- **S-10 outbox table buffer** — fast producers
  (`PostgresOutboxAdapter.EnqueueAsync`) feeding a fixed-rate single
  drain (`KafkaOutboxPublisher` `LIMIT 100` per 1 s poll) with no
  rejection path and no depth signal. UNBOUNDED-OPEN.
- **S-16 ChainAnchorService global lock** — `SemaphoreSlim(1,1)`
  serializing every commit, with two awaited operations (in-process
  engine + external persist) inside the held section, no
  observability. BOUNDED-OPAQUE (severity escalated at Step B).
- **S-18 Npgsql connection pool** — undeclared global ceiling, no
  pool stats exported, all adapters constructing
  `new NpgsqlConnection(...)` per call against the library default.
  UNBOUNDED-IMPLICIT.

A repo-wide grep for `Channel<`, `BlockingCollection`,
`ConcurrentQueue`, `Task.Run`, and `_ = Task` returned **zero hits in
`src/`** — confirming the absence of in-process unbounded buffers and
concentrating §5.2.1 attention on the boundary seams above.

---

## Step B — B-Narrow Probe Summary

Eight static-analysis probes targeting the four highest-risk surfaces
plus four supporting opacity surfaces. **All eight returned ABSENT**
for the bound or signal they were looking for.

| Probe | Target | Result | Key finding |
|---|---|---|---|
| P-B1 | S-01/S-02 HTTP intake | ABSENT | No `AddRateLimiter` registration anywhere in `src/`. |
| P-B2 | S-10 outbox depth metric | ABSENT | `Whyce.Outbox` meter exports counters only, no depth gauge. |
| P-B3 | S-10 outbox high-water-mark | ABSENT | `EnqueueAsync` performs no precheck, no `OutboxSaturatedException` type exists. |
| P-B4 | S-16 chain anchor wait/hold | ABSENT (signal) / PRESENT (I/O inside lock) | Held section spans two awaited operations including external persist. **R-03 escalated S1 → S0.** |
| P-B5 | S-18 Npgsql `MaxPoolSize` | ABSENT | `appsettings.json` was 8 lines (Logging only). No `NpgsqlDataSourceBuilder` anywhere. |
| P-B6 | S-12 Kafka prefetch | ABSENT | `ConsumerConfig` set 4 properties only — `queued.max.messages.kbytes` defaulted to ~1 GiB. |
| P-B7 | S-13/S-11 lag/age metrics | ABSENT | No `projection.lag*` or `outbox.age*` metric anywhere. |
| P-B8 | S-06/S-20 hot-path I/O | PRESENT | `OpaPolicyEvaluator` performs unbounded HTTP per command (no timeout, no breaker). `PostgresIdempotencyStoreAdapter` opens 2 fresh `NpgsqlConnection` per command. **R-06 escalated S1 → S0; new R-10 added.** |

Probe set produced two material escalations:

1. **R-06 → S0** — OPA evaluator unbounded synchronous HTTP on every
   command, $8 policy-primacy meaning *every* command depends on OPA
   latency, OPA outage = full runtime stall.
2. **R-10 (new) → S1** — every load-bearing operational parameter
   lived at the framework/library default rather than declared
   configuration. Governance-class finding affecting the entire
   §5.2.1 patch list.

---

## Closed Risk Register

| Risk | Surfaces | Pre-§5.2.1 Severity / Class | Post-§5.2.1 Class | Closing Patch |
|---|---|---|---|---|
| **R-01** | S-01, S-02 HTTP intake | S0 / UNBOUNDED-OPEN | BOUNDED-OBSERVABLE | PC-1 |
| **R-02** | S-10 outbox buffer | S0 / UNBOUNDED-OPEN | BOUNDED-OBSERVABLE | PC-3 |
| **R-03** | S-16 chain anchor lock | S0 / BOUNDED-OPAQUE | BOUNDED-OBSERVABLE (observability half) | PC-5 |
| **R-04** | S-07/S-08/S-09/S-13/S-18 Npgsql pool | S1 / UNBOUNDED-IMPLICIT | BOUNDED-OBSERVABLE for the seven covered adapters | PC-4 |
| **R-06** | S-06 OPA evaluator + S-12 Kafka prefetch | S0 (OPA half) + S2 (Kafka half) | BOUNDED-OBSERVABLE (both halves) | PC-2 (OPA) + PC-6 (Kafka) |
| **R-07** | S-11/S-12/S-13 lag observability | S2 / BOUNDED-OPAQUE | BOUNDED-OBSERVABLE | PC-3 (outbox age half) + PC-7 (projection lag half) |
| **R-10** | All operational parameters | S1 (governance) | BOUNDED-OBSERVABLE for `Intake.*`, `Opa.*`, `Outbox.*`, `Postgres.Pools.*`, `KafkaConsumer.*` | PC-1..PC-7 (collectively) |

R-03 closure note: PC-5 closes the *observability* half. The
structural concern (single global semaphore on the commit path) is
intentionally outside §5.2.1 scope per the opening pack — the lock
is now `BOUNDED-OBSERVABLE` rather than `BOUNDED-OPAQUE`, satisfying
the §5.2.1 mandate that every binding constraint be measurable.
Restructuring is a §5.2.2 candidate.

R-04 closure note: seven adapters refactored
(`PostgresEventStoreAdapter`, `PostgresOutboxAdapter`,
`PostgresIdempotencyStoreAdapter`, `PostgresSequenceStoreAdapter`,
`OutboxDepthSampler`, `KafkaOutboxPublisher`,
`WhyceChainPostgresAdapter`). `PostgresProjectionWriter` and
`TodoController.Get` are PC-4 residual.

---

## Step C Patch Summaries

### PC-1 — HTTP Intake Admission Control (R-01)

- **Files added**: `src/shared/contracts/infrastructure/admission/IntakeOptions.cs`, `src/platform/host/adapters/IntakeMetrics.cs`.
- **Files modified**: `src/platform/host/Program.cs`, `src/platform/host/appsettings.json`.
- Registers `AddRateLimiter` with a `PartitionedRateLimiter<HttpContext, string>` keyed `X-Tenant-Id` → remote IP. `ConcurrencyLimiterOptions` sized from `IntakeOptions.PerTenantConcurrency` / `GlobalConcurrency` / `QueueLimit`. `OnRejected` writes `Retry-After` and `429`.
- **Meter**: `Whyce.Intake` v1.0 — `intake.admitted`, `intake.rejected`, `intake.queue.full` (tagged by `partition ∈ {tenant, ip}`).
- **Refusal class**: RETRYABLE REFUSAL (429 + `Retry-After`).
- **Config block**: `Intake.{GlobalConcurrency, QueueLimit, PerTenantConcurrency, RejectionResponse, RetryAfterSeconds}`.
- Wired before `MapControllers`. Build green.

### PC-2 — OPA Hardening (R-06 OPA half)

- **Files added**: `src/shared/contracts/infrastructure/policy/OpaOptions.cs`, `src/shared/contracts/infrastructure/policy/PolicyEvaluationUnavailableException.cs`, `src/platform/api/middleware/PolicyEvaluationUnavailableExceptionHandler.cs`.
- **Files modified**: `src/platform/host/adapters/OpaPolicyEvaluator.cs` (full I/O path rewrite), `src/platform/host/composition/infrastructure/InfrastructureComposition.cs`, `src/platform/host/Program.cs`, `src/platform/host/appsettings.json`.
- Per-call `CancellationTokenSource(OpaOptions.RequestTimeoutMs)` linked to the `PostAsync`. In-process consecutive-failure circuit breaker (Closed/Open/HalfOpen) keyed off `IClock` for $9 consistency. Every failure branch (`http_status`, `timeout`, `transport`, `breaker_open`) throws `PolicyEvaluationUnavailableException` — never an implicit allow. `EnsureSuccessStatusCode` removed.
- **Meter**: `Whyce.Policy` v1.0 — `policy.evaluate.duration` histogram, `policy.evaluate.timeout` / `policy.evaluate.breaker_open` / `policy.evaluate.failure` counters.
- **Refusal class**: RETRYABLE REFUSAL (503 + `Retry-After`).
- **Config block**: `Opa.{Endpoint, RequestTimeoutMs, BreakerThreshold, BreakerWindowSeconds, OpenStateBehavior}`.
- $8 policy-primacy preserved by construction (typed exception bubbles untouched from throw site to edge handler). Build green.

### PC-3 — Outbox Depth Gauge + High-Water-Mark Rejection (R-02, R-07 outbox half)

- **Files added**: `src/shared/contracts/infrastructure/messaging/IOutboxDepthSnapshot.cs`, `src/shared/contracts/infrastructure/messaging/OutboxSaturatedException.cs`, `src/platform/host/adapters/OutboxDepthSnapshot.cs`, `src/platform/host/adapters/OutboxDepthSampler.cs`, `src/platform/api/middleware/OutboxSaturatedExceptionHandler.cs`.
- **Files modified**: `src/shared/contracts/infrastructure/messaging/OutboxOptions.cs`, `src/platform/host/adapters/PostgresOutboxAdapter.cs`, `src/platform/host/composition/infrastructure/InfrastructureComposition.cs`, `src/platform/host/Program.cs`, `src/platform/host/appsettings.json`.
- Periodic `OutboxDepthSampler` `BackgroundService` runs `SELECT COUNT(*), MIN(created_at)` over `status IN ('pending','failed')` every `OutboxOptions.SamplingIntervalSeconds`. Publishes to shared `IOutboxDepthSnapshot` singleton (read by `PostgresOutboxAdapter` — no per-enqueue COUNT).
- **Meter**: `Whyce.Outbox` v1.0 (extended) — `outbox.depth` and `outbox.oldest_pending_age_seconds` `ObservableGauge`s alongside the existing publisher counters.
- **Refusal class**: RETRYABLE REFUSAL (503 + `Retry-After`).
- **Config block extension**: `Outbox.{HighWaterMark, SamplingIntervalSeconds, SaturationResponse, RetryAfterSeconds}` joins the existing `MaxRetry`.
- Events refused, never silently dropped. Build green.

### PC-4 — Npgsql Pool Declared + Observable (R-04, R-10 Postgres half)

- **Files added**: `src/shared/contracts/infrastructure/persistence/PostgresPoolOptions.cs`, `src/platform/host/adapters/PostgresPoolMetrics.cs`, `src/platform/host/adapters/EventStoreDataSource.cs`, `src/platform/host/adapters/ChainDataSource.cs`.
- **Files modified**: `PostgresEventStoreAdapter.cs`, `PostgresOutboxAdapter.cs`, `PostgresIdempotencyStoreAdapter.cs`, `PostgresSequenceStoreAdapter.cs`, `OutboxDepthSampler.cs`, `KafkaOutboxPublisher.cs`, `WhyceChainPostgresAdapter.cs`, `InfrastructureComposition.cs`, `appsettings.json`.
- Two declared logical pools — `event-store` (six adapters) and `chain` (one adapter). `BuildDataSource(PostgresPoolOptions)` applies `MaxPoolSize`, `MinPoolSize`, `Timeout`, `CommandTimeout` via `NpgsqlConnectionStringBuilder`. Adapters acquire via `OpenInstrumentedAsync(poolName)` extension on `NpgsqlDataSource`.
- **Meter**: `Whyce.Postgres` v1.0 — `postgres.pool.acquisitions` and `postgres.pool.acquisition_failures` (tagged by `pool`, failures additionally by `reason`).
- **Config block**: `Postgres.Pools.EventStore.*` and `Postgres.Pools.Chain.*`.
- All query / transaction / advisory-lock / ordering semantics preserved. Build green.

### PC-5 — ChainAnchorService Wait/Hold Observability (R-03)

- **Files modified**: `src/runtime/event-fabric/ChainAnchorService.cs`.
- New `Whyce.Chain` v1.0 meter with two `Histogram<double>` instruments: `chain.anchor.wait_ms` (semaphore acquisition time) and `chain.anchor.hold_ms` (critical section time, tagged by `outcome ∈ {ok, engine_failed, exception}`). Two `Stopwatch.GetTimestamp` reads per call. `catch when (outcome == "ok") { outcome = "exception"; throw; }` filter classifies and rethrows without swallowing.
- **No semantic change**: lock, ordering (`_chainEngine.Anchor → _chainAnchor.AnchorAsync`), exception propagation, and `_lock.Release()` lifecycle preserved verbatim. Build green.

### PC-6 — Kafka Consumer Prefetch Ceiling Declared (R-06 Kafka half, R-10 Kafka half)

- **Files added**: `src/shared/contracts/infrastructure/messaging/KafkaConsumerOptions.cs`.
- **Files modified**: `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs`, `src/platform/host/composition/infrastructure/InfrastructureComposition.cs`, `src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs`, `src/platform/host/appsettings.json`.
- `ConsumerConfig` now sets `QueuedMaxMessagesKbytes`, `MessageMaxBytes` (Confluent.Kafka 2.x canonical name for the librdkafka `message.max.bytes` semantic), `MaxPollIntervalMs`, `SessionTimeoutMs` from `KafkaConsumerOptions`. Default prefetch ceiling 16 MiB — three orders of magnitude tighter than the librdkafka default.
- Single startup `LogInformation` line per worker records the applied envelope.
- **Config block**: `KafkaConsumer.{QueuedMaxMessagesKbytes, FetchMessageMaxBytes, MaxPollIntervalMs, SessionTimeoutMs}`.
- Sequential `consume → handle → commit` shape preserved. Build green.

### PC-7 — Projection Lag Gauge (R-07 projection half)

- **Files modified**: `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs`.
- New `projection.lag_seconds` `Histogram<double>` on the existing `Whyce.Projection.Consumer` meter, tagged by `topic`. Recorded once per successfully projected message at the post-write convergence point, before `consumer.Commit(result)`.
- **Definition**: `(_clock.UtcNow.UtcDateTime - result.Message.Timestamp.UtcDateTime).TotalSeconds`, where `result.Message.Timestamp` is the durable broker-set Kafka record timestamp.
- DLQ-routed messages bypass the lag site (each DLQ branch contains its own `Commit; continue`). Build green.

---

## Resulting Boundedness Model

### Now BOUNDED-OBSERVABLE

| Surface | Bound | Observability | Refusal Class |
|---|---|---|---|
| HTTP intake (S-01/S-02) | `IntakeOptions.GlobalConcurrency` / `PerTenantConcurrency` + `QueueLimit` | `Whyce.Intake.{intake.admitted, intake.rejected, intake.queue.full}` | RETRYABLE REFUSAL → 429 + `Retry-After` |
| OPA policy evaluator (S-06) | `OpaOptions.RequestTimeoutMs` + breaker | `Whyce.Policy.{policy.evaluate.duration, .timeout, .breaker_open, .failure}` | RETRYABLE REFUSAL → 503 + `Retry-After` |
| Outbox enqueue (S-09 / S-10) | `OutboxOptions.HighWaterMark` via shared snapshot | `Whyce.Outbox.{outbox.depth, outbox.oldest_pending_age_seconds, outbox.published, outbox.failed, outbox.deadlettered}` | RETRYABLE REFUSAL → 503 + `Retry-After` |
| Outbox publisher drain (S-11) | `LIMIT 100` per `_pollInterval`, `OutboxOptions.MaxRetry` | publisher counters above | (no upstream refusal needed) |
| Event-store append (S-07) | per-aggregate advisory lock + declared `event-store` pool ceiling | `Whyce.Postgres.{postgres.pool.acquisitions, postgres.pool.acquisition_failures}` (`pool="event-store"`) | (advisory-lock waiter shedding deferred to §5.2.2) |
| Idempotency / sequence stores (S-20 / supporting) | declared `event-store` pool ceiling | same Postgres pool metrics | (— ) |
| Chain anchor (S-16) | observability — held duration measured | `Whyce.Chain.{chain.anchor.wait_ms, chain.anchor.hold_ms}` (with `outcome` tag) | (structural restructuring deferred to §5.2.2) |
| Chain persist (S-17) | declared `chain` pool ceiling | `Whyce.Postgres` (`pool="chain"`) | (— ) |
| Kafka consumer prefetch (S-12) | `KafkaConsumerOptions.QueuedMaxMessagesKbytes` (16 MiB default) | `Whyce.Projection.Consumer.{consumer.consumed, consumer.dlq_routed, consumer.handler_invoked, projection.lag_seconds}` | broker-side lag (correct backpressure) |
| Projection writer (S-13) | sequential in-flight=1 per worker (consumer-induced) | `projection.lag_seconds` (post-write, broker-timestamp derived) | (— ) |

### Residual / Outside §5.2.1 Gate

| Item | Class | Why outside the gate |
|---|---|---|
| `PostgresProjectionWriter` raw `NpgsqlConnection` per call | UNBOUNDED-IMPLICIT | Constructed by domain bootstrap modules; refactor would touch the domain bootstrap surface. Recorded as PC-4 residual. |
| `TodoController.Get` direct `NpgsqlConnection` | UNBOUNDED-IMPLICIT | Operational sample controller; refactor widens scope into systems layer. |
| DLQ depth bounds (S-14, S-15) | UNBOUNDED-OPEN | R-08, S2 — not on the §5.2.1 PC-* order; queued for §5.2.x. |
| `LoadEventsAsync` unbounded list | UNBOUNDED-IMPLICIT | R-09, S2 — latent rather than load-bearing at current scale. |
| `ChainAnchorService` global semaphore structural restructuring | BOUNDED-OBSERVABLE (acceptable) | §5.2.1 requires *measurable*, not *restructured*; restructuring is §5.2.2 candidate. |
| Native Npgsql / Confluent.Kafka client counter bridging | (acceptable) | Explicit `Whyce.*` counters cover §5.3.x load-work needs; native counter bridging requires OTel instrumentation packages or custom DiagnosticListener — out of scope. |
| Per-route admission policies / `/metrics` exemption | (acceptable) | Limiter is global by design; per-route policies are §5.2.x candidates. |

The residual list is preserved verbatim from the relevant PC-* PASS
reports. None of the items above match an opening-pack §2.9
acceptance criterion that would block §5.2.1 PASS.

---

## Final Verification Evidence

- **Build**: `dotnet build src/platform/host/Whycespace.Host.csproj` → 0 Warning(s), 0 Error(s). Verified at the close of every Step C patch (PC-1 through PC-7) and at the close of §5.2.1 overall.
- **Declared configuration blocks present in `appsettings.json`**: `Intake`, `Opa`, `Outbox`, `Postgres.Pools.EventStore`, `Postgres.Pools.Chain`, `KafkaConsumer`. Every key bound by `Program.cs` or `InfrastructureComposition`. No orphaned keys, no unbound keys.
- **New meters registered**:
  - `Whyce.Intake` v1.0 (PC-1)
  - `Whyce.Policy` v1.0 (PC-2)
  - `Whyce.Outbox` v1.0 (extended by PC-3)
  - `Whyce.Postgres` v1.0 (PC-4)
  - `Whyce.Chain` v1.0 (PC-5)
  - `Whyce.Projection.Consumer` v1.0 (extended by PC-7)
- **New typed exceptions and edge handlers**:
  - `PolicyEvaluationUnavailableException` → `PolicyEvaluationUnavailableExceptionHandler` → 503 + `Retry-After`
  - `OutboxSaturatedException` → `OutboxSaturatedExceptionHandler` → 503 + `Retry-After`
  - existing `ConcurrencyConflictException` → `ConcurrencyConflictExceptionHandler` → 409 (REJECT, unchanged)
  - HTTP rate limiter `OnRejected` → 429 + `Retry-After` (RETRYABLE REFUSAL, no exception)
- **Adapters refactored to use declared Npgsql pools**: 7 of 9 (`PostgresEventStoreAdapter`, `PostgresOutboxAdapter`, `PostgresIdempotencyStoreAdapter`, `PostgresSequenceStoreAdapter`, `OutboxDepthSampler`, `KafkaOutboxPublisher`, `WhyceChainPostgresAdapter`). The two remaining (`PostgresProjectionWriter`, `TodoController.Get`) are PC-4 residual.
- **Semantic preservation verified by inspection**: per-aggregate advisory lock, optimistic concurrency, multi-row INSERT batching, `FOR UPDATE SKIP LOCKED`, `ON CONFLICT DO NOTHING`, exponential backoff, DLQ routing, per-message Kafka commit, per-aggregate Kafka ordering, $8 policy-primacy, $9 determinism — all unchanged.
- **Cardinality discipline**: every new metric tag is low-cardinality (`partition`, `pool`, `topic`, `outcome`, `reason`, `policy_id`). No `correlation_id`, `aggregate_id`, `event_id`, or `decision_hash` tags introduced.

---

## Final Status Recommendation

**PASS (2026-04-08).**

All §5.2.1 acceptance criteria from the opening pack §2.9 are
satisfied:

1. ✅ Every runtime entry boundary and async seam in §2.5 scope is
   enumerated and classified (Step A inventory, 20 surfaces).
2. ✅ Every seam has at least one reproducible probe (Step B B-Narrow
   + per-PC-* verification).
3. ✅ Every probe has reproducible evidence (per-PC-* PASS reports).
4. ✅ Every probe result classified with one-line justification.
5. ✅ Every non-`BOUNDED-OBSERVABLE` finding has S0–S3 severity per
   $16.
6. ✅ Every non-`BOUNDED-OBSERVABLE` finding has a remediation patch
   list entry with externalized configuration shape — and PC-1..PC-7
   delivered every entry on the §5.2.1 PC-* order.
7. ✅ Every runtime entry boundary has a declared response-class
   mapping: REJECT (409 concurrency), RETRYABLE REFUSAL (429 intake,
   503 policy, 503 outbox).
8. ✅ Every downstream saturation source has a documented propagation
   path or is explicitly flagged as residual.
9. ✅ WHYCEPOLICY $8 evaluation order vs. admission shedding declared
   and consistent — PolicyMiddleware runs before any engine
   execution; OPA failure refuses at the edge instead of allowing.
10. ✅ Admission and shedding decisions use `IClock` (chain anchor,
    OPA breaker), no `Guid.NewGuid`, no wall-clock dependency on
    correctness paths.
11. ✅ Minimum observability surface for §5.3.x soak defined and
    implemented across `Whyce.Intake`, `Whyce.Policy`, `Whyce.Outbox`,
    `Whyce.Postgres`, `Whyce.Chain`, `Whyce.Projection.Consumer`.
12. ✅ No remediation patch applied during the audit pass; opening
    pack discipline preserved through Step A and Step B; Step C
    patches each gated on green build.
13. ✅ No newly discovered guard rule required `claude/new-rules/`
    capture during this workstream (all findings tracked through the
    existing risk register).
14. ✅ Final specification (this document) returns explicit terminal
    status.
15. ✅ README §6.0 row 5.2.1 promoted on real state change, not by
    the opening pack.
