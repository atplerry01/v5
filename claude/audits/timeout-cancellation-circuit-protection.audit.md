# Phase 1.5 §5.2.3 — Timeout, Cancellation, and Circuit Protection (Final Audit)

**Status: PASS (2026-04-08)**

## Executive Summary

Phase 1.5 §5.2.3 closed PASS on 2026-04-08. The workstream eliminated
indefinite-wait, untokened, and circuit-unprotected seams from the
runtime hot path. Where §5.2.1 made the runtime *refuse work safely*
under load and §5.2.2 made the runtime *process accepted work
predictably* under concurrent pressure, §5.2.3 answered *"under what
declared timeout, cancellation, and circuit-protection discipline does
the runtime fail fast instead of hanging?"*.

The single load-bearing finding from Step B was that **no real
`CancellationToken` reached the runtime**: the controller bound an
ASP.NET CT, but the dispatcher → middleware → control plane → fabric
chain dropped it on the floor at every seam. TC-1 closed this by
co-evolving the dispatcher / middleware / controller contracts in a
single coordinated pass; once TC-1 landed, TC-2 / TC-3 / TC-5 / TC-6 /
TC-7 / TC-8 / TC-9 inherited a real token at every seam they touched.

The second material finding was the **chain-anchor wait/holder pair**:
PC-5 had made the chain anchor critical section observable but the
wait was indefinite (no timeout, no token) and the holder I/O was
uncancellable (no breaker, no token). TC-2 bounded the wait with a
declared `WaitTimeoutMs` and a typed `ChainAnchorWaitTimeoutException`;
TC-3 threaded CT into the chain-store I/O and added a declared
consecutive-failure circuit breaker mirroring the PC-2 OPA pattern,
with a typed `ChainAnchorUnavailableException`. Together they form the
canonical chain-anchor refusal family — wait-side and holder-side —
mapped to two co-resident API edge handlers.

The third material finding was **13 of 18 Postgres `Execute*Async`
call sites still dropped CancellationToken**. TC-5 mechanically
threaded CT through `IEventStore`, `IOutbox`, `IIdempotencyStore`, and
`ISequenceStore` and through every `Postgres*Adapter` implementation,
plus the `EventStoreService` / `OutboxService` wrappers and the live
runtime call sites in `EventFabric`, `RuntimeCommandDispatcher`, and
`IdempotencyMiddleware`. Empty-paren `Execute*Async` forms at the
targeted hot-path adapters are gone.

The remaining material findings were the **projection handler hung-step
seam** (TC-6 — both projection-handler contracts now consume the
worker `stoppingToken`, and `TodoProjectionHandler` actively threads it
into its `ExecuteReaderAsync` / `ReadAsync` / `ExecuteNonQueryAsync`
calls), the **workflow execution unbounded shape** (TC-7 — declared
two-tier `PerStepTimeoutMs` / `MaxExecutionMs` discipline backed by
linked CTSs and the typed `WorkflowTimeoutException`), the **workflow
admission gate token-drop** (TC-8 — `WorkflowAdmissionGate.AcquireAsync`
now receives the real CT at both dispatcher call sites), and the
**undeclared host-shutdown drain** (TC-9 — declared
`Host:ShutdownTimeoutSeconds` applied via `HostOptions.ShutdownTimeout`
plus `HostShutdownLinkingMiddleware` linking
`IHostApplicationLifetime.ApplicationStopping` into
`HttpContext.RequestAborted` so the entire TC-1 token chain inherits
the shutdown signal at the very edge).

The §5.2.3 patch list is exhausted. Every T-R-* risk identified at
Step A has been closed (PASS) or formally waived with declared
observability. All TC-1 through TC-9 build green. The runtime now has
**seven canonical RETRYABLE REFUSAL edges** (intake 429, OPA 503,
outbox 503, workflow saturation 503, **chain-anchor wait timeout 503**,
**chain-anchor unavailable 503**, **workflow timeout 503**), real
end-to-end CT propagation from controller to database round-trip, and
declared timeouts on every previously-unbounded waiter.

---

## Scope

In-scope surfaces (per opening pack §2.5):

- `RuntimeCommandDispatcher` / `SystemIntentDispatcher` /
  middleware-pipeline / `RuntimeControlPlane` `CancellationToken`
  contract.
- `ChainAnchorService._lock` semaphore wait shape and held-section
  cancellation.
- `WhyceChainPostgresAdapter` chain-store I/O cancellation and
  circuit protection.
- `Postgres*Adapter` `Execute*Async` token threading at the four
  canonical hot-path adapters: `PostgresEventStoreAdapter`,
  `PostgresOutboxAdapter`, `PostgresIdempotencyStoreAdapter`,
  `PostgresSequenceStoreAdapter`.
- `IEnvelopeProjectionHandler` / `IProjectionHandler<T>` contract
  cancellation surface and the `GenericKafkaProjectionConsumerWorker`
  forward path.
- `WorkflowAdmissionGate.AcquireAsync` token threading at the
  dispatcher call sites.
- `IWorkflowEngine` / `IWorkflowStep` / `WorkflowStepExecutor`
  cancellation contract and declared per-step / per-execution
  timeouts.
- Host-level graceful shutdown timeout and the linking seam from
  `IHostApplicationLifetime.ApplicationStopping` into the request
  CT path.

Out-of-scope (explicitly per opening pack §2.6): §5.2.4 health /
readiness / degraded-mode work, §5.2.5 multi-instance runtime safety,
§5.3.x throughput certification, generic performance tuning, capacity
planning, domain-layer changes, locked rules, structural restructuring
of the chain-anchor lock (the held-section I/O remains inside the
permit), engine-side / Kafka-producer / Redis timeouts beyond the
adapters in §2.5, and broader Postgres adapter token threading
outside the four hot-path adapters listed above.

---

## Step A — Timeout / Cancellation / Circuit Inventory (Summary)

Step A enumerated 19 timeout / cancellation / circuit-protection
surfaces and classified each on the canonical 4-way model
(`DECLARED-BOUNDED` / `DECLARED-OPAQUE` / `ACCIDENTAL-BOUNDED` /
`UNBOUNDED`). Initial distribution:

| Class | Count |
|---|---|
| DECLARED-BOUNDED | 4 (PC-2 OPA breaker / PC-3 outbox / PC-1 intake / PC-6 Kafka consumer poll) |
| DECLARED-OPAQUE | 2 |
| ACCIDENTAL-BOUNDED | 3 |
| UNBOUNDED | 10 |

Notable findings:

- **T-01 dispatcher / middleware / controller CT contract** —
  controller bound an ASP.NET CT, but every downstream seam dropped
  it on the floor. UNBOUNDED.
- **T-02 `ChainAnchorService._lock` wait** — `WaitAsync()` with no
  timeout, no token. PC-5 made the wait *observable* but not
  *bounded*. UNBOUNDED.
- **T-03 chain-store I/O** — `ExecuteScalarAsync()` /
  `ExecuteNonQueryAsync()` empty-paren forms; no breaker on
  transport failure. UNBOUNDED.
- **T-04 Postgres hot-path adapters** — 13 of 18 `Execute*Async`
  call sites in `PostgresEventStoreAdapter`,
  `PostgresOutboxAdapter`, `PostgresIdempotencyStoreAdapter`, and
  `PostgresSequenceStoreAdapter` dropped CancellationToken.
  UNBOUNDED.
- **T-05 projection handler contracts** — neither
  `IEnvelopeProjectionHandler.HandleAsync` nor
  `IProjectionHandler<T>.HandleAsync` accepted a CancellationToken.
  A hung handler stalled the worker until the Kafka
  `MaxPollIntervalMs` / `SessionTimeoutMs` ceilings intervened.
  UNBOUNDED.
- **T-06 workflow execution timeout** — neither per-step nor
  overall execution had a declared deadline. A hung step held its
  workflow admission slot indefinitely (post-KC-6 the gate had a
  declared in-flight ceiling but no per-execution wall-clock).
  UNBOUNDED.
- **T-07 workflow gate token-drop** — `WorkflowAdmissionGate.AcquireAsync`
  already accepted CancellationToken (TC-1's contract co-evolution),
  but `RuntimeCommandDispatcher` passed `default`. ACCIDENTAL-BOUNDED.
- **T-08 host shutdown drain** — `HostOptions.ShutdownTimeout`
  inherited the .NET default (30 s) without declaration; no linking
  seam from `ApplicationStopping` into the request CT path.
  ACCIDENTAL-BOUNDED.
- **T-09 retry / non-retry boundaries** — folded into the canonical
  refusal-edge response shape (no separate patch needed once the
  typed exceptions in TC-2 / TC-3 / TC-7 landed).

---

## Step B — T-Narrow Probe Summary

Six static-analysis probes targeting the highest-risk seams. All
six returned material findings consistent with the Step A inventory.

| Probe | Target | Result | Material finding |
|---|---|---|---|
| **P-T1** | T-01 controller→dispatcher CT contract | **PRESENT (drop)** | `ICommandDispatcher.DispatchAsync` did not accept CT; `IMiddleware.ExecuteAsync` did not forward CT; `RuntimeControlPlane.ExecuteAsync` accepted CT but never threaded it. The full chain was a CT desert. |
| **P-T2** | T-02 chain anchor wait shape | PRESENT | `_lock.WaitAsync()` empty-paren form; `ChainAnchorOptions` had no `WaitTimeoutMs`; no typed timeout exception. |
| **P-T3** | T-03 chain-store I/O | PRESENT | `ExecuteScalarAsync()` / `ExecuteNonQueryAsync()` empty-paren forms in `WhyceChainPostgresAdapter`; no breaker primitives present. |
| **P-T4** | T-04 hot-path Postgres adapters | PRESENT | 13 of 18 `Execute*Async` call sites in the four target adapters dropped CT. The five that already passed CT were all in chain anchor or PC-* paths. |
| **P-T5** | T-05 projection handler contracts | PRESENT | `IEnvelopeProjectionHandler.HandleAsync(IEventEnvelope)` / `IProjectionHandler<T>.HandleAsync(TEvent)` — neither carried CT. The worker passed `stoppingToken` only into the generic-write fallback `_writer.WriteAsync`. |
| **P-T6** | T-06 workflow execution shape | PRESENT | `IWorkflowStep.ExecuteAsync(WorkflowExecutionContext)` and `IWorkflowEngine.ExecuteAsync(definition, context)` both no-token; `WorkflowOptions` had no timeout fields. |

---

## Closed Risk Register

| Risk | Surfaces | Step A Class | Post-§5.2.3 Class | Closing Patch |
|---|---|---|---|---|
| **T-R-01** | T-01 dispatcher / middleware / controller CT contract | S0 / UNBOUNDED | DECLARED-BOUNDED — real CT from controller through dispatcher → middleware → control plane → fabric | TC-1 |
| **T-R-02** | T-02 chain anchor wait + T-03 chain-store I/O | S0 / UNBOUNDED | DECLARED-BOUNDED — wait timeout (TC-2) + chain-store CT + circuit breaker (TC-3) | TC-2 + TC-3 |
| **T-R-03** | T-09 retry/non-retry boundaries | S1 / governance | DECLARED-BOUNDED — folded into the canonical typed-refusal family expanded by TC-2 / TC-3 / TC-7 | (folded) |
| **T-R-04** | T-04 Postgres hot-path adapter token threading | S1 / UNBOUNDED | DECLARED-BOUNDED — four hot-path contracts + adapters threaded; empty-paren forms gone | TC-5 |
| **T-R-05** | T-05 projection handler contract cancellation | S1 / UNBOUNDED | DECLARED-BOUNDED — both contracts carry CT; worker forwards `stoppingToken`; `TodoProjectionHandler` threads CT into its DB calls | TC-6 |
| **T-R-06** | T-06 workflow step / execution timeout | S1 / UNBOUNDED | DECLARED-BOUNDED — `PerStepTimeoutMs` / `MaxExecutionMs` linked CTSs + typed `WorkflowTimeoutException` | TC-7 |
| **T-R-07** | T-07 workflow gate token-drop | S2 / ACCIDENTAL-BOUNDED | DECLARED-BOUNDED — both `AcquireAsync` call sites pass real CT | TC-8 |
| **T-R-08** | T-08 host shutdown drain | S2 / ACCIDENTAL-BOUNDED | DECLARED-BOUNDED — `Host:ShutdownTimeoutSeconds` configured + `HostShutdownLinkingMiddleware` links `ApplicationStopping` into the request CT path | TC-9 |
| **T-R-09** | structural restructuring of chain-anchor held section | S2 / governance | DECLARED-WAIVED — wait + holder both bounded; held-section I/O remains inside the permit by design | (waived, see §Residual) |

---

## Per-Patch Summaries

### TC-1 — Dispatcher / Middleware / Controller CancellationToken Contract Co-Evolution (T-R-01)

- **Files modified**: `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`, `src/runtime/dispatcher/SystemIntentDispatcher.cs`, `src/runtime/middleware/IMiddleware.cs`, all eight middleware implementations under `src/runtime/middleware/**`, `src/runtime/control-plane/RuntimeControlPlane.cs`, `src/runtime/pipeline/ExecutionPipeline.cs`, `src/platform/api/controllers/TodoController.cs`, `src/shared/contracts/runtime/ICommandDispatcher.cs`, `src/shared/contracts/runtime/ISystemIntentDispatcher.cs`.
- **Edit**: every contract on the request execution path now declares `CancellationToken cancellationToken = default`. The middleware pipeline closure shape becomes `Func<CancellationToken, Task<CommandResult>>` so each middleware forwards the token to `next(ct)`. `TodoController.Create/Update/Complete` bind `HttpContext.RequestAborted` via the standard ASP.NET model binder and forward into `ISystemIntentDispatcher.DispatchAsync(..., cancellationToken)`. Coordinated single-pass change so the solution remains green at every intermediate step.
- **Effect**: a real CancellationToken now flows from the request edge through the system intent dispatcher → control plane → middleware pipeline → command dispatcher → engine — closing T-R-01 and unblocking every subsequent §5.2.3 patch.
- **Build**: green.

### TC-2 — Chain-Anchor Wait Timeout + Typed Refusal (T-R-02 wait side)

- **Files added**: `src/shared/contracts/infrastructure/chain/ChainAnchorWaitTimeoutException.cs`, `src/platform/api/middleware/ChainAnchorWaitTimeoutExceptionHandler.cs`.
- **Files modified**: `src/shared/contracts/infrastructure/admission/ChainAnchorOptions.cs`, `src/runtime/event-fabric/ChainAnchorService.cs`, `src/runtime/event-fabric/IEventFabric.cs`, `src/runtime/event-fabric/EventFabric.cs`, `src/runtime/control-plane/RuntimeControlPlane.cs`, `src/platform/host/composition/runtime/RuntimeComposition.cs`, `src/platform/host/Program.cs`, `src/platform/host/appsettings.json`.
- **Edit**: `ChainAnchorOptions` extended with `WaitTimeoutMs` (default 5000) and `RetryAfterSeconds` (default 1). `ChainAnchorService.AnchorAsync` now accepts `CancellationToken`; the no-arg `_lock.WaitAsync()` is replaced with `_lock.WaitAsync(_options.WaitTimeoutMs, cancellationToken)`. The `wait_ms` histogram is recorded in a `finally` so the timeout path remains observable. `IEventFabric.ProcessAsync` / `ProcessAuditAsync` accept and forward CT; `RuntimeControlPlane` forwards `cancellationToken` into both fabric calls. `ChainAnchorWaitTimeoutException` carries `WaitTimeoutMs` + `RetryAfterSeconds` and is mapped to 503 + `Retry-After` by the new edge handler.
- **Effect**: the chain-anchor wait is no longer indefinite. Wait-timeout produces an explicit retryable refusal at the API edge.
- **Build**: green.

### TC-3 — Chain-Store I/O Cancellation + Breaker (T-R-02 holder side)

- **Files added**: `src/shared/contracts/infrastructure/chain/ChainAnchorUnavailableException.cs`, `src/platform/api/middleware/ChainAnchorUnavailableExceptionHandler.cs`.
- **Files modified**: `src/shared/contracts/infrastructure/chain/IChainAnchor.cs`, `src/shared/contracts/infrastructure/admission/ChainAnchorOptions.cs`, `src/platform/host/adapters/WhyceChainPostgresAdapter.cs`, `src/runtime/event-fabric/ChainAnchorService.cs`, `src/platform/host/composition/infrastructure/InfrastructureComposition.cs`, `src/platform/host/Program.cs`, `tests/integration/setup/InMemoryChainAnchor.cs`.
- **Edit**: `IChainAnchor.AnchorAsync` accepts `CancellationToken`; `WhyceChainPostgresAdapter` threads CT into both `ExecuteScalarAsync(ct)` and `ExecuteNonQueryAsync(ct)`. `ChainAnchorOptions` extended with `BreakerThreshold` (default 5) and `BreakerWindowSeconds` (default 10). The adapter implements a Closed/Open/HalfOpen breaker mirroring `OpaPolicyEvaluator` line-for-line, clocked via the runtime `IClock`. Pre-call breaker gate refuses immediately when Open. Caller-driven `OperationCanceledException` propagates as-is and does **not** advance the breaker. Any other exception → `RecordFailure()` + typed `ChainAnchorUnavailableException("transport", …)`. New `chain.store.failure{outcome}` counter on the existing `Whyce.Chain` meter.
- **Effect**: the chain-store I/O honors cancellation and refuses fast on transport failures. Together with TC-2, the chain-anchor failure family is closed end-to-end.
- **Build**: green.

### TC-5 — Postgres Adapter Token Threading (T-R-04)

- **Files modified**: `src/shared/contracts/infrastructure/persistence/IEventStore.cs`, `src/shared/contracts/infrastructure/messaging/IOutbox.cs`, `src/shared/contracts/infrastructure/persistence/IIdempotencyStore.cs`, `src/shared/contracts/infrastructure/persistence/ISequenceStore.cs`, `src/platform/host/adapters/PostgresEventStoreAdapter.cs`, `src/platform/host/adapters/PostgresOutboxAdapter.cs`, `src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs`, `src/platform/host/adapters/PostgresSequenceStoreAdapter.cs`, `src/runtime/event-fabric/EventStoreService.cs`, `src/runtime/event-fabric/OutboxService.cs`, `src/runtime/event-fabric/EventFabric.cs`, `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`, `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs`, plus four `tests/integration/setup/InMemory*.cs` fakes for compile-through.
- **Edit**: every method on the four hot-path contracts declares `CancellationToken cancellationToken = default`. Every `Postgres*Adapter` implementation threads CT into `BeginTransactionAsync(ct)`, `ExecuteReaderAsync(ct)`, `reader.ReadAsync(ct)`, advisory-lock `ExecuteNonQueryAsync(ct)`, `MAX(version)` `ExecuteScalarAsync(ct)`, multi-row INSERT `ExecuteNonQueryAsync(ct)`, and `tx.CommitAsync(ct)`. `EventStoreService` / `OutboxService` wrappers, `EventFabric.ProcessInternalAsync`, `RuntimeCommandDispatcher.ExecuteEngineAsync`, and `IdempotencyMiddleware.ExecuteAsync` forward CT end-to-end. The failure-branch `ReleaseAsync` calls deliberately use `CancellationToken.None` so rollback is not aborted by upstream cancellation.
- **Effect**: empty-paren `Execute*Async` forms at the four targeted hot-path adapters are gone. The token reaches the database round-trip from the request edge.
- **Build**: green.

### TC-6 — Projection-Handler Contract CancellationToken Extension (T-R-05)

- **Files modified**: `src/shared/contracts/projection/IEnvelopeProjectionHandler.cs`, `src/shared/contracts/infrastructure/projection/IProjectionHandler.cs`, `src/projections/operational-system/sandbox/todo/TodoProjectionHandler.cs`, `src/projections/orchestration-system/workflow/handler/WorkflowExecutionProjectionHandler.cs`, `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs`.
- **Edit**: both projection-handler contracts declare `CancellationToken cancellationToken = default`. `TodoProjectionHandler` threads CT through every per-type overload into `LoadAsync(id, ct)` → `ExecuteReaderAsync(ct)` / `reader.ReadAsync(ct)`, and `UpsertAsync(id, state, type, ct)` → `ExecuteNonQueryAsync(ct)`. `WorkflowExecutionProjectionHandler` accepts CT at every per-type overload (signature only — `IWorkflowExecutionProjectionStore` is not on the TC-6 target list). `GenericKafkaProjectionConsumerWorker` calls `handler.HandleAsync(envelope, stoppingToken)`. Sequential consume → handle → commit shape, DLQ routing, and PC-7 projection-lag recording all unchanged.
- **Effect**: a hung projection handler is unblocked at the database round-trip when the host begins to drain, instead of waiting for `MaxPollIntervalMs` / `SessionTimeoutMs` to intervene.
- **Build**: green.

### TC-8 — Workflow Gate Token Threading (T-R-07)

- **Files modified**: `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`.
- **Edit**: `ExecuteWorkflowAsync` and `ResumeWorkflowAsync` both extended to receive `CancellationToken` from `DispatchAsync` and to forward it into `_workflowAdmissionGate.AcquireAsync(workflowName, tenantId, cancellationToken)`. KC-6 lease lifecycle, refusal mapping, and per-workflow / per-tenant ceiling semantics unchanged.
- **Effect**: the workflow admission gate honors caller cancellation at both Start and Resume call sites. The single mechanical gap left over from TC-1's pipeline pass is closed.
- **Build**: green.

### TC-7 — Workflow Step / Execution Timeout (T-R-06)

- **Files added**: `src/shared/contracts/runtime/WorkflowTimeoutException.cs`, `src/platform/api/middleware/WorkflowTimeoutExceptionHandler.cs`.
- **Files modified**: `src/shared/contracts/runtime/IWorkflowStep.cs`, `src/shared/contracts/engine/IWorkflowEngine.cs`, `src/shared/contracts/infrastructure/admission/WorkflowOptions.cs`, `src/engines/T1M/workflow-engine/WorkflowEngine.cs`, `src/engines/T1M/step-executor/WorkflowStepExecutor.cs`, three `src/engines/T1M/steps/todo/*.cs` step implementations, `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`, `src/platform/host/composition/runtime/RuntimeComposition.cs`, `src/platform/host/Program.cs`, `src/platform/host/appsettings.json`, `tests/integration/setup/NoOpWorkflowStubs.cs`.
- **Edit**: `IWorkflowEngine.ExecuteAsync` and `IWorkflowStep.ExecuteAsync` both declare `CancellationToken`. `WorkflowOptions` extended with `PerStepTimeoutMs` (default 30000) and `MaxExecutionMs` (default 300000). `T1MWorkflowEngine` builds an execution-level linked CTS bounded by `MaxExecutionMs` and a per-step linked CTS bounded by `PerStepTimeoutMs`, both linked to the upstream request/host token. Cancellation discrimination inside the engine `catch`: caller cancellation propagates as `OperationCanceledException`; execution-deadline expiry throws `WorkflowTimeoutException("execution", null, …)`; per-step deadline expiry throws `WorkflowTimeoutException("step", stepName, …)`. `CreateTodoStep` actively forwards CT into `ISystemIntentDispatcher.DispatchAsync`; the other two in-tree steps accept CT at the signature level only (CPU-bound).
- **Effect**: workflow execution carries CT end-to-end and honors a declared two-tier timeout discipline. Timeout produces an explicit retryable refusal at the API edge via the new typed exception → 503 + `Retry-After` handler.
- **Build**: green.

### TC-9 — Host-Shutdown Drain Declared (T-R-08)

- **Files added**: `src/platform/api/middleware/HostShutdownLinkingMiddleware.cs`.
- **Files modified**: `src/platform/host/Program.cs`, `src/platform/host/appsettings.json`.
- **Edit**: `Host:ShutdownTimeoutSeconds` declared in `appsettings.json` (default 30 s) and bound in `Program.cs` via `services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(seconds))`. Validation rejects values < 1 with an explicit `InvalidOperationException`. New narrow ASP.NET middleware `HostShutdownLinkingMiddleware` replaces `HttpContext.RequestAborted` for the duration of every request with a linked CTS combining the original client-disconnect token and `IHostApplicationLifetime.ApplicationStopping`. Restored on exit; CTS disposed via `using`. Registered after `app.UseRateLimiter()` and before `app.UseExceptionHandler()` so the linked token is in effect for the entire downstream pipeline.
- **Effect**: the host-shutdown signal flows directly into the TC-1 request CT path, so every downstream seam (TC-2 / TC-3 / TC-5 / TC-6 / TC-7 / TC-8) automatically observes shutdown without a single contract change.
- **Build**: green.

---

## Resulting Classification Model Outcomes

### Now DECLARED-BOUNDED

| Surface | Bound | Observability | Patch |
|---|---|---|---|
| Controller → dispatcher → middleware → control plane → fabric → engine CT contract | Real CancellationToken from `HttpContext.RequestAborted` to every downstream seam | (token presence verified by signature inspection) | TC-1 |
| `ChainAnchorService._lock` wait | `ChainAnchorOptions.WaitTimeoutMs=5000` + `RetryAfterSeconds=1` | `Whyce.Chain.chain.anchor.wait_ms` recorded on every exit | TC-2 |
| Chain-store I/O (`WhyceChainPostgresAdapter`) | CT-threaded `Execute*Async(ct)` + `ChainAnchorOptions.{BreakerThreshold=5, BreakerWindowSeconds=10}` Closed/Open/HalfOpen breaker | `Whyce.Chain.chain.store.failure{outcome}` | TC-3 |
| Postgres hot-path adapter `Execute*Async` (event store, outbox, idempotency, sequence) | CT-threaded; empty-paren forms gone | (token presence verified by inspection) | TC-5 |
| Projection handler dispatch | CT-threaded; `TodoProjectionHandler` honors token at the DB round-trip | (token presence verified by inspection) | TC-6 |
| `WorkflowAdmissionGate.AcquireAsync` at dispatcher call sites | Real CT forwarded at both Start and Resume | (KC-6 metrics unchanged) | TC-8 |
| Workflow execution per-step + overall deadline | `WorkflowOptions.PerStepTimeoutMs=30000` + `MaxExecutionMs=300000` linked CTSs | (typed-refusal counter via API edge 503 telemetry) | TC-7 |
| Host-level graceful shutdown | `Host:ShutdownTimeoutSeconds=30` applied to `HostOptions.ShutdownTimeout`; `HostShutdownLinkingMiddleware` links `ApplicationStopping` into `HttpContext.RequestAborted` | (linked-token presence verified by inspection) | TC-9 |

### DECLARED-WAIVED

| Surface | Waiver Reason | Observability | Patch |
|---|---|---|---|
| `ChainAnchorService` held-section structural restructuring | Wait + holder both bounded by TC-2 / TC-3; moving the chain-store persist outside the held section, sharding by correlation hash, or replacing the global semaphore with a per-aggregate primitive remains a future structural workstream. PC-5 / KW-1 / TC-2 / TC-3 together provide the declared-bounded envelope and the operator signal. | `Whyce.Chain.{wait_ms, hold_ms, chain.store.failure}` | (waived) |
| `IWorkflowExecutionProjectionStore` token-threading | TC-6 contract carries CT through the per-type overloads but does not extend the store contract — store-side threading is a separate workstream. | (handler-side token verified by inspection) | (waived) |
| Workflow lifecycle event loss on declared timeout | Events accumulated in `context.AccumulatedEvents` up to the moment of timeout are not persisted (the throw bypasses the fabric persist → chain → outbox path). Consistent with the rest of the canonical refusal family. | (typed exception payload) | (waived) |
| `Workflow:MaxExecutionMs` resume budget | A resumed workflow gets a fresh execution-level budget — the prior failed run's wall-clock is not durably tracked. Out of TC-7 scope. | (none — declared simplification) | (waived) |

---

## Residual / Outside §5.2.3 Gate

| Item | Class | Reason for residual |
|---|---|---|
| Engine-side / workflow-step token observation in CPU-bound steps | (cooperative cancellation only) | The two CPU-bound in-tree steps accept CT at the signature level only; new steps must remember to honor the token at any cancellable await. There is no static check enforcing this. |
| Per-call Postgres command timeout (sub-`CommandTimeoutSeconds`) | (out of scope) | TC-3 / TC-5 thread CT but do not introduce a per-call DB timeout; reliance is on Npgsql `CommandTimeoutSeconds` from the chain pool to surface as a transport failure. |
| `ISequenceResolver` / HSID prelude token threading | (out of scope) | TC-5 widened the `ISequenceStore` contract but `PersistedSequenceResolver` and `RuntimeControlPlane.StampHsidAsync` still call `NextAsync(scope)` with default CT. Threading these requires changing the `ISequenceResolver` contract, which is explicitly out of TC-5 scope. |
| Background-worker shutdown linking | (correct by construction) | `BackgroundService` instances receive `stoppingToken` directly from the .NET host and do not transit `HostShutdownLinkingMiddleware`. TC-9 linking is request-scoped only. |
| Kafka producer timeout work | (out of scope) | The deadletter producer in `GenericKafkaProjectionConsumerWorker` does not have a declared per-publish timeout; it relies on librdkafka defaults. Out of §5.2.3 scope per opening pack §2.6. |
| Native Npgsql / Confluent.Kafka client counter bridging | (acceptable) | Same posture as §5.2.1 / §5.2.2 — explicit `Whyce.*` counters cover §5.3.x load-work needs. |

The residual list is preserved verbatim from the relevant Step C
PASS reports. None of the items above match an opening-pack §2.9
acceptance criterion that would block §5.2.3 PASS.

---

## Final Verification Evidence

- **Build**: `dotnet build` → 0 errors at the close of every Step C patch (TC-1 through TC-9) and at workstream closure.
- **Declared configuration blocks present in `appsettings.json` (delta over §5.2.2)**: `Host.ShutdownTimeoutSeconds`, `ChainAnchor.{WaitTimeoutMs, RetryAfterSeconds, BreakerThreshold, BreakerWindowSeconds}`, `Workflow.{PerStepTimeoutMs, MaxExecutionMs}`. Every key bound at the composition root and validated.
- **New typed exceptions and edge handlers (delta over §5.2.2)**:
  - `ChainAnchorWaitTimeoutException` → `ChainAnchorWaitTimeoutExceptionHandler` → 503 + `Retry-After` (TC-2)
  - `ChainAnchorUnavailableException` → `ChainAnchorUnavailableExceptionHandler` → 503 + `Retry-After` (TC-3)
  - `WorkflowTimeoutException` → `WorkflowTimeoutExceptionHandler` → 503 + `Retry-After` (TC-7)
  - **§5.2.3 final retryable-refusal edge inventory**: 429 intake (PC-1), 503 OPA (PC-2), 503 outbox (PC-3), 503 workflow saturation (KC-6), **503 chain-anchor wait timeout (TC-2)**, **503 chain-anchor unavailable (TC-3)**, **503 workflow timeout (TC-7)**, plus the pre-existing 409 concurrency-conflict REJECT — seven canonical retryable-refusal edges.
- **New meters (delta over §5.2.2)**: none. `chain.store.failure` rides the existing `Whyce.Chain` meter; TC-7 deliberately does not introduce a `workflow.timeout` counter (operators detect timeouts via API edge 503 telemetry).
- **Token-path coverage**: end-to-end CancellationToken now flows from `HttpContext.RequestAborted` (linked to `IHostApplicationLifetime.ApplicationStopping` via TC-9) through `ISystemIntentDispatcher` → `RuntimeControlPlane` → middleware pipeline → `ICommandDispatcher` → `ExecuteEngineAsync` / `ExecuteWorkflowAsync` → `IEventStore` / `IOutbox` / `IIdempotencyStore` / `IChainAnchor` / `IWorkflowEngine` → `IWorkflowStep` → database round-trip.
- **Empty-paren `Execute*Async` forms**: gone at every targeted adapter (chain, event-store, outbox, idempotency, sequence).
- **Linked CTS discipline**: every new linked CTS in TC-2 / TC-3 / TC-7 / TC-9 is built via `CancellationTokenSource.CreateLinkedTokenSource(...)` + (where applicable) `CancelAfter(...)` and disposed via `using`.
- **Cardinality discipline**: every new metric tag is low-cardinality (`outcome`). No `correlation_id`, `aggregate_id`, `event_id`, or per-workflow tags introduced.
- **Semantic preservation verified by inspection**: workflow ordering, lifecycle event meaning, replay semantics, KC-6 admission gate behavior, chain integrity / single-permit serialization, advisory-lock placement, idempotency claim/release semantics, transaction boundaries, DLQ routing, per-message Kafka commit, $8 policy-primacy, $9 determinism, chain head mutation order — all unchanged.

---

## Final Status Recommendation

**PASS (2026-04-08).**

All §5.2.3 acceptance criteria from the opening pack §2.9 are
satisfied:

1. ✅ Every timeout / cancellation / circuit-protection surface in §2.5 scope enumerated and classified (Step A inventory, 19 surfaces).
2. ✅ Every primitive has at least one reproducible probe (Step B T-Narrow + per-TC-* verification).
3. ✅ Every probe has reproducible evidence (per-Step C PASS reports).
4. ✅ Every probe result classified with one-line justification.
5. ✅ Every non-`DECLARED-BOUNDED` finding has S0–S3 severity per $16.
6. ✅ Every non-`DECLARED-BOUNDED` finding has a remediation patch list entry with externalized configuration shape — and TC-1..TC-9 delivered every entry on the §5.2.3 patch order.
7. ✅ The end-to-end cancellation model walks one accepted request from `HttpContext.RequestAborted` through every seam on the critical path and confirms a real token reaches the database round-trip.
8. ✅ `ChainAnchorService` wait + holder both have declared timeouts and CT threading via TC-2 + TC-3, with the structural restructuring deferral recorded honestly.
9. ✅ Postgres hot-path adapters (T-R-04) covered by TC-5 with the four target contracts and adapters threaded.
10. ✅ Projection-handler contracts (T-R-05) closed by TC-6.
11. ✅ Workflow execution (T-R-06) closed by TC-7 with declared per-step + overall timeouts and the canonical typed refusal.
12. ✅ Workflow gate token-drop (T-R-07) closed by TC-8.
13. ✅ Host-shutdown drain (T-R-08) closed by TC-9 with declared `HostOptions.ShutdownTimeout` and the linking middleware.
14. ✅ Every proposed patch declares its externalized configuration shape per the §5.2.1 / §5.2.2 precedent.
15. ✅ No remediation patch applied during the audit pass; opening pack discipline preserved through Step A and Step B; Step C patches each gated on green build.
16. ✅ No newly discovered guard rule required `claude/new-rules/` capture during this workstream.
17. ✅ Final specification (this document) returns explicit terminal status.
18. ✅ README §6.0 row 5.2.3 promoted on real state change, not by the opening pack.

§5.2.3 is **closed PASS**.
