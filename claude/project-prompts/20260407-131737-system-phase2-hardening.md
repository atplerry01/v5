# WHYCESPACE — PHASE 2 HARDENING PROMPT (CANONICAL LOCK)

## CLASSIFICATION: system / governance / hardening

## PHASE: Phase 2 Entry Gate

## PRIORITY: CRITICAL

## MODE: NON-NEGOTIABLE (ZERO DRIFT)

---

# 🎯 OBJECTIVE

Harden the entire codebase to meet **Phase 2 financial-grade deterministic execution requirements**.

This prompt MUST:

* eliminate all non-determinism
* enforce engine purity
* enforce runtime authority
* isolate projections correctly
* prepare the system for economic domain implementation

---

# 🔒 GLOBAL RULES (MANDATORY — NO EXCEPTION)

1. **Determinism is absolute**

   * No randomness
   * No system time
   * No hidden side effects

2. **Runtime is the ONLY authority for:**

   * persistence
   * chain anchoring
   * kafka publishing

3. **Engines are PURE execution**

   * no persistence
   * no infra access
   * no time generation
   * no ID generation

4. **Domain is PURE**

   * no external dependencies
   * no system calls
   * no time or ID creation

5. **Policy MUST execute before domain logic**

   * denial MUST block execution BEFORE aggregate load

---

# 🚨 TASK GROUP 1 — GLOBAL DETERMINISM HARD LOCK

## 1.1 REMOVE ALL NON-DETERMINISTIC ID GENERATION

### FIND (GLOBAL)

* Guid.NewGuid()

### REPLACE WITH

* DeterministicIdHelper.FromSeed(...)

### RULE

* Seed MUST come from:

  * aggregateId
  * command input
  * deterministic context

---

## 1.2 REMOVE ALL DIRECT TIME USAGE

### FIND (GLOBAL)

* DateTime.UtcNow
* DateTimeOffset.UtcNow
* DateTime.Now
* DateTimeOffset.Now

### REPLACE WITH

* IClock.UtcNow

---

## 1.3 ENFORCE ZERO TOLERANCE (ADD GUARD)

CREATE:

```
/claude/guards/determinism.guard.md
```

CONTENT:

* FAIL if ANY of the following exist in:

  * src/domain/**
  * src/engines/**
  * src/systems/**
  * src/runtime/**

BLOCK LIST:

* Guid.NewGuid
* DateTime.UtcNow
* DateTimeOffset.UtcNow
* DateTime.Now
* DateTimeOffset.Now

---

## 1.4 ADD DETERMINISM AUDIT

CREATE:

```
/claude/audits/determinism.audit.md
```

Must report:

* count of violations
* file paths
* PASS only if ZERO

---

# 🚨 TASK GROUP 2 — ENGINE PURITY ENFORCEMENT

## 2.1 REMOVE ALL PERSISTENCE FROM ENGINES

### FIND (IN /src/engines/**)

* SaveChanges
* DbContext
* repository usage
* direct database calls

### ACTION

* REMOVE immediately
* Replace with:

  * EngineContext.EmitEvents(...)

---

## 2.2 ENFORCE ENGINE CONTRACT

ALL engines MUST:

* accept input
* execute domain logic
* emit events ONLY

NO:

* persistence
* kafka
* redis
* http calls

---

## 2.3 ADD ENGINE PURITY GUARD

CREATE:

```
/claude/guards/engine-purity.guard.md
```

FAIL IF FOUND IN /src/engines/**:

* DbContext
* SaveChanges
* HttpClient
* Redis
* KafkaProducer
* File IO

---

## 2.4 ADD ENGINE AUDIT

CREATE:

```
/claude/audits/engine-purity.audit.md
```

---

# 🚨 TASK GROUP 3 — RUNTIME AUTHORITY ENFORCEMENT

## 3.1 ENSURE SINGLE ENTRY POINT

VERIFY:

* ALL execution goes through:

  * IRuntimeControlPlane.ExecuteAsync

FAIL IF:

* any direct engine invocation bypass exists

---

## 3.2 ENFORCE EXECUTION ORDER (STRICT)

Runtime MUST enforce:

1. Guard (pre)
2. Policy evaluation
3. Guard (post-policy)
4. Execution (engine/workflow)
5. Persist events
6. Chain anchor
7. Kafka publish

FAIL IF ANY ORDER IS BROKEN

---

## 3.3 ADD RUNTIME GUARD

CREATE:

```
/claude/guards/runtime-authority.guard.md
```

Checks:

* no persistence outside runtime
* no kafka outside runtime
* no chain outside runtime

---

# 🚨 TASK GROUP 4 — PROJECTION ISOLATION HARDENING

## 4.1 ENFORCE STRICT BOUNDARY

### src/projections

* ONLY:

  * read models
  * projection handlers

### src/runtime/projection

* ONLY:

  * kafka consumers
  * routing
  * rebuild tools

---

## 4.2 REMOVE VIOLATIONS

FAIL IF:

* domain logic in runtime/projection
* kafka write in projections
* direct db write outside handlers

---

## 4.3 ADD PROJECTION GUARD

CREATE:

```
/claude/guards/projection-boundary.guard.md
```

---

# 🚨 TASK GROUP 5 — POLICY ENFORCEMENT HARDENING

## 5.1 ENFORCE PRE-EXECUTION POLICY

VERIFY:

* policy executes BEFORE:

  * aggregate load
  * domain execution

FAIL IF:

* policy runs after execution

---

## 5.2 ENSURE DENY BLOCKS COMPLETELY

ON DENY:

* NO domain execution
* NO events
* NO persistence

---

## 5.3 ADD POLICY GUARD

CREATE:

```
/claude/guards/policy-enforcement.guard.md
```

---

# 🚨 TASK GROUP 6 — ECONOMIC PHASE PREPARATION (FOUNDATION ONLY)

CREATE STRUCTURE:

```
src/domain/economic-system/
  capital/
  ledger/
  transaction/
  settlement/
```

FOR EACH DOMAIN:

MANDATORY FOLDERS:

* aggregate/
* entity/
* value-object/
* event/
* service/
* specification/
* error/

---

## 6.1 ADD BASE INVARIANT PLACEHOLDERS

* Ledger must balance (debit = credit)
* Transactions must be idempotent
* Settlement must be replay-safe

(NO implementation yet — ONLY structure + contracts)

---

# 🚨 TASK GROUP 7 — FINAL VALIDATION

## 7.1 SYSTEM MUST PASS:

* Determinism audit → 0 violations
* Engine purity audit → 0 violations
* Runtime authority audit → PASS
* Projection boundary audit → PASS
* Policy enforcement audit → PASS

---

## 7.2 E2E VALIDATION

VERIFY:

```
API → Systems → Runtime → Engine → Events → Projection → API
```

WITH:

* deterministic replay producing identical results
* no duplicate events
* no ordering violations

---

# 🧾 OUTPUT REQUIREMENTS

Produce:

## 1. HARDENING REPORT

* all violations found
* all fixes applied
* before/after summary

## 2. AUDIT RESULTS

* each audit file result

## 3. FINAL STATUS

* PASS / FAIL

---

# 🔐 LOCK CONDITION

Phase 2 MUST NOT START unless:

ALL conditions = PASS

---

# END OF PROMPT
