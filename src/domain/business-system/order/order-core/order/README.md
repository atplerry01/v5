# Domain: Order

## Classification

business-system

## Context

marketplace

## Boundary Statement

This domain defines marketplace order commitment structure. No fulfillment, payment, or execution logic.

## Lifecycle Pattern

**SEQUENTIAL** — Created → Confirmed → Completed. Immutable after confirmation. No reversal.

## Purpose

Defines a marketplace order — a commitment to transact, based on an offer or listing.

## Core Responsibilities

* Define order identity, source reference, and description
* Track order lifecycle state (Created → Confirmed → Completed)
* Enforce immutability after confirmation
* Ensure no implicit state changes

## Aggregate(s)

* OrderAggregate
  * Manages the lifecycle and integrity of a marketplace order

## Value Objects

* OrderId — Unique identifier for an order instance
* OrderStatus — Enum for lifecycle state (Created, Confirmed, Completed)
* OrderSourceReference — Reference to the offer or listing that originated this order

## Domain Events

* OrderCreatedEvent — Raised when a new order is created
* OrderConfirmedEvent — Raised when the order is confirmed (immutable after this)
* OrderCompletedEvent — Raised when the order is completed (terminal)

## Specifications

* CanConfirmOrderSpecification — Only Created orders can be confirmed
* CanCompleteOrderSpecification — Only Confirmed orders can be completed
* IsConfirmedOrderSpecification — Checks if order is currently confirmed

## Errors

* MissingId — OrderId is required
* MissingSourceReference — OrderSourceReference is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyConfirmed — Order already confirmed and immutable

## Invariants

* OrderId must not be null/default
* OrderSourceReference must not be null/default (must be based on offer or listing)
* OrderDescription must not be null or empty
* OrderStatus must be a defined enum value
* Order is immutable after confirmation (enforced by sequential lifecycle)
* Confirm/Complete governed by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* offer (orders are based on accepted offers)
* listing (orders may reference listings)
* match (orders may produce matches)

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Status

**S4 — Invariants + Specifications Complete**
