# Phase 1.5 — Re-Open Amendment (CANONICAL OVERRIDE)

**STATUS: PHASE 1.5 RE-OPENED — PRIOR CERTIFICATION OVERRIDDEN**
**AMENDMENT DATE: 2026-04-09**
**AUTHORITY: Direct user instruction (in-session decision, Path (a) of MI-2 follow-up dialogue)**
**CLAUDE.md COMPLIANCE: Explicit user-authored scope amendment per $5 (anti-drift) — NOT inferred, NOT inveneted, NOT extrapolated.**

---

## 1. OVERRIDE STATEMENT

The Phase 1.5 final consolidated audit recorded at
[`claude/audits/phase1.5/phase1.5-final.audit.md`](phase1.5-final.audit.md)
("STATUS: PASS — UNCONDITIONAL CERTIFICATION SIGNED", dated 2026-04-09)
is hereby **OVERRIDDEN** and Phase 1.5 is re-opened.

The eight audit modules listed in that document remain valid as evidence
for the *narrow correctness scope* they covered (determinism, policy
enforcement, execution lock, runtime control plane integrity, health
system completeness, failure semantics vocabulary, dependency graph,
test/build verification). Their PASS status is preserved as historical
record. They are NOT sufficient on their own to certify Phase 1.5
under the expanded scope defined in §3 below.

The prior certification was **premature** in the sense that it validated
*correctness* but did not validate *infrastructure-grade resilience and
operational readiness* — the load-bearing requirements of the
Whycespace doctrine (infrastructure-grade system, 30–100 year horizon).

## 2. RATIONALE

Whycespace is positioned as infrastructure-grade, multi-decade-horizon
software. Under that doctrine, "passing all unit and integration tests
under happy-path conditions" is a *necessary but insufficient* gate.
Certifying a phase complete on correctness alone is what would later
become a multi-year liability when the system meets real-world failure
modes, sustained load, and operator handoff.

Phase 1.5 closure must therefore additionally validate:

  - **Failure & recovery behavior** under explicit infrastructure
    outages (Kafka, Postgres, OPA, chain store).
  - **Load / stress / soak** behavior under sustained and bursty
    workloads, with backpressure semantics observed and bounded.
  - **Observability and SLO definition** so operators can run the
    system, not just developers.
  - **Multi-instance system-level correctness** — not just the unit-
    level multi-instance safety baselines (MI-1, MI-2) that are
    already proven, but the full system running with N runtime
    instances against shared infrastructure.

These four scopes are added to Phase 1.5 closure criteria. They are
**not optional**, and Phase 1.5 cannot be re-certified until all four
are implemented and proven with executable evidence.

## 3. EXPANDED PHASE 1.5 SCOPE — NEW SECTIONS

### §5.2.6 — Runtime Failure & Recovery Validation

**Purpose:** prove that the runtime degrades gracefully and recovers
correctly when its critical dependencies fail.

**Failure modes in scope (each MUST be validated):**

  1. **Kafka outage** — broker unreachable mid-publish.
  2. **Postgres connection failure** — pool unreachable / connection
     drops mid-transaction.
  3. **OPA unavailability** — policy evaluator unreachable.
  4. **Chain store unreachable** — IChainAnchor.AnchorAsync transport
     failure.

**Acceptance criteria (per failure mode):**

  - **A1. No data loss.** Every domain event that entered the runtime
    successfully must reach the outbox in `pending`, `published`, or
    `deadletter` terminal state. No event may be silently dropped.
  - **A2. No duplicate processing.** Repeated publish attempts must not
    produce duplicate Kafka messages for the same outbox row (subject
    to the at-least-once seam already documented in MI-2 at the
    Kafka-broker boundary).
  - **A3. Retry & DLQ semantics honored.** Failed rows must traverse
    the canonical retry → backoff → deadletter promotion path.
    `retry_count` must increment correctly. `next_retry_at` must
    enforce backoff. Deadletter promotion must occur exactly when
    `retry_count + 1 >= MaxRetry`.
  - **A4. Breaker behavior observable.** Where a circuit breaker
    exists for the dependency (PC-2 OPA breaker, TC-3 chain-store
    breaker), failure injection must trip the breaker, refusal must
    surface as the canonical RETRYABLE REFUSAL exception type, and
    recovery (dependency restored) must close the breaker within the
    declared window.
  - **A5. Recovery is automatic.** Once the failed dependency is
    restored, the runtime must resume normal operation without
    operator intervention or process restart.
  - **A6. Evidence is reproducible.** Each failure-mode test must
    run as part of the integration suite (gated on infrastructure
    availability) and produce a signed evidence record under
    `claude/audits/phase1.5/evidence/5.2.6/`.

**Required artifacts:**

  - `tests/integration/failure-recovery/OutboxKafkaOutageRecoveryTest.cs`
  - `tests/integration/failure-recovery/PostgresConnectionFailureRecoveryTest.cs`
  - `tests/integration/failure-recovery/OpaUnavailabilityRecoveryTest.cs`
  - `tests/integration/failure-recovery/ChainStoreUnreachableRecoveryTest.cs`
  - `claude/audits/phase1.5/evidence/5.2.6/<failure-mode>.evidence.md`
    per failure mode, recording test name(s), command line, output
    summary, and the specific A1–A5 invariants the run proved.

### §5.3 — Load / Stress (Burst) Testing

**SCOPE NOTE (amended 2026-04-09 in-session):** §5.3 in this Phase 1.5
re-open is **stress / burst validation only**, not long-duration soak.
The user explicitly narrowed this section to a 60-second sustained
burst — the goal is to prove immediate stability and backpressure
behavior, not long-horizon memory / connection / metric drift.

> **Long-duration soak (≥1 hour, RSS / leak / drift detection)
> is EXPLICITLY OUT OF SCOPE for Phase 1.5** and is deferred to a
> later phase. This section was originally scoped to include soak
> and was narrowed by direct user instruction. The narrower scope
> is recorded here verbatim per $5 anti-drift; do not re-introduce
> soak workloads under the §5.3 banner without an explicit further
> amendment.

**Purpose:** prove the runtime maintains stability and backpressure
correctness under a sustained burst workload.

**Workload in scope:**

  - **60-second sustained burst** — 1,000 RPS for 60 seconds against
    the canonical Todo command path through the runtime control plane
    (middleware → dispatcher → engine → event fabric → outbox seam).
    The harness drives commands through `TestHost` (the existing
    composition that wires every middleware in production order with
    in-memory persistence seams), so the same middleware / engine /
    fabric production code is exercised as in the rest of the
    integration suite.

**Acceptance criteria (expanded 2026-04-09 in-session per user instruction
to add post-load drain / Ready-state / breaker / stuck-message validation):**

During-load:

  - **L1. No request failure under load.** Every dispatched command
    must return a non-failure `CommandResult`. Zero exceptions
    escape the runtime control plane.
  - **L2. No duplicate processing.** Each command id appears at
    most once in the recorded persist log.
  - **L3. No data loss.** Every dispatched command produces the
    expected event(s) in the persistence seam (in-memory event store
    for the runtime-side test, real Postgres outbox for the
    Postgres-backed drain test).
  - **L4. Stable outbox behavior — no uncontrolled backlog.** Outbox
    pending depth grows in proportion to the workload but never
    diverges; on a Postgres-backed composition the publisher's drain
    rate keeps pace with the insert rate within bounded slack.
  - **L5. System remains in Ready state during load.** The
    `IRuntimeStateAggregator` reports `Healthy` (or at most
    `Degraded` for declared, expected reasons) at every sample point
    during the burst window. Never `NotReady`.

Post-load:

  - **L6. Outbox drains to zero post-load.** After dispatch stops
    and a bounded drain window elapses, the count of `pending`
    rows for the test correlation set is exactly zero. Every row
    reaches `published`.
  - **L7. No delayed retries or hidden failures.** After the drain
    window, the count of `failed` rows for the test correlation
    set is exactly zero. No row is silently retrying behind the
    scenes.
  - **L8. No stuck messages.** After the drain window, the count
    of `deadletter` rows for the test correlation set is exactly
    zero. (The §5.2.6 / FR-1 test exercises the deadletter path
    against an outage; §5.3 exercises the success path and asserts
    that path stays clean.)
  - **L9. No breaker stuck open.** No circuit breaker (PC-2 OPA,
    TC-3 chain-store) is open at the end of the test. For the
    §5.3 narrowed scope (which exercises only the outbox publish
    seam, not OPA or chain anchor), this is satisfied vacuously
    by the absence of any breaker-related exception in the test
    path; the assertion is recorded so a future test that DOES
    exercise OPA / chain inherits the same gate.

Diagnostic / wall-clock:

  - **L10. Evidence is reproducible.** Load harness committed under
    `tests/integration/load/`. Run record committed under
    `claude/audits/phase1.5/evidence/5.3/burst.evidence.md`
    recording workload parameters, total dispatched, distinct
    commands, event count, outbox state before / during / after,
    wall-clock, post-drain row counts, and pass/fail per L1–L9.

  - **L11. No immediate resource exhaustion.** Wall-clock budget
    (`targetDurationSeconds + drainWindowSeconds + 30s` slack)
    must not be exceeded — a stalled test surfaces as a budget
    overrun rather than a hang. Working-set growth is recorded as
    diagnostic only because the in-memory composition is *designed*
    to accumulate state across the burst window; soak / leak
    detection is explicitly out of scope.

**Out of scope for §5.3 in Phase 1.5:**

  - Long-duration soak (≥1 hour). Deferred.
  - HTTP API edge ingress (Kestrel + middleware pipeline at the
    edge). The harness drives the runtime control plane directly;
    edge-load is a separate workstream.
  - Real Kafka broker throughput. The outbox-publish dimension is
    proven by §5.2.6 / FR-1 against a stub producer; throughput
    against a real broker is a separate workstream.
  - SLO target enforcement. §5.3 acceptance criteria are operational
    floor checks, not SLO compliance — SLO targets are §5.4's
    responsibility and are explicitly operator-decided per the §5.4
    constraints.

**Required artifacts:**

  - `tests/integration/load/RuntimeBurstLoadTest.cs` — the harness
    itself, gated on `LoadTest__Enabled=true` so it does not run as
    part of the default integration suite. Contains TWO tests:
      1. `Runtime_Burst_1k_Rps_For_60s_Is_Stable` — drives the
         `RuntimeControlPlane` through `TestHost.ForTodo()` (in-memory
         composition) for L1, L2, L3, L5 coverage. Proves runtime-side
         stability under burst load.
      2. `Postgres_Outbox_Drains_Cleanly_After_Burst_Insert_Load` —
         seeds `pending` rows into the real Postgres outbox table at
         burst rate with the real `KafkaOutboxPublisher` running
         against a succeeding stub `IProducer`, then asserts L4, L6,
         L7, L8, L9 after the drain window. Proves outbox-side
         stability and clean drain behavior under burst load.
  - `claude/audits/phase1.5/evidence/5.3/burst.evidence.md` — signed
    evidence record per the L1–L11 invariants above.

### §5.4 — Observability & SLO Definition

**STATUS (in-session 2026-04-09):** ✅ **COMPLETE.** Marked done by
direct user statement: SLO scaffold, metric mapping, and runbook
templates were delivered by parallel work and meet all §5.4
requirements; no thresholds were invented; all gaps explicitly logged;
all mappings align with existing instrumentation.

**Purpose:** make the runtime operable. Define what "healthy" means in
quantified terms, what alerts fire when, and what an oncall responder
does about each.

**Metrics already in place** (catalogued, not invented; verified from
Phase 1.5 §5.2.x work):

  - `Whyce.Outbox` meter — `outbox.published`, `outbox.failed`,
    `outbox.deadlettered`, `outbox.dlq_published`
  - `Whyce.Postgres` meter — `postgres.pool.acquisitions`,
    `postgres.pool.acquisition_failures`, `postgres.pool.in_flight`
  - `Whyce.Chain` meter — `chain.anchor.wait_ms`, `chain.anchor.hold_ms`
  - `Whyce.EventStore` meter — `event_store.append.advisory_lock_wait_ms`,
    `event_store.append.hold_ms`, `event_store.replay_rows`
  - `Whyce.Workflow` meter — `workflow.admitted`, `workflow.rejected`

**Acceptance criteria:**

  - **O1. SLO document exists.** `claude/operations/slos/runtime.slo.md`
    defines per-pipeline-stage latency, error rate, and recovery
    time objectives. Targets MUST be supplied by the operator team
    and recorded as such — they are NOT to be invented by code.
  - **O2. Alert thresholds defined.** Each SLO has at least one alert
    threshold with `<warning>` and `<page>` tiers, derivation query,
    and the metric ID it consumes.
  - **O3. Runbook exists per alert.** Each alert has a corresponding
    runbook entry under `claude/operations/runbooks/` describing:
    symptoms, likely causes, diagnostic queries, remediation steps,
    rollback path, and the metric / log query that proves the
    incident is resolved.
  - **O4. Pipeline coverage.** Every stage of the canonical pipeline
    (API edge → runtime middleware → engine → event store → chain
    anchor → outbox → Kafka → projection) MUST have at least one
    metric exposing its health, and that metric MUST appear in at
    least one SLO.
  - **O5. Coverage proof.** A static check (script or audit) maps
    every pipeline stage to its observability surface and FAILs if
    any stage is unmapped.

**Required artifacts:**

  - `claude/operations/slos/runtime.slo.md`
  - `claude/operations/slos/outbox.slo.md`
  - `claude/operations/runbooks/<alert>.runbook.md` per alert
  - `claude/audits/phase1.5/observability-coverage.audit.md`

**Hard constraint:** SLO target VALUES (latency thresholds, RTOs,
failure budgets) are operator decisions. The amendment requires the
DOCUMENTS to exist and the TEMPLATES to be populated with metric
plumbing, but actual numeric targets MUST be supplied by the user
or marked `TARGET: <TBD by ops, owner: ___, due: ___>` until they
are. Inventing target numbers is forbidden under $5.

### §5.5 — Full System Multi-Instance Validation

**Purpose:** prove the entire runtime — not just unit-level seams —
behaves correctly with N>1 host processes against shared
infrastructure.

**Distinction from MI-1 / MI-2** (which are already proven and
remain valid):

  - MI-1 proves the *execution lock* primitive is multi-instance safe.
  - MI-2 proves the *outbox publish* SQL contract is multi-instance safe.
  - §5.5 proves the *full system* — composition of all the above plus
    every other shared seam — is multi-instance safe end-to-end.

**Acceptance criteria:**

  - **M1. Two-host steady-state.** N=2 host processes run against
    shared Kafka + Postgres + Redis + chain store, take traffic from
    a load harness, and converge on a consistent read model. Zero
    duplicate side effects, zero lost events.
  - **M2. Rolling restart.** While N=2 is taking traffic, restart
    one host. Inflight requests must either complete on the
    surviving host (via execution lock release + retry) or fail
    cleanly with a typed retryable error. Zero data loss across
    the restart.
  - **M3. Asymmetric failure.** Drop the network between one host
    and one dependency (e.g. Postgres). The healthy host must
    continue serving; the partitioned host must trip its breakers
    and refuse cleanly. Recovery on heal must be automatic.
  - **M4. Evidence is reproducible.** Multi-host topology defined
    in `infrastructure/deployment/multi-instance.compose.yml`;
    test driver under `tests/multi-instance/`; evidence under
    `claude/audits/phase1.5/evidence/5.5/`.

**Required artifacts:**

  - `infrastructure/deployment/multi-instance.compose.yml`
  - `tests/multi-instance/<scenario>.cs` per scenario
  - `claude/audits/phase1.5/evidence/5.5/<scenario>.evidence.md`

## 4. RE-CERTIFICATION GATE

Phase 1.5 may be re-certified COMPLETE only when **all four** of the
following are simultaneously true:

  1. Every acceptance criterion in §5.2.6, §5.3, §5.4, §5.5 above has
     a corresponding evidence record under
     `claude/audits/phase1.5/evidence/<section>/` with the test name,
     command line, output summary, and the specific invariants the
     run proved.
  2. The full integration test suite (post-TB-1 baseline) is green,
     including every new failure-recovery test added under §5.2.6.
  3. The §5.4 SLO documents exist and have been signed by the
     operator team (or carry explicit `TBD by ops` markers — but the
     documents themselves must exist, populated with the canonical
     metrics).
  4. A new `phase1.5-re-certification.audit.md` file is written
     summarising the above and is signed in the same format as the
     prior `phase1.5-final.audit.md`.

Until that re-certification record exists, Phase 1.5 status is
**RE-OPENED — IN PROGRESS**. No work on Phase 2 may begin.

## 5. EXECUTION PRIORITY ORDER

The new sections will be executed in this order, with each section
gated on the prior section's evidence existing:

  1. §5.2.6 — failure & recovery (begins immediately after this
     amendment is committed). Smallest scope, highest signal,
     unblocks the rest.
  2. §5.4 — SLO scaffolding (in parallel with §5.2.6 once §5.2.6
     evidence begins landing, because §5.3 acceptance criteria L1
     consume §5.4 outputs).
  3. §5.3 — load / stress / soak (after §5.4 targets are at least
     placeholder-defined).
  4. §5.5 — full-system multi-instance (last; depends on the
     stability proven in §5.2.6 + §5.3).

## 6. AUDIT TRAIL — DOCUMENTS THIS AMENDMENT SUPERSEDES

This amendment supersedes the *certification status* recorded in:

  - `claude/audits/phase1.5/phase1.5-final.audit.md` (status PASS
    → status SUPERSEDED, evidence preserved)

This amendment does NOT supersede the *test results, audit findings,
or evidence* contained in any of the eight phase1.5 audit module
files. Those remain valid as narrow correctness evidence for the
scopes they cover.

## 7. SIGNATURE BLOCK

**Amendment authored:** 2026-04-09
**Authority:** Direct user instruction, in-session, after explicit
review of the conflict between (a) the prior certification record
and (b) the user's stated requirement for resilience / operability
gates. The user explicitly invoked Path (a) of the four resolution
paths offered in the preceding turn.
**Compliance with $5 (anti-drift):** This amendment is an
**explicit user-authored scope expansion**, not an inferred or
extrapolated one. The new sections, their failure modes, their
acceptance criteria, and the re-certification gate are recorded
verbatim from the user's instruction so future executions can
distinguish "amended scope" from "drift."

**Phase 1.5 is RE-OPENED. §5.2.6 begins now.**
