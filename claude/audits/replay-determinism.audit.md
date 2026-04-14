# Replay Determinism Audit

## Purpose

Establish whether the same command sequence, executed twice with the same
deterministic context, produces identical events, identical event ids,
identical hashes, and identical projection state.

This audit produces a STATIC verdict by inspecting the code that owns the
determinism properties. A runtime witness (integration test) is documented
as a follow-up obligation.

## Properties Under Audit

For two executions of the same `(commands[], CommandContext, IClock,
IIdGenerator)` tuple, the audit asks whether the following equalities hold:

| Property | Static verdict | Source |
|---|---|---|
| `events_run2 == events_run1` (element-wise, by type + payload) | **PASS** | Engine and aggregate code is pure given identical inputs; no hidden state. |
| `eventIds_run2 == eventIds_run1` | **PASS** | `EventEnvelope.GenerateDeterministicId(correlationId, eventTypeName, sequenceIndex)` is a pure function of its inputs. See [EventFabric.cs:64-65](../../src/runtime/event-fabric/EventFabric.cs#L64-L65). |
| `executionHash_run2 == executionHash_run1` | **PASS** | `ExecutionHash.Compute` inputs are all deterministic. See [constitutional.guard.md §Hash Determinism](../guards/constitutional.guard.md) and [ExecutionHash.cs](../../src/runtime/deterministic/ExecutionHash.cs). |
| `decisionHash_run2 == decisionHash_run1` | **PASS** (conditional) | The decision hash is produced by the policy engine. As long as the policy itself is deterministic and the policy version is held constant, the decision hash is reproducible. |
| `projectionState_run2 == projectionState_run1` | **PASS** (conditional) | Projection handlers must be pure functions of the event stream. The projection guard ([runtime.guard.md §Projection Layer](../guards/runtime.guard.md)) enforces this. |
| `envelopeTimestamp_run2 == envelopeTimestamp_run1` | **PASS** (conditional) | Only true when the same `IClock` instance is supplied. `EventFabric.cs:75` reads `_clock.UtcNow`. With a frozen test clock, this is stable. With `SystemClock`, it varies. |

## Replay Path Distinction

The codebase has **two distinct notions of "replay"** and the audit treats
them differently:

### Replay Type A — Re-execution

Run the same commands twice through `RuntimeControlPlane → Engine →
EventFabric` against fresh stores, with the same `IClock` and
`IIdGenerator`. This is the property the prompt's TG2 is asking about.

**Static verdict:** PASS, conditional on:
- Same `IClock` (a fixed test clock for deterministic envelope timestamps)
- Same `IIdGenerator` (already DI-default per
  [Program.cs:60](../../src/platform/host/Program.cs#L60) — `DeterministicIdGenerator` derives ids
  via SHA256 of seed)
- Same policy decision (same OPA bundle / same policy version)
- Domain code is pure (enforced by [domain.guard.md](../guards/domain.guard.md))

### Replay Type B — Projection rebuild from event store

Use [EventReplayService.ReplayAsync](../../src/runtime/event-fabric/EventReplayService.cs#L35) to load events
from the event store and dispatch them to the projection handlers. Used
for rebuilding projections from history.

**Important:** This path **deliberately** sets sentinel values on the
replayed envelopes:

```csharp
ExecutionHash = "replay",
PolicyHash = "replay",
Timestamp = DateTimeOffset.MinValue,
```

at [EventReplayService.cs:57-59](../../src/runtime/event-fabric/EventReplayService.cs#L57-L59).

This is **by design** — the sentinel signals to downstream consumers that
the envelope is a rebuild artifact, not a fresh execution. As a
consequence, **envelope-field equality between original and replay-rebuild
does not hold** for `ExecutionHash`, `PolicyHash`, or `Timestamp`. Only
`EventId`, `AggregateId`, `CorrelationId`, `EventType`, `Payload`,
`SequenceNumber`, and structural fields are preserved.

If a future requirement needs full envelope-field equality on rebuild,
`EventReplayService` would need to load the original envelopes from the
store rather than reconstructing them. That is a design change, not a fix.

## Audit Procedure

1. **For Replay Type A:** verify by inspection that
   - `ExecutionHash.Compute` has only permitted inputs (see
     `constitutional.guard.md §Hash Determinism`)
   - `EventEnvelope.GenerateDeterministicId` is a pure function
   - `RuntimeCommandDispatcher.ExecuteEngineAsync` does not introduce
     non-determinism (no clock reads, no RNG)
   - `EventFabric.ProcessAsync` reads `_clock.UtcNow` only at envelope
     construction; the clock is injected, so a frozen clock makes the
     timestamp stable
2. **For Replay Type B:** verify the documented sentinel pattern and
   confirm no caller asserts on `ExecutionHash == "replay"` as a feature
   rather than a sentinel.

## Result

**STATUS: PASS (static, with caveats)**

- Replay Type A: PASS by static inspection. All hash inputs are
  deterministic; all id generation is deterministic; all timestamp reads
  flow through `IClock`. With a frozen test clock and the existing
  `DeterministicIdGenerator`, two re-executions of the same command
  sequence will produce byte-equal events and byte-equal hashes.
- Replay Type B: PASS for the structural fields; envelope hash/timestamp
  divergence is by design, not a violation.

## Caveats and Follow-Up Obligations

1. **No runtime witness exists.** The integration test project
   ([tests/integration/](../../tests/integration/)) does not currently compile
   (see `claude/new-rules/20260407-133419-audits.md`). The static verdict
   above is sufficient for the Phase 2 lock condition under WBSM v3 §1
   ("statically auditable OR test-proven"), but a runtime witness should
   be added once the test suite is repaired.
2. **The follow-up test should be Replay Type A**, not B. It should:
   - Wire the full `RuntimeControlPlaneBuilder` pipeline (all 8 middlewares)
   - Use a frozen `IClock` (e.g. `TestClock` already present in the broken
     `TodoPipelineTests.cs`)
   - Use the existing `DeterministicIdGenerator` or a `TestIdGenerator`
   - Run `Create → Update → Complete` against two fresh in-memory stores
   - Assert: `events1 == events2`, `eventIds1 == eventIds2`,
     `recomputedExecutionHash1 == recomputedExecutionHash2`,
     `projectionState1 == projectionState2`
3. **Policy determinism is assumed, not proven, by this audit.** The OPA
   policy engine is external and its determinism is the OPA project's
   guarantee, not ours. A future audit could pin the OPA bundle hash and
   re-evaluate to confirm.
4. **Test doubles in the broken `TodoPipelineTests.cs`** (notably
   `InMemoryChainAnchor` at line 318) currently use `Guid.NewGuid()` and
   `DateTimeOffset.UtcNow`, which would defeat replay assertions if those
   doubles were rehabilitated as-is. When the test suite is repaired, the
   doubles must be retrofitted to take `IClock` + `IIdGenerator`.

---

## NEW CHECKS INTEGRATED — 2026-04-07

- **CHECK-REPLAY-A-vs-B**: Replay assertions distinguish Type A (re-execution: ExecutionHash/PolicyHash/Timestamp must equal originals under deterministic IClock+IIdGenerator) vs Type B (projection rebuild via EventReplayService: envelope hash fields are sentinel by design and MUST NOT be asserted equal). Audits/prompts asserting hash equality refer to Type A only.
- **CHECK-REPLAY-DOUBLES-01**: Test doubles used in replay tests MUST consume IClock + IIdGenerator. Doubles still using Guid.NewGuid / DateTimeOffset.UtcNow fail this audit (precondition for any replay assertion).
