-- R3.B.1 / D-R3B-4 / R-OUT-EFF-QUEUE-01..03 —
-- Dedicated queue table for outbound-effect dispatch.
-- This table is a PROJECTION of the aggregate's event stream; the authoritative
-- source of truth remains the event store. Lost rows are reconstructable from
-- the OutboundEffect* event history (recovery command is R3.B.3 scope).
--
-- Uniqueness on (provider_id, idempotency_key) enforces dedup-on-schedule at
-- the DB level; the dispatcher performs an application-level pre-check and
-- then relies on this constraint to short-circuit concurrent schedules.

CREATE TABLE IF NOT EXISTS outbound_effect_dispatch_queue (
    effect_id          UUID        PRIMARY KEY,
    provider_id        TEXT        NOT NULL,
    effect_type        TEXT        NOT NULL,
    idempotency_key    TEXT        NOT NULL,
    status             TEXT        NOT NULL,
    attempt_count      INT         NOT NULL DEFAULT 0,
    max_attempts       INT         NOT NULL,
    next_attempt_at    TIMESTAMPTZ NOT NULL,
    dispatch_deadline  TIMESTAMPTZ NOT NULL,
    ack_deadline       TIMESTAMPTZ NULL,
    finality_deadline  TIMESTAMPTZ NULL,
    last_error         TEXT        NULL,
    claimed_by         TEXT        NULL,
    claimed_at         TIMESTAMPTZ NULL,
    created_at         TIMESTAMPTZ NOT NULL,
    updated_at         TIMESTAMPTZ NOT NULL,
    payload            JSONB       NOT NULL,

    CONSTRAINT ux_outbound_effect_dispatch_queue_idempotency
        UNIQUE (provider_id, idempotency_key)
);

-- Fast lookup: which rows are ready to dispatch?
CREATE INDEX IF NOT EXISTS ix_outbound_effect_dispatch_queue_next_attempt
    ON outbound_effect_dispatch_queue (status, next_attempt_at)
    WHERE claimed_by IS NULL AND status IN ('Scheduled', 'TransientFailed');

-- Fast lookup: which acknowledged rows have an expired ack deadline?
CREATE INDEX IF NOT EXISTS ix_outbound_effect_dispatch_queue_ack_deadline
    ON outbound_effect_dispatch_queue (ack_deadline)
    WHERE ack_deadline IS NOT NULL AND status = 'Dispatched';

-- Fast lookup: which rows' finality window has expired?
CREATE INDEX IF NOT EXISTS ix_outbound_effect_dispatch_queue_finality_deadline
    ON outbound_effect_dispatch_queue (finality_deadline)
    WHERE finality_deadline IS NOT NULL AND status = 'Acknowledged';
