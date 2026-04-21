# Phase 2.5 — Structural-System Hardening Checklist

**Generated:** 2026-04-20
**Anchor prompt:** `claude/project-prompts/20260420-215431-structural-structural-system-central-spine-enforcement.md`
**Companion report:** `phase-2.5-structural-violations-report.md`
**Goal:** Make `structural-system` the enforced center of the architecture. Every non-structural system references structural truth; none duplicates it.

---

## Phased Execution Plan

Each phase is a hard gate — P1 completes before P2 begins. Each task carries an authorization marker. `[AWAIT]` = user authorization required before code edits. No invasive move happens without it.

---

## Phase P1 — Foundation (unblocks everything else)

### P1-R1 — Decide canonical `*Ref` location  `[AWAIT]`
**Options:**
- (A) `src/domain/structural-system/shared/reference/` — refs live inside the owning super-system; other systems import.
- (B) `src/shared/contracts/structural/` — refs are cross-system contract surface, published once.
- (C) `src/domain/shared-kernel/primitive/reference/` — refs are treated as kernel primitives.
**Recommendation:** **(A)** — refs belong to the system that owns the truth they reference. Aligns with `structural-system = where it belongs`. Keeps shared-kernel free of domain concepts.
**Deliverable upon authorization:** canonical location decision recorded as a new-rule per $1c and wired into `domain.guard.md`.

### P1-R2 — Publish six canonical structural refs  `[AWAIT]`
Create under the location chosen in P1-R1:
- `ClusterRef` — wraps `ClusterId`, validates non-empty.
- `ClusterAuthorityRef` — wraps `AuthorityId` of a structural authority.
- `ClusterProviderRef` — wraps structural `ProviderId`.
- `ClusterAdministrationRef` — wraps `AdministrationId` (created in P1-A1).
- `SubClusterRef` — wraps `SubclusterId`.
- `SpvRef` — wraps `SpvId`.

Every ref is a `readonly record struct` with explicit ctor and a domain-error on empty. Pattern matches the validated structural `ClusterId` (see `src/domain/structural-system/cluster/cluster/value-object/ClusterId.cs`).

### P1-A1 — Create `ClusterAdministration` aggregate  `[AWAIT]`
Scaffold the missing canonical cluster child:
- `cluster/administration/aggregate/AdministrationAggregate.cs`
- `cluster/administration/value-object/{AdministrationId, AdministrationDescriptor, AdministrationStatus}.cs`
- `cluster/administration/event/{AdministrationEstablishedEvent, AdministrationActivatedEvent, AdministrationSuspendedEvent, AdministrationRetiredEvent}.cs`
- `cluster/administration/specification/{CanActivateSpecification, CanSuspendSpecification, CanRetireSpecification}.cs`
- `cluster/administration/error/AdministrationErrors.cs`
- `cluster/administration/service/AdministrationService.cs`

Template: follow `cluster/authority/` structure verbatim. Parent ref: `ClusterRef` (from P1-R2).

### P1-D1 — Dedupe `ClusterId`  `[AWAIT]`
- Canonical: structural-system version (validated).
- Delete `src/domain/shared-kernel/primitive/identity/ClusterId.cs`.
- Sweep all `using Whycespace.Domain.SharedKernel.Primitive.Identity;` sites; re-point to `Whycespace.Domain.StructuralSystem.Cluster.Cluster`.
- CI grep guard: no `ClusterId` outside structural-system / refs.

**P1 exit criteria:** six refs compile + are used by at least the structural-system cluster aggregates themselves (descriptors replace raw `Guid` with `ClusterRef`); `ClusterAdministration` passes aggregate unit tests; zero `ClusterId` duplicates remain.

---

## Phase P2 — Binding Lifecycle + Cross-System ID Discipline

### P2-L1 — Harmonize status enums across cluster children  `[AWAIT]`
Canonical lifecycle: `{Defined | Established, Active, Suspended, Retired}` (+ `Reactivated` as a state-transition event).

Per-aggregate deltas:
| Aggregate   | Add event(s)                              | Rename terminal          |
|-------------|-------------------------------------------|--------------------------|
| Authority   | `AuthoritySuspended`, `AuthorityReactivated`, `AuthorityRetired` | `Revoked` → `Retired` (preserve event with rename) |
| Provider    | `ProviderReactivated`, `ProviderRetired` | add Retired terminal      |
| Subcluster  | `SubclusterSuspended`, `SubclusterReactivated`, `SubclusterRetired` | `Archived` → `Retired` |
| SPV         | `SpvReactivated`, `SpvRetired`           | `Closed` → `Retired`     |

Migration note: keep old event types for back-compat with projections until a sweep of projection handlers + event-store is coordinated.

### P2-B1 — Explicit attachment events  `[AWAIT]`
Emit on aggregate construction, carrying parent ref + effective date:
- `AuthorityAttachedEvent(AuthorityId, ClusterRef, EffectiveDate)`
- `ProviderAttachedEvent(ProviderId, ClusterRef, EffectiveDate)`
- `AdministrationAttachedEvent(AdministrationId, ClusterRef, EffectiveDate)`
- `SubClusterAttachedEvent(SubclusterId, ClusterRef, EffectiveDate)`
- `SpvAttachedEvent(SpvId, SubClusterRef, EffectiveDate)` (decide: SPV parent is SubCluster or Cluster — per doctrine, SubCluster)
- `StructuralBindingSuspendedEvent` / `StructuralBindingReactivatedEvent` / `StructuralBindingRetiredEvent` (generic, may replace per-aggregate suspended/retired if a unified binding aggregate is preferred).

### P2-D1 — Dedupe `ProviderId`  `[AWAIT]`
Decision required: does `business-system` have a BUSINESS-side `Provider` aggregate distinct from the structural provider node?
- If NO → delete business-system's `ProviderId` + migrate consumers to structural `ProviderId` via `ClusterProviderRef`.
- If YES → rename business-system's identity (e.g. `ProviderContractId` / `ProviderAgreementHolderId`) to remove the name collision.

### P2-D2 — Dedupe `AuthorityId` (semantic collision)  `[AWAIT]`
- Rename `src/domain/decision-system/governance/authority/value-object/AuthorityId.cs` → `GovernanceAuthorityId`.
- Sweep decision-system consumers.
- Document the distinction in `domain.guard.md` under Structural Rules.

### P2-D3 — Consolidate `ProviderRef` duplicates  `[AWAIT]`
- Canonical `ClusterProviderRef` (from P1-R2) replaces all five copies:
  - `business-system/provider/provider-core/provider-capability/value-object/ProviderRef.cs`
  - `business-system/provider/provider-governance/provider-agreement/value-object/ProviderRef.cs`
  - `business-system/provider/provider-scope/provider-availability/value-object/ProviderRef.cs`
  - `business-system/provider/provider-scope/provider-coverage/value-object/ProviderRef.cs`
  - business-system shared provider reference folder (5th copy)
- Sweep namespaces. Remove duplicates. Run build.

### P2-I1 — Parent-validity specifications  `[AWAIT]`
Introduce domain specs (pure, no I/O — executed by runtime with a ParentLookup port) enforcing:
- Parent aggregate exists.
- Parent is in `Active` state (`Retired`/`Suspended` parents reject new children).
- Re-parenting blocked unless explicit `Rebind` command passes `CanRebindSpecification`.

**P2 exit criteria:** lifecycle symmetric across children; canonical ref used everywhere; no structural ID name-collision; `ProviderRef` single-source; parent-validity specs wired into attach commands.

---

## Phase P3 — Cross-System Ref Discipline + Reference Layer

### P3-E1 — Replace `string SpvId` in economic-system  `[AWAIT]`
- `DistributionAggregate.SpvId : string` → `SpvRef`
- `RevenueAggregate.SpvId : string` → `SpvRef`
- `CapitalAllocatedToSpvEvent.TargetId : string` → `SpvRef`
- Migration: update event schemas; add event-version handling.

### P3-S1 — Carve `structure/topology-definition/`  `[AWAIT]`
Split `cluster/topology/` into:
- `structure/topology-definition/` — rules, topology types, allowed shapes (definition layer).
- `cluster/topology/` — live topology instances, each bound to a `TopologyDefinitionRef` (new).

### P3-S2 — Create `structure/reference-vocabularies/`  `[AWAIT]`
Centralize:
- Cluster-type codes.
- Authority-role codes.
- Provider-category codes.
- SPV-purpose codes (extract `SpvType` enum from `cluster/spv/value-object/`).
- Administration-scope codes.

### P3-H1 — Humancapital structural binding (minimal)  `[AWAIT]`
Scope-restricted to `assignment` + `participant` for this phase:
- Add `StructuralPlacementRef` = union (`ClusterRef | ClusterAuthorityRef | SubClusterRef | SpvRef`) to each aggregate's descriptor.
- Enforce on construction that the referenced structural node is Active.
- Emit `AssignmentPlacedEvent` / `ParticipantPlacedEvent` with the placement ref.
- Defer full lifecycle build-out of other humancapital aggregates.

**P3 exit criteria:** economic-system carries typed SPV refs; definition vs instance separation holds for topology; reference vocabularies centralized; humancapital minimally bound to structure.

---

## Phase P4 — Guard + Audit Promotion (lock the discipline)

### P4-G1 — Promote structural-ownership rules into `domain.guard.md`  `[AWAIT]`
New domain-aligned rules (capture first in `claude/new-rules/` per $1c, then promote):
- `DG-STRUCT-OWN-01` (S0) — only `structural-system` may define `{Cluster, Authority, Provider, Administration, Subcluster, Spv}Id` records.
- `DG-STRUCT-REF-01` (S0) — cross-system parent references MUST use canonical `*Ref` types from P1-R2; raw `Guid` parent fields forbidden outside structural-system.
- `DG-STRUCT-CMD-01` (S0) — only `structural-system` may emit events whose type names match `{Cluster|Authority|Provider|Administration|SubCluster|Spv}(Attached|Bound|Established|Registered|Defined|Created|Suspended|Reactivated|Retired)`.
- `DG-STRUCT-HIER-01` (S1) — no value-object/entity named `Parent*` or `Hierarchy*` or `Topology*` outside `structural-system`.
- `DG-STRUCT-STRING-01` (S2) — any `string` field named `*Id` representing a structural node is forbidden; must be typed ref.

### P4-A1 — Audit rules (for `claude/audits/domain.audit.md`)  `[AWAIT]`
- Grep regression: `public\s+Guid\s+\w*(Cluster|Authority|Provider|Administration|Subcluster|Spv)\w*Id` outside structural-system = fail.
- Grep regression: canonical ref duplication (more than one file per ref name across domain tree) = fail.
- Grep regression: `public\s+string\s+Spv(Id|Reference)` = fail.

---

## Traceability: Task ↔ Finding

| Task    | Closes findings           |
|---------|---------------------------|
| P1-R1/2 | F-102                     |
| P1-A1   | F-101                     |
| P1-D1   | F-201                     |
| P2-L1   | F-103                     |
| P2-B1   | F-103                     |
| P2-D1   | F-202                     |
| P2-D2   | F-203                     |
| P2-D3   | F-204                     |
| P2-I1   | F-104                     |
| P3-E1   | F-205                     |
| P3-S1/2 | F-105                     |
| P3-H1   | F-106                     |
| P4-G1/A1| hard-lock F-101..F-205    |

---

## Success Criteria (end of P4)

- `structural-system` is the sole source of placement / hierarchy / parent-binding truth.
- Zero duplicated parent definitions outside `structural-system`.
- All non-structural systems reference structure via canonical `*Ref` types only.
- Canonical cluster model is five-child complete (Authority / Providers / Administration / SubCluster / SPVs).
- Binding lifecycle is symmetric and explicit; every binding is addressable and auditable.
- Guard rules in `domain.guard.md` + audit rules in `domain.audit.md` make recurrence a CI failure.

---

## Authorization Gate

Each `[AWAIT]` task is gated. User to say either:
- *"proceed P1"* → execute P1-R1 through P1-D1 inclusive.
- *"proceed P1-A1 only"* → execute that single task.
- *"revise"* → adjust the checklist before execution.

No code edits happen until authorized. This checklist is the contract.
