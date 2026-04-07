-- Outbox recovery — Phase 1 gate unblock (S0).
-- KafkaOutboxPublisher uses three terminal-ish statuses:
--   pending    → eligible for next batch
--   failed     → eligible for retry while retry_count < max_retry_count
--   deadletter → excluded from retry; requires operator action
--   published  → terminal success
--
-- Re-queues failed/deadletter rows so they get a fresh publish attempt.
-- Resets retry_count and last_error so the retry budget starts over.
-- Safe to run repeatedly.

-- Option 1 (default): rescue everything that hasn't been published.
UPDATE outbox
SET status      = 'pending',
    retry_count = 0,
    last_error  = NULL
WHERE status IN ('failed', 'deadletter', 'pending');

-- Option 2 (DEV ONLY — destructive): full reset.
-- TRUNCATE outbox;
