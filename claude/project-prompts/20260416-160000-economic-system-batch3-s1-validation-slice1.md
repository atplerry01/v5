# Batch 3 (S1 Validation) — Slice 1: E2E expansion + invariant unit tests

## TITLE
Economic-System Batch 3 S1 Validation — Slice 1 of N

## CONTEXT
Batch 3 asked for unit tests across 104 aggregates, E2E coverage across 18 bounded contexts, and failure-path tests (settlement compensation, idempotency/replay). Real codebase inventory shows 434 aggregates total, 7 unit-tested, 3 of 12 economic contexts with E2E (capital, exchange, compliance/audit), and 6 infra-level failure-recovery tests with no domain-level failure-path coverage.

Full batch execution is multi-day work; a single-pass attempt would produce shallow tests that fail the DoD "reject if tests only cover happy paths" criterion. This slice delivers high-quality coverage for a bounded scope. Compensation tests are explicitly deferred pending Batch 2 saga.

## OBJECTIVE
Deliver meaningful validation coverage for four additional economic contexts with lifecycle + invariant + failure paths, plus a failure-path idempotency suite for the compliance/audit context, plus deep unit coverage for the Journal aggregate (double-entry invariant).

## CONSTRAINTS
- Real infrastructure only (API host, Postgres projection store, Kafka); no mocks or stubs.
- Match the per-domain E2E convention established by capital / exchange / compliance setups.
- Zero tolerance for happy-path-only tests — every test class must include invariant enforcement and/or failure-path coverage.
- Determinism: every aggregate id derived via `TestIdGenerator` (SHA-256 of `RunId:seed`); clock exception documented per E12 pattern.

## EXECUTION STEPS
1. E2E suite for `risk/exposure` — 5 setup files + lifecycle/invariant/failure tests.
2. E2E suite for `enforcement/rule` — 5 setup files + lifecycle/invariant/failure tests.
3. E2E suite for `reconciliation/process` — 5 setup files + matched/mismatched/terminal/failure tests (projection-only verification, no GET endpoint).
4. E2E failure-path suite for `compliance/audit` — idempotency + terminal-state replay tests.
5. Unit tests for `JournalAggregate` — 13 tests covering create, add-entry invariants, post with balance invariant, replay.
6. Build all test projects; run Journal unit tests for green signal.

## OUTPUT FORMAT
- Per suite: files, tests, invariants exercised.
- Coverage delta (before vs after).
- Explicit list of slices deferred to Batch 3 Slices 2-N.

## VALIDATION CRITERIA
- All test projects compile with 0 errors.
- Journal unit tests pass (13/13).
- New files carry no determinism block-list hits.
- Every new test class includes at least one invariant-enforcement or failure-path test.
