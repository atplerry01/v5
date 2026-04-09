# Phase 1.5 §5.2.2 — Concurrency Control and Resource Bounds (Final Audit)

**Status: PASS (2026-04-08)**

## Executive Summary

Phase 1.5 §5.2.2 closed PASS on 2026-04-08. The workstream promoted
runtime concurrency primitives, shared locks, in-flight ceilings,
and resource budgets from incidental defaults / opaque hardcoded
constants to declared, observable, bounded primitives. Where §5.2.1
asked *"can the runtime refuse work safely?"*, §5.2.2 answered
*"under what concurrency model does the runtime process accepted
work, and is every binding constraint declared?"*.

The single load-bearing finding was **K-R-03 / C14**: Step B
confirmed the §5.2.1 admission ceiling (`IntakeOptions.GlobalConcurrency = 256`)
admitted ~42× more concurrent work than the realistic downstream
event-store capacity (`MaxPoolSize = 32` ÷ 5 acquisitions per
command ≈ 6 effective in-flight commands). Saturation under the
pre-§5.2.2 envelope produced generic 500s from pool-acquisition
timeout instead of the canonical RETRYABLE REFUSAL edges. KC-1
resolved this by lowering the intake ceiling to match real
downstream capacity; KC-2 further softened it by coalescing the
two-step idempotency exists+mark into a single round-trip via
`INSERT ... ON CONFLICT DO NOTHING`. Together they restored the
§5.2.1 acceptance discipline that pool exhaustion is a *signal*,
not a *failure mode*.

The second material finding was **K-R-01 / K-R-12**: Step B P-K1
discovered that the engine-side `ChainLock` had **zero callers** in
production code, demoting the §5.2.1 hypothesis of "two stacked
chokepoints" to "one global semaphore + one dead-code declaration".
KW-1 promoted the surviving `ChainAnchorService._lock` to declared
status via the new `ChainAnchorOptions.PermitLimit`; KC-7 deleted
the dead `ChainLock` entirely. Structural restructuring of the
chain anchor lock — moving external I/O outside the held section,
sharding by correlation hash — is **explicitly waived** via KW-1
and deferred to a future workstream (likely §5.2.3 or beyond).

KC-3 closed the DLQ growth observability gap on both sides
(outbox-side via a new `outbox.deadletter_depth` gauge from a
single `COUNT(*) FILTER` extension to the existing
`OutboxDepthSampler` probe; consumer-side via a new
`consumer.dlq_publish_failed` counter inside the existing
`$12`-compliant catch block) and added a declared
`OutboxOptions.DeadletterRetention = "operator-managed"` policy.
KC-4 introduced the third declared `NpgsqlDataSource` pool
(`projections`) joining the PC-4 `event-store` and `chain` pools,
covering `PostgresProjectionWriter` and the cross-layer
`TodoProjectionHandler` (with a small local `Whyce.Postgres` meter
mirror inside the projections assembly). `TodoController.Get` is
declared as residual due to the `Whycespace.Api → Whycespace.Host`
project dependency cycle. KC-5 added the `Whyce.EventStore` meter
with `event_store.append.advisory_lock_wait_ms` and
`event_store.append.hold_ms` histograms, measuring the
`pg_advisory_xact_lock` round-trip distinctly from PC-4 pool
acquisitions. KC-6 introduced the `WorkflowAdmissionGate` —
two `PartitionedRateLimiter<string>` instances (per-workflow-name +
per-tenant) gating both Start and Resume — with the typed
`WorkflowSaturatedException` mapped to 503 + `Retry-After` via the
fourth canonical RETRYABLE REFUSAL edge handler. KC-8 added the
`event_store.replay_rows` histogram and explicitly waived
structural streaming of `LoadEventsAsync`.

The §5.2.2 patch list is exhausted. Every K-R-* risk identified at
Step A has been closed (PASS), restructured to reflect Step B
findings (K-R-01, K-R-12), or formally waived with declared
observability (K-R-04, KW-1 chain restructuring). All KC-1 through
KC-8 plus KW-1 build green. The runtime now has four canonical
RETRYABLE REFUSAL edges, eight declared configuration blocks, and
eight canonical `Whyce.*` meters covering every concurrency
primitive on the hot path.

---

## Scope

In-scope surfaces (per opening pack §2.5):

- `ChainAnchorService` semaphore and mutable runtime state
  (`_lastBlockHash`, `_lastSequence`).
- Engine-side `ChainLock` semaphore.
- `RuntimeCommandDispatcher` workflow + engine execution
  concurrency.
- `LoadEventsAsync` fan-in and replay shape.
- `pg_advisory_xact_lock` per-aggregate serialization.
- `PostgresIdempotencyStoreAdapter` per-command 2× connection
  amplification.
- `PostgresProjectionWriter` and projections-side raw connection
  residual from PC-4.
- `TodoController.Get` direct projection read.
- `KafkaOutboxPublisher` drain concurrency / single-instance
  ceiling.
- `GenericKafkaProjectionConsumerWorker` sequential in-flight model.
- DLQ growth / processing bounds (consumer + outbox sides).
- All `SemaphoreSlim`, `lock`, `Monitor`, `Interlocked`, and
  equivalent runtime-control primitives.
- C14 capacity-model envelope: `IntakeOptions.GlobalConcurrency`
  vs `Postgres.Pools.EventStore.MaxPoolSize` vs per-command
  acquisition amplification.

Out-of-scope (explicitly per opening pack §2.6): §5.2.3 timeout /
cancellation / circuit-protection work, §5.3.x throughput
certification, generic performance tuning, capacity planning,
domain-layer changes, locked rules, and replacement of
`SemaphoreSlim` with a different primitive type unless the Step C
analysis demonstrated the substitution was the narrowest fix.

---

## Step A — Concurrency and Resource-Bound Inventory (Summary)

Step A enumerated 16 concurrency / resource-bound surfaces and
classified each under the canonical 4-way model
(`DECLARED-BOUNDED` / `DECLARED-OPAQUE` / `ACCIDENTAL-BOUNDED` /
`UNBOUNDED`). Initial distribution:

| Class | Count |
|---|---|
| DECLARED-BOUNDED | 5 (PC-* outputs) |
| DECLARED-OPAQUE | 5 |
| ACCIDENTAL-BOUNDED | 4 |
| UNBOUNDED | 4 |

Notable findings:

- **K-01 `ChainAnchorService._lock`** — single-permit semaphore
  around external I/O, observable via PC-5 but not declared.
  DECLARED-OPAQUE.
- **K-02 `ChainLock._semaphore`** — second engine-side semaphore,
  caller relationship to K-01 unknown. DECLARED-OPAQUE pending
  Step B clarification.
- **K-03 `pg_advisory_xact_lock`** — per-aggregate serialization
  with unbounded waiter queue and zero observability. DECLARED-OPAQUE.
- **K-08 `PostgresIdempotencyStoreAdapter` 2× amplification** —
  contributor to C14. ACCIDENTAL-BOUNDED.
- **K-09 / K-04 / K-08 capacity model (C14)** — intake 256 vs
  pool 32 vs ~5 acquisitions per command. The load-bearing
  finding.
- **K-12 `LoadEventsAsync`** — full-stream `List<object>` with no
  bound. UNBOUNDED.
- **K-14 / K-15 DLQ growth** — both sides invisible. UNBOUNDED.
- **K-16 projections-pool residual** — `PostgresProjectionWriter`
  and `TodoController.Get` raw `NpgsqlConnection` per call.
  UNBOUNDED.

A repo-wide grep for in-process queues (`Channel<`,
`BlockingCollection`, `ConcurrentQueue`, `Task.Run`, `_ = Task`)
returned **zero hits in `src/`** — confirming the absence of
hidden in-process unbounded buffers and concentrating §5.2.2
attention on the boundary primitives above.

---

## Step B — K-Narrow Probe Summary

Eight static-analysis probes targeting the highest-risk seams.
Three probes returned material findings; five returned ABSENT for
the bound or signal sought.

| Probe | Target | Result | Material finding |
|---|---|---|---|
| **P-K1** | K-01 + K-02 chain lock relationship | **ABSENT (callers)** | `ChainLock` has zero callers under `src/` — it is dead code. K-R-01 demoted from S0 to S1 ("one global lock, not two"); new K-R-12 added (S2 governance) for the dead-code declaration. |
| **P-K2** | K-03 advisory-lock waiter visibility | ABSENT | No `event_store.append.advisory_lock_wait_ms` histogram exists; no `Whyce.EventStore` meter. K-R-06 confirmed S1. |
| **P-K3** | K-R-03 / C14 capacity model | **PRESENT** | Per-command event-store acquisitions = 5: idempotency exists + idempotency mark + load events + append events + outbox enqueue. Effective in-flight ≈ 32/5 ≈ 6 vs intake 256 = ~42× over-provisioned. |
| **P-K4** | K-08 idempotency caller count | PRESENT (2 calls) | Single caller (`IdempotencyMiddleware`); both methods open a fresh connection. K-08 amplification confirmed exactly. |
| **P-K5** | K-12 `LoadEventsAsync` unbounded list | PRESENT | Full-stream `List<object>` with no `LIMIT`, no streaming alternative. K-R-04 confirmed S1. |
| **P-K6** | K-14 + K-15 DLQ depth observability | ABSENT | Zero matches for `dlq.depth`, `outbox.deadletter_depth`, `consumer.dlq_publish_failed`. K-R-02 confirmed S0. |
| **P-K7** | K-06 workflow concurrency cap | ABSENT | Five files reference `WorkflowStartCommand`; none declare a `WorkflowOptions`, in-flight cap, or any concurrency primitive. K-R-05 confirmed S1. |
| **P-K8** | K-16 projections-pool touch surface | PRESENT (3 sites) | Three projections-side raw `NpgsqlConnection` sites: `PostgresProjectionWriter`, `TodoController.Get`, **`TodoProjectionHandler` lines 83+101** (the third site Step A missed). K-R-07 scope expanded. |

---

## Closed Risk Register

| Risk | Surfaces | Step A Class | Post-§5.2.2 Class | Closing Patch |
|---|---|---|---|---|
| **K-R-01** | K-01 `ChainAnchorService._lock` | S0 / DECLARED-OPAQUE | DECLARED-BOUNDED (declared) + observable (PC-5) | KW-1 (declared options) — structural restructuring waived |
| **K-R-02** | K-14 + K-15 DLQ growth | S0 / UNBOUNDED | DECLARED-OBSERVABLE (outbox depth gauge + consumer publish-failure counter) + declared retention policy | KC-3 |
| **K-R-03 / C14** | K-09 + K-04 + K-08 capacity model | S0 / mismatch | DECLARED-BOUNDED (intake matched to downstream) + amplification reduced | KC-1 (intake 256→6, 32→4) + KC-2 (5→4 acquisitions) |
| **K-R-04** | K-12 `LoadEventsAsync` unbounded list | S1 / UNBOUNDED | DECLARED-OBSERVABLE (waived) — `event_store.replay_rows` histogram | KC-8 (observability + declared waiver) |
| **K-R-05** | K-06 workflow execution | S1 / ACCIDENTAL-BOUNDED | DECLARED-BOUNDED — per-workflow-name + per-tenant ceilings | KC-6 (`WorkflowAdmissionGate`) |
| **K-R-06** | K-03 advisory-lock waiter | S1 / DECLARED-OPAQUE | DECLARED-OBSERVABLE | KC-5 (`event_store.append.{advisory_lock_wait_ms,hold_ms}`) |
| **K-R-07** | K-16 projections-pool residual | S1 / UNBOUNDED | DECLARED-BOUNDED for `PostgresProjectionWriter` + `TodoProjectionHandler`; **`TodoController.Get` recorded as declared residual** | KC-4 |
| **K-R-08** | K-08 idempotency 2× amplification | S1 / ACCIDENTAL-BOUNDED | DECLARED-BOUNDED — single-round-trip claim | KC-2 |
| **K-R-12 (NEW at Step B)** | K-02 `ChainLock` dead code | S2 / governance | REMOVED | KC-7 (file deleted) |

K-R-09, K-R-10, K-R-11 from the original Step A risk list either
folded into other rows or were classified S2 with declared
acceptance during Step B and are not load-bearing for §5.2.2
closure.

---

## Per-Patch Summaries

### KC-1 — Capacity-Model Resolution (K-R-03 / C14)

- **Files modified**: [src/shared/contracts/infrastructure/admission/IntakeOptions.cs](src/shared/contracts/infrastructure/admission/IntakeOptions.cs), [src/platform/host/appsettings.json](src/platform/host/appsettings.json).
- **Edit**: `IntakeOptions.GlobalConcurrency` default + appsettings 256 → 6; `PerTenantConcurrency` 32 → 4. Both tied via doc-block comment to the confirmed Step B arithmetic `floor(MaxPoolSize / acquisitions_per_command) = 32/5 = 6`. `QueueLimit=64` and `RetryAfterSeconds=1` preserved.
- **Effect**: a single saturating IP partition holds at most 6 × 5 = 30 connections, within the 32-permit pool. Overload now refuses at the PC-1 limiter as 429 + `Retry-After` instead of timing out at the pool as 500.
- **Build**: green.

### KC-2 — Idempotency Coalescing (K-R-08)

- **Files modified**: [src/shared/contracts/infrastructure/persistence/IIdempotencyStore.cs](src/shared/contracts/infrastructure/persistence/IIdempotencyStore.cs), [src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs](src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs), [src/runtime/middleware/post-policy/IdempotencyMiddleware.cs](src/runtime/middleware/post-policy/IdempotencyMiddleware.cs).
- **Edit**: new `IIdempotencyStore.TryClaimAsync(string key) → bool` returning true if first-seen via `INSERT ... ON CONFLICT (key) DO NOTHING` and reading `rowsAffected == 1`. New `ReleaseAsync(string key)` for failure-path rollback. Pre-KC-2 `ExistsAsync` and `MarkAsync` retained behind `[Obsolete]` for one cycle. `IdempotencyMiddleware` now calls `TryClaimAsync` once + `ReleaseAsync` only on `!IsSuccess` or thrown exception.
- **Effect**: per-command event-store pool consumption drops from 5 to 4. Effective in-flight capacity rises from 32/5 = 6 to 32/4 = 8. KC-1 ceilings remain conservative against the new envelope.
- **Build**: green.

### KC-3 — DLQ Depth Observability + Retention Policy (K-R-02)

- **Files modified**: [src/shared/contracts/infrastructure/messaging/OutboxOptions.cs](src/shared/contracts/infrastructure/messaging/OutboxOptions.cs), [src/platform/host/adapters/OutboxDepthSampler.cs](src/platform/host/adapters/OutboxDepthSampler.cs), [src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs](src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs), [src/platform/host/composition/infrastructure/InfrastructureComposition.cs](src/platform/host/composition/infrastructure/InfrastructureComposition.cs), [src/platform/host/appsettings.json](src/platform/host/appsettings.json).
- **Edit**: `OutboxDepthSampler` probe rewritten to use `COUNT(*) FILTER` to compute pending+failed depth, oldest pending age, and **deadletter depth** in a single round-trip. New `outbox.deadletter_depth` `ObservableGauge` on the existing `Whyce.Outbox` meter. New `consumer.dlq_publish_failed{source_topic, reason}` counter on the existing `Whyce.Projection.Consumer` meter, incremented in the existing `$12`-compliant catch block of `PublishToDeadletterAsync`. New declared `OutboxOptions.DeadletterRetention` (default `"operator-managed"`).
- **Effect**: outbox-side DLQ growth and consumer-side DLQ publish failures are both visible; deadletter handling has a declared posture.
- **Build**: green.

### KC-4 — Projections-Pool Declared `NpgsqlDataSource` (K-R-07)

- **Files added**: [src/platform/host/adapters/ProjectionsDataSource.cs](src/platform/host/adapters/ProjectionsDataSource.cs).
- **Files modified**: [src/platform/host/composition/infrastructure/InfrastructureComposition.cs](src/platform/host/composition/infrastructure/InfrastructureComposition.cs), [src/platform/host/adapters/PostgresProjectionWriter.cs](src/platform/host/adapters/PostgresProjectionWriter.cs), [src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs), [src/projections/operational-system/sandbox/todo/TodoProjectionHandler.cs](src/projections/operational-system/sandbox/todo/TodoProjectionHandler.cs), [src/platform/host/appsettings.json](src/platform/host/appsettings.json).
- **Edit**: third declared logical pool (`projections`) joining `event-store` and `chain` from PC-4. `PostgresProjectionWriter` takes `ProjectionsDataSource`. `TodoProjectionHandler` (in the projections assembly which cannot reference host-adapters) takes `NpgsqlDataSource` directly with a small local mirror of the `Whyce.Postgres` meter (same name → listeners collapse).
- **Residual**: `TodoController.Get` blocked by the api↔host project dependency cycle — declared as residual with a documented forward path (shared `IDbConnectionFactory` abstraction).
- **Effect**: 2 of 3 hot-path projections sites flow through declared pool acquisition with `pool="projections"` tag.
- **Build**: green.

### KC-5 — Advisory-Lock Wait Histogram (K-R-06)

- **Files modified**: [src/platform/host/adapters/PostgresEventStoreAdapter.cs](src/platform/host/adapters/PostgresEventStoreAdapter.cs).
- **Edit**: new `Whyce.EventStore` meter with two histograms — `event_store.append.advisory_lock_wait_ms` (untagged) and `event_store.append.hold_ms` (tagged by `outcome ∈ {ok, concurrency_conflict, exception}`). `Stopwatch.GetTimestamp` brackets the `pg_advisory_xact_lock` `ExecuteNonQueryAsync` and the held section through `tx.CommitAsync`. `try { ... } catch (ConcurrencyConflictException) when (outcome == "ok") { ... } catch when ... { ... } finally { ... }` shape mirrors PC-5.
- **Effect**: per-aggregate Postgres-side contention is now distinct from PC-4 pool acquisition latency.
- **Build**: green.

### KC-6 — Workflow In-Flight Ceiling (K-R-05)

- **Files added**: [src/shared/contracts/infrastructure/admission/WorkflowOptions.cs](src/shared/contracts/infrastructure/admission/WorkflowOptions.cs), [src/shared/contracts/runtime/WorkflowSaturatedException.cs](src/shared/contracts/runtime/WorkflowSaturatedException.cs), [src/runtime/dispatcher/WorkflowAdmissionGate.cs](src/runtime/dispatcher/WorkflowAdmissionGate.cs), [src/platform/api/middleware/WorkflowSaturatedExceptionHandler.cs](src/platform/api/middleware/WorkflowSaturatedExceptionHandler.cs).
- **Files modified**: [src/runtime/Whycespace.Runtime.csproj](src/runtime/Whycespace.Runtime.csproj) (adds `System.Threading.RateLimiting` 9.0.0 package), [src/runtime/dispatcher/RuntimeCommandDispatcher.cs](src/runtime/dispatcher/RuntimeCommandDispatcher.cs), [src/platform/host/composition/runtime/RuntimeComposition.cs](src/platform/host/composition/runtime/RuntimeComposition.cs), [src/platform/host/Program.cs](src/platform/host/Program.cs), [src/platform/host/appsettings.json](src/platform/host/appsettings.json).
- **Edit**: `WorkflowAdmissionGate` composes two `PartitionedRateLimiter<string>` instances (per-workflow-name, per-tenant) sized from `WorkflowOptions { PerWorkflowConcurrency=4, PerTenantConcurrency=6, QueueLimit=8, RetryAfterSeconds=5 }`. `RuntimeCommandDispatcher.ExecuteWorkflowAsync` and `ResumeWorkflowAsync` both acquire the composite lease via `using var admissionLease = await _workflowAdmissionGate.AcquireAsync(workflowName, tenantId)`. Saturation throws `WorkflowSaturatedException` mapped to 503 + `Retry-After` via the new `WorkflowSaturatedExceptionHandler`.
- **Meter**: `Whyce.Workflow` v1.0 — `workflow.admitted{workflow_name}`, `workflow.rejected{workflow_name, partition}`.
- **Effect**: workflow execution has a declared per-workflow-name and per-tenant ceiling with the canonical RETRYABLE REFUSAL edge.
- **Build**: green (after adding the `System.Threading.RateLimiting` package reference to the runtime project).

### KW-1 — ChainAnchor Declared-Options / Waiver (K-R-01)

- **Files added**: [src/shared/contracts/infrastructure/admission/ChainAnchorOptions.cs](src/shared/contracts/infrastructure/admission/ChainAnchorOptions.cs).
- **Files modified**: [src/runtime/event-fabric/ChainAnchorService.cs](src/runtime/event-fabric/ChainAnchorService.cs), [src/platform/host/composition/projections/ProjectionComposition.cs](src/platform/host/composition/projections/ProjectionComposition.cs), [src/platform/host/composition/runtime/RuntimeComposition.cs](src/platform/host/composition/runtime/RuntimeComposition.cs), [src/platform/host/appsettings.json](src/platform/host/appsettings.json).
- **Edit**: new `ChainAnchorOptions` with exactly one field — `PermitLimit` (default 1). `ChainAnchorService` constructor takes the options and constructs `_lock = new SemaphoreSlim(options.PermitLimit, options.PermitLimit)` instead of the literal `1`. No `WaitTimeoutMs`, no `MaxWaiters`, no `Mode` — no invented knobs. The `ChainAnchorOptions` doc block records the structural restructuring deferral honestly with the future workstream owner.
- **Effect**: the chain anchor lock is now `DECLARED-BOUNDED`. Structural restructuring is the declared waiver, not silent residual.
- **Build**: green.

### KC-7 — `ChainLock` Dead-Code Cleanup (K-R-12)

- **Files deleted**: `src/engines/T0U/whycechain/lock/ChainLock.cs` (and the empty parent directory `src/engines/T0U/whycechain/lock/`).
- **Edit**: pure deletion. No replacement, no shim, no annotation. Pre-deletion grep confirmed exactly two repo-wide references — the file itself and a markdown citation in the §5.2.2 opening pack (historical, not compiled). Post-deletion grep returns zero matches under `src/`.
- **Effect**: the "second hidden global semaphore on the chain path" hypothesis is resolved by removal. K-R-12 closed.
- **Build**: green.

### KC-8 — `LoadEventsAsync` Observability + Declared Waiver (K-R-04)

- **Files modified**: [src/platform/host/adapters/PostgresEventStoreAdapter.cs](src/platform/host/adapters/PostgresEventStoreAdapter.cs).
- **Edit**: new `event_store.replay_rows` `Histogram<double>` on the existing `Whyce.EventStore` meter (KC-5). Recorded once per successful `LoadEventsAsync` call as `events.Count`, immediately after the reader loop and before `AsReadOnly()`. The histogram declaration carries a multi-paragraph doc block recording the explicit structural waiver: full streaming/paging requires extending the `IEventStore` interface and is out of §5.2.2 scope; the future structural workstream owns the streaming refactor.
- **Effect**: K-R-04 transitions from `UNBOUNDED` (silent unbounded list) to `DECLARED-OBSERVABLE (waived)`.
- **Build**: green.

---

## Resulting Classification Model Outcomes

### Now DECLARED-BOUNDED

| Surface | Bound | Observability | Patch |
|---|---|---|---|
| HTTP intake (PC-1, KC-1 retuned) | `IntakeOptions.GlobalConcurrency=6`, `PerTenantConcurrency=4`, `QueueLimit=64` | `Whyce.Intake.{intake.admitted, intake.rejected, intake.queue.full}` | KC-1 |
| Idempotency hot path | Single `TryClaimAsync` round-trip, `INSERT ... ON CONFLICT DO NOTHING` | counted via `Whyce.Postgres.postgres.pool.acquisitions{pool="event-store"}` | KC-2 |
| Outbox enqueue (PC-3, unchanged) | `OutboxOptions.HighWaterMark=10000` via shared snapshot | `Whyce.Outbox.{outbox.depth, outbox.oldest_pending_age_seconds}` | (PC-3) |
| Outbox deadletter handling | `OutboxOptions.DeadletterRetention="operator-managed"` | `outbox.deadletter_depth` (KC-3) | KC-3 |
| Projections pool (`PostgresProjectionWriter`, `TodoProjectionHandler`) | `Postgres.Pools.Projections.MaxPoolSize=16` | `Whyce.Postgres.{...}{pool="projections"}` | KC-4 |
| Workflow execution (Start + Resume) | `WorkflowOptions.{PerWorkflowConcurrency=4, PerTenantConcurrency=6, QueueLimit=8}` | `Whyce.Workflow.{workflow.admitted, workflow.rejected}` | KC-6 |
| `ChainAnchorService._lock` | `ChainAnchorOptions.PermitLimit=1` (declared single permit) | `Whyce.Chain.{chain.anchor.wait_ms, chain.anchor.hold_ms}` (PC-5) | KW-1 |
| `KafkaOutboxPublisher` (PC-3 / PC-6, unchanged) | `LIMIT 100` per `_pollInterval`, `OutboxOptions.MaxRetry=5` | `Whyce.Outbox.{outbox.published, outbox.failed, outbox.deadlettered, outbox.dlq_published}` | (PC-3) |
| `GenericKafkaProjectionConsumerWorker` (PC-6, unchanged) | `KafkaConsumerOptions.{QueuedMaxMessagesKbytes=16384, ...}` | `Whyce.Projection.Consumer.{consumer.consumed, consumer.dlq_routed, consumer.handler_invoked, projection.lag_seconds, consumer.dlq_publish_failed}` | (PC-6 + KC-3) |

### Now DECLARED-OBSERVABLE

| Surface | Observability | Bound | Patch |
|---|---|---|---|
| `pg_advisory_xact_lock` per-aggregate wait | `Whyce.EventStore.event_store.append.advisory_lock_wait_ms` | per-aggregate serialization (Postgres-side) | KC-5 |
| `PostgresEventStoreAdapter.AppendEventsAsync` held section | `Whyce.EventStore.event_store.append.hold_ms{outcome}` | per-aggregate via the advisory lock | KC-5 |

### DECLARED-OBSERVABLE (Waived)

| Surface | Waiver Reason | Observability | Patch |
|---|---|---|---|
| `LoadEventsAsync` unbounded `List<object>` | Structural fix requires extending `IEventStore` interface contract, out of §5.2.2 scope. Future structural workstream owns the streaming refactor. | `Whyce.EventStore.event_store.replay_rows` histogram | KC-8 |
| `ChainAnchorService._lock` structural restructuring | Moving I/O outside the held section / sharding by correlation hash requires §5.2.3 (Timeout, Cancellation, Circuit Protection) co-evolution. | `Whyce.Chain.{wait_ms, hold_ms}` (PC-5) + declared `PermitLimit` (KW-1) | KW-1 |

### Removed

- `Whyce.Engines.T0U.WhyceChain.Lock.ChainLock` and `ChainLockHandle` — KC-7. Zero remaining references.

### Residual / Outside §5.2.2 Gate

| Item | Class | Reason for residual |
|---|---|---|
| `TodoController.Get` raw `NpgsqlConnection` | UNBOUNDED (read-side only) | `Whycespace.Api → Whycespace.Host` would create a project dependency cycle; the cleanest fix is a shared `IDbConnectionFactory` abstraction in `shared/contracts` (BCL `DbConnection` shape). Out of §5.2.2 narrow scope. |
| Multi-instance `KafkaOutboxPublisher` drain | DECLARED-OPAQUE | The single-instance shape is the global drain ceiling; multi-instance is architecturally supported (`FOR UPDATE SKIP LOCKED`) but not registered. Declaration-only is sufficient for §5.2.2. |
| Multi-worker per-topic projection parallelism | DECLARED-OPAQUE | In-flight=1 per worker is correct for ordering; partitioned parallelism is a §5.3.x throughput-tuning concern. |
| Native Npgsql / Confluent.Kafka client counter bridging | (acceptable) | Explicit `Whyce.*` counters cover §5.3.x load-work needs; native client counter bridging requires OTel instrumentation packages or custom DiagnosticListener. |
| `PostgreSqlHealthCheck.cs` raw `NpgsqlConnection` | (out of scope) | Health probe, not a hot-path projections site. Not in K-R-07 patch list. |

The residual list is preserved verbatim from the relevant Step C
PASS reports. None of the items above match an opening-pack §2.9
acceptance criterion that would block §5.2.2 PASS.

---

## Final Verification Evidence

- **Build**: `dotnet build src/platform/host/Whycespace.Host.csproj` → 0 errors at the close of every Step C patch (KC-1 through KC-8 plus KW-1) and at workstream closure.
- **Declared configuration blocks present in `appsettings.json`**: `Logging`, `Opa`, `Intake`, `Outbox`, `Postgres.Pools.{EventStore, Chain, Projections}`, `KafkaConsumer`, `Workflow`, `ChainAnchor`. Every key bound at the composition root by `Program.cs`, `InfrastructureComposition`, or `RuntimeComposition`. No orphaned keys, no unbound keys.
- **New meters registered (delta over §5.2.1)**:
  - `Whyce.EventStore` v1.0 (KC-5, extended by KC-8)
  - `Whyce.Workflow` v1.0 (KC-6)
  - **§5.2.2 final meter inventory**: `Whyce.Intake`, `Whyce.Policy`, `Whyce.Outbox`, `Whyce.Postgres`, `Whyce.Chain`, `Whyce.Projection.Consumer`, `Whyce.EventStore`, `Whyce.Workflow` — eight canonical meters.
- **New typed exceptions and edge handlers (delta over §5.2.1)**:
  - `WorkflowSaturatedException` → `WorkflowSaturatedExceptionHandler` → 503 + `Retry-After`
  - **§5.2.2 final retryable-refusal edge inventory**: 429 intake (PC-1), 503 OPA (PC-2), 503 outbox (PC-3), **503 workflow (KC-6)**, plus the pre-existing 409 concurrency-conflict REJECT.
- **New declared options records (delta over §5.2.1)**: `WorkflowOptions`, `ChainAnchorOptions`. Plus extension of `OutboxOptions` with `DeadletterRetention`, extension of `IntakeOptions` defaults, and addition of the `Postgres.Pools.Projections.*` block under the existing `PostgresPoolOptions` shape.
- **Files deleted**: `src/engines/T0U/whycechain/lock/ChainLock.cs` (and the empty parent directory). Zero remaining references in `src/`.
- **Adapters refactored to declared `NpgsqlDataSource` pools (delta over §5.2.1)**: 2 of 3 projections-side hot-path sites (`PostgresProjectionWriter`, `TodoProjectionHandler`); `TodoController.Get` declared residual.
- **Per-command event-store pool consumption**: pre-§5.2.2 = 5 acquisitions per command (P-K3 confirmed); post-KC-2 = 4 acquisitions per command. Effective in-flight ceiling rose from `32/5 ≈ 6` to `32/4 = 8` commands; KC-1 intake ceiling (6) remains conservative against the new envelope.
- **Semantic preservation verified by inspection**: per-aggregate advisory lock, optimistic concurrency check, multi-row INSERT batching, `FOR UPDATE SKIP LOCKED`, `ON CONFLICT DO NOTHING`, exponential backoff, DLQ routing, per-message Kafka commit, per-aggregate Kafka ordering, $8 policy-primacy, $9 determinism, chain head mutation order — all unchanged.
- **Cardinality discipline**: every new metric tag is low-cardinality (`partition`, `pool`, `outcome`, `reason`, `topic`, `source_topic`, `workflow_name`, `policy_id`). No `correlation_id`, `aggregate_id`, `event_id`, or `decision_hash` tags introduced.

---

## Final Status Recommendation

**PASS (2026-04-08).**

All §5.2.2 acceptance criteria from the opening pack §2.9 are
satisfied:

1. ✅ Every concurrency primitive and resource ceiling in §2.5
   scope enumerated and classified (Step A inventory, 16
   surfaces).
2. ✅ Every primitive has at least one reproducible probe (Step B
   K-Narrow + per-KC-* verification).
3. ✅ Every probe has reproducible evidence (per-Step C PASS
   reports).
4. ✅ Every probe result classified with one-line justification.
5. ✅ Every non-`DECLARED-BOUNDED` finding has S0–S3 severity per
   $16.
6. ✅ Every non-`DECLARED-BOUNDED` finding has a remediation patch
   list entry with externalized configuration shape — and KC-1..KC-8
   plus KW-1 delivered every entry on the §5.2.2 patch order.
7. ✅ The end-to-end capacity model walks one accepted request from
   intake through every concurrency primitive on the critical path
   and the C14 mismatch is resolved with a declared decision (KC-1).
8. ✅ `ChainAnchorService` (K-01) has a declared options block via
   KW-1 with the structural restructuring deferral recorded
   honestly.
9. ✅ Projections-side concurrency (K-R-07) covered by KC-4 for the
   two refactorable hot-path sites; `TodoController.Get` recorded
   as declared residual.
10. ✅ DLQ depth bounds (K-R-02) closed by KC-3 with both observability
    halves and the declared retention policy.
11. ✅ Idempotency amplification (K-R-08) closed by KC-2.
12. ✅ Workflow in-flight ceiling (K-R-05) declared via KC-6 with
    the canonical RETRYABLE REFUSAL edge.
13. ✅ Every proposed patch declares its externalized configuration
    shape per the §5.2.1 precedent.
14. ✅ No remediation patch applied during the audit pass; opening
    pack discipline preserved through Step A and Step B; Step C
    patches each gated on green build.
15. ✅ No newly discovered guard rule required `claude/new-rules/`
    capture during this workstream.
16. ✅ Final specification (this document) returns explicit terminal
    status.
17. ✅ README §6.0 row 5.2.2 promoted on real state change, not by
    the opening pack.
