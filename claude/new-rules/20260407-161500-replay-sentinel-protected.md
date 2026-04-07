# REPLAY-SENTINEL-PROTECTED-01

```
CLASSIFICATION: replay-determinism / design-protection
SOURCE:         claude/audits/sweeps/20260407-160500-prompt-a-bugfix.md
SEVERITY:       S1 (architectural intent protection)
DATE:           2026-04-07
STATUS:         CANDIDATE — promote into replay-determinism.guard.md (or audit) on next sweep
```

## DESCRIPTION

The sentinel envelope fields produced by `EventReplayService.ReplayAsync`
are protected design artifacts:

- `PolicyHash = "replay"`
- `ExecutionHash = "replay"`
- `Timestamp = DateTimeOffset.MinValue`

See [src/runtime/event-fabric/EventReplayService.cs:55-59](../../src/runtime/event-fabric/EventReplayService.cs#L55-L59)
and the by-design clause in
[claude/audits/replay-determinism.audit.md:53-72](../audits/replay-determinism.audit.md#L53-L72).

## PROPOSED RULE

> Any prompt or audit instructing the replacement of these sentinels with
> "real" envelope values must be **challenged** and rejected unless it
> first updates `replay-determinism.audit.md` to remove the by-design
> clause AND provides a documented design-change rationale that explains
> what new requirement justifies the divergence between Replay Type A
> (re-execution) and Replay Type B (rebuild).
>
> The path to lifting this protection is:
> 1. Update `replay-determinism.audit.md` to record the new requirement
>    and remove the sentinel-by-design language.
> 2. Extend `EventStoreService` (or its successor) to persist and return
>    per-event envelope metadata (`PolicyHash`, `ExecutionHash`,
>    `Timestamp`) at append time.
> 3. Modify `EventReplayService` to read those values from the store
>    rather than reconstructing envelopes.
>
> Steps 2 and 3 may not be performed without step 1 already merged.

## RATIONALE

The Phase 2 hardening prompt (commit 1e990c6 predecessor) instructed
direct replacement of the sentinels. Pre-flight caught the structural
problem (no inbound envelope at construction time). The post-execution
audit sweep then surfaced the deeper issue: the sentinels are
**intentional**, and erasing them would silently remove a downstream
signal that distinguishes rebuild artifacts from fresh executions.

This rule exists so the next pass cannot relitigate the same instruction
without first updating the audit that documents the intent.

## PROMOTION PATH

On next governance sweep, promote into:
- `claude/guards/replay-determinism.guard.md` (new file), or
- a new `## NEW RULES INTEGRATED` section under
  `claude/audits/replay-determinism.audit.md` itself.

Tag: `REPLAY-SENTINEL-PROTECTED-01`.
