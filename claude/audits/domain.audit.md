# Domain Audit (Canonical)

**Validates:** [`claude/guards/domain.guard.md`](../../guards/domain.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the domain layer (business truth protection) complies with all rules consolidated in the canonical domain guard: layer purity, classification/context/domain nesting, classification suffix conventions, DTO naming, behavioral invariants, structural rules, and domain-aligned bounded contexts (economic, governance, identity, observability, workflow).

## Scope

- `src/domain/**` — all bounded contexts at all activation levels (D0/D1/D2)
- DTO definitions, aggregate roots, domain events, domain errors
- Domain-aligned guard families (economic / governance / identity / observability / workflow)

## Source guard

This audit checks the rules defined in [`claude/guards/domain.guard.md`](../../guards/domain.guard.md). Rule ID conventions (Domain numbered 1–24 + D-PURITY-/D-DET-/D-NO-SYSCLOCK, DS-R1..R8, CLASS-SFX-R1..R3, DTO-R1..R4, Behavioral numbered 1–16 + B-CLOCK-01/B-ID-01/B-CHAIN-01, Structural numbered 1–16 + STR-*-01, Economic ECON-*-01 + T1..T5/R1..R5/X1..X5/RC1..RC5/CR1..CR5/E1..E10 with D41..D79 + C26..C65, POLICY-*-01, ID-*-01, OBS-*-01, WF-*-01, GE-01..05) are owned by that guard.

---

## Validation Checklist

### Section 1 — Domain Layer Purity

- [ ] **Core Purity Rules 1–24** (domain.guard.md §Domain Layer Purity — unnumbered rules 1–24): zero external dependencies; no infrastructure imports; no engine/runtime imports; pure DDD primitives only; event-first (rule 23)
- [ ] **D-PURITY-01** — purity extension
- [ ] **D-DET-01** — determinism extension
- [ ] **D-NO-SYSCLOCK** — no system clock in domain

### Section 2 — Domain Structure (Classification / Context / Domain Nesting)

- [ ] **DS-R1** — canonical nesting `{classification}-system/{context}/[{domain-group}/]{domain}/` (3-level or 4-level per DS-R3a)
- [ ] **DS-R3a** — domain-group atomicity per context (no half-adopted contexts; group names carry semantic meaning)
- [ ] **DS-R2** — classification layer present
- [ ] **DS-R3** — context layer present
- [ ] **DS-R4** — domain leaf present
- [ ] **DS-R5** — no premature nesting
- [ ] **DS-R6** — no cross-classification leakage
- [ ] **DS-R7** — single-responsibility per domain
- [ ] **DS-R8** — aggregate placement discipline

### Section 3 — Classification Suffix Conventions

- [ ] **CLASS-SFX-R1** — `-system` suffix on classification folders (S0; supersedes DS-R1/R2 S1 — audit treats as S0)
- [ ] **CLASS-SFX-R2** — suffix consistency
- [ ] **CLASS-SFX-R3** — no stray suffixes

### Section 4 — DTO Naming

- [ ] **DTO-R1** — role clarity
- [ ] **DTO-R2** — naming consistency
- [ ] **DTO-R3** — no domain duplication
- [ ] **DTO-R4** — no ambiguous DTOs

### Section 5 — Behavioral Rules

- [ ] **Behavioral rules 1–16** (domain.guard.md §Behavioral Rule Set — unnumbered rules): aggregate behavior invariants; event emission from aggregate; no external calls from aggregate; deterministic behavior; rule 15 event-first
- [ ] **B-CLOCK-01** — clock access only through abstraction
- [ ] **B-ID-01** — ID access only through abstraction
- [ ] **B-CHAIN-01** — chain access only through abstraction
- [ ] **D-VO-TYPING-01** — aggregates/events use VO types for ids, not raw Guid/string
- [ ] **D-ERR-TYPING-01** — domain MUST NOT throw framework exceptions (ArgumentException, InvalidOperationException, etc.); use Guard / `{Bc}Errors`
- [ ] **DOM-LIFECYCLE-INIT-IDEMPOTENT-01** — lifecycle-init action (`Open*`/`Create*`/`Initialize*`) refuses re-emission on already-loaded aggregate (`Version >= 0`)
- [ ] **REG-CONSISTENCY-01** — bidirectional consistency between `claude/registry/activation-registry.json` and `src/domain/{classification}/{context}/{domain}/` — no orphans either way

### Section 6 — Structural Rules

- [ ] **Structural rules 1–16** (domain.guard.md §Structural Rule Set — unnumbered rules): aggregate placement; event placement; error placement; factory placement
- [ ] **STR-AUTH-01** — authorization structural constraint
- [ ] **STR-HEALTH-01** — health structural constraint
- [ ] **STR-OBS-01** — observability structural constraint
- [ ] **STR-GUARD-REGISTRY-01** — guard registry structural constraint

### Section 7 — Domain-Aligned: Economic — Core

- [ ] **ECON-ES-01** — event sourcing is source of truth
- [ ] **ECON-CQRS-01** — read/write separation
- [ ] **ECON-LEDGER-01** — invariant enforcement

### Section 8 — Domain-Aligned: Economic — Transaction Context

- [ ] **T1** — transaction flow rule
- [ ] **T2** — transaction flow rule
- [ ] **T3** — transaction flow rule
- [ ] **T4** — transaction flow rule
- [ ] **T5** — transaction flow rule
- [ ] **D41..D45** — transaction domain constraints
- [ ] **C26..C30** — transaction violation codes

### Section 9 — Domain-Aligned: Economic — Revenue Context

- [ ] **R1** — revenue flow rule (scoped to revenue context; disambiguate from runtime/DG/dead-code R1)
- [ ] **R2** — revenue flow rule
- [ ] **R3** — revenue flow rule
- [ ] **R4** — revenue flow rule
- [ ] **R5** — revenue flow rule
- [ ] **D46..D50** — revenue domain constraints
- [ ] **C31..C35** — revenue violation codes

### Section 10 — Domain-Aligned: Economic — Exposure Context

- [ ] **X1** — exposure flow rule (scoped to exposure context; disambiguate from exchange X1..X5)
- [ ] **X2** — exposure flow rule
- [ ] **X3** — exposure flow rule
- [ ] **X4** — exposure flow rule
- [ ] **X5** — exposure flow rule
- [ ] **D51..D55** — exposure domain constraints
- [ ] **C36..C40** — exposure violation codes

### Section 11 — Domain-Aligned: Economic — Reconciliation Context

- [ ] **RC1** — reconciliation flow rule
- [ ] **RC2** — reconciliation flow rule
- [ ] **RC3** — reconciliation flow rule
- [ ] **RC4** — reconciliation flow rule
- [ ] **RC5** — reconciliation flow rule
- [ ] **D56..D62** — reconciliation domain constraints
- [ ] **C41..C47** — reconciliation violation codes

### Section 12 — Domain-Aligned: Economic — Cross-Domain Reconciliation

- [ ] **CR1** — cross-domain reconciliation rule
- [ ] **CR2** — cross-domain reconciliation rule
- [ ] **CR3** — cross-domain reconciliation rule
- [ ] **CR4** — cross-domain reconciliation rule
- [ ] **CR5** — cross-domain reconciliation rule
- [ ] **D63..D65** — cross-domain reconciliation constraints
- [ ] **C48..C50** — cross-domain reconciliation violation codes

### Section 13 — Domain-Aligned: Economic — Enforcement & Compliance

- [ ] **E1** — economic enforcement rule (scoped to economic context; disambiguate from engine E1..E16)
- [ ] **E2** — economic enforcement rule
- [ ] **E3** — economic enforcement rule
- [ ] **E4** — economic enforcement rule
- [ ] **E5** — economic enforcement rule
- [ ] **E6** — economic enforcement rule
- [ ] **E7** — economic enforcement rule
- [ ] **E8** — economic enforcement rule
- [ ] **E9** — economic enforcement rule
- [ ] **E10** — economic enforcement rule
- [ ] **D66..D74** — economic enforcement domain constraints
- [ ] **C51..C60** — economic enforcement violation codes

### Section 14 — Domain-Aligned: Economic — Cross-Domain Exchange

- [ ] **X1** — exchange flow rule (scoped to exchange context — collision with exposure X1 is intentional)
- [ ] **X2** — exchange flow rule
- [ ] **X3** — exchange flow rule
- [ ] **X4** — exchange flow rule
- [ ] **X5** — exchange flow rule
- [ ] **D75..D79** — exchange domain constraints
- [ ] **C61..C65** — exchange violation codes

### Section 15 — Domain-Aligned: Governance

- [ ] **POLICY-ENFORCEMENT-01** — governance policy enforcement
- [ ] **POLICY-CHAIN-01** — governance policy chain binding
- [ ] **POLICY-DETERMINISM-01** — governance policy determinism

### Section 16 — Domain-Aligned: Identity

- [ ] **ID-POLICY-01** — identity policy
- [ ] **ID-DETERMINISM-01** — identity determinism

### Section 17 — Domain-Aligned: Observability

- [ ] **OBS-TRACE-01** — observability trace
- [ ] **OBS-REPLAY-01** — observability replay

### Section 18 — Domain-Aligned: Workflow

- [ ] **WF-PLACEMENT-01** — workflow placement (engines, not domain)
- [ ] **WF-TYPE-01** — workflow type discipline
- [ ] **WF-PIPELINE-01** — workflow pipeline shape

*Note: WF-PLACEMENT-01 (workflows in engines) vs runtime dependency-graph R2 (engines cannot reference runtime) — compatible per guard; audit confirms compatibility at validation time.*

### Section 19 — WBSM v3 Global Enforcement (shared)

- [ ] **GE-01** — deterministic execution
- [ ] **GE-02** — WHYCEPOLICY enforcement
- [ ] **GE-03** — WHYCECHAIN anchoring
- [ ] **GE-04** — event-first architecture
- [ ] **GE-05** — CQRS enforcement

### Section 20 — Activation Lifecycle

- [ ] D0/D1/D2 activation levels match canonical registry
- [ ] Scaffolded BCs remain scaffolded
- [ ] D1 BCs have minimum DDD artifacts (aggregate/event/error)
- [ ] D2 BCs have full DDD + engine wiring + runtime integration
- [ ] No activation regressions; no premature D2 claims

### Section 21 — Rules Promoted from new-rules/ (2026-04-18)

- [ ] **RUNBOOK-CONTROL-PLANE-COVERAGE-01** — route restriction/removal PR updates all referencing operator runbooks in the same patch

### Section 22 — Rules Promoted from new-rules/ (2026-04-20) — Phase 2.5 Structural Central-Spine

**Severity categorization (2026-04-20 scheme):** S0 = correctness-critical · S1 = structural integrity · S2 = advisory / audit.

**S0 — Correctness-critical (compile / replay / data-integrity)**

- [ ] **DG-STRUCT-CONTRACTS-IMPORT-01** — no non-structural system imports `StructuralSystem.Cluster.*` / `.Structure.*` / `.Humancapital.*`; only `Contracts.References`.
  Static check: `grep -rE "using Whycespace\.Domain\.StructuralSystem\.(Cluster|Structure|Humancapital)" src/domain/{business,content,economic,operational,decision}-system/`.
- [ ] **DG-STRUCT-ID-DEDUPE-01** — structural identity records (`Cluster*Id`, `AuthorityId`, `ProviderId`, `AdministrationId`, `Subcluster*Id`, `SpvId`) exist only in `structural-system/cluster/**`; non-structural duplicates forbidden unless qualified by prefix (`GovernanceAuthorityId`, `BusinessProviderId`, etc.).
  Static check: `grep -rE "(readonly record struct|record|class) (Cluster[A-Za-z]*Id|AuthorityId|ProviderId|AdministrationId|Subcluster[A-Za-z]*Id|SpvId)\b" src/domain/` — fail on matches outside `structural-system/cluster/**` without qualifying prefix.

**S1 — Structural integrity (must not drift)**

- [ ] **DG-STRUCT-REF-CANONICAL-01** — six canonical refs (`ClusterRef`, `ClusterAuthorityRef`, `ClusterProviderRef`, `ClusterAdministrationRef`, `SubClusterRef`, `SpvRef`) live only in `structural-system/contracts/references/`; no duplicates.
  Static check: `grep -rE "readonly record struct (ClusterRef|ClusterAuthorityRef|ClusterProviderRef|ClusterAdministrationRef|SubClusterRef|SpvRef)\b" src/` — fail outside the canonical folder.
- [ ] **DG-STRUCT-CONTRACTS-PURITY-01** — `structural-system/contracts/**` holds only `readonly record struct`, `record`, `enum`, and interface declarations; no aggregates / events / services / error classes.
  Static check: `grep -rE "(public\s+)?(sealed\s+)?(class|abstract class) " src/domain/structural-system/contracts/` excluding interface declarations — fail on matches.
- [ ] **DG-STRUCT-ATTACH-EFFECTIVE-01** — every cluster-child factory has a `DateTimeOffset effectiveAt` overload; every `*AttachedEvent` uses a canonical ref (no raw `Guid` parent).
  Static check: per aggregate in `structural-system/cluster/**`, `grep -E "public static .+Aggregate (Establish|Register|Create|Define)\(.*DateTimeOffset"`; `grep -rE "record .*AttachedEvent\([^)]*\bGuid\s+(Cluster|Authority|SubCluster|Spv|Administration)" src/domain/structural-system/cluster/` — fail on matches.
- [ ] **DG-STRUCT-PARENT-LOOKUP-01** — `CanAttach*` / `CanRebind*` specs take `IStructuralParentLookup` via ctor; runtime wires at least one concrete lookup; humancapital placement factories take the lookup.
  Static check: per spec file in `structural-system/cluster/**/specification/`, verify `CanAttach*` class signatures include `(IStructuralParentLookup lookup)`; verify `IStructuralParentLookup.cs` exists in contracts; verify composition root registers an implementation.
- [ ] **DG-STRUCT-VOCAB-LOCATION-01** — `*Type` / `*Role` / `*Category` / `*Class` / `*Kind` enums (excluding `*Status`) live only in `structural-system/structure/reference-vocabularies/`. No duplicate enums with matching name+values outside. No `string` type-code fields where the canonical vocab enum exists.
  Static check: `grep -rE "public enum [A-Za-z]+(Type|Role|Category|Class|Kind)$" src/domain/structural-system/cluster/**/value-object/` — fail on matches; repo-wide grep per vocab for duplicate declarations; `grep -rE "public string (SpvType|AuthorityRole|ProviderCategory)" src/domain/` — fail on matches.
- [ ] **DG-STRUCT-DEFN-VS-INSTANCE-01** — definition-model aggregates in `structure/<topic>-definition/`, live instances in `cluster/<topic>/`. No new `.cs` files under `cluster/topology/**` after 2026-04-20 without documented replay/compat rationale.
  Static check: `git log --diff-filter=A --since=2026-04-21 -- 'src/domain/structural-system/cluster/topology/**.cs'` — any new file = fail unless paired new-rules capture justifies.
- [ ] **DG-HC-STRUCTURAL-BINDING-01** — humancapital placement fields use canonical refs only (no raw `Guid`/`string` parent fields); placement factories enforce (non-null lookup, active parent, bounded effective date).
  Static check: `grep -rE "(Guid|string) (Cluster|Authority|Provider|Subcluster|Spv|Administration)(Id|Reference|Ref)\b" src/domain/structural-system/humancapital/` — fail on matches; per-factory inspection of `ParticipantAggregate.Place` / `AssignmentAggregate.Assign` for the three invariant checks.

**S2 — Advisory / audit**

- [ ] **DG-STRUCT-LIFECYCLE-CANONICAL-01** — cluster-child status enums include `Suspended` + `Retired`; `*ReactivatedEvent` present. New `*ArchivedEvent`/`*RevokedEvent`/`*ClosedEvent` after 2026-04-20 = advisory drift.
  Static check: `grep -E "Suspended|Retired" src/domain/structural-system/cluster/**/value-object/*Status.cs` — warn on enums missing either; `git log --since=2026-04-21` for new `(Archived|Revoked|Closed)Event` declarations = advisory flag.
- [ ] **DG-ECON-SPV-REF-ACCESS-01** — economic surfaces carrying string SPV IDs expose `[JsonIgnore] SpvRef?` typed accessor; new factory overloads accept `SpvRef`.
  Static check: per `.cs` in `economic-system/**` with `public string SpvId`/`TargetId`: verify sibling `[JsonIgnore] public SpvRef?` property exists. Missing = advisory flag.

---

## Check Procedure

1. Load the domain guard rule set.
2. Execute per-section `Check Procedure` blocks in the guard.
3. Record verdicts with file:line evidence.
4. For domain-aligned sections, scope checks to `src/domain/{classification}/{context}/{domain}` paths.
5. For ID-collision families (R1..R5, E1..E10, X1..X5), disambiguate by bounded context per guard.

## Pass / Fail Criteria

- **PASS:** All rules `PASS` or `N/A`.
- **FAIL:** Any S0/S1 failure.
- **CONDITIONAL:** S2/S3 captured per $1c.

## Output Format

```
AUDIT:           domain
GUARD:           claude/guards/domain.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   ~171
SECTIONS:        22
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list>
S1_FAILURES:     <list>
S2_FAILURES:     <list>
EVIDENCE:        <path>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12 and $1c.
