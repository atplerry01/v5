# Domain: Renewal

## Classification

business-system

## Context

subscription

## Domain Responsibility

Defines renewal lifecycle structure only. This domain models the initiation, completion, or failure of a subscription renewal. It contains no scheduling or payment logic.

## Boundary

This domain defines renewal lifecycle structure only and contains no scheduling or payment logic.

## Aggregate

* **RenewalAggregate** — Root aggregate representing a subscription renewal and its terminal lifecycle.
  * Private constructor; created via `Initiate(RenewalId, RenewalRequest)` factory method.
  * State transitions via `Complete()` and `Fail()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Pending ──Complete()──> Renewed (terminal)
Pending ──Fail()──────> Failed  (terminal)
```

## Value Objects

* **RenewalId** — Deterministic identifier (validated non-empty Guid).
* **RenewalStatus** — Enum: `Pending`, `Renewed`, `Failed`.
* **RenewalRequest** — Record struct containing `EnrollmentReference` (non-empty Guid) and `TermDescription` (non-empty string).

## Events

* **RenewalInitiatedEvent** — Raised when a new renewal is initiated (status: Pending).
* **RenewalCompletedEvent** — Raised when renewal completes successfully (status: Renewed).
* **RenewalFailedEvent** — Raised when renewal fails (status: Failed).

## Invariants

* RenewalId must not be null/default.
* RenewalRequest must not be default.
* RenewalStatus must be a defined enum value.
* State transitions enforced by specifications (only Pending allows transitions).

## Specifications

* **CanCompleteSpecification** — Validates that status is Pending before completing.
* **CanFailSpecification** — Validates that status is Pending before failing.

## Errors

* **MissingId** — RenewalId is required.
* **MissingRequest** — RenewalRequest is required.
* **InvalidStateTransition** — Guard for illegal status transitions (InvalidOperationException).

## Domain Services

* **RenewalService** — Reserved for cross-aggregate coordination within renewal context.

## Lifecycle

**TERMINAL** — Pending transitions to either Renewed or Failed. No further transitions allowed.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY.

## Notes

Business-system defines structure only. No financial, execution, scheduling, or workflow logic allowed.

## Status

**S4 — Invariants + Specifications Complete**
