#!/usr/bin/env bash
# Phase 1.5 Final Certification Runner
# Executes all scenarios sequentially, collects results, generates report
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Configuration
export API_BASE="${API_BASE:-http://localhost:18080}"
export DB_USER="${DB_USER:-whyce}"
export DB_PASS="${DB_PASS:-change_me_securely}"
export KAFKA_CONTAINER="${KAFKA_CONTAINER:-whyce-kafka}"
export POSTGRES_CONTAINER="${POSTGRES_CONTAINER:-whyce-postgres}"
export OPA_CONTAINER="${OPA_CONTAINER:-whyce-opa}"

# Timeout guards (global)
export MAX_WAIT_SECONDS="${MAX_WAIT_SECONDS:-30}"
export SLEEP_INTERVAL="${SLEEP_INTERVAL:-2}"
export KAFKA_RECOVERY_MAX_WAIT="${KAFKA_RECOVERY_MAX_WAIT:-60}"
export LOAD_PROJECTION_MAX_WAIT="${LOAD_PROJECTION_MAX_WAIT:-60}"
export REPLAY_REBUILD_MAX_WAIT="${REPLAY_REBUILD_MAX_WAIT:-60}"
export CERT_META_DIR="/tmp"

# Result files
export BASELINE_RESULT="/tmp/cert-baseline.result"
export IDEMPOTENCY_RESULT="/tmp/cert-idempotency.result"
export KAFKA_RESULT="/tmp/cert-kafka-failure.result"
export POSTGRES_RESULT="/tmp/cert-postgres-failure.result"
export OPA_RESULT="/tmp/cert-opa-failure.result"
export REPLAY_RESULT="/tmp/cert-replay.result"
export LOAD_RESULT="/tmp/cert-load.result"

# Log files
export BASELINE_LOG="/tmp/cert-baseline.log"
export IDEMPOTENCY_LOG="/tmp/cert-idempotency.log"
export KAFKA_LOG="/tmp/cert-kafka-failure.log"
export POSTGRES_LOG="/tmp/cert-postgres-failure.log"
export OPA_LOG="/tmp/cert-opa-failure.log"
export REPLAY_LOG="/tmp/cert-replay.log"
export LOAD_LOG="/tmp/cert-load.log"

# Report output
REPORT_FILE="${SCRIPT_DIR}/phase1.5-final-certification.md"

echo "============================================================"
echo " PHASE 1.5 FINAL CERTIFICATION"
echo " $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
echo "============================================================"
echo ""
echo " API:      $API_BASE"
echo " Kafka:    $KAFKA_CONTAINER"
echo " Postgres: $POSTGRES_CONTAINER"
echo " OPA:      $OPA_CONTAINER"
echo ""

# Clean previous results, metadata, and metrics
rm -f /tmp/cert-*.result /tmp/cert-*.log /tmp/cert-*.correlation /tmp/cert-kafka-metrics.txt

# Pre-flight checks
echo "[STEP] Pre-flight checks..."

preflight_ok=true

# Verify API is healthy — /health must return 200 with status=HEALTHY
api_health_body=$(mktemp)
api_status=$(curl -s -o "$api_health_body" -w '%{http_code}' --max-time 10 "${API_BASE}/health" 2>&1) || true

if [ -z "$api_status" ] || [ "$api_status" = "000" ]; then
    echo "[FAIL] API not reachable at $API_BASE (connection refused or timeout)"
    preflight_ok=false
elif [ "$api_status" != "200" ]; then
    echo "[FAIL] API health check returned HTTP $api_status (expected 200)"
    preflight_ok=false
else
    # Verify body contains HEALTHY status
    health_status=$(jq -r '.status // empty' "$api_health_body" 2>/dev/null) || true
    if [ "$health_status" = "HEALTHY" ]; then
        echo "[PASS] API healthy at $API_BASE (status=$api_status, body.status=$health_status)"
        # Report per-service status
        unhealthy=$(jq -r '.services[] | select(.status != "HEALTHY") | .name + "=" + .status' "$api_health_body" 2>/dev/null) || true
        if [ -n "$unhealthy" ]; then
            echo "[WARN] Unhealthy services: $unhealthy"
        fi
    else
        echo "[FAIL] API returned 200 but status is '${health_status:-missing}' (expected HEALTHY)"
        cat "$api_health_body" 2>/dev/null | head -5 | sed 's/^/   /'
        preflight_ok=false
    fi
fi
rm -f "$api_health_body"

if ! docker ps --format '{{.Names}}' | grep -q "$KAFKA_CONTAINER"; then
    echo "[FAIL] Kafka container '$KAFKA_CONTAINER' not running"
    preflight_ok=false
fi

if ! docker ps --format '{{.Names}}' | grep -q "$POSTGRES_CONTAINER"; then
    echo "[FAIL] Postgres container '$POSTGRES_CONTAINER' not running"
    preflight_ok=false
fi

if ! docker ps --format '{{.Names}}' | grep -q "$OPA_CONTAINER"; then
    echo "[FAIL] OPA container '$OPA_CONTAINER' not running"
    preflight_ok=false
fi

if ! command -v jq >/dev/null 2>&1; then
    echo "[FAIL] jq not found — required for JSON parsing"
    preflight_ok=false
fi

if ! docker exec "$POSTGRES_CONTAINER" psql --version >/dev/null 2>&1; then
    echo "[FAIL] psql not available inside $POSTGRES_CONTAINER — required for DB verification"
    preflight_ok=false
fi

if [ "$preflight_ok" = false ]; then
    echo ""
    echo "[FAIL] Pre-flight checks failed — aborting certification"
    exit 1
fi
echo "[PASS] Pre-flight checks passed"
echo ""

# Execute scenarios sequentially
run_scenario() {
    local name="$1"
    local script="$2"
    local result_file="$3"
    local log_file="$4"

    echo "------------------------------------------------------------"
    echo " SCENARIO: $name"
    echo " $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
    echo "------------------------------------------------------------"

    if bash "$script" "$result_file" > "$log_file" 2>&1; then
        local result
        result=$(cat "$result_file" 2>/dev/null | tr -d '[:space:]')
        echo " Result: $result"
    else
        echo "FAIL" > "$result_file"
        echo " Result: FAIL (script error)"
    fi

    # Print log summary (last 5 lines)
    echo " Log tail:"
    tail -5 "$log_file" 2>/dev/null | sed 's/^/   /'
    echo ""
}

run_scenario "Baseline Execution" \
    "$SCRIPT_DIR/scenarios/baseline.sh" "$BASELINE_RESULT" "$BASELINE_LOG"

run_scenario "Idempotency Test" \
    "$SCRIPT_DIR/scenarios/idempotency.sh" "$IDEMPOTENCY_RESULT" "$IDEMPOTENCY_LOG"

run_scenario "Kafka Failure Recovery" \
    "$SCRIPT_DIR/scenarios/kafka-failure.sh" "$KAFKA_RESULT" "$KAFKA_LOG"

run_scenario "Postgres Failure Handling" \
    "$SCRIPT_DIR/scenarios/postgres-failure.sh" "$POSTGRES_RESULT" "$POSTGRES_LOG"

run_scenario "OPA Failure Handling" \
    "$SCRIPT_DIR/scenarios/opa-failure.sh" "$OPA_RESULT" "$OPA_LOG"

run_scenario "Replay Consistency" \
    "$SCRIPT_DIR/scenarios/replay.sh" "$REPLAY_RESULT" "$REPLAY_LOG"

run_scenario "Load Test" \
    "$SCRIPT_DIR/scenarios/load.sh" "$LOAD_RESULT" "$LOAD_LOG"

# Generate report
echo "------------------------------------------------------------"
echo " GENERATING REPORT"
echo "------------------------------------------------------------"
bash "$SCRIPT_DIR/report/generate-report.sh" "$REPORT_FILE"

echo ""
echo "============================================================"
echo " CERTIFICATION COMPLETE"
echo " Report: $REPORT_FILE"
echo "============================================================"

# Print summary
echo ""
cat "$REPORT_FILE" | head -30
echo ""
echo "Full report at: $REPORT_FILE"
echo "Logs at: /tmp/cert-*.log"
