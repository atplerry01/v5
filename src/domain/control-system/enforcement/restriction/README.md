# Domain: Restriction

## Classification
economic-system

## Context
enforcement

## Purpose
Represents partial constraints (operation blocks, limits) applied to a subject. A restriction is narrower than a full system lock — it restricts a specific scope (capital / account / system partial).

## Aggregate(s)
- **RestrictionAggregate** — Event-sourced. Lifecycle: Applied -> Removed (terminal).
  - Invariants: RestrictionId cannot be empty; must reference a subject; cannot update or double-remove a removed restriction.

## Value Objects
- **RestrictionId** — Typed Guid wrapper
- **SubjectId** — Typed Guid wrapper
- **RestrictionStatus** — Enum: Applied, Removed
- **RestrictionScope** — Enum: Capital, Account, System
- **Reason** — Non-empty string wrapper

## Domain Events
- **RestrictionAppliedEvent** — Restriction placed on subject
- **RestrictionUpdatedEvent** — Restriction scope/reason updated
- **RestrictionRemovedEvent** — Restriction removed (terminal)

## Specifications
- **CanUpdateSpecification** — Status == Applied
- **CanRemoveSpecification** — Status == Applied

## Invariants (CRITICAL)
- Every restriction must reference a subject
- Every restriction must include a reason
- Only Applied restrictions can be updated or removed
- Removed restrictions are terminal

## Integration Points
- **sanction** — A sanction of type Restriction may result in a RestrictionAggregate being applied at enforcement time
- **capital** — Runtime bridges translate restriction application into capital-account state changes

## Lifecycle
```
Apply() -> Applied (requires subject, scope, reason)
  Update() -> Applied (scope/reason rewritten)
  Remove() -> Removed (terminal, requires reason)
```
