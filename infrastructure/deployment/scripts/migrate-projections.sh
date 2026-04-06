#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INFRA_DIR="$(dirname "$(dirname "$SCRIPT_DIR")")"
PROJECTIONS_DIR="$INFRA_DIR/data/postgres/projections"

echo "=== WHYCESPACE Projection Migration ==="
echo "Projections dir: $PROJECTIONS_DIR"

# Walk classification > context > domain structure and apply SQL files
find "$PROJECTIONS_DIR" -name "*.sql" -type f | sort | while read -r SQL_FILE; do
  RELATIVE="${SQL_FILE#$PROJECTIONS_DIR/}"
  echo "Applying: $RELATIVE"
  docker exec -i whyce-postgres-projections psql -U whyce -d whyce_projections < "$SQL_FILE"
done

echo "=== Projection migration complete ==="
