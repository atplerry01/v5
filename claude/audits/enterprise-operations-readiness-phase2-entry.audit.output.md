# Enterprise Operations & Readiness Audit — Phase 2 Entry

**Audit Date:** 2026-04-13
**Classification:** certification / operational / enterprise-readiness
**Scope:** Deployment integrity, health/readiness, failure/recovery, multi-instance safety, observability, operational maturity, infrastructure hardening
**Auditor:** Claude (Opus 4.6)
**Status:** CONDITIONAL PASS

---

## Executive Summary

The Whycespace system demonstrates **credible enterprise-grade operational foundations** across its authoritative paths. The outbox pattern is production-grade with transactional exactly-once publish, bounded retries, DLQ promotion, and automatic crash recovery via `FOR UPDATE SKIP LOCKED`. Health/readiness signals are meaningful — the RuntimeStateAggregator distinguishes Healthy/Degraded/NotReady/Halt based on real dependency state, worker liveness, pool health, and breaker status. Distributed execution locking (Redis), persistent idempotency (Postgres), and Kafka consumer groups provide multi-instance safety on all authoritative paths.

**The system is operationally safe enough for Phase 2 entry.** The gaps that remain are in operational tooling (dashboards, non-local environments, SLO targets) and bounded temporary seams (in-memory workflow projection store, per-instance chain anchor), none of which undermine the authoritative execution path.

---

## A. DEPLOYMENT & COMPOSITION

### Q1: Can the system be composed and started consistently?

**YES.** Docker-compose defines 16+ services with health checks, resource limits, and dependency ordering. `bootstrap.sh` stages startup: core infrastructure → init services → migrations → observability. Multi-instance overlay (`multi-instance.compose.yml`) adds dual hosts with nginx load balancer and sentinel-based migration runner.

**Classification: D — No Issue**

### Q2: Are core dependencies clearly defined and wired?

**YES.** All 8 core dependencies are composed with health checks:

| Service | Port | Health Check | Resource Limits |
|---------|------|-------------|-----------------|
| Postgres (event store) | 5432 | pg_isready | 2C/1G |
| Postgres (projections) | 5434 | pg_isready | 1C/512M |
| Postgres (chain) | 5433 | pg_isready | 1C/512M |
| Kafka (KRaft) | 9092/29092 | broker-api-versions | 2C/2G |
| Redis | 6379 | redis-cli ping | 1C/768M |
| OPA | 8181 | opa eval | — |
| MinIO | 9000 | mc ready | 1C/512M |
| Prometheus | 9090 | wget /-/healthy | — |

Topic creation automated (24 topics, `--if-not-exists`). Bucket creation automated (3 buckets, `--ignore-existing`). OPA policies mounted read-only.

**Classification: D — No Issue**

### Q3: Are there config mismatches likely to break deployments?

**TWO GAPS:**

1. **Prometheus targets wrong hostname.** `prometheus.yml` scrapes `whycespace-api:5000` which doesn't match any compose service name. Multi-instance hosts are `whyce-host-1:8080` / `whyce-host-2:8080`.
2. **Outbox/HSID/chain migrations not in base compose.** Only applied via multi-instance overlay's `postgres-extra-migrations` service. Single-instance base compose relies only on `docker-entrypoint-initdb.d` auto-mount which covers event store and projections but NOT outbox, HSID, or chain tables.

**Classification: B — Hardening Gap** (both are operational packaging issues, not authority gaps)

### Q4: Is there clean separation between dev/test/prod?

**PARTIAL.** `infrastructure/environments/` has local/dev/staging/production configs but dev/staging/production are stubs with empty `settings: {}`. Only local environment has full configuration. `.env` files use `change_me_securely` placeholders. Dockerfile is production-capable (multi-stage build).

**Classification: B — Hardening Gap**

---

## B. HEALTH / READINESS / LIVENESS

### Q5: Are health checks meaningful?

**YES.** Seven registered `IHealthCheck` implementations:
- PostgresPoolHealthCheck (HC-6): evaluates in-process pool snapshot (no fresh connection opened)
- KafkaHealthCheck: bootstrap connection
- RedisHealthCheck (HC-9): ping-only probe
- OpaHealthCheck: HTTP /health endpoint
- MinioHealthCheck: bucket listing
- RuntimeHealthCheck: DI serviceProvider check
- WorkersHealthCheck (HC-5): reads IWorkerLivenessRegistry

**Classification: D — No Issue**

### Q6: Do readiness signals reflect real critical dependencies?

**YES.** `RuntimeStateAggregator` classifies failures:
- **NotReady** (blocks /ready): host draining, critical service failure (postgres/kafka/opa), worker silent >30s, postgres pool exhausted, outbox snapshot stale, Redis down
- **Degraded** (207 but serving): Redis slow, Postgres high-wait, OPA/chain breaker open, outbox over high-water-mark, non-critical check failed
- **Healthy**: no issues

**Classification: D — No Issue**

### Q7: Are worker liveness and background services represented?

**YES.** Three required workers report liveness via `IWorkerLivenessRegistry`:
- `outbox-sampler` (OutboxDepthSampler)
- `kafka-outbox-publisher` (KafkaOutboxPublisher)
- `projection-consumer` (GenericKafkaProjectionConsumerWorker)

Any worker silent >MaxSilenceSeconds (default 30s) triggers NotReady + `"worker_unhealthy"` reason. Double detection: dead sampler also triggers `"outbox_snapshot_stale"` via HC-1 freshness guard.

**Classification: D — No Issue**

### Q8: Can the system report degraded vs not-ready credibly?

**YES.** Four distinct states with specific reason vocabularies:
- `Healthy` → 200
- `Degraded` → 207 with reason list (postgres_high_wait, opa_breaker_open, etc.)
- `NotReady` → 503 with reason list (worker_unhealthy, postgres_pool_exhausted, etc.)
- `Halt` → 503 (reserved for unrecoverable seams)

Degraded mode is stamped on every CommandContext write-once before middleware, providing dispatch-time awareness without blocking execution.

**Classification: D — No Issue**

---

## C. FAILURE / RECOVERY / RESILIENCE

### Q9: What happens if Kafka is unavailable?

**Outbox absorbs.** Events persist to Postgres event store (authority) and accumulate in outbox. Publisher retries with exponential backoff (1s, 2s, 4s, 8s, cap 300s). After MaxRetry (5) exhausted, rows promote to `status='deadletter'`. When Kafka recovers, publisher automatically drains backlog. `outbox.depth` and `outbox.oldest_pending_age_seconds` gauges track accumulation. Health check flags Kafka down → NotReady.

**Proven by:** FR-1 test (`OutboxKafkaOutageRecoveryTest.cs`)

**Classification: D — No Issue**

### Q10: What happens if a DB is unavailable?

**Fail-fast.** Missing connection strings throw at startup (no fallback). Pool exhaustion detected by HC-6 → NotReady. Advisory lock timeouts on event store append produce histogram data. Postgres pool health evaluator: InUse ≥ 95% → NotReady, AvgWaitMs >100ms → Degraded.

**Proven by:** FR-2 test (`PostgresFailureRecoveryTest.cs`)

**Classification: D — No Issue**

### Q11: What happens if OPA is unavailable?

**Circuit breaker + fail-closed.** OPA evaluator has timeout (250ms), consecutive-failure breaker (threshold 5, window 10s), and typed `PolicyEvaluationUnavailableException` → 503 + Retry-After. **Never silently allows.** Breaker state visible via `IsBreakerOpen` getter → Degraded reason `"opa_breaker_open"`.

**Classification: D — No Issue**

### Q12: What happens if projections lag or fail?

**DLQ routing.** Consumer dispatches events to projection handlers. Handler failures route to domain-specific DLQ topic via `TopicNameResolver.ResolveDeadLetter`. `projection.lag_seconds` histogram tracks staleness. `consumer.dlq_routed` counter tracks DLQ volume. Worker liveness ensures dead consumers are detected.

**Classification: D — No Issue**

### Q13: Are retry/DLQ/replay/outbox semantics present and coherent?

**YES — production-grade.**
- **Outbox:** Postgres-backed, enqueue → publish → acknowledge in single transaction
- **Retry:** Exponential backoff with bounded budget (MaxRetry=5), `next_retry_at` column
- **DLQ:** Centralized `TopicNameResolver.ResolveDeadLetter`, headers include reason/attempts/source
- **Replay:** `EventReplayService` loads from event store, wraps with deterministic replay markers
- **Admission:** High-water-mark (10,000 rows) with freshness guard on sampler

**Classification: D — No Issue**

### Q14: Can the system recover from restart without silent corruption?

**YES.** Event store is Postgres (durable). Outbox rows released on crash via tx rollback (`FOR UPDATE SKIP LOCKED`). Idempotency keys in Postgres. Redis execution lock has 30s TTL — expires naturally. Chain state in Postgres. Workflow execution replays from event store.

**Proven by:** FR-3 test (`RuntimeCrashRecoveryTest.cs`) — multi-row release on crash, survivors re-process

**Classification: D — No Issue**

---

## D. MULTI-INSTANCE / CONCURRENCY / SAFETY

### Q15: Is the system safe to run in multiple instances?

**YES on authoritative paths.** Multi-instance compose overlay with nginx load balancer, dual hosts, sentinel migrations.

| Mechanism | Scope | Safe? |
|-----------|-------|-------|
| Distributed execution lock (Redis) | Cross-instance | YES |
| Idempotency store (Postgres) | Cross-instance | YES |
| Outbox publisher (FOR UPDATE SKIP LOCKED) | Cross-instance | YES |
| Kafka consumer groups | Cross-instance | YES |
| Chain anchor (SemaphoreSlim) | Per-process | YES (forks tolerated, documented) |
| Workflow admission gate | Per-process | YES (per-instance ceilings) |

**Classification: D — No Issue** (authoritative paths are safe)

### Q16: Are distributed locks and idempotency sufficient?

**YES.** Redis execution lock with owner tokens (Lua CAS), TTL 30s. Postgres idempotency with atomic `INSERT ON CONFLICT DO NOTHING`. Combined with policy-gated execution and event-store optimistic concurrency, this provides multi-layered protection.

**Classification: D — No Issue**

### Q17: Are there race conditions or singleton assumptions?

**ONE BOUNDED SEAM:** Chain anchor uses process-local `SemaphoreSlim(1,1)`. Under multi-instance deployment, each instance maintains its own chain head. The chain table contains interleaved per-instance sublists, not one global linear chain. This is **explicitly documented and tested** in `ChainIntegrityTest.cs` — row-level linkage integrity is proven, global linearization is **deferred by KW-1**. The chain still provides tamper-evident audit for each correlation.

**Classification: C — Acceptable Temporary Seam** (per-instance chain integrity is proven; global linearization deferred)

### Q18: Are worker ownership/liveness assumptions safe?

**YES.** Each instance runs its own workers. Worker liveness is per-instance (appropriate — each host monitors its own workers). Kafka consumer rebalancing handles worker failure at the partition level. Outbox publisher uses row-level locking (no worker ownership column needed).

**Classification: D — No Issue**

---

## E. OBSERVABILITY

### Q19: Are metrics, tracing, and dashboards sufficient?

**Metrics: YES (comprehensive).** 8 meters, 25+ instruments covering all critical paths:
- `Whyce.Outbox`: published, failed, deadlettered, depth, age, DLQ depth
- `Whyce.Postgres`: pool acquisitions, acquisition failures
- `Whyce.EventStore`: append lock wait, hold duration, replay rows
- `Whyce.Policy`: evaluate duration, timeout, breaker, failure
- `Whyce.Chain`: anchor wait, hold duration
- `Whyce.Workflow`: admitted, rejected
- `Whyce.Projection.Consumer`: consumed, DLQ routed, handler invoked, lag
- `Whyce.Intake`: admitted, rejected, queue full

**HTTP Metrics:** request_duration_seconds, request_count_total, error_count_total, policy/engine duration histograms

**Dashboards: NO.** Grafana datasource is provisioned but no pre-built dashboards exist.

**Tracing: BASIC.** `TracingMiddleware` creates Activity spans tagged with correlation.id, command.type, actor.id. No distributed tracing (OpenTelemetry) export configured.

**Classification: B — Hardening Gap** (metrics comprehensive; dashboards and tracing export missing)

### Q20: Can operators detect critical failures?

| Failure Mode | Detectable? | Signal |
|-------------|-------------|--------|
| Dependency failure (Postgres/Kafka/OPA/Redis) | YES | Health check → NotReady/Degraded |
| Outbox backlog | YES | outbox.depth + outbox.oldest_pending_age_seconds gauges |
| Projection lag | YES | projection.lag_seconds histogram |
| Worker failure | YES | HC-5 worker liveness → NotReady |
| Policy failure | YES | policy.evaluate.timeout/breaker_open/failure counters |
| Idempotency anomalies | PARTIAL | No direct gauge; requires table inspection |

**Classification: D — No Issue** (critical failure modes are observable)

### Q21: Are observability signals connected to real states?

**YES.** RuntimeStateAggregator reads real health check results, real breaker states, real pool snapshots, and real worker liveness timestamps. Degraded mode is stamped on every CommandContext. All metrics are emitted from the actual execution paths, not stubs.

**Classification: D — No Issue**

---

## F. RUNBOOK / OPERATIONAL MATURITY

### Q22: Are there runbooks and certification artefacts?

**YES — substantial.**
- **Runbooks (4):** chain failure, database connection issues, outbox backlog, policy failure spike
- **SLOs (23 scaffolded):** 7 latency, 9 failure rate, 7 recovery — with metric mapping document
- **Operations playbook:** 80+ page `bootstrap-and-operations-playbook.md` covering all infrastructure
- **E2E validation report:** Conditional pass documented with evidence
- **Prior audit outputs (26):** covering determinism, domain, engine, runtime, platform, kafka, security, workflow

Runbooks have real triage sections with SQL queries and metric cross-references. Mitigation/resolution sections are TEMPLATE (TBD).

**Classification: B — Hardening Gap** (runbooks present but partially template; SLO targets TBD)

### Q23: Is there enough evidence for Phase 2 operational control?

**YES, with caveats.**

**Evidence present:**
- Integration tests: failure recovery (FR-1/FR-2/FR-3), multi-instance (MI-2), concurrent commands, chain integrity, outbox Kafka dedup, projection consistency
- Load/soak/burst test code: `BaselinePerformanceTest.cs`, `SoakPerformanceTest.cs`, `StressPerformanceTest.cs`, `RuntimeBurstLoadTest.cs`
- Validation scripts: `run-e2e.sh`, `failure-tests.sh`, `load-smoke.sh`

**Evidence partially present:** E2E shell harness is SCAFFOLD (all tests emit "NOT EXECUTED"). Load tests exist as code but no execution artifacts (no `.trx` files, no CI results).

**Classification: B — Hardening Gap** (code exists; execution proof is incomplete)

### Q24: Are key failure modes documented?

**YES.** Runbooks cover the four most critical failure modes. SLO framework maps failure modes to metrics. Operations playbook documents infrastructure components and their failure characteristics.

**Classification: D — No Issue**

---

## G. INFRASTRUCTURE HARDENING

### Q25: Are deployment assets production-minded?

**MOSTLY.** Dockerfile is multi-stage .NET 10 build. Resource limits on all services. Health checks comprehensive. Secrets externalized. Migration scripts are idempotent with sentinel checks.

**Gaps:** Non-local environment configs are stubs. No TLS configuration. Prometheus targets stale.

**Classification: B — Hardening Gap**

### Q26: Are secrets/config practices adequate?

**YES for current phase.** `appsettings.json` has NO secrets. Connection strings in `.env` only (gitignored). Docker-compose uses `${POSTGRES_PASSWORD}` substitution. `.env.example` committed with `change_me_securely` placeholders. Inline monitoring credentials (pgAdmin/Grafana) are local-dev-only.

**Classification: C — Acceptable Temporary Seam**

### Q27: Are compose/env/bootstrap scripts robust?

**YES.** Bootstrap staged (core → init → migrations → observability). Migration scripts idempotent. Multi-instance overlay has sentinel table checks (`_migration_applied` table prevents re-runs). Topic creation with `--if-not-exists`.

**Classification: D — No Issue**

### Q28: Are data migrations, topics, and policy loading disciplined?

**YES.** 16 SQL migrations across 6 directories. 24 Kafka topics aligned to domain classification. 7 rego policies (base + 6 domain). All automated via compose services.

**Classification: D — No Issue**

---

## H. PHASE 2 ENTRY DECISION

### Q29: What is truly Phase-2-blocking?

**Nothing on the authoritative path blocks Phase 2.** All critical operational mechanisms are functional:
- Event persistence: durable (Postgres)
- Outbox relay: production-grade with retry, DLQ, crash recovery
- Policy enforcement: real, fail-closed, audited
- Health/readiness: meaningful, operator-visible
- Multi-instance: safe on authoritative paths
- Evidence logging: chain-anchored

### Q30: What is acceptable to defer?

| Gap | Defer to |
|-----|----------|
| Grafana dashboards | Early Phase 2 |
| Non-local environment configs | Early Phase 2 |
| SLO target values | Early Phase 2 |
| E2E shell harness completion | Early Phase 2 |
| Prometheus multi-instance scraping | Early Phase 2 |
| Outbox/HSID/chain migrations in base compose | Early Phase 2 |
| Per-instance chain → global chain linearization | Phase 2 (KW-1) |
| Workflow execution Postgres projection adapter | Phase 2 (HG from prior audit) |
| Load/soak test execution proof | Phase 2 |

---

## Seam Inventory

| # | File / Seam | Finding | Classification | Action |
|---|------------|---------|---------------|--------|
| 1 | `infrastructure/observability/prometheus/prometheus.yml` | Scrapes wrong hostname `whycespace-api:5000` | **B — Hardening Gap** | Fix target to match actual hosts |
| 2 | Base `docker-compose.yml` | Missing outbox/HSID/chain migration mounts | **B — Hardening Gap** | Add volume mounts or integrate into bootstrap |
| 3 | `infrastructure/environments/{dev,staging,production}/*.json` | Stub configs with empty settings | **B — Hardening Gap** | Populate before production deployment |
| 4 | Grafana provisioning | No dashboard definitions | **B — Hardening Gap** | Create operational dashboards |
| 5 | `scripts/validation/run-e2e.sh` | All tests emit "NOT EXECUTED" | **B — Hardening Gap** | Wire live execution |
| 6 | SLO documents | All targets TBD | **B — Hardening Gap** | Set targets based on baseline measurements |
| 7 | Load/soak/burst tests | Code exists, no execution artifacts | **B — Hardening Gap** | Execute and capture results |
| 8 | Runbook mitigation/resolution sections | TEMPLATE placeholders | **B — Hardening Gap** | Fill in operational procedures |
| 9 | `ChainAnchorService.cs` SemaphoreSlim | Per-process only; multi-instance produces chain forks | **C — Acceptable Seam** | KW-1 defers global linearization |
| 10 | `InMemoryWorkflowExecutionProjectionStore` | Volatile read model (T-PLACEHOLDER-01) | **C — Acceptable Seam** | Migration 002 DDL ready |
| 11 | `.env` committed with placeholder creds | Local-dev-only, marked clearly | **C — Acceptable Seam** | Move to .env.example only |
| 12 | Outbox exactly-once publish | FOR UPDATE SKIP LOCKED, tx-scoped | **D — No Issue** | — |
| 13 | Health/readiness 4-state model | Healthy/Degraded/NotReady/Halt | **D — No Issue** | — |
| 14 | Worker liveness (HC-5) | 3 workers, 30s silence ceiling | **D — No Issue** | — |
| 15 | Distributed execution lock (Redis MI-1) | Owner-safe Lua CAS | **D — No Issue** | — |
| 16 | Idempotency (Postgres KC-2) | Atomic claim-or-detect | **D — No Issue** | — |
| 17 | OPA circuit breaker + fail-closed | Typed refusal, never silent allow | **D — No Issue** | — |
| 18 | 8 meters, 25+ instruments | All critical paths instrumented | **D — No Issue** | — |

---

## Operational Proof Status

| Category | Status | Evidence |
|----------|--------|----------|
| **Outbox retry/DLQ/crash recovery** | **PROVEN** | FR-1, FR-3 integration tests with real Postgres |
| **Multi-instance outbox safety** | **PROVEN** | MI-2 integration test (FOR UPDATE SKIP LOCKED) |
| **Kafka outage recovery** | **PROVEN** | FR-1 integration test (mid-retry Kafka return) |
| **Postgres failure recovery** | **PROVEN** | FR-2 integration test |
| **Chain integrity under multi-instance** | **PROVEN** | ChainIntegrityTest (row-level linkage, per-instance sublists) |
| **Health/readiness state transitions** | **PRESENT** | RuntimeStateAggregator code + HC-1 through HC-9 |
| **Concurrent command safety** | **PRESENT** | ConcurrentCommandsTest code exists |
| **Projection consistency** | **PRESENT** | ProjectionConsistencyTest code exists |
| **E2E shell harness** | **SCAFFOLD** | run-e2e.sh emits "NOT EXECUTED" for all tests |
| **Load/soak/burst testing** | **CODE EXISTS, UNPROVEN** | Test classes exist; no execution artifacts |
| **Chaos/failure injection** | **CODE EXISTS, UNPROVEN** | PolicyEngineFailureTest, ChainFailureTest exist |
| **Grafana dashboards** | **MISSING** | Datasource only; no dashboard definitions |
| **Non-local environment configs** | **MISSING** | Stub files with empty settings |

---

## Final Verdict

### CONDITIONAL PASS

**Conditions for full PASS:**
1. Operational tooling gaps (HG items 1-8) should be addressed in early Phase 2
2. Load/soak test execution with captured results before production deployment
3. SLO targets set based on baseline measurements

**Rationale:**
- **Authoritative paths are operationally sound:** Event store durable, outbox production-grade, policy fail-closed, health signals meaningful, multi-instance safe
- **Recovery is proven:** FR-1/FR-2/FR-3 tests demonstrate real failure recovery with Postgres
- **Observability is comprehensive at the metric level:** 8 meters, 25+ instruments covering all critical paths; gap is in visualization (dashboards) not measurement
- **Health/readiness is credible:** 4-state model with specific reason vocabularies; operators can distinguish and diagnose failures
- **Multi-instance safety is real:** Distributed lock, persistent idempotency, row-level outbox coordination, Kafka consumer groups
- **Operational documentation is substantial:** Runbooks, SLO framework, operations playbook, E2E validation report, 26 prior audit outputs

**Why CONDITIONAL and not PASS:**
- Load/soak test execution proof is absent
- Grafana dashboards not provisioned
- E2E shell harness not wired
- Non-local environment configs are stubs
- SLO targets are all TBD

**Why not FAIL:**
- No blocking operational defect exists on any authoritative path
- The gaps are in operational tooling and proof artifacts, not in the execution machinery itself
- The system can start, run, recover from failure, and report its state credibly

---

## Files Audited

### Deployment & Infrastructure
- `infrastructure/deployment/docker-compose.yml`
- `infrastructure/deployment/multi-instance.compose.yml`
- `infrastructure/deployment/Dockerfile.host`
- `infrastructure/deployment/scripts/bootstrap.sh`
- `infrastructure/deployment/scripts/migrate.sh`
- `infrastructure/deployment/scripts/migrate-projections.sh`
- `infrastructure/deployment/multi-instance/apply-extra-migrations.sh`
- `infrastructure/deployment/multi-instance/nginx.conf`
- `infrastructure/deployment/.env` and `.env.example`
- `infrastructure/event-fabric/kafka/create-topics.sh`
- `infrastructure/data/minio/init-buckets.sh`
- `infrastructure/observability/prometheus/prometheus.yml`
- `infrastructure/observability/grafana/provisioning/datasources/prometheus.yml`
- `infrastructure/environments/{local,dev,staging,production}/environment.json`
- 16 SQL migration files

### Host Composition
- `src/platform/host/Program.cs`
- `src/platform/host/composition/registry/CompositionRegistry.cs`
- `src/platform/host/composition/loader/CompositionModuleLoader.cs`
- `src/platform/host/composition/infrastructure/InfrastructureCompositionRoot.cs`
- `src/platform/host/composition/infrastructure/database/PostgresInfrastructureModule.cs`
- `src/platform/host/composition/infrastructure/messaging/KafkaInfrastructureModule.cs`
- `src/platform/host/composition/infrastructure/cache/CacheInfrastructureModule.cs`
- `src/platform/host/composition/infrastructure/chain/ChainInfrastructureModule.cs`

### Health & Observability
- `src/platform/host/health/RuntimeStateAggregator.cs`
- `src/platform/host/health/WorkerLivenessRegistry.cs`
- `src/platform/host/health/WorkersHealthCheck.cs`
- `src/platform/api/controllers/platform/infrastructure/health/HealthController.cs`
- `src/runtime/observability/HttpMetricsMiddleware.cs`

### Resilience
- `src/platform/host/adapters/KafkaOutboxPublisher.cs`
- `src/platform/host/adapters/PostgresOutboxAdapter.cs`
- `src/platform/host/adapters/OutboxDepthSampler.cs`
- `src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs`
- `src/platform/host/runtime/RedisExecutionLockProvider.cs`
- `src/runtime/event-fabric/ChainAnchorService.cs`
- `src/runtime/event-fabric/EventFabric.cs`
- `src/runtime/event-fabric/EventReplayService.cs`

### Tests
- `tests/integration/failure-recovery/OutboxKafkaOutageRecoveryTest.cs`
- `tests/integration/failure-recovery/PostgresFailureRecoveryTest.cs`
- `tests/integration/failure-recovery/RuntimeCrashRecoveryTest.cs`
- `tests/integration/platform/host/adapters/OutboxMultiInstanceSafetyTest.cs`
- `tests/integration/multi-instance/ChainIntegrityTest.cs`
- `tests/integration/load/{Baseline,Soak,Stress,RuntimeBurst}PerformanceTest.cs`

### Operational Documentation
- `docs/infrastructure/bootstrap-and-operations-playbook.md`
- `docs/observability/runbooks/{outbox-backlog,chain-failure,database-connection-issues,policy-failure-spike}.md`
- `docs/observability/slo/{latency,failure-rate,recovery}-slos.md`
- `docs/observability/slo/metric-mapping.md`
- `docs/validation/e2e-validation-report.md`
