# Domain: Transaction

## Classification
economic-system

## Context
transaction

## Purpose
Orchestrates execution of financial operations and produces balanced journals. Transaction MUST NOT create entries directly — it produces a journal, which then produces entries. This is the critical boundary between intent and truth.

## Core Responsibilities
- Executing instructions by initiating transactions
- Validating constraints (limits) before execution
- Generating journal references on completion
- Recording failures with reasons

## Aggregate(s)
- **TransactionAggregate**
  - Event-sourced, sealed. Manages transaction lifecycle from initiation through completion or failure
  - Invariants: Must produce a journal on completion (JournalId required — T1); must reference instruction (InstructionId required — T5); only Initiated can transition; cannot complete a failed transaction; cannot fail a completed transaction

## Entities
None

## Value Objects
- **TransactionId** — Typed Guid wrapper for unique transaction identity
- **TransactionStatus** — Enum: Initiated, Completed, Failed

## Domain Events
- **TransactionInitiatedEvent** — Execution begins (captures TransactionId, InstructionId)
- **TransactionCompletedEvent** — Journal produced, execution finalized (captures JournalId — T1 satisfied)
- **TransactionFailedEvent** — Execution failed with reason

## Specifications
- **CanExecuteSpecification** — Status == Initiated
- **CanCompleteSpecification** — Status == Initiated

## Domain Services
- **TransactionJournalLinkService** — Validates cross-domain invariant T1: transaction is Completed AND JournalId is non-empty

## Invariants (CRITICAL)
- Must produce a journal on completion (JournalId required — T1)
- Must reference an instruction (InstructionId required — T5)
- Cannot execute twice
- Only Initiated transactions can be completed or failed
- Cannot complete a failed transaction
- Cannot fail a completed transaction

## Policy Dependencies
- T1: Every transaction MUST produce exactly one journal
- T3: Limits must be validated before execution (enforced by caller)
- T4: Charges must be applied before completion (enforced by caller)
- T5: Transaction must originate from instruction

## Integration Points
- **instruction** — Transactions reference source instruction via InstructionId
- **journal** (ledger context) — Transactions produce journals; JournalId set on completion
- **limit** — Limits validated before transaction completion
- **charge** — Charges applied before transaction completion

## Lifecycle
```
Initiate() -> Initiated (requires InstructionId)
  Complete() -> Completed (requires JournalId, terminal)
  OR
  Fail() -> Failed (terminal, with reason)
```

## Notes
- CRITICAL: Transaction MUST NOT create entries directly — it must produce a journal
- EnsureInvariants() enforces that Completed status always has non-empty JournalId
- Cross-domain references (InstructionId, JournalId) use raw Guid to avoid coupling
- All error methods are strongly typed via static TransactionErrors class
