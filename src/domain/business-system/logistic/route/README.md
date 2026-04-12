# Domain: Route

## Classification

business-system

## Context

logistic

## Domain Responsibility

Defines the movement path — the ordered sequence of waypoints for transporting goods between locations. Routes are immutable once locked, ensuring path integrity throughout the logistics lifecycle.

## Aggregate

* **RouteAggregate** — Root entity for a route
  * Event-sourced with full lifecycle management
  * Enforces invariants after every state change
  * Supports optimistic concurrency via `Version` property

## State Model (TERMINAL)

```
Draft → Defined → Locked
```

* Draft: initial state on creation with path definition
* Defined: route path has been validated and accepted
* Locked: route is immutable and committed for use (terminal)

## Value Objects

* **RouteId** — Unique identifier (validated, non-empty GUID)
* **Waypoint** — A single point on the route path (validated, non-empty string)
* **RoutePath** — Ordered collection of waypoints (validated, minimum two waypoints)
* **RouteStatus** — Lifecycle state enum (Draft, Defined, Locked)

## Events

* **RouteCreatedEvent** — Raised when a new route is created (carries Id, Path)
* **RouteDefinedEvent** — Raised when route is validated and accepted
* **RouteLockedEvent** — Raised when route is locked and made immutable

## Invariants

* RouteId must not be empty
* RoutePath must contain at least two waypoints
* Status must be a defined enum value
* Route is immutable once Locked — no further state changes permitted

## Specifications

* **CanDefineSpecification** — Only Draft routes can be defined
* **CanLockSpecification** — Only Defined routes can be locked

## Errors

* **MissingId** — RouteId is required
* **PathRequired** — Route must have a defined path with at least two waypoints
* **InvalidStateTransition** — Illegal lifecycle transition attempted
* **AlreadyLocked** — Route is locked and cannot be modified

## Domain Services

* **RouteService** — Reserved for cross-aggregate coordination

## Boundary Statement

This domain defines route structure and path definition only. It does NOT perform routing algorithms, path optimization, distance calculation, GPS integration, or real-time navigation. Shipments are managed by the Shipment domain. Tracking is managed by the Tracking domain.

## Status

**S4 — Invariants + Specifications Complete**
