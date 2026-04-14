# Runtime Audit (Canonical)

**Validates:** [`claude/guards/runtime.guard.md`](../../guards/runtime.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the runtime layer (execution + enforcement) complies with all rules consolidated in the canonical runtime guard: runtime order, composition loading, engine purity, projections, determinism (IClock, deterministic IDs, hash determinism), replay determinism, and Phase 1.5 runtime hardening.

## Scope

- `src/runtime/**` — runtime execution support, internal projection routing/orchestration
- `src/engines/**` — engine layer (stateless, events only)
- `src/projections/**` — CQRS read models
- Composition root (`src/platform/host/`) — composition loader and program composition
- All event emission sites
- All ID generation, time access, and hashing call sites

## Source guard

This audit checks the rules defined in [`claude/guards/runtime.guard.md`](../../guards/runtime.guard.md). Rule families (R*, RO-*, R-RT-*, E*, P*, G-COMPLOAD-*, G-PROGCOMP-*, REPLAY-*, DET-*, G1–G20, HASH-*, GE-01..05, plus integrated new-rules R-CTX-*, R-ORDER-*, R-UOW-*, R-WORKFLOW-PIPE-*, R-DOM-LEAK-*, R-POLICY-PATH-*, R-WF-*, etc.) are owned by that guard.

---

## Validation Checklist

### Section 1 — Runtime Order & Lifecycle
- [ ] **R1–R15 + RO-LOCKED-ORDER + RO-CANONICAL-11** — Pipeline stages execute in the locked canonical order; no out-of-order dispatch; lifecycle factories invoked at correct call sites (E-LIFECYCLE-FACTORY-*).
- [ ] **R-CTX-01 / R-ORDER-01 / R-UOW-01 / R-WORKFLOW-PIPE-01** — Context propagation, ordering invariants, unit-of-work boundaries, and workflow pipeline sequencing intact.

### Section 2 — Composition & Loading
- [ ] **G-COMPLOAD-01..07** — Composition loader rules (registration, idempotency, scoping).
- [ ] **G-PROGCOMP-01..05** — Program composition root constraints.

### Section 3 — Engine Purity
- [ ] **Engine R1–R16 + ENG-PURITY-01 + ENG-DOMAIN-ALIGN-01** — Engines are stateless, events-only, no persistence, no I/O.
- [ ] **E-WORKFLOW-01 / E-STEP-01,02 / E-VERSION-01 / E-RESUME-01..03 / E-STATE-01..03 / E-TYPE-01..03** — Engine workflow, step, version, resume, state, and type constraints.
- [ ] **E7 amendment (2026-04-13)** — No cross-tier engine imports; same-tier permitted per amendment.

### Section 4 — Projections (CQRS read models)
- [ ] **Projection R1–R32 + P-TYPE-ALIGN-01 + P-AGNOSTIC-01 + PROJ-READ-ONLY-01 + PROJ-DOMAIN-ALIGN-01 + PROJ-WF-EXEC-01 + PROJ-REPLAY-SAFE-01 + PROJ-NO-INPLACE-MUTATION-01** — Projections read-only, domain-aligned, replay-safe, no in-place mutation.

### Section 5 — Determinism Core
- [ ] **DET-BLOCKLIST + DET-REQUIRED-REPLACEMENTS + DET-EXCEPTIONS** — No `Guid.NewGuid`, no `DateTime.Now`/`Utc.Now`, no `Random()` in non-exempt paths.
- [ ] **DET-ADAPTER-01 / DET-EXCEPTION-01 / DET-SEED-01 / DET-DUAL-SEAM-01 / DET-HSID-CALLSITE-01 / DET-SEED-DERIVATION-01 / DET-IDCHECK-COVERAGE-01 / DET-STOPWATCH-OBSERVABILITY-01 / DET-SQL-NOW-ADDENDUM-01** — Adapter, exception, seed derivation, dual-seam, HSID call sites, stopwatch observability, SQL `now()` addendum.

### Section 6 — Deterministic IDs
- [ ] **G1–G8 + G12–G20** — Deterministic ID generation rules (factory usage, namespace correctness, no Guid.NewGuid, idempotent ID derivation).

### Section 7 — Hash Determinism
- [ ] **HASH-PERMITTED-INPUTS / HASH-FORBIDDEN-INPUTS / HASH-REQUIRED-PATTERNS / HASH-CURRENT-COMPLIANCE** — SHA256 only, deterministic input shapes, no time-varying inputs.

### Section 8 — Replay Determinism
- [ ] **REPLAY-SENTINEL-PROTECTED-01 / REPLAY-SENTINEL-LIFT-01 / REPLAY-A-vs-B-DISTINCTION-01 / POLICY-REPLAY-INTEGRITY-01** — Replay sentinels protected; A/B replay distinction preserved; policy replay integrity intact.

### Section 9 — Phase 1.5 Runtime Rules
- [ ] **R-RT-01..R-RT-10** — Phase 1.5 runtime invariants (incl. R-RT-06 IClock + IIdGenerator with MI-1 owner-token shape exemption).

### Section 10 — Integrated New-Rules
- [ ] **R-DOM-LEAK-01 / R-POLICY-PATH-01 / R-WF-EVENTIFIED-01 / R-WF-RESUME-01 / R-POLICY-FIRST-01 / R-CANONICAL-PIPELINE-01 / POLICY-PIPELINE-INTEGRATION-01 / R-WF-PAYLOAD-01 / R-WF-PAYLOAD-TYPED-01 / R-EVENT-AUDIT-COLS-01 / R-CHAIN-CORRELATION-01** — All integrated additions verified.
- [ ] **R-WF-OBSERVER-01 (REVOKED 2026-04-07)** — Confirm not active; superseded by R-WF-EVENTIFIED-01.

### Section 11 — WBSM v3 Global Enforcement (shared)
- [ ] **GE-01..GE-05** — Deterministic execution, WHYCEPOLICY enforcement, WHYCECHAIN anchoring, event-first architecture, CQRS enforcement.

---

## Check Procedure

1. Load the runtime guard rule set (~185 rules).
2. For each section, execute the per-section `Consolidated Check Procedure` declared in the guard.
3. Record a verdict per rule: `PASS` / `FAIL` / `N/A` with file:line evidence.
4. Aggregate by section and overall.

## Pass / Fail Criteria

- **PASS:** All rules `PASS` or `N/A`.
- **FAIL:** Any S0 or S1 failure.
- **CONDITIONAL:** S2/S3 failures captured to `claude/new-rules/` per CLAUDE.md $1c.

## Output Format

```
AUDIT:           runtime
GUARD:           claude/guards/runtime.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   ~185
SECTIONS:        11
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
