# Domain: Revenue

## Classification
economic-system

## Context
revenue

## Purpose
Records the fact that an SPV has earned income. A revenue record is the
system acknowledgement of an SPV-sourced cash event; normalization into
the SPV's vault slice is handled by the orchestration layer, not here.

## Canonical Model (SPV-based, single-shot)
- Revenue originates from an SPV (`SpvId`), never from a contract.
- `RecordRevenue(...)` is single-shot: it emits `RevenueRecordedEvent`
  and sets `Status = Recorded`. The aggregate has **no post-record
  lifecycle transitions** (no `MarkDistributed`, no `Recognize →
  Distributed`).

## Core Responsibilities
- Recording SPV revenue facts immutably.
- Carrying the canonical `SpvId`, amount, currency, and external
  source reference (`SourceRef`) for downstream orchestration.

## Aggregate(s)
- **RevenueAggregate** — event-sourced, sealed.
  - Factory: `RecordRevenue(revenueId, spvId, amount, currency, sourceRef)`
  - Invariants: amount > 0 at recognition; invariant holds that a
    `Recorded` aggregate cannot carry amount ≤ 0.

## Entities
None.

## Value Objects
- **RevenueId** — typed Guid wrapper with `From()` factory.
- **RevenueStatus** — post-record status carrier (current model sets
  `Recorded`).

## Domain Events
- **RevenueRecordedEvent** — captures `RevenueId`, `SpvId`, `Amount`,
  `Currency`, `SourceRef`.

```text
ECONOMIC FLOW (SPV-based single-shot)

1. RevenueAggregate.RecordRevenue(spvId, amount, currency, sourceRef)
     → RevenueRecordedEvent → Status = Recorded
2. DistributionAggregate → DistributionCreatedEvent
3. PayoutAggregate → PayoutExecutedEvent

Vault mutation and cross-aggregate conservation are handled by the
Phase 2D orchestration layer outside the domain.
```

## Specifications
None at E1. (Pre-SPV `CanDistributeSpecification` has been removed —
no `Recognized` state exists in the current model, so the predicate
was unreachable.)

## Domain Services
- **RevenueTraceService** — revenue trace helper (no cross-aggregate
  calls).

## Invariants (CRITICAL)
- SpvId must be non-empty (R1 — SPV origin).
- Amount must be > 0 at recognition.
- Amount must be > 0 as an invariant for `Recorded` aggregates.

## Integration Points
- **SPV / vault** (orchestration layer) — the Phase 2D orchestrator
  reads `RevenueRecordedEvent` and funds the SPV's Slice1 via
  `VaultAccountAggregate.ApplyRevenue`. No direct call from this
  aggregate.
- **distribution** — the orchestration layer supplies the participant
  allocations to `DistributionAggregate.CreateDistribution` using
  ownership percentages from `CapitalAllocationAggregate`.

## Notes
- Pure domain — no runtime, infrastructure, or engine dependencies.
- Errors are raised via shared-kernel `DomainException` /
  `DomainInvariantViolationException` directly; the pre-SPV
  `RevenueErrors` class has been removed as all its methods were
  unreachable.
