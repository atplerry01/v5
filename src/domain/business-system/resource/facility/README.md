# Domain: Facility

## Classification

business-system

## Context

resource

## Domain Responsibility

Defines the structure of facility records — physical or logical locations where operations are conducted. This domain models facility identity and context, not usage or scheduling.

## Aggregate

* **FacilityAggregate** — Root aggregate representing a facility location.
  * Private constructor; created via `Create(FacilityId)` factory method.
  * State transitions via `Activate()` and `Close()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via Version property.

## State Model

```
Pending → Active (terminal)
Active → Closed (terminal)
```

## Value Objects

* **FacilityId** — Unique identifier (validated non-empty Guid).
* **FacilityStatus** — Enum: Pending, Active, Closed.

## Events

* **FacilityCreatedEvent** — Raised when a new facility is created.
* **FacilityActivatedEvent** — Raised when a facility is activated.
* **FacilityClosedEvent** — Raised when a facility is closed.

## Invariants

* FacilityId must not be null/default.
* FacilityStatus must be a defined enum value.
* State transitions enforced by specifications.
* Must define location/context.
* Cannot be duplicated.

## Specifications

* **CanActivateFacilitySpecification** — Only Pending facilities can be activated.
* **CanCloseFacilitySpecification** — Only Active facilities can be closed.

## Errors

* **MissingId** — FacilityId is required.
* **AlreadyActive** — Cannot activate an already-active facility.
* **AlreadyClosed** — Cannot close an already-closed facility.
* **InvalidStateTransition** — Generic guard for illegal transitions.

## Domain Services

* **FacilityService** — Reserved for cross-aggregate coordination.

## Lifecycle Pattern

TERMINAL — Once closed, the facility cannot return to a prior state.

## Boundary Statement

This domain defines facility structure only. No scheduling logic, no allocation execution, no external system interaction.

## Status

**S4 — Invariants + Specifications Complete**
