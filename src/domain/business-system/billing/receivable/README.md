# Domain: Receivable

## Classification

business-system

## Context

billing

## Domain Responsibility

Models the outstanding amounts owed by parties for billed goods or services. Tracks receivable lifecycle from outstanding through settled or written-off terminal states. Write-off is irreversible.

## Aggregate

* **ReceivableAggregate** — Root aggregate representing a receivable instance.
  * Private constructor; created via `Create(ReceivableId)` factory method.
  * State transitions via `Settle()` and `WriteOff()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Outstanding ──Settle()──> Settled (terminal)
Outstanding ──WriteOff()──> WrittenOff (terminal)
```

## Value Objects

* **ReceivableId** — Deterministic identifier (validated non-empty Guid).
* **ReceivableStatus** — Enum: `Outstanding`, `Settled`, `WrittenOff`.

## Events

* **ReceivableCreatedEvent** — Raised when a new receivable is created (status: Outstanding).
* **ReceivableSettledEvent** — Raised when receivable is settled.
* **ReceivableWrittenOffEvent** — Raised when receivable is written off.

## Invariants

* ReceivableId must not be null/default.
* ReceivableStatus must be a defined enum value.
* Cannot settle an already-settled receivable.
* Write-off is irreversible (enforced by CanWriteOffSpecification: only Outstanding allows write-off).

## Specifications

* **CanSettleSpecification** — Validates that status is Outstanding before settling.
* **CanWriteOffSpecification** — Validates that status is Outstanding before writing off.

## Errors

* **MissingId** — ReceivableId is required.
* **AlreadySettled** — Cannot settle an already-settled receivable.
* **AlreadyWrittenOff** — Cannot write off an already written-off receivable.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **ReceivableService** — Reserved for cross-aggregate coordination within receivable context.

## Status

**S4 — Invariants + Specifications Complete**
