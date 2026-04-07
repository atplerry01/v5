# economic.guard.md

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: ECON-ES-01 — EVENT SOURCING IS SOURCE OF TRUTH
All economic state changes MUST occur via domain events.
No direct state mutation is allowed outside aggregate methods.

ENFORCEMENT:
- Aggregates must raise events before state change
- No repository-level mutation logic allowed

---

### RULE: ECON-CQRS-01 — READ/WRITE SEPARATION
Write model (aggregates) and read model (projections) MUST be separated.

ENFORCEMENT:
- src/domain MUST NOT depend on projections
- src/projections MUST NOT mutate domain state

---

### RULE: ECON-LEDGER-01 — INVARIANT ENFORCEMENT
Economic aggregates MUST enforce invariant checks BEFORE emitting events.

ENFORCEMENT:
- Ledger must balance
- Allocation/reserve must validate constraints before event emission
