# Domain: Process

## Classification
economic-system

## Context
reconciliation

## Domain Responsibility
Defines reconciliation process lifecycle — the structured progression from trigger through comparison to resolution. This domain defines reconciliation lifecycle only and contains no reconciliation algorithms.

## Aggregate
* **ProcessAggregate** — Root aggregate representing a reconciliation process.
  * Private constructor; created via `Trigger(ProcessId, SourceReference, SourceReference, Timestamp)` factory method.
  * State transitions via `MarkMatched()`, `MarkMismatched()`, and `Resolve()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Pending ──MarkMatched()────> Matched ──Resolve()──> Resolved (terminal)
Pending ──MarkMismatched()─> Mismatched ──Resolve()──> Resolved (terminal)
```

## Value Objects
* **ProcessId** — Deterministic identifier (validated non-empty Guid).
* **ReconciliationStatus** — Enum: `Pending`, `InProgress`, `Matched`, `Mismatched`, `Resolved`.
* **SourceReference** — Typed reference to a reconciliation source.

## Events
* **ReconciliationTriggeredEvent** — Raised when reconciliation is triggered (status: Pending).
* **ReconciliationMatchedEvent** — Raised when sources match.
* **ReconciliationMismatchedEvent** — Raised when sources do not match.
* **ReconciliationResolvedEvent** — Raised when process is resolved (terminal).

## Invariants
* ProcessId must not be null/default.
* LedgerReference must not be null/default.
* ObservedReference must not be null/default.
* ReconciliationStatus must be a defined enum value.
* Must not perform reconciliation — lifecycle definition only.
* State transitions enforced by specifications.

## Specifications
* **CanMatchSpecification** — Only Pending or InProgress processes can produce match results.
* **CanResolveSpecification** — Only Matched or Mismatched processes can be resolved.

## Errors
* **MissingId** — ProcessId is required.
* **MissingLedgerReference** — Must reference a ledger source.
* **MissingObservedReference** — Must reference an observed source.
* **InvalidStateTransition** — Guard for illegal status transitions.
* **NoResultProduced** — Must produce a result before resolution.

## Domain Services
* **ProcessService** — Reserved for cross-aggregate coordination within process context.

## Lifecycle Pattern
SEQUENTIAL — Process moves through a defined sequence: Pending -> Matched/Mismatched -> Resolved.

## Boundary Statement
This domain defines reconciliation lifecycle only and contains no reconciliation algorithms.

## Status
**S4 — Invariants + Specifications Complete**
