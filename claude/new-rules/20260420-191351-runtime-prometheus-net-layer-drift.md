---
CLASSIFICATION: runtime
SOURCE: full-system audit sweep of src/runtime/ (2026-04-20 19:13)
SEVERITY: S1
---

# PROPOSED RULE — R-TRACE-LAYER-DISCIPLINE-01 must ban prometheus-net in runtime, not only OpenTelemetry.*

## DESCRIPTION

`claude/guards/runtime.guard.md` rule `R-TRACE-LAYER-DISCIPLINE-01` (§R5.A, line 4649) declares that `src/runtime/Whycespace.Runtime.csproj` "MUST NOT reference any `OpenTelemetry.*` NuGet package" and asserts "runtime depends only on Shared".

Current state drifts against both claims:

1. `src/runtime/Whycespace.Runtime.csproj:17` carries `<PackageReference Include="prometheus-net" Version="8.2.1" />`, used by `src/runtime/observability/HttpMetricsMiddleware.cs` (imports `using Prometheus;`, instantiates `Metrics.CreateHistogram/CreateCounter` statics).
2. The companion rule `R-TRACE-EXEMPLAR-DEFERRED-01` (line 4711) itself states: *"switching the runtime histograms to native prometheus-net would force a `prometheus-net` NuGet reference into `Whycespace.Runtime.csproj`, violating `R-TRACE-LAYER-DISCIPLINE-01`"*. The csproj already carries the reference — so by the guard's own reasoning, the rule is currently violated.
3. The pinning test `Runtime_csproj_does_not_reference_opentelemetry` only matches `OpenTelemetry.*`, so prometheus-net slipped past the gate. The gate enforces the letter of the rule (OTel-family packages), not the stated invariant ("stdlib only" / "only Shared").
4. The csproj also depends on `Whycespace.Domain` and `Whycespace.Engines`, so the "runtime depends only on Shared" phrasing is incorrect independent of prometheus-net.

The R5 closure record (`claude/audits/sweeps/20260420-175949-r5-shipped-closure-record.md` §5 Extension H) restates the claim that prometheus-net in runtime would be a layer violation — this is now load-bearing in the closure narrative but false in fact.

## PROPOSED RULE

Either:
- **(a)** Tighten `R-TRACE-LAYER-DISCIPLINE-01` to explicitly ban `prometheus-net` alongside `OpenTelemetry.*`, remove the runtime csproj reference, and relocate `HttpMetricsMiddleware` (and any `prometheus-net` histograms) into `src/platform/` where prometheus-net already belongs. Extend the pinning architecture test from `*OpenTelemetry*` to a closed allow-list of permitted runtime NuGet packages, OR
- **(b)** Explicitly carve out `HttpMetricsMiddleware` as the acknowledged single exception, update `R-TRACE-LAYER-DISCIPLINE-01` + `R-TRACE-EXEMPLAR-DEFERRED-01` to reflect reality (rule no longer says runtime must not have prometheus-net; exemplar-deferral rationale stops citing a violation that does not exist), and add a guard rule locking the exception to exactly that one file.
- In either path, correct the misstated "runtime depends only on Shared" to the accurate dependency set (Shared + Domain + Engines).

Recommend **(a)** — the R5 closure treats prometheus-net-in-runtime as a violation and the body of the exemplar-deferral argument relies on it being forbidden. (b) would weaken a load-bearing R5 claim.

## PROPOSED PINNING TEST

Replace the `*OpenTelemetry*` substring scan with an allow-list assertion over `Whycespace.Runtime.csproj`'s PackageReference nodes: `{Microsoft.AspNetCore.Http.Abstractions, Microsoft.Extensions.DependencyInjection.Abstractions, System.Threading.RateLimiting}`. Any drift fails red.
