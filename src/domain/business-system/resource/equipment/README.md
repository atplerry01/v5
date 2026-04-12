# Domain: Equipment

## Classification

business-system

## Context

resource

## Domain Responsibility

Defines the structure of equipment records — usable operational assets that belong to a facility or resource group. This domain models equipment identity and lifecycle, not usage or scheduling.

## Aggregate

* **EquipmentAggregate** — Root aggregate representing an operational equipment unit.
  * Private constructor; created via `Create(EquipmentId)` factory method.
  * State transitions via `Activate()` and `Retire()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via Version property.

## State Model

```
Pending → Active (terminal)
Active → Retired (terminal)
```

## Value Objects

* **EquipmentId** — Unique identifier (validated non-empty Guid).
* **EquipmentStatus** — Enum: Pending, Active, Retired.

## Events

* **EquipmentCreatedEvent** — Raised when new equipment is created.
* **EquipmentActivatedEvent** — Raised when equipment is activated.
* **EquipmentRetiredEvent** — Raised when equipment is retired.

## Invariants

* EquipmentId must not be null/default.
* EquipmentStatus must be a defined enum value.
* State transitions enforced by specifications.
* Must belong to facility or resource group.
* Must be uniquely identified.

## Specifications

* **CanActivateEquipmentSpecification** — Only Pending equipment can be activated.
* **CanRetireEquipmentSpecification** — Only Active equipment can be retired.

## Errors

* **MissingId** — EquipmentId is required.
* **AlreadyActive** — Cannot activate already-active equipment.
* **AlreadyRetired** — Cannot retire already-retired equipment.
* **InvalidStateTransition** — Generic guard for illegal transitions.

## Domain Services

* **EquipmentService** — Reserved for cross-aggregate coordination.

## Lifecycle Pattern

TERMINAL — Once retired, equipment cannot return to a prior state.

## Boundary Statement

This domain defines equipment structure only. No scheduling logic, no allocation execution, no external system interaction.

## Status

**S4 — Invariants + Specifications Complete**
