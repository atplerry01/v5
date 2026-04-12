# Domain: PriceEstimate

## Classification

intelligence-system

## Context

estimation

## Purpose

Defines the structure of price estimate artifacts — the projected prices for goods, services, or assets.

## Core Responsibilities

* Define price estimate structure and metadata
* Enforce price estimate invariants and validation rules
* Emit domain events on price estimate lifecycle transitions

## Aggregate(s)

* PriceEstimateAggregate

  * Root aggregate representing a price estimate with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* PriceEstimateId — Unique identifier for a price estimate instance

## Domain Events

* PriceEstimateCreatedEvent — Raised when a new price estimate is created
* PriceEstimateUpdatedEvent — Raised when price estimate metadata is updated
* PriceEstimateStateChangedEvent — Raised when price estimate lifecycle state transitions

## Specifications

* PriceEstimateSpecification — Validates price estimate structure and completeness

## Domain Services

* PriceEstimateService — Domain operations for price estimate management

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
