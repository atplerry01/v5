# Post-Execution Audit Sweep — R5.A Tracing Pipeline (Phase 1)

Date: 2026-04-20
Prompt: `claude/project-prompts/20260420-161357-runtime-r5-a-tracing-pipeline.md`
Scope: OTEL tracing bootstrap + canonical ActivitySource vocabulary + 2 seam instrumentations + correlation bridge + Jaeger wiring + validators + guards
Stage: $1b (post-execution audit sweep) after successful $1 execution

---

## 1. Guard coverage check

Rules promoted to `claude/guards/runtime.guard.md` → §R5.A Tracing Pipeline:

- [x] R-TRACE-SOURCE-VOCABULARY-01 — canonical `ActivitySource` + span-name constants declared in `WhyceActivitySources`
- [x] R-TRACE-DISPATCH-SPAN-01 — `SystemIntentDispatcher.DispatchAsync` wraps dispatch in a `runtime.command.dispatch` span with the full canonical attribute set + outcome classification on every branch
- [x] R-TRACE-OPERATOR-ACTION-SPAN-01 — `OperatorActionAuditRecorder.RecordAsync` wraps audit emission in a `runtime.admin.operator_action` span on every outcome branch (accepted/refused/failed)
- [x] R-TRACE-EXPORTER-OTEL-01 — OTEL NuGet packages declared in Host csproj; `TracingInfrastructureModule.AddTracing` registers both canonical sources + AspNetCore/Http auto-instrumentation + OTLP exporter; Jaeger service wired in docker-compose
- [x] R-TRACE-CORRELATION-BRIDGE-01 — `TraceCorrelationMiddleware` tags current Activity with `whyce.correlation_id`; echoes `traceresponse` header; middleware registered after `CorrelationIdMiddleware`
- [x] R-TRACE-LAYER-DISCIPLINE-01 — `Whycespace.Runtime.csproj` carries no OTEL NuGet reference; platform_api inlines the attribute key with drift-guard test

---

## 2. Scope-boundary sweep

### What R5.A Phase 1 delivered
- Canonical `ActivitySource` vocabulary: 2 sources (`Whycespace.Runtime.ControlPlane`, `Whycespace.Runtime.Admin`), 2 span names (`runtime.command.dispatch`, `runtime.admin.operator_action`), 13 canonical attribute keys.
- 2 high-value seam instrumentations: command dispatch (every command) + operator-action audit (every admin mutation).
- Full OTEL exporter pipeline: TracerProvider with AspNetCore + Http auto-instrumentation + OTLP (gRPC) exporter targeting Jaeger.
- Trace/correlation bridge: `whyce.correlation_id` on every span; `traceresponse` W3C-format header on every response.
- Jaeger all-in-one service in docker-compose (UI :16686, OTLP :4317).
- 10 validator tests pinning vocabulary + wiring + layer discipline as executable invariants.
- 6 new R5.A guard rules.

### What R5.A Phase 1 explicitly did NOT do
- **No broad seam instrumentation** — only command dispatch + operator action. Event-fabric batches, outbound-effect lifecycle stages, workflow engine steps, admin controllers get the baseline HTTP span from AspNetCore auto-instrumentation but no custom canonical spans yet (Phase 2).
- **No prometheus-net exemplars** — histogram → trace-id linkage deferred to Phase 2.
- **No custom samplers** — `AlwaysOn` behavior is fine for local dev + initial prod; head-based sampling tuning is Phase 2 / R5.C.
- **No log correlation** — ILogger scope enrichment with trace id is Phase 2.
- **No Tempo / Grafana Loki integration** — Phase 2 / R5.C.
- **No new metrics** — R4.A surface unchanged.
- **No new handlers / alerts / failure modes** — R5.B certification surface unchanged.

### Layer-discipline sweep
- `Whycespace.Runtime.csproj`: NO `OpenTelemetry.*` package reference (verified by `Runtime_csproj_does_not_reference_opentelemetry` test).
- `Whycespace.Api.csproj`: NO `Whycespace.Runtime` project reference (pre-existing DG-R5-01 constraint preserved). `TraceCorrelationMiddleware` inlines the canonical attribute key string with a drift-guard test that pins the inline to the runtime-layer constant.
- `Whycespace.Host.csproj`: OTEL NuGet packages confined here.

### Instrumentation correctness sweep
- `SystemIntentDispatcher`: span wraps `_controlPlane.ExecuteAsync`. Try/catch sets `ActivityStatusCode.Error` + outcome tag on unexpected exception then re-throws (exception path untouched per canonical layer rule). Success/failure branches set the status explicitly.
- `OperatorActionAuditRecorder`: span emitted at entry; status set based on outcome constant (accepted → Ok; else Error). Attributes carry the full evidence set mirroring the audit event.

---

## 3. Test coverage

10 new R5.A validator tests in `tests/unit/observability/R5ATracingPipelineTests.cs`:

1. `Canonical_activity_source_names_are_declared`
2. `Canonical_span_names_are_low_cardinality_constants`
3. `SystemIntentDispatcher_uses_canonical_control_plane_source_and_dispatch_span`
4. `OperatorActionAuditRecorder_uses_canonical_admin_source_and_operator_action_span`
5. `Tracing_infrastructure_module_registers_both_canonical_sources`
6. `Host_csproj_declares_required_otel_packages`
7. `Runtime_csproj_does_not_reference_opentelemetry`
8. `Trace_correlation_middleware_uses_canonical_whyce_correlation_id_attribute_key`
9. `Docker_compose_wires_jaeger_service_with_otlp_port`
10. `Program_cs_registers_trace_correlation_middleware_after_correlation_middleware`

Final test-run summary across R4 + R5 surface:
- **Unit tests (architecture + admin + R4.A observability + R5.A tracing + certification): 108/108 pass**
- **Integration tests (exception handler certification): 12/12 pass**
- **Total: 120/120 pass**

---

## 4. Drift / new-rules capture

No drift captured. The only cross-layer discipline issue surfaced during execution (platform_api can't reference runtime) was resolved by inlining the canonical attribute key with a drift-guard test — promoted directly into R-TRACE-LAYER-DISCIPLINE-01.

---

## 5. Files modified / created

### Runtime (3 files)
- `src/runtime/observability/WhyceActivitySources.cs` (new) — canonical vocabulary
- `src/runtime/dispatcher/SystemIntentDispatcher.cs` (extended) — dispatch span
- `src/runtime/control-plane/admin/OperatorActionAuditRecorder.cs` (extended) — operator-action span

### Platform (2 files new, 2 extended)
- `src/platform/host/composition/infrastructure/observability/TracingInfrastructureModule.cs` (new) — TracerProvider + OTLP bootstrap
- `src/platform/api/middleware/TraceCorrelationMiddleware.cs` (new) — bridge
- `src/platform/host/composition/infrastructure/InfrastructureCompositionRoot.cs` (extended) — calls `AddTracing`
- `src/platform/host/Program.cs` (extended) — registers `TraceCorrelationMiddleware`
- `src/platform/host/Whycespace.Host.csproj` (extended) — 4 OTEL packages

### Infrastructure (1 extended)
- `infrastructure/deployment/docker-compose.yml` — Jaeger all-in-one service

### Tests (1 new)
- `tests/unit/observability/R5ATracingPipelineTests.cs` — 10 tests

### Guards (1 extended)
- `claude/guards/runtime.guard.md` — §R5.A Tracing Pipeline (6 rules)

### Prompt + sweep
- `claude/project-prompts/20260420-161357-runtime-r5-a-tracing-pipeline.md`
- `claude/audits/sweeps/20260420-161357-r5-a-tracing-pipeline.md` (this file)

---

## 6. Result

**STATUS: PASS** — R5.A Phase 1 tracing pipeline landed inside the bounded scope. The runtime now emits canonical spans on every command dispatch and every operator action, correlation ids are bridged both directions, the OTLP pipeline is wired end-to-end to Jaeger, and 6 guard rules + 10 validator tests lock the vocabulary.

### Maturity statement (explicit scope boundary)

R5.A Phase 1 delivers **span-level traceability for the two hottest operator paths**: command dispatch and admin operator actions. An operator who sees a 503 on the R4.A control-plane dashboard can pivot via the `traceresponse` header into Jaeger and drill down to the exact command span, see which middleware wrote the failure, and follow the trace into downstream HTTP calls instrumented by auto-instrumentation.

R5.A Phase 1 explicitly does NOT deliver:
- **broader custom-span instrumentation** (event-fabric batches, outbound-effect lifecycle, workflow engine steps) — AspNetCore auto-instrumentation gives the HTTP root span but not subsystem-level drill-down. Phase 2.
- **prometheus-net exemplars** linking R4.A histogram buckets to trace ids — Phase 2 unlocks metric ↔ trace join.
- **Log correlation** (trace id in structured log output) — Phase 2.
- **Sampler tuning + sustained-load cost analysis** — Phase 2 / R5.C.
- **Tempo + Grafana Loki integration** — Phase 2 / R5.C.
- **Sustained-load + replay certification** — R5.C.

The foundation is now in place: anything added later slots into the existing TracerProvider + OTLP pipeline without further infrastructure work.
