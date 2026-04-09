# Whyce Runbook Templates

**STATUS:** TEMPLATES — resolution playbooks are placeholders.
**SCOPE:** Phase 1.5 §5.4 — Observability & SLO Definition

This directory contains operator-facing runbooks for the most common
failure modes covered by the §5.4 SLO scaffold. Each runbook is
**template-shaped**: triage steps reference real metric names and real
file paths so operators can act, but final resolution playbooks are
placeholders pending operational experience.

## Runbooks

- [`outbox-backlog.md`](outbox-backlog.md) — outbox depth growing,
  oldest-pending age rising, dead-letter depth growing.
- [`policy-failure-spike.md`](policy-failure-spike.md) — OPA timeout,
  breaker-open, or generic failure rate spike.
- [`chain-failure.md`](chain-failure.md) — chain anchor critical
  section failing or wait-time exceeded.
- [`database-connection-issues.md`](database-connection-issues.md) —
  postgres pool acquisition failure spike.

## Runbook structure

Every runbook in this directory follows the same shape so operators
can move between them without re-learning the layout.

| Section | Purpose |
|---------|---------|
| **Symptom** | What an operator sees first (alert text, metric pattern). |
| **Linked SLO(s)** | Which scaffolded SLO this runbook services. |
| **Severity guide** | TEMPLATE: how to triage severity (S0..S3) once thresholds exist. |
| **Triage** | Concrete steps to confirm the issue, with metric queries and file references. Real and actionable today. |
| **Mitigation** | TEMPLATE: short-term steps to stop the bleeding. |
| **Resolution** | TEMPLATE: root-cause closure. |
| **Post-incident** | TEMPLATE: what to verify, what to record. |

## What is real today vs. placeholder

- **Real**: meter names, instrument names, file paths, code references,
  cause-effect descriptions of how the metric changes during the
  incident, and the correct guard / health-check translations.
- **Placeholder**: severity thresholds, alert routing destinations,
  paging policies, on-call escalation paths, and any concrete numeric
  threshold. These are marked `TBD`.

## Editing rules

1. Do **not** invent thresholds. Every numeric value is `TBD` until
   baseline measurement is in hand.
2. Every metric reference MUST resolve through
   [`../slo/metric-mapping.md`](../slo/metric-mapping.md) (the
   authoritative cross-reference).
3. Do not introduce new instruments to satisfy a runbook. If a step
   needs an instrument that does not exist, mark it `UNMAPPED — requires
   new instrument` and record the gap in `metric-mapping.md`.
