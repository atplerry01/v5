# Post-Execution Audit Sweep — R5.A Phase 2 Deeper Instrumentation

Date: 2026-04-20
Prompt: `claude/project-prompts/20260420-163108-runtime-r5-a-phase-2-deeper-instrumentation.md`
Scope: event-fabric spans + outbound-effect dispatch span + log correlation + vocabulary extensions + guards + validators
Stage: $1b (post-execution audit sweep) after successful $1 execution

---

## 1. Guard coverage check

Rules promoted to `claude/guards/runtime.guard.md` → §R5.A Tracing Pipeline (appended):

- [x] R-TRACE-EVENT-FABRIC-SPAN-01 — EventFabric.Process{,Audit}Async emit canonical spans with event_count / aggregate / classification / context / domain / correlation id; exceptions re-propagate with Error status (pinned by `EventFabric_uses_canonical_source_and_process_spans`)
- [x] R-TRACE-OUTBOUND-SCHEDULE-SPAN-01 — OutboundEffectDispatcher.ScheduleAsync emits canonical span with 4 canonical outcome tags (dedup_hit, scheduled, provider_not_registered, options_missing); argument-validation failures stay on caller's span (pinned by `OutboundEffectDispatcher_uses_canonical_source_and_schedule_span`)
- [x] R-TRACE-LOG-CORRELATION-01 — LogCorrelationMiddleware wraps every request in an ILogger scope with trace_id / span_id / correlation_id / tenant_id; registered after TraceCorrelationMiddleware (pinned by 2 tests)
- [x] R-TRACE-EXEMPLAR-DEFERRED-01 — explicit documentation of why System.Diagnostics histogram exemplars are structurally deferred; interim metric↔trace join path documented (traceresponse header + R4.A time-range filters + log-scope trace_id)

Phase 1 guards (R-TRACE-SOURCE-VOCABULARY-01, R-TRACE-DISPATCH-SPAN-01, R-TRACE-OPERATOR-ACTION-SPAN-01, R-TRACE-EXPORTER-OTEL-01, R-TRACE-CORRELATION-BRIDGE-01, R-TRACE-LAYER-DISCIPLINE-01) remain green — Phase 2 extends the source list registered in `TracingInfrastructureModule`.

---

## 2. Scope-boundary sweep

### What Phase 2 delivered
- 2 new `ActivitySource` constants (`EventFabricName`, `OutboundEffectsName`)
- 3 new canonical span names (`event.fabric.process`, `event.fabric.process_audit`, `outbound.effect.schedule`)
- 5 new canonical attribute keys (`event.count`, `provider.id`, `effect.type`, `idempotency.key`, `dedup.hit`)
- 3 seam instrumentations with canonical status + outcome tagging:
  - `EventFabric.ProcessAsync` — every persist → chain → outbox batch
  - `EventFabric.ProcessAuditAsync` — every audit-stream emission (distinct span name)
  - `OutboundEffectDispatcher.ScheduleAsync` — every outbound-effect schedule
- `LogCorrelationMiddleware` — every request is an ILogger scope carrying canonical correlation keys
- 8 new validator tests pinning the extended vocabulary + instrumentation + middleware wiring
- 4 new R5.A guard rules (3 span + 1 log + 1 exemplar-deferred)

### What Phase 2 explicitly did NOT do
- **Workflow engine step spans** — T1M engine seam instrumentation deferred to Phase 3.
- **Full outbound-effect lifecycle spans** — dispatch-loop iteration, ack, finality, reconciliation are Phase 3. Only the schedule entrypoint is covered.
- **Exemplars** — documented as structurally deferred (R-TRACE-EXEMPLAR-DEFERRED-01); System.Diagnostics.Metrics API does not expose exemplar hooks and switching to native prometheus-net would violate layer discipline.
- **Tempo / Grafana Loki integration** — R5.C.
- **Sampler tuning / sustained-load cost analysis** — R5.C.
- **New metrics** — R4.A surface unchanged.
- **New handlers / alerts / failure modes** — R5.B surface unchanged.

### Layer-discipline sweep
- Runtime csproj still OTEL-free — `Runtime_csproj_does_not_reference_opentelemetry` passes.
- platform_api `LogCorrelationMiddleware` uses only `Microsoft.AspNetCore.Http` + `Microsoft.Extensions.Logging` — no reach into runtime.
- New vocabulary constants live in `Whycespace.Runtime.Observability.WhyceActivitySources` as before.

### Instrumentation correctness sweep
- `EventFabric.ProcessAsync` / `ProcessAuditAsync` now async — matches the `IEventFabric` interface contract (`Task` return). No caller-visible behavior change.
- `OutboundEffectDispatcher.ScheduleAsync` span starts AFTER argument-validation so malformed-intent failures stay on the caller's span (explicit choice; documented in the guard rule).
- Exception paths in all three seams re-propagate untouched; span status is set to Error with the exception type name.

---

## 3. Test coverage

New tests:
- `tests/unit/observability/R5APhase2TracingTests.cs` — 8 tests

Final run across the full R4/R5 surface:
- **Unit tests**: 116/116 pass (architecture + admin + R4.A observability + R5.A Phase 1 + R5.A Phase 2 + certification)
- **Integration tests**: 12/12 pass (exception handler certification)
- **Total: 128/128 pass**

Pre-existing failures (PolicyArtifactCoverageTests — 8 failures from missing revenue/payout rego files) remain pre-existing drift unrelated to this work.

---

## 4. Drift / new-rules capture

No new drift. The exemplar structural limitation was anticipated during Phase 1 recon and is now formally documented as R-TRACE-EXEMPLAR-DEFERRED-01 with an explicit interim metric↔trace join path. Not drift — a documented architectural boundary.

---

## 5. Files modified / created

### Runtime (3 extended)
- `src/runtime/observability/WhyceActivitySources.cs` — 2 new sources, 3 new span names, 5 new attribute keys
- `src/runtime/event-fabric/EventFabric.cs` — both fabric methods now span-wrapped
- `src/runtime/outbound-effects/OutboundEffectDispatcher.cs` — ScheduleAsync span-wrapped with 4-way outcome classification

### Platform (1 new, 2 extended)
- `src/platform/api/middleware/LogCorrelationMiddleware.cs` (new) — ILogger scope wrapper
- `src/platform/host/Program.cs` (extended) — registers LogCorrelationMiddleware after TraceCorrelationMiddleware
- `src/platform/host/composition/infrastructure/observability/TracingInfrastructureModule.cs` (extended) — registers EventFabric + OutboundEffects sources

### Tests (1 new)
- `tests/unit/observability/R5APhase2TracingTests.cs` — 8 tests

### Guards (1 extended)
- `claude/guards/runtime.guard.md` — §R5.A extended with 4 new rules

### Prompt + sweep
- `claude/project-prompts/20260420-163108-runtime-r5-a-phase-2-deeper-instrumentation.md`
- `claude/audits/sweeps/20260420-163108-r5-a-phase-2-deeper-instrumentation.md` (this file)

---

## 6. Result

**STATUS: PASS** — R5.A Phase 2 landed inside the bounded scope. The event fabric + outbound-effect dispatcher now emit canonical spans; every request has a trace-joinable log scope; the exemplar structural limitation is documented as a first-class guard rule with an interim workaround.

### Maturity statement (explicit scope boundary)

R5.A Phase 1 + Phase 2 together deliver **four subsystems with canonical span coverage** (runtime control plane, admin surface, event fabric, outbound-effect dispatch) plus **request-scoped log correlation**. An operator investigating an incident can now:

1. Read the `traceresponse` header on the failed HTTP response (Phase 1).
2. Open the trace in Jaeger and drill through: HTTP root (AspNetCore auto) → command dispatch → event fabric persist → outbound-effect schedule, all with canonical routing coordinates on every span.
3. Pivot to logs: filter Seq/Loki/Kibana by `trace_id` to see every log line emitted inside that request, already joined by `correlation_id` for the audit trail.
4. Pivot to metrics: use the R4.A dashboards with the request's time range + matching labels to see aggregate posture at the moment of failure.

R5.A Phase 2 explicitly does NOT deliver:
- **workflow engine step spans** — T1M per-step instrumentation is Phase 3.
- **full outbound-effect lifecycle spans** — the dispatch-loop iteration, ack, finality, and reconciliation transitions are Phase 3.
- **System.Diagnostics histogram exemplars** — structurally blocked; interim workaround documented as canonical.
- **sustained-load cost + replay certification** — R5.C.

The tracing pipeline is now load-bearing across the four most common investigation paths. R5.C (proof under sustained load + replay cert) has the full observability surface to read from.
