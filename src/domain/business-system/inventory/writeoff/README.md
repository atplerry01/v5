# Domain: Writeoff

## Classification

business-system

## Context

inventory

## Purpose

Defines the structure of inventory write-offs — explicit, irreversible removal of inventory from active stock. A writeoff must reference stock or item, include a positive quantity, and provide a reason. No silent write-offs.

## Core Responsibilities

* Define the structural representation of inventory write-offs
* Enforce explicit and irreversible removal semantics
* Require reference, quantity, and reason for every write-off
* Emit domain events on writeoff lifecycle transitions

## Aggregate(s)

* WriteoffAggregate
  * Represents the root entity for an inventory write-off, encapsulating reference, quantity, reason, and lifecycle rules

## Entities

* None

## Value Objects

* WriteoffId — Unique identifier for a writeoff instance
* WriteoffReference — Reference to the stock or item being written off (non-empty Guid)
* WriteoffQuantity — Positive integer representing quantity to write off
* WriteoffReason — Mandatory reason for the write-off (no silent deletions)
* WriteoffStatus — Lifecycle state (Pending, Confirmed)

## Domain Events

* WriteoffCreatedEvent — Raised when a new write-off is created
* WriteoffConfirmedEvent — Raised when a write-off is confirmed (irreversible)
* WriteoffUpdatedEvent — Raised when writeoff metadata is updated
* WriteoffStateChangedEvent — Raised when writeoff lifecycle state transitions

## Specifications

* CanConfirmWriteoffSpecification — Guards transition from Pending to Confirmed
* WriteoffSpecification — Validates writeoff structure and completeness

## Domain Services

* WriteoffService — Coordination placeholder for writeoff domain operations

## Invariants

* Writeoff must have a valid identity (non-empty WriteoffId)
* Must reference stock or item (non-empty WriteoffReference)
* Quantity must be positive (no zero, no negative)
* Reason must be provided (no silent write-offs)
* Write-off is explicit and irreversible once confirmed (TERMINAL)
* Status must be a defined enum value

## Boundary Statement

This domain owns write-off definition only. Actual stock adjustment is handled by the stock domain reacting to writeoff events. No implicit stock mutation, no silent deletion.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* stock (writeoff references stock)
* item (writeoff references item)
* structural-system

## Lifecycle

Pending → Confirmed

**Pattern: TERMINAL** — Once confirmed, write-off is irreversible.

## Status

**S4 — Invariants + Specifications Complete**
