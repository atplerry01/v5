-- Migration: Create workflow_state table for workflow execution persistence
-- Classification: runtime / workflow-state / persistence

CREATE TABLE IF NOT EXISTS workflow_state (
    workflow_id     TEXT        PRIMARY KEY,
    workflow_name   TEXT        NOT NULL,
    current_step_index INTEGER NOT NULL DEFAULT 0,
    execution_hash  TEXT        NOT NULL DEFAULT '',
    status          TEXT        NOT NULL DEFAULT 'Running',
    serialized_state TEXT       NOT NULL DEFAULT '{}',
    created_at      TIMESTAMPTZ NOT NULL,
    updated_at      TIMESTAMPTZ NOT NULL
);

CREATE INDEX idx_workflow_state_status ON workflow_state (status);
CREATE INDEX idx_workflow_state_workflow_name ON workflow_state (workflow_name);
