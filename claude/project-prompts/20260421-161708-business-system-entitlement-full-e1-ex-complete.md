# Business-System / Entitlement — Full E1→Ex context-wide completion

## CLASSIFICATION

- Classification: business-system
- Context: entitlement
- Scope: **All 6 populated BCs to D2 / Full E1→Ex complete**

## CONTEXT

Third business-system context completed today (after agreement 141122 and customer 155509). 6 BCs across 2 domain groups:
- eligibility-and-grant: assignment, eligibility, grant
- usage-control: allocation, limit, usage-right (has 1 entity `UsageRecord`)

All 6 BCs at D1 pre-execution; domain already aligned.

## OBJECTIVE

All 6 entitlement BCs verified at **D2 Active** across:
1. Domain (D1 already complete)
2. Engine T2E handlers (19 commands: 3+3+4+3+3+3)
3. Runtime wiring (6 schema modules + DomainSchemaCatalog dispatches)
4. Projection/API (6 reducers + 6 handlers + 6 controllers + 6 read-model tables)
5. Composition (6 application modules + BusinessSystemCompositionRoot wired)
6. Infrastructure (6 Kafka topic manifests + 6 Rego policies)

## CONSTRAINTS

- Preserve every existing domain method signature.
- Follow the contract BC pilot pattern line-for-line.
- 3-tuple `DomainRoute("business", "entitlement", "{bc}")` — hyphenated bc for `usage-right`.
- Kafka topic prefix: `whyce.business.entitlement.{bc}` (hyphenated where source is hyphenated).
- Policy id: full 5-segment `whyce.business.entitlement.{group}.{bc}.{action}`.
- API route: `api/{group}/{bc}/{action}`.
- DET-SEED-DERIVATION-01: controllers use caller-supplied ids; no clock/random in seed derivation.

## EXECUTION STEPS

1. PHASE 0/1 — Discovery + gap: 6 BCs × 3–4 events each.
2. PHASE 2 — 2 parallel agents × 3 BCs:
   - eligibility-and-grant (assignment/eligibility/grant) — 40 files
   - usage-control (allocation/limit/usage-right) — 42 files (usage-right flattens UsageRecord entity)
3. PHASE 3 — Shared-wiring integration:
   - DomainSchemaCatalog: +6 dispatches
   - BusinessSystemCompositionRoot: 6 `Add{Bc}Application()` + 6 schemas + 6 engine registrations
   - BusinessPolicyModule: +19 `CommandPolicyBinding`s
   - BusinessProjectionModule: +`WireEntitlementBc<T,H>` helper + 6 wirings + 19 event registrations
4. PHASE 4 — Clean rebuild 0/0; 71/72 architecture tests (1 pre-existing content-system/streaming failure).

## OUTPUT

Matrix per BC; files created/modified; no domain fixes needed; remaining gaps.

## VALIDATION CRITERIA

- 6 aggregates inherit `AggregateRoot` (already done).
- Per BC: 5 shared-contract files + 1 event-schema file + N T2E handlers + 2 projection files + 1 controller + 1 application module + 1 schema module + 1 topics.json + 1 rego.
- Shared wiring: 6 catalog entries, 6 app-modules, 19 policy bindings, 6 projection workers, 19 event registrations.
- `dotnet build --no-incremental` → 0 warnings, 0 errors.
- Architecture tests → 71/72 (1 failure in `content/streaming/**` controllers — pre-existing DET-SEED-DERIVATION-01, not in entitlement scope).

## REMAINING GAPS (explicit + justified)

1. **E2E tests not authored** for the 6 entitlement BCs. `BusinessE2EFixture` exists; per-BC e2e is straightforward follow-up.
2. **Pre-existing DET-SEED failure** in content-system/streaming controllers (8 files) — unchanged from customer pass; separate content-system hardening task.
3. **No cross-BC workflows** — allocation↔limit↔usage-right interactions are pre-D2 additions.
4. **UsageRight projection tracks running totals** (`TotalUsed`, `LastRecordId`) but no per-record history table — extension follows product demand.
