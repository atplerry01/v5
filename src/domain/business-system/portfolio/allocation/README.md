# Domain: Allocation

## Classification

business-system

## Context

portfolio

## Domain Responsibility

Defines distribution rules for portfolio assets across target segments. Tracks allocation identity, weight, target reference, and reversible lifecycle state.

## Aggregate

* **AllocationAggregate** — Root entity for an allocation rule
  * Event-sourced with reversible lifecycle management
  * Enforces invariants after every state change
  * Supports optimistic concurrency via `Version` property

## State Model (REVERSIBLE)

```
Proposed → Applied ↔ Reverted
```

* Proposed: initial state on creation
* Applied: allocation rule is in effect
* Reverted: allocation rule has been rolled back (can be re-applied)

## Value Objects

* **AllocationId** — Unique identifier (validated, non-empty GUID)
* **AllocationStatus** — Lifecycle state enum (Proposed, Applied, Reverted)
* **AllocationWeight** — Distribution weight (validated, > 0 and <= 1)
* **TargetReference** — Reference to allocation target (validated, non-empty GUID)
* **AllocationPortfolioReference** — Reference to parent portfolio (validated, non-empty GUID)

## Events

* **AllocationCreatedEvent** — Raised when a new allocation is created (carries Id, PortfolioReference, TargetReference, Weight)
* **AllocationAppliedEvent** — Raised when allocation transitions to Applied
* **AllocationRevertedEvent** — Raised when allocation transitions to Reverted

## Invariants

* AllocationId must not be empty
* PortfolioReference must not be empty
* TargetReference must not be empty
* AllocationWeight must be > 0 and <= 1 (cannot exceed total allocation bounds)
* Status must be a defined enum value

## Specifications

* **CanApplySpecification** — Only Proposed or Reverted allocations can be applied
* **CanRevertSpecification** — Only Applied allocations can be reverted

## Errors

* **MissingId** — AllocationId is required
* **TargetReferenceRequired** — Allocation must reference a target
* **PortfolioReferenceRequired** — Allocation must reference a portfolio
* **WeightOutOfBounds** — Weight must be > 0 and <= 1
* **InvalidStateTransition** — Illegal lifecycle transition attempted

## Domain Services

* **AllocationService** — Reserved for cross-aggregate coordination

## Boundary Statement

This domain defines allocation distribution rules only. It does NOT perform portfolio optimization, financial calculations, rebalancing logic, or external data integration. Weight represents a structural distribution rule, not a computed value.

## Status

**S4 — Invariants + Specifications Complete**
