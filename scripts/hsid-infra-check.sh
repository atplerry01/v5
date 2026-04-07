#!/usr/bin/env bash
# HSID v2.1 H7 — CI infrastructure readiness check.
# Verifies the hsid_sequences table exists in $DATABASE_URL.
# Mirrors deterministic-id.guard.md G20 / audit A17.
set -euo pipefail

if [ -z "${DATABASE_URL:-}" ]; then
  echo "ERROR: DATABASE_URL is not set"
  exit 2
fi

echo "Checking HSID infrastructure on $DATABASE_URL ..."

if ! psql "$DATABASE_URL" -tAc "\dt hsid_sequences" | grep -q "hsid_sequences"; then
  echo "ERROR: hsid_sequences table missing"
  echo "Apply: infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql"
  exit 1
fi

echo "HSID infrastructure check PASSED"
