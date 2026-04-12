# Domain: Instruction

## Classification
economic-system

## Context
transaction

## Purpose
Defines the intent of an economic action before execution. An instruction represents what should happen — it captures source, destination, amount, and type — while remaining execution-agnostic.

## Core Responsibilities
- Representing economic intent (what should happen)
- Validating basic structure (accounts, amount, type)
- Remaining execution-agnostic — the instruction does not know how it will be fulfilled

## Aggregate(s)
- **TransactionInstructionAggregate**
  - Event-sourced, sealed. Declares intent with full validation at creation
  - Invariants: Amount >= 0 (checked in EnsureInvariants); amount must be > 0 at creation; accounts must be non-empty and different; unidirectional state transitions

## Entities
None

## Value Objects
- **InstructionId** — Typed Guid wrapper for unique instruction identity
- **InstructionType** — Enum: Transfer, Payment, Allocation, Refund
- **InstructionStatus** — Enum: Pending, Executed, Cancelled

## Domain Events
- **TransactionInstructionCreatedEvent** — Intent declared with source, destination, amount, currency, type
- **TransactionInstructionExecutedEvent** — Instruction fulfilled
- **TransactionInstructionCancelledEvent** — Instruction cancelled with reason

## Specifications
- **CanExecuteSpecification** — Status == Pending
- **CanCancelSpecification** — Status == Pending

## Domain Services
- **InstructionValidationService** — Validates instruction structure: Amount > 0, FromAccountId non-empty, ToAccountId non-empty, accounts differ

## Invariants (CRITICAL)
- Amount must be greater than zero
- Source and destination accounts must be valid (non-empty)
- Source and destination accounts must differ (no self-transfer)
- Cannot execute twice
- Cannot execute a cancelled instruction
- Cannot cancel an executed instruction

## Policy Dependencies
- T5: Transaction must originate from instruction

## Integration Points
- **transaction** — Instructions create transactions; InstructionId referenced by TransactionAggregate
- **wallet** — Wallets initiate instructions via TransactionRequestedEvent

## Lifecycle
```
CreateInstruction() -> Pending
  MarkExecuted() -> Executed (terminal)
  OR
  CancelInstruction() -> Cancelled (terminal, with reason)
```

## Notes
- Cross-domain references (FromAccountId, ToAccountId) use raw Guid to avoid coupling
- All error methods are strongly typed via static InstructionErrors class
