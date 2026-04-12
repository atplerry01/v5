# Domain: Stock

## Classification

business-system

## Context

inventory

## Boundary Statement

This domain defines inventory quantity state. No database or calculation logic outside aggregates.

## Lifecycle Pattern

**SEQUENTIAL** — Initialized → Tracked → Depleted. No state reversal.

## Purpose

Tracks quantity state for inventory items. Stock represents the current quantity level of an item, enforcing non-negative invariants and sequential lifecycle progression.

## Core Responsibilities

* Define stock identity and item reference
* Track stock lifecycle state (Initialized → Tracked → Depleted)
* Enforce non-negative quantity invariant
* Ensure all quantity changes go through events

## Aggregate(s)

* StockAggregate
  * Manages the lifecycle and quantity state of inventory stock

## Value Objects

* StockId — Unique identifier for a stock instance
* StockStatus — Enum for lifecycle state (Initialized, Tracked, Depleted)
* StockItemId — Reference to the inventory item
* Quantity — Non-negative quantity value object

## Domain Events

* StockCreatedEvent — Raised when stock is initialized
* StockTrackedEvent — Raised when stock begins tracking
* StockDepletedEvent — Raised when stock is depleted

## Specifications

* CanTrackSpecification — Only Initialized stock can begin tracking
* CanDepleteSpecification — Only Tracked stock can be depleted
* IsTrackedSpecification — Checks if stock is currently tracked

## Domain Services

* StockService — Domain operations for stock management

## Errors

* MissingId — StockId is required
* MissingItemId — StockItemId is required
* NegativeQuantity — Quantity must not be negative
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyTracked — Stock already being tracked
* AlreadyDepleted — Stock already depleted

## Invariants

* StockId must not be null/default
* StockItemId must not be null/default
* Quantity must NEVER be negative
* StockStatus must be a defined enum value
* Sequential progression enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* item (stock references items)
* movement (movements modify stock quantity)
* reservation (reservations reduce available stock)

## Status

**S4 — Invariants + Specifications Complete**
