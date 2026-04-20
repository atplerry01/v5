# Domain: Renewal

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the extension or continuation of an agreement beyond its initial term. Each renewal is tied to a source (contract or term) and tracks lifecycle from pending through renewal or expiry.

## Aggregate

* **RenewalAggregate** — Root aggregate representing a renewal of an agreement or term.
  * Private constructor; created via `Create(RenewalId, RenewalSourceId)` factory method.
  * State transitions via `Renew()` and `Expire()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Pending ──Renew()──> Renewed (terminal)
Pending ──Expire()──> Expired (terminal)
```

## Value Objects

* **RenewalId** — Deterministic identifier (validated non-empty Guid).
* **RenewalSourceId** — Reference to the source contract or term being renewed.
* **RenewalStatus** — Enum: `Pending`, `Renewed`, `Expired`.

## Events

* **RenewalCreatedEvent** — Raised when a new renewal is created (status: Pending, includes SourceId).
* **RenewalRenewedEvent** — Raised when the renewal is executed.
* **RenewalExpiredEvent** — Raised when the renewal window expires.

## Invariants

* RenewalId must not be null/default.
* RenewalSourceId must not be null/default.
* RenewalStatus must be a defined enum value.
* Cannot renew an expired entity.
* State transitions enforced by specifications (only Pending allows transitions).

## Specifications

* **CanRenewSpecification** — Validates that status is Pending before renewing.
* **CanExpireRenewalSpecification** — Validates that status is Pending before expiring.

## Errors

* **MissingId** — RenewalId is required.
* **MissingSourceId** — RenewalSourceId is required.
* **AlreadyRenewed** — Cannot renew an already-renewed renewal.
* **AlreadyExpired** — Cannot act on an expired renewal.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **RenewalService** — Reserved for cross-aggregate coordination within renewal context.

## Status

**S4 — Invariants + Specifications Complete**
