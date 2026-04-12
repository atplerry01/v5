# Domain: Ledger

## Classification
economic-system

## Context
ledger

## Purpose
Reconstruction layer. Aggregates posted journals into an append-only ledger and provides the mechanism to reconstruct account balances from entries. The ledger does NOT store balances — it derives them on demand from the event stream.

## Core Responsibilities
- Opening ledgers per currency
- Appending posted journals (append-only, no removal)
- Preventing duplicate journal references
- Providing balance reconstruction from flat entry data

## Aggregate(s)
- **LedgerAggregate**
  - Event-sourced, sealed. Manages a collection of PostedJournalReference entities
  - Invariants: Append-only (journals can be added but never removed); no duplicate journal references; no direct balance storage; no direct state mutation outside event-sourced Apply()

## Entities
- **PostedJournalReference** — Lightweight reference to a posted journal within the ledger. Properties: JournalId, PostedAt.

## Value Objects
- **LedgerId** — Typed Guid wrapper for unique ledger identity
- **LedgerAccountBalance** — Computed balance value object: AccountId, DebitTotal, CreditTotal, NetBalance. Never stored, always reconstructed.

## Domain Events
- **LedgerOpenedEvent** — Ledger initialized with currency
- **JournalAppendedToLedgerEvent** — Posted journal added to ledger

## Specifications
- **CanAppendJournalSpecification** — JournalId non-empty AND not already in Journals list
- **HasJournalsSpecification** — Journals.Count > 0

## Domain Services
- **LedgerReconstructionService** — Reconstructs account balances from flat entry data. Takes ReconstructionEntry records (AccountId, Amount, IsDebit), groups by account, calculates debit total, credit total, and net balance. Returns IReadOnlyList<LedgerAccountBalance>. Never stores balances.

## Invariants (CRITICAL)
- Ledger is append-only — journals can be added but never removed
- No duplicate journal references allowed
- No direct balance storage — balances are ALWAYS reconstructed from entries
- No direct state mutation outside of event-sourced Apply()

## Policy Dependencies
- Append-only enforcement
- Balance reconstruction over direct storage

## Integration Points
- **journal** — Posted journals are appended to the ledger via PostedJournalReference

## Lifecycle
```
Open() -> Initialized
  AppendJournal() (append-only, repeatable)
```

## Notes
- The ledger is fully reconstructable: load all LedgerEntryRecordedEvent events, pass to ReconstructBalances(), get computed balances
- LedgerAccountBalance is a value object, not stored state — it exists only as a computation result
- All error methods are strongly typed via static LedgerErrors class
