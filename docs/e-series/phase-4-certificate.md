# PHASE 1.5 FINAL GATE — KANBAN SYSTEM CERTIFICATION (STRICT WBSM v3.5)

## OBJECTIVE

Prove the kanban system is:

✔ Deterministic
✔ Replay-safe
✔ Failure-safe
✔ Idempotent
✔ Infrastructure-ready

This is the FINAL gate before Phase 2.

---

# SECTION 1 — END-TO-END EXECUTION PROOF

## Test Flow

1. Create board
2. Create lists (Backlog, InProgress, Done)
3. Create 3 cards
4. Move cards across lists
5. Reorder cards
6. Complete card
7. Update card

---

## REQUIRED EVIDENCE (CAPTURE ALL)

For EACH operation:

* HTTP response (200 or expected error)
* correlation_id
* event_id
* aggregate_id

---

## VERIFY

✔ EventStore → row created
✔ WhyceChain → block created and linked
✔ Outbox → message created
✔ Kafka → message present
✔ Projection → state updated
✔ API GET → reflects correct state

---

# SECTION 2 — DETERMINISM PROOF

## Repeat identical command

POST same command twice

EXPECT:

✔ second call rejected OR no-op
✔ no duplicate event
✔ no projection duplication

---

## Execution Hash Consistency

Verify:

same command + same state → same execution result

---

# SECTION 3 — REPLAY PROOF (CRITICAL)

## Steps

1. Clear projection table
2. Trigger replay

---

## EXPECT

✔ projection fully rebuilt
✔ state EXACTLY matches pre-replay
✔ no missing data
✔ no duplication

---

## VERIFY

* last_event_id progression correct
* ordering preserved
* no divergence

---

# SECTION 4 — FAILURE & DLQ PROOF

## Simulate failure

Break projection handler intentionally

Trigger event

---

## EXPECT

✔ message goes to:
whyce.operational.sandbox.kanban.deadletter

✔ headers present:

* event-id
* aggregate-id
* event-type
* correlation-id
* dlq-reason

✔ NO silent skip

---

## Restore handler

Replay DLQ event

✔ system recovers correctly

---

# SECTION 5 — ORDERING & CONSISTENCY

## Rapid sequence test

Send:

* multiple CreateCard
* multiple MoveCard
* multiple ReorderCard

---

## EXPECT

✔ no duplicate positions
✔ no missing cards
✔ no ordering corruption

---

# SECTION 6 — LOAD TEST (BASELINE)

## Simulate:

* 100–1000 commands burst

---

## VERIFY

✔ no runtime crash
✔ no deadlocks
✔ no event loss
✔ outbox drains correctly

---

# SECTION 7 — CONCURRENCY TEST

Simulate:

* multiple clients modifying same board

---

## EXPECT

✔ deterministic resolution
✔ no corrupted state
✔ no race condition visible in projection

---

# SECTION 8 — OBSERVABILITY CHECK

Verify logs/metrics:

✔ correlation_id present everywhere
✔ event_id traceable
✔ no silent failure logs
✔ errors structured

---

# SECTION 9 — POLICY ENFORCEMENT

Test:

* missing ActorId
* invalid action

---

## EXPECT

✔ request denied at policy layer
✔ audit event emitted
✔ no domain execution

---

# SECTION 10 — FINAL SYSTEM INTEGRITY

Verify:

✔ dependency graph still clean
✔ no domain leakage
✔ no infra misuse
✔ no config safety violations

---

# FINAL PASS CRITERIA

System PASSES ONLY IF:

✔ All sections validated
✔ No silent failure observed
✔ Replay produces identical state
✔ DLQ works correctly
✔ Idempotency proven
✔ Ordering preserved under load

---

# FAILURE CONDITIONS

FAIL immediately if:

* event lost
* duplicate state appears
* replay mismatch
* silent consumer skip
* runtime bypass detected

---

# OUTPUT REQUIRED

Produce:

1. Execution logs (key steps)
2. DB snapshots (event store + projection)
3. Kafka topic evidence
4. DLQ evidence (if triggered)
5. Replay proof (before vs after)
6. Summary verdict:

PASS / FAIL

---

## EXECUTION MODE

NO ASSUMPTIONS
EVIDENCE ONLY
INFRASTRUCTURE-GRADE VALIDATION
SYSTEM MUST PROVE ITSELF
