#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INFRA_DIR="$(dirname "$(dirname "$SCRIPT_DIR")")"
MIGRATIONS_DIR="$INFRA_DIR/data/postgres/event-store/migrations"

echo "=== WHYCESPACE Database Migration ==="
echo "Migrations dir: $MIGRATIONS_DIR"

for SQL_FILE in "$MIGRATIONS_DIR"/*.sql; do
  if [ -f "$SQL_FILE" ]; then
    echo "Applying: $(basename "$SQL_FILE")"
    docker exec -i whyce-postgres psql -U whyce -d whyce_eventstore < "$SQL_FILE"
  fi
done

echo "=== Migration complete ==="
