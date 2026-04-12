# Domain: SPV

## Classification
structural-system

## Context
cluster

## Domain Responsibility
This domain defines Special Purpose Vehicle (SPV) structure within a cluster and contains no execution or financial logic.

## Aggregate

* **SpvAggregate** -- Root aggregate representing an SPV entity within a cluster.
  * Private constructor; created via `Create(SpvId, SpvDescriptor)` factory method.
  * State transitions via `Activate()`, `Suspend()`, and `Close()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Create() -> Created --Activate()--> Active --Suspend()--> Suspended --Close()--> Closed (terminal)
```

## Value Objects

* **SpvId** -- Deterministic identifier (validated non-empty Guid).
* **SpvStatus** -- Enum: `Created`, `Active`, `Suspended`, `Closed`.
* **SpvType** -- Enum: `Operating`, `Brand`, `Hybrid`.
* **SpvDescriptor** -- Record struct with `ClusterReference` (Guid, non-empty), `SpvName` (string, non-empty), and `SpvType` (validated enum).

## Events

* **SpvCreatedEvent(SpvId, SpvDescriptor)** -- SPV created with initial descriptor including SPVType.
* **SpvActivatedEvent(SpvId)** -- SPV transitioned to Active.
* **SpvSuspendedEvent(SpvId)** -- SPV transitioned to Suspended.
* **SpvClosedEvent(SpvId)** -- SPV transitioned to Closed (terminal).

## Invariants

* SpvId must not be null/default.
* SpvDescriptor must not be default.
* SpvDescriptor must include a valid SpvType (Operating, Brand, or Hybrid).
* SpvStatus must be a defined enum value.
* State transitions enforced by specifications (Created allows Activate; Active allows Suspend; Suspended allows Close).

## Specifications

* **CanActivateSpecification** -- Validates that status is Created before activating.
* **CanSuspendSpecification** -- Validates that status is Active before suspending.
* **CanCloseSpecification** -- Validates that status is Suspended before closing.

## Errors

* **MissingId** -- SpvId is required.
* **MissingDescriptor** -- SpvDescriptor is required.
* **InvalidStateTransition** -- Guard for illegal status transitions (includes current status and attempted action).

## Domain Services

* **SpvService** -- Reserved for cross-aggregate coordination within SPV context.

## Status

**S4 -- Invariants + Specifications Complete**
