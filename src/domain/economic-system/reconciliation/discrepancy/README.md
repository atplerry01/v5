# Domain: Discrepancy

## Classification
economic-system

## Context
reconciliation

## Domain Responsibility
Defines discrepancy structures — the recorded mismatches between expected and actual values in reconciliation. This domain defines mismatch structure only and contains no reconciliation algorithms.

## Aggregate
* **DiscrepancyAggregate** — Root aggregate representing a detected discrepancy.
  * Private constructor; created via `Detect(DiscrepancyId, ProcessReference, DiscrepancySource, Amount, Amount, Amount, Timestamp)` factory method.
  * State transitions via `Investigate()` and `Resolve()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Open ──Investigate()──> Investigating ──Resolve()──> Resolved (terminal)
Open ──Resolve()──────────────────────> Resolved (terminal)
```

## Value Objects
* **DiscrepancyId** — Deterministic identifier (validated non-empty Guid).
* **DiscrepancyStatus** — Enum: `Open`, `Investigating`, `Resolved`.
* **DiscrepancySource** — Enum: `Projection`, `ExternalSystem`.
* **ProcessReference** — Typed reference to reconciliation process.

## Events
* **DiscrepancyDetectedEvent** — Raised when a discrepancy is detected (status: Open).
* **DiscrepancyInvestigatedEvent** — Raised when investigation begins.
* **DiscrepancyResolvedEvent** — Raised when discrepancy is resolved (terminal).

## Invariants
* DiscrepancyId must not be null/default.
* ProcessReference must not be null/default.
* DiscrepancyStatus must be a defined enum value.
* Must not perform reconciliation — mismatch structure definition only.
* State transitions enforced by specifications.

## Specifications
* **CanInvestigateSpecification** — Only Open discrepancies can be investigated.
* **CanResolveSpecification** — Open or Investigating discrepancies can be resolved.

## Errors
* **MissingId** — DiscrepancyId is required.
* **MissingProcessReference** — Must reference a reconciliation process.
* **EmptyResolution** — Resolution description must not be empty.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services
* **DiscrepancyService** — Reserved for cross-aggregate coordination within discrepancy context.

## Lifecycle Pattern
TERMINAL — Once resolved, a discrepancy cannot be reopened.

## Boundary Statement
This domain defines mismatch structure only and contains no reconciliation algorithms.

## Status
**S4 — Invariants + Specifications Complete**
