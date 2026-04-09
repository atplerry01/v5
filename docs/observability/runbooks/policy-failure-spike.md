# Runbook — Policy failure spike

**STATUS:** TEMPLATE (resolution playbook is placeholder)

## Symptom

One or more of:

- `policy.evaluate.timeout` rising (`Whyce.Policy`).
- `policy.evaluate.breaker_open` rising.
- `policy.evaluate.failure` rising (tagged by `reason`).
- API edge returning 503 with the canonical Retry-After header for
  `PolicyEvaluationUnavailableException`.
- Aggregated runtime state surfacing `opa_breaker_open` as a degraded
  reason (per R-RT-04 vocabulary).

## Linked SLO(s)

- [F-5 — Policy evaluation timeout rate](../slo/failure-rate-slos.md#f-5--policy-evaluation-timeout-rate)
- [F-6 — Policy breaker-open rate](../slo/failure-rate-slos.md#f-6--policy-breaker-open-rate)
- [F-7 — Policy generic failure rate](../slo/failure-rate-slos.md#f-7--policy-generic-failure-rate)
- [L-1 — Policy evaluation duration](../slo/latency-slos.md#l-1--policy-evaluation-duration)
- [R-4 — Policy breaker recovery time](../slo/recovery-slos.md#r-4--policy-breaker-recovery-time)

## Severity guide (TEMPLATE)

| Severity | Condition | Routing |
|----------|-----------|---------|
| S0 | `TBD` (e.g. breaker open globally for ≥ X minutes) | `TBD` |
| S1 | `TBD` | `TBD` |
| S2 | `TBD` | `TBD` |
| S3 | `TBD` | `TBD` |

## Triage (real and actionable today)

1. **Identify which failure class is rising.** Query the three
   counters together and read the dominant tag:
   - `policy.evaluate.timeout`     → OPA is reachable but slow.
   - `policy.evaluate.breaker_open`→ OPA breaker has tripped (≥
                                     `BreakerThreshold` failures within
                                     `BreakerWindowSeconds`).
   - `policy.evaluate.failure{reason=http_status}` → OPA is returning
                                                     non-2xx.
   - `policy.evaluate.failure{reason=transport}`   → network / DNS
                                                     issue between
                                                     runtime and OPA.
2. **Check OPA-side health.** Inspect the OPA process logs and the
   OPA admin endpoint. Confirm OPA is running, listening, and not
   itself in an error state.
3. **Confirm fail-closed posture.** A policy spike DOES NOT cause
   silent allows — every refused command is observable as a 503 at
   the API edge with reason `PolicyEvaluationUnavailableException`.
   This is the canonical FR-4 invariant — see
   [tests/integration/failure-recovery/PolicyEngineFailureTest.cs](../../../tests/integration/failure-recovery/PolicyEngineFailureTest.cs).
4. **Cross-check the breaker math.** The breaker config (defaults
   in `OpaOptions`) is the natural recovery floor. If `breaker_open`
   is rising, the breaker has accumulated `BreakerThreshold` failures
   inside `BreakerWindowSeconds`. Operators cannot reset the breaker
   manually; it self-heals after the window expires AND a successful
   call is observed.
5. **Inspect call duration.** `policy.evaluate.duration` histogram
   tells you whether successful calls are also slow. If p99 is near
   `RequestTimeoutMs`, the timeout budget itself may be the issue
   (and changing it requires a config push, not a hot fix).

## Mitigation (TEMPLATE)

- `TBD` — define short-term steps:
  - Increase `OpaOptions.RequestTimeoutMs` (config push, requires
    rollout).
  - Failover to a standby OPA cluster (if one exists).
  - Drain affected runtime instances.

## Resolution (TEMPLATE)

- `TBD` — root-cause closure.

## Post-incident (TEMPLATE)

- `TBD` — verify:
  - All three policy failure counters returned to baseline.
  - `policy.evaluate.duration` p99 is back below the SLO target
    (once defined).
  - Aggregator no longer reports `opa_breaker_open`.

## References

- Code: [src/platform/host/adapters/OpaPolicyEvaluator.cs](../../../src/platform/host/adapters/OpaPolicyEvaluator.cs)
- Code: [src/runtime/middleware/policy/PolicyMiddleware.cs](../../../src/runtime/middleware/policy/PolicyMiddleware.cs)
- Contract: [src/shared/contracts/infrastructure/policy/PolicyEvaluationUnavailableException.cs](../../../src/shared/contracts/infrastructure/policy/PolicyEvaluationUnavailableException.cs)
- Tests: [tests/integration/failure-recovery/PolicyEngineFailureTest.cs](../../../tests/integration/failure-recovery/PolicyEngineFailureTest.cs) (FR-4)
- SLO map: [docs/observability/slo/metric-mapping.md](../slo/metric-mapping.md)
