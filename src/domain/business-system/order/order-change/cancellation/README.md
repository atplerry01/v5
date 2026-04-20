# Domain: Cancellation

## Classification

business-system

## Context

subscription

## Purpose

Defines the structure of subscription cancellations — the lifecycle from request through confirmation.

## Boundary

This domain defines cancellation structure only and contains no refund or billing execution logic.

## Core Responsibilities

* Define and maintain cancellation record structure
* Track cancellation requests and their confirmation
* Enforce structural rules for cancellation lifecycle transitions

## Aggregate(s)

* CancellationAggregate

  * Root aggregate representing a structured cancellation record and its lifecycle
  * Factory: `RequestCancellation(id, request)` — creates a new cancellation in Requested status
  * Transition: `Confirm()` — moves from Requested to Confirmed (terminal)

## Entities

* None

## Value Objects

* CancellationId — Validated unique identifier for a cancellation instance (Guid, non-empty)
* CancellationStatus — Enum: Requested, Confirmed
* CancellationRequest — Record struct with EnrollmentReference (Guid, non-empty) and Reason (string, non-empty)

## Domain Events

* CancellationRequestedEvent(CancellationId, CancellationRequest) — Raised when a new cancellation is requested
* CancellationConfirmedEvent(CancellationId) — Raised when a cancellation is confirmed

## Specifications

* CanConfirmSpecification — Validates that status is Requested before allowing confirmation

## Domain Services

* CancellationService — Placeholder for future cross-aggregate coordination

## Errors

* MissingId — Cancellation ID must not be empty
* MissingRequest — Cancellation request must have a non-empty enrollment reference and reason
* InvalidStateTransition(status, action) — Cannot perform action in current status

## Invariants

* CancellationId must be non-empty
* CancellationRequest must be non-default (valid enrollment reference and reason)
* Status must be a defined enum value
* No refund or billing execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* subscription/enrollment (read-only reference via EnrollmentReference)

## Lifecycle

**TERMINAL**: Requested -> Confirmed

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
