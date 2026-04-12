# Domain: Retention

## Status

S4 — Invariants + Specifications Complete

## Classification

business-system

## Context

document

## Purpose

Defines the structure of document retention rules — the policies governing how long documents must be preserved and the lifecycle transitions from active through retained to expired.

## Core Responsibilities

* Define retention rule structure and metadata
* Track retention identity and classification
* Maintain retention lifecycle state (Active -> Retained -> Expired)
* Enforce state transition invariants via specifications
* Emit domain events on all state changes

## Aggregate(s)

* RetentionAggregate
  * Event-sourced aggregate managing the lifecycle and integrity of a retention entity
  * Factory: `Create(RetentionId)` — produces a new aggregate in Active status
  * Commands: `Retain()`, `Expire()`
  * Tracks uncommitted events and version

## Entities

* None

## Value Objects

* RetentionId — Validated unique identifier (readonly record struct, rejects Guid.Empty)
* RetentionStatus — Enum: Active, Retained, Expired

## Domain Events

* RetentionCreatedEvent(RetentionId) — Raised when a new retention is created
* RetentionRetainedEvent(RetentionId) — Raised when retention transitions to Retained
* RetentionExpiredEvent(RetentionId) — Raised when retention transitions to Expired

## Specifications

* CanRetainSpecification — Satisfied when status is Active
* CanExpireSpecification — Satisfied when status is Retained
* IsRetainedSpecification — Satisfied when status is Retained

## Domain Errors

* RetentionDomainException — Sealed domain exception
* RetentionErrors — Static factory: MissingId, InvalidStateTransition, RetentionConditionNotMet

## Domain Services

* RetentionService — Scaffold for future domain services

## Invariants

* RetentionId must not be default (Guid.Empty)
* RetentionStatus must be a defined enum value
* State transitions enforced: Active -> Retained -> Expired
* Cannot Retain unless status is Active
* Cannot Expire unless status is Retained

## State Machine

```
Active --[Retain]--> Retained --[Expire]--> Expired
```

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
