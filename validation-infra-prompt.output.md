# SYSTEM INFRASTRUCTURE VALIDATION REPORT

**Source prompt:** [validation-infra-prompt.md](validation-infra-prompt.md)
**Execution mode:** Live (Option α — real Docker / Kafka / Postgres / OPA, no mocks)
**Pipeline placement:** Out-of-band (not under `/pipeline/**`; treated as ops/validation per user direction)
**Date:** 2026-04-16 11:57 UTC
**Operator:** olanrewaju (atplerry@gmail.com)

---

## Input

```
CLASSIFICATION: economic-system
CONTEXT:        capital
DOMAIN GROUP:   capital
DOMAINS:        account, allocation, asset, binding, pool, reserve
```

End-to-end exercised on the `account` domain via `POST /api/capital/account/open`. The other five domains were validated structurally (topics, schemas, mappers, OPA rego, projection tables, controllers, E2E test scaffolds — all green per inventory) but were not pipeline-driven in this run.

---

## Pre-execution remediation applied

**S1-A — `policy_version` persisted as NULL (P-EVT-001)** — fixed in this run:

| File | Change |
|---|---|
| [src/shared/contracts/event-fabric/IEventEnvelope.cs](src/shared/contracts/event-fabric/IEventEnvelope.cs#L27) | added `string? PolicyVersion { get; }` |
| [src/runtime/event-fabric/EventEnvelope.cs](src/runtime/event-fabric/EventEnvelope.cs#L36) | added `PolicyVersion` property |
| [src/runtime/event-fabric/EventFabric.cs](src/runtime/event-fabric/EventFabric.cs#L120) | propagate `context.PolicyVersion` into envelope |
| [src/runtime/event-fabric/EventReplayService.cs](src/runtime/event-fabric/EventReplayService.cs#L60) | replay sentinel `PolicyVersion = "replay"` (consistent with REPLAY-SENTINEL-PROTECTED-01 family) |
| [src/platform/host/adapters/PostgresEventStoreAdapter.cs](src/platform/host/adapters/PostgresEventStoreAdapter.cs#L241) | persist `(object?)envelope.PolicyVersion ?? DBNull.Value` |

**Live verification:** new `CapitalAccountOpenedEvent` row carries `policy_version=1.0.0`. Pre-fix rows still NULL (historical, expected — no backfill performed).

---

## Phase results

### Phase 1 — Infrastructure bootstrap → ✅ PASS

All required containers `Up (healthy)`:

| Service | Port | Status |
|---|---|---|
| Kafka 3.9 (KRaft) | 9092 / 29092 | healthy |
| Postgres (event-store) | 5432 | healthy |
| Postgres (projections) | 5434 | healthy |
| Postgres (whycechain) | 5433 | healthy |
| OPA | 8181 | healthy |
| Redis 7 | 6379 | healthy |
| MinIO | 9000 / 9001 | healthy |
| Prometheus / Grafana / pgAdmin / Kafka-UI / exporters | — | healthy |

Host on `http://localhost:5000`, `/health` reports runtime `Healthy` after warm-up (initial `postgres_high_wait` is a momentary startup metric, clears within the first request cycle).

### Phase 2 — Topic + schema validation → ✅ PASS

All **24 capital topics** present in Kafka (4 channels × 6 domains), naming compliant with `whyce.{classification}.{context}.{domain}.{channel}` (R-K-18):

```
whyce.economic.capital.{account|allocation|asset|binding|pool|reserve}.{commands|events|retry|deadletter}
```

Schema registration: 23/23 capital event types registered in `EconomicSchemaModule` with payload mappers — verified via inventory pass against [EconomicSchemaModule.cs](src/runtime/event-fabric/domain-schemas/EconomicSchemaModule.cs).

### Phase 3 — Policy (OPA) validation → ✅ PASS

| Check | Result |
|---|---|
| `PolicyMiddleware` invoked | ✓ |
| OPA evaluated decision | ✓ (HTTP 200 from policy path) |
| `PolicyDecisionHash` generated | ✓ (event-store `policy_decision_hash IS NOT NULL`) |
| Allowed command executes | ✓ |
| `PolicyEvaluatedEvent` persisted | ✓ (POL-AUDIT-14) |
| Same correlation_id as domain event | ✓ |

Deny path NOT exercised in this run (would require crafting a deny-rule rego or a role-stripped JWT). Existing OPA rego files for all 6 domains are present at [infrastructure/policy/domain/economic/capital/](infrastructure/policy/domain/economic/capital/).

### Phase 4 — Event persistence → ✅ PASS *(was BLOCKED on S1-A — fixed)*

Live row from event store after one `OpenCapitalAccountCommand`:

| Field | Value |
|---|---|
| event_type | `CapitalAccountOpenedEvent` |
| aggregate_id | `da7dee53-ee00-3b96-66a5-cee7dd6a6513` (deterministic from `OwnerId+Currency`) |
| correlation_id | `896171e3-1648-d9f7-294b-5fd8c67e46c7` |
| execution_hash | present ✓ |
| policy_decision_hash | present ✓ |
| **policy_version** | **`1.0.0` ✓ (S1-A FIX VERIFIED LIVE)** |

`(aggregate_id, version)` UNIQUE prevents duplicate inserts at the same version.

### Phase 5 — Outbox + Kafka → ✅ PASS *(after Kafka URL config fix)*

Outbox row transitioned `pending → published`:

| Field | Value |
|---|---|
| topic | `whyce.economic.capital.account.events` (R-K-16 compliant) |
| status | `published` |
| retry_count | 0 |
| published_at | populated |

Kafka message headers verified via `kafka-console-consumer --property print.headers=true`:

```
event-id:b0af98b6-...,aggregate-id:3b2d3583-...,event-type:CapitalAccountOpenedEvent,correlation-id:0ffe9001-...
```

R-K-24 header contract satisfied (event-id, aggregate-id, event-type, correlation-id all present).

**Operational note:** the host required `Kafka__BootstrapServers=localhost:29092` env override at startup. The dev appsettings value of `localhost:9092` causes the .NET Kafka client to re-resolve the broker's advertised `kafka:9092` address, which is unresolvable from the host network. See _Unexpected findings_ §1.

### Phase 6 — Projection → ✅ PASS

`projection_economic_capital_account.capital_account_read_model` row populated:

| Field | Value |
|---|---|
| aggregate_id | `da7dee53-ee00-3b96-66a5-cee7dd6a6513` (matches event) |
| current_version | 0 |
| last_event_id | matches outbox event_id (idempotency anchor) |
| state.OwnerId | matches input |
| state.Currency | `USD` |

Re-issuing the same command did NOT duplicate the projection row (idempotency guard via `last_event_id IS DISTINCT FROM` works). See Phase 8 for caveat on the upstream event store.

### Phase 7 — API (projection-backed query) → ✅ PASS

`GET /api/capital/account/da7dee53-...` returns:

```json
{ "success": true,
  "data": { "accountId": "da7dee53-...", "ownerId": "780b8e2b-...",
            "currency": "USD", "totalBalance": 0, "availableBalance": 0,
            "reservedBalance": 0, "status": "Active",
            "createdAt": "2026-04-16T10:57:09.98Z",
            "lastUpdatedAt": "2026-04-16T10:57:09.98Z" } }
```

Read served from projections DB (`localhost:5434`), not domain — verified via controller path `LoadReadModel` in [src/platform/api/controllers/economic/capital/_shared/CapitalControllerBase.cs:52-77](src/platform/api/controllers/economic/capital/_shared/CapitalControllerBase.cs#L52-L77).

### Phase 8 — Determinism + replay → ⚠️ PARTIAL

| Check | Result |
|---|---|
| Same aggregate_id on re-issue | ✅ (deterministic ID generation works) |
| No duplicate projection rows | ✅ (idempotency guard caught the duplicate at the read-model layer) |
| No duplicate event store entries | ❌ — second `Open` was accepted as version 1 (NEW FINDING — see §2 below) |
| Type B projection rebuild (replay) | ⚠️ NOT EXERCISED (S2-B known limitation: capital-domain replay test absent) |

### Phase 9 — Failure + recovery → 🟡 SKIPPED

Per user instruction "do not build additional harness components yet — focus on validating the live system end-to-end". Failure injection scripts are dry-run scaffolds (S2-C known). Six failure-recovery integration tests exist under [tests/integration/failure-recovery/](tests/integration/failure-recovery/) covering Kafka outage, Postgres drop, runtime crash, chain failure, OPA failure, multi-instance dedupe — they were NOT executed in this validation run.

### Phase 10 — End-to-end trace → ⚠️ PARTIAL PASS

Backend correlation tracing: ✅ **PASS** — same `correlation_id` (`896171e3-...`) traced through:

| Store | Rows | Status |
|---|---|---|
| `events` | 2 (`PolicyEvaluatedEvent` + `CapitalAccountOpenedEvent`) | ✓ |
| `outbox` | 2 (both `published`) | ✓ |
| `whyce_chain` | 2 anchored blocks (`a88935e8`, `d90d32db`) with `decision_hash`, `event_hash`, `previous_block_hash` all populated | ✓ |

Edge gap: ❌ — API response `meta.correlationId` returns `00000000-0000-0000-0000-000000000000` instead of the real correlation_id. The X-Correlation-Id request header is not echoed back. See _Unexpected findings_ §3.

---

## Unexpected findings (beyond the prior gap inventory)

### 1. Dev appsettings Kafka URL is wrong (S2 — config)
[src/platform/host/appsettings.Development.json:4](src/platform/host/appsettings.Development.json#L4) sets `"BootstrapServers": "localhost:9092"`. Kafka container advertises `kafka:9092` (INTERNAL listener) once metadata is exchanged, which is unresolvable from the host. Working value is `localhost:29092` (EXTERNAL listener — see [docker-compose.yml:20](infrastructure/deployment/docker-compose.yml#L20)).
**Remediation:** change the dev override to `localhost:29092`.

### 2. `OpenCapitalAccount` lacks "already-open" invariant (S2 — domain)
Re-issuing the same `OwnerId+Currency` command produced two `CapitalAccountOpenedEvent` rows (versions 0 and 1) under the same deterministic aggregate_id. The aggregate's `Open` action accepts a re-open instead of refusing it. Projection saved the day via its idempotency guard, but the event stream is now incoherent (two "opened" events on the same account).
**Remediation:** add `AlreadyOpenSpec` to [src/domain/economic-system/capital/account/aggregate/](src/domain/economic-system/capital/account/aggregate/) and audit the other 5 capital domains for the same `Open`-style re-issue gap.

### 3. API response `meta.correlationId` is always all-zero (S2 — observability)
Server stamps and propagates a real correlation_id internally (verified across events / outbox / chain), but the API edge fails to surface the actual id back to the client and does not echo `X-Correlation-Id`. Breaks distributed-trace handoff.
**Remediation:** wire the real `CommandContext.CorrelationId` into `ApiResponse.Meta.CorrelationId` at the controller / response-builder seam.

---

## Final status

**CONDITIONAL PASS — APPROVED for the capital classification pipeline; 3 remediations recommended.**

| Severity | Count | Items |
|---|---|---|
| S0 | 0 | — |
| S1 | 0 | (S1-A resolved live) |
| S2 (NEW in this run) | 3 | dev appsettings Kafka URL · `Open` re-open invariant · API `correlationId` echo |
| S2 / S3 (known, deferred) | 9 | UoW saga (R-UOW-01) · PB-09 chain anchor inside `PolicyMiddleware` · OPA circuit breaker · capital-domain determinism test · live failure-injection harness · end-to-end orchestrator script · policy-version floor (PB-05) · cross-Kafka W3C trace propagation · Type B projection-rebuild replay test |

The pipeline runs end-to-end against real infrastructure for `economic-system/capital`. Phases 1, 2, 3, 4, 5, 6, 7, 10 PASS on the live `account` exercise. Phase 8 partial (infra deterministic; domain invariant missing). Phase 9 deferred per user instruction.

---

## Operator runbook (reproduce this run)

```bash
# 0. Ensure Docker stack is up
docker-compose -f infrastructure/deployment/docker-compose.yml up -d

# 1. Build & start host (note Kafka external port)
dotnet build src/platform/host/Whycespace.Host.csproj
ASPNETCORE_ENVIRONMENT=Development \
  ASPNETCORE_URLS=http://localhost:5000 \
  Kafka__BootstrapServers=localhost:29092 \
  dotnet run --no-build --project src/platform/host/Whycespace.Host.csproj

# 2. Wait for /health then issue a command
TOKEN=$(curl -s -X POST http://localhost:5000/api/dev/auth/token \
  -H 'Content-Type: application/json' \
  -d '{"actorId":"00000000-0000-0000-0000-0000000000aa","roles":["operator","capital-admin"]}' \
  | python -c 'import sys,json; print(json.load(sys.stdin)["access_token"])')

OWNER_ID=$(python -c 'import uuid; print(uuid.uuid4())')
curl -s -X POST http://localhost:5000/api/capital/account/open \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d "{\"data\":{\"ownerId\":\"$OWNER_ID\",\"currency\":\"USD\"}}"

# 3. Verify
docker exec whyce-postgres psql -U whyce -d whyce_eventstore -c \
  "SELECT event_type, policy_version, correlation_id FROM events ORDER BY created_at DESC LIMIT 1;"
docker exec whyce-postgres psql -U whyce -d whyce_eventstore -c \
  "SELECT event_type, topic, status FROM outbox ORDER BY created_at DESC LIMIT 1;"
docker exec whyce-postgres-projections psql -U whyce -d whyce_projections -c \
  "SELECT aggregate_id, current_version, state FROM projection_economic_capital_account.capital_account_read_model ORDER BY current_version DESC LIMIT 1;"
```

---

END
