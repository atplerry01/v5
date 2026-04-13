-- S0 fix: enforce NOT NULL on correlation_id and causation_id.
--
-- Migration 003 added these columns as nullable. The adapter now populates
-- them on every INSERT (PostgresEventStoreAdapter persists envelope metadata).
-- This migration:
--   1. Backfills any existing NULL rows with a sentinel so the constraint
--      can be applied (existing rows predate the fix and have no context).
--   2. Sets NOT NULL on both columns.
--
-- Idempotency: the UPDATE is a no-op when no NULLs remain, and ALTER COLUMN
-- SET NOT NULL is idempotent in Postgres (re-running on a NOT NULL column
-- succeeds silently).

-- Backfill existing rows that predate this fix
UPDATE events
SET correlation_id = '00000000-0000-0000-0000-000000000000'
WHERE correlation_id IS NULL;

UPDATE events
SET causation_id = '00000000-0000-0000-0000-000000000000'
WHERE causation_id IS NULL;

-- Enforce NOT NULL going forward
ALTER TABLE events ALTER COLUMN correlation_id SET NOT NULL;
ALTER TABLE events ALTER COLUMN causation_id SET NOT NULL;
