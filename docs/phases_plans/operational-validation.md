# STANDARD PROMPT — OPERATIONAL VALIDATION AND RESILIENCE TESTING

Validate the system using **realistic or real data**, **real API calls**, and **real infrastructure behavior** for the following target area:

* Classification: [CLASSIFICATION]
* Context: [CONTEXT]
* Domain Group: [DOMAIN GROUP]
* Domain: [DOMAIN]
* Environment: [LOCAL / DEV / STAGING / PRE-PROD / PROD-LIKE]
* Entry Path: [SWAGGER / CURL / POSTMAN / SDK / OTHER]

This is **not a unit/integration test exercise**.
This is a **production-like operational validation** covering domain behavior, runtime behavior, infrastructure behavior, failure handling, and recovery integrity.

## 1. Validation Objective

Validate that the system behaves correctly under realistic operating conditions across:

* API execution
* systems orchestration
* runtime control flow
* engines and domain execution
* persistence
* messaging
* projections
* policy enforcement
* infrastructure failure and recovery
* concurrency
* load and stress conditions

The goal is to prove **correctness, resilience, recoverability, and observability**, not just functional success.

---

## 2. Scope of Validation

Include real or production-like validation across the full execution path:

Platform API → Systems → Runtime → Engines → Domain → Events → Persistence → Messaging → Projections → API Response

Infrastructure and dependent services in scope must include, where applicable:

* API gateway / service entry
* database
* Kafka
* Redis
* OPA / policy engine
* projection workers
* background processors
* retry handlers
* DLQ handlers
* tracing / metrics / logs

---

## 3. Test Inputs

Use:

* real or realistic payloads
* real entity relationships
* realistic identifiers
* realistic timestamps
* realistic state transitions
* realistic policy contexts
* realistic workflow states
* realistic data volumes
* realistic concurrency levels

Execute requests through real interfaces such as:

* Swagger
* curl
* Postman
* SDK/client integration

Avoid toy payloads unless explicitly needed for fault isolation.

---

## 4. Validation Categories

Execute and validate the following categories.

### A. Functional Operational Validation

Validate normal business execution using real API calls and realistic data.

Checks:

* request accepted/rejected correctly
* domain invariants enforced
* policy decisions applied correctly
* correct events emitted
* persistence correct
* projections updated correctly
* response shape and status correct

---

### B. Load Testing

Validate behavior under expected and sustained operational load.

Checks:

* throughput
* latency
* queue growth
* worker saturation
* projection lag
* retry behavior under load
* system stability over time

Record:

* request rate
* success/failure ratio
* p50/p95/p99 latency
* backlog growth
* recovery after load reduction

---

### C. Stress Testing

Validate behavior beyond normal capacity to identify failure boundaries.

Checks:

* degradation pattern
* error handling quality
* backpressure behavior
* queue overflow handling
* stability under overload
* graceful degradation versus uncontrolled failure

Record:

* breaking point
* first bottleneck
* downstream failure sequence
* recovery path after stress is removed

---

### D. Concurrency Testing

Validate parallel execution and contention behavior.

Checks:

* race conditions
* optimistic concurrency handling
* duplicate command handling
* idempotency correctness
* ordering guarantees
* lock/contention behavior
* replay safety after concurrent execution

Use scenarios such as:

* multiple updates to same aggregate
* multiple actors on same workflow
* parallel event production
* simultaneous retries and original requests

---

### E. Failure Simulation

Validate system behavior when components fail during live execution.

Simulate failures such as:

* Kafka unavailable
* Redis unavailable
* OPA unavailable
* database latency or temporary failure
* projection consumer failure
* worker crash during processing
* network interruption
* downstream timeout

Checks:

* failure detection
* containment
* retries
* fallback behavior
* error transparency
* no hidden corruption
* no invalid state transitions
* correct recovery path

---

### F. Pause and Resume Simulation

Validate ability to safely pause and resume execution.

Scenarios:

* workflow paused intentionally
* worker shutdown during processing
* projection consumer paused
* messaging paused and resumed
* dependent service restored after outage

Checks:

* resume correctness
* no duplicate side effects
* no lost events
* no invalid state jumps
* preserved correlation and trace continuity

---

### G. Infrastructure Failure and Recovery Validation

Specifically validate infrastructure fault and restoration behavior.

Test component failures and recoveries for:

* Kafka
* Redis
* OPA
* database
* projection workers
* retry executors
* chain/policy dependencies if applicable

Checks:

* service degradation behavior
* recovery sequencing
* replay correctness
* data integrity after restore
* backlog drain correctness
* consistency after recovery
* operational observability during outage and restore

---

### H. DLQ and Retry Validation

Validate dead-letter, retry, and replay behavior.

Checks:

* failed messages routed correctly
* retry policies applied correctly
* poison message containment
* replay safety
* duplicate prevention
* auditability of failure and retry history
* operator ability to inspect and recover

---

### I. Recovery and Continuity Validation

Validate that interrupted execution can continue safely.

Checks:

* recovery after dependency outage
* event consistency after restore
* projection rebuild correctness
* message replay correctness
* workflow continuation correctness
* operator recovery path quality
* no silent data loss

---

### J. Observability Validation

Validate that the system is diagnosable during success, stress, and failure.

Capture:

* correlation IDs
* traces across layers
* service logs
* domain events
* metrics
* queue depth
* retry counts
* DLQ counts
* projection lag
* policy decision traces
* failure markers
* recovery markers

---

## 5. Required Evidence

Collect evidence for each validation run:

* API requests and responses
* curl or Swagger execution samples
* logs
* traces
* event samples
* persistence snapshots
* projection snapshots
* queue/DLQ state
* retry history
* metrics and latency data
* failure injection timestamps
* recovery timestamps

---

## 6. Result Classification

Classify each scenario as:

* PASS — correct, resilient, and recoverable
* WARNING — functionally correct but with risk, drift, or weak recovery behavior
* FAIL — incorrect, unsafe, inconsistent, or unrecoverable
* BLOCKED — scenario could not be completed due to missing environment readiness

---

## 7. Outcome Assessment

For each scenario, answer:

* Did the business/domain behavior remain correct?
* Did infrastructure behavior remain safe?
* Did the system fail gracefully?
* Did the system recover correctly?
* Was data integrity preserved?
* Was messaging integrity preserved?
* Were retries and DLQ behavior correct?
* Was pause/resume safe?
* Was concurrency handled correctly?
* Was observability sufficient for diagnosis?
* Is the system operationally ready for this path?

---

## 8. Improvement Output

Document:

* domain issues
* runtime issues
* infrastructure weaknesses
* policy gaps
* messaging weaknesses
* projection inconsistencies
* observability gaps
* recovery weaknesses
* scale bottlenecks
* operator recovery gaps

---

## 9. Rules

* Use real API calls where possible
* Use real or production-like data
* Do not reduce validation to scripted pass/fail assertions only
* Validate infrastructure, not only domain logic
* Validate failure and recovery, not only success paths
* Validate concurrency and load, not only single-request behavior
* Surface weaknesses honestly
* Treat this as operational certification input, not just testing

---

## 10. Minimum Scenario Set

At minimum, execute:

* normal success path
* invalid request path
* policy-denied path
* load test
* stress test
* concurrent update test
* Kafka failure and recovery
* Redis failure and recovery
* OPA failure and recovery
* pause/resume scenario
* DLQ scenario
* replay/recovery scenario
* projection consistency check after recovery

Produce a final readiness summary for the target classification/domain.
