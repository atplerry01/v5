---
CLASSIFICATION: core-system
SOURCE: User confirmation 2026-04-22 following E1→EX audit pass
DESCRIPTION: core-system must be treated as a Non-Executable System for E1→EX stage mapping
PROPOSED_RULE: CORE-SYS-TIER-01
SEVERITY: S2 (advisory — prevents incorrect stage expectations, not a code correctness rule)
---

# CORE-SYS-TIER-01 — core-system is a Non-Executable System

## Finding

core-system (temporal, ordering, identifier contexts) is a shared kernel / foundational classification.
It owns only immutable value-object primitives with no commands, events, engines, policies, or workflows.

The E1→EX delivery pattern must be applied with the following stage mapping:

| Stage | Status for core-system |
|---|---|
| E1 (domain layer) | ✅ Required |
| E2 (commands) | N/A — no commands by design (topic 6) |
| E3 (queries) | N/A — read is direct VO access |
| E4 (engines) | N/A — no domain engines (topic 12) |
| E5 (policy) | N/A — no policy model (topic 10) |
| E6 (events) | N/A — no events by design (topic 6) |
| E7 (serialization) | ✅ Required (proven via E12 test suite) |
| E8 (projections) | N/A — no mutable state, no projections |
| E9 (runtime) | N/A — no runtime integration layer |
| E10 (API) | N/A — no controller surface |
| E11 (infra) | N/A — no Kafka topics, no schemas |
| E12 (test/E2E) | ✅ Required (expanded: replay + serialization + regression) |

## Proposed Rule

E1→EX stage mapping for core-system:
- Mandatory: E1 (structural compliance), E7 (serialization integrity), E12 (test coverage)
- Required proof: replay determinism, UTC normalization, boundary values, cross-type safety
- All other stages: explicitly N/A, must not be added

## When to Promote

Promote to `domain.guard.md` under a new Section 24 "Classification-Specific Stage Restrictions"
or into `e1-ex-domain.audit.md` as a classification table.

## Audit Accepted

User acceptance confirmed 2026-04-22: "Audit accepted. Result is correct and consistent with system rules."
Classification locked as: **Non-executable system (E1-only with extended validation)**
