# Domain: EntitlementGrant

## Classification

business-system

## Context

entitlement

## Purpose

Manages the formal granting and revocation of entitlements to subjects. An entitlement grant represents the bestowing of a specific right to a specific subject, tracked through its full lifecycle.

## Core Responsibilities

* Define entitlement grant identity and assignment mapping
* Track grant lifecycle state (Pending → Granted → Revoked)
* Enforce that grants cannot be duplicated
* Ensure revocation immediately invalidates access
* Map subjects to rights via EntitlementAssignment entity

## Aggregate(s)

* EntitlementGrantAggregate
  * Manages the lifecycle and integrity of an entitlement grant

## Entities

* EntitlementAssignment — Maps a subject to a specific right

## Value Objects

* EntitlementGrantId — Unique identifier for an entitlement grant instance
* EntitlementGrantStatus — Enum for lifecycle state (Pending, Granted, Revoked)
* GrantSubjectId — Reference to the subject receiving the grant
* EntitlementRightId — Reference to the right being granted

## Domain Events

* EntitlementGrantCreatedEvent — Raised when a new entitlement grant is created
* EntitlementGrantGrantedEvent — Raised when the entitlement is granted
* EntitlementGrantRevokedEvent — Raised when the entitlement is revoked

## Specifications

* CanGrantSpecification — Only Pending grants can be granted
* CanRevokeSpecification — Only Granted entitlements can be revoked
* IsGrantedSpecification — Checks if grant is currently active

## Domain Services

* EntitlementGrantService — Domain operations for entitlement grant management

## Errors

* MissingId — EntitlementGrantId is required
* MissingAssignment — EntitlementAssignment is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyGranted — Grant already in Granted state
* AlreadyRevoked — Grant already in Revoked state
* CannotRevokeBeforeGrant — Cannot revoke before granting

## Invariants

* EntitlementGrantId must not be null/default
* EntitlementAssignment must not be null
* EntitlementGrantStatus must be a defined enum value
* Cannot grant twice (enforced by CanGrantSpecification)
* Revocation only from Granted state (enforced by CanRevokeSpecification)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* eligibility (eligibility evaluation gates grant decisions)
* allocation (grant may trigger resource allocation)

## Status

**S4 — Invariants + Specifications Complete**
