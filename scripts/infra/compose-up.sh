#!/usr/bin/env bash
# compose-up.sh — bring up the validation stack (idempotent).
#
# Classification : phase5-operational-activation / economic-system
# Usage          : ./scripts/infra/compose-up.sh
#
# Requires infrastructure/docker/.env.local (see .env.example).

set -u

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$REPO_ROOT"

COMPOSE_FILE="infrastructure/docker/docker-compose.validation.yml"
ENV_FILE="infrastructure/docker/.env.local"

if [[ ! -f "$ENV_FILE" ]]; then
  printf '[compose-up] missing %s — copy .env.example and fill it in.\n' "$ENV_FILE" >&2
  exit 2
fi

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --build
rc=$?

if [[ $rc -ne 0 ]]; then
  printf '[compose-up] compose up failed (rc=%d)\n' "$rc" >&2
fi

exit $rc
