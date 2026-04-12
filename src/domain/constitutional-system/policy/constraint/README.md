# Domain: Constraint

## Classification

constitutional-system

## Context

policy

## Purpose

Defines constitutional constraint structures — the hard boundaries and limits that govern what is permissible within the system. Constraints are declarative rules that define what MUST or MUST NOT occur, independent of enforcement.

## Core Responsibilities

* Define constraint rule structure and identity
* Define constraint boundaries and limits
* Define governance authority references for constraints

## Aggregate(s)

* ConstraintAggregate

  * Represents the constraint rule container

## Entities

* None

## Value Objects

* ConstraintId — Unique identifier for a constraint definition

## Domain Events

* ConstraintCreatedEvent — Raised when a new constraint is defined
* ConstraintUpdatedEvent — Raised when constraint metadata is updated
* ConstraintStateChangedEvent — Raised when constraint lifecycle state transitions

## Specifications

* ConstraintSpecification — Validates constraint structure and completeness

## Domain Services

* ConstraintService — Domain operations for constraint management

## Invariants

* Constraints must be immutable once activated (unless versioned)
* Constraints must be traceable
* Constraints must be policy-bound (WHYCEPOLICY)

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
