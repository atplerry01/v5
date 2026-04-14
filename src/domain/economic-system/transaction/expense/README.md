# Domain: Expense

## Classification
economic-system

## Context
transaction

## Domain
expense

## Purpose
Records and tracks operating expenses tied to a source document. The
aggregate is a pure event-sourced write model: it validates expense-local
invariants (positive amount, mandatory source reference, legal state
transitions) and emits events. Runtime is responsible for persistence,
Kafka publication, projection, and WhyceChain anchoring. Cross-domain
conservation (ledger postings, capital impact, settlement) happens
outside this aggregate via downstream events.

## Owns
- The canonical lifecycle of an individual expense
- Expense-local invariants (amount, source reference, category)
- Deterministic state transitions between Created / Recorded / Cancelled

## Does NOT Own
- Double-entry ledger posting (ledger domain)
- Balance mutation on capital pools (capital domain)
- External settlement / payment execution (settlement domain)
- Persistence, publication, or chain anchoring (runtime)

## Aggregate
- **ExpenseAggregate** — event-sourced, sealed. Emits
  `ExpenseCreatedEvent`, `ExpenseRecordedEvent`, `ExpenseCancelledEvent`.

## Entities
- **ExpenseLine** — per-line allocation within an expense (internal
  factory, Entity base).

## Value Objects
- **ExpenseId** — typed Guid wrapper, `From()` factory, non-empty invariant
- **ExpenseStatus** — enum (Created, Recorded, Cancelled)
- **ExpenseCategory** — strong string wrapper, non-empty invariant
- **ExpenseSourceReference** — strong string wrapper, non-empty invariant
- **ExpenseMetadata** — currency + optional description/memo

## Domain Events
- **ExpenseCreatedEvent** (ExpenseId, Amount, Currency, Category, SourceReference)
- **ExpenseRecordedEvent** (ExpenseId, Amount, Currency)
- **ExpenseCancelledEvent** (ExpenseId, Reason)

## Specifications
- **ExpenseSpecification** — amount > 0 and source reference non-empty
- **ExpenseLifecycleSpecification** — allowed transitions
  (Created -> Recorded, Created -> Cancelled)

## Domain Services
- **ExpenseService.CalculateTotal(expenses)** — sums non-cancelled
  expense amounts across a collection

## Errors
Strongly-typed via `ExpenseErrors` static:
- `InvalidAmount`
- `InvalidStateTransition(from, to)`
- `AlreadyRecorded`
- `MissingSourceReference`
- `NegativeAmount` (invariant)

## Lifecycle

```
Create(...)  -> Created
Record()     -> Recorded   (only from Created)
Cancel(...)  -> Cancelled  (only from Created)
```

## Invariants (CRITICAL)
- Amount must be strictly greater than zero at creation.
- Amount must never be negative (`EnsureInvariants`).
- SourceReference must be non-empty.
- Status transitions restricted to the specification above.
- An already-Recorded expense cannot be re-recorded or cancelled.

## Integration Domains
- **ledger** — consumes `ExpenseRecordedEvent` for double-entry posting
- **capital** — consumes `ExpenseRecordedEvent` for balance impact
- **settlement** — consumes `ExpenseRecordedEvent` for external payment

These integrations happen strictly through events; this aggregate has
zero dependencies on any of them.

## Runtime Flow
```
POST /api/expense/record
  -> ExpenseController
  -> IWorkflowDispatcher (DomainRoute: economic/transaction/expense)
  -> RuntimeControlPlane (T0U policy gate)
  -> T2E RecordExpenseHandler
  -> ExpenseAggregate.Create -> Record
  -> DomainEvents emitted via IEngineContext.EmitEvents
  -> Runtime persists event store row, outbox row, publishes to
     whyce.economic.transaction.expense.events, anchors chain block
  -> ExpenseProjectionHandler reduces into ExpenseReadModel
```

## Determinism
- ExpenseId is derived via `IIdGenerator` at the entry point — no
  `Guid.NewGuid()`.
- No `DateTime.UtcNow` — all temporal stamping is runtime-owned via
  envelope metadata.

## Notes
- The aggregate is deliberately minimal; cross-aggregate mutations are
  intentionally absent and must be orchestrated outside the domain.
