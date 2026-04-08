# Replay Determinism Audit Output — Phase 1 Gate
**Audit Date:** 2026-04-08  
**Branch:** dev_wip  
**Guard:** replay-determinism.guard.md

---

## SCOPE

All code related to replay and event reconstruction:
- EventReplayService (Type B projection rebuild with sentinel fields)
- Type A re-execution tests (DeterministicReplayTest.cs)
- EventEnvelope construction
- Execution hash computation
- Policy event emission

---

## FILES_AUDITED

- src/runtime/event-fabric/EventReplayService.cs
- src/runtime/event-fabric/EventEnvelope.cs
- src/runtime/deterministic/ExecutionHash.cs
- src/runtime/dispatcher/RuntimeCommandDispatcher.cs
- src/engines/T1M/lifecycle/WorkflowExecutionReplayService.cs
- tests/e2e/replay/DeterministicReplayTest.cs
- src/engines/T0U/whycepolicy/PolicyDecisionEventFactory.cs

---

## FINDINGS

### REPLAY-SENTINEL-PROTECTED-01: PASS

**Verification of protected sentinels in EventReplayService.cs (lines 55-59):**

Code snippet:
```csharp
ExecutionHash = "replay",
PolicyHash    = "replay",
Timestamp     = DateTimeOffset.MinValue,
```

Status: All three sentinels present, exact literal values matched, in specified order.

Evidence:
- EventReplayService.cs line 57: ExecutionHash = "replay" ✓
- EventReplayService.cs line 58: PolicyHash = "replay" ✓
- EventReplayService.cs line 59: Timestamp = DateTimeOffset.MinValue ✓

**No attempt to replace with real envelope values detected.** Sentinels are protected design artifacts.

---

### REPLAY-SENTINEL-LIFT-01: PASS

**Verification that the lift procedure has NOT been invoked:**

1. claude/audits/replay-determinism.audit.md still contains the by-design clause (lines 53-72): ✓
2. No code in src/runtime/event-fabric/EventReplay*.cs reads stored envelope metadata for ExecutionHash/PolicyHash/Timestamp: ✓
3. EventReplayService only reads domain event payloads, not persisted envelope fields.

Conclusion: The protection is intact. The lift procedure has not been started.

---

### REPLAY-A-vs-B-DISTINCTION-01: PASS

**Verification that audits and tests respect Type A vs Type B distinction:**

**Type A — Re-execution (DeterministicReplayTest.cs):**
- Runs identical commands twice through full RuntimeControlPlane → Engine → EventFabric pipeline.
- Uses frozen TestClock and deterministic TestIdGenerator.
- Asserts byte-equal ExecutionHash, PolicyHash, Timestamp, and persisted event lists (lines 30-70).
- Correctly classifies as Type A re-execution.

Evidence from test (lines 47-68):
- "Recomputed execution hashes are byte-equal across runs" (line 47)
- "Chain block timestamps are deterministic across runs (frozen TestClock)" (line 65)
- Correctly uses frozen TestClock to ensure Type A determinism.

**Type B — Projection rebuild (EventReplayService.cs):**
- Loads events from event store and dispatches to projection handlers.
- Sets sentinel values: ExecutionHash="replay", PolicyHash="replay", Timestamp=MinValue.
- Does NOT assert envelope equality; only identity fields survive rebuild.

Evidence from service (lines 47-61):
- Sentinels are deliberately set for rebuild signals.
- No assertion of envelope equality to originals.

Conclusion: Tests correctly distinguish Type A from Type B. No misclassification violations.

---

### POLICY-REPLAY-INTEGRITY-01: PASS

**Verification that EventReplayService does NOT re-evaluate policy during replay:**

Scanned EventReplayService.cs and WorkflowExecutionReplayService.cs:
- No call to IPolicy* or policy evaluator found. ✓
- Both services load domain events and fold them through aggregate.LoadFromHistory().
- WorkflowExecutionReplayService (src/engines/T1M/lifecycle/, lines 37-88):
  - Calls aggregate.LoadFromHistory(events) (line 48).
  - No policy re-evaluation.
  - Event payload rehydration only (lines 70-76).

Stored PolicyEvaluatedEvent / PolicyDeniedEvent records are the source of truth.

Conclusion: Policy is NOT re-evaluated during replay. Integrity maintained.

---

## HASH DETERMINISM INTEGRATION: PASS

**Verification that ExecutionHash does not read IClock or produce non-deterministic hashes:**

ExecutionHash.cs review (lines 18-83):
- Compute(CommandContext, IReadOnlyList<object>) inputs:
  - correlationId, commandId, aggregateId (stable IDs)
  - identityId, roles, trustScore (identity context, normalized)
  - policyId, policyDecisionHash, policyDecisionAllowed (policy artifacts)
  - event types, positions, payload hashes (deterministic per event)
  - count (derived from event list)

No forbidden inputs:
- No DateTime.Now / DateTime.UtcNow / IClock.UtcNow
- No Guid.NewGuid()
- No Random
- No unordered collections (payloadHashes built in event iteration order)

Per-event signatures include position index (line 50): `$"{evt.GetType().Name}:{i}:{payloadHash}"` ✓

Conclusion: Hash determinism is maintained. Replay hashes match re-execution hashes.

---

## VERDICT

PASS — All replay determinism rules are enforced and validated.

| Rule | Status | Notes |
|------|--------|-------|
| REPLAY-SENTINEL-PROTECTED-01 | PASS | All three sentinels present, exact values |
| REPLAY-SENTINEL-LIFT-01 | PASS | Protection intact, lift procedure not started |
| REPLAY-A-vs-B-DISTINCTION-01 | PASS | Tests correctly classify and assert appropriately |
| POLICY-REPLAY-INTEGRITY-01 | PASS | No policy re-evaluation during replay |
| Hash Determinism (integration check) | PASS | ExecutionHash free of clock/RNG, position-aware |

---

## NEW_RULE_CANDIDATES

None identified. All replay determinism rules are correctly implemented.

---

## Audit Completeness Check

- [x] EventReplayService sentinel assignments verified (all three present).
- [x] Lift procedure verification (by-design clause still in place).
- [x] Type A vs Type B distinction checked in tests and services.
- [x] Policy re-evaluation scan (none found).
- [x] ExecutionHash inputs scanned for clock/RNG (none present).
- [x] Per-event signature position index verified.
- [x] WorkflowExecutionReplayService (engine-side impl) verified.

