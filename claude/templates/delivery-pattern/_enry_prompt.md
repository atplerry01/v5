
Execute E1–Ex template against an existing domain context.

---

## INPUT

Classification: core-system
Context: {{all context}}
Phases: {{PHASES_IN_SCOPE}}

---

## OBJECTIVE

Align all domains under the context with the template (01–06).

---

## PROCESS

### 1. Scan

Read all domains under:

src/domain/{{CLASSIFICATION}}/{{CONTEXT}}/*

## template location @/claude/templates/*
---

### 2. Compare

For each domain:

* validate against template 01–06
* identify deviations

---

### 3. Classify deviations

Label each:

* missing
* inconsistent
* drift

---

### 4. Apply fixes

Fix only:

* structural issues
* template misalignment

Do NOT:

* redesign domain logic
* introduce new abstractions

---

### 5. Verify

Ensure:

* all templates satisfied
* no regressions introduced
* tests still pass

---

## OUTPUT

* list of fixes applied
* files modified
* invariants preserved
* verification results

---

## SUCCESS

Context is fully aligned with template and safe for reuse.
