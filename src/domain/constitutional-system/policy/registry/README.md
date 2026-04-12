# Domain: Registry

## Classification

constitutional-system

## Context

policy

## Purpose

Defines the constitutional policy registry structure — the catalogue of all registered policies within the system. The registry provides the canonical index of policy definitions, their versions, and their activation status.

## Core Responsibilities

* Define policy registry structure and identity
* Define policy registration rules
* Define governance authority references for policy cataloguing

## Aggregate(s)

* RegistryAggregate

  * Represents the policy registry container

## Entities

* None

## Value Objects

* RegistryId — Unique identifier for a registry entry

## Domain Events

* RegistryCreatedEvent — Raised when a new registry entry is created
* RegistryUpdatedEvent — Raised when registry entry metadata is updated
* RegistryStateChangedEvent — Raised when registry entry lifecycle state transitions

## Specifications

* RegistrySpecification — Validates registry entry structure and completeness

## Domain Services

* RegistryService — Domain operations for registry management

## Invariants

* Registry entries must be immutable once activated (unless versioned)
* Registry entries must be traceable
* Registry entries must be policy-bound (WHYCEPOLICY)

## Policy Dependencies

* WHYCEPOLICY is the execution authority
* Domain only defines structure, not enforcement

## Integration Points

* decision-system
* trust-system
* economic-system
* WHYCEPOLICY engine (external)

## Lifecycle

Draft → Defined → Activated → Versioned → Deprecated

## Notes

This domain defines constitutional structure ONLY.
All enforcement is external via WHYCEPOLICY and runtime.
