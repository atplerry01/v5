# Whyce SLO Scaffold

**STATUS:** SCAFFOLD (no target values set)
**SCOPE:** Phase 1.5 §5.4 — Observability & SLO Definition
**ANCHOR DATE:** 2026-04-09

This directory contains the scaffolded Service Level Objectives (SLOs)
for the Whyce runtime. **No target values, error budgets, or alert
thresholds are set.** Every numeric field is `TBD` and must be filled
in only after baseline measurement against production traffic.

The scaffold exists so that:

1. The shape of each SLO is reviewed and agreed before numbers are
   committed.
2. Each SLO is anchored to an instrument that **already exists** in
   the system's meter inventory — so adopting an SLO never requires
   adding new metric code.
3. Operators have a single canonical map between informal SLO intent
   ("policy evaluation should be fast") and the concrete instrument
   that measures it (`policy.evaluate.duration` on the `Whyce.Policy`
   meter).

## Files

- [`latency-slos.md`](latency-slos.md) — request and pipeline
  latency SLOs.
- [`failure-rate-slos.md`](failure-rate-slos.md) — error /
  refusal / breaker-open rate SLOs.
- [`recovery-slos.md`](recovery-slos.md) — time-to-drain,
  time-to-clear, retry-budget SLOs.
- [`metric-mapping.md`](metric-mapping.md) — full table of
  SLO → meter → instrument → unit.

## How to use

1. Read [`metric-mapping.md`](metric-mapping.md) first — it is the
   authoritative cross-reference.
2. For each SLO in this directory, the **Mapped instrument** field
   is the only place where a meter / instrument name appears. If you
   need to change the binding, change it there and update
   `metric-mapping.md` in the same patch.
3. Do **not** add invented threshold values. The `Target` field is
   `TBD` until baseline measurement is in hand.
4. Do **not** add new instruments to satisfy a new SLO without first
   recording it as `UNMAPPED — requires new instrument` in
   `metric-mapping.md` and getting agreement on the new instrument
   shape ($5 anti-drift).

## Out of scope for this pass

- Alert wiring (Prometheus rules, Grafana panels)
- Error budgets and burn-rate calculations
- Customer-facing SLAs
- Per-tenant SLO partitioning
