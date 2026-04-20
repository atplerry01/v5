-- R2.A.3b / R-DLQ-STORE-01 — dead-letter persistence table.
--
-- Mirrors every Kafka `*.deadletter` message so operator inspection,
-- re-drive controls (R4.B), and retention sweeps (R2.A.4) operate
-- against queryable storage rather than Kafka offsets.
--
-- Idempotency invariant: INSERT ... ON CONFLICT (event_id) DO NOTHING.
-- Two concurrent consumers routing the same poison message converge
-- to a single row — the first writer wins, subsequent writers no-op.
--
-- Never hard-delete rows. `MarkReprocessedAsync` sets the reprocessed_*
-- columns and `ListAsync` filters by `reprocessed_at IS NULL` by default.
-- Retention is a separate sweep job (R2.A.4).

CREATE TABLE IF NOT EXISTS dead_letter_entries (
    event_id                UUID            PRIMARY KEY,
    source_topic            TEXT            NOT NULL,
    event_type              TEXT            NOT NULL,
    correlation_id          UUID            NOT NULL,
    causation_id            UUID            NULL,
    enqueued_at             TIMESTAMPTZ     NOT NULL,
    failure_category        TEXT            NULL,
    last_error              TEXT            NOT NULL,
    attempt_count           INTEGER         NOT NULL DEFAULT 0,
    payload                 BYTEA           NOT NULL,
    schema_version          INTEGER         NULL,
    reprocessed_at          TIMESTAMPTZ     NULL,
    reprocessed_by_identity TEXT            NULL
);

-- Queries by source topic + time window are the R4.B inspection shape.
-- Partial index on the unreprocessed slice keeps the working set small
-- (operators overwhelmingly inspect active deadletter entries).
CREATE INDEX IF NOT EXISTS idx_dead_letter_entries_topic_enqueued
    ON dead_letter_entries (source_topic, enqueued_at DESC)
    WHERE reprocessed_at IS NULL;

-- Correlation-id lookups for linking a deadletter row to the originating
-- execution (R4 operator forensic flows).
CREATE INDEX IF NOT EXISTS idx_dead_letter_entries_correlation
    ON dead_letter_entries (correlation_id);
