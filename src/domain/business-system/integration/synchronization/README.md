# Domain: Synchronization

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines coordination contracts only and contains no execution logic.

## Lifecycle Pattern

**SEQUENTIAL** — Defined → Pending → Confirmed. No state reversal. Confirmed is terminal.

## Domain Responsibility

Models consistency expectation contracts for integration. A synchronization defines the expected consistency state between systems, progressing through definition, pending, and confirmation without executing any sync jobs, polling, or data propagation.

## Aggregate

* **SynchronizationAggregate** — Root aggregate managing sync contract lifecycle.
  * Private constructor; created via `Create(SynchronizationId, SyncPolicyId)` factory method.
  * State transitions via `MarkPending()` and `Confirm()` methods.
  * Event-sourced with optimistic concurrency via `Version`.

## State Model

```
Defined ──MarkPending()──> Pending ──Confirm()──> Confirmed (terminal)
```

## Value Objects

* **SynchronizationId** — Deterministic identifier (validated non-empty Guid).
* **SynchronizationStatus** — Enum: `Defined`, `Pending`, `Confirmed`.
* **SyncPolicyId** — Reference to the consistency policy (validated non-empty Guid).

## Specifications

* **CanMarkPendingSpecification** — Only Defined syncs can be marked pending.
* **CanConfirmSpecification** — Only Pending syncs can be confirmed.
* **IsConfirmedSpecification** — Checks if sync is confirmed.

## Errors

* **MissingId** / **MissingPolicyId** / **AlreadyPending** / **AlreadyConfirmed** / **InvalidStateTransition**

## Status

**S4 — Invariants + Specifications Complete**
