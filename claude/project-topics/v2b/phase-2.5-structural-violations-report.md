# Phase 2.5 — Structural Violations Report

**Generated:** 2026-04-20
**Scope:** `src/domain/**` — every classification-system vs `structural-system` central-spine doctrine
**Doctrine anchor:** `structural-system` is the single source of truth for placement / hierarchy / parent binding / cluster relationships. All other systems reference, never own.
**Severity scale:** S0 breaks doctrine • S1 duplicated ownership • S2 missing ref / type-safety gap • S3 cosmetic / advisory

---

## 1. Executive Summary

Current structural-system posture is **foundational but incomplete**. Core cluster aggregates (Cluster / Authority / Provider / Subcluster / SPV) exist with basic event sourcing, but:

- The canonical cluster model has a hole: **`ClusterAdministration` aggregate does not exist**.
- All parent references are raw `Guid`; no strongly-typed `ClusterRef` / `AuthorityRef` / `ProviderRef` / `AdministrationRef` / `SubClusterRef` / `SpvRef` value objects anywhere in the codebase.
- Cross-aggregate invariants (parent-exists, parent-is-active, cascade-on-retire, rebind guard) are NOT enforced at the domain boundary.
- Binding lifecycle is asymmetric (Authority/Subcluster lack Suspended; nothing has Reactivated; SPV uses Closed not Retired).
- Five `ProviderRef` duplicates exist **inside** `business-system` alone.
- `ClusterId` is defined twice (shared-kernel + structural-system) with divergent validation.
- `ProviderId` is defined twice (business-system + structural-system).
- `AuthorityId` is defined twice with semantic collision (decision-system governance authority vs structural-system cluster authority).
- `economic-system` uses raw `string SpvId` — no type safety, no structural traceability.

**Worst-leakage system:** `business-system` (5× duplicated ProviderRef + shadow ProviderId).
**Highest-risk finding:** F-101 (ClusterAdministration missing) and F-201 (no canonical structural refs exist) — every other remediation depends on these.

---

## 2. Findings — Structural-System Completeness Gaps

### F-101 — `ClusterAdministration` aggregate is MISSING (S0)
**Source:** canonical cluster model requires Cluster → {Authority, Providers, Administration, SubCluster, SPVs}.
**Evidence:** `src/domain/structural-system/cluster/` contains `authority/`, `provider/`, `subcluster/`, `spv/`, `cluster/`, `lifecycle/`, `topology/` — no `administration/`.
**Impact:** The spine is incomplete. Operational and compliance flows that would bind to an administration node have nowhere to attach.
**Remediation:** Create `cluster/administration/` with `AdministrationAggregate`, events (`AdministrationEstablished` / `Activated` / `Suspended` / `Retired`), value-objects (`AdministrationId`, `AdministrationDescriptor`, `AdministrationStatus`), specs, errors. Follow the Authority aggregate as template.

### F-102 — No canonical structural `*Ref` value objects (S0)
**Evidence:** Codebase-wide grep: `ClusterRef`, `AuthorityRef` / `ClusterAuthorityRef`, `ProviderRef` (structural), `AdministrationRef` / `ClusterAdministrationRef`, `SubClusterRef`, `SpvRef` do not exist at the structural-system or shared-kernel level. Every parent pointer is a raw `Guid` — see `AuthorityDescriptor(Guid clusterReference, …)`, `ProviderProfile(Guid clusterReference, …)`, `SubclusterDescriptor(Guid parentClusterReference, …)`, `SpvDescriptor(Guid clusterReference, …)`.
**Impact:** Non-structural systems have no vocabulary to reference structural truth with type safety. They inevitably either (a) drift by redefining their own refs (see F-301 through F-305) or (b) fall back to raw `Guid`/`string` (see F-302).
**Remediation:** Publish strongly-typed `*Ref` value-objects under a canonical location (see Checklist task R1 for the location decision).

### F-103 — Binding lifecycle is asymmetric across cluster children (S1)

| Aggregate       | Status enum                            | Suspended | Reactivated | Retired (terminal) | Explicit "Attached" event |
|-----------------|----------------------------------------|-----------|-------------|--------------------|---------------------------|
| Cluster         | Defined / Active / Archived            | missing   | missing     | Archived          | N/A (root)                |
| Authority       | Established / Active / Revoked         | **missing** | missing   | Revoked           | **missing**               |
| Provider        | Registered / Active / Suspended        | present   | missing     | **missing** (no Retired) | **missing**         |
| Subcluster      | Defined / Active / Archived            | **missing** | missing   | Archived          | **missing**               |
| SPV             | Created / Active / Suspended / Closed  | present   | missing     | Closed (≠ Retired semantically) | **missing**     |

**Impact:** No cross-aggregate uniformity; operators cannot suspend authorities without revoking them; no reactivation path from Suspended; binding is implicit (lives only in a descriptor Guid), so audits cannot reconstruct attachment history.
**Remediation:** Harmonize on {Defined/Established, Active, Suspended, Retired}. Add `*Attached` event per child (emits on construction with parent ref + effective date). Add `Reactivated` where missing.

### F-104 — No cross-aggregate parent-validity invariants (S1)
**Evidence:**
- `SubclusterDescriptor` checks `ParentClusterReference != Guid.Empty` (nothing else).
- No spec/service verifies the referenced Cluster is (a) an existing aggregate, (b) in `Active` state, (c) not retired/archived.
- No cascade: when a Cluster is archived, children are not notified / suspended / blocked from new work.
- No rebind guard: no aggregate has `Rebind(newParent)` with invariant check; implicit re-parenting is possible.
**Impact:** The invariants the user doctrine calls for ("SPV cannot exist without valid parent", "retired parent cannot accept children", "invalid re-parenting blocked") are **not enforced**.
**Remediation:** Introduce structural binding specifications that run at Attach-time (see Checklist task H2).

### F-105 — `structure/` reference layer incomplete (S2)
**Evidence:** Present: `structure/classification/`, `structure/type-definition/`, `structure/hierarchy-definition/`. Missing: `structure/topology-definition/` (topology currently lives at `cluster/topology/`, conflating definition with instance). Missing: `structure/reference-vocabularies/` (enums like `SpvType` are scattered inside cluster value-objects, not centralized).
**Impact:** Live instances do not validate against definitions. `HierarchyDefinition` holds one self-consistency rule (parent ≠ self, `HierarchyDefinitionAggregate.cs:93-94`) but live Subcluster/Authority/Provider creation does not check the definition.
**Remediation:** Carve a canonical `structure/topology-definition/` and `structure/reference-vocabularies/`. Add live-vs-definition validation on instance creation.

### F-106 — `humancapital/` aggregates are stubs without structural binding (S2)
**Evidence:** `humancapital/{assignment, eligibility, governance, incentive, operator, participant, performance, reputation, sanction, sponsorship, stewardship, workforce}/` — most aggregates have a `Create()` method with little or no event emission or state. None reference `ClusterRef` / `AuthorityRef` / `SubClusterRef` / `SpvRef` as the structural node they are placed under.
**Impact:** Workforce placement truth is unenforced. If humancapital is the structural participation layer, a person can be "assigned" with no validated structural parent.
**Remediation:** Phase-scope. For Phase 2.5: add structural parent ref to `assignment` + `participant` only. Defer full lifecycle build-out.

---

## 3. Findings — Cross-System Structural Leakage

### F-201 — `ClusterId` defined twice with divergent semantics (S0)

| File                                                                            | Form                                                                                 | Notes                               |
|---------------------------------------------------------------------------------|--------------------------------------------------------------------------------------|-------------------------------------|
| `src/domain/shared-kernel/primitive/identity/ClusterId.cs`                      | `public readonly record struct ClusterId(Guid Value);` — no validation               | Minimal primitive                   |
| `src/domain/structural-system/cluster/cluster/value-object/ClusterId.cs`        | Explicit ctor with `throw ClusterErrors.MissingId()` on `Guid.Empty`                 | Canonical structural version        |

**Impact:** Two sources of truth for the single most central identity in the spine.
**Remediation options:**
- Option A (preferred): structural-system keeps the validated `ClusterId`; shared-kernel version is removed and consumers re-point. Requires sweep of `using Whycespace.Domain.SharedKernel.Primitive.Identity` for `ClusterId`.
- Option B: shared-kernel version absorbs the validation, structural-system version re-exports via `using alias`. Keeps compatibility at cost of splitting validation authority from ownership. Not recommended.

### F-202 — `ProviderId` defined twice (S0)

| File                                                                                              | Namespace                                                  |
|---------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| `src/domain/business-system/provider/provider-core/provider/value-object/ProviderId.cs`          | `Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider` |
| `src/domain/structural-system/cluster/provider/value-object/ProviderId.cs`                       | `Whycespace.Domain.StructuralSystem.Cluster.Provider`     |

**Impact:** The two `ProviderId`s are structurally identical records — business-system has its own `Provider` aggregate which duplicates the placement identity the structural provider already owns. Per doctrine, business-system must *reference* a provider, not own one.
**Remediation:** Treat structural-system's `ProviderId` as canonical. Business-system's provider core aggregate (if it represents the same provider entity and not a distinct business-side artifact) should be refactored to consume a `ProviderRef`. If it is a distinct business-side concept (e.g., "provider contract" vs "provider structural node"), rename to avoid name collision.

### F-203 — `AuthorityId` defined twice with semantic collision (S1)

| File                                                                                    | Semantic                                     |
|-----------------------------------------------------------------------------------------|----------------------------------------------|
| `src/domain/decision-system/governance/authority/value-object/AuthorityId.cs`           | GOVERNANCE authority (who decides)           |
| `src/domain/structural-system/cluster/authority/value-object/AuthorityId.cs`            | STRUCTURAL authority (authority cluster-node)|

**Impact:** Same name, different concept — read-and-review hazard, maintenance trap.
**Remediation:** Keep structural-system's name `AuthorityId` (owns the cluster-authority concept). Rename decision-system's identity to `GovernanceAuthorityId` (or `DecisionAuthorityId`) and document the distinction.

### F-204 — `ProviderRef` duplicated 5× inside `business-system` (S1)
**Evidence:**
- `business-system/provider/provider-core/provider-capability/value-object/ProviderRef.cs`
- `business-system/provider/provider-governance/provider-agreement/value-object/ProviderRef.cs`
- `business-system/provider/provider-scope/provider-availability/value-object/ProviderRef.cs`
- `business-system/provider/provider-scope/provider-coverage/value-object/ProviderRef.cs`
- (plus a 5th copy under provider/shared reference folder — discovered during survey)

All five copies are near-identical record structs wrapping a `Guid`, each in its own namespace. No canonical `ProviderRef` exists under structural-system.
**Impact:** Five independent "truth" types for provider reference. Cross-context type mismatches silently fail at compile time in consumers that import the wrong namespace.
**Remediation:** Create canonical `ProviderRef` under structural-system, remove the five duplicates, re-point all consumers. Sweep via `using` directives.

### F-205 — `economic-system` uses `string SpvId` instead of typed ref (S2)
**Evidence:**
- `src/domain/economic-system/revenue/distribution/aggregate/DistributionAggregate.cs:21,30` — `public string SpvId { get; }`
- `src/domain/economic-system/revenue/revenue/aggregate/RevenueAggregate.cs:15,28` — `public string SpvId { get; }`
- `src/domain/economic-system/capital/allocation/event/CapitalAllocatedToSpvEvent.cs:5-8` — `string TargetId` for SPV target

**Impact:** No type safety. A malformed or wrong-kind id can flow through the economic pipeline undetected. No structural traceability.
**Remediation:** Replace with canonical `SpvRef` once F-102 is resolved.

### F-206 — No cross-system illegitimate command/event leakage detected (informational)
**Finding:** Grep across non-structural systems for commands/events named `AttachToCluster` / `ClusterBound` / `AuthorityEstablished` / etc. returned **zero matches**. Business/content/economic/operational systems do not emit structural-binding events. This boundary is currently clean.
**Impact:** Positive — protect this via new-rule promotion.

---

## 4. Severity Matrix

| Finding | Category                           | Severity | Phase |
|---------|------------------------------------|----------|-------|
| F-101   | Missing ClusterAdministration      | S0       | P1    |
| F-102   | No canonical structural refs       | S0       | P1    |
| F-201   | Duplicate ClusterId                | S0       | P1    |
| F-202   | Duplicate ProviderId               | S0       | P2    |
| F-103   | Asymmetric binding lifecycle       | S1       | P2    |
| F-104   | No parent-validity invariants      | S1       | P2    |
| F-203   | AuthorityId semantic collision     | S1       | P2    |
| F-204   | ProviderRef duplicated 5×          | S1       | P2    |
| F-105   | Incomplete structure/ layer        | S2       | P3    |
| F-106   | humancapital stubs + no binding    | S2       | P3    |
| F-205   | string SpvId in economic           | S2       | P3    |
| F-206   | Command/event leakage boundary OK  | S3       | guard |

---

## 5. What This Report Does NOT Do

- Does not perform refactors. Anti-drift ($5) prevents unilateral cross-system edits.
- Does not move or delete any duplicated file. Consumers must be swept before deletion.
- Does not invent new guards. Proposed guard rules go into `claude/new-rules/` per $1c when the Phase 2.5 plan is authorized.

Authorization gate: see `phase-2.5-structural-hardening-checklist.md` for the phased execution plan.
