#!/usr/bin/env bash
# Verify event store — confirm events exist for a given aggregate
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../utils/db.sh"
source "$SCRIPT_DIR/../utils/constants.sh"

# Usage: verify-eventstore.sh <aggregate_id> [expected_count] [expected_event_type] [--correlation-id <cid>]
verify_eventstore() {
    local aggregate_id="$1"
    local expected_count="${2:-1}"
    local expected_type="${3:-}"
    local opt_correlation_id=""

    # Parse --correlation-id from remaining args
    shift 3 2>/dev/null || true
    while [[ $# -gt 0 ]]; do
        case "$1" in
            --correlation-id) opt_correlation_id="$2"; shift 2 ;;
            *) shift ;;
        esac
    done

    echo "[STEP] Verifying event store for aggregate: $aggregate_id"
    if [ -n "$opt_correlation_id" ]; then
        log_correlation "$opt_correlation_id"
    fi

    local actual_count
    actual_count=$(count_events "$aggregate_id")

    if [ "$actual_count" -eq "$expected_count" ]; then
        echo "[PASS] Event count matches: $actual_count == $expected_count"
    else
        echo "[FAIL] Event count mismatch: actual=$actual_count expected=$expected_count"
        return 1
    fi

    if [ -n "$expected_type" ]; then
        local events
        events=$(get_events "$aggregate_id")
        if echo "$events" | grep -q "$expected_type"; then
            echo "[PASS] Event type '$expected_type' found"
        else
            echo "[FAIL] Event type '$expected_type' not found in events"
            return 1
        fi
    fi

    # Verify audit columns exist
    local audit_check
    audit_check=$(eventstore_query "SELECT COUNT(*) FROM events WHERE aggregate_id = '${aggregate_id}' AND correlation_id IS NOT NULL;")
    if [ "$audit_check" -ge 1 ]; then
        echo "[PASS] Audit columns populated (correlation_id present)"
    else
        echo "[FAIL] Audit columns missing — correlation_id is NULL"
        return 1
    fi

    return 0
}

# Allow direct invocation
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    verify_eventstore "$@"
fi
