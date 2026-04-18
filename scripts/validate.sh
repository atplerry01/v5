#!/usr/bin/env bash
# validate.sh — single validation entry point for Whycespace.
#
# Classification : phase2-validation + phase3-resilience / economic-system
# Contract       : PIPELINE-EXEC-01 ($17) compatible; runs the full Phase 2
#                  correctness gate + Phase 3 resilience / adversarial /
#                  observability gates against the existing C#/DDD repository
#                  using only /tests, /scripts, /infrastructure per $6.
#
# Emits a final line (verdict escalates to the highest active phase):
#
#     PHASE 3 VALIDATION: PASS
#   — or —
#     PHASE 3 VALIDATION: FAIL
#
# Structured JSON report written to:
#
#     tests/reports/validation-report.json
#
# Exit code: 0 on PASS, non-zero on FAIL.
#
# Usage:
#   ./scripts/validate.sh                       # Phase 2 + Phase 3 + certification (soak skipped)
#   ./scripts/validate.sh --skip-static         # skip static shell checks
#   ./scripts/validate.sh --skip-load           # skip optional burst load tests
#   ./scripts/validate.sh --skip-phase3         # skip Phase 3 + certification suites
#   ./scripts/validate.sh --soak                # include Phase 3 soak (1h default)
#   ./scripts/validate.sh --soak --duration=2h  # soak 2 hours
#   ./scripts/validate.sh --reset               # run reset-and-seed before suites
#   LoadTest__Enabled=true ./scripts/validate.sh
#   BaselineTest__Enabled=true ./scripts/validate.sh
#
# Emits an extra final line when the certification suite ran:
#
#     ECONOMIC SYSTEM CERTIFICATION: GO    (or NO-GO, or SKIP)

set -u  # unset-var safety; intentionally NOT -e so we collect ALL failures

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

SKIP_STATIC=0
SKIP_LOAD=0
SKIP_PHASE3=0
RUN_SOAK=0
DO_RESET=0
RUN_REAL_INFRA=0
SOAK_DURATION_RAW="1h"
SOAK_WINDOW_RAW="60"
for arg in "$@"; do
  case "$arg" in
    --skip-static)  SKIP_STATIC=1 ;;
    --skip-load)    SKIP_LOAD=1 ;;
    --skip-phase3)  SKIP_PHASE3=1 ;;
    --soak)         RUN_SOAK=1 ;;
    --reset)        DO_RESET=1 ;;
    --real-infra)   RUN_REAL_INFRA=1 ;;
    --duration=*)   SOAK_DURATION_RAW="${arg#--duration=}"; RUN_SOAK=1 ;;
    --window=*)     SOAK_WINDOW_RAW="${arg#--window=}" ;;
    -h|--help)
      sed -n '2,32p' "$0"
      exit 0
      ;;
    *) printf '[validate] unknown arg: %s\n' "$arg" >&2; exit 2 ;;
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
    *) printf '[validate] bad --duration=%s\n' "$raw" >&2; return 1 ;;
  esac
}

SOAK_DURATION_SECONDS="$(parse_duration_seconds "$SOAK_DURATION_RAW")" || exit 2
SOAK_WINDOW_SECONDS="$SOAK_WINDOW_RAW"

REPORT_DIR="$REPO_ROOT/tests/reports"
REPORT_JSON="$REPORT_DIR/validation-report.json"
RUN_TAG="$(date -u +'%Y%m%dT%H%M%SZ')"
LOG_DIR="$REPORT_DIR/logs/$RUN_TAG"
PHASE3_DIR="$REPORT_DIR/phase3"
INFRA_DIR="$REPORT_DIR/infra"
SOAK_SUMMARY="$PHASE3_DIR/soak-$RUN_TAG.json"
mkdir -p "$LOG_DIR" "$PHASE3_DIR" "$INFRA_DIR"

log()   { printf '[validate] %s\n' "$*" >&2; }
stamp() { date -u +'%Y-%m-%dT%H:%M:%SZ'; }

declare -A SUITE_STATUS    # suite -> PASS|FAIL|SKIP
declare -A SUITE_MESSAGE   # suite -> human-readable line
declare -A SUITE_LOG       # suite -> log file path

record() {
  local suite="$1" status="$2" message="$3" logfile="${4:-}"
  SUITE_STATUS["$suite"]="$status"
  SUITE_MESSAGE["$suite"]="$message"
  SUITE_LOG["$suite"]="$logfile"
  log "$suite: $status — $message"
}

# ── STAGE 0 — build ────────────────────────────────────────────────────────────
run_build() {
  local log="$LOG_DIR/00-build.log"
  log "stage 0: dotnet build"
  if dotnet build -nologo -v minimal >"$log" 2>&1; then
    record "build" "PASS" "dotnet build exited 0" "$log"
    return 0
  else
    record "build" "FAIL" "dotnet build failed (see $log)" "$log"
    return 1
  fi
}

# ── STAGE 1 — static checks ────────────────────────────────────────────────────
run_static() {
  if [[ $SKIP_STATIC -eq 1 ]]; then
    record "static-checks" "SKIP" "skipped via --skip-static" ""
    return 0
  fi
  local rc=0
  local agg="$LOG_DIR/10-static.log"
  : > "$agg"
  for s in deterministic-id-check.sh dependency-check.sh hsid-infra-check.sh infrastructure-check.sh; do
    local path="$REPO_ROOT/scripts/$s"
    if [[ ! -f "$path" ]]; then
      printf '[skip] %s — not present\n' "$s" >>"$agg"
      continue
    fi
    printf '\n===== %s =====\n' "$s" >>"$agg"
    if bash "$path" >>"$agg" 2>&1; then
      printf '[ok]   %s\n' "$s" >>"$agg"
    else
      printf '[fail] %s\n' "$s" >>"$agg"
      rc=1
    fi
  done
  if [[ $rc -eq 0 ]]; then
    record "static-checks" "PASS" "all static shell checks exited 0" "$agg"
  else
    record "static-checks" "FAIL" "one or more static shell checks failed (see $agg)" "$agg"
  fi
  return $rc
}

# ── STAGE 2 — existing economic-system integration tests ───────────────────────
run_economic_system_suite() {
  local log="$LOG_DIR/20-economic-system.log"
  local trx="$LOG_DIR/20-economic-system.trx"
  log "stage 2: existing economic-system integration tests"
  dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
    --no-build \
    --filter "FullyQualifiedName~Tests.Integration.EconomicSystem&FullyQualifiedName!~Phase2Validation&FullyQualifiedName!~Phase3Resilience" \
    --logger "trx;LogFileName=$trx" \
    --nologo -v minimal \
    >"$log" 2>&1
  local rc=$?
  if [[ $rc -eq 0 ]]; then
    record "economic-system" "PASS" "existing economic-system suite green" "$log"
  else
    record "economic-system" "FAIL" "existing economic-system suite failed (see $log)" "$log"
  fi
  return $rc
}

# ── STAGE 3 — Phase 2 new validation suite ─────────────────────────────────────
run_phase2_suite() {
  local log="$LOG_DIR/30-phase2-validation.log"
  local trx="$LOG_DIR/30-phase2-validation.trx"
  log "stage 3: Phase 2 new validation tests"
  dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
    --no-build \
    --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Phase2Validation" \
    --logger "trx;LogFileName=$trx" \
    --nologo -v minimal \
    >"$log" 2>&1
  local rc=$?
  if [[ $rc -eq 0 ]]; then
    record "phase2-validation" "PASS" "concurrency/determinism/consistency/seeding green" "$log"
  else
    record "phase2-validation" "FAIL" "phase2 validation suite failed (see $log)" "$log"
  fi
  return $rc
}

# ── STAGE 4 — optional load tests (gated by env vars) ──────────────────────────
run_load_suite() {
  if [[ $SKIP_LOAD -eq 1 ]]; then
    record "load" "SKIP" "skipped via --skip-load" ""
    return 0
  fi
  local baseline="${BaselineTest__Enabled:-false}"
  local burst="${LoadTest__Enabled:-false}"
  if [[ "$baseline" != "true" && "$burst" != "true" ]]; then
    record "load" "SKIP" "BaselineTest__Enabled / LoadTest__Enabled not set" ""
    return 0
  fi
  local log="$LOG_DIR/40-load.log"
  local trx="$LOG_DIR/40-load.trx"
  log "stage 4: load tests (baseline=$baseline burst=$burst)"
  dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
    --no-build \
    --filter "FullyQualifiedName~Tests.Integration.Load" \
    --logger "trx;LogFileName=$trx" \
    --nologo -v minimal \
    >"$log" 2>&1
  local rc=$?
  if [[ $rc -eq 0 ]]; then
    record "load" "PASS" "load suite green (baseline=$baseline burst=$burst)" "$log"
  else
    record "load" "FAIL" "load suite failed (see $log)" "$log"
  fi
  return $rc
}

# ── STAGE 5 — Phase 3 failure-behavior suite ───────────────────────────────────
run_phase3_failure_suite() {
  if [[ $SKIP_PHASE3 -eq 1 ]]; then
    record "phase3-failure" "SKIP" "skipped via --skip-phase3" ""
    return 0
  fi
  local log="$LOG_DIR/50-phase3-failure.log"
  local trx="$LOG_DIR/50-phase3-failure.trx"
  log "stage 5: Phase 3 failure-behavior tests"
  dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
    --no-build \
    --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Phase3Resilience.Phase3Failure" \
    --logger "trx;LogFileName=$trx" \
    --nologo -v minimal \
    >"$log" 2>&1
  local rc=$?
  if [[ $rc -eq 0 ]]; then
    record "phase3-failure" "PASS" "idempotency/retry-safety/partial-execution green" "$log"
  else
    record "phase3-failure" "FAIL" "phase3 failure suite failed (see $log)" "$log"
  fi
  return $rc
}

# ── STAGE 6 — Phase 3 adversarial suite ────────────────────────────────────────
run_phase3_adversarial_suite() {
  if [[ $SKIP_PHASE3 -eq 1 ]]; then
    record "phase3-adversarial" "SKIP" "skipped via --skip-phase3" ""
    return 0
  fi
  local log="$LOG_DIR/60-phase3-adversarial.log"
  local trx="$LOG_DIR/60-phase3-adversarial.trx"
  log "stage 6: Phase 3 adversarial tests"
  dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
    --no-build \
    --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Phase3Resilience.Phase3Adversarial" \
    --logger "trx;LogFileName=$trx" \
    --nologo -v minimal \
    >"$log" 2>&1
  local rc=$?
  if [[ $rc -eq 0 ]]; then
    record "phase3-adversarial" "PASS" "concurrency/edge-values/ordering green" "$log"
  else
    record "phase3-adversarial" "FAIL" "phase3 adversarial suite failed (see $log)" "$log"
  fi
  return $rc
}

# ── STAGE 7 — Phase 3 observability suite ──────────────────────────────────────
run_phase3_observability_suite() {
  if [[ $SKIP_PHASE3 -eq 1 ]]; then
    record "phase3-observability" "SKIP" "skipped via --skip-phase3" ""
    return 0
  fi
  local log="$LOG_DIR/70-phase3-observability.log"
  local trx="$LOG_DIR/70-phase3-observability.trx"
  log "stage 7: Phase 3 observability tests"
  dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
    --no-build \
    --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Phase3Resilience.Phase3Observability" \
    --logger "trx;LogFileName=$trx" \
    --nologo -v minimal \
    >"$log" 2>&1
  local rc=$?
  if [[ $rc -eq 0 ]]; then
    record "phase3-observability" "PASS" "metrics/tracing/anomaly-detection green" "$log"
  else
    record "phase3-observability" "FAIL" "phase3 observability suite failed (see $log)" "$log"
  fi
  return $rc
}

# ── STAGE 7b — Certification suite ─────────────────────────────────────────────
# phase5-operational-activation + phase6-hardening: when --real-infra is
# set, the certification suite runs against the live validation compose
# stack (Postgres + Kafka + Redis) instead of the in-memory TestHost
# harness. REAL_INFRA=true is consumed by TestHost.ForTodo, which
# delegates to RealInfraTestHost when the flag is active. Connection
# strings are sourced from infrastructure/docker/.env.local and wired
# into the test process environment below so the host-network client
# can reach the compose services (Kafka uses the EXTERNAL listener
# per R-K-27).
run_certification_suite() {
  if [[ $SKIP_PHASE3 -eq 1 ]]; then
    record "certification" "SKIP" "skipped via --skip-phase3" ""
    return 0
  fi
  local log="$LOG_DIR/75-certification.log"
  local trx="$LOG_DIR/75-certification.trx"
  local env_prefix=""
  local mode_label="in-memory TestHost"
  if [[ $RUN_REAL_INFRA -eq 1 ]]; then
    mode_label="real-infra (postgres/kafka/redis)"
    # Host-network client endpoints. Kafka uses the EXTERNAL listener
    # (R-K-27). Postgres + Redis reach the compose services via their
    # published ports.
    local env_file="$REPO_ROOT/infrastructure/docker/.env.local"
    local pg_password=""
    if [[ -f "$env_file" ]]; then
      pg_password="$(grep -E '^POSTGRES_PASSWORD=' "$env_file" | head -n1 | cut -d= -f2-)"
    fi
    env_prefix="REAL_INFRA=true \
Postgres__ConnectionString=\"Host=localhost;Port=5432;Database=whyce_eventstore;Username=whyce;Password=${pg_password}\" \
Postgres__ChainConnectionString=\"Host=localhost;Port=5433;Database=whycechain;Username=whyce;Password=${pg_password}\" \
Redis__ConnectionString=\"localhost:6379\" \
Kafka__BootstrapServers=\"localhost:29092\""
  fi
  log "stage 7b: Certification suite — $mode_label"
  eval $env_prefix dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
    --no-build \
    --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Certification" \
    --logger "trx\;LogFileName=$trx" \
    --nologo -v minimal \
    >"$log" 2>&1
  local rc=$?
  if [[ $rc -eq 0 ]]; then
    record "certification" "PASS" "domain-integrity/failure-injection/compensation/dlq-replay/concurrency green ($mode_label)" "$log"
  else
    record "certification" "FAIL" "certification suite failed (see $log)" "$log"
  fi
  return $rc
}

# ── STAGE 0b — Real-infrastructure compose-up (opt-in) ─────────────────────────
# phase5-operational-activation + phase6-hardening: brings up the validation
# compose stack BEFORE the certification suite runs when --real-infra is set,
# so run_certification_suite can exercise the REAL_INFRA=true test path
# (RealInfraTestHost) against live Postgres + Kafka + Redis containers. This
# closes CERT-INFRA-01: certification is no longer bound exclusively to the
# in-memory TestHost harness.
#
# On failure: records the real-infra suite FAIL and aborts the run so the
# rest of the pipeline does not run against a half-up stack.
RUN_REAL_INFRA_UP=0
run_real_infra_compose_up() {
  if [[ $RUN_REAL_INFRA -ne 1 ]]; then
    return 0
  fi

  local env_file="$REPO_ROOT/infrastructure/docker/.env.local"
  if [[ ! -f "$env_file" ]]; then
    record "real-infra" "FAIL" "missing infrastructure/docker/.env.local (copy .env.example)" ""
    return 1
  fi

  if ! command -v docker >/dev/null 2>&1; then
    record "real-infra" "FAIL" "docker CLI not available on PATH" ""
    return 1
  fi

  local log="$LOG_DIR/90-real-infra.log"
  : > "$log"

  log "stage 0b: real-infra compose up"
  if ! bash "$REPO_ROOT/scripts/infra/compose-up.sh" >>"$log" 2>&1; then
    record "real-infra" "FAIL" "compose-up failed (see $log)" "$log"
    return 1
  fi

  if ! bash "$REPO_ROOT/scripts/infra/wait-healthy.sh" 240 >>"$log" 2>&1; then
    record "real-infra" "FAIL" "wait-healthy timed out (see $log)" "$log"
    bash "$REPO_ROOT/scripts/infra/compose-down.sh" >>"$log" 2>&1 || true
    return 1
  fi

  RUN_REAL_INFRA_UP=1
  return 0
}

# ── STAGE 7c — Real-infrastructure probes + teardown (opt-in) ──────────────────
# Runs AFTER the certification suite has exercised the live stack. Performs
# failure injection (Kafka / Postgres / runtime restarts), confirms recovery,
# runs the DLQ + observability probes, and tears the stack down. Per-probe
# JSON is written to tests/reports/infra/ and folded into the final report.
run_real_infra_stage() {
  if [[ $RUN_REAL_INFRA -ne 1 ]]; then
    record "real-infra" "SKIP" "--real-infra not set" ""
    return 0
  fi

  if [[ $RUN_REAL_INFRA_UP -ne 1 ]]; then
    # Compose-up already FAILed earlier; record was emitted there.
    return 1
  fi

  local log="$LOG_DIR/90-real-infra.log"
  local rc=0

  # Failure injection — cycle the Kafka and Postgres containers and observe
  # that the host recovers. Per-injection JSON is written to tests/reports/infra/.
  printf '\n--- failure injection: kafka restart ---\n' >>"$log"
  bash "$REPO_ROOT/scripts/infra/inject-failure.sh" restart whyce-kafka 5 >>"$log" 2>&1 || rc=1
  printf '\n--- failure injection: postgres restart ---\n' >>"$log"
  bash "$REPO_ROOT/scripts/infra/inject-failure.sh" restart whyce-postgres 5 >>"$log" 2>&1 || rc=1
  printf '\n--- failure injection: economic-system restart mid-op ---\n' >>"$log"
  bash "$REPO_ROOT/scripts/infra/inject-failure.sh" restart whyce-economic-system 3 >>"$log" 2>&1 || rc=1

  # Re-wait for health after the cycles; if recovery fails, mark degraded.
  if ! bash "$REPO_ROOT/scripts/infra/wait-healthy.sh" 180 >>"$log" 2>&1; then
    printf '\n[real-infra] stack did not recover after injections\n' >>"$log"
    rc=1
  fi

  # DLQ drain + observability probes.
  printf '\n--- DLQ probe ---\n' >>"$log"
  bash "$REPO_ROOT/scripts/infra/dlq-probe.sh" 200 >>"$log" 2>&1 || rc=1
  printf '\n--- Observability probe ---\n' >>"$log"
  bash "$REPO_ROOT/scripts/infra/obs-probe.sh" >>"$log" 2>&1 || rc=1

  printf '\n--- compose down ---\n' >>"$log"
  bash "$REPO_ROOT/scripts/infra/compose-down.sh" >>"$log" 2>&1 || true

  if [[ $rc -eq 0 ]]; then
    record "real-infra" "PASS" "compose stack stood up, certification ran against real infra, failures injected + recovered, probes green" "$log"
  else
    record "real-infra" "FAIL" "one or more real-infra probes failed (see $log)" "$log"
  fi
  return $rc
}

# ── STAGE 8 — Phase 3 soak suite (opt-in) ──────────────────────────────────────
run_phase3_soak_suite() {
  if [[ $SKIP_PHASE3 -eq 1 ]]; then
    record "phase3-soak" "SKIP" "skipped via --skip-phase3" ""
    return 0
  fi
  if [[ $RUN_SOAK -ne 1 && "${Phase3Soak__Enabled:-false}" != "true" ]]; then
    record "phase3-soak" "SKIP" "soak opt-in (--soak / Phase3Soak__Enabled not set)" ""
    return 0
  fi
  local log="$LOG_DIR/80-phase3-soak.log"
  local trx="$LOG_DIR/80-phase3-soak.trx"
  log "stage 8: Phase 3 soak (duration=${SOAK_DURATION_SECONDS}s window=${SOAK_WINDOW_SECONDS}s)"
  Phase3Soak__DurationSeconds="$SOAK_DURATION_SECONDS" \
  Phase3Soak__WindowSeconds="$SOAK_WINDOW_SECONDS" \
  Phase3Soak__SummaryPath="$SOAK_SUMMARY" \
    dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
      --no-build \
      --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Phase3Resilience.Phase3Soak" \
      --logger "trx;LogFileName=$trx" \
      --nologo -v minimal \
      >"$log" 2>&1
  local rc=$?
  if [[ $rc -eq 0 ]]; then
    record "phase3-soak" "PASS" "soak stable — memory/latency/error-rate within budget" "$log"
  else
    record "phase3-soak" "FAIL" "phase3 soak suite failed (see $log)" "$log"
  fi
  return $rc
}

# ── Python detection (Windows WindowsApps stub safe) ───────────────────────────
detect_python() {
  for candidate in python3 python; do
    if command -v "$candidate" >/dev/null 2>&1; then
      local resolved
      resolved="$(command -v "$candidate")"
      case "$resolved" in
        *WindowsApps/python*|*WindowsApps\\python*) continue ;;
      esac
      if "$candidate" -c 'import sys; assert sys.version_info >= (3, 8)' >/dev/null 2>&1; then
        printf '%s' "$candidate"
        return 0
      fi
    fi
  done
  if command -v py >/dev/null 2>&1; then
    if py -3 -c 'import sys; assert sys.version_info >= (3, 8)' >/dev/null 2>&1; then
      printf 'py -3'
      return 0
    fi
  fi
  return 1
}

# ── Aggregate + emit JSON ──────────────────────────────────────────────────────
emit_report() {
  local overall="$1"
  local suites_args=()
  for suite in "${!SUITE_STATUS[@]}"; do
    suites_args+=( "--suite" "$suite=${SUITE_STATUS[$suite]}|${SUITE_MESSAGE[$suite]}|${SUITE_LOG[$suite]}" )
  done
  local py
  py="$(detect_python)" || {
    log "WARNING: no usable python3 (>=3.8) found — JSON report NOT emitted"
    log "         suite results are still reflected in the exit code + stdout verdict"
    return 0
  }
  local soak_arg=()
  if [[ -f "$SOAK_SUMMARY" ]]; then
    soak_arg=( "--soak-summary" "$SOAK_SUMMARY" )
  fi
  local infra_arg=()
  if [[ -d "$INFRA_DIR" ]]; then
    infra_arg=( "--infra-dir" "$INFRA_DIR" )
  fi
  # shellcheck disable=SC2086
  $py "$REPO_ROOT/scripts/validation/emit-validation-report.py" \
    --out "$REPORT_JSON" \
    --run-tag "$RUN_TAG" \
    --started "$RUN_STARTED" \
    --finished "$(stamp)" \
    --overall "$overall" \
    --log-dir "$LOG_DIR" \
    "${soak_arg[@]}" \
    "${infra_arg[@]}" \
    "${suites_args[@]}"
}

# ── Orchestrate ────────────────────────────────────────────────────────────────
mkdir -p "$REPORT_DIR"
RUN_STARTED="$(stamp)"
log "Phase 2+3 validation start — run=$RUN_TAG"

overall_rc=0

run_build                              || overall_rc=1
if [[ $overall_rc -eq 0 && $DO_RESET -eq 1 ]]; then
  RESET_LOG="$LOG_DIR/05-reset-and-seed.log"
  log "stage 0.5: reset-and-seed (--reset)"
  if bash "$REPO_ROOT/scripts/reset-and-seed.sh" --skip-build >"$RESET_LOG" 2>&1; then
    record "reset-and-seed" "PASS" "transient state cleared + deterministic seed reproducible" "$RESET_LOG"
  else
    record "reset-and-seed" "FAIL" "reset-and-seed failed (see $RESET_LOG)" "$RESET_LOG"
    overall_rc=1
  fi
fi
if [[ $overall_rc -eq 0 ]]; then
  # phase5-operational-activation + phase6-hardening: compose-up BEFORE
  # the test stages so run_certification_suite can exercise RealInfraTestHost
  # (REAL_INFRA=true) against the live stack. Skipped when --real-infra not set.
  run_real_infra_compose_up            || overall_rc=1
fi
if [[ $overall_rc -eq 0 ]]; then
  run_static                           || overall_rc=1
  run_economic_system_suite            || overall_rc=1
  run_phase2_suite                     || overall_rc=1
  run_load_suite                       || overall_rc=1
  run_phase3_failure_suite             || overall_rc=1
  run_phase3_adversarial_suite         || overall_rc=1
  run_phase3_observability_suite       || overall_rc=1
  run_certification_suite              || overall_rc=1
  run_phase3_soak_suite                || overall_rc=1
  run_real_infra_stage                 || overall_rc=1
else
  for suite in static-checks economic-system phase2-validation load \
               phase3-failure phase3-adversarial phase3-observability \
               certification phase3-soak real-infra; do
    record "$suite" "SKIP" "skipped: build failed" ""
  done
fi

if [[ $overall_rc -eq 0 ]]; then
  VERDICT="PASS"
else
  VERDICT="FAIL"
fi

emit_report "$VERDICT"

if [[ $SKIP_PHASE3 -eq 1 ]]; then
  VERDICT_LABEL="PHASE 2 VALIDATION"
else
  VERDICT_LABEL="PHASE 3 VALIDATION"
fi

# ── Certification GO/NO-GO verdict ─────────────────────────────────────────────
# GO iff the certification suite passed AND every non-opt-in suite passed
# or was explicitly skipped. A suite FAILure escalates to NO-GO regardless
# of the primary verdict. When --real-infra is requested, the real-infra
# stage result also gates the verdict: a FAIL there is NO-GO; a SKIP is
# tolerated (opt-in).
CERT_STATUS="${SUITE_STATUS[certification]:-ABSENT}"
INFRA_STATUS="${SUITE_STATUS[real-infra]:-ABSENT}"
if [[ "$VERDICT" == "PASS" && "$CERT_STATUS" == "PASS" ]]; then
  if [[ $RUN_REAL_INFRA -eq 1 && "$INFRA_STATUS" != "PASS" ]]; then
    CERT_VERDICT="NO-GO"
  else
    CERT_VERDICT="GO"
  fi
elif [[ "$CERT_STATUS" == "SKIP" && $SKIP_PHASE3 -eq 1 ]]; then
  CERT_VERDICT="SKIP"
else
  CERT_VERDICT="NO-GO"
fi

printf '\n================================================================\n'
printf '%s: %s\n' "$VERDICT_LABEL" "$VERDICT"
printf 'Report: %s\n' "$REPORT_JSON"
printf 'Logs:   %s\n' "$LOG_DIR"
printf '================================================================\n'
printf 'ECONOMIC SYSTEM CERTIFICATION: %s\n' "$CERT_VERDICT"
printf '================================================================\n'

exit $overall_rc
