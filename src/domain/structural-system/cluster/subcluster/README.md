# Domain: Subcluster

## Classification
structural-system

## Context
cluster

## Domain Responsibility
This domain defines nested cluster hierarchy structure only and contains no orchestration or execution logic.

## Aggregate

* **SubclusterAggregate** -- Root aggregate representing a nested cluster within a parent cluster.
  * Private constructor; created via `Define(SubclusterId, SubclusterDescriptor)` factory method.
  * State transitions via `Activate()` and `Archive()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Define() -> Defined --Activate()--> Active --Archive()--> Archived (terminal)
```

## Value Objects

* **SubclusterId** -- Deterministic identifier (validated non-empty Guid).
* **SubclusterStatus** -- Enum: `Defined`, `Active`, `Archived`.
* **SubclusterDescriptor** -- Record struct with `ParentClusterReference` (Guid, non-empty) and `SubclusterName` (string, non-empty).

## Events

* **SubclusterDefinedEvent(SubclusterId, SubclusterDescriptor)** -- Subcluster created with initial descriptor.
* **SubclusterActivatedEvent(SubclusterId)** -- Subcluster transitioned to Active.
* **SubclusterArchivedEvent(SubclusterId)** -- Subcluster transitioned to Archived (terminal).

## Invariants

* SubclusterId must not be null/default.
* SubclusterDescriptor must not be default.
* SubclusterStatus must be a defined enum value.
* State transitions enforced by specifications (Defined allows Activate; Active allows Archive).

## Specifications

* **CanActivateSpecification** -- Validates that status is Defined before activating.
* **CanArchiveSpecification** -- Validates that status is Active before archiving.

## Errors

* **MissingId** -- SubclusterId is required.
* **MissingDescriptor** -- SubclusterDescriptor is required.
* **InvalidStateTransition** -- Guard for illegal status transitions (includes current status and attempted action).

## Domain Services

* **SubclusterService** -- Reserved for cross-aggregate coordination within subcluster context.

## Notes

* Cross-domain references use raw Guid to maintain domain isolation.
* ParentClusterReference is a raw Guid, not a typed ClusterId.

## Status

**S4 -- Invariants + Specifications Complete**
