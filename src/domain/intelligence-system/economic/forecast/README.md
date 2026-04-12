# Domain: Forecast

## Classification

intelligence-system

## Context

economic

## Purpose

Defines the structure of economic forecast artifacts — projected economic outcomes and trends.

## Core Responsibilities

* Define forecast structure and metadata
* Track lifecycle state of forecast artifacts
* Enforce structural invariants for forecast validity

## Aggregate(s)

* ForecastAggregate

  * Manages the lifecycle and invariants of a forecast artifact

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
