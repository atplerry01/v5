# Domain: Tracking

## Classification

business-system

## Context

logistic

## Domain Responsibility

Defines the tracking structure — the mechanism for recording tracking points associated with a shipment. Supports a reversible lifecycle where tracking can be paused and resumed, without relying on real-time updates or external tracking systems.

## Aggregate

* **TrackingAggregate** — Root entity for a tracking record
  * Event-sourced with full lifecycle management
  * Enforces invariants after every state change
  * Supports optimistic concurrency via `Version` property

## State Model (REVERSIBLE)

```
Created → Active ↔ Paused → Completed
```

* Created: initial state on creation with shipment reference and initial tracking point
* Active: tracking is actively recording points
* Paused: tracking is temporarily suspended (can resume to Active)
* Completed: tracking is finalized (terminal)

## Value Objects

* **TrackingId** — Unique identifier (validated, non-empty GUID)
* **ShipmentReference** — Reference to the associated shipment (validated, non-empty GUID)
* **TrackingPoint** — A single tracking checkpoint (validated, non-empty string)
* **TrackingStatus** — Lifecycle state enum (Created, Active, Paused, Completed)

## Events

* **TrackingCreatedEvent** — Raised when tracking is created (carries Id, ShipmentReference, InitialPoint)
* **TrackingActivatedEvent** — Raised when tracking begins active recording
* **TrackingPausedEvent** — Raised when tracking is temporarily suspended
* **TrackingResumedEvent** — Raised when tracking resumes from paused state
* **TrackingCompletedEvent** — Raised when tracking is finalized

## Invariants

* TrackingId must not be empty
* ShipmentReference must not be empty
* InitialPoint must not be empty
* Status must be a defined enum value
* Active ↔ Paused transitions are reversible
* Completed is terminal — no further state changes

## Specifications

* **CanActivateSpecification** — Only Created tracking can be activated
* **CanPauseSpecification** — Only Active tracking can be paused
* **CanResumeSpecification** — Only Paused tracking can be resumed
* **CanCompleteSpecification** — Only Active or Paused tracking can be completed

## Errors

* **MissingId** — TrackingId is required
* **ShipmentReferenceRequired** — Tracking must reference a shipment
* **TrackingPointRequired** — Tracking must have at least one tracking point
* **InvalidStateTransition** — Illegal lifecycle transition attempted
* **AlreadyCompleted** — Tracking is completed and cannot be modified

## Domain Services

* **TrackingService** — Reserved for cross-aggregate coordination

## Boundary Statement

This domain defines tracking structure and lifecycle only. It does NOT perform real-time tracking, GPS integration, location services, tracking system integration, or shipment movement logic. Shipments are managed by the Shipment domain. Routes are managed by the Route domain.

## Status

**S4 — Invariants + Specifications Complete**
