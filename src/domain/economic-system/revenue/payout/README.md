# Domain: Payout

## Classification
economic-system

## Context
revenue

## Purpose
Executes payment of distributed revenue. A payout triggers the actual transaction that moves money from a distribution to a recipient.

## Core Responsibilities
- Initiating payouts from distribution references
- Finalizing payments (Pending -> Completed or Failed)
- Ensuring payout matches distribution (R3)

## Aggregate(s)
- **PayoutAggregate**
  - Event-sourced, sealed. Manages payout lifecycle from initiation through completion or failure
  - Invariants: Must reference distribution (DistributionId non-empty — R3); only Pending can transition; cannot complete a failed payout; cannot fail a completed payout

## Entities
None

## Value Objects
- **PayoutId** — Typed Guid wrapper with From() factory for unique payout identity
- **PayoutStatus** — Enum: Pending, Completed, Failed

## Domain Events
- **PayoutExecutedEvent** — Payout intent emitted; execution deferred to orchestration (no direct vault mutation)
- **PayoutFailedEvent** — Payout failed with reason

```text
ECONOMIC FLOW

1. RevenueAggregate → RevenueRecordedEvent
2. DistributionAggregate → DistributionCreatedEvent
3. PayoutAggregate → PayoutExecutedEvent

Execution of vault mutation is handled outside the domain.
```

## Specifications
- **CanCompleteSpecification** — Status == Pending

## Domain Services
- **PayoutMatchingService** — Validates payout references distribution (DistributionId non-empty); enforces R3

## Invariants (CRITICAL)
- Must reference a distribution (DistributionId non-empty — R3)
- Only Pending payouts can be completed or failed
- Cannot complete a failed payout
- Cannot fail a completed payout

## Policy Dependencies
- R3: Payout must match distribution exactly
- R5: Payout must go through transaction (architectural constraint)

## Integration Points
- **distribution** — Payout references distribution via DistributionId
- **transaction** (transaction context) — Payout triggers actual money movement (R5)

## Lifecycle
```
Initiate() -> Pending (requires DistributionId)
  Complete() -> Completed (terminal)
  OR
  Fail() -> Failed (terminal, with reason)
```

## Notes
- Payout never writes to ledger directly (R4/R5) — it flows through transaction context
- Cross-domain references (DistributionId) use raw Guid to avoid coupling
- All error methods are strongly typed via static PayoutErrors class
