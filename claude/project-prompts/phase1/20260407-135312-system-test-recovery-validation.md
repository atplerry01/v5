# WHYCESPACE — PHASE 2 TEST RECOVERY + VALIDATION PROMPT (FULL LOCK)

## CLASSIFICATION: system / validation / test-recovery

## PHASE: Phase 2 Entry Gate (FINAL LOCK)

## PRIORITY: S0 — BLOCKER

## MODE: EXECUTION + PROOF (NO THEATER)

# 🎯 OBJECTIVE

1. Repair integration test project to compile and execute
2. Implement runtime execution proof
3. Implement deterministic replay proof
4. Implement event ordering proof
5. Enforce CI gate on test compilation + execution

# 🔒 GLOBAL RULES

1. No green tests on broken build — compilation MUST be fixed FIRST
2. Tests must use REAL runtime pipeline (no mocks for RuntimeControlPlane,
   event store, chain, kafka — kafka can be test adapter)
3. Determinism must be enforced in tests (TestClock fixed time,
   deterministic IIdGenerator)
4. Tests must FAIL on ANY deviation — no soft assertions

# TG1 — INTEGRATION TEST RECOVERY

1.1 Fix compilation failure in tests/integration/Whycespace.Tests.Integration.csproj
    Issue: RuntimeCommandDispatcher constructor no longer exists.
1.2 Replace direct instantiation with IRuntimeControlPlane via DI.
1.3 Create tests/integration/setup/TestHostBuilder.cs with full DI container
    identical to src/platform/host/Program.cs (control plane, middleware
    pipeline, event store adapter, chain, kafka test adapter, IClock,
    IIdGenerator).
1.4 Create tests/shared/TestClock.cs (UtcNow = fixed constant).
1.5 Create tests/shared/TestIdGenerator.cs (deterministic).

# TG2 — EXECUTION ORDER TEST

Create tests/integration/runtime/ExecutionOrderTest.cs.
Build TestHost, execute CreateTodo, capture markers from each stage,
assert GuardPre < Policy < GuardPost < Execution < Persist < Chain < Kafka.
Implementation: middleware hooks OR tracing context OR instrumentation
events. FAIL if any stage missing or out of order.

# TG3 — DETERMINISTIC REPLAY TEST

Create tests/e2e/replay/DeterministicReplayTest.cs.
STEP 1: Execute Create+Update+Complete, capture EventIds, ExecutionHashes,
DecisionHashes, FinalState.
STEP 2: Rebuild aggregate from event store, re-run execution
deterministically.
STEP 3: Assert strict equality across all four collections.
FAIL on any mismatch, ordering change, or additional/missing event.

# TG4 — EVENT ORDERING TEST

Create tests/integration/eventstore/EventOrderingTest.cs.
Assert version increments strictly, no gaps, no duplicates, append-only
integrity.

# TG5 — TEST INFRASTRUCTURE RULES

5.1 Event store: real Postgres OR in-memory deterministic store.
5.2 Kafka: test adapter preserving ordering and partition logic.
5.3 Chain: real hash logic, in-memory storage acceptable.

# TG6 — CI GATE (MANDATORY)

6.1 Add `dotnet build tests/integration/Whycespace.Tests.Integration.csproj`
    — fail on compile error.
6.2 Add `dotnet test` — fail on any failing test.
6.3 Optional: determinism audit script.

# TG7 — FINAL VALIDATION OUTPUT

Produce execution order report, replay report, event order report.
Final status: PHASE 2 FULL LOCK: PASS / FAIL.

# 🔐 LOCK CONDITION (ABSOLUTE)

Phase 2 fully locked ONLY IF:
1. Integration tests COMPILE
2. ExecutionOrderTest PASSES
3. DeterministicReplayTest PASSES
4. EventOrderingTest PASSES
5. CI gate ENFORCES ALL ABOVE

# 🚫 FAILURE CONDITIONS

- tests do not compile
- runtime pipeline not used
- mocks bypass real execution
- replay mismatch
- ordering mismatch

# END OF PROMPT
