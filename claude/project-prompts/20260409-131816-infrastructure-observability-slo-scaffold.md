# Phase 1.5 — §5.4 Observability & SLO Scaffold

**CLASSIFICATION:** infrastructure-observability
**CONTEXT:** phase1.5 / §5.4 (observability & SLO definition)
**DOMAIN:** documentation-scaffold (slo + runbooks)
**STORED:** 2026-04-09 13:18:16
**STATUS:** EXECUTING

## TITLE
§5.4 Observability & SLO Definition — scaffold-only pass.

## CONTEXT
§5.4 of the phase1.5 re-open amendment requires SLO definitions and
runbook templates anchored to the existing meter inventory. Per the
explicit user instruction, this is a SCAFFOLD pass: no target values,
no thresholds, no alert numbers. The deliverables map informal SLO
intent onto the canonical instruments emitted by the system today.

Existing meters (inventoried 2026-04-09):
- `Whyce.Outbox`        — `KafkaOutboxPublisher` + `OutboxDepthSampler`
- `Whyce.Postgres`      — `PostgresPoolMetrics` (+ `TodoProjectionHandler` reuse)
- `Whyce.EventStore`    — `PostgresEventStoreAdapter`
- `Whyce.Policy`        — `OpaPolicyEvaluator`
- `Whyce.Chain`         — `ChainAnchorService`
- `Whyce.Workflow`      — `WorkflowAdmissionGate`
- `Whyce.Projection.Consumer` — `GenericKafkaProjectionConsumerWorker`
- `Whyce.Intake`        — `IntakeMetrics`

## OBJECTIVE
Produce SLO scaffold + metric mapping + runbook templates anchored to
the meters above. No production code touched. Do NOT proceed to §5.3
or §5.5.

## CONSTRAINTS
- **No invented target values.** Every threshold field is `TBD` with
  the rationale "to be set after baseline measurement against
  production traffic."
- **No new metrics introduced.** $5 anti-drift. Every SLO maps to an
  instrument that exists today; if no mapping exists, the SLO row is
  marked `UNMAPPED — requires new instrument (out of scope)`.
- **Scaffold only.** No alerting rules wired into Prometheus / Grafana.
- **Runbooks are templates.** Triage steps reference real metric names
  + real file paths so operators can act, but resolution playbooks
  are placeholders pending operational experience.

## EXECUTION STEPS
1. Save this prompt under `claude/project-prompts/`.
2. Create `docs/observability/slo/` with:
   - `README.md` (index)
   - `latency-slos.md`
   - `failure-rate-slos.md`
   - `recovery-slos.md`
   - `metric-mapping.md`
3. Create `docs/observability/runbooks/` with:
   - `README.md` (index)
   - `outbox-backlog.md`
   - `policy-failure-spike.md`
   - `chain-failure.md`
   - `database-connection-issues.md`
4. Anchor the work under `claude/audits/phase1.5/20260409-131816-slo-scaffold-evidence.md`.

## OUTPUT FORMAT
Markdown documents only. No code. No threshold numbers.

## VALIDATION CRITERIA
- Every SLO row maps to an existing canonical instrument (or is
  explicitly marked UNMAPPED).
- Zero invented thresholds — every numeric target is `TBD` with a
  rationale.
- Runbooks reference real file paths and real metric names.
- No `src/` files modified.
