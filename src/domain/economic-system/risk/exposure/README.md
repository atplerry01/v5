# Domain: Exposure

## Classification
economic-system

## Context
risk

## Domain Responsibility
Defines risk exposure positions — the tracked boundary of financial risk for a given source. This domain defines risk position structure only and contains no risk calculation logic.

## Aggregate
* **ExposureAggregate** — Root aggregate representing a risk exposure position.
  * Private constructor; created via `Create(ExposureId, SourceId, ExposureType, Amount, Currency, Timestamp)` factory method.
  * State transitions via `IncreaseExposure()`, `ReduceExposure()`, and `CloseExposure()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Active ──ReduceExposure()──> Reduced ──IncreaseExposure()──> Active
Active ──CloseExposure()───> Closed (terminal)
Reduced ──CloseExposure()──> Closed (terminal)
```

## Value Objects
* **ExposureId** — Deterministic identifier (validated non-empty Guid).
* **ExposureStatus** — Enum: `Active`, `Reduced`, `Closed`.
* **ExposureType** — Enum: `Allocation`, `Obligation`, `Transaction`.
* **SourceId** — Typed reference to exposure source.

## Events
* **ExposureCreatedEvent** — Raised when exposure is created (status: Active).
* **ExposureIncreasedEvent** — Raised when exposure is increased.
* **ExposureReducedEvent** — Raised when exposure is reduced.
* **ExposureClosedEvent** — Raised when exposure is closed (terminal).

## Invariants
* ExposureId must not be null/default.
* SourceId must not be null/default.
* TotalExposure must be >= 0.
* Must not perform risk calculations — position structure only.
* State transitions enforced by specifications.

## Specifications
* **CanIncreaseSpecification** — Non-closed exposures can be increased.
* **CanReduceSpecification** — Non-closed exposures can be reduced.
* **CanCloseSpecification** — Non-closed exposures can be closed.
* **ExposureThresholdSpecification** — Validates exposure against threshold limit.

## Errors
* **MissingId** — ExposureId is required.
* **InvalidExposureAmount** — Amount must be positive.
* **AlreadyClosed** — Cannot modify a closed exposure.
* **ReductionExceedsTotal** — Cannot reduce below zero.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services
* **ExposureService** — Reserved for cross-aggregate coordination within exposure context.

## Lifecycle Pattern
REVERSIBLE — Exposure can be increased and reduced; closed is terminal.

## Boundary Statement
This domain defines risk position structure only and contains no risk calculation logic.

## Status
**S4 — Invariants + Specifications Complete**
