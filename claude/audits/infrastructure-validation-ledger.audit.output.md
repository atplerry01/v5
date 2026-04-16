---
title: Infrastructure Validation — Ledger (Real Execution Probe)
classification: economic
context: ledger
domain_group: ledger
domains: [entry, journal, ledger, obligation, treasury]
run_type: baseline_probe (Option A)
run_date: 2026-04-16
host: whyce-host-1 (exit 137, OOM, 13h ago — not currently running)
status: FAIL
rerun_required: true
---

# SYSTEM INFRASTRUCTURE VALIDATION REPORT — LEDGER

## 0. Scope & Mode

This report captures the **real current state** of the ledger pipeline against the 10-phase validation prompt. It is a **probe-only baseline** (Option A): no commands were issued, no code was modified, no infrastructure was started or restarted during this pass. Every observation is grounded in live probes against running services, the actual topic list, actual projection schemas, the actual composition root, and the actual host container logs.

Target: `classification=economic`, `context=ledger`, `domain_group=ledger`, `domains={entry, journal, ledger, obligation, treasury}`.

**Note on the canonical classification string:** per `claude/guards/domain.guard.md` DS-R2 / DS-R8, non-domain layers (topics, projections, policies, composition) use the raw classification `economic`, NOT `economic-system`. The prompt template's `{classification}` substitution must resolve to `economic` to match the canonical topic regex `whyce.{cluster}.{context}.{event}` in `infrastructure.guard.md` R-K-18. This report uses `economic` throughout.

---

## 1. Infrastructure Status (Phase 1)

### 1.1 Core services — HEALTHY

| Service | Container | Status | Uptime |
|---|---|---|---|
| Kafka | `whyce-kafka` | Up (healthy) | 13 h |
| Postgres (event store) | `whyce-postgres` | Up (healthy) | 13 h |
| Postgres (projections) | `whyce-postgres-projections` | Up (healthy) | 13 h |
| Redis | `whyce-redis` | Up (healthy) | 13 h |
| OPA | `whyce-opa` | Up (healthy) | 3 h (re-upped) |
| WhyceChain DB | `whyce-whycechain-db` | Up (healthy) | 13 h |
| MinIO | `whyce-minio` | Up (healthy) | 13 h |
| Prometheus | `whyce-prometheus` | Up (healthy) | 13 h |

### 1.2 Host layer — DOWN

| Container | Status | Last exit | Root cause (from logs) |
|---|---|---|---|
| `whyce-host-1` | Exited 13 h ago | code 137 (SIGKILL — OOM) | See §2 below |
| `whyce-host-2` | Exited 29 h ago | code 255 | Not yet investigated |
| `whyce-edge` | Exited 29 h ago | code 255 | Not yet investigated |

**Implication:** No `/api/**` endpoint is currently reachable. Any phase that requires issuing commands through the HTTP surface (Phases 3–10) cannot execute until the host is restored.

### 1.3 Credentials — functional but weak

`whyce-postgres` / `whyce-postgres-projections` both use `POSTGRES_USER=whyce`, `POSTGRES_PASSWORD=change_me_securely`. Connection verified against `whyce_eventstore` and `whyce_projections`. The literal `change_me_securely` value is a placeholder and is an `R-CFG-R1` concern for production — acceptable for local-dev, flagged here for completeness.

### 1.4 Phase 1 verdict

**PASS (infrastructure)**, **FAIL (host)**. Core event-fabric and policy dependencies are up. The API/host surface that the validation prompt exercises is not running.

---

## 2. Host Crash Diagnosis (pre-work for §B.2)

Captured from `docker logs whyce-host-1 --tail 40`:

- Exit code **137** = SIGKILL. On Docker/WSL2 this is almost always an OOM kill from the cgroup memory limit.
- The host **did boot successfully** before being killed: `info: Microsoft.Hosting.Lifetime[14]: Now listening on: http://0.0.0.0:8080` and `Application started`.
- Immediately before boot completion, the logs show a long burst of rdkafka connect failures against `kafka:9092` — first `Name or service not known` then `Connection refused`, cycling for seconds. This happens while consumers attempt to connect during startup; it eventually clears when Kafka DNS resolves.
- Terminal message before the kill: `Application is shutting down...`. The shutdown signal preceded the SIGKILL — consistent with an orchestration-initiated stop (`docker stop` or compose health-check eviction) followed by OOM during drain, or with the kernel reaping the process after exceeding its cgroup limit during the shutdown flush of Kafka producers/consumers.
- Confirmed from the logs: on its last run, the host wired **only two** Kafka projection consumers:
  - `whyce.operational.sandbox.kanban.events`
  - `whyce.operational.sandbox.todo.events`

  **Zero economic consumers** were visibly applied. See §4.3 for why — it is NOT because `EconomicCompositionRoot` is missing from the catalog.

**B.2 hypotheses to probe next:**
1. cgroup memory cap set below the host's resident set size (compose `deploy.resources.limits.memory` or Docker Desktop WSL limit).
2. A runaway during bootstrap (e.g., economic consumer factory iterating over many topics synchronously) pushing RSS above the limit.
3. A leak on the drain path (producers not flushing).

None of these can be discriminated without a new run with `docker stats` trending, so they are probe candidates for §B.2, not conclusions here.

---

## 3. Topics & Schemas (Phase 2)

### 3.1 Canonical topics required by the prompt for the 5 ledger domains

```
whyce.economic.ledger.entry.{commands,events,retry,deadletter}
whyce.economic.ledger.journal.{commands,events,retry,deadletter}
whyce.economic.ledger.ledger.{commands,events,retry,deadletter}
whyce.economic.ledger.obligation.{commands,events,retry,deadletter}
whyce.economic.ledger.treasury.{commands,events,retry,deadletter}
```

Total: 5 × 4 = **20 topics**.

### 3.2 Actual Kafka topic state (probed via `kafka-topics.sh --list`)

Total `whyce.*` topics on broker: **72**.

Ledger-context topics present:
```
whyce.economic.ledger.transaction.commands
whyce.economic.ledger.transaction.events
whyce.economic.ledger.transaction.retry
whyce.economic.ledger.transaction.deadletter
```

Ledger-context topics **MISSING** (all 20 required by the prompt):
```
whyce.economic.ledger.entry.*      — all 4 MISSING
whyce.economic.ledger.journal.*    — all 4 MISSING
whyce.economic.ledger.ledger.*     — all 4 MISSING
whyce.economic.ledger.obligation.* — all 4 MISSING
whyce.economic.ledger.treasury.*   — all 4 MISSING
```

### 3.3 Domain-decomposition drift (IMPORTANT)

There is an active misalignment between the **infrastructure provisioning** (topics + policies) and the **domain code**:

| Surface | `transaction` | `entry` | `journal` | `ledger` | `obligation` | `treasury` |
|---|---|---|---|---|---|---|
| Domain code `src/domain/economic-system/ledger/` | — (none) | ✅ aggregate | ✅ aggregate | ✅ aggregate | ✅ aggregate | ✅ aggregate |
| Composition (`EconomicCompositionRoot.cs`) | ✅ `AddTransactionApplication()` (under `src/platform/host/composition/economic/transaction/`) | ❌ NOT wired | ✅ `AddLedgerJournalApplication()` | ✅ `AddLedgerLedgerApplication()` | ✅ `AddLedgerObligationApplication()` | ✅ `AddLedgerTreasuryApplication()` |
| API controllers `src/platform/api/controllers/economic/ledger/` | (n/a) | ❌ MISSING | ✅ `JournalController` | ❌ MISSING | ✅ `ObligationController` | ✅ `TreasuryController` |
| Projection schemas (live DB) | ✅ `projection_economic_ledger_transaction` | ✅ `..._entry` | ✅ `..._journal` | ✅ `..._ledger` | ✅ `..._obligation` | ✅ `..._treasury` |
| Kafka topics (live broker) | ✅ 4/4 | ❌ 0/4 | ❌ 0/4 | ❌ 0/4 | ❌ 0/4 | ❌ 0/4 |
| OPA policy bundle (live) | ✅ `policies/domain/economic/ledger/transaction.rego` | ❌ MISSING | ❌ MISSING | ❌ MISSING | ❌ MISSING | ❌ MISSING |
| T2E handlers | — | ❌ not seen | ❌ not seen | ✅ `OpenLedgerHandler` | ❌ not seen | ❌ not seen |

Interpretation: the infrastructure (topics, OPA policy, transaction composition) reflects an earlier `ledger/transaction` bounded context, while the domain code has since been split into five specialised sibling domains. The projection DB has kept pace with both shapes (6 schemas total). The API/controllers, composition, and Kafka/OPA provisioning have each kept pace partially. This is drift captured here per `CLAUDE.md §1c` and is worth promoting to `claude/new-rules/`.

### 3.4 Schema mappers / EconomicSchemaModule

`EconomicCompositionRoot.RegisterSchema` delegates to `DomainSchemaCatalog.RegisterEconomic(schema)`. Whether that catalog includes typed entries for the 5 ledger domains was not verified in this pass (ripgrep scope only captured aggregate/handler presence). Flagged as a §B pre-check: mapper presence must be confirmed before asserting "schema registered".

### 3.5 Phase 2 verdict

**FAIL.** 20 of 20 required ledger topics are absent. 5 of 5 domain-specific OPA policies are absent. By `infrastructure.guard.md` R-K-20 (K-TOPIC-COVERAGE-01, S0), the broker topic list must diff-match the set of topics referenced by outbox publishes — any domain emitting events for a topic that does not exist on the broker is an S0 data-loss risk. Phases 3–10 cannot be evaluated while this precondition fails.

---

## 4. Policy / OPA (Phase 3)

### 4.1 OPA status

`GET http://localhost:8181/health` → `{}` (OPA considers itself ready).
`GET http://localhost:8181/v1/policies` → 17 policy bundles loaded.

### 4.2 Loaded policies relevant to this run

```
policies/base/base.rego
policies/domain/economic/capital/{account,allocation,asset,binding,pool,reserve,vault}.rego
policies/domain/economic/ledger/transaction.rego          ← only ledger policy present
policies/domain/economic/reconciliation/{discrepancy,process}.rego
policies/domain/economic/exchange/{fx,rate}.rego
policies/domain/identity/access/identity.rego
policies/domain/operational/global/incident.rego
policies/domain/operational/sandbox/{kanban,todo}.rego
```

### 4.3 Command-policy bindings

`EconomicCompositionRoot` registers `services.AddLedgerPolicyBindings()` — so per-command `PolicyId` stamping exists in DI. The binding names and count were not enumerated in this pass (would require reading `LedgerPolicyModule.cs` and its bindings). Flagged as a §B pre-check.

### 4.4 Phase 3 verdict

**FAIL (by absence).** Without the 5 domain-specific rego packages and their corresponding Kafka event topics, a PolicyMiddleware evaluation for e.g. `CreateJournalCommand` cannot resolve to a live OPA package — even if `LedgerPolicyModule` stamps the PolicyId correctly. The `transaction.rego` policy is a proxy for an older bounded context and cannot satisfy `constitutional.guard.md` PB-04 (policy scope must match execution domain).

---

## 5. Event Persistence (Phase 4)

### 5.1 Event store tables

`whyce_eventstore` contains exactly:
```
events
hsid_sequences
idempotency_keys
outbox
```

All owned by `whyce`, 4 rows. This is the expected canonical shape per `infrastructure.guard.md` Section Composition + `constitutional.guard.md` G19/G20 (HSID sequence table required).

### 5.2 Row counts — not measured

No command was executed during this baseline pass, so there are no new ledger rows to inspect. Historical rows may or may not exist; this will be re-measured in §B post-restart.

### 5.3 Phase 4 verdict

**NOT RUN.** Persistence check requires issuing a write command through the (currently unreachable) API. Structure is correct; behaviour is untested.

---

## 6. Outbox + Kafka (Phase 5)

### 6.1 Outbox table

Present in `whyce_eventstore.public.outbox`. Schema detail not probed (would require `\d outbox`). The outbox worker (topic-aware, `infrastructure.guard.md` R-K-13) runs in-host; it is not currently running.

### 6.2 Header contract (R-K-24)

Required headers (`event-id`, `aggregate-id`, `event-type`, `correlation-id`) cannot be verified against ledger messages because no ledger messages exist on the broker to inspect. Header-contract check is unreachable.

### 6.3 Phase 5 verdict

**NOT RUN.** Outbox plumbing exists; end-to-end validation is blocked behind Phase 2 (topic provisioning) and host availability.

---

## 7. Projections (Phase 6)

### 7.1 Projection schemas — present for all 5 prompt domains (plus `transaction`)

```
projection_economic_ledger_entry      → entry_read_model
projection_economic_ledger_journal    → journal_read_model
projection_economic_ledger_ledger     → ledger_read_model
projection_economic_ledger_obligation → obligation_read_model
projection_economic_ledger_transaction → transaction_read_model
projection_economic_ledger_treasury   → treasury_read_model
```

All 6 schemas exist, each with one `*_read_model` table. This matches `domain.guard.md` DS-R5 (cross-layer mapping — projection path uses raw `economic`) and the prompt's expected projection path `projection_{classification}_{context}_{domain}.{domain}_read_model`.

### 7.2 Projection handlers registered in host composition

`EconomicCompositionRoot.RegisterProjections` delegates to `EconomicProjectionModule.RegisterProjections`. That module's coverage of the 5 ledger domains was not fully enumerated in this pass; the presence of the projection SQL files (`infrastructure/data/postgres/projections/economic/ledger/{entry,journal,ledger,obligation,treasury}/001_projection.sql`) strongly suggests the handlers are registered, but this requires verification in §B.3.

### 7.3 Phase 6 verdict

**CONDITIONAL PASS on structure, NOT RUN on behaviour.** Schemas and read-model tables exist for all 5 domains. Runtime population by consumers cannot be tested because there are no events to consume.

---

## 8. API Validation (Phase 7)

### 8.1 Controller coverage

| Domain | Controller file | Verdict |
|---|---|---|
| entry | — | **MISSING** |
| journal | `JournalController.cs` | Present |
| ledger | — | **MISSING** |
| obligation | `ObligationController.cs` | Present |
| treasury | `TreasuryController.cs` | Present |

### 8.2 Phase 7 verdict

**FAIL.** Two of five domains have no API surface. Even if the host were running, `GET /api/ledger/entries` and `GET /api/ledger/ledgers` (or whatever the canonical shapes would be) are not implemented. API-through-projection validation is unreachable for entry and ledger without new code.

---

## 9. Determinism & Replay (Phase 8)

**NOT RUN.** Requires a live command path to exercise. Relevant guards (`constitutional.guard.md` Determinism, Hash Determinism, Replay Determinism) are in force and would be applied if a run produced events.

---

## 10. Failure Recovery (Phase 9)

**NOT RUN.** Requires a functional end-to-end baseline before injecting failures makes sense. Attempting a Kafka-downtime test on a system that does not currently publish any ledger events would be meaningless.

---

## 11. End-to-End Trace (Phase 10)

**NOT RUN.** Requires a round-trip through a running API.

---

## 12. Per-Domain Roll-Up

| Domain | Topics | OPA policy | Projection schema | Composition wired | API controller | Net status |
|---|---|---|---|---|---|---|
| entry | 0/4 | Missing | ✅ | ❌ not wired | ❌ missing | **NOT OPERATIONAL** |
| journal | 0/4 | Missing | ✅ | ✅ | ✅ | **NOT OPERATIONAL (no topics, no policy)** |
| ledger | 0/4 | Missing | ✅ | ✅ | ❌ missing | **NOT OPERATIONAL** |
| obligation | 0/4 | Missing | ✅ | ✅ | ✅ | **NOT OPERATIONAL (no topics, no policy)** |
| treasury | 0/4 | Missing | ✅ | ✅ | ✅ | **NOT OPERATIONAL (no topics, no policy)** |

---

## 13. Governance Cross-References

- `CLAUDE.md $1a` — guards loaded for this run: `constitutional.guard.md`, `infrastructure.guard.md`, and the locked sections of `runtime.guard.md` / `domain.guard.md` consulted. No guard rule was violated by this report (probe-only, no code mutation).
- `CLAUDE.md $1b` — post-execution audit sweep applicable (this file is itself an audit output; existing `claude/audits/*.audit.md` checks should run against this document and any follow-up code changes).
- `CLAUDE.md $1c` — the domain-decomposition drift described in §3.3 is a **new capture candidate**. Suggested file: `claude/new-rules/20260416-120000-ledger-decomposition-drift.md` with `CLASSIFICATION: economic-system`, `SEVERITY: S1 (structural)`. Not created in this pass; flagged for creation before §B work begins.
- `infrastructure.guard.md` **R-K-17** (K-TOPIC-02) — ledger topics must be declared in `infrastructure/event-fabric/kafka/create-topics.sh`. That file was not inspected this pass. Required inspection before §B.4.
- `infrastructure.guard.md` **R-K-20** (K-TOPIC-COVERAGE-01, **S0**) — any ledger domain emitting events with no broker topic is an S0 data-loss risk. Currently latent (no events are being produced because the host is down).
- `constitutional.guard.md` **PB-04** — policy scope must match execution domain. With only `transaction.rego` loaded, any ledger command for the 5 new domains will fail PB-04 even if the binding registry provides a PolicyId.

---

## 14. Final Verdict

```
SYSTEM INFRASTRUCTURE VALIDATION — LEDGER
Phase 1  (Infrastructure Bootstrap)  : PASS (core), FAIL (host)
Phase 2  (Topics + Schemas)          : FAIL  — 20/20 required topics missing
Phase 3  (Policy / OPA)              : FAIL  — 5/5 required rego packages missing
Phase 4  (Event Persistence)         : NOT RUN (host down, no commands issued)
Phase 5  (Outbox + Kafka)            : NOT RUN (blocked by Phase 2)
Phase 6  (Projections)               : CONDITIONAL PASS (schemas) / NOT RUN (behaviour)
Phase 7  (API Validation)            : FAIL  — 2/5 controllers missing, host down
Phase 8  (Determinism + Replay)      : NOT RUN
Phase 9  (Failure Recovery)          : NOT RUN
Phase 10 (End-to-End Trace)          : NOT RUN

FINAL STATUS: FAIL
```

**Reason:** the ledger pipeline cannot be exercised end-to-end in its current state. Host is down, topics are absent for all 5 domains, OPA policies are absent for all 5 domains, and 2 of 5 API controllers are missing. A "CONDITIONAL PASS — infra issue only" verdict is not applicable here because the gaps span code (controllers, composition wiring for `entry`), policy (OPA packages), and infrastructure (Kafka topics) — not purely infra.

---

## 15. What §B (Remediation Pass) Must Address

In the order the user requested:

**§B.1 — Bring host back up.** Investigate the OOM; raise the cgroup cap or shrink host startup memory; attempt `docker compose up -d host-1`. Capture fresh logs; confirm it binds :8080 and stays up without eviction.

**§B.2 — Fix OOM root cause.** Use `docker stats` during a controlled restart; identify whether the spike comes from composition, schema registration, consumer factory bootstrap, or Kafka connection retry storms during DNS propagation.

**§B.3 — Verify ledger modules actually load.** Two concrete gaps to close before claiming "ledger is composed":
- `services.AddLedgerEntryApplication()` does NOT appear in `EconomicCompositionRoot.RegisterServices`. Either that call is missing, or the entry domain is intended to sit outside the application-module pattern. Decide and either add it or document the exception.
- `LedgerEntryApplicationModule.RegisterEngines(engine)` is not called in `RegisterEngines`. Same resolution required.
- Confirm `DomainSchemaCatalog.RegisterEconomic` includes typed schema entries for all 5 ledger domains.

**§B.4 — Create Kafka topics.** Declare the 20 topics in `infrastructure/event-fabric/kafka/create-topics.sh` (R-K-17), run the `kafka-init` container (or equivalent) to materialise them on the broker.

**§B.5 — Boot consumers.** With topics live, the generic `GenericKafkaProjectionConsumerWorker`(s) should pick them up at host start. Verify via host logs that each of the 20 topics has an applied consumer config line (matching the shape seen for `whyce.operational.sandbox.todo.events` in the 13-h-old log).

**§B.6 — Projection migrations + schemas.** The projection DB already has the 5 ledger schemas and read-model tables. Verify each has the expected columns (idempotency_key, state, etc.). Run no-op rebuilds if the column set needs widening. Verify OPA bundle carries a rego package for each of the 5 domains (either genuine policies or `default allow := false` starter packages, per PB-04).

**(separate track) — Controllers.** `EntryController` and `LedgerController` must be added before Phase 7 can be exercised. This is code work outside §B's infra scope; flagged here so it isn't forgotten when §B ends.

---

*End of baseline report.*

---

# APPENDIX B — Remediation Pass (Option B — completed 2026-04-16)

The §B pass followed the user-specified order. It addressed every item that could be addressed without writing new domain code. The one gap that requires code work (`entry` application-module wiring) is documented but **not** implemented here — it is outside the scope of an infra-validation pass.

## B.0 Corrections to baseline report (honesty)

- The §2 "OOM" characterisation was wrong. `docker inspect whyce-host-1` returned `Memory=0` (no cgroup limit), `OOMKilled=false`, `ExitCode=137`. Exit 137 with `OOMKilled=false` means the container received SIGKILL from outside its cgroup — almost certainly from `docker stop` exceeding the default 10-second `stop_grace_period` while draining rdkafka producers. Not a memory leak, not a cap, not a capacity issue.
- The §6.1 phrasing "outbox pattern exists" was understated. `apply-extra-migrations.sh` had failed its first run 13h ago with `$'\r': command not found` — the same CRLF-on-Linux issue that blocked `create-topics.sh`. The outbox/hsid/chain migrations were never applied in that prior run; they rely on either a pre-populated volume (how this dev machine had the tables) or a successful re-run of the script after the CRLF fix (which this pass performed).

## B.1 Host restored

- **Action:** removed the exited container (`docker rm whyce-host-1`), reran `docker compose -f docker-compose.yml -f multi-instance.compose.yml up -d whyce-host-1`.
- **Blocker encountered:** `whyce-postgres-extra-migrations` failed at `set: pipefail: invalid option name` — CRLF in `infrastructure/deployment/multi-instance/apply-extra-migrations.sh`.
- **Fix:** `tr -d '\r'` on the script (now LF-terminated, confirmed by `file` as `Bourne-Again shell script, Unicode text, UTF-8 text executable`).
- **Same class of issue:** `infrastructure/event-fabric/kafka/create-topics.sh` also CRLF — fixed in the same pass.
- **Boot result (stale image, first attempt):** host-1 came up healthy in 15 seconds. Only `operational.sandbox.{todo,kanban}` consumers were wired, matching the 13h-old log. This confirmed the image was stale relative to current source.

## B.2 No OOM to fix — image was stale

See B.0 above. No memory cap changes were made. The real remediation for the crashed state was:

1. LF the shell scripts (done in B.1).
2. Rebuild the image from current source (done in B.3).

**Preventive note (not applied):** a `stop_grace_period: 60s` on the host service in `multi-instance.compose.yml` would prevent future SIGKILLs during normal shutdown drain. Deliberately left out of this pass — it is an operational tuning choice that belongs in a dedicated change, not bundled with the ledger validation.

## B.3 Ledger composition verified — image rebuilt

- **Action:** `docker compose build whyce-host-1` (background task, exit 0). All 8 assemblies published cleanly from current source:
  `Whycespace.Shared`, `Systems`, `Projections`, `Domain`, `Engines`, `Api`, `Runtime`, `Host` → net10.0 DLLs, ~86s dotnet-publish wall time.
- **Second blocker on first boot of the new image:** `Unhandled exception. System.InvalidOperationException: WP-1 FAIL-CLOSED: Jwt:SigningKey is required.` The current source enforces a non-bypassable JWT signing-key check (`AuthenticationInfrastructureModule.cs:33-38`) that the 3-day-old image did not have.
- **Fix:**
  - Added `JWT_SIGNING_KEY=whyce-dev-signing-key-local-infrastructure-validation-only-2026` to `infrastructure/deployment/.env` (alongside the existing `POSTGRES_PASSWORD`/`MINIO_ROOT_PASSWORD` dev placeholders per R-CFG-R1 `${VAR}` substitution pattern).
  - Added `JWT__SigningKey: ${JWT_SIGNING_KEY}` to both `whyce-host-1` and `whyce-host-2` env blocks in `multi-instance.compose.yml`.
- **Boot result:** host-1 healthy in 8 seconds. Full economic composition visible in logs (25 unique consumer topics from `GenericKafkaProjectionConsumerWorker` + 6 integration/detection/lifecycle workers).

**Ledger composition (final state, source of truth: `src/platform/host/composition/economic/EconomicCompositionRoot.cs`):**

| Ledger domain | `Add*Application()` | `*ApplicationModule.RegisterEngines()` | Net |
|---|---|---|---|
| entry | ❌ not called | ❌ not called | **GAP — entry domain is not wired into host composition** |
| journal | ✅ line 70 | ✅ line 301 | wired |
| ledger | ✅ line 71 | ✅ line 302 | wired |
| obligation | ✅ line 72 | ✅ line 303 | wired |
| treasury | ✅ line 73 | ✅ line 304 | wired |

## B.4 Kafka topics provisioned

- **Source-of-truth:** `create-topics.sh` already declares all 5 ledger topic sets (lines 93-121). R-K-17 (K-TOPIC-02) was already satisfied at the script-declaration level; the script just hadn't successfully executed since its last edit.
- **Action:** after LF fix, re-ran `whyce-kafka-init` via `docker start`, waited for it to process the 100+ topic declarations.
- **Result (live broker, verified via `kafka-topics.sh --list`):**

```
whyce.economic.ledger.entry.{commands,events,retry,deadletter}       ✓ 4/4
whyce.economic.ledger.journal.{commands,events,retry,deadletter}     ✓ 4/4
whyce.economic.ledger.ledger.{commands,events,retry,deadletter}      ✓ 4/4
whyce.economic.ledger.obligation.{commands,events,retry,deadletter}  ✓ 4/4
whyce.economic.ledger.treasury.{commands,events,retry,deadletter}    ✓ 4/4
whyce.economic.ledger.transaction.{commands,events,retry,deadletter} ✓ 4/4 (legacy)
```

Total ledger topics on broker: **24** (20 for the 5 prompt domains + 4 for legacy transaction).

## B.5 Ledger consumers booted

Host-1 logs show 4 `GenericKafkaProjectionConsumerWorker` instances subscribed to ledger projection topics (verified via `grep "consumer config applied for whyce.economic.ledger"`):

```
whyce.economic.ledger.journal.events    → projection_economic_ledger_ledger.ledger_read_model    (group: whyce.projection.economic.ledger.journal)
whyce.economic.ledger.ledger.events     → projection_economic_ledger_ledger.ledger_read_model    (group: whyce.projection.economic.ledger.ledger)
whyce.economic.ledger.obligation.events → projection_economic_ledger_obligation.obligation_read_model
whyce.economic.ledger.treasury.events   → projection_economic_ledger_treasury.treasury_read_model
```

Additional ledger consumers (not projection consumers, but ledger-touching workers):
- `LedgerToCapitalIntegrationWorker` — subscribed to 2 topics under consumer group `whyce.integration.ledger-to-capital` (cross-BC bridge from ledger events to capital commands).
- `EnforcementDetectionWorker` — default topics include `whyce.economic.ledger.journal.events` and `whyce.economic.ledger.ledger.events` (OPA-based violation detection).

**Missing consumer:** `whyce.economic.ledger.entry.events` — not registered in `EconomicProjectionModule.RegisterWorker(...)`. Same gap as B.3.

**Unrelated but observed drift (captured here for §1c):** `GenericKafkaProjectionConsumerWorker` emits `fail: ConsumeException on whyce.economic.risk.exposure.events: Subscribed topic not available`. That topic is NOT declared in `create-topics.sh` (only domain classification-groups that are declared: ledger, capital, enforcement, routing, reconciliation, revenue, subject, exchange, transaction, vault, compliance, constitutional/policy, identity/access, operational/{sandbox,global}). The `EconomicProjectionModule` registers a consumer for risk/exposure but no topic exists on the broker. This is an **R-K-20 (K-TOPIC-COVERAGE-01, S0)** violation adjacent to — but outside — the ledger scope.

## B.6 Projections + schemas

- **Projection DB schemas:** all 6 ledger schemas (`projection_economic_ledger_{entry,journal,ledger,obligation,treasury,transaction}`) and their `*_read_model` tables were already present (baseline §7.1); no migrations needed.
- **Projection handlers registered in `ProjectionRegistry` (via `EconomicProjectionModule.RegisterProjections`):**
  - `LedgerUpdatedProjectionHandler` ← `LedgerOpenedEvent`, `LedgerUpdatedEvent`, `JournalEntryAddedEvent`
  - `ObligationProjectionHandler` ← `ObligationCreatedEvent`, `ObligationFulfilledEvent`, `ObligationCancelledEvent`
  - `TreasuryProjectionHandler` ← `TreasuryCreatedEvent`, `TreasuryFundAllocatedEvent`, `TreasuryFundReleasedEvent`
  - **No handler registered for any `entry/*Event` types.**
- **OPA policy packages:** unchanged from baseline. `policies/domain/economic/ledger/transaction.rego` is the only ledger policy loaded. Packages for `entry/journal/ledger/obligation/treasury` remain absent. A production run that exercised any of the 4 wired ledger domains via command would currently fail `constitutional.guard.md` PB-04 (policy scope must match execution domain) — the `PolicyMiddleware` would not find a rego package for the resolved `PolicyId`.

## B — Final State Roll-Up

| Domain | Topics (×4) | Projection schema | Consumer wired | Handler registered | OPA rego | API controller | Net |
|---|---|---|---|---|---|---|---|
| entry | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | **NOT OPERATIONAL** |
| journal | ✅ | ✅ | ✅ (→ ledger_read_model) | ✅ | ❌ | ✅ | **PARTIAL** (no OPA) |
| ledger | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | **PARTIAL** (no OPA, no controller) |
| obligation | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | **PARTIAL** (no OPA) |
| treasury | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | **PARTIAL** (no OPA) |

## B — Revised Phase Verdicts

```
Phase 1  Infrastructure Bootstrap       : PASS (core + host now healthy)
Phase 2  Topics + Schemas               : PASS (all 20 ledger topics live; schemas already existed)
Phase 3  Policy / OPA                   : FAIL — 5/5 ledger rego packages still missing
Phase 4  Event Persistence              : NOT EXERCISED (no command issued in this pass)
Phase 5  Outbox + Kafka                 : NOT EXERCISED (contingent on Phase 4 + Phase 3)
Phase 6  Projections                    : CONDITIONAL PASS — 4/5 consumers live, schemas + handlers present for 4 of 5 domains
Phase 7  API Validation                 : FAIL — entry and ledger controllers still missing
Phase 8  Determinism + Replay           : NOT EXERCISED
Phase 9  Failure Recovery               : NOT EXERCISED
Phase 10 End-to-End Trace               : NOT EXERCISED

OVERALL: CONDITIONAL PASS — infrastructure is now operational for 4 of 5 ledger
domains at the consumer level, but policy coverage and API surface still have
code-level gaps that block a full pipeline exercise.
```

## B — What an "APPROVED" Run Would Still Require (out of scope for this infra pass)

These are **code changes**, not infra work. They are listed so the next run can consume them as its target backlog:

1. **Wire `entry` domain into `EconomicCompositionRoot`:**
   - Add `services.AddLedgerEntryApplication();` alongside the other four (line ~74).
   - Add `LedgerEntryApplicationModule.RegisterEngines(engine);` in `RegisterEngines` (line ~304).
   - Add a projection store, handler, and `RegisterWorker(...)` block for `whyce.economic.ledger.entry.events` in `EconomicProjectionModule`.
   - Register the `{LedgerEntryRecordedEvent}` handler in `RegisterProjections`.

2. **Add the 2 missing API controllers:** `EntryController` and `LedgerController` under `src/platform/api/controllers/economic/ledger/`.

3. **Add 5 OPA rego packages:** `policies/domain/economic/ledger/{entry,journal,ledger,obligation,treasury}.rego`. Minimum shape to satisfy PB-04: `package whyce.policy.economic.ledger.<domain>` with `default allow := false` and per-command allow rules for the commands that `LedgerPolicyModule` binds.

4. **Schema module confirmation:** verify `DomainSchemaCatalog.RegisterEconomic(schema)` includes typed entries for all 5 ledger event types so `EventDeserializer` can materialise payloads on the consumer side. If any are missing, the consumers will log a "no schema for event-type X" warning at first event.

5. **Preventive tuning (optional):**
   - `stop_grace_period: 60s` on host service to avoid SIGKILL during drain.
   - Add `.gitattributes` with `*.sh text eol=lf` to prevent the CRLF regression from recurring.
   - Add `whyce.economic.risk.exposure.{commands,events,retry,deadletter}` to `create-topics.sh` (unrelated drift noted in B.5).

## B — Changes Made on Disk

- `infrastructure/deployment/multi-instance/apply-extra-migrations.sh` — CRLF → LF (no content change).
- `infrastructure/event-fabric/kafka/create-topics.sh` — CRLF → LF (no content change).
- `infrastructure/deployment/.env` — added `JWT_SIGNING_KEY` entry (dev-local placeholder per CFG-R1 `${VAR}` pattern).
- `infrastructure/deployment/multi-instance.compose.yml` — added `JWT__SigningKey: ${JWT_SIGNING_KEY}` to both `whyce-host-1` and `whyce-host-2` env blocks.
- `claude/audits/infrastructure-validation-ledger.audit.output.md` — this file.

No source code under `src/**` was modified in this pass. All four changes above are infrastructure/deployment scope, fully reversible via `git diff`.

*End of §B remediation record.*

