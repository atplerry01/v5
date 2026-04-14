# TITLE
PHASE 2 — ECONOMIC-SYSTEM — LEDGER DOMAIN (S4 + E1→EX IMPLEMENTATION)

# CONTEXT
Phase 2 of Whycespace WBSM v3 build. Establish the financial system of record:
append-only ledger, double-entry accounting, deterministic balance computation.

# CLASSIFICATION
- classification: economic-system
- context:        ledger
- domain:         ledger | journal | entry

# LOCKED EXECUTION DECISIONS (from user, 2026-04-14)
1. Extend existing `economic-system/ledger/{ledger, journal, entry}` split (OPTION A).
2. Ledger is the CONTEXT, not a flattened domain.
3. Responsibilities:
   - LedgerAggregate → balance state, versioning, invariants
   - Journal         → owns PostEntries (atomic transaction boundary)
   - Entry           → immutable debit/credit records
4. DO NOT collapse entry into ledger.
5. DO NOT introduce parallel transaction/ledger domain.
6. DO NOT delete or rename existing scaffolding ($5 anti-drift).
7. Namespace alignment: remove all `transaction/ledger` namespace references
   from the original prompt — context is `ledger`, not `transaction`.
8. Event model:
   - EntryPostedEvent    → belongs to journal context
   - LedgerUpdatedEvent  → emitted by ledger aggregate after journal commit
9. E2E scope: code-complete only; runtime validation marked pending.
10. Expense integration: design hook but do not block; assume ExpenseRecordedEvent
    will exist.

# OBJECTIVE
Implement economic-system/ledger context to full S4 production standard and wire
E1 → EX execution for journal posting.

# CONSTRAINTS
## DOMAIN
- NO persistence logic; NO DateTime.UtcNow; NO Guid.NewGuid()
- ONLY deterministic IDs (SHA256 hashing per $9)
- Append-only ledger (no mutation of past entries)
- Balance is DERIVED (fold), never stored as mutable state
- ONLY emit events; NO cross-domain mutation
## ACCOUNTING
- Every posting MUST be double-entry
- Total debit == total credit (STRICT invariant)
- No partial posting; immutable once posted
- Accounts identified deterministically
## ENGINE (T2E)
- Stateless; returns events only; no DB / Kafka / Redis access
## RUNTIME FLOW
Platform API → SystemIntentDispatcher → RuntimeControlPlane → T0U (Policy)
→ T1M (optional) → T2E (Ledger execution) → Domain Aggregate → Domain Events
→ Runtime persists → chain → outbox → kafka

# EXECUTION STEPS
1. Domain entry   (S4): value-objects, entity, event, spec, error, service, README
2. Domain journal (S4): PostEntries aggregate, EntryPostedEvent, specs, errors
3. Domain ledger  (S4): extend aggregate; LedgerUpdatedEvent; balance fold
4. Application E1: PostJournalEntriesCommand
5. Engine EX T2E:  PostJournalEntriesHandler
6. Projection:     LedgerReadModel + LedgerProjectionHandler
7. Kafka topic:    whyce.economic.ledger.events
8. Platform API:   LedgerController, POST /api/ledger/post
9. Expense hook:   event-driven trigger design (no tight coupling)
10. Guard + audit sweep

# OUTPUT FORMAT
1. Full folder tree
2. All code files
3. API test result          (PENDING — runtime not guaranteed)
4. DB verification          (PENDING — runtime not guaranteed)
5. Kafka verification       (PENDING — runtime not guaranteed)
6. Audit result (PASS/FAIL)

# VALIDATION CRITERIA
- Debit == credit invariant enforced
- No mutable ledger state
- No time/guid violations
- Topic naming correct
- Projection isolation respected
- No domain leakage
- All guard files PASS
