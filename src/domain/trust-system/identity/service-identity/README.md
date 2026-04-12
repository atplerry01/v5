# Domain: ServiceIdentity

## Classification

trust-system

## Context

identity

## Purpose

Represents the domain responsible for managing service identities — non-human principals (applications, services, APIs, bots) that operate within the trust system and require their own identity, credentials, and trust relationships.

## Core Responsibilities

* Register and manage service-level identities distinct from human identities
* Enforce service identity policies including scope and capability limits
* Emit events when service identities are created, modified, or decommissioned

## Aggregate(s)

* ServiceIdentityAggregate
  * Enforces invariants around service identity creation and capability scope
  * Validates service identity policies before committing changes

## Entities

* None

## Value Objects

* ServiceIdentityId — Strongly-typed identifier for a service identity record

## Domain Events

* ServiceIdentityCreatedEvent — Raised when a new service identity is registered
* ServiceIdentityStateChangedEvent — Raised when service identity state transitions
* ServiceIdentityUpdatedEvent — Raised when a service identity is modified

## Specifications

* ServiceIdentitySpecification — Validates service identity eligibility and scope criteria

## Domain Services

* ServiceIdentityService — Coordinates service identity lifecycle management logic

## Invariants

* Service identities must have a defined owner (human identity or organisational unit)
* Service identity capabilities must not exceed their defined scope
* Decommissioned service identities must cascade revocation to dependent credentials and sessions

## Policy Dependencies

* Service identity registration rules, capability scope limits, rotation schedules (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Service identities are a specialised identity type
* Credential — Service credentials (API keys, certificates)
* Registry — Service identity registration
* Access (context) — Service identities participate in access control
* Governance — Service identity audit trail

## Lifecycle

Registered → Active → Updated → Suspended → Decommissioned. All transitions emit domain events and enforce invariants.

## Notes

Service identities are first-class principals in the trust system. They are not second-class or delegated — they have their own credentials, trust levels, and access policies. Service identity governance is WHYCEPOLICY controlled.
