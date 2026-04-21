# Domain: Limit

## Classification

business-system

## Context

entitlement

## Purpose

Defines and enforces boundary controls on entitlements. A limit represents a threshold that, when exceeded, triggers a breach. Limits gate access and usage within the entitlement context.

## Core Responsibilities

* Define limit identity, subject, and threshold
* Track limit lifecycle state (Defined → Enforced → Breached)
* Enforce threshold constraints deterministically
* Ensure breach only occurs when threshold is exceeded

## Aggregate(s)

* LimitAggregate
  * Manages the lifecycle and integrity of a boundary limit

## Value Objects

* LimitId — Unique identifier for a limit instance
* LimitStatus — Enum for lifecycle state (Defined, Enforced, Breached)
* LimitSubjectId — Reference to the subject the limit applies to

## Domain Events

* LimitCreatedEvent — Raised when a new limit is defined
* LimitEnforcedEvent — Raised when the limit is activated for enforcement
* LimitBreachedEvent — Raised when the limit threshold is exceeded

## Specifications

* CanEnforceSpecification — Only Defined limits can be enforced
* CanBreachSpecification — Only Enforced limits can be breached
* IsEnforcedSpecification — Checks if limit is currently enforced

## Errors

* MissingId — LimitId is required
* MissingSubjectId — LimitSubjectId is required
* InvalidThreshold — ThresholdValue must be greater than zero
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyEnforced — Limit already in Enforced state
* AlreadyBreached — Limit already in Breached state

## Invariants

* LimitId must not be null/default
* LimitSubjectId must not be null/default
* ThresholdValue must be greater than zero
* LimitStatus must be a defined enum value
* Breach requires observed value exceeding threshold
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* quota (limits may constrain quota capacity)
* restriction (breach may trigger restriction)

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Status

**S4 — Invariants + Specifications Complete**
