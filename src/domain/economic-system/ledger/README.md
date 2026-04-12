# Domain: Ledger

## Classification
economic-system

## Context
ledger

## Purpose
Financial truth layer implementing a fully deterministic, append-only, double-entry accounting system. Every financial movement is recorded as an immutable entry, grouped into balanced journals, and reconstructable from the event stream. No balances are stored directly — they are always derived.

## Core Responsibilities
- Recording atomic, immutable financial entries (debit/credit)
- Grouping entries into balanced journals (TotalDebit == TotalCredit)
- Appending posted journals to an append-only ledger
- Reconstructing account balances from the event stream
- Tracking financial obligations (payable/receivable)
- Settling obligations by linking journals to external finalization
- Managing treasury liquidity pools

## Aggregate(s)
- **LedgerEntryAggregate** (`entry/`)
  - Atomic, immutable financial movement — the smallest unit of truth
  - Invariants: Immutable after creation; must reference journal and account; amount must be > 0

- **JournalAggregate** (`journal/`)
  - Groups entries and enforces double-entry balance
  - Invariants: TotalDebit == TotalCredit for posted journals; minimum 2 entries; single-currency; immutable after posting

- **LedgerAggregate** (`ledger/`)
  - Append-only journal aggregation — reconstructs account balances from entries
  - Invariants: Append-only (no removal); no duplicate journals; no direct balance storage

- **ObligationAggregate** (`obligation/`)
  - Tracks financial commitments (payable/receivable)
  - Invariants: Amount must be positive; only Pending can transition; mutually exclusive terminal states

- **SettlementAggregate** (`settlement/`)
  - Links internal journals to external finalization
  - Invariants: Must reference journal and obligation; amount must be positive; only Initiated can transition

- **TreasuryAggregate** (`treasury/`)
  - Manages liquidity pools and fund movement readiness
  - Invariants: Balance must never be negative; allocation cannot exceed balance

## Entities
- **JournalEntry** (`journal/entity/`) — Lightweight entry reference within journal domain; uses BookingDirection (decoupled from entry domain's EntryDirection)
- **PostedJournalReference** (`ledger/entity/`) — Lightweight reference to posted journal within ledger; tracks JournalId and PostedAt

## Value Objects
- **EntryId** — Unique entry identifier
- **EntryDirection** — Enum: Debit, Credit
- **JournalId** — Unique journal identifier
- **JournalStatus** — Enum: Open, Posted
- **BookingDirection** — Enum: Debit, Credit (local to journal domain)
- **LedgerId** — Unique ledger identifier
- **LedgerAccountBalance** — Computed balance per account (DebitTotal, CreditTotal, NetBalance)
- **ObligationId** — Unique obligation identifier
- **ObligationType** — Enum: Payable, Receivable
- **ObligationStatus** — Enum: Pending, Fulfilled, Cancelled
- **SettlementId** — Unique settlement identifier
- **SettlementStatus** — Enum: Initiated, Completed, Failed
- **TreasuryId** — Unique treasury identifier

## Domain Events
- **LedgerEntryRecordedEvent** — Immutable entry recorded (one-time)
- **JournalCreatedEvent** — Empty journal opened
- **JournalEntryAddedEvent** — Entry added to open journal
- **JournalPostedEvent** — Journal balanced and finalized
- **LedgerOpenedEvent** — Ledger initialized
- **JournalAppendedToLedgerEvent** — Posted journal added to ledger
- **ObligationCreatedEvent** — Commitment recorded (payable/receivable)
- **ObligationFulfilledEvent** — Obligation satisfied via settlement
- **ObligationCancelledEvent** — Obligation cancelled with reason
- **SettlementInitiatedEvent** — Settlement links journal + obligation
- **SettlementCompletedEvent** — External finalization confirmed
- **SettlementFailedEvent** — Settlement failed with reason
- **TreasuryCreatedEvent** — Treasury pool initialized
- **TreasuryFundAllocatedEvent** — Funds allocated (balance decreased)
- **TreasuryFundReleasedEvent** — Funds released (balance increased)

## Specifications
- **ValidEntrySpecification** (entry) — Amount > 0, JournalId and AccountId non-empty
- **CanPostSpecification** (journal) — Status=Open AND entries >= 2 AND TotalDebit == TotalCredit
- **IsBalancedSpecification** (journal) — TotalDebit == TotalCredit
- **CanAppendJournalSpecification** (ledger) — JournalId non-empty AND not already appended
- **HasJournalsSpecification** (ledger) — Journals.Count > 0
- **CanFulfilSpecification** (obligation) — Status == Pending
- **CanCancelSpecification** (obligation) — Status == Pending
- **CanCompleteSpecification** (settlement) — Status == Initiated
- **CanFailSpecification** (settlement) — Status == Initiated
- **CanAllocateSpecification** (treasury) — Amount > 0 AND Amount <= Balance

## Domain Services
- **EntryValidationService** (entry) — Validates entry integrity (amount, journal ref, account ref, direction)
- **JournalBalanceService** (journal) — Calculates and verifies journal balances; returns imbalance amount
- **LedgerReconstructionService** (ledger) — Reconstructs account balances from flat entry data; never stores balances
- **ObligationMatchingService** (obligation) — Validates settlement amounts match obligations
- **SettlementReconciliationService** (settlement) — Validates settlement references are complete (JournalId and ObligationId non-empty)
- **TreasuryLiquidityService** (treasury) — Checks if treasury has sufficient liquidity for a given amount

## Invariants (CRITICAL)
- Entry Immutability: LedgerEntryAggregate cannot be modified after recording
- Journal Balance: TotalDebit == TotalCredit for every posted journal
- Journal Minimum: Journal must have >= 2 entries before posting
- Ledger Append-Only: Journals can only be appended, never removed or modified
- No Direct Balance Storage: Balances are always reconstructed from entries
- Entry-Journal Link: Every entry references a journal; every journal entry references an account
- Settlement-Obligation Link: Settlements must reference both journal and obligation
- Non-negative treasury balance

## Policy Dependencies
- Double-entry accounting enforcement
- Currency consistency per journal
- Immutability after finalization (entries, posted journals, finalized audit records)

## Integration Points
- **entry <-> journal** — Entries belong to journals via JournalEntry entity
- **journal <-> ledger** — Posted journals appended via PostedJournalReference
- **obligation <-> settlement** — Settlements fulfil obligations
- **settlement <-> journal** — Settlements reference posted journals
- **treasury <-> settlement** — Treasury provides liquidity for settlements

## Lifecycle

### Entry
```
Record() -> Recorded (immutable, no further transitions)
```

### Journal
```
Create() -> Open
  AddEntry() (repeatable while Open)
Post() -> Posted (immutable, terminal)
```

### Ledger
```
Open() -> Initialized
  AppendJournal() (append-only, repeatable)
```

### Obligation
```
Create() -> Pending
  Fulfil() -> Fulfilled (terminal)
  OR
  Cancel() -> Cancelled (terminal)
```

### Settlement
```
Initiate() -> Initiated
  Complete() -> Completed (terminal)
  OR
  Fail() -> Failed (terminal)
```

### Treasury
```
Create() -> Initialized (zero balance)
  AllocateFunds() / ReleaseFunds() (repeatable)
```

## Notes
- Pure domain — zero runtime, infrastructure, or engine dependencies
- Only shared kernel primitives referenced
- The ledger is fully reconstructable: load all LedgerEntryRecordedEvent events, pass to LedgerReconstructionService, get computed LedgerAccountBalance per account
- The event stream IS the ledger — no stored balances needed
