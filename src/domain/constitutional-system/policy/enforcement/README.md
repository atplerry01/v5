# Domain: Enforcement

## Classification

constitutional-system

## Context

policy

## Purpose

This domain defines enforcement structure only and contains no enforcement execution logic.

## Core Responsibilities

* Define enforcement action structure and identity
* Define enforcement status lifecycle (Pending, Applied, Withdrawn)
* Define enforcement action references to policy decisions

## Aggregate(s)

* EnforcementAggregate

  * Factory: Record(id, action) — creates a new enforcement in Pending status
  * Transitions: ApplyEnforcement() (Pending → Applied), Withdraw() (Applied → Withdrawn)

## Entities

* None

## Value Objects

* EnforcementId — Validated Guid identifier for an enforcement record
* EnforcementStatus — Enum: Pending, Applied, Withdrawn
* EnforcementAction — Record struct with DecisionReference (Guid, non-empty) and ActionType (string, non-empty)

## Domain Events

* EnforcementRecordedEvent(EnforcementId, EnforcementAction) — Raised when an enforcement is recorded
* EnforcementAppliedEvent(EnforcementId) — Raised when an enforcement transitions to Applied
* EnforcementWithdrawnEvent(EnforcementId) — Raised when an enforcement is withdrawn

## Specifications

* CanApplySpecification — Status must be Pending
* CanWithdrawSpecification — Status must be Applied

## Domain Services

* EnforcementService — Reserved for future domain operations

## Invariants

* EnforcementId must not be empty
* EnforcementAction must not be default
* Status must be a defined EnforcementStatus value

## Lifecycle

REVERSIBLE: Pending → Applied → Withdrawn

## Boundary

This domain defines enforcement structure only and contains no enforcement execution logic.
All enforcement execution is external via WHYCEPOLICY and runtime.
