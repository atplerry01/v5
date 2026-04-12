# Domain: CostVariance

## Classification

intelligence-system

## Context

cost

## Purpose

Defines the structure of cost variance records — the measured deviations between expected and actual costs.

## Core Responsibilities

* Define cost variance structure and metadata
* Track lifecycle state of cost variance records
* Enforce structural invariants for cost variance validity

## Aggregate(s)

* CostVarianceAggregate

  * Manages the lifecycle and invariants of a cost variance record

## Entities

* None

## Value Objects

* CostVarianceId — Unique identifier for a cost variance instance

## Domain Events

* CostVarianceCreatedEvent — Raised when a new cost variance is created
* CostVarianceUpdatedEvent — Raised when cost variance metadata is updated
* CostVarianceStateChangedEvent — Raised when cost variance lifecycle state transitions

## Specifications

* CostVarianceSpecification — Validates cost variance structure and completeness

## Domain Services

* CostVarianceService — Domain operations for cost variance management

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
