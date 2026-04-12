# Domain: Preference

## Classification

business-system

## Context

notification

## Domain Responsibility

Defines user notification preferences — enforceable choices about how and when a user receives notifications. This domain defines notification structure only and contains no execution logic.

## Aggregate

* **PreferenceAggregate** — Root aggregate representing a notification preference definition.
  * Private constructor; created via `Create(PreferenceId, PreferenceRule)` factory method.
  * State transitions via `Enforce()`, `Suspend()`, and `Reinstate()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## Entities

* None

## State Model

```
Draft ──Enforce()──> Enforced ──Suspend()──> Suspended ──Reinstate()──> Enforced
```

## Value Objects

* **PreferenceId** — Deterministic identifier (validated non-empty Guid).
* **PreferenceStatus** — Enum: `Draft`, `Enforced`, `Suspended`.
* **PreferenceRule** — Owner reference and rule name defining the preference constraint.

## Events

* **PreferenceDefinedEvent** — Raised when a new preference is defined (status: Draft).
* **PreferenceEnforcedEvent** — Raised when preference is enforced.
* **PreferenceSuspendedEvent** — Raised when preference is suspended.
* **PreferenceReinstatedEvent** — Raised when preference is reinstated from suspension.

## Invariants

* PreferenceId must not be null/default.
* PreferenceRule must define an owner reference and rule name.
* PreferenceStatus must be a defined enum value.
* Must not execute notification filtering — preference definition only.
* State transitions enforced by specifications.

## Specifications

* **CanEnforceSpecification** — Only Draft preferences can be enforced.
* **CanSuspendSpecification** — Only Enforced preferences can be suspended.
* **CanReinstateSpecification** — Only Suspended preferences can be reinstated.

## Errors

* **MissingId** — PreferenceId is required.
* **InvalidRule** — Preference must define a valid rule with owner reference.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services

* **PreferenceService** — Reserved for cross-aggregate coordination within preference context.

## Lifecycle Pattern

REVERSIBLE — Preferences can be suspended and reinstated.

## Boundary Statement

This domain defines notification structure only and contains no execution logic.

## Status

**S4 — Invariants + Specifications Complete**
