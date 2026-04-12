# Domain: Permission

## Classification

trust-system

## Context

access

## Purpose

Defines permission structure — the representation of named capabilities. This domain defines permission structure only and contains no access control enforcement logic.

## Core Responsibilities

* Define discrete permission units representing specific capabilities
* Manage permission lifecycle through terminal state transitions (Defined -> Active -> Deprecated)
* Emit events when permissions are defined, activated, or deprecated

## Aggregate(s)

* PermissionAggregate
  * Factory: Define(id, descriptor) — creates a permission in Defined status
  * Transitions: Activate(), Deprecate()
  * Enforces invariants around permission identity, descriptor validity, and status

## Entities

* None

## Value Objects

* PermissionId — Validated Guid identifier for a permission record
* PermissionStatus — Enum: Defined, Active, Deprecated
* PermissionDescriptor — Record struct with PermissionName (string, non-empty) and ResourceType (string, non-empty)

## Domain Events

* PermissionDefinedEvent(PermissionId, PermissionDescriptor) — Raised when a new permission is defined
* PermissionActivatedEvent(PermissionId) — Raised when a permission is activated
* PermissionDeprecatedEvent(PermissionId) — Raised when a permission is deprecated

## Specifications

* CanActivateSpecification — Satisfied when status is Defined
* CanDeprecateSpecification — Satisfied when status is Active

## Domain Services

* PermissionService — Empty (no cross-aggregate coordination required)

## Errors

* MissingId — PermissionId is required and must not be empty
* MissingDescriptor — Permission must include a valid descriptor
* InvalidStateTransition(status, action) — InvalidOperationException for illegal transitions

## Invariants

* PermissionId must not be default/empty
* PermissionDescriptor must not be default/empty
* PermissionStatus must be a defined enum value
* State transitions must follow terminal lifecycle: Defined -> Active -> Deprecated

## Lifecycle

**TERMINAL**: Defined -> Active -> Deprecated. No reverse transitions. All transitions emit domain events and enforce invariants.

## Boundary

This domain defines permission structure only and contains no access control enforcement logic.

## Policy Dependencies

* Permission naming conventions, resource type classification (WHYCEPOLICY controlled)

## Integration Points

* Role — Permissions are assigned to roles
* Grant — Permissions define what grants confer
* Authorization — Permissions are evaluated during authorization decisions
* Governance — Permission definitions subject to governance audit
