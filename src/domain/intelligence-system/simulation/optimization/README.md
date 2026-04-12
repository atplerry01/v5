# Domain: Optimization

## Classification

intelligence-system

## Context

simulation

## Purpose

Defines the structure of simulation optimizations — the target-seeking configurations within simulation runs.

## Core Responsibilities

* Define and validate optimization structures for simulation targets
* Track optimization lifecycle from creation through evaluation
* Maintain traceability of optimization objectives and constraints

## Aggregate(s)

* OptimizationAggregate

  * Root aggregate governing the lifecycle and integrity of a simulation optimization

## Entities

* None

## Value Objects

* OptimizationId — Unique identifier for a optimization instance

## Domain Events

* OptimizationCreatedEvent — Raised when a new optimization is created
* OptimizationUpdatedEvent — Raised when optimization metadata is updated
* OptimizationStateChangedEvent — Raised when optimization lifecycle state transitions

## Specifications

* OptimizationSpecification — Validates optimization structure and completeness

## Domain Services

* OptimizationService — Domain operations for optimization management

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
