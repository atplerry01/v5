TITLE: Infrastructure Validation — Real Execution (Docker / Kafka / Postgres / OPA / Full Pipeline)
# Ledger 
OBJECTIVE:
Validate that the full system pipeline executes correctly against **real infrastructure**, not mocks, for a given:

* CLASSIFICATION
* CONTEXT
* DOMAIN GROUP
* DOMAINS

This validation must prove:

API → Dispatcher → Runtime → Policy → Engine → Event Store → Outbox → Kafka → Projection → API

---

INPUT (REQUIRED):

CLASSIFICATION: {classification}
CONTEXT: {context}
DOMAIN GROUP: {domain_group}

DOMAINS:

* {domain_1}
* {domain_2}
* ...

Example:
CLASSIFICATION: economic-system
CONTEXT: vault
DOMAIN GROUP: vault
DOMAINS:

* account
* metrics
* slice




---

GLOBAL RULES (NON-NEGOTIABLE):

1. MUST run against real infrastructure:

   * Docker (compose)
   * Postgres (event store + projections)
   * Kafka (event fabric)
   * OPA (policy engine)

2. NO mocks, NO in-memory substitutes.

3. MUST validate:

   * determinism
   * idempotency
   * policy enforcement
   * projection correctness

4. MUST use canonical topics:
   whyce.{classification}.{context}.{domain}.{channel}

5. MUST verify full event fabric:
   persist → chain → outbox → Kafka → projection

---

PHASE 1 — INFRASTRUCTURE BOOTSTRAP

1. Start all services:

docker-compose -f infrastructure/deployment/docker-compose.yml up -d

2. Validate services are healthy:

* Postgres reachable
* Kafka brokers available
* OPA responding
* Redis (if used) reachable

3. Wait until all services are ready before proceeding

FAIL if any dependency is unavailable.

---

PHASE 2 — TOPIC + SCHEMA VALIDATION

For each domain:

1. Verify Kafka topics exist:

whyce.{classification}.{context}.{domain}.commands
whyce.{classification}.{context}.{domain}.events
whyce.{classification}.{context}.{domain}.retry
whyce.{classification}.{context}.{domain}.deadletter

2. Verify schemas registered in EconomicSchemaModule (or equivalent)

3. Verify payload mappers exist and correctly map domain events → primitives

FAIL if:

* topic missing
* schema missing
* mapper missing

---

PHASE 3 — POLICY (OPA) VALIDATION

1. Trigger a command per domain

2. Verify:

* PolicyMiddleware invoked
* OPA evaluated decision
* PolicyDecisionHash generated

3. Test:

* Allowed command → executes
* Denied command (simulate constraint) → blocked

FAIL if:

* command bypasses policy
* policy not invoked
* decision not enforced

---

PHASE 4 — EVENT PERSISTENCE VALIDATION

For each domain:

1. Execute a write command via API

2. Verify in Postgres event store:

* event written
* correct aggregate_id
* correct event_type
* correct correlation_id

3. Verify:

* event is immutable
* no duplicate entries for same idempotency key

FAIL if:

* event not persisted
* incorrect metadata
* duplicate writes

---

PHASE 5 — OUTBOX + KAFKA VALIDATION

1. Verify outbox entry created in same transaction

2. Verify outbox publishes to Kafka

3. Inspect Kafka message:

* headers:

  * event-id
  * aggregate-id
  * event-type
  * correlation-id

4. Verify:

* message appears in correct topic
* retry/deadletter behavior correct on failure

FAIL if:

* message not published
* headers missing
* wrong topic

---

PHASE 6 — PROJECTION VALIDATION

For each domain:

1. Verify projection table exists:

projection_{classification}*{context}*{domain}.{domain}_read_model

2. Execute commands → events

3. Verify:

* projection row created/updated
* state JSONB populated correctly
* idempotency_key prevents duplicates

4. Re-run same command:

* projection must NOT duplicate or corrupt state

FAIL if:

* projection missing
* incorrect state
* duplicate rows

---

PHASE 7 — API VALIDATION

1. Call GET endpoints

2. Verify:

* responses come from projections (NOT domain)
* data matches projection state

3. Validate query filters (e.g., by id, by key attributes)

FAIL if:

* API bypasses projection
* stale or inconsistent data returned

---

PHASE 8 — DETERMINISM + REPLAY VALIDATION

1. Re-run same command (same input)

2. Verify:

* same aggregate_id
* no duplicate state
* idempotency holds

3. Replay event stream:

* resulting state must be identical

FAIL if:

* different outcomes
* duplicate effects

---

PHASE 9 — FAILURE + RECOVERY TEST

Simulate:

* Kafka downtime
* Postgres restart
* partial execution

Verify:

* outbox retries correctly
* no event loss
* no duplicate execution

---

PHASE 10 — END-TO-END TRACE VALIDATION

Trace a single request:

API → Dispatcher → Runtime → Policy → Engine → Event Store → Kafka → Projection → API

Verify:

* correlation_id consistent across all layers
* full chain observable

---

FINAL OUTPUT:

SYSTEM INFRASTRUCTURE VALIDATION REPORT

INCLUDE:

* Infrastructure status (Docker, Kafka, Postgres, OPA)
* Per-domain validation results
* Event pipeline verification
* Policy enforcement verification
* Projection correctness
* Determinism + replay results
* Failure recovery results

FINAL STATUS:

* APPROVED → all phases PASS
* CONDITIONAL PASS → infra issue only
* FAIL → any pipeline violation

---

END
