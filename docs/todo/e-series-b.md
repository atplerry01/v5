Good — now we move from **example (Todo)** → **canonical reusable structure for ANY domain**, governed by your attached guard file .

What you already built is **correct**, but it is still **instance-specific**.

Now we elevate it into a **domain-agnostic execution template** that Claude / any builder can apply without drift.

---

# ✅ Canonical E1 → EX Structure (Domain-Agnostic)

This is the **universal template** every domain must follow.

---

# 0. Canonical Domain Placement (MANDATORY GUARD)

Before E1 starts, enforce:

## 0.1 Domain Addressing

```
src/domain/{classification}/{context}/{domain}/
```

## 0.2 Example

```
economic-system / capital / account
operational-system / sandbox / todo
structural-system / registry / structure
```

## 0.3 Guard Rules

* MUST follow: **CLASSIFICATION → CONTEXT → DOMAIN**
* Folder names: lowercase kebab-case
* Domain must be **isolated (no external dependencies)**

---

# E1 — Domain Model (Pure DDD — UNIVERSAL TEMPLATE)

## E1.1 Domain Definition

* Define:

  * Purpose
  * Boundary
  * Invariants

## E1.2 Aggregate (MANDATORY)

```
aggregate/{Domain}Aggregate.cs
```

* All state mutation happens here

## E1.3 Entities (Optional)

```
entity/
```

## E1.4 Value Objects (MANDATORY)

```
value-object/
```

* IDs must be deterministic

## E1.5 Domain Events (MANDATORY)

```
event/
```

* Past tense naming

## E1.6 Errors (MANDATORY)

```
error/
```

## E1.7 Specifications (MANDATORY)

```
specification/
```

## E1.8 Domain Invariants (MANDATORY)

* Defined explicitly
* Enforced BEFORE event emission

---

# E2 — Contracts & Events (Boundary Layer)

## E2.1 Commands

```
shared/contracts/{domain}/command/
```

## E2.2 Integration Events

```
shared/contracts/{domain}/event/
```

## E2.3 DTOs

```
shared/contracts/{domain}/dto/
```

## E2.4 Mapping Rules

* Domain → Integration
* Aggregate → DTO

---

# E3 — Persistence & Infrastructure (Runtime-Owned)

## E3.1 Event Store

* Append-only
* Stream:

```
{domain}-{aggregateId}
```

## E3.2 Repository Pattern (Runtime Only)

* LoadAggregate<T>()
* Save via events only

## E3.3 Messaging (Kafka)

```
whyce.{classification}.{context}.{domain}.commands
whyce.{classification}.{context}.{domain}.events
whyce.{classification}.{context}.{domain}.retry
whyce.{classification}.{context}.{domain}.deadletter
```

## E3.4 Outbox

* Mandatory before publish

---

# E4 — Determinism & Integrity (GLOBAL GUARD LAYER)

## E4.1 Deterministic IDs

* No randomness

## E4.2 Idempotency

* Command-level protection

## E4.3 Execution Hash

* Deterministic per execution

## E4.4 Time Governance

* IClock only

## E4.5 Replay Integrity

* Event replay must produce same state

---

# E5 — Engine Execution (T2E — STRICT RULES)

## E5.1 Engine

```
src/engines/{classification}/{context}/{domain}/{Domain}Engine.cs
```

## E5.2 Execution Pattern

```
LoadAggregate → Execute → EmitEvents
```

## E5.3 HARD RULES

* ❌ No persistence
* ❌ No infra access
* ❌ No policy logic
* ✅ Stateless only

---

# E6 — Runtime Binding (CONTROL PLANE)

## E6.1 Handlers

```
src/runtime/handlers/{domain}/
```

## E6.2 Middleware Pipeline (LOCKED)

```
Validation
→ Authorization
→ Policy (MANDATORY)
→ Idempotency
→ Guard (Pre)
→ Execution
→ Guard (Post)
```

## E6.3 Event Flow (LOCKED)

```
Persist → Chain → Kafka
```

## E6.4 Context Injection

* Identity
* Economic
* Workflow
* Correlation

---

# E7 — System Orchestration (MIDSTREAM)

## E7.1 Placement

```
src/systems/midstream/{system}/
```

## E7.2 Rules

* ❌ No domain logic
* ❌ No persistence
* ✅ Orchestration only

## E7.3 Responsibility

* Workflow coordination
* Intent dispatch

---

# E8 — Platform API (ENTRY LAYER)

## E8.1 Controllers

```
src/platform/api/{domain}/
```

## E8.2 Rules

* ❌ No domain logic
* ❌ No engine calls directly
* ✅ Only via system/runtime

## E8.3 Validation

* Input validation mandatory

## E8.4 Response Standard

```
status
data
error
```

---

# E9 — Projection / Read Model (CANONICAL)

## E9.1 Placement (LOCKED CHANGE)

```
src/projections/{classification}/{context}/{domain}/
```

## E9.2 Projection Rules

* Event-driven only
* No business logic

## E9.3 Storage

* Redis (primary)
* Optional Postgres

## E9.4 Query APIs

* Read-only endpoints

---

# E10 — Policy / Guard / Chain (T0U ENFORCEMENT)

## E10.1 Policy (WHYCEPOLICY)

* Externalized rules
* Versioned

## E10.2 Guard System

* Pre-policy guard
* Post-policy guard

## E10.3 Chain (WhyceChain)

* Policy decision anchored
* Event payload anchored

## E10.4 LOCKED EXECUTION FLOW

```
Guard (pre)
→ Policy
→ Guard (post)
→ Execute
→ Persist
→ Chain
→ Publish
```

---

# EX — Full Activation (UNIVERSAL PROOF)

## EX.1 End-to-End Flow (UNCHANGED)

```
API
→ Platform
→ System
→ Runtime
→ Engine
→ Domain
→ Events
→ Store
→ Chain
→ Kafka
→ Projection
→ API Response
```

---

## EX.2 Mandatory Proof Cases (ALL DOMAINS)

### 1. Lifecycle Flow

* Create → Update → Close

### 2. Workflow Flow

* Via WSS orchestration

### 3. Determinism

* Replay consistency

### 4. Idempotency

* Duplicate safety

### 5. Policy Enforcement

* Deny before execution

### 6. Chain Integrity

* Anchors exist

---

# 🔒 Canonical Guard Interpretation (From Your File)

Your attached structure  already enforces:

* strict E1 → EX layering
* deterministic execution
* policy-first enforcement
* runtime-owned persistence
* full vertical slice validation

### The upgrade we just did is:

✅ From **Todo-specific**
→ to **System-wide reusable contract**

---

# 🔥 Final Canonical Insight (Important)

This structure is not just “guideline”.

It becomes:

> **The ONLY legal way a domain can exist inside Whycespace**

Meaning:

* Any domain that **skips E4 (determinism)** → ❌ INVALID
* Any domain that **bypasses E6 (runtime pipeline)** → ❌ INVALID
* Any domain that **does not produce EX proof** → ❌ NOT COMPLETE

---

# If you want next step

We can now formalize this into:

✅ **GLOBAL DOMAIN IMPLEMENTATION GUARD (Claude-ready)**
→ enforced across all domains automatically

or

✅ **AUTO-GENERATION PROMPT**
→ "give me a domain name → generate full E1–EX structure instantly"

or

✅ **AUDIT RULESET**
→ detect violations across all implemented domains

---

Just tell me which one you want.
