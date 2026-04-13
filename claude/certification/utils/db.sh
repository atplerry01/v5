#!/usr/bin/env bash
# Database utility — psql helpers for event store, outbox, projections, chain
# All queries routed via docker exec — no local psql install required
set -euo pipefail

DB_USER="${DB_USER:-whyce}"
DB_PASS="${DB_PASS:-change_me_securely}"

EVENTSTORE_DB="${EVENTSTORE_DB:-whyce_eventstore}"
PROJECTIONS_DB="${PROJECTIONS_DB:-whyce_projections}"
CHAIN_DB="${CHAIN_DB:-whycechain}"

# Canonical projection schema: projection_{classification}_{context}_{domain}
TODO_PROJECTION_TABLE="${TODO_PROJECTION_TABLE:-projection_operational_sandbox_todo.todo_read_model}"

# Container names (matching docker-compose)
POSTGRES_CONTAINER="${POSTGRES_CONTAINER:-whyce-postgres}"
PROJECTIONS_CONTAINER="${PROJECTIONS_CONTAINER:-whyce-postgres-projections}"
CHAIN_CONTAINER="${CHAIN_CONTAINER:-whyce-whycechain-db}"

# Run SQL against event store (via docker exec into whyce-postgres)
eventstore_query() {
    local sql="$1"
    docker exec -e PGPASSWORD="$DB_PASS" "$POSTGRES_CONTAINER" \
        psql -h localhost -U "$DB_USER" -d "$EVENTSTORE_DB" -t -A -c "$sql"
}

# Run SQL against projections DB (via docker exec into whyce-postgres-projections)
projections_query() {
    local sql="$1"
    docker exec -e PGPASSWORD="$DB_PASS" "$PROJECTIONS_CONTAINER" \
        psql -h localhost -U "$DB_USER" -d "$PROJECTIONS_DB" -t -A -c "$sql"
}

# Run SQL against chain DB (via docker exec into whyce-whycechain-db)
chain_query() {
    local sql="$1"
    docker exec -e PGPASSWORD="$DB_PASS" "$CHAIN_CONTAINER" \
        psql -h localhost -U "$DB_USER" -d "$CHAIN_DB" -t -A -c "$sql"
}

# Count events for an aggregate
count_events() {
    local aggregate_id="$1"
    eventstore_query "SELECT COUNT(*) FROM events WHERE aggregate_id = '${aggregate_id}';"
}

# Get event by aggregate ID
get_events() {
    local aggregate_id="$1"
    eventstore_query "SELECT id, event_type, version, correlation_id FROM events WHERE aggregate_id = '${aggregate_id}' ORDER BY version;"
}

# Get outbox status for a correlation ID (domain event only, excludes policy audit events)
get_outbox_status() {
    local correlation_id="$1"
    eventstore_query "SELECT status FROM outbox WHERE correlation_id = '${correlation_id}' AND event_type NOT LIKE 'Policy%' ORDER BY created_at DESC LIMIT 1;"
}

# Count outbox rows for a correlation ID (domain events only)
count_outbox() {
    local correlation_id="$1"
    eventstore_query "SELECT COUNT(*) FROM outbox WHERE correlation_id = '${correlation_id}' AND event_type NOT LIKE 'Policy%';"
}

# Get outbox rows pending
count_outbox_pending() {
    eventstore_query "SELECT COUNT(*) FROM outbox WHERE status = 'pending';"
}

# Get projection for an aggregate
get_projection() {
    local aggregate_id="$1"
    projections_query "SELECT aggregate_id, current_version, state, correlation_id FROM ${TODO_PROJECTION_TABLE} WHERE aggregate_id = '${aggregate_id}';"
}

# Count projections for an aggregate
count_projections() {
    local aggregate_id="$1"
    projections_query "SELECT COUNT(*) FROM ${TODO_PROJECTION_TABLE} WHERE aggregate_id = '${aggregate_id}';"
}

# Truncate todo projection table
truncate_todo_projection() {
    projections_query "TRUNCATE TABLE ${TODO_PROJECTION_TABLE};"
}

# Count all todo projections
count_all_projections() {
    projections_query "SELECT COUNT(*) FROM ${TODO_PROJECTION_TABLE};"
}

# Get chain block by correlation ID
get_chain_block() {
    local correlation_id="$1"
    chain_query "SELECT block_id, event_hash, decision_hash, previous_block_hash FROM whyce_chain WHERE correlation_id = '${correlation_id}';"
}

# Count chain blocks for a correlation ID
count_chain_blocks() {
    local correlation_id="$1"
    chain_query "SELECT COUNT(*) FROM whyce_chain WHERE correlation_id = '${correlation_id}';"
}

# Total events count
count_all_events() {
    eventstore_query "SELECT COUNT(*) FROM events;"
}
