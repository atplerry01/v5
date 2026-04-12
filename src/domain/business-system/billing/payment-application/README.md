# Domain: PaymentApplication

## Classification

business-system

## Context

billing

## Domain Responsibility

Models the settlement of payments against invoices. Tracks payment application lifecycle from pending through applied or reversed states. Enforces allocation integrity ensuring applied amounts do not exceed outstanding balances.

## Aggregate

* **PaymentApplicationAggregate** — Root aggregate representing a payment application instance.
  * Private constructor; created via `Create(PaymentApplicationId, invoiceReference, paymentSourceReference, outstandingAmount)` factory method.
  * State transitions via `ApplyPayment()` and `ReverseApplication()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Pending ──ApplyPayment()──> Applied ──ReverseApplication()──> Reversed (terminal)
```

## Entities

* **PaymentAllocation** — Maps payment to invoice lines (AllocationId, InvoiceLineId, Amount). Must have at least one before applying.

## Value Objects

* **PaymentApplicationId** — Deterministic identifier (validated non-empty Guid).
* **PaymentApplicationStatus** — Enum: `Pending`, `Applied`, `Reversed`.

## Events

* **PaymentApplicationCreatedEvent** — Raised when a new payment application is created (status: Pending).
* **PaymentApplicationAppliedEvent** — Raised when payment is applied to invoice.
* **PaymentApplicationReversedEvent** — Raised when a previously applied payment is reversed.

## Invariants

* PaymentApplicationId must not be null/default.
* PaymentApplicationStatus must be a defined enum value.
* Must reference an invoice and payment source.
* Cannot apply more than outstanding amount (enforced at allocation level).
* Reversal must only apply to Applied state (enforced by CanReverseApplicationSpecification).
* Must have at least one allocation before applying.

## Specifications

* **CanApplyPaymentSpecification** — Validates that status is Pending before applying.
* **CanReverseApplicationSpecification** — Validates that status is Applied before reversing.
* **IsFullyAllocatedSpecification** — Validates that total allocated amount meets outstanding amount.

## Errors

* **MissingId** — PaymentApplicationId is required.
* **MissingInvoiceReference** — Must reference an invoice.
* **MissingPaymentSource** — Must reference a payment source.
* **ExceedsOutstandingAmount** — Applied amount exceeds outstanding balance.
* **AllocationRequired** — Must contain at least one allocation.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **PaymentApplicationService** — Reserved for cross-aggregate coordination within payment application context.

## Status

**S4 — Invariants + Specifications Complete**
