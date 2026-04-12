# Domain: Dispatch

## Classification

business-system

## Context

logistic

## Domain Responsibility

Defines the dispatch unit — the release action for a shipment. Tracks dispatch identity, shipment reference, and lifecycle state through a terminal progression representing the act of releasing a shipment for transport.

## Aggregate

* **DispatchAggregate** — Root entity for a dispatch
  * Event-sourced with full lifecycle management
  * Enforces invariants after every state change
  * Supports optimistic concurrency via `Version` property

## State Model (TERMINAL)

```
Created → Released → Completed
```

* Created: initial state on creation with shipment reference
* Released: dispatch has been released, shipment is handed off for transport
* Completed: dispatch process is finalized (terminal)

## Value Objects

* **DispatchId** — Unique identifier (validated, non-empty GUID)
* **ShipmentReference** — Reference to the associated shipment (validated, non-empty GUID)
* **DispatchStatus** — Lifecycle state enum (Created, Released, Completed)

## Events

* **DispatchCreatedEvent** — Raised when a new dispatch is created (carries Id, ShipmentReference)
* **DispatchReleasedEvent** — Raised when dispatch is released for transport
* **DispatchCompletedEvent** — Raised when dispatch process is finalized

## Invariants

* DispatchId must not be empty
* ShipmentReference must not be empty
* Status must be a defined enum value
* Lifecycle transitions are strictly terminal (no reversal)

## Specifications

* **CanReleaseSpecification** — Only Created dispatches can be released
* **CanCompleteSpecification** — Only Released dispatches can be completed

## Errors

* **MissingId** — DispatchId is required
* **ShipmentReferenceRequired** — Dispatch must reference a shipment
* **InvalidStateTransition** — Illegal lifecycle transition attempted
* **AlreadyCompleted** — Dispatch is completed and cannot be modified

## Domain Services

* **DispatchService** — Reserved for cross-aggregate coordination

## Boundary Statement

This domain defines dispatch structure and lifecycle only. It does NOT perform dispatch execution, shipment movement logic, routing algorithms, or transport coordination. Shipments are managed by the Shipment domain. Routes are managed by the Route domain.

## Status

**S4 — Invariants + Specifications Complete**
