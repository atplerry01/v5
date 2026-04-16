TITLE: Infrastructure Validation & Certification — End-to-End Execution (Generic Template)

OBJECTIVE:
Validate full **live infrastructure execution** (not static) across:

* Postgres (event store + projections)
* Kafka (topics, outbox relay, consumers)
* Redis (if applicable)
* OPA / WHYCEPOLICY (policy enforcement)
* WhyceChain (anchor integrity)

---

## INPUT FORMAT (GENERIC)

CLASSIFICATION: {classification}
CONTEXT: {context}
DOMAIN GROUP: {domain_group}

DOMAINS:
{domain_1}
{domain_2}
...

---

## INSTANTIATED INPUT

CLASSIFICATION: economic-system
CONTEXT: reconciliation
DOMAIN GROUP: reconciliation

DOMAINS:

* process
* discrepancy

---

## 🚨 NON-NEGOTIABLE VALIDATION RULES

1. Validation MUST run against LIVE infrastructure (Docker stack).
2. ALL flows must pass through full pipeline:
   API → Dispatcher → Runtime → T2E → EventStore → Chain → Outbox → Kafka → Projection → Response
3. Each domain MUST:

   * Execute at least one command
   * Produce at least one event
   * Update projection
4. At least ONE multi-step lifecycle MUST be validated.
5. Any failure in:

   * policy enforcement
   * event persistence
   * Kafka publishing
   * projection update
     = AUTOMATIC FAIL

---

## PHASE 0 — INFRASTRUCTURE BOOTSTRAP

### 0.1 Start Stack

```bash
docker-compose up -d
```

Validate services:

```bash
docker ps
```

MUST include:

* postgres
* kafka
* zookeeper (if used)
* redis (optional)
* opa
* whycechain (or chain service)
* api/host service

---

### 0.2 Apply Migrations

```bash
psql -h localhost -U postgres -d whyce \
-f infrastructure/data/postgres/migrations.sql
```

Apply projections:

```bash
psql -h localhost -U postgres -d projections \
-f infrastructure/data/postgres/projections/**/001_projection.sql
```

---

### 0.3 Create Kafka Topics

```bash
./infrastructure/event-fabric/kafka/create-topics.sh
```

Verify:

```bash
kafka-topics.sh --bootstrap-server localhost:9092 --list \
| grep "whyce.economic.reconciliation"
```

---

## PHASE 1 — POLICY VALIDATION (OPA)

### 1.1 Verify Policy Load

```bash
curl http://localhost:8181/v1/policies
```

Ensure:

```text
policy.economic.reconciliation.process.*
policy.economic.reconciliation.discrepancy.*
```

---

### 1.2 Test Policy Enforcement

Send invalid request (missing actor):

```bash
curl -X POST http://localhost:8080/api/economic/reconciliation/process/trigger \
-H "Content-Type: application/json" \
-d '{"data":{"Reference":"fail-test"}}'
```

EXPECTED:

* ❌ Request rejected (403 / policy deny)

---

## PHASE 2 — SINGLE COMMAND VALIDATION

### 2.1 Trigger Reconciliation

```bash
curl -X POST http://localhost:8080/api/economic/reconciliation/process/trigger \
-H "Content-Type: application/json" \
-H "X-Actor-Id: test-actor" \
-H "Correlation-Id: rec-001" \
-d '{"data":{"Source":"ledger","Target":"external","Reference":"rec-001"}}'
```

---

### 2.2 Validate Event Store

```sql
SELECT event_type, aggregate_id, version
FROM events
WHERE event_type='ReconciliationTriggeredEvent'
ORDER BY sequence_number DESC LIMIT 1;
```

---

### 2.3 Validate Chain Anchor

```sql
SELECT block_id, decision_hash
FROM chain_blocks
ORDER BY created_at DESC LIMIT 1;
```

---

### 2.4 Validate Outbox

```sql
SELECT topic, status
FROM outbox
ORDER BY created_at DESC LIMIT 1;
```

---

### 2.5 Validate Kafka

```bash
kafka-console-consumer.sh \
--bootstrap-server localhost:9092 \
--topic whyce.economic.reconciliation.process.events \
--from-beginning --max-messages 1
```

---

### 2.6 Validate Projection

```sql
SELECT aggregate_id, state
FROM projection_economic_reconciliation_process.process_read_model
ORDER BY projected_at DESC LIMIT 1;
```

---

## PHASE 3 — FULL LIFECYCLE VALIDATION (MANDATORY)

### 3.1 Execute Flow

```bash
# Step 1 — Trigger
POST /process/trigger

# Step 2 — Mismatch
POST /process/mismatched

# Step 3 — Detect discrepancy
POST /discrepancy/detect

# Step 4 — Investigate
POST /discrepancy/investigate

# Step 5 — Resolve discrepancy
POST /discrepancy/resolve

# Step 6 — Resolve reconciliation
POST /process/resolve
```

---

### 3.2 Validate State Evolution

```sql
SELECT state->>'Status'
FROM projection_economic_reconciliation_process.process_read_model
WHERE aggregate_id='<id>';
```

EXPECTED FINAL STATE:

```text
Resolved
```

---

## PHASE 4 — IDEMPOTENCY TEST

Replay same command:

```bash
POST /process/trigger (same CorrelationId)
```

EXPECTED:

* No duplicate events
* Same response (idempotent)

---

## PHASE 5 — FAILURE & RETRY TEST

### 5.1 Stop Kafka

```bash
docker stop kafka
```

Trigger command → EXPECT:

* Outbox entry persists
* API may succeed or degrade (depending on policy)

---

### 5.2 Restart Kafka

```bash
docker start kafka
```

Verify:

* Outbox relay publishes pending events

---

## PHASE 6 — CONCURRENCY TEST

Run parallel:

```bash
seq 1 10 | xargs -P10 -I{} curl ...
```

Verify:

* No race condition
* Aggregate versioning holds
* No duplicate state

---

## PHASE 7 — HARD GATES

| Gate        | Requirement |
| ----------- | ----------- |
| Policy      | Enforced    |
| Event Store | Persisted   |
| Chain       | Anchored    |
| Outbox      | Written     |
| Kafka       | Delivered   |
| Projection  | Updated     |
| Idempotency | Verified    |
| Concurrency | Safe        |

---

## FINAL OUTPUT FORMAT

Provide:

1. INFRASTRUCTURE STATUS (services running)
2. COMMAND EXECUTION RESULTS
3. DATABASE VERIFICATION (events, outbox, projections)
4. KAFKA VERIFICATION
5. POLICY VERIFICATION
6. FAILURE TEST RESULTS
7. CONCURRENCY RESULTS
8. FINAL STATUS: PASS / FAIL

---

## FINAL CERTIFICATION RULE

```text
If ALL gates pass → FULL PASS
If ANY gate fails → FAIL
If infrastructure not executed → CONDITIONAL PASS ONLY
```

---
