# Domain: TypeDefinition

## Classification
structural-system

## Context
structure

## Domain Responsibility
This domain defines structural type definitions and contains no execution or orchestration logic.

## Aggregate

* **TypeDefinitionAggregate** -- Root aggregate representing a structural type definition.
  * Private constructor; created via `Define(TypeDefinitionId, TypeDefinitionDescriptor)` factory method.
  * State transitions via `Activate()` and `Retire()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Define() -> Defined --Activate()--> Active --Retire()--> Retired (terminal)
```

## Value Objects

* **TypeDefinitionId** -- Deterministic identifier (validated non-empty Guid).
* **TypeDefinitionStatus** -- Enum: `Defined`, `Active`, `Retired`.
* **TypeDefinitionDescriptor** -- Record struct with `TypeName` (string, non-empty) and `TypeCategory` (string, non-empty).

## Events

* **TypeDefinitionDefinedEvent(TypeDefinitionId, TypeDefinitionDescriptor)** -- Type definition created with initial descriptor.
* **TypeDefinitionActivatedEvent(TypeDefinitionId)** -- Type definition transitioned to Active.
* **TypeDefinitionRetiredEvent(TypeDefinitionId)** -- Type definition transitioned to Retired (terminal).

## Invariants

* TypeDefinitionId must not be null/default.
* TypeDefinitionDescriptor must not be default.
* TypeDefinitionStatus must be a defined enum value.
* State transitions enforced by specifications (Defined allows Activate; Active allows Retire).

## Specifications

* **CanActivateSpecification** -- Validates that status is Defined before activating.
* **CanRetireSpecification** -- Validates that status is Active before retiring.

## Errors

* **MissingId** -- TypeDefinitionId is required.
* **MissingDescriptor** -- TypeDefinitionDescriptor is required.
* **InvalidStateTransition** -- Guard for illegal status transitions (includes current status and attempted action).

## Domain Services

* **TypeDefinitionService** -- Reserved for cross-aggregate coordination within type-definition context.

## Status

**S4 -- Invariants + Specifications Complete**
