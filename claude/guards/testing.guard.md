# Testing Guard (Canonical)

## Purpose

Enforce test architecture rules across the WBSM v3 system. Tests must mirror the domain structure, respect layer boundaries, cover the full execution pipeline, and ensure that simulation capabilities exist for policy and chain validation. Tests are a constitutional enforcement layer — not optional, not advisory.

This canonical guard also serves as the Phase 1.5 certification gate governing E2E validation. It is loaded before any prompt that claims to validate, certify, or smoke-test the Whycespace system end-to-end.

## Scope

All test files across `tests/`. Applies to unit tests, integration tests, end-to-end tests, and simulation tests. Evaluated at CI, code review, and governance audit.

**Classification (E2E section):** validation
**Scope (E2E section):** Phase 1.5 certification gate. Loaded before any prompt that claims to validate, certify, or smoke-test the Whycespace system end-to-end.

## Source consolidation

This canonical guard merges the following predecessor guard files verbatim / near-verbatim:
- `tests.guard.md` — test structure, conventions, and WBSM v3 global enforcement baseline.
- `e2e-validation.guard.md` — Phase 1.5 E2E certification gate rules (G-E2E-*).

All rules from both sources are preserved. Deduplication was applied only to exact overlaps (notably the `G-E2E-010 — Untested = FAIL` rule, which appeared in both files — preserved once, with both source references captured).

---

## Rules

### Section: Test Structure & Conventions

#### R1 — UNIT TESTS MUST MIRROR DOMAIN
The unit test folder structure must mirror `src/domain/` exactly. For each bounded context at `src/domain/{system}/{context}/{domain}/`, a corresponding test folder must exist at `tests/unit/{system}/{context}/{domain}/`. Each aggregate, entity, value object, service, and specification must have a corresponding test class following the `{ClassName}Tests` naming pattern.

**Source:** tests.guard.md (Rule 1)
**Severity:** S1 (missing folder) / S2 (missing class) / S3 (naming drift)

#### R2 — INTEGRATION TESTS MUST USE RUNTIME
Integration tests must exercise the full runtime pipeline: command dispatch → middleware → engine → domain → event → projection. Integration tests must not call domain aggregates or engines directly. They must dispatch commands through the runtime command bus and verify results through the query/projection path. This validates the complete execution flow.

**Source:** tests.guard.md (Rule 2)
**Severity:** S0

#### R3 — E2E TESTS MUST COVER FULL PIPELINE
End-to-end tests must cover the complete pipeline from platform entry point to persistence and projection. E2E tests verify: API request → platform → systems → runtime → engine → domain → event store → outbox → Kafka → projection. E2E tests use real infrastructure (or containerized equivalents) — no mocks at this level.

**Source:** tests.guard.md (Rule 3)
**Severity:** S2 (missing stages)

#### R4 — NO INFRASTRUCTURE IN UNIT TESTS
Unit tests must not reference any infrastructure type: no `DbContext`, no `HttpClient`, no Kafka types, no file system calls, no external service clients. Unit tests operate on pure domain logic with in-memory fakes or stubs for repository interfaces. Any infrastructure import in a unit test is a structural violation.

**Source:** tests.guard.md (Rule 4)
**Severity:** S1

#### R5 — SIMULATION TESTS MUST EXIST
Policy simulation tests and chain verification tests must exist. Simulation tests validate:
- Policy evaluation produces correct `DecisionHash` for known inputs
- Chain anchoring produces valid `ChainBlock` entries
- Policy simulation mode correctly reports what would happen without enforcing
- Event replay produces consistent aggregate state

Systems without simulation tests cannot pass governance audit.

**Source:** tests.guard.md (Rule 5)
**Severity:** S0

#### R6 — T-BUILD-01: tests/integration/ MUST compile on every CI run
A red integration project halts merge. The CI gate fails when `dotnet build tests/integration/Whycespace.Tests.Integration.csproj` fails.

**Source:** tests.guard.md (NEW RULES INTEGRATED — 2026-04-07)
**Severity:** S0 (CI gate)

#### R7 — T-DOUBLES-01: In-memory test doubles must take IClock and IIdGenerator
All in-memory test doubles (`InMemoryChainAnchor`, `InMemoryOutbox`, `InMemoryEventStore`, etc.) MUST take `IClock` and `IIdGenerator` constructor parameters. No `Guid.NewGuid()` / `DateTimeOffset.UtcNow` inside test doubles.

**Source:** tests.guard.md (NEW RULES INTEGRATED — 2026-04-07)
**Severity:** S1

#### R8 — T-PLACEHOLDER-01: In-memory repositories used in production must be marked + migration-ready
In-memory repository implementations used in production composition MUST be clearly marked as placeholders AND have a corresponding migration script in `scripts/migrations/` ready for swap.

**Source:** tests.guard.md (NEW RULES INTEGRATED — 2026-04-07)
**Severity:** S1

#### R9 — T1M-RESUME-TEST-COVERAGE-01: workflow resume harness required
A `T1MWorkflowHarness` test fixture MUST exist under `tests/integration/orchestration-system/workflow/` wiring `T1MWorkflowEngine`, `WorkflowStepExecutor`, in-memory `IWorkflowRegistry`, and a real `IEventStore`. Required scenarios: resume midway (cursor = 2 of 4 → executes steps 2,3); resume completed → fails with "not in resumable state"; resume after failure → re-runs the failed step per chosen policy. `NoOpWorkflowEngine` stubs do not satisfy this rule.

**Source:** tests.guard.md (NEW RULES INTEGRATED — 2026-04-07 workflow resume test coverage; `claude/new-rules/_archives/20260407-230000-workflow-resume-payload-and-test-coverage.md`)
**Severity:** S2

#### R10 — ACT-FABRIC-ROUNDTRIP-TEST-01: fabric round-trip coverage for every registered event
Every domain event whose schema is registered with `IEventSchemaRegistry` MUST have at least one integration test that constructs the event, persists it via the real `IEventFabric` (not a double), reloads it from the real event store, and asserts payload integrity round-trip. Tests that bypass the real fabric MUST be tagged `[Trait("Fabric","Bypass")]`. CI gate enumerates registered events and fails the build on any uncovered registration. Rationale: prevents the `WorkflowExecutionResumedEvent` class of drift where in-memory doubles green-light a path the real fabric does not know about.

**Source:** tests.guard.md (NEW RULES INTEGRATED — 2026-04-10; `_archives/20260408-103326-activation.md`)
**Severity:** S0

#### R11 — T-POLICY-001: command-path fixtures must assert decision_hash and policy.version
Test fixtures touching the command path MUST assert non-null `decision_hash` and `policy.version` on the resulting event/command-result. Production code is covered by `policy-binding.guard.md`; this closes the test-side gap.

**Source:** tests.guard.md (NEW RULES INTEGRATED — 2026-04-10; `_archives/20260408-142631-validation.md` Finding 3)
**Severity:** S1

#### R12 — T-BUILD-01 STRENGTHENING: interface changes and test doubles move together
A commit that changes a production interface signature (constructor args or interface members) MUST update the corresponding test doubles in `tests/integration/setup/` in the SAME commit. Orphaned test doubles are an S1 architectural violation. The executable enforcement is `claude/audits/tests-integration-build.audit.md`.

**Source:** tests.guard.md (NEW RULES INTEGRATED — 2026-04-10; `_archives/20260409-120500-infrastructure-tests-integration-baseline-drift.md`)
**Severity:** S1

---

### Section: E2E Validation

#### G-E2E-001 — No PASS without execution evidence
Any test entry in `/docs/validation/*.md` marked `STATUS: PASS` MUST carry an `EVIDENCE:` block containing at minimum: command run, exit code, captured event_id(s), kafka offset, and timestamp of execution. Missing evidence = `STATUS: FAIL — NOT EXECUTED`.

**Source:** e2e-validation.guard.md
**Severity:** gate rule (see G-E2E-009 severity ladder)

#### G-E2E-002 — Layer coverage is mandatory
Every E2E test MUST exercise: API → Runtime → Engine → Event Store → Kafka → Projection → Read API. Skipping any layer = S1 violation. Single-layer unit assertions are NOT E2E.

**Source:** e2e-validation.guard.md
**Severity:** S1

#### G-E2E-003 — Determinism in fixtures
Test fixtures MUST NOT embed `Guid.NewGuid()`, `DateTime.UtcNow`, `new Random()`, or wall-clock-derived IDs. All IDs derived via `IDeterministicIdGenerator`; all clocks via `IClock`. Violations = S1 ($9).

**Source:** e2e-validation.guard.md
**Severity:** S1

#### G-E2E-004 — Policy decision required
Every command-side test MUST capture `policy.decision`, `policy.decision_hash`, `policy.version`. Absence = S0 ($8 — no operation may bypass WHYCEPOLICY).

**Source:** e2e-validation.guard.md
**Severity:** S0

#### G-E2E-005 — Chain anchor required
Every event-emitting test MUST capture `chain.block_id` and `chain.hash`. Hash MUST be reproducible across two runs of the same fixture. Non-reproducibility = S1 ($9).

**Source:** e2e-validation.guard.md
**Severity:** S1

#### G-E2E-006 — DLQ before commit
Failure-injection tests MUST assert that on engine/projection/consumer failure the message lands on the DLQ topic BEFORE the source offset is committed. Commit-then-DLQ = S0 (data loss risk).

**Source:** e2e-validation.guard.md
**Severity:** S0

#### G-E2E-007 — Replay equivalence
Every aggregate touched by an E2E test MUST be replayable from the event store and produce a byte-equal projection state. Divergence = S1.

**Source:** e2e-validation.guard.md
**Severity:** S1

#### G-E2E-008 — No test self-cleanup that hides failures
Tests MUST NOT delete event-store rows, kafka topics, or projection state on failure. Cleanup runs ONLY on PASS, after evidence capture.

**Source:** e2e-validation.guard.md
**Severity:** gate rule

#### G-E2E-009 — Severity ladder
Failures classified per source prompt §14: CRITICAL (blocks Phase 1.5) / HIGH / MEDIUM / LOW. CRITICAL is reserved for: data loss, policy bypass, chain break, replay divergence, DLQ-after-commit.

**Source:** e2e-validation.guard.md
**Severity:** meta-rule (classification ladder)

#### G-E2E-010 — Untested = FAIL
Validation, audit, and e2e prompts MUST treat any test case that is unreachable, un-runnable, or skipped as a FAILURE, never a SKIP. Reports listing SKIP in place of executable evidence are non-compliant. Applies cross-cutting to all test/validation/audit prompts.

Per source §15, any case the harness cannot execute (missing service, missing fixture, environmental gap) is recorded as `FAIL — NOT EXECUTED` with `severity: CRITICAL` if it sits on the gate path. Silent skips are forbidden.

**Source:** e2e-validation.guard.md AND tests.guard.md (NEW RULES INTEGRATED — 2026-04-10; `_archives/20260408-142631-validation.md` Finding 1) — exact duplicate; consolidated into single entry.
**Severity:** S1 (per tests.guard.md promotion); CRITICAL on gate path (per e2e-validation.guard.md)

#### G-E2E-011 — Static checks are STAGE 0
`scripts/validation/run-e2e.sh` MUST invoke every `scripts/*-check.sh` as STAGE 0 before any HTTP/Kafka call. Any non-zero exit aborts the run with status `FAIL — STAGE 0`. Rationale: cheap signal catches config and dependency bugs before expensive live execution.

**Source:** e2e-validation.guard.md
**Severity:** gate rule

---

### Section: WBSM v3 Global Enforcement

#### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

**Source:** tests.guard.md (WBSM v3 GLOBAL ENFORCEMENT)
**Severity:** S1 ($9)

#### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

**Source:** tests.guard.md (WBSM v3 GLOBAL ENFORCEMENT)
**Severity:** S0 ($8)

#### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

**Source:** tests.guard.md (WBSM v3 GLOBAL ENFORCEMENT)
**Severity:** S1

#### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

**Source:** tests.guard.md (WBSM v3 GLOBAL ENFORCEMENT)
**Severity:** S1 ($10)

#### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

**Source:** tests.guard.md (WBSM v3 GLOBAL ENFORCEMENT)
**Severity:** S1

---

## Check Procedure

1. Enumerate all BCs at D2 activation level and verify corresponding test folders exist in `tests/unit/`.
2. Verify each D2 aggregate has a corresponding `{Aggregate}Tests` class.
3. Scan integration test files for direct aggregate or engine instantiation (must use runtime pipeline).
4. Verify integration tests dispatch commands through `ICommandBus` or runtime mediator.
5. Verify E2E tests exercise the full platform-to-projection pipeline.
6. Scan unit test files for infrastructure imports (`DbContext`, `HttpClient`, `Confluent.Kafka`, etc.).
7. Verify simulation test files exist for policy evaluation and chain anchoring.
8. Verify simulation tests cover: DecisionHash generation, ChainBlock creation, policy simulation mode, event replay consistency.
9. Verify test naming follows `{ClassName}Tests` pattern.
10. Verify test folder structure mirrors domain structure.

**Source:** tests.guard.md

## Pass Criteria

- All D2-level BCs have mirrored unit test folders.
- All aggregates, services, and specifications at D2 have test classes.
- Integration tests use runtime pipeline exclusively.
- E2E tests cover full pipeline.
- Zero infrastructure imports in unit tests.
- Simulation tests exist for policy and chain.
- Test folder structure mirrors domain structure.

**Source:** tests.guard.md

## Fail Criteria

- D2-level BC without corresponding test folder.
- Aggregate without test class.
- Integration test calling domain/engine directly (bypassing runtime).
- E2E test missing pipeline stages.
- Infrastructure import in unit test.
- Missing simulation tests for policy or chain.
- Test folder structure does not mirror domain.

**Source:** tests.guard.md

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Integration test bypasses runtime | Test calls `aggregate.Apply()` directly instead of dispatching command |
| **S0 — CRITICAL** | Missing simulation tests | No policy simulation or chain verification tests exist |
| **S1 — HIGH** | Infrastructure in unit test | `using Microsoft.EntityFrameworkCore;` in unit test |
| **S1 — HIGH** | D2 BC without test folder | `economic-system/capital/vault/` has no test mirror |
| **S2 — MEDIUM** | Missing aggregate test | `VaultAggregate` has no `VaultAggregateTests` class |
| **S2 — MEDIUM** | E2E test missing stages | E2E test skips outbox/Kafka stage |
| **S3 — LOW** | Test naming violation | `VaultTest` instead of `VaultAggregateTests` |
| **S3 — LOW** | Test folder structure drift | Test folder doesn't match domain folder hierarchy |

**Source:** tests.guard.md

## Enforcement Action

- **S0**: Block merge. Fail CI. Immediate remediation required.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
TESTS_GUARD_VIOLATION:
  test_type: <unit|integration|e2e|simulation>
  bc: <classification/context/domain>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct test structure>
  actual: <detected issue>
  remediation: <fix instruction>
```

**Source:** tests.guard.md

---

## Integration

- Loaded by `$1a` pre-execution stage for any prompt classified `validation`, `phase1.5-gate`, or any prompt touching `tests/**`.
- Audited by `/claude/audits/e2e-validation.audit.md` and `/claude/audits/tests-integration-build.audit.md`.

**Source:** e2e-validation.guard.md (INTEGRATION) — extended with tests.guard.md audit references.
