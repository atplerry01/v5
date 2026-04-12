# Domain: Charge

## Classification

business-system

## Context

execution

## Purpose

Manages the structural records of fees or costs associated with business execution. Tracks charge lifecycle from pending through charged or reversed states. Requires a cost source reference and at least one charge item before charging.

## Core Responsibilities

* Define charge identity and cost source reference
* Track charge lifecycle state (Pending → Charged → Reversed)
* Enforce that charges reference a valid cost source
* Ensure reversal only occurs after charging
* Maintain charge item inventory

## Aggregate(s)

* ChargeAggregate
  * Manages the lifecycle and integrity of a business charge

## Entities

* ChargeItem — Individual cost breakdown or unit charge (ItemId, Description)

## Value Objects

* ChargeId — Unique identifier for a charge instance
* ChargeStatus — Enum for lifecycle state (Pending, Charged, Reversed)
* CostSourceId — Reference to the action or resource incurring the cost

## Domain Events

* ChargeCreatedEvent — Raised when a new charge is created
* ChargeChargedEvent — Raised when charge is applied
* ChargeReversedEvent — Raised when charge is reversed

## Specifications

* CanChargeSpecification — Only Pending charges can be charged
* CanReverseSpecification — Only Charged charges can be reversed
* IsChargedSpecification — Checks if charge is currently active

## Domain Services

* ChargeService — Domain operations for charge management

## Errors

* MissingId — ChargeId is required
* MissingCostSourceId — CostSourceId is required
* ItemRequired — Charge must contain at least one charge item
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyCharged — Charge already in Charged state
* AlreadyReversed — Charge already in Reversed state

## Invariants

* ChargeId must not be null/default
* CostSourceId must not be null/default
* ChargeStatus must be a defined enum value
* Must contain at least one charge item before charging
* Cannot charge twice (enforced by CanChargeSpecification)
* Reversal only after charging (enforced by CanReverseSpecification)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* activation (charges may result from activations)
* allocation (charges may result from resource allocations)

## Status

**S4 — Invariants + Specifications Complete**
