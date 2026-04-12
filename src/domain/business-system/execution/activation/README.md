# Domain: Activation

## Classification

business-system

## Context

execution

## Purpose

Manages the lifecycle of business activations — tracking when entities or services are brought into active operation and subsequently deactivated. Requires a valid target reference for every activation.

## Core Responsibilities

* Define activation identity and target reference
* Track activation lifecycle state (Pending → Active → Deactivated)
* Enforce that activation requires a valid target
* Ensure deactivation only occurs after activation

## Aggregate(s)

* ActivationAggregate
  * Manages the lifecycle and integrity of a business activation

## Value Objects

* ActivationId — Unique identifier for an activation instance
* ActivationStatus — Enum for lifecycle state (Pending, Active, Deactivated)
* ActivationTargetId — Reference to the entity or service being activated

## Domain Events

* ActivationCreatedEvent — Raised when a new activation is created
* ActivationActivatedEvent — Raised when activation is confirmed
* ActivationDeactivatedEvent — Raised when activation is deactivated

## Specifications

* CanActivateSpecification — Only Pending activations can be activated
* CanDeactivateSpecification — Only Active activations can be deactivated
* IsActiveSpecification — Checks if activation is currently active

## Domain Services

* ActivationService — Domain operations for activation management

## Errors

* MissingId — ActivationId is required
* MissingTargetId — ActivationTargetId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Activation already in Active state
* AlreadyDeactivated — Activation already in Deactivated state

## Invariants

* ActivationId must not be null/default
* ActivationTargetId must not be null/default
* ActivationStatus must be a defined enum value
* Cannot activate twice (enforced by CanActivateSpecification)
* Deactivation only after activation (enforced by CanDeactivateSpecification)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* allocation (activation may trigger resource allocation)
* charge (activation may incur charges)

## Status

**S4 — Invariants + Specifications Complete**
