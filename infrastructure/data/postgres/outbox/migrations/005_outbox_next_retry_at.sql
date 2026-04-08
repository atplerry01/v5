-- phase1-gate-S5: exponential backoff for failed outbox rows.
-- Without this column, failed rows are re-attempted on the very next 1-second
-- poll, hammering Kafka and burning the retry budget in seconds. next_retry_at
-- gates re-selection until backoff has elapsed.
--
-- Backoff schedule (computed in KafkaOutboxPublisher): 2^retry_count seconds,
--   attempt 1 fail → +1s,  attempt 2 fail → +2s,  attempt 3 fail → +4s, ...
--
-- NULL means "eligible immediately" (default for pending rows that have never failed).

ALTER TABLE outbox ADD COLUMN IF NOT EXISTS next_retry_at TIMESTAMPTZ;

CREATE INDEX IF NOT EXISTS idx_outbox_next_retry_at
    ON outbox (next_retry_at)
    WHERE status = 'failed';
