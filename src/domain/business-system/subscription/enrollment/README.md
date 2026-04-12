# Domain: Enrollment

## Classification

business-system

## Context

subscription

## Purpose

Defines the structure of subscription enrollments — the records of parties joining a subscription plan.

## Boundary

This domain defines enrollment structure only and contains no subscription activation or billing logic.

## Core Responsibilities

* Define and maintain enrollment record structure
* Track enrollment lifecycle (Pending, Active, Cancelled)
* Enforce structural rules for enrollment definitions

## Aggregate(s)

* EnrollmentAggregate

  * Factory: `Request(id, request)` — creates enrollment in Pending status
  * Transitions: `Activate()`, `Cancel()`
  * Apply + EnsureInvariants on every state change

## Entities

* None

## Value Objects

* EnrollmentId — Validated Guid (rejects Guid.Empty)
* EnrollmentStatus — Enum: Pending, Active, Cancelled
* EnrollmentRequest — Record struct with AccountReference (Guid, non-empty) and PlanReference (Guid, non-empty)

## Domain Events

* EnrollmentRequestedEvent(EnrollmentId, EnrollmentRequest) — Raised when an enrollment is requested
* EnrollmentActivatedEvent(EnrollmentId) — Raised when enrollment is activated
* EnrollmentCancelledEvent(EnrollmentId) — Raised when enrollment is cancelled

## Specifications

* CanActivateSpecification — Status must be Pending
* CanCancelSpecification — Status must be Pending or Active

## Domain Services

* EnrollmentService — Domain operations for enrollment management

## Errors

* MissingId — Enrollment ID must not be empty
* MissingRequest — Enrollment request must have non-empty references
* InvalidStateTransition(status, action) — Invalid lifecycle transition

## Invariants

* EnrollmentId must not be default/empty
* EnrollmentRequest must not be default/empty
* State transitions enforced by specifications

## Lifecycle

Pending -> Active -> Cancelled (also Pending -> Cancelled direct)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
