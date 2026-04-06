-- Add topic column for canonical Kafka topic routing.
-- Topic is resolved at enqueue time by TopicNameResolver and stored with the outbox entry.
-- KafkaOutboxPublisher reads this column instead of using a hardcoded default topic.

ALTER TABLE outbox ADD COLUMN topic TEXT NOT NULL DEFAULT 'whyce.events';

-- Remove the default after backfill so future inserts MUST provide a topic.
-- ALTER TABLE outbox ALTER COLUMN topic DROP DEFAULT;
