# Phase 1.5 §5.2.4 — Health, Readiness, and Degraded Modes (Workstream Opening Pack)

## TITLE
Phase 1.5 §5.2.4 Health, Readiness, and Degraded Modes — canonical workstream opening pack.

## CLASSIFICATION
system / runtime / health-readiness-degraded-modes

## CONTEXT
The Phase 1.5 §5.1.x structural hardening series and the first three
§5.2.x runtime-hardening workstreams all closed PASS on 2026-04-08:

- §5.1.1 Dependency Graph Remediation — **PASS** (2026-04-08)
- §5.1.2 Boundary Purity Validation — **PASS** (2026-04-08)
- §5.1.3 Canonical Documentation Alignment — **PASS** (2026-04-08)
- §5.2.1 Admission Control and Backpressure — **PASS** (2026-04-08)
- §5.2.2 Concurrency Control and Resource Bounds — **PASS** (2026-04-08)
- §5.2.3 Timeout, Cancellation, and Circuit Protection — **PASS** (2026-04-08)
  ([20260408-235960-phase-1-5-5-2-3-pass-closure.md](20260408-235960-phase-1-5-5-2-3-pass-closure.md))

§5.2.1 established four canonical RETRYABLE REFUSAL edges. §5.2.2
established eight declared concurrency / resource ceilings, eight
canonical `Whyce.*` meters, and a fifth refusal edge. §5.2.3
established declared timeout / cancellation / circuit-protection
discipline across every blocking seam, brought the canonical
refusal-edge count to **seven** (intake 429, OPA 503, outbox 503,
workflow saturation 503, chain-anchor wait timeout 503, chain-anchor
unavailable 503, workflow timeout 503, plus the pre-existing 409
concurrency-conflict REJECT), and threaded a real `CancellationToken`
end-to-end from `HttpContext.RequestAborted` (linked to
`IHostApplicationLifetime.ApplicationStopping` via TC-9) to the
database round-trip.

The runtime now refuses safely under load (§5.2.1), processes
accepted work predictably (§5.2.2), and fails fast within a
declared envelope (§5.2.3) — but **does not yet have a declared
health, readiness, or degraded-mode model**. The runtime can be
*operationally unsafe and still report itself as healthy*; an
operator or load balancer reading the existing health endpoint
cannot distinguish "process alive" from "ready to accept work" from
"actively refusing because a downstream is in canonical-refusal
state". Several known gaps were left explicitly outside the §5.2.3
PASS gate and recorded against §5.2.4:

- **No canonical degraded-state model.** The runtime today produces
  either a 2xx (success), a 4xx (client / refusal), or a 5xx
  (failure) response — but there is no third state that says "I am
  alive, my dependencies are partially impaired, I am refusing some
  classes of work but accepting others, and I expect to recover."
- **The existing health endpoint (if any) only proves process
  liveness.** It does not consult the §5.2.1 / §5.2.2 / §5.2.3
  declared envelopes; a host whose chain-anchor breaker is Open
  and whose outbox is over-watermark will still answer 200 OK.
- **No declared readiness rule for Postgres pool exhaustion.** The
  PC-4 declared `Postgres.Pools.*.MaxPoolSize` enforces a hard
  ceiling, but a pool at 100% utilization with growing acquisition
  failures should subtract readiness — there is no current rule
  that says so.
- **No readiness / degraded rule for OPA timeout / breaker-open.**
  PC-2 trips a Closed/Open/HalfOpen breaker on consecutive
  failures and exposes a `policy.evaluate.breaker_open` counter,
  but the breaker state is not consulted by any readiness path.
- **No readiness / degraded rule for the chain-anchor refusal
  family.** TC-2 / TC-3 introduced
  `ChainAnchorWaitTimeoutException` and
  `ChainAnchorUnavailableException` plus the
  `chain.store.failure{outcome}` counter, but neither the breaker
  state nor the failure rate is consulted by any readiness path.
- **No readiness / degraded rule for outbox saturation.** PC-3
  produces `outbox.depth` and `outbox.oldest_pending_age_seconds`
  gauges; KC-3 added `outbox.deadletter_depth`. None of these
  feed any readiness signal — a host with a permanently-stuck
  outbox publisher would still answer healthy.
- **No readiness / degraded rule for workflow saturation /
  workflow timeout.** KC-6 produces `workflow.admitted` /
  `workflow.rejected`; TC-7 produces typed timeouts via the
  edge handler. Neither feeds readiness.
- **No readiness / degraded rule for projection lag growth.**
  PC-7 produces `projection.lag_seconds` per topic. A host
  whose projections fall hours behind reality is not "healthy"
  in any operational sense, but the existing surface does not
  consult lag.
- **Background-worker partial failure semantics are unclear.**
  `KafkaOutboxPublisher` and `GenericKafkaProjectionConsumerWorker`
  are `BackgroundService` instances. If one of them dies (KC-3
  publish-failure storm, Kafka connectivity loss, projection
  handler exception), the host stays alive and continues to
  serve HTTP. Whether that should count as "ready" or "degraded"
  is undeclared.
- **No declared startup-readiness gate ordering.** `Program.cs`
  runs the `HsidInfrastructureValidator` before `app.Run()`,
  which is correct fail-fast posture, but there is no declared
  readiness rule that says "the host is not ready until the
  outbox depth sampler has produced its first observation, the
  projection consumer worker has joined its consumer group, and
  the chain anchor breaker is Closed". Today, the moment Kestrel
  binds, the host accepts traffic.
- **No readiness posture during host-shutdown drain.** TC-9
  declared `Host:ShutdownTimeoutSeconds` and linked
  `ApplicationStopping` into the request CT path, but did not
  declare what readiness should report during the drain window.
  A load balancer that polls during drain should immediately
  see "not ready" so traffic stops; today it would still see
  whatever the existing endpoint returns.
- **No audit-visible mapping from runtime failure families to
  health / readiness / degraded state.** The runtime has seven
  canonical refusal edges and at least nine canonical meters;
  none of them are connected to a single declared
  state-aggregation rule.

§5.2.4 Health, Readiness, and Degraded Modes is the **fourth**
workstream in the §5.2.x Runtime Infrastructure-Grade Hardening
cluster. Where §5.2.1 asked *"can the runtime refuse work
safely?"*, §5.2.2 asked *"under what concurrency model does the
runtime process accepted work?"*, and §5.2.3 asked *"under what
fail-fast envelope does the runtime interact with every blocking
seam?"*, §5.2.4 asks *"under what declared rules does the runtime
report itself as healthy, ready, and operationally safe — and
when those rules are violated, how does it expose degradation
without falsely claiming health?"*.

This workstream is the **direct precondition for §5.3.x
operational certification**. Soak, stress, and chaos workstreams
need a canonical "this run was observing a healthy system" /
"this run was observing a degraded system" / "this run was
observing a failed system" reading at every sample point;
otherwise their results are unreviewable. A 30-minute 1k RPS soak
that produces a 0.1% 503 burst is interpreted very differently
depending on whether the burst was canonical RETRYABLE REFUSAL
under degraded posture or generic 5xx under unreported impairment.

This artifact is the **opening pack only**. No remediation work
is performed here. No source, guard, audit, script, configuration,
or README file is modified. The workstream is created in `OPEN`
state and handed off for execution in subsequent prompts.

---

## 1. EXECUTIVE SUMMARY

§5.2.4 Health, Readiness, and Degraded Modes verifies that the
runtime exposes a **declared, audit-visible, dependency-aware
health / readiness / degraded-mode model** rather than the
incidental "200 OK if the process is alive" posture that
§5.2.1–§5.2.3 left in place. The workstream is the fourth and
final runtime-hardening workstream of the §5.2.x cluster, and
is the **direct precondition for §5.3.x operational certification**:
no soak / stress / chaos workstream can produce reviewable results
without a canonical state model that distinguishes healthy from
degraded from failed at every sample point.

The deliverable is, for every component / dependency / refusal
family in scope, an evidence-backed determination of:

1. The **health rule** — what constitutes process aliveness, and
   what state changes (if any) remove it.
2. The **readiness rule** — what dependencies must be observed in
   a known-good state before the host accepts traffic, and what
   state changes (singly or in combination) remove readiness.
3. The **degraded rule** — what observable signals (refusal-edge
   bursts, breaker-open transitions, lag growth, depth growth,
   pool saturation) must drop the host into a declared `DEGRADED`
   posture without dropping readiness entirely, and what posture
   it adopts when degraded.
4. The **failure mapping** from each canonical refusal family
   (intake 429, OPA 503, outbox 503, workflow saturation 503,
   chain-anchor wait timeout 503, chain-anchor unavailable 503,
   workflow timeout 503, concurrency-conflict 409) into one of
   {healthy, degraded, not-ready, halt}.
5. The **observability** — health endpoint contract, readiness
   endpoint contract, degraded gauge, state-transition counters
   — required to *prove* the model is in effect.

The structural follow-on from §5.2.3 is direct: every typed
exception family TC-2 / TC-3 / TC-7 / KC-6 / PC-2 / PC-3
introduced is a candidate degraded-state input, and the breaker
state (`OpaPolicyEvaluator`, `WhyceChainPostgresAdapter`) is the
canonical signal source for "downstream impairment without total
outage". §5.2.4 must aggregate these into a single declared model
rather than treating each as an isolated counter.

Initial status: **OPEN**.

---

## 2. WORKSTREAM DEFINITION

### 2.1 Purpose
Ensure that the runtime exposes a canonical, declared,
dependency-aware health / readiness / degraded-mode model rather
than relying on incidental "process alive" semantics. Every
operationally meaningful state change in the runtime — refusal-edge
burst, breaker transition, pool saturation, lag growth, depth
growth, background-worker death, drain — must be visible through
the declared model and consultable by load balancers, operators,
and §5.3.x certification harnesses.

### 2.2 Objective
Produce, for every component / dependency / refusal family in
scope, an evidence-backed determination of:

1. The **declared health rule** — process aliveness criteria.
2. The **declared readiness rule** — startup gate, steady-state
   acceptance criteria, and drain-state behavior.
3. The **declared degraded rule** — what signals drop the host
   into `DEGRADED` posture, what subset of work it continues to
   accept while degraded, and what response shape it returns to
   degraded-mode requests.
4. The **failure-family mapping** — every canonical refusal edge
   and every canonical meter signal mapped to its
   {healthy / degraded / not-ready / halt} contribution.
5. The **specification** — a single canonical declared model that
   §5.3.x can consume.

### 2.3 Why This Matters Before Phase 2
- §5.3.x operational certification (throughput, soak, stress,
  chaos) **requires** a canonical state read at every sample
  point. Without §5.2.4, a soak result of "0.1% 503 burst" is
  unreviewable: was the host degraded by design, or was it
  failing silently?
- Phase 2 will introduce real workloads behind real load
  balancers. A load balancer needs a `/health` and a `/ready`
  endpoint that answer with declared semantics, not "the process
  is up". Without §5.2.4, every Phase 2 deployment is a
  rolling-update hazard: the new instance binds Kestrel and
  immediately receives traffic before its outbox sampler, its
  Kafka consumer group join, or its chain-anchor breaker
  recovery have completed.
- §5.2.3 introduced three new canonical refusal families
  (chain-anchor wait timeout, chain-anchor unavailable, workflow
  timeout). Each is a typed degradation signal, but none is
  currently aggregated into any state model. §5.2.4 is the
  canonical owner of that aggregation.
- §5.2.2 introduced declared `Whyce.EventStore` and
  `Whyce.Workflow` meters; PC-7 introduced `projection.lag_seconds`;
  KC-3 introduced `outbox.deadletter_depth`. These are the
  building blocks of degraded-mode signals — but no current
  rule consults any of them for readiness purposes.
- TC-9 declared `HostOptions.ShutdownTimeout` but did not
  declare a drain-state readiness posture. A load balancer that
  polls a draining host should see "not ready" instantly so
  traffic stops within one poll interval, not at the end of the
  drain window.
- WHYCEPOLICY $8 + $11: a runtime that reports healthy while
  failing audit-chain anchoring violates $11 audit invariants
  silently. §5.2.4 must declare that any chain-anchor failure
  mode is at minimum a degraded-state input, and at maximum a
  not-ready / halt input.
- $9 determinism + idempotency: a degraded host that continues
  to accept commands must do so under the same determinism
  rules as a healthy host. §5.2.4 must declare what classes of
  work are accepted while degraded so determinism is preserved.

### 2.4 Known Risk Areas
- **H1** — **No canonical degraded-state model.** The runtime
  has only two states today: alive and dead. There is no
  declared `DEGRADED` state, no aggregation rule, no observable
  state gauge.
- **H2** — **The existing health endpoint (if any) only proves
  process aliveness.** A host whose every dependency is impaired
  but whose ASP.NET pipeline is running will still return 200 OK
  to `/health`. Step A must inventory the existing endpoint
  surface and decide whether it stays, splits into
  `/health` + `/ready`, or is replaced.
- **H3** — **No declared readiness rule for Postgres pool
  exhaustion.** PC-4 declares
  `Postgres.Pools.{event-store,chain,projections}.MaxPoolSize`
  and exposes `postgres.pool.acquisitions` /
  `postgres.pool.acquisition_failures`. A pool at 100%
  utilization with growing failures should subtract readiness;
  no rule says so.
- **H4** — **No readiness / degraded rule for OPA timeout /
  breaker-open.** PC-2 exposes `policy.evaluate.breaker_open`
  but the breaker state is not consulted by any readiness path.
- **H5** — **No readiness / degraded rule for the chain-anchor
  wait-timeout / unavailable family.** TC-2 / TC-3 introduced
  the typed refusals and the `chain.store.failure{outcome}`
  counter; neither feeds readiness.
- **H6** — **No readiness / degraded rule for outbox saturation
  / outbox depth growth.** PC-3 / KC-3 produce `outbox.depth`,
  `outbox.oldest_pending_age_seconds`, and
  `outbox.deadletter_depth`; none feed readiness.
- **H7** — **No readiness / degraded rule for workflow
  saturation / workflow timeout.** KC-6 + TC-7 produce
  `workflow.admitted` / `workflow.rejected` and the typed
  timeout exception; neither feeds readiness.
- **H8** — **No readiness / degraded rule for projection lag
  growth.** PC-7 produces `projection.lag_seconds{topic}`. A
  host whose projection lag grows past minutes is not
  operationally healthy.
- **H9** — **Background-worker partial failure semantics are
  unclear.** If `KafkaOutboxPublisher` or
  `GenericKafkaProjectionConsumerWorker` dies (`StopAsync`
  fired by an unhandled exception, or stuck in a retry loop),
  the host stays alive and Kestrel continues to serve. Whether
  this should drop readiness, drop to degraded, or halt the
  host is undeclared.
- **H10** — **The host may be live but operationally unsafe.**
  The combination of H1–H9 means a host can be in a state where
  every accepted command will refuse at the chain anchor edge,
  but `/health` still answers 200 — operationally indistinguishable
  from healthy from outside the host.
- **H11** — **No canonical aggregation model across the eight
  `Whyce.*` meters.** `Whyce.Intake`, `Whyce.Policy`,
  `Whyce.Outbox`, `Whyce.Postgres`, `Whyce.Chain`,
  `Whyce.EventStore`, `Whyce.Workflow`,
  `Whyce.Projection.Consumer` each carry signals that are
  individually meaningful but never combined into a single
  declared state read.
- **H12** — **No declared startup-readiness gate ordering.**
  `Program.cs` runs `HsidInfrastructureValidator` (correct
  fail-fast posture). But the moment `app.Run()` begins,
  Kestrel binds and accepts traffic — before the
  `OutboxDepthSampler` has produced its first observation,
  before the projection consumer worker has joined its
  consumer group, and before the chain-anchor breaker has
  reset from any prior-process state.
- **H13** — **No declared degraded-mode response contract for
  clients / operators.** When the host is degraded, a client
  expects a stable, machine-readable contract — RFC 7807
  `application/problem+json` with a degraded-state `type` URI,
  consistent with the §5.2.1 / §5.2.2 / §5.2.3 canonical
  refusal-edge handlers. No such contract exists.
- **H14** — **No readiness posture during host-shutdown drain.**
  TC-9 linked `ApplicationStopping` into the request CT path
  but did not declare that readiness must report "not ready"
  the moment `ApplicationStopping` fires.
- **H15** — **No audit-visible mapping from runtime failure
  families to health / readiness / degraded state.** The
  seven canonical refusal edges, the at-least-nine canonical
  meters, and the typed exceptions from §5.2.1–§5.2.3 are not
  connected to any declared aggregation rule.
- **H16** — **No declared "halt" rule for unrecoverable state.**
  Some failures are not "degraded" — they are "this host can
  never serve correctly again until restarted". Database schema
  drift, missing migration, identity store unreachable at
  startup, chain-anchor genesis-hash mismatch — these should
  halt the host, not degrade it. §5.2.4 must enumerate what
  qualifies and what the halt mechanism is.
- **H17** — **No correlation between TC-9 host-shutdown drain
  and KC-6 workflow admission lease release.** A workflow
  admission lease is held across the entire workflow execution.
  If a host enters drain with N in-flight workflows, the
  drain timeout must be sized to allow them to complete or
  cancel — and the readiness posture must say "not ready"
  immediately to bleed off new traffic.

### 2.5 Scope
- `src/platform/host/Program.cs` — health / readiness endpoint
  registration, startup gate ordering, drain posture
  declaration.
- `src/platform/api/middleware/**` — any new
  health-endpoint or readiness-endpoint middleware, plus the
  declared degraded-mode response contract handler.
- `src/runtime/observability/**` — declared state-aggregation
  rule, breaker-state gauge subscribers, failure-family
  aggregator.
- `src/shared/contracts/infrastructure/admission/**` — any new
  declared options block (`Health.*` / `Readiness.*` /
  `Degraded.*`).
- `src/runtime/middleware/**` — read-only consultation of the
  declared state model (no new gates introduced; this is a
  reporting workstream, not a refusal workstream).
- `OpaPolicyEvaluator` (PC-2) breaker state subscription —
  reference pattern for breaker-state aggregation.
- `WhyceChainPostgresAdapter` (TC-3) breaker state subscription.
- `OutboxDepthSampler` (PC-3 / KC-3) — depth signal
  subscription.
- `KafkaOutboxPublisher` and
  `GenericKafkaProjectionConsumerWorker` (background services) —
  liveness / death detection and readiness contribution.
- `IHostApplicationLifetime` consumers — drain-state readiness
  posture.
- `appsettings.json` — any new declared options block.
- `claude/audits/health-readiness-degraded-modes.audit.md` —
  the canonical final audit artifact, to be created during
  T-N.

### 2.6 Non-Scope
- §5.1.1 / §5.1.2 / §5.1.3 / §5.2.1 / §5.2.2 / §5.2.3 (closed)
  re-verification beyond confirming that §5.2.4 changes do not
  regress them.
- §5.2.5 (Multi-Instance Runtime Safety) — sibling §5.2.x
  workstream, opens after §5.2.4.
- §5.3.x throughput certification, soak, stress, chaos —
  consume §5.2.4 as a precondition but are not in scope here.
- §5.4.x security, §5.5.x governance.
- New refusal edges. §5.2.4 is a *reporting* workstream — it
  *consults* the §5.2.1 / §5.2.2 / §5.2.3 refusal edges; it
  does not introduce new ones.
- Generic performance tuning. §5.2.4 is about declared
  state semantics, not fast-path latency.
- Capacity planning, hardware sizing, autoscaling policy.
- Domain-layer changes. The domain layer has zero dependencies
  per $7 and is not a health / readiness surface.
- Engine-layer changes. The engines do not own dependency
  health.
- Replacement of any §5.2.1 / §5.2.2 / §5.2.3 declared options
  block with a different shape unless §5.2.4 finds a
  load-bearing reason.
- Re-litigating any locked rule.
- Distributed health / cluster-wide readiness — §5.2.5 owns
  multi-instance posture; §5.2.4 is single-instance only.

### 2.7 Remediation Strategy
1. **Inventory** — enumerate every component / dependency /
   refusal family / meter / background worker that contributes
   to operational state. Classify each on the canonical 4-way
   model: **DECLARED-OBSERVED** (the signal exists, is
   declared, and is consulted by a state rule),
   **DECLARED-UNOBSERVED** (the signal exists and is declared
   but is not consulted), **INCIDENTAL** (the signal exists
   but is not declared), **ABSENT** (no signal).
2. **Probe** — for each item, define a reproducible probe
   answering: (a) does a state rule consult it, (b) what state
   read does it produce, (c) what response shape does the host
   return when the state rule is violated, (d) is the state
   visible in metrics.
3. **State-aggregation map** — single end-to-end diagram from
   each `Whyce.*` meter and each typed exception family into
   the declared state-aggregation rule and out to the
   `/health` + `/ready` endpoints.
4. **Triage** — assign severity per $16: S0 system-breaking
   ("host can be in operationally-unsafe state and report
   healthy"), S1 architectural ("readiness exists but doesn't
   consult a load-bearing signal"), S2 structural ("declared
   but no aggregation"), S3 cosmetic.
5. **Patch list** — non-`DECLARED-OBSERVED` findings become a
   remediation patch list with file paths, intended edit,
   externalized configuration, and acceptance probe per item.
6. **Specification** — produce the canonical Health, Readiness,
   and Degraded Modes specification: per-component declared
   health rule, declared readiness rule, declared degraded
   rule, and failure-family-to-state mapping.
7. **Promote** — execution and remediation occur in follow-up
   prompts; this opening pack ends at the patch-list and
   specification handoff.

### 2.8 Task Breakdown
- **T-A** Health / readiness / degraded inventory — enumerate
  every component, dependency, refusal family, meter, and
  background worker in §2.5 scope; initial-classify
  DECLARED-OBSERVED / DECLARED-UNOBSERVED / INCIDENTAL /
  ABSENT.
- **T-B** Probe matrix — define probes per item covering
  state-rule consultation, state read produced, response
  shape, and metric visibility.
- **T-C** Probe execution — run the probe matrix against the
  current tree (static analysis, configuration inspection,
  targeted code reading); capture verbatim raw evidence.
- **T-D** Classification — DECLARED-OBSERVED /
  DECLARED-UNOBSERVED / INCIDENTAL / ABSENT per probe with
  one-line justification.
- **T-E** Severity triage — S0/S1/S2/S3 for every
  non-`DECLARED-OBSERVED` finding per $16.
- **T-F** State-aggregation map — single end-to-end diagram
  from every `Whyce.*` meter and every typed exception family
  into the declared state-aggregation rule and out to the
  `/health` + `/ready` endpoints. Every break-point marked.
- **T-G** Patch list — file path + intended edit + acceptance
  probe + externalized configuration shape per
  non-`DECLARED-OBSERVED` finding.
- **T-H** Health-endpoint contract proposal — declared
  `/health` semantics: process-aliveness only, response shape,
  metric.
- **T-I** Readiness-endpoint contract proposal — declared
  `/ready` semantics: dependency-aware, startup gate ordering,
  drain-state posture, response shape, metric.
- **T-J** Degraded-mode contract proposal — declared
  `DEGRADED` posture: which signals drop into degraded, what
  subset of work continues to be accepted, what response shape
  is returned, RFC 7807 type URI.
- **T-K** Failure-family-to-state mapping — every canonical
  refusal edge (intake 429, OPA 503, outbox 503, workflow
  saturation 503, chain-anchor wait timeout 503, chain-anchor
  unavailable 503, workflow timeout 503, concurrency-conflict
  409) mapped to its {healthy / degraded / not-ready / halt}
  contribution. Every canonical meter signal mapped likewise.
- **T-L** Background-worker liveness proposal — declared rule
  for `KafkaOutboxPublisher` and
  `GenericKafkaProjectionConsumerWorker` partial-failure
  semantics: what dropping a worker does to readiness /
  degraded.
- **T-M** Startup + shutdown posture proposal — declared
  startup-readiness gate ordering and TC-9 drain-state
  readiness posture.
- **T-N** Final Health, Readiness, and Degraded Modes
  specification — single artifact bundling inventory, probe
  matrix, raw evidence, classifications, severities,
  state-aggregation map, patch list, all proposals (T-H..T-M),
  and explicit terminal status.

### 2.9 Acceptance Criteria
1. Every component / dependency / refusal family / meter /
   background worker in §2.5 scope is enumerated and
   initial-classified.
2. Every item has at least one reproducible probe covering
   state-rule consultation, state read, response shape, and
   metric visibility.
3. Every probe has reproducible evidence (command, file
   reference, or grep predicate + raw output) stored alongside
   the specification.
4. Every probe result is classified `DECLARED-OBSERVED` /
   `DECLARED-UNOBSERVED` / `INCIDENTAL` / `ABSENT` with a
   one-line justification.
5. Every non-`DECLARED-OBSERVED` finding has S0–S3 severity
   per $16.
6. Every non-`DECLARED-OBSERVED` finding has a remediation
   patch list entry with file path, intended change,
   externalized configuration shape, and acceptance probe.
7. The end-to-end state-aggregation map walks every `Whyce.*`
   meter and every typed exception family into the declared
   state-aggregation rule and out to the `/health` + `/ready`
   endpoints. Every break-point is marked.
8. **Canonical health model declared** — single rule for
   process aliveness with explicit response shape.
9. **Canonical readiness model declared** — single rule for
   dependency-aware acceptance, with declared startup gate
   ordering and drain-state posture.
10. **Canonical degraded-mode model declared** — single rule
    for which signals drop into `DEGRADED`, what subset of
    work the host continues to accept while degraded, and the
    RFC 7807 response shape for degraded-mode requests.
11. **Every major refusal family mapped to health / readiness
    / degraded impact** — every one of the seven canonical
    retryable-refusal edges + concurrency-conflict 409 has a
    declared {healthy / degraded / not-ready / halt}
    contribution.
12. **Startup posture declared** — what dependencies must be
    observed in known-good state before Kestrel begins
    accepting traffic, and what the gate mechanism is.
13. **Shutdown posture declared** — what `/ready` returns the
    moment `ApplicationStopping` fires; load balancer
    drain-window contract.
14. **Background-worker posture declared** — what
    `KafkaOutboxPublisher` and
    `GenericKafkaProjectionConsumerWorker` partial-failure
    contributes to readiness / degraded / halt.
15. **Dependency-specific readiness rules declared** for
    Postgres pools, OPA, chain anchor, outbox, workflow gate,
    projection lag, identity store.
16. Every newly proposed patch declares its externalized
    configuration shape per the §5.2.1 / §5.2.2 / §5.2.3
    precedent.
17. Every newly proposed response shape declares its content
    type, status code, and (if applicable) `Retry-After` /
    state-URI fields per the canonical refusal-edge precedent.
18. No remediation patch is applied during the audit pass;
    opening pack discipline is preserved until §5.2.4 advances
    out of the audit phase.
19. Any newly discovered guard rule or governance finding is
    captured under `claude/new-rules/` with the canonical
    5-field shape per $1c.
20. **Final audit artifact required** —
    `claude/audits/health-readiness-degraded-modes.audit.md` —
    explicitly returns one of: `PASS`, `FAIL`, `PARTIAL`,
    `BLOCKED`, `WAIVED`, with the reason recorded.
21. **README promotion gated on actual execution** — the §5.2.4
    row in README §6.0 and the §5.2.4 README section are
    updated only when the workstream actually advances state,
    not by the opening pack itself.

### 2.10 Evidence Required
- Health / readiness / degraded inventory table with initial
  classification.
- Probe matrix (probe ID, item ID, risk ID H1–H17,
  command/predicate, expected `DECLARED-OBSERVED` shape).
- Raw probe output for every probe (verbatim).
- Classification table (probe ID → DECLARED-OBSERVED /
  DECLARED-UNOBSERVED / INCIDENTAL / ABSENT + reason).
- Severity table (finding ID → S0/S1/S2/S3).
- End-to-end state-aggregation map with break-points.
- Remediation patch list with externalized configuration shape
  per item.
- Health-endpoint contract proposal (T-H).
- Readiness-endpoint contract proposal (T-I).
- Degraded-mode contract proposal (T-J).
- Failure-family-to-state mapping (T-K).
- Background-worker liveness proposal (T-L).
- Startup + shutdown posture proposal (T-M).
- New-rules capture file (if any).
- Final Health, Readiness, and Degraded Modes specification
  with explicit terminal status.

---

## 3. TRACKING TABLE

| Field | Value |
|---|---|
| **ID** | 5.2.4 |
| **Topic** | Health, Readiness, and Degraded Modes |
| **Objective** | Ensure the runtime exposes a canonical, declared, dependency-aware health / readiness / degraded-mode model rather than relying on incidental "process alive" semantics. For every component / dependency / refusal family in scope, declare the health rule, the readiness rule, the degraded rule, and the failure-family-to-state mapping. Resolve the §5.2.3 follow-on by aggregating the seven canonical retryable-refusal edges, the eight `Whyce.*` meters, and the typed exception families introduced by §5.2.1–§5.2.3 into a single declared state model that load balancers, operators, and §5.3.x certification harnesses can consult. |
| **Tasks** | T-A Inventory · T-B Probe matrix · T-C Probe execution · T-D Classification · T-E Severity triage · T-F State-aggregation map · T-G Patch list · T-H Health-endpoint contract · T-I Readiness-endpoint contract · T-J Degraded-mode contract · T-K Failure-family-to-state mapping · T-L Background-worker liveness · T-M Startup + shutdown posture · T-N Final specification |
| **Deliverables** | Health/readiness/degraded inventory · Probe matrix · Raw probe evidence · Classification table · Severity table · State-aggregation map · Remediation patch list · Health-endpoint contract proposal · Readiness-endpoint contract proposal · Degraded-mode contract proposal · Failure-family-to-state mapping · Background-worker liveness proposal · Startup + shutdown posture proposal · New-rules capture (if any) · Final Health, Readiness, and Degraded Modes specification |
| **Evidence Required** | Reproducible probe (command / file ref / grep predicate) and raw output for every item in §2.5; declared health rule, readiness rule, degraded rule, and failure-family mapping per item; state-aggregation map with all break-points marked; classification + severity for every finding; explicit terminal status (PASS/FAIL/PARTIAL/BLOCKED/WAIVED) |
| **Status** | OPEN (NOT STARTED — workstream defined, no execution yet) |
| **Risk** | HIGH — every Phase 1.5 §5.3.x throughput / soak / stress / chaos workstream is gated on §5.2.4 in addition to §5.2.1, §5.2.2, and §5.2.3. H1 (no canonical degraded-state model), H10 (host may be live but operationally unsafe), and H12 (no startup-readiness gate ordering) are S0 candidates: a host whose every dependency is impaired today still answers "alive", which makes any §5.3.x soak or chaos result unreviewable. The §5.2.3 typed-refusal families (TC-2 / TC-3 / TC-7) are the canonical inputs to a degraded-state model that does not yet exist. |
| **Blockers** | None known. §5.1.1, §5.1.2, §5.1.3, §5.2.1, §5.2.2, §5.2.3 prerequisites all satisfied 2026-04-08. The §5.2.3 declared timeout / breaker / cancellation envelope is the input surface; §5.2.4 *consults* it and does not need any change to it. |
| **Owner** | Whycespace runtime / operational hardening track |
| **Notes** | Opening pack only. No remediation in this prompt. The §5.2.3 PASS report identifies §5.2.4 as the canonical owner of the health / readiness / degraded-mode model and lists the chain-anchor refusal family, the workflow timeout family, the breaker-state gauges, and the projection-lag histogram as direct inputs. The phase1.5-§5.2.x options precedent (`Intake.*`, `Opa.*`, `Outbox.*`, `Postgres.Pools.*`, `KafkaConsumer.*`, `Workflow.*`, `ChainAnchor.*`, `Host.*`) is the canonical config shape for any new declared options block (`Health.*` / `Readiness.*` / `Degraded.*`). The phase1-gate-api-edge `IExceptionHandler` precedent is the canonical response-shape pattern for any new degraded-mode response contract. Continuity with §5.1.x, §5.2.1, §5.2.2, §5.2.3 (all PASS 2026-04-08) preserved. §5.2.4 is the **fourth and final** runtime-hardening workstream of the §5.2.x cluster; §5.2.5 (Multi-Instance Runtime Safety) follows. §5.2.4 is the **direct precondition for §5.3.x operational certification** — soak / stress / chaos results are unreviewable without a canonical state read at every sample point. |

**Status legend:** NOT STARTED · IN PROGRESS · PARTIAL · BLOCKED · PASS · FAIL · WAIVED.

---

## 4. ACCEPTANCE CRITERIA
(See §2.9 above. Reproduced here for tracking convenience.)

1. Every in-scope component / dependency / refusal family / meter / background worker enumerated and initial-classified.
2. Every item has ≥1 reproducible probe covering state-rule consultation, state read, response shape, and metric visibility.
3. Every probe has reproducible raw evidence.
4. Every probe result classified DECLARED-OBSERVED / DECLARED-UNOBSERVED / INCIDENTAL / ABSENT with reason.
5. Every non-DECLARED-OBSERVED finding has S0–S3 severity.
6. Every non-DECLARED-OBSERVED finding has a remediation patch list entry with externalized configuration shape and acceptance probe.
7. End-to-end state-aggregation map produced; every break-point marked.
8. Canonical health model declared.
9. Canonical readiness model declared.
10. Canonical degraded-mode model declared.
11. Every major refusal family mapped to health/readiness/degraded impact.
12. Startup posture declared.
13. Shutdown posture declared.
14. Background-worker posture declared.
15. Dependency-specific readiness rules declared (Postgres pools, OPA, chain anchor, outbox, workflow gate, projection lag, identity store).
16. Every proposed patch declares its externalized configuration shape per the §5.2.1 / §5.2.2 / §5.2.3 precedent.
17. Every proposed response shape declares its canonical contract.
18. No remediation applied during audit pass.
19. Any newly discovered guard rule captured under `claude/new-rules/`.
20. Final audit artifact `claude/audits/health-readiness-degraded-modes.audit.md` returns explicit terminal status.
21. README §5.2.4 + README §6.0 row 5.2.4 updated only on real state change.

---

## 5. REQUIRED ARTIFACTS

- `claude/project-prompts/20260409-010000-phase-1-5-5-2-4-health-readiness-degraded-modes-open.md`
  — this opening pack.
- `claude/audits/health-readiness-degraded-modes.audit.md` —
  to be created during T-N (final specification). Not created
  by this opening pack.
- `claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md` — to be created
  during T-D, T-H, T-I, T-J, T-K, T-L, or T-M if and only if
  newly discovered governance rules emerge.
- README §5.2.4 (existing, currently `NOT STARTED`) — unchanged
  by this opening pack; the workstream definition is anchored
  there, but state promotion is gated on real execution.
- README §6.0 master tracking table row 5.2.4 — unchanged by
  this opening pack.

---

## 6. CLAUDE EXECUTION PROMPT

> **Use this prompt to execute §5.2.4 in a follow-up session. Do
> not execute it as part of this opening pack.**

```
Phase 1.5 §5.2.4 — Health, Readiness, and Degraded Modes (Execution Pass)

CLASSIFICATION: system / runtime / health-readiness-degraded-modes
CONTEXT:
  §5.1.1 PASS (2026-04-08); §5.1.2 PASS (2026-04-08); §5.1.3 PASS (2026-04-08);
  §5.2.1 PASS (2026-04-08); §5.2.2 PASS (2026-04-08); §5.2.3 PASS (2026-04-08).
  Opening pack:
  claude/project-prompts/20260409-010000-phase-1-5-5-2-4-health-readiness-degraded-modes-open.md

OBJECTIVE: Execute T-A through T-N of §5.2.4 as defined in the opening
  pack. Produce
  claude/audits/health-readiness-degraded-modes.audit.md as
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
    adapter, api middleware, observability, and composition layers
    are in scope for proposals.
  - Policy ($8): WHYCEPOLICY $8 evaluation order is preserved by
    every proposed patch. Health / readiness endpoints MUST NOT
    bypass policy on any state-changing path; they are read-only.
  - Determinism ($9): every proposed health / readiness primitive
    must be compatible with IClock-based time and deterministic
    aggregation (no Random, no Guid.NewGuid, no DateTime.UtcNow).
  - No remediation patches applied; produce the patch list and
    proposals only.
  - No new refusal edges. §5.2.4 is a *reporting* workstream — it
    *consults* the §5.2.1 / §5.2.2 / §5.2.3 refusal edges; it does
    not introduce new ones.
  - Any newly discovered guard rule → claude/new-rules/ per $1c.
  - Risk areas: H1–H17 from §2.4 of the opening pack.

EXECUTION STEPS:
  1. T-A Inventory — enumerate every component, dependency, refusal
     family, meter, and background worker in §2.5 scope and
     initial-classify DECLARED-OBSERVED / DECLARED-UNOBSERVED /
     INCIDENTAL / ABSENT.
  2. T-B Probe matrix — define probes per item covering state-rule
     consultation, state read produced, response shape, and metric
     visibility.
  3. T-C Probe execution — run every probe (static analysis,
     configuration inspection, targeted code reading); capture
     verbatim raw evidence.
  4. T-D Classification — DECLARED-OBSERVED / DECLARED-UNOBSERVED /
     INCIDENTAL / ABSENT per probe with one-line justification.
  5. T-E Severity triage — S0/S1/S2/S3 for every non-DECLARED-OBSERVED
     finding per $16.
  6. T-F State-aggregation map — single end-to-end diagram from
     every Whyce.* meter and every typed exception family into the
     declared state-aggregation rule and out to the /health + /ready
     endpoints. Every break-point marked.
  7. T-G Patch list — file path + intended edit + acceptance probe +
     externalized configuration shape per non-DECLARED-OBSERVED
     finding. Follow the §5.2.1 / §5.2.2 / §5.2.3 PC-* / KC-* / TC-*
     precedent.
  8. T-H Health-endpoint contract proposal — declared /health
     semantics: process-aliveness only, response shape, metric.
  9. T-I Readiness-endpoint contract proposal — declared /ready
     semantics: dependency-aware, startup gate ordering, drain-state
     posture, response shape, metric.
 10. T-J Degraded-mode contract proposal — declared DEGRADED posture:
     which signals drop into degraded, what subset of work continues
     to be accepted, what response shape is returned, RFC 7807 type
     URI.
 11. T-K Failure-family-to-state mapping — every canonical refusal
     edge and every canonical meter signal mapped to its
     {healthy / degraded / not-ready / halt} contribution.
 12. T-L Background-worker liveness proposal — declared rule for
     KafkaOutboxPublisher and GenericKafkaProjectionConsumerWorker
     partial-failure semantics.
 13. T-M Startup + shutdown posture proposal — declared
     startup-readiness gate ordering and TC-9 drain-state readiness
     posture.
 14. T-N Final specification — write
     claude/audits/health-readiness-degraded-modes.audit.md
     bundling inventory, probe matrix, raw evidence, classifications,
     severities, state-aggregation map, patch list, all six proposals
     (T-H..T-M), and explicit terminal status.

OUTPUT FORMAT:
  - Single audit artifact:
    claude/audits/health-readiness-degraded-modes.audit.md
  - Optional: claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md
  - Structured failure report on any halt ($12: STATUS / STAGE /
    REASON / ACTION_REQUIRED).

VALIDATION CRITERIA:
  - All twenty-one acceptance criteria from §2.9 / §4 satisfied.
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
§5.2.4 enters the Phase 1.5 work queue as the **fourth and
final** runtime-hardening workstream in the §5.2.x cluster,
immediately following §5.2.3 PASS (2026-04-08), and is the
**canonical owner of the runtime health / readiness / degraded-mode
model** needed before §5.3.x operational certification can begin.
The §5.2.1 / §5.2.2 / §5.2.3 declared options blocks, canonical
`Whyce.*` meters, and seven canonical retryable-refusal edges are
the input surface; §5.2.4 *consults* them and aggregates them into
a single declared state model. §5.2.5 (Multi-Instance Runtime
Safety) follows §5.2.4 as the **fifth and final** workstream in
the §5.2.x cluster.
