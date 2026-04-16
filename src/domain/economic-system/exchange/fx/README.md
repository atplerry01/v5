# Domain: Fx

## Classification
economic-system

## Context
exchange

## Domain Responsibility
Defines FX currency pair structures — the pairings of base and quote currencies for foreign exchange. This domain defines FX structure only and contains no computation logic.

## Aggregate
* **FxAggregate** — Root aggregate representing an FX currency pair definition.
  * Private constructor; created via `Register(FxId, CurrencyPair)` factory method.
  * State transitions via `Activate()` and `Deactivate()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Defined ──Activate()──> Active ──Deactivate()──> Deactivated (terminal)
```

## Value Objects
* **FxId** — Deterministic identifier (validated non-empty Guid).
* **FxStatus** — Enum: `Defined`, `Active`, `Deactivated`.
* **CurrencyPair** — Base and quote currency pairing.

## Events
* **FxPairRegisteredEvent** — Raised when a new FX pair is registered (status: Defined).
* **FxPairActivatedEvent** — Raised when FX pair is activated.
* **FxPairDeactivatedEvent** — Raised when FX pair is deactivated (terminal).

## Invariants
* FxId must not be null/default.
* CurrencyPair must define valid base and quote currencies.
* Must not perform FX computation — structure definition only.
* State transitions enforced by specifications.

## Specifications
* **CanActivateSpecification** — Only Defined FX pairs can be activated.
* **CanDeactivateSpecification** — Only Active FX pairs can be deactivated.

## Errors
* **MissingId** — FxId is required.
* **MissingCurrencyPair** — FX pair must define a valid currency pair.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services
* None — no cross-aggregate coordination required for structural FX pair definitions.

## Lifecycle Pattern
TERMINAL — Once deactivated, an FX pair cannot be reactivated.

## Boundary Statement
This domain defines FX structure only and contains no computation logic.

## Status
**S4 — Invariants + Specifications Complete**
