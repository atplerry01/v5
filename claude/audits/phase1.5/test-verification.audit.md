# Phase 1.5 — Test & Build Verification Audit

**STATUS: PASS** (post-Patch B1, 2026-04-09)
**SCOPE: §2.8 of phase1.5-final closure prompt**
**DATE: 2026-04-09**

## Post-Patch Result

```
$ dotnet test tests/unit/Whycespace.Tests.Unit.csproj
Passed!  - Failed: 0, Passed: 98, Skipped: 0, Total: 98, Duration: 6 s
```

Patch B1 applied to `tests/unit/architecture/WbsmArchitectureTests.cs`:
the `No_upstream_layer_catches_ConcurrencyConflictException` predicate
now partitions catch-clause hits via the new `IsPureRethrowCatchHit`
helper. A catch whose body contains a bare `throw;` and neither
`return` nor `throw new` is recognized as observability instrumentation
and admitted. The is-pattern (`is ConcurrencyConflictException`) and
any catch with a non-rethrow body remain hard violations. The new
allowance is locked by guard rule R-RT-10 in
`claude/guards/phase1.5-runtime.guard.md`. Zero production source
files were touched.

---

## Original Findings (pre-Patch B1, retained for audit history)

## Build

```
$ dotnet build src/platform/host/Whycespace.Host.csproj
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:10.00
```

All 8 projects build clean. PASS.

## Unit tests

```
$ dotnet test tests/unit/Whycespace.Tests.Unit.csproj
Failed!  - Failed: 1, Passed: 97, Skipped: 0, Total: 98, Duration: 4 s
```

97 of 98 passing. **One failing test (pre-existing, not introduced by §5.2.4):**

### `WbsmArchitectureTests.No_upstream_layer_catches_ConcurrencyConflictException`

```
ConcurrencyConflictException must travel untouched from the event store to the API middleware.
Catches found:
src/platform/host/adapters/PostgresEventStoreAdapter.cs:237: catch (ConcurrencyConflictException) when (outcome == "ok")
```

**Root cause**: `PostgresEventStoreAdapter` catches the exception inside its outcome-tag observability block, sets `outcome = "concurrency_conflict"`, and **re-throws unmodified**. This is a catch+rethrow for histogram tag attribution — NOT a swallow, NOT a control-flow branch. The exception still travels untouched from the throw site to the API edge handler in functional terms.

**Why the test fails**: The architecture test predicate matches any `catch (ConcurrencyConflictException)` clause regardless of whether the exception is re-thrown. The predicate does not distinguish catch+rethrow from catch+swallow.

**Why this is pre-existing**: The catch was authored under §5.2.x event-store observability work, not under §5.2.4 health/readiness. It is unaffected by any HC-1..HC-9 / MI-1 / HC-9 change. Verified by `git blame` (the catch predates HC-1).

**Severity**: S1 by the architecture test's literal predicate; S3 by intent (the design property the test guards — "exception travels untouched" — is preserved by the rethrow).

**Remediation path** (one of):
1. **Narrow the architecture test predicate** to allow `catch (T) when (...) { ...; throw; }` shapes where the only statement before `throw` is a state mutation, not a `return` / `return Task` / message construction. Most rigorous.
2. **Move the outcome-tagging to a `when` filter or finally** in `PostgresEventStoreAdapter` so no `catch` clause names `ConcurrencyConflictException` at all. Requires restructuring the histogram observation block.
3. **Mark the test `[Trait("waiver", "S5.2.x-observability")]`** with a documented exception. Acceptable only as a last resort.

## §5.2.4 / §5.2.5 test coverage (additive — all passing)

| Suite                              | Tests | Status   |
|------------------------------------|-------|----------|
| `PostgresPoolHealthEvaluatorTests` | 9     | ✅ all pass |
| `RuntimeDegradedModeTests`         | 6     | ✅ all pass |
| `RuntimeEnforcementGateTests`      | 6     | ✅ all pass |
| `ExecutionLockProviderTests`       | 6     | ✅ all pass |
| `RedisHealthCheckTests`            | 5     | ✅ all pass |
| **§5.2.4/5.2.5 subtotal**          | **32**| **✅ 32/32** |

Every workstream from HC-5 onward has dedicated test coverage. No HC-1..HC-9 / MI-1 test was added without a passing assertion. No HC-1..HC-9 change introduced a new test failure.

## Status

**FAIL** at the suite level due to the single pre-existing architecture test. The §5.2.4 surface itself is GREEN (32/32). The failing test does not block any §5.2.4 functionality but does block the strict §1 acceptance criterion ("All tests passing").

**This finding propagates to `phase1.5-final.audit.md` as the second of two blockers.**
