# CLEAN CODE AUDIT — WBSM v3.5

## CLASSIFICATION

governance / audit / clean-code

## MODE

SCORING + REPORTING

---

## AUDIT CATEGORIES

### A1 — READABILITY (Weight: 15%)

Checks:

* Naming clarity
* Function clarity
* Cognitive load

Score:

* PASS / PARTIAL / FAIL

---

### A2 — SIMPLICITY (Weight: 15%)

Checks:

* Over-engineering
* Unnecessary abstraction
* Complexity vs value

---

### A3 — STRUCTURE (Weight: 15%)

Checks:

* Function size
* Nesting depth
* Flow clarity

---

### A4 — DOMAIN PURITY (Weight: 20%) [CRITICAL]

Checks:

* Domain logic location
* Aggregate correctness
* No leakage

---

### A5 — LAYER ISOLATION (Weight: 15%)

Checks:

* Dependency graph
* Forbidden imports

---

### A6 — DETERMINISM (Weight: 10%) [CRITICAL]

Checks:

* No randomness
* No system time usage
* Deterministic IDs

---

### A7 — CONSISTENCY (Weight: 5%)

Checks:

* Naming conventions
* Folder structure
* DDD compliance

---

### A8 — TESTABILITY (Weight: 5%)

Checks:

* Dependency injection
* Pure logic
* Isolation

---

## SCORING MODEL

| Score  | Status  |
| ------ | ------- |
| 90–100 | PASS    |
| 75–89  | WARNING |
| <75    | FAIL    |

---

## OUTPUT FORMAT

```
CLEAN CODE AUDIT REPORT

Overall Score: XX/100
Status: PASS | WARNING | FAIL

CATEGORY BREAKDOWN:
A1 Readability: XX
A2 Simplicity: XX
A3 Structure: XX
A4 Domain Purity: XX
A5 Layer Isolation: XX
A6 Determinism: XX
A7 Consistency: XX
A8 Testability: XX

CRITICAL VIOLATIONS:
* [Rule] File -> Issue

RECOMMENDATIONS:
* Actionable fixes

FINAL VERDICT:
SYSTEM COMPLIANT | REQUIRES FIXES | BLOCKED
```

---

## AUTOMATION INTEGRATION

* Must be runnable via:

  * CI pipeline
  * Claude audit agent
* Must scan:

  * /src/domain
  * /src/engines
  * /src/runtime
  * /src/systems
  * /src/platform

---

## LOCK STATUS

LOCKED — CANONICAL

Any modification requires:

* governance approval
* version bump
* WhyceChain anchoring
