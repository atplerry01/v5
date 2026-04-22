# Runtime Audit (Canonical)

**Validates:** [`claude/guards/runtime.guard.md`](../../guards/runtime.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the runtime layer (execution + enforcement) complies with all rules consolidated in the canonical runtime guard: runtime order & lifecycle, Phase 1.5 runtime hardening, engine purity, projections (CQRS read models), prompt container, dependency graph & layer boundaries, contracts boundary, code quality enforcement (CCG / dead code / stubs) as a Runtime subsystem, and Test & E2E validation as a Runtime subsystem, plus shared WBSM v3 Global Enforcement.

## Scope

- `src/runtime/**` — runtime execution support, internal projection routing/orchestration
- `src/engines/**` — engine layer (stateless, events only)
- `src/projections/**` — CQRS read models
- Prompt container / prompt reconciliation surfaces
- Dependency-graph edges across `src/domain`, `src/engines`, `src/runtime`, `src/platform`, `src/systems`, `src/projections`, `src/shared`
- Contracts boundary (`src/shared/contracts/**`) from the runtime/engine side
- Code quality enforcement (CCG / dead-code / stub-detection)
- Test & E2E validation harnesses

Note: Determinism, HSID, Hash, Replay, and System Invariants live in `constitutional.audit.md`. Composition Loader and Program Composition live in `infrastructure.audit.md`.

## Source guard

This audit checks the rules defined in [`claude/guards/runtime.guard.md`](../../guards/runtime.guard.md). Rule ID conventions (R1..R15, R-CTX-/R-ORDER-/R-UOW-/R-WORKFLOW-/R-WF-/R-POLICY-/R-CANONICAL-PIPELINE-/R-EVENT-/R-CHAIN-/RO-/R-RT-, E1..E16 + ENG-/E-WORKFLOW-/E-STEP-/E-RESUME-/E-STATE-/E-TYPE-/E-VERSION-/E-LIFECYCLE-FACTORY-, P1..P32 + P-TYPE-ALIGN-/P-AGNOSTIC-/PROJ-, PROMPT-RECONCILE-, Dependency-graph R1..R7 + DG-R5-/DG-R7-/DG-BASELINE-/DG-SCRIPT-HYGIENE-/DG-COMPOSITION-ROOT-, G-CONTRACTS-, CCG-, Dead-code R1..R4, STUB-R1..R6, T-BUILD-/T-DOUBLES-/T-PLACEHOLDER-/T-POLICY-/T1M-/ACT-FABRIC-, G-E2E-, GE-01..05) are owned by that guard.

---

## Validation Checklist

### Section 1 — Runtime Order & Lifecycle

- [ ] **R1** — runtime is sole command router
- [ ] **R2** — runtime is sole event dispatcher
- [ ] **R3** — middleware registered in runtime only
- [ ] **R4** — projections triggered by runtime events
- [ ] **R5** — no direct engine invocation from platform
- [ ] **R6** — runtime owns transaction scope
- [ ] **R7** — runtime is sole persist / publish / anchor authority
- [ ] **R8** — runtime owns retry and circuit breaker
- [ ] **R9** — runtime pipeline is linear
- [ ] **R10** — runtime context propagation
- [ ] **R11** — no domain logic in runtime
- [ ] **R12** — runtime must enforce policy middleware
- [ ] **R13** — runtime must anchor events to chain
- [ ] **R14** — outbox is mandatory path
- [ ] **R15** — no engine direct invocation outside dispatcher
- [ ] **R-CTX-01** — context propagation invariant
- [ ] **R-ORDER-01** — ordering invariant
- [ ] **R-UOW-01** — unit-of-work boundary
- [ ] **R-WORKFLOW-PIPE-01** — workflow pipeline sequencing
- [ ] **R-DOM-LEAK-01** — domain-leak detector (sub-clause of R11)
- [ ] **R-POLICY-PATH-01** — policy evaluation path enforced
- [ ] **R-WF-EVENTIFIED-01** — workflow observer paths eventified
- [ ] **R-WF-RESUME-01** — workflow resume path intact
- [ ] **R-POLICY-FIRST-01** — policy-first ordering
- [ ] **R-CANONICAL-PIPELINE-01** — canonical pipeline shape
- [ ] **POLICY-PIPELINE-INTEGRATION-01** — policy pipeline integration
- [ ] **R-WF-PAYLOAD-01** — workflow payload contract
- [ ] **R-WF-PAYLOAD-TYPED-01** — workflow payload typed
- [ ] **R-EVENT-AUDIT-COLS-01** — event audit columns populated
- [ ] **R-CHAIN-CORRELATION-01** — chain correlation id propagated
- [ ] **RT-API-CORRELATION-ECHO-01** / **R-CHAIN-CORRELATION-SURFACE-01** — API `meta.correlationId` echoes runtime-stamped id on command paths; `Guid.Empty` on success-command response = violation
- [ ] **R-RT-CMD-AGGID-01** — every `*Command` implements `IHasAggregateId`; reflective property-name fallback deprecated
- [ ] **RT-OUTBOX-AGGID-FROM-ENVELOPE-01** — outbox row `aggregate_id` sourced from `IEventEnvelope.AggregateId`; no reflection on payload type
- [ ] **RT-BACKGROUND-IDENTITY-EXPLICIT-01** — every `BackgroundService`/`IHostedService` wraps dispatch in `SystemIdentityScope.Begin("system/<worker>")`
- [ ] **RO-LOCKED-ORDER** — locked pipeline order
- [ ] **RO-1** — no reordering
- [ ] **RO-2** — no optional middlewares
- [ ] **RO-3** — no parallel fabric stages
- [ ] **RO-4** — no alternative entry points
- [ ] **RO-5** — policy between pre- and post-policy guards
- [ ] **RO-6** — chain must follow persist
- [ ] **RO-7** — outbox must follow chain
- [ ] **RO-CANONICAL-11** — canonical 11-stage pipeline locked

*Note: `R-WF-OBSERVER-01` is REVOKED (2026-04-07). Do not validate.*

### Section 2 — Phase 1.5 Runtime Rules

- [ ] **R-RT-01** — Phase 1.5 runtime invariant
- [ ] **R-RT-02** — Phase 1.5 runtime invariant
- [ ] **R-RT-03** — Phase 1.5 runtime invariant
- [ ] **R-RT-04** — Phase 1.5 runtime invariant
- [ ] **R-RT-05** — Phase 1.5 runtime invariant
- [ ] **R-RT-06** — IClock + IIdGenerator injection (with MI-1 owner-token shape exemption)
- [ ] **R-RT-07** — Phase 1.5 runtime invariant
- [ ] **R-RT-08** — Phase 1.5 runtime invariant
- [ ] **R-RT-09** — Phase 1.5 runtime invariant
- [ ] **R-RT-10** — Phase 1.5 runtime invariant

### Section 3 — Engine Purity

- [ ] **E1** — tier classification
- [ ] **E2** — T0U: no domain imports
- [ ] **E3** — T1M: no direct domain mutation
- [ ] **E4** — T2E: domain execution only
- [ ] **E5** — T3I: external boundary
- [ ] **E6** — T4A: schedule and trigger
- [ ] **E7** — no cross-tier engine imports (same-tier permitted per 2026-04-13 amendment)
- [ ] **E8** — engines never define domain models
- [ ] **E9** — engines are stateless
- [ ] **E10** — engine folder structure
- [ ] **E11** — engine input/output types
- [ ] **E12** — engine testability
- [ ] **E13** — no persistence in engines
- [ ] **E14** — event emission only output
- [ ] **E15** — `EngineContext` surface restriction
- [ ] **E16** — policy pre-condition required
- [ ] **E-WORKFLOW-01** — engine workflow constraint
- [ ] **E-STEP-01** — engine step constraint
- [ ] **E-STEP-02** — engine step constraint
- [ ] **E-VERSION-01** — engine version discipline
- [ ] **E-LIFECYCLE-FACTORY-01** — lifecycle factory required
- [ ] **E-LIFECYCLE-FACTORY-CALL-SITE-01** — lifecycle factory call site enforced
- [ ] **E-RESUME-01** — engine resume constraint
- [ ] **E-RESUME-02** — engine resume constraint
- [ ] **E-RESUME-03** — engine resume constraint
- [ ] **E-STATE-01** — engine state constraint
- [ ] **E-STATE-02** — engine state constraint
- [ ] **E-STATE-03** — engine state constraint
- [ ] **E-TYPE-01** — engine type discipline
- [ ] **E-TYPE-02** — engine type discipline
- [ ] **E-TYPE-03** — engine type discipline
- [ ] **ENG-PURITY-01** — engine purity consolidated rule
- [ ] **ENG-DOMAIN-ALIGN-01** — engine domain alignment

### Section 4 — Projections (CQRS read models)

- [ ] **P1..P13** — projection rules block 1 (lines 641–695 of runtime.guard.md)
- [ ] **P-IDEMPOTENCY-KEY-NOT-NULL-01** — projection upserts populate `idempotency_key`; `count(*) WHERE idempotency_key IS NULL` = 0
- [ ] **P-VERSION-MONOTONE-01** — projection upserts set `current_version = envelope.Version`; rows with `current_version = 0 AND last_event_id IS NOT NULL` = 0
- [ ] **P-JSONB-KEY-CASE-01** — JSONB state casing matches index/query extractor casing (single documented convention per read model)
- [ ] **P-EVENT-TIMESTAMP-STAMP-01** — temporal read-model fields stamped from `envelope.Timestamp`; `default(DateTimeOffset)` (`0001-01-01`) on populated rows = violation
- [ ] **P14..P19** — projection rules block 2
- [ ] **P20..P27** — projection rules block 3
- [ ] **P28..P32** — projection rules block 4
- [ ] **P-TYPE-ALIGN-01** — projection type alignment
- [ ] **P-AGNOSTIC-01** — projection engine-agnostic
- [ ] **PROJ-READ-ONLY-01** — projections are read-only
- [ ] **PROJ-DOMAIN-ALIGN-01** — projections domain-aligned
- [ ] **PROJ-WF-EXEC-01** — projection workflow execution constraint
- [ ] **PROJ-REPLAY-SAFE-01** — projections replay-safe
- [ ] **PROJ-NO-INPLACE-MUTATION-01** — projections do not mutate in place

### Section 5 — Prompt Container

- [ ] **Prompt Container rules 1–15** (runtime.guard.md §Prompt Container, unnumbered — validate by rule order): mandatory sections present; classification declared; policy binding declared; no prompts stored outside `claude/project-prompts/`
- [ ] **PROMPT-RECONCILE-01** — prompt reconciliation pre-execution pass completed

### Section 6 — Dependency Graph & Layer Boundaries

- [ ] **Dependency-graph R1** — domain purity edge
- [ ] **Dependency-graph R2** — engine isolation edge
- [ ] **Dependency-graph R3** — runtime authority edge
- [ ] **Dependency-graph R4** — platform boundary edge
- [ ] **Dependency-graph R5** — host-domain edge
- [ ] **Dependency-graph R6** — projection consumer edge
- [ ] **Dependency-graph R7** — shared kernel edge
- [ ] **DG-R5-EXCEPT-01** — documented exceptions only for R5
- [ ] **DG-R5-HOST-DOMAIN-FORBIDDEN** — host → domain forbidden (5 clauses, 2026-04-08)
- [ ] **DG-R5-01** — converted to exception; validate exception registry only
- [ ] **DG-R7-01** — baseline drift detection integrated
- [ ] **DG-BASELINE-01** — dependency-graph baseline present and up to date
- [ ] **DG-SCRIPT-HYGIENE-01** — script hygiene rules applied
- [ ] **DG-COMPOSITION-ROOT-01** — composition-root boundary held at runtime/platform seam

*Note: bare `R1..R7` in this section collide with Runtime Order `R1..R15` and Dead Code `R1..R4`. Validators MUST disambiguate by guard section.*

### Section 7 — Contracts Boundary

- [ ] **G-CONTRACTS-01** — shared contracts purity
- [ ] **G-CONTRACTS-02** — no domain leakage into shared kernel
- [ ] **G-CONTRACTS-03** — cross-domain contracts placement
- [ ] **G-CONTRACTS-04** — contracts naming
- [ ] **G-CONTRACTS-05** — contracts versioning
- [ ] **G-CONTRACTS-06** — contracts consumer discipline

### Section 8 — Code Quality Enforcement (Runtime Subsystem)

*Subsystem of Runtime Enforcement per GUARD-LAYER-MODEL-01.*

- [ ] **CCG-01** — readability
- [ ] **CCG-02** — function size & focus
- [ ] **CCG-03** — no spaghetti logic
- [ ] **CCG-04** — no over-engineering
- [ ] **CCG-05** — domain purity
- [ ] **CCG-06** — layer isolation
- [ ] **CCG-07** — determinism
- [ ] **CCG-08** — self-documenting code
- [ ] **CCG-09** — consistency
- [ ] **CCG-10** — testability
- [ ] **CCG-FILE-NAME-MATCHES-TYPE-01** — source `.cs` file name matches single public top-level type (S3 baseline; S2 when boundary-misleading)
- [ ] **GUARD-PIPELINE-TEMPLATE-01** — `/pipeline/*.md` templates MUST be per-batch-stateless; batch-specific inputs appear only as `<placeholder>` / `{classification}` markers; no literal classification names in template files
- [ ] **Dead Code R1** — reference check (runtime.guard.md §Dead Code Elimination)
- [ ] **Dead Code R2** — registration check
- [ ] **Dead Code R3** — projection consumption
- [ ] **Dead Code R4** — single pattern rule
- [ ] **STUB-R1** — no empty catch
- [ ] **STUB-R2** — no placeholder returns
- [ ] **STUB-R3** — no TODO-only methods
- [ ] **STUB-R4** — no unimplemented interfaces that ship
- [ ] **STUB-R5** — stub registry discipline
- [ ] **STUB-R6** — stub escape-hatch discipline

### Section 9 — Test & E2E Validation (Runtime Subsystem)

*Subsystem of Runtime Enforcement per GUARD-LAYER-MODEL-01.*

- [ ] **Test Architecture rules 1–5** (runtime.guard.md §Test Architecture — unnumbered rules): naming, isolation, determinism, invariant coverage, deterministic-test discipline
- [ ] **T-BUILD-01** — build gate (plus its strengthening clause)
- [ ] **T-DOUBLES-01** — test doubles policy
- [ ] **T-PLACEHOLDER-01** — no placeholder tests
- [ ] **T-POLICY-001** — policy-test enforcement
- [ ] **T1M-RESUME-TEST-COVERAGE-01** — T1M resume test coverage
- [ ] **ACT-FABRIC-ROUNDTRIP-TEST-01** — activation-fabric roundtrip test present
- [ ] **G-E2E-001** — no PASS without execution evidence
- [ ] **G-E2E-002** — layer coverage mandatory
- [ ] **G-E2E-003** — determinism in fixtures
- [ ] **G-E2E-004** — policy decision required
- [ ] **G-E2E-005** — chain anchor required
- [ ] **G-E2E-006** — DLQ before commit
- [ ] **G-E2E-007** — replay equivalence
- [ ] **G-E2E-008** — no test self-cleanup
- [ ] **G-E2E-009** — severity ladder
- [ ] **G-E2E-010** — untested = FAIL
- [ ] **G-E2E-011** — static checks are STAGE 0

### Section 10 — WBSM v3 Global Enforcement (shared)

- [ ] **GE-01** — deterministic execution
- [ ] **GE-02** — WHYCEPOLICY enforcement
- [ ] **GE-03** — WHYCECHAIN anchoring
- [ ] **GE-04** — event-first architecture
- [ ] **GE-05** — CQRS enforcement

### Section 11 — Rules Promoted from new-rules/ (2026-04-18)

- [ ] **RUNTIME-LAYER-PURITY-01** — `src/engines/**` does not reference `Whycespace.Runtime.*` namespace
- [ ] **SYSTEM-ORIGIN-BYPASS-01** — `CommandContext.IsSystem` set only by `ISystemIntentDispatcher.DispatchSystemAsync`
- [ ] **STEP-EXCEPTION-CONTRACT-01** — workflow step exceptions translated uniformly to `WorkflowStepResult.Failure`
- [ ] **ESCAPE-HATCH-COMMITMENT-01** — escape-hatch code carries tracked deadline comment
- [ ] **R-RT-USING-RUNTIME-01** — step/handler files carry `using Whycespace.Shared.Contracts.Runtime;`
- [ ] **R-TEST-PATH-01** — no absolute path literals under `tests/**`
- [ ] **R-TEST-PROJREF-01** — test projects' `ProjectReference` aligns with transitive source usage
- [ ] **PIPELINE-CANONICAL-ANCHOR-01** — prompts referencing `/pipeline/*` verify file presence before execution

---

## Check Procedure

1. Load the runtime guard rule set.
2. For each section, execute the per-section `Consolidated Check Procedure` in the guard.
3. Record a verdict per rule: `PASS` / `FAIL` / `N/A` with file:line evidence.
4. Aggregate by section and overall.

## Pass / Fail Criteria

- **PASS:** All rules `PASS` or `N/A`.
- **FAIL:** Any S0 or S1 failure.
- **CONDITIONAL:** S2/S3 captured to `claude/new-rules/` per CLAUDE.md $1c.

### Section 14 — R2.A Resilience

**R2.A.1 Retry Primitive**

- [ ] **R-RETRY-DET-01** — `src/runtime/resilience/**` contains no `DateTime.UtcNow` / `DateTimeOffset.UtcNow` / `Random` / `Stopwatch` (for event values) / raw `RandomNumberGenerator` calls. `DeterministicRetryExecutorTests.Two_Runs_With_Same_Context_And_Outcomes_Produce_Identical_Delays` passes. `DeterministicRandomProviderTests.NextDouble_Same_Seed_Returns_Same_Value` passes.
- [ ] **R-RETRY-CAT-01** — `RetryEligibility.IsRetryable` returns the canonical mapping for every `RuntimeFailureCategory` (including `PolicyEvaluationDeferred` added in R2.A.2). Theory test `RetryEligibility_Canonical_Mapping` covers 15 categories.
- [ ] **R-RETRY-CAP-01** — `DeterministicRetryExecutor.ExecuteAsync` completes in exactly `Policy.MaxAttempts` attempts under sustained retryable failure. `Sustained_Retryable_Failures_Exhaust_After_MaxAttempts` passes.
- [ ] **R-RETRY-EVIDENCE-01** — Every `RetryResult` returns `Attempts.Count == AttemptsMade` and every record carries non-default `StartedAt`/`CompletedAt` + correct `DelayBeforeAttempt`. `Attempt_Records_Contain_Every_Attempt_With_Timestamps` passes.

**R2.A.D.4 Circuit Breaker Registry + Runtime-State Aggregator Integration**

- [ ] **R-BREAKER-REGISTRY-01** — `ICircuitBreakerRegistry` lives at `src/shared/contracts/runtime/`; `CircuitBreakerRegistry` at `src/runtime/resilience/`. Exposes `Get(name)` (throws `KeyNotFoundException` on unknown), `TryGet(name)` (null-tolerant), `GetAll()` (sorted by `Name` for deterministic iteration). Duplicate-name registration throws at construction. `CircuitBreakerRegistryTests` (10 tests) passes.
- [ ] **R-BREAKER-HEALTH-POSTURE-01** — `RuntimeStateAggregator` constructor takes `ICircuitBreakerRegistry` (concrete `OpaPolicyEvaluator` dep removed). Both `ComputeFromResults` and `GetDegradedMode` emit identical reason strings for breaker signals via `AppendBreakerReasons`. Canonical name → reason mapping preserves existing reasons (`"opa-policy-evaluator"` → `"opa_breaker_open"`); unknown names fall through to `{normalized_name}_breaker_open`.
- [ ] **R2AD4-KEYED-TO-REGISTRY-01** — `PolicyInfrastructureModule` OPA breaker registration moved from `AddKeyedSingleton<ICircuitBreaker>("opa-policy-evaluator", ...)` to plain `AddSingleton<ICircuitBreaker>(...)` with `Name = "opa-policy-evaluator"`. `OpaPolicyEvaluator` consumer now resolves via `sp.GetRequiredService<ICircuitBreakerRegistry>().Get("opa-policy-evaluator")`. Replaces R2.A.D.2's keyed-DI approach.
- [ ] **R2AD4-REGISTRY-DI-WIRING-01** — `ObservabilityComposition` registers `ICircuitBreakerRegistry` via `sp.GetServices<ICircuitBreaker>()` enumeration so any module that adds a breaker (R2.A.D.3 Kafka / Postgres / Chain / Redis) flows into posture automatically without aggregator changes.

**R2.A.D.3d Redis Circuit Breaker (shared across execution lock + health probe)**

- [ ] **R-REDIS-BREAKER-01** — single shared `ICircuitBreaker` named `"redis"` registered in `CacheInfrastructureModule.AddCache`. Tunables via `Redis:BreakerThreshold` / `Redis:BreakerWindowSeconds`. Two consumers wrap it: `RedisExecutionLockProvider` (StringSetAsync + ScriptEvaluateAsync) and `RedisHealthSnapshotProvider` (PingAsync). Aggregator whitelist maps `"redis"` → `"redis_breaker_open"`.
- [ ] **R-REDIS-BREAKER-OPEN-FAIL-CLOSED-01** — `CircuitBreakerOpenException` never escapes either provider. `RedisExecutionLockProvider.TryAcquireAsync` catches → returns `false` (existing control-plane maps to `"execution_lock_unavailable"`). `ReleaseAsync` catches → no-op (lease expires via TTL). `RedisHealthSnapshotProvider.GetSnapshotAsync` catches → returns `IsHealthy: false, IsConnected: true, PingLatencyMs: null` snapshot. No new exception propagation paths at the API edge.
- [ ] **R2AD3D-LOCK-PROVIDER-WIRING-01** — `RedisExecutionLockProvider` constructor requires `ICircuitBreaker` (not optional — lock provider is load-bearing for every command dispatch, composition MUST wire it). `CacheInfrastructureModule` resolves via `ICircuitBreakerRegistry.Get("redis")`.
- [ ] **R2AD3D-HEALTH-PROVIDER-WIRING-01** — `RedisHealthSnapshotProvider` constructor takes optional `ICircuitBreaker?` (backward compat with tests that don't register the registry). `ObservabilityComposition` resolves via `sp.GetService<ICircuitBreakerRegistry>()?.TryGet("redis")`.

**R2.A.D.3c Postgres Pool Circuit Breaker (shared across all pooled acquisitions)**

- [ ] **R-POSTGRES-POOL-BREAKER-01** — single shared `ICircuitBreaker` named `"postgres-pool"` registered in `PostgresInfrastructureModule.AddDatabase`. Tunables via `Postgres:BreakerThreshold` / `Postgres:BreakerWindowSeconds` (defaults 5 / 30). All three `EventStoreDataSource` / `ChainDataSource` / `ProjectionsDataSource` wrappers carry the breaker and expose `OpenAsync(ct)` as the canonical acquire seam. Aggregator whitelist maps `"postgres-pool"` → `"postgres_pool_breaker_open"`.
- [ ] **R-POSTGRES-POOL-BREAKER-OPEN-SEMANTICS-01** — per-adapter Open-state handling per the posture table: Outbox enqueue / Idempotency / Sequence / Event store / Chain anchor / DLQ read paths / Projection writer / Projection store re-throw (mapper classifies as `DependencyUnavailable`); Kafka outbox publisher + Outbox depth sampler catch + log + skip; Advisory lease `TryAcquireAsync` returns `null`; DLQ `RecordAsync` catches + logs + swallows (Kafka DLQ is primary durability). `RuntimeExceptionMapper.Map` has a `CircuitBreakerOpenException` → `DependencyUnavailable` case.
- [ ] **R-POSTGRES-POOL-BREAKER-DATASOURCE-API-01** — `EventStoreDataSource.OpenAsync`, `ChainDataSource.OpenAsync`, `ProjectionsDataSource.OpenAsync` exist and delegate through the breaker. `grep -rn "\.Inner\.OpenInstrumentedAsync(" src/platform/host/adapters/` returns zero hits post-refactor. `Inner` property remains public for `Whycespace.Projections` cross-assembly consumption.
- [ ] **R2AD3C-PROJECTIONSTORE-FACTORY-01** — `ProjectionStoreFactory` gains optional trailing `ICircuitBreaker? poolBreaker` ctor parameter and passes it into every `PostgresProjectionStore<T>` it builds. Registered as a DI singleton in `PostgresInfrastructureModule`; the six projection modules (Todo / Kanban / MediaAsset / Course / Messaging / Economic) resolve the factory via DI instead of `new`ing it inline. `grep -rn "new ProjectionStoreFactory(" src/` returns zero hits post-refactor.

**R2.A.D.3b Kafka Producer Breaker (shared across outbox + consumer DLQ)**

- [ ] **R-KAFKA-BREAKER-01** — single shared `ICircuitBreaker` named `"kafka-producer"` registered in `KafkaInfrastructureModule.AddMessaging`. Tunables via `Kafka:BreakerThreshold` / `Kafka:BreakerWindowSeconds`. FOUR ProduceAsync call sites wrap it: outbox primary publish, outbox DLQ publish, consumer DLQ publish, enforcement→policy feedback handler. Aggregator whitelist maps `"kafka-producer"` → `"kafka_producer_breaker_open"`. Architecture test `No_direct_Kafka_publish_outside_outbox_publisher` sanctions exactly these four call sites + the `KafkaInfrastructureModule.cs` producer factory.
- [ ] **R-KAFKA-BREAKER-OPEN-BEHAVIOR-01** — per-call-site Open-state behavior: outbox primary publish catches and `continue`s without advancing `retry_count`; outbox DLQ publish catches and skips (DB row already at `status='deadletter'`); consumer DLQ publish re-throws to preserve K-DLQ-001 (no offset commit); `EnforcementToPolicyFeedbackHandler` catches + logs + swallows (fire-and-forget feedback bridge; source consumer MUST continue advancing offsets, feedback stream self-repairs on replay).
- [ ] **R2AD3B-FEEDBACK-BRIDGE-BREAKER-01** — `EnforcementToPolicyFeedbackHandler` takes optional trailing `ICircuitBreaker? kafkaBreaker` + `ILogger? logger` ctor parameters; `EconomicCompositionRoot` resolves breaker via `ICircuitBreakerRegistry.TryGet("kafka-producer")`. Handler's `HandleAsync` is now async + wraps `ProduceAsync` in the breaker when supplied, catching `CircuitBreakerOpenException` on the fire-and-forget path.

**R2.E.1 Consumer Rebalance Safety**

- [ ] **R-CONSUMER-REBALANCE-01** — every `ConsumerBuilder<>.Build()` in `src/platform/host/adapters/` is preceded by `KafkaRebalanceObservability.Attach(builder, topic, workerName, logger)`. 11 worker files wired: `GenericKafkaProjectionConsumerWorker` + 10 saga/integration workers. Three counters on the `Whycespace.Kafka.Consumer` meter: `consumer.rebalance.assigned` / `consumer.rebalance.revoked` / `consumer.rebalance.lost` tagged `{topic, worker}`. Architecture test `Every_ConsumerBuilder_Build_is_preceded_by_KafkaRebalanceObservability_Attach` pins the invariant.
- [ ] **R-CONSUMER-REBALANCE-COOPERATIVE-STICKY-01** — every `ConsumerConfig` in `src/platform/host/adapters/` sets `PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky`. Architecture test `Every_ConsumerConfig_sets_CooperativeSticky_partition_assignment_strategy` pins the invariant. Safe under per-message commit pattern — no cross-message batch state differs between strategies.
- [ ] **R-CONSUMER-REBALANCE-OBSERVABILITY-HELPER-01** — `src/platform/host/adapters/KafkaRebalanceObservability.cs` exists as a stateless static class. Single canonical `Attach` entry point; no worker registers inline `SetPartitionsAssignedHandler` / `SetPartitionsRevokedHandler` / `SetPartitionsLostHandler`. All observability flows through the dedicated `Whycespace.Kafka.Consumer` meter (separate from `Whycespace.Projection.Consumer`).

**R2.E.2 Consumer Lag Awareness**

- [ ] **R-CONSUMER-LAG-01** — every file in `src/platform/host/adapters/` that calls `consumer.Consume(` MUST also call `KafkaLagObservability.Record(consumer, result, workerName, topicTag)` after the null-check early return. All 11 consumer worker files wired. `consumer.lag_messages` histogram on the `Whycespace.Kafka.Consumer` meter tagged `{topic, worker, partition}`. Lag formula: `HighWatermark - ConsumedOffset - 1`. Architecture test `Every_Consumer_Consume_is_followed_by_KafkaLagObservability_Record` pins the invariant.
- [ ] **R-CONSUMER-LAG-HELPER-01** — `src/platform/host/adapters/KafkaLagObservability.cs` exists as a stateless static class. Single canonical `Record` entry point. `KafkaException` from `GetWatermarkOffsets` is swallowed and emitted as `consumer.lag_unknown` counter — the operator can distinguish "zero lag" from "couldn't read lag" (sustained `lag_unknown` indicates a post-rebalance pre-fetch state, a distinct health signal from healthy zero-lag).

**R2.E.3 Partition Skew Visibility**

- [ ] **R-CONSUMER-PARTITION-SKEW-01** — `KafkaLagObservability.Record` emits `consumer.messages_processed` (Counter<long>) on every successful lag observation, tagged `{topic, worker, partition}` identically to `consumer.lag_messages`. Throughput counter + lag histogram emit from the same method body so divergence is impossible by construction. `consumer.lag_unknown` path does NOT increment throughput — semantic parity.
- [ ] **R-CONSUMER-HOT-PARTITION-DETECTION-01** — hot-partition detection is a DERIVED signal from `consumer.lag_messages` per-partition percentile outliers; NO new `consumer.hot_partition_detected` metric exists. Threshold-based alerts live in operator alerting config (Grafana / Prometheus), not in adapter code.
- [ ] **R-CONSUMER-HOT-KEY-DETECTION-DEFERRED-01** — hot-key detection (stateful sketch with rebalance-reset semantics) explicitly deferred. Gap-matrix §12 "Hot-key detection" stays ABSENT with a documented reason.

**R2.E.4 Topic Provisioning Alignment**

- [ ] **R-TOPIC-ALIGNMENT-01** — `TopicProvisioningHostedService` registered FIRST in `KafkaInfrastructureModule.AddMessaging`. On `StartAsync` it calls `KafkaTopicVerifier.FindMissingTopicsAsync(admin, KafkaCanonicalTopics.All, 10s, logger, ct)` and logs WARN per missing topic plus an aggregate summary. With `Kafka:FailIfTopicsMissing=true` it throws `InvalidOperationException` so host doesn't reach ready. Broker-unreachable logs ERROR but does NOT fail startup. Config: `Kafka:VerifyTopicsOnStartup` (default true), `Kafka:FailIfTopicsMissing` (default false), `Kafka:TopicMetadataTimeoutMs` (default 10000).
- [ ] **R-TOPIC-CANONICAL-DECLARATION-01** — `KafkaCanonicalTopics.All` mirrors `infrastructure/event-fabric/kafka/create-topics.sh`. Both carry cross-reference comments. Architecture test `KafkaCanonicalTopics_mirrors_create_topics_sh_exactly` extracts topics from both files and asserts set equality; pass = no drift between runtime declaration and bash script.
- [ ] **R-TOPIC-VERIFIER-HELPER-01** — `KafkaTopicVerifier.cs` exists as stateless static class. `FindMissingTopicsAsync` is the single entry point. Uses `IAdminClient.GetMetadata` (single round-trip). Caller owns admin client lifecycle. `KafkaException` propagates; `OperationCanceledException` propagates unchanged.

**R2.A.3d Retry Tier Infrastructure**

- [ ] **R-RETRY-TIER-01** — four-tier shape (`commands` / `events` / `retry` / `deadletter`) enforced at 3 canonical routing seams: `TopicNameResolver.ResolveRetry`, `TopicNameResolver.ResolveDeadLetter`, `KafkaRetryEscalator.EscalateAsync`. Consumer error paths MUST use the escalator for handler-throws; poison-message paths continue direct to `.deadletter`.
- [ ] **R-RETRY-HEADERS-01** — `RetryHeaders.cs` declares four canonical header name constants: `retry-attempt-count`, `retry-max-attempts`, `retry-deliver-after-unix-ms`, `retry-source-topic`. Original envelope headers preserved across retry-tier transitions.
- [ ] **R-RETRY-ESCALATOR-01** — `KafkaRetryEscalator.EscalateAsync` is the single canonical escalation seam. Returns `RetryEscalationOutcome` record. Backoff formula: `min(maxBackoff, baseBackoff × 2^(attempt-1)) × (1 + jitter × 0.25)` with jitter from `IRandomProvider.NextDouble` seeded by `"{sourceTopic}:{eventId}:retry:{attemptCount}"`. `ProduceAsync` wrapped in shared `"kafka-producer"` breaker (5th sanctioned site).
- [ ] **R-RETRY-CONSUMER-WORKER-01** — `KafkaRetryConsumerWorker` subscribes to all `.retry` topics under consumer group `whyce.retry-consumer`. Parses retry headers; gate on deliver-after; republish to source `.events` topic; commit `.retry` offset. DelayCeilingMs=60000 for in-loop sleep; longer delays yield without committing. Uses R2.E.1 rebalance + R2.E.2 lag observability.
- [ ] **R-RETRY-DELIVER-AFTER-DETERMINISM-01** — deliver-after is replay-deterministic. Architecture test `KafkaRetryEscalator_delay_formula_uses_only_deterministic_entropy_sources` pins the invariant by scanning `KafkaRetryEscalator.cs` for forbidden entropy sources (`DateTime.UtcNow`, `DateTimeOffset.UtcNow`, `new Random`, `Stopwatch.*`).
- [ ] **R-RETRY-CONSUMER-INTEGRATION-01** — `GenericKafkaProjectionConsumerWorker` handler + writer in dedicated inner try/catch. Escalates on exception via `KafkaRetryEscalator` when retry tier wired (all 4 new ctor params non-null); falls back to Kafka-redelivery otherwise. `CircuitBreakerOpenException` during escalate → do NOT commit. Poison-message paths unchanged (direct DLQ). 6 projection composition modules (Todo / Kanban / MediaAsset / Course / Messaging / Economic) pass the 3 new optional parameters via DI.
- [ ] **R-RETRY-CONSUMER-INTEGRATION-02** — R2.A.3d Phase B shipped. 9 of 10 non-projection workers migrated to the canonical retry pattern: inner try/catch around handler dispatch + local `EscalateOrRedeliverAsync` helper that delegates to `KafkaRetryEscalator.EscalateAsync`. 5 new optional trailing ctor params per worker (`producer`, `topicNameResolver`, `retryOptions`, `randomProvider`, `kafkaBreaker`) wired in `EconomicCompositionRoot` at all 9 sites. `RetryHeaders.ReadPriorAttemptCount` promoted to shared helper. Architecture test `Every_consumer_worker_except_reconciliation_escalates_handler_throws_via_KafkaRetryEscalator` pins the invariant (scans `adapters/*.cs` for `consumer.Consume(` AND lack of both `KafkaRetryEscalator.EscalateAsync(` and `EscalateOrRedeliverAsync(`).
- [ ] **R-RETRY-CONSUMER-INTEGRATION-EXCLUSION-01** — `ReconciliationLifecycleWorker` intentionally excluded: documented commit-on-failure semantics to avoid poison-loop stalls. Architecture test exempts this file by name.
- [ ] **R2A3D-COMPOSITION-WIRING-01** — `KafkaInfrastructureModule` registers `RetryTierOptions` singleton (config keys `Kafka:Retry:MaxAttempts`, `Kafka:Retry:BaseBackoffSeconds`, `Kafka:Retry:MaxBackoffSeconds`; defaults 5/5/300) and `KafkaRetryConsumerWorker` as `IHostedService`.
- [ ] **R-CIRCUIT-BREAKER-VOID-EXT-01** — `CircuitBreakerExtensions.ExecuteAsync(Func<CancellationToken, Task>, CancellationToken)` extension at `src/shared/contracts/runtime/CircuitBreakerExtensions.cs`. Ergonomic helper for side-effect-only operations like `ProduceAsync`. Extension-only — no interface change.
- [ ] **R2AD3B-OUTBOX-BREAKER-WIRING-01** — `KafkaOutboxPublisher` takes optional `ICircuitBreaker? kafkaBreaker`; composition resolves via `ICircuitBreakerRegistry.Get("kafka-producer")`.
- [ ] **R2AD3B-CONSUMER-BREAKER-WIRING-01** — `GenericKafkaProjectionConsumerWorker` takes optional `ICircuitBreaker? kafkaBreaker`; 6 projection composition sites (Todo / Kanban / MediaAsset / Messaging / Course / Economic) resolve via `sp.GetService<ICircuitBreakerRegistry>()?.TryGet("kafka-producer")`.

**R2.A.D.3a Chain Adapter Refactor (second consumer of ICircuitBreaker)**

- [ ] **R-CHAIN-BREAKER-DELEGATION-01** — `WhyceChainPostgresAdapter` constructor takes `ICircuitBreaker` parameter. Inline state (`_breakerLock` / `_consecutiveFailures` / `_openedAt`) + private helpers removed. `AnchorAsync` DB work wrapped in `_breaker.ExecuteAsync<ChainBlock>(...)`. Architecture test `WhyceChainPostgresAdapter_Delegates_Breaker_State_To_ICircuitBreaker` (comment-stripped scan, mirrors OPA arch test) passes.
- [ ] **R-CHAIN-BREAKER-BOUNDARY-01** — `CircuitBreakerOpenException` caught at the adapter boundary and translated to `ChainAnchorUnavailableException(reason: "breaker_open", retryAfterSeconds: breakerEx.RetryAfterSeconds, ...)`. Pre-refactor external contract preserved — `ChainAnchorService` + API-edge handler see the same typed exception.
- [ ] **R2AD3A-COMPOSITION-WIRING-01** — `ChainInfrastructureModule.AddChain` registers a plain `ICircuitBreaker` with `Name = "chain-anchor"` and `FailureThreshold`/`WindowSeconds` sourced from `ChainAnchorOptions`. `WhyceChainPostgresAdapter` resolves its breaker via `ICircuitBreakerRegistry.Get("chain-anchor")`. Registry picks it up automatically via `sp.GetServices<ICircuitBreaker>()` enumeration (R2.A.D.4).
- [ ] **R2AD3A-AGGREGATOR-CLEANUP-01** — `RuntimeStateAggregator.CanonicalBreakerReasons` gains `"chain-anchor"` → `"chain_anchor_breaker_open"` entry. Concrete `WhyceChainPostgresAdapter _chainAdapter` constructor dependency REMOVED — registry iteration now covers chain too. Both `ComputeFromResults` and `GetDegradedMode` emit chain reason via `AppendBreakerReasons` only. `grep -Pn "_chainAdapter" src/platform/host/health/RuntimeStateAggregator.cs` returns zero hits.

**R2.A.D.2 OPA Evaluator Refactor (first consumer of ICircuitBreaker)**

- [ ] **R-OPA-BREAKER-DELEGATION-01** — `OpaPolicyEvaluator` constructor takes `ICircuitBreaker` parameter. Inline state (`_breakerLock` / `_consecutiveFailures` / `_openedAt`) + private helpers (`IsBreakerOpenInternal` / `RecordSuccess` / `RecordFailure`) REMOVED. Architecture test `OpaPolicyEvaluator_Delegates_Breaker_State_To_ICircuitBreaker` (using `StripCommentAndString` to avoid matching explanatory comments) passes.
- [ ] **R-OPA-BREAKER-BOUNDARY-01** — `CircuitBreakerOpenException` caught at the adapter boundary and translated to `PolicyEvaluationUnavailableException(reason: "breaker_open", retryAfterSeconds: breakerEx.RetryAfterSeconds, ...)`. Pre-R2.A.D.2 external contract preserved — `PolicyMiddleware` retry wrapper + API-edge 503 handler see the same typed exception. `PolicyEvaluationRetryPatternTests` (6 tests) still passes unchanged.
- [ ] **R2AD2-COMPOSITION-WIRING-01** — `PolicyInfrastructureModule.AddPolicy` registers a keyed `ICircuitBreaker` under name `"opa-policy-evaluator"` with `FailureThreshold`/`WindowSeconds` sourced from `OpaOptions`. Uses `AddKeyedSingleton` + `GetRequiredKeyedService` so other future breakers (Kafka / Postgres / Chain / Redis) coexist under distinct keys.
- [ ] **R2AD2-HC2-GETTER-PRESERVED-01** — public `IsBreakerOpen` getter on `OpaPolicyEvaluator` retained with adapted semantics (`_breaker.State != CircuitBreakerState.Closed`) so the `IRuntimeStateAggregator` HC-2 consumer sees the same boolean value pre/post refactor.

**R2.A.D.1 Circuit Breaker Primitive**

- [ ] **R-CIRCUIT-BREAKER-01** — `ICircuitBreaker` + `CircuitBreakerState` + `CircuitBreakerOpenException` + `CircuitBreakerOptions` live at `src/shared/contracts/runtime/ICircuitBreaker.cs`. `DeterministicCircuitBreaker` at `src/runtime/resilience/DeterministicCircuitBreaker.cs` implements the three-state machine. `DeterministicCircuitBreakerTests` (18 tests) passes.
- [ ] **R-CIRCUIT-BREAKER-DET-01** — `DeterministicCircuitBreaker.cs` contains no `DateTime.UtcNow` / `DateTimeOffset.UtcNow` / `Random` / `Stopwatch`-derived values. All timing via `IClock.UtcNow`. Determinism test `Two_Breakers_With_Same_Clock_And_Outcomes_Reach_Identical_State` passes. `State_Getter_Does_Not_Consume_HalfOpen_Trial_Slot` passes.
- [ ] **R-CIRCUIT-BREAKER-PER-DEPENDENCY-01** — awaiting R2.A.D.2 OPA evaluator refactor (first consumer) + R2.A.D.3 composition wiring for Kafka / Postgres / Chain / Redis.

**R2.A.C.2.5 Worker-Fleet Leader Migration (audit + second migration)**

- [ ] **R2AC25-FLEET-AUDIT-01** — `claude/new-rules/20260419-110000-guards.md` records the R-MULTI-INSTANCE-AUDIT-01 verdict for every BackgroundService under `src/platform/host/adapters/`. 2 migrated (SystemLock + SanctionExpiry timer-based schedulers); 13 explicitly SKIP with documented reason (Kafka consumer-group, per-instance sampler, or existing row-lock forbid). Fleet audit complete.
- [ ] **R2AC25-SANCTION-MIGRATION-01** — `SanctionExpirySchedulerWorker` optional `IDistributedLeaseProvider? leaseProvider` trailing constructor param; `ExecuteAsync` branches between legacy `ScanLoopAsync` and `LeaderElection.RunAsLeaderAsync` wrapper; composition in `EconomicCompositionRoot` threads `sp.GetService<IDistributedLeaseProvider>()`. Direct parallel to the R2.A.C.2 SystemLock migration — same 10 `LeaderElectionTests` exercise the helper pattern; no per-worker tests duplicate coverage.
- [ ] **R2AC25-DEFAULT-PATTERN-RULE-01** — new BackgroundServices added to `src/platform/host/adapters/` require a fleet-audit entry in `claude/new-rules/` BEFORE being eligible for leader migration. Default pattern per worker type: Kafka consumer → consumer group; timer DB-scan → leader-election; admission-sampler → per-instance.

**R2.A.C.2 Single-Leader Helper + Reference Migration**

- [ ] **R-LEADER-ELECTION-01** — `LeaderElection.RunAsLeaderAsync` lives at `src/runtime/resilience/LeaderElection.cs`. Composes over `IDistributedLeaseProvider` + worker loop lambda. No TTL arithmetic. `LeaderElectionTests` (10 tests) passes.
- [ ] **R-MULTI-INSTANCE-AUDIT-01** — `SystemLockExpirySchedulerWorker` audit verdict recorded in `claude/new-rules/20260419-100000-guards.md`. Worker constructor has optional `IDistributedLeaseProvider? leaseProvider` trailing parameter; `ExecuteAsync` branches between legacy `ScanLoopAsync` (null provider) and `LeaderElection.RunAsLeaderAsync` (non-null). Log line reports `leaderMode=on/off` at startup.
- [ ] **R2AC2-COMPOSITION-WIRING-01** — `EconomicCompositionRoot` threads `sp.GetService<IDistributedLeaseProvider>()` into `SystemLockExpirySchedulerWorker`. `sp.GetService` (not `GetRequiredService`) tolerates legacy hosts.
- [ ] **R2AC2-FORBID-LEADER-ON-ROW-LOCK-WORKERS-01** — `KafkaOutboxPublisher` + `GenericKafkaProjectionConsumerWorker` MUST NOT be migrated to `LeaderElection`. Their existing row-lock / consumer-group patterns are correct; adding leader election violates documented MI-2 / consumer-group invariants.

**R2.A.C.1 Distributed Lease Primitive**

- [ ] **R-LEASE-CONTRACT-01** — `IDistributedLeaseProvider.TryAcquireAsync` returns `ILease?` (null on contention, NOT exception). `ILease` is `IAsyncDisposable`. Contract lives at `src/shared/contracts/infrastructure/persistence/IDistributedLeaseProvider.cs`. `DistributedLeaseContractTests` (13 tests) passes — covers acquire / release / contention / reacquire-after-release / 50-way concurrent-acquire single-winner / double-dispose idempotency / input validation.
- [ ] **R-LEASE-POSTGRES-01** — `PostgresAdvisoryLeaseProvider` uses `pg_try_advisory_lock(bigint)` at session level. String keys hashed via SHA256-prefix → bigint. Dedicated connection held per lease; returned to pool on dispose. Busy-key path releases connection before returning null. Registered in `PostgresInfrastructureModule`.
- [ ] **R-LEASE-CRASH-SAFE-01** — NO application-level TTL / expiry calculation in `PostgresAdvisoryLeaseProvider`. Probe: `grep -Pn "TimeSpan.*TTL\|expires_at\|lease_expiry" src/platform/host/adapters/PostgresAdvisoryLeaseProvider.cs` MUST return zero hits. Session closes → Postgres releases lock → another holder acquires. No clock-skew bugs by construction. R5 integration test: kill process mid-lease → reacquire within TCP keepalive window.
- [ ] **R2AC1-DIFFERENT-FROM-EXEC-LOCK-01** — `IDistributedLeaseProvider` and `IExecutionLockProvider` coexist: former = longer-lived coordination (leadership, workflow sequencing), latter = short-lived per-command execution lock (Redis-backed, TTL). Callers MUST pick the right primitive; documentation on `IDistributedLeaseProvider` references the distinction.

**R2.A OPA Resilience (OPA Adapter Classification)**

- [ ] **R-POL-OPA-RETRY-01** — `PolicyMiddleware.EvaluateOpaWithRetryAsync` catches `PolicyEvaluationUnavailableException` inside an `IRetryExecutor` loop, classifies as `PolicyEvaluationDeferred`, re-throws with `retry_exhausted:<original>` + preserved `RetryAfterSeconds` on exhaustion. `PolicyEvaluationRetryPatternTests` (6 tests) passes. Architecture guard `PolicyMiddleware_Catches_PolicyEvaluationUnavailableException_Inside_Retry_Loop` passes.
- [ ] **R2AOPA-DI-WIRING-01** — `RuntimeComposition` threads `IRetryExecutor` into `PolicyMiddleware` via optional `retryExecutor:` named argument. `sp.GetService<IRetryExecutor>()` (not `GetRequiredService`) — legacy test hosts without the executor registered fall back to pre-R2.A.OPA pass-through.
- [ ] **R2AOPA-RETHROW-PRESERVES-RETRY-HINT-01** — exhausted retry re-throws a fresh `PolicyEvaluationUnavailableException` with `RetryAfterSeconds` copied from the LAST caught exception (not the first) — API-edge `Retry-After` header reflects the most recent backoff signal. `Exhaustion_Final_Exception_Preserves_Last_RetryAfter_Not_First` test pins this.
- [ ] **R2AOPA-NO-SILENT-ALLOW-01** — no path in `EvaluateOpaWithRetryAsync` returns a `Success` outcome without a genuine `PolicyDecision` from the evaluator. `Retries_Surface_Deny_Decision_When_Recovered` test confirms a DENY after transient failures is surfaced verbatim, not silently converted to ALLOW.

**R2.A.3c Consumer DLQ Mirror + Category Classification**

- [ ] **R-DLQ-STORE-CONSUMER-MIRROR-01** — `GenericKafkaProjectionConsumerWorker.PublishToDeadletterAsync` mirrors every Kafka `.deadletter` publish to `IDeadLetterStore.RecordAsync` after broker ack. Best-effort + non-blocking. All 6 composition modules (Todo / Kanban / MediaAsset / Messaging / Course / Economic projections) thread `IDeadLetterStore` via `sp.GetService<IDeadLetterStore>()` — null-tolerant for legacy tests, populated in production by `PostgresInfrastructureModule`.
- [ ] **R-DLQ-CATEGORY-POISON-01** — All 6 in-worker `PublishToDeadletterAsync` callsites pass `RuntimeFailureCategory.PoisonMessage` (4 header-malformation paths + 1 unparseable-GUID path + 1 `IsPoisonedPayload` payload path). Category persisted on `DeadLetterEntry.FailureCategory` and emitted as `dlq-category` Kafka header.
- [ ] **R2A3C-FALLBACK-EVENTID-01** — When a poison message lacks a parseable `event-id` header, `DeriveFallbackEventId` produces a deterministic SHA256-based `Guid` from `(key, value)`. Idempotent on identical poison bytes — retry of the same poison record collapses to a single DLQ store row.
- [ ] **R2A3C-REGRESSION-GUARD-01** — Full unit suite (299 tests) passes post-R2.A.3c. Host project builds 0 errors across all 6 updated composition modules.

**R2.A.3b DLQ Store Implementation + Outbox Wiring**

- [ ] **R-DLQ-STORE-01 implementation** — `PostgresDeadLetterStore` at `src/platform/host/adapters/PostgresDeadLetterStore.cs`. Uses `INSERT ... ON CONFLICT (event_id) DO NOTHING` for idempotency. Migration `infrastructure/data/postgres/deadletter/migrations/001_deadletter.sql` creates the `dead_letter_entries` table with partial index on `(source_topic, enqueued_at DESC) WHERE reprocessed_at IS NULL` for the R4.B operator-inspection query shape.
- [ ] **R2A3B-OUTBOX-MIRROR-01** — `KafkaOutboxPublisher.TryPublishToDeadletterAsync` mirrors every Kafka `.deadletter` publish to `IDeadLetterStore` after the broker acknowledges the Kafka message. Best-effort: store-write failure is caught + logged; the Kafka message + DB outbox row remain the authoritative records. `IDeadLetterStore` is an optional constructor dependency — legacy tests without the store registered see no behaviour change.
- [ ] **R2A3B-DLQ-CONTRACT-IDEMPOTENCY-01** — `DeadLetterStoreContractTests.RecordAsync_Under_Concurrent_Contention_Collapses_To_Single_Row` passes (50-way concurrent record of same EventId → single stored entry).
- [ ] **R2A3B-DLQ-CONTRACT-QUERIES-01** — `ListAsync` filters by source topic, respects `since` time window, orders newest-first, caps `limit` at 1000. `MarkReprocessedAsync` hides entry from default list + preserves row for audit. Tests in `DeadLetterStoreContractTests` pass.

**R2.A.3a Tiered Topic Naming + DLQ Store Contract**

- [ ] **R-TOPIC-TIER-01** — `TopicNameResolver.ResolveRetry` + `ResolveDeadLetter` honor the `.events` → `.retry` → `.deadletter` chain. Resolver rejects retry-from-deadletter (terminal) and retry-from-commands (use `IRetryExecutor` at call site). `TopicNameResolverRetryTierTests` (13 tests) passes. Probe: `grep -RnP "\.(retry|deadletter)" src/platform/host/adapters src/runtime/event-fabric | grep -v TopicNameResolver.cs` expected to return zero hits for string literals once R2.A.3c migrates existing inline usages.
- [ ] **R-DLQ-STORE-01** — `IDeadLetterStore` contract exists at `src/shared/contracts/infrastructure/messaging/IDeadLetterStore.cs`. `DeadLetterEntry` record carries `EventId` / `SourceTopic` / `EventType` / `CorrelationId` / `EnqueuedAt` / `FailureCategory` / `LastError` / `AttemptCount` / `Payload` / `SchemaVersion` / `ReprocessedAt` / `ReprocessedByIdentityId`. Implementation deferred to R2.A.3b (PostgresDeadLetterStore + idempotency integration test).

**R2.A.2 Policy-Failure Classification + Reference Migration**

- [ ] **POL-FAIL-MODE-01** — `PolicyDecision.FailureMode` populated ONLY when the evaluator could not produce a deterministic allow/deny. Null on normal allow/deny. When set, `IsAllowed` MUST be `false`. Values: `FailClosed` (default, reject) / `FailOpen` (audited degraded posture, never default) / `Defer` (retryable via `IRetryExecutor`). Source: `src/shared/contracts/infrastructure/policy/PolicyFailureMode.cs`.
- [ ] **POL-FAIL-DEFERRED-CAT-01** — `RuntimeFailureCategory.PolicyEvaluationDeferred` is retryable per `RetryEligibility.IsRetryable`. On exhaustion, retry caller converts the outcome to `PolicyDenied` per POL-FAIL-CLASS-01. Current implementations of `IPolicyEvaluator` default to `FailureMode = null` (no deferred classification yet) — upgrade path in a future R2.A.2+ batch when OPA adapter gains 503/timeout classification.
- [ ] **R2A2-POSTTOLEDGER-MIGRATION-01** — `PostToLedgerStep.ExecuteAsync` uses `IRetryExecutor` when injected (host DI path) and falls back to the legacy in-line loop only when not injected (pre-DI tests). The legacy path is time-boxed for removal in a later R2.A batch once all hosts register the executor.
- [ ] **R2A2-DI-WIRING-01** — `CoreComposition.AddCoreComposition` registers `IRandomProvider → DeterministicRandomProvider` and `IRetryExecutor → DeterministicRetryExecutor` as singletons. `Whycespace.Host.csproj` builds 0 errors with the registration.

### Section 13 — R1 Replay Determinism Certification (Batch 6)

Formal checklist for R1 exit. Each item is a concrete audit probe; the R1 audit-sweep output under `claude/audits/sweeps/` records pass/fail per item.

**Determinism sources (§3 of the spec):**
- [ ] `IClock` is the single source of time in `src/runtime/**`, `src/engines/**`, `src/domain/**`, `src/platform/host/adapters/**`. Probe: `grep -RnP "\bDateTime(\.UtcNow|\.Now)\b|\bDateTimeOffset(\.UtcNow|\.Now)\b" src/runtime src/engines src/domain src/platform/host/adapters | grep -v "SystemClock"` → zero hits.
- [ ] `IIdGenerator` is the single source of identity. Probe: `grep -RnP "\bGuid\.NewGuid\s*\(" src/runtime src/engines src/domain src/platform/host/adapters | grep -v "DeterministicIdGenerator"` → zero hits.
- [ ] `IRandomProvider` is the single source of non-cryptographic randomness. R1 state: contract exists, no callers. R2 onward: probe `grep -RnP "\bRandom\b|\bRandom\.Shared\b|RandomNumberGenerator\.GetBytes" src/runtime src/engines src/domain src/platform/host/adapters | grep -v "DeterministicRandomProvider"` → zero hits.

**Execution hash determinism:**
- [ ] `ExecutionHash.Compute(context, events)` is called exactly once per middleware stage that needs it and produces identical output for identical inputs. Probe: `CommandContextReplayResetTests.ResetForReplay_Clears_Every_WriteOnce_Guard_In_One_Shot` + any `ExecutionHashDeterminismTests` pass.
- [ ] `CommandContext.ResetForReplay()` clears EVERY write-once backing field except those explicitly exempt (posture stamps). Probe: `CommandContextReplayResetTests.ResetForReplay_Covers_Every_Known_WriteOnce_Field` passes.

**Policy evaluation determinism:**
- [ ] Every `IPolicyEvaluator.EvaluateAsync` receives a `PolicyContext` with `Now` stamped from a single `IClock.UtcNow` read (no re-read inside the evaluator). Probe: visual review + `POLICY-INPUT-ENVELOPE-01`.
- [ ] `PolicyInputBuilder.Enrich` is deterministic: same inputs → same output, no dictionary-order leak. R1 state: 4-arg + 6-arg overloads confirmed pure.
- [ ] `PolicyDecisionHash` is a pure function of `PolicyContext` + `command` state. Probe: two consecutive evaluations of identical input produce identical hash.

**Replay semantics:**
- [ ] `EventReplayService.ReplayAsync` produces events byte-identical to the original execution for every aggregate type with a test. Probe: replay-determinism regression suite (R1: pinned for PayoutAggregate + LedgerAggregate; expand in R5).
- [ ] Workflow replay via `WorkflowExecutionReplayService.ReplayAsync` produces identical step ids, execution hashes, step outputs. Probe: existing workflow-replay tests (`tests/unit/runtime/CommandContextReplayResetTests` + `tests/integration` workflow-resume).

**Failure-taxonomy stability:**
- [ ] Every `CommandResult.Failure(...)` in `src/runtime/**` carries a `RuntimeFailureCategory`. Probe: `RuntimeLayer_uses_only_categorized_CommandResult_Failure_overloads` architecture test passes.
- [ ] `RuntimeExceptionMapper.Map(ex)` is a pure function — same exception type + SQLSTATE → same category. Probe: unit coverage on mapper (to be added in R2 alongside the first real try/catch consumer).

**Retry / idempotency replay:**
- [ ] Idempotency keys derive from `command.GetType().Name` + `context.CommandId` — no clock, no random. Probe: visual review of `IdempotencyMiddleware.ExecuteAsync`.
- [ ] Idempotency outcome is deterministic across instances sharing the `IIdempotencyStore`: under concurrent contention, exactly one `Miss` + N-1 `Hit`. Probe: `IdempotencyConcurrencyStressTests.TryClaim_Under_100_Way_Contention_Resolves_To_Exactly_One_Winner` passes.

**Runtime state seams:**
- [ ] No middleware or control-plane type holds static mutable state. Probe: `RuntimeMiddleware_holds_no_static_mutable_state` architecture test passes.
- [ ] `CommandContext` write-once setters throw on second assignment for every R1 write-once field. Probe: `WriteOnce_All_Guarded_Fields_Throw_On_Second_Assignment`.

**R1 exit condition:** Every probe above returns PASS. Any FAIL captured to `claude/new-rules/` per $1c with promotion path defined.

### Section 12 — R1 Foundation Hardening (Batch 3.5)

- [ ] **R-FAIL-CAT-01** — every `CommandResult.Failure(...)` in `src/runtime/**` uses a categorized overload (no plain 1-arg `Failure(err)` calls). Probe: `grep -Pn "CommandResult\.Failure\([^,)]+\)" src/runtime/` returns zero hits.
- [ ] **R-FAIL-CAT-02** — category-to-stage mapping honored: ValidationMiddleware→ValidationFailure(*); AuthorizationGuardMiddleware→AuthorizationDenied; PolicyMiddleware deny→PolicyDenied; ExecutionGuardMiddleware enforcement→RuntimeGuardRejection; ExecutionGuardMiddleware lock-unavailable→DependencyUnavailable; IdempotencyMiddleware duplicate→ConcurrencyConflict+IsDuplicate; ControlPlane cancellation→Cancellation; ControlPlane WHYCEPOLICY hard-stop→PolicyDenied.
- [ ] **R-EXC-MAP-01** — no `catch { return CommandResult.Failure(ex.Message); }` patterns under `src/runtime/**`. `RuntimeExceptionMapper.ToCommandResult` is the only exception→result bridge. S2 today, S1 after R2 top-level try/catch lands.
- [ ] **R-IDEM-EVIDENCE-01** — `IdempotencyMiddleware` stamps `CommandContext.IdempotencyKey` + `IdempotencyOutcome` on every invocation; duplicate path sets `IsDuplicate=true` + `IdempotencyKey` on the result.
- [ ] **R-POLICY-OVERLAY-01** — `PolicyMiddleware.ExecuteAsync` calls the 6-arg `PolicyInputBuilder.Enrich` (with environment + jurisdiction, even if currently null).
- [ ] **R-CTX-SESSION-01** — `CommandContext` has write-once setters for `SessionId`, `TokenFingerprint`, `StepUpAuthenticatedAt`, `IdempotencyKey`, `IdempotencyOutcome`; `ResetForReplay()` clears all five alongside the pre-R1 write-once set.
- [ ] **R-STATE-BOUNDARY-01** — architecture test `RuntimeMiddleware_holds_no_static_mutable_state` passes. No non-readonly static fields under `src/runtime/middleware/**` or `src/runtime/control-plane/**`.
- [ ] **DET-RAND-01** — `IRandomProvider` exists at `src/shared/kernel/domain/IRandomProvider.cs`. No production callers required in R1. `Random` / `Random.Shared` / `RandomNumberGenerator.GetBytes` for non-crypto use absent from `src/runtime/**`, `src/engines/**`, `src/domain/**`, `src/platform/host/adapters/**`.
- [ ] **POL-FAIL-CLASS-01** — policy-failure classification rule captured in `claude/new-rules/20260419-010000-guards.md`. No implementation in R1; R2 retry logic implements FAIL_CLOSED / FAIL_OPEN / DEFER enum + `PolicyEvaluationDeferred` category. Audit-by-code check deferred to R2.

**R3.A Workflow Runtime Reliability (R3.A.1 + R3.A.2 shipped)**

- [ ] **R-WORKFLOW-OBSERVABILITY-01** — `T1MWorkflowEngine` emits `workflow.execution.duration` + `workflow.step.duration` histograms on the `Whycespace.Workflow` meter, tagged `{workflow_name, outcome}` (execution) and `{workflow_name, step_name, outcome}` (step). Outcome vocabulary exactly 5 values: `success`, `failed`, `timeout_step`, `timeout_execution`, `cancelled`. Every exit path (`throw` / `return` / normal completion) records an outcome. `workflow.step.duration` emits once PER ATTEMPT post-R3.A.2 so histogram sample count equals total step attempts.
- [ ] **R-WORKFLOW-OBSERVABILITY-COMPLETION-COUNTER-01** — `workflow.execution.completed` counter; one increment per `ExecuteAsync` return, tagged identically. Emitted from the shared `RecordExecution` helper.
- [ ] **R-WORKFLOW-OBSERVABILITY-DETERMINISM-NOTE-01** — `T1MWorkflowEngine.cs` contains NO `DateTime.UtcNow` / `DateTimeOffset.UtcNow` / `new Random` / `Random.Shared`. `Stopwatch.GetTimestamp` + `Stopwatch.GetElapsedTime` + deterministic exponential backoff are the only timing sources. Architecture test pins this.
- [ ] **R-WORKFLOW-STEP-RETRY-01** — retry loop bounded by `WorkflowOptions.StepRetryMaxAttempts`. Soft (`!stepResult.IsSuccess`) and hard (non-cancel, non-timeout `Exception`) failures retry with exponential backoff `min(MaxBackoff, Base × 2^(attempt-1))`. Sleep runs under `executionCts.Token`. Each attempt gets a fresh per-step CTS.
- [ ] **R-WORKFLOW-STEP-RETRY-NON-RETRYABLE-EXCLUSION-01** — three catch filters (caller-cancel, execution-timeout, per-step-timeout) preserve pre-R3.A.2 shape; retry loop does NOT wrap them. Architecture test `T1MWorkflowEngine_step_retry_loop_has_explicit_non_retryable_guards` pins the filter forms.
- [ ] **R-WORKFLOW-STEP-RETRY-OBSERVABILITY-01** — `workflow.step.retry_attempts` (Counter<long>) incremented once per retry (attempts after the first). Tagged `{workflow_name, step_name}` (no outcome tag). `StepRetryAttempts.Add(1, ...)` call pinned by architecture test.
- [ ] **R-WORKFLOW-STEP-RETRY-REPLAY-DETERMINISM-01** — retries emit metrics only, NOT domain lifecycle events. Event stream sees final outcome. Resume from `WorkflowLifecycleEventFactory.Resumed` resets attempt counter to 1. Cross-process audit-visible retry is `.retry` Kafka tier (R2.A.3d).
- [ ] **R-WORKFLOW-STEP-EXCEPTION-CLASSIFICATION-01** — `WorkflowStepFailureClassifier` at `src/engines/T1M/core/workflow-engine/WorkflowStepFailureClassifier.cs`. Pattern-matches BCL types: `ArgumentException` → ValidationFailed, `UnauthorizedAccessException`/`SecurityException` → AuthorizationDenied, `InvalidOperationException`/`NotSupportedException` → InvalidState, `OperationCanceledException` → Cancellation. All other types → Retryable (conservative default). Architecture test `WorkflowStepFailureClassifier_lives_in_engine_layer_without_runtime_dependency` pins layer discipline.
- [ ] **R-WORKFLOW-STEP-EXCEPTION-ENGINE-WIRING-01** — `T1MWorkflowEngine` `catch (Exception hardEx)` branch calls `Classify(hardEx)` BEFORE deciding to retry. Terminal → fast-fail (counter + lifecycle event + return). Retryable → R3.A.2 backoff loop. Architecture test `T1MWorkflowEngine_classifies_hard_failures_via_WorkflowStepFailureClassifier_before_retry` pins the integration.
- [ ] **R-WORKFLOW-STEP-EXCEPTION-COUNTER-01** — `workflow.step.terminal_failures` counter on `Whycespace.Workflow` meter. Tagged `{workflow_name, step_name, category}` (~6 category values = bounded cardinality).
- [ ] **R-WORKFLOW-STEP-EXCEPTION-REPLAY-DETERMINISM-01** — no new domain events. Final outcome event unchanged. Classification info in `Failed` reason string is human-readable only, not a replay discriminator.
- [ ] **R-WORKFLOW-CANCELLATION-EVENT-01** — `WorkflowExecutionCancelledEvent` domain record + `WorkflowExecutionCancelledEventSchema` shared-contract schema. `WorkflowExecutionStatus.Cancelled` appended (ordinal 4). Aggregate `Apply` case transitions status. Terminal state; cannot resume.
- [ ] **R-WORKFLOW-CANCELLATION-FACTORY-01** — `WorkflowLifecycleEventFactory.Cancelled(workflowExecutionId, stepName, reason)` is the canonical construction site. No public `Cancel` method on aggregate.
- [ ] **R-WORKFLOW-CANCELLATION-ENGINE-EMISSION-01** — engine caller-cancel catch branch emits the factory-constructed event BEFORE the OCE re-throw. Reason: `"caller_cancellation: TypeName: message"`. Architecture test pins emit-before-throw ordering via source index comparison.
- [ ] **R-WORKFLOW-CANCELLATION-SCHEMA-REGISTRATION-01** — `WorkflowExecutionSchemaModule` registers the new schema + payload mapper alongside the existing 5 lifecycle schemas. Architecture test pins the registration.
- [ ] **R-WORKFLOW-CANCELLATION-REPLAY-DETERMINISM-01** — `Cancelled` is terminal; existing `Resumed` guard (post-R3.A.3: `Status != Failed && Status != Suspended`) blocks resume. Re-run requires new aggregate id.
- [ ] **R-WORKFLOW-SUSPEND-EVENT-01** — `WorkflowExecutionSuspendedEvent` domain record + `WorkflowExecutionSuspendedEventSchema` shared-contract schema. `WorkflowExecutionStatus.Suspended` appended (ordinal 5). Aggregate `Apply` case transitions status. NON-terminal state; resumable.
- [ ] **R-WORKFLOW-SUSPEND-FACTORY-01** — `WorkflowLifecycleEventFactory.Suspended(aggregate, stepName, reason)` constructs the event with a Running-only precondition guard. Error: `WorkflowExecutionErrors.CannotSuspendUnlessRunning`. Aggregate has no `Suspend` method.
- [ ] **R-WORKFLOW-SUSPEND-RESUME-GUARD-01** — `Resumed` guard accepts both `Failed` and `Suspended`. Error constant renamed to `CannotResumeUnlessFailedOrSuspended`; pre-R3.A.3 alias retained for source-compat. Architecture test pins the status-pair check.
- [ ] **R-WORKFLOW-SUSPEND-SCHEMA-REGISTRATION-01** — `WorkflowExecutionSchemaModule` registers the schema + payload mapper alongside the existing 6 lifecycle schemas.
- [ ] **R-WORKFLOW-SUSPEND-REPLAY-DETERMINISM-01** — Suspended is replay-stable non-terminal. No new replay discriminator. Arbitrary suspend/resume chains converging on the same end state are replay-equivalent.

**R3.A.6 Human-Approval Wait-State (§13 closure — reuse-first)**

- [ ] **R-WF-APPROVAL-01** — `WorkflowStepResult.AwaitingApproval(approvalSignal, approvalStepName?)` halts T1M execution and emits `WorkflowExecutionSuspendedEvent` via `WorkflowLifecycleEventFactory.Suspended(Guid, string?, string)` with canonical `human_approval[:signal]` Reason. Direct event-store writes bypassing the factory = FAIL. Probe: AwaitingApproval branch in `T1MWorkflowEngine.ExecuteAsync` → factory call → `OutcomeSuspended` metric. Direct engine-harness integration test is deferred pending `T1MWorkflowHarness` fixture (T1M-RESUME-TEST-COVERAGE-01); indirect coverage via projection + replay-service tests.
- [ ] **R-WF-APPROVAL-02** — Approve/Reject handlers refuse with `CannotApproveUnlessAwaitingApproval` / `CannotRejectUnlessAwaitingApproval` when status ≠ Suspended OR latest Suspended Reason lacks `human_approval` prefix. Probe: `WorkflowApprovalReplayServiceTests.ResumeWithApproval_Throws_When_{Not_Suspended, Failed_Not_Suspended, Suspended_With_Non_Approval_Signal}` + mirrors for `CancelSuspended_Throws_When_*`. All PASS.
- [ ] **R-WF-APPROVAL-03** — Approve re-enters `IWorkflowExecutionReplayService.ResumeWithApprovalAsync` only (no parallel API). Emitted Resumed event's `PreviousFailureReason` starts with `human_approval_granted:` + `:{signal}:{actor}[:{rationale}]`. **Temporary carrier (D4): doc language MUST clarify approval-granted is not failure semantics.** Probe: `ResumeWithApproval_Emits_Resumed_With_Granted_Prefix`, `_Preserves_Signal_From_Suspended_Event`, `_Includes_Authoritative_Actor`, `_Includes_Rationale_Suffix_When_Provided`, `_Handles_Bare_Human_Approval_Prefix_Without_Signal`. All PASS.
- [ ] **R-WF-APPROVAL-04** — Reject calls `CancelSuspendedAsync(Guid, approver, rationale?)`; emitted Cancelled event's Reason starts with `human_approval_rejected:{signal}:{actor}[:{rationale}]`; service asserts Suspended + `human_approval` prefix; StepName inherits from Suspended. Probe: `CancelSuspended_Emits_Cancelled_With_Rejected_Prefix`, `_Inherits_StepName_From_Suspended_Event`, `_Includes_Rationale_Suffix_When_Provided`, `_Preserves_Signal_And_Actor_Segments`. All PASS.
- [ ] **R-WF-APPROVAL-05** — No new approval-specific lifecycle events under `src/domain/**` or `src/shared/contracts/events/**`. Probe: `grep -Pn "WorkflowExecution(AwaitingApproval|Approved|Rejected)Event" src/domain src/shared/contracts/events` returns zero hits.
- [ ] **R-WF-APPROVAL-06** — Approval context channels: carrier prefix/suffix + command envelope + `ApprovalDecisionPayload` registered in `IPayloadTypeRegistry`. No duplicate-context-on-event violations. Probe: `ApprovalDecisionPayload` exists with `(Rationale, ApprovalKey?)` shape and no approver field.
- [ ] **R-WF-APPROVAL-07** — Authoritative approver from `CommandContext.ActorId`; carrier actor segment is non-authoritative observability. Probe: `ApprovalDecisionPayload` has no approver field; dispatcher sources `context.ActorId`; replay-service composes carrier from caller-supplied approver parameter — no trust of event-side actor on re-read.
- [ ] **R-WF-APPROVAL-PROJ-01** — Projection preserves canonical `Status` (`Suspended`, `Cancelled`, `Running`, …); approval semantics surfaced on derived `ApprovalState` / `ApprovalSignal` / `ApprovalDecision`. Probe: `WorkflowApprovalProjectionTests.Canonical_Lifecycle_Status_Never_Overloaded_By_Approval_Semantics`, `Suspended_With_Human_Approval_Sets_AwaitingApproval_And_Parses_Signal`, `Cancelled_With_Human_Approval_Rejected_Sets_Rejected_And_Parses_Decision`, `Resumed_With_Approval_Granted_Sets_Granted_And_Clears_Decision`. All PASS.

### Section 15 — Rules Promoted from new-rules/ (2026-04-19) — Test Coverage + Audit Hygiene

- [ ] **TESTS-UNIT-RUNTIME-DISABLED-01** — `tests/unit/Whycespace.Tests.Unit.csproj` MUST NOT carry a `<Compile Remove="runtime\**\*.cs" />` exclusion (or any blanket exclusion over the `runtime/` test subtree) whose underlying reason has been resolved. Periodic check: enumerate all `<Compile Remove>` directives in `tests/**/*.csproj`; verify each is still load-bearing; remove stale exclusions. Runtime unit test dll MUST contain types in the `Whycespace.Tests.Unit.Runtime` namespace and all pre-existing tests under `tests/unit/runtime/**` MUST be CI-reachable. Severity: S1 (silently removed functional coverage).
- [ ] **AUDIT-PROBE-COMMENT-STRIP-01** — every grep-based probe in `runtime.audit.md` (and peer audit files) that scans source files for forbidden tokens (e.g. `DateTime.UtcNow`, `Guid.NewGuid`, `Random`) MUST either (a) pipe through a comment-stripping filter (`sed 's|//.*$||'` before grep) or (b) reference an architecture test that performs the stripping. Grep probes that match comment-only references produce false positives and MUST be refined before reporting FAIL. Prefer architecture-test references over raw grep where a test already exists. Severity: S3 (probe hygiene — raise to S2 when any new audit probe is written without comment-stripping).

## Output Format

```
AUDIT:           runtime
GUARD:           claude/guards/runtime.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   ~182
SECTIONS:        15
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list>
S1_FAILURES:     <list>
EVIDENCE:        <path>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12 and $1c.
