# Domain: Warehouse

## Classification

business-system

## Context

inventory

## Purpose

Defines the structure of warehouses — the physical or logical storage locations for inventory. A warehouse has identity and capacity, and cannot be deleted once active (only deactivated).

## Core Responsibilities

* Define the structural representation of warehouse storage locations
* Maintain warehouse identity, capacity, and lifecycle integrity
* Emit domain events on warehouse lifecycle transitions

## Aggregate(s)

* WarehouseAggregate
  * Represents the root entity for a warehouse, encapsulating its identity, capacity, state, and lifecycle rules

## Entities

* None

## Value Objects

* WarehouseId — Unique identifier for a warehouse instance
* WarehouseCapacity — Positive integer representing storage capacity
* WarehouseStatus — Lifecycle state (Active, Deactivated)

## Domain Events

* WarehouseCreatedEvent — Raised when a new warehouse is created
* WarehouseDeactivatedEvent — Raised when a warehouse is deactivated
* WarehouseUpdatedEvent — Raised when warehouse metadata is updated
* WarehouseStateChangedEvent — Raised when warehouse lifecycle state transitions

## Specifications

* CanDeactivateWarehouseSpecification — Guards transition to Deactivated status
* WarehouseSpecification — Validates warehouse structure and completeness

## Domain Services

* WarehouseService — Coordination placeholder for warehouse domain operations

## Invariants

* Warehouse must have a valid identity (non-empty WarehouseId)
* Warehouse must have positive capacity
* Cannot be deleted once active — only deactivated (TERMINAL lifecycle)
* Status must be a defined enum value

## Boundary Statement

This domain owns warehouse identity and capacity definition only. No stock logic, no financial calculations, no execution workflow. Warehouse is a structural boundary for inventory placement.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Created (Active) → Deactivated

**Pattern: TERMINAL** — Once deactivated, warehouse cannot be reactivated.

## Status

**S4 — Invariants + Specifications Complete**
