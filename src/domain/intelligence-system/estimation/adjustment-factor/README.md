# Domain: AdjustmentFactor

## Classification

intelligence-system

## Context

estimation

## Purpose

Defines the structure of estimation adjustment factors — the multipliers or corrections applied to base estimates.

## Core Responsibilities

* Define adjustment factor structure and metadata
* Enforce adjustment factor invariants and validation rules
* Emit domain events on adjustment factor lifecycle transitions

## Aggregate(s)

* AdjustmentFactorAggregate

  * Root aggregate representing an estimation adjustment factor with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* AdjustmentFactorId — Unique identifier for an adjustment factor instance

## Domain Events

* AdjustmentFactorCreatedEvent — Raised when a new adjustment factor is created
* AdjustmentFactorUpdatedEvent — Raised when adjustment factor metadata is updated
* AdjustmentFactorStateChangedEvent — Raised when adjustment factor lifecycle state transitions

## Specifications

* AdjustmentFactorSpecification — Validates adjustment factor structure and completeness

## Domain Services

* AdjustmentFactorService — Domain operations for adjustment factor management

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
