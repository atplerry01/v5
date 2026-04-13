#!/usr/bin/env bash
# SCENARIO: Load Test
# Fire 200 concurrent POST requests -> verify no crashes, all persisted, kafka processed, projections complete
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="$(dirname "$SCRIPT_DIR")"

source "$CERT_DIR/utils/constants.sh"
source "$CERT_DIR/utils/http.sh"
source "$CERT_DIR/utils/db.sh"

RESULT_FILE="${1:-/tmp/cert-load.result}"
CONCURRENCY="${LOAD_CONCURRENCY:-200}"

run_load() {
    echo "==========================================="
    echo " SCENARIO: LOAD TEST ($CONCURRENCY concurrent)"
    echo "==========================================="
    echo ""

    local ts
    ts=$(date +%s%N)
    local user_id="cert-user-load"
    local results_dir
    results_dir=$(mktemp -d)

    # Capture baseline counts
    local events_before
    events_before=$(count_all_events)
    local projections_before
    projections_before=$(count_all_projections)
    echo "[STEP] Baseline: events=$events_before projections=$projections_before"
    save_correlation "load" "batch-${ts}"

    # Step 1: Fire N concurrent requests (each with unique title for unique aggregate)
    echo ""
    echo "[STEP] Firing $CONCURRENCY concurrent create requests..."
    local start_time
    start_time=$(date +%s)

    for i in $(seq 1 "$CONCURRENCY"); do
        (
            local title="cert-load-${ts}-${i}"
            local payload="{\"Title\":\"${title}\",\"Description\":\"Load test item $i\",\"UserId\":\"${user_id}\"}"
            local tmp
            tmp=$(mktemp)
            local status
            status=$(curl -s -o "$tmp" -w '%{http_code}' \
                -X POST "${API_BASE:-http://localhost:18080}/api/todo/create" \
                -H "Content-Type: application/json" \
                -d "$payload" \
                --max-time 30) || true
            local body
            body=$(cat "$tmp")
            rm -f "$tmp"
            echo "${status}|${body}" > "$results_dir/req-${i}.txt"
        ) &
    done
    wait

    local end_time
    end_time=$(date +%s)
    local duration=$((end_time - start_time))
    echo "[STEP] All $CONCURRENCY requests completed in ${duration}s"

    # Step 2: Count results
    local success_count=0
    local fail_count=0
    local error_count=0
    for f in "$results_dir"/req-*.txt; do
        local line status
        line=$(cat "$f")
        status=$(echo "$line" | cut -d'|' -f1)
        if [ "$status" = "200" ] || [ "$status" = "201" ]; then
            success_count=$((success_count + 1))
        elif [ "$status" = "000" ] || [ -z "$status" ]; then
            error_count=$((error_count + 1))
        else
            fail_count=$((fail_count + 1))
        fi
    done
    rm -rf "$results_dir"

    echo "[STEP] Results: success=$success_count failed=$fail_count errors=$error_count"

    # Verify no crashes (no connection errors)
    if [ "$error_count" -gt 0 ]; then
        echo "[FAIL] $error_count requests had connection errors (possible crash)"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi
    echo "[PASS] No connection errors — system remained stable"

    # Allow pipeline to settle
    echo ""
    echo "[STEP] Waiting for pipeline to settle (15s)..."
    sleep 15

    # Step 3: Verify events persisted
    echo ""
    echo "[STEP] Verifying event persistence..."
    local events_after
    events_after=$(count_all_events)
    local new_events=$((events_after - events_before))
    echo "[STEP] New events persisted: $new_events (expected: $success_count)"

    if [ "$new_events" -eq "$success_count" ]; then
        echo "[PASS] All successful requests produced events"
    elif [ "$new_events" -ge "$success_count" ]; then
        echo "[PASS] Events match or exceed successes: $new_events >= $success_count"
    else
        echo "[FAIL] Event count mismatch: new_events=$new_events successes=$success_count"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    # Step 4: Wait for projections to catch up (timeout-guarded)
    echo ""
    echo "[STEP] Waiting for projections to catch up (max ${LOAD_PROJECTION_MAX_WAIT}s)..."
    local elapsed=0
    local max_wait="$LOAD_PROJECTION_MAX_WAIT"
    local target_projections=$((projections_before + success_count))
    while [ $elapsed -lt $max_wait ]; do
        local current
        current=$(count_all_projections)
        if [ "$current" -ge "$target_projections" ]; then
            echo "[STEP] Projections caught up at ${elapsed}s: $current"
            break
        fi
        sleep "$SLEEP_INTERVAL"
        elapsed=$((elapsed + SLEEP_INTERVAL))
    done

    local projections_after
    projections_after=$(count_all_projections)
    local new_projections=$((projections_after - projections_before))
    echo "[STEP] New projections: $new_projections (expected: $success_count)"

    if [ "$new_projections" -ge "$success_count" ]; then
        echo "[PASS] All projections completed"
    else
        echo "[FAIL] Projection count mismatch: new=$new_projections expected=$success_count"
        echo "FAIL" > "$RESULT_FILE"
        return 1
    fi

    echo ""
    echo "[STEP] Summary: ${CONCURRENCY} concurrent | ${success_count} succeeded | ${duration}s elapsed | ${new_events} events | ${new_projections} projections"
    echo ""
    echo "==========================================="
    echo " LOAD TEST: ALL CHECKS PASSED"
    echo "==========================================="
    echo "PASS" > "$RESULT_FILE"
    return 0
}

run_load
