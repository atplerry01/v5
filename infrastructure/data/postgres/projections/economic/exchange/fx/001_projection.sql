CREATE SCHEMA IF NOT EXISTS projection_economic_exchange_fx;

CREATE TABLE projection_economic_exchange_fx.fx_read_model (
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

CREATE INDEX idx_eefx_aggregate_type    ON projection_economic_exchange_fx.fx_read_model (aggregate_type);
CREATE INDEX idx_eefx_correlation_id    ON projection_economic_exchange_fx.fx_read_model (correlation_id);
CREATE INDEX idx_eefx_projected_at      ON projection_economic_exchange_fx.fx_read_model (projected_at);
CREATE INDEX idx_eefx_currency_pair     ON projection_economic_exchange_fx.fx_read_model ((state->>'baseCurrency'), (state->>'quoteCurrency'));
CREATE INDEX idx_eefx_status            ON projection_economic_exchange_fx.fx_read_model ((state->>'status'));
