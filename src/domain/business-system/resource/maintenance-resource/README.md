# Domain: MaintenanceResource

## Classification

business-system

## Context

resource

## Domain Responsibility

Defines maintenance requirements for resources and equipment. Tracks maintenance lifecycle from definition through activation, suspension, and completion. This domain defines resource usage contracts, not execution.

## Aggregate

* **MaintenanceResourceAggregate** — Root aggregate representing a maintenance resource requirement.
  * Private constructor; created via `Create(MaintenanceResourceId, ResourceLink, MaintenanceRequirement)` factory method.
  * State transitions via `Activate()`, `Suspend()`, `Resume()`, and `Complete()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model (REVERSIBLE)

```
Defined ──Activate()──> Active ──Suspend()──> Suspended ──Resume()──> Active
                        Active ──Complete()──> Completed
```

## Value Objects

* **MaintenanceResourceId** — Deterministic identifier (validated non-empty Guid).
* **MaintenanceResourceStatus** — Enum: `Defined`, `Active`, `Suspended`, `Completed`.
* **ResourceLink** — Reference to the resource or equipment requiring maintenance (validated non-empty Guid).
* **MaintenanceRequirement** — Description of the maintenance condition (validated non-empty string).

## Events

* **MaintenanceResourceCreatedEvent** — Raised when a new maintenance resource is created (status: Defined).
* **MaintenanceResourceActivatedEvent** — Raised when maintenance is activated (or resumed).
* **MaintenanceResourceSuspendedEvent** — Raised when maintenance is suspended.
* **MaintenanceResourceCompletedEvent** — Raised when maintenance is completed.

## Invariants

* MaintenanceResourceId must not be null/default.
* Must link to a resource or equipment (ResourceLink must not be default).
* Must define a maintenance requirement (non-empty).
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Defined maintenance resources can be activated.
* **CanSuspendSpecification** — Only Active maintenance resources can be suspended.
* **CanResumeSpecification** — Only Suspended maintenance resources can be resumed.
* **CanCompleteSpecification** — Only Active maintenance resources can be completed.

## Errors

* **MissingId** — MaintenanceResourceId is required.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **RequirementRequired** — Maintenance resource must define a requirement.
* **ResourceLinkRequired** — Must link to a resource or equipment.

## Domain Services

* **MaintenanceResourceService** — Reserved for cross-aggregate coordination within maintenance-resource context.

## Boundary Statement

This domain defines maintenance requirement contracts only. No scheduling logic, no execution logic, no time-driven behavior, no background processes.

## Status

**S4 — Invariants + Specifications Complete**
