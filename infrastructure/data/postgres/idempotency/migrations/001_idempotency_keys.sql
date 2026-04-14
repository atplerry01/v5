CREATE TABLE IF NOT EXISTS idempotency_keys (
    key         TEXT        NOT NULL PRIMARY KEY,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
