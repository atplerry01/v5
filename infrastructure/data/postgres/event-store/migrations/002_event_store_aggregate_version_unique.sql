-- phase1-gate-migration: enforce per-aggregate ordering at the database
-- level AND provide the supporting load-path index in a single object.
--
-- Single canonical index that:
--   • enforces UNIQUE (aggregate_id, version) — physical defense in depth
--     for H8a (advisory lock) and H8b (optimistic concurrency). Even if a
--     future code path bypasses both checks, the database itself rejects
--     duplicate version assignments.
--   • supports LoadEventsAsync's `WHERE aggregate_id = @id ORDER BY version`
--     query as a single index range scan in physical version order
--     (replaces today's full table scan + sort).
--
-- Pre-flight: verify no existing duplicates BEFORE attempting to create
-- the unique index. The deterministic event-id PK has historically
-- prevented duplicates (id = SHA256("{aggregateId}:{version}")), so this
-- check is expected to be a no-op against any healthy event store. If it
-- is NOT a no-op, the migration aborts with a clear message and the index
-- is never created — converts a silent post-deploy surprise into an
-- explicit migration failure.
--
-- CONCURRENTLY: the index is built without an exclusive table lock so
-- production writes proceed during the build. CREATE INDEX CONCURRENTLY
-- cannot run inside an explicit transaction block; this file must be
-- executed via the project's migrate.sh runner which pipes each file to
-- psql in auto-commit mode (NOT -1/--single-transaction). Verified
-- 2026-04-08 against infrastructure/deployment/scripts/migrate.sh.

DO $$
DECLARE
    dup_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO dup_count FROM (
        SELECT aggregate_id, version
        FROM events
        GROUP BY aggregate_id, version
        HAVING COUNT(*) > 1
    ) dups;

    IF dup_count > 0 THEN
        RAISE EXCEPTION
            'phase1-gate-migration: refusing to create UNIQUE (aggregate_id, version): '
            '% duplicate pair(s) found in events table. The deterministic event-id PK '
            'should have prevented this — investigate before promoting to constraint.',
            dup_count;
    END IF;
END$$;

CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS uq_events_aggregate_version
    ON events (aggregate_id, version);
