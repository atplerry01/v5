# Phase 1.5 §5.2.2 — Concurrency Control and Resource Bounds (Workstream Opening Pack)

## TITLE
Phase 1.5 §5.2.2 Concurrency Control and Resource Bounds — canonical workstream opening pack.

## CLASSIFICATION
system / runtime / concurrency-control-resource-bounds

## CONTEXT
The Phase 1.5 §5.1.x structural hardening series and the first §5.2.x
workstream all closed PASS on 2026-04-08:

- §5.1.1 Dependency Graph Remediation — **PASS** (2026-04-08)
- §5.1.2 Boundary Purity Validation — **PASS** (2026-04-08)
- §5.1.3 Canonical Documentation Alignment — **PASS** (2026-04-08)
- §5.2.1 Admission Control and Backpressure — **PASS** (2026-04-08)
  ([20260408-220000-phase-1-5-5-2-1-pass-closure.md](20260408-220000-phase-1-5-5-2-1-pass-closure.md))

§5.2.1 established declared admission control at the runtime HTTP
edge and bounded refusal behavior at the three S0 chokepoints
(intake, OPA, outbox). It deliberately did **not** restructure any
shared lock, did not bound in-flight workflow concurrency, did not
shed Postgres connection pool exhaustion as a typed refusal, and
left several known concurrency/resource items as explicit residual:

- **`ChainAnchorService` global semaphore** is now observable
  (PC-5 `chain.anchor.{wait_ms,hold_ms}`) but still serializes the
  entire commit path through a single permit with an unbounded
  waiter queue and external I/O inside the held section.
- **Projections-side `NpgsqlConnection`** is still constructed
  per-call by `PostgresProjectionWriter` (and `TodoController.Get`)
  outside the declared `event-store` / `chain` pool seams that PC-4
  introduced.
- **DLQ depth bounds** (R-08) — neither the consumer-side DLQ
  publish path nor the outbox-side DLQ publish path has any growth
  bound, and no `dlq.depth` gauge exists.
- **In-flight workflow concurrency** is unbounded — there is no
  per-workflow-name or per-tenant cap on concurrent
  `WorkflowStartCommand` execution.
- **`LoadEventsAsync` reads the entire event stream into a `List<object>`**
  per command (R-09) — bounded only by aggregate lifetime, not by a
  declared limit.
- **`ChainLock` engine-internal `SemaphoreSlim(1,1)`** in
  `Whyce.Engines.T0U.WhyceChain` is a second global serial point
  feeding `ChainAnchorService` and was not analyzed by §5.2.1.
- **Idempotency-store connection amplification** — every command
  acquires two extra Npgsql connections through
  `PostgresIdempotencyStoreAdapter` (one per `ExistsAsync`, one per
  `MarkAsync`). PC-4 moved them onto the declared `event-store`
  pool but did not bound or coalesce them.

§5.2.2 Concurrency Control and Resource Bounds is the **second**
workstream in the §5.2.x Runtime Infrastructure-Grade Hardening
cluster. Where §5.2.1 asked *"can the runtime refuse work safely
when overloaded?"*, §5.2.2 asks *"is every concurrency limit and
resource ceiling in the runtime declared, bounded, fair, and
observable — or is the system serializing accidentally on a hidden
single-permit lock?"*.

This workstream is the precondition for any §5.3.x throughput / soak
/ stress / chaos certification that wants to *interpret* its
results: §5.2.1 made the system *refuse safely*; §5.2.2 makes the
system *behave predictably under sustained pressure*. A load test
against an accidentally-serialized chokepoint reports the chokepoint,
not the throughput envelope.

This artifact is the **opening pack only**. No remediation work is
performed here. No source, guard, audit, script, configuration, or
README file is modified. The workstream is created in `OPEN` state
and handed off for execution in subsequent prompts.

---

## 1. EXECUTIVE SUMMARY

§5.2.2 Concurrency Control and Resource Bounds verifies that every
concurrency primitive, shared lock, in-flight ceiling, and resource
budget on the runtime hot path is **declared, bounded, fair, and
observable**. Where §5.2.1 asked *"can the runtime refuse?"*,
§5.2.2 asks *"under what concurrency model does the runtime
process accepted work?"*.

The workstream produces, for every concurrency primitive and
resource ceiling in scope, an evidence-backed determination of:

1. The **declared in-flight ceiling** (or its absence) and where it
   is enforced.
2. The **fairness model** — FIFO / LIFO / random / accidental — and
   whether unbounded waiter queues exist behind any single-permit
   lock.
3. The **resource ownership** — which logical pool / lock / counter
   bounds the resource and which subsystem is responsible for
   sizing it.
4. The **coupling to admission control** — does §5.2.1's intake
   ceiling propagate consistently to every downstream concurrency
   limit, or does an accepted request hit a hidden serial point?
5. The **observability surface** — wait-time histograms, queue-depth
   gauges, contention counters — required to *prove* the bound is
   in effect during a future §5.3.x soak.

The deliverable is a runtime concurrency / resource-bound
specification, a remediation patch list against any primitive that
is currently unbounded, accidentally serialized, or invisible, and
a set of acceptance probes that future §5.3.x throughput, soak, and
stress workstreams can call as preconditions. Where §5.2.1 produced
seven `Whyce.*` meters and five `*.Options` configuration blocks,
§5.2.2 will extend that surface with declared concurrency primitives
(per-workflow-name caps, projections-pool, restructured chain
anchor, bounded idempotency batching) and the matching observability.

Initial status: **OPEN**.

---

## 2. WORKSTREAM DEFINITION

### 2.1 Purpose
Ensure that runtime concurrency, shared locks, resource ceilings,
and in-flight work limits are explicitly bounded, measurable, and
canonically controlled rather than emerging accidentally from
library defaults, single-process locks, or hidden contention.

### 2.2 Objective
Produce, for every concurrency primitive and resource ceiling in
scope, an evidence-backed determination of:
1. The **declared in-flight bound** and how it is enforced.
2. The **fairness shape** of the underlying primitive (FIFO,
   accidental, etc.) and whether any unbounded waiter queue exists
   behind a single-permit lock.
3. The **resource ownership** — which logical pool, lock, or
   counter bounds the resource.
4. The **coupling to §5.2.1 admission control** — every accepted
   request must reach a downstream concurrency model that is
   consistent with the intake ceiling.
5. The **gap** between the current implementation and the declared
   target, captured as a remediation patch list with severity per
   $16 and an acceptance probe per item.

### 2.3 Why This Matters Before Phase 2
- Phase 2 expansion will introduce real workload at sustained RPS.
  A runtime whose binding constraint is an accidentally-serialized
  global semaphore will collapse under that workload in ways that
  look like application bugs rather than capacity bugs.
- §5.2.1 made the runtime *refuse safely*. §5.2.2 must make the
  runtime *process accepted work predictably*. Without §5.2.2, the
  intake limiter sized at 256 concurrent requests admits 256
  requests that all queue on a single chain-anchor permit.
- WHYCEPOLICY $8 + $9 require deterministic, policy-gated execution.
  Accidental serialization on shared locks is *non-deterministic
  contention* — two requests that observe different wait times for
  the same lock observe different total latencies, and a sufficiently
  pathological schedule violates throughput predictability the
  $9 envelope assumes.
- Future §5.3.x workstreams (throughput certification, 1k RPS soak,
  stress, chaos) **measure noise** until concurrency primitives are
  declared. A throughput run against `ChainAnchorService` in its
  current shape reports the lock's serial throughput, not the
  runtime's capacity envelope.
- §5.2.1 closed three S0 risks but explicitly left **R-03 structural
  half**, **R-04 projections half**, **R-08 (DLQ depth)**, and
  **R-09 (`LoadEventsAsync` unbounded list)** as residual. §5.2.2
  is their canonical owner — they cannot be deferred indefinitely
  without violating the §5.2.x cluster contract.
- Determinism $9 + idempotency: the
  `PostgresIdempotencyStoreAdapter` 2× connection amplification per
  command interacts with declared pool sizing in ways that should
  be measured, not inferred. A hidden 5× pool consumption per
  command turns the pool ceiling into one-fifth of its declared
  value at every concurrency level.

### 2.4 Known Concurrency / Resource-Bound Risk Areas
- **C1** — `ChainAnchorService._lock = SemaphoreSlim(1,1)`
  ([src/runtime/event-fabric/ChainAnchorService.cs:22](../../src/runtime/event-fabric/ChainAnchorService.cs#L22))
  serializes the entire commit path through a single permit. PC-5
  made it observable (`chain.anchor.{wait_ms,hold_ms}`) but the
  held section still spans `_chainEngine.Anchor` + external
  `_chainAnchor.AnchorAsync`. **Binding constraint on §5.3.x
  throughput.** Waiter queue is unbounded by `SemaphoreSlim` API.
- **C2** — `Whyce.Engines.T0U.WhyceChain.Lock.ChainLock._semaphore`
  ([src/engines/T0U/whycechain/lock/ChainLock.cs:9](../../src/engines/T0U/whycechain/lock/ChainLock.cs#L9))
  is a second global serial point inside the engine layer that
  feeds `ChainAnchorService`. Was not analyzed by §5.2.1.
- **C3** — `RuntimeCommandDispatcher` workflow execution
  ([src/runtime/dispatcher/RuntimeCommandDispatcher.cs](../../src/runtime/dispatcher/RuntimeCommandDispatcher.cs))
  has no per-workflow-name or per-tenant in-flight cap. A burst of
  `WorkflowStartCommand` for the same workflow runs concurrently
  up to whatever the dispatcher thread pool will admit.
- **C4** — `RuntimeCommandDispatcher.ExecuteEngineAsync` engine
  resolution and `LoadEventsAsync` happen with no concurrency
  primitive — bounded only by `IntakeOptions.GlobalConcurrency`
  upstream. There is no per-aggregate in-flight cap (relevant when
  multiple commands target the same hot aggregate).
- **C5** — `PostgresEventStoreAdapter.AppendEventsAsync`
  per-aggregate `pg_advisory_xact_lock`
  ([src/platform/host/adapters/PostgresEventStoreAdapter.cs:66](../../src/platform/host/adapters/PostgresEventStoreAdapter.cs#L66))
  serializes per-aggregate writes correctly, but the **waiter
  queue is unbounded** — arbitrary numbers of in-flight
  `AppendEventsAsync` calls can stack against the same advisory
  lock with no shedding and no waiter visibility.
- **C6** — `PostgresIdempotencyStoreAdapter` 2× connection
  amplification per command (`ExistsAsync` + `MarkAsync`,
  [src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs](../../src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs)).
  PC-4 moved both onto the declared `event-store` pool but did not
  coalesce them; effective per-command pool consumption is ~5×
  rather than 1×, turning a 32-permit pool into ~6 effective
  in-flight commands.
- **C7** — `PostgresProjectionWriter` raw `NpgsqlConnection` per
  call ([src/platform/host/adapters/PostgresProjectionWriter.cs](../../src/platform/host/adapters/PostgresProjectionWriter.cs))
  — the PC-4 residual. Constructed by domain bootstrap modules
  (e.g. [src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs](../../src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs))
  with a connection string, not a declared `NpgsqlDataSource`.
  Pool sizing for the projections database is undeclared and
  unobservable.
- **C8** — `TodoController.Get` direct `NpgsqlConnection`
  ([src/platform/api/controllers/TodoController.cs:64](../../src/platform/api/controllers/TodoController.cs#L64))
  — controller-side direct read against the projections connection
  string, same residual class as C7.
- **C9** — DLQ growth bounds (R-08) — neither
  `GenericKafkaProjectionConsumerWorker.PublishToDeadletterAsync`
  nor `KafkaOutboxPublisher.TryPublishToDeadletterAsync` has any
  cap on DLQ topic growth or DB-side `'deadletter'` row growth. A
  poison-message storm or a sustained broker outage produces
  unbounded DLQ accumulation.
- **C10** — `LoadEventsAsync` reads the entire event stream into a
  `List<object>` per command (R-09,
  [src/platform/host/adapters/PostgresEventStoreAdapter.cs:29](../../src/platform/host/adapters/PostgresEventStoreAdapter.cs#L29)).
  Memory bound is the aggregate lifetime — latent at current
  scale, load-bearing at Phase 2 scale.
- **C11** — `KafkaOutboxPublisher` is registered as a single-instance
  `BackgroundService`. The drain rate ceiling (`LIMIT 100` per 1 s
  poll = ~100 rows/s per instance) is therefore *also* the global
  drain rate ceiling. There is no declared multi-instance shape.
- **C12** — Sequential `GenericKafkaProjectionConsumerWorker` —
  in-flight=1 per worker is correct for ordering but is also the
  global per-topic projection throughput ceiling. There is no
  declared multi-worker / partition-keyed parallelism shape.
- **C13** — Implicit fairness on every `SemaphoreSlim`. The
  `SemaphoreSlim` API guarantees FIFO release order *per release
  call*, but the kernel-level scheduler determines waiter wakeup;
  in practice this is FIFO-ish but not strictly bounded. No
  per-tenant fairness anywhere.
- **C14** — Coupling between `IntakeOptions.GlobalConcurrency`
  (256), `Postgres.Pools.EventStore.MaxPoolSize` (32), and the
  `~5× per command amplification (C6)`. The declared admission
  ceiling admits 256 in-flight commands against a pool that
  realistically supports ~6 — i.e. **the intake limit is 40×
  larger than the effective downstream capacity**. Either the
  intake limit is too high, or the pool is too small, or the
  amplification needs to be coalesced. §5.2.2 must declare which.
- **C15** — Hidden contention on shared mutable runtime state
  inside `ChainAnchorService` (`_lastBlockHash`, `_lastSequence`)
  — currently protected by the global semaphore, but any future
  sharding of the lock changes this invariant.

### 2.5 Scope
- `src/runtime/event-fabric/ChainAnchorService.cs` — the C1
  binding constraint.
- `src/engines/T0U/whycechain/lock/ChainLock.cs` — the C2 second
  global serial point.
- `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`,
  `src/runtime/dispatcher/SystemIntentDispatcher.cs` — workflow
  and engine in-flight surfaces.
- `src/runtime/middleware/**` — every middleware in the locked
  pipeline order, inspected for hidden locks or accidental
  serialization (e.g. policy evaluation contention,
  idempotency-store coordination).
- `src/platform/host/adapters/PostgresEventStoreAdapter.cs` —
  per-aggregate advisory lock (C5) waiter visibility and shedding.
- `src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs`
  — connection amplification (C6).
- `src/platform/host/adapters/PostgresProjectionWriter.cs` and
  every domain bootstrap module that constructs it — projections
  pool refactor (C7).
- `src/platform/api/controllers/TodoController.cs` and any other
  controller performing direct `NpgsqlConnection` against the
  projections database (C8).
- `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs`
  — sequential in-flight (C12) and DLQ growth (C9, consumer side).
- `src/platform/host/adapters/KafkaOutboxPublisher.cs` —
  single-instance drain ceiling (C11) and DLQ growth (C9, outbox
  side).
- `src/platform/host/adapters/PostgresOutboxAdapter.cs` —
  high-water-mark seam (PC-3) interaction with C14 capacity model.
- `src/platform/host/adapters/OutboxDepthSampler.cs` — coupling
  with declared pool budget.
- `src/platform/host/composition/**` — every bootstrap module that
  registers a worker, hosted service, or background loop, with
  attention to worker count and per-instance bounds.
- `src/runtime/projection/**` — projection registry / dispatcher
  for any hidden in-flight coordination state.
- Configuration surfaces — extend the declared `*.Options` set
  with new concurrency blocks (e.g. `Workflow.*`, `ChainAnchor.*`,
  `Idempotency.*`).

### 2.6 Non-Scope
- §5.1.1 / §5.1.2 / §5.1.3 (closed) re-verification.
- §5.2.1 (closed) re-verification of admission-control behavior
  beyond confirming that §5.2.2 changes do not regress it.
- §5.2.3 timeout / cancellation / circuit-protection behavior —
  sibling §5.2.x workstream, opens after §5.2.2.
- §5.3.x throughput certification, soak, stress, chaos — these
  consume §5.2.2 as a precondition but are not in scope here.
- §5.4.x security, §5.5.x governance.
- Generic performance tuning (CPU profiling, GC tuning, query
  optimization, index design). §5.2.2 is about *declared
  concurrency*, not *fast concurrency*.
- Capacity planning, hardware sizing, autoscaling policy.
- Domain-layer changes. The domain layer has zero dependencies per
  $7 and is not a concurrency surface.
- Engine-layer changes beyond `ChainLock` (C2). The T0U engines
  are stateless per $7 and the only stateful seam is the chain
  lock that feeds `ChainAnchorService`.
- Re-litigating any locked rule (DG-R5-EXCEPT-01,
  DG-R5-HOST-DOMAIN-FORBIDDEN, R-DOM-01, runtime middleware order,
  etc.).
- Replacing `SemaphoreSlim` with a different primitive type unless
  the Step C analysis demonstrates the substitution is the
  narrowest fix for an identified bound.

### 2.7 Remediation Strategy
1. **Inventory** — enumerate every concurrency primitive
   (`SemaphoreSlim`, `lock { }`, `Interlocked`, `Channel<T>`,
   `BackgroundService`, advisory lock, declared
   `NpgsqlDataSource`) and every resource ceiling (`MaxPoolSize`,
   `IntakeOptions.*Concurrency`, drain rate, in-flight count) in
   scope. Classify each as **DECLARED-BOUNDED** (declared
   ceiling, observable, fair), **DECLARED-OPAQUE** (declared but
   no observability), **ACCIDENTAL-BOUNDED** (incidental ceiling
   from upstream, e.g. `IntakeOptions.GlobalConcurrency`
   transitively bounding the dispatcher), or **UNBOUNDED**.
2. **Probe** — for each primitive, define a reproducible probe
   that answers: (a) what is the declared bound, (b) what is the
   waiter / queue shape, (c) what is the saturation signal, (d)
   how does the primitive interact with the §5.2.1 admission
   ceiling, (e) is the primitive on the critical write path or a
   side path.
3. **Capacity model** — produce a single end-to-end capacity
   table that walks one accepted request through every
   concurrency primitive it touches and computes the *effective*
   ceiling at each stage. Resolve the C14 mismatch (intake 256 vs
   pool 32 vs amplification 5×) explicitly.
4. **Triage** — assign severity per $16: S0 system-breaking
   (unbounded waiter queue on critical write path), S1
   architectural (declared admission ceiling inconsistent with
   downstream capacity), S2 structural (declared but
   non-observable), S3 cosmetic.
5. **Patch list** — non-`DECLARED-BOUNDED` findings become a
   remediation patch list with file paths, intended edit,
   externalized configuration shape (mirroring the §5.2.1
   precedent), and acceptance probe per item. No inline edits
   during the audit pass itself.
6. **Specification** — produce the canonical Concurrency Control
   and Resource Bounds specification: per-primitive declared
   bound, fairness model, saturation signal, capacity-model
   coupling, and propagation to admission control. This becomes
   the precondition document for §5.3.x throughput / soak / stress
   / chaos workstreams.
7. **Promote** — execution and remediation occur in follow-up
   prompts; this opening pack ends at the patch-list and
   specification handoff.

### 2.8 Task Breakdown
- **T-A** Concurrency primitive inventory — enumerate every
  primitive in §2.5 scope; initial-classify
  DECLARED-BOUNDED / DECLARED-OPAQUE / ACCIDENTAL-BOUNDED /
  UNBOUNDED.
- **T-B** Probe matrix — define probes per primitive covering
  declared bound, waiter shape, saturation signal,
  admission-coupling, and critical-path classification.
- **T-C** Probe execution — run the probe matrix against the
  current tree (static analysis, configuration inspection,
  targeted code reading); capture verbatim raw evidence.
- **T-D** Classification — assign DECLARED-BOUNDED /
  DECLARED-OPAQUE / ACCIDENTAL-BOUNDED / UNBOUNDED per probe with
  one-line justification.
- **T-E** Severity triage — assign S0/S1/S2/S3 to every
  non-`DECLARED-BOUNDED` finding per $16.
- **T-F** Capacity model — single end-to-end table from intake
  through every concurrency primitive on the critical path,
  computing the effective ceiling at each stage.
- **T-G** Patch list — remediation entries with file paths,
  intended edit, externalized configuration, and acceptance probe
  per item.
- **T-H** Restructuring proposal for `ChainAnchorService` (C1) —
  declared options, considered alternatives (move I/O outside the
  lock, shard by correlation hash, replace with per-aggregate
  primitive), and the chosen minimal-risk path. No edit applied.
- **T-I** Projections-pool refactor proposal (C7, C8) — declared
  `ProjectionsDataSource` wrapper, the bootstrap-module touch
  surface, and the controller refactor sequence. No edit applied.
- **T-J** DLQ depth bounds proposal (C9) — observability shape
  (`dlq.depth`, `dlq.oldest_age_seconds`) and rejection-policy
  shape if any. No edit applied.
- **T-K** Idempotency coalescing proposal (C6) — single-roundtrip
  upsert vs current 2-roundtrip exists+mark, plus the
  amplification-vs-pool-budget tradeoff captured in T-F.
- **T-L** Workflow in-flight ceiling proposal (C3) — per-workflow-name
  and per-tenant cap shape, fairness model, refusal class.
- **T-M** Final Concurrency Control and Resource Bounds
  specification — single artifact bundling inventory, probe
  matrix, raw evidence, classifications, severities, capacity
  model, patch list, restructuring proposals, and explicit
  terminal status.

### 2.9 Acceptance Criteria
1. Every concurrency primitive and resource ceiling in §2.5 scope
   is enumerated in the inventory and initial-classified.
2. Every primitive has at least one reproducible probe covering
   declared bound, waiter shape, saturation signal, admission
   coupling, and critical-path classification.
3. Every probe has reproducible evidence (command, file
   reference, or grep predicate + raw output) stored alongside
   the specification.
4. Every probe result is classified `DECLARED-BOUNDED` /
   `DECLARED-OPAQUE` / `ACCIDENTAL-BOUNDED` / `UNBOUNDED` with a
   one-line justification.
5. Every non-`DECLARED-BOUNDED` finding has S0–S3 severity per
   $16.
6. Every non-`DECLARED-BOUNDED` finding has a remediation patch
   list entry with file path, intended change, externalized
   configuration shape, and acceptance probe.
7. The end-to-end capacity model walks one accepted request from
   intake through every concurrency primitive on the critical
   path and computes the effective ceiling at each stage. The
   C14 (intake 256 vs pool 32 vs amplification 5×) mismatch is
   resolved explicitly with a declared decision.
8. `ChainAnchorService` (C1) has either (a) a structural
   restructuring proposal with chosen minimal-risk path or (b) an
   explicit declared waiver with reason — no silent deferral.
9. Projections-side concurrency (C7, C8) has a refactor proposal
   covering bootstrap-module touch surface and controller
   sequence.
10. DLQ depth bounds (C9) have either an observability +
    rejection patch list entry or an explicit declared waiver.
11. Idempotency amplification (C6) has either a coalescing
    patch list entry or an explicit declared waiver.
12. Workflow in-flight ceiling (C3) has either a declared cap
    proposal or an explicit declared waiver.
13. Every newly proposed patch declares its externalized
    configuration shape per the §5.2.1 precedent (`*.Options`
    record, composition root binding, `appsettings.json` block).
14. No remediation patch is applied during the audit pass;
    opening pack discipline is preserved until §5.2.2 advances
    out of the audit phase.
15. Any newly discovered guard rule or governance finding is
    captured under `claude/new-rules/` with the canonical 5-field
    shape per $1c.
16. Final specification explicitly returns one of: `PASS`,
    `FAIL`, `PARTIAL`, `BLOCKED`, `WAIVED`, with the reason
    recorded.
17. The §5.2.2 row in README §6.0 is updated only when the
    workstream actually advances state — not by the opening pack
    itself.

### 2.10 Evidence Required
- Concurrency primitive inventory table with initial
  classification.
- Probe matrix (probe ID, primitive ID, risk ID C1–C15,
  command/predicate, expected `DECLARED-BOUNDED` shape).
- Raw probe output for every probe (verbatim).
- Classification table (probe ID → DECLARED-BOUNDED /
  DECLARED-OPAQUE / ACCIDENTAL-BOUNDED / UNBOUNDED + reason).
- Severity table (finding ID → S0/S1/S2/S3).
- End-to-end capacity model table.
- Remediation patch list with externalized configuration shape
  per item.
- `ChainAnchorService` restructuring proposal (T-H).
- Projections-pool refactor proposal (T-I).
- DLQ depth bounds proposal (T-J).
- Idempotency coalescing proposal (T-K).
- Workflow in-flight ceiling proposal (T-L).
- New-rules capture file (if any).
- Final Concurrency Control and Resource Bounds specification
  with explicit terminal status.

---

## 3. TRACKING TABLE

| Field | Value |
|---|---|
| **ID** | 5.2.2 |
| **Topic** | Concurrency Control and Resource Bounds |
| **Objective** | Ensure runtime concurrency, shared locks, resource ceilings, and in-flight work limits are explicitly bounded, measurable, and canonically controlled. For every primitive and ceiling in scope, declare the bound, fairness model, saturation signal, and admission-coupling, with reproducible evidence and a remediation patch list against any primitive that is currently unbounded, accidentally serialized, or invisible. Resolve the C14 capacity-model mismatch between §5.2.1 admission ceiling and downstream pool/amplification budget. |
| **Tasks** | T-A Inventory · T-B Probe matrix · T-C Probe execution · T-D Classification · T-E Severity triage · T-F Capacity model · T-G Patch list · T-H ChainAnchor restructuring proposal · T-I Projections-pool refactor proposal · T-J DLQ depth bounds proposal · T-K Idempotency coalescing proposal · T-L Workflow in-flight ceiling proposal · T-M Final specification |
| **Deliverables** | Concurrency primitive inventory · Probe matrix · Raw probe evidence · Classification table · Severity table · End-to-end capacity model · Remediation patch list · ChainAnchor restructuring proposal · Projections-pool refactor proposal · DLQ depth bounds proposal · Idempotency coalescing proposal · Workflow in-flight ceiling proposal · New-rules capture (if any) · Final Concurrency Control and Resource Bounds specification |
| **Evidence Required** | Reproducible probe (command / file ref / grep predicate) and raw output for every primitive in §2.5; declared bound, waiter shape, saturation signal, admission coupling, and critical-path classification per primitive; capacity-model resolution of C14; classification + severity for every finding; explicit terminal status (PASS/FAIL/PARTIAL/BLOCKED/WAIVED) |
| **Status** | OPEN (NOT STARTED — workstream defined, no execution yet) |
| **Risk** | HIGH — every Phase 1.5 §5.3.x throughput / soak / stress / chaos workstream is gated on §5.2.2 in addition to §5.2.1. C1 (`ChainAnchorService`) is the runtime's binding throughput constraint and is currently a single global semaphore around external I/O. C14 (intake-vs-pool capacity mismatch) means the §5.2.1 admission ceiling admits ~40× the realistic downstream capacity. Latent S0 findings on the critical write path are very plausible. |
| **Blockers** | None known. §5.1.1, §5.1.2, §5.1.3, and §5.2.1 prerequisites all satisfied 2026-04-08. |
| **Owner** | Whycespace runtime / operational hardening track |
| **Notes** | Opening pack only. No remediation in this prompt. The §5.2.1 PASS report records C1 (chain anchor restructuring), C7 (projections-pool refactor), C9 (DLQ depth), C10 (`LoadEventsAsync` streaming), and the idempotency 2× amplification as explicitly deferred residual; §5.2.2 is their canonical owner. The phase1.5-§5.2.1 PC-* options precedent (`Intake.*`, `Opa.*`, `Outbox.*`, `Postgres.Pools.*`, `KafkaConsumer.*`) is the canonical shape for any new declared concurrency block. The phase1-gate-api-edge `IExceptionHandler` precedent is the canonical refusal-edge shape if §5.2.2 introduces any new typed-exception RETRYABLE REFUSAL surface. Continuity with §5.1.x and §5.2.1 (all PASS 2026-04-08) preserved. §5.2.2 is the **second** workstream in the §5.2.x cluster; §5.2.3 (Timeout, Cancellation, and Circuit Protection) follows. |

**Status legend:** NOT STARTED · IN PROGRESS · PARTIAL · BLOCKED · PASS · FAIL · WAIVED.

---

## 4. ACCEPTANCE CRITERIA
(See §2.9 above. Reproduced here for tracking convenience.)

1. Every in-scope concurrency primitive enumerated and initial-classified.
2. Every primitive has ≥1 reproducible probe.
3. Every probe has reproducible raw evidence.
4. Every probe result classified DECLARED-BOUNDED / DECLARED-OPAQUE / ACCIDENTAL-BOUNDED / UNBOUNDED with reason.
5. Every non-DECLARED-BOUNDED finding has S0–S3 severity.
6. Every non-DECLARED-BOUNDED finding has a remediation patch list entry with externalized configuration shape and acceptance probe.
7. End-to-end capacity model produced; C14 mismatch resolved with a declared decision.
8. `ChainAnchorService` (C1) restructuring proposal or declared waiver.
9. Projections-side concurrency (C7, C8) refactor proposal.
10. DLQ depth bounds (C9) observability + rejection proposal or declared waiver.
11. Idempotency amplification (C6) coalescing proposal or declared waiver.
12. Workflow in-flight ceiling (C3) cap proposal or declared waiver.
13. Every proposed patch declares its externalized configuration shape per the §5.2.1 precedent.
14. No remediation applied during audit pass.
15. Any newly discovered guard rule captured under `claude/new-rules/`.
16. Final specification returns explicit terminal status.
17. README §6.0 row 5.2.2 updated only on real state change.

---

## 5. REQUIRED ARTIFACTS

- `claude/project-prompts/20260408-230000-phase-1-5-5-2-2-concurrency-control-and-resource-bounds-open.md`
  — this opening pack.
- `claude/audits/concurrency-control-resource-bounds.audit.md`
  — to be created during T-M (final specification). Not created
  by this opening pack.
- `claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md` — to be created
  during T-D, T-H, T-I, T-J, T-K, or T-L if and only if newly
  discovered governance rules emerge.
- README §5.2.2 (existing, currently `NOT STARTED`) — unchanged
  by this opening pack; the workstream definition is anchored
  there, but state promotion is gated on real execution.
- README §6.0 master tracking table row 5.2.2 — unchanged by
  this opening pack.

---

## 6. CLAUDE EXECUTION PROMPT

> **Use this prompt to execute §5.2.2 in a follow-up session. Do not
> execute it as part of this opening pack.**

```
Phase 1.5 §5.2.2 — Concurrency Control and Resource Bounds (Execution Pass)

CLASSIFICATION: system / runtime / concurrency-control-resource-bounds
CONTEXT:
  §5.1.1 PASS (2026-04-08); §5.1.2 PASS (2026-04-08); §5.1.3 PASS (2026-04-08);
  §5.2.1 PASS (2026-04-08).
  Opening pack:
  claude/project-prompts/20260408-230000-phase-1-5-5-2-2-concurrency-control-and-resource-bounds-open.md

OBJECTIVE: Execute T-A through T-M of §5.2.2 as defined in the opening
  pack. Produce
  claude/audits/concurrency-control-resource-bounds.audit.md as the
  single consolidated deliverable. Do not modify source, guards,
  scripts, configuration, or README outside the audit artifact and (if
  needed) one or more claude/new-rules/ capture files.

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
  - Layer purity ($7): domain unchanged; engine unchanged except for
    C2 ChainLock analysis; only the runtime + host adapter layers are
    in scope for restructuring proposals.
  - Policy ($8): WHYCEPOLICY $8 evaluation order is preserved by every
    proposed patch.
  - Determinism ($9): every proposed concurrency primitive must be
    compatible with IClock-based time and deterministic IDs; no
    Guid.NewGuid, no DateTime.UtcNow.
  - No remediation patches applied; produce the patch list and
    proposals only.
  - No generic performance tuning; this workstream is about declared
    concurrency, not fast concurrency.
  - Any newly discovered guard rule → claude/new-rules/ per $1c.
  - Risk areas: C1–C15 from §2.4 of the opening pack.

EXECUTION STEPS:
  1. T-A Inventory — enumerate every concurrency primitive and resource
     ceiling in §2.5 scope and initial-classify DECLARED-BOUNDED /
     DECLARED-OPAQUE / ACCIDENTAL-BOUNDED / UNBOUNDED.
  2. T-B Probe matrix — define probes per primitive covering declared
     bound, waiter shape, saturation signal, admission-coupling, and
     critical-path classification. Each probe declares its expected
     DECLARED-BOUNDED shape.
  3. T-C Probe execution — run every probe (static analysis,
     configuration inspection, targeted code reading); capture
     verbatim raw evidence.
  4. T-D Classification — DECLARED-BOUNDED / DECLARED-OPAQUE /
     ACCIDENTAL-BOUNDED / UNBOUNDED per probe with one-line
     justification.
  5. T-E Severity triage — S0/S1/S2/S3 for every non-DECLARED-BOUNDED
     finding per $16.
  6. T-F Capacity model — single end-to-end table walking one accepted
     request through every primitive on the critical path. Resolve C14
     (intake 256 vs pool 32 vs ~5× amplification) explicitly.
  7. T-G Patch list — file path + intended edit + acceptance probe +
     externalized configuration shape per non-DECLARED-BOUNDED
     finding. Follow the phase1.5-§5.2.1 PC-* options precedent.
  8. T-H ChainAnchorService restructuring proposal — declared options,
     considered alternatives (move I/O outside lock, shard by
     correlation hash, replace with per-aggregate primitive), chosen
     minimal-risk path. No edit applied.
  9. T-I Projections-pool refactor proposal — declared
     ProjectionsDataSource wrapper, bootstrap-module touch surface,
     controller refactor sequence. No edit applied.
 10. T-J DLQ depth bounds proposal — observability shape and
     rejection-policy shape if any. No edit applied.
 11. T-K Idempotency coalescing proposal — single-roundtrip upsert vs
     current 2-roundtrip exists+mark, with the amplification-vs-pool
     tradeoff from T-F.
 12. T-L Workflow in-flight ceiling proposal — per-workflow-name and
     per-tenant cap shape, fairness model, refusal class.
 13. T-M Final specification — write
     claude/audits/concurrency-control-resource-bounds.audit.md
     bundling inventory, probe matrix, raw evidence, classifications,
     severities, capacity model, patch list, all five proposals
     (T-H..T-L), and explicit terminal status.

OUTPUT FORMAT:
  - Single audit artifact:
    claude/audits/concurrency-control-resource-bounds.audit.md
  - Optional: claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md
  - Structured failure report on any halt ($12: STATUS / STAGE /
    REASON / ACTION_REQUIRED).

VALIDATION CRITERIA:
  - All seventeen acceptance criteria from §2.9 / §4 satisfied.
  - Terminal status one of: PASS / FAIL / PARTIAL / BLOCKED / WAIVED.
  - Audit sweep ($1b) clean against the produced artifact.
  - No source/guard/script/configuration/README modification outside
    the explicitly named artifacts.
```

---

## 7. INITIAL STATUS

**OPEN** — workstream defined, tracked, and ready for execution. No
remediation performed. No source, guard, audit, script,
configuration, or README file modified by this opening pack. §5.2.2
enters the Phase 1.5 work queue as the **second** workstream in the
§5.2.x Runtime Infrastructure-Grade Hardening cluster, immediately
following §5.2.1 PASS (2026-04-08), and is the canonical owner of
the C1 (`ChainAnchorService` restructuring), C7 (projections-pool
refactor), C9 (DLQ depth bounds), C10 (`LoadEventsAsync` streaming),
and C6 (idempotency amplification) residual items recorded by the
§5.2.1 closure. §5.2.3 (Timeout, Cancellation, and Circuit
Protection) follows §5.2.2 as the third workstream in the §5.2.x
cluster.
