# Phase 1.5 §5.2.3 — Timeout, Cancellation, and Circuit Protection (Workstream Opening Pack)

## TITLE
Phase 1.5 §5.2.3 Timeout, Cancellation, and Circuit Protection — canonical workstream opening pack.

## CLASSIFICATION
system / runtime / timeout-cancellation-circuit-protection

## CONTEXT
The Phase 1.5 §5.1.x structural hardening series and the first two
§5.2.x workstreams all closed PASS on 2026-04-08:

- §5.1.1 Dependency Graph Remediation — **PASS** (2026-04-08)
- §5.1.2 Boundary Purity Validation — **PASS** (2026-04-08)
- §5.1.3 Canonical Documentation Alignment — **PASS** (2026-04-08)
- §5.2.1 Admission Control and Backpressure — **PASS** (2026-04-08)
  ([20260408-220000-phase-1-5-5-2-1-pass-closure.md](20260408-220000-phase-1-5-5-2-1-pass-closure.md))
- §5.2.2 Concurrency Control and Resource Bounds — **PASS** (2026-04-08)
  ([20260408-235900-phase-1-5-5-2-2-pass-closure.md](20260408-235900-phase-1-5-5-2-2-pass-closure.md))

§5.2.1 established declared admission control and four canonical
RETRYABLE REFUSAL edges (intake 429, OPA 503, outbox 503, plus
the pre-existing concurrency-conflict 409 REJECT). §5.2.2
established declared concurrency primitives and resource ceilings
across the runtime hot path, added a fifth canonical RETRYABLE
REFUSAL edge (workflow 503), introduced eight declared
configuration blocks and eight canonical `Whyce.*` meters, and
explicitly deferred the chain-anchor structural restructuring and
broader cancellation co-evolution to **§5.2.3**.

The runtime now refuses safely under load (§5.2.1), processes
accepted work predictably (§5.2.2) — but **does not yet have
declared timeout, cancellation, or circuit-protection discipline
across most blocking seams**. Several known gaps were recorded
verbatim by the §5.2.1 / §5.2.2 closure notes:

- **`ChainAnchorService._lock` `WaitAsync()` is no-timeout, no-token.**
  KW-1 explicitly deferred adding a `WaitAsync(timeout, ct)` shape
  to §5.2.3 because the right answer requires a typed
  `ChainAnchorWaitTimeoutException` mapped to a refusal edge —
  i.e. timeout/cancellation co-evolution.
- **`PostgresEventStoreAdapter.AppendEventsAsync` advisory-lock**
  acquisition (`pg_advisory_xact_lock`) blocks inside Postgres
  with no client-side timeout — KC-5 made the wait observable but
  did not bound it.
- **`PostgresOutboxAdapter`, `OutboxDepthSampler`, `KafkaOutboxPublisher`,
  `PostgresIdempotencyStoreAdapter`, `PostgresSequenceStoreAdapter`,
  `PostgresProjectionWriter`, `WhyceChainPostgresAdapter`** all use
  `OpenInstrumentedAsync` which respects the declared
  `Postgres.Pools.*.ConnectionTimeoutSeconds` (PC-4) but most
  per-command SQL operations themselves do not pass a
  `CancellationToken` and rely on the declared `CommandTimeout=30s`
  as their only bound. Cancellation propagation is incomplete.
- **`OpaPolicyEvaluator` (PC-2)** is the only seam with a real
  circuit breaker. Other external dependencies (chain store,
  Kafka producer, Redis) have no breaker behavior.
- **`KafkaOutboxPublisher` `_producer.ProduceAsync`** (per outbox
  row) has no per-call timeout and no breaker. A broker-side
  hang produces a stuck publisher loop that the existing 1 s poll
  cadence cannot recover from.
- **`GenericKafkaProjectionConsumerWorker` `consumer.Consume(_pollTimeout)`**
  has a 1 s `pollTimeout` (PC-6 declared `MaxPollIntervalMs`) but
  the per-handler write call (`handler.HandleAsync(envelope)` /
  `_writer.WriteAsync(...)`) has no per-call timeout — a hung
  projection writer stalls the consume loop indefinitely until
  Kafka session timeout fires.
- **Workflow execution** (KC-6) has admission ceilings but no
  per-step timeout, no overall workflow-execution timeout, and
  no cancellation propagation from the API edge through the
  step executor.
- **`HttpClient` instances** are constructed by hand
  (`OpaPolicyEvaluator` uses `new HttpClient()` directly with no
  `Timeout` set — the per-call `CancellationTokenSource` from PC-2
  is the only bound). No `IHttpClientFactory` registration anywhere.
- **`ChainLock` was deleted** by KC-7, eliminating one stale
  cancellation surface, but `ChainAnchorService._lock` still
  blocks indefinitely on `WaitAsync()`.
- **`SemaphoreSlim.WaitAsync` waiter cancellation** is unhooked
  across the codebase — every waiter blocks until release, with
  no host-shutdown drain semantics for in-flight blocked callers.
- **`CancellationToken` flow through the dispatcher pipeline**
  is partial: `RuntimeCommandDispatcher.DispatchAsync(object,
  CommandContext)` does not currently take a `CancellationToken`,
  so HTTP-edge cancellation cannot propagate to the engine
  layer at all.
- **No canonical "circuit-open" gauge** exists for any seam other
  than OPA — operators cannot tell when a downstream is in the
  middle of a half-open / open transition without reading per-call
  metric counters.

§5.2.3 Timeout, Cancellation, and Circuit Protection is the
**third** workstream in the §5.2.x Runtime Infrastructure-Grade
Hardening cluster. Where §5.2.1 asked *"can the runtime refuse
work safely?"* and §5.2.2 asked *"under what concurrency model
does the runtime process accepted work?"*, §5.2.3 asks *"under
what time-bounded, cancellation-aware, fail-fast envelope does
the runtime interact with every blocking seam — and how does it
react when that envelope is exceeded?"*.

This workstream is the precondition for any §5.3.x workload that
runs longer than the existing test envelope: a 1k RPS soak that
exercises a 30-second window will eventually hit a hung adapter,
and without §5.2.3 the runtime has no canonical recovery posture
beyond "wait for the OS / pool / Kafka session timeout."

This artifact is the **opening pack only**. No remediation work
is performed here. No source, guard, audit, script, configuration,
or README file is modified. The workstream is created in `OPEN`
state and handed off for execution in subsequent prompts.

---

## 1. EXECUTIVE SUMMARY

§5.2.3 Timeout, Cancellation, and Circuit Protection verifies that
every blocking seam, external dependency call, lock wait, and
in-flight runtime path is **time-bounded, cancellation-aware, and
fail-fast in canonical observable ways** — rather than relying on
incidental library defaults, OS socket timeouts, or indefinite
waits. Where §5.2.1 added the four refusal edges and §5.2.2 added
declared concurrency ceilings, §5.2.3 fills in the third leg of
the runtime-hardening tripod: *what happens when accepted work
takes too long?*

The workstream produces, for every blocking seam in scope, an
evidence-backed determination of:

1. The **declared timeout** (or its absence) and where it is
   enforced.
2. The **cancellation source** — request cancellation token,
   host shutdown token, per-call CTS, or none — and the
   propagation chain from the API edge through the dispatcher
   to the seam.
3. The **failure shape on timeout** — typed exception, generic
   `OperationCanceledException`, swallowed log line, or
   indefinite hang — and whether the failure maps to a canonical
   refusal edge.
4. The **circuit-protection posture** — none, half-open / open
   breaker (PC-2 OPA pattern), or fail-fast on a different signal.
5. The **observability** — timeout counter, breaker-state gauge,
   cancellation counter — required to *prove* the envelope is in
   effect.

The deliverable is a runtime timeout / cancellation / circuit
specification, a remediation patch list against any seam that is
currently unbounded or that maps timeout to a non-canonical
shape, and a set of acceptance probes that future §5.3.x
throughput, soak, stress, and chaos workstreams can call as
preconditions. Where §5.2.1 produced four meters and four refusal
edges, and §5.2.2 produced two more meters and a fifth refusal
edge, §5.2.3 will extend the surface with a sixth canonical
declared options block (`Timeouts.*` or per-seam options) and the
matching observability.

The structural follow-on from KW-1 — restructuring
`ChainAnchorService._lock` so external I/O moves outside the held
section, or adding a `WaitAsync(timeout, ct)` shape — sits at the
center of §5.2.3 because it is the load-bearing example of the
"timeout co-evolves with cancellation co-evolves with refusal
class" pattern §5.2.3 needs to canonicalize.

Initial status: **OPEN**.

---

## 2. WORKSTREAM DEFINITION

### 2.1 Purpose
Ensure that runtime waits, external calls, blocking seams, and
retry/circuit boundaries are explicitly time-bounded,
cancellation-aware, and fail-fast in canonical, observable ways
rather than relying on incidental defaults, OS-level timeouts, or
indefinite waits.

### 2.2 Objective
Produce, for every blocking seam in scope, an evidence-backed
determination of:
1. The **declared timeout envelope** (per-call, per-batch, or
   per-host) and how it is enforced.
2. The **cancellation propagation chain** from the API edge
   (`HttpContext.RequestAborted`, `IHostApplicationLifetime.ApplicationStopping`)
   through the dispatcher pipeline to the seam.
3. The **typed failure mapping** on timeout — every timeout must
   produce either a canonical RETRYABLE REFUSAL edge (503 +
   `Retry-After`) or a declared cancellation acknowledgment;
   never a generic 500, never a silent hang.
4. The **circuit-protection shape** for external dependencies
   (chain store, Kafka, Redis) where appropriate, mirroring the
   PC-2 OPA breaker pattern.
5. The **gap** between the current implementation and the declared
   target, captured as a remediation patch list with severity per
   $16 and an acceptance probe per item.

### 2.3 Why This Matters Before Phase 2
- Phase 2 expansion will introduce real workload over real time
  windows. A runtime whose per-call latency is bounded only by
  "the OS will eventually time out the socket" will hang under
  any sustained downstream slowness — and the existing PC-2 OPA
  breaker is the only seam that fails fast today.
- §5.2.1 made the runtime *refuse safely*. §5.2.2 made it
  *process accepted work predictably*. §5.2.3 must make it *fail
  fast within a declared envelope*. Without §5.2.3, an accepted
  request whose chain-store persist hangs occupies a dispatcher
  slot, a Postgres pool slot, and a chain anchor permit
  indefinitely — undoing both prior workstreams' bounds.
- WHYCEPOLICY $8 + $9 require deterministic, policy-gated
  execution. A timeout that produces an `OperationCanceledException`
  with no canonical mapping is a non-deterministic failure shape:
  the same command can produce 500, 503, or a partial-success
  depending on which clock fires first. §5.2.3 must canonicalize
  the mapping.
- Future §5.3.x workstreams (throughput certification, 1k RPS
  soak, stress, chaos) **measure recovery** under timeout. A
  chaos test that introduces a 30-second downstream stall should
  produce a measurable burst of canonical refusals followed by a
  measurable recovery — not an indefinite host hang.
- §5.2.2 explicitly deferred the **`ChainAnchorService._lock`
  structural restructuring** (KW-1 declared waiver) and the
  **broader cancellation co-evolution** to §5.2.3. Both items are
  load-bearing: the chain anchor is the binding throughput
  constraint identified by Step B / P-K1 / PC-5, and cancellation
  propagation is the precondition for any host-shutdown drain
  posture.
- Determinism $9 + idempotency: a timeout must not corrupt
  partially-applied state. The advisory-lock + multi-row INSERT
  + outbox enqueue chain inside `AppendEventsAsync` is currently
  protected by Postgres transaction rollback, but a cancellation
  fired *between* `AppendEventsAsync` and the chain anchor would
  leave events persisted without an anchor — violating the
  $11 audit chain invariant. §5.2.3 must declare the
  cancellation seams that are safe and forbid the ones that are
  not.
- The §5.2.1 OPA breaker is a successful pattern. §5.2.3 must
  decide which other external dependencies merit the same
  treatment (likely: chain store, Kafka producer) and which can
  rely on declared timeouts alone.

### 2.4 Known Timeout / Cancellation / Circuit Risk Areas
- **T1** — `ChainAnchorService._lock.WaitAsync()`
  ([src/runtime/event-fabric/ChainAnchorService.cs](../../src/runtime/event-fabric/ChainAnchorService.cs))
  is called with no timeout and no `CancellationToken`. A hung
  external persist holds the lock; every subsequent caller
  blocks indefinitely. **Direct KW-1 follow-on.** The §5.2.2
  closure note explicitly identified §5.2.3 as the owner of
  this restructuring.
- **T2** — `pg_advisory_xact_lock` in `PostgresEventStoreAdapter.AppendEventsAsync`
  ([src/platform/host/adapters/PostgresEventStoreAdapter.cs](../../src/platform/host/adapters/PostgresEventStoreAdapter.cs))
  blocks inside Postgres. The PC-4 declared
  `Postgres.Pools.EventStore.CommandTimeoutSeconds=30` bounds the
  per-command wait, but there is no client-side `CancellationToken`
  threaded into `lockCmd.ExecuteNonQueryAsync()`. KC-5 made the
  wait observable; T2 makes it cancelable.
- **T3** — `KafkaOutboxPublisher._producer.ProduceAsync(topic, message, ct)`
  ([src/platform/host/adapters/KafkaOutboxPublisher.cs](../../src/platform/host/adapters/KafkaOutboxPublisher.cs))
  takes a `CancellationToken` (good) but the loop's only token
  is `stoppingToken` from `BackgroundService`. There is no
  per-publish timeout and no breaker. A broker-side hang produces
  a stuck publish that survives until host shutdown.
- **T4** — `GenericKafkaProjectionConsumerWorker` consume loop
  ([src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs](../../src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs))
  uses `consumer.Consume(_pollTimeout)` with `_pollTimeout = 1s`
  (good for the consume side), but `handler.HandleAsync(envelope)`
  and `_writer.WriteAsync(...)` are awaited unbounded — a hung
  projection writer stalls the loop until the PC-6 declared
  `MaxPollIntervalMs=300000` (5 minutes) fires and Kafka rebalances.
- **T5** — `OpaPolicyEvaluator` (PC-2 breaker exists, declared
  `OpaOptions.RequestTimeoutMs=250`). **Reference shape** for
  the §5.2.3 canonical pattern. No work needed here other than
  documenting it as the precedent.
- **T6** — `WhyceChainPostgresAdapter.AnchorAsync` and
  `GetPreviousBlockHashAsync`
  ([src/platform/host/adapters/WhyceChainPostgresAdapter.cs](../../src/platform/host/adapters/WhyceChainPostgresAdapter.cs))
  use `OpenInstrumentedAsync` with no per-call `CancellationToken`
  threading. PC-4 `Postgres.Pools.Chain.CommandTimeoutSeconds=30`
  is the only bound. **The chain persist is the I/O held inside
  T1's lock**, so T6 timeout behavior co-evolves with T1.
- **T7** — All other Postgres adapters
  (`PostgresEventStoreAdapter.LoadEventsAsync`,
  `PostgresOutboxAdapter.EnqueueAsync`,
  `PostgresIdempotencyStoreAdapter.{TryClaimAsync,ReleaseAsync,ExistsAsync,MarkAsync}`,
  `PostgresSequenceStoreAdapter.NextAsync`, `PostgresProjectionWriter.WriteAsync`,
  `OutboxDepthSampler.SampleOnceAsync`) — most do not currently
  thread a `CancellationToken` into `cmd.ExecuteNonQueryAsync()`
  / `ExecuteScalarAsync()` / `ExecuteReaderAsync()`. PC-4
  `CommandTimeoutSeconds=30` is the bound; cancellation propagation
  is the gap.
- **T8** — `RuntimeCommandDispatcher.DispatchAsync(object command,
  CommandContext context)` does not take a `CancellationToken`.
  The dispatcher pipeline is therefore *uncancelable* from the
  HTTP edge — `HttpContext.RequestAborted` cannot reach any
  middleware, the dispatcher, the engine, or the event fabric.
  This is the **single largest cancellation gap** in the
  runtime: a client that disconnects mid-request leaves the
  command running to completion.
- **T9** — `IMiddleware.ExecuteAsync(CommandContext context,
  object command, Func<Task<CommandResult>> next)` middleware
  contract does not include a `CancellationToken` parameter.
  Adding it requires extending the contract — touching every
  middleware in the locked pipeline order. T8 and T9 are
  coupled.
- **T10** — `WorkflowAdmissionGate.AcquireAsync(workflowName,
  tenantId, ct)` (KC-6) takes a `CancellationToken` (good).
  But `RuntimeCommandDispatcher.ExecuteWorkflowAsync` calls it
  with the default token — i.e. no cancellation. T10 is a
  one-line fix once T8 is in place.
- **T11** — `OutboxDepthSampler` periodic loop catches
  `OperationCanceledException` correctly on `stoppingToken` —
  this is a **good pattern** to mirror, not a gap.
- **T12** — `OpaPolicyEvaluator._httpClient` is constructed by
  hand (`new HttpClient()`) with no `IHttpClientFactory`. Per-call
  timeout is enforced by the PC-2 CTS. Other potential `HttpClient`
  consumers (none currently, but Phase 2 will add some) need a
  declared factory shape.
- **T13** — Redis (`StackExchangeRedisClient`,
  `IConnectionMultiplexer`) timeout / cancel behavior is opaque
  — uses StackExchange.Redis defaults. Not currently on the hot
  path (only consumed by `IRedisClient` in test/diagnostic
  surfaces) but should be declared if it appears on a Phase 2
  path.
- **T14** — Workflow per-step execution
  (`WorkflowStepExecutor` / `T1MWorkflowEngine`) has no per-step
  timeout, no overall workflow execution timeout, and no
  cancellation propagation. A workflow whose step hangs occupies
  its KC-6 admission slot indefinitely (the `WorkflowAdmissionLease`
  is held until `using` block exit, which only happens when
  `next()` returns).
- **T15** — Host-shutdown drain posture is undeclared. When the
  host receives `IHostApplicationLifetime.ApplicationStopping`,
  the in-flight commands continue to run; there is no graceful
  drain timeout and no forced cancellation after a deadline.
  The workflow gate, the chain anchor, and the Postgres pool
  all hold permits across shutdown.

### 2.5 Scope
- `src/runtime/event-fabric/ChainAnchorService.cs` — T1, T6
  co-evolution.
- `src/runtime/event-fabric/EventFabric.cs` — orchestrator
  cancellation propagation through the persist → chain → outbox
  pipeline.
- `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`,
  `src/runtime/dispatcher/SystemIntentDispatcher.cs` — T8 dispatcher
  signature change (with the contract co-evolution implications).
- `src/runtime/middleware/**` — T9 every middleware in the locked
  pipeline order, with attention to whether the contract change
  is mechanical (most are) or load-bearing (idempotency middleware
  needs cancellation in the rollback path).
- `src/runtime/control-plane/**` — `IRuntimeControlPlane`
  signature, `RuntimeControlPlaneBuilder`, the executing
  pipeline.
- `src/platform/host/adapters/PostgresEventStoreAdapter.cs` —
  T2 advisory lock + T7 cancellation threading on `LoadEventsAsync`
  and `AppendEventsAsync`.
- `src/platform/host/adapters/WhyceChainPostgresAdapter.cs` — T6
  cancellation threading + breaker decision.
- `src/platform/host/adapters/PostgresOutboxAdapter.cs`,
  `KafkaOutboxPublisher.cs`, `OutboxDepthSampler.cs` — T7 / T3
  cancellation + per-publish timeout + breaker decision.
- `src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs`,
  `PostgresSequenceStoreAdapter.cs`, `PostgresProjectionWriter.cs`
  — T7 cancellation threading on the hot-path adapters.
- `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs`
  — T4 per-handler timeout / cancellation.
- `src/platform/host/adapters/OpaPolicyEvaluator.cs` — **reference
  pattern**, no remediation needed.
- `src/runtime/dispatcher/WorkflowAdmissionGate.cs` — T10 token
  threading.
- T1M workflow engine surfaces (`Whyce.Engines.T1M.WorkflowEngine`,
  `WorkflowStepExecutor`) — T14 per-step / per-workflow timeout
  decision.
- `src/platform/host/Program.cs` — host-shutdown drain
  declaration (T15) and any new exception-handler registrations.
- `src/platform/api/middleware/**` — new typed-exception edge
  handlers for any new RETRYABLE REFUSAL classes that emerge.
- Configuration surfaces — likely a new `Timeouts.*` declared
  block (or per-seam extensions to existing options blocks).

### 2.6 Non-Scope
- §5.1.1 / §5.1.2 / §5.1.3 / §5.2.1 / §5.2.2 (closed)
  re-verification beyond confirming that §5.2.3 changes do not
  regress them.
- §5.2.4 (Health, Readiness, and Degraded Modes) — sibling §5.2.x
  workstream, opens after §5.2.3.
- §5.3.x throughput certification, soak, stress, chaos —
  consume §5.2.3 as a precondition but are not in scope here.
- §5.4.x security, §5.5.x governance.
- Generic performance tuning. §5.2.3 is about *fail-fast
  envelope*, not *fast-path latency*.
- Capacity planning, hardware sizing, autoscaling policy.
- Domain-layer changes. The domain layer has zero dependencies
  per $7 and is not a timeout surface.
- Engine-layer changes beyond `T1MWorkflowEngine` per-step
  timeout (T14) — which is in-engine but is a runtime hardening
  concern, not a domain logic change.
- Replacement of `SemaphoreSlim` with a different primitive type
  unless the Step C analysis demonstrates the substitution is
  the narrowest fix for an identified timeout.
- Redis hot-path work unless §5.2.3 surfaces a Redis path on
  the critical write path that does not exist today.
- Re-litigating any locked rule (DG-R5-EXCEPT-01, R-DOM-01,
  middleware order, etc.).

### 2.7 Remediation Strategy
1. **Inventory** — enumerate every blocking seam, external
   dependency call, lock wait, and per-call I/O site in scope.
   Classify each as **DECLARED-BOUNDED** (declared timeout +
   declared cancellation source + canonical refusal mapping +
   observability), **DECLARED-PARTIAL** (one or two of the four
   present), **INCIDENTAL** (only library / pool default
   bounds), or **UNBOUNDED** (no bound at all).
2. **Probe** — for each seam, define a reproducible probe
   answering: (a) declared timeout, (b) cancellation source,
   (c) failure shape on timeout, (d) breaker / fail-fast
   posture, (e) observability.
3. **Cancellation propagation map** — single end-to-end diagram
   from `HttpContext.RequestAborted` through every middleware,
   the dispatcher, the engine, the event fabric, every adapter,
   and out to each external dependency. Identify every break in
   the chain.
4. **Triage** — assign severity per $16: S0 system-breaking
   (indefinite hang on critical write path), S1 architectural
   (timeout exists but maps to non-canonical failure), S2
   structural (declared but non-cancelable), S3 cosmetic.
5. **Patch list** — non-`DECLARED-BOUNDED` findings become a
   remediation patch list with file paths, intended edit,
   externalized configuration, and acceptance probe per item.
   No inline edits during the audit pass itself.
6. **Contract co-evolution** — T8 + T9 + T10 + T14 form a
   coupled cluster (dispatcher signature + middleware contract +
   workflow gate token + per-step timeout). The Step C order
   must address them in the right sequence so the build stays
   green at each step.
7. **Specification** — produce the canonical Timeout, Cancellation,
   and Circuit Protection specification: per-seam declared
   timeout, cancellation source, refusal class, observability,
   and breaker posture.
8. **Promote** — execution and remediation occur in follow-up
   prompts; this opening pack ends at the patch-list and
   specification handoff.

### 2.8 Task Breakdown
- **T-A** Blocking-seam inventory — enumerate every site in
  §2.5 scope; initial-classify DECLARED-BOUNDED / DECLARED-PARTIAL
  / INCIDENTAL / UNBOUNDED.
- **T-B** Probe matrix — define probes per seam covering
  declared timeout, cancellation source, refusal mapping,
  breaker posture, and observability.
- **T-C** Probe execution — run the probe matrix against the
  current tree (static analysis, configuration inspection,
  targeted code reading); capture verbatim raw evidence.
- **T-D** Classification — DECLARED-BOUNDED / DECLARED-PARTIAL /
  INCIDENTAL / UNBOUNDED per probe with one-line justification.
- **T-E** Severity triage — S0/S1/S2/S3 for every
  non-`DECLARED-BOUNDED` finding per $16.
- **T-F** Cancellation propagation map — single end-to-end
  diagram from `HttpContext.RequestAborted` to every external
  dependency, with break-points marked.
- **T-G** Patch list — file path + intended edit + acceptance
  probe + externalized configuration shape per
  non-`DECLARED-BOUNDED` finding.
- **T-H** Dispatcher / middleware contract co-evolution proposal
  — T8 + T9 sequencing, with build-green checkpoints documented.
- **T-I** ChainAnchor restructuring proposal — KW-1 follow-on.
  The chain anchor is the load-bearing example of timeout +
  cancellation + refusal-edge co-evolution; the proposal must
  cover (a) typed `ChainAnchorWaitTimeoutException`, (b) refusal
  edge mapping (likely 503 + `Retry-After`), (c) whether to move
  external I/O outside the held section, (d) whether to introduce
  a breaker on the chain store, and (e) the declared
  configuration shape (`ChainAnchor.WaitTimeoutMs`,
  `ChainAnchor.PersistTimeoutMs`).
- **T-J** Per-adapter cancellation threading proposal — T7
  mechanical refactor across the Postgres adapters, with the
  declared `Postgres.Pools.*.CommandTimeoutSeconds` as the
  existing bound and the new
  `OpenInstrumentedAsync(..., ct)` overload as the propagation
  seam.
- **T-K** Kafka publisher / consumer timeout proposal — T3 + T4
  per-publish and per-handler timeouts, with the breaker
  decision for the producer side.
- **T-L** Workflow per-step / per-execution timeout proposal —
  T14, with the declared `WorkflowOptions.PerStepTimeoutMs` /
  `WorkflowOptions.MaxExecutionMs` shape and the cancellation
  propagation through `WorkflowExecutionContext`.
- **T-M** Host-shutdown drain proposal — T15, declared graceful
  drain timeout with forced cancellation after the deadline.
- **T-N** Final Timeout, Cancellation, and Circuit Protection
  specification — single artifact bundling inventory, probe
  matrix, raw evidence, classifications, severities, propagation
  map, patch list, all proposals (T-H..T-M), and explicit
  terminal status.

### 2.9 Acceptance Criteria
1. Every blocking seam, external dependency call, lock wait,
   and per-call I/O site in §2.5 scope is enumerated and
   initial-classified.
2. Every seam has at least one reproducible probe covering
   declared timeout, cancellation source, refusal mapping,
   breaker posture, and observability.
3. Every probe has reproducible evidence (command, file
   reference, or grep predicate + raw output) stored alongside
   the specification.
4. Every probe result is classified `DECLARED-BOUNDED` /
   `DECLARED-PARTIAL` / `INCIDENTAL` / `UNBOUNDED` with a
   one-line justification.
5. Every non-`DECLARED-BOUNDED` finding has S0–S3 severity per
   $16.
6. Every non-`DECLARED-BOUNDED` finding has a remediation patch
   list entry with file path, intended change, externalized
   configuration shape, and acceptance probe.
7. The end-to-end cancellation propagation map walks
   `HttpContext.RequestAborted` and `IHostApplicationLifetime.ApplicationStopping`
   through every middleware, the dispatcher, the engine, the
   event fabric, every adapter, and out to each external
   dependency. Every break-point is marked.
8. `ChainAnchorService._lock` (T1) has either (a) a structural
   restructuring + cancellation proposal closing the KW-1
   follow-on, or (b) an explicit declared waiver with reason —
   no silent deferral.
9. The dispatcher / middleware contract co-evolution (T8 + T9)
   has a sequenced patch order with build-green checkpoints,
   either in this opening pack's Step C order or as part of
   the final patch list.
10. Per-adapter cancellation threading (T7 / T-J) covers every
    Postgres adapter on the hot path with a declared shape.
11. Kafka producer / consumer timeout posture (T3 + T4 / T-K)
    is declared with per-call envelopes and a breaker decision.
12. Workflow per-step / per-execution timeout posture (T14 / T-L)
    is declared, including the cancellation propagation through
    the admission lease and the typed refusal class.
13. Host-shutdown drain posture (T15 / T-M) is declared with a
    graceful timeout and forced cancellation deadline.
14. Every newly proposed patch declares its externalized
    configuration shape per the §5.2.1 / §5.2.2 precedent
    (`*.Options` record, composition root binding,
    `appsettings.json` block).
15. Every newly proposed typed exception declares its API-edge
    refusal mapping (canonical RETRYABLE REFUSAL → 503 +
    `Retry-After`, or REJECT, or declared cancellation
    acknowledgment).
16. No remediation patch is applied during the audit pass;
    opening pack discipline is preserved until §5.2.3 advances
    out of the audit phase.
17. Any newly discovered guard rule or governance finding is
    captured under `claude/new-rules/` with the canonical
    5-field shape per $1c.
18. Final specification explicitly returns one of: `PASS`,
    `FAIL`, `PARTIAL`, `BLOCKED`, `WAIVED`, with the reason
    recorded.
19. The §5.2.3 row in README §6.0 is updated only when the
    workstream actually advances state — not by the opening
    pack itself.

### 2.10 Evidence Required
- Blocking-seam inventory table with initial classification.
- Probe matrix (probe ID, seam ID, risk ID T1–T15,
  command/predicate, expected `DECLARED-BOUNDED` shape).
- Raw probe output for every probe (verbatim).
- Classification table (probe ID → DECLARED-BOUNDED /
  DECLARED-PARTIAL / INCIDENTAL / UNBOUNDED + reason).
- Severity table (finding ID → S0/S1/S2/S3).
- End-to-end cancellation propagation map with break-points.
- Remediation patch list with externalized configuration shape
  per item.
- ChainAnchor structural + cancellation restructuring proposal
  (T-I, KW-1 follow-on).
- Dispatcher / middleware contract co-evolution proposal (T-H,
  T8 + T9).
- Per-adapter cancellation threading proposal (T-J, T7).
- Kafka timeout / breaker proposal (T-K, T3 + T4).
- Workflow per-step / per-execution timeout proposal (T-L, T14).
- Host-shutdown drain proposal (T-M, T15).
- New-rules capture file (if any).
- Final Timeout, Cancellation, and Circuit Protection
  specification with explicit terminal status.

---

## 3. TRACKING TABLE

| Field | Value |
|---|---|
| **ID** | 5.2.3 |
| **Topic** | Timeout, Cancellation, and Circuit Protection |
| **Objective** | Ensure runtime waits, external calls, blocking seams, and retry/circuit boundaries are explicitly time-bounded, cancellation-aware, and fail-fast in canonical observable ways. For every seam in scope, declare the timeout envelope, cancellation source, typed failure mapping, breaker posture, and observability. Resolve the KW-1 chain-anchor follow-on by declaring the timeout + cancellation + refusal-edge structural shape. Close the dispatcher / middleware contract gap that currently makes the entire pipeline uncancelable from the API edge. |
| **Tasks** | T-A Inventory · T-B Probe matrix · T-C Probe execution · T-D Classification · T-E Severity triage · T-F Cancellation propagation map · T-G Patch list · T-H Dispatcher/middleware co-evolution · T-I ChainAnchor restructuring · T-J Per-adapter cancellation threading · T-K Kafka timeout/breaker · T-L Workflow per-step timeout · T-M Host-shutdown drain · T-N Final specification |
| **Deliverables** | Blocking-seam inventory · Probe matrix · Raw probe evidence · Classification table · Severity table · Cancellation propagation map · Remediation patch list · ChainAnchor restructuring proposal · Dispatcher/middleware contract co-evolution proposal · Per-adapter cancellation threading proposal · Kafka timeout/breaker proposal · Workflow per-step timeout proposal · Host-shutdown drain proposal · New-rules capture (if any) · Final Timeout, Cancellation, and Circuit Protection specification |
| **Evidence Required** | Reproducible probe (command / file ref / grep predicate) and raw output for every seam in §2.5; declared timeout, cancellation source, typed failure mapping, breaker posture, and observability per seam; cancellation propagation map with all break-points marked; classification + severity for every finding; explicit terminal status (PASS/FAIL/PARTIAL/BLOCKED/WAIVED) |
| **Status** | OPEN (NOT STARTED — workstream defined, no execution yet) |
| **Risk** | HIGH — every Phase 1.5 §5.3.x throughput / soak / stress / chaos workstream is gated on §5.2.3 in addition to §5.2.1 and §5.2.2. T1 (`ChainAnchorService._lock` no-timeout `WaitAsync`) and T8 (uncancelable dispatcher) are S0 candidates: a single hung downstream + a client disconnect today produces unbounded server-side execution. The KW-1 chain-anchor follow-on is the canonical example of how all four `*.Options` axes (timeout, cancellation, breaker, refusal) compose. |
| **Blockers** | None known. §5.1.1, §5.1.2, §5.1.3, §5.2.1, §5.2.2 prerequisites all satisfied 2026-04-08. The dispatcher/middleware contract change (T8 + T9) requires careful sequencing but does not require any external coordination. |
| **Owner** | Whycespace runtime / operational hardening track |
| **Notes** | Opening pack only. No remediation in this prompt. The §5.2.2 PASS report records KW-1 (chain-anchor structural restructuring), broader cancellation propagation, and `LoadEventsAsync` streaming as deferred items; §5.2.3 is the canonical owner of the first two (LoadEventsAsync streaming was waived under KC-8 and remains a separate future structural workstream). The PC-2 OPA breaker is the **reference pattern** for timeout + breaker + typed refusal — every §5.2.3 patch should mirror its shape. The phase1.5-§5.2.x options precedent (`Intake.*`, `Opa.*`, `Outbox.*`, `Postgres.Pools.*`, `KafkaConsumer.*`, `Workflow.*`, `ChainAnchor.*`) is the canonical config shape for any new declared timeout block. The phase1-gate-api-edge `IExceptionHandler` precedent is the canonical refusal-edge shape if §5.2.3 introduces any new typed-exception RETRYABLE REFUSAL surface. Continuity with §5.1.x, §5.2.1, and §5.2.2 (all PASS 2026-04-08) preserved. §5.2.3 is the **third** workstream in the §5.2.x cluster; §5.2.4 (Health, Readiness, and Degraded Modes) follows. |

**Status legend:** NOT STARTED · IN PROGRESS · PARTIAL · BLOCKED · PASS · FAIL · WAIVED.

---

## 4. ACCEPTANCE CRITERIA
(See §2.9 above. Reproduced here for tracking convenience.)

1. Every in-scope blocking seam enumerated and initial-classified.
2. Every seam has ≥1 reproducible probe covering timeout, cancellation, refusal mapping, breaker, and observability.
3. Every probe has reproducible raw evidence.
4. Every probe result classified DECLARED-BOUNDED / DECLARED-PARTIAL / INCIDENTAL / UNBOUNDED with reason.
5. Every non-DECLARED-BOUNDED finding has S0–S3 severity.
6. Every non-DECLARED-BOUNDED finding has a remediation patch list entry with externalized configuration shape and acceptance probe.
7. End-to-end cancellation propagation map produced; every break-point marked.
8. `ChainAnchorService._lock` (T1) has restructuring proposal or declared waiver — KW-1 follow-on resolved.
9. Dispatcher / middleware contract co-evolution (T8 + T9) sequenced.
10. Per-adapter cancellation threading (T7 / T-J) covers every Postgres adapter on the hot path.
11. Kafka producer / consumer timeout (T3 + T4 / T-K) declared with breaker decision.
12. Workflow per-step / per-execution timeout (T14 / T-L) declared.
13. Host-shutdown drain (T15 / T-M) declared with graceful + forced deadlines.
14. Every proposed patch declares its externalized configuration shape per the §5.2.1 / §5.2.2 precedent.
15. Every proposed typed exception declares its API-edge refusal mapping.
16. No remediation applied during audit pass.
17. Any newly discovered guard rule captured under `claude/new-rules/`.
18. Final specification returns explicit terminal status.
19. README §6.0 row 5.2.3 updated only on real state change.

---

## 5. REQUIRED ARTIFACTS

- `claude/project-prompts/20260409-000000-phase-1-5-5-2-3-timeout-cancellation-and-circuit-protection-open.md`
  — this opening pack.
- `claude/audits/timeout-cancellation-circuit-protection.audit.md`
  — to be created during T-N (final specification). Not created
  by this opening pack.
- `claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md` — to be created
  during T-D, T-H, T-I, T-J, T-K, T-L, or T-M if and only if
  newly discovered governance rules emerge.
- README §5.2.3 (existing, currently `NOT STARTED`) — unchanged
  by this opening pack; the workstream definition is anchored
  there, but state promotion is gated on real execution.
- README §6.0 master tracking table row 5.2.3 — unchanged by
  this opening pack.

---

## 6. CLAUDE EXECUTION PROMPT

> **Use this prompt to execute §5.2.3 in a follow-up session. Do
> not execute it as part of this opening pack.**

```
Phase 1.5 §5.2.3 — Timeout, Cancellation, and Circuit Protection (Execution Pass)

CLASSIFICATION: system / runtime / timeout-cancellation-circuit-protection
CONTEXT:
  §5.1.1 PASS (2026-04-08); §5.1.2 PASS (2026-04-08); §5.1.3 PASS (2026-04-08);
  §5.2.1 PASS (2026-04-08); §5.2.2 PASS (2026-04-08).
  Opening pack:
  claude/project-prompts/20260409-000000-phase-1-5-5-2-3-timeout-cancellation-and-circuit-protection-open.md

OBJECTIVE: Execute T-A through T-N of §5.2.3 as defined in the opening
  pack. Produce
  claude/audits/timeout-cancellation-circuit-protection.audit.md as
  the single consolidated deliverable. Do not modify source, guards,
  scripts, configuration, or README outside the audit artifact and
  (if needed) one or more claude/new-rules/ capture files.

CONSTRAINTS:
  - WBSM v3 canonical execution rules ($1–$16) apply in full.
  - Pre-execution: load every guard in claude/guards/ ($1a). No skip,
    no cache, no summary.
  - Post-execution: run every audit in claude/audits/ ($1b). Inline-fix
    any drift discovered against the audit artifact itself before
    completion.
  - Anti-drift ($5): no architecture changes, no renames, no file
    moves, no inference of missing components.
  - File system ($6): only operate in /src, /infrastructure, /tests,
    /docs, /scripts, /claude.
  - Layer purity ($7): domain unchanged; only the runtime, host
    adapter, api middleware, and engine T1M layers are in scope for
    restructuring proposals.
  - Policy ($8): WHYCEPOLICY $8 evaluation order is preserved by
    every proposed patch.
  - Determinism ($9): every proposed timeout / cancellation primitive
    must be compatible with IClock-based time and deterministic IDs.
  - No remediation patches applied; produce the patch list and
    proposals only.
  - No generic performance tuning; this workstream is about declared
    fail-fast envelope, not fast-path latency.
  - Any newly discovered guard rule → claude/new-rules/ per $1c.
  - Risk areas: T1–T15 from §2.4 of the opening pack.

EXECUTION STEPS:
  1. T-A Inventory — enumerate every blocking seam, external dep
     call, lock wait, and per-call I/O site in §2.5 scope and
     initial-classify DECLARED-BOUNDED / DECLARED-PARTIAL /
     INCIDENTAL / UNBOUNDED.
  2. T-B Probe matrix — define probes per seam covering declared
     timeout, cancellation source, refusal mapping, breaker posture,
     and observability.
  3. T-C Probe execution — run every probe (static analysis,
     configuration inspection, targeted code reading); capture
     verbatim raw evidence.
  4. T-D Classification — DECLARED-BOUNDED / DECLARED-PARTIAL /
     INCIDENTAL / UNBOUNDED per probe with one-line justification.
  5. T-E Severity triage — S0/S1/S2/S3 for every non-DECLARED-BOUNDED
     finding per $16.
  6. T-F Cancellation propagation map — single end-to-end diagram
     from HttpContext.RequestAborted and ApplicationStopping through
     every middleware, dispatcher, engine, fabric, and adapter, with
     every break-point marked.
  7. T-G Patch list — file path + intended edit + acceptance probe +
     externalized configuration shape per non-DECLARED-BOUNDED
     finding. Follow the §5.2.1 / §5.2.2 PC-* / KC-* precedent.
  8. T-H Dispatcher / middleware contract co-evolution proposal —
     T8 + T9 sequencing, with build-green checkpoints documented.
  9. T-I ChainAnchor restructuring proposal — declared timeout +
     cancellation token + typed ChainAnchorWaitTimeoutException +
     refusal edge mapping. Closes the KW-1 declared waiver.
 10. T-J Per-adapter cancellation threading proposal — T7 mechanical
     refactor across every Postgres adapter on the hot path.
 11. T-K Kafka publisher / consumer timeout proposal — T3 + T4
     per-publish and per-handler timeouts, with the breaker decision
     for the producer side.
 12. T-L Workflow per-step / per-execution timeout proposal — T14
     declared options + cancellation propagation through the
     admission lease + typed refusal.
 13. T-M Host-shutdown drain proposal — T15 declared graceful +
     forced cancellation deadlines.
 14. T-N Final specification — write
     claude/audits/timeout-cancellation-circuit-protection.audit.md
     bundling inventory, probe matrix, raw evidence, classifications,
     severities, propagation map, patch list, all six proposals
     (T-H..T-M), and explicit terminal status.

OUTPUT FORMAT:
  - Single audit artifact:
    claude/audits/timeout-cancellation-circuit-protection.audit.md
  - Optional: claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md
  - Structured failure report on any halt ($12: STATUS / STAGE /
    REASON / ACTION_REQUIRED).

VALIDATION CRITERIA:
  - All nineteen acceptance criteria from §2.9 / §4 satisfied.
  - Terminal status one of: PASS / FAIL / PARTIAL / BLOCKED / WAIVED.
  - Audit sweep ($1b) clean against the produced artifact.
  - No source/guard/script/configuration/README modification outside
    the explicitly named artifacts.
```

---

## 7. INITIAL STATUS

**OPEN** — workstream defined, tracked, and ready for execution.
No remediation performed. No source, guard, audit, script,
configuration, or README file modified by this opening pack.
§5.2.3 enters the Phase 1.5 work queue as the **third** workstream
in the §5.2.x Runtime Infrastructure-Grade Hardening cluster,
immediately following §5.2.2 PASS (2026-04-08), and is the
canonical owner of the KW-1 chain-anchor structural restructuring
follow-on, the broader cancellation propagation gap (T8 + T9),
and the per-seam timeout / breaker discipline that completes the
§5.2.x runtime-hardening tripod (admission + concurrency +
fail-fast envelope). §5.2.4 (Health, Readiness, and Degraded
Modes) follows §5.2.3 as the fourth workstream in the §5.2.x
cluster.
