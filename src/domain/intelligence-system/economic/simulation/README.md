# Domain: Simulation

## Classification

intelligence-system

## Context

economic

## Purpose

Defines the structure of economic simulation artifacts — the parameterized scenarios for economic modeling.

## Core Responsibilities

* Define simulation structure and metadata
* Track lifecycle state of simulation artifacts
* Enforce structural invariants for simulation validity

## Aggregate(s)

* SimulationAggregate

  * Manages the lifecycle and invariants of a simulation artifact

## Entities

* None

## Value Objects

* SimulationId — Unique identifier for a simulation instance

## Domain Events

* SimulationCreatedEvent — Raised when a new simulation is created
* SimulationUpdatedEvent — Raised when simulation metadata is updated
* SimulationStateChangedEvent — Raised when simulation lifecycle state transitions

## Specifications

* SimulationSpecification — Validates simulation structure and completeness

## Domain Services

* SimulationService — Domain operations for simulation management

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
