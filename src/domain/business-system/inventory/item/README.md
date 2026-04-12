# Domain: Item

## Classification

business-system

## Context

inventory

## Boundary Statement

This domain defines inventory item identity and classification. No database or calculation logic.

## Lifecycle Pattern

**TERMINAL** — Active → Discontinued. Discontinued is final.

## Purpose

Defines inventory item identity — the classification and naming of items tracked within the inventory system.

## Core Responsibilities

* Define item identity, type, and name
* Track item lifecycle state (Active → Discontinued)
* Enforce immutability after discontinuation
* Ensure no implicit state changes

## Aggregate(s)

* ItemAggregate
  * Manages the lifecycle and integrity of an inventory item definition

## Value Objects

* ItemId — Unique identifier for an item instance
* ItemStatus — Enum for lifecycle state (Active, Discontinued)
* ItemTypeId — Reference to the item type classification

## Domain Events

* ItemCreatedEvent — Raised when a new item is defined
* ItemDiscontinuedEvent — Raised when the item is discontinued (terminal)

## Specifications

* CanDiscontinueSpecification — Only Active items can be discontinued
* IsActiveSpecification — Checks if item is currently active

## Domain Services

* ItemService — Domain operations for item management

## Errors

* MissingId — ItemId is required
* MissingTypeId — ItemTypeId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyDiscontinued — Item already discontinued (terminal)

## Invariants

* ItemId must not be null/default
* ItemTypeId must not be null/default
* ItemName must not be null or empty
* ItemStatus must be a defined enum value
* Discontinued is terminal (enforced by specification)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* stock (items are tracked in stock)
* movement (movements reference items)
* reservation (reservations reference items)

## Status

**S4 — Invariants + Specifications Complete**
