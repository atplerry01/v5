#!/usr/bin/env bash
# load-smoke.sh — Phase 1.5 load smoke test
# Classification: validation / phase1.5-gate
#
# Source §10: 100 / 500 / 1000 RPS minimum. Validates: no crash, latency in band,
# no event loss, kafka lag acceptable.
#
# Requires one of: k6, vegeta, bombardier. Detects in that order.

set -euo pipefail

API_BASE="${API_BASE:-http://localhost:5000}"
ENDPOINT="${ENDPOINT:-/todo}"
DURATION="${DURATION:-60s}"
LATENCY_P99_MS_MAX="${LATENCY_P99_MS_MAX:-500}"
DRY_RUN=0

for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=1 ;;
  esac
done

log() { printf '[load-smoke] %s\n' "$*" >&2; }

detect_tool() {
  if command -v k6         >/dev/null 2>&1; then echo k6;         return; fi
  if command -v vegeta     >/dev/null 2>&1; then echo vegeta;     return; fi
  if command -v bombardier >/dev/null 2>&1; then echo bombardier; return; fi
  echo none
}

run_stage() {
  local rps="$1"
  log "stage: ${rps} RPS for ${DURATION} on ${API_BASE}${ENDPOINT}"
  if [[ $DRY_RUN -eq 1 ]]; then
    printf 'STAGE: %s rps\nSTATUS: SKIPPED — dry-run\n---\n' "$rps"
    return 0
  fi
  local tool; tool="$(detect_tool)"
  if [[ "$tool" == "none" ]]; then
    printf 'STAGE: %s rps\nSTATUS: FAIL — NOT EXECUTED\nNOTES: no load tool installed (k6/vegeta/bombardier)\n---\n' "$rps"
    return 1
  fi
  printf 'STAGE: %s rps\nSTATUS: FAIL — NOT EXECUTED\nNOTES: tool=%s present but invocation not wired in scaffold\n---\n' "$rps" "$tool"
  return 1
}

main() {
  local rc=0
  run_stage 100  || rc=1
  run_stage 500  || rc=1
  run_stage 1000 || rc=1
  if [[ $DRY_RUN -eq 1 ]]; then exit 0; fi
  exit "$rc"
}

main "$@"
