# Phase 1.5 — Failure Semantics Audit

**STATUS: PASS** (with one S2 observation, see §Observations)
**SCOPE: §2.6 of phase1.5-final closure prompt**
**DATE: 2026-04-09**

## Finding

Every §5.2.4 / §5.2.5 failure path returns a deterministic `CommandResult.Failure(reason)` with a low-cardinality canonical identifier. The Redis lock provider is exception-free under outage. The reason vocabulary is finite and audit-visible.

## Canonical reason vocabulary (Phase 1.5 §5.2.4 / §5.2.5)

### Hard-block CommandResult reasons (returned via `CommandResult.Failure`)

| Reason                              | Origin                              | Workstream |
|-------------------------------------|-------------------------------------|------------|
| `execution_lock_unavailable`        | `RuntimeControlPlane` (lock acquire false, no cancellation) | HC-9 |
| `execution_cancelled`               | `RuntimeControlPlane` (lock acquire false, CT cancelled) | HC-9 |
| `system_maintenance_mode`           | `RuntimeEnforcementGate.BlockMaintenance` | HC-8 |
| `restricted_during_degraded_mode`   | `RuntimeEnforcementGate.BlockRestricted`  | HC-8 |

### Aggregator reason vocabulary (`RuntimeStateSnapshot.Reasons`)

NotReady class:
- `host_draining`
- `critical_healthcheck_failed`
- `redis_unhealthy`
- `postgres_invalid_pool_config`
- `postgres_pool_exhausted`
- `postgres_acquisition_failures`
- `worker_unhealthy`
- `outbox_snapshot_stale`

Degraded class (= `RuntimeDegradedMode.CanonicalReasons`, locked by test, size 6):
- `postgres_high_wait`
- `opa_breaker_open`
- `chain_anchor_breaker_open`
- `outbox_over_high_water_mark`
- `noncritical_healthcheck_failed`
- `redis_degraded_latency`

All identifiers are snake_case, low-cardinality, never include per-instance payload, never include correlation IDs.

## No exceptions used for control flow

Verified at the §5.2.4 surface:

| Path                        | Exception-free? | Verified at |
|-----------------------------|-----------------|-------------|
| Redis lock acquire          | ✅              | `RedisExecutionLockProvider.TryAcquireAsync` catch-all (HC-9) |
| Redis lock release          | ✅              | `RedisExecutionLockProvider.ReleaseAsync` catch-all (HC-9) |
| Maintenance / degraded enforcement | ✅       | `RuntimeEnforcementGate.Evaluate` returns enum, `ExecuteAsync` translates |
| Health check fan-out        | ✅              | Each `IHealthCheck` returns `HealthCheckResult`; exceptions captured per-check |
| Pool snapshot               | ✅              | `PostgresPoolSnapshotProvider` is pure read |
| Degraded mode evaluation    | ✅              | `RuntimeStateAggregator.GetDegradedMode` reads in-process state only |

## Observations (S2 — non-blocking but worth recording)

**Pre-existing typed-exception refusal handlers (§5.2.1 / §5.2.2 / §5.2.3) are NOT touched by §5.2.4.** The runtime continues to use `IExceptionHandler` mappings for:
- `OutboxSaturatedException` → 503
- `PolicyEvaluationUnavailableException` → 503
- `WorkflowSaturatedException` → 503
- `ChainAnchorWaitTimeoutException` → 503
- `ChainAnchorUnavailableException` → 503
- `WorkflowTimeoutException` → 503
- `ConcurrencyConflictException` → 409

These are intentional, declared, edge-bounded refusal families with documented `Retry-After` semantics. They are NOT used as runtime control flow — they bubble untouched from a single throw site to a single edge handler. This is the canonical Phase 1.5 §5.2.x discipline and is consistent with the "no exception-based control flow in runtime path" rule (the exceptions cross the runtime, but they exit it via a typed handler at the API edge — they do not branch behavior inside the runtime).

**S2 finding**: `PostgresEventStoreAdapter.cs:237` has a `catch (ConcurrencyConflictException) when (outcome == "ok") { outcome = "concurrency_conflict"; throw; }`. This catch is **observability-only** — it sets the histogram outcome tag and re-throws unmodified. The `WbsmArchitectureTests.No_upstream_layer_catches_ConcurrencyConflictException` test rejects ANY catch of this type upstream of the API edge regardless of whether the exception is re-thrown. The test failure is documented in `test-verification.audit.md`. This is NOT a control-flow violation by intent (the exception is re-thrown unmodified), but the test predicate does not distinguish catch+rethrow from catch+swallow. Resolution path is one of: (a) narrow the test predicate to allow catch+rethrow when no swallowing occurs, (b) move the histogram outcome tagging to a finally block / exception filter without a catch clause.

## Result

PASS for the canonical §5.2.4 / §5.2.5 failure surface. The S2 observation above is recorded for closure remediation but does not violate the §2.6 acceptance criteria — the failing test reflects a pre-existing observability pattern, not a §5.2.4 regression.
