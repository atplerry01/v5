# PHASE 1.5 FINAL GATE -- DOMAIN CERTIFICATION TEMPLATE v2

## PURPOSE

Prove a domain implementation is deterministic, replay-safe, failure-safe,
idempotent, and infrastructure-ready. This is the mandatory gate between
implementation and production readiness.

## VERSION: 2.0 -- Based on Kanban live validation (April 2026)

---

# PRE-FLIGHT CHECKLIST (BEFORE ANY TESTING)

These items MUST be verified before sending the first request. Every single
one caused a live failure during Kanban validation when missing.

## Infrastructure Setup

* [ ] Docker containers rebuilt with latest code
* [ ] All containers healthy (check `docker ps` and `/health` endpoint)
* [ ] Idempotency table exists in event store DB
  ```sql
  CREATE TABLE IF NOT EXISTS idempotency_keys (
    key TEXT PRIMARY KEY, created_at TIMESTAMPTZ NOT NULL DEFAULT NOW());
  ```

## Kafka Topics

* [ ] Domain event topic created: `whyce.{c}.{ctx}.{domain}.events`
* [ ] Dead letter topic created: `whyce.{c}.{ctx}.{domain}.deadletter`
* [ ] Retry topic created: `whyce.{c}.{ctx}.{domain}.retry`
* [ ] Verify with: `kafka-topics.sh --list --bootstrap-server localhost:9092`

LESSON: If topics don't exist when the consumer starts, it throws
ConsumeException and the worker dies. The outbox publisher retries but
eventually deadletters the events.

## OPA Policy

* [ ] Rego file created at `infrastructure/policy/domain/{c}/{ctx}/{domain}.rego`
* [ ] Policy loaded into OPA: `curl -X PUT http://opa:8181/v1/policies/... --data-binary @{domain}.rego`
* [ ] Verify with: `curl http://opa:8181/v1/policies` (should list the domain)

LESSON: Without a matching OPA policy, every command returns
"OPA policy denied: Policy denied by OPA. No bypass allowed."

## Projection Database

* [ ] Migration SQL applied to projections DB
* [ ] Verify schema exists: `\dn` in psql

## Consumer Workers

* [ ] Both domain consumers running (check host logs for "Kafka consumer config applied for whyce.{c}.{ctx}.{domain}.events")
* [ ] If missing, check if AddHostedService was used (deduplicates). Fix: use AddSingleton<IHostedService>

---

# SECTION 1 -- END-TO-END EXECUTION PROOF

## Test Sequence

Execute ALL domain operations in order. For Kanban this was:

1. Create board
2. Create 3 lists (Backlog, InProgress, Done)
3. Create 3 cards in Backlog
4. Move card from Backlog to InProgress
5. Reorder card within Backlog
6. Complete card
7. Update card title/description

## For Each Operation Capture

* HTTP status code (200 expected)
* correlationId from response
* aggregateId / entityId from response

## Verify After Each Operation

```sql
-- Event Store
SELECT event_type, version FROM events
WHERE aggregate_id = '{id}' ORDER BY version;

-- WhyceChain
SELECT block_id, correlation_id, previous_block_hash
FROM whyce_chain ORDER BY timestamp DESC LIMIT 3;

-- Outbox
SELECT event_type, status FROM outbox
WHERE aggregate_id = '{id}' ORDER BY created_at;
```

## Verify Projection (after async consume, wait 10-15s)

```sql
SELECT current_version, last_event_type, state
FROM projection_{c}_{ctx}_{domain}.{domain}_read_model
WHERE aggregate_id = '{id}';
```

State must reflect ALL operations applied in correct order.

---

# SECTION 2 -- DETERMINISM PROOF

## Repeat Identical Command

POST the exact same create request again.

EXPECT:
* Response: `{"error":"Duplicate command detected."}`
* Event count: unchanged
* Projection: unchanged

LESSON: Idempotency key must be CommandType:CommandId (unique per command instance),
not CommandType:AggregateId (would block all same-type commands on same aggregate).

---

# SECTION 3 -- REPLAY PROOF

## Steps

1. Record current projection state
2. DELETE FROM projection table
3. Trigger replay service
4. Compare new projection state with recorded state

EXPECT:
* Projection fully rebuilt
* State EXACTLY matches pre-replay
* No missing data, no duplication
* last_event_id progression correct

---

# SECTION 4 -- FAILURE & DLQ PROOF

## Test

Force a consumer failure (e.g., corrupt an event payload or stop the projection DB).

EXPECT:
* Message routed to `whyce.{c}.{ctx}.{domain}.deadletter`
* DLQ message headers present:
  - event-id
  - aggregate-id
  - event-type
  - correlation-id
  - dlq-reason
  - dlq-source-topic
* NO silent skip in consumer
* Consumer continues processing subsequent messages

---

# SECTION 5 -- ORDERING & CONSISTENCY

## Test

Send multiple rapid mutations (create cards, move cards, reorder) in quick succession.

EXPECT:
* All events in EventStore have monotonically increasing versions
* No duplicate positions in projection
* No missing entities
* Cards in correct lists

---

# SECTION 6 -- CONCURRENCY PROOF

## Test

Send 5+ simultaneous requests modifying the same aggregate.

EXPECT:
* Most succeed (optimistic concurrency serializes writes)
* Failed requests get HTTP 409 with `urn:whyce:error:concurrency-conflict`
* No corrupted state in projection
* No duplicate events in EventStore
* Aggregate version uniqueness constraint holds

LESSON: This is expected behaviour, not a bug. OCC conflicts are the correct
resolution for concurrent writers to the same aggregate.

---

# SECTION 7 -- LOAD TEST (BASELINE)

## Test

Send 20-100 rapid sequential requests.

EXPECT:
* All return HTTP 200
* System health remains HEALTHY
* No crashes, no deadlocks
* Outbox drains (may take a few seconds for batch publish)
* Event count matches request count

---

# SECTION 8 -- API READ VALIDATION

## Test

```
GET /api/{domain}/{aggregateId}
```

EXPECT:
* Full hierarchical state returned
* Lists/entities ordered correctly
* All mutations reflected (moves, completes, updates)
* State matches raw projection DB state

LESSON: Ensure JsonSerializer.Deserialize uses
PropertyNameCaseInsensitive = true when projection stores camelCase JSON
and DTOs use PascalCase properties.

---

# SECTION 9 -- POLICY ENFORCEMENT

## Test

Send request with invalid/missing action or role.

EXPECT:
* HTTP 400 with policy denial message
* Audit event (PolicyEvaluatedEvent) emitted
* No domain execution occurred
* No events in domain aggregate stream

---

# SECTION 10 -- DEPENDENCY GRAPH INTEGRITY

## Automated Check

Grep all domain files for forbidden imports:

```bash
# Domain layer: ONLY SharedKernel allowed
grep -r "using Whyce\.\(Runtime\|Engine\|Platform\|System\)" src/domain/{c}-system/{ctx}/{domain}/

# Engine layer: ONLY Contracts + Domain allowed
grep -r "using.*\(Kafka\|Npgsql\|Redis\)" src/engines/T2E/{c}/{ctx}/{domain}/

# Projection layer: NO Domain, Runtime, Engine
grep -r "using Whycespace\.Domain\|using Whyce\.Runtime\|using Whyce\.Engine" src/projections/{c}/{ctx}/{domain}/
```

All must return zero matches.

---

# FINAL PASS CRITERIA

System PASSES ONLY IF:

* [ ] All operations return correct HTTP responses
* [ ] EventStore contains all events with correct versions
* [ ] WhyceChain blocks linked correctly
* [ ] Outbox fully published
* [ ] Projection state matches all operations
* [ ] API GET returns correct state
* [ ] Idempotency proven (no duplicates)
* [ ] Concurrency handled (409 on conflict, no corruption)
* [ ] Load test stable
* [ ] Dependency graph clean
* [ ] No silent failures observed

---

# EVIDENCE FORMAT

Certification output must include:

1. All curl commands with responses
2. SQL query results (EventStore, WhyceChain, Outbox, Projection)
3. Kafka topic verification
4. Idempotency test results (before/after event counts)
5. Concurrency test results (success/conflict counts)
6. Load test results (HTTP status codes, final health check)
7. Final verdict: PASS / FAIL

---

# FAILURE CONDITIONS (IMMEDIATE STOP)

* Event lost (EventStore count < expected)
* Duplicate state in projection
* Replay produces different state
* Consumer silently skips messages
* Runtime bypass detected (events without policy decision)
* Headers missing in Kafka messages
* DLQ events missing reason/source headers
