# Domain: HierarchyDefinition

## Classification
structural-system

## Context
structure

## Domain Responsibility
This domain defines structural hierarchy definitions and enforces valid parent-child relationships. Contains no execution or orchestration logic.

## Aggregate

* **HierarchyDefinitionAggregate** -- Root aggregate representing a hierarchy definition.
  * Private constructor; created via `Define(HierarchyDefinitionId, HierarchyDefinitionDescriptor)` factory method.
  * State transitions via `Validate()` and `Lock()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change, including parent-child structural validity.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Define() -> Defined --Validate()--> Validated --Lock()--> Locked (terminal)
```

## Value Objects

* **HierarchyDefinitionId** -- Deterministic identifier (validated non-empty Guid).
* **HierarchyDefinitionStatus** -- Enum: `Defined`, `Validated`, `Locked`.
* **HierarchyDefinitionDescriptor** -- Record struct with `HierarchyName` (string, non-empty) and `ParentReference` (Guid, may be empty for root nodes).

## Events

* **HierarchyDefinitionDefinedEvent(HierarchyDefinitionId, HierarchyDefinitionDescriptor)** -- Hierarchy definition created with initial descriptor.
* **HierarchyDefinitionValidatedEvent(HierarchyDefinitionId)** -- Hierarchy definition validated.
* **HierarchyDefinitionLockedEvent(HierarchyDefinitionId)** -- Hierarchy definition locked (terminal).

## Invariants

* HierarchyDefinitionId must not be null/default.
* HierarchyDefinitionDescriptor must not be default.
* HierarchyDefinitionStatus must be a defined enum value.
* A hierarchy definition cannot reference itself as its own parent (self-referencing guard).
* State transitions enforced by specifications (Defined allows Validate; Validated allows Lock).

## Specifications

* **CanValidateSpecification** -- Validates that status is Defined before validation.
* **CanLockSpecification** -- Validates that status is Validated before locking.

## Errors

* **MissingId** -- HierarchyDefinitionId is required.
* **MissingDescriptor** -- HierarchyDefinitionDescriptor is required.
* **InvalidParentChild** -- Self-referencing parent-child relationship detected.
* **InvalidStateTransition** -- Guard for illegal status transitions (includes current status and attempted action).

## Domain Services

* **HierarchyDefinitionService** -- Reserved for cross-aggregate coordination within hierarchy-definition context.

## Notes

* ParentReference may be Guid.Empty for root-level hierarchy nodes.
* Self-referencing is explicitly guarded: a node cannot be its own parent.

## Status

**S4 -- Invariants + Specifications Complete**
