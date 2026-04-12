# Domain: Entry

## Classification
economic-system

## Context
ledger

## Purpose
Represents the atomic unit of financial truth. Each ledger entry is an immutable record of a single debit or credit against an account, linked to a journal. Entries cannot be modified after creation — corrections require new compensating entries.

## Core Responsibilities
- Recording atomic financial movements (debit or credit)
- Enforcing immutability after creation
- Linking each entry to a journal and account
- Validating entry integrity (amount, direction, references)

## Aggregate(s)
- **LedgerEntryAggregate**
  - Event-sourced, sealed, CREATE-ONLY. Creates and finalizes the entry in one step via static Record() factory
  - Invariants: Immutable after creation; must belong to a journal (JournalId non-empty); must reference an account (AccountId non-empty); amount must be > 0; direction must be Debit or Credit

## Entities
None

## Value Objects
- **EntryId** — Typed Guid wrapper for unique entry identity
- **EntryDirection** — Enum: Debit, Credit

## Domain Events
- **LedgerEntryRecordedEvent** — Entry recorded (one-time, immutable). Captures EntryId, JournalId, AccountId, Amount, Currency, Direction, CreatedAt

## Specifications
- **ValidEntrySpecification** — Amount > 0 AND JournalId non-empty AND AccountId non-empty

## Domain Services
- **EntryValidationService** — Validates entry integrity: amount, journal reference, account reference, and direction

## Invariants (CRITICAL)
- Entry is immutable after creation — no update or delete operations exist
- Must belong to a journal (JournalId must not be empty)
- Must reference an account (AccountId must not be empty)
- Amount must be greater than zero
- Direction must be Debit or Credit

## Policy Dependencies
- Immutability enforcement: no modifications after recording

## Integration Points
- **journal** — Entries belong to journals; every entry references a JournalId

## Lifecycle
```
Record() -> Recorded (immutable, no further transitions)
```

## Notes
- This is a create-only aggregate — no update or delete behaviors exist
- Corrections are handled by creating new compensating entries, never by modifying existing ones
- All error methods are strongly typed via static EntryErrors class
