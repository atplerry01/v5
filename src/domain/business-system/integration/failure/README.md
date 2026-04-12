# Domain: Failure

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines behavior contracts only and contains no execution logic.

## Lifecycle Pattern

**TERMINAL** — Detected → Classified → Resolved. Resolved is final and cannot transition further.

## Purpose

Defines failure classification contracts for integration. A failure captures the type and classification of integration failures, without containing any recovery, retry execution, or remediation logic.

## Core Responsibilities

* Define failure identity and type reference
* Track failure lifecycle state (Detected → Classified → Resolved)
* Enforce classification before resolution
* Ensure Resolved state is terminal
* Ensure no recovery or retry execution logic in domain

## Aggregate(s)

* FailureAggregate
  * Manages the lifecycle and integrity of a failure classification

## Value Objects

* FailureId — Unique identifier for a failure instance
* FailureStatus — Enum for lifecycle state (Detected, Classified, Resolved)
* FailureTypeId — Reference to the failure type classification

## Domain Events

* FailureDetectedEvent — Raised when a new failure is detected
* FailureClassifiedEvent — Raised when the failure is classified
* FailureResolvedEvent — Raised when the failure is resolved (terminal)

## Specifications

* CanClassifySpecification — Only Detected failures can be classified
* CanResolveSpecification — Only Classified failures can be resolved
* IsResolvedSpecification — Checks if failure is in terminal Resolved state

## Domain Services

* FailureService — Domain operations for failure classification management

## Errors

* MissingId — FailureId is required
* MissingTypeId — FailureTypeId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyClassified — Failure already classified
* AlreadyResolved — Failure already resolved (terminal)

## Invariants

* FailureId must not be null/default
* FailureTypeId must not be null/default
* FailureStatus must be a defined enum value
* Classification requires a non-empty classification string
* Resolved is terminal (enforced by specifications)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* retry (failure classification may determine retry policy)
* connector (failures may trigger connector disconnection)

## Status

**S4 — Invariants + Specifications Complete**
