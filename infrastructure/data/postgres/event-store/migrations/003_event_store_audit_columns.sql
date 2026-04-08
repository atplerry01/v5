-- phase1.6-S1.3: extend events table with audit columns required for full
-- WhyceChain traceability + policy lineage + cross-event causality.
--
-- Background: the phase-1 audit sweep (S1.3 / DRIFT-3) identified that
-- EventEnvelope at src/runtime/event-fabric/EventFabric.cs:99-114 already
-- populates execution_hash, correlation_id, causation_id, policy_decision_hash
-- and policy_version on every persist call, but the events table had no
-- columns to receive them. Audit metadata was being silently dropped at the
-- adapter boundary, breaking the phase-1 evidence chain.
--
-- This migration adds the columns. It DOES NOT:
--   • backfill any existing rows
--   • enforce NOT NULL on the new columns
--   • add indexes
--   • modify any code path that writes to the events table
--
-- The columns are introduced as nullable, no-default — existing rows remain
-- valid (NULL audit columns), new rows can opt-in once the adapter is
-- updated to include them in INSERT statements. The adapter update is
-- deliberately deferred to a follow-up gate so this migration can land
-- atomically with no behavioral coupling.
--
-- Field provenance (locked alignment with EventEnvelope, for the
-- forthcoming adapter update):
--   execution_hash       — runtime/determinism (ExecutionHash.Compute)
--   correlation_id       — CommandContext.CorrelationId
--   causation_id         — CommandContext.CausationId
--   policy_decision_hash — WHYCEPOLICY (PolicyMiddleware)
--   policy_version       — WHYCEPOLICY (PolicyMiddleware)
--
-- Idempotency: each ADD COLUMN uses IF NOT EXISTS so re-running the
-- migration is a no-op. Forward-only — there is no DROP COLUMN companion.
-- Safe under migrate.sh which runs each file in auto-commit mode.

ALTER TABLE events
    ADD COLUMN IF NOT EXISTS execution_hash       TEXT,
    ADD COLUMN IF NOT EXISTS correlation_id       UUID,
    ADD COLUMN IF NOT EXISTS causation_id         UUID,
    ADD COLUMN IF NOT EXISTS policy_decision_hash TEXT,
    ADD COLUMN IF NOT EXISTS policy_version       TEXT;
