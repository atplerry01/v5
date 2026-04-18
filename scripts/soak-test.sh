#!/usr/bin/env bash
# soak-test.sh — Phase 3 resilience soak runner for the economic-system.
#
# Classification : phase3-resilience / economic-system
# Contract       : PIPELINE-EXEC-01 ($17) compatible. Drives the Phase 3
#                  soak validation suite
#                  (Tests.Integration.EconomicSystem.Phase3Resilience.Phase3Soak)
#                  for a configurable duration, repeatedly executing the
#                  validation flow and logging iteration count, latency,
#                  and error rate.
#
# Emits a final line:
#
#     PHASE 3 VALIDATION: PASS
#   — or —
#     PHASE 3 VALIDATION: FAIL
#
# Structured soak summary:  tests/reports/phase3/soak-<run_tag>.json
# Aggregated report:        tests/reports/validation-report.json
#
# Exit code: 0 on PASS, non-zero on FAIL.
#
# Usage:
#   ./scripts/soak-test.sh                        # 1h default
#   ./scripts/soak-test.sh --duration=2h          # 2h soak
#   ./scripts/soak-test.sh --duration=15m         # 15 min soak
#   ./scripts/soak-test.sh --duration=3600        # raw seconds
#   ./scripts/soak-test.sh --window=60            # window size seconds (default 60)
#   ./scripts/soak-test.sh --duration=30m --skip-build
#
# Notes:
#   * Honors the canonical Phase 3 in-test gates (memory growth,
#     latency degradation, error rate) — the test fails the run if any
#     is breached; soak-test.sh forwards that verdict.
#   * Writes the per-iteration summary via Phase3Soak__SummaryPath so
#     the report emitter can fold soak samples into validation-report.json.

set -u  # unset-var safety; intentionally NOT -e so we collect ALL failures

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

DURATION_RAW="1h"
WINDOW_RAW="60"
SKIP_BUILD=0
for arg in "$@"; do
  case "$arg" in
    --duration=*) DURATION_RAW="${arg#--duration=}" ;;
    --window=*)   WINDOW_RAW="${arg#--window=}" ;;
    --skip-build) SKIP_BUILD=1 ;;
    -h|--help)
      sed -n '2,30p' "$0"
      exit 0
      ;;
    *) printf '[soak-test] unknown arg: %s\n' "$arg" >&2; exit 2 ;;
  esac
done

parse_duration_seconds() {
  local raw="$1"
  local num="${raw%[smhSMH]}"
  local unit="${raw#$num}"
  case "$unit" in
    ""|s|S) printf '%d' "$num" ;;
    m|M)    printf '%d' "$(( num * 60 ))" ;;
    h|H)    printf '%d' "$(( num * 3600 ))" ;;
    *) printf '[soak-test] bad --duration=%s (use 1h, 15m, 3600)\n' "$raw" >&2; return 1 ;;
  esac
}

DURATION_SECONDS="$(parse_duration_seconds "$DURATION_RAW")" || exit 2
WINDOW_SECONDS="$WINDOW_RAW"
if ! [[ "$WINDOW_SECONDS" =~ ^[0-9]+$ ]] || [[ "$WINDOW_SECONDS" -lt 1 ]]; then
  printf '[soak-test] bad --window=%s\n' "$WINDOW_RAW" >&2
  exit 2
fi

REPORT_DIR="$REPO_ROOT/tests/reports"
PHASE3_DIR="$REPORT_DIR/phase3"
RUN_TAG="$(date -u +'%Y%m%dT%H%M%SZ')"
LOG_DIR="$REPORT_DIR/logs/$RUN_TAG"
mkdir -p "$LOG_DIR" "$PHASE3_DIR"

SOAK_SUMMARY="$PHASE3_DIR/soak-$RUN_TAG.json"

log()   { printf '[soak-test] %s\n' "$*" >&2; }
stamp() { date -u +'%Y-%m-%dT%H:%M:%SZ'; }

log "Phase 3 soak start — duration=${DURATION_SECONDS}s window=${WINDOW_SECONDS}s run=$RUN_TAG"

overall_rc=0

if [[ $SKIP_BUILD -eq 0 ]]; then
  BUILD_LOG="$LOG_DIR/00-build.log"
  log "stage 0: dotnet build"
  if ! dotnet build -nologo -v minimal >"$BUILD_LOG" 2>&1; then
    log "BUILD FAILED — see $BUILD_LOG"
    overall_rc=1
  fi
fi

SOAK_LOG="$LOG_DIR/50-soak.log"
SOAK_TRX="$LOG_DIR/50-soak.trx"

if [[ $overall_rc -eq 0 ]]; then
  log "stage 5: Phase 3 soak suite"
  Phase3Soak__DurationSeconds="$DURATION_SECONDS" \
  Phase3Soak__WindowSeconds="$WINDOW_SECONDS" \
  Phase3Soak__SummaryPath="$SOAK_SUMMARY" \
    dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
      --no-build \
      --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Phase3Resilience.Phase3Soak" \
      --logger "trx;LogFileName=$SOAK_TRX" \
      --nologo -v minimal \
      >"$SOAK_LOG" 2>&1
  soak_rc=$?
  if [[ $soak_rc -ne 0 ]]; then
    log "SOAK FAILED (rc=$soak_rc) — see $SOAK_LOG"
    overall_rc=1
  else
    log "SOAK PASSED — summary=$SOAK_SUMMARY"
  fi
fi

if [[ $overall_rc -eq 0 ]]; then
  VERDICT="PASS"
else
  VERDICT="FAIL"
fi

printf '\n================================================================\n'
printf 'PHASE 3 VALIDATION: %s\n' "$VERDICT"
printf 'Duration: %ss (window=%ss)\n' "$DURATION_SECONDS" "$WINDOW_SECONDS"
printf 'Summary:  %s\n' "$SOAK_SUMMARY"
printf 'Logs:     %s\n' "$LOG_DIR"
printf '================================================================\n'

exit $overall_rc
