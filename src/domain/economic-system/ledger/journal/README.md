# Domain: Journal

## Classification
economic-system

## Context
ledger

## Purpose
Balancing engine of the ledger. Groups entries and enforces the fundamental accounting invariant: TotalDebit == TotalCredit. No unbalanced journal can be posted. This is the gatekeeper of financial integrity.

## Core Responsibilities
- Creating journals to group related entries
- Adding debit and credit entries to open journals
- Enforcing single-currency consistency per journal
- Enforcing double-entry balance before posting
- Finalizing journals (posting) — making them immutable

## Aggregate(s)
- **JournalAggregate**
  - Event-sourced, sealed. Manages a collection of JournalEntry entities
  - Invariants: TotalDebit == TotalCredit (enforced on posting and in EnsureInvariants for posted journals); minimum 2 entries before posting; single currency; immutable after posting; all entry amounts must be positive

## Entities
- **JournalEntry** — Lightweight entry reference within journal domain. Uses BookingDirection (local to journal domain, decoupled from entry domain's EntryDirection). Properties: EntryId, AccountId, Amount, Currency, Direction.

## Value Objects
- **JournalId** — Typed Guid wrapper for unique journal identity
- **JournalStatus** — Enum: Open, Posted
- **BookingDirection** — Enum: Debit, Credit (local to journal domain, decoupled from entry domain)

## Domain Events
- **JournalCreatedEvent** — Empty journal opened
- **JournalEntryAddedEvent** — Entry added to open journal (captures EntryId, AccountId, Amount, Currency, Direction)
- **JournalPostedEvent** — Journal balanced and finalized (captures TotalDebit, TotalCredit, PostedAt)

## Specifications
- **CanPostSpecification** — Status=Open AND entries >= 2 AND TotalDebit == TotalCredit
- **IsBalancedSpecification** — TotalDebit == TotalCredit

## Domain Services
- **JournalBalanceService** — Calculates and verifies journal balances; IsBalanced() returns bool, GetImbalance() returns the absolute difference

## Invariants (CRITICAL)
- TotalDebit == TotalCredit for every posted journal
- Must have at least 2 entries before posting
- Cannot modify a posted journal
- All entries within a journal must use the same currency
- Entry amounts must be positive

## Policy Dependencies
- Double-entry accounting enforcement
- Currency consistency per journal

## Integration Points
- **entry** — Entries belong to journals; journal entries reference accounts
- **ledger** — Posted journals are appended to the ledger

## Lifecycle
```
Create() -> Open
  AddEntry() (repeatable while Open)
Post() -> Posted (immutable, terminal)
```

## Notes
- BookingDirection is intentionally separate from entry domain's EntryDirection to maintain domain decoupling
- Currency is set on first entry added and enforced for all subsequent entries
- All error methods are strongly typed via static JournalErrors class
