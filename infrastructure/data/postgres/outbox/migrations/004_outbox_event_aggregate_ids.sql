-- phase1-gate-S2: hoist event_id and aggregate_id out of the JSON payload
-- into discrete columns on the outbox table. Removes the publisher's
-- JsonDocument.Parse workaround (TryExtractAggregateId) and lets the
-- KafkaOutboxPublisher emit headers without touching the payload body.
--
-- Backfill strategy for existing rows:
--   event_id     ← outbox.id (rows already use deterministic ids)
--   aggregate_id ← '00000000-0000-0000-0000-000000000000' (legacy rows have no
--                  reliable way to recover the original aggregate id; they are
--                  test data on dev_wip and will be cleared before the next gate run)

ALTER TABLE outbox ADD COLUMN IF NOT EXISTS event_id     UUID;
ALTER TABLE outbox ADD COLUMN IF NOT EXISTS aggregate_id UUID;

UPDATE outbox SET event_id     = id          WHERE event_id     IS NULL;
UPDATE outbox SET aggregate_id = '00000000-0000-0000-0000-000000000000'
                                              WHERE aggregate_id IS NULL;

ALTER TABLE outbox ALTER COLUMN event_id     SET NOT NULL;
ALTER TABLE outbox ALTER COLUMN aggregate_id SET NOT NULL;

CREATE INDEX IF NOT EXISTS idx_outbox_aggregate_id ON outbox (aggregate_id);
