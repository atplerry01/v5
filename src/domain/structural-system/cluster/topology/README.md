# Domain: Topology

## Classification
structural-system

## Context
cluster

## Domain Responsibility
This domain defines relationship topology within a cluster and contains no execution logic. Topology defines relationships (NOT execution).

## Aggregate

* **TopologyAggregate** -- Root aggregate representing a topology definition within a cluster.
  * Private constructor; created via `Define(TopologyId, TopologyDescriptor)` factory method.
  * State transitions via `Validate()` and `Lock()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Define() -> Defined --Validate()--> Validated --Lock()--> Locked (terminal)
```

## Value Objects

* **TopologyId** -- Deterministic identifier (validated non-empty Guid).
* **TopologyStatus** -- Enum: `Defined`, `Validated`, `Locked`.
* **TopologyDescriptor** -- Record struct with `ClusterReference` (Guid, non-empty) and `TopologyName` (string, non-empty).

## Events

* **TopologyDefinedEvent(TopologyId, TopologyDescriptor)** -- Topology created with initial descriptor.
* **TopologyValidatedEvent(TopologyId)** -- Topology validated and relationships confirmed.
* **TopologyLockedEvent(TopologyId)** -- Topology locked (terminal, no further changes).

## Invariants

* TopologyId must not be null/default.
* TopologyDescriptor must not be default.
* TopologyStatus must be a defined enum value.
* State transitions enforced by specifications (Defined allows Validate; Validated allows Lock).

## Specifications

* **CanValidateSpecification** -- Validates that status is Defined before validation.
* **CanLockSpecification** -- Validates that status is Validated before locking.

## Errors

* **MissingId** -- TopologyId is required.
* **MissingDescriptor** -- TopologyDescriptor is required.
* **InvalidStateTransition** -- Guard for illegal status transitions (includes current status and attempted action).

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Notes

* Topology defines structural relationships between cluster components.
* No execution, orchestration, or workflow logic is permitted.

## Status

**S4 -- Invariants + Specifications Complete**
