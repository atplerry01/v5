# Domain: Classification

## Classification

structural-system

## Context

structure

## Domain Responsibility

This domain defines structural classification categories only and contains no execution logic.

## Aggregate

* **ClassificationAggregate** -- Root aggregate representing a structural classification category.
  * Private constructor; created via `Define(ClassificationId, ClassificationDescriptor)` factory method.
  * State transitions via `Activate()` and `Deprecate()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Defined --Activate()--> Active --Deprecate()--> Deprecated (terminal)
```

## Value Objects

* **ClassificationId** -- Deterministic identifier (validated non-empty Guid).
* **ClassificationStatus** -- Enum: `Defined`, `Active`, `Deprecated`.
* **ClassificationDescriptor** -- Record struct with `ClassificationName` (string, non-empty) and `ClassificationCategory` (string, non-empty).

## Events

* **ClassificationDefinedEvent** -- Raised when a new classification is defined (status: Defined).
* **ClassificationActivatedEvent** -- Raised when classification is activated.
* **ClassificationDeprecatedEvent** -- Raised when classification is deprecated.

## Invariants

* ClassificationId must not be null/default.
* ClassificationDescriptor must not be default.
* ClassificationStatus must be a defined enum value.
* State transitions enforced by specifications (Defined allows Activate; Active allows Deprecate).

## Specifications

* **CanActivateSpecification** -- Validates that status is Defined before activating.
* **CanDeprecateSpecification** -- Validates that status is Active before deprecating.

## Errors

* **MissingId** -- ClassificationId is required.
* **MissingDescriptor** -- ClassificationDescriptor is required.
* **InvalidStateTransition** -- Guard for illegal status transitions (includes current status and attempted action).

## Domain Services

* **ClassificationService** -- Reserved for cross-aggregate coordination within classification context.

## Status

**S4 -- Invariants + Specifications Complete**
