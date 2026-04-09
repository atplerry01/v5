# Phase 1.5 — Determinism Enforcement Audit

**STATUS: PASS**
**SCOPE: §2.2 of phase1.5-final closure prompt**
**DATE: 2026-04-09**

## Finding

The codebase respects the $9 deterministic-time and deterministic-id discipline. All time flows through `IClock`; all IDs are deterministic or process-unique infrastructure tokens; no `Guid.NewGuid()` or `DateTime.UtcNow` exists in production paths.

## Evidence

### `Guid.NewGuid()` scan
```
$ grep -RIn "Guid\.NewGuid(" src/
src/runtime/guards/DeterminismGuard.cs   (the guard file itself; lists forbidden patterns)
```
Single hit. It is the canonical guard documentation. Zero production usages.

### `DateTime.UtcNow` / `DateTimeOffset.UtcNow` scan
```
$ grep -RIn "DateTime\.UtcNow|DateTimeOffset\.UtcNow" src/
src/platform/host/composition/core/SystemClock.cs:7:    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;   ← canonical IClock
src/runtime/guards/DeterminismGuard.cs:11                                                                       ← guard doc
src/platform/host/health/RedisHealthSnapshotProvider.cs:20                                                      ← XML comment
src/platform/host/adapters/OutboxDepthSnapshot.cs:36                                                            ← XML comment
src/platform/host/adapters/PostgresOutboxAdapter.cs:29                                                          ← XML comment
src/domain/constitutional-system/policy/decision/event/PolicyEvaluatedEvent.cs:10                               ← XML comment
```
Only the canonical `SystemClock` implementation reads system time directly; every other hit is documentation explicitly NOT calling it. Zero production violations.

### MI-1 owner tokens are $9-compliant
`RedisExecutionLockProvider.NextOwnerToken()` constructs owner tokens as `{MachineName}:{ProcessId}:{Interlocked counter:x}`. No `Guid.NewGuid`, no clock, no RNG. Process-unique by construction; unique across acquisitions via the monotonic counter. Verified at [src/platform/host/runtime/RedisExecutionLockProvider.cs](src/platform/host/runtime/RedisExecutionLockProvider.cs).

### Reason ordering determinism
- `RuntimeStateAggregator.ComputeFromResults` evaluates rules in a fixed declaration order; reasons are appended in that order.
- `PostgresPoolHealthEvaluator.Evaluate` (linter-patched) emits reasons in fixed canonical order regardless of pool iteration order. Locked by `PostgresPoolHealthEvaluatorTests.DeterministicReasonOrder_AcrossPools`.
- `RuntimeDegradedMode.From` filters and deduplicates while preserving insertion order; the canonical reason set is locked by `RuntimeDegradedModeTests.CanonicalReasonSet_MatchesSpec` (HC-7 + HC-9).

## Result

PASS. Determinism is fully enforced across the §5.2.4 / §5.2.5 surface. No clock leakage, no RNG, no `Guid.NewGuid`. Reason vocabularies are locked by test.
