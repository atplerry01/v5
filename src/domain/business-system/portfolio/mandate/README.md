# Domain: Mandate

## Classification

business-system

## Context

portfolio

## Purpose

Defines the structure of portfolio mandates — the governing rules and constraints that define how a portfolio must be managed.

## Boundary Statement

This domain defines portfolio behavior contracts only and contains no execution logic.

## Core Responsibilities

* Define and maintain mandate constraint structure and metadata
* Track mandate lifecycle state transitions
* Enforce structural invariants for mandate consistency
* Must define enforceable constraints, must not contain execution or optimization logic

## Aggregate(s)

* MandateAggregate
  * Represents the root entity for a portfolio mandate, encapsulating its structure and lifecycle

## Value Objects

* MandateId — Unique identifier for a mandate instance
* MandateName — Descriptive name for the mandate
* MandateStatus — Lifecycle state (Draft, Enforced, Revoked)

## Domain Events

* MandateCreatedEvent — Raised when a new mandate is created
* MandateEnforcedEvent — Raised when a mandate is put into enforcement
* MandateRevokedEvent — Raised when a mandate is revoked
* MandateUpdatedEvent — Raised when mandate metadata is updated
* MandateStateChangedEvent — Raised when mandate lifecycle state transitions

## Specifications

* MandateSpecification — Validates mandate structure and completeness
* CanEnforceSpecification — Draft → Enforced transition guard
* CanRevokeSpecification — Enforced → Revoked transition guard

## Domain Services

* MandateService — Reserved for cross-aggregate coordination within mandate context

## Invariants

* MandateId must not be empty
* MandateName must not be empty
* Status must be a defined enum value
* Must define constraints, must be enforceable via rules
* No financial or execution logic allowed

## Lifecycle Pattern

TERMINAL: Draft → Enforced → Revoked

* Draft: Initial state upon creation
* Enforced: Mandate is actively governing portfolio behavior
* Revoked: Terminal state, mandate is no longer in effect

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
