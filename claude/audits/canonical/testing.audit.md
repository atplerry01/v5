# Testing Audit (Canonical)

**Validates:** [`claude/guards/testing.guard.md`](../../guards/testing.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the test suite (validation + E2E correctness) complies with all rules consolidated in the canonical testing guard: test structure conventions, integration test requirements (Postgres smoke, Kafka integration, build), and E2E validation criteria.

## Scope

- `tests/**` — all unit, integration, E2E test projects
- Test fixtures, test doubles, test policies, test events
- Integration test infrastructure (Postgres, Kafka)
- CI pipeline test stages

## Source guard

This audit checks the rules defined in [`claude/guards/testing.guard.md`](../../guards/testing.guard.md). Rule families (R1–R17 from tests.guard.md core + GE block + integrated new-rules; R18–R28 from e2e-validation.guard.md as G-E2E-001..G-E2E-011) are owned by that guard.

---

## Validation Checklist

### Section 1 — Test Structure & Conventions (R1–R17 from tests.guard.md)
- [ ] **R1–R5** — Core test structure rules: project layout, naming, isolation, determinism.
- [ ] **R6–R10 (GE-01..GE-05)** — WBSM v3 global enforcement applied at test layer (deterministic execution in tests, policy capture in test events, chain anchoring in test outputs, event-first test scenarios, CQRS in test setup).
- [ ] **R11–R17** — Integrated new-rules entries: full-pipeline coverage (R3 broadly), test doubles vs fixtures (R12 vs R20 per guard — distinct layers, complementary), policy-decision capture in command-result tests (R16) vs `policy.decision` events (R21).

### Section 2 — E2E Validation (R18–R28 from e2e-validation.guard.md)
- [ ] **G-E2E-001..G-E2E-011** — E2E coverage requirements:
  - layer coverage (G-E2E-002 via R19)
  - integration test build (must compile + execute in CI)
  - Postgres smoke tests (db reachable, migrations applied, smoke queries pass)
  - Kafka integration tests (broker reachable, topics created, produce/consume round-trip)
  - **R27 (G-E2E-010 "Untested = FAIL")** — every behavior MUST have a test; severity S1 baseline, CRITICAL on gate path. *Note: deduplicated across both source guards per consolidation ledger.*

---

## Check Procedure

1. Load the testing guard rule set (28 rules).
2. Verify integration test build passes (`tests-integration-build` was a separate audit, now merged here).
3. Verify Postgres smoke tests pass (was `tests-integration-postgres-smoke`, now merged here).
4. Verify Kafka integration tests pass.
5. Verify E2E coverage matrix has no `UNTESTED` entries on gate path (R27 / G-E2E-010).
6. Record verdicts with file:line + test-name evidence.

## Pass / Fail Criteria

- **PASS:** All rules `PASS` or `N/A`; integration build passes; Postgres + Kafka smoke pass.
- **FAIL:** Any S0/S1 failure; any `UNTESTED` on gate path; integration build broken.
- **CONDITIONAL:** S2/S3 per $1c.

## Output Format

```
AUDIT:           testing
GUARD:           claude/guards/testing.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   28
SECTIONS:        2
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list>
S1_FAILURES:     <list>
INTEGRATION_BUILD:    PASS | FAIL
POSTGRES_SMOKE:       PASS | FAIL
KAFKA_INTEGRATION:    PASS | FAIL
EVIDENCE:        <path>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12 + $1c.
