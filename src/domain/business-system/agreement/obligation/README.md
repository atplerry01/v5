# Domain: Obligation

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the duties and responsibilities assigned to parties within an agreement. Tracks obligation lifecycle from pending through fulfillment or breach.

## Aggregate

* **ObligationAggregate** — Root aggregate representing a duty assigned to a party within an agreement.
  * Private constructor; created via `Create(ObligationId)` factory method.
  * State transitions via `Fulfill()` and `Breach()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Pending ──Fulfill()──> Fulfilled (terminal)
Pending ──Breach()───> Breached (terminal)
```

## Value Objects

* **ObligationId** — Deterministic identifier (validated non-empty Guid).
* **ObligationStatus** — Enum: `Pending`, `Fulfilled`, `Breached`.

## Events

* **ObligationCreatedEvent** — Raised when a new obligation is created (status: Pending).
* **ObligationFulfilledEvent** — Raised when obligation is fulfilled.
* **ObligationBreachedEvent** — Raised when obligation is breached.

## Invariants

* ObligationId must not be null/default.
* ObligationStatus must be a defined enum value.
* State transitions enforced by specifications (only Pending allows transitions).

## Specifications

* **CanFulfillSpecification** — Validates that status is Pending before fulfilling.
* **CanBreachSpecification** — Validates that status is Pending before breaching.

## Errors

* **MissingId** — ObligationId is required.
* **AlreadyFulfilled** — Cannot transition an already-fulfilled obligation.
* **AlreadyBreached** — Cannot transition an already-breached obligation.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **ObligationService** — Reserved for cross-aggregate coordination within obligation context.

## Status

**S4 — Invariants + Specifications Complete**
