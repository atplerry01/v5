#!/usr/bin/env bash
# reset-and-seed.sh — certification infrastructure reset + deterministic reseed.
#
# Classification : certification / economic-system
# Contract       : clears transient validation artefacts and re-runs the
#                  Phase 2 deterministic-seeding suite plus the
#                  Certification domain-integrity suite's DI6 10k-seed
#                  probe, so every certification run starts from a
#                  byte-identical baseline.
#
# The repository's integration tests target in-memory infrastructure
# (InMemoryEventStore / InMemoryOutbox / InMemoryChainAnchor) per
# tests/integration/setup/TestHost.cs, so "clear db / cache / event store"
# reduces to:
#
#   * wiping tests/reports/ ephemeral outputs from prior certification
#     runs so the next run cannot confuse new output with old, AND
#   * re-running the deterministic seed probes which assert reseed is
#     reproducible and collision-free (Phase2 S1/S2/S3 + Certification
#     DI6 10k seed).
#
# Exit 0 on reproducible seed; non-zero on any failure.
#
# Usage:
#   ./scripts/reset-and-seed.sh
#   ./scripts/reset-and-seed.sh --skip-build

set -u  # unset-var safety; collect all failures

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

SKIP_BUILD=0
for arg in "$@"; do
  case "$arg" in
    --skip-build) SKIP_BUILD=1 ;;
    -h|--help)
      sed -n '2,26p' "$0"
      exit 0
      ;;
    *) printf '[reset-and-seed] unknown arg: %s\n' "$arg" >&2; exit 2 ;;
  esac
done

REPORT_DIR="$REPO_ROOT/tests/reports"
RUN_TAG="$(date -u +'%Y%m%dT%H%M%SZ')"
LOG_DIR="$REPORT_DIR/logs/$RUN_TAG"
mkdir -p "$LOG_DIR"

log() { printf '[reset-and-seed] %s\n' "$*" >&2; }

overall_rc=0

# ── Stage 1 — clear transient state ────────────────────────────────────────────
log "stage 1: clearing transient certification state"
if [[ -d "$REPORT_DIR/logs" ]]; then
  find "$REPORT_DIR/logs" -mindepth 1 -maxdepth 1 -type d ! -name "$RUN_TAG" -exec rm -rf {} + 2>/dev/null || true
fi
if [[ -d "$REPORT_DIR/phase3" ]]; then
  find "$REPORT_DIR/phase3" -mindepth 1 -type f -name 'soak-*.json' -delete 2>/dev/null || true
fi
log "transient state cleared (report_dir=$REPORT_DIR)"

# ── Stage 2 — build ────────────────────────────────────────────────────────────
if [[ $SKIP_BUILD -eq 0 ]]; then
  BUILD_LOG="$LOG_DIR/00-build.log"
  log "stage 2: dotnet build"
  if ! dotnet build -nologo -v minimal >"$BUILD_LOG" 2>&1; then
    log "BUILD FAILED — see $BUILD_LOG"
    overall_rc=1
  fi
fi

# ── Stage 3 — Phase 2 deterministic seed probe (1000 aggregates) ──────────────
if [[ $overall_rc -eq 0 ]]; then
  SEED_LOG="$LOG_DIR/10-seed-phase2.log"
  SEED_TRX="$LOG_DIR/10-seed-phase2.trx"
  log "stage 3: Phase 2 seeding probe (S1/S2/S3)"
  if ! dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
        --no-build \
        --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Phase2Validation.Phase2SeedingValidationTests" \
        --logger "trx;LogFileName=$SEED_TRX" \
        --nologo -v minimal \
        >"$SEED_LOG" 2>&1; then
    log "PHASE 2 SEED FAILED — see $SEED_LOG"
    overall_rc=1
  fi
fi

# ── Stage 4 — Certification 10k-seed probe (DI6) ──────────────────────────────
if [[ $overall_rc -eq 0 ]]; then
  DI6_LOG="$LOG_DIR/20-seed-certification.log"
  DI6_TRX="$LOG_DIR/20-seed-certification.trx"
  log "stage 4: Certification 10k-seed probe (DI6)"
  if ! dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
        --no-build \
        --filter "FullyQualifiedName~Tests.Integration.EconomicSystem.Certification.DomainIntegrityTests.DI6" \
        --logger "trx;LogFileName=$DI6_TRX" \
        --nologo -v minimal \
        >"$DI6_LOG" 2>&1; then
    log "CERTIFICATION 10K SEED FAILED — see $DI6_LOG"
    overall_rc=1
  fi
fi

# ── Verdict ────────────────────────────────────────────────────────────────────
if [[ $overall_rc -eq 0 ]]; then
  VERDICT="PASS"
else
  VERDICT="FAIL"
fi

printf '\n================================================================\n'
printf 'RESET + SEED: %s\n' "$VERDICT"
printf 'Run tag:  %s\n' "$RUN_TAG"
printf 'Logs:     %s\n' "$LOG_DIR"
printf '================================================================\n'

exit $overall_rc
