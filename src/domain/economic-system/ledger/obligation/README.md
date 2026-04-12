# Domain: Obligation

## Classification
economic-system

## Context
ledger

## Purpose
Tracks financial commitments — amounts that are payable or receivable. Obligations represent promises that must be fulfilled through settlement or explicitly cancelled.

## Core Responsibilities
- Creating obligations with counterparty and type (payable/receivable)
- Tracking obligation lifecycle: Pending -> Fulfilled or Cancelled
- Linking fulfilment to settlements

## Aggregate(s)
- **ObligationAggregate**
  - Event-sourced, sealed. Manages financial commitments through creation, fulfilment, and cancellation
  - Invariants: Amount must be positive; only Pending obligations can be fulfilled or cancelled; cannot fulfil a cancelled obligation; cannot cancel a fulfilled obligation

## Entities
None

## Value Objects
- **ObligationId** — Typed Guid wrapper for unique obligation identity
- **ObligationType** — Enum: Payable, Receivable
- **ObligationStatus** — Enum: Pending, Fulfilled, Cancelled

## Domain Events
- **ObligationCreatedEvent** — Commitment recorded with counterparty, type, amount, currency
- **ObligationFulfilledEvent** — Obligation satisfied via settlement (captures SettlementId)
- **ObligationCancelledEvent** — Obligation cancelled with reason

## Specifications
- **CanFulfilSpecification** — Status == Pending
- **CanCancelSpecification** — Status == Pending

## Domain Services
- **ObligationMatchingService** — Validates settlement amounts match obligations; checks if settlement amount >= obligation amount

## Invariants (CRITICAL)
- Amount must be positive
- CounterpartyId must be non-empty
- Only Pending obligations can be fulfilled or cancelled
- Cannot fulfil a cancelled obligation
- Cannot cancel a fulfilled obligation

## Policy Dependencies
- Settlement-obligation matching enforcement

## Integration Points
- **settlement** — Settlements fulfil obligations; ObligationFulfilledEvent references SettlementId

## Lifecycle
```
Create() -> Pending
  Fulfil() -> Fulfilled (terminal, references settlement)
  OR
  Cancel() -> Cancelled (terminal, with reason)
```

## Notes
- Cross-domain references (CounterpartyId) use raw Guid to avoid coupling
- All error methods are strongly typed via static ObligationErrors class
