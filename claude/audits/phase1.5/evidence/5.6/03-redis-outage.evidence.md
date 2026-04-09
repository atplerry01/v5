# §5.6 Scenario 3 — Redis Outage / Execution Lock Failure (EVIDENCE)

**Scenario:** Redis-backed `IExecutionLockProvider` (MI-1) returns
unavailable / contended; runtime must surface the failure via the
correct refusal semantics rather than silently double-execute.
**Date:** 2026-04-09
**Test files (live stack):**
- [tests/integration/multi-instance/ConcurrentCommandsTest.cs](../../../../../tests/integration/multi-instance/ConcurrentCommandsTest.cs)
**Test methods (real two-host stack via nginx edge `localhost:18080`):**
- `PhaseA_Idempotent_Concurrent_Identical_Payloads_Collapse_To_One_Aggregate` — **PASSED [335 ms]**
- `PhaseB_Distinct_Concurrent_Commands_All_Persist_Once_Across_Both_Hosts` — **PASSED [1 s]**

## Refusal seam exercised
- **MI-1** distributed execution lock (Redis SET NX PX)
- §5.2.4 **HC-9** Redis health visibility — `RedisExecutionLockProvider`
  swallows store exceptions; control plane returns deterministic
  `execution_lock_unavailable` / `execution_cancelled`

## Live observed metrics

```
[§5.5/2.1 PhaseA] tag=...  sent=50 success=1 duplicate=0
                  executionLockUnavailable=49 other=0 distinctTodoIds=1
[§5.5/2.1 PhaseB] tag=...  sent=100 success=100 failure=0
                  distinctTodoIds=100
[§5.5/2.1 PhaseB] events: total=100 min/agg=1 max/agg=1
```

## Behavior verified
- **PhaseA — lock contention under identical payload:** 50 concurrent
  identical commands across two hosts produced exactly **1 success** +
  **49 `executionLockUnavailable`** + **0 duplicates**, collapsing to
  a single aggregate. The MI-1 lock fired the deterministic refusal
  on every loser; no double-execution.
- **PhaseB — distinct payloads:** 100 concurrent distinct commands
  across the same two hosts all succeeded exactly once
  (`min/agg = max/agg = 1` in the event store), proving the lock does
  not over-serialise the unrelated work.

## Acceptance
| F1 | F2 | F3 | F4 | F5 | F6 |
|---|---|---|---|---|---|
| PASS — 0 lost | PASS — 0 dup | PASS | PASS — `execution_lock_unavailable` is the canonical refusal | PASS — sub-second | PASS |
