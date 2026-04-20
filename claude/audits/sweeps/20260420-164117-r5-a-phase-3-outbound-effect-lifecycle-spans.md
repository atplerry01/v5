# Post-Execution Audit Sweep — R5.A Phase 3 Outbound-Effect Lifecycle Spans

Date: 2026-04-20
Prompt: `claude/project-prompts/20260420-164117-runtime-r5-a-phase-3-outbound-effect-lifecycle-spans.md`
Scope: 3 outbound-effect lifecycle seams (dispatch / finalize / operator-reconcile) + deferral of workflow engine spans + guards + validators
Stage: $1b (post-execution audit sweep) after successful $1 execution

---

## 1. Guard coverage check

Rules promoted to `claude/guards/runtime.guard.md` → §R5.A Tracing Pipeline (appended):

- [x] R-TRACE-OUTBOUND-DISPATCH-SPAN-01 — per-attempt dispatch span with ActivityKind.Client + canonical outcome tag matching the meter's `OutcomeTag(result)` vocabulary (pinned by `OutboundEffectRelay_uses_dispatch_span_with_attempt_number`)
- [x] R-TRACE-OUTBOUND-FINALIZE-SPAN-01 — per-transition finalize span with outcome + finality_source + compensation_emitted
- [x] R-TRACE-OUTBOUND-RECONCILE-SPAN-01 — per-operator-reconcile span with reconciler.actor_id + compensation_emitted; sweeper path explicitly unwrapped (pinned by `Phase3_sweeper_MarkReconciliationRequired_is_intentionally_unwrapped`)
- [x] R-TRACE-WORKFLOW-ENGINE-SPAN-DEFERRED-01 — workflow engine per-step spans formally deferred with two resolution paths documented

Phase 1 + Phase 2 guards all remain green. Phase 3 extends the `OutboundEffects` source with 3 new span names; no new source needed.

---

## 2. Scope-boundary sweep

### What Phase 3 delivered
- 3 new canonical span names (`outbound.effect.dispatch`, `outbound.effect.finalize`, `outbound.effect.reconcile`)
- 4 new canonical attribute keys (`attempt.number`, `finality.source`, `compensation.emitted`, `reconciler.actor_id`)
- 3 seam instrumentations at the runtime layer (no layer-discipline issues):
  - `OutboundEffectRelay.DispatchOneAsync` — per-attempt span with ActivityKind.Client + canonical outcome matching the existing meter vocabulary
  - `OutboundEffectFinalityService.FinalizeAsync` — per-transition finalize span with compensation-emitted flag
  - `OutboundEffectFinalityService.ReconcileAsync` — per-operator-reconcile span, natural child of the admin operator-action span
- 5 new validator tests (vocabulary + 3 seam instrumentations + sweeper-unwrapped invariant)
- 4 new guard rules (3 span + 1 deferral)

### What Phase 3 explicitly did NOT do
- **Workflow engine per-step spans** — formally deferred as R-TRACE-WORKFLOW-ENGINE-SPAN-DEFERRED-01. Engine layer doesn't reference runtime; fixing this needs either name-constant move to shared kernel OR a peer EnginesActivitySources. Either is a clean Phase 4 scope; bundling into Phase 3 would muddle goals.
- **Sweeper internal spans** (`MarkReconciliationRequiredAsync` + poll loops) — intentionally unwrapped; background-worker timer noise pollutes span storage. The test pins the unwrapped choice.
- **Compensation dispatcher spans** — downstream of finality emission; deferred.
- **New metrics / alerts / failure modes** — R4.A / R5.B surfaces unchanged.

### Layer-discipline sweep
- Runtime csproj still OTEL-free.
- All 3 instrumentations live in `Whycespace.Runtime.OutboundEffects` — no cross-layer reach.
- Existing outbound-effect integration tests (`OutboundEffectFinalityServiceTests`, 8 tests in tests/integration/integration-system) all pass unchanged — the span wrapping is transparent to the test fabric.

### Instrumentation correctness sweep
- Dispatch span: ActivityKind.Client (the method IS the provider call from distributed-tracing perspective); `attempt.number = entry.AttemptCount + 1` matches the existing attempt-counter contract; outcome tag reuses the existing `OutcomeTag(result)` helper so span ↔ metric alignment is automatic.
- Finalize span: started AFTER arg validation per the Phase 2 precedent; precondition-failure `InvalidOperationException` re-propagates to the caller's span.
- Reconcile span: same pattern; naturally nests as a child of the admin `runtime.admin.operator_action` span when invoked via `/api/admin/outbound-effects/{id}/reconcile`.
- Sweeper path verified unwrapped via a source-scan test that confirms `MarkReconciliationRequiredAsync`'s body does not contain `OutboundEffects.StartActivity`.

---

## 3. Test coverage

New tests:
- `tests/unit/observability/R5APhase3TracingTests.cs` — 5 tests

Final run across the full R4/R5 surface:
- **Unit tests**: 121/121 pass (architecture + admin + R4.A observability + R5.A Phase 1/2/3 + certification)
- **Integration tests**: 20/20 pass (exception handler certification — 12, outbound-effect finality behavior — 8)
- **Total: 141/141 pass**

---

## 4. Drift / new-rules capture

No new drift. The workflow-engine layer-discipline issue is a known architectural boundary that surfaced during Phase 3 scoping and is formally documented as R-TRACE-WORKFLOW-ENGINE-SPAN-DEFERRED-01 with two explicit resolution paths for Phase 4.

---

## 5. Files modified / created

### Runtime (3 extended)
- `src/runtime/observability/WhyceActivitySources.cs` — 3 new span names, 4 new attribute keys
- `src/runtime/outbound-effects/OutboundEffectRelay.cs` — per-attempt dispatch span with canonical outcome
- `src/runtime/outbound-effects/OutboundEffectFinalityService.cs` — finalize + reconcile spans (sweeper path left unwrapped)

### Tests (1 new)
- `tests/unit/observability/R5APhase3TracingTests.cs` — 5 tests

### Guards (1 extended)
- `claude/guards/runtime.guard.md` — §R5.A extended with 4 new rules (3 span + 1 deferral)

### Prompt + sweep
- `claude/project-prompts/20260420-164117-runtime-r5-a-phase-3-outbound-effect-lifecycle-spans.md`
- `claude/audits/sweeps/20260420-164117-r5-a-phase-3-outbound-effect-lifecycle-spans.md` (this file)

---

## 6. Result

**STATUS: PASS** — R5.A Phase 3 landed inside the bounded scope. The outbound-effect lifecycle now has complete operator-visible span coverage from schedule (Phase 2) through per-attempt dispatch + finalize + operator reconcile (Phase 3). Sweeper noise is explicitly excluded.

### Maturity statement (explicit scope boundary)

R5.A Phase 1 + Phase 2 + Phase 3 together deliver **complete span coverage across the command → event → outbound-effect lifecycle**:

- Command dispatch (Phase 1) → event fabric persist (Phase 2) → outbound-effect schedule (Phase 2) → outbound-effect dispatch attempts (Phase 3) → outbound-effect finalize (Phase 3).
- Operator actions (Phase 1) → admin reconcile (Phase 3) child spans.
- Every span carries canonical low-cardinality outcome tags that match the R4.A metric vocabulary, so trace ↔ metric cross-reference is drift-free.
- Log correlation (Phase 2) gives every log line inside a request the canonical `trace_id` / `correlation_id` / `tenant_id` fields.

R5.A Phase 3 explicitly does NOT deliver:
- **workflow engine per-step spans** — layer-discipline issue; formally deferred with two resolution paths documented as R-TRACE-WORKFLOW-ENGINE-SPAN-DEFERRED-01.
- **sweeper-path spans** — background-worker noise; intentionally unwrapped.
- **compensation dispatcher spans** — downstream of finality; Phase 4.
- **System.Diagnostics histogram exemplars** — structurally blocked.
- **sustained-load + replay certification** — R5.C.

With Phase 3 done, **four subsystems now have canonical span coverage**: runtime control plane, admin surface, event fabric, and the full outbound-effect lifecycle. The only remaining canonical subsystem missing span coverage is the workflow engine (deferred to Phase 4 for layer reasons). R5.C (chaos-under-load + replay certification) can now read the tracing surface as a first-class signal alongside R4.A metrics and R5.B certified faults.
