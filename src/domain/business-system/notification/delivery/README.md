# Domain: Delivery

## Classification

business-system

## Context

notification

## Domain Responsibility

Defines delivery contracts — the agreements for how notifications are delivered through channels. This domain defines notification structure only and contains no execution logic.

## Aggregate

* **DeliveryAggregate** — Root aggregate representing a delivery contract definition.
  * Private constructor; created via `Create(DeliveryId, DeliveryContract)` factory method.
  * State transitions via `Activate()`, `Suspend()`, and `Resume()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## Entities

* None

## State Model

```
Draft ──Activate()──> Active ──Suspend()──> Suspended ──Resume()──> Active
```

## Value Objects

* **DeliveryId** — Deterministic identifier (validated non-empty Guid).
* **DeliveryStatus** — Enum: `Draft`, `Active`, `Suspended`.
* **DeliveryContract** — Channel reference and contract name defining the delivery agreement.

## Events

* **DeliveryDefinedEvent** — Raised when a new delivery contract is defined (status: Draft).
* **DeliveryActivatedEvent** — Raised when delivery contract is activated.
* **DeliverySuspendedEvent** — Raised when delivery contract is suspended.
* **DeliveryResumedEvent** — Raised when delivery contract is resumed from suspension.

## Invariants

* DeliveryId must not be null/default.
* DeliveryContract must reference a valid channel and define a contract name.
* DeliveryStatus must be a defined enum value.
* Must not execute delivery — contract definition only.
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Draft deliveries can be activated.
* **CanSuspendSpecification** — Only Active deliveries can be suspended.
* **CanResumeSpecification** — Only Suspended deliveries can be resumed.

## Errors

* **MissingId** — DeliveryId is required.
* **InvalidContract** — Delivery must define a valid contract with channel reference.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services

* **DeliveryService** — Reserved for cross-aggregate coordination within delivery context.

## Lifecycle Pattern

REVERSIBLE — Delivery contracts can be suspended and resumed.

## Boundary Statement

This domain defines notification structure only and contains no execution logic.

## Status

**S4 — Invariants + Specifications Complete**
