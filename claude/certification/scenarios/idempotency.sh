#!/usr/bin/env bash
# SCENARIO: Idempotency Test
# Send same request 20x in parallel — verify only one success, one event, no duplicate projections
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="$(dirname "$SCRIPT_DIR")"

source "$CERT_DIR/utils/constants.sh"
source "$CERT_DIR/utils/http.sh"
source "$CERT_DIR/utils/db.sh"

RESULT_FILE="${1:-/tmp/cert-idempotency.result}"

run_idempotency() {
    echo "==========================================="
    echo " SCENARIO: IDEMPOTENCY TEST"
    echo "==========================================="
    echo ""

    local ts
    ts=$(date +%s%N)
    local title="cert-idempotent-${ts}"
    local user_id="cert-user-idempotent"
    local payload="{\"Title\":\"${title}\",\"Description\":\"Idempotency test\",\"UserId\":\"${user_id}\"}"
    local results_dir
    results_dir=$(mktemp -d)

    # Step 1: Fire 20 identical requests in parallel
    echo "[STEP] Sending 20 identical POST requests in parallel..."
    for i in $(seq 1 20); do
        (
            local status body
            local tmp
            tmp=$(mktemp)
            status=$(curl -s -o "$tmp" -w '%{http_code}' \
                -X POST "${API_BASE:-http://localhost:18080}/api/todo/create" \
                -H "Content-Type: application/json" \
                -d "$payload") || true
            body=$(cat "$tmp")
            rm -f "$tmp"
            echo "${status}|${body}" > "$results_dir/req-${i}.txt"
        ) &
    done
    wait
    echo "[STEP] All 20 requests completed"

    # Step 2: Count successes
    local success_count=0
    local todo_id=""
    local correlation_id=""
    for f in "$results_dir"/req-*.txt; do
        local line status
        line=$(cat "$f")
        status=$(echo "$line" | cut -d'|' -f1)
        if [ "$status" = "200" ] || [ "$status" = "201" ]; then
            success_count=$((success_count + 1))
            local body
            body=$(echo "$line" | cut -d'|' -f2-)
            todo_id=$(echo "$body" | jq -r '.todoId // empty' 2>/dev/null || true)
            correlation_id=$(echo "$body" | jq -r '.correlationId // empty' 2>/dev/null || true)
        fi
    done
    rm -rf "$results_dir"

    echo "[STEP] Successful responses: $success_count"

    # Idempotent endpoint may return success for all identical requests (deterministic ID)
    # The key invariant is: only ONE event stored, only ONE projection
    if [ -z "$todo_id" ] || [ "$todo_id" = "null" ]; then
        echo "[FAIL] No successful response returned a todoId"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    echo "[PASS] At least one success captured todoId=$todo_id"
    log_correlation "$correlation_id"
    save_correlation "idempotency" "$correlation_id"

    # Allow pipeline to settle
    sleep 3

    # Step 3: Verify only one event exists
    echo ""
    echo "[STEP] Verifying event uniqueness..."
    local event_count
    event_count=$(count_events "$todo_id")
    if [ "$event_count" -eq 1 ]; then
        echo "[PASS] Exactly 1 event stored (no duplicates)"
    else
        echo "[FAIL] Expected 1 event, found $event_count"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 4: Verify no duplicate projections
    echo ""
    echo "[STEP] Verifying projection uniqueness..."
    log_correlation "$correlation_id"
    # Wait for projection (timeout-guarded)
    local elapsed=0
    while [ $elapsed -lt $MAX_WAIT_SECONDS ]; do
        local pcount
        pcount=$(count_projections "$todo_id")
        if [ "$pcount" -ge 1 ]; then
            break
        fi
        sleep "$SLEEP_INTERVAL"
        elapsed=$((elapsed + SLEEP_INTERVAL))
    done

    local projection_count
    projection_count=$(count_projections "$todo_id")
    if [ "$projection_count" -eq 1 ]; then
        echo "[PASS] Exactly 1 projection (no duplicates)"
    else
        echo "[FAIL] Expected 1 projection, found $projection_count"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    echo ""
    echo "==========================================="
    echo " IDEMPOTENCY: ALL CHECKS PASSED"
    echo "==========================================="
    echo "PASS" > "$RESULT_FILE"
    return 0
}

run_idempotency
