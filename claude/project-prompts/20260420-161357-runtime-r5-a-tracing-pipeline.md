# R5.A — OTEL Tracing Pipeline

Classification: runtime
Context: observability / tracing
Domain: span-emission + correlation

## TITLE
R5.A — Bolt an OpenTelemetry tracing pipeline onto the existing runtime so every command/operator-action has a span and R4.A signals are drillable.

## CONTEXT
R4.A packaged metrics. R4.B + R5.B gave operators governed control + certified the fault-to-alert mapping. What's missing: when an alert fires or a command fails, the operator has metric aggregates and audit events but no request-scoped execution trace. R5.A closes that gap.

The runtime emits no `Activity` / `ActivitySource` instruments today (recon confirmed zero). Adding tracing from scratch is bounded: `System.Diagnostics.ActivitySource` lives in the .NET stdlib (no new runtime dependency), and the OTEL exporter stack lives only in the platform host.

## OBJECTIVE
Deliver a bounded Phase 1 of tracing:
- canonical `ActivitySource` constants per subsystem (runtime-layer)
- instrumentation at the two highest-value seams: `SystemIntentDispatcher.DispatchAsync` (every command) and `OperatorActionAuditRecorder.RecordAsync` (every operator action)
- OTEL composition module in the host: `TracerProvider` with AspNetCore + Http auto-instrumentation + OTLP exporter
- trace/correlation bridge: every span carries `whyce.correlation_id`; the existing correlation middleware echoes the current trace id on the response as `traceresponse` header
- Jaeger all-in-one wired into `infrastructure/deployment/docker-compose.yml`
- guard rules that lock span-source vocabulary + attribute naming + export-endpoint shape
- validation tests that pin the vocabulary as an executable invariant

Deep seam instrumentation (event-fabric batches, outbound-effect lifecycle, workflow engine steps), prometheus-net exemplars, and custom samplers are **R5.A Phase 2** — deferred by design.

## CONSTRAINTS
- Runtime layer MUST only use `System.Diagnostics.ActivitySource` (stdlib). No OTEL NuGet reference in `Whycespace.Runtime.csproj` — that would break layer discipline (DG-R5-01: runtime depends on Shared only).
- OTEL NuGet packages live exclusively in `Whycespace.Host.csproj`.
- Span names MUST be low-cardinality canonical strings (e.g. `runtime.command.dispatch`). Command type / workflow name / action type go on **attributes**, not in the span name.
- Span attributes MAY include higher-cardinality dimensions (correlation_id, aggregate_id, actor_id) — the low-cardinality rule from R4.A applies to *metrics*, not trace attributes. Jaeger/Tempo handle them natively.
- NO broad instrumentation sweep in Phase 1 — only the two declared seams.
- NO new metrics in R5.A.
- Existing metrics + R4.A dashboards + R5.B certification surface MUST remain unchanged.

## EXECUTION STEPS (as delivered)
1. Prompt stored per $2.
2. `src/runtime/observability/WhyceActivitySources.cs` — canonical source names + span-name constants.
3. Instrument `SystemIntentDispatcher.DispatchAsync` (spans wrap dispatch with canonical attributes: command.type, classification, context, domain, aggregate.id, actor.id, correlation.id). Spans emit on both success and failure paths via `Activity.SetStatus`.
4. Instrument `OperatorActionAuditRecorder.RecordAsync` (spans wrap audit emission with action_type, target_resource_type, target_id, outcome, operator.id).
5. `src/platform/host/composition/infrastructure/observability/TracingInfrastructureModule.cs` — `AddOpenTelemetry().WithTracing(...)` registering the canonical sources + AspNetCore + Http auto-instrumentation + OTLP exporter. Exporter endpoint from `Otel:Endpoint` config with a sane default.
6. `src/platform/api/middleware/TraceCorrelationMiddleware.cs` — on inbound request, tag the current Activity with `whyce.correlation_id` from the correlation bag; on response, stamp `traceresponse` header.
7. OTEL NuGet packages added to `Whycespace.Host.csproj`.
8. Wire middleware into `Program.cs` (after CorrelationIdMiddleware, before routing).
9. Wire `TracingInfrastructureModule` into `InfrastructureCompositionRoot` (after Authentication, parallel to AddAdminAuthorization).
10. Add Jaeger all-in-one service to `infrastructure/deployment/docker-compose.yml` + ports 16686 (UI) + 4317 (OTLP gRPC).
11. Promote R5.A guard rules into `runtime.guard.md` §R5.A Tracing Pipeline.
12. Validation tests: ActivitySource names + span-name constants exist, instrumented seams reference the canonical source, Host csproj declares the OTEL packages, Jaeger service is wired in docker-compose.
13. Sweep record per $1b.

## OUTPUT FORMAT
Summary in conversation: files, spans added, packages added, guards promoted, validation performed, deferred scope.

## VALIDATION CRITERIA
- All R5.A validation tests pass.
- Host project compiles with the new OTEL packages.
- `SystemIntentDispatcher` + `OperatorActionAuditRecorder` both reference `WhyceActivitySources`.
- No new dependencies in `Whycespace.Runtime.csproj` (layer discipline preserved).
- All prior R4.A / R4.B / R5.B tests remain green.

## DEFERRED (R5.A Phase 2 / R5.C scope)
- Span instrumentation at event-fabric / outbound-effect lifecycle / workflow engine / admin controllers (AspNetCore auto-instrumentation gives baseline HTTP spans; canonical attributes per subsystem are Phase 2).
- prometheus-net exemplars linking histogram buckets to trace ids.
- Custom samplers / head-based sampling tuning.
- Log correlation (ILogger scope enrichment with trace id).
- Tempo / Grafana Loki integration for trace-log join.
- Sustained-load + replay certification (R5.C).
