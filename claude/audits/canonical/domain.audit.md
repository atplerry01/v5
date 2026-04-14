# Domain Audit (Canonical)

**Validates:** [`claude/guards/domain.guard.md`](../../guards/domain.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the domain layer (business truth protection) complies with all rules consolidated in the canonical domain guard: classification/context/domain nesting, zero external dependencies, classification suffix conventions, contracts boundary, DTO naming, behavioral invariants, dependency-graph layer purity, and domain-aligned bounded-context rules (economic, governance, identity, observability, workflow).

## Scope

- `src/domain/**` — all bounded contexts at all activation levels (D0/D1/D2)
- `src/shared/contracts/**` — cross-domain contracts boundary
- DTO definitions, aggregate roots, domain events, domain errors
- Dependency graph across `src/domain`, `src/engines`, `src/runtime`, `src/platform`, `src/systems`, `src/projections`, `src/shared`

## Source guard

This audit checks the rules defined in [`claude/guards/domain.guard.md`](../../guards/domain.guard.md). Rule families (Domain Rxx, DS-Rxx, CLASS-SFX-Rxx, G-CONTRACTS-xx, DTO families, Behavioral Bxx + B-CLOCK/B-ID/B-CHAIN, DG-Rxx + DG-* exceptions, ECON-*, T1–T5, D41–D79, C26–C65, R1–R5, X1–X5, RC1–RC5, CR1–CR5, E1–E10, POLICY-*, ID-*, OBS-*, WF-*, GE-01..05) are owned by that guard.

---

## Validation Checklist

### Section 1 — Domain Purity
- [ ] **Domain R1–R24 + 3 extensions** — Domain has zero external dependencies; no infrastructure imports; no engine/runtime imports; pure DDD primitives only.

### Section 2 — Classification / Context / Domain Nesting
- [ ] **DS-R1..R8** — Three-level nesting `{classification}/{context}/{domain}/` enforced under `src/domain/`.
- [ ] **CLASS-SFX-R1..R3** — Classification suffix conventions; `-system` suffix per CLASS-SFX-R1/R2 (S0 severity per CLASS-SFX, supersedes DS-R1/R2 S1 — flagged conflict, audit treats as S0).

### Section 3 — Contracts Boundary
- [ ] **G-CONTRACTS-01..06** — Shared contracts boundary purity; no domain leakage into shared kernel; cross-domain contracts placement.

### Section 4 — DTO Naming
- [ ] **DTO families (6 enforcement rules + pattern families)** — DTO naming conventions enforced; suffix correctness; no DTO impersonation.

### Section 5 — Behavioral Rules
- [ ] **Behavioral B1–B16 + B-CLOCK + B-ID + B-CHAIN** — Aggregate behavior invariants; clock/ID/chain access through abstractions only.

### Section 6 — Dependency Graph / Layer Purity
- [ ] **DG-R1..DG-R7** — Layer purity edges; enforcement matrix; code-level checks.
- [ ] **DG-R5-EXCEPT-01** — Documented exceptions only.
- [ ] **DG-R5-HOST-DOMAIN-FORBIDDEN (2026-04-08, 5 clauses)** — Host → domain references forbidden (supersedes legacy G-PLATFORM-07 permission — flagged conflict, stronger rule wins).
- [ ] **DG-R7-01 / DG-BASELINE-01 / DG-SCRIPT-HYGIENE-01 / DG-COMPOSITION-ROOT-01** — Baseline drift detection; script hygiene; composition root constraints.

### Section 7 — Domain-Aligned: Economic
- [ ] **ECON-01..03** — Top-level economic invariants.
- [ ] **T1–T5** — Token / treasury rules.
- [ ] **D41–D45 / C26–C30 / R1–R5** — Distribution, capital allocation, revenue rule families.
- [ ] **D46–D50 / C31–C35** — Settlement / clearing.
- [ ] **X1–X5 (Exposure) / D51–D55 / C36–C40** — Exposure controls. *Note: X-prefix collision with Exchange X1–X5 is intentional; disambiguate by section context per guard.*
- [ ] **RC1–RC5 / D56–D62 / C41–C47** — Reconciliation.
- [ ] **CR1–CR5 / D63–D65 / C48–C50** — Credit / risk.
- [ ] **E1–E10 / D66–D74 / C51–C60** — Earnings / events.
- [ ] **X1–X5 (Exchange) / D75–D79 / C61–C65** — Exchange controls.

### Section 8 — Domain-Aligned: Governance
- [ ] **POLICY-ENFORCEMENT-01 / POLICY-CHAIN-01 / POLICY-DETERMINISM-01** — Governance domain policy enforcement.

### Section 9 — Domain-Aligned: Identity
- [ ] **ID-POLICY-01 / ID-DETERMINISM-01** — Identity domain policy + determinism.

### Section 10 — Domain-Aligned: Observability
- [ ] **OBS-TRACE-01 / OBS-REPLAY-01** — Observability domain trace + replay.

### Section 11 — Domain-Aligned: Workflow
- [ ] **WF-PLACEMENT-01 / WF-TYPE-01 / WF-PIPELINE-01** — Workflow placement (engines), type discipline, pipeline.
- [ ] **Conflict note:** WF-PLACEMENT-01 (workflows in engines) vs DG-R2 (engines cannot reference runtime) — compatible per guard, audit confirms compatibility.

### Section 12 — WBSM v3 Global Enforcement (shared)
- [ ] **GE-01..GE-05** — Per shared block.

### Section 13 — Activation Lifecycle
- [ ] D0/D1/D2 activation levels match canonical registry; scaffolded BCs remain scaffolded; D1 BCs have minimum DDD artifacts (aggregate/event/error); D2 BCs have full DDD + engine wiring + runtime integration; no activation regressions; no premature D2 claims.

---

## Check Procedure

1. Load the domain guard rule set (~140+ rules including economic).
2. Execute per-section `Check Procedure` blocks.
3. Record verdicts with file:line evidence.
4. For domain-aligned sections, scope checks to corresponding `src/domain/{classification}/{context}/{domain}` paths.

## Pass / Fail Criteria

- **PASS:** All rules `PASS` or `N/A`.
- **FAIL:** Any S0/S1 failure.
- **CONDITIONAL:** S2/S3 captured per $1c.

## Output Format

```
AUDIT:           domain
GUARD:           claude/guards/domain.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   ~140+
SECTIONS:        13
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list>
S1_FAILURES:     <list>
EVIDENCE:        <path>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12 + $1c.
