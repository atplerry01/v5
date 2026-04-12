#!/usr/bin/env bash
# phase1.5-S5.5 / Stage A — apply migrations the base compose does not
# auto-mount.
#
# The base docker-compose.yml mounts only event-store/migrations into
# /docker-entrypoint-initdb.d on the main postgres container, and only
# the projections tree on postgres-projections. The whycechain-db
# container mounts NOTHING — its schema (chain/migrations/001) was
# applied manually at some prior point and is now baked into the
# whycechain_data volume on existing dev machines, but a fresh
# compose-up against an empty volume would crash the host on first
# anchor.
#
# This script runs after all three databases report healthy and
# applies the missing SQL via psql. It is idempotent (every migration
# uses CREATE TABLE IF NOT EXISTS), so re-running against a populated
# database is a no-op.
#
# Targets:
#   - postgres        (whyce_eventstore)  — outbox, hsid
#   - whycechain-db   (whycechain)        — chain
#
# postgres-projections is left alone — its init mount already covers it.
#
# Connection details mirror the base compose service environment.

set -euo pipefail

log() { printf '[apply-extra-migrations] %s\n' "$*" >&2; }

PGUSER=whyce
PGPASSWORD="${POSTGRES_PASSWORD:-change_me_securely}"
export PGPASSWORD

apply_dir() {
    local host="$1" db="$2" dir="$3" sentinel_table="$4"
    log "applying $dir to $host/$db (sentinel table: $sentinel_table) ..."
    # phase1.5-S5.5 / Stage B FIX #1: the original loop globbed
    # `/migrations/$dir/*.sql` directly, but the migrations directories
    # actually live one level deeper at `/migrations/$dir/migrations/`
    # (e.g. `infrastructure/data/postgres/outbox/migrations/*.sql`).
    # The earlier glob matched zero files in every directory and the
    # `[[ -f ]]` guard returned 0, silently skipping every migration.
    # Stage A only appeared to work because the named volumes had the
    # schemas baked in from prior manual psql runs. Stage B exposed
    # this when the chain DB (which had NEVER been populated) failed.
    #
    # phase1.5-S5.5 / Stage B FIX #2: not every migration in this repo
    # uses `CREATE TABLE IF NOT EXISTS` (outbox/001 and chain/001 use
    # bare `CREATE TABLE`), so re-applying against a populated database
    # fails with "relation already exists". We therefore pre-check a
    # sentinel table per directory and skip the entire directory if it
    # exists. The sentinel is the table created by the FIRST migration
    # in the directory; later migrations in the same directory are
    # ALTER-only and would not collide. This makes the runner safely
    # idempotent against both fresh and pre-populated volumes.
    local exists
    exists=$(psql -h "$host" -U "$PGUSER" -d "$db" -tAc \
        "SELECT 1 FROM information_schema.tables WHERE table_name = '$sentinel_table' LIMIT 1" 2>/dev/null || echo "")
    if [[ "$exists" == "1" ]]; then
        log "  sentinel '$sentinel_table' already present — skipping (idempotent re-run)"
        return 0
    fi

    local sql_glob="/migrations/$dir/migrations/*.sql"
    local found=0
    for sql in $sql_glob; do
        [[ -f "$sql" ]] || continue
        found=1
        log "  $sql"
        psql -v ON_ERROR_STOP=1 -h "$host" -U "$PGUSER" -d "$db" -f "$sql"
    done
    if [[ $found -eq 0 ]]; then
        log "  FATAL: no .sql files matched $sql_glob — refusing to claim success"
        return 1
    fi
}

# Wait briefly for healthchecks (compose depends_on already gates this,
# but defensive retry handles a slow first boot).
wait_pg() {
    local host="$1" db="$2"
    for attempt in {1..30}; do
        if pg_isready -h "$host" -U "$PGUSER" -d "$db" >/dev/null 2>&1; then
            return 0
        fi
        sleep 1
    done
    log "FATAL: $host/$db not ready after 30s"
    return 1
}

wait_pg postgres whyce_eventstore
wait_pg whycechain-db whycechain

apply_dir postgres      whyce_eventstore outbox  outbox
apply_dir postgres      whyce_eventstore hsid    hsid_sequences
apply_dir whycechain-db whycechain       chain   whyce_chain

log "all extra migrations applied successfully"
