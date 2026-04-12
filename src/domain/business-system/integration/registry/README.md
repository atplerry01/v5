# Domain: Registry

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines coordination contracts only and contains no execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Can toggle between Active and Deactivated states.

## Domain Responsibility

Models lookup and discovery entry definitions for integration. A registry entry defines what is discoverable in the integration catalog, without executing any actual lookup or resolution logic.

## Aggregate

* **RegistryAggregate** — Root aggregate managing registry entry lifecycle.
  * Private constructor; created via `Create(RegistryId, RegistryEntryId)` factory method.
  * State transitions via `Activate()` and `Deactivate()` methods.
  * Event-sourced with optimistic concurrency via `Version`.

## State Model

```
Defined ──Activate()──> Active ──Deactivate()──> Deactivated ──Activate()──> Active
```

## Value Objects

* **RegistryId** — Deterministic identifier (validated non-empty Guid).
* **RegistryStatus** — Enum: `Defined`, `Active`, `Deactivated`.
* **RegistryEntryId** — Reference to the registry entry definition (validated non-empty Guid).

## Specifications

* **CanActivateSpecification** — Defined or Deactivated entries can be activated (REVERSIBLE).
* **CanDeactivateSpecification** — Only Active entries can be deactivated.
* **IsActiveSpecification** — Checks if entry is currently active.

## Errors

* **MissingId** — RegistryId is required.
* **MissingEntryId** — RegistryEntryId is required.
* **AlreadyActive** / **AlreadyDeactivated** / **InvalidStateTransition**

## Status

**S4 — Invariants + Specifications Complete**
