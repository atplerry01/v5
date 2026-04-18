#!/usr/bin/env bash
# obs-probe.sh — runtime observability verification against the live stack.
# Hits Prometheus, Grafana, the host /metrics endpoint, and asserts a handful
# of canonical economic-system metrics are being emitted.
#
# Emits:
#   tests/reports/infra/obs-probe-<ts>.json
#
# Classification : phase6-hardening / economic-system

set -u

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
OUT_DIR="$REPO_ROOT/tests/reports/infra"
mkdir -p "$OUT_DIR"
OUT_JSON="$OUT_DIR/obs-probe-$(date -u +'%Y%m%dT%H%M%SZ').json"

PROM="${PROMETHEUS_URL:-http://localhost:9090}"
GRAF="${GRAFANA_URL:-http://localhost:3000}"
HOST="${HOST_URL:-http://localhost:5000}"

status_code() { curl -s -o /dev/null -w "%{http_code}" --max-time 5 "$1" 2>/dev/null || echo 000; }

prom_up=$(status_code "$PROM/-/healthy")
graf_up=$(status_code "$GRAF/api/health")
host_up=$(status_code "$HOST/health")
metrics_up=$(status_code "$HOST/metrics")

metrics_body=""
metrics_hits=0
if [[ "$metrics_up" == "200" ]]; then
  metrics_body=$(curl -s --max-time 5 "$HOST/metrics" 2>/dev/null || true)
  for token in "whyce_" "http_request" "outbox_" "kafka_" "dotnet_"; do
    if printf '%s' "$metrics_body" | grep -q "$token"; then
      metrics_hits=$(( metrics_hits + 1 ))
    fi
  done
fi

overall="PASS"
[[ "$prom_up" != "200" ]] && overall="FAIL"
[[ "$host_up" != "200" ]] && overall="FAIL"
[[ "$metrics_up" != "200" ]] && overall="FAIL"
[[ $metrics_hits -lt 2 ]]    && overall="FAIL"

cat >"$OUT_JSON" <<EOF
{
  "probed_utc": "$(date -u +'%Y-%m-%dT%H:%M:%SZ')",
  "prometheus_healthy": $([[ "$prom_up" == "200" ]] && echo true || echo false),
  "grafana_healthy":    $([[ "$graf_up" == "200" ]] && echo true || echo false),
  "host_healthy":       $([[ "$host_up" == "200" ]] && echo true || echo false),
  "host_metrics_exposed": $([[ "$metrics_up" == "200" ]] && echo true || echo false),
  "canonical_metric_families_seen": $metrics_hits,
  "status": "$overall"
}
EOF

printf '[obs-probe] status=%s wrote %s\n' "$overall" "$OUT_JSON" >&2
[[ "$overall" == "PASS" ]] && exit 0 || exit 1
