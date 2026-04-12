# Domain: Pool

## Classification
economic-system

## Context
capital

## Purpose
System-wide capital aggregation. Tracks the total capital across all accounts and enforces the critical invariant that no artificial capital can exist — the pool total must equal the sum of all account balances.

## Core Responsibilities
- Creating capital pools per currency
- Aggregating capital from account funding events
- Reducing capital from account allocation events
- Validating pool-to-account balance reconciliation

## Aggregate(s)
- **CapitalPoolAggregate**
  - Event-sourced, sealed. Tracks system-wide capital total through aggregation and reduction
  - Invariants: TotalCapital must equal sum of all account totals (no artificial capital); TotalCapital >= 0; cannot reduce below zero; amounts must be positive

## Entities
None

## Value Objects
- **PoolId** — Typed Guid wrapper for unique pool identity

## Domain Events
- **PoolCreatedEvent** — Capital pool initialized with currency
- **CapitalAggregatedEvent** — Capital added to pool from source account
- **CapitalReducedEvent** — Capital removed from pool to source account

## Specifications
- **CanReduceSpecification** — amount > 0 AND amount <= TotalCapital
- **PoolBalanceSpecification** — TotalCapital <= expected account sum

## Domain Services
- **PoolAggregationService** — Reconciles pool total with sum of account balances; validates pool total == sum of all account TotalBalance values

## Invariants (CRITICAL)
- TotalCapital must equal sum of all account totals
- TotalCapital must be non-negative
- Cannot reduce below zero
- No artificial capital: pool cannot contain more capital than exists across all accounts

## Policy Dependencies
- Pool-account alignment enforcement

## Integration Points
- **account** — Pool aggregates capital from all accounts; pool total must equal sum of account totals

## Lifecycle
```
Create() -> Initialized (zero balance)
  AggregateCapital() / ReduceCapital() (repeatable)
```

## Notes
- Pool is a global consistency aggregate, not per-account
- ArtificialCapitalDetected error enforces the no-creation-from-nothing rule
- All error methods are strongly typed via static PoolErrors class
