# Business-System — Full E1→Ex Pilot (agreement/commitment/contract)

## CLASSIFICATION

- Classification: business-system
- Context: agreement
- Domain: commitment/contract
- Scope: **Full E1→Ex vertical slice** across all 6 template layers (01–06)

## CONTEXT

User correction 2026-04-21T12:44: prior runs (103840, 112627, 122210, 123444) delivered only D1 Partial (sections 1–6, domain-only) across business-system (55 BCs), structural-system (24), core-system (31). Those were explicitly NOT E1→Ex complete per the user's definition — "Do NOT stop at domain-only. Do NOT report success if phases 7–18 / 02–06 equivalents are incomplete."

Pre-execution scan of non-domain business-system layers showed **zero** engines / runtime wiring / projections / API controllers / tests / infrastructure / rego policies for any of 55 BCs. Scaling the full pattern to all 55 BCs = ~660–1100 new files = multi-session undertaking.

**This prompt narrows scope to one exemplar BC** — `agreement/commitment/contract` — taken fully through all 6 layers using `economic/capital/account` as the canonical reference. Establishes the pattern all other 54 BCs will replicate in future sessions.

## OBJECTIVE

Produce one BC (`agreement/commitment/contract`) classified as **Full E1→Ex complete** across:

1. Domain (already done)
2. Engine (T2E; no T1M — no multi-step orchestration in contract)
3. Runtime wiring (dispatcher via `IEngineRegistry` + schema catalog + composition root + bootstrap catalog entry)
4. Projection (reducer + handler + read model)
5. API (controller + base class)
6. Infrastructure (Kafka topics manifest + Rego policy)
7. Test scaffold (e2e shell — skipped pending fixture)

## CONSTRAINTS

- Do NOT touch domain files (D1 already complete — domain is frozen).
- Do NOT add policy-feedback reactors, OPA detection workers, expiry schedulers, compensation workflows, cross-system integration bridges. Not scope for a 1-BC pilot.
- Do NOT invent domain events (AddParty doesn't emit an event — preserve that, flag as follow-up).
- Do NOT touch other business-system BCs.
- Preserve `ContractAggregate.Create(ContractId)` static factory pattern (differs from economic's instance `Open()` method — adjust handler accordingly).

## EXECUTION STEPS

1. Read exemplar across all 6 layers under `src/*/economic/capital/account/` and `infrastructure/{event-fabric,policy}/.../economic/capital/account*`.
2. Author shared contracts (commands, event schemas, read model, queries, policy IDs).
3. Author 5 T2E handlers (one per command: Create, AddParty, Activate, Suspend, Terminate).
4. Author projection reducer + handler + `BusinessProjectionModule`.
5. Author `BusinessControllerBase` + `ContractController`.
6. Author composition: `ContractApplicationModule`, `BusinessPolicyModule`, `BusinessSystemCompositionRoot`.
7. Author runtime schema module + `DomainSchemaCatalog.RegisterBusinessAgreementCommitmentContract` dispatch.
8. Add `new BusinessSystemCompositionRoot()` to `BootstrapModuleCatalog.All`.
9. Author Kafka topics.json + Rego policy.
10. Author e2e test shell (skipped, documented blocker: `BusinessE2EFixture` not yet authored).
11. Verify: `dotnet build` 0/0 + 72/72 architecture tests.
12. Produce matrix + store this prompt.

## OUTPUT

- 22 files created + 2 modified.
- Per-layer matrix.
- Honest status per layer including stubs/skips.
- Explicit list of blockers preventing declaration as "Full E1→Ex complete."

## VALIDATION CRITERIA

- `dotnet build` → 0 warnings, 0 errors.
- `dotnet test --filter "FullyQualifiedName~Architecture"` → 72/72 pass.
- 5 commands map 1:1 to 5 T2E handlers.
- 4 domain events map 1:1 to 4 event schemas in shared-contracts + 4 schema catalog registrations + 4 projection reducer Apply methods + 4 projection handler dispatches.
- Every command has a policy ID binding.
- Topic manifest declares exactly 4 topics: commands/events/retry/deadletter.
- Rego policy declares an allow rule per policy ID + default deny + hard-deny for missing inputs.
- `BusinessSystemCompositionRoot` is in `BootstrapModuleCatalog.All`.

## BLOCKERS TO DECLARING D2 ACTIVE

Documented explicit — each requires explicit follow-up work and cannot be silently declared complete:

1. **E2E test is a skipped scaffold.** No `BusinessE2EFixture` exists (would need `HealthProbe` seeding, deterministic `IIdGenerator`, `ProjectionVerifier` wiring). Test file compiles with correct happy-path shape; body won't run until fixture authored.
2. **`AddPartyToContract` emits no domain event.** Pre-existing — `ContractAggregate.AddParty` mutates `_parties` directly. Projection's `Parties` list stays empty because there's no event for the reducer to apply. Follow-up: decide whether to add `ContractPartyAddedEvent` to the domain (would alter domain — intentionally deferred).
3. **No T1M workflow.** Contract is single-step lifecycle (Create → Activate → Suspend/Terminate). N/A for this BC.
4. **`ContractCreatedEvent` schema has no `CreatedAt` field.** Matches the domain event shape (domain event carries only `ContractId`). Projection seeds `CreatedAt = DateTimeOffset.MinValue`. Either add `CreatedAt` to the domain event or compute in reducer via `envelope.OccurredAt` — flagged as follow-up.
5. **Bootstrapped composition is minimal** — no integration workers, detection workers, schedulers. All deferred per pilot scope constraint.

## SCALING PATH

With this pilot as the canonical business-system E1→Ex pattern, the remaining 54 BCs follow the same 22-file template, adjusted for per-BC commands/events. Each subsequent BC is ~12 files/hour of authoring once the pattern is internalized. Reasonable future cadence: 3–5 BCs per session via parallel subagents, ~11–18 sessions to land all 55 BCs at D2 Active.
