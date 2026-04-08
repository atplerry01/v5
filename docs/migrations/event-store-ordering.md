# Event Store + Outbox + HSID Migration Ordering

**Status:** locked (phase1.6-S2.4)
**Audience:** anyone running `infrastructure/deployment/scripts/migrate.sh` against a fresh or upgraded database.

This document is the canonical ordering reference for the three Postgres migration trees that participate in the WBSM v3 event-sourced runtime. Until S2.4 the ordering was implicit lexicographic, which is correct *today* but invisible to anyone reading the migration files cold.

## TL;DR

Run the migration trees in this order:

```
1. infrastructure/data/postgres/event-store/migrations/
2. infrastructure/data/postgres/outbox/migrations/
3. infrastructure/data/postgres/hsid/migrations/
```

Within each tree, files are applied in **filename ascending order** (lexicographic). The `NNN_` prefix is load-bearing — it determines runtime order. Renaming a file changes when it runs.

## Why this order

### 1. event-store first

The event store is the source of truth. Nothing else can be replayed, anchored, or projected without it. Its migrations have no upstream dependencies.

| File | Adds | Depends on |
|---|---|---|
| `001_event_store.sql` | base `events` table | — |
| `002_event_store_aggregate_version_unique.sql` | `UNIQUE(aggregate_id, version)` index, built `CONCURRENTLY` | 001 |
| `003_event_store_audit_columns.sql` | five nullable audit columns (execution_hash, correlation_id, causation_id, policy_decision_hash, policy_version) | 001; independent of 002 |

**002 must run before 003** even though 003 does not technically depend on 002. The reason is operational: 002 is the only file in the tree that uses `CREATE INDEX CONCURRENTLY`, which cannot run inside an explicit transaction block. `migrate.sh` pipes each file to `psql` in auto-commit mode, so this works — but if you ever wrap the migrations in a single transaction (e.g. for a CI test rebuild), 002 will fail and 003 will silently never run. The fix is to keep `migrate.sh` auto-commit, not to reorder.

### 2. outbox second

The outbox table references no event-store columns directly, but the relay path it powers (Postgres → Kafka → projection consumer) reads payloads that are produced by code that depends on the event store schema being correct. Running outbox migrations against an event store that is missing columns is a deployment bug — the row insert will succeed but the runtime publish path will reference fields that do not exist.

| File | Adds | Depends on |
|---|---|---|
| `001_outbox.sql` | base `outbox` table | — |
| `002_outbox_add_topic.sql` | `topic TEXT NOT NULL DEFAULT 'whyce.events'` (default acts as backfill for existing rows; new inserts always supply an explicit topic from `TopicNameResolver`) | 001 |
| `003_outbox_failure_tracking.sql` | `retry_count INT`, `last_error TEXT` | 001 |
| `004_outbox_event_aggregate_ids.sql` | `event_id UUID`, `aggregate_id UUID`, both `NOT NULL` after backfill, plus `idx_outbox_aggregate_id` | 001 |
| `005_outbox_next_retry_at.sql` | `next_retry_at TIMESTAMPTZ` + partial index `WHERE status = 'failed'` | 003 (uses `status='failed'`) |

**On 004's all-zeros backfill:** the migration backfills `aggregate_id` for any pre-existing rows with `00000000-0000-0000-0000-000000000000`. This is intentional and documented in the migration body — legacy outbox rows have no reliable way to recover the original aggregate id, and they exist only on `dev_wip` test databases. The backfill value is a sentinel, not a real aggregate. Any production database that runs 004 against rows with the all-zeros sentinel should clear those rows manually before promoting to `main`. The architectural invariant "every outbox row has a real aggregate id" is enforced by the runtime publish path, not by the schema, because the backfill value is technically a valid UUID.

### 3. hsid last

The HSID sequence store is consulted by the runtime *during* command execution to stamp HSID v2.1 identifiers. It depends on no event-store or outbox tables, but it must exist before the host can boot — `HsidInfrastructureValidator` fails fast at startup if the sequence rows are missing.

| File | Adds | Depends on |
|---|---|---|
| `001_hsid_sequences.sql` | sequence storage rows for the canonical topology codes | — |

It is last in the ordering only by convention — being last makes startup failures easier to diagnose. If `migrate.sh` errors halfway through, the operator knows the missing piece is the HSID one.

## Failure modes if you ignore this

| Scenario | Symptom |
|---|---|
| Run outbox 004 before event-store 001 | Outbox table exists but the runtime publish path crashes at startup because no events table to read from |
| Run event-store 002 inside a transaction wrapper | `CREATE INDEX CONCURRENTLY` aborts; 003 never runs; events table is missing the audit columns; runtime persists envelopes whose audit metadata silently disappears |
| Apply event-store 003 to a database missing 002 | Audit columns land but the unique-version constraint never does; you lose the database-level defense in depth for H8b optimistic concurrency. Functional runtime, weakened invariant. |
| Apply hsid 001 to an empty database (no event store) | Sequence rows exist; first command dispatch fails because the runtime cannot append to a non-existent events table. Startup error, not silent. |

## Rebuilding from scratch

```bash
# stop the host
docker compose down

# wipe the data volumes (DESTRUCTIVE)
docker volume rm whyce_postgres_data

# bring postgres back up
docker compose up -d whyce-postgres

# apply all three migration trees in canonical order
./infrastructure/deployment/scripts/migrate.sh

# verify
docker exec whyce-postgres psql -U whyce -d whyce_eventstore -c '\d events'
docker exec whyce-postgres psql -U whyce -d whyce_eventstore -c '\d outbox'
docker exec whyce-postgres psql -U whyce -d whyce_eventstore -c '\d hsid_sequences'
```

The `\d` output should show every column listed in the tables above, plus the indexes named in the migrations.

## Adding a new migration

1. Pick the right tree. Event-store, outbox, and hsid each have their own migrations directory; cross-tree DDL is forbidden.
2. Use the next `NNN_` prefix in the chosen tree. Never reuse a number.
3. Use `IF NOT EXISTS` on every `ADD COLUMN` / `CREATE INDEX` so the migration is idempotent. Re-running migrate.sh against an already-migrated database must be a no-op.
4. If your migration uses `CREATE INDEX CONCURRENTLY` or any other DDL that cannot run inside a transaction, document it at the top of the file. `migrate.sh` already runs each file in auto-commit mode, but the constraint is invisible without the comment.
5. Forward-only — never add a `DROP COLUMN` companion. Schema rollback is by snapshot restore, not by reverse migration.
6. If the migration cross-references another tree (e.g. requires the event-store to be present), document the dependency at the top of the file *and* update this document.
