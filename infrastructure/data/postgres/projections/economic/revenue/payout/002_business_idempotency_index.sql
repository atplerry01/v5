-- Phase 3.5 T3.5.4 — index on the business idempotency key carried inside
-- the payout JSONB state column. The existing `idempotency_key` column on
-- the table is reserved by PostgresProjectionStore for envelope-id
-- de-duplication (one entry per event-id), so the per-payout business
-- idempotency key (`payout|{distributionId:N}|{spvId}`) lives inside
-- `state->>'idempotencyKey'` instead. This index gives O(log n) lookup
-- when retry-detection or reconciliation queries scan by business key.
--
-- Safe for existing environments: additive, IF NOT EXISTS, no constraint
-- change, no row rewrite.

CREATE INDEX IF NOT EXISTS idx_errp_business_idempotency_key
    ON projection_economic_revenue_payout.revenue_payout_read_model ((state->>'idempotencyKey'))
    WHERE state ? 'idempotencyKey';
