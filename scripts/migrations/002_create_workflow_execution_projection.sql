-- Migration: Create workflow_execution_read_model for projections layer
-- Classification: projections / orchestration-system / workflow
-- Replaces the deprecated runtime-owned workflow_state table.

CREATE SCHEMA IF NOT EXISTS projection_orchestration_workflow;

CREATE TABLE IF NOT EXISTS projection_orchestration_workflow.workflow_execution_read_model (
    workflow_execution_id UUID        PRIMARY KEY,
    workflow_name         TEXT        NOT NULL,
    current_step_index    INTEGER     NOT NULL DEFAULT 0,
    execution_hash        TEXT        NOT NULL DEFAULT '',
    status                TEXT        NOT NULL DEFAULT 'Running',
    failed_step_name      TEXT        NULL,
    failure_reason        TEXT        NULL,
    projected_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    correlation_id        UUID        NULL
);

CREATE INDEX IF NOT EXISTS idx_workflow_exec_status ON projection_orchestration_workflow.workflow_execution_read_model (status);
CREATE INDEX IF NOT EXISTS idx_workflow_exec_workflow_name ON projection_orchestration_workflow.workflow_execution_read_model (workflow_name);
