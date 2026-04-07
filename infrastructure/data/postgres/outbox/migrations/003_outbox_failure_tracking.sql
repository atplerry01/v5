-- Phase 1 gate unblock (S0): row-level failure tracking for KafkaOutboxPublisher.
-- Allows the publisher to mark individual rows as 'failed' without crashing the batch,
-- and to record retry counts + last error for diagnostics and recovery.

ALTER TABLE outbox ADD COLUMN IF NOT EXISTS retry_count INT  NOT NULL DEFAULT 0;
ALTER TABLE outbox ADD COLUMN IF NOT EXISTS last_error  TEXT;

-- 'failed' is a valid status value alongside 'pending' and 'published'.
-- No CHECK constraint added — status remains free-form TEXT to match 001_outbox.sql.
