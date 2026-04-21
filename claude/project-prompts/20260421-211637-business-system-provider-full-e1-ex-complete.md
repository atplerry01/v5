# Business-System / Provider — Full E1→Ex context-wide completion

## CLASSIFICATION

- Classification: business-system
- Context: provider
- Scope: **All 5 populated BCs to D2 / Full E1→Ex complete + e2e tests**

## CONTEXT

Sixth business-system context. 5 BCs across 3 domain groups:
- provider-core: provider-capability, provider-tier
- provider-governance: provider-agreement
- provider-scope: provider-availability, provider-coverage

Notable: `provider-coverage` has 5 events (lifecycle + CoverageScope Added/Removed) requiring flattened scope-set management in the projection reducer — mirrors `contract/ContractPartyAddedEvent` pattern.

## OBJECTIVE

All 5 provider BCs at **D2 Active** with:
1. Domain (D1 complete)
2. Engine T2E handlers (21 commands: 4+4+4+4+5)
3. Runtime wiring (5 schema modules + dispatches)
4. Projection/API (5 reducers + handlers + controllers + read-models — `ProviderCoverageReadModel` includes a `Scopes` list)
5. Composition (5 app modules + BusinessSystemCompositionRoot)
6. Infrastructure (5 topics + 5 rego)
7. E2E tests (5 authored — ProviderCoverage verifies both `Status == "Active"` and `Scopes` collection contents)

## CONSTRAINTS

- Preserve domain signatures.
- 3-tuple `DomainRoute("business", "provider", "{bc}")` — hyphenated bc.
- Policy id: `whyce.business.provider.{group}.{bc}.{action}`.
- API route: `api/{group}/{bc}/{action}`.
- Kafka: `whyce.business.provider.{bc}` (hyphenated).
- DET-SEED-DERIVATION-01: caller-supplied ids only.
- `CoverageScope` VO `(CoverageScopeKind Kind, string Descriptor)` flattened to `(string ScopeKind, string ScopeDescriptor)` on the wire.

## EXECUTION STEPS

1. PHASE 0/1 — 5 BCs, 21 events total.
2. PHASE 2 — 2 parallel agents (provider-core+governance 3 BCs, provider-scope 2 BCs) → 70 files.
3. PHASE 3 — Shared wiring:
   - DomainSchemaCatalog: +5 dispatches
   - BusinessSystemCompositionRoot: 5 `Add{Bc}Application()` + 5 schemas + 5 engine registrations
   - BusinessPolicyModule: +21 bindings
   - BusinessProjectionModule: +`WireProviderBc<T,H>` helper + 5 wirings + 21 event registrations (includes `CoverageScopeAddedEvent`/`CoverageScopeRemovedEvent`)
4. PHASE 4 — 5 e2e tests authored. Clean build 0/0. Architecture **72/72** ✓.

## OUTPUT

Matrix per BC + incidental structural fixes (none this pass — structural drift already resolved).

## VALIDATION CRITERIA

- 5 aggregates inherit `AggregateRoot`.
- Per BC: standard 14 files + 1 e2e test.
- Shared wiring: 5 catalog entries, 5 app-modules, 21 policy bindings, 5 projection workers, 21 event registrations.
- `dotnet build` → 0/0.
- Architecture tests → 72/72 green.

## REMAINING GAPS

1. **E2E tests compile but don't execute** — require Kafka + Postgres + OPA + API host + `BUSINESS_E2E_*` env stack. Same blocker for all prior contexts.
2. **Cross-BC provider workflows** (capability → availability → coverage binding) out of scope.
3. **Provider-agreement time-window** is a rich VO; the test only exercises non-null start/end happy path. Edge cases deferred.

## CUMULATIVE BUSINESS-SYSTEM PROGRESS

| Context | BCs | E1→Ex | E2E | Status |
|---|---|---|---|---|
| agreement | 10 | ✓ | 10/10 | D2 ✓ |
| customer | 6 | ✓ | 6/6 | D2 ✓ |
| entitlement | 6 | ✓ | 6/6 | D2 ✓ |
| order | 6 | ✓ | 6/6 | D2 ✓ |
| service | 6 | ✓ | 6/6 | D2 ✓ |
| provider | 5 | ✓ | 5/5 | D2 ✓ |
| **Total D2** | **39** | | **39/39** | |
| offering | 7 | pending | - | |
| pricing | 9 | pending | - | |
| **Remaining populated** | **16** | | | |
