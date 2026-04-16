# Runtime Audit (Canonical)

**Validates:** [`claude/guards/runtime.guard.md`](../../guards/runtime.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the runtime layer (execution + enforcement) complies with all rules consolidated in the canonical runtime guard: runtime order & lifecycle, Phase 1.5 runtime hardening, engine purity, projections (CQRS read models), prompt container, dependency graph & layer boundaries, contracts boundary, code quality enforcement (CCG / dead code / stubs) as a Runtime subsystem, and Test & E2E validation as a Runtime subsystem, plus shared WBSM v3 Global Enforcement.

## Scope

- `src/runtime/**` — runtime execution support, internal projection routing/orchestration
- `src/engines/**` — engine layer (stateless, events only)
- `src/projections/**` — CQRS read models
- Prompt container / prompt reconciliation surfaces
- Dependency-graph edges across `src/domain`, `src/engines`, `src/runtime`, `src/platform`, `src/systems`, `src/projections`, `src/shared`
- Contracts boundary (`src/shared/contracts/**`) from the runtime/engine side
- Code quality enforcement (CCG / dead-code / stub-detection)
- Test & E2E validation harnesses

Note: Determinism, HSID, Hash, Replay, and System Invariants live in `constitutional.audit.md`. Composition Loader and Program Composition live in `infrastructure.audit.md`.

## Source guard

This audit checks the rules defined in [`claude/guards/runtime.guard.md`](../../guards/runtime.guard.md). Rule ID conventions (R1..R15, R-CTX-/R-ORDER-/R-UOW-/R-WORKFLOW-/R-WF-/R-POLICY-/R-CANONICAL-PIPELINE-/R-EVENT-/R-CHAIN-/RO-/R-RT-, E1..E16 + ENG-/E-WORKFLOW-/E-STEP-/E-RESUME-/E-STATE-/E-TYPE-/E-VERSION-/E-LIFECYCLE-FACTORY-, P1..P32 + P-TYPE-ALIGN-/P-AGNOSTIC-/PROJ-, PROMPT-RECONCILE-, Dependency-graph R1..R7 + DG-R5-/DG-R7-/DG-BASELINE-/DG-SCRIPT-HYGIENE-/DG-COMPOSITION-ROOT-, G-CONTRACTS-, CCG-, Dead-code R1..R4, STUB-R1..R6, T-BUILD-/T-DOUBLES-/T-PLACEHOLDER-/T-POLICY-/T1M-/ACT-FABRIC-, G-E2E-, GE-01..05) are owned by that guard.

---

## Validation Checklist

### Section 1 — Runtime Order & Lifecycle

- [ ] **R1** — runtime is sole command router
- [ ] **R2** — runtime is sole event dispatcher
- [ ] **R3** — middleware registered in runtime only
- [ ] **R4** — projections triggered by runtime events
- [ ] **R5** — no direct engine invocation from platform
- [ ] **R6** — runtime owns transaction scope
- [ ] **R7** — runtime is sole persist / publish / anchor authority
- [ ] **R8** — runtime owns retry and circuit breaker
- [ ] **R9** — runtime pipeline is linear
- [ ] **R10** — runtime context propagation
- [ ] **R11** — no domain logic in runtime
- [ ] **R12** — runtime must enforce policy middleware
- [ ] **R13** — runtime must anchor events to chain
- [ ] **R14** — outbox is mandatory path
- [ ] **R15** — no engine direct invocation outside dispatcher
- [ ] **R-CTX-01** — context propagation invariant
- [ ] **R-ORDER-01** — ordering invariant
- [ ] **R-UOW-01** — unit-of-work boundary
- [ ] **R-WORKFLOW-PIPE-01** — workflow pipeline sequencing
- [ ] **R-DOM-LEAK-01** — domain-leak detector (sub-clause of R11)
- [ ] **R-POLICY-PATH-01** — policy evaluation path enforced
- [ ] **R-WF-EVENTIFIED-01** — workflow observer paths eventified
- [ ] **R-WF-RESUME-01** — workflow resume path intact
- [ ] **R-POLICY-FIRST-01** — policy-first ordering
- [ ] **R-CANONICAL-PIPELINE-01** — canonical pipeline shape
- [ ] **POLICY-PIPELINE-INTEGRATION-01** — policy pipeline integration
- [ ] **R-WF-PAYLOAD-01** — workflow payload contract
- [ ] **R-WF-PAYLOAD-TYPED-01** — workflow payload typed
- [ ] **R-EVENT-AUDIT-COLS-01** — event audit columns populated
- [ ] **R-CHAIN-CORRELATION-01** — chain correlation id propagated
- [ ] **RT-API-CORRELATION-ECHO-01** / **R-CHAIN-CORRELATION-SURFACE-01** — API `meta.correlationId` echoes runtime-stamped id on command paths; `Guid.Empty` on success-command response = violation
- [ ] **R-RT-CMD-AGGID-01** — every `*Command` implements `IHasAggregateId`; reflective property-name fallback deprecated
- [ ] **RT-OUTBOX-AGGID-FROM-ENVELOPE-01** — outbox row `aggregate_id` sourced from `IEventEnvelope.AggregateId`; no reflection on payload type
- [ ] **RT-BACKGROUND-IDENTITY-EXPLICIT-01** — every `BackgroundService`/`IHostedService` wraps dispatch in `SystemIdentityScope.Begin("system/<worker>")`
- [ ] **RO-LOCKED-ORDER** — locked pipeline order
- [ ] **RO-1** — no reordering
- [ ] **RO-2** — no optional middlewares
- [ ] **RO-3** — no parallel fabric stages
- [ ] **RO-4** — no alternative entry points
- [ ] **RO-5** — policy between pre- and post-policy guards
- [ ] **RO-6** — chain must follow persist
- [ ] **RO-7** — outbox must follow chain
- [ ] **RO-CANONICAL-11** — canonical 11-stage pipeline locked

*Note: `R-WF-OBSERVER-01` is REVOKED (2026-04-07). Do not validate.*

### Section 2 — Phase 1.5 Runtime Rules

- [ ] **R-RT-01** — Phase 1.5 runtime invariant
- [ ] **R-RT-02** — Phase 1.5 runtime invariant
- [ ] **R-RT-03** — Phase 1.5 runtime invariant
- [ ] **R-RT-04** — Phase 1.5 runtime invariant
- [ ] **R-RT-05** — Phase 1.5 runtime invariant
- [ ] **R-RT-06** — IClock + IIdGenerator injection (with MI-1 owner-token shape exemption)
- [ ] **R-RT-07** — Phase 1.5 runtime invariant
- [ ] **R-RT-08** — Phase 1.5 runtime invariant
- [ ] **R-RT-09** — Phase 1.5 runtime invariant
- [ ] **R-RT-10** — Phase 1.5 runtime invariant

### Section 3 — Engine Purity

- [ ] **E1** — tier classification
- [ ] **E2** — T0U: no domain imports
- [ ] **E3** — T1M: no direct domain mutation
- [ ] **E4** — T2E: domain execution only
- [ ] **E5** — T3I: external boundary
- [ ] **E6** — T4A: schedule and trigger
- [ ] **E7** — no cross-tier engine imports (same-tier permitted per 2026-04-13 amendment)
- [ ] **E8** — engines never define domain models
- [ ] **E9** — engines are stateless
- [ ] **E10** — engine folder structure
- [ ] **E11** — engine input/output types
- [ ] **E12** — engine testability
- [ ] **E13** — no persistence in engines
- [ ] **E14** — event emission only output
- [ ] **E15** — `EngineContext` surface restriction
- [ ] **E16** — policy pre-condition required
- [ ] **E-WORKFLOW-01** — engine workflow constraint
- [ ] **E-STEP-01** — engine step constraint
- [ ] **E-STEP-02** — engine step constraint
- [ ] **E-VERSION-01** — engine version discipline
- [ ] **E-LIFECYCLE-FACTORY-01** — lifecycle factory required
- [ ] **E-LIFECYCLE-FACTORY-CALL-SITE-01** — lifecycle factory call site enforced
- [ ] **E-RESUME-01** — engine resume constraint
- [ ] **E-RESUME-02** — engine resume constraint
- [ ] **E-RESUME-03** — engine resume constraint
- [ ] **E-STATE-01** — engine state constraint
- [ ] **E-STATE-02** — engine state constraint
- [ ] **E-STATE-03** — engine state constraint
- [ ] **E-TYPE-01** — engine type discipline
- [ ] **E-TYPE-02** — engine type discipline
- [ ] **E-TYPE-03** — engine type discipline
- [ ] **ENG-PURITY-01** — engine purity consolidated rule
- [ ] **ENG-DOMAIN-ALIGN-01** — engine domain alignment

### Section 4 — Projections (CQRS read models)

- [ ] **P1..P13** — projection rules block 1 (lines 641–695 of runtime.guard.md)
- [ ] **P-IDEMPOTENCY-KEY-NOT-NULL-01** — projection upserts populate `idempotency_key`; `count(*) WHERE idempotency_key IS NULL` = 0
- [ ] **P-VERSION-MONOTONE-01** — projection upserts set `current_version = envelope.Version`; rows with `current_version = 0 AND last_event_id IS NOT NULL` = 0
- [ ] **P-JSONB-KEY-CASE-01** — JSONB state casing matches index/query extractor casing (single documented convention per read model)
- [ ] **P-EVENT-TIMESTAMP-STAMP-01** — temporal read-model fields stamped from `envelope.Timestamp`; `default(DateTimeOffset)` (`0001-01-01`) on populated rows = violation
- [ ] **P14..P19** — projection rules block 2
- [ ] **P20..P27** — projection rules block 3
- [ ] **P28..P32** — projection rules block 4
- [ ] **P-TYPE-ALIGN-01** — projection type alignment
- [ ] **P-AGNOSTIC-01** — projection engine-agnostic
- [ ] **PROJ-READ-ONLY-01** — projections are read-only
- [ ] **PROJ-DOMAIN-ALIGN-01** — projections domain-aligned
- [ ] **PROJ-WF-EXEC-01** — projection workflow execution constraint
- [ ] **PROJ-REPLAY-SAFE-01** — projections replay-safe
- [ ] **PROJ-NO-INPLACE-MUTATION-01** — projections do not mutate in place

### Section 5 — Prompt Container

- [ ] **Prompt Container rules 1–15** (runtime.guard.md §Prompt Container, unnumbered — validate by rule order): mandatory sections present; classification declared; policy binding declared; no prompts stored outside `claude/project-prompts/`
- [ ] **PROMPT-RECONCILE-01** — prompt reconciliation pre-execution pass completed

### Section 6 — Dependency Graph & Layer Boundaries

- [ ] **Dependency-graph R1** — domain purity edge
- [ ] **Dependency-graph R2** — engine isolation edge
- [ ] **Dependency-graph R3** — runtime authority edge
- [ ] **Dependency-graph R4** — platform boundary edge
- [ ] **Dependency-graph R5** — host-domain edge
- [ ] **Dependency-graph R6** — projection consumer edge
- [ ] **Dependency-graph R7** — shared kernel edge
- [ ] **DG-R5-EXCEPT-01** — documented exceptions only for R5
- [ ] **DG-R5-HOST-DOMAIN-FORBIDDEN** — host → domain forbidden (5 clauses, 2026-04-08)
- [ ] **DG-R5-01** — converted to exception; validate exception registry only
- [ ] **DG-R7-01** — baseline drift detection integrated
- [ ] **DG-BASELINE-01** — dependency-graph baseline present and up to date
- [ ] **DG-SCRIPT-HYGIENE-01** — script hygiene rules applied
- [ ] **DG-COMPOSITION-ROOT-01** — composition-root boundary held at runtime/platform seam

*Note: bare `R1..R7` in this section collide with Runtime Order `R1..R15` and Dead Code `R1..R4`. Validators MUST disambiguate by guard section.*

### Section 7 — Contracts Boundary

- [ ] **G-CONTRACTS-01** — shared contracts purity
- [ ] **G-CONTRACTS-02** — no domain leakage into shared kernel
- [ ] **G-CONTRACTS-03** — cross-domain contracts placement
- [ ] **G-CONTRACTS-04** — contracts naming
- [ ] **G-CONTRACTS-05** — contracts versioning
- [ ] **G-CONTRACTS-06** — contracts consumer discipline

### Section 8 — Code Quality Enforcement (Runtime Subsystem)

*Subsystem of Runtime Enforcement per GUARD-LAYER-MODEL-01.*

- [ ] **CCG-01** — readability
- [ ] **CCG-02** — function size & focus
- [ ] **CCG-03** — no spaghetti logic
- [ ] **CCG-04** — no over-engineering
- [ ] **CCG-05** — domain purity
- [ ] **CCG-06** — layer isolation
- [ ] **CCG-07** — determinism
- [ ] **CCG-08** — self-documenting code
- [ ] **CCG-09** — consistency
- [ ] **CCG-10** — testability
- [ ] **CCG-FILE-NAME-MATCHES-TYPE-01** — source `.cs` file name matches single public top-level type (S3 baseline; S2 when boundary-misleading)
- [ ] **GUARD-PIPELINE-TEMPLATE-01** — `/pipeline/*.md` templates MUST be per-batch-stateless; batch-specific inputs appear only as `<placeholder>` / `{classification}` markers; no literal classification names in template files
- [ ] **Dead Code R1** — reference check (runtime.guard.md §Dead Code Elimination)
- [ ] **Dead Code R2** — registration check
- [ ] **Dead Code R3** — projection consumption
- [ ] **Dead Code R4** — single pattern rule
- [ ] **STUB-R1** — no empty catch
- [ ] **STUB-R2** — no placeholder returns
- [ ] **STUB-R3** — no TODO-only methods
- [ ] **STUB-R4** — no unimplemented interfaces that ship
- [ ] **STUB-R5** — stub registry discipline
- [ ] **STUB-R6** — stub escape-hatch discipline

### Section 9 — Test & E2E Validation (Runtime Subsystem)

*Subsystem of Runtime Enforcement per GUARD-LAYER-MODEL-01.*

- [ ] **Test Architecture rules 1–5** (runtime.guard.md §Test Architecture — unnumbered rules): naming, isolation, determinism, invariant coverage, deterministic-test discipline
- [ ] **T-BUILD-01** — build gate (plus its strengthening clause)
- [ ] **T-DOUBLES-01** — test doubles policy
- [ ] **T-PLACEHOLDER-01** — no placeholder tests
- [ ] **T-POLICY-001** — policy-test enforcement
- [ ] **T1M-RESUME-TEST-COVERAGE-01** — T1M resume test coverage
- [ ] **ACT-FABRIC-ROUNDTRIP-TEST-01** — activation-fabric roundtrip test present
- [ ] **G-E2E-001** — no PASS without execution evidence
- [ ] **G-E2E-002** — layer coverage mandatory
- [ ] **G-E2E-003** — determinism in fixtures
- [ ] **G-E2E-004** — policy decision required
- [ ] **G-E2E-005** — chain anchor required
- [ ] **G-E2E-006** — DLQ before commit
- [ ] **G-E2E-007** — replay equivalence
- [ ] **G-E2E-008** — no test self-cleanup
- [ ] **G-E2E-009** — severity ladder
- [ ] **G-E2E-010** — untested = FAIL
- [ ] **G-E2E-011** — static checks are STAGE 0

### Section 10 — WBSM v3 Global Enforcement (shared)

- [ ] **GE-01** — deterministic execution
- [ ] **GE-02** — WHYCEPOLICY enforcement
- [ ] **GE-03** — WHYCECHAIN anchoring
- [ ] **GE-04** — event-first architecture
- [ ] **GE-05** — CQRS enforcement

---

## Check Procedure

1. Load the runtime guard rule set.
2. For each section, execute the per-section `Consolidated Check Procedure` in the guard.
3. Record a verdict per rule: `PASS` / `FAIL` / `N/A` with file:line evidence.
4. Aggregate by section and overall.

## Pass / Fail Criteria

- **PASS:** All rules `PASS` or `N/A`.
- **FAIL:** Any S0 or S1 failure.
- **CONDITIONAL:** S2/S3 captured to `claude/new-rules/` per CLAUDE.md $1c.

## Output Format

```
AUDIT:           runtime
GUARD:           claude/guards/runtime.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   ~180
SECTIONS:        10
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list>
S1_FAILURES:     <list>
EVIDENCE:        <path>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12 and $1c.
