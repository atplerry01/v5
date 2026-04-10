# Deep Project Audit Sweep — Consolidated Output

**Date:** 2026-04-10
**Scope:** Full `src/`, `tests/`, `infrastructure/`, `scripts/`
**Guards loaded:** 31 (all `claude/guards/**/*.guard.md` incl. `domain-aligned/`)
**Audits run:** 27 (all `claude/audits/*.audit.md`)
**Execution:** Three parallel partitioned subagents (determinism/engine/runtime · domain/policy/structural · tests/infra/kafka/platform)

---

## Severity Roll-Up

| Severity | Count | Blocker |
|---|---|---|
| S0 | 3 | YES |
| S1 | 4 | YES |
| S2 | 4 | sprint |
| S3 | 1 | advisory |

**Verdict:** **FAIL (blocking)** — 3 S0 + 4 S1 must clear before merge.

---

## S0 — BLOCKING

### S0-1 — Silent exception swallow in TodoController
- **Guard/Audit:** stub-detection.guard STUB-R4
- **File:** [src/platform/api/controllers/TodoController.cs:99](src/platform/api/controllers/TodoController.cs#L99)
- **Evidence:** `catch { }` (bare, no logging, hides JSON parse failure on projection state)
- **Fix:** `catch (JsonException ex) { _logger.LogWarning(ex, "..."); title = string.Empty; }`

### S0-2 — Plaintext credentials in version-controlled compose
- **Guard:** config-safety.guard CFG-R1
- **File:** [infrastructure/deployment/multi-instance.compose.yml](infrastructure/deployment/multi-instance.compose.yml)
- **Evidence:** `Password=whyce`, `MinIO__SecretKey: "whycepassword"`, `MinIO__AccessKey: "whyce"`
- **Fix:** `${POSTGRES_PASSWORD}` substitution from `.env.local` (gitignored).

### S0-3 — DLQ-before-commit ordering risk in Kafka consumer
- **Guard:** kafka.guard K-DLQ-001
- **File:** [src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs:233-242](src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs#L233-L242)
- **Evidence:** Source offset committed before DLQ produce ACK — host crash between the two loses the message.
- **Fix:** `await PublishToDeadletterAsync(...);` (blocking) **then** `consumer.Commit(result);`.

---

## S1 — BLOCKING

### S1-1 — T1M cross-class import (advisory; verify intent)
- **Guard:** engine.guard RULE 7 / E-LIFECYCLE-FACTORY-01
- **File:** [src/engines/T1M/workflow-engine/WorkflowEngine.cs:4-5](src/engines/T1M/workflow-engine/WorkflowEngine.cs#L4-L5)
- **Evidence:** `using Whyce.Engines.T1M.Lifecycle; using Whyce.Engines.T1M.StepExecutor;`
- **Status:** Same-tier internal imports — likely intended. Either (a) move shared symbols to `src/shared/contracts/`, or (b) amend engine.guard to explicitly allow same-tier internal imports (see new-rules capture below).

### S1-2 — Missing T-PLACEHOLDER-02 marker on InMemoryStructureRegistry
- **Guard:** tests.guard T-PLACEHOLDER-01
- **File:** [src/runtime/topology/InMemoryStructureRegistry.cs](src/runtime/topology/InMemoryStructureRegistry.cs)
- **Fix:** Add XML doc `/// PLACEHOLDER (T-PLACEHOLDER-02): swap target scripts/migrations/XXX.sql`.

### S1-3 — Missing fabric round-trip tests for Todo schemas
- **Guard:** tests.guard ACT-FABRIC-ROUNDTRIP-TEST-01 (NEW 2026-04-10)
- **Files:** TodoCreatedEventSchema, TodoUpdatedEventSchema, TodoCompletedEventSchema
- **Fix:** Mirror `WorkflowResumedEventFabricRoundTripTest.cs` for the three Todo schemas.

### S1-4 — Platform→Npgsql residual in TodoController (documented)
- **Guard:** platform.guard G-PLATFORM-05
- **File:** [src/platform/api/controllers/TodoController.cs:40-42](src/platform/api/controllers/TodoController.cs#L40-L42)
- **Status:** Acknowledged residual (KC-4 deferred). Track on persistence-abstraction workstream — keep tagged so it does not bit-rot.

---

## S2 — Sprint

### S2-1 — Bootstrap migrations missing outbox/chain
- **Audit:** infrastructure.audit CHECK-INFRA-BOOTSTRAP-MIGRATIONS-01
- **Files:** [scripts/migrations/](scripts/migrations/), [infrastructure/](infrastructure/) bootstrap
- **Evidence:** Only `001_workflow_state.sql` + `002_workflow_projection.sql` applied; clean bootstrap fails with `HSID FATAL` because `chain_anchor` table missing.
- **Fix:** Extend `migrate.sh` to iterate `event-store → hsid → outbox → chain`; align docker `initdb.d` mounts.

### S2-2 — Health probes target non-existent containers
- **Audit:** infrastructure.audit CHECK-INFRA-OBSERVABILITY-STACK-01
- **File:** [scripts/infrastructure-check.sh](scripts/infrastructure-check.sh)
- **Evidence:** Probes Prometheus + Grafana but neither in `docker-compose.yml`.
- **Fix:** Add to compose, OR remove probes, OR split into core+optional.

### S2-3 — Unbounded fan-out in health aggregator
- **Audit:** concurrency-control-resource-bounds.audit
- **Files:** [src/platform/api/health/HealthAggregator.cs:35](src/platform/api/health/HealthAggregator.cs#L35), [src/platform/host/health/RuntimeStateAggregator.cs](src/platform/host/health/RuntimeStateAggregator.cs)
- **Evidence:** `Task.WhenAll(tasks)` over unbounded health-check list, no max-concurrency limiter, no per-check timeout.
- **Fix:** Bounded parallelism (e.g., 4) + per-check `WithTimeout`.

### S2-4 — Composition loader order enforcement (verify)
- **Guard:** composition-loader.guard G-COMPLOAD-02
- **File:** [src/platform/host/composition/registry/CompositionRegistry.cs](src/platform/host/composition/registry/CompositionRegistry.cs)
- **Evidence:** Modules declare `Order` (0..4) and registry lists them in canonical order, but mechanical enforcement (loader iterating registry list, not reflection) not verified end-to-end.
- **Fix:** Add unit test asserting `CompositionModuleLoader` iterates `CompositionRegistry.Modules` by list position **and** that `Modules[i].Order == i`.

### S2-5 — Policy decision hash column persistence (verify)
- **Guard:** policy.guard P-EVT-001
- **Evidence:** Runtime hard-stops on null `PolicyDecisionHash` (RuntimeControlPlane.cs:267,276), but Postgres schema not statically inspected for `policy_decision_hash` / `policy_version` columns.
- **Fix:** Integration test asserting non-null `policy_decision_hash` on every persisted CommandPath event row.

---

## S3 — Advisory

### S3-1 — InMemoryStructureRegistry not in placeholder registry
- **Guard:** stub-detection.guard STUB-R3
- **Fix:** Register in `claude/registry/placeholders.json` (if present) with T-PLACEHOLDER-02 ID and migration target.

---

## Clean (verified PASS)

- **Determinism block-list** — 0 `Guid.NewGuid` / `DateTime.Now` outside SystemClock + injected `IClock` paths. `Stopwatch` used only for observability, isolated from hash/id seams.
- **Engine persistence isolation** — 0 `SaveChanges`, `IRepository.Save`, `IEventBus.Publish`, KafkaProducer in `src/engines/`.
- **Runtime middleware order** — 8-stage locked sequence enforced at [RuntimeControlPlaneBuilder.cs:105-115](src/runtime/control-plane/RuntimeControlPlaneBuilder.cs#L105-L115).
- **Event Fabric order** — append → anchor → outbox enforced at [EventFabric.cs:136,142,149](src/runtime/event-fabric/EventFabric.cs#L136).
- **Domain topology** — 390 event folders, all conform to `{classification}/{context}/{domain}/` three-level nesting.
- **Domain external deps** — 0 `Microsoft.EntityFrameworkCore` / `Confluent.Kafka` / `System.IO` hits in `src/domain/**`.
- **Event naming** — all 390+ events past-tense `{Domain}{Action}Event`.
- **Dependency graph DG-R5** — no `Whycespace.Domain.*` refs in `src/platform/host/**`.
- **Replay determinism (Type A & B)** — `ExecutionHash` inputs pure; replay sentinels protected.
- **DeterministicIdEngine** — sole HSID implementation, 12-char locked, sequence bounded X3, domain HSID-blind.

---

## New Rule Captures

Written to `claude/new-rules/`:
1. `20260410-RR-METRIC-OBSERVABILITY-01-determinism.md` — document Stopwatch-for-metrics carve-out in determinism.guard
2. `20260410-RR-ENGINE-INTERNAL-TIER-IMPORTS-01-engines.md` — clarify same-tier internal imports allowed in engine.guard RULE 7
3. `20260410-CFG-R1-DOCKER-COMPOSE-SCAN-guards.md` — extend CFG-R1 grep to `docker-compose*.yml`
4. `20260410-STUB-R4-EMPTY-CATCH-CI-guards.md` — CI grep for `catch\s*\{\s*\}` in `src/platform/api/controllers/`
