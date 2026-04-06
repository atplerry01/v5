#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOYMENT_DIR="$(dirname "$SCRIPT_DIR")"
INFRA_DIR="$(dirname "$DEPLOYMENT_DIR")"

# Environment binding: default to local, accept override
ENVIRONMENT="${1:-local}"
ENV_FILE="$INFRA_DIR/environments/$ENVIRONMENT/environment.json"

echo "=== WHYCESPACE Infrastructure Bootstrap ==="
echo "Environment:    $ENVIRONMENT"
echo "Env config:     $ENV_FILE"
echo "Deployment dir: $DEPLOYMENT_DIR"

# Validate environment exists
if [ ! -f "$ENV_FILE" ]; then
  echo "ERROR: Environment config not found: $ENV_FILE"
  echo "Available: local, dev, staging, production"
  exit 1
fi

echo ""
echo "Environment config:"
cat "$ENV_FILE"
echo ""

# Export environment for docker-compose
export WHYCE_ENVIRONMENT="$ENVIRONMENT"
export WHYCE_ENV_FILE="$ENV_FILE"

echo "[1/4] Loading environment: $ENVIRONMENT"
echo "  Kafka:      $(cat "$ENV_FILE" | grep -o '"bootstrap_servers"[^,]*' | head -1 || echo 'default')"
echo "  Postgres:   $(cat "$ENV_FILE" | grep -o '"database"[^,]*' | head -1 || echo 'default')"
echo "  Redis:      $(cat "$ENV_FILE" | grep -o '"port": 6379' | head -1 || echo 'default')"

echo "[2/5] Starting core infrastructure..."
docker compose -f "$DEPLOYMENT_DIR/docker-compose.yml" up -d kafka postgres postgres-projections redis opa minio prometheus

echo "[3/5] Running init services (topics + buckets)..."
docker compose -f "$DEPLOYMENT_DIR/docker-compose.yml" up -d kafka-init minio-init

echo "[4/5] Running projection migrations..."
bash "$SCRIPT_DIR/migrate-projections.sh"

echo "[5/5] Starting observability and UI services..."
docker compose -f "$DEPLOYMENT_DIR/docker-compose.yml" up -d \
  kafka-exporter postgres-exporter redis-exporter node-exporter \
  kafka-ui pgadmin redisinsight grafana whycechain-db

echo ""
echo "=== Bootstrap complete (environment: $ENVIRONMENT) ==="
docker compose -f "$DEPLOYMENT_DIR/docker-compose.yml" ps
