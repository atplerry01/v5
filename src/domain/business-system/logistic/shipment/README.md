# Domain: Shipment

## Classification

business-system

## Context

logistic

## Domain Responsibility

Defines the shipment unit — the container of goods being transported from origin to destination. Tracks shipment identity, item references, origin/destination, and lifecycle state through a sequential progression.

## Aggregate

* **ShipmentAggregate** — Root entity for a shipment
  * Event-sourced with full lifecycle management
  * Enforces invariants after every state change
  * Supports optimistic concurrency via `Version` property

## State Model (SEQUENTIAL)

```
Created → Packed → Dispatched → InTransit → Delivered
```

* Created: initial state on creation with origin, destination, and item reference
* Packed: shipment is prepared for dispatch
* Dispatched: shipment has been released for transport
* InTransit: shipment is moving between origin and destination
* Delivered: shipment has arrived at destination (terminal)

## Value Objects

* **ShipmentId** — Unique identifier (validated, non-empty GUID)
* **Origin** — Origin location (validated, non-empty string)
* **Destination** — Destination location (validated, non-empty string)
* **ItemReference** — Reference to items/resources in shipment (validated, non-empty GUID)
* **ShipmentStatus** — Lifecycle state enum (Created, Packed, Dispatched, InTransit, Delivered)

## Events

* **ShipmentCreatedEvent** — Raised when a new shipment is created (carries Id, Origin, Destination, ItemReference)
* **ShipmentPackedEvent** — Raised when shipment is packed for dispatch
* **ShipmentDispatchedEvent** — Raised when shipment is released for transport
* **ShipmentInTransitEvent** — Raised when shipment begins transit
* **ShipmentDeliveredEvent** — Raised when shipment arrives at destination

## Invariants

* ShipmentId must not be empty
* Origin must not be empty
* Destination must not be empty
* ItemReference must not be empty
* Status must be a defined enum value
* Lifecycle transitions are strictly sequential

## Specifications

* **CanPackSpecification** — Only Created shipments can be packed
* **CanDispatchSpecification** — Only Packed shipments can be dispatched
* **CanMarkInTransitSpecification** — Only Dispatched shipments can be marked in transit
* **CanDeliverSpecification** — Only InTransit shipments can be delivered

## Errors

* **MissingId** — ShipmentId is required
* **OriginRequired** — Shipment must have an origin
* **DestinationRequired** — Shipment must have a destination
* **ItemReferenceRequired** — Shipment must reference items
* **InvalidStateTransition** — Illegal lifecycle transition attempted
* **AlreadyDelivered** — Shipment is delivered and cannot be modified

## Domain Services

* **ShipmentService** — Reserved for cross-aggregate coordination

## Boundary Statement

This domain defines shipment structure and lifecycle only. It does NOT perform shipment movement logic, routing algorithms, dispatch execution, GPS tracking, or real-time tracking integration. Dispatch is managed by the Dispatch domain. Routes are managed by the Route domain.

## Status

**S4 — Invariants + Specifications Complete**
