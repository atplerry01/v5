# CLEAN CODE GUARD — WBSM v3.5 (LOCKED)

## CLASSIFICATION

governance / clean-code / enforcement

## MODE

MANDATORY — BLOCKING

---

## PRINCIPLE

Clean code in Whycespace is defined as:

> Deterministic, readable, domain-aligned, non-over-engineered, and structurally consistent code that is easy to understand, test, and modify.

---

## ENFORCEMENT RULES

### CCG-01 — READABILITY (MANDATORY)

* All variables, methods, classes MUST use descriptive, intention-revealing names
* Single-letter variables are FORBIDDEN (except loop counters)
* Abbreviations are FORBIDDEN unless domain-standard

BLOCK IF:

* ambiguous naming detected
* non-semantic identifiers used

---

### CCG-02 — FUNCTION SIZE & FOCUS

* Functions MUST do ONE thing only
* Max recommended length: 20–30 lines
* Nested depth MUST NOT exceed 3 levels

BLOCK IF:

* multiple responsibilities detected
* deep nesting (>3)
* large monolithic methods

---

### CCG-03 — NO SPAGHETTI LOGIC

* Deep nested conditionals MUST be flattened
* Early returns MUST be preferred
* Flow MUST be linear and predictable

BLOCK IF:

* nested if/else chains > 3 levels
* branching chaos without clear flow

---

### CCG-04 — NO OVER-ENGINEERING

* Do NOT introduce abstractions without clear necessity
* Avoid premature generalization
* Avoid unused interfaces, factories, patterns

BLOCK IF:

* unused abstractions
* speculative architecture
* indirection without value

---

### CCG-05 — DOMAIN PURITY (CRITICAL)

* Business logic MUST exist ONLY in domain aggregates/entities
* No business logic in:

  * controllers
  * runtime
  * engines (outside orchestration role)

BLOCK IF:

* domain logic leakage detected outside domain layer

---

### CCG-06 — LAYER ISOLATION

STRICT enforcement:

| Layer    | Allowed Access   |
| -------- | ---------------- |
| Platform | Systems only     |
| Systems  | Runtime only     |
| Runtime  | Engines only     |
| Engines  | Domain only      |
| Domain   | NOTHING external |

BLOCK IF:

* any cross-layer violation occurs

---

### CCG-07 — DETERMINISM (CRITICAL)

FORBIDDEN:

* Guid.NewGuid()
* DateTime.UtcNow
* Random()

REQUIRED:

* DeterministicIdHelper
* Injected IClock

BLOCK IF:

* non-deterministic behavior detected

---

### CCG-08 — SELF-DOCUMENTING CODE

* Code MUST express intent without comments
* Comments ONLY allowed for:

  * WHY, not WHAT

BLOCK IF:

* excessive comments explaining obvious logic
* unclear logic requiring explanation

---

### CCG-09 — CONSISTENCY

* Naming conventions MUST be uniform
* Folder structure MUST follow canonical rules
* DDD structure MUST be complete

BLOCK IF:

* inconsistent naming
* structural deviations

---

### CCG-10 — TESTABILITY

* Code MUST be:

  * deterministic
  * side-effect controlled
  * dependency-injected

BLOCK IF:

* hidden dependencies
* untestable logic

---

## ENFORCEMENT MODE

* PRE-COMMIT: WARNING
* CI/CD: HARD BLOCK
* RUNTIME: NOT APPLICABLE

---

## OUTPUT

Violation MUST return:

```
CLEAN_CODE_VIOLATION
Rule: <RULE_ID>
File: <path>
Reason: <description>
Fix: <required correction>
```

---

## LOCK STATUS

LOCKED — CANONICAL
