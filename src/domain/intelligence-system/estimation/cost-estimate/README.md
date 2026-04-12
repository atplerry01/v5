# Domain: CostEstimate

## Classification

intelligence-system

## Context

estimation

## Purpose

Defines the structure of cost estimate artifacts — the projected costs for planned activities or resources.

## Core Responsibilities

* Define cost estimate structure and metadata
* Enforce cost estimate invariants and validation rules
* Emit domain events on cost estimate lifecycle transitions

## Aggregate(s)

* CostEstimateAggregate

  * Root aggregate representing a cost estimate with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* CostEstimateId — Unique identifier for a cost estimate instance

## Domain Events

* CostEstimateCreatedEvent — Raised when a new cost estimate is created
* CostEstimateUpdatedEvent — Raised when cost estimate metadata is updated
* CostEstimateStateChangedEvent — Raised when cost estimate lifecycle state transitions

## Specifications

* CostEstimateSpecification — Validates cost estimate structure and completeness

## Domain Services

* CostEstimateService — Domain operations for cost estimate management

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
