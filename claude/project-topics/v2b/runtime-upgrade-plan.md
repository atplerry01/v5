# Runtime Enterprise Upgrade — Phased Execution Plan

**Source gap matrix:** [runtime-gap-matrix.md](runtime-gap-matrix.md)
**Source spec:** [runtime.md](runtime.md)
**Prompt store:** `claude/project-prompts/20260419-000000-runtime-enterprise-upgrade.md`
**Plan date:** 2026-04-19
**Scope:** 93 ABSENT + 101 PARTIAL features across 23 sections = 194 work items.

---

## 1 — Strategy

### Why phased execution

The gap matrix identifies 194 work items. Attempting them in a single pass would produce:
- Unreviewable diffs (thousands of LOC)
- Uncertified architectural choices (retry library, DLQ naming, lease backend)
- No enforceable exit criteria per work item
- High risk of drift against locked constraints (RO-CANONICAL-11, R11, determinism)

Phased execution forces explicit checkpoints, bounds blast radius per phase, and preserves WBSM $1a/$1b audit discipline.

### Dependency order

```
R1 Foundation hardening   (PARTIAL closures, no new architecture)
    ↓
R2 Resilience             (failure/outage/backpressure/lease/DLQ — biggest block)
    ↓
R3 Workflow & side-effects (depends on R2 retry/DLQ primitives)
    ↓
R4 Operator surface       (depends on R2 posture + R3 workflow inspection)
    ↓
R5 Certification          (exercises all of R1–R4)
```

Skipping order risks: R3 workflow suspend/resume requires R2 durable retry; R4 admin DLQ inspection requires R2 DLQ plumbing; R5 chaos tests require R2 outage handling to exercise.

### Guardrails for every phase

- All new middleware must fit the locked 11-stage pipeline — no reordering per `RO-CANONICAL-11`.
- No `Whycespace.Domain.*` imports in `src/runtime/` outside `event-fabric/domain-schemas/**` and `RuntimeCommandDispatcher.cs`.
- All IDs via `IIdGenerator`, all time via `IClock`, **all randomness via `IRandomProvider`** (introduced in R1 as contract, becomes mandatory for all new code from R2 onward — replay determinism breaks without it).
- Every new mutation path registers a WHYCEPOLICY policy and emits a domain event.
- Every persistence change respects R7 / R14 (outbox mandatory).
- Every phase ends with an audit sweep per $1b and captures any new drift rules under `claude/new-rules/` per $1c.
- **Policy-failure classification invariant** (defined in R1 docs, enforced in R2 retry logic): every policy evaluation failure resolves to exactly one of `FAIL_CLOSED` (reject), `FAIL_OPEN` (allow under explicit degraded posture), or `DEFER` (retryable with bounded attempts). Silent bypass under degraded mode = S0 violation. Infinite retry on "policy unavailable" = S0 violation.
- **R2 retry determinism invariant** (locked 2026-04-19 by user): any retry primitive MUST be fully replay-deterministic. Timestamps flow through `IClock`. Jitter flows through `IRandomProvider` seeded from operation coordinates (e.g. `$"{correlationId}:retry:{attempt}"`). No wall-clock reads in retry decision paths or in values that land in audit events. `Task.Delay` for the actual wait is permitted (replay does not re-execute sleeps) but the scheduled-at timestamp and jitter value written to `RetryAttemptedEvent` / `RetryExhaustedEvent` MUST be derived deterministically.

---

## 2 — Phase R1: Foundation Hardening

**Goal:** Close every PARTIAL in §1-5, §7, §8, §10, §16. No new architecture; no new libraries. Codify what's already informal. Add two pure-contract additions (`IRandomProvider`, policy-failure classification) that unblock R2.

**Effort:** ~3-5 focused sessions. Low risk, no architectural decisions needed.

**R1 adjustments (approved 2026-04-19):**
- **Removed** "Crash-safe lock/lease recovery" — implicitly defines lease model; moved to R2.C where the backend decision lives.
- **Added** `IRandomProvider` contract in `src/shared/kernel/domain/` — pure seam, no heavy use in R1. Mandatory from R2 onward for retry jitter, rebalance decisions, breaker half-open probes.
- **Added** "Policy-failure classification rule" (FAIL_CLOSED / FAIL_OPEN / DEFER) documented in R1 (no implementation), enforced by R2 retry logic.

### In-scope PARTIALs + R1 additions (37 items)

| § | Item | Approach |
|---|---|---|
| 1 | Explicit short-running vs long-running paths | Codify via attribute/marker on command type; dispatcher routes by marker |
| 2 | Runtime exception mapping discipline | `RuntimeExceptionMapper` service; canonical mapping enum |
| 2 | Runtime response shaping discipline | Extend `RuntimeResult` with canonical envelope shape |
| 3 | Stable retry behavior | Inventory current retry sites; document invariants (no code yet — R2 replaces) |
| 3 | Deterministic side-effect gating | Audit middleware ordering to confirm side effects gated on `CommandContext.ShouldEmitSideEffects` flag |
| 3 | Determinism verification tests | Expand `CommandContextReplayResetTests` coverage to all middleware stages |
| 3 | Replay determinism certification | Formal replay checklist as `runtime.audit.md` sub-section |
| 4 | Runtime-local state boundary discipline | `StateBoundaryGuard` — compile-time check that no middleware holds static mutable state |
| 5 | Semantic validation | Introduce `ISemanticValidator<TCommand>` contract in `pre-policy/ValidationMiddleware.cs` |
| 5 | State-transition eligibility validation | Extend `ExecutionGuardMiddleware` to consult `IStateTransitionValidator` per command |
| 5 | Business invariant validation before mutation | Codify invariant-check step already present in aggregates; audit handler entry |
| 5 | Dependency readiness validation | Wire `IRuntimeStateAggregator.IsReady()` as admission gate input |
| 5 | Validation failure classification | `ValidationFailureCategory` enum in `src/runtime/contracts/` |
| 6 | Jurisdiction/environment overlays | Extend `PolicyInputBuilder` to include `input.environment` + `input.jurisdiction` |
| 6 | Threshold-based enforcement | Document existing policy thresholds; formal test coverage |
| 7 | Session/token context propagation | Extend `RuntimeExecutionContext` with session-token fields |
| 7 | Sensitive-operation authorization rules | Sensitive-op marker + elevated authorization path |
| 7 | Administrative-operation separation | Admin commands require distinct policy scope |
| 8 | Duplicate message tolerance | Audit consumer handlers for idempotent consumption tags |
| 8 | Duplicate side-effect prevention | Side-effect idempotency key derivation convention |
| 8 | Idempotent workflow step execution | Step-level idempotency key on workflow step payload |
| 8 | Idempotent outbox publication discipline | Audit `OutboxService.cs` for deduplication guarantees |
| 8 | Canonical duplicate response behavior | `RuntimeResult.AlreadyProcessed(previousResult)` |
| 8 | Idempotency evidence tracking | Surface idempotency hit/miss in `CommandContext` + metrics |
| 9 | Aggregate version checking | Audit `EventFabric` aggregate-version check path; surface version mismatch as dedicated failure code |
| 9 | Conflict detection and safe rejection | `ConcurrencyConflictException` → `RuntimeResult` mapping |
| 9 | Per-workflow sequencing discipline | Workflow step sequence guard in `WorkflowStepExecutor` |
| 9 | Concurrency stress validation | Concurrency stress test suite (no chaos — pure load) |
| 10 | Persistence failure classification | `PersistenceFailureCategory` enum + mapping |
| 10 | Partial-write prevention | Audit outbox+eventstore atomicity; add assertion |
| 10 | Safe retry after persistence failure | Inventory — defer primitives to R2 |
| 10 | Durable state before external side effects | Confirm via audit; write test |
| 10 | Persistence observability | Postgres/outbox metrics: rows written, fsync latency |
| 14 | Canonical failure taxonomy | `RuntimeFailureCategory` / `ValidationFailureCategory` / `PersistenceFailureCategory` enums in `src/shared/contracts/runtime/` (co-located with `CommandResult` — runtime→shared dependency direction forced this over the originally-planned runtime-layer location). Foundation for R2. |
| 16 | Runtime failure audit trail | Failure events emitted via EventFabric on every rejection |
| R1+ | `IRandomProvider` contract | `src/shared/kernel/domain/IRandomProvider.cs` + `DeterministicRandomProvider` implementation in host composition (seeded per execution). No callers in R1. |
| R1+ | Policy-failure classification rule | Drift rule captured under `claude/new-rules/` defining FAIL_CLOSED / FAIL_OPEN / DEFER enum; no implementation in R1 — R2 retry logic consumes it. |

### Entry criteria
- Gap matrix reviewed and approved.
- This plan reviewed and approved.
- R1 work-item list confirmed with user.

### Exit criteria
- All 36 PARTIALs re-evaluated — status must be PRESENT in a re-run of the gap survey.
- New `runtime.guard.md` rules added for: `RuntimeExceptionMapper`, `ValidationFailureCategory`, `PersistenceFailureCategory`, `RuntimeFailureCategory`.
- New `runtime.audit.md` checklist entries for each rule above.
- All existing tests pass; new unit tests cover every new enum/service.
- Full `$1b` audit sweep returns zero new violations.

### Architectural decisions
- **None required.** R1 only codifies what exists or adds pure enums/contracts.

---

## 3 — Phase R2: Resilience

**Goal:** Close the 93 ABSENT features clustered in §9 concurrency, §11 event-fabric DLQ/retry/rebalance, §14 failure handling, §18 health, §19 multi-instance, §20 backpressure.

**Effort:** The largest phase. ~10-15 focused sessions.

**Risk:** Requires architectural decisions — must not start until the user signs off on the decision log (§7).

### In-scope ABSENT features (grouped by subsystem)

**R2.A — Retry / backoff / dead-letter plumbing (§11, §14)**
- Retry scheduling
- Backoff policy with jitter
- Retry exhaustion handling
- Dead-letter transition rules
- DLQ routing / poison-message isolation / re-drive support
- Consumer deduplication where needed
- Idempotency expiry/retention rules
- Poison-work-item handling

**R2.B — Outage handling (§6, §11, §14, §18)**
- Broker outage handling
- Recovery-after-broker-outage behavior
- Recovery-after-database-outage behavior
- Recovery-after-policy-outage behavior
- Policy dependency health awareness
- Policy outage handling discipline
- Policy fallback posture rules
- Kafka health posture
- Policy engine health posture
- Chain/evidence dependency posture

**R2.C — Concurrency primitives (§9, §19)**
- Single-writer protection
- Lease-based coordination
- Invariant-safe command serialization
- Distributed lease coordination
- Multi-instance consumer safety
- Duplicate worker protection
- Leader/worker separation
- Partition/assignment safety
- Horizontal scaling constraints documented
- **Crash-safe lock/lease recovery** (moved from R1 — depends on D3 backend decision)

**R2.D — Backpressure & circuit protection (§15, §20)**
- Circuit-breaker patterns
- External dependency circuit protection
- Slow-dependency protection
- Resource-based posture downgrading
- Duplicate external-call prevention
- Safe third-party timeout handling

**R2.E — Consumer fabric hardening (§11, §12)**
- Consumer lag awareness
- Consumer health awareness
- Consumer rebalance safety
- Safe consumer-group rebalance behavior
- Partition ownership discipline
- Hot-partition / hot-key detection
- Partition skew visibility
- Topic provisioning alignment
- Commands/events/retry/dead-letter topic separation (close the PARTIAL)

### Entry criteria
- R1 exit criteria met.
- Decision log (§7 below) signed off by user.
- Guard additions for new primitives drafted in `claude/new-rules/` before code lands.

### Exit criteria
- Chaos test suite covers Kafka outage, Postgres outage, policy engine outage, Redis outage.
- DLQ round-trip test passes: fail → DLQ → inspect → re-drive → succeed.
- Multi-instance test passes: start 3 replicas, kill leader mid-command, no duplicate side effect.
- Circuit breaker opens under synthetic slow-dependency; admission control sheds load; health posture transitions DEGRADED → HEALTHY on recovery.
- `runtime.guard.md` and `infrastructure.guard.md` updated with new rules for: retry policy, DLQ naming convention, lease protocol, circuit breaker invariants.
- All §11 and §14 rows in the gap matrix re-survey to PRESENT.

### Architectural decisions required → see §7 Decision Log

---

## 4 — Phase R3: Workflow Runtime & External Side-Effect Control

**Goal:** Close §13 workflow runtime gaps and §15 external side-effect discipline. Depends on R2 retry/DLQ primitives.

**Effort:** ~5-8 focused sessions.

### In-scope

**R3.A — Workflow runtime primitives (§13)**
- Workflow timeout handling (PARTIAL → PRESENT)
- Workflow retry handling (uses R2 primitives)
- Workflow suspend / resume
- Workflow cancellation (PARTIAL → PRESENT)
- Human-approval wait-state support
- Exception-path handling
- Workflow observability

**R3.B — External side-effect discipline (§15)**
- Webhook/API call retry discipline (uses R2 retry primitives)
- External confirmation tracking
- External failure classification
- Duplicate external-call prevention (uses R2 idempotency primitives)
- Safe third-party timeout handling
- Side-effect observability (PARTIAL → PRESENT)
- Side-effect auditability (PARTIAL → PRESENT)

### Entry criteria
- R2 exit criteria met — retry/DLQ/lease primitives available.

### Exit criteria
- Durable workflow with suspend/resume passes survival test (kill runtime mid-step, resume on restart).
- Human-approval wait state integrated with policy (approval → policy decision → resume).
- External side-effect ledger table populated for every outbound call.
- §13 and §15 rows re-survey to PRESENT.

### Architectural decisions required
- Workflow definition format (YAML? C# fluent? already established?).
- Human-approval storage (dedicated table vs generic aggregate).
- External side-effect ledger retention.

---

## 5 — Phase R4: Operator Surface, Observability, Security

**Goal:** Close §17 SLO/dashboard/alerting gaps, §21 admin controls, §22 security hardening.

**Effort:** ~5-8 focused sessions.

### In-scope

**R4.A — Observability completion (§17)**
- Retry / DLQ / outbox / projection-lag / partition / consumer-group / dependency-health metrics
- Runtime posture metrics (PARTIAL → PRESENT)
- Dashboard support (Grafana/Prometheus dashboards under `infrastructure/observability/`)
- Alerting support (Prometheus alert rules)
- SLO/SLA instrumentation (`SloInstrument` wrapper on pipeline)
- Structured logs (PARTIAL → PRESENT)

**R4.B — Admin controls (§21)**
- Pause/resume controls
- Retry/re-drive controls
- DLQ inspection controls
- Workflow inspection controls
- Execution inspection controls
- Safe override controls
- Runtime feature-flag governance
- Operator audit logging

**R4.C — Security hardening (§22)**
- Secret-safe dependency access
- Secure configuration loading
- Payload size / abuse controls
- Bot-hostile runtime posture
- Malicious automation resistance hooks
- Sensitive-operation confirmation pathways
- Service identity hardening (PARTIAL → PRESENT)
- Internal boundary hardening (PARTIAL → PRESENT)

**R4.D — Audit surface (§16)**
- Operator-action audit trail
- Searchable runtime evidence
- Evidence completeness checks

### Entry criteria
- R2 + R3 exit criteria met.

### Exit criteria
- Operator admin API: pause, resume, re-drive, DLQ inspect, workflow inspect — all behind elevated-authorization policy, all logged to operator audit trail.
- Grafana dashboards show all required metrics; alert rules trigger in staging.
- SLO error budget reporting visible.
- §17, §21, §22 rows re-survey to PRESENT.

### Architectural decisions required
- Metrics backend (Prometheus? already chosen?).
- Admin API surface (separate host? same host, separate policy scope?).
- Feature-flag backend (if any — could be config-file based).

---

## 6 — Phase R5: Certification

**Goal:** Close §23 testing gaps. Formal go/no-go checklist.

**Effort:** ~5 focused sessions, plus test-environment setup.

### In-scope
- Replay determinism tests (PARTIAL → PRESENT)
- Idempotency tests (PARTIAL → PRESENT)
- Concurrency tests (PARTIAL → PRESENT)
- Multi-instance tests
- Kafka / Redis / Postgres / OPA failure tests
- Timeout / cancellation tests (PARTIAL → PRESENT)
- DLQ / retry tests
- Recovery / restart tests (PARTIAL → PRESENT)
- Projection rebuild tests
- Load / stress / soak / burst tests
- Real-data runtime validation
- Formal runtime go/no-go checklist

### Entry criteria
- R1–R4 exit criteria met.

### Exit criteria
- `tests/certification/runtime/` contains full suite; CI green.
- Load test at production-target throughput sustains for 24h.
- Soak test 72h with chaos injection: no unbounded memory, no deadlocks, no missed events.
- Formal checklist produced at `claude/certification/runtime-enterprise-certification.md`.

### Architectural decisions required
- Chaos injection framework (Toxiproxy? Pumba? k6? — likely infrastructure choice not runtime).
- Load test target numbers (TPS, p99, max workflow count).

---

## 7 — Decision Log

**Status as of 2026-04-19:**
- **LOCKED NOW** (influence R1 shape even if implementation comes later): D1, D2, D4.
- **DEFERRED** until after R1: D3, D5, D6, D7, D8.

### D1 — Retry / backoff library · **LOCKED → (b) Custom `IRetryExecutor`**

**Question:** What primitive do new retry paths use?
**Options:**
- (a) **Polly** (industry standard, mature, but external dep).
- (b) **Custom `IRetryExecutor`** on top of `IClock` (no external dep, deterministic, matches project determinism stance).
- (c) Hybrid — Polly as implementation behind a custom `IRetryExecutor` abstraction (replaceable later).

**Recommendation:** (b) custom. Matches your determinism posture (`IClock` and `IIdGenerator` seams already exist) and avoids wrapping an external clock.

**Decision:** (b) Custom `IRetryExecutor` built on `IClock` + the new `IRandomProvider` (for jitter). Lives in `src/runtime/resilience/` (new subdirectory created in R2).

### D2 — DLQ topic naming convention · **LOCKED → (c) Tiered `{topic}.retry` → `{topic}.dlq`**

**Question:** How are DLQ topics named and governed?
**Options:**
- (a) `{original-topic}.dlq` suffix (simple, widely used).
- (b) Separate `whycespace.dlq.{domain}` namespace (centralized inspection).
- (c) Tiered: `{original}.retry` → `{original}.dlq` (retry happens before DLQ).

**Recommendation:** (c) tiered. Aligns with the spec's explicit "Commands / events / retry / dead-letter separation" (§11) and matches Kafka best practice.

**Decision:** (c) tiered. Naming convention: producer topic `X.events` → retry topic `X.retry` → dead-letter topic `X.deadletter`. Codebase already uses `.deadletter` (not `.dlq`) per phase1.6-S1.6 / DLQ-RESOLVER-01; aligning D2 with that convention avoids a topic-rename migration. `TopicNameResolver` extends to emit the tiered names deterministically (R2.A.3a).

### D3 — Distributed lease backend · **LOCKED → (b) Postgres advisory locks** (2026-04-19)

**Question:** What backs `IDistributedLease`?
**Options:**
- (a) **Redis** with `SETNX` + TTL (existing Redis dependency, fast).
- (b) **Postgres** advisory locks (no new infrastructure).
- (c) **Kubernetes leases** (if running on k8s — may not match dev workflow).

**Recommendation:** (b) Postgres advisory locks first. Zero new infrastructure, transactional alignment with outbox, adequate for <1000 RPS. Re-evaluate at scale.

**Decision:** (b) Postgres advisory locks. Rationale (user 2026-04-19): already depends on Postgres for outbox + durability; advisory locks are transaction-aware (matters for command-execution boundaries + avoiding split-brain during retries); zero new operational surface during R2; Redis SETNX introduces clock/TTL drift, weaker transactional coupling, and another failure domain. Re-evaluate if contention exceeds Postgres capacity.

### D4 — Circuit-breaker scope · **LOCKED → (a) Per-dependency**

**Question:** Per-dependency or per-operation?
**Options:**
- (a) Per-dependency (one breaker per Kafka broker, one per OPA, one per Postgres pool).
- (b) Per-operation (one breaker per outbound command type).

**Recommendation:** (a) per-dependency. Matches §18 health posture model; fewer breakers to tune.

**Decision:** (a) one circuit breaker per dependency (Kafka, Postgres pool, OPA / policy engine, chain anchor service, Redis). Breaker state feeds `IRuntimeStateAggregator` so health posture and breaker state converge.

### D5 — Health posture metrics backend · **LOCKED → (a) Prometheus + Grafana** (2026-04-19)

**Question:** Where do posture / SLO metrics land?
**Options:**
- (a) Prometheus + Grafana (assumed from existing `HttpMetricsMiddleware` + `MetricsMiddleware`).
- (b) Something else already chosen that I haven't seen.

**Recommendation:** Confirm (a) — if Prometheus is correct, R4.A can proceed without new infra.

**Decision:** (a) Prometheus + Grafana. Rationale (user 2026-04-19): existing `HttpMetricsMiddleware` + `MetricsMiddleware` already point at Prometheus via `System.Diagnostics.Metrics`. R2 + R4 can emit immediately, no abstraction layer needed. Standardize metric naming early.

### D6 — Admin surface host · **DEFERRED until R1 complete**

**Question:** Does admin API share the main API host with a separate scope, or run as a separate host process?
**Options:**
- (a) Same host, new route prefix `/admin/*`, admin-only policy scope.
- (b) Separate admin host (stronger isolation, more infra).

**Recommendation:** (a) same host with policy separation. Matches "Administrative separation from public access surface" (§21, currently PARTIAL) via policy, not network.

### D7 — Chaos test framework · **DEFERRED until R1 complete**

**Question:** What chaos tooling for R5?
**Options:**
- (a) Toxiproxy (TCP-level faults, mature).
- (b) Pumba (container-level, needs Docker).
- (c) Custom fault injection inside test harness.

**Recommendation:** (a) Toxiproxy for Kafka/Postgres/Redis link faults + (c) custom injection for policy/OPA. Pumba only if already running Docker in CI.

### D8 — Workflow definition format (R3 precondition) · **DEFERRED until R1 complete**

**Question:** How are workflows authored?
**Options:**
- (a) C# fluent (already established pattern visible in `src/engines/T1M/`).
- (b) YAML + interpreter.
- (c) Hybrid — YAML for step graph, C# for step bodies.

**Recommendation:** (a) C# fluent — matches existing `WorkflowEngine` / `WorkflowStepExecutor`.

---

## 8 — Effort Summary

| Phase | Items | Estimated Sessions | Risk | Architectural Decisions |
|---|---|---|---|---|
| R1 Foundation | 36 PARTIAL | 3-5 | Low | None |
| R2 Resilience | ~50 ABSENT + 20 PARTIAL | 10-15 | **High** | D1–D4 |
| R3 Workflow/Side-effects | ~18 items | 5-8 | Medium | D8 |
| R4 Ops / Observability / Security | ~30 items | 5-8 | Medium | D5–D6 |
| R5 Certification | ~15 items | 5+ | Medium | D7 |
| **Total** | **194** | **~30-45 sessions** | — | **8 decisions** |

---

## 9 — Immediate Next Step

**Before any code changes:**
1. User reviews this plan and confirms R1 scope (36 PARTIALs listed above).
2. User decides D1–D8 OR explicitly defers D1–D4 until R1 is complete (R1 does not require those decisions).
3. User approves the guard/audit update path (new rules land first in `claude/new-rules/`, then promoted to `runtime.guard.md` / `runtime.audit.md`).

Once confirmed, R1 execution can start safely.
