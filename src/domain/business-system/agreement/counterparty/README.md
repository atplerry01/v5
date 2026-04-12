# Domain: Counterparty

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the external parties involved in an agreement relationship. Tracks counterparty lifecycle from active through suspended or terminated states. Maintains identity reference integrity for each counterparty.

## Aggregate

* **CounterpartyAggregate** — Root aggregate representing an external party in an agreement.
  * Private constructor; created via `Create(CounterpartyId, CounterpartyProfile)` factory method.
  * State transitions via `Suspend()` and `Terminate()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Active ──Suspend()──> Suspended
Active ──Terminate()──> Terminated (terminal)
Suspended ──Terminate()──> Terminated (terminal)
```

## Entities

* **CounterpartyProfile** — Identity profile for the counterparty (IdentityReference, Name). Must have valid identity reference.

## Value Objects

* **CounterpartyId** — Deterministic identifier (validated non-empty Guid).
* **CounterpartyStatus** — Enum: `Active`, `Suspended`, `Terminated`.

## Events

* **CounterpartyCreatedEvent** — Raised when a new counterparty is created (status: Active).
* **CounterpartySuspendedEvent** — Raised when counterparty is suspended.
* **CounterpartyTerminatedEvent** — Raised when counterparty is terminated.

## Invariants

* CounterpartyId must not be null/default.
* CounterpartyProfile must not be null.
* CounterpartyStatus must be a defined enum value.
* Termination is irreversible (enforced by CanTerminateSpecification excluding Terminated state).

## Specifications

* **CanSuspendSpecification** — Validates that status is Active before suspending.
* **CanTerminateSpecification** — Validates that status is Active or Suspended before terminating.

## Errors

* **MissingId** — CounterpartyId is required.
* **MissingProfile** — CounterpartyProfile is required.
* **AlreadySuspended** — Cannot suspend an already-suspended counterparty.
* **AlreadyTerminated** — Cannot transition an already-terminated counterparty.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **CounterpartyService** — Reserved for cross-aggregate coordination within counterparty context.

## Status

**S4 — Invariants + Specifications Complete**
