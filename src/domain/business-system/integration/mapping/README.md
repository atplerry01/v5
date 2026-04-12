# Domain: Mapping

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines behavior contracts only and contains no execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Can toggle between Active and Disabled states.

## Purpose

Defines transformation mapping rules for integration. A mapping specifies how data is transformed across system boundaries, without containing any transformation execution logic.

## Core Responsibilities

* Define mapping identity and definition reference
* Track mapping lifecycle state (Defined → Active ↔ Disabled)
* Enforce definition presence before activation
* Ensure no transformation execution logic in domain

## Aggregate(s)

* MappingAggregate
  * Manages the lifecycle and integrity of a mapping definition

## Value Objects

* MappingId — Unique identifier for a mapping instance
* MappingStatus — Enum for lifecycle state (Defined, Active, Disabled)
* MappingDefinitionId — Reference to the mapping definition

## Domain Events

* MappingCreatedEvent — Raised when a new mapping is defined
* MappingActivatedEvent — Raised when the mapping is activated
* MappingDisabledEvent — Raised when the mapping is disabled

## Specifications

* CanActivateSpecification — Defined or Disabled mappings can be activated
* CanDisableSpecification — Only Active mappings can be disabled
* IsActiveSpecification — Checks if mapping is currently active

## Domain Services

* MappingService — Domain operations for mapping management

## Errors

* MissingId — MappingId is required
* MissingDefinitionId — MappingDefinitionId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Mapping already in Active state
* AlreadyDisabled — Mapping already in Disabled state

## Invariants

* MappingId must not be null/default
* MappingDefinitionId must not be null/default
* MappingStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* event-bridge (uses mappings for event transformation rules)
* command-bridge (uses mappings for command transformation rules)

## Status

**S4 — Invariants + Specifications Complete**
