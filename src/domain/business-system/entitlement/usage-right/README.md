# Domain: UsageRight

## Classification

business-system

## Context

entitlement

## Purpose

Manages usage control for entitlement rights. A usage right tracks the consumption of a specific right by a subject, enforcing usage limits and recording consumption events.

## Core Responsibilities

* Define usage right identity, subject, and right reference
* Track usage lifecycle state (Available -> InUse -> Consumed)
* Enforce usage limits against available units
* Record consumption via UsageRecord entities
* Ensure deterministic consumption tracking

## Aggregate(s)

* UsageRightAggregate
  * Manages the lifecycle and integrity of a usage right

## Entities

* UsageRecord — Tracks individual usage events and consumption units

## Value Objects

* UsageRightId — Unique identifier for a usage right instance
* UsageRightStatus — Enum for lifecycle state (Available, InUse, Consumed)
* UsageRightSubjectId — Reference to the subject using the right
* UsageRightReferenceId — Reference to the right being used

## Domain Events

* UsageRightCreatedEvent — Raised when a new usage right is created
* UsageRightUsedEvent — Raised when units are used against the right
* UsageRightConsumedEvent — Raised when the usage right is fully consumed

## Specifications

* CanUseSpecification — Available or InUse rights can be used
* CanConsumeSpecification — Only InUse rights can be consumed
* IsAvailableSpecification — Checks if usage right has available units

## Domain Services

* UsageRightService — Domain operations for usage right management

## Errors

* MissingId — UsageRightId is required
* MissingSubjectId — UsageRightSubjectId is required
* MissingReferenceId — UsageRightReferenceId is required
* InvalidTotalUnits — TotalUnits must be greater than zero
* InvalidStateTransition — Attempted transition not allowed from current status
* UsageExceedsAvailable — Requested usage exceeds remaining units
* UsageRemaining — Cannot consume with units still remaining
* AlreadyConsumed — Usage right already fully consumed

## Invariants

* UsageRightId must not be null/default
* UsageRightSubjectId must not be null/default
* UsageRightReferenceId must not be null/default
* TotalUnits must be greater than zero
* TotalUsed cannot exceed TotalUnits
* UsageRightStatus must be a defined enum value
* Consumption requires Remaining == 0
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* right (usage right references an active right)
* quota (usage may be constrained by quota)
* limit (usage may be constrained by limit)
* restriction (restriction may block usage)

## Status

**S4 — Invariants + Specifications Complete**
