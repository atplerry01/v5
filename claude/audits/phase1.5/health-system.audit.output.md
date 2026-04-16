# Phase 1.5 — Health System Completeness Audit

**STATUS: PASS**
**SCOPE: §2.3 of phase1.5-final closure prompt**
**DATE: 2026-04-09**

## Finding

The health system covers every component listed in the closure matrix. `RuntimeStateAggregator` evaluates the canonical rule chain in the locked priority order with no duplication and no severity drift.

## Component coverage

| Component       | Check / Source                                      | Workstream |
|-----------------|-----------------------------------------------------|------------|
| Postgres pool   | `PostgreSqlHealthCheck` + `PostgresPoolHealthEvaluator` | HC-6       |
| Workers         | `WorkersHealthCheck` (outbox-sampler / kafka-outbox-publisher / projection-consumer) | HC-5 |
| Redis           | `RedisHealthCheck` (Ping probe via singleton multiplexer) | HC-9       |
| OPA             | `OpaHealthCheck` + `OpaPolicyEvaluator.IsBreakerOpen` | HC-2 / pre-existing |
| Chain           | `WhyceChainPostgresAdapter.IsBreakerOpen` (read by aggregator) | HC-2       |
| Outbox          | `IOutboxDepthSnapshot.IsFresh` + `CurrentDepth` vs `HighWaterMark` | HC-1       |

## Locked rule chain (verified against [src/platform/host/health/RuntimeStateAggregator.cs](src/platform/host/health/RuntimeStateAggregator.cs))

```
1.  host_draining                                 → NotReady
2.  critical_healthcheck_failed                   → NotReady   (excludes "postgres" and "redis")
2c. redis_unhealthy                               → NotReady   (HC-9, MI-1 dispatch dependency)
2b. postgres_pool_exhausted /                     → NotReady   (HC-6 via PostgresPoolHealthEvaluator)
    postgres_acquisition_failures (windowed) /
    postgres_invalid_pool_config
3.  worker_unhealthy                              → NotReady   (HC-5)
4.  outbox_snapshot_stale                         → NotReady   (HC-1)
--- Degraded contributors (collected, not short-circuiting) ---
5.  redis_degraded_latency                        → Degraded   (HC-9)
6.  postgres_high_wait                            → Degraded   (HC-6)
7.  opa_breaker_open                              → Degraded
8.  chain_anchor_breaker_open                     → Degraded
9.  outbox_over_high_water_mark                   → Degraded
10. noncritical_healthcheck_failed                → Degraded   (excludes postgres, redis, workers)
11. else                                          → Healthy
```

## Endpoint surface

- `GET /Health` — full IHealthCheck fan-out + canonical state + reasons + per-service detail (HC-2)
- `GET /Health/ping` — backwards-compatibility liveness
- `GET /live` — process aliveness, no aggregation (HC-3)
- `GET /ready` — consumes `IRuntimeStateAggregator.GetCurrentStateAsync`; flips on host drain via `ApplicationStopping` (HC-3)
- All four routes are exempt from the PC-1 rate limiter via `[DisableRateLimiting]` on `HealthController` (HC-4)

## No duplication

Three exclusions guarantee a single canonical reason per failure mode:
- `"postgres"` is excluded from the critical-services scan and the noncritical-Degraded scan because the pool evaluator owns it (HC-6).
- `"redis"` is excluded from the critical-services scan and the noncritical-Degraded scan because the Redis-specific rule owns it (HC-9).
- `"workers"` is excluded from the noncritical-Degraded scan because the worker rule owns it (HC-5).

## Dispatch-time degraded posture

`IRuntimeStateAggregator.GetDegradedMode()` (HC-7) reads only in-process signals (OPA breaker, chain breaker, outbox HWM, postgres pool high-wait) — no IHealthCheck fan-out. The result is filtered through `RuntimeDegradedMode.CanonicalReasons` (6 entries after HC-9). Stamped onto `CommandContext.DegradedMode` before the middleware pipeline runs. Locked by `RuntimeDegradedModeTests.CanonicalReasonSet_MatchesSpec`.

## Result

PASS. All six required components are covered. Rule order is deterministic and exclusions prevent double-counting.
