# Domain: Distribution

## Classification
economic-system

## Context
revenue

## Purpose
Splits SPV revenue across participant shares. A distribution takes a
total amount and an ownership table `(participantId, ownershipPct)` and
computes per-participant share amounts. This domain does not mutate the
vault — participant-side vault movement is the orchestration layer's
responsibility.

## Canonical Model (SPV-based, single-shot)
- Distribution is keyed by `SpvId`, not a revenue record id.
- Allocations are supplied as `(participantId, ownershipPercentage)`
  pairs derived from `CapitalAllocationAggregate` ownership — the
  aggregate does not look up participants itself.
- `CreateDistribution(...)` is single-shot: it emits
  `DistributionCreatedEvent` and sets `Status = Created`.
  **No `AssignAllocation` / `MarkDistributed` transitions exist.**

## Core Responsibilities
- Computing `ParticipantShare` amounts from SPV total and ownership
  percentages.
- Enforcing the invariant that ownership percentages sum to exactly 100.

## Aggregate(s)
- **DistributionAggregate** — event-sourced, sealed.
  - Factory: `CreateDistribution(distributionId, spvId, totalAmount, allocations)`
  - Allocations: `IReadOnlyList<(string ParticipantId, decimal OwnershipPercentage)>`
  - Each share amount is `totalAmount × (ownershipPct / 100)`.
  - Invariants: SpvId non-empty; totalAmount > 0; allocations non-empty;
    ownership percentages sum to exactly 100.

## Entities
- **DistributionShare** — per-participant share entity.

## Value Objects
- **DistributionId** — typed Guid wrapper with `From()` factory.
- **DistributionStatus** — post-create status carrier (current model
  sets `Created`).
- **ParticipantShare** — ParticipantId, Amount, Percentage.

## Domain Events
- **DistributionCreatedEvent** — captures `DistributionId`, `SpvId`,
  `TotalAmount`.

```text
ECONOMIC FLOW (SPV-based single-shot)

1. RevenueAggregate → RevenueRecordedEvent (SpvId, Amount)
2. DistributionAggregate.CreateDistribution(spvId, totalAmount, allocations)
     → DistributionCreatedEvent → Status = Created
3. PayoutAggregate → PayoutExecutedEvent

Vault mutation for each participant is orchestrated outside the domain.
```

## Specifications
- **IsFullyAllocatedSpecification** — shares sum exactly to TotalAmount.

## Domain Services
- **DistributionSplitService** — validates allocations sum equals total
  revenue (enforces R2).

## Invariants (CRITICAL)
- SpvId must be non-empty.
- TotalAmount must be > 0.
- At least one allocation.
- Ownership percentages across allocations must sum to exactly 100.

## Integration Points
- **revenue** — distribution is produced in response to a
  `RevenueRecordedEvent` for the same SPV.
- **capital / allocation** — ownership percentages are sourced from
  `CapitalAllocationAggregate` by the orchestration layer.
- **payout** — produces the shares consumed by
  `PayoutAggregate.ExecutePayout`.

## Notes
- Pure domain — no runtime, infrastructure, or engine dependencies.
- Errors are raised via shared-kernel `ArgumentException` /
  `DomainInvariantViolationException` directly; the pre-SPV
  `DistributionErrors` class has been removed as all its methods were
  unreachable.
