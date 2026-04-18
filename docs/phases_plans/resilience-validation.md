# STANDARD PROMPT — RESILIENCE VALIDATION

Validate the resilience of the following target area under production-like conditions:

* Classification: [CLASSIFICATION]
* Context: [CONTEXT]
* Domain Group: [DOMAIN GROUP]
* Domain: [DOMAIN]
* Environment: [LOCAL / DEV / STAGING / PRE-PROD / PROD-LIKE]
* Entry Path: [SWAGGER / CURL / POSTMAN / SDK / OTHER]

This validation must use **real or production-like data**, **real API calls**, and **real infrastructure components** where possible.

This is **not** a functional test-suite exercise.
This is a **resilience validation** exercise focused on failure handling, degradation behavior, continuity, recovery, replay safety, and operational integrity.

---

## 1. Objective

Validate that the system remains safe, observable, and recoverable when exposed to operational stress, dependency failures, concurrency, retries, pauses, resumptions, and infrastructure disruptions.

The purpose is to prove:

* graceful degradation
* controlled failure behavior
* no invalid state corruption
* correct retry and DLQ behavior
* correct pause/resume behavior
* correct recovery behavior
* data and event integrity after restoration
* operational diagnosability during failure and recovery

---

## 2. In-Scope Execution Path

Validate resilience across the full execution chain:

Platform API → Systems → Runtime → Engines → Domain → Persistence → Messaging → Projections → API Response

Infrastructure components in scope may include:

* API/service entry
* database
* Kafka
* Redis
* OPA / policy engine
* projection workers
* background processors
* retry executors
* DLQ handlers
* observability stack

---

## 3. Data and Request Rules

Use:

* real or production-like payloads
* realistic IDs and timestamps
* realistic workflow and entity states
* realistic policy context
* realistic request volume
* realistic concurrency levels

Execute through real interfaces such as:

* Swagger
* curl
* Postman
* SDK/client path

Do not use toy payloads unless isolating a specific failure.

---

## 4. Resilience Scenarios to Execute

### A. Load Resilience

Run sustained load at expected operating levels.

Validate:

* stable throughput
* acceptable latency
* queue stability
* projection lag behavior
* retry growth under load
* worker saturation behavior
* recovery after load reduction

Capture:

* request rate
* p50 / p95 / p99 latency
* success/failure rate
* backlog growth
* recovery time

---

### B. Stress / Overload Resilience

Push beyond expected capacity.

Validate:

* graceful degradation
* backpressure behavior
* timeout handling
* failure isolation
* overload safety
* operator visibility of degradation
* recovery after pressure is removed

Capture:

* saturation point
* first bottleneck
* cascading failure pattern
* time to stabilize

---

### C. Concurrency Resilience

Run concurrent requests against shared state and shared workflows.

Validate:

* race-condition handling
* optimistic concurrency correctness
* idempotency correctness
* duplicate command handling
* event ordering behavior
* no hidden state corruption
* replay safety after concurrent execution

Use scenarios such as:

* concurrent updates to same aggregate
* concurrent workflow actions
* duplicate submit/retry overlap
* simultaneous producer/consumer pressure

---

### D. Dependency Failure Simulation

Intentionally disrupt critical infrastructure.

Simulate:

* Kafka unavailable
* Redis unavailable
* OPA unavailable
* database latency or temporary outage
* projection worker crash
* background processor crash
* network interruption
* downstream timeout

Validate:

* error detection
* containment
* safe rejection or buffering behavior
* retry behavior
* operator visibility
* absence of silent corruption
* safe restoration path

---

### E. Pause and Resume Validation

Pause and later resume key parts of execution.

Scenarios:

* workflow pause/resume
* projection consumer pause/resume
* message consumption pause/resume
* worker shutdown and restart
* temporary dependency suspension and restore

Validate:

* no duplicate side effects
* no lost work
* no invalid state jumps
* preserved traceability
* correct continuation semantics

---

### F. Retry and DLQ Resilience

Force message and processing failures.

Validate:

* retry policy correctness
* retry exhaustion behavior
* poison message containment
* DLQ routing correctness
* replay safety
* no duplicate side effects during replay
* operator ability to inspect and recover

Capture:

* retry counts
* DLQ counts
* failed payload evidence
* replay outcome

---

### G. Recovery and Continuity Validation

Restore failed components and confirm the system heals correctly.

Validate:

* backlog drain behavior
* projection catch-up behavior
* event replay correctness
* workflow continuation correctness
* no lost persisted truth
* no divergence between write model and read model
* safe recovery order

Capture:

* outage start time
* restore time
* recovery completion time
* reconciliation result

---

### H. Observability Under Failure

Validate that failures and recoveries are diagnosable.

Capture:

* correlation IDs
* traces across layers
* service logs
* domain events
* retry markers
* DLQ markers
* queue depth
* projection lag
* policy decision logs
* recovery markers
* latency spikes
* saturation warnings

Validate:

* can operators see the problem
* can operators locate the failed component
* can operators confirm recovery
* can operators inspect replay and DLQ state

---

## 5. Mandatory Checks

For every scenario, verify:

* Was the system safe during failure?
* Was the failure contained?
* Did invariants remain protected?
* Was invalid state prevented?
* Were retries correct?
* Was DLQ behavior correct?
* Was replay safe?
* Was pause/resume safe?
* Was recovery correct?
* Was read-model consistency restored?
* Was observability sufficient?
* Is the system operationally resilient for this path?

---

## 6. Required Evidence

Collect:

* request samples
* curl / Swagger samples
* response samples
* logs
* traces
* metrics
* event samples
* persistence snapshots
* projection snapshots
* queue state
* retry evidence
* DLQ evidence
* failure injection timestamps
* recovery timestamps
* post-recovery consistency checks

---

## 7. Result Classification

Classify each scenario as:

* PASS — resilient, recoverable, observable
* WARNING — functionally survived but with operational risk
* FAIL — unsafe, inconsistent, unrecoverable, or not diagnosable
* BLOCKED — environment or tooling not ready

---

## 8. Final Readiness Summary

Summarize:

* strongest resilience behaviors
* weakest failure points
* bottlenecks
* recovery gaps
* DLQ / replay gaps
* concurrency risks
* observability gaps
* operator recovery gaps
* readiness decision:

  * READY
  * READY WITH RISKS
  * NOT READY

---

## 9. Rules

* Use real API calls where possible
* Use real or production-like data
* Test both failure and recovery
* Do not validate success path only
* Do not hide degraded behavior behind superficial pass/fail
* Do not mark resilient unless recovery is proven
* Surface data integrity, replay, and projection issues explicitly
* Treat this as resilience certification input
