# Domain: AllocationForecast

## Classification

intelligence-system

## Context

capacity

## Purpose

Defines the structure of allocation forecast artifacts — predicted distribution of capacity across resources or time periods.

## Core Responsibilities

* Define allocation forecast structure and metadata
* Track lifecycle state of allocation forecast artifacts
* Enforce structural invariants for allocation forecast validity

## Aggregate(s)

* AllocationForecastAggregate

  * Manages the lifecycle and invariants of an allocation forecast artifact

## Entities

* None

## Value Objects

* AllocationForecastId — Unique identifier for an allocation forecast instance

## Domain Events

* AllocationForecastCreatedEvent — Raised when a new allocation forecast is created
* AllocationForecastUpdatedEvent — Raised when allocation forecast metadata is updated
* AllocationForecastStateChangedEvent — Raised when allocation forecast lifecycle state transitions

## Specifications

* AllocationForecastSpecification — Validates allocation forecast structure and completeness

## Domain Services

* AllocationForecastService — Domain operations for allocation forecast management

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
