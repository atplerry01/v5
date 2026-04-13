#!/usr/bin/env bash
# Verify projection — confirm read model exists for aggregate
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../utils/db.sh"
source "$SCRIPT_DIR/../utils/constants.sh"

# Usage: verify-projection.sh <aggregate_id> [expected_count] [--correlation-id <cid>]
verify_projection() {
    local aggregate_id="$1"
    local expected_count="${2:-1}"

    # Parse --correlation-id from remaining args
    local opt_correlation_id=""
    shift 2 2>/dev/null || true
    while [[ $# -gt 0 ]]; do
        case "$1" in
            --correlation-id) opt_correlation_id="$2"; shift 2 ;;
            *) shift ;;
        esac
    done

    echo "[STEP] Verifying projection for aggregate: $aggregate_id"
    if [ -n "$opt_correlation_id" ]; then
        log_correlation "$opt_correlation_id"
    fi

    local actual_count
    actual_count=$(count_projections "$aggregate_id")

    if [ "$actual_count" -eq "$expected_count" ]; then
        echo "[PASS] Projection count matches: $actual_count == $expected_count"
    else
        echo "[FAIL] Projection count mismatch: actual=$actual_count expected=$expected_count"
        return 1
    fi

    if [ "$expected_count" -ge 1 ]; then
        local projection
        projection=$(get_projection "$aggregate_id")
        if [ -n "$projection" ]; then
            echo "[PASS] Projection data retrieved: $projection"
        else
            echo "[FAIL] Projection data is empty"
            return 1
        fi
    fi

    return 0
}

# Wait for projection to appear (timeout-guarded)
wait_for_projection() {
    local aggregate_id="$1"
    local max_wait="${2:-$MAX_WAIT_SECONDS}"
    local interval="${SLEEP_INTERVAL:-2}"
    local elapsed=0

    echo "[STEP] Waiting for projection to appear (max ${max_wait}s, interval ${interval}s)..."
    while [ $elapsed -lt $max_wait ]; do
        local count
        count=$(count_projections "$aggregate_id")
        if [ "$count" -ge 1 ]; then
            echo "[PASS] Projection appeared after ${elapsed}s"
            return 0
        fi
        sleep "$interval"
        elapsed=$((elapsed + interval))
    done
    echo "[FAIL] Timeout: projection did not appear within ${max_wait}s"
    return 1
}

if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    verify_projection "$@"
fi
