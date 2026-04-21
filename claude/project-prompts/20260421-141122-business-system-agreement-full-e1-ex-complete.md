# Business-System / Agreement — Full E1→Ex context-wide completion

## CLASSIFICATION

- Classification: business-system
- Context: agreement
- Scope: **All 10 BCs to D2 / Full E1→Ex complete** across all 6 template layers

## CONTEXT

User demanded full 18-section E1→Ex delivery (per `00-section-checklist.md`), not D1 domain-only. Prior pilot (20260421-130805) had the contract BC at E1→Ex with 5 documented blockers. This pass resolves those blockers and extends the pattern to all 9 remaining BCs across the agreement context.

## OBJECTIVE

All 10 agreement BCs verified at **D2 Active** with:
1. Domain (D1 already complete)
2. Engine T2E handlers (32 total commands)
3. Runtime wiring (10 schema modules, all dispatched from DomainSchemaCatalog)
4. Projection/API (10 reducers + 10 handlers + 10 controllers + 10 read-model tables)
5. Composition (10 application modules, all wired in BusinessSystemCompositionRoot)
6. Infrastructure (10 Kafka topic manifests, 10 Rego policies)
7. Verification (e2e fixture authored, e2e test live — pending runtime infra)

## CONSTRAINTS

- Preserve every existing domain method signature except where a missing event was required by the projection (ContractPartyAddedEvent + CreatedAt on ContractCreatedEvent).
- Do NOT redesign domain logic.
- Follow the contract BC pilot pattern line-for-line.
- 3-tuple `DomainRoute(classification, context, domain)` — flat, no domain-group in the route.
- Kafka topic prefix: `whyce.business.agreement.{bc}` (3-tuple).
- Policy id: `whyce.business.agreement.{group}.{bc}.{action}` (5-segment — matches existing policy-binding convention).
- API route: `api/{group}/{bc}/{action}`.

## EXECUTION STEPS

1. PHASE 0 — Discovery: scan `src/domain/business-system/agreement/**` → 10 BCs enumerated.
2. PHASE 1 — Gap matrix: 9 BCs missing engine/runtime/projection/API/infra; contract D2 with 5 blockers.
3. PHASE 2 — Parallel sub-agent repair:
   - Agent 1: contract domain fix (add `ContractPartyAddedEvent` + `CreatedAt`) + author `BusinessE2EFixture` + live e2e test.
   - Agent 2: change-control (amendment, approval, clause, renewal) — 4 BCs.
   - Agent 3: commitment (acceptance, obligation, validity) — 3 BCs.
   - Agent 4: party-governance (counterparty, signature) — 2 BCs.
   - Change-control agent hit rate limit mid-work (after shared-contracts + engines + projections); controllers (3), app modules (4), schema modules (4), topics.json (4), rego (4) finished manually.
4. PHASE 3 — Shared-wiring integration (main conversation):
   - `DomainSchemaCatalog.cs` — added 9 `Register...` dispatch methods.
   - `BusinessSystemCompositionRoot.cs` — wires 10 application modules + 10 schema dispatches + 10 engine registrations.
   - `BusinessPolicyModule.cs` — 32 `CommandPolicyBinding`s.
   - `BusinessProjectionModule.cs` — generic `WireBc<TReadModel, THandler>` helper + 10 invocations + 32 projection registrations.
5. PHASE 4 — Verification: `dotnet build` 0/0; architecture tests 72/72.

## OUTPUT

Matrix by BC covering Domain / Engine / Runtime / Projection-API / Infrastructure / Verification / Final status.

## VALIDATION CRITERIA

- All 10 aggregates inherit `AggregateRoot`.
- All 10 BCs have:
  - 3–5 T2E handlers per BC (one per command)
  - 1 controller with POST-per-command + GET by id
  - 1 projection handler + 1 reducer + 1 read-model
  - 1 schema module with N `RegisterSchema` + N `RegisterPayloadMapper` pairs
  - 1 application module (`Add{Bc}Application` + `RegisterEngines`)
  - 1 Kafka topics.json (4 topics: commands/events/retry/deadletter)
  - 1 Rego policy (default deny + allow rule per policy id + hard-deny for missing inputs)
- Shared wiring:
  - 10 entries in `DomainSchemaCatalog`
  - 10 `services.Add{Bc}Application()` + 10 `{Bc}ApplicationModule.RegisterEngines` in `BusinessSystemCompositionRoot`
  - 10 `WireBc<...>` calls + 32 `projection.Register(...)` in `BusinessProjectionModule`
  - 32 `CommandPolicyBinding` registrations in `BusinessPolicyModule`
- `dotnet build` → 0 warnings, 0 errors.
- Architecture tests → 72/72 pass.

## REMAINING GAPS (explicit + justified)

1. **E2E test execution pending runtime infra.** `BusinessE2EFixture` exists and `ContractE2ETests` has a live happy-path test, but actual execution requires Kafka/Postgres/OPA + `BUSINESS_E2E_*` env vars. Test compiles and is discovered by xUnit. Non-contract BCs do not yet have their own e2e tests — adding them is a straightforward per-BC expansion following the contract shape.
2. **No cross-BC integration flows wired.** No compensation workflows, no OPA detection, no expiry schedulers, no cross-system event bridges (e.g. amendment → contract). These are out of scope per "pure vertical per BC" pilot plus "agreement context only" scope. When a real workflow surfaces, add under `BusinessSystemCompositionRoot.RegisterServices`.
3. **Per-BC read-model shapes are minimal** — status + id + CreatedAt/LastUpdatedAt. Domain-specific fields (e.g. `TargetId` on amendment, `ClauseType` on clause, `Role` on signature) are plumbed where the event schema carries them, but richer query shapes may be needed per product. Extend per-BC as requirements surface.
4. **No unit tests added** for the new handlers/controllers/projections. Architecture tests (72/72) validate layer purity + naming + topology, but command-handler unit tests would be a valuable follow-up.
