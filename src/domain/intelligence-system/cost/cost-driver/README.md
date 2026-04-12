# Domain: CostDriver

## Classification

intelligence-system

## Context

cost

## Purpose

Defines the structure of cost driver records — the factors that influence or cause cost changes.

## Core Responsibilities

* Define cost driver structure and metadata
* Track lifecycle state of cost driver records
* Enforce structural invariants for cost driver validity

## Aggregate(s)

* CostDriverAggregate

  * Manages the lifecycle and invariants of a cost driver record

## Entities

* None

## Value Objects

* CostDriverId — Unique identifier for a cost driver instance

## Domain Events

* CostDriverCreatedEvent — Raised when a new cost driver is created
* CostDriverUpdatedEvent — Raised when cost driver metadata is updated
* CostDriverStateChangedEvent — Raised when cost driver lifecycle state transitions

## Specifications

* CostDriverSpecification — Validates cost driver structure and completeness

## Domain Services

* CostDriverService — Domain operations for cost driver management

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
