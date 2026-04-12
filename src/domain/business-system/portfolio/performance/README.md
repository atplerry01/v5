# Domain: Performance

## Classification

business-system

## Context

portfolio

## Purpose

Defines the structure of portfolio performance tracking — the measurement context for monitoring portfolio outcomes over time.

## Boundary Statement

This domain defines portfolio behavior contracts only and contains no execution logic.

## Core Responsibilities

* Define and maintain performance measurement structure and metadata
* Track performance lifecycle state transitions
* Enforce structural invariants for performance tracking consistency
* Must not compute values or perform calculations

## Aggregate(s)

* PerformanceAggregate
  * Represents the root entity for a portfolio performance record, encapsulating its structure and lifecycle

## Value Objects

* PerformanceId — Unique identifier for a performance instance
* PerformanceName — Descriptive name for the performance tracking context
* PerformanceStatus — Lifecycle state (Draft, Active, Suspended, Closed)

## Domain Events

* PerformanceCreatedEvent — Raised when a new performance record is created
* PerformanceActivatedEvent — Raised when performance tracking is activated
* PerformanceSuspendedEvent — Raised when performance tracking is suspended
* PerformanceResumedEvent — Raised when performance tracking is resumed from suspension
* PerformanceClosedEvent — Raised when performance tracking is closed
* PerformanceUpdatedEvent — Raised when performance metadata is updated
* PerformanceStateChangedEvent — Raised when performance lifecycle state transitions

## Specifications

* PerformanceSpecification — Validates performance structure and completeness
* CanActivateSpecification — Draft → Active transition guard
* CanSuspendSpecification — Active → Suspended transition guard
* CanResumeSpecification — Suspended → Active transition guard (reversible)
* CanCloseSpecification — Active → Closed transition guard

## Domain Services

* PerformanceService — Reserved for cross-aggregate coordination within performance context

## Invariants

* PerformanceId must not be empty
* PerformanceName must not be empty
* Status must be a defined enum value
* Must define measurement context, must not compute values
* No financial or execution logic allowed

## Lifecycle Pattern

REVERSIBLE: Draft → Active ↔ Suspended → Closed

* Draft: Initial state upon creation
* Active: Performance tracking is live
* Suspended: Temporarily paused (can resume to Active)
* Closed: Terminal state, no further transitions

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
