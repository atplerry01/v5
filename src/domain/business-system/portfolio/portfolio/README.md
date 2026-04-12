# Domain: Portfolio

## Classification

business-system

## Context

portfolio

## Domain Responsibility

Defines the container for portfolio assets — the root business entity representing a collection of holdings managed as a unit. Tracks portfolio identity and lifecycle state.

## Aggregate

* **PortfolioAggregate** — Root entity for a portfolio
  * Event-sourced with full lifecycle management
  * Enforces invariants after every state change
  * Supports optimistic concurrency via `Version` property

## State Model (TERMINAL)

```
Draft → Active → Closed → Terminated
```

* Draft: initial state on creation
* Active: portfolio is open for holdings
* Closed: portfolio is closed, no new holdings
* Terminated: final state, irreversible

## Value Objects

* **PortfolioId** — Unique identifier (validated, non-empty GUID)
* **PortfolioName** — Portfolio name (validated, non-empty string)
* **PortfolioStatus** — Lifecycle state enum (Draft, Active, Closed, Terminated)

## Events

* **PortfolioCreatedEvent** — Raised when a new portfolio is created (carries Id, Name)
* **PortfolioActivatedEvent** — Raised when portfolio transitions to Active
* **PortfolioClosedEvent** — Raised when portfolio transitions to Closed
* **PortfolioTerminatedEvent** — Raised when portfolio is permanently terminated

## Invariants

* PortfolioId must not be empty
* PortfolioName must not be empty
* Status must be a defined enum value
* Terminal state (Terminated) is irreversible

## Specifications

* **CanActivateSpecification** — Only Draft portfolios can be activated
* **CanCloseSpecification** — Only Active portfolios can be closed
* **CanTerminateSpecification** — Only Closed portfolios can be terminated

## Errors

* **MissingId** — PortfolioId is required
* **NameRequired** — Portfolio must have a name
* **InvalidStateTransition** — Illegal lifecycle transition attempted
* **AlreadyTerminated** — Portfolio is terminated and cannot be modified

## Domain Services

* **PortfolioService** — Reserved for cross-aggregate coordination

## Boundary Statement

This domain defines portfolio structure and lifecycle only. It does NOT perform financial calculations, portfolio optimization, performance computation, or external data integration. Holdings are managed by the Holding domain.

## Status

**S4 — Invariants + Specifications Complete**
