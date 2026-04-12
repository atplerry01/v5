# Domain: Term

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the specific conditions, durations, and parameters governing an agreement. Each term has a defined duration and follows a lifecycle from draft through activation to expiry.

## Aggregate

* **TermAggregate** — Root aggregate representing a specific time-bound condition governing an agreement.
  * Private constructor; created via `Create(TermId, TermDuration)` factory method.
  * State transitions via `Activate()` and `Expire()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Draft ──Activate()──> Active ──Expire()──> Expired (terminal)
```

## Value Objects

* **TermId** — Deterministic identifier (validated non-empty Guid).
* **TermDuration** — Immutable duration value (validated positive integer, in days).
* **TermStatus** — Enum: `Draft`, `Active`, `Expired`.

## Events

* **TermCreatedEvent** — Raised when a new term is created (status: Draft, includes Duration).
* **TermActivatedEvent** — Raised when term is activated.
* **TermExpiredEvent** — Raised when term expires.

## Invariants

* TermId must not be null/default.
* TermDuration must not be default (must have positive duration).
* TermStatus must be a defined enum value.
* Expiry must follow activation (enforced by specification).

## Specifications

* **IsValidTermDurationSpecification** — Validates that duration is positive.
* **CanActivateTermSpecification** — Only Draft terms can be activated.
* **CanExpireTermSpecification** — Only Active terms can expire.

## Errors

* **MissingId** — TermId is required.
* **InvalidDuration** — Term duration must be greater than zero.
* **AlreadyActive** — Cannot activate an already-active term.
* **AlreadyExpired** — Cannot act on an expired term.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **TermService** — Reserved for cross-aggregate coordination within term context.

## Status

**S4 — Invariants + Specifications Complete**
