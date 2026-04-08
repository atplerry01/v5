# WHYCESPACE — CLAUDE CODE CANONICAL EXECUTION RULES (WBSM v3)

Version: v1.0 (LOCKED)
Authority: WHYCEPOLICY™ Alignment
Scope: ALL Claude prompt execution, generation, modification, and orchestration

---

## 0. CORE PRINCIPLE (NON-NEGOTIABLE)

Claude operates as a **deterministic execution agent** within Whycespace.

* No improvisation
* No assumption
* No silent deviation
* No structural drift

If it is not explicitly defined → it MUST NOT be inferred.

---

## 1. PROMPT EXECUTION GOVERNANCE

### 1.1 Mandatory Execution Flow

ALL prompts MUST follow:

1. Policy Binding (WHYCEPOLICY™)
2. Prompt Integrity Validation
3. Guard Validation (Pre-execution)
4. Execution (Deterministic)
5. Audit + Trace Recording
6. Output Validation (Post-execution)

If ANY stage fails → execution MUST halt.

---

### 1.2 Prompt Integrity Rules

* Every prompt MUST:

  * Be hash-verifiable (prompt-hashes.json)
  * Be immutable once executed
  * Have a unique identity

* Any modification:
  → MUST generate a new prompt version
  → MUST NOT overwrite existing prompt

---

## 2. PROMPT STORAGE (MANDATORY — NEW RULE)

### 2.1 Canonical Storage Path

ALL prompts MUST be saved under:

/claude/project-prompts/{datetime}-{classification}-{topic}.md

---

### 2.2 Naming Convention (STRICT)

| Component      | Rule                           |
| -------------- | ------------------------------ |
| datetime       | ISO format → YYYYMMDD-HHMMSS   |
| classification | Must match WBSM classification |
| topic          | kebab-case, concise, no spaces |

---

### 2.3 Example

/claude/project-prompts/20260404-121530-economic-system-ledger-hardening.md

---

### 2.4 Storage Enforcement

Claude MUST:

* Save EVERY executed prompt
* NEVER skip storage
* NEVER overwrite an existing prompt
* NEVER store outside `/claude/project-prompts/`

Violation = CRITICAL FAILURE

---

## 3. PROMPT STRUCTURE RULES

### 3.1 Mandatory Prompt Format

Every prompt MUST follow:

# TITLE

# CONTEXT

# OBJECTIVE

# CONSTRAINTS

# EXECUTION STEPS

# OUTPUT FORMAT

# VALIDATION CRITERIA

---

### 3.2 Structural Integrity

* No missing sections
* No free-form prompts
* No ambiguous instructions

---

## 4. CLASSIFICATION ALIGNMENT (WBSM v3)

### 4.1 Mandatory Classification Mapping

Every prompt MUST declare:

* Classification
* Context
* Domain (if applicable)

Example:

classification: economic-system
context: ledger
domain: transaction

---

### 4.2 Invalid Classification Handling

If classification is unclear:

→ Claude MUST STOP
→ Request clarification
→ MUST NOT proceed

---

## 5. ANTI-DRIFT ENFORCEMENT

### 5.1 Forbidden Actions

Claude MUST NOT:

* Change architecture structure
* Introduce new patterns without instruction
* Rename canonical elements
* Move files outside canonical structure
* Infer missing system components

---

### 5.2 Drift Detection

If drift is detected:

1. STOP execution
2. Flag violation
3. Output drift report

---

## 6. FILE SYSTEM GOVERNANCE

### 6.1 Allowed Directories

Claude MUST ONLY operate within:

/src
/infrastructure
/tests
/docs
/scripts
/claude

---

### 6.2 Restricted Actions

* No hidden file creation
* No temporary file leakage
* No cross-layer violations

---

## 7. ENGINE + DOMAIN PURITY RULES

### 7.1 Domain Layer

* ZERO external dependencies
* No runtime references
* No infrastructure leakage

---

### 7.2 Engine Layer

* MUST be stateless
* MUST NOT persist data
* MUST ONLY emit events

---

### 7.3 Runtime Layer

* ONLY layer allowed to:

  * Persist
  * Publish events
  * Anchor to WhyceChain

---

## 8. POLICY ENFORCEMENT (MANDATORY)

* ALL operations MUST pass through WHYCEPOLICY
* No bypass allowed unless explicitly marked

NoPolicyFlag → MUST emit anomaly

---

## 9. DETERMINISM RULES

Claude MUST ensure:

* No randomness (no Guid.NewGuid)
* No system time usage (use IClock)
* Deterministic IDs only
* Deterministic hashing (SHA256)

---

## 10. EVENT-DRIVEN ENFORCEMENT

* ALL state changes MUST emit events
* Events MUST follow naming:

{Domain}{Action}Event

* No silent state mutation

---

## 11. AUDIT + TRACEABILITY

Every execution MUST produce:

* Execution Trace
* Policy Decision
* Guard Result
* Chain Anchor (if required)

---

## 12. FAILURE HANDLING

On ANY failure:

* STOP execution
* DO NOT partially complete
* Return structured failure report:

STATUS: FAILED
STAGE: <stage>
REASON: <reason>
ACTION_REQUIRED: <next step>

---

## 13. OUTPUT RULES

Claude MUST:

* Be explicit
* Be structured
* Be complete
* Avoid conversational fluff

---

## 14. PROMPT VERSIONING

* Versioning is immutable

Format:
v1.0 → initial
v1.1 → minor change
v2.0 → structural change

---

## 15. ENFORCEMENT PRIORITY

Priority Order:

1. WHYCEPOLICY™
2. Canonical Architecture (WBSM v3)
3. This Rule Set
4. Prompt Instructions

---

## 16. VIOLATION CLASSIFICATION

| Severity | Description               |
| -------- | ------------------------- |
| S0       | System-breaking violation |
| S1       | Architectural violation   |
| S2       | Structural inconsistency  |
| S3       | Minor formatting issue    |

---

## 17. SELF-VALIDATION REQUIREMENT

Before output, Claude MUST validate:

* Structure compliance
* Classification correctness
* Storage completed
* No drift introduced

---

## 18. LOCK CONDITION

This document is:

* CANONICAL
* IMMUTABLE
* NON-OVERRIDABLE

Any change requires:

→ Explicit user instruction
→ Version increment
→ Full audit trail

---

# END OF RULES
