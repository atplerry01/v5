# Domain: Reservation

## Classification

business-system

## Context

inventory

## Boundary Statement

This domain defines temporary inventory allocation. No database logic or implicit stock mutation.

## Lifecycle Pattern

**REVERSIBLE** — Reserved → Confirmed or Released. Both Confirm and Release are available from Reserved state.

## Purpose

Manages temporary inventory allocations. A reservation holds a quantity of an item, which can either be confirmed (converting to permanent allocation) or released (returning to available stock).

## Core Responsibilities

* Define reservation identity, item reference, and quantity
* Track reservation lifecycle state (Reserved → Confirmed | Released)
* Enforce positive quantity constraint
* Support both confirmation and release from Reserved state

## Aggregate(s)

* ReservationAggregate
  * Manages the lifecycle and integrity of an inventory reservation

## Value Objects

* ReservationId — Unique identifier for a reservation instance
* ReservationStatus — Enum for lifecycle state (Reserved, Confirmed, Released)
* ReservationItemId — Reference to the reserved item
* ReservedQuantity — Positive quantity value object

## Domain Events

* ReservationCreatedEvent — Raised when a new reservation is made
* ReservationConfirmedEvent — Raised when the reservation is confirmed
* ReservationReleasedEvent — Raised when the reservation is released

## Specifications

* CanConfirmSpecification — Only Reserved reservations can be confirmed
* CanReleaseSpecification — Only Reserved reservations can be released
* IsReservedSpecification — Checks if reservation is currently reserved

## Domain Services

* ReservationService — Domain operations for reservation management

## Errors

* MissingId — ReservationId is required
* MissingItemId — ReservationItemId is required
* InvalidQuantity — Reserved quantity must be greater than zero
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyConfirmed — Reservation already confirmed
* AlreadyReleased — Reservation already released

## Invariants

* ReservationId must not be null/default
* ReservationItemId must not be null/default
* ReservedQuantity must be greater than zero
* ReservationStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* item (reservations reference items)
* stock (reservations reduce available stock logically)
* movement (confirmation may trigger movement)

## Status

**S4 — Invariants + Specifications Complete**
