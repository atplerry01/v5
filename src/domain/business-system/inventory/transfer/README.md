# Domain: Transfer

## Classification

business-system

## Context

inventory

## Purpose

Defines the structure of inventory transfers — the atomic movement of stock between source and destination warehouses. Transfer must include source, destination, and positive quantity.

## Core Responsibilities

* Define the structural representation of inventory transfers
* Enforce atomic transfer semantics (source + destination + quantity)
* Maintain transfer identity and lifecycle integrity
* Emit domain events on transfer lifecycle transitions

## Aggregate(s)

* TransferAggregate
  * Represents the root entity for an inventory transfer, encapsulating source, destination, quantity, and lifecycle rules

## Entities

* None

## Value Objects

* TransferId — Unique identifier for a transfer instance
* TransferQuantity — Positive integer representing transfer quantity
* TransferStatus — Lifecycle state (Pending, Completed, Cancelled)

## Domain Events

* TransferCreatedEvent — Raised when a new transfer is created
* TransferCompletedEvent — Raised when a transfer is completed
* TransferCancelledEvent — Raised when a transfer is cancelled
* TransferUpdatedEvent — Raised when transfer metadata is updated
* TransferStateChangedEvent — Raised when transfer lifecycle state transitions

## Specifications

* CanCompleteTransferSpecification — Guards transition to Completed status
* CanCancelTransferSpecification — Guards transition to Cancelled status
* TransferSpecification — Validates transfer structure and completeness

## Domain Services

* TransferService — Coordination placeholder for transfer domain operations

## Invariants

* Transfer must have a valid identity (non-empty TransferId)
* Source and destination warehouse must be different
* Quantity must be positive (no zero, no negative)
* Transfer is atomic — no partial completion
* Status must be a defined enum value

## Boundary Statement

This domain owns transfer definition only. No direct stock mutation — stock changes are handled by the stock domain reacting to transfer events. No implicit changes.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* warehouse (source/destination references)
* stock (reacts to transfer events)
* structural-system

## Lifecycle

Pending → Completed | Cancelled

**Pattern: TERMINAL** — Once completed or cancelled, transfer cannot be modified.

## Status

**S4 — Invariants + Specifications Complete**
