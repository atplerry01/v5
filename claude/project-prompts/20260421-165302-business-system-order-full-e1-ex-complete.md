# Business-System / Order — Full E1→Ex context-wide completion

## CLASSIFICATION

- Classification: business-system
- Context: order
- Scope: **All 6 populated BCs to D2 / Full E1→Ex complete**

## CONTEXT

Fourth business-system context completed today. 6 BCs across 2 domain groups:
- order-change: amendment, cancellation, fulfillment-instruction
- order-core: line-item, order, reservation

Notable: `Amendment` BC name COLLIDES with `agreement/change-control/amendment`. Required type-aliasing in `BusinessSystemCompositionRoot`, `BusinessPolicyModule`, and `BusinessProjectionModule` to disambiguate.

## OBJECTIVE

All 6 order BCs at **D2 Active** with:
1. Domain (D1 already complete)
2. Engine T2E handlers (23 commands: 5+3+4+3+4+4)
3. Runtime wiring (6 schema modules + DomainSchemaCatalog dispatches)
4. Projection/API (6 reducers + 6 handlers + 6 controllers + 6 read-model tables)
5. Composition (6 application modules + BusinessSystemCompositionRoot wired with Amendment alias)
6. Infrastructure (6 Kafka topic manifests + 6 Rego policies)

## CONSTRAINTS

- Preserve every existing domain method signature.
- Order's Amendment uses `Request/Accept/Apply/Reject/Cancel` verbs (vs agreement's `Create/Apply/Revert`). Both have `ApplyAmendmentCommand` — distinct types in distinct namespaces.
- Order's Amendment app module renamed extension method to `AddOrderChangeAmendmentApplication()` to avoid clashing with agreement's `AddAmendmentApplication()`.
- Type aliases used in shared-wiring files: `OrderAmendment`, `AgreementAmendmentApp`, `OrderAmendmentApp`, `OrderAmendmentProj`, `OrderAmendmentRm`, `OrderCoreOrderProj`, `OrderCoreOrderRm`.

## EXECUTION STEPS

1. PHASE 0/1 — Discovery + gap: 6 BCs, 23 events.
2. PHASE 2 — 2 parallel sub-agents:
   - order-change: Wave A authored 38 files for amendment + cancellation + most of fulfillment-instruction; agent stalled mid-fulfillment-instruction (rate-limit).
   - order-core: Wave B authored 39 files for line-item + order + reservation, fully clean.
3. **Manual completion**: 5 missing fulfillment-instruction files authored (controller, app module, schema module, topics.json, rego).
4. PHASE 3 — Shared-wiring integration:
   - DomainSchemaCatalog: +6 dispatches.
   - BusinessSystemCompositionRoot: 6 application modules + 6 schemas + 6 engine registrations (with Amendment aliases).
   - BusinessPolicyModule: +23 `CommandPolicyBinding`s (using `OrderAmendment.` prefix for order's commands).
   - BusinessProjectionModule: +`WireOrderBc<T,H>` helper + 6 wirings + 23 event registrations (with Amendment + Order aliases on read models and projection handlers).
5. PHASE 4 — Clean rebuild 0/0; 71/72 architecture tests (1 pre-existing content-system/streaming failure).

## OUTPUT

Matrix per BC, files created/modified, no domain fixes, remaining gaps.

## VALIDATION CRITERIA

- 6 aggregates inherit `AggregateRoot`.
- Per BC: 5 shared-contract files + 1 event-schema + N T2E handlers + 2 projection files + 1 controller + 1 application module + 1 schema module + 1 topics.json + 1 rego.
- Shared wiring: 6 catalog entries, 6 app-modules, 23 policy bindings, 6 projection workers, 23 event registrations.
- `dotnet build` → 0 warnings, 0 errors.
- Architecture tests → 71/72 (1 pre-existing).

## REMAINING GAPS

1. **E2E tests not authored** for the 6 order BCs.
2. **Pre-existing content-system/streaming DET-SEED failure** unchanged.
3. **No cross-BC workflows** (e.g. cancellation → reservation release, line-item → order completion).
4. **Order-change/Amendment aliases** add cognitive load to shared-wiring files. Acceptable per pilot scope; could be eliminated by renaming the order Amendment BC (out of scope).
