-- Phase 3.6 T3.6.2 — verification that the parties backfill landed cleanly.
--
-- Pass/fail contract:
--   * The single SELECT must return COUNT = 0.
--   * Any non-zero row count is a hard failure: at least one contract row
--     still lacks a populated `parties` array, so the Phase 3 pipeline will
--     reject any new revenue-processing intent that targets that contract.
--
-- Run inside `whyce_projections` after 001_backfill_parties.sql completes.
-- Wire into deployment as a post-backfill smoke gate (e.g. CI step that
-- fails the deploy if the count is non-zero).

SELECT
    COUNT(*) AS contract_rows_missing_parties,
    array_agg(aggregate_id ORDER BY aggregate_id) FILTER (
        WHERE NOT (state ? 'parties')
           OR state->'parties' IS NULL
           OR jsonb_typeof(state->'parties') = 'null'
           OR (jsonb_typeof(state->'parties') = 'array'
               AND jsonb_array_length(state->'parties') = 0)
    ) AS offending_contract_ids
FROM projection_economic_revenue_contract.revenue_contract_read_model
WHERE NOT (state ? 'parties')
   OR state->'parties' IS NULL
   OR jsonb_typeof(state->'parties') = 'null'
   OR (jsonb_typeof(state->'parties') = 'array'
       AND jsonb_array_length(state->'parties') = 0);
