# Domain: Assumption

## Classification

intelligence-system

## Context

simulation

## Purpose

Defines the structure of simulation assumptions — the premises and conditions that underpin a simulation scenario.

## Core Responsibilities

* Define and validate assumption structures for simulation scenarios
* Track assumption lifecycle from creation through evaluation
* Maintain traceability of assumption origins and dependencies

## Aggregate(s)

* AssumptionAggregate

  * Root aggregate governing the lifecycle and integrity of a simulation assumption

## Entities

* None

## Value Objects

* AssumptionId — Unique identifier for a assumption instance

## Domain Events

* AssumptionCreatedEvent — Raised when a new assumption is created
* AssumptionUpdatedEvent — Raised when assumption metadata is updated
* AssumptionStateChangedEvent — Raised when assumption lifecycle state transitions

## Specifications

* AssumptionSpecification — Validates assumption structure and completeness

## Domain Services

* AssumptionService — Domain operations for assumption management

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
