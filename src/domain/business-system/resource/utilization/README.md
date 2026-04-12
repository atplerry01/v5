# Domain: Utilization

## Classification

business-system

## Context

resource

## Domain Responsibility

Defines usage measurement for resources. Tracks utilization from initiation through recording to completion. This domain defines resource usage contracts, not execution.

## Aggregate

* **UtilizationAggregate** — Root aggregate representing resource utilization tracking.
  * Private constructor; created via `Create(UtilizationId, ResourceReference, CapacityLimit)` factory method.
  * State transitions via `StartRecording()`, `RecordUsage(UsageAmount)`, and `Complete()` methods.
  * Tracks cumulative usage against capacity limits.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model (SEQUENTIAL)

```
Initiated ──StartRecording()──> Recording ──Complete()──> Completed
                                Recording ──RecordUsage()──> Recording (cumulative)
```

## Value Objects

* **UtilizationId** — Deterministic identifier (validated non-empty Guid).
* **UtilizationStatus** — Enum: `Initiated`, `Recording`, `Completed`.
* **ResourceReference** — Reference to the resource being tracked (validated non-empty Guid).
* **CapacityLimit** — Maximum capacity constraint (must be > 0).
* **UsageAmount** — Recorded usage quantity (must be >= 0).

## Events

* **UtilizationCreatedEvent** — Raised when a new utilization record is created (status: Initiated).
* **UtilizationRecordingStartedEvent** — Raised when recording begins.
* **UtilizationRecordedEvent** — Raised when usage is recorded (incremental).
* **UtilizationCompletedEvent** — Raised when utilization tracking is completed.

## Invariants

* UtilizationId must not be null/default.
* Must reference a resource (ResourceReference must not be default).
* Capacity limit must be greater than zero.
* Cumulative usage must not exceed capacity limit.
* Usage amount must not be negative.
* State transitions enforced by specifications.

## Specifications

* **CanStartRecordingSpecification** — Only Initiated utilizations can start recording.
* **CanRecordUsageSpecification** — Only Recording utilizations can record usage.
* **CanCompleteSpecification** — Only Recording utilizations can be completed.

## Errors

* **MissingId** — UtilizationId is required.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **ResourceReferenceRequired** — Utilization must reference a resource.
* **UsageMustNotBeNegative** — Usage amount must be >= 0.
* **ExceedsCapacityConstraint** — Cumulative usage exceeds capacity limit.
* **CapacityLimitRequired** — Capacity limit must be > 0.

## Domain Services

* **UtilizationService** — Reserved for cross-aggregate coordination within utilization context.

## Boundary Statement

This domain defines usage measurement contracts only. No scheduling logic, no execution logic, no time-driven behavior, no background processes.

## Status

**S4 — Invariants + Specifications Complete**
