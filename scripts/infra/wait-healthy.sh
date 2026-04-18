#!/usr/bin/env bash
# wait-healthy.sh — block until every container in the validation stack
# reports a healthcheck status of "healthy", or until TIMEOUT seconds
# elapse. Prints the unhealthy containers on timeout and exits non-zero.
#
# Classification : phase5-operational-activation / economic-system
# Usage          : ./scripts/infra/wait-healthy.sh [timeout-seconds]

set -u

TIMEOUT="${1:-180}"
DEADLINE=$(( $(date +%s) + TIMEOUT ))

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$REPO_ROOT"

COMPOSE_FILE="infrastructure/docker/docker-compose.validation.yml"
ENV_FILE="infrastructure/docker/.env.local"

EXTRA=()
[[ -f "$ENV_FILE" ]] && EXTRA+=( "--env-file" "$ENV_FILE" )

list_services() {
  docker compose "${EXTRA[@]}" -f "$COMPOSE_FILE" ps --format '{{.Service}}\t{{.State}}\t{{.Health}}' 2>/dev/null
}

while :; do
  now=$(date +%s)
  if (( now >= DEADLINE )); then
    printf '[wait-healthy] TIMEOUT after %ss — current state:\n' "$TIMEOUT" >&2
    list_services >&2
    exit 1
  fi

  pending=$(list_services | awk -F'\t' '
    $3 == "" && $2 == "running" { next }        # running without healthcheck = accept
    $3 == "healthy"             { next }
    { print $1 " (" $2 "/" ($3 == "" ? "no-hc" : $3) ")" }
  ')

  if [[ -z "$pending" ]]; then
    printf '[wait-healthy] all services healthy\n' >&2
    exit 0
  fi

  printf '[wait-healthy] pending: %s\n' "$(printf '%s' "$pending" | tr '\n' ' ')" >&2
  sleep 3
done
