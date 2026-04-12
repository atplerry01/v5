# Domain: Recommendation

## Classification

intelligence-system

## Context

simulation

## Purpose

Defines the structure of simulation recommendations — the suggested actions derived from simulation analysis.

## Core Responsibilities

* Define and validate recommendation structures for simulation-derived actions
* Track recommendation lifecycle from creation through evaluation
* Maintain traceability of recommendation rationale and confidence

## Aggregate(s)

* RecommendationAggregate

  * Root aggregate governing the lifecycle and integrity of a simulation recommendation

## Entities

* None

## Value Objects

* RecommendationId — Unique identifier for a recommendation instance

## Domain Events

* RecommendationCreatedEvent — Raised when a new recommendation is created
* RecommendationUpdatedEvent — Raised when recommendation metadata is updated
* RecommendationStateChangedEvent — Raised when recommendation lifecycle state transitions

## Specifications

* RecommendationSpecification — Validates recommendation structure and completeness

## Domain Services

* RecommendationService — Domain operations for recommendation management

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
