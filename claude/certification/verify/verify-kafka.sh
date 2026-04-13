#!/usr/bin/env bash
# Verify Kafka — confirm message exists on topic for an aggregate
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../utils/kafka.sh"
source "$SCRIPT_DIR/../utils/constants.sh"

# Usage: verify-kafka.sh <aggregate_id> [topic] [timeout_ms] [--correlation-id <cid>]
verify_kafka() {
    local aggregate_id="$1"
    local topic="${2:-$TODO_EVENTS_TOPIC}"
    local timeout="${3:-15000}"

    # Parse --correlation-id from remaining args
    local opt_correlation_id=""
    shift 3 2>/dev/null || true
    while [[ $# -gt 0 ]]; do
        case "$1" in
            --correlation-id) opt_correlation_id="$2"; shift 2 ;;
            *) shift ;;
        esac
    done

    echo "[STEP] Verifying Kafka topic '$topic' contains aggregate: $aggregate_id"
    if [ -n "$opt_correlation_id" ]; then
        log_correlation "$opt_correlation_id"
    fi

    if kafka_topic_contains "$topic" "$aggregate_id" "$timeout"; then
        echo "[PASS] Kafka message found for aggregate $aggregate_id on topic $topic"
    else
        echo "[FAIL] Kafka message NOT found for aggregate $aggregate_id on topic $topic"
        return 1
    fi

    return 0
}

if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    verify_kafka "$@"
fi
