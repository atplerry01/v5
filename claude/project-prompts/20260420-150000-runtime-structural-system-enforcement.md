---
classification: runtime
context: structural-system
domain: identity-topology-enforcement
status: completed
completed-at: 2026-04-20
---

# Structural System Enforcement Pass

## TITLE
Structural System Enforcement — Identity + Topology + Parent Binding + Bypass Removal + Fail-Fast

## CONTEXT
Existing codebase already has:
- HSID v2.1 compact-ID seam (`IDeterministicIdEngine`, string `PPP-LLLL-TTT-TOPOLOGY-SEQ`).
- Deterministic Guid seam (`IIdGenerator.Generate(seed) → Guid`) — canonical aggregate primary key.
- `DomainRoute = (classification, context, domain)` 3-tuple on `CommandContext` (per DS-R8).
- `AggregateRoot` base class with event-sourced apply/replay loop and `HydrateIdentity` rehydration contract (`AGGREGATE-IDENTITY-REHYDRATION-01`).
- `domain.guard.md` defining structural rules (DS-R1..R8, D-DET-01, B-ID-01, E2/E9, D69/D74 source-traceability, etc.).

Known baseline constraints (must NOT be violated):
- Do NOT introduce a new identity system.
- Do NOT replace `DomainRoute`.
- Do NOT add parallel structural layers.
- Only enforce what already exists.

## OBJECTIVE
Make existing structural rules unbypassable at construction + runtime boundaries:
1. No aggregate/entity/VO may be constructed with a primitive/non-deterministic identity.
2. Every aggregate whose domain truth requires a structural parent (per `domain.guard.md`) MUST enforce it in its constructor / factory, not at application layer.
3. Every rule in `domain.guard.md` that is documented-only must be lifted into executable enforcement at aggregate or policy sites.
4. All bypass paths (raw factory deserialization, reflection rehydration without validation, test shortcuts) are eliminated.
5. All structural violations fail fast with diagnostic detail.

## CONSTRAINTS (hardened from user prompt)
- HARD: No new identity system. No parallel structural layer.
- HARD: `DomainRoute` stays a `CommandContext` 3-tuple — it is NOT lifted onto `AggregateRoot` as a universal field (doing so would be a parallel structural layer).
- HARD: Enforcement sites = aggregate constructors / domain policies. Not application layer.
- HARD: Tests must cover both valid + invalid construction paths.
- SOFT: Per-aggregate parent bindings are enforced where `domain.guard.md` already declares the relationship (e.g., `AuditRecordAggregate` requiring `SourceDomain`+`SourceAggregateId`+`SourceEventId`, D69/D74). Universal base-class parent is NOT introduced.

## EXECUTION STEPS
1. **Phase 1 — Identity Enforcement (HSID + IIdGenerator)**
   1. Enumerate every aggregate/entity/VO under `src/domain/**`.
   2. For each, catalog the identity field type and constructor inputs.
   3. Flag:
      - raw `Guid` / `string` ID fields NOT wrapped in a typed value object,
      - identity creation bypassing `IIdGenerator` (direct `Guid.NewGuid`) — violates D-DET-01 / B-ID-01,
      - HSID string fields without `IDeterministicIdEngine.IsValid` check on construction.
   4. Add constructor guards that reject `Guid.Empty` / blank / structurally-invalid identities.
2. **Phase 2 — Structural Parent Binding (per-aggregate, per domain.guard.md)**
   1. Extract the set of aggregates whose `domain.guard.md` rules mandate a source/parent reference (starting with D69/D74 for audit + violation aggregates).
   2. For each such aggregate, verify the constructor / factory method enforces the declared parent reference is non-empty and structurally valid.
   3. Do NOT introduce a universal `AggregateRoot.Parent` field; enforcement is local to each aggregate contract.
3. **Phase 3 — Topology Rule Enforcement**
   1. Parse `domain.guard.md` rules (DS-R*, B-* family, D-DET-01, source-traceability rules, etc.).
   2. For each, locate the enforcement point in code.
   3. If rule is documented-only, add invariant check in `EnsureInvariants()` or constructor of the responsible aggregate / value object.
4. **Phase 4 — Remove Bypass Paths**
   1. Scan for reflection-based rehydration that skips constructor validation.
   2. Scan for test-only factory shortcuts that bypass invariants.
   3. Scan for `JsonSerializer.Deserialize<T>` into aggregate types without a post-deserialize validation call.
   4. Eliminate or gate each bypass behind a compile-time contract (internal constructor + friend-assembly policy).
5. **Phase 5 — Fail-Fast Guarantees**
   1. All constructor invariant violations throw typed domain invariant exceptions (leveraging `IDomainInvariantViolation`).
   2. Add xUnit coverage: invalid construction → throw; valid construction → pass.
   3. Wire structural-violation exception into existing runtime dispatch failure pipeline so it surfaces as a canonical failure report ($12).

## OUTPUT FORMAT
For each phase:
- `FINDING` block: file, rule, severity, description.
- `REMEDIATION` block: file, patch summary.
- `PROOF` block: test file + assertion name.

Final consolidated report at top-level with per-classification pass/fail tally.

## VALIDATION CRITERIA
- Zero primitive-ID aggregates (all identity fields typed VO or HSID-validated).
- Zero `Guid.NewGuid` / `DateTime.UtcNow` in `src/domain/**`.
- Every aggregate whose `domain.guard.md` rule mandates a parent reference enforces it in constructor.
- Every doc-only invariant rule in `domain.guard.md` has an executable counterpart.
- Zero reflection rehydration / deserialization bypasses remain.
- All structural violations produce typed failure + test coverage.

## PROMOTION / DRIFT CAPTURE
Any new rule discovered during enforcement must be captured under `claude/new-rules/{YYYYMMDD-HHMMSS}-domain.md` per $1c, with fields CLASSIFICATION / SOURCE / DESCRIPTION / PROPOSED_RULE / SEVERITY.
