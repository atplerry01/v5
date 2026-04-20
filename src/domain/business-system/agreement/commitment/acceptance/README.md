# Domain: Acceptance

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the formal acknowledgment by a party that they accept or reject the terms of an agreement. Tracks acceptance lifecycle from pending through accepted or rejected terminal states.

## Aggregate

* **AcceptanceAggregate** — Root aggregate representing a formal acceptance decision on agreement terms.
  * Private constructor; created via `Create(AcceptanceId)` factory method.
  * State transitions via `Accept()` and `Reject()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Pending ──Accept()──> Accepted (terminal)
Pending ──Reject()──> Rejected (terminal)
```

## Value Objects

* **AcceptanceId** — Deterministic identifier (validated non-empty Guid).
* **AcceptanceStatus** — Enum: `Pending`, `Accepted`, `Rejected`.

## Events

* **AcceptanceCreatedEvent** — Raised when a new acceptance is created (status: Pending).
* **AcceptanceAcceptedEvent** — Raised when acceptance is approved.
* **AcceptanceRejectedEvent** — Raised when acceptance is rejected.

## Invariants

* AcceptanceId must not be null/default.
* AcceptanceStatus must be a defined enum value.
* State transitions enforced by specifications (only Pending allows transitions).

## Specifications

* **CanAcceptSpecification** — Validates that status is Pending before accepting.
* **CanRejectSpecification** — Validates that status is Pending before rejecting.

## Errors

* **MissingId** — AcceptanceId is required.
* **AlreadyAccepted** — Cannot transition an already-accepted acceptance.
* **AlreadyRejected** — Cannot transition an already-rejected acceptance.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **AcceptanceService** — Reserved for cross-aggregate coordination within acceptance context.

## Status

**S4 — Invariants + Specifications Complete**
