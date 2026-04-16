---
classification: audits
source: /pipeline/execution_context_v1.md batch (economic-system/revenue, 2026-04-16)
severity: S1 (blocks projection write; read-model unavailable)
---

# Drift capture: missing projection DDL for contract + pricing read models

## CLASSIFICATION
infrastructure — follow-up backlog.

## SOURCE
Pipeline audit of the `economic-system / revenue` batch. Two new projections were introduced:

- `ContractReadModel` written by `ContractProjectionHandler`
- `PricingReadModel` written by `PricingProjectionHandler`

Both use `PostgresProjectionStore<T>` (canonical per existing distribution/payout/revenue projections).

## DESCRIPTION
`PostgresProjectionStore<T>.UpsertAsync(...)` targets a per-read-model Postgres table whose DDL must exist in `infrastructure/data/postgres/projections/economic/revenue/...`. Without the migrations the Kafka-projection consumer will fail on first event with an `UndefinedTable` error.

Existing revenue projections have migrations (distribution, payout, revenue). Contract and pricing do not.

## PROPOSED RULE
Create migration SQL files mirroring the existing pattern:

- `infrastructure/data/postgres/projections/economic/revenue/contract/0001_contract_read_model.sql`
- `infrastructure/data/postgres/projections/economic/revenue/pricing/0001_pricing_read_model.sql`

Each migration must include the canonical audit columns used by `PostgresProjectionStore` (event_id, event_version, correlation_id, last_event_type) and an aggregate-id primary key.

Verify column list against an existing migration (e.g. `infrastructure/data/postgres/projections/economic/revenue/distribution/*.sql`) before committing.

## SEVERITY
S1 — blocks read-model availability but does not block write-side execution. Engine, event store, chain anchor, outbox, and Kafka publish all function; only the projection consumer stalls.
