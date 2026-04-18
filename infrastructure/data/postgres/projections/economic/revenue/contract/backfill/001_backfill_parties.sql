-- Phase 3.6 T3.6.1 — backfill `state->'parties'` on every contract row in
-- `projection_economic_revenue_contract.revenue_contract_read_model` from
-- the canonical `RevenueContractCreatedEvent` payload in the event store.
--
-- Why this script exists
-- ----------------------
-- Phase 3 added the `parties` field to `ContractReadModel` so the Phase 3
-- pipeline (TriggerDistributionStep → CreateDistributionCommand) can resolve
-- distribution allocations from the read side without touching the write
-- aggregate. Contracts created before that change have no `parties` key in
-- their projected JSONB state, so `IContractAllocationsResolver.ResolveAsync`
-- returns an empty list and the pipeline fails closed.
--
-- Source of truth
-- ---------------
-- The `RevenueContractCreatedEvent` row in `whyce_eventstore.events` carries
-- the canonical `RevenueShareRules` array. We rebuild the projection's
-- `parties` array from that authoritative source — no fabrication.
-- Contracts whose creation event is missing from the event store are NOT
-- mutated; the validation query (002_validate_parties.sql) will fail loudly
-- so the operator can investigate before re-running the pipeline.
--
-- Cross-database access
-- ---------------------
-- `whyce_eventstore` and `whyce_projections` are separate Postgres databases
-- (see infrastructure/deployment/docker-compose.yml). This script runs INSIDE
-- the `whyce_projections` database and reads the events table via the
-- `dblink` extension. If your environment does not permit `dblink`, see
-- `README.md` in this directory for the equivalent two-step `\copy`-based
-- procedure.
--
-- Required env / parameters
-- -------------------------
--   :event_store_conn — libpq connection string to whyce_eventstore.
--                       Example: 'host=postgres dbname=whyce_eventstore user=whyce password=...'
--                       Pass via psql:  -v event_store_conn="'<conn>'"
--
-- Idempotent
-- ----------
-- The UPDATE filter targets only rows where `state->'parties'` is missing
-- or empty. Re-running this script after a successful backfill is a no-op.
-- Deterministic: the JSONB array is built in the exact order of the source
-- event's RevenueShareRules array.

CREATE EXTENSION IF NOT EXISTS dblink;

BEGIN;

-- 1. Pull (aggregate_id, parties_json) tuples from the event store into a
--    temp staging table inside this transaction.
CREATE TEMP TABLE _contract_parties_backfill (
    aggregate_id UUID PRIMARY KEY,
    parties      JSONB NOT NULL
) ON COMMIT DROP;

INSERT INTO _contract_parties_backfill (aggregate_id, parties)
SELECT
    src.aggregate_id,
    -- The event payload is PascalCase (default JsonSerializer); the read-model
    -- JSONB state is camelCase (PostgresProjectionStore.StateJsonOptions).
    -- Re-key from PartyId/SharePercentage → partyId/sharePercentage so the
    -- backfilled `parties` array matches what live reducer writes look like.
    (
        SELECT jsonb_agg(
            jsonb_build_object(
                'partyId',         rule->>'PartyId',
                'sharePercentage', (rule->>'SharePercentage')::numeric
            )
            ORDER BY ord  -- preserve event-order for determinism
        )
        FROM jsonb_array_elements(src.payload->'RevenueShareRules')
            WITH ORDINALITY AS x(rule, ord)
    ) AS parties
FROM dblink(
    :'event_store_conn',
    $sql$
        SELECT aggregate_id, payload
        FROM events
        WHERE event_type = 'RevenueContractCreatedEvent'
    $sql$
) AS src(aggregate_id UUID, payload JSONB);

-- 2. UPDATE only the contract rows that lack a populated parties array.
--    `WHERE NOT (state ? 'parties')` catches rows where the key is absent.
--    `state->'parties' IS NULL` catches rows where the key is present-but-null.
--    `jsonb_array_length(state->'parties') = 0` catches rows with an empty array.
WITH targets AS (
    SELECT rm.aggregate_id, b.parties
    FROM projection_economic_revenue_contract.revenue_contract_read_model AS rm
    JOIN _contract_parties_backfill AS b ON b.aggregate_id = rm.aggregate_id
    WHERE NOT (rm.state ? 'parties')
       OR rm.state->'parties' IS NULL
       OR jsonb_typeof(rm.state->'parties') = 'null'
       OR (jsonb_typeof(rm.state->'parties') = 'array'
           AND jsonb_array_length(rm.state->'parties') = 0)
)
UPDATE projection_economic_revenue_contract.revenue_contract_read_model AS rm
SET
    state        = jsonb_set(rm.state, '{parties}', t.parties, true),
    projected_at = NOW()
FROM targets t
WHERE rm.aggregate_id = t.aggregate_id;

-- 3. Report what was touched. The validation script (002) is the formal
--    pass/fail; this NOTICE is for operator visibility during the run.
DO $$
DECLARE
    backfilled INT;
    remaining  INT;
BEGIN
    SELECT COUNT(*) INTO backfilled
    FROM _contract_parties_backfill;

    SELECT COUNT(*) INTO remaining
    FROM projection_economic_revenue_contract.revenue_contract_read_model
    WHERE NOT (state ? 'parties')
       OR state->'parties' IS NULL
       OR jsonb_typeof(state->'parties') = 'null'
       OR (jsonb_typeof(state->'parties') = 'array'
           AND jsonb_array_length(state->'parties') = 0);

    RAISE NOTICE 'parties backfill: % source events available, % rows still missing parties (must be 0 for the validation query to pass).',
        backfilled, remaining;
END$$;

COMMIT;
