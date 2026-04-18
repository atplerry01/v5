# Operational Validation — economic-system (all contexts)

**Executed:** 2026-04-18 21:43 UTC
**Environment:** LOCAL docker-compose, multi-instance (`whyce-host-1:18081`, `whyce-host-2:18082`, `whyce-edge:18080`)
**Entry path:** curl with minted HS256 JWT (operator+admin roles)
**Canonical prompt:** [20260418-121953-economic-operational-validation.md](../../claude/project-prompts/20260418-121953-economic-operational-validation.md)

## 1. Environment Readiness Matrix

| Component | State | Notes |
|---|---|---|
| Platform API (host-1/host-2) | ✅ healthy | `/health` = DEGRADED but individual deps all HEALTHY; `postgres_high_wait` non-blocking |
| Postgres eventstore (`whyce-postgres`) | ✅ healthy | tables: events, outbox, hsid_sequences, idempotency_keys |
| Postgres projections (`whyce-postgres-projections`) | ✅ healthy | 40+ projection schemas provisioned by `postgres-extra-migrations` one-shot |
| Kafka (`whyce-kafka`) | ✅ healthy | 167/~160 topics provisioned; **kafka-init still running at test time (race)** |
| OPA (`whyce-opa`) | ✅ healthy | deny-by-default — 4 contexts returned `OPA policy denied` with no permit rule |
| Redis (`whyce-redis`) | ✅ healthy | — |
| MinIO (`whyce-minio`) | ✅ healthy | — |
| Observability (`prometheus`/`grafana`) | ✅ started | not actively probed here |
| Swagger JSON schema | ❌ 404 | `/swagger/v1/swagger.json` and `/openapi/v1.json` both 404; UI shell 200 |
| Dev auth token (`/api/dev/auth/token`) | ❌ 404 | `Auth:DevMode=false` overridden in container env despite `appsettings.Development.json:DevMode=true` |

## 2. Scenario A — Functional Success Path (per context)

One representative POST per context issued with a minted operator+admin JWT. Evidence under [evidence/](evidence/).

| # | Context | Endpoint | HTTP | Classification | Evidence |
|---|---------|----------|------|----------------|----------|
| 1 | capital | `POST /api/capital/account/open` | 200 | **PASS** | [capital-response.json](evidence/capital-response.json) — 7 rows in `projection_economic_capital_account.capital_account_read_model` |
| 2 | compliance | `POST /api/compliance/audit/create` | 500 | **FAIL** | [compliance-response.json](evidence/compliance-response.json) — `InvalidOperationException: Unknown audit type: 'vault_opened'` — unhandled, not returned as 400 |
| 3 | enforcement | `POST /api/enforcement/lock/lock` | 400 | **WARNING** | [enforcement-response.json](evidence/enforcement-response.json) — structured `OPA policy denied` — policy correctly blocking but test credentials lack role/scope for permit rule |
| 4 | exchange | `POST /api/exchange/fx/register` | 500 | **FAIL (S0)** | [exchange-response.json](evidence/exchange-response.json) — `Npgsql.PostgresException: duplicate key value violates unique constraint "outbox_pkey"` — **determinism / idempotency drift** |
| 5 | ledger | `POST /api/economic/ledger/open` | 200 | **PASS** | [ledger-response.json](evidence/ledger-response.json) — 4 rows in `projection_economic_ledger_ledger.ledger_read_model` |
| 6 | reconciliation | `POST /api/economic/reconciliation/discrepancy/detect` | 500 | **FAIL** | [reconciliation-response.json](evidence/reconciliation-response.json) — `ArgumentException: Unknown DiscrepancySource value: 'scenario-a'` — unhandled enum parse, not 400 |
| 7 | revenue | `POST /api/economic/contract/create` | 400 | **WARNING** | [revenue-response.json](evidence/revenue-response.json) — structured `OPA policy denied` |
| 8 | risk | `POST /api/risk/exposure/create` | 400 | **WARNING** | [risk-response.json](evidence/risk-response.json) — structured `OPA policy denied` |
| 9 | routing | `POST /api/routing/execution/start` | 200 | **WARNING** | [routing-response.json](evidence/routing-response.json) — controller returned 200 but outbox row for `whyce.economic.routing.execution.events` went to `failed` status (topic not yet provisioned at publish time — race with `kafka-init`); **silent data loss risk** |
| 10 | subject | `POST /api/economic/subject/register` | 500 | **FAIL** | [subject-response.json](evidence/subject-response.json) — `ArgumentException: Requested value 'user' was not found` (enum parse) — unhandled, not 400 |
| 11 | transaction | `POST /api/charge/calculate` | 400 | **WARNING** | [transaction-response.json](evidence/transaction-response.json) — structured `OPA policy denied` |
| 12 | vault | `POST /api/economic/vault/account/create` | 200 | **PASS** | [vault-response.json](evidence/vault-response.json) — 6 rows in `projection_economic_vault_account.vault_account_read_model` |

**Totals:** 3 PASS · 5 WARNING · 4 FAIL · 0 BLOCKED

**Eventstore delta** ([_delta.txt](evidence/_delta.txt)): `events=13 outbox=12 published=5` — the outbox relay is catching up; 7 new rows still pending at snapshot time.

**Outbox status distribution post-run:** `published=541 · failed=1 · deadletter=8` (baseline `published=530 · failed=0 · deadletter=8`). The `failed=1` is the routing execution row blocked on missing topic; 4+ retries before this report was written.

## 3. Scenarios B–J (per framework)

All 12 contexts are classified **BLOCKED** on Scenarios B through J for this execution. Rationale (applies identically across contexts):

| Scenario | Classification | Rationale |
|----------|----------------|-----------|
| **B. Load** | BLOCKED | No committed load-generation harness (no k6 / NBomber / Locust config under `tests/` or `scripts/`). p50/p95/p99 latency cannot be measured without it. |
| **C. Stress** | BLOCKED | Same: no stress driver. Breaking-point and bottleneck identification require one. |
| **D. Concurrency** | BLOCKED | No committed concurrency test harness. Note: `multi-instance.compose.yml` ships the topology (2 hosts + nginx round-robin on :18080) needed for this scenario — topology ready, driver missing. |
| **E. Failure simulation** | BLOCKED | No chaos-injection wiring (no toxiproxy/pumba/litmus compose overlay). Kafka/Redis/OPA/postgres healthy; cannot selectively fault-inject without an operator. |
| **F. Pause/Resume** | BLOCKED | No workflow pause/resume operator commit. Scheduler queries in host logs (`PostgresExpirableLockQuery`, `PostgresExpirableSanctionQuery`) showed cold-pool timeout under load — related but not a dedicated pause/resume test. |
| **G. Infrastructure failure/recovery** | BLOCKED + evidence from Scenario A | Not exercised as a scenario, but Scenario A incidentally revealed the routing/reconciliation/compliance/subject kafka-init race — topics were not provisioned when host services began publishing. See §4 and drift capture **OPS-VAL-001**. |
| **H. DLQ and retry** | BLOCKED + evidence | Outbox publisher is following retry → deadletter path per R-K-23 (DLQ publish rule). 8 historical deadletter rows carry structured `last_error` including `K-AGGREGATE-ID-HEADER-01`. Routing execution row currently at `retry_count=4, status='failed'` — behavior consistent with spec but not validated end-to-end. |
| **I. Recovery/continuity** | BLOCKED | No committed replay-test harness. |
| **J. Observability** | PARTIAL | Correlation IDs present on every response. Tracing middleware emits `Command exception: {CommandName}` log lines with correlation + command IDs. Grafana/Prometheus up but no per-context dashboard commit. |

## 4. Cross-Context Findings (promoted to new-rules per $1c)

Four drift findings captured under [claude/new-rules/](../../claude/new-rules/) from this execution:

1. **OPS-VAL-001 (S1) — Kafka-init race on host startup.** Host services depend on Kafka `service_healthy`, not on all topics provisioned. `create-topics.sh` creates ~160 topics serially (~5 min); hosts publish/consume well before the script finishes. Symptom: outbox rows land in `status='failed'` with `Local: Unknown topic` and eventually deadletter on otherwise-correct POSTs. Violates R-K-17 (topic declaration coverage) in spirit and R-K-20 (topic-coverage startup guard) in letter.

2. **OPS-VAL-002 (S2) — Unhandled enum/type parse → 500 instead of 400.** Three controllers (`AuditController`, `DiscrepancyController`, `SubjectController`) let `ArgumentException` / `InvalidOperationException` from enum parsing bubble into `ExceptionHandlerMiddleware`, producing a generic 500 ProblemDetails with no domain error code. Should validate and return 400 with a structured `ApiResponse.Fail(code, message, …)` — the pattern used by other controllers. Related to R-PLAT-12 (CommandResult envelope expectations).

3. **OPS-VAL-003 (S0 candidate) — Exchange controller hits `outbox_pkey` unique-constraint violation.** Second `POST /api/exchange/fx/register USD/EUR` call produced `Npgsql.PostgresException: duplicate key value violates unique constraint "outbox_pkey"`. Interpretation pending — either the HSID generator is not uniquely keyed per event (determinism violation of GE-01) or the Register-FX path is double-inserting under retry. If HSID collision: S0 (breaks determinism + replay). If retry-double-insert: S1. Needs code-level follow-up; see drift file for investigation steps.

4. **OPS-VAL-004 (S2) — No OpenAPI schema served.** `/swagger/v1/swagger.json` and `/openapi/v1.json` return 404; only the Swagger UI shell is reachable. Blocks auto-discovery for Scenarios B/C/D drivers and violates the "Entry Path: Swagger" option in the operational-validation prompt.

## 5. Outcome Assessment

Questions answered per §7 of the framework:

| Question | Answer |
|----------|--------|
| Domain behavior correct? | Partial — 3/12 contexts fully correct; 5 blocked by policy deny (by design); 4 thrown 500 (drift) |
| Infrastructure behavior safe? | No data corruption observed on the eventstore; Kafka race allows silent `failed`/`deadletter` rows on otherwise-successful commands |
| Graceful failure? | Partial — failures that route through `ApiResponse.Fail` are graceful; enum-parse exceptions are not |
| Recovered correctly? | Not exercised as a dedicated scenario |
| Data integrity preserved? | Eventstore + outbox consistent; projections lag but on-path |
| Messaging integrity preserved? | Spotty — routing/compliance/reconciliation/subject topics were not yet provisioned at test time |
| Retry / DLQ correct? | Behavior consistent with rules R-K-23 / R-K-25; 8 historical deadletter rows have `last_error` populated per contract |
| Pause/Resume safe? | Not tested |
| Concurrency handled correctly? | Not tested (topology ready, driver missing) |
| Observability sufficient? | Correlation IDs + command IDs on every response/log; per-context dashboards absent |
| Operationally ready? | **No.** See §6. |

## 6. Readiness Verdict

- **Ready-for-wider-traffic contexts:** capital, ledger, vault.
- **Ready-but-policy-gated contexts:** enforcement, revenue, risk, transaction. These need OPA permit rules mapped to `operator`/`admin` roles OR a dedicated `ops-validator` role wired to their policy packages.
- **Not ready (controller-level drift):** compliance, reconciliation, subject. Controllers must validate inputs and return 400, not 500.
- **Not ready (infrastructure-level drift):** routing — relies on topics that are provisioned *after* host start; `failed` → `deadletter` silently on otherwise-successful commands.
- **Not ready (determinism drift):** exchange — `outbox_pkey` violation on second call. Needs investigation before the context can be trusted under any real load.

## 7. Re-run Procedure

To repeat Scenario A after topic provisioning completes and drift is addressed:

```bash
# 1. Wait for kafka-init to finish
docker wait whyce-kafka-init

# 2. Mint a fresh JWT
TOKEN=$(/tmp/mint-jwt.sh "whyce-dev-signing-key-local-infrastructure-validation-only-2026" | tr -d '\n\r')
echo "$TOKEN" > /tmp/token.jwt

# 3. Re-run
docs/operational-validation/economic-system/evidence/run-scenario-a.sh
```

Scenario B/C/D/E/F/H/I require dedicated harnesses to be committed to `tests/operational/` before they can exit BLOCKED.
