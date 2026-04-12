# Domain: Constraint

## Classification

intelligence-system

## Context

capacity

## Purpose

Defines the structure of capacity constraint records — the limitations and bottlenecks affecting available capacity.

## Core Responsibilities

* Define constraint structure and metadata
* Track lifecycle state of constraint records
* Enforce structural invariants for constraint validity

## Aggregate(s)

* ConstraintAggregate

  * Manages the lifecycle and invariants of a constraint record

## Entities

* None

## Value Objects

* ConstraintId — Unique identifier for a constraint instance

## Domain Events

* ConstraintCreatedEvent — Raised when a new constraint is created
* ConstraintUpdatedEvent — Raised when constraint metadata is updated
* ConstraintStateChangedEvent — Raised when constraint lifecycle state transitions

## Specifications

* ConstraintSpecification — Validates constraint structure and completeness

## Domain Services

* ConstraintService — Domain operations for constraint management

## Invariants

* Intelligence artifacts must be deterministic and traceable
* No execution logic allowed
* No inference logic allowed

## Policy Dependencies

* Governance or usage constraints may be governed by WHYCEPOLICY

## Integration Points

* decision-system (consumes insights)
* trust-system (signals influence trust)
* economic-system (signals influence risk)

## Lifecycle

Created → Updated → Evaluated → Archived

## Notes

This domain represents intelligence structure ONLY. All AI/ML execution is external (T3I layer).
