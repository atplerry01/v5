# R5.C.2 Phase 1 — Chaos Observability-Loop Catalog

Classification: operational
Context: runtime-reliability
Domain: chaos-observability-loop-certification

## TITLE
R5.C.2 Phase 1 — Formalize the full observability loop (fault → exception → handler → status → metric → alert → span → log) as a guard-locked catalog. Executable live-infrastructure chaos runs follow in Phase 2.

## CONTEXT
R5.B proved fault → exception → handler → HTTP status. R4.A proved metric + alert shape. R5.A proved spans exist on canonical seams. R5.C.1 proved determinism invariants. What's still unproven: when a canonical fault fires in production, does the FULL observability loop light up? That's what an on-call operator needs — not just that the 503 fires, but that the metric increments, the alert fires, a span records the error, and the log line carries the trace id.

R5.C.2 Phase 1 delivers the catalog + validators that make the full observability loop an executable contract per R5.B failure mode. Phase 2 executes it against live infrastructure.

## OBJECTIVE
Deliver a bounded Phase 1:
- `infrastructure/observability/certification/chaos-observability-loop.yml` — machine-readable catalog binding each R5.B failure mode to its full signal chain (canonical exception, handler, HTTP status, feeding metric, R4.A alert, R5.A span family, log-scope contract)
- `tests/unit/certification/CanonicalChaosLoops.cs` — C# mirror
- Validator tests that cross-reference: every R5.B certified failure mode has a chaos-loop; every cited R4.A alert exists in rules/*.yml; every cited metric matches the canonical prefix set; every cited span family matches a canonical `WhyceActivitySources` entry
- Guard rules locking the observability-loop discipline

Not in scope:
- Live-infrastructure chaos runs (R5.C.2 Phase 2)
- New runtime code / metrics / alerts / spans
- New failure modes / handlers

## CONSTRAINTS
- Catalog shape MUST mirror R5.B + R5.C.1 pattern (YAML source of truth + C# mirror + validators).
- Every chaos-loop entry MUST reference only existing canonical primitives (R5.B failure mode id, R4.A alert name, R5.A span name, canonical metric prefix).
- Low-cardinality discipline applies to cited metric names / alert labels (inherited from R4.A).
- Unproven-at-runtime status carries a `rationale:` block.

## EXECUTION STEPS (as delivered)
1. Prompt stored per $2.
2. Publish the chaos observability-loop catalog. Each entry pins: source R5.B failure mode id, canonical exception, handler, http_status, feeding metric name, R4.A alert name, R5.A span family name, log-scope expected keys, loop_proof_status (cataloged | live_proven).
3. Mirror as `CanonicalChaosLoops.cs`.
4. Validator tests: mirror sync, every R5.B certified failure mode is in the loop catalog, every cited alert is in R4.A rules/*.yml, every cited metric starts with a canonical prefix, every cited span name matches a `WhyceActivitySources.Spans.*` constant.
5. Promote R5.C.2 guard rules into `runtime.guard.md` §R5.C.2.
6. Sweep record per $1b.

## OUTPUT FORMAT
Summary: loops cataloged, cross-references validated, guards, validation, deferred.

## VALIDATION CRITERIA
- Every R5.B failure mode with `status: certified` (9 entries) has a chaos-loop catalog entry.
- Every cited R4.A alert exists in `infrastructure/observability/prometheus/rules/*.yml`.
- Every cited span name exists as a constant on `WhyceActivitySources.Spans`.
- Every cited metric identifier starts with a canonical prefix from R5.B's `AlertExpressionMetricReferenceTests` set.
- No regressions — 149/149 prior tests remain green.

## DEFERRED (R5.C.2 Phase 2 / R5.C.3)
- **Live-infrastructure chaos runs** — docker-compose up + scripted load + scripted fault injection + end-to-end signal-chain assertion. Phase 2.
- **Soak SLO proving** — R5.C.3.
- **Multi-instance replay equivalence** — carried over from R5.C.1 as an unproven invariant.
