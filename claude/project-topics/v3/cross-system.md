Now we formalize the **interaction contract across the triad**.
This is the piece that prevents your architecture from collapsing back into coupling over time.

What we’re defining here is:

> **How control-system, core-system, and platform-system are allowed to interact—without violating their identities.**

---

# Cross-Classification Interaction Contract

## 1. Canonical Relationship Model

At the highest level:

* **core-system → foundation (language)**
* **platform-system → transport (communication)**
* **control-system → authority (governance)**

### Dependency direction (must never be violated)

```plaintext
core-system        (no dependencies)

platform-system    → depends on core-system only

control-system     → depends on core-system + platform-system

all other systems  → depend on all three (but must not influence them)
```

### Hard rule

> Dependencies only flow **downward**.
> No upward or lateral dependency is allowed.

---

## 2. Interaction Matrix

### control-system ↔ core-system

**Allowed:**

* control-system uses:

  * identifiers
  * time-window / temporal primitives
  * ordering
* control-system embeds core primitives in:

  * policy evaluation
  * audit records
  * configuration resolution
  * access decisions

**Forbidden:**

* core-system depending on control-system
* control-system extending core primitives with behavior
* redefining primitives (e.g., “CustomTimeWindow”)

**Contract:**

> control-system **consumes core-system as-is**, without modification or extension

---

### platform-system ↔ core-system

**Allowed:**

* platform-system uses:

  * correlation-id / causation-id
  * timestamps
  * ordering keys
* embeds core primitives into:

  * command metadata
  * event metadata
  * envelopes
  * routing decisions (structural only)

**Forbidden:**

* modifying primitive semantics
* introducing alternative identity models
* redefining time or ordering

**Contract:**

> platform-system **transports core primitives faithfully**, without interpretation

---

### control-system ↔ platform-system

This is the most sensitive boundary.

**Allowed:**

* control-system plugs into platform via:

  * middleware hooks
  * interceptors
  * validation layers

Examples:

* policy evaluation before command execution
* authorization checks before dispatch
* audit logging on command/event flow
* observability instrumentation

**Forbidden:**

* control-system owning command dispatch
* control-system defining command/event schemas
* control-system mutating message payloads
* platform-system embedding policy or authorization logic

**Contract:**

> control-system may **observe and constrain platform execution**, but must not **own or redefine it**

---

## 3. Integration Flow (End-to-End)

A canonical interaction flow:

### 1. Command creation (platform-system)

* command constructed using:

  * core identifiers
  * temporal primitives
* wrapped in envelope

### 2. Pre-dispatch control (control-system)

* policy evaluation
* authorization check
* configuration resolution
* audit pre-log

### 3. Dispatch (platform-system)

* routing resolved
* command delivered

### 4. Domain execution (business system)

* aggregates process command
* events emitted

### 5. Event propagation (platform-system)

* events enveloped and routed

### 6. Post-execution control (control-system)

* audit logging
* observability signals
* reconciliation hooks (optional)

### 7. Persistence / projection

* core primitives preserved
* platform contracts maintained
* control evidence recorded

---

## 4. Boundary Rules

### Rule 1 — No semantic leakage

* platform-system must not contain business meaning
* control-system must not define communication semantics
* core-system must not contain behavior

---

### Rule 2 — No responsibility overlap

* only control-system governs
* only platform-system communicates
* only core-system defines primitives

---

### Rule 3 — No duplication

* no duplicate identifier models
* no duplicate time models
* no duplicate messaging contracts
* no duplicate policy logic outside control-system

---

### Rule 4 — No inversion

* platform must not depend on control
* core must not depend on anything
* control must not depend on business systems

---

### Rule 5 — No mutation across boundaries

* control cannot mutate message payloads
* platform cannot mutate domain state
* core primitives cannot be mutated after creation

---

## 5. Contract Enforcement Points

These are where violations typically occur—and must be guarded.

### A. Command pipeline

* enforce:

  * policy (control)
  * structure (platform)
  * primitives (core)

---

### B. Event emission

* enforce:

  * schema (platform)
  * identifiers and time (core)
  * audit (control)

---

### C. Configuration resolution

* enforce:

  * configuration rules (control)
  * no protocol leakage (platform)
  * primitive consistency (core)

---

### D. Authorization

* enforce:

  * access-control (control)
  * no embedding in platform
  * no duplication in business domains

---

### E. Observability

* enforce:

  * trace propagation (platform)
  * correlation integrity (core)
  * evidence logging (control)

---

## 6. Anti-Drift Rules

These are the long-term survival rules.

### If something feels “shared”

Ask:

* Is it governance? → control-system
* Is it communication? → platform-system
* Is it primitive language? → core-system

If none:
→ it does NOT belong in the triad

---

### If something “needs to be reused everywhere”

Check:

* Is it immutable and semantic? → core
* Is it structural communication? → platform
* Is it system-wide constraint? → control

Otherwise:
→ it’s likely a misplaced domain concept

---

### If something grows too fast

* control-system growing too fast → becoming god-domain
* platform-system growing too fast → semantic leakage
* core-system growing too fast → abstraction dumping ground

---

## 7. Violation Patterns (watch these closely)

### Pattern 1 — “Smart commands”

Commands containing:

* business decisions
* policy logic

→ violates platform-system

---

### Pattern 2 — “Utility creep”

Core-system gaining:

* helpers
* services
* logic

→ violates core-system

---

### Pattern 3 — “Central brain”

control-system:

* orchestrates everything
* owns workflows

→ violates control-system

---

### Pattern 4 — “Shadow models”

Different:

* IDs
* time models
* schemas

across systems

→ violates core-system

---

### Pattern 5 — “Policy in transport”

Authorization or policy inside:

* message handlers
* routing logic

→ violates separation

---

## 8. Integration Standards

### control-system integration

* via hooks, interceptors, middleware
* never direct ownership of platform flow

---

### platform-system integration

* via contracts and pipelines
* no domain logic injection

---

### core-system integration

* via type usage only
* no extension or override

---

## 9. Certification Checklist

Before accepting any new domain or change:

* Does it belong to exactly one classification?
* Does it violate dependency direction?
* Does it duplicate an existing primitive?
* Does it introduce behavior into core?
* Does it introduce semantics into platform?
* Does it introduce ownership into control?
* Is it testable in isolation?
* Is it replay-safe?
* Is it observable and auditable?

If any answer fails → reject or refactor.

---

## Final synthesis

You now have a **fully defined triad with enforced interaction rules**:

* **core-system**
  → defines *what things are*

* **platform-system**
  → defines *how things move*

* **control-system**
  → defines *what is allowed*

When these three remain clean:

* your domain model stays stable
* your runtime stays deterministic
* your governance stays enforceable
* and your system scales without collapsing into ambiguity

---

If you want to push this further, the next powerful step is:

> **map your existing domains (from the original model) into this triad and identify violations**

That’s where the real architectural cleanup happens.
