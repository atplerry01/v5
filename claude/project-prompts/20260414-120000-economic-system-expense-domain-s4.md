# PHASE 2 — ECONOMIC-SYSTEM — EXPENSE DOMAIN (S4 + E1→EX IMPLEMENTATION)

## TITLE
Phase 2 — Economic-System — Expense Domain S4 Implementation and E1→EX Wiring

## CONTEXT
Implements the expense domain end-to-end under the canonical binding
`economic-system:transaction:expense`. Follows WBSM v3 layer purity:
domain owns truth, runtime persists/anchors, engines emit events only,
platform exposes a POST endpoint that dispatches via workflow runtime.

Two architectural adjustments applied per CLAUDE.md priority ($15):
- Domain path: `src/domain/economic-system/transaction/expense/`
  (three-level nesting enforced by $7 and matching all sibling domains).
- Command storage: `src/shared/contracts/economic/transaction/expense/`
  (no `src/application/` exists in the repo; creating a new top-level
  folder would violate $5 anti-drift. Placement follows the existing
  `DistributionCommands.cs` precedent).

## CLASSIFICATION
Classification: economic-system
Context:        transaction
Domain:         expense

## OBJECTIVE
Deliver a full S4 expense domain with aggregate, entities, value objects,
events, specifications, errors, service, and README; wire an application
command (`RecordExpenseCommand`), a T2E engine (`RecordExpenseHandler`),
a read-model projection, Kafka topics, and an API endpoint.

## CONSTRAINTS
- Domain: zero external dependencies; deterministic IDs only; no
  Guid.NewGuid; no DateTime.UtcNow; emits events only; no persistence.
- Engine (T2E): stateless; emits events via `IEngineContext.EmitEvents`;
  no runtime, no infra, no persistence.
- Projection: idempotent upsert via `last_event_id`; depends ONLY on
  shared contracts; never imports domain.
- Kafka topic naming: `whyce.{classification}.{context}.{domain}.{type}`
  => `whyce.economic.transaction.expense.{commands|events|retry|deadletter}`.
- Event naming in source: `{Domain}{Action}Event` → `ExpenseCreatedEvent`,
  `ExpenseRecordedEvent`, `ExpenseCancelledEvent`.

## EXECUTION STEPS
1. Create S4 domain tree.
2. Register shared contracts (command + read model + event schemas).
3. Implement T2E `RecordExpenseHandler`.
4. Validate envelope compatibility (event-id, aggregate-id,
   correlation-id — inherited from runtime envelope, schema just carries
   aggregate + payload fields).
5. Add `ExpenseProjectionHandler` with idempotent reducer.
6. Register Kafka topics in `infrastructure/event-fabric/kafka/create-topics.sh`.
7. Add `ExpenseController` POST /api/expense/record.
8. Build-verify; capture E2E readiness notes (runtime wiring required
   before the full E2E flow is exercisable).
9. Run guard + audit sweep.

## OUTPUT FORMAT
Full file tree, new files, modified files, audit summary, E2E readiness.

## VALIDATION CRITERIA
- Three-level nesting respected.
- No Guid.NewGuid / DateTime.UtcNow in domain.
- No persistence or external calls in engine.
- Projection free of domain imports.
- Topic names canonical.
- Event records follow `{Domain}{Action}Event`.
- README present and complete.
