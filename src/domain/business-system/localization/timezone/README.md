# Domain: Timezone

## Classification

business-system

## Context

localization

## Domain Responsibility

Defines time context — the IANA identifier and UTC offset that determines temporal presentation. Timezone definitions are immutable once activated. This domain defines localization structure only and contains no runtime logic.

## Aggregate

* **TimezoneAggregate** — Root aggregate representing a timezone definition.
  * Private constructor; created via `Create(TimezoneId, TimezoneOffset)` factory method.
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

* **TimezoneId** — Deterministic identifier (validated non-empty Guid).
* **TimezoneStatus** — Enum: `Draft`, `Active`, `Deactivated`.
* **TimezoneOffset** — IANA timezone identifier and UTC offset in minutes.

## Events

* **TimezoneCreatedEvent** — Raised when a new timezone is created (status: Draft).
* **TimezoneActivatedEvent** — Raised when timezone is activated.
* **TimezoneDeactivatedEvent** — Raised when timezone is deactivated (terminal).

## Invariants

* TimezoneId must not be null/default.
* TimezoneOffset must define valid IANA identifier and UTC offset within bounds (-720 to +840 minutes).
* TimezoneStatus must be a defined enum value.
* Timezone is immutable once created — offset cannot change.
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Draft timezones can be activated.
* **CanDeactivateSpecification** — Only Active timezones can be deactivated.

## Errors

* **MissingId** — TimezoneId is required.
* **InvalidTimezoneOffset** — Timezone must define valid IANA identifier and UTC offset.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **DuplicateTimezone** — Timezone with same IANA identifier already exists.

## Domain Services

* **TimezoneService** — Reserved for cross-aggregate coordination within timezone context.

## Lifecycle Pattern

TERMINAL — Once deactivated, a timezone cannot be reactivated.

## Boundary Statement

This domain defines localization structure only and contains no runtime logic.

## Status

**S4 — Invariants + Specifications Complete**
