# Domain: Right

## Classification

business-system

## Context

entitlement

## Purpose

Defines entitlement capabilities — the specific rights that can be granted to subjects. A right represents a formal definition of what a subject is entitled to, including scope, capability, and constraints.

## Core Responsibilities

* Define right identity and capability definition
* Track right lifecycle state (Defined → Active → Deprecated)
* Enforce that activation requires a complete definition
* Ensure deprecated rights cannot be reused for new grants

## Aggregate(s)

* RightAggregate
  * Manages the lifecycle and integrity of an entitlement right definition

## Entities

* RightDefinition — Defines the scope, capability, and constraints of the right

## Value Objects

* RightId — Unique identifier for a right instance
* RightStatus — Enum for lifecycle state (Defined, Active, Deprecated)
* RightScopeId — Reference to the scope the right applies to

## Domain Events

* RightCreatedEvent — Raised when a new right is defined
* RightActivatedEvent — Raised when the right is activated for use
* RightDeprecatedEvent — Raised when the right is deprecated

## Specifications

* CanActivateSpecification — Only Defined rights can be activated
* CanDeprecateSpecification — Only Active rights can be deprecated
* IsActiveSpecification — Checks if right is currently active

## Domain Services

* RightService — Domain operations for right management

## Errors

* MissingId — RightId is required
* MissingDefinition — RightDefinition is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Right already in Active state
* AlreadyDeprecated — Right already in Deprecated state

## Invariants

* RightId must not be null/default
* RightDefinition must not be null
* RightStatus must be a defined enum value
* Cannot activate without complete definition (enforced by entity validation)
* Deprecated rights are terminal (enforced by CanDeprecateSpecification)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* entitlement-grant (rights are granted to subjects)
* usage-right (active rights enable usage)
* revocation (revocation may deprecate rights)

## Status

**S4 — Invariants + Specifications Complete**
