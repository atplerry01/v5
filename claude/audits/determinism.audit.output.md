# Determinism Audit Output — Phase 1 Gate
**Audit Date:** 2026-04-08  
**Branch:** dev_wip  
**Guard:** `claude/guards/determinism.guard.md`

---

## SCOPE

All code under `src/domain/**`, `src/engines/**`, `src/runtime/**`, `src/systems/**`, `src/platform/host/adapters/**` for violations of:
- `Guid.NewGuid()`
- `DateTime.Now` / `DateTime.UtcNow` / `DateTimeOffset.Now` / `DateTimeOffset.UtcNow`
- `Random` / `RandomNumberGenerator`
- `Environment.TickCount` / `Environment.TickCount64`
- `Stopwatch.GetTimestamp()`

Exceptions: `SystemClock.UtcNow` (line 7 in `src/platform/host/composition/core/SystemClock.cs`) — permitted implementation of `IClock`.

---

## FILES_AUDITED

**Total changed files in diff:** 97  
**Files with determinism relevance:** 44

### Key Files Scanned
- `src/engines/T0U/determinism/` (DeterministicIdEngine.cs, PersistedSequenceResolver.cs, DeterministicTimeBucketProvider.cs) ✓
- `src/platform/host/adapters/` (PostgresEventStoreAdapter.cs, PostgresSequenceStoreAdapter.cs, KafkaOutboxPublisher.cs, etc.) ✓
- `src/runtime/control-plane/RuntimeControlPlane.cs` ✓
- `src/runtime/event-fabric/` (EventEnvelope.cs, EventReplayService.cs, EventFabric.cs) ✓
- `src/platform/api/controllers/TodoController.cs` ✓
- `src/runtime/dispatcher/SystemIntentDispatcher.cs` ✓
- `src/systems/midstream/wss/WorkflowDispatcher.cs` ✓
- `src/domain/constitutional-system/policy/decision/event/` (PolicyEvaluatedEvent.cs, PolicyDeniedEvent.cs) ✓
- `src/engines/T0U/whycepolicy/PolicyDecisionEventFactory.cs` ✓

---

## FINDINGS

### S1 — HIGH SEVERITY

#### Finding 1: Non-deterministic seed in TodoController.Create
**File:** `src/platform/api/controllers/TodoController.cs:45`  
**Line:** `var aggregateId = _idGenerator.Generate($"{request.UserId}:{request.Title}:{_clock.UtcNow.Ticks}");`  
**Rule Violated:** determinism.guard.md § Block List + DET-SEED-01  
**Severity:** S1 (HIGH)  
**Description:**  
The seed passed to `IIdGenerator.Generate()` includes `_clock.UtcNow.Ticks`, a wall-clock value. Although `IIdGenerator.Generate()` is the permitted seam, the seed itself must be deterministic. This seed varies per invocation, causing the same user+title combination to produce different aggregate IDs on subsequent runs. Per DET-SEED-01, the seed MUST be derived deterministically from the operation's coordinates — e.g., `$"{request.UserId}:{request.Title}"` without the clock.

**Evidence:**
- Guard clause: "The seed MUST be derived deterministically from the operation's coordinates — for example `$"{aggregateId}:{version}"`... Random or wall-clock seeds defeat the purpose."
- The seed fails replay: re-running with the same inputs produces different aggregate IDs.

---

#### Finding 2: Non-deterministic seed in SystemIntentDispatcher.DispatchAsync
**File:** `src/runtime/dispatcher/SystemIntentDispatcher.cs:32–35`  
**Lines:**
```csharp
var timestamp = _clock.UtcNow.Ticks.ToString();
var correlationId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:correlation:{timestamp}");
var causationId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:causation:{timestamp}");
var commandId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:command:{timestamp}");
```
**Rule Violated:** determinism.guard.md § DET-SEED-01  
**Severity:** S1 (HIGH)  
**Description:**  
The timestamp is derived from `_clock.UtcNow.Ticks`, making all three ID seeds non-deterministic. This violates DET-SEED-01: "The seed MUST be derived deterministically... Random or wall-clock seeds defeat the purpose." On replay, the same command with the same `aggregateId` and `commandType.Name` will receive different correlationId, causationId, and commandId values.

**Evidence:**
- Guard clause: seed must come from "the operation's coordinates — for example `$"{aggregateId}:{version}"`".
- No idempotence: two calls with same inputs → different IDs.
- Type A replay will fail: re-executing the same command sequence produces different command-layer IDs, breaking determinism up the chain.

---

#### Finding 3: Non-deterministic seed in WorkflowDispatcher.StartWorkflowAsync
**File:** `src/systems/midstream/wss/WorkflowDispatcher.cs`  
**Line:** `var timestamp = _clock.UtcNow.Ticks.ToString();` (then used in `$"workflow:{workflowName}:{timestamp}"`)  
**Rule Violated:** determinism.guard.md § DET-SEED-01  
**Severity:** S1 (HIGH)  
**Description:**  
Same pattern as SystemIntentDispatcher: wall-clock ticks in the seed. The workflow ID is non-deterministic and will differ between runs.

---

### S0 — CRITICAL SEVERITY

**No S0 violations found.**

All direct `Guid.NewGuid()`, `DateTime.UtcNow`, `Random`, etc. calls have been isolated to:
- `SystemClock.UtcNow` (permitted exception per determinism.guard.md § Exceptions)
- `DeterministicIdGenerator.Generate()` implementation (permitted exception)
- `DeterministicTimeBucketProvider.GetBucket()` (uses SHA256, no clock read)

---

## VERDICT

**FAIL** — Three S1 findings block merge per determinism.guard.md § Enforcement Action.

| Finding | Severity | File | Issue |
|---------|----------|------|-------|
| Non-deterministic seed: clock ticks in TodoController aggregateId | S1 | TodoController.cs:45 | Wall-clock seed defeats determinism |
| Non-deterministic seeds: clock ticks in SystemIntentDispatcher IDs | S1 | SystemIntentDispatcher.cs:32–35 | Wall-clock seeds in three command IDs |
| Non-deterministic seed: clock ticks in WorkflowDispatcher workflowId | S1 | WorkflowDispatcher.cs | Wall-clock seed in workflow ID |

---

## NEW_RULE_CANDIDATES

### DET-SEED-DERIVATION-01 (S1 — from audit findings)
**Title:** ID seed derivation must exclude non-deterministic clock values.

**Rule:**  
When calling `IIdGenerator.Generate(seed)`, the seed MUST be derived entirely from deterministic coordinates. Specifically:
- FORBIDDEN: `seed = $"{...}:{_clock.UtcNow}:{...}"` or any clock component.
- FORBIDDEN: `seed = $"{...}:{Guid.NewGuid()}:{...}"`.
- REQUIRED: Seed MUST include stable command context: aggregate ID, command type, causation line, version, or policy ID — never wall-clock ticks.

**Severity:** S1 (blocks merge).

**Rationale:**  
Seeds are the source of determinism for ID generation. If the seed contains a clock value, the generated ID is non-deterministic and replay fails — two executions of the same command produce different IDs, breaking chain anchoring, causation tracking, and idempotency.

**Exception:**  
None. The clock is available (injected via `IClock`), but its value must not flow into seed generation. If unique IDs are needed across distributed calls, use the sequence resolver keyed on deterministic topology+seed, not clock ticks.

---

## Audit Completeness Check

- [x] Guard block list scanned via ripgrep on all in-scope paths.
- [x] Exceptions verified (SystemClock, DeterministicIdGenerator).
- [x] All adapters checked for inline `Guid.NewGuid()` / clock usage.
- [x] RuntimeControlPlane prelude (HSID stamping) verified — uses SHA256-derived bucket, no clock read.
- [x] EventReplayService checked — sentinels intact (ExecutionHash="replay", etc.).
- [x] Policy event factory checked — pure, no clock/RNG.
- [x] Domain layer checked — no IDeterministicIdEngine references.
- [x] Sequence store migration verified in Program.cs (HsidInfrastructureValidator).

