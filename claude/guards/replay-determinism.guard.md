---
name: replay-determinism
type: structural
severity: S1
locked: true
---

# Replay Determinism Guard

## Purpose

Lock the design intent behind `EventReplayService.ReplayAsync` and protect
the sentinel envelope fields it produces during projection rebuild. This
guard exists to prevent future passes from "fixing" sentinels that are
intentional design markers.

## Scope

- [src/runtime/event-fabric/EventReplayService.cs](../../src/runtime/event-fabric/EventReplayService.cs)
- Any future `EventReplay*.cs` file under `src/runtime/event-fabric/`
- The audit document
  [claude/audits/replay-determinism.audit.md](../audits/replay-determinism.audit.md)
  which records the by-design rationale and is the source of truth for
  this guard.

## Background

The codebase maintains two distinct notions of replay, documented in
[replay-determinism.audit.md:31-72](../audits/replay-determinism.audit.md#L31-L72):

- **Type A — Re-execution.** Run the same commands twice through the
  full RuntimeControlPlane → Engine → EventFabric pipeline. With a
  frozen `IClock` and the existing `DeterministicIdGenerator`, every
  envelope field including `ExecutionHash`, `PolicyHash`, and
  `Timestamp` is byte-equal between runs. This is the property
  protected by `hash-determinism.guard.md`.

- **Type B — Projection rebuild.** Use `EventReplayService.ReplayAsync`
  to load events from the event store and dispatch them to projection
  handlers. This path **deliberately** sets sentinel values:
  - `PolicyHash = "replay"`
  - `ExecutionHash = "replay"`
  - `Timestamp = DateTimeOffset.MinValue`

  The sentinels signal to downstream consumers that the envelope is a
  rebuild artifact, not a fresh execution. They are a feature, not a
  bug.

## Rules

### REPLAY-SENTINEL-PROTECTED-01 — Sentinels are protected design artifacts

The three sentinel assignments in `EventReplayService.ReplayAsync` MUST
remain in place. Any code change, prompt instruction, or audit finding
that proposes replacing them with "real" envelope values is a
violation of this guard and MUST be rejected at the guard-load stage.

**The protected statements are:**

```csharp
ExecutionHash = "replay",
PolicyHash    = "replay",
Timestamp     = DateTimeOffset.MinValue,
```

Located at
[EventReplayService.cs:55-59](../../src/runtime/event-fabric/EventReplayService.cs#L55-L59).

**Severity:** S1 — HIGH (block merge).

### REPLAY-SENTINEL-LIFT-01 — How to lift the protection

The protection is **not absolute**, but lifting it requires a documented
design change, not a hardening fix. The path to changing the sentinel
behavior is:

1. **First** update
   [claude/audits/replay-determinism.audit.md](../audits/replay-determinism.audit.md)
   to remove the by-design clause at lines 53-72 and record the new
   requirement that justifies the change. Without this update, no
   downstream change is permitted.
2. Extend `EventStoreService` (or its successor) to persist and return
   per-event envelope metadata (`PolicyHash`, `ExecutionHash`,
   `Timestamp`) at the time the events are appended to the store.
3. Modify `EventReplayService.ReplayAsync` to read those values from
   the store rather than reconstructing envelopes from raw events.

Steps 2 and 3 may not be performed in any commit that does not also
contain step 1.

**Severity:** S1 — HIGH (block merge).

### REPLAY-A-vs-B-DISTINCTION-01 — Audits and prompts must respect the distinction

Any audit, prompt, or test that asserts envelope-field equality on
replay MUST distinguish Type A (re-execution) from Type B (rebuild):

- For Type A: `ExecutionHash`, `PolicyHash`, and `Timestamp` MUST be
  byte-equal between runs under a frozen `IClock` and deterministic
  `IIdGenerator`. Failure here is a true determinism violation under
  `hash-determinism.guard.md` / `determinism.guard.md`.
- For Type B: `ExecutionHash`, `PolicyHash`, and `Timestamp` are
  sentinel values and MUST NOT be asserted equal to the originals. The
  fields that DO survive rebuild are `EventId`, `AggregateId`,
  `CorrelationId`, `EventType`, `Payload`, and `SequenceNumber`.

Asserting Type A equality on a Type B replay is a misclassification
and a violation of this rule.

**Severity:** S2 — MEDIUM (advisory in CI; block in audit).

## Check Procedure

1. Open `EventReplayService.cs`. Verify the three sentinel assignments
   are present, in the order shown above, with the exact literal
   values (`"replay"`, `"replay"`, `DateTimeOffset.MinValue`).
2. Grep `src/runtime/event-fabric/EventReplay*.cs` for any code that
   reads `ExecutionHash`, `PolicyHash`, or `Timestamp` from a stored
   event metadata source — if present, confirm
   `replay-determinism.audit.md` no longer contains the by-design
   clause (i.e. step 1 of the lift procedure has been completed). If
   the audit still has the clause, the read is a violation of
   REPLAY-SENTINEL-LIFT-01.
3. Grep `tests/**` and `claude/audits/**` for assertions on
   `ExecutionHash` or `PolicyHash` equality across replays. For each
   hit, classify as Type A or Type B per
   REPLAY-A-vs-B-DISTINCTION-01.

## Pass Criteria

- All three sentinel assignments present and unchanged.
- No code in `src/runtime/event-fabric/` reads stored envelope metadata
  for `ExecutionHash` / `PolicyHash` / `Timestamp` unless the lift
  procedure has been completed.
- All replay equality assertions correctly distinguish A from B.

## Fail Criteria (S1 unless noted)

- Any sentinel assignment removed or changed without the lift
  procedure complete.
- New code reading stored metadata for the protected fields without
  the audit update.
- Type B replay assertions claiming envelope equality on the
  protected fields (S2).

## Enforcement Action

- **S1**: Block merge. The matching audit `replay-determinism.audit.md`
  must continue to PASS its by-design clause check.
- **S2**: Warn in CI; correct the assertion classification.

## Provenance

Promoted into a canonical guard on 2026-04-07 from
`claude/new-rules/20260407-161500-replay-sentinel-protected.md`. The
candidate was created during the post-execution audit sweep of the
Phase 2 hardening Prompt A
([claude/audits/sweeps/20260407-160500-prompt-a-bugfix.md](../audits/sweeps/20260407-160500-prompt-a-bugfix.md))
after the original Phase 2 mega-prompt instructed direct replacement
of the sentinels — an instruction that was first caught at pre-flight
as structurally wrong, and then revealed by the audit sweep to be a
design-intent violation as well.

## Related Guards and Audits

- [hash-determinism.guard.md](hash-determinism.guard.md) — protects
  Type A hash determinism on the forward execution path.
- [determinism.guard.md](determinism.guard.md) — global determinism
  block list.
- [claude/audits/replay-determinism.audit.md](../audits/replay-determinism.audit.md) —
  the by-design source-of-truth for the sentinel pattern.

## NEW RULES INTEGRATED — 2026-04-07 (policy eventification)

- **POLICY-REPLAY-INTEGRITY-01** (S0): `EventReplayService` MUST NOT re-evaluate policy during replay.
  Stored `PolicyEvaluatedEvent` / `PolicyDeniedEvent` records are the source of truth for replayed decisions.
  Re-evaluation would risk drift if policy versions or trust scores have changed since original evaluation.
- Source: `claude/new-rules/_archives/20260407-190000-policy-eventification.md`.
