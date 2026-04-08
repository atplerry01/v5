-- Add topic column for canonical Kafka topic routing.
-- Topic is resolved at enqueue time by TopicNameResolver and stored with the
-- outbox entry. KafkaOutboxPublisher reads this column instead of using a
-- hardcoded default topic.
--
-- phase1.6-S2.2 cleanup: the previous version of this migration carried a
-- commented-out `ALTER TABLE ... DROP DEFAULT` line as a "future cleanup"
-- placeholder. That comment is removed. The DEFAULT 'whyce.events' is the
-- intentional steady-state of this migration — it acts as a backfill value
-- for the existing rows at the moment 002 runs, and forward inserts always
-- supply an explicit topic via TopicNameResolver. Dropping the default
-- would break the backfill semantics for any database that has not yet
-- been migrated past 002, so the default stays. The architectural
-- intent is "all NEW inserts must supply a topic", which is enforced
-- in code at the call site (PostgresOutboxAdapter), not at the schema
-- level.

ALTER TABLE outbox ADD COLUMN topic TEXT NOT NULL DEFAULT 'whyce.events';
