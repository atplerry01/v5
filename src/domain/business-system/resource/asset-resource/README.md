# Domain: AssetResource

## Classification

business-system

## Context

resource

## Domain Responsibility

Defines the structure of assignable resource records — physical or digital assets that can be referenced by other domains. This domain models resource identity and classification, not usage or allocation.

## Aggregate

* **AssetResourceAggregate** — Root aggregate representing an assignable resource.
  * Private constructor; created via `Create(AssetResourceId)` factory method.
  * State transitions via `Activate()` and `Decommission()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via Version property.

## State Model

```
Pending → Active (terminal)
Active → Decommissioned (terminal)
```

## Value Objects

* **AssetResourceId** — Unique identifier (validated non-empty Guid).
* **AssetResourceStatus** — Enum: Pending, Active, Decommissioned.

## Events

* **AssetResourceCreatedEvent** — Raised when a new asset resource is created.
* **AssetResourceActivatedEvent** — Raised when an asset resource is activated.
* **AssetResourceDecommissionedEvent** — Raised when an asset resource is decommissioned.

## Invariants

* AssetResourceId must not be null/default.
* AssetResourceStatus must be a defined enum value.
* State transitions enforced by specifications.
* Must have identity and classification.
* Cannot exist without type.

## Specifications

* **CanActivateAssetResourceSpecification** — Only Pending assets can be activated.
* **CanDecommissionAssetResourceSpecification** — Only Active assets can be decommissioned.

## Errors

* **MissingId** — AssetResourceId is required.
* **AlreadyActive** — Cannot activate an already-active resource.
* **AlreadyDecommissioned** — Cannot decommission an already-decommissioned resource.
* **InvalidStateTransition** — Generic guard for illegal transitions.

## Domain Services

* **AssetResourceService** — Reserved for cross-aggregate coordination.

## Lifecycle Pattern

TERMINAL — Once decommissioned, the asset resource cannot return to a prior state.

## Boundary Statement

This domain defines resource structure only. No scheduling logic, no allocation execution, no external system interaction.

## Status

**S4 — Invariants + Specifications Complete**
