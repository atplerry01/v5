CREATE SCHEMA IF NOT EXISTS projection_content_streaming_delivery_governance_entitlement_hook;

CREATE TABLE projection_content_streaming_delivery_governance_entitlement_hook.entitlement_hook_read_model (
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

CREATE INDEX idx_pcsdgeh_aggregate_type
    ON projection_content_streaming_delivery_governance_entitlement_hook.entitlement_hook_read_model (aggregate_type);

CREATE INDEX idx_pcsdgeh_correlation_id
    ON projection_content_streaming_delivery_governance_entitlement_hook.entitlement_hook_read_model (correlation_id);

CREATE INDEX idx_pcsdgeh_projected_at
    ON projection_content_streaming_delivery_governance_entitlement_hook.entitlement_hook_read_model (projected_at);
