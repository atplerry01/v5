# TITLE
Todo Final System Validation (Post-Infrastructure)

# CONTEXT
Classification: operational
Context: sandbox
Domain: todo

Full end-to-end system validation of the Todo bounded context after infrastructure deployment. Real execution against live infrastructure (Postgres, Kafka, Redis, OPA).

# OBJECTIVE
Validate the complete Todo pipeline from HTTP request through Identity, Policy, Runtime, Engine, EventStore, ChainAnchor, Outbox, Kafka, Projection, and Redis.

# CONSTRAINTS
- Real execution only (no code inspection)
- All infrastructure must be live and healthy
- Every layer must produce verifiable evidence
- Any unproven step = FAIL

# EXECUTION STEPS
1. Start infrastructure (docker-compose)
2. Verify database tables (events, whyce_chain, outbox, projection)
3. Verify Kafka topics (commands, events, retry, deadletter)
4. Verify OPA policy loads and evaluates
5. POST /api/todo/create
6. Trace full flow (identity, policy, runtime, engine)
7. Database validation (events, whyce_chain, outbox)
8. Redis validation
9. Kafka event validation
10. Projection DB validation
11. Determinism test

# OUTPUT FORMAT
Layer/Status/Evidence table + Trace (ExecutionHash, EventId, PolicyVersion, ChainHash) + Final PASS/FAIL

# VALIDATION CRITERIA
All layers must produce evidence. Any unproven step = FAIL.

# RESULT
## FAIL — 3 Blocking Defects

### Defect 1 — S1: Kafka Outbox Topic Misconfiguration
- KafkaOutboxPublisher.cs:26 hardcodes default topic `whyce.events` (doesn't exist)
- Crashes host via BackgroundServiceExceptionBehavior.StopHost

### Defect 2 — S1: Projection Type Mismatch
- TodoProjectionBridge switches on `TodoCreatedEventSchema` (Whyce.Shared.Contracts)
- EventFabric sends `TodoCreatedEvent` (Whycespace.Domain) — no case matches, event dropped
- Redis never populated, projection DB never populated

### Defect 3 — S2: Missing Todo Topics in create-topics.sh
- Only economic, identity, incident topics defined
- Todo topics manually created during validation

### What Works
- Identity resolution (WhyceIdEngine)
- Policy evaluation (WhycePolicyEngine + AllowAll sandbox)
- Runtime middleware pipeline (8-stage locked order)
- Engine execution (TodoEngine emits TodoCreatedEvent)
- EventStore persistence (events table)
- ChainAnchor (whyce_chain with genesis block)
- Outbox enqueue (outbox table, pending status)
- Deterministic ID generation (SHA256-based)
