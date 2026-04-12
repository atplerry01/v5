# Domain: Influence

## Classification

intelligence-system

## Context

relationship

## Purpose

Defines the structure of influence records — the measured impact one entity has on another within the relationship network.

## Core Responsibilities

* Model influence structures between related entities
* Track measured impact and influence metrics
* Maintain influence lifecycle and state transitions

## Aggregate(s)

* InfluenceAggregate

  * Encapsulates the lifecycle and invariants of an influence record

## Entities

* None

## Value Objects

* InfluenceId — Unique identifier for an influence instance

## Domain Events

* InfluenceCreatedEvent — Raised when a new influence is created
* InfluenceUpdatedEvent — Raised when influence metadata is updated
* InfluenceStateChangedEvent — Raised when influence lifecycle state transitions

## Specifications

* InfluenceSpecification — Validates influence structure and completeness

## Domain Services

* InfluenceService — Domain operations for influence management

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
