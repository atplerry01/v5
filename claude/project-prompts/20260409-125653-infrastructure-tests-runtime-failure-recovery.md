# Phase 1.5 — §5.2.6 Runtime Failure & Recovery Validation

**CLASSIFICATION:** infrastructure-tests
**CONTEXT:** phase1.5 / §5.2.6 (runtime failure & recovery validation)
**DOMAIN:** integration-test-suite (failure-recovery)
**STORED:** 2026-04-09 12:56:53
**STATUS:** EXECUTING

---

## TITLE

Phase 1.5 §5.2.6 — Runtime Failure & Recovery Validation (FR-2 .. FR-5)

## CONTEXT

Phase 1.5 §5.2.6 ("Runtime Failure & Recovery Validation") requires proof
that the WBSM v3 system maintains correctness — no loss, no duplication,
deterministic recovery — under real failure conditions across Kafka,
Postgres, OPA, runtime workers, and the WhyceChain anchor.

FR-1 (Kafka outage → retry → DLQ → recovery) is **already canonical**
under
`tests/integration/failure-recovery/OutboxKafkaOutageRecoveryTest.cs` and
serialized via `OutboxSharedTableCollection`. This prompt extends the FR
suite with FR-2 .. FR-5.

Path / scope deviation from the spec (decided 2026-04-09 with explicit
user approval before code was written):

- **Path**: tests live under `tests/integration/failure-recovery/`
  (NOT a new `tests/integration/runtime/failure/` tree). Rationale:
  the existing FR suite + `OutboxSharedTableCollection` enforce
  cross-test serialization on the shared `outbox` table; a parallel
  tree would split the suite across two namespaces and reintroduce
  the parallel-corruption risk MI-2 / FR-1 already closed.
- **Skipped file**: `KafkaFailureRecoveryTests.cs` is intentionally not
  created — FR-1 already exhaustively covers scenario 1.1.
- **Net new files**: 4 (FR-2 .. FR-5) instead of 5.

## OBJECTIVE

Prove, via integration tests against real infrastructure (Postgres) and
deterministic in-process substitutes (Kafka, OPA, Chain), that:

1. **FR-2 Postgres failure** — tx rollback on connection drop produces
   no partial writes; idempotent re-enqueue via `idempotency_key` is
   safe; multi-row batches are all-or-nothing.
2. **FR-3 Runtime crash** — `SELECT ... FOR UPDATE SKIP LOCKED` row
   locks release on tx dispose; survivor reprocesses; no stuck locks;
   distinct from MI-2 by exercising multi-row claim release.
3. **FR-4 Policy engine failure** — `IPolicyEvaluator` throwing
   `PolicyEvaluationUnavailableException` is fail-closed: bubbles
   through `PolicyMiddleware` unmodified, no event persisted, no
   chain anchor, no outbox enqueue.
4. **FR-5 Chain failure** — `IChainAnchor` throwing causes the event
   store append (Step 2 of `EventFabric`) to commit, but the chain
   anchor (Step 3) throws and the outbox enqueue (Step 4) NEVER runs
   — event is in source-of-truth but invisible to consumers, exactly
   as designed for replay-recovery.

## CONSTRAINTS

- **No architectural changes** ($5). The only additive surface change
  is two optional parameters on `TestHost.ForTodo` for stub injection
  (`IPolicyEvaluator?`, `IChainAnchor?`). Defaults preserve all
  pre-existing call sites byte-identically.
- **No production failure-injection seams**. Tests use NSubstitute /
  hand-rolled stubs at the canonical interface boundary.
- **Postgres gating** identical to MI-2 / FR-1: silent skip when
  `Postgres__TestConnectionString` is unset.
- **No real Kafka required** — producer interactions in FR-4/FR-5 are
  observed via `InMemoryOutbox.Batches` (or its absence).
- **Determinism ($9 / R-RT-06)** — all in-test substitutes consume
  `IClock` + `IIdGenerator` per **T-DOUBLES-01**. `Guid.NewGuid()` is
  permitted only as a per-test correlation isolation tag, mirroring
  the MI-2 / FR-1 precedent (and so noted in test comments).
- **Outbox table sharing** — every new test class is decorated with
  `[Collection(OutboxSharedTableCollection.Name)]` so it serializes
  against MI-2 + FR-1 on the shared `outbox` table.
- **Namespace** — `Whycespace.Tests.Integration.FailureRecovery`.

## EXECUTION STEPS

1. Save this prompt under `claude/project-prompts/` per $2.
2. Add additive optional parameters to `TestHost.ForTodo` so FR-4 and
   FR-5 can inject throwing stubs without restructuring the fixture.
3. Implement FR-2 (`PostgresFailureRecoveryTest.cs`) against real
   Postgres outbox: rollback purity + multi-row atomicity + idempotent
   re-enqueue.
4. Implement FR-3 (`RuntimeCrashRecoveryTest.cs`) against real Postgres
   outbox: multi-row claim → crash → all rows release → survivor
   drains the entire claim set without duplicates.
5. Implement FR-4 (`PolicyEngineFailureTest.cs`) against `TestHost`
   with a stub `IPolicyEvaluator` that throws
   `PolicyEvaluationUnavailableException`. Assert fail-closed: no
   events, no chain blocks, no outbox batches for the correlation_id.
6. Implement FR-5 (`ChainFailureTest.cs`) against `TestHost` with a
   stub `IChainAnchor` that throws. Assert: event store HAS the
   events (source of truth), chain blocks empty, outbox batches empty
   (Step 4 never ran).
7. Build `tests/integration/Whycespace.Tests.Integration.csproj` to
   prove T-BUILD-01 compliance.
8. Write the §5.2.6 evidence index under
   `claude/audits/phase1.5/20260409-125653-fr-evidence.md` enumerating
   FR-1 .. FR-5 status.
9. Run post-execution audits per $1b.

## OUTPUT FORMAT

- 1 prompt file (this file).
- 1 additive `TestHost.cs` edit (two optional parameters).
- 4 new test files under `tests/integration/failure-recovery/`.
- 1 evidence index under `claude/audits/phase1.5/`.
- Build green on `tests/integration/Whycespace.Tests.Integration.csproj`.

## VALIDATION CRITERIA

- Build passes (T-BUILD-01).
- All four new test classes carry `[Collection(OutboxSharedTableCollection.Name)]`.
- All four test classes silent-skip when Postgres is unavailable for
  the tests that touch real Postgres (FR-2 / FR-3); FR-4 / FR-5 do not
  require Postgres.
- No new production code is added; the only `src/`-side or `tests/setup/`-side
  edit is the additive `TestHost.ForTodo` parameter list.
- No new metric names introduced; observability §4 of the spec is
  satisfied by inspecting existing `Whyce.Outbox` / `Whyce.Postgres` /
  `Whyce.Policy` / `Whyce.Chain` meters via the documented field names.
- The §5.2.6 evidence index lists FR-1 .. FR-5 with file paths and
  status (CANONICAL / NEW) for each.
