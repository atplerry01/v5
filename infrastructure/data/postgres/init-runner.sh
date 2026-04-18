#!/bin/bash
# init-runner.sh — applies every *.sql file found in direct subdirectories of
# /docker-entrypoint-initdb.d/ in lexical order, then in lexical filename order
# within each subdirectory. Exists because the postgres docker-entrypoint only
# globs the top-level of /docker-entrypoint-initdb.d/ and ignores subdirs, so
# a fan-out across multiple canonical migration trees (event-store / outbox /
# idempotency / hsid / chain) cannot be expressed by directory mounts alone.
#
# Naming convention: mount each canonical migration tree at a lex-sortable
# prefix such as 01-event-store/, 02-outbox/, 03-idempotency/, 04-hsid/. This
# script must itself sort BEFORE any of them (file names starting with 00-
# run first per postgres docker-entrypoint's ordering).
#
# Classification: phase5-operational-activation / economic-system

set -euo pipefail

echo "[init-runner] applying migrations from subdirectories of /docker-entrypoint-initdb.d/"

shopt -s nullglob
for dir in /docker-entrypoint-initdb.d/*/; do
  dir_name="$(basename "$dir")"
  # Recurse — some trees (projections) are deeply nested by domain. Sort gives
  # deterministic order within a single tree; ordering ACROSS trees is handled
  # by the lex-sorted subdirectory prefix (01-, 02-, ...). Skip `backfill`
  # paths: those are one-shot operational scripts that require runtime psql
  # variables (e.g. `:event_store_conn`) and are applied manually post-init,
  # not at schema bootstrap.
  for sql in $(find "$dir" -name '*.sql' -type f -not -path '*/backfill/*' | sort); do
    rel="${sql#/docker-entrypoint-initdb.d/}"
    echo "[init-runner] $rel"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -f "$sql"
  done
done

echo "[init-runner] all subdirectory migrations applied"
