#!/usr/bin/env bash
# SCENARIO: Postgres Failure Handling
# Stop event store DB -> execute command -> verify request fails, no partial writes
# Restart DB -> verify system recovers
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="$(dirname "$SCRIPT_DIR")"

source "$CERT_DIR/utils/constants.sh"
source "$CERT_DIR/utils/http.sh"
source "$CERT_DIR/utils/db.sh"
source "$CERT_DIR/utils/docker-control.sh"

RESULT_FILE="${1:-/tmp/cert-postgres-failure.result}"

run_postgres_failure() {
    echo "==========================================="
    echo " SCENARIO: POSTGRES FAILURE HANDLING"
    echo "==========================================="
    echo ""

    local ts
    ts=$(date +%s%N)
    local title="cert-pg-fail-${ts}"
    local user_id="cert-user-pg-fail"

    # Capture event count before test
    local events_before
    events_before=$(count_all_events)
    echo "[STEP] Events before test: $events_before"

    # Step 1: Stop event store Postgres
    echo ""
    echo "[STEP] Phase 1 — Stopping event store Postgres..."
    stop_postgres
    sleep 2

    # Step 2: Execute command while DB is down
    echo ""
    echo "[STEP] Sending create request with Postgres down..."
    http_post "/api/todo/create" "{\"Title\":\"${title}\",\"Description\":\"Postgres failure test\",\"UserId\":\"${user_id}\"}"

    echo "[STEP] HTTP status: $HTTP_STATUS"

    # Request MUST fail — no event store = no persistence
    if [ "$HTTP_STATUS" = "200" ] || [ "$HTTP_STATUS" = "201" ]; then
        echo "[FAIL] Request succeeded when it should have failed (DB is down)"
        start_postgres
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    echo "[PASS] Request correctly failed with status $HTTP_STATUS"

    # Step 3: Restart Postgres
    echo ""
    echo "[STEP] Phase 2 — Restarting event store Postgres..."
    start_postgres
    sleep 2

    # Step 4: Verify no partial writes occurred
    echo ""
    echo "[STEP] Verifying no partial writes..."
    local events_after
    events_after=$(count_all_events)
    echo "[STEP] Events after test: $events_after"

    if [ "$events_after" -eq "$events_before" ]; then
        echo "[PASS] No partial writes — event count unchanged"
    else
        echo "[FAIL] Partial write detected: before=$events_before after=$events_after"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 5: Verify system recovers — send a new request
    echo ""
    echo "[STEP] Verifying system recovery with new request..."
    local recovery_title="cert-pg-recovery-${ts}"
    http_post "/api/todo/create" "{\"Title\":\"${recovery_title}\",\"Description\":\"Recovery test\",\"UserId\":\"${user_id}\"}"

    local recovery_correlation="$CORRELATION_ID"
    log_correlation "$recovery_correlation"
    save_correlation "postgres-failure" "$recovery_correlation"

    if [ "$HTTP_STATUS" = "200" ] || [ "$HTTP_STATUS" = "201" ]; then
        echo "[PASS] System recovered — new request succeeded with status $HTTP_STATUS"
    else
        echo "[FAIL] System did not recover — request failed with status $HTTP_STATUS"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    echo ""
    echo "==========================================="
    echo " POSTGRES FAILURE: ALL CHECKS PASSED"
    echo "==========================================="
    echo "PASS" > "$RESULT_FILE"
    return 0
}

run_postgres_failure
