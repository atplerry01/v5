# Domain: Affiliation

## Classification

intelligence-system

## Context

relationship

## Purpose

Defines the structure of affiliation records — the formal or informal associations between entities.

## Core Responsibilities

* Model affiliation structures between entities
* Track formal and informal association types
* Maintain affiliation lifecycle and state transitions

## Aggregate(s)

* AffiliationAggregate

  * Encapsulates the lifecycle and invariants of an affiliation record

## Entities

* None

## Value Objects

* AffiliationId — Unique identifier for an affiliation instance

## Domain Events

* AffiliationCreatedEvent — Raised when a new affiliation is created
* AffiliationUpdatedEvent — Raised when affiliation metadata is updated
* AffiliationStateChangedEvent — Raised when affiliation lifecycle state transitions

## Specifications

* AffiliationSpecification — Validates affiliation structure and completeness

## Domain Services

* AffiliationService — Domain operations for affiliation management

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
