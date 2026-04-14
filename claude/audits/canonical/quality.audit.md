# Quality Audit (Canonical)

**Validates:** [`claude/guards/quality.guard.md`](../../guards/quality.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the codebase complies with quality-hygiene rules consolidated in the canonical quality guard: clean-code standards, stub/empty-catch detection, dead-code elimination, structural code organization, and prompt-container hygiene.

## Scope

- `src/**` — all source for clean-code, stub-detection, dead-code, structural rules
- `claude/project-prompts/**` — prompt container hygiene
- `claude/prompts/**` — legacy prompt directory (still subject to PC rules per guard)
- All `try`/`catch` blocks (stub detection)
- All public APIs (dead-code reachability)

## Source guard

This audit checks the rules defined in [`claude/guards/quality.guard.md`](../../guards/quality.guard.md). Rule families (CCG-01..10, STUB-R1..R6, ND-P1..P5 + ND-R1..R4, STR-R1..R16 + STR-AUTH-01 + STR-HEALTH-01 + STR-OBS-01 + STR-GUARD-REGISTRY-01, PC-R1..R15 + PROMPT-RECONCILE-01, GE-01..05) are owned by that guard.

---

## Validation Checklist

### Section 1 — Clean Code Standards
- [ ] **CCG-01..10** — Clean code rules: naming, function size, single responsibility, no magic numbers, layer isolation table (CCG-06), domain purity (CCG-05), determinism (CCG-07 — note: uses `DeterministicIdHelper`/`IClock` naming, distinct from but compatible with runtime guard's `IIdGenerator`/`ITimeProvider` per documented near-duplicate).

### Section 2 — Stub Detection
- [ ] **STUB-R1..R6** — No empty catch blocks, no stub methods, no `NotImplementedException` in production paths, tracked placeholders only.
- [ ] CI enforcement check: stub-detection scan is wired into CI.

### Section 3 — Dead Code Elimination
- [ ] **ND-P1..P5** — Prohibited dead-code patterns (commented-out blocks, unreachable code, unreferenced public APIs, dead branches, orphaned files).
- [ ] **ND-R1..R4** — Enforcement rules.
- [ ] Documented exceptions only.

### Section 4 — Structural Code Organization
- [ ] **STR-R1..R16** — Structural rules: dependency direction (R1–R9), business logic placement (STR-R13 — domain only; near-duplicate of CCG-05 by intent), file organization, namespace alignment.
- [ ] **STR-AUTH-01** — Authentication structural placement.
- [ ] **STR-HEALTH-01** — Health endpoint placement.
- [ ] **STR-OBS-01** — Observability structural placement.
- [ ] **STR-GUARD-REGISTRY-01** — Guard registry structural placement.

### Section 5 — Prompt Container Hygiene
- [ ] **PC-R1..R15** — Prompts stored only in canonical `claude/project-prompts/{YYYYMMDD-HHMMSS}-{classification}-{topic}.md` per CLAUDE.md $2; no overwrites; no prompts outside canonical store; structure per $3.
- [ ] **PROMPT-RECONCILE-01** — Prompt reconciliation pre-execution.

### Section 6 — WBSM v3 Global Enforcement (shared)
- [ ] **GE-01..GE-05** — Per shared block (guard notes single instance applies to both Structural and Prompt-Container sections).

---

## Check Procedure

1. Load the quality guard rule set (~63 rules).
2. Execute per-section check operations (file scans, regex sweeps, CI configuration checks).
3. For PC-R1..R15: enumerate prompt files and verify path/naming compliance.
4. For STUB-R*: regex sweep for `catch.*\{\s*\}`, `throw new NotImplementedException`, etc.
5. For ND-*: dependency graph reachability + commented-block scan.
6. Record verdicts with file:line evidence.

## Pass / Fail Criteria

- **PASS:** All rules `PASS` or `N/A`.
- **FAIL:** Any S0/S1 failure.
- **CONDITIONAL:** S2/S3 per $1c.

## Output Format

```
AUDIT:           quality
GUARD:           claude/guards/quality.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   ~63
SECTIONS:        6
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
