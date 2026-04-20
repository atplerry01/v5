# R5.B — Chaos / Failure Certification

Classification: operational
Context: runtime-reliability
Domain: chaos-certification

## TITLE
R5.B — Prove that the R4.A observability package actually fires under real failure modes. Certification, not new packaging.

## CONTEXT
R4.A closed the packaging question: six dashboards + five alert-rule groups + provisioning + low-cardinality guards. But closure ≠ proof. We don't yet know whether:
- every canonical exception actually maps to its documented HTTP status
- every R4.A alert's PromQL actually references metrics the runtime emits
- every canonical fault has a known handler + a known alert + a known degraded-mode reason
- no R4.A alert is an orphan (keyed off a metric that no canonical fault produces)

R5.B bolts down those mappings as executable invariants. This is Phase 1 of R5 — low-risk, no infrastructure required. Phase 2 (integration-level fault injection against running services) rides on the existing `tests/integration/failure-recovery/` harness and is deferred to R5.B follow-up.

## OBJECTIVE
Deliver a minimum bounded chaos-certification package:
- a canonical failure-mode catalog that enumerates every fault → handler → status → type URI → degraded reason → alert family mapping
- a manifest-coverage validator that pins every R4.A alert to a known failure mode
- a handler-coverage validator that pins every canonical handler to its registered status code + type URI
- an alert-PromQL sanity validator that confirms every expression references only metric families the runtime actually emits
- guard rules that lock the certification discipline so new alerts / handlers / faults can't land without a matching catalog entry

## CONSTRAINTS
- Reuse-first: no new metrics, no new handlers, no new alert rules. The certification layer is a validator over what's already in R4.A + prior runtime.
- No infrastructure-required tests in this pass — text-based + in-memory only. Integration-level chaos (real Postgres/Kafka/OPA fault injection) is R5.B follow-up.
- Certification manifest MUST be machine-readable YAML and MUST be the single source of truth for failure-mode → alert linkage.
- Every alert either maps to a failure mode OR is explicitly tagged "operational-only" (e.g. scrape failure) with rationale.
- Every failure-mode entry is either `certified` (proof file exists) or `unproven` (deferred, with rationale).
- Do NOT reopen R1–R4.A correctness work.

## EXECUTION STEPS (as delivered)
1. Publish the canonical failure-mode catalog: `infrastructure/observability/certification/runtime-failure-modes.yml`. Each entry carries id / fault / canonical_exception / exception_handler / http_status / type_uri / degraded_reason / feeding_metrics / r4a_alerts / proof_test / status.
2. Mirror the catalog as a C# registry under `tests/unit/certification/CanonicalFailureModes.cs` so validators can consume it without a YAML parser dependency.
3. Add four validator tests under `tests/unit/certification/`:
   - `FailureModeManifestTests` — YAML parses, structure is respected, every status is `certified|unproven`, every certified entry's proof file exists.
   - `CanonicalHandlerCoverageTests` — every entry's handler class exists + is registered in Program.cs + declares the documented status code + declares the documented type URI.
   - `AlertToFailureModeMappingTests` — every alert in R4.A rules/*.yml is referenced by at least one failure mode OR is on a declared "operational-only" allowlist.
   - `AlertExpressionMetricReferenceTests` — every alert PromQL expression's metric references match the canonical metric-family set (derived from source meter declarations).
4. Promote R5.B certification rules into `runtime.guard.md` §R5.B Chaos / Failure Certification (test & E2E subsystem).
5. Record the closure in `claude/audits/sweeps/`.

## OUTPUT FORMAT
Summary in the conversation: files created, failure modes cataloged, alerts linked, guards promoted, deferred scope (integration-level chaos tests for R5.B follow-up).

## VALIDATION CRITERIA
- All four certification validator tests pass.
- Every handler registered in Program.cs has a matching failure-mode entry.
- Every alert under `infrastructure/observability/prometheus/rules/` is cross-referenced in the manifest (linked or explicitly orphan-allowlisted).
- Every PromQL expression compiles against the canonical metric-family set.
- R4.A + R4.B tests remain green.

## DEFERRED (R5.B follow-up / R5.C scope)
- Integration-level fault injection against running Postgres/Kafka/OPA — belongs in `tests/integration/chaos/` with real infrastructure; runs on the existing `failure-recovery/` harness.
- Soak / sustained-load SLO proving — R5.C.
- Replay-equivalence certification — R5.C.
- OTEL tracing pipeline — R5.A.
