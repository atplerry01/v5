# R4.A — Runtime Observability Packaging

Classification: operational
Context: infrastructure / observability
Domain: dashboards + alerts

## TITLE
R4.A — Package runtime observability into enterprise-grade operational dashboards + Prometheus alert rules.

## CONTEXT
R1–R4.B are complete: the runtime is correctness-closed, operator-governable, and audit-safe. Substantial metrics already emit from 14 meters (79 instruments) including runtime command pipeline, workflow, outbound-effects, Kafka consumer fabric, Postgres pools, outbox, intake admission, chain anchor, policy, and R4.B operator-action audit.

R4.A is observability packaging — turn the existing signals into a usable operator package. NOT a redesign, NOT a new-metrics expansion.

## OBJECTIVE
Ship the minimum bounded observability package an operator can use today:
- six canonical Grafana dashboards covering runtime, workflow, outbound-effect, DLQ/retry, Kafka consumer fabric, persistence/pools/outbox
- five Prometheus alert-rule groups mapped to existing posture + lifecycle vocabulary
- dashboard/alert assets validated by a bounded unit test
- canonical placement under `infrastructure/observability/` with Grafana provisioning wired for auto-load
- guard rules that lock asset placement + canonical dashboard coverage + low-cardinality discipline

## CONSTRAINTS
- Reuse-first: no new runtime metrics unless packaging reveals a truly load-bearing gap.
- Respect canonical vocabulary: degraded mode, readiness/liveness, breaker posture, retry/DLQ/backlog, reconciliation/compensation, workflow & outbound-effect lifecycle states.
- Low-cardinality only — no dashboards or alerts keyed on correlation id / actor id / free text.
- Every alert must answer: what failed, why it matters, what metric/state triggered it, what to inspect next.
- No new runtime metric names, no broad renames, no chaos harness, no frontend UI, no replay certification — those are R4.C / R5.
- R1–R4.B correctness work is NOT reopened.

## EXECUTION STEPS (as delivered)
1. Recon existing metrics (14 meters, 79 instruments) + canonical vocabulary + existing phase3 baseline dashboard.
2. Add one packaging-discovered gap metric: `whyce.runtime.operator.action.total` counter (tags: `action_type`, `outcome`) on `Whycespace.Runtime.ControlPlane` meter. Low-cardinality (< 15 combinations). Wired into `OperatorActionAuditRecorder`. Report as discovered gap.
3. Package six canonical Grafana dashboards under `infrastructure/observability/grafana/dashboards/` — runtime, workflow, outbound-effect, dlq-retry, kafka-consumer, persistence-outbox.
4. Package five Prometheus alert-rule groups under `infrastructure/observability/prometheus/rules/` — posture, workflow, outbound-effect, dlq-retry, persistence.
5. Wire Grafana dashboard provisioning (`dashboards.yml`) + Prometheus `rule_files` glob into existing `prometheus.yml`.
6. Validation test in `tests/unit/observability/` — parses every dashboard JSON + alert YAML; asserts required panel titles + alert names are present.
7. Promote R4.A rules into `infrastructure.guard.md` §Observability Packaging: asset placement, canonical dashboard coverage, low-cardinality discipline, alert vocabulary alignment.

## OUTPUT FORMAT
See the summary in the conversation (files, dashboards, alerts, new metric + rationale, guard updates, validation, deferred).

## VALIDATION CRITERIA
- Every dashboard JSON parses as valid Grafana schema; panels reference only known PromQL metric names.
- Every alert YAML parses; expressions reference only known metric names; every rule has `labels.severity` + `annotations.summary` + `annotations.description`.
- New `operator_action_total` counter compiles, emits, and appears on the runtime-control-plane dashboard.
- `AdminSurfaceArchitectureTests` remains green; no regression in prior R4.B tests.
- `infrastructure.guard.md` §Observability Packaging cites every dashboard + alert family; validation test pins the file list.

## DEFERRED (intentional scope boundary)
- New metric names beyond the single operator-action counter — existing names cover every dashboard cleanly.
- Grafana alerting-as-code (unified alerting), OnCall / PagerDuty routing, runbook CMS — out of scope.
- Trace telemetry (OTEL tracing pipeline) — R5 concern.
- Frontend admin UI, broad metric renames, chaos harness — R4.C / R5.
