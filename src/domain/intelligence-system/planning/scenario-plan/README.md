# Domain: ScenarioPlan

## Classification

intelligence-system

## Context

planning

## Purpose

Defines the structure of scenario plans — the alternative planning paths based on different assumptions.

## Core Responsibilities

* Define the canonical structure of scenario plan artifacts
* Enforce scenario plan validity and completeness invariants
* Emit domain events on scenario plan lifecycle transitions

## Aggregate(s)

* ScenarioPlanAggregate

  * Root aggregate representing a scenario plan instance and its lifecycle

## Entities

* None

## Value Objects

* ScenarioPlanId — Unique identifier for a scenario plan instance

## Domain Events

* ScenarioPlanCreatedEvent — Raised when a new scenario plan is created
* ScenarioPlanUpdatedEvent — Raised when scenario plan metadata is updated
* ScenarioPlanStateChangedEvent — Raised when scenario plan lifecycle state transitions

## Specifications

* ScenarioPlanSpecification — Validates scenario plan structure and completeness

## Domain Services

* ScenarioPlanService — Domain operations for scenario plan management

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
