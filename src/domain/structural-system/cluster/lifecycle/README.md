# Domain: Lifecycle

## Classification
structural-system

## Context
cluster

## Domain Responsibility
This domain defines lifecycle state definitions within a cluster and contains no execution or orchestration logic.

## Aggregate

* **LifecycleAggregate** -- Root aggregate representing a lifecycle definition within a cluster.
  * Private constructor; created via `Define(LifecycleId, LifecycleDescriptor)` factory method.
  * State transitions via `Transition()` and `Complete()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Define() -> Defined --Transition()--> Transitioned --Complete()--> Completed (terminal)
```

## Value Objects

* **LifecycleId** -- Deterministic identifier (validated non-empty Guid).
* **LifecycleStatus** -- Enum: `Defined`, `Transitioned`, `Completed`.
* **LifecycleDescriptor** -- Record struct with `ClusterReference` (Guid, non-empty) and `LifecycleName` (string, non-empty).

## Events

* **LifecycleDefinedEvent(LifecycleId, LifecycleDescriptor)** -- Lifecycle created with initial descriptor.
* **LifecycleTransitionedEvent(LifecycleId)** -- Lifecycle transitioned to active phase.
* **LifecycleCompletedEvent(LifecycleId)** -- Lifecycle completed (terminal).

## Invariants

* LifecycleId must not be null/default.
* LifecycleDescriptor must not be default.
* LifecycleStatus must be a defined enum value.
* State transitions enforced by specifications (Defined allows Transition; Transitioned allows Complete).

## Specifications

* **CanTransitionSpecification** -- Validates that status is Defined before transitioning.
* **CanCompleteSpecification** -- Validates that status is Transitioned before completing.

## Errors

* **MissingId** -- LifecycleId is required.
* **MissingDescriptor** -- LifecycleDescriptor is required.
* **InvalidStateTransition** -- Guard for illegal status transitions (includes current status and attempted action).

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Status

**S4 -- Invariants + Specifications Complete**
