# Domain: Distribution

## Classification
economic-system

## Context
revenue

## Purpose
Splits revenue among parties based on contract rules. A distribution takes total revenue and assigns allocations to individual recipients based on share percentages.

## Core Responsibilities
- Initiating revenue splits from recognized revenue
- Assigning per-recipient allocations with share percentages
- Enforcing allocation sum equals total revenue (R2)

## Aggregate(s)
- **DistributionAggregate**
  - Event-sourced, sealed. Manages Allocation entities
  - Invariants: Sum of allocations <= TotalAmount (enforced in EnsureInvariants); TotalAmount >= 0; must reference revenue record (RevenueId non-empty)

## Entities
- **Allocation** — Per-recipient allocation within a distribution. Properties: RecipientId (Guid), Amount, SharePercentage (decimal 0-100).

## Value Objects
- **DistributionId** — Typed Guid wrapper with From() factory for unique distribution identity

## Domain Events
- **DistributionCreatedEvent** — Revenue split initiated (captures RevenueId, TotalAmount, Currency)
- **AllocationAssignedEvent** — Per-recipient allocation assigned (captures RecipientId, AllocationAmount, SharePercentage)

## Specifications
- **IsFullyAllocatedSpecification** — All allocations sum exactly to TotalAmount (returns false if no allocations)

## Domain Services
- **DistributionSplitService** — Validates allocations sum equals total revenue; enforces R2

## Invariants (CRITICAL)
- Sum of allocations must equal total revenue (R2)
- TotalAmount must be non-negative
- Must reference a revenue record (RevenueId non-empty)
- Share percentages must be between 0 and 100
- Recipients must be valid (non-empty)

## Policy Dependencies
- R2: Distribution must equal total revenue

## Integration Points
- **revenue** — Distribution references revenue record via RevenueId
- **payout** — Distribution triggers payout per allocation

## Lifecycle
```
Distribute() -> Created (total amount set from revenue)
  AssignAllocation() (repeatable, until IsFullyAllocated satisfied)
```

## Notes
- All error methods are strongly typed via static DistributionErrors class
- Allocation entity uses internal factory Create() method
