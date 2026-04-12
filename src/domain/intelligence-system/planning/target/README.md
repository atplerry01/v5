# Domain: Target

## Classification

intelligence-system

## Context

planning

## Purpose

Defines the structure of planning targets — the specific measurable outcomes that plans must achieve.

## Core Responsibilities

* Define the canonical structure of target artifacts
* Enforce target validity and completeness invariants
* Emit domain events on target lifecycle transitions

## Aggregate(s)

* TargetAggregate

  * Root aggregate representing a target instance and its lifecycle

## Entities

* None

## Value Objects

* TargetId — Unique identifier for a target instance

## Domain Events

* TargetCreatedEvent — Raised when a new target is created
* TargetUpdatedEvent — Raised when target metadata is updated
* TargetStateChangedEvent — Raised when target lifecycle state transitions

## Specifications

* TargetSpecification — Validates target structure and completeness

## Domain Services

* TargetService — Domain operations for target management

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
