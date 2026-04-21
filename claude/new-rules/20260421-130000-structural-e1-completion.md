---
name: Structural-system E1 completion — relationship rules, binding proof, cluster cardinality
description: New canonical surfaces introduced during structural-system E1 completion. Captures five additive structural concepts (relationship-rules folder, IStructuralRelationshipPolicy contract, enforcement specs pattern, AttachedUnder binding proof, cluster child-cardinality tracking) that do not appear in any existing guard. All rules are drafted from implemented code; none require retroactive refactoring.
type: guards
---

# Structural-System E1 Completion (2026-04-21)

Captured per CLAUDE.md $1c following an additive implementation that closes E1 gaps identified in the preceding structural audit (interaction constraints, cross-aggregate binding proof, cluster cardinality uniqueness, and Guid→Ref type discipline on descriptors). All rules below reflect code that now exists on `dev_wip`; the guard file should absorb them in the next consolidation pass so `domain.audit.md` Section 22 can cite them directly.

**Scope boundary:** these rules govern the **structural-system** only. They do not introduce a new canonical classification, do not alter the four-layer guard model, and do not touch the `IStructuralParentLookup` contract that already exists.

---

## R-STRUCT-RELATIONSHIP-RULES-LOCATION-01 — Canonical home for declarative interaction constraints

- **CLASSIFICATION:** domain / structural / structure
- **SEVERITY:** S1
- **DESCRIPTION:**
  - All declarative interaction/compatibility/scope constraints between structural nodes MUST live under `src/domain/structural-system/structure/relationship-rules/`.
  - The folder holds **pure immutable classes** — no aggregates, no lifecycle, no services, no events, no persistence concerns. A relationship-rule class is a data holder that answers `IsAllowed(...)` / `AllowsUnderSubcluster(...)` queries against a frozen definition.
  - Canonical members today: `AuthorityProviderMatrix`, `AuthoritySubclusterConstraint`, `SpvScopeConstraint`. New structural relationship rules MUST land here rather than spawning a sibling subsystem.
  - Each rule class MUST expose a static `Permissive` (or equivalent named) factory so composition roots can wire a safe default before a project declares its restricted matrix.
- **CHECK:**
  - `find src/domain/structural-system/structure/relationship-rules -type f -name "*.cs"` — all files declare a single public class, no aggregate/event/service patterns, no mutable state.
  - Repo-wide `grep -rE "(AuthorityProviderMatrix|AuthoritySubclusterConstraint|SpvScopeConstraint)" src/` — no shadow declarations outside `structural-system/structure/relationship-rules/`.

## R-STRUCT-RELATIONSHIP-POLICY-CONTRACT-01 — Single shared-contract port for relationship evaluation

- **CLASSIFICATION:** domain / structural / contracts
- **SEVERITY:** S1
- **DESCRIPTION:**
  - Relationship rule access from structural specs MUST flow through `Whycespace.Domain.StructuralSystem.Contracts.References.IStructuralRelationshipPolicy`. This mirrors `IStructuralParentLookup`: a narrow interface in `contracts/references/` that the runtime wires with a concrete implementation.
  - Specs that consume relationship rules MUST take `IStructuralRelationshipPolicy` as a constructor parameter (primary-constructor form is acceptable — matches `CanAttachUnderParentSpecification`). Specs MUST NOT reach for static `.Permissive` defaults at evaluation time; the permissive default is a composition-root concern.
  - `IStructuralRelationshipPolicy` stays in `contracts/references/` even though its properties reference types in `structure/relationship-rules/`. Rationale: the contract is the binding surface the runtime injects; co-locating it with the concrete types would force consumers to depend on the definition subsystem.
- **CHECK:**
  - `grep -nE "interface IStructuralRelationshipPolicy" src/domain/structural-system/` — exactly one match in `contracts/references/IStructuralRelationshipPolicy.cs`.
  - `grep -rE "class Can(Bind|Report)[A-Za-z]+To[A-Za-z]+Specification|class SpvScopeSpecification" src/domain/structural-system/cluster/` — every spec takes `IStructuralRelationshipPolicy` via primary ctor (no field-level `= new ...Permissive` fallbacks).

## R-STRUCT-ATTACHMENT-PROOF-01 — Every attach-time factory persists an AttachedUnder proof

- **CLASSIFICATION:** domain / structural / cluster
- **SEVERITY:** S1
- **DESCRIPTION:**
  - Each cluster-child aggregate (`AuthorityAggregate`, `ProviderAggregate`, `AdministrationAggregate`, `SubclusterAggregate`, `SpvAggregate`) MUST expose an `AttachedUnder?` property whose value is populated from a dedicated `{Name}BindingValidatedEvent` during the attach-time factory overload that takes `IStructuralParentLookup`.
  - The proof VO (`public readonly record struct AttachedUnder(ClusterRef Parent, StructuralParentState ParentState, DateTimeOffset EffectiveAt)`) lives in the aggregate's own `value-object/` folder. Cross-aggregate reuse is intentional duplication: the parent type may evolve per aggregate in a future iteration (today all five share `ClusterRef`).
  - `{Name}BindingValidatedEvent` MUST carry `(Id, ClusterRef Parent, StructuralParentState ParentState, DateTimeOffset EffectiveAt)` — the `ParentState` field is the load-bearing addition vs. existing `*AttachedEvent` records. The existing `*AttachedEvent` records stay unchanged (no wire break).
  - The factory overload with lookup MUST:
    1. call `CanAttachUnderParentSpecification.IsSatisfiedBy(parent)` and throw `{Name}Errors.InvalidParent()` on false;
    2. invoke the non-lookup overload to raise the canonical domain events;
    3. append the proof event so the aggregate's own state carries a replayable record of the validation.
- **CHECK:**
  - `grep -nE "AttachedUnder\?\s+AttachedUnder\s*\{ get; private set; \}" src/domain/structural-system/cluster/` — exactly five matches (one per cluster-child aggregate).
  - `grep -rnE "record [A-Z][A-Za-z]+BindingValidatedEvent\(" src/domain/structural-system/cluster/` — five matches; every record includes `StructuralParentState ParentState`.
  - `grep -nE "public static .+Aggregate (Establish|Register|Define|Create)\([^)]*IStructuralParentLookup" src/domain/structural-system/cluster/**/aggregate/*.cs` — five matches; each body calls `CanAttachUnderParentSpecification` before raising the proof event.

## R-STRUCT-CLUSTER-CARDINALITY-01 — ClusterAggregate tracks active authority + administration bindings

- **CLASSIFICATION:** domain / structural / cluster
- **SEVERITY:** S1
- **DESCRIPTION:**
  - `ClusterAggregate` MUST expose `IReadOnlyCollection<ClusterAuthorityRef> ActiveAuthorities` and `IReadOnlyCollection<ClusterAdministrationRef> ActiveAdministrations`. Both are derived from private sets updated via dedicated binding/release events.
  - Four events are canonical: `ClusterAuthorityBoundEvent`, `ClusterAuthorityReleasedEvent`, `ClusterAdministrationBoundEvent`, `ClusterAdministrationReleasedEvent`. These events live in `cluster/cluster/event/` and carry `(ClusterId, <typed-ref>)`.
  - Uniqueness enforcement lives in two specs under `cluster/cluster/specification/`: `UniqueActiveAuthoritySpecification` and `UniqueAdministrationSpecification`. Both take `(currentActive, incoming)` and return false if the incoming ref already appears.
  - The aggregate exposes `RecordAuthorityAttached` / `RecordAuthorityReleased` / `RecordAdministrationAttached` / `RecordAdministrationReleased`. The attach methods MUST consult the uniqueness spec and throw `ClusterErrors.DuplicateAuthority()` / `ClusterErrors.DuplicateAdministration()` on violation.
  - Cross-aggregate wiring (who calls `RecordAuthorityAttached`) is a runtime concern; the domain rule stops at "the method exists and enforces uniqueness."
- **CHECK:**
  - `grep -nE "RecordAuthorityAttached|RecordAdministrationAttached|RecordAuthorityReleased|RecordAdministrationReleased" src/domain/structural-system/cluster/cluster/aggregate/ClusterAggregate.cs` — all four methods present.
  - `grep -nE "UniqueActiveAuthoritySpecification|UniqueAdministrationSpecification" src/domain/structural-system/cluster/cluster/specification/` — two spec files present.
  - `grep -nE "DuplicateAuthority|DuplicateAdministration" src/domain/structural-system/cluster/cluster/error/ClusterErrors.cs` — both errors present.

## R-STRUCT-DESCRIPTOR-REF-DISCIPLINE-01 — Cluster-child descriptors carry typed refs, not Guids

- **CLASSIFICATION:** domain / structural / cluster
- **SEVERITY:** S1
- **DESCRIPTION:**
  - `AuthorityDescriptor`, `ProviderProfile`, `SubclusterDescriptor`, `SpvDescriptor`, and `AdministrationDescriptor` MUST expose their parent-cluster reference as `ClusterRef` — never `Guid`. Constructors accept `ClusterRef` positionally; the null-check uses `== default` per readonly-record-struct idiom.
  - Factory helpers that previously constructed `new ClusterRef(descriptor.X)` MUST be simplified to `descriptor.X` after this rule lands. A grep for `new ClusterRef\(descriptor\.` under `structural-system/cluster/` MUST return zero matches.
  - Event records (`*AttachedEvent`, `*BindingValidatedEvent`) continue to carry typed `ClusterRef` — they already complied pre-rule. This rule closes the descriptor-side gap that forced the aggregate to perform a runtime wrap at attach time.
- **CHECK:**
  - `grep -nE "public Guid (Cluster|Parent|Authority|Provider|Subcluster|Spv|Administration)" src/domain/structural-system/cluster/**/value-object/*Descriptor.cs src/domain/structural-system/cluster/**/value-object/*Profile.cs` — zero matches.
  - `grep -nE "new ClusterRef\(descriptor\.|new ClusterRef\(profile\." src/domain/structural-system/cluster/` — zero matches (aggregate bodies simplified).
  - `grep -nE "readonly record struct (Authority|Subcluster|Spv)Descriptor|readonly record struct ProviderProfile" src/domain/structural-system/cluster/` — every ctor signature contains a `ClusterRef` parameter.

## R-ECON-CAPITAL-ALLOCATION-SPVREF-01 — Aggregate state stores typed SpvRef; wire keeps string

- **CLASSIFICATION:** domain / economic / capital
- **SEVERITY:** S1
- **DESCRIPTION:**
  - `CapitalAllocationAggregate.TargetSpv` MUST be typed `SpvRef?`. The prior `string? SpvTargetId` property is removed. Wire surfaces (`CapitalAllocatedToSpvEvent`, `CapitalAllocatedToSpvEventSchema`, `AllocateCapitalToSpvCommand`, `CapitalAllocationReadModel`) keep `string SpvTargetId` for backward compatibility — `CapitalAllocatedToSpvEvent.TargetSpv` is the existing `[JsonIgnore] SpvRef?` accessor that mediates.
  - The aggregate MUST expose BOTH overloads of `AllocateToSpv`: a primary `AllocateToSpv(SpvRef, decimal)` and a string-accepting `AllocateToSpv(string, decimal)` that normalises via `Guid.TryParse` and delegates. Engine handlers should migrate to the typed overload over time; the string overload exists solely so today's command wiring (`AllocateCapitalToSpvCommand.SpvTargetId` as `string`) does not break.
  - `EnsureInvariants` MUST assert `TargetSpv is not null` (not `string.IsNullOrWhiteSpace`) when `TargetType == SPV`.
- **CHECK:**
  - `grep -nE "public string\? SpvTargetId" src/domain/economic-system/capital/allocation/aggregate/CapitalAllocationAggregate.cs` — zero matches.
  - `grep -nE "public SpvRef\? TargetSpv" src/domain/economic-system/capital/allocation/aggregate/CapitalAllocationAggregate.cs` — exactly one match.
  - `grep -nE "public void AllocateToSpv\(" src/domain/economic-system/capital/allocation/aggregate/CapitalAllocationAggregate.cs` — two matches (SpvRef overload and string overload).

---

## Promotion plan

1. Absorb all six rules into `claude/guards/domain.guard.md` Section 22 (Structural Central-Spine rules) on the next consolidation pass. Rule IDs stay verbatim.
2. Add entries to `claude/audits/domain.audit.md` Section 22 that cite each rule ID with its static check command.
3. Retire this new-rules capture by moving it under `claude/new-rules/_archives/` once absorbed.

## Rationale (why these are NOT covered by existing rules)

- **DG-STRUCT-CONTRACTS-PURITY-01** allows interfaces in `contracts/references/` but says nothing about WHICH interfaces. R-STRUCT-RELATIONSHIP-POLICY-CONTRACT-01 promotes `IStructuralRelationshipPolicy` to canonical alongside `IStructuralParentLookup`.
- **DG-STRUCT-PARENT-LOOKUP-01** covers the existing lookup but not the new relationship policy port. R-STRUCT-ATTACHMENT-PROOF-01 closes the gap that was visible at audit time: the lookup existed but the aggregate never persisted the result.
- **DG-STRUCT-VOCAB-LOCATION-01** guards enums — it does not constrain where declarative constraint objects live. R-STRUCT-RELATIONSHIP-RULES-LOCATION-01 is the companion rule for the class-form vocabulary.
- **DG-ECON-SPV-REF-ACCESS-01** is an advisory (S2) rule about accessor existence on wire types. R-ECON-CAPITAL-ALLOCATION-SPVREF-01 is the stronger rule on the domain aggregate state — separate concern, separate severity.
