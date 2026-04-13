#!/usr/bin/env bash
# Docker control utility — stop/start infrastructure services
set -euo pipefail

KAFKA_CONTAINER="${KAFKA_CONTAINER:-whyce-kafka}"
POSTGRES_CONTAINER="${POSTGRES_CONTAINER:-whyce-postgres}"
PROJECTIONS_CONTAINER="${PROJECTIONS_CONTAINER:-whyce-postgres-projections}"
CHAIN_CONTAINER="${CHAIN_CONTAINER:-whyce-whycechain-db}"
OPA_CONTAINER="${OPA_CONTAINER:-whyce-opa}"

docker_stop() {
    local container="$1"
    echo "[STEP] Stopping container: $container"
    docker stop "$container" 2>/dev/null || true
    echo "[STEP] Container $container stopped"
}

docker_start() {
    local container="$1"
    echo "[STEP] Starting container: $container"
    docker start "$container"
    echo "[STEP] Container $container started"
}

wait_for_postgres() {
    local container="$1"
    local port="${2:-5432}"
    local user="${3:-whyce}"
    local max_wait="${4:-30}"
    local elapsed=0
    echo "[STEP] Waiting for postgres in $container (max ${max_wait}s)..."
    while [ $elapsed -lt $max_wait ]; do
        if docker exec "$container" pg_isready -U "$user" -p "$port" >/dev/null 2>&1; then
            echo "[STEP] Postgres in $container is ready"
            return 0
        fi
        sleep 1
        elapsed=$((elapsed + 1))
    done
    echo "[FAIL] Postgres in $container did not become ready within ${max_wait}s"
    return 1
}

wait_for_kafka() {
    local max_wait="${1:-60}"
    local interval="${SLEEP_INTERVAL:-2}"
    local elapsed=0
    echo "[STEP] Waiting for Kafka broker + metadata (max ${max_wait}s)..."
    while [ $elapsed -lt $max_wait ]; do
        # kafka-topics --list requires a fully operational broker with metadata.
        # sh -c wrapping prevents Git Bash path mangling on Windows.
        if docker exec "$KAFKA_CONTAINER" sh -c \
            "/opt/kafka/bin/kafka-topics.sh --bootstrap-server localhost:9092 --list" \
            >/dev/null 2>&1; then
            echo "[PASS] Kafka ready (broker + metadata) after ${elapsed}s"
            return 0
        fi
        sleep "$interval"
        elapsed=$((elapsed + interval))
    done
    echo "[FAIL] Kafka did not become ready within ${max_wait}s"
    return 1
}

wait_for_opa() {
    local max_wait="${1:-15}"
    local elapsed=0
    echo "[STEP] Waiting for OPA (max ${max_wait}s)..."
    while [ $elapsed -lt $max_wait ]; do
        if curl -s http://localhost:8181/health >/dev/null 2>&1; then
            echo "[STEP] OPA is ready"
            return 0
        fi
        sleep 1
        elapsed=$((elapsed + 1))
    done
    echo "[FAIL] OPA did not become ready within ${max_wait}s"
    return 1
}

stop_kafka()    { docker_stop "$KAFKA_CONTAINER"; }
start_kafka()   { docker_start "$KAFKA_CONTAINER"; wait_for_kafka; }
stop_postgres() { docker_stop "$POSTGRES_CONTAINER"; }
start_postgres(){ docker_start "$POSTGRES_CONTAINER"; wait_for_postgres "$POSTGRES_CONTAINER" 5432; }
stop_opa()      { docker_stop "$OPA_CONTAINER"; }
start_opa()     { docker_start "$OPA_CONTAINER"; wait_for_opa; }
