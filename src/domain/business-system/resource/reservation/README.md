# Domain: Reservation

## Classification

business-system

## Context

resource

## Domain Responsibility

Defines the allocation of resources through reservations. Tracks reservation lifecycle from pending through confirmation, release, and cancellation. This domain defines resource usage contracts, not execution.

## Aggregate

* **ReservationAggregate** — Root aggregate representing a resource reservation.
  * Private constructor; created via `Create(ReservationId, ResourceReference, ReservedCapacity)` factory method.
  * State transitions via `Confirm()`, `Release()`, and `Cancel()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model (REVERSIBLE)

```
Pending ──Confirm()──> Confirmed ──Release()──> Released
Pending ──Cancel()──> Cancelled
```

## Value Objects

* **ReservationId** — Deterministic identifier (validated non-empty Guid).
* **ReservationStatus** — Enum: `Pending`, `Confirmed`, `Released`, `Cancelled`.
* **ResourceReference** — Reference to the resource being reserved (validated non-empty Guid).
* **ReservedCapacity** — Quantity of resource capacity reserved (must be > 0).

## Events

* **ReservationCreatedEvent** — Raised when a new reservation is created (status: Pending).
* **ReservationConfirmedEvent** — Raised when reservation is confirmed.
* **ReservationReleasedEvent** — Raised when reservation is released.
* **ReservationCancelledEvent** — Raised when reservation is cancelled.

## Invariants

* ReservationId must not be null/default.
* Must reference a resource (ResourceReference must not be default).
* Reserved capacity must be greater than zero.
* Cannot reserve more than available capacity.
* State transitions enforced by specifications.

## Specifications

* **CanConfirmSpecification** — Only Pending reservations can be confirmed.
* **CanReleaseSpecification** — Only Confirmed reservations can be released.
* **CanCancelSpecification** — Only Pending reservations can be cancelled.

## Errors

* **MissingId** — ReservationId is required.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **ResourceReferenceRequired** — Reservation must reference a resource.
* **CapacityMustBePositive** — Reserved capacity must be > 0.
* **ExceedsAvailableCapacity** — Requested capacity exceeds available.

## Domain Services

* **ReservationService** — Reserved for cross-aggregate coordination within reservation context.

## Boundary Statement

This domain defines resource allocation contracts only. No scheduling logic, no allocation execution, no time-based execution, no background processes.

## Status

**S4 — Invariants + Specifications Complete**
