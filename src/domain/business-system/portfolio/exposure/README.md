# Domain: Exposure

## Classification

business-system

## Context

portfolio

## Domain Responsibility

Defines risk and position visibility within a portfolio. Tracks exposure identity, context, defined limits, and reversible lifecycle state for breach/clear cycles.

## Aggregate

* **ExposureAggregate** — Root entity for an exposure record
  * Event-sourced with reversible lifecycle management
  * Enforces invariants after every state change
  * Supports optimistic concurrency via `Version` property

## State Model (REVERSIBLE)

```
Defined → Active ↔ Breached → Cleared
                                  ↓
                               Active (re-activation)
```

* Defined: initial state on creation
* Active: exposure monitoring is live
* Breached: exposure limit has been exceeded (can be cleared)
* Cleared: breach has been resolved (can be re-activated)

## Value Objects

* **ExposureId** — Unique identifier (validated, non-empty GUID)
* **ExposureStatus** — Lifecycle state enum (Defined, Active, Breached, Cleared)
* **ExposureLimit** — Maximum exposure threshold (validated, must be > 0)
* **ExposureContext** — Reference defining the scope of exposure (validated, non-empty GUID)

## Events

* **ExposureCreatedEvent** — Raised when a new exposure is created (carries Id, ExposureContext, Limit)
* **ExposureActivatedEvent** — Raised when exposure transitions to Active
* **ExposureBreachedEvent** — Raised when exposure limit is exceeded
* **ExposureClearedEvent** — Raised when breach is resolved

## Invariants

* ExposureId must not be empty
* ExposureContext must not be empty
* ExposureLimit must be greater than zero
* Status must be a defined enum value
* Exposure must not exceed defined limits

## Specifications

* **CanActivateExposureSpecification** — Only Defined or Cleared exposures can be activated
* **CanBreachSpecification** — Only Active exposures can be breached
* **CanClearSpecification** — Only Breached exposures can be cleared

## Errors

* **MissingId** — ExposureId is required
* **ContextRequired** — Exposure must define a context
* **LimitRequired** — Exposure must define a limit > 0
* **LimitExceeded** — Exposure value exceeds defined limit
* **InvalidStateTransition** — Illegal lifecycle transition attempted

## Domain Services

* **ExposureService** — Reserved for cross-aggregate coordination

## Boundary Statement

This domain defines exposure structure and limit tracking only. It does NOT perform exposure calculations, risk computation, portfolio optimization, or external data integration. Breach detection is a state transition signal, not a calculation engine.

## Status

**S4 — Invariants + Specifications Complete**
