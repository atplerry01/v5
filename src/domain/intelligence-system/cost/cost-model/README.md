# Domain: CostModel

## Classification

intelligence-system

## Context

cost

## Purpose

Defines the structure of cost model artifacts — the structural representations of how costs are calculated and attributed.

## Core Responsibilities

* Define cost model structure and metadata
* Track lifecycle state of cost model artifacts
* Enforce structural invariants for cost model validity

## Aggregate(s)

* CostModelAggregate

  * Manages the lifecycle and invariants of a cost model artifact

## Entities

* None

## Value Objects

* CostModelId — Unique identifier for a cost model instance

## Domain Events

* CostModelCreatedEvent — Raised when a new cost model is created
* CostModelUpdatedEvent — Raised when cost model metadata is updated
* CostModelStateChangedEvent — Raised when cost model lifecycle state transitions

## Specifications

* CostModelSpecification — Validates cost model structure and completeness

## Domain Services

* CostModelService — Domain operations for cost model management

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
