#!/usr/bin/env python3
"""emit-validation-report.py — aggregate Phase 2 + Phase 3 validation suite
results into a single tests/reports/validation-report.json.

Invoked by scripts/validate.sh (and scripts/soak-test.sh via its inherited
report emit) once every suite has finished executing. Parses each suite's
TRX file (if present) for latency metrics and counts, then merges those
with the PASS/FAIL/SKIP status / message pairs the runner collected from
each stage. Folds the Phase 3 soak summary (when present) into the top-
level report so `validation-report.json` is the single source of truth for
both phases.

Classification : phase2-validation + phase3-resilience / economic-system
File scope     : scripts/** per $6 (no /src writes, no /validation dir).
"""

from __future__ import annotations

import argparse
import json
import statistics
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Any


TRX_NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

PREFIX_MAP = {
    "economic-system": "20-economic-system",
    "phase2-validation": "30-phase2-validation",
    "load": "40-load",
    "phase3-failure": "50-phase3-failure",
    "phase3-adversarial": "60-phase3-adversarial",
    "phase3-observability": "70-phase3-observability",
    "certification": "75-certification",
    "phase3-soak": "80-phase3-soak",
}

PHASE3_SUITES = {
    "phase3-failure",
    "phase3-adversarial",
    "phase3-observability",
    "phase3-soak",
}

CERTIFICATION_SUITE = "certification"


def parse_suite_arg(raw: str) -> tuple[str, str, str, str]:
    """suite arg format: name=STATUS|message|logpath"""
    name, rest = raw.split("=", 1)
    parts = rest.split("|", 2)
    status = parts[0] if len(parts) >= 1 else "UNKNOWN"
    message = parts[1] if len(parts) >= 2 else ""
    logpath = parts[2] if len(parts) >= 3 else ""
    return name, status, message, logpath


def parse_trx(path: Path) -> dict[str, Any]:
    if not path.exists():
        return {
            "trx_present": False,
            "total": 0, "passed": 0, "failed": 0, "skipped": 0,
            "latency_ms": None,
            "samples_ms": [],
        }

    try:
        tree = ET.parse(path)
    except ET.ParseError as ex:
        return {
            "trx_present": True,
            "trx_parse_error": str(ex),
            "total": 0, "passed": 0, "failed": 0, "skipped": 0,
            "latency_ms": None,
            "samples_ms": [],
        }

    root = tree.getroot()
    counters = root.find("t:ResultSummary/t:Counters", TRX_NS)
    total = int(counters.attrib.get("total", 0)) if counters is not None else 0
    passed = int(counters.attrib.get("passed", 0)) if counters is not None else 0
    failed = int(counters.attrib.get("failed", 0)) if counters is not None else 0
    skipped = int(counters.attrib.get("notExecuted", 0)) if counters is not None else 0

    durations_s: list[float] = []
    for ut in root.findall("t:Results/t:UnitTestResult", TRX_NS):
        dur = ut.attrib.get("duration")
        if not dur:
            continue
        try:
            h, m, rest = dur.split(":", 2)
            sec = float(rest)
            durations_s.append(int(h) * 3600 + int(m) * 60 + sec)
        except (ValueError, AttributeError):
            continue

    latency: dict[str, float] | None = None
    samples_ms: list[float] = []
    if durations_s:
        ms = [d * 1000.0 for d in durations_s]
        samples_ms = [round(v, 3) for v in ms]
        ms_sorted = sorted(ms)
        latency = {
            "avg_ms": round(statistics.fmean(ms), 3),
            "p50_ms": round(ms_sorted[int(len(ms_sorted) * 0.50) - 1] if ms_sorted else 0, 3),
            "p95_ms": round(ms_sorted[max(0, int(len(ms_sorted) * 0.95) - 1)] if ms_sorted else 0, 3),
            "p99_ms": round(ms_sorted[max(0, int(len(ms_sorted) * 0.99) - 1)] if ms_sorted else 0, 3),
            "max_ms": round(max(ms), 3),
            "sample_count": len(ms),
        }

    return {
        "trx_present": True,
        "total": total,
        "passed": passed,
        "failed": failed,
        "skipped": skipped,
        "latency_ms": latency,
        "samples_ms": samples_ms,
    }


def match_trx(log_dir: Path, suite_name: str) -> Path | None:
    prefix = PREFIX_MAP.get(suite_name)
    if not prefix:
        return None
    candidates = list(log_dir.glob(f"{prefix}*.trx"))
    return candidates[0] if candidates else None


def load_soak_summary(path: str | None) -> dict[str, Any] | None:
    if not path:
        return None
    p = Path(path)
    if not p.exists():
        return None
    try:
        return json.loads(p.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return None


def load_infra_section(
    infra_dir: str | None,
    real_infra_status: str | None,
    test_harness_is_in_memory: bool,
) -> dict[str, Any]:
    """Build the infrastructure block for the report.

    Folds per-probe JSON fragments emitted by scripts/infra/*.sh into a
    consolidated shape, plus a deficiencies list that makes the honest
    in-memory / real-infra gap first-class data rather than hidden text.
    """
    # phase5-operational-activation + phase6-hardening: when the real-infra
    # stage passed, certification ran against live Postgres + Kafka + Redis
    # via RealInfraTestHost (TestHost.ForTodo delegates on REAL_INFRA=true).
    # The harness is therefore NOT in-memory for that run — CERT-INFRA-01
    # is resolved and the mode is flipped from "in-memory" to "real".
    mode = "real" if real_infra_status == "PASS" else "in-memory"
    section: dict[str, Any] = {
        "mode": mode,
        "db": "postgres" if mode == "real" else "in-memory",
        "broker": "kafka" if mode == "real" else "in-memory",
        "cache": "redis" if mode == "real" else "in-memory",
        "status": "validated" if mode == "real" else "logic-only",
        "real_infra_stage_status": real_infra_status or "ABSENT",
        "failure_injections": [],
        "dlq_probe": None,
        "observability_probe": None,
        "deficiencies": [],
    }

    if test_harness_is_in_memory and mode != "real":
        section["deficiencies"].append({
            "id": "CERT-INFRA-01",
            "description": (
                "Category=Certification tests bind to the in-memory "
                "TestHost harness (InMemoryEventStore / InMemoryOutbox / "
                "InMemoryChainAnchor). Certification PASS therefore proves "
                "suite-level correctness but not real-infrastructure "
                "correctness. Invoke scripts/validate.sh --real-infra to "
                "run the suite against Postgres + Kafka + Redis."
            ),
            "severity": "S2",
            "blocking": False,
        })

    if not infra_dir:
        return section

    p = Path(infra_dir)
    if not p.is_dir():
        return section

    for f in sorted(p.glob("failure-injection-*.json")):
        try:
            section["failure_injections"].append(json.loads(f.read_text(encoding="utf-8")))
        except (OSError, json.JSONDecodeError):
            continue

    dlq_probes = sorted(p.glob("dlq-probe-*.json"))
    if dlq_probes:
        try:
            section["dlq_probe"] = json.loads(dlq_probes[-1].read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            pass

    obs_probes = sorted(p.glob("obs-probe-*.json"))
    if obs_probes:
        try:
            section["observability_probe"] = json.loads(obs_probes[-1].read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            pass

    injection_failures = [fi for fi in section["failure_injections"] if fi.get("status") != "PASS"]
    if injection_failures:
        section["deficiencies"].append({
            "id": "CERT-INFRA-02",
            "description": (
                f"{len(injection_failures)} failure-injection run(s) did not "
                "complete cleanly. Stack recovery is not proven for the "
                "affected targets."
            ),
            "severity": "S1",
            "blocking": True,
        })

    obs = section["observability_probe"]
    if obs is not None and obs.get("status") != "PASS":
        section["deficiencies"].append({
            "id": "CERT-INFRA-03",
            "description": (
                "Observability probe did not confirm runtime metrics "
                "emission + Prometheus/Grafana reachability."
            ),
            "severity": "S2",
            "blocking": True,
        })

    return section


def flag_anomalies(
    aggregate_latency: dict[str, float] | None,
    error_rate: float,
    soak_summary: dict[str, Any] | None,
) -> list[dict[str, Any]]:
    flags: list[dict[str, Any]] = []

    if aggregate_latency is not None:
        if aggregate_latency.get("p95_ms", 0) > 1000:
            flags.append({
                "kind": "latency_p95_critical",
                "value_ms": aggregate_latency["p95_ms"],
                "threshold_ms": 1000,
            })
        elif aggregate_latency.get("p95_ms", 0) > 500:
            flags.append({
                "kind": "latency_p95_warning",
                "value_ms": aggregate_latency["p95_ms"],
                "threshold_ms": 500,
            })

    if error_rate > 0.01:
        flags.append({
            "kind": "error_rate_critical",
            "value": round(error_rate, 6),
            "threshold": 0.01,
        })
    elif error_rate > 0.001:
        flags.append({
            "kind": "error_rate_warning",
            "value": round(error_rate, 6),
            "threshold": 0.001,
        })

    if soak_summary is not None:
        soak_anomalies = soak_summary.get("anomalies") or {}
        for spike in soak_anomalies.get("latency_spikes", []) or []:
            flags.append({"kind": "soak_latency_spike", **spike})
        for burst in soak_anomalies.get("error_bursts", []) or []:
            flags.append({"kind": "soak_error_burst", **burst})

    return flags


def certification_criteria(
    suites_out: dict[str, Any],
    phase3: dict[str, Any],
    aggregate_latency: dict[str, float] | None,
    error_rate: float,
) -> dict[str, Any]:
    """GO / NO-GO block for the certification verdict.

    GO iff:
      * the certification suite PASSED,
      * error rate < 0.1 %,
      * Phase 3 (when present) did not regress,
      * aggregate p95 latency under 1s (advisory; certification tolerates
        the same ceiling as Phase 3).
    """
    entry = suites_out.get(CERTIFICATION_SUITE)
    if entry is None:
        return {"certification_present": False}

    status = entry.get("status")
    suite_total = entry.get("total", 0) or 0
    suite_failed = entry.get("failed", 0) or 0

    error_rate_ok = error_rate < 0.001  # 0.1% financial-grade gate
    latency_ok = aggregate_latency is None or aggregate_latency.get("p95_ms", 0) < 1000.0
    phase3_ok = (not phase3.get("phase3_present")) or phase3.get("phase3_overall") == "PASS"
    all_cert_tests_pass = suite_total > 0 and suite_failed == 0

    go = (
        status == "PASS"
        and all_cert_tests_pass
        and error_rate_ok
        and latency_ok
        and phase3_ok
    )

    return {
        "certification_present": True,
        "certification_status": status,
        "certification_total": suite_total,
        "certification_failed": suite_failed,
        "error_rate_under_0_1pct": error_rate_ok,
        "latency_p95_under_1000ms": latency_ok,
        "phase3_regression_free": phase3_ok,
        "verdict": "GO" if go else ("SKIP" if status == "SKIP" else "NO-GO"),
    }


def phase3_criteria(
    suites_out: dict[str, Any],
    aggregate_latency: dict[str, float] | None,
    error_rate: float,
    soak_summary: dict[str, Any] | None,
) -> dict[str, Any]:
    phase3_present = any(name in suites_out for name in PHASE3_SUITES)
    if not phase3_present:
        return {"phase3_present": False}

    non_soak_statuses = [
        suites_out[name]["status"]
        for name in PHASE3_SUITES - {"phase3-soak"}
        if name in suites_out
    ]
    failure_safety = "phase3-failure" in suites_out and suites_out["phase3-failure"]["status"] == "PASS"
    adversarial = "phase3-adversarial" in suites_out and suites_out["phase3-adversarial"]["status"] == "PASS"
    observability = "phase3-observability" in suites_out and suites_out["phase3-observability"]["status"] == "PASS"
    soak_status = suites_out.get("phase3-soak", {}).get("status")

    phase3_pass = (
        all(s == "PASS" for s in non_soak_statuses)
        and (soak_status in (None, "PASS", "SKIP"))
    )

    soak_block: dict[str, Any] | None = None
    if soak_summary is not None:
        metrics = soak_summary.get("metrics", {}) or {}
        windows = soak_summary.get("windows", []) or []
        soak_block = {
            "duration_seconds": soak_summary.get("duration_seconds"),
            "iteration_count": soak_summary.get("iteration_count"),
            "metrics": metrics,
            "window_count": len(windows),
            "first_window_avg_ms": windows[0].get("avg_latency_ms") if windows else None,
            "last_window_avg_ms": windows[-1].get("avg_latency_ms") if windows else None,
            "first_window_heap_bytes": windows[0].get("managed_heap_bytes") if windows else None,
            "last_window_heap_bytes": windows[-1].get("managed_heap_bytes") if windows else None,
            "anomalies": soak_summary.get("anomalies", {}),
        }

    return {
        "phase3_present": True,
        "failure_safety": failure_safety,
        "adversarial_correctness": adversarial,
        "observability": observability,
        "soak_status": soak_status or "SKIP",
        "phase3_overall": "PASS" if phase3_pass else "FAIL",
        "phase3_latency_p95_under_1000ms": (
            aggregate_latency is None or aggregate_latency.get("p95_ms", 0) < 1000
        ),
        "phase3_error_rate_under_1pct": error_rate < 0.01,
        "soak": soak_block,
    }


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", required=True, help="output JSON path")
    ap.add_argument("--run-tag", required=True)
    ap.add_argument("--started", required=True)
    ap.add_argument("--finished", required=True)
    ap.add_argument("--overall", required=True, choices=["PASS", "FAIL"])
    ap.add_argument("--log-dir", required=True)
    ap.add_argument(
        "--suite",
        action="append",
        default=[],
        help="suite spec: name=STATUS|message|logpath (repeatable)",
    )
    ap.add_argument(
        "--soak-summary",
        default=None,
        help="path to the Phase 3 soak summary JSON (optional)",
    )
    ap.add_argument(
        "--infra-dir",
        default=None,
        help="path to tests/reports/infra/ (per-probe JSON fragments; optional)",
    )
    args = ap.parse_args()

    log_dir = Path(args.log_dir)
    total_tests = 0
    total_failed = 0
    suites_out: dict[str, Any] = {}
    time_series_samples: list[dict[str, Any]] = []

    for raw in args.suite:
        name, status, message, logpath = parse_suite_arg(raw)
        trx_path = match_trx(log_dir, name)
        trx_summary = parse_trx(trx_path) if trx_path is not None else {
            "trx_present": False,
            "total": 0, "passed": 0, "failed": 0, "skipped": 0,
            "latency_ms": None,
            "samples_ms": [],
        }
        suite_entry = {
            "status": status,
            "message": message,
            "log": logpath,
            "trx_path": str(trx_path) if trx_path else None,
        }
        # Keep the canonical subset (matches schema_version=1 consumers)
        for key in ("trx_present", "total", "passed", "failed", "skipped", "latency_ms"):
            suite_entry[key] = trx_summary.get(key)
        suites_out[name] = suite_entry

        for sample in trx_summary.get("samples_ms") or []:
            time_series_samples.append({"suite": name, "latency_ms": sample})

        total_tests += trx_summary.get("total", 0) or 0
        total_failed += trx_summary.get("failed", 0) or 0

    error_rate = (total_failed / total_tests) if total_tests else 0.0

    latency_rows = [
        s["latency_ms"] for s in suites_out.values() if s.get("latency_ms")
    ]
    aggregate_latency: dict[str, float] | None = None
    if latency_rows:
        aggregate_latency = {
            "avg_ms": round(statistics.fmean(r["avg_ms"] for r in latency_rows), 3),
            "p95_ms": round(max(r["p95_ms"] for r in latency_rows), 3),
            "p99_ms": round(max(r["p99_ms"] for r in latency_rows), 3),
            "max_ms": round(max(r["max_ms"] for r in latency_rows), 3),
        }

    criteria = {
        "error_rate_under_1pct": error_rate < 0.01,
        "avg_latency_under_500ms": (
            aggregate_latency is None
            or aggregate_latency["avg_ms"] < 500.0
        ),
        "all_suites_pass_or_skip": all(
            s["status"] in ("PASS", "SKIP") for s in suites_out.values()
        ),
    }

    soak_summary = load_soak_summary(args.soak_summary)
    phase3 = phase3_criteria(suites_out, aggregate_latency, error_rate, soak_summary)
    anomalies = flag_anomalies(aggregate_latency, error_rate, soak_summary)
    certification = certification_criteria(
        suites_out, phase3, aggregate_latency, error_rate
    )
    infrastructure = load_infra_section(
        args.infra_dir,
        real_infra_status=(suites_out.get("real-infra") or {}).get("status"),
        test_harness_is_in_memory=True,
    )

    blocking_deficiencies = [
        d for d in infrastructure.get("deficiencies", []) if d.get("blocking")
    ]
    if blocking_deficiencies and certification.get("verdict") == "GO":
        certification["verdict"] = "NO-GO"
        certification["blocking_deficiencies"] = [d.get("id") for d in blocking_deficiencies]

    # Keep schema backward-compatible with v1 consumers (Phase 2), bump to
    # v2 when phase3 present so downstream dashboards can detect the new
    # block without a feature flag.
    schema_version = 2 if phase3.get("phase3_present") else 1
    classification = "phase3-resilience" if phase3.get("phase3_present") else "phase2-validation"

    report = {
        "schema_version": schema_version,
        "classification": classification,
        "context": "economic-system",
        "run_tag": args.run_tag,
        "started_utc": args.started,
        "finished_utc": args.finished,
        "overall": args.overall,
        "totals": {
            "total_tests": total_tests,
            "failed_tests": total_failed,
            "error_rate": round(error_rate, 6),
        },
        "latency": aggregate_latency,
        "criteria": criteria,
        "phase3": phase3,
        "certification": certification,
        "infrastructure": infrastructure,
        "time_series": {
            "sample_count": len(time_series_samples),
            "samples": time_series_samples[:500],
        },
        "anomalies": anomalies,
        "suites": suites_out,
        "log_dir": str(log_dir),
    }

    out_path = Path(args.out)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")

    print(f"[emit-validation-report] wrote {out_path}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    sys.exit(main())
