#!/usr/bin/env bash
# failure-tests.sh — DLQ / retry / failure injection harness
# Classification: validation / phase1.5-gate
#
# Per Source §7: simulate engine failure, projection failure, consumer failure.
# Asserts: DLQ-before-commit, exponential backoff with cap, no message loss, idempotency.

set -euo pipefail

API_BASE="${API_BASE:-http://localhost:5000}"
KAFKA_BOOTSTRAP="${KAFKA_BOOTSTRAP:-localhost:9092}"
DLQ_TOPIC="${DLQ_TOPIC:-whyce.dlq}"
RETRY_TOPIC="${RETRY_TOPIC:-whyce.retry}"
DRY_RUN=0

for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=1 ;;
  esac
done

log() { printf '[failure-tests] %s\n' "$*" >&2; }

emit() {
  local name="$1" status="$2" note="${3:-}"
  printf 'TEST: %s\nSTATUS: %s\nNOTES: %s\n---\n' "$name" "$status" "$note"
}

inject_engine_failure() {
  emit "failure.engine.dlq-before-commit" "FAIL — NOT EXECUTED" \
    "operator: poison fixture must hit engine, fail handler, route to $DLQ_TOPIC, source offset MUST NOT advance"
}

inject_projection_failure() {
  emit "failure.projection.dlq-before-commit" "FAIL — NOT EXECUTED" \
    "operator: projection handler throws, message routes to $DLQ_TOPIC, no read-model write"
}

inject_consumer_failure() {
  emit "failure.consumer.dlq-and-retry" "FAIL — NOT EXECUTED" \
    "operator: consumer crash mid-batch, on restart batch replays from last committed offset, dupes idempotent"
}

assert_backoff_curve() {
  emit "failure.retry.exponential-backoff-capped" "FAIL — NOT EXECUTED" \
    "expected attempts: 1s, 2s, 4s, 8s, ..., capped at 300s; no infinite retry"
}

assert_no_message_loss() {
  emit "failure.no-message-loss" "FAIL — NOT EXECUTED" \
    "produce 1000 messages with deterministic ids, sum events_in == events_out + dlq"
}

assert_idempotency() {
  emit "failure.idempotency.no-duplicates" "FAIL — NOT EXECUTED" \
    "redeliver same envelope 3x, projection state unchanged after 1st apply"
}

main() {
  log "mode: $([[ $DRY_RUN -eq 1 ]] && echo dry-run || echo live)"
  inject_engine_failure
  inject_projection_failure
  inject_consumer_failure
  assert_backoff_curve
  assert_no_message_loss
  assert_idempotency
  if [[ $DRY_RUN -eq 1 ]]; then exit 0; fi
  log "live injection not wired in scaffold — exiting non-zero per gate policy"
  exit 1
}

main "$@"
