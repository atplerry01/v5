# R5.A Phase 2 — Deeper Instrumentation + Log Correlation

Classification: runtime
Context: observability / tracing
Domain: span-emission + log correlation

## TITLE
R5.A Phase 2 — Extend tracing to event-fabric + outbound-effect dispatch, and wire trace↔log correlation so log lines are trace-joinable.

## CONTEXT
R5.A Phase 1 landed: canonical `ActivitySource` vocabulary, two high-value seams (command dispatch + operator-action audit), OTLP → Jaeger pipeline, correlation middleware. Phase 2 extends the coverage to two more canonical seams that are on the critical path for almost every runtime command: the event fabric (persist → chain → outbox orchestration) and the outbound-effect dispatcher (sole schedule seam).

Phase 2 also adds log correlation — every log line emitted inside a request scope carries `trace_id` + `correlation_id` so operators can pivot between logs, traces, and metrics without scraping raw timestamps.

## OBJECTIVE
Deliver a bounded Phase 2:
- 3 new canonical span names: `event.fabric.process`, `event.fabric.process_audit`, `outbound.effect.schedule`
- span instrumentation in `EventFabric.ProcessAsync` + `ProcessAuditAsync` + `OutboundEffectDispatcher.ScheduleAsync`
- trace↔log correlation middleware that wraps every request in an ILogger scope carrying `trace_id`, `span_id`, `correlation_id`, `tenant_id`
- guard rules locking the new vocabulary + log-scope contract
- validation tests pinning the new instrumentation + scope invariants

Deferred (explicitly):
- **Exemplars on System.Diagnostics.Metrics histograms** — blocked upstream by the .NET stdlib API not yet exposing exemplar hooks on `Histogram<T>`. prometheus-net's native histograms support exemplars, but all high-value runtime histograms use the stdlib API. Switching them to native prometheus-net would violate the runtime layer rule (prometheus-net is a Host-tier NuGet). Structural limitation, documented inline.
- Workflow engine step spans, outbound-effect lifecycle spans — Phase 3.
- Tempo/Loki integration, Grafana trace-log join UI — R5.C.

## CONSTRAINTS
- Runtime layer stays stdlib-only (no OTEL NuGet).
- No new metrics, no new handlers, no new alerts.
- Existing spans + R4/R5 test surface remain unchanged.
- Log-scope keys MUST match the canonical trace attribute vocabulary so Grafana / Kibana / Seq trivially join on them.
- Log-correlation middleware registered AFTER `CorrelationIdMiddleware` + `TraceCorrelationMiddleware` so both ids are in scope.

## EXECUTION STEPS (as delivered)
1. Prompt stored per $2.
2. Extend `WhyceActivitySources` with `EventFabricName`, `OutboundEffectsName`, and the 3 new span-name constants.
3. Instrument `EventFabric.ProcessAsync` + `ProcessAuditAsync` with canonical spans carrying event count, aggregate id, classification/context/domain, correlation id. Distinguish `audit` via the dedicated span name.
4. Instrument `OutboundEffectDispatcher.ScheduleAsync` with canonical span carrying provider id, effect type, idempotency key, dedup-hit outcome.
5. Extend `TracingInfrastructureModule` to register the two new sources.
6. Add `LogCorrelationMiddleware` in platform/api — wraps each request in an ILogger scope with `trace_id`, `span_id`, `correlation_id`, `tenant_id` so downstream `ILogger<T>.Log...` calls inherit those fields.
7. Wire `LogCorrelationMiddleware` into `Program.cs` after `TraceCorrelationMiddleware`.
8. Promote R5.A Phase 2 guard rules into `runtime.guard.md` §R5.A.
9. Validation tests for the new span instrumentation + log scope wiring + deferred-exemplar rationale.
10. Sweep record per $1b.

## OUTPUT FORMAT
Summary in conversation: files, new spans, log-correlation middleware, guards, validation, deferred.

## VALIDATION CRITERIA
- 3 new validator tests pass (event-fabric span, outbound-dispatcher span, log-correlation middleware).
- Existing 108 unit + 12 integration tests remain green.
- Runtime csproj remains OTEL-free (layer discipline).
- TracingInfrastructureModule registers all 4 canonical sources.

## DEFERRED (Phase 3 / R5.C)
- Workflow engine per-step spans (T1M engine seam).
- Full outbound-effect lifecycle spans (dispatch-loop iteration, ack, finality, reconciliation).
- System.Diagnostics histogram exemplars — BLOCKED structurally; see R-TRACE-EXEMPLAR-DEFERRED-01.
- Tempo + Grafana trace-log join UI.
- Sustained-load certification (R5.C).
