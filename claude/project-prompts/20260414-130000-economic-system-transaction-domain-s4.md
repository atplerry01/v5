# PHASE 2 — ECONOMIC-SYSTEM — TRANSACTION DOMAIN (S4 ORCHESTRATION)

## TITLE
Transaction Domain — Orchestration Envelope for Economic Actions

## CONTEXT
Implements the `economic-system/transaction/transaction` orchestration
aggregate. Transaction is the commit/abort envelope around one or more
economic actions (expense, revenue, future). It does NOT embed any
action's logic — it holds only references and a canonical lifecycle
(Initiated → Committed → Failed). Downstream ledger and capital domains
subscribe to `TransactionCommittedEvent` to perform their posting.

## CLASSIFICATION
Classification: economic-system
Context:        transaction
Domain:         transaction

## OBJECTIVE
- Deliver S4 Transaction domain (aggregate, VOs, events, specs, errors,
  service, README).
- Wire T2E engines (Initiate, Commit, Fail) under single-aggregate rule.
- Wire API endpoints + projection + Kafka topics.
- Route expense recording through Transaction orchestration without
  duplicating expense logic (chained dispatch at controller seam).

## CONSTRAINTS
- Aggregate holds references (Kind + Guid) only — no ledger/capital/
  expense logic.
- Lifecycle: Initiated → Committed; Initiated → Failed (terminal both).
- Fully deterministic; no `Guid.NewGuid`; no `DateTime.UtcNow` — all
  timestamps are supplied by the runtime clock through engine context.
- Replay-safe: every state transition routed through Apply() from an
  emitted event; aggregate reconstructible from event history.
- ENG-DOMAIN-ALIGN-01: one engine = one aggregate; orchestration across
  aggregates happens at the workflow/controller seam.

## EXECUTION STEPS
1. Rework TransactionAggregate for Initiated → Committed → Failed;
   accept references list; drop ledger coupling.
2. Rename event `TransactionCompletedEvent` → `TransactionCommittedEvent`;
   add Kind + References to Initiated/Committed.
3. Replace `TransactionJournalLinkService` with generic
   `TransactionOrchestrationService`.
4. Add shared contracts (commands + read model + event schemas).
5. Add T2E handlers: Initiate, Commit, Fail.
6. Add projection + reducer.
7. Add Kafka topics `whyce.economic.transaction.transaction.{commands,events,retry,deadletter}`.
8. Add `TransactionController` — Initiate/Commit/Fail endpoints, plus
   orchestrated `record-expense` endpoint that chains Initiate → Record
   → Commit (or Fail).
9. Update README; run guard + audit sweep.

## OUTPUT FORMAT
File tree, new/modified files, audit result.

## VALIDATION CRITERIA
- No ledger/capital/expense imports in transaction domain.
- No Guid.NewGuid / DateTime.UtcNow.
- Aggregate replay-safe (every state change via Apply on event).
- Topic names canonical.
- Single aggregate per engine.
- README updated to new lifecycle.
