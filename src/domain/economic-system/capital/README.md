# Domain: Capital

## Classification
economic-system

## Context
capital

## Purpose
Models the financial backbone of the economic system: creation, movement, reservation, binding, valuation, and aggregation of capital. Every unit of capital is traceable from origin (account) through allocation, reservation, vault storage, and system-wide pooling.

## Core Responsibilities
- Capital account lifecycle management (open, fund, allocate, reserve, freeze, close)
- Directed capital allocation from source accounts to targets
- Time-bound capital reservations with automatic expiry
- Hierarchical capital storage via vaults and slices
- Account-to-owner binding with ownership transfer
- Asset valuation lifecycle (create, revalue, dispose)
- System-wide capital pooling and reconciliation

## Aggregate(s)
- **CapitalAccountAggregate** (`account/`)
  - Source of truth for capital balances (funding, allocation, reservation, freeze/close lifecycle)
  - Invariants: AvailableBalance + ReservedBalance = TotalBalance (when Active); all balances >= 0; currency consistency on all operations; cannot operate when Frozen or Closed

- **CapitalAllocationAggregate** (`allocation/`)
  - Tracks directed capital movement from source account to target
  - Invariants: Amount >= 0; unidirectional state transitions (Pending -> Completed | Released); cannot complete if Released; cannot release if Completed

- **AssetAggregate** (`asset/`)
  - Valued assets tied to owners with revaluation lifecycle
  - Invariants: Value >= 0; OwnerId must not be empty for non-disposed assets; cannot revalue or dispose a disposed asset

- **BindingAggregate** (`binding/`)
  - Account-to-owner relationship and ownership transfer
  - Invariants: AccountId and OwnerId must not be empty for Active bindings; new owner must differ from current; can only operate on Active bindings

- **CapitalPoolAggregate** (`pool/`)
  - System-wide capital aggregation across accounts
  - Invariants: TotalCapital >= 0; cannot reduce below zero; pool total must equal sum of all account totals (no artificial capital)

- **ReserveAggregate** (`reserve/`)
  - Time-bound capital holds with expiry semantics
  - Invariants: Amount > 0; ExpiresAt must be after ReservedAt; only Active reserves can change state

- **VaultAggregate** (`vault/`)
  - Segmented capital storage with slice-based capacity, allocation, and utilization tracking
  - Invariants: TotalStored = SUM(slice.TotalCapacity); all amounts >= 0; cannot operate on Closed slices; currency consistency

## Entities
- **VaultSlice** (`vault/entity/`) — Partition within a vault for segmented capital management. Enforces UsedAmount + AvailableAmount = TotalCapacity.

## Value Objects
- **AccountId** — Typed Guid wrapper for capital account identity
- **OwnerId** — Typed Guid wrapper for account owner identity
- **CapitalAccountStatus** — Enum: Active, Frozen, Closed
- **AllocationId** — Typed Guid wrapper for allocation identity
- **AllocationStatus** — Enum: Pending, Completed, Released
- **TargetId** — Typed Guid wrapper for allocation target identity
- **AssetId** — Typed Guid wrapper for asset identity
- **AssetStatus** — Enum: Active, Valued, Disposed
- **BindingId** — Typed Guid wrapper for binding identity
- **BindingStatus** — Enum: Active, Transferred, Released
- **OwnershipType** — Enum: Individual, Joint, Corporate, Trust
- **PoolId** — Typed Guid wrapper for pool identity
- **ReserveId** — Typed Guid wrapper for reserve identity
- **ReserveStatus** — Enum: Active, Released, Expired
- **VaultId** — Typed Guid wrapper for vault identity
- **SliceId** — Typed Guid wrapper for vault slice identity
- **SliceStatus** — Enum: Active, FullyAllocated, Closed

## Domain Events
- **CapitalAccountOpenedEvent** — Account created with zero balance
- **CapitalFundedEvent** — External capital enters system via account
- **AccountCapitalAllocatedEvent** — Available balance reduced by allocation
- **AccountCapitalReservedEvent** — Available reduced, reserved increased
- **AccountReservationReleasedEvent** — Reserved released back to available
- **CapitalAccountFrozenEvent** — Account operations suspended
- **CapitalAccountClosedEvent** — Account finalized (zero balance required)
- **AllocationCreatedEvent** — Capital allocation from source to target created
- **AllocationCompletedEvent** — Allocation finalized
- **AllocationReleasedEvent** — Allocation cancelled, capital returned
- **AssetCreatedEvent** — New asset registered with initial value
- **AssetValuedEvent** — Asset revalued (previous and new value captured)
- **AssetDisposedEvent** — Asset end-of-life
- **CapitalBoundEvent** — Account bound to owner with ownership type
- **OwnershipTransferredEvent** — Binding ownership transferred to new owner
- **BindingReleasedEvent** — Account-owner binding broken
- **PoolCreatedEvent** — Capital pool initialized
- **CapitalAggregatedEvent** — Capital added to pool from source account
- **CapitalReducedEvent** — Capital removed from pool to source account
- **ReserveCreatedEvent** — Time-bound capital hold created with expiry
- **ReserveReleasedEvent** — Reserve released early
- **ReserveExpiredEvent** — Reserve expired by time
- **VaultCreatedEvent** — Vault initialized for owner
- **VaultSliceCreatedEvent** — New slice added to vault
- **CapitalDepositedEvent** — Capital deposited into vault slice
- **CapitalAllocatedToSliceEvent** — Slice capital marked as used
- **CapitalReleasedFromSliceEvent** — Used capital returned to slice available
- **CapitalWithdrawnEvent** — Capital withdrawn from vault slice

## Specifications
- **CanAllocateSpecification** (account) — Status=Active, amount > 0, amount <= AvailableBalance
- **CanCloseSpecification** (account) — Status != Closed, TotalBalance=0, ReservedBalance=0
- **CanReserveSpecification** (account) — Status=Active, amount > 0, amount <= AvailableBalance
- **CanCompleteSpecification** (allocation) — Status=Pending
- **CanReleaseSpecification** (allocation) — Status=Pending
- **CanDisposeSpecification** (asset) — Status != Disposed
- **CanRevalueSpecification** (asset) — Status != Disposed
- **CanReleaseSpecification** (binding) — Status=Active
- **CanTransferSpecification** (binding) — Status=Active
- **CanReduceSpecification** (pool) — amount > 0, amount <= TotalCapital
- **PoolBalanceSpecification** (pool) — TotalCapital <= expected account sum
- **CanExpireSpecification** (reserve) — Status=Active, currentTime >= ExpiresAt
- **CanReleaseSpecification** (reserve) — Status=Active
- **CanAllocateToSliceSpecification** (vault) — amount > 0, slice exists, slice not Closed, amount <= slice.AvailableAmount
- **CanWithdrawSpecification** (vault) — amount > 0, slice exists, slice not Closed, amount <= slice.AvailableAmount

## Domain Services
- **CapitalTransferService** (account) — Coordinates capital movement between two accounts; validates currency match, allocates from source, funds destination
- **AllocationTraceService** (allocation) — Validates that allocations are traceable to valid source and target
- **AssetValuationService** (asset) — Calculates asset value appreciation
- **OwnershipValidationService** (binding) — Ensures each account has only one active binding
- **PoolAggregationService** (pool) — Reconciles pool total with sum of account balances
- **ReserveExpiryService** (reserve) — Manages time-based reserve expiration
- **VaultReconciliationService** (vault) — Validates vault total = sum of slice capacities; validates vault total <= account total

## Invariants (CRITICAL)
- **Balance Equation**: For any active account: AvailableBalance + ReservedBalance = TotalBalance
- **Vault-Slice Consistency**: VaultTotalStored = SUM(all slice.TotalCapacity)
- **Slice Capacity**: slice.UsedAmount + slice.AvailableAmount = slice.TotalCapacity
- **No Negative Balances**: All Amount values across all aggregates must be >= 0
- **Binding Uniqueness**: Each account has exactly one active binding (enforced via OwnershipValidationService)
- **Pool-Account Alignment**: PoolTotalCapital = SUM(AccountTotalBalance); no artificial capital
- **Capital Origin**: All capital in the system MUST originate from a CapitalAccountAggregate. No domain may create capital from nothing.
- **Traceability**: Every allocation must reference a valid source account and target

## Policy Dependencies
- **Currency Consistency**: All operations enforce currency matching across aggregates
- **Status-Based Authorization**: Operations restricted by aggregate state (e.g., cannot operate on Frozen/Closed accounts)
- **Time-Based Constraints**: Reserves enforce ExpiresAt > ReservedAt; expiry triggered by IClock
- **Cross-Aggregate Coordination**: CapitalTransferService validates before moving capital between accounts

## Integration Points
- **account <-> allocation**: Allocations source from AccountId
- **account <-> reserve**: Reserves hold against AccountId
- **account <-> vault**: Vault operations reference corresponding account balance
- **account <-> binding**: Bindings associate with AccountId
- **account <-> pool**: Pool aggregates account capital
- **vault -> VaultSlice**: Vault manages slice collection (composition)
- **asset -> binding**: Asset owner must match binding owner

## Lifecycle

### Account Lifecycle
```
[Open] -> Active -> Frozen -> Active (unfreeze via re-open)
                  -> Closed (requires zero balance, zero reserved)
```

### Allocation Lifecycle
```
[Allocate] -> Pending -> Completed
                      -> Released
```

### Asset Lifecycle
```
[Create] -> Active -> Valued (revalued) -> Disposed
                   -> Disposed
```

### Binding Lifecycle
```
[Bind] -> Active -> Transferred (ownership change)
                 -> Released
```

### Reserve Lifecycle
```
[Create] -> Active -> Released (early release)
                   -> Expired (time-based)
```

### Vault Slice Lifecycle
```
[AddSlice] -> Active -> FullyAllocated (available=0) -> Active (on release)
                     -> Closed
```

## Notes
- This context is pure domain: zero runtime, infrastructure, or engine dependencies
- Shared kernel primitives (AggregateRoot, DomainEvent, Amount, IClock) are the only external references
- All Guid-based value objects use record structs with validation guards
- Errors are strongly typed via static error classes using DomainException and DomainInvariantViolationException
- Cross-domain references use raw Guid (not typed IDs) to avoid coupling between subdomains
