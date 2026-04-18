# Contract `parties` projection backfill — Phase 3.6

Closes Finding 7 from `claude/new-rules/_archives/20260417-113821-economic-system-phase3-pipeline-drift.md`.

## What it does

Adds a `parties` array to the JSONB `state` column of every existing row in
`projection_economic_revenue_contract.revenue_contract_read_model`. The data
is rebuilt from the canonical `RevenueContractCreatedEvent` payload in
`whyce_eventstore.events` — no fabrication, no inference.

## Why it is needed

Phase 3 (`Phase 3 — Canonical Economic Pipeline`) introduced the `parties`
projection so `IContractAllocationsResolver.ResolveAsync` can derive
distribution allocations from the read side. Contracts created before that
reducer change have no `parties` key in their state JSONB, so the resolver
returns an empty list and `TriggerDistributionStep` fails closed. New
contracts created after Phase 3 are unaffected.

## Files

- `001_backfill_parties.sql` — the backfill itself (idempotent; safe to re-run).
- `002_validate_parties.sql` — verification query. Must return `0`.

## Two ways to run

The event store and projections live in **separate** PostgreSQL databases
(`whyce_eventstore` and `whyce_projections`, per
`infrastructure/deployment/docker-compose.yml`). Pick the option that fits
your environment.

### Option A — `dblink` (recommended; matches the script as written)

Requires the `dblink` extension (the script issues `CREATE EXTENSION IF NOT
EXISTS dblink`, which needs the `whyce` role to have `CREATE` on the
projections database).

```bash
EVENT_STORE_CONN="host=postgres dbname=whyce_eventstore user=whyce password=${POSTGRES_PASSWORD}"

psql "host=postgres dbname=whyce_projections user=whyce password=${POSTGRES_PASSWORD}" \
    -v event_store_conn="'${EVENT_STORE_CONN}'" \
    -f 001_backfill_parties.sql

psql "host=postgres dbname=whyce_projections user=whyce password=${POSTGRES_PASSWORD}" \
    -f 002_validate_parties.sql
```

The validation script must report `contract_rows_missing_parties = 0`.

### Option B — two-step `\copy` (no `dblink` required)

For locked-down environments where `dblink` is not permitted.

```bash
# 1. Export source rows from the event store as TSV.
psql "host=postgres dbname=whyce_eventstore user=whyce password=${POSTGRES_PASSWORD}" \
    -c "\copy (SELECT aggregate_id, payload FROM events WHERE event_type = 'RevenueContractCreatedEvent') TO STDOUT" \
    > /tmp/contract_creates.tsv

# 2. Load into a staging table inside the projections database, then run
#    the same UPDATE used in 001 (skip the dblink CREATE EXTENSION + the
#    dblink-INSERT statement; INSERT into _contract_parties_backfill from
#    the staging table instead).
psql "host=postgres dbname=whyce_projections user=whyce password=${POSTGRES_PASSWORD}" <<'SQL'
BEGIN;

CREATE TEMP TABLE _contract_creates_staging (
    aggregate_id UUID NOT NULL,
    payload      JSONB NOT NULL
) ON COMMIT DROP;

\copy _contract_creates_staging FROM '/tmp/contract_creates.tsv'

CREATE TEMP TABLE _contract_parties_backfill (
    aggregate_id UUID PRIMARY KEY,
    parties      JSONB NOT NULL
) ON COMMIT DROP;

INSERT INTO _contract_parties_backfill (aggregate_id, parties)
SELECT
    s.aggregate_id,
    (
        SELECT jsonb_agg(
            jsonb_build_object(
                'partyId',         rule->>'PartyId',
                'sharePercentage', (rule->>'SharePercentage')::numeric
            )
            ORDER BY ord
        )
        FROM jsonb_array_elements(s.payload->'RevenueShareRules')
            WITH ORDINALITY AS x(rule, ord)
    )
FROM _contract_creates_staging s;

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

COMMIT;
SQL

psql "host=postgres dbname=whyce_projections user=whyce password=${POSTGRES_PASSWORD}" \
    -f 002_validate_parties.sql
```

## Idempotency

`001_backfill_parties.sql` only mutates rows that lack a populated `parties`
array. Re-running after a successful backfill is a no-op. The shape of the
JSONB array (camelCase `partyId` / `sharePercentage`) is identical to what
the live reducer writes, so a backfilled row is indistinguishable from a
freshly-projected row at runtime.

## Rollback

There is no rollback. The backfill is purely additive (it adds the `parties`
key); no existing field is mutated or dropped. To revert in an emergency,
run inside `whyce_projections`:

```sql
UPDATE projection_economic_revenue_contract.revenue_contract_read_model
SET state = state - 'parties';
```
