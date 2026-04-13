#!/usr/bin/env bash
# Global constants for certification harness — timeout guards and observability
# Sourced by all scenarios and verification scripts

# Timeout guards — no script waits indefinitely
MAX_WAIT_SECONDS="${MAX_WAIT_SECONDS:-30}"
SLEEP_INTERVAL="${SLEEP_INTERVAL:-2}"
KAFKA_RECOVERY_MAX_WAIT="${KAFKA_RECOVERY_MAX_WAIT:-60}"
LOAD_PROJECTION_MAX_WAIT="${LOAD_PROJECTION_MAX_WAIT:-60}"
REPLAY_REBUILD_MAX_WAIT="${REPLAY_REBUILD_MAX_WAIT:-60}"

# Correlation ID metadata directory
CERT_META_DIR="${CERT_META_DIR:-/tmp}"

# Log correlation ID for observability
# Usage: log_correlation "$correlation_id"
log_correlation() {
    local cid="${1:-unset}"
    echo "[INFO] correlationId=${cid}"
}

# Save scenario correlation ID to metadata file for report generation
# Usage: save_correlation "baseline" "$correlation_id"
save_correlation() {
    local scenario="$1"
    local cid="$2"
    echo "$cid" > "${CERT_META_DIR}/cert-${scenario}.correlation"
}

# Save kafka recovery metrics for report generation
# Usage: save_kafka_metrics "$recovery_time" "$retry_cycles" "$db_retry_count"
save_kafka_metrics() {
    local recovery_time="$1"
    local retry_cycles="$2"
    local db_retry_count="${3:-N/A}"
    cat > "${CERT_META_DIR}/cert-kafka-metrics.txt" <<EOF
recovery_time=${recovery_time}
retry_cycles=${retry_cycles}
db_retry_count=${db_retry_count}
EOF
}
