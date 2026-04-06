CREATE TABLE whyce_chain (
    block_id            UUID            PRIMARY KEY,
    correlation_id      UUID            NOT NULL,
    event_hash          TEXT            NOT NULL,
    decision_hash       TEXT            NOT NULL,
    previous_block_hash TEXT            NOT NULL,
    timestamp           TIMESTAMPTZ     NOT NULL
);

CREATE INDEX idx_whyce_chain_correlation ON whyce_chain (correlation_id);
CREATE INDEX idx_whyce_chain_timestamp ON whyce_chain (timestamp DESC);
