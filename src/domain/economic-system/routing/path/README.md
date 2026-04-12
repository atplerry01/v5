# Domain: Path

## Classification
economic-system

## Context
routing

## Domain Responsibility
Defines routing path structures — the defined routes with conditions and priority for economic flow routing. This domain defines routing structure only and contains no routing execution logic.

## Aggregate
* **RoutingPathAggregate** — Root aggregate representing a routing path definition.
  * Private constructor; created via `DefinePath(PathId, PathType, string, int)` factory method.
  * State transitions via `Activate()` and `Disable()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Defined ──Activate()──> Active ──Disable()──> Disabled (terminal)
```

## Value Objects
* **PathId** — Deterministic identifier (validated non-empty Guid).
* **RoutingPathStatus** — Enum: `Defined`, `Active`, `Disabled`.
* **PathType** — Enum: `Internal`, `External`, `MultiHop`.

## Events
* **RoutingPathDefinedEvent** — Raised when a routing path is defined (status: Defined).
* **RoutingPathActivatedEvent** — Raised when path is activated.
* **RoutingPathDisabledEvent** — Raised when path is disabled (terminal).

## Invariants
* PathId must not be null/default.
* Conditions must not be empty.
* Priority must be greater than zero.
* Must not perform routing — structure definition only.
* State transitions enforced by specifications.

## Specifications
* **CanActivatePathSpecification** — Only Defined paths with valid conditions and priority can be activated.
* **CanDisablePathSpecification** — Only Active paths can be disabled.

## Errors
* **InvalidPathCondition** — Conditions must not be empty.
* **InvalidPriority** — Priority must be positive.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services
* **RoutingPathService** — Validates path eligibility for routing decisions.

## Lifecycle Pattern
TERMINAL — Once disabled, a path cannot be reactivated.

## Boundary Statement
This domain defines routing structure only and contains no routing execution logic.

## Status
**S4 — Invariants + Specifications Complete**
