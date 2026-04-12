# Domain: Valuation

## Classification

business-system

## Context

inventory

## Purpose

Defines valuation policies — how inventory value is determined. A valuation policy specifies the method (cost basis, weighted average, FIFO, LIFO) but does NOT execute calculations. All valuation logic must be deterministic.

## Core Responsibilities

* Define the structural representation of valuation policies
* Enforce deterministic valuation method rules
* Support reversible lifecycle (suspend/reactivate)
* Emit domain events on valuation lifecycle transitions

## Aggregate(s)

* ValuationAggregate
  * Represents the root entity for a valuation policy, encapsulating method selection and lifecycle rules

## Entities

* None

## Value Objects

* ValuationId — Unique identifier for a valuation policy instance
* ValuationMethod — Enum defining valuation approach (CostBasis, WeightedAverage, Fifo, Lifo)
* ValuationStatus — Lifecycle state (Active, Suspended, Deactivated)

## Domain Events

* ValuationCreatedEvent — Raised when a new valuation policy is created
* ValuationSuspendedEvent — Raised when a policy is suspended
* ValuationReactivatedEvent — Raised when a suspended policy is reactivated
* ValuationDeactivatedEvent — Raised when a policy is permanently deactivated
* ValuationUpdatedEvent — Raised when valuation metadata is updated
* ValuationStateChangedEvent — Raised when valuation lifecycle state transitions

## Specifications

* CanSuspendValuationSpecification — Guards transition from Active to Suspended
* CanReactivateValuationSpecification — Guards transition from Suspended to Active
* CanDeactivateValuationSpecification — Guards transition to Deactivated
* ValuationSpecification — Validates valuation structure and completeness

## Domain Services

* ValuationService — Coordination placeholder for valuation domain operations

## Invariants

* Valuation policy must have a valid identity (non-empty ValuationId)
* Valuation method must be a defined enum value
* Must not calculate outside deterministic rules
* No external calculation engines
* Status must be a defined enum value

## Boundary Statement

This domain owns valuation method definition only. Actual value computation is handled by engines reacting to valuation events within deterministic rules. No calculation logic in this domain.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* stock (valuation target)
* economic-system (read-only relationship)

## Lifecycle

Active → Suspended → Active (reversible)
Active | Suspended → Deactivated (terminal)

**Pattern: REVERSIBLE** — Can be suspended and reactivated. Deactivation is terminal.

## Status

**S4 — Invariants + Specifications Complete**
