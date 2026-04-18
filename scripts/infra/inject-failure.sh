#!/usr/bin/env bash
# inject-failure.sh — inject a real infrastructure failure against a named
# container in the running validation stack. Records the outcome and emits a
# JSON line the report emitter can fold into the infrastructure section.
#
# Modes:
#   kill      — docker kill (SIGKILL) the target, wait N seconds, start it.
#   stop      — docker stop the target (SIGTERM + grace), wait, start.
#   restart   — docker restart the target (graceful cycle).
#   pause     — docker pause the target, wait, unpause.
#
# Classification : phase5-operational-activation + phase6-hardening / economic-system
# Usage          : ./scripts/infra/inject-failure.sh <mode> <container> [downtime-seconds]

set -u

MODE="${1:-}"
TARGET="${2:-}"
DOWN="${3:-5}"

if [[ -z "$MODE" || -z "$TARGET" ]]; then
  printf 'usage: inject-failure.sh <kill|stop|restart|pause> <container> [downtime-seconds]\n' >&2
  exit 2
fi

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
OUT_DIR="$REPO_ROOT/tests/reports/infra"
mkdir -p "$OUT_DIR"
OUT_JSON="$OUT_DIR/failure-injection-$(date -u +'%Y%m%dT%H%M%SZ').json"

start_ts=$(date -u +'%Y-%m-%dT%H:%M:%SZ')
rc=0
case "$MODE" in
  kill)
    docker kill "$TARGET" >/dev/null && sleep "$DOWN" && docker start "$TARGET" >/dev/null || rc=$?
    ;;
  stop)
    docker stop "$TARGET" >/dev/null && sleep "$DOWN" && docker start "$TARGET" >/dev/null || rc=$?
    ;;
  restart)
    docker restart "$TARGET" >/dev/null || rc=$?
    ;;
  pause)
    docker pause "$TARGET" >/dev/null && sleep "$DOWN" && docker unpause "$TARGET" >/dev/null || rc=$?
    ;;
  *)
    printf '[inject-failure] unknown mode: %s\n' "$MODE" >&2
    exit 2
    ;;
esac
end_ts=$(date -u +'%Y-%m-%dT%H:%M:%SZ')

if [[ $rc -eq 0 ]]; then
  status="PASS"
  msg="injection completed; target recovered"
else
  status="FAIL"
  msg="injection failed (rc=$rc); target may be stuck"
fi

cat >"$OUT_JSON" <<EOF
{
  "mode": "$MODE",
  "target": "$TARGET",
  "downtime_seconds": $DOWN,
  "started_utc": "$start_ts",
  "finished_utc": "$end_ts",
  "status": "$status",
  "message": "$msg"
}
EOF

printf '[inject-failure] %s %s: %s (wrote %s)\n' "$MODE" "$TARGET" "$status" "$OUT_JSON" >&2
exit $rc
