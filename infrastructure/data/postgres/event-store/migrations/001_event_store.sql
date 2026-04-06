CREATE TABLE events (
    id              UUID        PRIMARY KEY,
    aggregate_id    UUID        NOT NULL,
    aggregate_type  TEXT        NOT NULL,
    event_type      TEXT        NOT NULL,
    payload         JSONB       NOT NULL,
    version         INT         NOT NULL,
    created_at      TIMESTAMP   NOT NULL DEFAULT NOW()
);
