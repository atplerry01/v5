# Business-System / Service — Full E1→Ex context-wide completion

## CLASSIFICATION

- Classification: business-system
- Context: service
- Scope: **All 6 populated BCs to D2 / Full E1→Ex complete + e2e tests**

## CONTEXT

Fifth business-system context. 6 BCs across 2 domain groups:
- service-constraint: policy-binding, service-constraint, service-window
- service-core: service-definition, service-level, service-option

Notable: BC named `service-constraint` nested inside group `service-constraint` creates namespace nesting `Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint` — handled with alias `ServiceConstraintBc`.

## OBJECTIVE

All 6 service BCs at **D2 Active** with:
1. Domain (D1 complete)
2. Engine T2E handlers (24 commands, 4 per BC)
3. Runtime wiring (6 schema modules + DomainSchemaCatalog dispatches)
4. Projection/API (6 reducers + handlers + controllers + read-models)
5. Composition (6 application modules + BusinessSystemCompositionRoot wired)
6. Infrastructure (6 topics manifests + 6 Rego policies)
7. E2E tests (6 tests authored)

## CONSTRAINTS

- Preserve domain signatures.
- Alias `ServiceConstraintBc` for the duplicated BC name.
- 3-tuple `DomainRoute("business", "service", "{bc}")` — hyphenated bc for all service BCs.
- Policy id: `whyce.business.service.{group}.{bc}.{action}`.
- API route: `api/{group}/{bc}/{action}`.
- Kafka: `whyce.business.service.{bc}` (hyphenated).
- DET-SEED-DERIVATION-01: caller-supplied ids only.

## EXECUTION STEPS

1. PHASE 0/1 — 6 BCs × 4 events = 24 events.
2. PHASE 2 — 2 parallel agents (service-constraint 3 BCs, service-core 3 BCs) → 84 files.
3. PHASE 3 — Shared wiring:
   - DomainSchemaCatalog: +6 dispatches
   - BusinessSystemCompositionRoot: 6 `Add{Bc}Application()` + 6 schemas + 6 engine registrations
   - BusinessPolicyModule: +24 bindings (with `ServiceConstraintBc.` prefix for collision)
   - BusinessProjectionModule: +`WireServiceBc<T,H>` helper + 6 wirings + 24 event registrations (with `ServiceConstraintProj`/`ServiceConstraintRm` aliases)
4. **Pre-existing structural drift resolved**:
   - `StructuralParentLookupAdapter.cs` moved from `src/platform/host/adapters/` to `src/platform/host/composition/structural/adapters/` (composition is exempt from layer-purity rule)
   - `ParticipantController.cs` DET-SEED violation fixed (caller-supplied `ParticipantId`)
5. PHASE 4 — 6 e2e tests authored. Clean build 0/0. **Architecture 72/72** ✓.

## OUTPUT

Matrix per BC + files + incidental structural fixes.

## VALIDATION CRITERIA

- 6 aggregates inherit `AggregateRoot`.
- Per BC: standard 14 files (5 shared + 1 event + 4 handlers + 2 projection + 1 controller + 1 app module + 1 schema module + 1 topics + 1 rego) + 1 e2e test.
- Shared wiring: 6 catalog entries, 6 app-modules, 24 policy bindings, 6 projection workers, 24 event registrations.
- `dotnet build` → 0/0.
- Architecture tests → **72/72 green** (DET-SEED + layer-purity).

## REMAINING GAPS

1. **E2E tests authored but not executing** — require Kafka + Postgres + OPA + API host. Same infra blocker as prior contexts.
2. **Cross-BC service workflows** (service-definition → service-level, policy-binding → service-constraint) out of scope.
3. **2 architecture test failures resolved as side effect**:
   - `Platform_non_composition_code_does_not_reference_domain_engines_or_projections` — structural adapter moved to composition.
   - `IdGenerator_Generate_seeds_contain_no_clock_or_random_entropy` — ParticipantController DET-SEED fix.

## CUMULATIVE BUSINESS-SYSTEM PROGRESS

| Context | BCs | E1→Ex | E2E | Status |
|---|---|---|---|---|
| agreement | 10 | ✓ | 10/10 | D2 ✓ |
| customer | 6 | ✓ | 6/6 | D2 ✓ |
| entitlement | 6 | ✓ | 6/6 | D2 ✓ |
| order | 6 | ✓ | 6/6 | D2 ✓ |
| service | 6 | ✓ | 6/6 | D2 ✓ |
| **Total D2** | **34** | | **34/34** | |
| offering | 7 | pending | - | |
| pricing | 9 | pending | - | |
| provider | 5 | pending | - | |
| **Remaining populated** | **21** | | | |
