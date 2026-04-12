# Domain: Partner

## Classification

business-system

## Context

integration

## Boundary

This domain defines external partner structure only and contains no integration execution logic.

## Purpose

Defines the structure of integration partners — the external organizations or systems with which integrations are established.

## Core Responsibilities

* Define the structural identity and metadata of integration partners
* Track partner lifecycle states and transitions

## Aggregate(s)

* Partner

  * Represents an external organization or system with which integrations are established
  * Factory: Register(id, profile)
  * Transitions: Activate(), Suspend(), Deregister()

## Entities

* None

## Value Objects

* PartnerId — Validated Guid identifier for a partner instance
* PartnerStatus — Enum: Registered, Active, Suspended, Deregistered
* PartnerProfile — Record struct with PartnerName and PartnerType

## Domain Events

* PartnerRegisteredEvent(PartnerId, PartnerProfile) — Raised when a new partner is registered
* PartnerActivatedEvent(PartnerId) — Raised when a partner is activated
* PartnerSuspendedEvent(PartnerId) — Raised when a partner is suspended
* PartnerDeregisteredEvent(PartnerId) — Raised when a partner is deregistered

## Specifications

* CanActivateSpecification — status == Registered
* CanSuspendSpecification — status == Active
* CanDeregisterSpecification — status == Active OR Suspended

## Domain Services

* PartnerService — Empty (reserved for future domain operations)

## Errors

* MissingId — PartnerId is required
* MissingProfile — PartnerProfile is required
* InvalidStateTransition(status, action) — InvalidOperationException

## Invariants

* PartnerId must not be empty
* PartnerProfile must be provided
* State transitions must follow lifecycle rules
* No financial or execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Registered -> Active -> Suspended -> Deregistered (Active can also go directly to Deregistered)

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
