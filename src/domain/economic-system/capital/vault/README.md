# Domain: Vault

## Classification
economic-system

## Context
capital

## Purpose
Structured capital storage using segmented slices. Rather than a flat balance, vaults partition capital into discrete slices — each with its own capacity, used/available tracking, and status. Enables controlled allocation, utilization monitoring, and capital segregation within a single vault.

## Core Responsibilities
- Creating vaults for owners with a specific currency
- Managing vault slices (creation, capacity tracking)
- Depositing capital into specific slices
- Allocating capital from slice available to slice used
- Releasing allocated capital back to available
- Withdrawing capital from slices (reduces capacity)
- Enforcing slice-level and vault-level invariants

## Aggregate(s)
- **VaultAggregate**
  - Event-sourced, sealed. Manages a collection of VaultSlice entities
  - Invariants: TotalStored = SUM(all slice.TotalCapacity); all amounts >= 0; cannot operate on Closed slices; currency consistency; no duplicate slice IDs

## Entities
- **VaultSlice** — Partition of capital within a vault with independent capacity constraints. Tracks TotalCapacity, UsedAmount, AvailableAmount per slice. Enforces UsedAmount + AvailableAmount = TotalCapacity on every state change.

## Value Objects
- **VaultId** — Typed Guid wrapper with From() factory for unique vault identity
- **SliceId** — Typed Guid wrapper with From() factory for unique slice identity
- **SliceStatus** — Enum: Active, FullyAllocated, Closed

## Domain Events
- **VaultCreatedEvent** — Vault initialized for owner with currency
- **VaultSliceCreatedEvent** — New slice added to vault with initial capacity
- **CapitalDepositedEvent** — Capital deposited into vault slice
- **CapitalAllocatedToSliceEvent** — Slice capital moved from available to used
- **CapitalReleasedFromSliceEvent** — Slice capital moved from used to available
- **CapitalWithdrawnEvent** — Capital withdrawn from vault slice

## Specifications
- **CanAllocateToSliceSpecification** — amount > 0 AND slice exists AND slice not Closed AND amount <= slice.AvailableAmount
- **CanWithdrawSpecification** — amount > 0 AND slice exists AND slice not Closed AND amount <= slice.AvailableAmount

## Domain Services
- **VaultReconciliationService** — Validates vault internal consistency (vault total = sum of slice capacities) and account alignment (vault total <= account total)

## Invariants (CRITICAL)
- Slice Capacity: UsedAmount + AvailableAmount = TotalCapacity (per slice)
- Vault Total: TotalStored = SUM(all slice.TotalCapacity)
- All capital must belong to a slice — no unsliced capital
- Cannot exceed slice available when allocating
- Cannot release more than used amount
- Cannot withdraw more than available amount
- Cannot operate on closed slices
- Slice currency must match vault currency
- No duplicate slice IDs

## Policy Dependencies
- Currency consistency enforcement between vault and slices
- Cross-aggregate alignment: vault total <= account total

## Integration Points
- **account** — Vault operations reference corresponding account balance for alignment validation

## Lifecycle
### Vault
```
Create() -> Initialized (zero total, no slices)
  AddSlice() / DepositToSlice() / AllocateFromSlice() / ReleaseToSlice() / WithdrawFromSlice()
```

### Slice
```
AddSlice() -> Active
  Allocate() -> FullyAllocated (when available=0)
  Release() -> Active (when allocation released)
  -> Closed (terminal)
```

## Notes
- VaultSlice is a child entity within VaultAggregate (composition)
- Cross-domain references (OwnerId) use raw Guid to avoid coupling
- All error methods are strongly typed via static VaultErrors class
- 13 distinct error types covering both operation and invariant violations
