# Domain: Payout

## Classification
economic-system

## Context
revenue

## Purpose
Emits the intent that a payout has been executed. Pure domain state;
one canonical event per aggregate. No vault references, no
cross-aggregate calls — full SPV-debit / participant-credit conservation
is the responsibility of the Phase 2D orchestration layer.

## Canonical Model (SPV-based, single-shot)
- `ExecutePayout(payoutId, distributionId, shares)` is single-shot:
  it emits `PayoutExecutedEvent` and sets `Status = Completed`.
- There is **no `Pending` state**, **no `Failed` state**, and no
  post-execute transitions (no `Complete()`, no `Fail()`) inside the
  aggregate. Failure handling is an orchestration concern.

## Core Responsibilities
- Recording the fact that a distribution's shares have been executed
  as a payout.
- Enforcing the invariant that a payout references a distribution and
  carries non-empty shares with a positive total.

## Aggregate(s)
- **PayoutAggregate** — event-sourced, sealed.
  - Factory: `ExecutePayout(payoutId, distributionId, shares)`
  - `shares` is `IReadOnlyList<ParticipantShare>` (the distribution
    output).
  - Invariants: shares non-empty; sum of share amounts > 0.

## Entities
None.

## Value Objects
- **PayoutId** — typed Guid wrapper with `From()` factory.
- **PayoutStatus** — post-execute status carrier (current model sets
  `Completed`).

## Domain Events
- **PayoutExecutedEvent** — captures `PayoutId` and `DistributionId`.
  Intent only — execution is deferred to orchestration.

```text
ECONOMIC FLOW (SPV-based single-shot)

1. RevenueAggregate → RevenueRecordedEvent
2. DistributionAggregate → DistributionCreatedEvent (produces shares)
3. PayoutAggregate.ExecutePayout(payoutId, distributionId, shares)
     → PayoutExecutedEvent → Status = Completed

Actual money movement runs through the transaction context (R5) and
the vault, orchestrated outside the domain.
```

## Specifications
None at E1. (Pre-SPV `CanCompleteSpecification` has been removed —
no `Pending` state exists in the current model, so the predicate was
unreachable.)

## Domain Services
- **PayoutMatchingService** — validates payout references distribution
  (enforces R3).

## Invariants (CRITICAL)
- Shares must be non-empty.
- Sum of share amounts must be > 0.
- DistributionId must be present (encoded in the factory signature —
  required parameter).

## Integration Points
- **distribution** — payout consumes the `shares` produced by
  `DistributionAggregate.CreateDistribution`.
- **transaction** — actual money movement flows through the transaction
  context (R5), orchestrated outside this domain.

## Notes
- Pure domain — no runtime, infrastructure, or engine dependencies.
- Cross-domain references (DistributionId, PayoutId) use typed id
  value records.
- Errors are raised via shared-kernel `ArgumentException` /
  `InvalidOperationException` directly; the pre-SPV `PayoutErrors`
  class has been removed as all its methods were unreachable.
- `PayoutFailedEvent` was previously documented but was never
  implemented; per Option A (SPV single-shot), failure is an
  orchestration concern and no such event is produced by this
  aggregate.
