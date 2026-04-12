# Domain: Comparison

## Classification

intelligence-system

## Context

simulation

## Purpose

Defines the structure of simulation comparisons — the side-by-side analysis records between simulation outcomes.

## Core Responsibilities

* Define and validate comparison structures for simulation outcomes
* Track comparison lifecycle from creation through evaluation
* Maintain traceability of comparison criteria and results

## Aggregate(s)

* ComparisonAggregate

  * Root aggregate governing the lifecycle and integrity of a simulation comparison

## Entities

* None

## Value Objects

* ComparisonId — Unique identifier for a comparison instance

## Domain Events

* ComparisonCreatedEvent — Raised when a new comparison is created
* ComparisonUpdatedEvent — Raised when comparison metadata is updated
* ComparisonStateChangedEvent — Raised when comparison lifecycle state transitions

## Specifications

* ComparisonSpecification — Validates comparison structure and completeness

## Domain Services

* ComparisonService — Domain operations for comparison management

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
