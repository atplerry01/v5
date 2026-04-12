# Domain: Identity

## Classification

trust-system

## Context

identity

## Domain

identity

## Purpose

Defines identity structure — the foundational representation of a principal (person, service, or entity) within the trust system. This is the root identity aggregate from which all other identity-context domains derive their subject reference.

## Boundary

This domain defines identity structure only and contains no authentication or security execution logic. It does not perform token generation, credential validation, encryption, or any runtime security operations. Those concerns belong to sibling domains (credential, verification) or to the runtime/engine layers.

## Core Responsibilities

* Establish the core identity record with a validated identifier and descriptor
* Manage identity lifecycle transitions (Active, Suspended, Terminated)
* Emit domain events on all state changes
* Enforce invariants around identity integrity at every transition

## Aggregate(s)

* IdentityAggregate
  * Private constructor, event-sourced state via private Apply methods
  * Factory: `Establish(IdentityId, IdentityDescriptor)` — creates a new identity in Active status
  * Transitions: `Suspend()`, `Terminate()`
  * Maintains uncommitted events list for downstream consumption
  * Enforces invariants: Id not default, Descriptor not default, Status defined

## Entities

* None

## Value Objects

* IdentityId — Strongly-typed Guid identifier; rejects Guid.Empty
* IdentityStatus — Enum: Active, Suspended, Terminated
* IdentityDescriptor — Record struct with PrincipalName (string) and PrincipalType (string); both validated non-empty

## Domain Events

* IdentityEstablishedEvent(IdentityId, IdentityDescriptor) — Raised when a new identity is established
* IdentitySuspendedEvent(IdentityId) — Raised when an identity is suspended
* IdentityTerminatedEvent(IdentityId) — Raised when an identity is terminated

## Specifications

* CanSuspendSpecification — Satisfied when status is Active
* CanTerminateSpecification — Satisfied when status is Active or Suspended

## Domain Services

* IdentityService — Reserved for future domain coordination logic

## Errors

* IdentityErrors.MissingId() — IdentityId is required and must not be empty
* IdentityErrors.MissingDescriptor() — IdentityDescriptor is required and must not be default
* IdentityErrors.InvalidStateTransition(status, action) — Cannot perform action in current status

## Invariants

* Every identity must have a valid, non-default IdentityId
* Every identity must have a valid, non-default IdentityDescriptor
* IdentityStatus must always be a defined enum value
* Suspended or terminated identities must not transition to Active (no reactivation)

## Lifecycle

```
Active → Suspended → Terminated (terminal)
Active → Terminated (terminal, direct)
```

Terminated is a terminal state. There is no reactivation path.

## Policy Dependencies

* Identity establishment requirements, suspension rules, termination policies (WHYCEPOLICY controlled at runtime)

## Integration Points

* Profile — Identity profile data
* Credential — Authentication material bound to identity
* Trust — Trust score associated with identity
* Verification — Identity verification status
* Device — Devices bound to identity
* Identity Graph — Identity's position in the graph
* Access (context) — All access domains reference identity as principal

## Notes

This is the root domain of the identity context. All other identity-context domains (credential, device, profile, etc.) reference back to this domain's aggregate. Identity is the WhyceID anchor point. All identity policies are WHYCEPOLICY controlled.
