# Phase 3 Observability — Economic System

**Classification:** phase3-resilience
**Scope:** `infrastructure/observability/phase3/**`

This directory ships the Phase 3 resilience observability artifacts consumed by:

- Prometheus — alert rules for latency spikes and error bursts (`prometheus/alerts-phase3.yml`).
- Grafana — resilience dashboard (`grafana/dashboards/phase3-resilience.json`).
- `scripts/validate.sh` / `scripts/soak-test.sh` — report-writer (`anomaly-config.json`) that the Phase 3 AnomalyDetector reads to decide what counts as a spike/burst.

The **runtime-observable metrics** (latency, throughput, errors) are emitted by the Phase 3 test harness's in-process `MetricsCollector` (under `tests/integration/economic-system/phase3-resilience/_shared/MetricsCollector.cs`) and dumped to `tests/reports/phase3/metrics-*.json` after each run. The report emitter folds those samples, anomaly flags, and the soak summary into `tests/reports/validation-report.json`.

## Alert thresholds (canonical)

| Signal                 | Warning        | Critical       | Rationale                                                  |
|------------------------|----------------|----------------|------------------------------------------------------------|
| p95 dispatch latency   | > 500 ms       | > 1000 ms      | Phase 2 SLO floor × 2 (production-grade resilience budget) |
| p99 dispatch latency   | > 1000 ms      | > 2000 ms      | Long-tail degradation guard                                |
| Error rate (rolling)   | > 0.1 %        | > 1.0 %        | Phase 2 report already fails at >1 %                       |
| Latency spike          | sample > 3×avg | sample > 5×avg | Detect transient back-pressure / GC / lock contention      |
| Error burst (window)   | ≥ 3 / 60 s     | ≥ 5 / 60 s     | Protect against correlated failure                         |
| Soak memory growth     | > 10 % / hour  | > 25 % / hour  | Leak detector                                              |
| Soak latency trend     | > 1.5 ×        | > 2.0 ×        | End-of-run vs first-window degradation                     |

The thresholds live in `anomaly-config.json`; both the Phase 3 harness and the Prometheus rules read the same values so dashboard, alerts, and in-process anomaly detection agree by construction.
