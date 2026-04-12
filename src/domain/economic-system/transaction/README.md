# Domain: Transaction

## Classification
economic-system

## Context
transaction

## Purpose
Execution layer that transforms economic intent into valid, balanced financial transactions. Bridges user-facing wallets and instructions to the ledger system. Transaction does NOT create financial truth — it produces journals. Journals produce entries. Entries define truth.

## Core Responsibilities
- Capturing economic intent via instructions (what should happen)
- Orchestrating transaction execution and producing balanced journals
- Calculating and applying fees/pricing via charges
- Enforcing transaction constraints via limits (per-txn, daily volume/count)
- Providing user-facing wallet abstraction as entry point for transactions

## Aggregate(s)
- **TransactionInstructionAggregate** (`instruction/`)
  - Defines economic intent (source, destination, amount, type) before execution
  - Invariants: Amount > 0; accounts non-empty and must differ; unidirectional state transitions

- **TransactionAggregate** (`transaction/`)
  - Orchestrates execution, produces balanced journals. MUST NOT create entries directly
  - Invariants: Must produce a journal on completion (T1); must reference instruction (T5); only Initiated can transition

- **ChargeAggregate** (`charge/`)
  - Calculates and applies deterministic, traceable fees
  - Invariants: ChargeAmount >= 0; BaseAmount > 0; must reference transaction; only Calculated can be Applied

- **LimitAggregate** (`limit/`)
  - Enforces transaction constraints and tracks cumulative utilization
  - Invariants: Threshold > 0; CurrentUtilization >= 0; only Active limits can be checked

- **WalletAggregate** (`wallet/`)
  - User-facing abstraction mapping owner to account. Does NOT hold balances
  - Invariants: Active wallet must have AccountId (non-empty); OwnerId non-empty

## Entities
None

## Value Objects
- **InstructionId** — Unique instruction identifier
- **InstructionType** — Enum: Transfer, Payment, Allocation, Refund
- **InstructionStatus** — Enum: Pending, Executed, Cancelled
- **TransactionId** — Unique transaction identifier
- **TransactionStatus** — Enum: Initiated, Completed, Failed
- **ChargeId** — Unique charge identifier
- **ChargeType** — Enum: Fixed, Percentage
- **ChargeStatus** — Enum: Calculated, Applied
- **LimitId** — Unique limit identifier
- **LimitType** — Enum: PerTransaction, DailyVolume, DailyCount
- **LimitStatus** — Enum: Active, Exceeded
- **WalletId** — Unique wallet identifier
- **WalletStatus** — Enum: Active, Inactive

## Domain Events
- **TransactionInstructionCreatedEvent** — Intent declared
- **TransactionInstructionExecutedEvent** — Instruction fulfilled
- **TransactionInstructionCancelledEvent** — Instruction cancelled with reason
- **TransactionInitiatedEvent** — Execution begins
- **TransactionCompletedEvent** — Journal produced, execution finalized (T1)
- **TransactionFailedEvent** — Execution failed with reason
- **ChargeCalculatedEvent** — Fee calculated for a transaction
- **ChargeAppliedEvent** — Fee applied to the transaction (T4)
- **LimitDefinedEvent** — Limit created for account
- **LimitCheckedEvent** — Limit validated, transaction within bounds (T3)
- **LimitExceededEvent** — Transaction would exceed threshold
- **WalletCreatedEvent** — Wallet initialized with account mapping
- **TransactionRequestedEvent** — User requests transaction via wallet (signal event, no state mutation)

## Specifications
- **CanExecuteSpecification** (instruction) — Status == Pending
- **CanCancelSpecification** (instruction) — Status == Pending
- **CanExecuteSpecification** (transaction) — Status == Initiated
- **CanCompleteSpecification** (transaction) — Status == Initiated
- **CanApplySpecification** (charge) — Status == Calculated
- **IsWithinLimitSpecification** (limit) — Status=Active AND utilization + amount <= threshold
- **CanInitiateTransactionSpecification** (wallet) — Status=Active AND AccountId non-empty

## Domain Services
- **InstructionValidationService** (instruction) — Validates instruction structure: amount > 0, accounts non-empty, accounts differ
- **TransactionJournalLinkService** (transaction) — Validates cross-domain invariant T1: transaction Completed AND JournalId non-empty
- **ChargeCalculationService** (charge) — Calculates fees deterministically: CalculateFixedCharge and CalculatePercentageCharge (banker's rounding)
- **LimitEnforcementService** (limit) — Evaluates transaction against limit: Active AND utilization + amount <= threshold
- **WalletTransactionService** (wallet) — Retrieves and validates wallet account mapping

## Invariants (CRITICAL)
- T1: Every transaction MUST produce exactly one journal
- T2: Transaction cannot create entries directly
- T3: Transaction must validate limits before execution
- T4: Charges must be applied before journal creation
- T5: Transaction must originate from instruction
- Wallet must map to account when Active

## Policy Dependencies
- Transaction Rules (T1-T5) enforcement
- Canonical execution order: Instruction -> Limit Check -> Charge Calculate -> Charge Apply -> Transaction Initiate -> Transaction Complete -> Instruction Execute

## Integration Points
- **wallet -> instruction** — Wallet initiates instructions
- **instruction -> transaction** — Instruction creates transaction
- **transaction -> limit** — Limits validate BEFORE execution
- **transaction -> charge** — Charges applied BEFORE completion
- **transaction -> journal** (ledger context) — Transaction MUST produce a journal

## Lifecycle

### Instruction
```
CreateInstruction() -> Pending
  MarkExecuted() -> Executed (terminal)
  OR
  CancelInstruction() -> Cancelled (terminal)
```

### Transaction
```
Initiate() -> Initiated (requires InstructionId)
  Complete() -> Completed (requires JournalId, terminal)
  OR
  Fail() -> Failed (terminal, with reason)
```

### Charge
```
Calculate() -> Calculated
  ApplyCharge() -> Applied (terminal)
```

### Limit
```
Define() -> Active (zero utilization)
  Check() -> Active (utilization updated) OR Exceeded (terminal)
```

### Wallet
```
Create() -> Active (immutable status after creation)
  RequestTransaction() -> signal event only, no state mutation
```

## Notes
- Pure domain — zero runtime, infrastructure, or engine dependencies
- Canonical execution order: steps 2-4 (limit, charge calculate, charge apply) MUST complete before step 6 (transaction complete)
- TransactionRequestedEvent is a signal event — it does not mutate wallet state
- All cross-domain references use raw Guid to avoid coupling
