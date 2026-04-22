Good—this is exactly the moment to **codify intent into an enforceable prompt**.
This prompt should act as a **refactoring engine instruction**, not just guidance.

I’ll give you a **production-grade prompt** you can reuse to:

* refactor the repo
* validate structure
* enforce anti-drift
* generate missing skeleton pieces

---

# ✅ **Canonical Refactor Prompt — Domain Skeleton (Triad Model)**

You are refactoring a domain-driven design (DDD) codebase into a strict canonical structure.

Your task is to rebuild the domain model into a **three-classification system**:

1. control-system → system governance and administration
2. core-system → minimal universal primitives
3. platform-system → communication and messaging contracts

---

## OBJECTIVE

Transform the existing domain structure into a **clean skeleton domain model** that:

* strictly follows classification boundaries
* removes semantic overlap
* enforces dependency direction
* prepares for Phase 2.5 canonical implementation

---

## TARGET STRUCTURE

The final structure MUST follow:

domain/
control-system/
core-system/
platform-system/

Each classification must contain:

classification-system / bounded-context / domain

Example:

domain/
control-system/
system-policy/
policy-definition/
policy-decision/

core-system/
temporal/
time-window/

platform-system/
command/
command-envelope/

---

## CLASSIFICATION RULES

### control-system (authority)

Contains ONLY:

* governance
* configuration
* access control
* audit
* observability
* orchestration (administrative only)
* optional system-wide reconciliation

MUST NOT contain:

* business workflows
* domain-specific aggregates
* messaging constructs
* primitive-only models

---

### core-system (language)

Contains ONLY:

* temporal primitives
* ordering primitives
* identifier primitives

STRICT RULES:

* no services
* no aggregates with lifecycle
* no state transitions
* no behavior beyond validation
* immutable concepts only

---

### platform-system (communication)

Contains ONLY:

* command model
* event model
* envelope model
* routing model
* schema model

STRICT RULES:

* no business semantics
* no policy logic
* no authorization logic
* no domain-specific naming

---

## DEPENDENCY RULES (MANDATORY)

core-system → no dependencies
platform-system → may depend on core-system only
control-system → may depend on core + platform

NO OTHER DEPENDENCIES ARE ALLOWED

---

## TASKS

### 1. RECLASSIFY

* Move all domains into one of the three classifications
* Remove any domain that does not clearly belong
* Do NOT duplicate domains across classifications

---

### 2. NORMALIZE STRUCTURE

* Ensure all paths follow:
  classification / context / domain
* Flatten unnecessary nesting
* Remove inconsistent grouping

---

### 3. CREATE SKELETON

For each domain:

* create folder if missing
* create README.md

README must include:

* Purpose
* Scope
* Does Not Own
* Inputs
* Outputs
* Invariants
* Dependencies

---

### 4. REMOVE VIOLATIONS

Eliminate:

* business logic in platform-system
* orchestration in core-system
* primitives redefined outside core-system
* policy logic outside control-system
* messaging constructs outside platform-system

---

### 5. ENFORCE NAMING

* use noun-based names only
* no "service", "manager", "engine" in domain names
* no technical leakage
* no domain-specific prefixes in platform-system

---

### 6. VALIDATE

For every domain, confirm:

* belongs to exactly one classification
* respects dependency rules
* does not violate classification constraints
* does not duplicate existing semantics

---

## OUTPUT FORMAT

Return:

1. Final folder tree (cleaned and canonical)
2. List of moved domains (old → new location)
3. List of removed or merged domains
4. Any detected violations
5. Any ambiguous domains requiring decision

---

## STRICT RULES

* Do NOT invent new domains unless necessary
* Do NOT introduce behavior into core-system
* Do NOT allow platform-system to carry meaning
* Do NOT allow control-system to become a workflow engine
* Keep the model minimal and stable

---

## SUCCESS CRITERIA

* clear separation of authority, language, and communication
* zero duplication of primitives
* zero semantic leakage across classifications
* deterministic, enforceable structure
* ready for Phase 2.5 implementation

---

Proceed with refactoring.

---

## Why this prompt works

This isn’t just a “describe structure” prompt—it:

* **forces decisions** (no ambiguity allowed)
* **enforces constraints** (not just suggestions)
* **produces artifacts** (tree + diff + violations)
* **prevents regression** (anti-drift baked in)

---

## How to use it (practically)

You can now:

### 1. Run it against your current repo

→ get a full refactor plan

### 2. Use it incrementally

→ apply per classification:

* control-system first (recommended)
* then platform-system
* then core-system

### 3. Use it as CI validation later

→ reject PRs that violate:

* classification rules
* dependency direction
* naming standards

---

## If you want the next step

The next powerful move is:

> **generate a second prompt that enforces per-domain implementation structure (aggregate, entity, etc.)**

That’s where Phase 2.5 becomes executable.

Say the word and I’ll generate that.
