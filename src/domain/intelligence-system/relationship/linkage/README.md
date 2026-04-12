# Domain: Linkage

## Classification

intelligence-system

## Context

relationship

## Purpose

Defines the structure of linkage records — the direct connections between two entities with typed relationships.

## Core Responsibilities

* Model typed linkage structures between entity pairs
* Track direct connection properties and relationship types
* Maintain linkage lifecycle and state transitions

## Aggregate(s)

* LinkageAggregate

  * Encapsulates the lifecycle and invariants of a linkage record

## Entities

* None

## Value Objects

* LinkageId — Unique identifier for a linkage instance

## Domain Events

* LinkageCreatedEvent — Raised when a new linkage is created
* LinkageUpdatedEvent — Raised when linkage metadata is updated
* LinkageStateChangedEvent — Raised when linkage lifecycle state transitions

## Specifications

* LinkageSpecification — Validates linkage structure and completeness

## Domain Services

* LinkageService — Domain operations for linkage management

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
