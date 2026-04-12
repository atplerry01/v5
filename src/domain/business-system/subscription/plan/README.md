# Domain: Plan

## Classification

business-system

## Context

subscription

## Boundary

This domain defines subscription plan structure only and contains no billing or pricing logic.

## Purpose

Defines the structure of subscription plans — the defined tiers and configurations available for subscription.

## Core Responsibilities

* Define and maintain subscription plan structure
* Track defined tiers and configurations for subscription
* Enforce structural rules for plan definitions

## Aggregate(s)

* PlanAggregate

  * Root aggregate representing a structured subscription plan and its lifecycle
  * Factory: Draft(id, descriptor)
  * Transitions: Activate(), Deprecate()

## Entities

* None

## Value Objects

* PlanId — Validated Guid identifier for a plan instance
* PlanStatus — Enum: Draft, Active, Deprecated
* PlanDescriptor — Record struct with PlanName (string, non-empty) and PlanTier (string, non-empty)

## Domain Events

* PlanDraftedEvent(PlanId, PlanDescriptor) — Raised when a new plan is drafted
* PlanActivatedEvent(PlanId) — Raised when a plan is activated
* PlanDeprecatedEvent(PlanId) — Raised when a plan is deprecated

## Specifications

* CanActivateSpecification — Satisfied when status is Draft
* CanDeprecateSpecification — Satisfied when status is Active

## Domain Services

* PlanService — Domain operations for plan management

## Errors

* MissingId — PlanId is required and must not be empty
* MissingDescriptor — Plan must include a valid descriptor
* InvalidStateTransition(status, action) — InvalidOperationException for illegal transitions

## Invariants

* PlanId must not be default/empty
* PlanDescriptor must not be default/empty
* PlanStatus must be a defined enum value
* No financial or execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

TERMINAL: Draft -> Active -> Deprecated

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
