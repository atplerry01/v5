## TITLE

Phase 2.9 — Constitutional Chain D2 Delivery

## CONTEXT

Classification: constitutional-system
Context: chain
Domains: anchor-record (new), evidence-record (new), ledger (upgrade stub → D2)
Phase: 2.9

WhyceChain engine (T0U) and runtime ChainAnchorService were already implemented. The domain was a stub (ledger BC with empty aggregate). Phase 2.9 adds the full chain domain D2 slice.

## OBJECTIVE

Implement D2 vertical slice for constitutional-system/chain: 3 BCs, 6 engine handlers, 2 projection reducers, 17 unit tests.

## CONSTRAINTS

- Do not redesign WhyceChain engine (T0U) — it is complete
- Do not touch ChainAnchorService — it is production-ready
- Domain must compile clean (0 errors, 0 warnings)
- Tests must pass without regressions

## EXECUTION STEPS

### Shared Contracts
- `constitutional/chain/anchor-record/` — RecordAnchorCommand, SealAnchorCommand, AnchorRecordPolicyIds, AnchorRecordReadModel
- `constitutional/chain/evidence-record/` — RecordEvidenceCommand, ArchiveEvidenceCommand, EvidenceRecordPolicyIds, EvidenceRecordReadModel
- `constitutional/chain/ledger/` — OpenLedgerCommand, SealLedgerCommand, LedgerPolicyIds, LedgerReadModel
- `events/constitutional/chain/` — AnchorRecordEventSchemas, EvidenceRecordEventSchemas, LedgerEventSchemas

### Domain Layer
- `anchor-record` BC: AnchorRecordId, AnchorDescriptor, AnchorRecordStatus, AnchorRecordCreatedEvent, AnchorRecordSealedEvent, AnchorRecordErrors, AnchorNotSealedSpecification, AnchorRecordAggregate (Record + Seal)
- `evidence-record` BC: EvidenceRecordId, EvidenceDescriptor, EvidenceRecordStatus, EvidenceType, EvidenceRecordCreatedEvent, EvidenceRecordArchivedEvent, EvidenceRecordErrors, EvidenceNotArchivedSpecification, EvidenceRecordAggregate (Record + Archive)
- `ledger` BC: LedgerDescriptor, LedgerStatus, LedgerOpenedEvent, LedgerSealedEvent, LedgerErrors, LedgerNotSealedSpecification, LedgerAggregate (Open + Seal) — replaces stub

### Engine Handlers (T2E/constitutional/chain/)
- RecordAnchorHandler, SealAnchorHandler, RecordEvidenceHandler, ArchiveEvidenceHandler, OpenLedgerHandler, SealLedgerHandler

### Projections
- AnchorRecordProjectionReducer, EvidenceRecordProjectionReducer

### Tests
- AnchorRecordAggregateTests (6 tests)
- EvidenceRecordAggregateTests (6 tests)
- LedgerAggregateTests (5 tests)

## OUTPUT FORMAT

- Files created (count)
- Build status
- Test results

## VALIDATION CRITERIA

- Domain builds 0 errors 0 warnings
- Engines build 0 errors 0 warnings
- Projections build 0 errors 0 warnings
- Host builds clean
- All 17 new tests pass
- No regressions in existing test suite
