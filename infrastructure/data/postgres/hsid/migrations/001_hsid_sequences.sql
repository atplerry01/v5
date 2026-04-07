-- HSID v2.1 — sequence store table.
-- Backs PostgresSequenceStoreAdapter / PersistedSequenceResolver.
-- Per deterministic-id.guard.md G19/G20: this migration MUST be applied
-- before host startup in any environment.

CREATE TABLE IF NOT EXISTS hsid_sequences (
    scope      TEXT   PRIMARY KEY,
    next_value BIGINT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_hsid_sequences_scope
    ON hsid_sequences(scope);
