# Domain: Retry

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines behavior contracts only and contains no execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Can toggle between Active and Disabled states.

## Purpose

Defines retry policy contracts for integration. A retry defines the policy rules (max attempts, strategy metadata) that govern how retries should be handled, without containing any retry execution, timer, or backoff logic.

## Core Responsibilities

* Define retry policy identity and reference
* Track retry lifecycle state (Defined → Active ↔ Disabled)
* Enforce policy definition before activation
* Ensure no retry execution, timer, or scheduling logic in domain

## Aggregate(s)

* RetryAggregate
  * Manages the lifecycle and integrity of a retry policy definition

## Value Objects

* RetryId — Unique identifier for a retry policy instance
* RetryStatus — Enum for lifecycle state (Defined, Active, Disabled)
* RetryPolicyId — Reference to the retry policy configuration

## Domain Events

* RetryCreatedEvent — Raised when a new retry policy is defined
* RetryActivatedEvent — Raised when the policy is activated
* RetryDisabledEvent — Raised when the policy is disabled

## Specifications

* CanActivateSpecification — Defined or Disabled policies can be activated
* CanDisableSpecification — Only Active policies can be disabled
* IsActiveSpecification — Checks if policy is currently active

## Domain Services

* RetryService — Domain operations for retry policy management

## Errors

* MissingId — RetryId is required
* MissingPolicyId — RetryPolicyId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Policy already in Active state
* AlreadyDisabled — Policy already in Disabled state

## Invariants

* RetryId must not be null/default
* RetryPolicyId must not be null/default
* RetryStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* failure (retry policies may reference failure classifications)
* connector (retries govern connector reconnection policy)

## Status

**S4 — Invariants + Specifications Complete**
