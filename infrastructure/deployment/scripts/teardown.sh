#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOYMENT_DIR="$(dirname "$SCRIPT_DIR")"

echo "=== WHYCESPACE Infrastructure Teardown ==="

docker compose -f "$DEPLOYMENT_DIR/docker-compose.yml" down

echo "=== Teardown complete ==="
