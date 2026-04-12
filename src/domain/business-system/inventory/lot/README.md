# Domain: Lot

## Classification

business-system

## Context

inventory

## Purpose

Defines the structure of inventory lots — traceable production groupings that track origin. A lot must track its origin and is immutable after sealing.

## Core Responsibilities

* Define the structural representation of inventory lots
* Enforce origin tracking and immutability after sealing
* Emit domain events on lot lifecycle transitions

## Aggregate(s)

* LotAggregate
  * Represents the root entity for an inventory lot, encapsulating its identity, origin, and lifecycle rules

## Entities

* None

## Value Objects

* LotId — Unique identifier for a lot instance
* LotOrigin — Origin/source tracking for the lot
* LotStatus — Lifecycle state (Active, Sealed)

## Domain Events

* LotCreatedEvent — Raised when a new lot is created
* LotSealedEvent — Raised when a lot is sealed (immutable)
* LotUpdatedEvent — Raised when lot metadata is updated
* LotStateChangedEvent — Raised when lot lifecycle state transitions

## Specifications

* CanSealLotSpecification — Guards transition from Active to Sealed
* LotSpecification — Validates lot structure and completeness

## Domain Services

* LotService — Coordination placeholder for lot domain operations

## Invariants

* Lot must have a valid identity (non-empty LotId)
* Lot must track origin (non-empty LotOrigin)
* Immutable after sealing — no modifications once sealed (TERMINAL)
* Status must be a defined enum value

## Boundary Statement

This domain owns lot identity and origin tracking only. Lot defines production grouping but does not own item or batch logic. No implicit stock changes.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* batch (lots may be associated with batches)
* item (lot tracks origin of items)
* structural-system

## Lifecycle

Active → Sealed

**Pattern: TERMINAL** — Once sealed, lot cannot be modified.

## Status

**S4 — Invariants + Specifications Complete**
