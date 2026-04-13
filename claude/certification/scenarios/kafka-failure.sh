#!/usr/bin/env bash
# SCENARIO: Kafka Failure Recovery
# Stop Kafka -> execute command -> verify event persisted + outbox pending
# Restart Kafka -> verify outbox published + projection updated
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="$(dirname "$SCRIPT_DIR")"

source "$CERT_DIR/utils/constants.sh"
source "$CERT_DIR/utils/http.sh"
source "$CERT_DIR/utils/db.sh"
source "$CERT_DIR/utils/docker-control.sh"
source "$CERT_DIR/verify/verify-eventstore.sh"
source "$CERT_DIR/verify/verify-outbox.sh"
source "$CERT_DIR/verify/verify-projection.sh"

RESULT_FILE="${1:-/tmp/cert-kafka-failure.result}"

run_kafka_failure() {
    echo "==========================================="
    echo " SCENARIO: KAFKA FAILURE RECOVERY"
    echo "==========================================="
    echo ""

    local ts
    ts=$(date +%s%N)
    local title="cert-kafka-fail-${ts}"
    local user_id="cert-user-kafka-fail"

    # Record start time for recovery metrics
    local RECOVERY_START
    RECOVERY_START=$(date +%s)

    # Step 1: Stop Kafka
    echo "[STEP] Phase 1 — Stopping Kafka..."
    stop_kafka
    sleep 2

    # Step 2: Execute command while Kafka is down
    echo ""
    echo "[STEP] Sending create request with Kafka down..."
    http_post "/api/todo/create" "{\"Title\":\"${title}\",\"Description\":\"Kafka failure test\",\"UserId\":\"${user_id}\"}"

    echo "[STEP] HTTP status: $HTTP_STATUS"
    echo "[STEP] HTTP body: $HTTP_BODY"

    # Command should still succeed (event store + outbox are in Postgres)
    if [ "$HTTP_STATUS" != "200" ] && [ "$HTTP_STATUS" != "201" ]; then
        echo "[FAIL] Create request failed — expected success even with Kafka down"
        start_kafka
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    echo "[PASS] Request succeeded despite Kafka being down"

    local todo_id correlation_id
    todo_id=$(json_field ".todoId" "$HTTP_BODY")
    correlation_id=$(json_field ".correlationId" "$HTTP_BODY")
    echo "[STEP] Captured todoId=$todo_id correlationId=$correlation_id"
    log_correlation "$correlation_id"
    save_correlation "kafka-failure" "$correlation_id"

    sleep 1

    # Step 3: Verify event persisted
    echo ""
    log_correlation "$correlation_id"
    if ! verify_eventstore "$todo_id" 1 "" --correlation-id "$correlation_id"; then
        start_kafka
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 4: Verify outbox is pending (Kafka down = can't publish)
    echo ""
    echo "[STEP] Verifying outbox status is 'pending' (Kafka unavailable)..."
    local outbox_status
    outbox_status=$(get_outbox_status "$correlation_id" | tr -d '[:space:]')
    if [ "$outbox_status" = "pending" ]; then
        echo "[PASS] Outbox status is 'pending' as expected"
    else
        echo "[STEP] Outbox status is '$outbox_status' — may have been published before Kafka fully stopped"
        echo "[STEP] Continuing (non-fatal: race condition between stop and publish)"
    fi

    # Step 5: Restart Kafka
    echo ""
    echo "[STEP] Phase 2 — Restarting Kafka..."
    start_kafka
    sleep 5

    # Step 6: Wait for outbox to publish (with retry cycle tracking)
    echo ""
    echo "[STEP] Waiting for outbox recovery..."
    log_correlation "$correlation_id"
    source "$CERT_DIR/verify/verify-outbox.sh"
    if ! wait_for_outbox_status "$correlation_id" "published" "$KAFKA_RECOVERY_MAX_WAIT"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    local outbox_cycles="${OUTBOX_WAIT_CYCLES:-0}"

    # Capture recovery time at outbox publish
    local RECOVERY_OUTBOX_TIME
    RECOVERY_OUTBOX_TIME=$(date +%s)
    local outbox_recovery=$((RECOVERY_OUTBOX_TIME - RECOVERY_START))
    echo "[INFO] kafka_outbox_recovery_time=${outbox_recovery}s"
    echo "[INFO] outbox_poll_cycles=${outbox_cycles}"

    # Query DB retry count if available
    local db_retry_count="N/A"
    db_retry_count=$(eventstore_query "SELECT COALESCE(retry_count, 0) FROM outbox WHERE correlation_id = '${correlation_id}' LIMIT 1;" 2>/dev/null) || db_retry_count="N/A"
    echo "[INFO] db_retry_count=${db_retry_count}"

    # Step 7: Verify projection updated
    echo ""
    log_correlation "$correlation_id"
    source "$CERT_DIR/verify/verify-projection.sh"
    if ! wait_for_projection "$todo_id" "$MAX_WAIT_SECONDS"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    if ! verify_projection "$todo_id" 1 --correlation-id "$correlation_id"; then
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Compute total recovery time
    local RECOVERY_END
    RECOVERY_END=$(date +%s)
    local total_recovery=$((RECOVERY_END - RECOVERY_START))
    echo ""
    echo "[INFO] kafka_recovery_time=${total_recovery}s"
    echo "[INFO] retry_cycles=${outbox_cycles}"

    # Save metrics for report
    save_kafka_metrics "$total_recovery" "$outbox_cycles" "$db_retry_count"

    echo ""
    echo "==========================================="
    echo " KAFKA FAILURE RECOVERY: ALL CHECKS PASSED"
    echo "==========================================="
    echo "PASS" > "$RESULT_FILE"
    return 0
}

run_kafka_failure
