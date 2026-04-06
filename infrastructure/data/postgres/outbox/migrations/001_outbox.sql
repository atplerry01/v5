CREATE TABLE outbox (
    id                  UUID            PRIMARY KEY,
    correlation_id      UUID            NOT NULL,
    event_type          TEXT            NOT NULL,
    payload             JSONB           NOT NULL,
    idempotency_key     TEXT            NOT NULL,
    status              TEXT            NOT NULL DEFAULT 'pending',
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    published_at        TIMESTAMPTZ,

    CONSTRAINT uq_outbox_idempotency UNIQUE (idempotency_key)
);

CREATE INDEX idx_outbox_status_created ON outbox (status, created_at ASC)
    WHERE status = 'pending';
