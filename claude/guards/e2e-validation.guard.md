# GUARD: e2e-validation

**Classification:** validation
**Scope:** Phase 1.5 certification gate. Loaded before any prompt that claims to validate, certify, or smoke-test the Whycespace system end-to-end.

## RULES

### G-E2E-001 — No PASS without execution evidence
Any test entry in `/docs/validation/*.md` marked `STATUS: PASS` MUST carry an `EVIDENCE:` block containing at minimum: command run, exit code, captured event_id(s), kafka offset, and timestamp of execution. Missing evidence = `STATUS: FAIL — NOT EXECUTED`.

### G-E2E-002 — Layer coverage is mandatory
Every E2E test MUST exercise: API → Runtime → Engine → Event Store → Kafka → Projection → Read API. Skipping any layer = S1 violation. Single-layer unit assertions are NOT E2E.

### G-E2E-003 — Determinism in fixtures
Test fixtures MUST NOT embed `Guid.NewGuid()`, `DateTime.UtcNow`, `new Random()`, or wall-clock-derived IDs. All IDs derived via `IDeterministicIdGenerator`; all clocks via `IClock`. Violations = S1 ($9).

### G-E2E-004 — Policy decision required
Every command-side test MUST capture `policy.decision`, `policy.decision_hash`, `policy.version`. Absence = S0 ($8 — no operation may bypass WHYCEPOLICY).

### G-E2E-005 — Chain anchor required
Every event-emitting test MUST capture `chain.block_id` and `chain.hash`. Hash MUST be reproducible across two runs of the same fixture. Non-reproducibility = S1 ($9).

### G-E2E-006 — DLQ before commit
Failure-injection tests MUST assert that on engine/projection/consumer failure the message lands on the DLQ topic BEFORE the source offset is committed. Commit-then-DLQ = S0 (data loss risk).

### G-E2E-007 — Replay equivalence
Every aggregate touched by an E2E test MUST be replayable from the event store and produce a byte-equal projection state. Divergence = S1.

### G-E2E-008 — No test self-cleanup that hides failures
Tests MUST NOT delete event-store rows, kafka topics, or projection state on failure. Cleanup runs ONLY on PASS, after evidence capture.

### G-E2E-009 — Severity ladder
Failures classified per source prompt §14: CRITICAL (blocks Phase 1.5) / HIGH / MEDIUM / LOW. CRITICAL is reserved for: data loss, policy bypass, chain break, replay divergence, DLQ-after-commit.

### G-E2E-011 — Static checks are STAGE 0
`scripts/validation/run-e2e.sh` MUST invoke every `scripts/*-check.sh` as STAGE 0 before any HTTP/Kafka call. Any non-zero exit aborts the run with status `FAIL — STAGE 0`. Rationale: cheap signal catches config and dependency bugs before expensive live execution.

### G-E2E-010 — Untested = FAIL
Per source §15, any case the harness cannot execute (missing service, missing fixture, environmental gap) is recorded as `FAIL — NOT EXECUTED` with `severity: CRITICAL` if it sits on the gate path. Silent skips are forbidden.

## INTEGRATION
- Loaded by `$1a` pre-execution stage for any prompt classified `validation` or `phase1.5-gate`.
- Audited by `/claude/audits/e2e-validation.audit.md`.
