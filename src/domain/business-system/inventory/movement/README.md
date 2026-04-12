# Domain: Movement

## Classification

business-system

## Context

inventory

## Boundary Statement

This domain defines stock movement records. No database logic or direct stock mutation.

## Lifecycle Pattern

**TERMINAL** — Pending → Confirmed or Cancelled. Both are final states.

## Purpose

Defines stock movement records for inventory. A movement captures the transfer of quantity from a source to a target, with immutable confirmation or cancellation.

## Core Responsibilities

* Define movement identity, source, target, and quantity
* Track movement lifecycle state (Pending → Confirmed | Cancelled)
* Enforce positive quantity constraint
* Ensure immutability after confirmation or cancellation

## Aggregate(s)

* MovementAggregate
  * Manages the lifecycle and integrity of a stock movement

## Value Objects

* MovementId — Unique identifier for a movement instance
* MovementStatus — Enum for lifecycle state (Pending, Confirmed, Cancelled)
* MovementSourceId — Reference to the movement source
* MovementTargetId — Reference to the movement target
* MovementQuantity — Positive quantity value object

## Domain Events

* MovementCreatedEvent — Raised when a new movement is created
* MovementConfirmedEvent — Raised when the movement is confirmed (terminal)
* MovementCancelledEvent — Raised when the movement is cancelled (terminal)

## Specifications

* CanConfirmSpecification — Only Pending movements can be confirmed
* CanCancelSpecification — Only Pending movements can be cancelled
* IsConfirmedSpecification — Checks if movement is confirmed

## Domain Services

* MovementService — Domain operations for movement management

## Errors

* MissingId — MovementId is required
* MissingSourceId — MovementSourceId is required
* MissingTargetId — MovementTargetId is required
* InvalidQuantity — Movement quantity must be greater than zero
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyConfirmed — Movement already confirmed (terminal)
* AlreadyCancelled — Movement already cancelled (terminal)

## Invariants

* MovementId must not be null/default
* MovementSourceId must not be null/default
* MovementTargetId must not be null/default
* MovementQuantity must be greater than zero
* MovementStatus must be a defined enum value
* Confirmed and Cancelled are terminal (enforced by specifications)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* stock (movements affect stock quantity)
* item (movements reference inventory items)

## Status

**S4 — Invariants + Specifications Complete**
