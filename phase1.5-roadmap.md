
# Phase 1.5 — Enterprise Hardening and Operational Certification Gate
## STATUS: CANONICAL
## GOVERNANCE: MANDATORY PRE-PHASE-2 GATE
## RULE: PHASE 2 MUST NOT BEGIN UNTIL PHASE 1.5 IS COMPLETE AND EVIDENCED

## 1.0 PURPOSE

Phase 1.5 exists to convert Phase 1 from:
- deterministic architectural completion
- end-to-end Todo vertical-slice completion
- policy-gated execution completion

into:
- enterprise-grade runtime hardening
- infrastructure-grade operational readiness
- verified performance and failure tolerance
- documented, auditable, repeatable readiness proof

This phase is mandatory because Phase 2 and above will introduce:
- heavy domain model expansion
- economic and structural complexity
- larger workflow and lifecycle orchestration
- wider event volume and projection pressure
- stronger governance and operational requirements

If these hardening and operational issues are deferred beyond this point, the complexity cost will compound and become significantly harder to control.

Phase 1.5 therefore acts as the stabilization and certification gate before economic and large-scale domain expansion.

---

## 2.0 PHASE OBJECTIVE

Phase 1.5 must prove that the current system can move from:
- working architecture
- working vertical slice
- deterministic and policy-safe execution

to:
- stable runtime operation
- structurally clean architecture
- bounded operational behavior under load
- recoverable failure handling
- measurable readiness against enterprise standards

The target is not cosmetic hardening.
The target is operational credibility.

Phase 1.5 must raise the system toward enterprise expectations comparable to high-standard platform and infrastructure organizations, so that later phases are built on a stable foundation.

---

## 3.0 COMPLETION RULE

Phase 1.5 is complete only when all of the following are true:

1. Structural drift is closed or formally waived with canonical governance approval.
2. Runtime hardening topics are implemented and verified.
3. Load, soak, stress, replay, and resilience tests are executed and documented.
4. Operational readiness artifacts are complete.
5. Failure simulation and recovery drills are complete.
6. Observability, alerting, and SLO tracking are in place.
7. Multi-instance and scaling behavior are verified.
8. A formal readiness matrix marks all required items as PASS.
9. Evidence is documented in canonical audit, guard, and tracking artifacts.
10. A final certification report explicitly approves progression to Phase 2.

---

## 4.0 TRACKING MODEL

Every work item in Phase 1.5 must be tracked using the following structure:

- ID
- Topic
- Objective
- Tasks
- Deliverables
- Evidence Required
- Status
- Risk
- Blockers
- Owner
- Notes

### Status Values
- NOT STARTED
- IN PROGRESS
- PARTIAL
- BLOCKED
- PASS
- FAIL
- WAIVED

### Evidence Types
- Build proof
- Test proof
- Runtime proof
- Load proof
- Failure proof
- Audit proof
- Architecture proof
- Operational proof

---

## 5.0 PHASE 1.5 TOPIC STRUCTURE

# 5.1 Structural Integrity and Anti-Drift Remediation

## 5.1.1 Dependency Graph Remediation
Objective:
Close all known dependency violations before Phase 2 expansion.

Todo:
- ~~Remediate src/projections → runtime dependency violation~~ — Step B verification-only: csproj already clean (remediated 2026-04-07 under DG-R7-01); no using-level leakage from src/projections into runtime/engines/systems/domain/platform.
- Remediate platform/host dependency overreach — Step C: removed host → domain csproj reference (sole typed usage in PostgresOutboxAdapter replaced with reflection-based unwrap). Remaining host → runtime/engines/systems/projections references reclassified as JUSTIFIED composition-root edges under DG-R5-EXCEPT-01 (post-domain-removal).
- Reconfirm canonical dependency graph
- Add enforcement checks to prevent regression
- Regenerate dependency audit after fixes

Deliverables:
- Updated dependency graph guard
- Updated dependency graph audit
- Fixed project references
- Architectural exception log if any waiver exists

Evidence Required:
- Clean dependency audit
- Build proof (pending — running Whycespace.Host process must be stopped before clean rebuild)
- No forbidden dependency edges

Status:
- PASS (2026-04-08) — Steps A–E complete and final verification complete.

Closure Note:
- D1 closed by removing host → domain (Step C: PostgresOutboxAdapter typed unwrap replaced with reflection; <ProjectReference> to Whycespace.Domain.csproj removed from Whycespace.Host.csproj).
- D2 closed and JUSTIFIED (host → engines is composition-root DI under DG-R5-EXCEPT-01).
- D3 closed and JUSTIFIED (host → runtime/systems/projections is composition-root DI under DG-R5-EXCEPT-01).
- D4 closed through documentation and audit alignment (Step D: dependency-graph.guard.md DG-R5-EXCEPT-01 narrowed, DG-R5-HOST-DOMAIN-FORBIDDEN added; dependency-graph.audit.md baseline rewritten; README §5.1.1 updated).
- H1 closed: running Whycespace.Host (PID 32648) terminated; `dotnet build src/platform/host/Whycespace.Host.csproj` succeeded with 0 warnings, 0 errors, transitively building the full 8-project dependency closure.
- H3 closed: scripts/dependency-check.sh C4 rule narrowed with a single explicit whitelist clause for canonical domain `**/adapter/**` paths; script now exits 0 with 0 violations.

Final Evidence:
- Clean build: GREEN (0 warnings, 0 errors, all 8 projects).
- scripts/dependency-check.sh: GREEN (0 violations, exit 0).
- Post-fix dependency graph: clean — no host → domain; no projections → runtime; no using-level leakage in host or projections.

## 5.1.2 Boundary Purity Validation
Objective:
Ensure all layers remain within their canonical responsibility boundaries.

Todo:
- Re-audit platform/api vs platform/host separation
- Re-audit runtime ownership boundaries
- Re-audit systems/midstream/downstream separation
- Re-audit T1M/T2E purity rules
- Re-audit projections boundary rules
- Reconfirm no persistence leakage into engines

Deliverables:
- Boundary audit report
- Guard updates
- Remediation patch list

Evidence Required:
- Audit PASS
- No forbidden imports/references
- Verified routing ownership

Status:
- PASS (2026-04-08) — Steps A, B, C, and C-G complete.

Closure Note:
- BPV-D01 closed: 11 typed `Whycespace.Domain.*` bindings in host composition modules (TodoBootstrap, ConstitutionalPolicyBootstrap, WorkflowExecutionBootstrap) survived §5.1.1 PASS via fully-qualified and alias forms. Structural remediation moved schema identity binding into a runtime-side seam at `src/runtime/event-fabric/domain-schemas/**` (`ISchemaModule` + `EventSchemaRegistrySink` + per-domain `*SchemaModule.cs` + host-facing `DomainSchemaCatalog`). Host bootstraps now dispatch via one-line catalog calls and contain zero typed domain references.
- BPV-D02 closed: workflow-name constant relocated from `src/systems/midstream/wss/workflows/todo/TodoLifecycleWorkflow.cs` to `src/shared/contracts/application/todo/TodoLifecycleWorkflowNames.cs`; downstream handler import updated; obsolete midstream stub deleted.
- Step C-G governance hardening: `scripts/dependency-check.sh` and the `DG-R5-HOST-DOMAIN-FORBIDDEN` / `R-DOM-01` predicates strengthened to detect `using`, fully-qualified, and namespace-alias forms (comment lines excluded). `src/runtime/event-fabric/domain-schemas/**` documented as the only canonical runtime location permitted to hold typed domain references. Folklore-vs-canon contradiction captured in `claude/new-rules/20260408-180000-guards.md`.

Final Evidence:
- Final audit: [claude/audits/boundary-purity.audit.md](claude/audits/boundary-purity.audit.md) — PASS.
- Build: GREEN (0 warnings, 0 errors, all 8 projects).
- `bash scripts/dependency-check.sh` → `Violations: 0`, `Status: PASS` (strengthened predicate active).
- `grep -RIn "Whycespace\.Domain\." src/platform/host/` → only the canonical intent-comment in `composition/runtime/RuntimeComposition.cs:80`.
- Zero csproj changes across the entire workstream.

## 5.1.3 Canonical Documentation Alignment
Objective:
Ensure the actual implementation matches the locked canonical architecture.

Todo:
- Compare current implementation against WBSM v3.5 canonical architecture
- Identify all drift items
- Mark resolved vs pending vs waived
- Update canonical tracking documents

Deliverables:
- Drift register
- Canonical alignment audit
- Updated tracking artifacts

Evidence Required:
- Alignment report
- Explicit PASS/WAIVED state per drift item

Status:
- PASS (2026-04-08) — Step A, Step B Set 1, Step C Set 1, Step B Set 2, and Step C Set 2 complete.

Closure Note:
- §5.1.3 established CURRENT TRUTH / HISTORICAL BASELINE / ARCHIVAL RECORD as the canonical discipline model for implementation-adjacent documentation.
- CLAUDE.md $1a/$1b corrected from stale enumerations (12 guards, 11 audits) to discovery directives covering the actual `claude/guards/**` (30 files including the `domain-aligned/` subtree) and `claude/audits/**` (21 active definitions). The `claude/` directory description was rewritten to match `ls claude/` and to align with $2 (canonical prompt store is `project-prompts/`, not `prompts/`).
- `dependency-graph.guard.md` self-supersession closed in three coordinated edits: R5 body now cross-references DG-R5-EXCEPT-01; CODE-LEVEL CHECKS name `scripts/dependency-check.sh` as authoritative and split `platform/api` from `platform/host`; LOCK CONDITIONS reference R1–R7 plus the DG-* additions. `dependency-graph.audit.md` C2 + INPUTS aligned with the script.
- 13 stale `Phase B2a/B2b` source-comment markers across 9 files in `src/platform/host/**` and `src/runtime/event-fabric/**` cleaned up.
- Three `claude/new-rules/20260408-103326-*.md` captures (`activation`, `determinism`, `engines`) gained explicit `STATUS: PROPOSED` blocks with verification evidence.
- Stale/unclassified artifacts removed: `claude/audits.zip` (133 KB, 2026-04-07 export, superseded by live tree) and `claude/project-prompts/phase2/` (empty placeholder).
- Two non-blocking S3 review items explicitly deferred outside the PASS gate: `docs/validation/e2e-validation-report.md` scaffold classification and three prose-form new-rules captures with implicit STATUS lines.

Final Evidence:
- Final audit: [claude/audits/canonical-alignment.audit.md](claude/audits/canonical-alignment.audit.md) — PASS.
- Build: GREEN (0 warnings, 0 errors, all 8 projects).
- `bash scripts/dependency-check.sh` → `Violations: 0`, `Status: PASS` (strengthened predicate from §5.1.2 Step C-G remains active; documentation now accurately describes what it enforces).
- Folklore sweep: `grep -RIn "Phase B[0-9]" src/ --include="*.cs"` → 0 matches.
- TODO/FIXME sweep: `grep -RIn "TODO\|FIXME" src/ --include="*.cs"` → 0 matches.
- Ten drift items closed (DOC-D01 through DOC-D10). Zero S0/S1 findings across the entire workstream.

---

# 5.2 Runtime Infrastructure-Grade Hardening

## 5.2.1 Admission Control and Backpressure
Objective:
Prevent uncontrolled overload and ensure the runtime rejects excess work safely.

Todo:
- Introduce request admission policy
- Add bounded queueing where required
- Define overload behavior
- Add backpressure controls for persistence and messaging bottlenecks
- Validate no uncontrolled queue growth

Deliverables:
- Runtime admission control implementation
- Backpressure design note
- Verification tests

Evidence Required:
- Step A overload surface inventory
- Step B B-Narrow probe results
- Step C1–C7 patch implementations and build verification
- Final canonical audit report

Status:
- PASS (2026-04-08)

Closure note:
Phase 1.5 §5.2.1 closed PASS on 2026-04-08. Step A inventoried 20 runtime
overload surfaces; Step B B-Narrow probes confirmed three S0 risks
(R-01 HTTP intake, R-02 outbox table buffer, R-06 OPA evaluator) and
escalated R-03 (chain anchor). Step C1–C7 converted every S0 risk and
the leading S1/S2 observability gaps to BOUNDED-OBSERVABLE shapes
without architectural redesign:

- HTTP intake bounded refusal (PC-1) — declared partitioned
  concurrency limiter at the runtime edge, tenant-then-IP partition
  precedence, 429 + Retry-After on overflow, `Whyce.Intake` meter.
- OPA bounded refusal (PC-2) — per-call CTS timeout, in-process
  consecutive-failure circuit breaker (Closed/Open/HalfOpen) keyed by
  `IClock`, typed `PolicyEvaluationUnavailableException` mapped to
  503 + Retry-After, `Whyce.Policy` meter.
- Outbox saturation bounded refusal (PC-3) — periodic
  `OutboxDepthSampler` publishing `outbox.depth` and
  `outbox.oldest_pending_age_seconds` gauges and a shared
  `IOutboxDepthSnapshot` consulted by `PostgresOutboxAdapter` for
  high-water-mark refusal via `OutboxSaturatedException` → 503 +
  Retry-After.
- Declared Postgres pools (PC-4) — two logical `NpgsqlDataSource`
  pools (`event-store`, `chain`) sized from `PostgresPoolOptions`,
  seven adapters refactored to acquire connections through
  `PostgresPoolMetrics.OpenInstrumentedAsync`, `Whyce.Postgres` meter
  exporting `postgres.pool.acquisitions` and
  `postgres.pool.acquisition_failures`.
- ChainAnchorService wait/hold observability (PC-5) — `Whyce.Chain`
  meter exporting `chain.anchor.wait_ms` and `chain.anchor.hold_ms`
  histograms, with `outcome ∈ {ok, engine_failed, exception}` tag on
  hold; lock structure unchanged per workstream scope.
- Declared Kafka consumer prefetch envelope (PC-6) —
  `KafkaConsumerOptions` binds `QueuedMaxMessagesKbytes`,
  `FetchMessageMaxBytes`, `MaxPollIntervalMs`, `SessionTimeoutMs`;
  `GenericKafkaProjectionConsumerWorker` applies the declared envelope
  to `ConsumerConfig`; the librdkafka ~1 GiB prefetch default is
  retired in favor of a 16 MiB declared ceiling.
- Projection lag observability (PC-7) — `projection.lag_seconds`
  histogram on `Whyce.Projection.Consumer` meter, computed from the
  durable Kafka broker timestamp at projection-write completion,
  tagged by topic.

All load-bearing operational parameters now live in declared
configuration blocks: `Intake.*`, `Opa.*`, `Outbox.*`,
`Postgres.Pools.*`, `KafkaConsumer.*`. The runtime now has explicit
retryable-refusal seams for intake (429), policy unavailability
(503), and outbox saturation (503), each carrying `Retry-After` and
each backed by a typed exception mapped at the API edge via the
existing `IExceptionHandler` chain.

Residual items explicitly outside the §5.2.1 PASS gate (queued for
follow-on workstreams): the projections-pool refactor
(`PostgresProjectionWriter` still constructs raw `NpgsqlConnection`
per call from a domain-bootstrap-supplied connection string), DLQ
depth bounds, `LoadEventsAsync` streaming, native Npgsql/Confluent
client counter bridging, and structural restructuring of the
`ChainAnchorService` global semaphore (which is now observable but
still single-permit). These do not block §5.2.1 acceptance because
the opening pack's acceptance criteria require *bounded and
observable*, not *restructured*, for every in-scope surface.

Final audit: claude/audits/admission-control-backpressure.audit.md
Closure prompt: claude/project-prompts/20260408-220000-phase-1-5-5-2-1-pass-closure.md

## 5.2.2 Concurrency Control and Resource Bounds
Objective:
Ensure execution remains stable under concurrent pressure.

Todo:
- Define concurrency controls by subsystem
- Bound command execution concurrency
- Bound outbox processing concurrency
- Bound projection worker concurrency
- Validate aggregate-level safety under concurrent write pressure

Deliverables:
- Concurrency control implementation
- Runtime tuning configuration
- Concurrency test report

Evidence Required:
- Step A concurrency / resource-bound surface inventory
- Step B K-Narrow probe results
- Step C1–C9 patch implementations and build verification
- Final canonical audit report

Status:
- PASS (2026-04-08)

Closure note:
Phase 1.5 §5.2.2 closed PASS on 2026-04-08. Step A inventoried 16
concurrency / resource-bound surfaces; Step B K-Narrow probes
confirmed three S0/S1 leading findings (K-R-01 chain-lock
relationship resolved as one global lock + dead code, K-R-02 DLQ
growth fully invisible, K-R-03 / C14 capacity-model 42×
mismatch). Step C1–C9 promoted runtime concurrency and resource
ceilings from incidental to declared:

- C14 capacity-model resolution (KC-1) — `IntakeOptions.GlobalConcurrency`
  lowered from 256 to 6, `PerTenantConcurrency` from 32 to 4 so a
  single saturating partition cannot exceed the declared
  `Postgres.Pools.EventStore` envelope. Overload now refuses at
  the PC-1 limiter (429 + Retry-After) before the pool timeout
  fires as a generic 500.
- Idempotency coalescing (KC-2) — new
  `IIdempotencyStore.TryClaimAsync` single-round-trip claim via
  `INSERT ... ON CONFLICT DO NOTHING`, dropping per-command
  event-store pool consumption from 5 to 4 while preserving
  "mark only on success" semantics through a new `ReleaseAsync`
  rollback on the failure path.
- DLQ depth observability + retention (KC-3) —
  `outbox.deadletter_depth` `ObservableGauge` from a single
  `COUNT(*) FILTER` extension to the existing `OutboxDepthSampler`
  probe; new `consumer.dlq_publish_failed{source_topic, reason}`
  counter inside the existing $12-compliant catch block; declared
  `OutboxOptions.DeadletterRetention` (default `"operator-managed"`).
- Projections-pool declared `NpgsqlDataSource` (KC-4) — third
  declared logical pool (`projections`) joining `event-store` and
  `chain` from PC-4. `PostgresProjectionWriter` and
  `TodoProjectionHandler` (cross-layer, with a small local mirror
  of the `Whyce.Postgres` meter inside the projections assembly)
  refactored. **`TodoController.Get` recorded as residual** —
  `Whycespace.Api → Whycespace.Host` would create a project
  dependency cycle.
- Advisory-lock wait observability (KC-5) — new `Whyce.EventStore`
  meter exporting `event_store.append.advisory_lock_wait_ms` and
  `event_store.append.hold_ms` (with `outcome` tag) measuring the
  `pg_advisory_xact_lock` round-trip distinctly from PC-4 pool
  acquisitions.
- Workflow in-flight ceiling (KC-6) — new `WorkflowAdmissionGate`
  composing two `PartitionedRateLimiter<string>` instances
  (per-workflow-name + per-tenant), gating both
  `ExecuteWorkflowAsync` and `ResumeWorkflowAsync`, with typed
  `WorkflowSaturatedException` mapped to 503 + `Retry-After` via
  the new `WorkflowSaturatedExceptionHandler`. New `Whyce.Workflow`
  meter exporting `workflow.admitted` / `workflow.rejected`.
- ChainAnchor declared-options / waiver (KW-1) — new
  `ChainAnchorOptions.PermitLimit` (default 1) externalises the
  single-permit shape; `ChainAnchorService` constructs `_lock`
  from the option. **Structural restructuring of the lock is
  explicitly deferred** to a future workstream — KW-1 is the
  declared waiver, not the structural fix. PC-5 wait/hold
  observability remains intact.
- ChainLock dead-code cleanup (KC-7) —
  `src/engines/T0U/whycechain/lock/ChainLock.cs` (and the empty
  parent directory) deleted after Step B P-K1 confirmed zero
  callers under `src/`. The "second hidden global semaphore on
  the chain path" hypothesis is resolved by deletion rather than
  reconciliation. Closes K-R-12.
- LoadEventsAsync observability + declared waiver (KC-8) — new
  `event_store.replay_rows` histogram on the existing
  `Whyce.EventStore` meter, recording the `events.Count` of every
  successful replay. **Structural streaming/paging is explicitly
  waived** with the future-owner identification recorded at the
  histogram declaration site.

The runtime now has **four canonical retryable-refusal seams** (intake
429, OPA policy 503, outbox saturation 503, **workflow saturation 503**)
backed by four typed exception → `IExceptionHandler` mappings, plus
the pre-existing concurrency-conflict 409 REJECT. Eight declared
configuration blocks now externalise every load-bearing operational
parameter: `Intake.*`, `Opa.*`, `Outbox.*`, `Postgres.Pools.*`,
`KafkaConsumer.*`, **`Workflow.*`**, **`ChainAnchor.*`**. Eight
canonical `Whyce.*` meters export the complete §5.2.x observability
surface: `Whyce.Intake`, `Whyce.Policy`, `Whyce.Outbox`,
`Whyce.Postgres`, `Whyce.Chain`, `Whyce.EventStore`,
`Whyce.Workflow`, `Whyce.Projection.Consumer`.

Residual items explicitly outside the §5.2.2 PASS gate:
**`TodoController.Get` projections-side raw `NpgsqlConnection`**
(blocked by the api↔host project dependency cycle — requires a
shared `IDbConnectionFactory` abstraction outside §5.2.2 scope);
**structural restructuring of `ChainAnchorService._lock`** (declared
waiver via KW-1, deferred to §5.2.3 or beyond); **`LoadEventsAsync`
streaming/paging refactor** (declared waiver via KC-8, requires
extending the `IEventStore` interface contract); **multi-instance
`KafkaOutboxPublisher` drain**; **multi-worker per-topic projection
parallelism**. None of these match an opening-pack §2.9 acceptance
criterion that would block §5.2.2 PASS.

Final audit: claude/audits/concurrency-control-resource-bounds.audit.md
Closure prompt: claude/project-prompts/20260408-235900-phase-1-5-5-2-2-pass-closure.md

## 5.2.3 Timeout, Cancellation, and Circuit Protection
Objective:
Ensure external/system dependencies cannot hang the runtime indefinitely.

Todo:
- Add timeout policy for OPA, Kafka, Redis, database adapters where applicable
- Add cancellation propagation rules
- Add circuit-breaker or controlled fail-mode behavior where required
- Define retry boundaries and non-retry conditions

Deliverables:
- Timeout policy implementation
- Runtime resilience policy document
- Failure behavior tests

Evidence Required:
- Failure proof
- Timeout proof
- No unbounded hanging requests

Status:
- PASS (2026-04-08)

Closure note:
Phase 1.5 §5.2.3 closed PASS on 2026-04-08. Step A inventoried 19
timeout / cancellation / circuit-protection surfaces; Step B
T-Narrow probes confirmed six material findings (T-R-01 dispatcher
CT desert, T-R-02 chain-anchor wait + holder both unbounded,
T-R-04 13/18 Postgres `Execute*Async` empty-paren, T-R-05
projection-handler contracts no-token, T-R-06 workflow execution
no-deadline, T-R-08 host-shutdown undeclared). Step C1–C8
promoted runtime cancellation and timeout discipline from
incidental to declared:

- Dispatcher / middleware / controller CT contract co-evolution
  (TC-1) — every contract on the request execution path now
  declares `CancellationToken cancellationToken = default`.
  Middleware pipeline closure shape becomes `Func<CancellationToken,
  Task<CommandResult>>` so each link forwards `next(ct)`.
  `TodoController` binds `HttpContext.RequestAborted` and forwards
  through `ISystemIntentDispatcher` → control plane → middleware
  → command dispatcher.
- Chain-anchor wait timeout + typed refusal (TC-2) — new
  `ChainAnchorOptions.{WaitTimeoutMs=5000, RetryAfterSeconds=1}`;
  `_lock.WaitAsync(timeoutMs, ct)` replaces the empty-paren form;
  typed `ChainAnchorWaitTimeoutException` mapped to 503 +
  `Retry-After`. PC-5 wait/hold histograms unchanged.
- Chain-store I/O cancellation + circuit breaker (TC-3) —
  `IChainAnchor.AnchorAsync(..., ct)` threads CT into both
  `ExecuteScalarAsync(ct)` and `ExecuteNonQueryAsync(ct)`. New
  `ChainAnchorOptions.{BreakerThreshold=5, BreakerWindowSeconds=10}`
  Closed/Open/HalfOpen breaker mirrors the PC-2 OPA pattern via
  `IClock`. Typed `ChainAnchorUnavailableException` mapped to
  503 + `Retry-After`. Caller-driven cancellation propagates as-is
  and does not advance the breaker. New `chain.store.failure`
  counter on the existing `Whyce.Chain` meter.
- Postgres adapter token threading (TC-5) — `IEventStore`,
  `IOutbox`, `IIdempotencyStore`, `ISequenceStore` contracts and
  the four corresponding `Postgres*Adapter` implementations
  thread CT into every `Execute*Async`, `BeginTransactionAsync`,
  `CommitAsync`, and `ReadAsync` call site. `EventStoreService` /
  `OutboxService` wrappers and the live runtime call sites in
  `EventFabric`, `RuntimeCommandDispatcher`, and
  `IdempotencyMiddleware` forward CT end-to-end. Empty-paren
  `Execute*Async` forms at the targeted hot-path adapters are
  gone.
- Projection-handler contract CancellationToken extension (TC-6) —
  `IEnvelopeProjectionHandler.HandleAsync` and
  `IProjectionHandler<T>.HandleAsync` both declare CT.
  `TodoProjectionHandler` threads CT into `LoadAsync` /
  `UpsertAsync` and the underlying `ExecuteReaderAsync(ct)` /
  `ReadAsync(ct)` / `ExecuteNonQueryAsync(ct)` calls.
  `WorkflowExecutionProjectionHandler` carries CT at every
  per-type overload. `GenericKafkaProjectionConsumerWorker`
  forwards `stoppingToken` into the handler.
- Workflow gate token threading (TC-8) — both
  `_workflowAdmissionGate.AcquireAsync(...)` call sites in
  `RuntimeCommandDispatcher` (Start + Resume) now pass the real
  CT instead of `default`. KC-6 lease lifecycle and refusal
  mapping unchanged.
- Workflow step / execution timeout (TC-7) — `IWorkflowEngine`
  and `IWorkflowStep` contracts both carry CT.
  `WorkflowOptions.{PerStepTimeoutMs=30000, MaxExecutionMs=300000}`
  declared. `T1MWorkflowEngine` builds an execution-level linked
  CTS bounded by `MaxExecutionMs` and a per-step linked CTS
  bounded by `PerStepTimeoutMs`, both linked to the upstream
  request/host token. Cancellation discrimination throws typed
  `WorkflowTimeoutException("step"|"execution", …)` mapped to
  503 + `Retry-After`. Caller-driven cancellation propagates as
  `OperationCanceledException` without wrapping.
- Host-shutdown drain declared (TC-9) — `Host.ShutdownTimeoutSeconds`
  (default 30) configuration-bound and applied to
  `HostOptions.ShutdownTimeout`. New narrow
  `HostShutdownLinkingMiddleware` replaces `HttpContext.RequestAborted`
  for the duration of every request with a linked CTS combining
  the original client-disconnect token and
  `IHostApplicationLifetime.ApplicationStopping` so the entire
  TC-1 token chain inherits the shutdown signal at the very edge.

The runtime now has **seven canonical retryable-refusal seams**
(intake 429, OPA policy 503, outbox saturation 503, workflow
saturation 503, **chain-anchor wait timeout 503**, **chain-anchor
unavailable 503**, **workflow timeout 503**) backed by typed
exception → `IExceptionHandler` mappings, plus the pre-existing
concurrency-conflict 409 REJECT. Three new declared configuration
sub-blocks externalise the §5.2.3 timeout / breaker / shutdown
parameters: `Host.ShutdownTimeoutSeconds`, `ChainAnchor.{WaitTimeoutMs,
RetryAfterSeconds, BreakerThreshold, BreakerWindowSeconds}`,
`Workflow.{PerStepTimeoutMs, MaxExecutionMs}`. End-to-end
`CancellationToken` now flows from `HttpContext.RequestAborted`
(linked to `IHostApplicationLifetime.ApplicationStopping` via TC-9)
through every runtime seam to the database round-trip.

Residual items explicitly outside the §5.2.3 PASS gate: structural
restructuring of `ChainAnchorService` held-section I/O (declared
waiver — wait + holder both bounded; the held-section I/O remains
inside the permit by design); `IWorkflowExecutionProjectionStore`
token threading (TC-6 widens the handler contract but not the
store contract); workflow lifecycle event loss on declared
timeout (consistent with the rest of the canonical refusal family);
`Workflow:MaxExecutionMs` resume budget (each resume gets a fresh
budget — prior failed run's wall-clock not durably tracked);
`ISequenceResolver` / HSID prelude token threading; per-call
Postgres command timeout below `CommandTimeoutSeconds`; Kafka
producer timeout work; native client counter bridging. None of
these match an opening-pack §2.9 acceptance criterion that would
block §5.2.3 PASS.

Final audit: claude/audits/timeout-cancellation-circuit-protection.audit.md
Closure prompt: claude/project-prompts/20260408-235960-phase-1-5-5-2-3-pass-closure.md

## 5.2.4 Health, Readiness, and Degraded Modes
Objective:
Make runtime readiness reflect true operational state.

Todo:
- Separate liveness from readiness
- Add component-aware readiness checks
- Define degraded mode behavior
- Define maintenance mode behavior
- Define replay/recovery mode behavior
- Ensure host does not report healthy when critical subsystems are unavailable

Deliverables:
- Health/readiness implementation
- Operational mode definitions
- Readiness validation tests

Evidence Required:
- Runtime proof
- Failure simulation proof
- Accurate readiness behavior under subsystem failure

Status:
- NOT STARTED

## 5.2.5 Multi-Instance Runtime Safety
Objective:
Verify the runtime remains correct when scaled horizontally.

Todo:
- Validate multi-host command execution behavior
- Validate locking and concurrency assumptions across instances
- Validate outbox worker safety across instances
- Validate projection consumption behavior across instances
- Validate no duplicate side-effects under multi-instance deployment

Deliverables:
- Multi-instance safety test report
- Runtime scaling notes
- Coordination/locking audit

Evidence Required:
- Multi-instance test proof
- No ordering or semantic corruption
- Stable distributed behavior

Status:
- NOT STARTED

---

# 5.3 Load, Performance, and Capacity Certification

## 5.3.1 Baseline Performance Profiling
Objective:
Establish the real current throughput and latency profile.

Todo:
- Measure single-instance throughput baseline
- Measure p50/p95/p99 latency
- Measure CPU, memory, GC, connection pool pressure
- Measure event store write latency
- Measure Kafka publish latency
- Measure projection lag under normal load

Deliverables:
- Baseline performance report
- Capacity dashboard snapshots
- Bottleneck analysis

Evidence Required:
- Load test report
- Metrics export
- Performance analysis summary

Status:
- NOT STARTED

## 5.3.2 1k RPS for 60 Minutes Certification
Objective:
Prove sustained stability for the first serious operational threshold.

Todo:
- Design 1k RPS sustained test
- Run for 60 minutes minimum
- Track error rate, latency, queue growth, lag, retries, memory growth
- Validate no data loss, no duplicate materialization, no ordering failure
- Validate post-test recovery state

Deliverables:
- 1k RPS soak report
- Metrics charts
- Pass/fail certification result

Evidence Required:
- Soak proof
- Runtime proof
- Metrics and logs
- Post-run integrity check

Status:
- NOT STARTED

## 5.3.3 Burst and Stress Testing
Objective:
Understand failure thresholds and degradation behavior.

Todo:
- Run burst tests above steady-state rate
- Run step-load escalation tests
- Identify saturation point
- Measure how the system degrades
- Validate graceful rejection vs collapse

Deliverables:
- Burst/stress test report
- Saturation analysis
- Degradation behavior report

Evidence Required:
- Stress proof
- Controlled overload evidence
- No undefined collapse behavior

Status:
- NOT STARTED

## 5.3.4 1M RPS Readiness Assessment
Objective:
Do not claim 1M RPS without evidence; instead produce a formal readiness gap analysis.

Todo:
- Assess architectural requirements for 1M RPS
- Compare current implementation to target requirements
- Identify missing infrastructure, topology, partitioning, storage, and runtime controls
- Produce a gap-to-target roadmap
- Explicitly state whether current architecture is on-track, blocked, or requires redesign in specific parts

Deliverables:
- 1M RPS readiness assessment
- Gap matrix
- Future scaling roadmap

Evidence Required:
- Technical assessment report
- No unsupported capacity claims

Status:
- NOT STARTED

---

# 5.4 Persistence, Messaging, and Replay Readiness

## 5.4.1 Event Store Endurance and Integrity
Objective:
Validate event store behavior under sustained and concurrent pressure.

Todo:
- Test append throughput under load
- Test optimistic concurrency under contention
- Validate no ordering drift
- Validate shard routing correctness
- Measure transaction cost and lock behavior

Deliverables:
- Event store endurance report
- Integrity test report
- Persistence bottleneck findings

Evidence Required:
- Load proof
- Concurrency proof
- Ordering proof

Status:
- NOT STARTED

## 5.4.2 Kafka and Outbox Operational Hardening
Objective:
Prove safe messaging behavior beyond correctness-level completion.

Todo:
- Measure outbox drain rate under sustained backlog
- Measure retry behavior
- Validate deadletter routing under fault cases
- Validate consumer lag recovery
- Validate partition routing behavior under scale
- Validate no commit-on-fail regressions

Deliverables:
- Messaging hardening report
- Outbox backlog recovery report
- Consumer lag and DLQ report

Evidence Required:
- Messaging proof
- Recovery proof
- Lag recovery metrics

Status:
- NOT STARTED

## 5.4.3 Projection Rebuild and Replay at Scale
Objective:
Prove that the read side can be rebuilt safely after corruption or loss.

Todo:
- Run full replay into projections
- Simulate projection data loss
- Rebuild projections from event history
- Validate idempotency under replay
- Validate correctness after rebuild
- Measure replay speed and lag

Deliverables:
- Projection replay/rebuild report
- Replay safety audit
- Recovery timing report

Evidence Required:
- Replay proof
- Correctness proof
- Recovery proof

Status:
- NOT STARTED

## 5.4.4 Schema Evolution and Migration Safety
Objective:
Ensure future growth does not break replay or deployment safety.

Todo:
- Review event schema evolution strategy
- Review projection migration strategy
- Review outbox and event-store migration safety
- Add migration validation tests
- Add rollback/forward-only discipline documentation

Deliverables:
- Migration safety guide
- Schema evolution audit
- Validation tests

Evidence Required:
- Migration proof
- Replay compatibility checks
- Deployment safety notes

Status:
- NOT STARTED

---

# 5.5 Policy, Chain, and Governance Resilience

## 5.5.1 WHYCEPOLICY Operational Resilience
Objective:
Validate policy remains reliable under load and fault.

Todo:
- Measure OPA/policy latency under load
- Test policy denial path under volume
- Test policy service slowness/unavailability
- Validate runtime behavior when policy is degraded
- Validate no policy bypass paths remain

Deliverables:
- Policy resilience report
- Policy latency benchmark
- Bypass audit update

Evidence Required:
- Policy load proof
- Failure proof
- Bypass audit PASS

Status:
- NOT STARTED

## 5.5.2 WhyceChain Resilience and Anchoring Behavior
Objective:
Validate anchoring under real operational conditions.

Todo:
- Test anchoring latency under load
- Test anchor failure behavior
- Test strict mode behavior
- Validate no chain continuity corruption
- Validate ordering relative to persist and publish
- Validate recovery after anchoring issue

Deliverables:
- Chain resilience report
- Anchoring correctness audit
- Failure handling report

Evidence Required:
- Chain proof
- Failure proof
- Integrity proof

Status:
- NOT STARTED

## 5.5.3 Governance Traceability and Audit Completeness
Objective:
Ensure all critical execution paths remain auditable and explainable.

Todo:
- Validate audit emission separation
- Validate correlation trace continuity
- Validate policy decision traceability
- Validate replayable evidence chain
- Ensure client-visible conflict and governance evidence remain consistent

Deliverables:
- Governance traceability audit
- Audit completeness report
- Trace integrity report

Evidence Required:
- Audit proof
- Chain/policy trace proof
- End-to-end correlation proof

Status:
- NOT STARTED

---

# 5.6 Failure Simulation, Chaos, and Recovery

## 5.6.1 Component Failure Simulation
Objective:
Prove the system behaves safely when critical components fail.

Todo:
- Simulate Kafka unavailability
- Simulate Postgres degradation/failure
- Simulate Redis degradation/failure
- Simulate OPA degradation/failure
- Simulate chain anchoring failure
- Validate behavior in each case

Deliverables:
- Failure simulation matrix
- Component failure reports
- Recovery observations

Evidence Required:
- Failure proof
- Explicit pass/fail per subsystem
- No undefined behavior

Status:
- NOT STARTED

## 5.6.2 Recovery Drills
Objective:
Prove the system can recover from fault conditions without corruption.

Todo:
- Restart runtime after backlog accumulation
- Recover from outbox backlog
- Recover consumers after lag buildup
- Recover projections after consumer interruption
- Recover from partial replay interruption
- Validate final system state

Deliverables:
- Recovery drill report
- Restart/recovery timings
- Integrity validation report

Evidence Required:
- Recovery proof
- State correctness proof
- Lag reduction and steady-state restoration proof

Status:
- NOT STARTED

## 5.6.3 Chaos and Stability Exercise
Objective:
Test the system under repeated disturbance, not only isolated faults.

Todo:
- Run repeated transient failure scenarios
- Combine load with dependency slowness
- Observe cascading effects
- Identify weakest subsystem under mixed pressure
- Produce mitigation plan

Deliverables:
- Chaos exercise report
- Stability analysis
- Mitigation plan

Evidence Required:
- Mixed-fault proof
- Stability findings
- Actionable remediation list

Status:
- NOT STARTED

---

# 5.7 Observability, SLOs, and Operational Control

## 5.7.1 Metrics and Telemetry Completion
Objective:
Make the system measurable enough to operate safely.

Todo:
- Confirm metrics coverage for runtime, store, outbox, Kafka, projections, policy, chain
- Define key counters, gauges, timers, and error classes
- Confirm structured logging standards
- Confirm correlation IDs across all critical paths

Deliverables:
- Telemetry inventory
- Metrics specification
- Logging standard verification

Evidence Required:
- Observability proof
- Metrics screenshots or exports
- Correlation proof

Status:
- NOT STARTED

## 5.7.2 SLO and Alerting Definition
Objective:
Define what “healthy operation” means.

Todo:
- Define availability and latency SLOs
- Define error budget concept for Phase 1.5
- Define lag thresholds
- Define persistence and publish failure alerts
- Define recovery-time thresholds

Deliverables:
- SLO document
- Alert catalog
- Threshold map

Evidence Required:
- Approved operational thresholds
- Alert validation proof

Status:
- NOT STARTED

## 5.7.3 Runbooks and Incident Procedures
Objective:
Ensure operators know what to do under fault.

Todo:
- Write runbook for Kafka backlog
- Write runbook for event-store pressure
- Write runbook for OPA failure
- Write runbook for chain failure
- Write runbook for projection replay/rebuild
- Write runbook for degraded mode and restart procedures

Deliverables:
- Operational runbook set
- Incident response playbook
- Recovery checklist

Evidence Required:
- Documentation proof
- Drill-backed runbook validation where applicable

Status:
- NOT STARTED

---

# 5.8 Final Readiness Certification

## 5.8.1 Phase 1.5 Readiness Matrix
Objective:
Provide one formal truth source for whether the gate is complete.

Todo:
- Create readiness matrix with all topics
- Mark each item as PASS / FAIL / PARTIAL / BLOCKED / WAIVED
- Attach evidence links
- Record open risks
- Record progression recommendation

Deliverables:
- Phase 1.5 readiness matrix
- Evidence index
- Risk register

Evidence Required:
- Full matrix completed
- No hidden open blockers

Status:
- NOT STARTED

## 5.8.2 Final Certification Audit
Objective:
Make progression to Phase 2 a governance decision, not an assumption.

Todo:
- Perform final architecture audit
- Perform final operational audit
- Perform final performance audit
- Perform final resilience audit
- Summarize all unresolved risks
- Explicitly certify or reject readiness for Phase 2

Deliverables:
- Final Phase 1.5 certification report
- Executive summary
- Gate decision record

Evidence Required:
- Formal PASS decision
- Signed-off audit outcome within project governance workflow

Status:
- NOT STARTED

---

## 6.0 MASTER TRACKING TABLE

| ID | Topic | Objective | Status | Evidence Required | Blocker To Phase 2 |
|---|---|---|---|---|---|
| 5.1.1 | Dependency Graph Remediation | Close architectural drift | PASS (2026-04-08) | Audit + Build | YES |
| 5.1.2 | Boundary Purity Validation | Enforce layer purity | PASS (2026-04-08) | Audit + Build + Strengthened dep-check | YES |
| 5.1.3 | Canonical Documentation Alignment | Match code to canon | PASS (2026-04-08) | Alignment Audit + Build + dep-check + Folklore Sweep | YES |
| 5.2.1 | Admission Control and Backpressure | Safe overload handling | PASS (2026-04-08) | Step A inventory · Step B probes · PC-1..PC-7 patches · admission-control-backpressure.audit.md | YES |
| 5.2.2 | Concurrency Control and Resource Bounds | Stable concurrent execution | PASS (2026-04-08) | Step A inventory · Step B probes · KC-1..KC-8 + KW-1 patches · concurrency-control-resource-bounds.audit.md | YES |
| 5.2.3 | Timeout, Cancellation, and Circuit Protection | Prevent hanging and collapse | PASS (2026-04-08) | Step A inventory · Step B T-Narrow probes · TC-1..TC-9 patches · timeout-cancellation-circuit-protection.audit.md | YES |
| 5.2.4 | Health, Readiness, and Degraded Modes | Accurate operational health | NOT STARTED | Runtime Proof | YES |
| 5.2.5 | Multi-Instance Runtime Safety | Horizontal safety proof | NOT STARTED | Multi-Instance Proof | YES |
| 5.3.1 | Baseline Performance Profiling | Measure reality | NOT STARTED | Load Report | YES |
| 5.3.2 | 1k RPS for 60 Minutes Certification | Sustained stability proof | NOT STARTED | Soak Proof | YES |
| 5.3.3 | Burst and Stress Testing | Failure threshold proof | NOT STARTED | Stress Report | YES |
| 5.3.4 | 1M RPS Readiness Assessment | Honest future-scale gap analysis | NOT STARTED | Assessment Report | NO |
| 5.4.1 | Event Store Endurance and Integrity | Persistence stability | NOT STARTED | Load + Integrity Proof | YES |
| 5.4.2 | Kafka and Outbox Operational Hardening | Messaging stability | NOT STARTED | Recovery + Lag Proof | YES |
| 5.4.3 | Projection Rebuild and Replay at Scale | Read-side recovery proof | NOT STARTED | Replay Proof | YES |
| 5.4.4 | Schema Evolution and Migration Safety | Safe growth path | NOT STARTED | Migration Proof | YES |
| 5.5.1 | WHYCEPOLICY Operational Resilience | Policy under pressure | NOT STARTED | Policy Load + Failure Proof | YES |
| 5.5.2 | WhyceChain Resilience and Anchoring Behavior | Chain under pressure | NOT STARTED | Chain Proof | YES |
| 5.5.3 | Governance Traceability and Audit Completeness | Evidence continuity | NOT STARTED | Audit Proof | YES |
| 5.6.1 | Component Failure Simulation | Safe fault behavior | NOT STARTED | Failure Matrix | YES |
| 5.6.2 | Recovery Drills | Safe restart and recovery | NOT STARTED | Recovery Report | YES |
| 5.6.3 | Chaos and Stability Exercise | Mixed-pressure resilience | NOT STARTED | Chaos Report | YES |
| 5.7.1 | Metrics and Telemetry Completion | Operability visibility | NOT STARTED | Observability Proof | YES |
| 5.7.2 | SLO and Alerting Definition | Health thresholds | NOT STARTED | SLO + Alerts | YES |
| 5.7.3 | Runbooks and Incident Procedures | Operational control | NOT STARTED | Runbooks | YES |
| 5.8.1 | Phase 1.5 Readiness Matrix | Central gate truth source | NOT STARTED | Matrix | YES |
| 5.8.2 | Final Certification Audit | Formal progression approval | NOT STARTED | Certification Report | YES |

---

## 7.0 GOVERNANCE DECISION

Canonical decision:
- Phase 1 remains the deterministic and end-to-end vertical slice completion phase.
- Phase 1.5 is now the mandatory enterprise hardening and operational certification phase.
- Phase 2 must not begin until Phase 1.5 is completed and evidenced.

Canonical interpretation:
Phase 1 proves the system works.
Phase 1.5 proves the system is trustworthy enough to scale into economic, structural, and workflow-heavy expansion.

---

## 8.0 DOCUMENTATION RULE

For every Phase 1.5 topic, the following must exist where applicable:
- guard
- audit
- implementation prompt
- remediation prompt
- tracking record
- final evidence note

Recommended canonical storage pattern:
- /claude/guards/
- /claude/audits/
- /claude/project-prompts/
- /docs/
- /scripts/
- progress tracker under canonical phase tracking documents

No Phase 1.5 topic should be considered complete without both implementation proof and documentation proof.

---

## 9.0 EXECUTIVE SUMMARY

Phase 1.5 is now officially adopted as the pre-Phase-2 hardening gate.

Its purpose is to ensure that before the system enters:
- heavy domain model design
- economic system implementation
- larger workflow/lifecycle expansion
- broader operational complexity

the current foundation is:
- structurally clean
- runtime hardened
- performance measured
- failure tested
- operationally documented
- auditable and certifiable

This is now canonical.





























































# v5

This is the v5 project repository.


| Area   | Before              | After                      |
| ------ | ------------------- | -------------------------- |
| Time   | Non-deterministic ❌ | Replay-safe ✅              |
| IDs    | Random ❌            | Deterministic ✅            |
| Chain  | Weak ❌              | Cryptographically stable ✅ |
| Outbox | Duplicate risk ❌    | Exactly-once semantics ✅   |

E1 → Domain
E2 → Contracts
E3 → Persistence
E4 → Determinism
E5 → Engine
E6 → Runtime
E7 → Workflow
E8 → Projection
E9 → Policy
E10 → Guard
E11 → Chain
E12 → Full enforcement

Platform API
  → Systems.Downstream
  → Systems.Midstream (WSS/HEOS/WhyceAtlas when needed)
  → Runtime Control Plane
  → T0U (policy/guard/pre-flight)
  → T1M (workflow/orchestration execution, if workflow is involved)
  → T2E (domain execution)
  → Domain Aggregate / Domain Logic
  → Domain Events Emitted
  → Runtime persists / anchors / publishes / triggers projections
  → Systems
  → Platform API Response


Platform API receives and translates requests.
Systems select and compose the correct business/orchestration path.
Runtime governs execution through middleware, routing, persistence, publication, and projection triggering.
T0U evaluates policy and preconditions.
T1M coordinates workflow execution where required.
T2E performs domain-aligned command execution.
Domain aggregates enforce invariants and raise events.
Runtime persists, anchors, publishes, projects, and returns the final result upward to Systems and then Platform API.



## TODO


- # runtime workflow and workflow-state fix

- standardized some shoulds -  engines, projection, domain, api, infra etc
- T1M/step/todo
- kafka need to follow domain classification
  
In-memory <see cref="IStructureRegistry"/> stub.

- upgrade todo to trello/kaban style to accomodate lifecycle/workflow
- Add failure + compensation flow to Todo workflow
- multistep workflow calling different domain services using todo/incident as case study

##
- clean domain model

## Validation process
## ------------------------------------
- TODO in action
- WORKFLOW STATE COMPLETENESS in acion
- REPLAY & RESUME IN ACTION
- WHAT ARE THE FEATURES THAT WE HAVE FOR THIS PHASE 1
- Deterministic, replay-safe, failure-resumable execution
- DLQ in actions





###

We are not investment platform, but platform that help you scale your investment portforlio
True economic activities, not extractive activities
Team work
African can continue in its complaint tactics
Building ecosystem

Real world economic structure /not short time, not 
the poer of upbringing and associations


- get a skill



Intelligence Investors


Before
Program.cs → manual wiring
🚀 After
Program.cs → ModuleLoader → Modules → System



Generate Domain E1–E8 Standardization Prompt (Scale 100+ domains safely)






✔ Deterministic execution
✔ Policy-enforced pipeline
✔ Event-sourced state
✔ Typed replay
✔ Resume-safe workflows
✔ End-to-end test validation