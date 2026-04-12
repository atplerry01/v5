# Domain: Cost

## Classification

business-system

## Context

execution

## Purpose

Manages the aggregation of cost components for business execution activities. Tracks cost lifecycle from pending through calculated to finalized. Requires cost components before calculation and a valid cost context reference.

## Core Responsibilities

* Define cost identity and context reference
* Track cost lifecycle state (Pending → Calculated → Finalized)
* Aggregate cost components before calculation
* Enforce that finalization requires prior calculation
* Maintain cost component inventory

## Aggregate(s)

* CostAggregate
  * Manages the lifecycle and aggregation of execution costs

## Entities

* CostComponent — Individual cost breakdown unit (ComponentId, Description)

## Value Objects

* CostId — Unique identifier for a cost instance
* CostStatus — Enum for lifecycle state (Pending, Calculated, Finalized)
* CostContextId — Reference to the cost context (execution activity)

## Domain Events

* CostCreatedEvent — Raised when a new cost is created
* CostCalculatedEvent — Raised when cost components are calculated
* CostFinalizedEvent — Raised when cost is finalized and locked

## Specifications

* CanCalculateSpecification — Only Pending costs can be calculated
* CanFinalizeSpecification — Only Calculated costs can be finalized
* IsFinalizedSpecification — Checks if cost is finalized

## Domain Services

* CostService — Domain operations for cost management

## Errors

* MissingId — CostId is required
* MissingContextId — CostContextId is required
* ComponentRequired — Cost must contain at least one cost component
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyCalculated — Cost already calculated
* AlreadyFinalized — Cost already finalized

## Invariants

* CostId must not be null/default
* CostContextId must not be null/default
* CostStatus must be a defined enum value
* Must contain at least one cost component before calculating
* Cannot finalize before calculation (enforced by CanFinalizeSpecification)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* charge (cost aggregation feeds charge creation)
* allocation (costs may relate to resource allocations)

## Status

**S4 — Invariants + Specifications Complete**
