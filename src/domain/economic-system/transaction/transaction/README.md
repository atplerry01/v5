# Domain: Transaction

## Classification
economic-system

## Context
transaction

## Domain
transaction

## Purpose
Transaction is the **orchestration envelope** for economic actions. It
binds one or more action references (expense, revenue, and future
action types) under a single deterministic commit/abort lifecycle. The
aggregate carries references ONLY — no ledger, capital, or per-action
business logic is embedded here. Downstream ledger and capital domains
subscribe to `TransactionCommittedEvent` to perform their own postings.

## Owns
- The canonical commit/abort lifecycle of an economic transaction
- The list of `TransactionReference`s carried by a transaction
- Replay-safe state transitions driven by domain events

## Does NOT Own
- Expense, revenue, or any individual action's business logic
- Ledger journal entries (`ledger` domain subscribes to `TransactionCommittedEvent`)
- Capital balance mutations (`capital` domain subscribes to `TransactionCommittedEvent`)
- Persistence, publication, chain anchoring (runtime)

## Aggregate
- **TransactionAggregate** — event-sourced, sealed. Emits
  `TransactionInitiatedEvent`, `TransactionCommittedEvent`,
  `TransactionFailedEvent`. Fully reconstructible from its event
  history via `LoadFromHistory` + `Apply` (replay-safe).

## Entities
None.

## Value Objects
- **TransactionId** — typed Guid wrapper, non-empty invariant
- **TransactionStatus** — enum (Initiated, Committed, Failed)
- **TransactionReference** — typed link (`Kind` + `Guid Id`) to a wrapped
  economic action
- **TransactionKind** — canonical string constants for common kinds
  (expense, revenue, distribution, payout, charge)

## Domain Events
- **TransactionInitiatedEvent** (TransactionId, Kind, References, InitiatedAt)
- **TransactionCommittedEvent** (TransactionId, Kind, References, CommittedAt)
- **TransactionFailedEvent** (TransactionId, Reason, FailedAt)

All events carry timestamps supplied by the runtime clock — no
`DateTime.UtcNow` inside the aggregate.

## Specifications
- **CanCommitSpecification** — `Status == Initiated`
- **CanFailSpecification** — `Status == Initiated`

## Domain Services
- **TransactionOrchestrationService** — enumerates / queries references
  by kind for downstream consumers. Stateless; zero cross-domain deps.

## Errors
Strongly-typed via `TransactionErrors`:
- `MissingKind`
- `MissingReferences`
- `InvalidReferenceKind`
- `TransactionNotInitiated`
- `TransactionAlreadyCommitted`
- `TransactionAlreadyFailed`
- `CannotCommitFailedTransaction`
- `CannotFailCommittedTransaction`

## Lifecycle

```
Initiate(id, kind, references, t0)  -> Initiated
  Commit(t1)                        -> Committed   (terminal)
  Fail(reason, t1)                  -> Failed      (terminal)
```

Both terminal states are absorbing — once Committed, cannot Fail, and
vice versa.

## Invariants (CRITICAL)
- Kind must be non-empty on initiation.
- At least one TransactionReference must be supplied on initiation.
- Only `Initiated` transactions may Commit or Fail.
- Committed and Failed are terminal.
- All state mutation routed through `Apply(event)` — no out-of-band
  state changes (replay-safe).

## Integration Points
- **expense** — `ExpenseRecordingIntent` routes through a transaction
  envelope; controller chains Initiate → RecordExpense → Commit (or
  Fail on error). References carry `{kind: "expense", id: ExpenseId}`.
- **revenue** / **distribution** / **payout** / **charge** — same
  orchestration pattern; transaction acts as the commit/abort boundary.
- **ledger** (downstream) — subscribes to `TransactionCommittedEvent`
  to post journals. No reverse import.
- **capital** (downstream) — subscribes to `TransactionCommittedEvent`
  to mutate balances. No reverse import.

## Runtime Flow (expense example)

```
POST /api/transaction/record-expense
  -> TransactionController
     1. IIdGenerator.Generate(expense-seed)        => expenseId (deterministic)
     2. IIdGenerator.Generate(txn-seed)            => transactionId (deterministic)
     3. ISystemIntentDispatcher
          .DispatchAsync(InitiateTransactionCommand, txn-route)
     4. ISystemIntentDispatcher
          .DispatchAsync(RecordExpenseCommand, expense-route)
     5a. Success: DispatchAsync(CommitTransactionCommand, txn-route)
     5b. Failure: DispatchAsync(FailTransactionCommand,   txn-route)
  -> runtime persists events, publishes to
     whyce.economic.transaction.transaction.events, anchors chain
  -> ledger / capital consumers pick up TransactionCommittedEvent
```

## Determinism
- TransactionId generated via `IIdGenerator` with a stable seed at the
  controller seam — no `Guid.NewGuid()`.
- Timestamps supplied by `IClock` at the controller seam, carried into
  each command — domain sees only `Timestamp` value objects.
- No DateTime.UtcNow, no Random, no non-deterministic state.

## Notes
- References are frozen at initiation time; they cannot be mutated
  post-initiation. This preserves replay determinism.
- The aggregate intentionally has no JournalId / LedgerId coupling;
  those concerns live in the `ledger` and `capital` domains and
  subscribe to `TransactionCommittedEvent`.
