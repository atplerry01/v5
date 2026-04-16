---
classification: economic-system
context: exchange
domain_group: exchange
domains: [fx, rate]
type: infrastructure-validation
phases: [1, 2, 3, 4, 5, 6, 7, 8]
phase_9_deferred: true
stored_at_utc: 2026-04-16T11:11:45Z
---

# TITLE

Infrastructure Validation — Real Execution (Docker / Kafka / Postgres / OPA / Full Pipeline)
Scope: economic-system / exchange / exchange / { fx, rate }

# CONTEXT

User invoked the canonical "Infrastructure Validation — Real Execution" prompt
against the full **exchange** context of the **economic-system** classification.

Target: every domain under `src/domain/economic-system/exchange/**`.
Discovered domains: **fx**, **rate** (two aggregates: `FxAggregate`, `ExchangeRateAggregate`).

Reuse constraint: infrastructure is already running (docker containers up for 13+ hours,
all healthy — kafka, postgres, postgres-projections, redis, opa, whycechain-db, minio).
Do NOT destroy volumes, restart services, or delete Kafka topics.

# OBJECTIVE

Validate, against real infrastructure (no mocks, no in-memory substitutes), that the
full system pipeline executes correctly for every domain under the exchange context:

  API → Dispatcher → Runtime → Policy → Engine → Event Store → Outbox → Kafka
      → Projection → API

The validation must prove determinism, idempotency, policy enforcement, and
projection correctness for both `fx` and `rate`.

# CONSTRAINTS

- Run Phases 1–8 only. Phase 9 (failure injection) is DEFERRED to a separate pass.
- No mocks or in-memory substitutes anywhere in the pipeline.
- Canonical topics: `whyce.economic.exchange.{domain}.{commands|events|retry|deadletter}`.
- Must verify: persist → chain → outbox → Kafka → projection.
- No destructive resets of Postgres / Kafka / Redis state.
- Reuse existing containers if already running; never recreate.
- Hard rule: if either `fx` or `rate` is not fully E2E executable, overall status = FAIL.
- All execution subject to CLAUDE.md $1 / $1a / $1b / $1c (guard loading, audit sweep, drift capture).

# EXECUTION STEPS

Phase 1 — Infrastructure bootstrap (reuse; health probes only).
Phase 2 — Topic + schema validation (fx, rate).
Phase 3 — OPA policy validation (allow + deny).
Phase 4 — Event persistence validation (events table integrity).
Phase 5 — Outbox + Kafka validation (headers, topic routing).
Phase 6 — Projection validation (idempotent apply, state correctness).
Phase 7 — API read validation (projection-sourced reads only).
Phase 8 — Determinism + replay validation (re-execution byte equality).

Each phase: evidence-captured pass/fail. No silent passes. Any S0/S1 guard
violation halts the run per CLAUDE.md $12.

# OUTPUT FORMAT

Final structured report:
  1. Per-phase results (1–8)
  2. E2E trace for at least one domain path
  3. Kafka topic validation
  4. Projection validation
  5. Determinism + replay validation
  6. Audit sweep results
  7. FINAL STATUS: APPROVED | CONDITIONAL PASS | FAIL

# VALIDATION CRITERIA

- APPROVED — every phase passes for both `fx` and `rate`.
- CONDITIONAL PASS — infrastructure blocker only (e.g., host cannot start due to
  missing env vars) but architectural wiring is sound.
- FAIL — any phase failure for either domain, any guard violation, any silent
  pass, or any domain not fully E2E executable.

# PHASE 9 DEFERRAL

Phase 9 (failure injection — Kafka downtime, Postgres restart, partial execution)
is explicitly deferred to a separate pass after this baseline is clean.
