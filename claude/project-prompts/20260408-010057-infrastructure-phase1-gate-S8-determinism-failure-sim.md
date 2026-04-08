# TITLE
phase1-gate-S8 — Determinism + failure simulation closure

# CONTEXT
Phase 1 Hardening, Tasks 9 + 10. Bundled per the original execution plan
since both are test-only and have no production code dependencies.

Classification: infrastructure / phase1-hardening / test-closure
Domain: tests/e2e, tests/integration

# OBJECTIVE
Honestly account for the state of determinism and failure-simulation
coverage. Do not fabricate stubs that pretend to test what they don't.

# FINDINGS

## Task 9 — Determinism (already satisfied)
tests/e2e/replay/DeterministicReplayTest.cs:Two_Independent_Runs_Produce_Identical_Events_And_Hashes
runs the Todo lifecycle (Create + Update + Complete) twice in two independent
TestHost fixtures with identical TestClock + TestIdGenerator and asserts:
  - byte-equal persisted event lists
  - matching aggregate ids
  - matching ExecutionHashes per run
  - matching outbox topic + batch counts
  - matching chain block sequence + decision hashes

This is exactly the spec's "Repeat identical commands → assert identical
hashes + IDs" requirement. NO new test required. Status: PASS.

## Task 10 — Failure simulation (deferred — infra gap)
Spec requires:
  - Kafka down → publisher recovery
  - DB down → publisher/consumer recovery
  - Chain failure → recovery

Current state of test infrastructure that BLOCKS this:
  1. Neither tests/unit nor tests/integration references Whycespace.Host,
     where KafkaOutboxPublisher and GenericKafkaProjectionConsumerWorker live.
  2. There is no Testcontainers / docker-compose-in-test wiring; tests use
     in-memory TestHost fixtures with no Kafka/DB at all.
  3. No fault-injection harness exists (no toxiproxy, no embedded Kafka
     broker that can be killed mid-test, no Postgres test container).

Code-level resilience IS in place from earlier S-prompts:
  - phase1-gate-S0/S0b: KafkaOutboxPublisher has unconditional outer
    try/catch; per-row isolation; no row crashes the loop.
  - phase1-gate-S4: deadletter publish failure logged, never thrown.
  - phase1-gate-S5: exponential backoff capped at 300s; no busy-loop on failure.

What's needed to actually run Task 10:
  - New tests/integration/resilience/ subdir
  - Add Whycespace.Host project reference to tests/integration
  - Add Testcontainers.PostgreSql + Testcontainers.Kafka packages
  - Three test scenarios:
      a. Kafka container started → enqueue → stop container → assert
         publisher loop survives, rows marked failed, backoff schedules
         retry, restart container → assert rows publish.
      b. Postgres outbox container stopped mid-publish → assert outer
         catch swallows, publisher resumes when container restored.
      c. Chain anchor fails (inject IChainAnchor stub that throws) →
         assert EventStore append commits, chain failure surfaces, no
         silent data loss.

Estimated scope: ~1 day of dedicated work, separate phase. Recommended
follow-up prompt: phase2-resilience-harness.

# CONSTRAINTS
- Do not write tests that don't actually test the failure mode.
- Do not modify production code.
- Do not pad tests/unit or tests/integration with stubs.

# OUTPUT
Documentation only — this prompt file and a Phase 1 closure summary.

# VALIDATION CRITERIA
- No new .cs files
- DeterministicReplayTest still passes (sanity)
- Phase 1 gap table updated to reflect closure status
