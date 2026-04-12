# Domain: Workspace

## Classification

business-system

## Context

resource

## Domain Responsibility

Defines working context for resources. Represents logical or physical work areas that scope operational activity. This domain defines resource usage contracts, not execution.

## Aggregate

* **WorkspaceAggregate** — Root aggregate representing a workspace.
  * Private constructor; created via `Create(WorkspaceId, WorkspaceScope, WorkspaceLabel)` factory method.
  * State transitions via `Activate()` and `Decommission()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model (TERMINAL)

```
Provisioned ──Activate()──> Active ──Decommission()──> Decommissioned
```

## Value Objects

* **WorkspaceId** — Deterministic identifier (validated non-empty Guid).
* **WorkspaceStatus** — Enum: `Provisioned`, `Active`, `Decommissioned`.
* **WorkspaceScope** — Defines the scope/context of the workspace (validated non-empty string).
* **WorkspaceLabel** — Human-readable label for the workspace (validated non-empty string).

## Events

* **WorkspaceCreatedEvent** — Raised when a new workspace is created (status: Provisioned).
* **WorkspaceActivatedEvent** — Raised when workspace is activated.
* **WorkspaceDecommissionedEvent** — Raised when workspace is decommissioned (terminal).

## Invariants

* WorkspaceId must not be null/default.
* Scope must be defined (non-empty).
* Label must be defined (non-empty).
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Provisioned workspaces can be activated.
* **CanDecommissionSpecification** — Only Active workspaces can be decommissioned.

## Errors

* **MissingId** — WorkspaceId is required.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **ScopeRequired** — Workspace must define a scope.
* **LabelRequired** — Workspace must have a label.

## Domain Services

* **WorkspaceService** — Reserved for cross-aggregate coordination within workspace context.

## Boundary Statement

This domain defines working context contracts only. No scheduling logic, no execution logic, no time-driven behavior, no background processes.

## Status

**S4 — Invariants + Specifications Complete**
