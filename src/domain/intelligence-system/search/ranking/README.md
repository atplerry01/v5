# Domain: Ranking

## Classification

intelligence-system

## Context

search

## Purpose

Defines the structure of search ranking records — the scoring and ordering rules applied to search results.

## Core Responsibilities

* Model ranking structures and scoring rules
* Track ordering criteria and weight configurations
* Maintain ranking lifecycle and state transitions

## Aggregate(s)

* RankingAggregate

  * Encapsulates the lifecycle and invariants of a search ranking record

## Entities

* None

## Value Objects

* RankingId — Unique identifier for a ranking instance

## Domain Events

* RankingCreatedEvent — Raised when a new ranking is created
* RankingUpdatedEvent — Raised when ranking metadata is updated
* RankingStateChangedEvent — Raised when ranking lifecycle state transitions

## Specifications

* RankingSpecification — Validates ranking structure and completeness

## Domain Services

* RankingService — Domain operations for ranking management

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
