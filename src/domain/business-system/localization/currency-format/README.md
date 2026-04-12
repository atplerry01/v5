# Domain: CurrencyFormat

## Classification

business-system

## Context

localization

## Domain Responsibility

Defines currency display rules — the symbol, code, and decimal structure for presenting monetary values. This domain defines localization structure only and contains no runtime logic.

## Aggregate

* **CurrencyFormatAggregate** — Root aggregate representing a currency format definition.
  * Private constructor; created via `Create(CurrencyFormatId, CurrencyCode)` factory method.
  * State transitions via `Activate()` and `Deactivate()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## Entities

* None

## State Model

```
Draft ──Activate()──> Active ──Deactivate()──> Deactivated (terminal)
```

## Value Objects

* **CurrencyFormatId** — Deterministic identifier (validated non-empty Guid).
* **CurrencyFormatStatus** — Enum: `Draft`, `Active`, `Deactivated`.
* **CurrencyCode** — Currency code, symbol, and decimal places structure.

## Events

* **CurrencyFormatCreatedEvent** — Raised when a new currency format is created (status: Draft).
* **CurrencyFormatActivatedEvent** — Raised when currency format is activated.
* **CurrencyFormatDeactivatedEvent** — Raised when currency format is deactivated (terminal).

## Invariants

* CurrencyFormatId must not be null/default.
* CurrencyCode must define code, symbol, and valid decimal places.
* CurrencyFormatStatus must be a defined enum value.
* Must not perform formatting — structure definition only.
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Draft currency formats can be activated.
* **CanDeactivateSpecification** — Only Active currency formats can be deactivated.

## Errors

* **MissingId** — CurrencyFormatId is required.
* **InvalidCurrencyCode** — Currency format must define code, symbol, and decimal places.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **DuplicateCurrencyFormat** — Currency format with same code already exists.

## Domain Services

* **CurrencyFormatService** — Reserved for cross-aggregate coordination within currency format context.

## Lifecycle Pattern

TERMINAL — Once deactivated, a currency format cannot be reactivated.

## Boundary Statement

This domain defines localization structure only and contains no runtime logic.

## Status

**S4 — Invariants + Specifications Complete**
