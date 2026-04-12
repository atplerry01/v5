# Domain: Restriction

## Classification

business-system

## Context

entitlement

## Purpose

Defines and enforces access control restrictions on entitlements. A restriction represents a condition that must be satisfied; violation triggers enforcement action, and lifting restores access after resolution.

## Core Responsibilities

* Define restriction identity, subject, and condition
* Track restriction lifecycle state (Active → Violated → Lifted)
* Enforce violation deterministically when condition is breached
* Ensure lifting only occurs after violation

## Aggregate(s)

* RestrictionAggregate
  * Manages the lifecycle and integrity of an access restriction

## Value Objects

* RestrictionId — Unique identifier for a restriction instance
* RestrictionStatus — Enum for lifecycle state (Active, Violated, Lifted)
* RestrictionSubjectId — Reference to the subject the restriction applies to

## Domain Events

* RestrictionCreatedEvent — Raised when a new restriction is created
* RestrictionViolatedEvent — Raised when the restriction condition is violated
* RestrictionLiftedEvent — Raised when the restriction is lifted after violation

## Specifications

* CanViolateSpecification — Only Active restrictions can be violated
* CanLiftSpecification — Only Violated restrictions can be lifted
* IsActiveSpecification — Checks if restriction is currently active

## Domain Services

* RestrictionService — Domain operations for restriction management

## Errors

* MissingId — RestrictionId is required
* MissingSubjectId — RestrictionSubjectId is required
* MissingCondition — ConditionDescription is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyViolated — Restriction already in Violated state
* AlreadyLifted — Restriction already in Lifted state

## Invariants

* RestrictionId must not be null/default
* RestrictionSubjectId must not be null/default
* ConditionDescription must not be null or empty
* RestrictionStatus must be a defined enum value
* Violation requires a reason
* Lift only from Violated state
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* limit (breach of limit may trigger restriction)
* entitlement-grant (restriction may block or revoke grants)

## Status

**S4 — Invariants + Specifications Complete**
