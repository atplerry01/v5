# Domain: Treasury

## Classification
economic-system

## Context
ledger

## Purpose
Manages liquidity pools and fund movement readiness. Tracks available funds that can be allocated for settlements and ensures sufficient liquidity exists before financial operations proceed.

## Core Responsibilities
- Creating treasury pools per currency
- Allocating funds from the pool (reducing available liquidity)
- Releasing funds back to the pool (restoring liquidity)
- Enforcing non-negative balance

## Aggregate(s)
- **TreasuryAggregate**
  - Event-sourced, sealed. Manages available liquidity through allocation and release
  - Invariants: Allocation amount must be positive and not exceed balance; release amount must be positive; balance must never be negative

## Entities
None

## Value Objects
- **TreasuryId** — Typed Guid wrapper for unique treasury identity

## Domain Events
- **TreasuryCreatedEvent** — Treasury pool initialized with currency
- **TreasuryFundAllocatedEvent** — Funds allocated (balance decreased)
- **TreasuryFundReleasedEvent** — Funds released (balance increased)

## Specifications
- **CanAllocateSpecification** — Amount > 0 AND Amount <= Balance

## Domain Services
- **TreasuryLiquidityService** — Checks if treasury has sufficient liquidity; returns bool for balance >= required amount

## Invariants (CRITICAL)
- Allocation amount must be positive and not exceed balance
- Release amount must be positive
- Balance must never be negative

## Policy Dependencies
- Liquidity sufficiency enforcement before allocation

## Integration Points
- **settlement** — Treasury provides liquidity for settlements

## Lifecycle
```
Create() -> Initialized (zero balance)
  AllocateFunds() / ReleaseFunds() (repeatable)
```

## Notes
- All error methods are strongly typed via static TreasuryErrors class
