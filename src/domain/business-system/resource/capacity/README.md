# Domain: Capacity

## Classification

business-system

## Context

resource

## Domain Responsibility

Defines the structure of resource capacity records — the limits of available capacity for business resources. This domain models capacity definition and constraints, not usage or consumption.

## Aggregate

* **CapacityAggregate** — Root aggregate representing a resource capacity definition.
  * Private constructor; created via `Create(CapacityId, CapacityLimit)` factory method.
  * State transitions via `Activate()`, `Suspend()`, and `Reinstate()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via Version property.

## State Model

```
Pending → Active
Active → Suspended
Suspended → Active (via Reinstate)
```

## Value Objects

* **CapacityId** — Unique identifier (validated non-empty Guid).
* **CapacityStatus** — Enum: Pending, Active, Suspended.
* **CapacityLimit** — Non-negative integer representing the capacity limit.

## Events

* **CapacityCreatedEvent** — Raised when a new capacity is created (includes limit).
* **CapacityActivatedEvent** — Raised when capacity is activated.
* **CapacitySuspendedEvent** — Raised when capacity is suspended.
* **CapacityReinstatedEvent** — Raised when capacity is reinstated from suspension.

## Invariants

* CapacityId must not be null/default.
* CapacityLimit must be non-negative.
* CapacityLimit must be defined (not default).
* CapacityStatus must be a defined enum value.
* State transitions enforced by specifications.

## Specifications

* **CanActivateCapacitySpecification** — Only Pending capacity can be activated.
* **CanSuspendCapacitySpecification** — Only Active capacity can be suspended.
* **CanReinstateCapacitySpecification** — Only Suspended capacity can be reinstated.

## Errors

* **MissingId** — CapacityId is required.
* **MissingLimit** — Capacity must define a limit.
* **AlreadyActive** — Cannot activate already-active capacity.
* **AlreadySuspended** — Cannot suspend already-suspended capacity.
* **InvalidStateTransition** — Generic guard for illegal transitions.

## Domain Services

* **CapacityService** — Reserved for cross-aggregate coordination.

## Lifecycle Pattern

REVERSIBLE — Capacity can transition between Active and Suspended states. Suspension is reversible via reinstatement.

## Boundary Statement

This domain defines capacity limits only. No capacity calculation outside domain, no scheduling logic, no allocation execution, no external system interaction.

## Status

**S4 — Invariants + Specifications Complete**
