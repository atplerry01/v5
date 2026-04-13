#!/usr/bin/env bash
# Verify outbox — confirm outbox row exists with expected status
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../utils/db.sh"
source "$SCRIPT_DIR/../utils/constants.sh"

# Usage: verify-outbox.sh <correlation_id> <expected_status>
# expected_status: "published" or "pending"
verify_outbox() {
    local correlation_id="$1"
    local expected_status="${2:-published}"

    echo "[STEP] Verifying outbox for correlation: $correlation_id (expected: $expected_status)"
    log_correlation "$correlation_id"

    local count
    count=$(count_outbox "$correlation_id")
    if [ "$count" -lt 1 ]; then
        echo "[FAIL] No outbox row found for correlation_id=$correlation_id"
        return 1
    fi
    echo "[PASS] Outbox row exists (count=$count)"

    local actual_status
    actual_status=$(get_outbox_status "$correlation_id")
    # Trim whitespace
    actual_status=$(echo "$actual_status" | tr -d '[:space:]')

    if [ "$actual_status" = "$expected_status" ]; then
        echo "[PASS] Outbox status matches: $actual_status"
    else
        echo "[FAIL] Outbox status mismatch: actual='$actual_status' expected='$expected_status'"
        return 1
    fi

    return 0
}

# Wait for outbox to transition to a status (timeout-guarded)
# Sets: OUTBOX_WAIT_CYCLES — number of polling cycles completed
wait_for_outbox_status() {
    local correlation_id="$1"
    local expected_status="$2"
    local max_wait="${3:-$MAX_WAIT_SECONDS}"
    local interval="${SLEEP_INTERVAL:-2}"
    local elapsed=0
    OUTBOX_WAIT_CYCLES=0

    echo "[STEP] Waiting for outbox status='$expected_status' (max ${max_wait}s, interval ${interval}s)..."
    log_correlation "$correlation_id"
    while [ $elapsed -lt $max_wait ]; do
        local status
        status=$(get_outbox_status "$correlation_id" | tr -d '[:space:]')
        OUTBOX_WAIT_CYCLES=$((OUTBOX_WAIT_CYCLES + 1))
        if [ "$status" = "$expected_status" ]; then
            echo "[PASS] Outbox transitioned to '$expected_status' after ${elapsed}s (${OUTBOX_WAIT_CYCLES} cycles)"
            return 0
        fi
        sleep "$interval"
        elapsed=$((elapsed + interval))
    done
    echo "[FAIL] Timeout: outbox did not reach '$expected_status' within ${max_wait}s (${OUTBOX_WAIT_CYCLES} cycles)"
    return 1
}

if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    verify_outbox "$@"
fi
