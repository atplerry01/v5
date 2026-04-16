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
- **CapitalAllocatedToSpvEvent** — Allocation directed to an SPV target with declared ownership percentage
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

## Policy and Access Control

WHYCEPOLICY governs every capital command. Each command is bound to exactly one policy action; runtime middleware performs the policy decision before the engine executes the command. The decision is recorded in the WhyceChain block alongside the emitted events, so every capital operation is auditable end-to-end.

### Naming convention

`whyce.{classification}.{context}.{domain}.{action}` — matches the project-wide policy id format established by `TodoPolicyIds`. (Spec template `capital.{domain}.{action}` is a shorthand; the canonical id includes the `whyce.economic` prefix so policies remain unique across contexts.)

Constants for every action are codified per domain in:
- `Whycespace.Shared.Contracts.Economic.Capital.Account.CapitalAccountPolicyIds`
- `Whycespace.Shared.Contracts.Economic.Capital.Allocation.CapitalAllocationPolicyIds`
- `Whycespace.Shared.Contracts.Economic.Capital.Asset.CapitalAssetPolicyIds`
- `Whycespace.Shared.Contracts.Economic.Capital.Binding.CapitalBindingPolicyIds`
- `Whycespace.Shared.Contracts.Economic.Capital.Pool.CapitalPoolPolicyIds`
- `Whycespace.Shared.Contracts.Economic.Capital.Reserve.CapitalReservePolicyIds`
- `Whycespace.Shared.Contracts.Economic.Capital.Vault.CapitalVaultPolicyIds`

### Roles

- **Owner** — actor whose WhyceID matches the aggregate's `OwnerId` (or, for account-derived aggregates, the owner bound to the source `AccountId`).
- **Operator** — system actor with the `capital.operator` entitlement; performs orchestration the user cannot drive directly (allocation completion, pool aggregation, time-driven expiry, SPV declaration).
- **Admin** — system actor with the `capital.admin` entitlement; performs lifecycle-terminating or override actions (freeze, close, dispose, binding release).
- **External** — third-party caller without WhyceID context (e.g. external rail callback). Default deny; explicit allow only on `account.fund` against an attested rail.

### Command → Policy Action Map

| # | Command | Policy action constant | Policy id |
|---|---|---|---|
| account |
| 1 | `OpenCapitalAccountCommand` | `CapitalAccountPolicyIds.Open` | `whyce.economic.capital.account.open` |
| 2 | `FundCapitalAccountCommand` | `CapitalAccountPolicyIds.Fund` | `whyce.economic.capital.account.fund` |
| 3 | `AllocateCapitalAccountCommand` | `CapitalAccountPolicyIds.Allocate` | `whyce.economic.capital.account.allocate` |
| 4 | `ReserveCapitalAccountCommand` | `CapitalAccountPolicyIds.Reserve` | `whyce.economic.capital.account.reserve` |
| 5 | `ReleaseCapitalReservationCommand` | `CapitalAccountPolicyIds.ReleaseReservation` | `whyce.economic.capital.account.release_reservation` |
| 6 | `FreezeCapitalAccountCommand` | `CapitalAccountPolicyIds.Freeze` | `whyce.economic.capital.account.freeze` |
| 7 | `CloseCapitalAccountCommand` | `CapitalAccountPolicyIds.Close` | `whyce.economic.capital.account.close` |
| allocation |
| 8 | `CreateCapitalAllocationCommand` | `CapitalAllocationPolicyIds.Create` | `whyce.economic.capital.allocation.create` |
| 9 | `ReleaseCapitalAllocationCommand` | `CapitalAllocationPolicyIds.Release` | `whyce.economic.capital.allocation.release` |
| 10 | `CompleteCapitalAllocationCommand` | `CapitalAllocationPolicyIds.Complete` | `whyce.economic.capital.allocation.complete` |
| 11 | `AllocateCapitalToSpvCommand` | `CapitalAllocationPolicyIds.SpvDeclare` | `whyce.economic.capital.allocation.spv_declare` |
| asset |
| 12 | `CreateAssetCommand` | `CapitalAssetPolicyIds.Create` | `whyce.economic.capital.asset.create` |
| 13 | `RevalueAssetCommand` | `CapitalAssetPolicyIds.Revalue` | `whyce.economic.capital.asset.revalue` |
| 14 | `DisposeAssetCommand` | `CapitalAssetPolicyIds.Dispose` | `whyce.economic.capital.asset.dispose` |
| binding |
| 15 | `BindCapitalCommand` | `CapitalBindingPolicyIds.Bind` | `whyce.economic.capital.binding.bind` |
| 16 | `TransferBindingOwnershipCommand` | `CapitalBindingPolicyIds.Transfer` | `whyce.economic.capital.binding.transfer` |
| 17 | `ReleaseBindingCommand` | `CapitalBindingPolicyIds.Release` | `whyce.economic.capital.binding.release` |
| pool |
| 18 | `CreateCapitalPoolCommand` | `CapitalPoolPolicyIds.Create` | `whyce.economic.capital.pool.create` |
| 19 | `AggregateCapitalToPoolCommand` | `CapitalPoolPolicyIds.Aggregate` | `whyce.economic.capital.pool.aggregate` |
| 20 | `ReduceCapitalFromPoolCommand` | `CapitalPoolPolicyIds.Reduce` | `whyce.economic.capital.pool.reduce` |
| reserve |
| 21 | `CreateCapitalReserveCommand` | `CapitalReservePolicyIds.Create` | `whyce.economic.capital.reserve.create` |
| 22 | `ReleaseCapitalReserveCommand` | `CapitalReservePolicyIds.Release` | `whyce.economic.capital.reserve.release` |
| 23 | `ExpireCapitalReserveCommand` | `CapitalReservePolicyIds.Expire` | `whyce.economic.capital.reserve.expire` |
| vault |
| 24 | `CreateCapitalVaultCommand` | `CapitalVaultPolicyIds.Create` | `whyce.economic.capital.vault.create` |
| 25 | `AddCapitalVaultSliceCommand` | `CapitalVaultPolicyIds.SliceAdd` | `whyce.economic.capital.vault.slice_add` |
| 26 | `DepositToCapitalVaultSliceCommand` | `CapitalVaultPolicyIds.SliceDeposit` | `whyce.economic.capital.vault.slice_deposit` |
| 27 | `AllocateFromCapitalVaultSliceCommand` | `CapitalVaultPolicyIds.SliceAllocate` | `whyce.economic.capital.vault.slice_allocate` |
| 28 | `ReleaseToCapitalVaultSliceCommand` | `CapitalVaultPolicyIds.SliceRelease` | `whyce.economic.capital.vault.slice_release` |
| 29 | `WithdrawFromCapitalVaultSliceCommand` | `CapitalVaultPolicyIds.SliceWithdraw` | `whyce.economic.capital.vault.slice_withdraw` |

29 commands → 29 unique policy actions. No command is ungoverned. No policy id is duplicated.

### Restricted Action Matrix

Conditions are policy preconditions enforced before the engine executes. Domain invariants run after the policy gate; this matrix governs *who may attempt* the command, not whether the aggregate accepts it.

| Action | Role | Conditions | Allowed / Denied |
|---|---|---|---|
| `account.open` | Owner | KYC entitlement present on actor; currency in supported set | Allowed |
| `account.open` | Operator | system-provisioning entitlement | Allowed |
| `account.open` | External | — | Denied |
| `account.fund` | Operator | source account exists; currency match | Allowed |
| `account.fund` | External | attested external rail callback signature valid | Conditional |
| `account.fund` | Owner | — | Denied (funding flows through Operator/External) |
| `account.allocate` | Owner | actor.WhyceId == account.binding.owner; account Active | Allowed |
| `account.allocate` | Operator | — | Allowed |
| `account.reserve` | Owner | actor owns account; account Active | Allowed |
| `account.reserve` | Operator | — | Allowed |
| `account.release_reservation` | Owner | actor owns account; reserve was created by same owner | Allowed |
| `account.release_reservation` | Operator | — | Allowed |
| `account.freeze` | Admin | reason supplied | Allowed |
| `account.freeze` | Owner / Operator | — | Denied |
| `account.close` | Admin | TotalBalance == 0 ∧ ReservedBalance == 0 ∧ no active vaults | Allowed |
| `account.close` | Owner / Operator | — | Denied |
| `allocation.create` | Owner | actor owns SourceAccount; SourceAccount Active | Allowed |
| `allocation.create` | Operator | — | Allowed |
| `allocation.release` | Owner | actor owns SourceAccount; allocation Pending | Allowed |
| `allocation.release` | Operator | allocation Pending | Allowed |
| `allocation.complete` | Operator | allocation Pending; target receipt confirmed | Allowed |
| `allocation.complete` | Owner / Admin | — | Denied (system-completed) |
| `allocation.spv_declare` | Operator | SPV target registered in mapping | Allowed |
| `allocation.spv_declare` | Owner | — | Denied |
| `asset.create` | Owner | actor.WhyceId == OwnerId | Allowed |
| `asset.create` | Operator | — | Allowed |
| `asset.revalue` | Operator | revaluation source attestation valid | Allowed |
| `asset.revalue` | Owner | — | Denied |
| `asset.dispose` | Owner | actor.WhyceId == asset.OwnerId; asset not Disposed | Allowed |
| `asset.dispose` | Admin | — | Allowed |
| `binding.bind` | Operator | account exists; actor has KYC-pass attestation for target Owner | Allowed |
| `binding.bind` | Owner / External | — | Denied |
| `binding.transfer` | Owner | dual consent: current owner WhyceId AND new owner WhyceId both signed; new owner trust ≥ floor; binding Active | Allowed |
| `binding.transfer` | Admin | judicial / compliance override flag set | Conditional |
| `binding.transfer` | Operator / External | — | Denied |
| `binding.release` | Admin | binding Active; closure event present | Allowed |
| `binding.release` | Owner / Operator | — | Denied |
| `pool.create` | Operator | currency not yet pooled | Allowed |
| `pool.create` | Owner / Admin / External | — | Denied |
| `pool.aggregate` | Operator | source account exists; amount > 0 | Allowed |
| `pool.aggregate` | Owner / Admin / External | — | Denied |
| `pool.reduce` | Operator | pool.TotalCapital ≥ amount | Allowed |
| `pool.reduce` | Owner / Admin / External | — | Denied |
| `reserve.create` | Owner | actor owns AccountId; account Active; ExpiresAt > ReservedAt | Allowed |
| `reserve.create` | Operator | — | Allowed |
| `reserve.release` | Owner | actor owns reserve; reserve Active | Allowed |
| `reserve.release` | Operator | reserve Active | Allowed |
| `reserve.expire` | Operator | currentTime ≥ ExpiresAt; reserve Active | Allowed |
| `reserve.expire` | Owner / External | — | Denied |
| `vault.create` | Owner | actor.WhyceId == OwnerId | Allowed |
| `vault.create` | Operator | — | Allowed |
| `vault.slice_add` | Owner | actor owns vault; TotalCapacity > 0 | Allowed |
| `vault.slice_add` | Admin | — | Allowed |
| `vault.slice_deposit` | Owner | actor owns vault; currency match | Allowed |
| `vault.slice_deposit` | Operator | — | Allowed |
| `vault.slice_allocate` | Owner | actor owns vault; slice not Closed; amount ≤ slice.AvailableAmount | Allowed |
| `vault.slice_allocate` | Operator / Admin | — | Denied (allocation is owner-driven) |
| `vault.slice_release` | Owner | actor owns vault; slice not Closed | Allowed |
| `vault.slice_release` | Operator | — | Allowed |
| `vault.slice_withdraw` | Owner | actor owns vault; amount ≤ slice.AvailableAmount; amount ≤ withdrawal_threshold | Allowed |
| `vault.slice_withdraw` | Owner + Admin co-sign | amount > withdrawal_threshold; both signatures present | Conditional |
| `vault.slice_withdraw` | Operator / External | — | Denied |

### Trust score gates

- `binding.transfer`: new owner's trust score must meet `binding.transfer.trust_floor` (configurable; defaults to project anti-bot baseline).
- `vault.slice_withdraw` above `withdrawal_threshold`: requires elevated trust score on the owner AND admin co-signature.
- `account.fund` via External: rail attestation must be signed; failure returns 401 before policy evaluation.

### Integration notes

- **Where policy is enforced.** WHYCEPOLICY runs as a runtime middleware stage *between* the prePolicy admission middleware and engine dispatch. Each capital command surfaces its policy id via the constant declared in the `Capital{Domain}PolicyIds` class for that domain.
- **How commands are evaluated.** The middleware reads the actor identity from the dispatched `CommandContext`, looks up the policy id, and resolves the role + conditions. A `PolicyEvaluatedEvent` is emitted to the constitutional policy stream regardless of decision (per existing `PolicyEventificationTests` precedent). On Deny, the engine is short-circuited; no domain event is emitted; the API returns `400 BadRequest` with the policy decision code.
- **WhyceChain logging.** Every policy decision becomes a chain block — decision hash, actor id, policy id, conditions evaluated, outcome — anchored alongside the domain event chain so the audit trail remains end-to-end.
- **Replay determinism.** Policy decisions for a given (commandId, actorId, policyId, contextHash) are idempotent; replays produce the same decision under a frozen clock.

### Cross-references

- Project policy infrastructure: `src/runtime/middleware/Policy/PolicyMiddleware.cs`
- Policy evaluation contract: `src/shared/contracts/policy/`
- Existing precedent: [TodoPolicyIds.cs](../../../shared/contracts/operational/sandbox/todo/TodoPolicyIds.cs)
- Policy emission test: `tests/integration/constitutional/policy/decision/PolicyEventificationTests.cs`
