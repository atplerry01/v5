# Domain: Account

## Classification
economic-system

## Context
capital

## Purpose
Source of truth for all capital in the economic system. Every unit of capital enters and exits the system through a CapitalAccountAggregate. Tracks total, available, and reserved balances with strict invariants.

## Core Responsibilities
- Opening capital accounts with owner and currency binding
- Accepting external funding into the system
- Allocating capital (reducing available and total balance)
- Reserving capital (moving from available to reserved)
- Releasing reservations (moving from reserved back to available)
- Freezing accounts (blocking all operations)
- Closing accounts (only when fully settled)

## Aggregate(s)
- **CapitalAccountAggregate**
  - Event-sourced, sealed. Manages account balances through a strict state machine
  - Invariants: AvailableBalance + ReservedBalance = TotalBalance (when Active); all balances >= 0; currency consistency on all operations; cannot operate when Frozen or Closed; cannot close with outstanding balance or reserved funds

## Entities
None

## Value Objects
- **AccountId** — Typed Guid wrapper for unique account identity
- **OwnerId** — Typed Guid wrapper for account owner reference
- **CapitalAccountStatus** — Enum: Active, Frozen, Closed

## Domain Events
- **CapitalAccountOpenedEvent** — Account created with zero balance
- **CapitalFundedEvent** — External capital enters system via account
- **AccountCapitalAllocatedEvent** — Capital allocated out (available reduced)
- **AccountCapitalReservedEvent** — Capital moved from available to reserved
- **AccountReservationReleasedEvent** — Capital moved from reserved back to available
- **CapitalAccountFrozenEvent** — Account operations suspended with reason
- **CapitalAccountClosedEvent** — Account finalized (terminal state)

## Specifications
- **CanAllocateSpecification** — Status=Active AND amount > 0 AND amount <= AvailableBalance
- **CanReserveSpecification** — Status=Active AND amount > 0 AND amount <= AvailableBalance
- **CanCloseSpecification** — Status != Closed AND TotalBalance=0 AND ReservedBalance=0

## Domain Services
- **CapitalTransferService** — Coordinates inter-account capital transfers; validates currency match, allocates from source, funds destination

## Invariants (CRITICAL)
- AvailableBalance + ReservedBalance = TotalBalance (when Active)
- All balances must be non-negative
- Cannot operate on Frozen or Closed accounts
- Cannot close unless TotalBalance and ReservedBalance are zero
- Currency must match on all operations

## Policy Dependencies
- Currency consistency enforcement across all operations
- Status-based authorization (Frozen/Closed blocks all mutations)

## Integration Points
- **allocation** — Allocations source from AccountId
- **reserve** — Reserves hold against AccountId
- **vault** — Vault operations reference corresponding account balance
- **binding** — Bindings associate with AccountId
- **pool** — Pool aggregates account capital

## Lifecycle
```
Open() -> Active
  Fund() / Allocate() / Reserve() / ReleaseReservation() (while Active)
Freeze() -> Frozen (blocks all operations)
Close() -> Closed (requires zero balances, terminal)
```

## Notes
- Cross-domain references use raw Guid to avoid coupling
- All error methods are strongly typed via static CapitalAccountErrors class
- Uses DomainException for business rule violations and DomainInvariantViolationException for invariant breaches
