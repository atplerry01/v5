# PHASE 2 — LEDGER → CAPITAL INTEGRATION (EVENT-DRIVEN ADAPTER)

## TITLE
Ledger → Capital Event-Driven Integration Adapter

## CONTEXT
The economic model is: ledger owns financial truth (double-entry
journals); capital owns per-account balances. Capital state must
reflect every posted ledger entry without any direct ledger→capital
coupling. The adapter lives in the runtime event-fabric layer and
bridges the two domains via event subscription + command dispatch.

## CLASSIFICATION
Classification: economic-system
Context:        ledger | capital
Domain:         ledger.ledger ↔ capital.account

## OBJECTIVE
- Subscribe to ledger events.
- Map each `JournalEntryRecordedEventSchema` to a capital account
  mutation and dispatch the corresponding command via
  `ISystemIntentDispatcher`.
- Observe `LedgerUpdatedEventSchema` as the reconciliation checkpoint.

## CONSTRAINTS
- No direct import from `ledger` → `capital` (or vice versa).
- Adapter depends ONLY on shared contracts (event schemas + commands).
- Deterministic + replay-safe (EntryId is the idempotency key; capital
  aggregate version gating protects against double-apply).
- Handler does not persist, publish, or anchor — only dispatches.

## EXECUTION STEPS
1. Create `LedgerToCapitalIntegrationHandler` in
   `src/runtime/event-fabric/`.
2. Switch on `envelope.EventType`:
   - `JournalEntryRecordedEvent` → dispatch capital account command
     (Debit → Fund, Credit → Allocate).
   - `LedgerUpdatedEvent` → no-op observation (correlation checkpoint).
3. Handler mirrors the `WorkflowTriggerHandler` shape; wiring to the
   Kafka consumer pipeline is out of scope per existing pattern.

## OUTPUT FORMAT
File tree, adapter code, audit result.

## VALIDATION CRITERIA
- Handler compiles in runtime project.
- No domain imports.
- Debit/Credit mapping preserved (Debit = Fund, Credit = Allocate).
- Replay-safe via EntryId-based idempotency and capital version gating.
