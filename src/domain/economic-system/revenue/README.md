# Domain: Revenue

## Classification
economic-system

## Context
revenue

## Purpose
Defines how economic value is priced, recognized, distributed, and paid out across the system. Revenue represents entitlement. Transaction executes payment. Ledger records truth.

## Core Responsibilities
- Defining binding revenue agreements with parties and share rules
- Determining value via pricing models (Fixed, Tiered, PerUnit, Percentage)
- Recognizing earned value from contracts
- Splitting recognized revenue among recipients
- Executing payments via transaction context

## Aggregate(s)
- **RevenueContractAggregate** (`contract/`)
  - Defines binding revenue agreements with parties and share rules
  - Invariants: Must have >= 2 parties; revenue shares must total exactly 100%; end date must be after start date

- **PricingAggregate** (`pricing/`)
  - Determines value via pricing models
  - Invariants: Price must be >= 0; must reference a contract (ContractId non-empty)

- **RevenueAggregate** (`revenue/`)
  - Recognizes earned value from contracts
  - Invariants: Amount must be >= 0; must reference a contract (R1); only Recognized can be distributed

- **DistributionAggregate** (`distribution/`)
  - Splits recognized revenue among recipients
  - Invariants: Sum of allocations <= TotalAmount; TotalAmount >= 0; must reference revenue record

- **PayoutAggregate** (`payout/`)
  - Executes payments via transaction context
  - Invariants: Must reference distribution (R3); only Pending can transition; mutually exclusive terminal states

## Entities
- **ContractParty** (`contract/entity/`) — Party bound to contract with revenue share percentage
- **Allocation** (`distribution/entity/`) — Per-recipient allocation within a distribution (RecipientId, Amount, SharePercentage)

## Value Objects
- **RevenueContractId** — Unique contract identifier
- **ContractStatus** — Enum: Draft, Active, Terminated
- **ContractTerm** — Contract duration (StartDate, EndDate)
- **RevenueShareRule** — Input for contract creation (PartyId, SharePercentage)
- **PricingId** — Unique pricing identifier
- **PricingModel** — Enum: Fixed, Tiered, PerUnit, Percentage
- **RevenueId** — Unique revenue identifier
- **RevenueStatus** — Enum: Recognized, Distributed
- **DistributionId** — Unique distribution identifier
- **PayoutId** — Unique payout identifier
- **PayoutStatus** — Enum: Pending, Completed, Failed

## Domain Events
- **RevenueContractCreatedEvent** — Agreement drafted with parties and share rules
- **RevenueContractActivatedEvent** — Contract activated for revenue generation
- **RevenueContractTerminatedEvent** — Contract terminated with reason
- **PriceDefinedEvent** — Price determined from contract and model
- **PriceUpdatedEvent** — Price updated with reason tracking
- **RevenueRecordedEvent** — SPV revenue recorded from external income (R1)
- **DistributionCreatedEvent** — Ownership-based participant shares computed (R2)
- **PayoutExecutedEvent** — Payout intent emitted; execution deferred to orchestration (R3)
- **PayoutFailedEvent** — Payout failed with reason

```text
ECONOMIC FLOW

1. RevenueAggregate → RevenueRecordedEvent
2. DistributionAggregate → DistributionCreatedEvent
3. PayoutAggregate → PayoutExecutedEvent

Execution of vault mutation is handled outside the domain.
```

## Specifications
- **IsActiveSpecification** (contract) — Status == Active
- **HasContractSpecification** (pricing) — ContractId non-empty
- **CanDistributeSpecification** (revenue) — Status == Recognized
- **IsFullyAllocatedSpecification** (distribution) — Sum of allocations == TotalAmount
- **CanCompleteSpecification** (payout) — Status == Pending

## Domain Services
- **ContractValidationService** (contract) — Validates contract is within term and share rules total 100%
- **PricingCalculationService** (pricing) — Calculates per-unit and percentage pricing with banker's rounding
- **RevenueTraceService** (revenue) — Validates contract reference is non-empty (R1 enforcement)
- **DistributionSplitService** (distribution) — Validates allocations sum equals total revenue (R2 enforcement)
- **PayoutMatchingService** (payout) — Validates payout references distribution (R3 enforcement)

## Invariants (CRITICAL)
- R1: Revenue cannot exist without a contract
- R2: Distribution must equal total revenue (allocations sum = total)
- R3: Payout must match distribution exactly
- R4: Revenue cannot directly trigger ledger entries
- R5: Payout must go through transaction
- Contract must have >= 2 parties with shares totaling 100%

## Policy Dependencies
- Revenue Rules (R1-R5) enforcement
- Contract share rules validation (must total 100%)
- Canonical execution order enforcement

## Integration Points
- **contract -> pricing** — Pricing references contract for value rules
- **revenue -> contract** — Revenue recognition requires contract (R1)
- **distribution -> revenue** — Distribution allocations must equal total revenue (R2)
- **payout -> distribution** — Payout must match distribution (R3)
- **payout -> transaction** (transaction context) — Actual money movement (R5)

## Lifecycle

### Contract
```
CreateContract() -> Draft
  Activate() -> Active (can generate revenue)
  Terminate() -> Terminated (terminal)
```

### Pricing
```
DefinePrice() -> Defined
  UpdatePrice() -> Updated (repeatable, with reason tracking)
```

### Revenue
```
Recognize() -> Recognized (requires ContractId)
  MarkDistributed() -> Distributed (terminal)
```

### Distribution
```
Distribute() -> Created
  AssignAllocation() (repeatable, until fully allocated)
```

### Payout
```
Initiate() -> Pending (requires DistributionId)
  Complete() -> Completed (terminal)
  OR
  Fail() -> Failed (terminal, with reason)
```

## Notes
- Pure domain — zero runtime, infrastructure, or engine dependencies
- Revenue context never writes to ledger directly (R4)
- All cross-domain references use raw Guid to avoid coupling
- All aggregates are event-sourced and sealed
