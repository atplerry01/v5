# Domain: Validity

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the rules determining when and under what conditions an agreement is valid. Tracks validity lifecycle from valid through invalidated or expired terminal states. Enforces that expiry and invalidation can only occur from a valid state.

## Aggregate

* **ValidityAggregate** — Root aggregate representing the validity rules for an agreement.
  * Private constructor; created via `Create(ValidityId)` factory method.
  * State transitions via `Invalidate()` and `Expire()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Valid ──Invalidate()──> Invalid (terminal)
Valid ──Expire()──> Expired (terminal)
```

## Value Objects

* **ValidityId** — Deterministic identifier (validated non-empty Guid).
* **ValidityStatus** — Enum: `Valid`, `Invalid`, `Expired`.

## Events

* **ValidityCreatedEvent** — Raised when a new validity is created (status: Valid).
* **ValidityInvalidatedEvent** — Raised when validity conditions are no longer met.
* **ValidityExpiredEvent** — Raised when validity period has elapsed.

## Invariants

* ValidityId must not be null/default.
* ValidityStatus must be a defined enum value.
* State transitions enforced by specifications (only Valid allows transitions).

## Specifications

* **CanInvalidateSpecification** — Validates that status is Valid before invalidating.
* **CanExpireSpecification** — Validates that status is Valid before expiring.

## Errors

* **MissingId** — ValidityId is required.
* **AlreadyInvalid** — Cannot invalidate an already-invalid validity.
* **AlreadyExpired** — Cannot expire an already-expired validity.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **ValidityService** — Reserved for cross-aggregate coordination within validity context.

## Status

**S4 — Invariants + Specifications Complete**
