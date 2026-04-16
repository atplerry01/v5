---
title: Ledger Infrastructure Validation — Execution Plan
target: economic-system / ledger
domains: [entry, journal, ledger, obligation, treasury]
created: 20260416-190854
status: PLAN ONLY (no services started, no data written)
depends-on: 20260416-190854-economic-system-infrastructure-validation-ledger.md
---

# Ledger Infrastructure Validation — Execution Plan

Canonical topic pattern: `whyce.economic.ledger.{domain}.{channel}` where `{channel} ∈ {commands, events, retry, deadletter}` and `{domain} ∈ {entry, journal, ledger, obligation, treasury}`.

This plan is the script. Each PHASE is a checklist of concrete commands and assertions. Nothing here is executed in the dry-run invocation.

---

## PRE-EXECUTION: ENV + SECRETS

```
# Required environment (missing => CFG-R1 / CFG-R2 violation)
export POSTGRES_PASSWORD=<from .env.local>
export MINIO_ROOT_PASSWORD=<from .env.local>

# Sanity: confirm .env.local is gitignored
git check-ignore .env.local || echo "FAIL CFG-DC1: .env.local not gitignored"
```

---

## PHASE 1 — INFRASTRUCTURE BOOTSTRAP

```bash
cd infrastructure/deployment
docker-compose -f docker-compose.yml up -d

# Wait for readiness
docker-compose ps   # expect all services Healthy
```

Health assertions:
- `pg_isready -h localhost -p 5432 -U whyce -d whyce_eventstore`        → OK
- `pg_isready -h localhost -p 5434 -U whyce -d whyce_projections`       → OK
- `pg_isready -h localhost -p 5433 -U whyce -d whycechain`              → OK
- `redis-cli -h localhost -p 6379 ping`                                 → PONG
- `curl -s http://localhost:8181/health`                                → 200
- `kafka-broker-api-versions.sh --bootstrap-server localhost:29092`     → non-empty

FAIL ⇒ halt; do NOT proceed to PHASE 2.

---

## PHASE 2 — TOPIC + SCHEMA VALIDATION

### 2a. Kafka topics

```bash
# Expected: 20 topics (5 domains × 4 channels) under whyce.economic.ledger.*
kafka-topics.sh --bootstrap-server localhost:29092 --list \
  | grep '^whyce\.economic\.ledger\.' | sort
```

Expected set (strict):
```
whyce.economic.ledger.entry.commands
whyce.economic.ledger.entry.deadletter
whyce.economic.ledger.entry.events
whyce.economic.ledger.entry.retry
whyce.economic.ledger.journal.commands
whyce.economic.ledger.journal.deadletter
whyce.economic.ledger.journal.events
whyce.economic.ledger.journal.retry
whyce.economic.ledger.ledger.commands
whyce.economic.ledger.ledger.deadletter
whyce.economic.ledger.ledger.events
whyce.economic.ledger.ledger.retry
whyce.economic.ledger.obligation.commands
whyce.economic.ledger.obligation.deadletter
whyce.economic.ledger.obligation.events
whyce.economic.ledger.obligation.retry
whyce.economic.ledger.treasury.commands
whyce.economic.ledger.treasury.deadletter
whyce.economic.ledger.treasury.events
whyce.economic.ledger.treasury.retry
```

### 2b. Schema presence (per-event, C#)

Per domain, verify a registered schema for every emitted event:

| Domain     | Event                            | Expected schema file / registry entry                                     |
|-----------|----------------------------------|----------------------------------------------------------------------------|
| ledger    | LedgerOpenedEvent                | `LedgerOpenedEventSchema.cs`                                               |
| ledger    | LedgerUpdatedEvent               | schema record (currently **only** the inline record in `LedgerEventSchemas.cs`) |
| ledger    | JournalAppendedToLedgerEvent     | schema record (**MISSING**)                                                |
| journal   | JournalCreatedEvent              | schema record (**MISSING**)                                                |
| journal   | JournalEntryAddedEvent           | schema record (**MISSING**)                                                |
| journal   | JournalPostedEvent               | schema record (**MISSING**)                                                |
| entry     | LedgerEntryRecordedEvent         | schema record (**MISSING**)                                                |
| obligation| ObligationCreatedEvent           | schema record (**MISSING**)                                                |
| obligation| ObligationFulfilledEvent         | schema record (**MISSING**)                                                |
| obligation| ObligationCancelledEvent         | schema record (**MISSING**)                                                |
| treasury  | TreasuryCreatedEvent             | schema record (**MISSING**)                                                |
| treasury  | TreasuryFundAllocatedEvent       | schema record (**MISSING**)                                                |
| treasury  | TreasuryFundReleasedEvent        | schema record (**MISSING**)                                                |

### 2c. Payload mappers

```bash
# No *Ledger*Mapper* file exists as of this snapshot.
# MUST grep Runtime envelope producers for ledger event→primitive mapping.
```

### 2d. Consumer group registration (per R-K-07)

Expected naming: `wbsm.economic.ledger.{domain}.projection`.

```bash
kafka-consumer-groups.sh --bootstrap-server localhost:29092 --list | grep 'wbsm\.economic\.ledger\.'
```

---

## PHASE 3 — POLICY (OPA) VALIDATION

OPA container mounts `infrastructure/policy/domain` → `/policies/domain`. All five ledger rego bundles are present on disk (`entry.rego`, `journal.rego`, `ledger.rego`, `obligation.rego`, `treasury.rego`).

### 3a. Bundle load

```bash
curl -s http://localhost:8181/v1/policies | jq '.result[].id' \
  | grep -i 'ledger/'   # expect 5 matches
```

### 3b. Per-domain command evaluation (ALLOW path)

```
POST http://localhost:8181/v1/data/whyce/economic/ledger/<domain>/allow
body: { "input": { "command": ..., "actor": ..., "trustScore": ... } }
expect: { "result": true, "decisionHash": "<sha256>" }
```

Repeat per domain × per command type. Record `decisionHash` — must be byte-stable on replay.

### 3c. DENY path

Mutate one input (e.g. `actor.trustScore = 0`) and re-POST. Expect `result: false` and `PolicyDeniedEvent` emitted to the events topic, carrying `CorrelationId`, `CausationId`, `PolicyName`, `IsAllowed=false` (POL-AUDIT-14).

### 3d. `events.policy_decision_hash` NULL audit (P-EVT-001)

```sql
SELECT count(*) FROM events
WHERE policy_decision_hash IS NULL OR policy_version IS NULL;
-- MUST = 0
```

---

## PHASE 4 — EVENT PERSISTENCE VALIDATION

For each domain, trigger one canonical write-path command via API. Confirm:

```sql
SELECT event_id, aggregate_id, event_type, correlation_id,
       policy_decision_hash, policy_version
FROM events
WHERE aggregate_id = :aggregateId
ORDER BY sequence_number;
```

Assert:
- ≥1 row per command (INV-001)
- All metadata columns non-null (INV-302)
- No duplicates for same `idempotency_key` (INV-303)
- Chain-anchor row committed in same xact (INV-002)

---

## PHASE 5 — OUTBOX + KAFKA VALIDATION

```sql
SELECT id, aggregate_id, topic, status, retry_count
FROM outbox
WHERE aggregate_id = :aggregateId;
```

Assert: `topic` matches `whyce.economic.ledger.{domain}.events` (R-K-16). Status path: `pending → published`.

Kafka-side consumption:
```bash
kafka-console-consumer.sh --bootstrap-server localhost:29092 \
  --topic whyce.economic.ledger.<domain>.events \
  --from-beginning --property print.headers=true --max-messages 1
```

Assert headers present (R-K-24): `event-id`, `aggregate-id`, `event-type`, `correlation-id`.

---

## PHASE 6 — PROJECTION VALIDATION

Projection DBs live on `postgres-projections` (port 5434). Expected tables (from `infrastructure/data/postgres/projections/economic/ledger/`):

- `projection_economic_ledger_entry.entry_read_model`
- `projection_economic_ledger_journal.journal_read_model`
- `projection_economic_ledger_ledger.ledger_read_model`
- `projection_economic_ledger_obligation.obligation_read_model`
- `projection_economic_ledger_treasury.treasury_read_model`
- `projection_economic_ledger_transaction.transaction_read_model` (out of target scope but present on disk)

Assertions per domain:
- Row created/updated on event
- `state` JSONB shape matches reducer contract
- `idempotency_key` prevents duplicate rows on command replay

---

## PHASE 7 — API VALIDATION

Only `LedgerController` exists today (single `POST /api/economic/ledger/open`). Plan:

- Exercise `POST /api/economic/ledger/open` → assert 200 + `CommandAck("ledger_opened")`.
- **GAP**: no GET endpoints, no entry/journal/obligation/treasury controllers. These are MUST-ADD before PHASE 7 can run fully.

---

## PHASE 8 — DETERMINISM + REPLAY VALIDATION

- Re-dispatch identical command with `IClock` frozen + same seeds; assert identical `aggregate_id`, `ExecutionHash`, `PolicyDecisionHash` byte-for-byte (Type A, per constitutional REPLAY-A-vs-B-DISTINCTION-01).
- Replay event stream via `EventReplayService`; assert projection rebuild → same state, sentinel envelope fields preserved (`"replay"` / `DateTimeOffset.MinValue`) — do NOT assert envelope equality on the protected sentinel fields.

---

## PHASE 9 — FAILURE + RECOVERY

1. Stop Kafka mid-run: `docker-compose stop kafka`. Dispatch command. Outbox row must stay `pending`, host must NOT crash (R-K-21, K-OUTBOX-ISOLATION-01).
2. Start Kafka: `docker-compose start kafka`. Outbox row must publish on next relay tick.
3. Restart postgres-projections mid-stream; projection catch-up must be idempotent.
4. Poison a message (malformed payload): verify DLQ produce completes BEFORE source offset commit (R-K-25, K-DLQ-001).

---

## PHASE 10 — END-TO-END TRACE

Pick one correlation id. Trace through:
- `events.correlation_id`
- `whyce_chain.correlation_id`
- `outbox.correlation_id`
- Kafka header `correlation-id`
- `projection_*_read_model.correlation_id`
- API response `correlationId`

All must match. Any mismatch = INV-XSYS-003 / R-CHAIN-CORRELATION-01 violation.

---

## EXIT CRITERIA

- APPROVED: all phases PASS.
- CONDITIONAL PASS: only PHASE 1 infra flakiness; logic phases PASS.
- FAIL: any PHASE 3–10 assertion fails, or any determinism/hash/policy invariant is violated.

## TEARDOWN

```bash
docker-compose -f infrastructure/deployment/docker-compose.yml down
# Volumes preserved by default; add -v to wipe state.
```