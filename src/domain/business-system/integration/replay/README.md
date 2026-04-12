# Domain: Replay

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines behavior contracts only and contains no execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Can toggle between Active and Disabled states.

## Purpose

Defines replay eligibility rules for integration. A replay specifies the policy that determines whether and how events can be replayed, without containing any replay execution or message delivery logic.

## Core Responsibilities

* Define replay policy identity and reference
* Track replay lifecycle state (Defined → Active ↔ Disabled)
* Enforce policy definition before activation
* Ensure no replay execution or message delivery logic in domain

## Aggregate(s)

* ReplayAggregate
  * Manages the lifecycle and integrity of a replay policy definition

## Value Objects

* ReplayId — Unique identifier for a replay policy instance
* ReplayStatus — Enum for lifecycle state (Defined, Active, Disabled)
* ReplayPolicyId — Reference to the replay policy configuration

## Domain Events

* ReplayCreatedEvent — Raised when a new replay policy is defined
* ReplayActivatedEvent — Raised when the policy is activated
* ReplayDisabledEvent — Raised when the policy is disabled

## Specifications

* CanActivateSpecification — Defined or Disabled policies can be activated
* CanDisableSpecification — Only Active policies can be disabled
* IsActiveSpecification — Checks if policy is currently active

## Domain Services

* ReplayService — Domain operations for replay policy management

## Errors

* MissingId — ReplayId is required
* MissingPolicyId — ReplayPolicyId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Policy already in Active state
* AlreadyDisabled — Policy already in Disabled state

## Invariants

* ReplayId must not be null/default
* ReplayPolicyId must not be null/default
* ReplayStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* failure (replay eligibility may depend on failure classification)
* event-bridge (replay targets event bridge mappings)

## Status

**S4 — Invariants + Specifications Complete**
