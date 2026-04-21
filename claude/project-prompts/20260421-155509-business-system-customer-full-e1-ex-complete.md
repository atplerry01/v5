# Business-System / Customer — Full E1→Ex context-wide completion

## CLASSIFICATION

- Classification: business-system
- Context: customer
- Scope: **All 6 populated BCs to D2 / Full E1→Ex complete** across all 6 template layers

## CONTEXT

Next context after `agreement` (20260421-141122). 6 BCs across 2 domain groups:
- identity-and-profile: account, customer, profile
- segmentation-and-lifecycle: contact-point, lifecycle, segment

All 6 BCs at D1 pre-execution (domain complete; zero engine/runtime/projection/API/infra).

## OBJECTIVE

All 6 customer BCs verified at **D2 Active** with:
1. Domain (D1 already complete — no modifications needed)
2. Engine T2E handlers (28 commands total)
3. Runtime wiring (6 schema modules dispatched from DomainSchemaCatalog)
4. Projection/API (6 reducers + 6 handlers + 6 controllers + 6 read-model tables)
5. Composition (6 application modules wired in BusinessSystemCompositionRoot)
6. Infrastructure (6 Kafka topic manifests, 6 Rego policies)

## CONSTRAINTS

- Preserve every existing domain method signature.
- Follow the contract BC pilot pattern line-for-line.
- 3-tuple `DomainRoute(classification, context, domain)` — flat, drops the domain-group layer.
- Kafka topic prefix: `whyce.business.customer.{bc}` (hyphenated `bc` when source folder is hyphenated — e.g. `contact-point`).
- Policy id: full 5-segment `whyce.business.customer.{group}.{bc}.{action}`.
- API route: `api/{group}/{bc}/{action}`.

## EXECUTION STEPS

1. PHASE 0 — Discovery: 6 BCs × 3–6 events each (total 28 events).
2. PHASE 1 — Gap: all 6 BCs missing engine/runtime/projection/API/infra.
3. PHASE 2 — 2 parallel sub-agents (identity-and-profile: 3 BCs / segmentation-and-lifecycle: 3 BCs). Each sub-agent authored ~40 files.
4. PHASE 3 — Shared-wiring integration (main conversation):
   - `DomainSchemaCatalog.cs` — +6 `Register...` dispatches.
   - `BusinessSystemCompositionRoot.cs` — 6 `Add{Bc}Application()` + 6 schemas + 6 engine registrations.
   - `BusinessPolicyModule.cs` — +28 `CommandPolicyBinding`s.
   - `BusinessProjectionModule.cs` — introduced `WireCustomerBc<T,H>` helper + factored common worker-wiring into `WireWorker<T,H>`; 6 `WireCustomerBc` invocations + 28 `projection.Register(...)` calls.
5. PHASE 4 — Verification: clean build 0/0; architecture 71/72 (1 pre-existing content-system/streaming drift unrelated to this pass).

## OUTPUT

Matrix by BC covering Domain / Engine / Runtime / Projection-API / Infrastructure / Verification / Final status.

## VALIDATION CRITERIA

- All 6 aggregates inherit `AggregateRoot` (domain D1 — already verified).
- Per BC: commands + policy ids + queries + read model + event schemas + T2E handlers + projection handler + reducer + controller + application module + schema module + topics.json + rego.
- Shared wiring: 6 catalog entries, 6 app-module wirings, 28 policy bindings, 6 projection workers, 28 event registrations.
- `dotnet build` → 0 warnings, 0 errors (clean rebuild).
- Architecture tests → 71/72 passed (1 failure pre-existing in content-system/streaming, not in customer/agreement scope).

## REMAINING GAPS (explicit + justified)

1. **E2E tests not authored** for the 6 new BCs. `BusinessE2EFixture` exists; per-BC e2e is straightforward follow-up.
2. **Pre-existing DET-SEED-DERIVATION-01 violations** in content-system/streaming controllers (8 files) — out of scope for customer integration; should be captured as separate drift and fixed in a dedicated content-system hardening pass.
3. **`CustomerRef` naming collision** — per-BC local `CustomerRef` value-objects coexist with `Whycespace.Domain.BusinessSystem.Shared.Reference.CustomerRef`. Local type wins via same-namespace binding; vestigial `using Shared.Reference` in some aggregates does not cause ambiguity after `Shared.Reference` import removal in two pre-existing handler files (`StartLifecycleHandler`, `CreateContactPointHandler`).
4. **No cross-BC workflows** (e.g. lifecycle→contact-point). Out of scope for per-BC vertical pilots.
5. **Read-model shapes are minimal** — id + status + CreatedAt/LastUpdatedAt + domain-specific fields present in the event schema. Richer query shapes follow product requirements.
