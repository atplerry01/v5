# Domain: Invoice

## Classification

business-system

## Context

billing

## Domain Responsibility

Models the formal billing documents issued to parties for goods or services. Tracks invoice lifecycle from draft through issued, paid, or cancelled states. Maintains line item integrity for each invoice.

## Aggregate

* **InvoiceAggregate** — Root aggregate representing an invoice instance.
  * Private constructor; created via `Create(InvoiceId)` factory method.
  * State transitions via `Issue()`, `MarkAsPaid()`, and `Cancel()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Draft ──Issue()──> Issued ──MarkAsPaid()──> Paid (terminal)
Draft ──Cancel()──> Cancelled (terminal)
Issued ──Cancel()──> Cancelled (terminal)
```

## Entities

* **InvoiceLineItem** — Individual line item on the invoice (LineItemId, Description). Must have at least one before issuing.

## Value Objects

* **InvoiceId** — Deterministic identifier (validated non-empty Guid).
* **InvoiceStatus** — Enum: `Draft`, `Issued`, `Paid`, `Cancelled`.

## Events

* **InvoiceCreatedEvent** — Raised when a new invoice is created (status: Draft).
* **InvoiceIssuedEvent** — Raised when invoice is issued to a party.
* **InvoicePaidEvent** — Raised when invoice payment is recorded.
* **InvoiceCancelledEvent** — Raised when invoice is cancelled.

## Invariants

* InvoiceId must not be null/default.
* InvoiceStatus must be a defined enum value.
* Cannot pay an unissued invoice (enforced by CanMarkAsPaidSpecification: only Issued allows payment).
* Cannot cancel a paid invoice (enforced by CanCancelSpecification: only Draft or Issued allows cancellation).
* Must have at least one line item before issuing.

## Specifications

* **CanIssueSpecification** — Validates that status is Draft before issuing.
* **CanMarkAsPaidSpecification** — Validates that status is Issued before marking as paid.
* **CanCancelSpecification** — Validates that status is Draft or Issued before cancelling.

## Errors

* **MissingId** — InvoiceId is required.
* **LineItemRequired** — Invoice must contain at least one line item.
* **AlreadyIssued** — Cannot issue an already-issued invoice.
* **AlreadyPaid** — Cannot pay an already-paid invoice.
* **AlreadyCancelled** — Cannot cancel an already-cancelled invoice.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **InvoiceService** — Reserved for cross-aggregate coordination within invoice context.

## Status

**S4 — Invariants + Specifications Complete**
