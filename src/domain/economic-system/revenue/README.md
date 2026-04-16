# Context: Revenue

## Classification
economic-system

## Context
revenue

## Purpose
Defines how economic value is priced, recorded, distributed, and paid out
across the system. Revenue represents SPV income. Transaction executes
payment. Ledger records truth.

## Canonical Model (SPV-based, single-shot)
- **Revenue originates from an SPV** (`SpvId`). Revenue does NOT originate
  from a contract.
- Each lifecycle-relevant aggregate is **single-shot at E1**: the create
  factory emits the single canonical event and sets a single terminal
  status. No post-create lifecycle transitions live inside the aggregate.
- Post-create orchestration (vault mutation, money movement, failure
  handling) is the responsibility of the Phase 2D / runtime layer, not
  this domain.

## Core Responsibilities
- Defining binding revenue agreements with parties and share rules
  (`contract/`)
- Determining value via pricing models (`pricing/`)
- Recording SPV income as a revenue fact (`revenue/`)
- Splitting SPV revenue across participant shares based on ownership
  percentages (`distribution/`)
- Emitting the payout-executed intent (`payout/`)

## Aggregate(s)
- **RevenueContractAggregate** (`contract/`)
  - Draft → Active → Terminated. Lifecycle lives here, not in revenue.
  - Invariants: ≥ 2 parties; share rules total exactly 100%;
    end date > start date.

- **PricingAggregate** (`pricing/`)
  - DefinePrice → Adjusted (repeatable, with reason tracking).
  - Invariants: price > 0 at definition, ≥ 0 as invariant;
    ContractId non-empty.

- **RevenueAggregate** (`revenue/`)
  - `RecordRevenue(revenueId, spvId, amount, currency, sourceRef)` →
    `Recorded`. Single-shot; no further transitions at E1.
  - Invariants: amount > 0 at recognition.

- **DistributionAggregate** (`distribution/`)
  - `CreateDistribution(distributionId, spvId, totalAmount, allocations)` →
    `Created`. Shares computed from `(participantId, ownershipPercentage)`
    pairs supplied by the caller. Single-shot.
  - Invariants: SPV id required; total > 0; ownership percentages sum
    to 100.

- **PayoutAggregate** (`payout/`)
  - `ExecutePayout(payoutId, distributionId, shares)` → `Completed`.
    Single-shot — emits intent only; no vault mutation.
  - Invariants: shares non-empty; total share amount > 0.

## Entities
- **ContractParty** (`contract/entity/`) — party bound to a contract
  with a revenue share percentage.
- **DistributionShare** (`distribution/entity/`) — per-participant share
  entity; see `ParticipantShare` value object for the equivalent
  value-typed record.

## Value Objects
- **RevenueContractId** — typed Guid wrapper
- **ContractStatus** — Draft | Active | Terminated
- **ContractTerm** — StartDate / EndDate
- **RevenueShareRule** — PartyId + SharePercentage (creation input)
- **PricingId** — typed Guid wrapper
- **PricingModel** — Fixed | Tiered | PerUnit | Percentage
- **RevenueId** — typed Guid wrapper
- **RevenueStatus** — post-record status carrier (current model sets
  `Recorded`)
- **DistributionId** — typed Guid wrapper
- **DistributionStatus** — post-create status carrier (current model
  sets `Created`)
- **ParticipantShare** — ParticipantId, Amount, Percentage
- **PayoutId** — typed Guid wrapper
- **PayoutStatus** — post-execute status carrier (current model sets
  `Completed`)

## Domain Events (implemented)
- **RevenueContractCreatedEvent** / **RevenueContractActivatedEvent** /
  **RevenueContractTerminatedEvent** — contract lifecycle
- **PriceDefinedEvent** / **PriceAdjustedEvent** — pricing
- **RevenueRecordedEvent** — SPV income recorded (captures RevenueId,
  SpvId, Amount, Currency, SourceRef)
- **DistributionCreatedEvent** — distribution with SPV-sourced participant
  shares computed
- **PayoutExecutedEvent** — payout intent emitted; execution deferred
  to orchestration

```text
ECONOMIC FLOW (SPV-based single-shot)

1. RevenueAggregate.RecordRevenue(spvId, amount, currency, sourceRef)
     → RevenueRecordedEvent           → Status=Recorded
2. DistributionAggregate.CreateDistribution(spvId, totalAmount, allocations)
     → DistributionCreatedEvent       → Status=Created
3. PayoutAggregate.ExecutePayout(payoutId, distributionId, shares)
     → PayoutExecutedEvent            → Status=Completed

Vault mutation and cross-aggregate conservation are handled by the
Phase 2D orchestration layer outside the domain.
```

## Specifications (implemented)
- **IsActiveSpecification** (contract) — Status == Active
- **CanAdjustPriceSpecification** (pricing) — ContractId non-empty AND
  Price > 0
- **IsFullyAllocatedSpecification** (distribution) — shares sum exactly
  to TotalAmount

## Domain Services
- **ContractValidationService** (contract) — validates term and share
  rules (total 100%)
- **PricingCalculationService** (pricing) — per-unit / percentage
  pricing with banker's rounding
- **RevenueTraceService** (revenue) — revenue trace helper
- **DistributionSplitService** (distribution) — validates participant
  allocations
- **PayoutMatchingService** (payout) — validates payout references
  distribution

## Invariants (CRITICAL)
- R1 (SPV): Revenue originates from an SPV; SpvId must be non-empty.
- R2: Distribution ownership percentages sum to exactly 100.
- R3: Payout references a distribution and carries non-empty shares.
- R4: Revenue context never writes to ledger directly.
- R5: Money movement flows through the transaction context, not here.
- Contract must have ≥ 2 parties with shares totaling 100%.

## Integration Points
- **pricing → contract** — pricing references a contract for value rules
- **distribution → vault** — participant shares are applied to SPV vault
  by the orchestration layer (not by this domain)
- **payout → transaction** — actual money movement occurs via the
  transaction context (R5)

## Notes
- Pure domain: zero runtime, infrastructure, or engine dependencies.
- All aggregates are event-sourced and sealed.
- All cross-domain references use raw `Guid` / typed id records to avoid
  coupling.
- Pre-SPV revenue-origin artifacts (ContractId as revenue origin,
  Pending / Failed / Distributed lifecycle states, `MarkDistributed` /
  `AssignAllocation` transitions, `PayoutFailedEvent`) have been removed
  from this documentation to match the implemented model.
