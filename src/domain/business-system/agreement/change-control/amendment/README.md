# Domain: Amendment

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models formal modifications to existing agreement terms after initial execution. Each amendment references a target (contract or clause) and follows a lifecycle from draft through application to optional reversion.

## Aggregate

* **AmendmentAggregate** — Root aggregate representing a formal modification to agreement terms.
  * Private constructor; created via `Create(AmendmentId, AmendmentTargetId)` factory method.
  * State transitions via `ApplyAmendment()` and `RevertAmendment()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Draft ──ApplyAmendment()──> Applied ──RevertAmendment()──> Reverted (terminal)
```

## Value Objects

* **AmendmentId** — Deterministic identifier (validated non-empty Guid).
* **AmendmentTargetId** — Reference to the target contract or clause being amended.
* **AmendmentStatus** — Enum: `Draft`, `Applied`, `Reverted`.

## Events

* **AmendmentCreatedEvent** — Raised when a new amendment is created (status: Draft, includes TargetId).
* **AmendmentAppliedEvent** — Raised when amendment is applied to the target.
* **AmendmentRevertedEvent** — Raised when amendment is reverted.

## Invariants

* AmendmentId must not be null/default.
* AmendmentTargetId must not be null/default.
* AmendmentStatus must be a defined enum value.
* State transitions enforced by specifications.

## Specifications

* **CanApplyAmendmentSpecification** — Only Draft amendments can be applied.
* **CanRevertAmendmentSpecification** — Only Applied amendments can be reverted.

## Errors

* **MissingId** — AmendmentId is required.
* **MissingTargetId** — AmendmentTargetId is required.
* **AlreadyApplied** — Cannot apply an already-applied amendment.
* **AlreadyReverted** — Cannot revert an already-reverted amendment.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.

## Status

**S4 — Invariants + Specifications Complete**
