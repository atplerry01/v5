# Domain: Scenario

## Classification

intelligence-system

## Context

simulation

## Purpose

Defines the structure of simulation scenarios — the configured sets of assumptions and parameters for a simulation run.

## Core Responsibilities

* Define and validate scenario structures for simulation configuration
* Track scenario lifecycle from creation through evaluation
* Maintain traceability of scenario assumptions and parameter sets

## Aggregate(s)

* ScenarioAggregate

  * Root aggregate governing the lifecycle and integrity of a simulation scenario

## Entities

* None

## Value Objects

* ScenarioId — Unique identifier for a scenario instance

## Domain Events

* ScenarioCreatedEvent — Raised when a new scenario is created
* ScenarioUpdatedEvent — Raised when scenario metadata is updated
* ScenarioStateChangedEvent — Raised when scenario lifecycle state transitions

## Specifications

* ScenarioSpecification — Validates scenario structure and completeness

## Domain Services

* ScenarioService — Domain operations for scenario management

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
