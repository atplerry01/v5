# Domain: Forecast

## Classification

intelligence-system

## Context

simulation

## Purpose

Defines the structure of simulation forecasts — the projected future states generated from simulation models.

## Core Responsibilities

* Define and validate forecast structures for simulation projections
* Track forecast lifecycle from creation through evaluation
* Maintain traceability of forecast inputs and confidence levels

## Aggregate(s)

* ForecastAggregate

  * Root aggregate governing the lifecycle and integrity of a simulation forecast

## Entities

* None

## Value Objects

* ForecastId — Unique identifier for a forecast instance

## Domain Events

* ForecastCreatedEvent — Raised when a new forecast is created
* ForecastUpdatedEvent — Raised when forecast metadata is updated
* ForecastStateChangedEvent — Raised when forecast lifecycle state transitions

## Specifications

* ForecastSpecification — Validates forecast structure and completeness

## Domain Services

* ForecastService — Domain operations for forecast management

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
