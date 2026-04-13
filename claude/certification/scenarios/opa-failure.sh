#!/usr/bin/env bash
# SCENARIO: OPA Failure Handling
# Stop OPA -> execute command -> verify request denied, no execution, no event persisted
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="$(dirname "$SCRIPT_DIR")"

source "$CERT_DIR/utils/constants.sh"
source "$CERT_DIR/utils/http.sh"
source "$CERT_DIR/utils/db.sh"
source "$CERT_DIR/utils/docker-control.sh"

RESULT_FILE="${1:-/tmp/cert-opa-failure.result}"

run_opa_failure() {
    echo "==========================================="
    echo " SCENARIO: OPA FAILURE HANDLING"
    echo "==========================================="
    echo ""

    local ts
    ts=$(date +%s%N)
    local title="cert-opa-fail-${ts}"
    local user_id="cert-user-opa-fail"

    # Capture event count before test
    local events_before
    events_before=$(count_all_events)
    echo "[STEP] Events before test: $events_before"

    # Step 1: Stop OPA
    echo ""
    echo "[STEP] Stopping OPA..."
    stop_opa
    sleep 2

    # Step 2: Execute command while OPA is down
    echo ""
    echo "[STEP] Sending create request with OPA down..."
    http_post "/api/todo/create" "{\"Title\":\"${title}\",\"Description\":\"OPA failure test\",\"UserId\":\"${user_id}\"}"

    echo "[STEP] HTTP status: $HTTP_STATUS"
    echo "[STEP] HTTP body: $HTTP_BODY"

    # OPA circuit breaker should refuse the request (RetryableRefusal)
    if [ "$HTTP_STATUS" = "200" ] || [ "$HTTP_STATUS" = "201" ]; then
        echo "[FAIL] Request succeeded when it should have been denied (OPA down)"
        start_opa
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    echo "[PASS] Request correctly denied with status $HTTP_STATUS"

    # Step 3: Verify no events were persisted
    echo ""
    echo "[STEP] Verifying no events persisted..."
    # Restart OPA first so we can query if needed, then check event store
    start_opa

    local events_after
    events_after=$(count_all_events)
    echo "[STEP] Events after test: $events_after"

    if [ "$events_after" -eq "$events_before" ]; then
        echo "[PASS] No events persisted — policy gate held"
    else
        echo "[FAIL] Events were persisted despite OPA being down: before=$events_before after=$events_after"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 4: Verify system recovers after OPA restart
    echo ""
    echo "[STEP] Verifying system recovery..."
    sleep 2
    local recovery_title="cert-opa-recovery-${ts}"
    http_post "/api/todo/create" "{\"Title\":\"${recovery_title}\",\"Description\":\"OPA recovery test\",\"UserId\":\"${user_id}\"}"

    local recovery_correlation="$CORRELATION_ID"
    log_correlation "$recovery_correlation"
    save_correlation "opa-failure" "$recovery_correlation"

    if [ "$HTTP_STATUS" = "200" ] || [ "$HTTP_STATUS" = "201" ]; then
        echo "[PASS] System recovered — request succeeded after OPA restart"
    else
        echo "[FAIL] System did not recover after OPA restart — status $HTTP_STATUS"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    echo ""
    echo "==========================================="
    echo " OPA FAILURE: ALL CHECKS PASSED"
    echo "==========================================="
    echo "PASS" > "$RESULT_FILE"
    return 0
}

run_opa_failure
