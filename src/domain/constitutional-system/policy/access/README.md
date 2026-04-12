# Domain: Access

## Classification

constitutional-system

## Context

policy

## Purpose

Defines the constitutional access policy structure — the rules governing who or what may access resources, operations, or domains within the system. Access policies are declarative boundaries, not runtime gatekeepers.

## Core Responsibilities

* Define access rule structure and identity
* Define access boundary definitions
* Define governance authority references for access control

## Aggregate(s)

* AccessAggregate

  * Represents the access policy rule container

## Entities

* None

## Value Objects

* AccessId — Unique identifier for an access policy definition

## Domain Events

* AccessCreatedEvent — Raised when a new access rule is defined
* AccessUpdatedEvent — Raised when access rule metadata is updated
* AccessStateChangedEvent — Raised when access rule lifecycle state transitions

## Specifications

* AccessSpecification — Validates access rule structure and completeness

## Domain Services

* AccessService — Domain operations for access policy management

## Invariants

* Access rules must be immutable once activated (unless versioned)
* Access rules must be traceable
* Access rules must be policy-bound (WHYCEPOLICY)

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