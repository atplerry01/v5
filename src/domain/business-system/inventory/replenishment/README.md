# Domain: Replenishment

## Classification

business-system

## Context

inventory

## Purpose

Defines replenishment policies — the restocking rules that define when and how much inventory should be replenished. A replenishment policy defines a threshold and restock quantity. Cannot trigger below zero stock.

## Core Responsibilities

* Define the structural representation of replenishment policies
* Enforce threshold and restock quantity rules
* Support reversible lifecycle (suspend/reactivate)
* Emit domain events on replenishment lifecycle transitions

## Aggregate(s)

* ReplenishmentAggregate
  * Represents the root entity for a replenishment policy, encapsulating threshold, restock quantity, and lifecycle rules

## Entities

* None

## Value Objects

* ReplenishmentId — Unique identifier for a replenishment policy instance
* ReplenishmentThreshold — Non-negative integer threshold triggering restock
* RestockQuantity — Positive integer representing quantity to restock
* ReplenishmentStatus — Lifecycle state (Active, Suspended, Deactivated)

## Domain Events

* ReplenishmentCreatedEvent — Raised when a new replenishment policy is created
* ReplenishmentSuspendedEvent — Raised when a policy is suspended
* ReplenishmentReactivatedEvent — Raised when a suspended policy is reactivated
* ReplenishmentDeactivatedEvent — Raised when a policy is permanently deactivated
* ReplenishmentUpdatedEvent — Raised when replenishment metadata is updated
* ReplenishmentStateChangedEvent — Raised when replenishment lifecycle state transitions

## Specifications

* CanSuspendReplenishmentSpecification — Guards transition from Active to Suspended
* CanReactivateReplenishmentSpecification — Guards transition from Suspended to Active
* CanDeactivateReplenishmentSpecification — Guards transition to Deactivated
* ReplenishmentSpecification — Validates replenishment structure and completeness

## Domain Services

* ReplenishmentService — Coordination placeholder for replenishment domain operations

## Invariants

* Replenishment policy must have a valid identity (non-empty ReplenishmentId)
* Threshold cannot be negative
* Restock quantity must be positive
* Cannot trigger replenishment below zero stock
* Status must be a defined enum value

## Boundary Statement

This domain owns replenishment rule definition only. Actual restocking execution is handled by runtime reacting to replenishment events. No direct stock mutation.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* stock (threshold comparison)
* structural-system

## Lifecycle

Active → Suspended → Active (reversible)
Active | Suspended → Deactivated (terminal)

**Pattern: REVERSIBLE** — Can be suspended and reactivated. Deactivation is terminal.

## Status

**S4 — Invariants + Specifications Complete**
