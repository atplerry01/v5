# Domain: Usage

## Classification

business-system

## Context

subscription

## Boundary

This domain defines usage tracking structure only and contains no metering or calculation logic.

## Core Responsibilities

* Define and maintain usage record structure
* Track lifecycle of usage records (Recorded, Aggregated, Finalized)
* Enforce structural invariants for usage definitions

## Aggregate(s)

* UsageAggregate

  * Root aggregate representing a structured usage record and its terminal lifecycle
  * Factory: `Record(UsageId, UsageRecord)` — creates a new usage in Recorded status
  * Transitions: `Aggregate()` — moves to Aggregated; `Finalize()` — moves to Finalized (terminal)

## Entities

* None

## Value Objects

* UsageId — Validated unique identifier (non-empty Guid)
* UsageStatus — Enum: Recorded, Aggregated, Finalized
* UsageRecord — Record struct with EnrollmentReference (Guid, non-empty) and MetricName (string, non-empty)

## Domain Events

* UsageRecordedEvent(UsageId, UsageRecord) — Raised when a new usage is recorded
* UsageAggregatedEvent(UsageId) — Raised when usage is aggregated
* UsageFinalizedEvent(UsageId) — Raised when usage is finalized (terminal)

## Specifications

* CanAggregateSpecification — Satisfied when status is Recorded
* CanFinalizeSpecification — Satisfied when status is Aggregated

## Domain Services

* UsageService — Reserved for cross-aggregate coordination within subscription context

## Errors

* MissingId — UsageId is required and must not be empty
* MissingRecord — UsageRecord is required
* InvalidStateTransition(status, action) — InvalidOperationException for invalid lifecycle transitions

## Invariants

* UsageId must not be empty
* UsageRecord must not be default
* Status must be a defined enum value
* Lifecycle is terminal: Recorded -> Aggregated -> Finalized

## Lifecycle

**TERMINAL**: Recorded -> Aggregated -> Finalized

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Notes

Business-system defines structure only. No metering, calculation, financial, execution, or workflow logic allowed.
