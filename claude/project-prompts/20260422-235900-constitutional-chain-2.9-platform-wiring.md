## TITLE

Phase 2.9 Continuation — Constitutional Chain Platform Wiring

## CONTEXT

Classification: constitutional-system
Context: chain
Domains: anchor-record, evidence-record, ledger
Phase: 2.9 (continuation)

Domain D2 layer was complete (aggregates, events, value objects, 17 unit tests). This prompt covers the full platform wiring to complete the E1→EX vertical slice.

## OBJECTIVE

Wire constitutional-system chain BCs into the full platform stack: schema modules, projection handlers, application modules, composition root, API controller, and BootstrapModuleCatalog registration.

## CONSTRAINTS

- Follow existing wiring patterns (TrustProjectionModule, SessionProjectionHandler, AssetApplicationModule)
- No new patterns introduced
- Domain must remain untouched
- Host builds 0 errors 0 warnings

## EXECUTION STEPS

### Schema Modules (runtime/event-fabric/domain-schemas/)
- ConstitutionalChainAnchorRecordSchemaModule — RegisterSchema + RegisterPayloadMapper for Created/Sealed
- ConstitutionalChainEvidenceRecordSchemaModule — RegisterSchema + RegisterPayloadMapper for Created/Archived
- ConstitutionalChainLedgerSchemaModule — RegisterSchema + RegisterPayloadMapper for Opened/Sealed
- DomainSchemaCatalog — 3 new Register* static methods

### Projection Layer (projections/constitutional/chain/)
- AnchorRecordProjectionHandler — IEnvelopeProjectionHandler + 2 typed IProjectionHandler<T>
- EvidenceRecordProjectionHandler — IEnvelopeProjectionHandler + 2 typed IProjectionHandler<T>
- LedgerProjectionReducer — static reducer for Opened/Sealed
- LedgerProjectionHandler — IEnvelopeProjectionHandler + 2 typed IProjectionHandler<T>

### Application Module (platform/host/composition/constitutional/chain/application/)
- ChainApplicationModule — AddChainApplication + RegisterEngines for all 6 handlers

### Projection Module (platform/host/composition/constitutional/chain/projection/)
- ChainProjectionModule — AddChainProjection (3 stores + 3 Kafka workers) + RegisterProjections

### Composition Root
- ConstitutionalChainBootstrap — IDomainBootstrapModule wiring all above

### API Controller (platform/api/controllers/constitutional/chain/)
- ChainController — POST record/seal + GET for anchor-record, POST record/archive + GET for evidence-record, POST open/seal + GET for ledger
- Request model records inline

### BootstrapModuleCatalog
- Add ConstitutionalChainBootstrap entry

## OUTPUT FORMAT

- Files created (count)
- Build status
- Test results

## VALIDATION CRITERIA

- All 5 layers build 0 errors 0 warnings
- All 17 Phase 2.9 D2 tests pass
- No new regressions
