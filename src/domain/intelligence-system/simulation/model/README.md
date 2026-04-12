# Domain: Model

## Classification

intelligence-system

## Context

simulation

## Purpose

Defines the structure of simulation models — the parameterized representations of systems used for what-if analysis.

## Core Responsibilities

* Define and validate model structures for simulation parameterization
* Track model lifecycle from creation through evaluation
* Maintain traceability of model parameters and configurations

## Aggregate(s)

* ModelAggregate

  * Root aggregate governing the lifecycle and integrity of a simulation model

## Entities

* None

## Value Objects

* ModelId — Unique identifier for a model instance

## Domain Events

* ModelCreatedEvent — Raised when a new model is created
* ModelUpdatedEvent — Raised when model metadata is updated
* ModelStateChangedEvent — Raised when model lifecycle state transitions

## Specifications

* ModelSpecification — Validates model structure and completeness

## Domain Services

* ModelService — Domain operations for model management

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
