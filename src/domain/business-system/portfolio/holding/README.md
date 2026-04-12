# Domain: Holding

## Classification

business-system

## Context

portfolio

## Domain Responsibility

Defines individual asset positions held within a portfolio. Tracks holding identity, asset reference, quantity, and sequential lifecycle state.

## Aggregate

* **HoldingAggregate** — Root entity for a holding position
  * Event-sourced with sequential lifecycle management
  * Enforces invariants after every state change
  * Supports optimistic concurrency via `Version` property

## State Model (SEQUENTIAL)

```
Opened → Active → Suspended → Closed
```

* Opened: initial state on creation
* Active: holding is live and tracked
* Suspended: holding is temporarily paused
* Closed: holding is permanently closed

## Value Objects

* **HoldingId** — Unique identifier (validated, non-empty GUID)
* **HoldingStatus** — Lifecycle state enum (Opened, Active, Suspended, Closed)
* **HoldingQuantity** — Position quantity (validated, must be > 0)
* **AssetReference** — Reference to held asset (validated, non-empty GUID)
* **PortfolioReference** — Reference to parent portfolio (validated, non-empty GUID)

## Events

* **HoldingCreatedEvent** — Raised when a new holding is created (carries Id, PortfolioReference, AssetReference, Quantity)
* **HoldingActivatedEvent** — Raised when holding transitions to Active
* **HoldingSuspendedEvent** — Raised when holding transitions to Suspended
* **HoldingClosedEvent** — Raised when holding is permanently closed

## Invariants

* HoldingId must not be empty
* PortfolioReference must not be empty
* AssetReference must not be empty
* HoldingQuantity must be greater than zero
* Status must be a defined enum value
* Lifecycle transitions must follow sequential order

## Specifications

* **CanActivateHoldingSpecification** — Only Opened holdings can be activated
* **CanSuspendSpecification** — Only Active holdings can be suspended
* **CanCloseHoldingSpecification** — Only Suspended holdings can be closed

## Errors

* **MissingId** — HoldingId is required
* **AssetReferenceRequired** — Holding must reference an asset
* **PortfolioReferenceRequired** — Holding must reference a portfolio
* **QuantityMustBePositive** — Quantity must be > 0
* **InvalidStateTransition** — Illegal lifecycle transition attempted

## Domain Services

* **HoldingService** — Reserved for cross-aggregate coordination

## Boundary Statement

This domain defines holding structure and position tracking only. It does NOT perform financial calculations, valuation, performance computation, or external data integration. Portfolio container is managed by the Portfolio domain.

## Status

**S4 — Invariants + Specifications Complete**
