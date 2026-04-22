CREATE SCHEMA IF NOT EXISTS projection_platform_event_event_metadata;

CREATE TABLE projection_platform_event_event_metadata.event_metadata_read_model (
    aggregate_id        UUID        PRIMARY KEY,
    aggregate_type      TEXT        NOT NULL,
    current_version     INT         NOT NULL DEFAULT 0,
    state               JSONB       NOT NULL DEFAULT '{}',
    last_event_id       UUID,
    last_event_type     TEXT,
    correlation_id      UUID,
    idempotency_key     TEXT        UNIQUE,
    projected_at        TIMESTAMP   NOT NULL DEFAULT NOW(),
    created_at          TIMESTAMP   NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_pe_meta_aggregate_type
    ON projection_platform_event_event_metadata.event_metadata_read_model (aggregate_type);

CREATE INDEX idx_pe_meta_correlation_id
    ON projection_platform_event_event_metadata.event_metadata_read_model (correlation_id);

CREATE INDEX idx_pe_meta_projected_at
    ON projection_platform_event_event_metadata.event_metadata_read_model (projected_at);
