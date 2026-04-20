---
CLASSIFICATION: runtime
SOURCE: full-system audit sweep of src/runtime/ (2026-04-20 19:13)
SEVERITY: S2
---

# PROPOSED RULE — Dead static helpers on HttpMetricsMiddleware

## DESCRIPTION

`src/runtime/observability/HttpMetricsMiddleware.cs` exposes two public static helpers that have zero callers repo-wide:

- `HttpMetricsMiddleware.TrackPolicyEvaluation()` (line 93)
- `HttpMetricsMiddleware.TrackEngineExecution(string tier, string engineName)` (line 98)

Grep across `src/**`, `tests/**`, and all composition entry points returns only the definitions themselves — no invocations. The associated `PolicyEvaluationDuration` and `EngineExecutionDuration` histograms (lines 42-57) are therefore never observed; they occupy the prometheus-net Metrics registry without producing data.

This duplicates pattern-equivalent policy/engine timing now emitted via `System.Diagnostics.Metrics` in `MetricsMiddleware.cs` — whose docstring itself admits the same anti-pattern was cleaned up in R1 Batch 4 under `R-STATE-BOUNDARY-01` ("Both getters had zero callers and the counters duplicated the OpenTelemetry counters already published below — removed to honor the state-boundary rule and the clean-code guard.").

The same cleanup has not been applied to `HttpMetricsMiddleware.TrackPolicyEvaluation` / `TrackEngineExecution`.

## PROPOSED RULE

Extend `runtime.guard.md` code-quality §no-dead-code enforcement to include a pinning test that fails red if any **public** API on a runtime-layer `*.Middleware` class has zero callers across `src/` AND `tests/`. The rule must specifically cover:

- All `public` / `public static` members of middleware classes in `src/runtime/middleware/**` and `src/runtime/observability/*Middleware*.cs`.

Remediation for the current hits:
- Delete `HttpMetricsMiddleware.TrackPolicyEvaluation()`, `HttpMetricsMiddleware.TrackEngineExecution()`, and the two unused histograms (`PolicyEvaluationDuration`, `EngineExecutionDuration`).
- Remediation naturally bundles with the prometheus-net layer-drift remediation (new-rules entry `20260420-191351-runtime-prometheus-net-layer-drift.md`) — if `HttpMetricsMiddleware` moves out of runtime, these dead helpers should not accompany it.

## POINTERS

- `src/runtime/observability/HttpMetricsMiddleware.cs:42-57,93-101`
- Precedent: `src/runtime/middleware/observability/MetricsMiddleware.cs:13-18` (R-STATE-BOUNDARY-01 cleanup)
