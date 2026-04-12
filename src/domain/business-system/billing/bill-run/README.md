# Domain: BillRun

## Classification

business-system

## Context

billing

## Domain Responsibility

Models the batch processing cycles that generate billing records for a period. Tracks bill run lifecycle from created through running to completed or failed terminal states. Maintains item inventory for each run.

## Aggregate

* **BillRunAggregate** — Root aggregate representing a bill run instance.
  * Private constructor; created via `Create(BillRunId)` factory method.
  * State transitions via `Start()`, `Complete()`, and `Fail()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Created ──Start()──> Running ──Complete()──> Completed (terminal)
                     Running ──Fail()──> Failed (terminal)
```

## Entities

* **BillRunItem** — Individual item in the bill run (InvoiceReference, Label). Must have at least one before starting.

## Value Objects

* **BillRunId** — Deterministic identifier (validated non-empty Guid).
* **BillRunStatus** — Enum: `Created`, `Running`, `Completed`, `Failed`.

## Events

* **BillRunCreatedEvent** — Raised when a new bill run is created (status: Created).
* **BillRunStartedEvent** — Raised when bill run processing begins.
* **BillRunCompletedEvent** — Raised when bill run completes successfully.
* **BillRunFailedEvent** — Raised when bill run encounters a failure.

## Invariants

* BillRunId must not be null/default.
* BillRunStatus must be a defined enum value.
* Must contain at least one item before starting.
* Cannot complete or fail unless running.

## Specifications

* **CanStartSpecification** — Validates that status is Created before starting.
* **CanCompleteSpecification** — Validates that status is Running before completing.
* **CanFailSpecification** — Validates that status is Running before failing.

## Errors

* **MissingId** — BillRunId is required.
* **ItemRequired** — BillRun must contain at least one item.
* **AlreadyRunning** — Cannot start an already-running bill run.
* **AlreadyCompleted** — Cannot transition a completed bill run.
* **AlreadyFailed** — Cannot transition a failed bill run.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **BillRunService** — Reserved for cross-aggregate coordination within bill run context.

## Status

**S4 — Invariants + Specifications Complete**
