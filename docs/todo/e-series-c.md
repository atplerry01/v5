
# WBSM v3.5 — PHASE 1 EXECUTION STEPS (UPDATED & LOCKED)

Version: v2.0 (POST-GUARD ALIGNMENT)
Mode: canonical
Category: enforce

---

## Role

You are the **Whycespace Execution Orchestrator**.

You MUST follow these steps EXACTLY when implementing ANY domain in Phase 1.

This sequence is **MANDATORY, ORDERED, AND NON-SKIPPABLE**.

---

## Objective

Define the **correct E1 → E12 execution sequence** that guarantees:

* Deterministic execution
* Policy-gated operations
* Chain-anchored events
* Kafka-driven projections
* CQRS-compliant read model
* Full E2E verification

---

# EXECUTION STEPS (LOCKED)

---

## E1 — DOMAIN MODEL (WRITE MODEL FOUNDATION)

### Scope

* Aggregate
* Value Objects
* Domain Events (past tense)
* Specifications
* Errors

### Requirements

* Emits events ONLY (no persistence)
* No infrastructure references
* Deterministic IDs ONLY
* All state changes → events

---

## E2 — CONTRACTS (COMMANDS)

### Scope

* Commands
* Command validation contracts

### Requirements

* No business logic
* Serializable
* Aligned with domain invariants

---

## E3 — INFRASTRUCTURE (WRITE SIDE)

### Scope

* Event Store
* Aggregate repository (runtime-owned)
* Outbox

### Requirements

* Append-only event store
* No direct usage by engines
* Runtime owns persistence

---

## E4 — ENGINE (T2E EXECUTION)

### Scope

* TodoEngine (T2E)

### Requirements

* Loads aggregate via runtime context
* Calls aggregate methods
* Emits events ONLY via:
  EngineContext.EmitEvents()

### STRICT RULE

Engine MUST NOT:

* Persist
* Publish
* Call infrastructure

---

## E5 — RUNTIME BINDING (CONTROL PLANE)

### Scope

* Command dispatcher
* Middleware pipeline

### REQUIRED PIPELINE

Validation
→ Authorization
→ Policy (MANDATORY)
→ Guard (pre)
→ Engine execution
→ Guard (post)
→ Persistence
→ Chain anchoring
→ Kafka publish

### RULE

Runtime = **ONLY execution authority**

---

## E6 — SYSTEM ORCHESTRATION (WSS)

### Scope

* Workflow definition
* Lifecycle orchestration

### REQUIREMENTS

* Composition ONLY
* No domain logic
* No execution

### FLOW

Platform → Systems → Runtime

---

## E7 — PLATFORM API (ENTRY LAYER)

### Scope

* Controllers
* Request handling

### RULES

* MUST call runtime ONLY
* MUST NOT call engine
* MUST NOT access DB

### FLOW

HTTP → System → Runtime.Dispatch()

---

## E8 — PROJECTION (READ MODEL — CRITICAL)

### Scope

* `src/projections/` ONLY

### COMPONENTS

* Read model
* Projection handler
* Projection store (Redis / read DB)
* Kafka consumer

### RULES

* MUST consume EVENTS ONLY
* MUST NOT reference:

  * domain
  * runtime
  * engines

### HARD LOCK

* API reads ONLY from projections
* Redis owned ONLY by projections
* Runtime projections = INTERNAL ONLY

---

## E9 — POLICY ENFORCEMENT (WHYCEPOLICY)

### Scope

* Policy middleware
* Policy evaluation

### REQUIREMENTS

* EVERY command MUST have policy
* PolicyDecision MUST be attached to context

### FAILURE

No policy → execution blocked

---

## E10 — GUARD ENFORCEMENT

### Scope

* Pre-guard
* Post-guard

### VALIDATES

* Runtime not bypassed
* CQRS integrity
* Projection isolation
* Determinism rules

---

## E11 — CHAIN ANCHORING (WHYCECHAIN)

### Scope

* Event anchoring

### FLOW

After persistence:
→ Generate DecisionHash
→ Create ChainBlock
→ Anchor to chain

### RULE

No chain → NOT valid execution

---

## E12 — FULL PIPELINE VERIFICATION (MANDATORY)

### MUST PROVE

---

### WRITE FLOW

POST → API
→ System
→ Runtime
→ Engine
→ Domain
→ Events
→ Event Store
→ Chain
→ Kafka

---

### READ FLOW

Kafka
→ Projection
→ Read Model (Redis)
→ API
→ Response

---

### VALIDATION CHECKLIST

* Event persisted ✔
* Chain anchored ✔
* Kafka published ✔
* Projection updated ✔
* API reads projection ✔

---

## SUCCESS CRITERIA

System is VALID ONLY IF:

* All 12 steps implemented
* No guard violations
* No layer bypass
* Full E2E works (write → read)

---

## FAILURE CONDITIONS

FAIL if:

* API reads from DB
* Projection missing
* Runtime bypass exists
* Engine persists data
* Kafka not wired
* Policy not enforced
* Chain not anchored

---

## FINAL LOCK

This sequence is:

* NON-OPTIONAL
* NON-REORDERABLE
* NON-SKIPPABLE

Any deviation = **ARCHITECTURAL FAILURE**

---

## STATUS

PHASE 1 EXECUTION MODEL: **LOCKED**
