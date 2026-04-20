# Domain: Approval

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the formal decision process for agreement activation. Tracks approval lifecycle from pending through approved or rejected terminal states. Acts as an authorization gate that must be satisfied before an agreement can proceed.

## Aggregate

* **ApprovalAggregate** — Root aggregate representing a formal approval decision on agreement terms.
  * Private constructor; created via `Create(ApprovalId)` factory method.
  * State transitions via `Approve()` and `Reject()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Pending ──Approve()──> Approved (terminal)
Pending ──Reject()──> Rejected (terminal)
```

## Value Objects

* **ApprovalId** — Deterministic identifier (validated non-empty Guid).
* **ApprovalStatus** — Enum: `Pending`, `Approved`, `Rejected`.

## Events

* **ApprovalCreatedEvent** — Raised when a new approval is created (status: Pending).
* **ApprovalApprovedEvent** — Raised when approval is granted.
* **ApprovalRejectedEvent** — Raised when approval is denied.

## Invariants

* ApprovalId must not be null/default.
* ApprovalStatus must be a defined enum value.
* State transitions enforced by specifications (only Pending allows transitions).

## Specifications

* **CanApproveSpecification** — Validates that status is Pending before approving.
* **CanRejectSpecification** — Validates that status is Pending before rejecting.

## Errors

* **MissingId** — ApprovalId is required.
* **AlreadyApproved** — Cannot transition an already-approved approval.
* **AlreadyRejected** — Cannot transition an already-rejected approval.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **ApprovalService** — Reserved for cross-aggregate coordination within approval context.

## Status

**S4 — Invariants + Specifications Complete**
