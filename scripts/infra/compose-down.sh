#!/usr/bin/env bash
# compose-down.sh — tear down the validation stack. Preserves volumes by
# default; pass --volumes to wipe.
#
# Classification : phase5-operational-activation / economic-system

set -u

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$REPO_ROOT"

COMPOSE_FILE="infrastructure/docker/docker-compose.validation.yml"
ENV_FILE="infrastructure/docker/.env.local"

WIPE=0
for arg in "$@"; do
  case "$arg" in
    --volumes|-v) WIPE=1 ;;
    *) printf '[compose-down] unknown arg: %s\n' "$arg" >&2; exit 2 ;;
  esac
done

EXTRA=()
[[ -f "$ENV_FILE" ]] && EXTRA+=( "--env-file" "$ENV_FILE" )

if [[ $WIPE -eq 1 ]]; then
  docker compose "${EXTRA[@]}" -f "$COMPOSE_FILE" down --volumes --remove-orphans
else
  docker compose "${EXTRA[@]}" -f "$COMPOSE_FILE" down --remove-orphans
fi
