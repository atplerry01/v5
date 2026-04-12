# Domain: Rate

## Classification
economic-system

## Context
exchange

## Domain Responsibility
Defines exchange rate structures — the recorded rate values for currency pairs at specific points in time. This domain defines rate structure only and contains no computation logic.

## Aggregate
* **ExchangeRateAggregate** — Root aggregate representing an exchange rate definition.
  * Private constructor; created via `DefineRate(RateId, Currency, Currency, decimal, Timestamp, int)` factory method.
  * State transitions via `Activate()` and `Expire()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Defined ──Activate()──> Active ──Expire()──> Expired (terminal)
```

## Value Objects
* **RateId** — Deterministic identifier (validated non-empty Guid).
* **ExchangeRateStatus** — Enum: `Defined`, `Active`, `Expired`.

## Events
* **ExchangeRateDefinedEvent** — Raised when a new rate is defined (status: Defined).
* **ExchangeRateActivatedEvent** — Raised when rate is activated.
* **ExchangeRateExpiredEvent** — Raised when rate expires (terminal).

## Invariants
* RateId must not be null/default.
* RateValue must be greater than zero.
* Version must be greater than zero.
* Must not compute FX values — rate structure definition only.
* State transitions enforced by specifications.

## Specifications
* **CanActivateSpecification** — Only Defined rates with positive value can be activated.
* **CanExpireSpecification** — Only Active rates can expire.

## Errors
* **InvalidRateValue** — Rate value must be positive.
* **DuplicateActiveRate** — Duplicate active rate guard.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services
* **RateImmutabilityService** — Validates that active/expired rates have not been modified.

## Lifecycle Pattern
TERMINAL — Once expired, a rate cannot be reactivated.

## Boundary Statement
This domain defines rate structure only and contains no computation logic.

## Status
**S4 — Invariants + Specifications Complete**
