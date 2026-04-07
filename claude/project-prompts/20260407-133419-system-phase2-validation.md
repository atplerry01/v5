# WHYCESPACE — PHASE 2 VALIDATION PROMPT (ORDER + REPLAY PROOF)

## CLASSIFICATION: system / validation / determinism

## PHASE: Phase 2 Entry Gate (FINAL LOCK)

## PRIORITY: CRITICAL

## MODE: PROOF-DRIVEN (NO ASSUMPTIONS)

---

# 🎯 OBJECTIVE

Provide **formal, verifiable proof** that:

1. Runtime execution order is **strict, deterministic, and unbreakable**
2. Event replay produces **identical results (state + hashes + events)**

---

# 🔒 GLOBAL VALIDATION RULES

1. **No trust in runtime-only checks**

   * All guarantees must be:

     * statically auditable OR
     * test-proven

2. **Replay must be mathematically identical**

   * Same input MUST produce:

     * same events
     * same event IDs
     * same ExecutionHash
     * same DecisionHash
     * same final state

3. **Execution order is immutable**

   * Any deviation = FAIL

---

# 🚨 TASK GROUP 1 — EXECUTION ORDER PROOF

## 1.1 DEFINE CANONICAL ORDER (LOCK)

```
1. Guard (pre)
2. Policy evaluation
3. Guard (post-policy)
4. Execution (engine/workflow)
5. Persist events (EventStore)
6. Chain anchoring (WhyceChain)
7. Kafka publish (Outbox)
```

## 1.2 ADD RUNTIME ORDER AUDIT

CREATE: `/claude/audits/runtime-order.audit.md`

VALIDATION REQUIREMENTS: trace actual call path inside RuntimeControlPlane,
middleware pipeline, event persistence, chain service, kafka publisher.
MUST VERIFY no step is skipped, no step is reordered, no parallel execution
breaks sequence. OUTPUT PASS if strictly ordered, FAIL with file+line refs.

## 1.3 ADD EXECUTION ORDER INTEGRATION TEST

CREATE: `tests/integration/runtime/ExecutionOrderTest.cs`

TEST FLOW: execute a command (e.g. CreateTodo), capture markers at each
stage (guard pre, policy, guard post, engine, persist, chain, kafka),
assert strict sequence: GuardPre < Policy < GuardPost < Execution < Persist
< Chain < Kafka. Use injected IClock (no system time), deterministic
markers, FAIL on any deviation.

## 1.4 ADD ORDER GUARD (STATIC)

CREATE: `/claude/guards/runtime-order.guard.md`

FAIL IF kafka before chain, chain before persistence, policy after
execution, guard missing before or after policy.

---

# 🚨 TASK GROUP 2 — REPLAY DETERMINISM PROOF

## 2.1 DEFINE REPLAY REQUIREMENT (LOCK)

Replay MUST produce identical event sequence, event IDs, ExecutionHash,
DecisionHash, final state.

## 2.2 CREATE REPLAY TEST SUITE

CREATE: `tests/e2e/replay/DeterministicReplayTest.cs`

## 2.3 TEST SCENARIO (TODO DOMAIN)

STEP 1 — Execute CreateTodo, UpdateTodo, CompleteTodo. Capture EventIds[],
ExecutionHashes[], DecisionHashes[], FinalState.

STEP 2 — Rebuild aggregate from event store, re-run projection.

STEP 3 — Assert equality across all four collections.

FAIL CONDITIONS: any mismatch, any additional/missing event, any ordering
difference.

## 2.4 ADD REPLAY AUDIT

CREATE: `/claude/audits/replay-determinism.audit.md`

OUTPUT: PASS on exact match, FAIL with diff (events, hashes, state).

---

# 🚨 TASK GROUP 3 — HASH CONSISTENCY VALIDATION

## 3.1 VERIFY HASH INPUTS

ExecutionHash + DecisionHash MUST NOT include timestamps or random values,
ONLY deterministic inputs, normalized identity context, ordered data.

## 3.2 ADD HASH GUARD

CREATE: `/claude/guards/hash-determinism.guard.md`

FAIL IF DateTime in hash input, unordered collections, non-normalized data.

---

# 🚨 TASK GROUP 4 — EVENT ORDERING GUARANTEE

## 4.1 VERIFY EVENT STORE ORDERING

Events stored with version + sequence, strict ordering enforced.

## 4.2 ADD EVENT ORDER TEST

CREATE: `tests/integration/eventstore/EventOrderingTest.cs`

ASSERT version increments strictly, no gaps, no duplicates.

---

# 🚨 TASK GROUP 5 — FINAL VALIDATION

System must pass: runtime-order.audit, replay-determinism.audit,
ExecutionOrderTest, DeterministicReplayTest, EventOrderingTest.

Final certification output: execution order report (verified sequence,
evidence), replay determinism report (before vs replay), final status
PASS/FAIL.

---

# 🔐 LOCK CONDITION

Phase 2 is FULLY LOCKED ONLY IF:
- Execution Order = PROVEN
- Replay Determinism = PROVEN

# END OF PROMPT
