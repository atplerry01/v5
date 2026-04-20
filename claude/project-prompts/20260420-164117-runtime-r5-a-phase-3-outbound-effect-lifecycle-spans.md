# R5.A Phase 3 — Outbound-Effect Lifecycle Spans

Classification: runtime
Context: observability / tracing
Domain: span-emission

## TITLE
R5.A Phase 3 — Complete span coverage for the outbound-effect lifecycle: per-dispatch attempt, finality, and operator reconcile.

## CONTEXT
Phase 2 covered the outbound-effect **schedule** entrypoint. What remains for operator-visible observability: the lifecycle transitions the runtime applies after scheduling — the relay's per-attempt dispatch (the actual provider call), the finality service's finalization path (async callback outcome), and the operator-driven reconcile path (R4.B admin surface). Each currently shows only as a metric/count on the R4.A dashboards; operators cannot trace individual effect lifecycles through Jaeger.

**Workflow engine per-step instrumentation is intentionally deferred to Phase 4** — the `WorkflowStepExecutor` lives in `Whycespace.Engines.T1M`, which does NOT reference `Whycespace.Runtime`. Instrumenting it requires either moving `WhyceActivitySources` constants to the shared kernel or duplicating the vocabulary across layers. Either decision is a clean Phase 4 scope; bundling it with lifecycle spans here would muddle the goals.

## OBJECTIVE
Deliver a bounded Phase 3:
- 3 new canonical span names: `outbound.effect.dispatch`, `outbound.effect.finalize`, `outbound.effect.reconcile`
- span instrumentation in `OutboundEffectRelay.DispatchOneAsync`, `OutboundEffectFinalityService.FinalizeAsync`, `OutboundEffectFinalityService.ReconcileAsync`
- 1 new canonical attribute key: `attempt.number` (retry visibility on dispatch spans)
- extended guard rules + validator tests
- explicit deferral rule for workflow engine step instrumentation

## CONSTRAINTS
- Runtime-layer stdlib only (no OTEL NuGet).
- No new metrics.
- Spans wrap the HAPPY PATH of each seam; exceptions re-propagate with Error status + exception type name.
- Dispatch span captures per-ATTEMPT granularity, not per-effect-lifetime — a retrying effect gets multiple dispatch spans (one per attempt), which is the operator-useful shape.
- Reconcile span is on the `FinalityService.ReconcileAsync` seam (operator-driven). The sweeper's internal `MarkReconciliationRequiredAsync` is NOT span-wrapped — it runs on a background worker timer, not a per-request path; adding spans there pollutes span storage with background noise. Documented.

## EXECUTION STEPS (as delivered)
1. Prompt stored per $2.
2. Extend `WhyceActivitySources.Spans` with the 3 new constants; add `AttemptNumber` attribute key.
3. Instrument `OutboundEffectRelay.DispatchOneAsync` with `outbound.effect.dispatch` span carrying provider, effect type, attempt number, result classification.
4. Instrument `OutboundEffectFinalityService.FinalizeAsync` with `outbound.effect.finalize` span carrying effect id, provider, outcome, finality source, whether compensation was emitted.
5. Instrument `OutboundEffectFinalityService.ReconcileAsync` with `outbound.effect.reconcile` span carrying effect id, outcome, reconciler actor id, whether compensation was emitted.
6. Promote R5.A Phase 3 guard rules into `runtime.guard.md` §R5.A.
7. Validation tests pinning the new vocabulary + instrumentation.
8. Sweep record per $1b.

## OUTPUT FORMAT
Summary in conversation: new spans, guards, validation, deferred scope.

## VALIDATION CRITERIA
- All new tests pass; existing R4/R5 tests remain green.
- Runtime csproj remains OTEL-free.
- TracingInfrastructureModule continues to register only the 4 canonical sources (Phase 3 reuses `OutboundEffectsName`).

## DEFERRED (R5.A Phase 4 / R5.C scope)
- **Workflow engine per-step spans** — requires either moving `WhyceActivitySources` name constants to the shared kernel (clean but cross-layer split) or duplicating the vocabulary in engines (violates single-source-of-truth). Phase 4.
- **Sweeper internal spans** (`MarkReconciliationRequiredAsync`, poll loops) — background-worker noise; not operator-useful.
- **Compensation dispatch spans** — downstream of finality; deferred to Phase 4 pending layer design.
- **Sustained-load + replay certification** — R5.C.
