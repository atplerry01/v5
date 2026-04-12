# Domain: Settlement

## Classification
economic-system

## Context
ledger

## Purpose
Links internal journals to external finalization. Tracks the process of settling an obligation by connecting it to a posted journal and monitoring the external completion status.

## Core Responsibilities
- Initiating settlements that link a journal and obligation
- Tracking settlement lifecycle: Initiated -> Completed or Failed
- Validating journal and obligation references

## Aggregate(s)
- **SettlementAggregate**
  - Event-sourced, sealed. Manages settlement lifecycle from initiation through completion or failure
  - Invariants: Amount must be positive; must reference valid journal and obligation; only Initiated settlements can be completed or failed; cannot complete a failed settlement; cannot fail a completed settlement

## Entities
None

## Value Objects
- **SettlementId** — Typed Guid wrapper for unique settlement identity
- **SettlementStatus** — Enum: Initiated, Completed, Failed

## Domain Events
- **SettlementInitiatedEvent** — Settlement started (links journal + obligation, captures amount, currency)
- **SettlementCompletedEvent** — External finalization confirmed
- **SettlementFailedEvent** — Settlement failed with reason

## Specifications
- **CanCompleteSpecification** — Status == Initiated
- **CanFailSpecification** — Status == Initiated

## Domain Services
- **SettlementReconciliationService** — Validates settlement references are complete (JournalId and ObligationId non-empty)

## Invariants (CRITICAL)
- Amount must be positive
- Must reference a valid journal (JournalId non-empty)
- Must reference a valid obligation (ObligationId non-empty)
- Only Initiated settlements can be completed or failed
- Cannot complete a failed settlement
- Cannot fail a completed settlement

## Policy Dependencies
- Settlement-obligation link enforcement
- Settlement-journal link enforcement

## Integration Points
- **journal** — Settlements reference posted journals via JournalId
- **obligation** — Settlements fulfil obligations via ObligationId
- **treasury** — Treasury provides liquidity for settlements

## Lifecycle
```
Initiate() -> Initiated (links journal + obligation)
  Complete() -> Completed (terminal)
  OR
  Fail() -> Failed (terminal, with reason)
```

## Notes
- Cross-domain references (JournalId, ObligationId) use raw Guid to avoid coupling
- All error methods are strongly typed via static SettlementErrors class
