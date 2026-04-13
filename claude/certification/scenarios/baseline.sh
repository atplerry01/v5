#!/usr/bin/env bash
# SCENARIO: Baseline Execution
# POST /api/todo/create -> verify full pipeline (event, outbox, kafka, projection, chain)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="$(dirname "$SCRIPT_DIR")"

source "$CERT_DIR/utils/constants.sh"
source "$CERT_DIR/utils/http.sh"
source "$CERT_DIR/utils/db.sh"
source "$CERT_DIR/utils/kafka.sh"
source "$CERT_DIR/verify/verify-eventstore.sh"
source "$CERT_DIR/verify/verify-outbox.sh"
source "$CERT_DIR/verify/verify-kafka.sh"
source "$CERT_DIR/verify/verify-projection.sh"
source "$CERT_DIR/verify/verify-chain.sh"

RESULT_FILE="${1:-/tmp/cert-baseline.result}"

run_baseline() {
    echo "==========================================="
    echo " SCENARIO: BASELINE EXECUTION"
    echo "==========================================="
    echo ""

    local ts
    ts=$(date +%s%N)
    local title="cert-baseline-${ts}"
    local user_id="cert-user-baseline"

    # Step 1: POST /api/todo/create
    echo "[STEP] Creating todo: title='$title' userId='$user_id'"
    http_post "/api/todo/create" "{\"Title\":\"${title}\",\"Description\":\"Baseline certification test\",\"UserId\":\"${user_id}\"}"

    echo "[STEP] HTTP status: $HTTP_STATUS"
    echo "[STEP] HTTP body: $HTTP_BODY"

    if [ "$HTTP_STATUS" != "200" ] && [ "$HTTP_STATUS" != "201" ]; then
        echo "[FAIL] Create request failed with status $HTTP_STATUS"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    echo "[PASS] Create request succeeded"

    # Capture dynamic IDs
    local todo_id
    local correlation_id
    todo_id=$(json_field ".todoId" "$HTTP_BODY")
    correlation_id=$(json_field ".correlationId" "$HTTP_BODY")

    echo "[STEP] Captured todoId=$todo_id correlationId=$correlation_id"
    log_correlation "$correlation_id"
    save_correlation "baseline" "$correlation_id"

    if [ -z "$todo_id" ] || [ "$todo_id" = "null" ]; then
        echo "[FAIL] todoId not returned in response"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Allow pipeline to settle
    sleep 3

    # Step 2: Verify event store
    echo ""
    log_correlation "$correlation_id"
    if ! verify_eventstore "$todo_id" 1 "" --correlation-id "$correlation_id"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 3: Verify outbox (wait for publish cycle)
    echo ""
    log_correlation "$correlation_id"
    if ! wait_for_outbox_status "$correlation_id" "published" "$MAX_WAIT_SECONDS"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 4: Verify Kafka
    echo ""
    log_correlation "$correlation_id"
    if ! verify_kafka "$todo_id" "$TODO_EVENTS_TOPIC" 15000 --correlation-id "$correlation_id"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 5: Verify projection
    echo ""
    log_correlation "$correlation_id"
    if ! wait_for_projection "$todo_id" "$MAX_WAIT_SECONDS"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    if ! verify_projection "$todo_id" 1 --correlation-id "$correlation_id"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 6: Verify chain block
    echo ""
    log_correlation "$correlation_id"
    if ! verify_chain "$correlation_id"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    echo ""
    echo "==========================================="
    echo " BASELINE: ALL CHECKS PASSED"
    echo "==========================================="
    echo "PASS" > "$RESULT_FILE"
    return 0
}

run_baseline
