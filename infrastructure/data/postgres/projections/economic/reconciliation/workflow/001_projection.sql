CREATE SCHEMA IF NOT EXISTS projection_economic_reconciliation_workflow;

CREATE TABLE projection_economic_reconciliation_workflow.workflow_state (
    process_id      UUID        PRIMARY KEY,
    discrepancy_id  UUID        NULL,
    current_state   TEXT        NOT NULL DEFAULT 'Triggered',
    last_event      TEXT        NOT NULL DEFAULT '',
    correlation_id  UUID        NULL,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX idx_erw_discrepancy
    ON projection_economic_reconciliation_workflow.workflow_state (discrepancy_id)
    WHERE discrepancy_id IS NOT NULL;

CREATE INDEX idx_erw_current_state
    ON projection_economic_reconciliation_workflow.workflow_state (current_state);

CREATE INDEX idx_erw_correlation
    ON projection_economic_reconciliation_workflow.workflow_state (correlation_id);

CREATE INDEX idx_erw_updated_at
    ON projection_economic_reconciliation_workflow.workflow_state (updated_at DESC);
