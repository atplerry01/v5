# Domain: Adjustment

## Classification

business-system

## Context

billing

## Domain Responsibility

Models financial corrections applied to billing records after initial generation. Tracks adjustment lifecycle from draft through applied or voided states. Enforces that every adjustment carries a reason and cannot be applied twice.

## Aggregate

* **AdjustmentAggregate** — Root aggregate representing a billing adjustment instance.
  * Private constructor; created via `Create(AdjustmentId, reason)` factory method.
  * State transitions via `ApplyAdjustment()` and `VoidAdjustment()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Draft ──ApplyAdjustment()──> Applied ──VoidAdjustment()──> Voided (terminal)
```

## Entities

* None

## Value Objects

* **AdjustmentId** — Deterministic identifier (validated non-empty Guid).
* **AdjustmentStatus** — Enum: `Draft`, `Applied`, `Voided`.

## Events

* **AdjustmentCreatedEvent** — Raised when a new adjustment is created (status: Draft).
* **AdjustmentAppliedEvent** — Raised when adjustment is applied to billing records.
* **AdjustmentVoidedEvent** — Raised when a previously applied adjustment is voided.

## Invariants

* AdjustmentId must not be null/default.
* AdjustmentStatus must be a defined enum value.
* Adjustment must have a reason (enforced at creation and via invariant check).
* Cannot apply an already-applied adjustment (enforced by CanApplyAdjustmentSpecification: only Draft allows apply).
* Void only allowed after apply (enforced by CanVoidAdjustmentSpecification: only Applied allows void).

## Specifications

* **CanApplyAdjustmentSpecification** — Validates that status is Draft before applying.
* **CanVoidAdjustmentSpecification** — Validates that status is Applied before voiding.
* **HasReasonSpecification** — Validates that adjustment has a non-empty reason.

## Errors

* **MissingId** — AdjustmentId is required.
* **MissingReason** — Adjustment must have a reason.
* **AlreadyApplied** — Cannot apply an already-applied adjustment.
* **AlreadyVoided** — Cannot void an already-voided adjustment.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **AdjustmentService** — Reserved for cross-aggregate coordination within adjustment context.

## Status

**S4 — Invariants + Specifications Complete**
