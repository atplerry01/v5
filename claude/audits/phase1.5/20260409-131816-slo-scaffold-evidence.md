# Phase 1.5 §5.4 — SLO Scaffold Evidence Index

**STATUS:** EVIDENCE RECORD (not a rule capture; not a guard)
**SCOPE:** §5.4 Observability & SLO Definition
**ANCHOR DATE:** 2026-04-09 13:18:16
**SOURCE PROMPT:** [`claude/project-prompts/20260409-131816-infrastructure-observability-slo-scaffold.md`](../../project-prompts/20260409-131816-infrastructure-observability-slo-scaffold.md)

This file is the canonical map of the §5.4 Observability & SLO scaffold
deliverables. It enumerates each artifact, what it provides, and what
is intentionally **not** provided in this pass.

---

## Decisions made before implementation

1. **Scaffold only.** No target values, no error budgets, no alert
   thresholds. Every numeric field is `TBD` until baseline measurement
   exists. Per the explicit user instruction.
2. **No new instruments.** $5 anti-drift. Every SLO maps to an
   instrument that already exists in the meter inventory; the single
   gap (L-7 end-to-end command latency) is recorded as `UNMAPPED` in
   `metric-mapping.md`.
3. **No production code touched.** All deliverables are markdown.
4. **Runbooks are templates.** Triage steps reference real metric
   names + real file paths so operators can act today; resolution
   playbooks are placeholders pending operational experience.
5. **Location** — `docs/observability/{slo,runbooks}/`. Created fresh;
   no prior `docs/observability/` directory existed.

---

## Artifact map

### SLO documents

| File | Purpose |
|------|---------|
| [`docs/observability/slo/README.md`](../../../docs/observability/slo/README.md) | Index + scaffold rules. |
| [`docs/observability/slo/latency-slos.md`](../../../docs/observability/slo/latency-slos.md) | L-1 .. L-7 latency SLOs. L-7 is `UNMAPPED`. |
| [`docs/observability/slo/failure-rate-slos.md`](../../../docs/observability/slo/failure-rate-slos.md) | F-1 .. F-9 failure-rate SLOs. |
| [`docs/observability/slo/recovery-slos.md`](../../../docs/observability/slo/recovery-slos.md) | R-1 .. R-7 recovery SLOs. |
| [`docs/observability/slo/metric-mapping.md`](../../../docs/observability/slo/metric-mapping.md) | Authoritative cross-reference: meter → instruments → SLO IDs, plus the UNMAPPED gap list. |

### Runbook templates

| File | Triage entry path |
|------|------------------|
| [`docs/observability/runbooks/README.md`](../../../docs/observability/runbooks/README.md) | Index + structure rules. |
| [`docs/observability/runbooks/outbox-backlog.md`](../../../docs/observability/runbooks/outbox-backlog.md) | `outbox.depth` / `outbox.oldest_pending_age_seconds` / `outbox.deadletter_depth`. |
| [`docs/observability/runbooks/policy-failure-spike.md`](../../../docs/observability/runbooks/policy-failure-spike.md) | `policy.evaluate.{timeout,breaker_open,failure}`. |
| [`docs/observability/runbooks/chain-failure.md`](../../../docs/observability/runbooks/chain-failure.md) | `chain.anchor.{wait_ms,hold_ms}` (esp. `outcome != ok`). |
| [`docs/observability/runbooks/database-connection-issues.md`](../../../docs/observability/runbooks/database-connection-issues.md) | `postgres.pool.acquisition_failures` + `event_store.append.advisory_lock_wait_ms`. |

---

## Coverage map

| Category | SLOs scaffolded | Runbook(s) |
|----------|-----------------|------------|
| Latency | L-1 .. L-7 (L-7 UNMAPPED) | All four runbooks reference the relevant L-* SLOs. |
| Failure rate | F-1 .. F-9 | `outbox-backlog.md` (F-1, F-2), `policy-failure-spike.md` (F-5, F-6, F-7), `chain-failure.md` (F-8), `database-connection-issues.md` (F-4). |
| Recovery | R-1 .. R-7 | `outbox-backlog.md` (R-1, R-2, R-3), `policy-failure-spike.md` (R-4), `chain-failure.md` (R-6), `database-connection-issues.md` (R-5). |

## Meter coverage

Every meter in the system inventory (as of 2026-04-09) is referenced
by at least one SLO entry, **except**:

| Meter | Reason |
|-------|--------|
| `Whyce.Intake` | Intake admission/rejection counters exist but the §5.4 spec did not call out an intake-side SLO. Recorded here so future passes can decide whether to add one. |

## Gap log (UNMAPPED)

| ID | Reason | Resolution scope |
|----|--------|------------------|
| L-7 (end-to-end command latency) | No runtime-level histogram exists on `RuntimeControlPlane.ExecuteAsync`. The closest signal is the sum of L-1 + L-2 + L-4 + dispatcher overhead, not expressible as a single instrument. | Out of scope for §5.4 scaffold. Adding requires a new histogram + entry in `metric-mapping.md`. |

---

## Validation against §5.4 requirements

| §5.4 requirement | Status |
|------------------|--------|
| Create SLO scaffold documents | ✔ — 3 SLO files (latency, failure-rate, recovery) + index + mapping. |
| Do NOT invent target values | ✔ — every `Target` field is `TBD`. |
| Define latency / failure-rate / recovery metrics | ✔ — categorized into L-* / F-* / R-* identifiers. |
| Map each SLO to existing system metrics | ✔ — `metric-mapping.md` is the authoritative cross-reference; only L-7 is UNMAPPED and explicitly recorded. |
| Cover Whyce.Outbox / Whyce.Postgres / Whyce.Policy / Whyce.Chain / Whyce.Workflow | ✔ — every required meter is referenced (plus `Whyce.EventStore` and `Whyce.Projection.Consumer`; `Whyce.Intake` recorded as not-yet-covered). |
| Add alert condition placeholders (no thresholds invented) | ✔ — every severity guide row is `TBD`. |
| Add runbook templates for outbox backlog / policy failure spike / chain failure / database connection issues | ✔ — 4 runbook files committed, all template-shaped. |

## Out of scope (per user instruction)

- §5.3 (NOT touched)
- §5.5 (NOT touched)
