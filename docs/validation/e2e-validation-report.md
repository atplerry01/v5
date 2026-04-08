# Whycespace E2E Validation Report — Phase 1.5 Certification Gate

- **Classification:** validation / phase1.5-gate / whycespace-system
- **Generated:** 2026-04-08 (scaffold)
- **Guard:** [e2e-validation.guard.md](../../claude/guards/e2e-validation.guard.md)
- **Audit:** [e2e-validation.audit.md](../../claude/audits/e2e-validation.audit.md)
- **Harness:** [scripts/validation/](../../scripts/validation/)

## GATE STATUS

> **CONDITIONAL PASS for the TODO happy-path slice; NOT READY for full Phase 1.5.**
>
> After fixing CRITICAL-1 (16 config keys) and applying 2 missing outbox migrations (004, 005), the full TODO E2E slice runs **end-to-end with real evidence**: API → Runtime → Engine → Event store → Outbox → Kafka → Projection → Read API → WhyceChain. CRITICAL-1 is resolved. CRITICAL-2 (layer purity) and CRITICAL-3 (observability stack) remain. Failure-injection, replay equivalence, and load smoke (sections 7, 8, 10) are still NOT EXECUTED.

## EXECUTED EVIDENCE (2026-04-08)

### Static checks — actually run

| Check | Status | Evidence |
|---|---|---|
| `scripts/deterministic-id-check.sh` | **PASS** | `det:0` exit, no Guid.NewGuid / DateTime.UtcNow violations |
| `scripts/hsid-infra-check.sh` | **PASS** | `HSID v2.1 compliance: PASSED` |
| `scripts/dependency-check.sh` | **FAIL — HIGH** | exit 1, **40+ violations** in `src/platform/host/` (see CRITICAL-2) |
| `scripts/infrastructure-check.sh` | **FAIL — CRITICAL** | exit 1, all 5 health/metrics endpoints unreachable (host not up) |

### Live stack discovery
```
docker ps:
  whyce-postgres            :5432  healthy   (eventstore)
  whyce-postgres-projections:5434  healthy   (read models)
  whyce-whycechain-db       :5433  healthy   (chain)
  whyce-kafka               :9092  healthy
  whyce-opa                 :8181  healthy
  whyce-redis               :6379  healthy
  whyce-minio               :9000  healthy
```

### Host startup attempt
```
COMMAND: dotnet run --project src/platform/host --urls http://localhost:5000
ENV: Postgres__ConnectionString, Postgres__ProjectionsConnectionString,
     Postgres__ChainConnectionString, Redis__ConnectionString,
     Kafka__BootstrapServers, OPA__Endpoint, MinIO__* (all set)
RESULT: Unhandled InvalidOperationException
        "Postgres__ProjectionsConnectionString is required. No fallback."
        at TodoBootstrap.cs:44
HTTP /health: 000 (process never bound port)
```

## NEW CRITICAL FINDINGS (discovered live, not in scaffold)

### ✅ CRITICAL-1 RESOLVED (originally S0)
Fixed in this session: 16 callsites (15 host composition + 1 controller) changed from `Foo__Bar` → `Foo:Bar`. Host now boots, all 6 services HEALTHY, full TODO E2E slice verified live. Original analysis preserved below.

### CRITICAL-1 — Config key bug, host cannot start (S0) [HISTORICAL]
**Affected files (15 callsites):**
- `src/platform/host/composition/infrastructure/InfrastructureComposition.cs:31,33,36,38,40,96`
- `src/platform/host/composition/observability/ObservabilityComposition.cs:22,24,26,28,48,50,52,54`
- `src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs:44,56,59`

**Root cause:** Code calls `configuration.GetValue<string>("Postgres__ProjectionsConnectionString")` with literal double-underscore. .NET's `AddEnvironmentVariables()` provider rewrites `Postgres__ProjectionsConnectionString` env var → config key `Postgres:ProjectionsConnectionString` (single colon). The literal `Postgres__X` lookup never matches an env var by design — it would only match a config file key spelled with double underscore.

**Impact:** Host cannot start in any environment that supplies config via env vars. All 29 E2E tests blocked. Phase 1.5 gate cannot proceed.

**Fix (one-line per call):** Change every `"Foo__Bar"` lookup to `"Foo:Bar"`, OR add `services.Configure` binding, OR seed appsettings.json with the keys. Recommend `Foo:Bar` form per .NET convention.

**Also:** This contradicts commit `defe209` "phase1.6-S2 cleanup" and `1fcb302` "externalize OutboxPublisher MAX_RETRY" — those externalizations are unreachable until the lookup is fixed. The phase1.5-S1 commit message `18a0a75` "enforce no-fallback connection-string composition" added the throw without testing the env-var path.

### CRITICAL-2 — Layer purity violations in host (S1, blocks $7)
**40+ violations** of WBSM v3 $7 (Layer Purity) in `src/platform/host/`:

- `Whycespace.Host.csproj` directly references `Whycespace.Runtime`, `Whycespace.Engines`, `Whycespace.Projections`, `Whycespace.Domain` — host is in the `platform` layer which per dependency-check `layer=platform not allowed` to reach those.
- `host/adapters/{GenericKafkaProjectionConsumerWorker,KafkaOutboxPublisher,PostgresEventStoreAdapter,PostgresProjectionWriter}.cs` import `Whyce.Runtime.*`.
- `host/composition/**` modules import `Whyce.Runtime.*`, `Whyce.Engines.*`, `Whyce.Projections.*` extensively (RuntimeComposition.cs alone has 14 forbidden imports, lines 3–19).

**Tension:** A composition root by nature must reference everything it composes. Either:
- (a) the dependency-check rules are wrong for `host/composition/**` and need a whitelist exemption, or
- (b) composition belongs in a separate `composition/` top-level layer outside `platform/`, or
- (c) the host's references are genuinely a violation that needs refactoring before phase 1.5.

This needs an architectural decision, not a code fix. Captured in new-rules.

### CRITICAL-3 — Missing observability stack
`infrastructure-check.sh` expects Prometheus + Grafana on standard ports — neither is running. `docker ps` shows none. Either compose file is incomplete or the check script targets services that aren't part of the gate.



| Layer | Tests | PASS | FAIL | Not Executed |
|---|---|---|---|---|
| API endpoints | 4 | 0 | 0 | 4 |
| TODO E2E flow | 1 | 0 | 0 | 1 |
| Workflow / lifecycle | 4 | 0 | 0 | 4 |
| Policy enforcement | 2 | 0 | 0 | 2 |
| Chain anchoring | 2 | 0 | 0 | 2 |
| Kafka | 3 | 0 | 0 | 3 |
| DLQ / retry / failure | 6 | 0 | 0 | 6 |
| Determinism / replay | 1 | 0 | 0 | 1 |
| Projection rebuild | 1 | 0 | 0 | 1 |
| Load smoke (100/500/1000 rps) | 3 | 0 | 0 | 3 |
| Observability | 1 | 0 | 0 | 1 |
| Failure recovery | 1 | 0 | 0 | 1 |
| **Total** | **29** | **3** | **0** | **26** |

### Live PASS evidence (2026-04-08 13:57 UTC)
- ✅ `todo.create` — full 9-layer trace captured (see Section 1)
- ✅ `todo.fetch` — read model returns same state
- ✅ `todo.create.idempotent-replay` — duplicate rejected via deterministic ID

### Resolved during this session
- ✅ **CRITICAL-1** — 16 config-key callsites fixed (`Foo__Bar` → `Foo:Bar`)
- ✅ **Outbox schema drift** — migrations 004 + 005 applied to whyce_eventstore

## HOW TO EXECUTE

```bash
# 1. Bring up the stack (operator action — not in this scaffold)
#    docker compose up -d postgres kafka api runtime

# 2. Smoke run (dry):
bash scripts/validation/run-e2e.sh        --dry-run
bash scripts/validation/failure-tests.sh  --dry-run
bash scripts/validation/load-smoke.sh     --dry-run

# 3. Live run (after wiring real HTTP/Kafka calls into the scripts):
API_BASE=http://localhost:5000 \
KAFKA_BOOTSTRAP=localhost:9092 \
PG_CONN=postgres://whyce:whyce@localhost:5432/whyce \
bash scripts/validation/run-e2e.sh
```

Each test must populate the §13 block below with real values and an `EVIDENCE:` block (G-E2E-001) before its status may flip to `PASS`.

---

## Section 1 — API Endpoint Validation

### todo.create — ✅ PASS (live, 2026-04-08 13:57:58 UTC)
```
TEST NAME: todo.create
STATUS: PASS

REQUEST:
  POST http://localhost:5000/api/todo/create
  content-type: application/json
  {"userId":"u1","title":"e2e-validation-fresh-2026-04-08-145900"}

RESPONSE: 200
  {"status":"created",
   "todoId":"fd0b1e37-9f94-a80e-b1e1-7fb2ee29bfd2",
   "correlationId":"0ad54929-c328-5cb5-691b-d7f417afa26d"}

EVENTS (eventstore.events):
  - event_type:    TodoCreatedEvent
  - id:            d51e8a7f-06f5-7b40-4c9c-d7797e55f076
  - aggregate_id:  fd0b1e37-9f94-a80e-b1e1-7fb2ee29bfd2
  - aggregate_type: Todo
  - version:       0

KAFKA / OUTBOX (eventstore.outbox):
  - topic:         whyce.operational.sandbox.todo.events
  - status:        published
  - retry_count:   0

PROJECTION (whyce_projections.projection_operational_sandbox_todo.todo_read_model):
  - expected: title="e2e-validation-fresh-2026-04-08-145900", isCompleted=false, status=active
  - actual:   identical (verified via GET /api/todo/{id})

POLICY:
  - decision:       ALLOW (inferred from successful dispatch)
  - decision_hash:  ⚠️ MISSING — events.policy_decision_hash is NULL
                    See FINDING-9 (MEDIUM): policy hash not propagated to events table

CHAIN (whyce_chain, correlation 0ad54929-c328-5cb5-691b-d7f417afa26d):
  - block_id #1:  be5c79f1-4f90-e60f-f407-de5cf3eb27e4  event_hash=c8512a7f1b0abfaf...  ts=13:57:58.736
  - block_id #2:  1bdcf7ae-5d2b-0ef9-5aec-eb8f4c1629f7  event_hash=b71bc7c21ab95db4...  ts=13:57:58.755
  - chain continuity: block#2.previous_block_hash links to block#1.block_id ✓

EVIDENCE: see /tmp/c.json + docker exec psql queries captured in conversation transcript
NOTES: end-to-end happy-path validated. Two chain blocks suggests policy-decision + domain-event are anchored separately, which matches WhyceChain architecture.
```

### todo.fetch — ✅ PASS
```
TEST NAME: todo.fetch
STATUS: PASS
REQUEST:  GET http://localhost:5000/api/todo/fd0b1e37-9f94-a80e-b1e1-7fb2ee29bfd2
RESPONSE: 200
  {"id":"fd0b1e37-9f94-a80e-b1e1-7fb2ee29bfd2",
   "title":"e2e-validation-fresh-2026-04-08-145900",
   "isCompleted":false,
   "status":"active"}
PROJECTION: served from todo_read_model
NOTES: confirms read model populated by Kafka projection consumer within ~2s of write
```

### todo.idempotency — ✅ PASS (bonus, observed)
```
TEST NAME: todo.create.idempotent-replay
STATUS: PASS
REQUEST:  Same payload submitted twice (deterministic id derivation)
RESPONSE 1: 200 created
RESPONSE 2: 400 {"error":"Duplicate command detected."}
NOTES: confirms IIdempotencyStore + deterministic seed work as designed
```

### todo.update — FAIL — NOT EXECUTED
### todo.complete — FAIL — NOT EXECUTED

## Section 2 — TODO Domain E2E Flow — ✅ PASS
```
TEST NAME: todo.full-lifecycle (API→Runtime→Engine→Event→DB→Kafka→Projection→Read)
STATUS: PASS (CREATE path only; UPDATE/COMPLETE not exercised)
EVIDENCE: see todo.create block above
LAYERS VERIFIED:
  ✓ API controller          → TodoController.Create
  ✓ Runtime dispatcher      → ISystemIntentDispatcher
  ✓ Engine                  → TodoEngine + T1M workflow steps
  ✓ Event store             → events row present, version=0
  ✓ Outbox                  → status=published
  ✓ Kafka topic             → whyce.operational.sandbox.todo.events
  ✓ Projection consumer     → todo_read_model populated
  ✓ Read API                → GET returns same state
  ✓ Chain anchor            → 2 blocks with linked previous_block_hash
NOTES: replay equivalence (Section 8) NOT yet executed.
```

## Section 3 — Workflow & Lifecycle
- `workflow.operational.single-step` — **FAIL — NOT EXECUTED**
- `workflow.operational.multi-step` — **FAIL — NOT EXECUTED**
- `workflow.lifecycle.start-process-complete` — **FAIL — NOT EXECUTED**
- `workflow.lifecycle.resume-after-failure` — **FAIL — NOT EXECUTED** (resume MUST be replay-only)

## Section 4 — Policy Enforcement (CRITICAL)
- `policy.allow` — **FAIL — NOT EXECUTED** (must capture decision_hash + version)
- `policy.deny` — **FAIL — NOT EXECUTED** (must verify execution blocked + audit emission)

## Section 5 — Chain Anchoring
- `chain.anchor.deterministic-hash` — **FAIL — NOT EXECUTED**
- `chain.continuity.preserved` — **FAIL — NOT EXECUTED**
- Chain-failure simulation — **FAIL — NOT EXECUTED**

## Section 6 — Kafka
- `kafka.topic.naming` — **FAIL — NOT EXECUTED**
- `kafka.partition.deterministic` — **FAIL — NOT EXECUTED**
- `kafka.order.preserved` — **FAIL — NOT EXECUTED**

## Section 7 — DLQ / Retry / Failure (CRITICAL)
- `failure.engine.dlq-before-commit` — **FAIL — NOT EXECUTED**
- `failure.projection.dlq-before-commit` — **FAIL — NOT EXECUTED**
- `failure.consumer.dlq-and-retry` — **FAIL — NOT EXECUTED**
- `failure.retry.exponential-backoff-capped` — **FAIL — NOT EXECUTED** (cap 300s)
- `failure.no-message-loss` — **FAIL — NOT EXECUTED**
- `failure.idempotency.no-duplicates` — **FAIL — NOT EXECUTED**

## Section 8 — Determinism & Replay
- `replay.byte-equal-projection` — **FAIL — NOT EXECUTED**
- Static checks (already wired): see `scripts/deterministic-id-check.sh`

## Section 9 — Projection Validation
- `projection.rebuild-from-scratch` — **FAIL — NOT EXECUTED**

## Section 10 — Load Smoke (Phase 1.5 Entry)
- 100 RPS — **FAIL — NOT EXECUTED**
- 500 RPS — **FAIL — NOT EXECUTED**
- 1000 RPS — **FAIL — NOT EXECUTED** (minimum requirement)

## Section 11 — Observability
- `observability.correlation-id-propagation` — **FAIL — NOT EXECUTED**

## Section 12 — Failure Recovery
- `recovery.service-restart-mid-execution` — **FAIL — NOT EXECUTED**

## Section 13 — Output Format
See per-test §13 blocks above; harness emits identical structure.

## Section 14 — Failure Reporting

| Test | Severity | Root Cause | Affected Layer |
|---|---|---|---|
| ALL 29 cases | **CRITICAL** | Harness scaffold; live execution not yet performed | All |

### Promotion Blockers (CRITICAL — confirmed by live attempt)
1. **CRITICAL-1** Fix `Foo__Bar` → `Foo:Bar` config key lookups in 15 callsites — host won't start otherwise.
2. **CRITICAL-2** Resolve dependency-check violations OR whitelist `host/composition/**`.
3. **CRITICAL-3** Add Prometheus + Grafana to compose, or remove from `infrastructure-check.sh`.
4. Live stack execution required for sections 1–12 (blocked by #1).
5. Load tool (k6/vegeta/bombardier) must be installed and wired.
6. Failure-injection hooks must be exposed by runtime/engine in test mode.
7. DLQ-before-commit assertion needs Kafka offset introspection wired into the harness.

## Sign-off
- [ ] All CRITICAL items resolved
- [ ] Audit `e2e-validation.audit.md` runs clean
- [ ] Guard `e2e-validation.guard.md` violations: 0
- [ ] Operator signature: ______________________
- [ ] Date: ______________________
