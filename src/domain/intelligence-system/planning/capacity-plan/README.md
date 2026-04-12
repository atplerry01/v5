# Domain: CapacityPlan

## Classification

intelligence-system

## Context

planning

## Purpose

Defines the structure of capacity plan artifacts — the projected resource needs and allocation strategies.

## Core Responsibilities

* Define the canonical structure of capacity plan artifacts
* Enforce capacity plan validity and completeness invariants
* Emit domain events on capacity plan lifecycle transitions

## Aggregate(s)

* CapacityPlanAggregate

  * Root aggregate representing a capacity plan instance and its lifecycle

## Entities

* None

## Value Objects

* CapacityPlanId — Unique identifier for a capacity plan instance

## Domain Events

* CapacityPlanCreatedEvent — Raised when a new capacity plan is created
* CapacityPlanUpdatedEvent — Raised when capacity plan metadata is updated
* CapacityPlanStateChangedEvent — Raised when capacity plan lifecycle state transitions

## Specifications

* CapacityPlanSpecification — Validates capacity plan structure and completeness

## Domain Services

* CapacityPlanService — Domain operations for capacity plan management

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
