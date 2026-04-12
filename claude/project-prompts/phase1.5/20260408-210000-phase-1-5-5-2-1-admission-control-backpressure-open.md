# Phase 1.5 §5.2.1 — Admission Control and Backpressure (Workstream Opening Pack)

## TITLE
Phase 1.5 §5.2.1 Admission Control and Backpressure — canonical workstream opening pack.

## CLASSIFICATION
system / runtime / admission-control-backpressure

## CONTEXT
The Phase 1.5 §5.1.x structural hardening series closed on 2026-04-08:
- §5.1.1 Dependency Graph Remediation — **PASS** (2026-04-08)
  ([20260408-143500-phase-1-5-5-1-1-pass-closure.md](20260408-143500-phase-1-5-5-1-1-pass-closure.md)).
- §5.1.2 Boundary Purity Validation — **PASS** (2026-04-08)
  ([20260408-190000-phase-1-5-5-1-2-pass-closure.md](20260408-190000-phase-1-5-5-1-2-pass-closure.md)).
- §5.1.3 Canonical Documentation Alignment — **PASS** (2026-04-08)
  ([20260408-200000-phase-1-5-5-1-3-canonical-documentation-alignment-open.md](20260408-200000-phase-1-5-5-1-3-canonical-documentation-alignment-open.md)).

The repository is now structurally clean: the dependency graph is
verified, layer purity is enforced, and the canonical documentation
surface is aligned with reality. The next class of risk is
**operational** rather than **structural**: the runtime has not yet
been certified to behave safely under load. Specifically, the system
has no proven discipline around *which* work it accepts, *how much*
work it will hold in flight, *what* it does when saturated, and
*how* backpressure propagates across the persistence, messaging,
projection, and policy paths.

§5.2.1 Admission Control and Backpressure is the **first** workstream
in the Phase 1.5 §5.2.x **Runtime Infrastructure-Grade Hardening**
cluster. It is the precondition for any subsequent throughput,
soak, stress, or chaos certification: until admission and
backpressure are defined and observable, load testing measures
nothing — saturation will collapse the system in undefined ways
and the resulting numbers will not be meaningful.

This artifact is the **opening pack only**. No remediation work is
performed here. No source, guard, audit, script, configuration, or
README file is modified. The workstream is created in `OPEN` state
and handed off for execution in subsequent prompts.

---

## 1. EXECUTIVE SUMMARY

§5.2.1 Admission Control and Backpressure verifies that the
Whycespace runtime can **bound** the work it accepts, **shed** load
it cannot safely process, **propagate** backpressure across the
persistence / messaging / projection / policy paths, and **observe**
all of the above with reproducible evidence. Where §5.1.x asked
*“is the code structurally correct?”*, §5.2.1 asks *“does the
running system stay within its declared envelope when pushed, and
does it fail in declared ways rather than undefined ones?”*.

The workstream produces, for every runtime entry boundary, a
declared admission policy with three explicit response classes:
**REJECT** (terminal refusal — caller must not retry as-is),
**RETRYABLE REFUSAL** (transient refusal — caller may retry with
backoff), and **DEGRADED ACCEPTANCE** (work accepted on a reduced
path, e.g. async deferral, sampling, or shed-non-critical). It also
defines the queue, buffer, and in-flight bounds for every async
seam in the runtime; the saturation signal each seam emits; the
host responsibilities for load-shedding vs. processing; and the
observability required to *prove* bounded behavior end-to-end.

The deliverable is an evidence-backed runtime admission and
backpressure specification, a remediation patch list against any
seam that is currently unbounded or silently buffered, and a set
of acceptance probes that future §5.2.x throughput, soak, and
stress workstreams can call as preconditions. Initial status:
**OPEN**.

---

## 2. WORKSTREAM DEFINITION

### 2.1 Purpose
Ensure the runtime can reject, defer, or bound excess work safely
instead of allowing uncontrolled queue growth, saturation cascades,
or undefined collapse under load. Establish admission-control and
backpressure discipline as a runtime-infrastructure invariant
before any throughput, soak, or stress certification is attempted.

### 2.2 Objective
Produce, for every runtime entry boundary and every internal async
seam in scope, an evidence-backed determination of:
1. The **declared admission policy** (REJECT / RETRYABLE REFUSAL /
   DEGRADED ACCEPTANCE) with explicit thresholds.
2. The **bound** (queue depth, in-flight count, buffer size, byte
   budget, time budget) and how it is enforced.
3. The **saturation signal** (metric, log line, event, or
   structured outcome) that proves the bound is operating.
4. The **propagation path** by which backpressure from a downstream
   seam reaches the runtime entry boundary.
5. The **gap** between the current implementation and the declared
   target, captured as a remediation patch list with severity per
   $16 and an acceptance probe per item.

### 2.3 Why This Matters Before Phase 2
- Phase 2 expansion will introduce real workload, real ingestion
  rates, and real upstream pressure. A runtime that buffers
  silently or collapses under saturation will fail Phase 2
  immediately, and worse, will fail in ways that look like
  application bugs rather than capacity bugs.
- Future §5.2.x workstreams (throughput certification, 1k RPS soak,
  stress, chaos) **measure nothing meaningful** until admission
  and backpressure are defined. Load testing an unbounded system
  reports the moment it broke, not the envelope it can hold.
- WHYCEPOLICY $8 requires every operation to flow through policy.
  Admission control is the first place policy can refuse work; if
  the runtime cannot refuse safely, policy itself becomes
  advisory rather than load-bearing.
- Determinism $9 requires reproducible execution. A system whose
  failure mode under load is undefined is, by definition, not
  deterministic at the operational layer.
- Event-sourcing $10 places the persistence and outbox paths on
  the critical write path. If those paths cannot push back, the
  event log itself becomes the unbounded buffer — which is the
  worst possible place for one to live.
- The §5.1.x structural hardening series produced a *clean*
  system. §5.2.1 is the first step in proving it is also a
  *survivable* system.

### 2.4 Known Overload / Backpressure Risk Areas
- **B1** — Runtime entry boundary (HTTP / API / command intake)
  has no declared per-tenant or global admission limit; bursts
  are absorbed into the request thread pool with no shedding.
- **B2** — `GenericKafkaProjectionConsumerWorker` consume loop
  may not bound in-flight handler work; a slow projection writer
  could let the consumer prefetch unboundedly.
- **B3** — `PostgresOutboxAdapter` publish loop and outbox
  drain — what happens when the broker is slow or unavailable
  and the outbox table grows without bound? Is there a declared
  high-water-mark policy?
- **B4** — `PostgresEventStoreAdapter` append path under write
  contention — does the runtime apply a queue-depth or
  in-flight-append bound, or does it serialize unboundedly?
- **B5** — `PostgresProjectionWriter` apply path under
  projection lag — what is the policy when projection lag
  exceeds N events / N seconds? Catch-up vs. shed?
- **B6** — Workflow / orchestration intake (`WorkflowExecutionBootstrap`,
  intent handlers such as `TodoIntentHandler`) — is there a
  bound on concurrently in-flight workflow instances per tenant
  or per workflow type?
- **B7** — Constitutional policy evaluation
  (`ConstitutionalPolicyBootstrap`) — what happens when policy
  evaluation itself becomes the bottleneck? Does the runtime
  shed, queue, or block?
- **B8** — DLQ / retry topics — what bounds the DLQ growth, and
  what is the saturation signal when the DLQ is itself saturated?
- **B9** — Internal `Channel<T>` / `BlockingCollection<T>` /
  `ConcurrentQueue<T>` instances anywhere in the runtime —
  every unbounded channel is a latent silent buffer.
- **B10** — `Task.Run` / `_ = Task.Run` / fire-and-forget
  patterns — every fire-and-forget is an admission decision
  taken implicitly without a bound.
- **B11** — Backpressure propagation: when a downstream seam
  saturates (e.g. projection writer slow), is the saturation
  signal observable at the runtime entry boundary, or does the
  pressure build invisibly until something breaks?
- **B12** — Distinction between `REJECT` (terminal),
  `RETRYABLE REFUSAL` (transient, retry-with-backoff), and
  `DEGRADED ACCEPTANCE` (accepted on reduced path) — is this
  distinction expressible in the runtime's response surface, or
  does the system collapse rejection categories into a single
  `500` / generic error?
- **B13** — Host responsibilities: what is the host doing during
  saturation — is it shedding, processing, queuing, or
  oscillating? Is the host itself observable in this state?
- **B14** — Determinism interaction: does the admission /
  shedding decision become non-deterministic under load (e.g.
  random shedding, wallclock-based shedding via
  `DateTime.UtcNow` instead of `IClock`)?
- **B15** — Policy interaction: does WHYCEPOLICY $8 evaluate
  *before* admission shedding (so shed work is still
  policy-decided) or *after* (so shedding is itself a
  policy-bypass)? The correct answer must be declared.

### 2.5 Scope
- `src/platform/host/Whycespace.Host.csproj` and the host
  composition surface — runtime entry, hosted services, and
  background-worker registration.
- `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs`
  — consume loop, prefetch, in-flight handler bounds.
- `src/platform/host/adapters/PostgresEventStoreAdapter.cs`
  — append path, contention behavior, in-flight append bounds.
- `src/platform/host/adapters/PostgresOutboxAdapter.cs`
  — outbox drain, broker-slow behavior, high-water-mark policy.
- `src/platform/host/adapters/PostgresProjectionWriter.cs`
  — apply path, projection-lag policy.
- `src/platform/host/composition/**` — every bootstrap module
  that wires a runtime entry, worker, or async seam, including
  `constitutional/policy/ConstitutionalPolicyBootstrap.cs`,
  `operational/sandbox/todo/TodoBootstrap.cs`, and
  `orchestration/workflow/WorkflowExecutionBootstrap.cs`.
- `src/runtime/**` — every channel, queue, buffer, hosted
  service, and background loop. Specific attention to
  `event-fabric/EventDeserializer.cs`,
  `event-fabric/EventSchemaRegistry.cs`, and
  `projection/IPostgresProjectionWriter.cs`.
- `src/systems/downstream/**` intent handlers (e.g.
  `TodoIntentHandler.cs`) — admission boundary at the
  intent-handler entry.
- `claude/guards/runtime.guard.md` — current runtime guard
  rules, to confirm 5.2.1 acceptance probes do not contradict
  any locked rule.
- Configuration surfaces (`appsettings*.json`,
  `IConfiguration` bindings) that already externalize bounds
  (per phase1.6-S1.5 the OutboxPublisher MAX_RETRY was
  externalized — that pattern is the precedent).

### 2.6 Non-Scope
- §5.1.1 / §5.1.2 / §5.1.3 (closed) re-verification.
- §5.2.2+ runtime-infrastructure workstreams (idempotency,
  retry semantics, DLQ closure, failure-domain isolation,
  graceful shutdown) — these are sibling §5.2.x workstreams
  and will be opened separately.
- §5.3.x throughput certification, §5.3.x soak, §5.3.x stress,
  §5.3.x chaos — these consume §5.2.1 as a precondition but
  are not in scope here.
- §5.4.x security, §5.5.x governance.
- Generic performance tuning (CPU profiling, allocation
  reduction, GC tuning, query optimization). §5.2.1 is about
  **safety** under load, not **speed** under load.
- Capacity planning, hardware sizing, autoscaling policy.
- Domain-layer changes. The domain layer has zero dependencies
  per $7 and is not an admission-control surface.
- Engine-layer changes beyond confirming statelessness per $7.
- Re-litigating any locked rule (DG-R5-EXCEPT-01,
  DG-R5-HOST-DOMAIN-FORBIDDEN, R-DOM-01, etc.).

### 2.7 Remediation Strategy
1. **Inventory** — enumerate every runtime entry boundary and
   every internal async seam in scope; classify each as
   **BOUNDED** (already has a declared bound and observable
   saturation signal), **UNBOUNDED** (no bound or only an
   implicit one), or **UNKNOWN** (cannot be determined from
   reading the code alone).
2. **Probe** — for each seam, define a reproducible probe that
   answers: (a) what is the declared bound, (b) what is the
   enforcement mechanism, (c) what is the saturation signal,
   (d) what is the propagation path to the entry boundary,
   (e) which response class (REJECT / RETRYABLE REFUSAL /
   DEGRADED ACCEPTANCE) applies on saturation.
3. **Classify** — every probe result is `BOUNDED-OBSERVABLE`,
   `BOUNDED-OPAQUE` (bounded but no observable saturation
   signal), `UNBOUNDED-IMPLICIT` (no declared bound but
   limited by an incidental property such as a thread-pool
   size), or `UNBOUNDED-OPEN` (no bound at all).
4. **Triage** — assign severity per $16: S0 system-breaking
   (unbounded on a critical write path), S1 architectural
   (no declared admission policy at a runtime entry), S2
   structural (bounded but opaque or non-observable), S3
   formatting / cosmetic.
5. **Patch list** — non-`BOUNDED-OBSERVABLE` findings become
   a remediation patch list with file paths, intended edit,
   acceptance probe, and externalized configuration shape
   (following the phase1.6-S1.5 OutboxPublisher precedent).
   No inline edits during the audit pass itself.
6. **Specification** — produce the canonical Admission Control
   and Backpressure specification: per-seam declared policy,
   bound, signal, propagation, and response class. This becomes
   the precondition document for §5.3.x throughput / soak /
   stress / chaos workstreams.
7. **Promote** — execution and remediation occur in follow-up
   prompts; this opening pack ends at the patch-list and
   specification handoff.

### 2.8 Task Breakdown
- **T-A** Runtime seam inventory — enumerate every entry
  boundary and async seam in §2.5 scope; classify each as
  BOUNDED / UNBOUNDED / UNKNOWN by initial reading.
- **T-B** Probe matrix — define probes per seam covering
  declared bound, enforcement mechanism, saturation signal,
  propagation path, and response class. Each probe declares
  its expected `BOUNDED-OBSERVABLE` shape.
- **T-C** Probe execution — run the probe matrix against the
  current tree (static analysis, configuration inspection,
  targeted code reading); capture verbatim raw evidence.
- **T-D** Classification — assign `BOUNDED-OBSERVABLE` /
  `BOUNDED-OPAQUE` / `UNBOUNDED-IMPLICIT` / `UNBOUNDED-OPEN`
  with one-line justification per probe.
- **T-E** Severity triage — assign S0/S1/S2/S3 to every
  non-`BOUNDED-OBSERVABLE` finding per $16.
- **T-F** Patch list — produce remediation patch list with
  file paths, intended edit, acceptance probe, and
  externalized configuration shape per item.
- **T-G** Response-class taxonomy — declare, per runtime
  entry boundary, the mapping from saturation cause to
  response class (REJECT / RETRYABLE REFUSAL / DEGRADED
  ACCEPTANCE) and the wire-level / API-level shape of each.
- **T-H** Backpressure propagation map — for every downstream
  seam, trace the path by which its saturation signal reaches
  the runtime entry boundary; flag every silent break in the
  chain.
- **T-I** Policy and determinism interaction check — confirm
  WHYCEPOLICY $8 evaluation order vs. admission shedding,
  and confirm shedding decisions are deterministic per $9
  (no `Guid.NewGuid`, no `DateTime.UtcNow`, `IClock`-based).
- **T-J** Observability requirements — define the minimum
  metric / log / event surface required to prove bounded
  behavior end-to-end during a future §5.3.x soak.
- **T-K** Final Admission Control and Backpressure
  specification — single artifact bundling inventory, probe
  matrix, raw evidence, classifications, severities, patch
  list, response-class taxonomy, propagation map, policy /
  determinism interaction findings, observability
  requirements, and PASS/FAIL determination.

### 2.9 Acceptance Criteria
1. Every runtime entry boundary and async seam in §2.5 scope
   is enumerated in the inventory and classified as BOUNDED /
   UNBOUNDED / UNKNOWN by initial reading.
2. Every seam has at least one reproducible probe covering
   declared bound, enforcement, saturation signal,
   propagation, and response class.
3. Every probe has reproducible evidence (command, file
   reference, or grep predicate + raw output) stored
   alongside the specification.
4. Every probe result is classified `BOUNDED-OBSERVABLE` /
   `BOUNDED-OPAQUE` / `UNBOUNDED-IMPLICIT` / `UNBOUNDED-OPEN`
   with a one-line justification.
5. Every non-`BOUNDED-OBSERVABLE` finding has S0–S3 severity
   per $16.
6. Every non-`BOUNDED-OBSERVABLE` finding has a remediation
   patch list entry with file path, intended change,
   acceptance probe, and externalized configuration shape.
7. Every runtime entry boundary has a declared response-class
   mapping (REJECT / RETRYABLE REFUSAL / DEGRADED
   ACCEPTANCE) with the wire-level / API-level shape per
   class.
8. Every downstream saturation source has a documented
   propagation path to the runtime entry boundary, or is
   explicitly flagged as a silent break.
9. WHYCEPOLICY $8 evaluation order vs. admission shedding is
   explicitly stated and consistent with policy primacy.
10. Admission and shedding decisions are confirmed
    deterministic per $9 (no `Guid.NewGuid`, no
    `DateTime.UtcNow`, `IClock`-based time).
11. The minimum observability surface required to prove
    bounded behavior in a future §5.3.x soak is defined.
12. No remediation patch is applied during the audit pass;
    opening pack discipline is preserved until §5.2.1
    advances out of the audit phase.
13. Any newly discovered guard rule or governance finding is
    captured under `claude/new-rules/` with the canonical
    5-field shape per $1c.
14. Final specification explicitly returns one of: `PASS`,
    `FAIL`, `PARTIAL`, `BLOCKED`, `WAIVED`, with the reason
    recorded.
15. The §5.2.1 row in README §6.0 is updated only when the
    workstream actually advances state — not by the opening
    pack itself.

### 2.10 Evidence Required
- Runtime seam inventory table with initial classification.
- Probe matrix (probe ID, seam ID, risk ID B1–B15,
  command/predicate, expected `BOUNDED-OBSERVABLE` shape).
- Raw probe output for every probe (verbatim).
- Classification table (probe ID → BOUNDED-OBSERVABLE /
  BOUNDED-OPAQUE / UNBOUNDED-IMPLICIT / UNBOUNDED-OPEN +
  reason).
- Severity table (finding ID → S0/S1/S2/S3).
- Remediation patch list with externalized configuration
  shape per item.
- Response-class taxonomy per runtime entry boundary.
- Backpressure propagation map.
- Policy / determinism interaction findings.
- Observability requirements list.
- New-rules capture file (if any).
- Final Admission Control and Backpressure specification
  with explicit terminal status.

---

## 3. TRACKING TABLE

| Field | Value |
|---|---|
| **ID** | 5.2.1 |
| **Topic** | Admission Control and Backpressure |
| **Objective** | Ensure the runtime can reject, defer, or bound excess work safely under load. For every runtime entry boundary and internal async seam in scope, declare the admission policy, the bound, the saturation signal, the backpressure propagation path, and the response class (REJECT / RETRYABLE REFUSAL / DEGRADED ACCEPTANCE), with reproducible evidence and a remediation patch list against any seam that is currently unbounded or silently buffered. |
| **Tasks** | T-A Seam inventory · T-B Probe matrix · T-C Probe execution · T-D Classification · T-E Severity triage · T-F Patch list · T-G Response-class taxonomy · T-H Backpressure propagation map · T-I Policy / determinism interaction check · T-J Observability requirements · T-K Final specification |
| **Deliverables** | Runtime seam inventory · Probe matrix · Raw probe evidence · Classification table · Severity table · Remediation patch list · Response-class taxonomy · Backpressure propagation map · Policy / determinism interaction findings · Observability requirements · New-rules capture (if any) · Final Admission Control and Backpressure specification |
| **Evidence Required** | Reproducible probe (command / file ref / grep predicate) and raw output for every seam in §2.5; declared bound, enforcement mechanism, saturation signal, propagation path, and response class per seam; classification + severity for every finding; explicit terminal status (PASS/FAIL/PARTIAL/BLOCKED/WAIVED) |
| **Status** | OPEN (NOT STARTED — workstream defined, no execution yet) |
| **Risk** | HIGH — every Phase 1.5 §5.3.x throughput / soak / stress / chaos workstream is gated on §5.2.1. Latent S0 findings (unbounded buffers on critical write paths such as outbox drain, projection writer, event-store append) are very plausible. The §5.1.x series did not test this class of behavior at all. |
| **Blockers** | None known. §5.1.1 PASS, §5.1.2 PASS, and §5.1.3 PASS prerequisites all satisfied 2026-04-08. |
| **Owner** | Whycespace runtime / operational hardening track |
| **Notes** | Opening pack only. No remediation in this prompt. Focuses on **runtime overload safety**, not generic performance tuning. The phase1.6-S1.5 OutboxPublisher MAX_RETRY externalization is the precedent for how patch-list items should externalize bounds via `IConfiguration`. The phase1.6-S1.6 DLQ topic resolution via `TopicNameResolver` is the precedent for how saturation-related routing should be centralized rather than scattered. Continuity with §5.1.1 PASS, §5.1.2 PASS, and §5.1.3 PASS (all 2026-04-08) preserved. §5.2.1 is the **first** workstream of the §5.2.x Runtime Infrastructure-Grade Hardening cluster and is the precondition for §5.3.x throughput certification. |

**Status legend:** NOT STARTED · IN PROGRESS · PARTIAL · BLOCKED · PASS · FAIL · WAIVED.

---

## 4. ACCEPTANCE CRITERIA
(See §2.9 above. Reproduced here for tracking convenience.)

1. Every in-scope runtime seam enumerated and initial-classified.
2. Every seam has at least one reproducible probe covering bound, enforcement, signal, propagation, response class.
3. Every probe has reproducible raw evidence.
4. Every probe result classified `BOUNDED-OBSERVABLE` / `BOUNDED-OPAQUE` / `UNBOUNDED-IMPLICIT` / `UNBOUNDED-OPEN` with reason.
5. Every non-`BOUNDED-OBSERVABLE` finding has S0–S3 severity.
6. Every non-`BOUNDED-OBSERVABLE` finding has a remediation patch list entry with externalized configuration shape and acceptance probe.
7. Every runtime entry boundary has a declared response-class mapping with wire-level shape.
8. Every downstream saturation source has a documented propagation path or is explicitly flagged as a silent break.
9. WHYCEPOLICY $8 evaluation order vs. admission shedding declared and consistent with policy primacy.
10. Admission / shedding decisions confirmed deterministic per $9.
11. Minimum observability surface for §5.3.x soak is defined.
12. No remediation applied during audit pass.
13. Any newly discovered guard rule captured under `claude/new-rules/`.
14. Final specification returns explicit terminal status.
15. README §6.0 row 5.2.1 updated only on real state change.

---

## 5. REQUIRED ARTIFACTS

- `claude/project-prompts/20260408-210000-phase-1-5-5-2-1-admission-control-backpressure-open.md`
  — this opening pack.
- `claude/audits/admission-control-backpressure.audit.md` — to be
  created during T-K (final specification). Not created by this
  opening pack.
- `claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md` — to be created
  during T-D, T-G, T-I, or T-J if and only if newly discovered
  governance rules emerge.
- README §5.2.1 (to be added on workstream advance) — unchanged
  by this opening pack; the workstream definition is anchored
  here, but state promotion is gated on real execution.
- README §6.0 master tracking table row 5.2.1 — unchanged by
  this opening pack.

---

## 6. CLAUDE EXECUTION PROMPT

> **Use this prompt to execute §5.2.1 in a follow-up session. Do not
> execute it as part of this opening pack.**

```
Phase 1.5 §5.2.1 — Admission Control and Backpressure (Execution Pass)

CLASSIFICATION: system / runtime / admission-control-backpressure
CONTEXT:
  §5.1.1 PASS (2026-04-08); §5.1.2 PASS (2026-04-08); §5.1.3 PASS (2026-04-08).
  Opening pack:
  claude/project-prompts/20260408-210000-phase-1-5-5-2-1-admission-control-backpressure-open.md

OBJECTIVE: Execute T-A through T-K of §5.2.1 as defined in the opening
  pack. Produce claude/audits/admission-control-backpressure.audit.md
  as the single consolidated deliverable. Do not modify source,
  guards, scripts, configuration, or README outside the audit
  artifact and (if needed) one or more claude/new-rules/ capture
  files.

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
  - Layer purity ($7): domain unchanged; engine confirmed stateless;
    only the runtime layer is in scope for admission / backpressure.
  - Policy ($8): WHYCEPOLICY evaluation order vs. admission shedding
    must be declared and policy-primacy preserved.
  - Determinism ($9): admission and shedding decisions must use
    IClock and deterministic IDs; no Guid.NewGuid, no DateTime.UtcNow.
  - No remediation patches applied; produce the patch list only.
  - No generic performance tuning; this workstream is about safety
    under load, not speed under load.
  - Any newly discovered guard rule → claude/new-rules/ per $1c
    (do not silently extend an existing guard).
  - Risk areas: B1–B15 from §2.4 of the opening pack.

EXECUTION STEPS:
  1. T-A Seam inventory — enumerate every runtime entry boundary
     and async seam in §2.5 scope and initial-classify BOUNDED /
     UNBOUNDED / UNKNOWN.
  2. T-B Probe matrix — define probes per seam covering declared
     bound, enforcement, saturation signal, propagation path, and
     response class. Each probe declares its expected
     BOUNDED-OBSERVABLE shape.
  3. T-C Probe execution — run every probe (static analysis,
     configuration inspection, targeted code reading); capture
     verbatim raw evidence.
  4. T-D Classification — BOUNDED-OBSERVABLE / BOUNDED-OPAQUE /
     UNBOUNDED-IMPLICIT / UNBOUNDED-OPEN per probe with one-line
     justification.
  5. T-E Severity triage — S0/S1/S2/S3 for every non-BOUNDED-OBSERVABLE
     finding per $16.
  6. T-F Patch list — file path + intended edit + acceptance probe
     + externalized configuration shape per non-BOUNDED-OBSERVABLE
     finding (follow phase1.6-S1.5 OutboxPublisher precedent). No
     edits applied.
  7. T-G Response-class taxonomy — declare, per runtime entry
     boundary, the saturation-cause → response-class mapping
     (REJECT / RETRYABLE REFUSAL / DEGRADED ACCEPTANCE) and the
     wire-level / API-level shape of each.
  8. T-H Backpressure propagation map — trace, for every downstream
     seam, the path by which its saturation signal reaches the
     runtime entry boundary; flag every silent break.
  9. T-I Policy / determinism interaction check — confirm
     WHYCEPOLICY $8 evaluation order vs. admission shedding and
     confirm shedding decisions are deterministic per $9.
 10. T-J Observability requirements — define the minimum metric /
     log / event surface required to prove bounded behavior
     end-to-end during a future §5.3.x soak.
 11. T-K Final specification — write
     claude/audits/admission-control-backpressure.audit.md
     bundling inventory, probe matrix, raw evidence, classifications,
     severities, patch list, response-class taxonomy, propagation
     map, policy / determinism findings, observability
     requirements, and explicit terminal status.

OUTPUT FORMAT:
  - Single audit artifact:
    claude/audits/admission-control-backpressure.audit.md
  - Optional: claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md
  - Structured failure report on any halt ($12: STATUS / STAGE /
    REASON / ACTION_REQUIRED).

VALIDATION CRITERIA:
  - All fifteen acceptance criteria from §2.9 / §4 satisfied.
  - Terminal status one of: PASS / FAIL / PARTIAL / BLOCKED / WAIVED.
  - Audit sweep ($1b) clean against the produced artifact.
  - No source/guard/script/configuration/README modification outside
    the explicitly named artifacts.
```

---

## 7. INITIAL STATUS

**OPEN** — workstream defined, tracked, and ready for execution. No
remediation performed. No source, guard, audit, script, configuration,
or README file modified by this opening pack. §5.2.1 enters the
Phase 1.5 work queue as the **first** workstream in the §5.2.x
Runtime Infrastructure-Grade Hardening cluster, immediately following
the closure of the §5.1.x structural hardening series (§5.1.1 PASS,
§5.1.2 PASS, §5.1.3 PASS — all 2026-04-08), and is the precondition
for the §5.3.x throughput / soak / stress / chaos certification
workstreams.
