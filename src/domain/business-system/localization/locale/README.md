# Domain: Locale

## Classification

business-system

## Context

localization

## Domain Responsibility

Defines regional identity — the combination of language and region that determines content presentation context. This domain defines localization structure only and contains no runtime logic.

## Aggregate

* **LocaleAggregate** — Root aggregate representing a locale definition.
  * Private constructor; created via `Create(LocaleId, LocaleCode)` factory method.
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

* **LocaleId** — Deterministic identifier (validated non-empty Guid).
* **LocaleStatus** — Enum: `Draft`, `Active`, `Deactivated`.
* **LocaleCode** — Language + Region pair defining regional identity.

## Events

* **LocaleCreatedEvent** — Raised when a new locale is created (status: Draft).
* **LocaleActivatedEvent** — Raised when locale is activated.
* **LocaleDeactivatedEvent** — Raised when locale is deactivated (terminal).

## Invariants

* LocaleId must not be null/default.
* LocaleCode must define both language and region.
* LocaleStatus must be a defined enum value.
* Locale must be unique (language + region combination).
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Draft locales can be activated.
* **CanDeactivateSpecification** — Only Active locales can be deactivated.

## Errors

* **MissingId** — LocaleId is required.
* **InvalidLocaleCode** — Locale must define both language and region.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **DuplicateLocale** — Locale with same language+region already exists.

## Domain Services

* **LocaleService** — Reserved for cross-aggregate coordination within locale context.

## Lifecycle Pattern

TERMINAL — Once deactivated, a locale cannot be reactivated.

## Boundary Statement

This domain defines localization structure only and contains no runtime logic.

## Status

**S4 — Invariants + Specifications Complete**
