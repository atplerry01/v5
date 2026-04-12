# Domain: Adjustment

## Classification

business-system

## Context

inventory

## Purpose

Defines the structure of inventory adjustments — corrections to recorded inventory quantities.

## Core Responsibilities

* Define the structural representation of inventory adjustments
* Maintain adjustment identity and metadata integrity
* Emit domain events on adjustment lifecycle transitions

## Aggregate(s)

* AdjustmentAggregate

  * Represents the root entity for an inventory adjustment, encapsulating its identity, state, and lifecycle rules

## Entities

* None

## Value Objects

* AdjustmentId — Unique identifier for an adjustment instance

## Domain Events

* AdjustmentCreatedEvent — Raised when a new adjustment is created
* AdjustmentUpdatedEvent — Raised when adjustment metadata is updated
* AdjustmentStateChangedEvent — Raised when adjustment lifecycle state transitions

## Specifications

* AdjustmentSpecification — Validates adjustment structure and completeness

## Domain Services

* AdjustmentService — Domain operations for adjustment management

## Invariants

* Business entities must remain consistent
* Relationships must be valid
* No financial or execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Created → Active → Updated → Inactive

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
