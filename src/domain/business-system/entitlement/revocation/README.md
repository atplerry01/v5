# Domain: Revocation

## Classification

business-system

## Context

entitlement

## Purpose

Manages the termination of entitlements and grants. A revocation targets a specific entitlement or grant, transitions through revocation, and can be finalized to permanently lock the revocation state.

## Core Responsibilities

* Define revocation identity and target reference
* Track revocation lifecycle state (Active → Revoked → Finalized)
* Enforce single-revocation constraint
* Ensure finalization permanently locks state

## Aggregate(s)

* RevocationAggregate
  * Manages the lifecycle and integrity of an entitlement revocation

## Value Objects

* RevocationId — Unique identifier for a revocation instance
* RevocationStatus — Enum for lifecycle state (Active, Revoked, Finalized)
* RevocationTargetId — Reference to the entitlement or grant being revoked

## Domain Events

* RevocationCreatedEvent — Raised when a new revocation is initiated
* RevocationRevokedEvent — Raised when the target is revoked
* RevocationFinalizedEvent — Raised when the revocation is permanently locked

## Specifications

* CanRevokeSpecification — Only Active revocations can be revoked
* CanFinalizeSpecification — Only Revoked revocations can be finalized
* IsRevokedSpecification — Checks if revocation is in Revoked state

## Domain Services

* RevocationService — Domain operations for revocation management

## Errors

* MissingId — RevocationId is required
* MissingTargetId — RevocationTargetId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyRevoked — Revocation already in Revoked state
* AlreadyFinalized — Revocation already finalized and locked

## Invariants

* RevocationId must not be null/default
* RevocationTargetId must not be null/default
* RevocationStatus must be a defined enum value
* Revocation reason is mandatory
* Cannot revoke twice (enforced by CanRevokeSpecification)
* Finalization only from Revoked state (enforced by CanFinalizeSpecification)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* entitlement-grant (revocation targets grants)
* right (revocation may deprecate associated rights)
* restriction (revocation may trigger restriction)

## Status

**S4 — Invariants + Specifications Complete**
