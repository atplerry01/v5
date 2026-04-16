TITLE: End-to-End Validation & Certification — Domain Batch (Generic Canonical)

OBJECTIVE:
Perform a full-system validation of a domain batch implemented under the Whycespace canonical architecture.

This validation MUST confirm that all domains in the batch are:

* correctly implemented (minimum S4 domain standard unless otherwise specified)
* structurally compliant with canonical hierarchy
* fully wired through the execution pipeline
* integrated with infrastructure (Postgres, Kafka, Redis, OPA, WhyceChain)
* deterministic, policy-enforced, and production-ready

This is a SYSTEM CERTIFICATION PASS — not a superficial review.

---

0. REQUIRED INPUTS (TO BE FILLED PER RUN)

---

CLASSIFICATION: economic-system
CONTEXT: enforcement
DOMAIN GROUP: enforcement

DOMAINS:

* escalation
* lock
* restriction
* rule
* sanction
* violation


Optional description:
<business/system purpose>

---

1. VALIDATION MODE (STRICT)

---

* Do NOT assume correctness
* Every layer must be verified with evidence
* If any critical layer fails → mark FAIL
* No partial validation

Final output must be:

PASS / CONDITIONAL PASS / FAIL

---

2. SCOPE & STRUCTURE VALIDATION

---

VERIFY:

1. Canonical path:
   src/domain/{classification}-system/{context}/{domain-group}/

2. Domain group exists and contains ALL domains listed

3. No domain misplacement across contexts

4. Naming correctness:

   * lowercase domains
   * canonical naming conventions respected

OUTPUT:

* PASS / FAIL
* list of structural violations

---

3. DOMAIN MODEL VALIDATION (E1 — S4 STANDARD)

---

For EACH domain:

VERIFY:

A. Folder Structure (only justified folders):

* aggregate/
* entity/ (if needed)
* event/
* value-object/
* error/
* specification/
* service/ (only if justified)
* README.md

B. Domain Purity:

* NO infrastructure dependencies
* NO DbContext / EF / Dapper
* NO HttpClient
* NO external services

C. Determinism:

* NO Guid.NewGuid()
* NO DateTime.UtcNow / Now
* NO randomness
* IDs deterministic
* time abstracted

D. Aggregate Integrity:

* valid lifecycle/state transitions
* invariants enforced internally
* no invalid mutation paths

E. Events:

* represent real business transitions
* correctly named and meaningful

F. Errors:

* explicit domain-specific errors
* no generic exceptions as business logic

G. Specifications:

* reusable rules extracted

OUTPUT:

* PASS / FAIL per domain
* violations list

---

4. COMMAND LAYER VALIDATION (E2)

---

VERIFY:

* commands exist for all domain actions
* commands are:

  * deterministic
  * immutable
  * correctly named

CHECK:

* no business logic inside commands
* no infrastructure leakage

---

5. QUERY LAYER VALIDATION (E3)

---

VERIFY:

* queries exist for all required read use cases
* CQRS separation respected
* DTOs clean and minimal

---

6. ENGINE HANDLER VALIDATION (E4 — T2E)

---

VERIFY:

* command handlers exist
* handlers:

  * map command → aggregate
  * enforce idempotency
  * respect control-plane middleware

CHECK:

* no direct DB access
* repository abstraction used
* events emitted correctly

---

7. POLICY INTEGRATION VALIDATION (E5)

---

VERIFY:

* WHYCEPOLICY is invoked before execution

FOR EACH domain action:

* policy action defined:
  policy.{classification}.{context}.{domain}.{action}

CHECK:

* decision invoked
* DecisionHash generated
* deny path enforced

---

8. EVENT FABRIC VALIDATION (E6)

---

VERIFY Kafka integration:

Topic naming MUST follow:
whyce.{classification}.{context}.{domain}.{channel}

Channels:

* .commands
* .events
* .retry
* .deadletter

CHECK:

* events published via outbox
* headers present:

  * event-id
  * aggregate-id
  * correlation-id
  * event-type

---

9. POSTGRES VALIDATION (EVENT STORE + PROJECTIONS)

---

VERIFY:

1. Event Store:

* events persisted before publish
* aggregate streams correct
* versioning enforced

2. Outbox:

* entries created after commit
* retry behavior correct

3. Projections:

* read models exist for each domain

CHECK:

* consistency between event store and projections

---

10. REDIS VALIDATION

---

VERIFY:

* used only for:

  * caching OR
  * distributed locks

CHECK:

* NOT used as source of truth
* invalidation works post-event

---

11. WORKFLOW VALIDATION (E9)

---

VERIFY:

IF workflows exist:

* justified by:

  * long-running
  * multi-step
  * cross-domain

ELSE:

* must remain T2E-only

---

12. API LAYER VALIDATION (E8)

---

VERIFY:

* endpoints exist for commands and queries

CHECK:

* API → Runtime → Engine flow complete
* no domain logic in controllers
* routing follows canonical structure:
  /api/{classification}/{context}/{domain}/...

---

13. END-TO-END VALIDATION (E12)

---

EXECUTE FULL FLOW PER DOMAIN:

1. API call

2. VERIFY:

* command created

* middleware executed:
  tracing → validation → policy → authorization → idempotency → execution

* aggregate updated

* event stored (Postgres)

* chain anchored (WhyceChain)

* outbox entry created

* event published (Kafka)

* projection updated

* API response returned

VERIFY WITH EVIDENCE:

* DB (event store)
* Kafka
* projection
* API response

---

14. OBSERVABILITY VALIDATION (E10)

---

VERIFY:

* logs include:

  * correlation-id
  * command-id

* metrics:

  * execution time
  * failure rate

* tracing:
  API → runtime → engine → domain → event → projection

---

15. SECURITY & ENFORCEMENT (E11)

---

VERIFY:

* WhyceID used for identity
* authorization enforced
* restricted operations blocked where required

CHECK:

* no anonymous execution
* trust / role checks applied

---

16. FINAL CERTIFICATION OUTPUT

---

RETURN:

1. Overall Status:
   PASS / CONDITIONAL PASS / FAIL

2. Per-Domain Status:

   * <domain-1>: PASS / FAIL
   * <domain-2>: PASS / FAIL

3. Infrastructure Status:

   * Postgres: PASS / FAIL
   * Kafka: PASS / FAIL
   * Redis: PASS / FAIL
   * OPA (WHYCEPOLICY): PASS / FAIL

4. Critical Failures:
   (must fix before progression)

5. Non-Critical Gaps:
   (trackable items)

6. Evidence Summary:

   * API proof
   * Kafka proof
   * DB proof
   * projection proof

7. Certification Decision:

   * APPROVED FOR PHASE PROGRESSION
   * BLOCKED

---

## MANDATORY FAILURE RULE

If ANY of the following fails:

* determinism
* policy enforcement
* event persistence
* kafka publishing
* projection update

→ SYSTEM = FAIL

No exceptions.

---

## END OF PROMPT
