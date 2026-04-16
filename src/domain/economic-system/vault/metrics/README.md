# Vault Metrics (Value-Object Sub-Domain)

Classification: `economic-system`
Context: `vault`
Domain Group: `vault`
Sub-Domain: `metrics`

## Doctrine — Intentional Sub-Domain

`metrics` is a **value-object-only sub-domain** under the vault domain
group. It intentionally DOES NOT constitute an independent bounded
context:

- no aggregate root,
- no domain events (per locked vault doctrine),
- no commands, handlers, or projection of its own,
- no Kafka topic,
- no API surface.

Per the vault doctrine, metrics are a **view of vault state, not a BC**.
Updates happen via business methods on `VaultAccountAggregate`, which
rebuilds the `VaultMetrics` value and stores it on itself and on each
owned `VaultSliceEntity`.

If a future batch elevates metrics to an independent BC (e.g. time-series
metrics emission), that change MUST update this README first and be
captured under `/claude/new-rules/` per CLAUDE.md §1c before any code
change.

## Value Objects

- `VaultMetrics` — immutable snapshot of the vault's capital distribution
  across `Total` / `Free` / `Locked` / `Invested`. Invariant-enforcing
  constructor: `Total == Free + Locked + Invested`. Non-negativity is
  enforced per bucket.

  Builder operations:
  - `Zero()` — the zero distribution.
  - `WithFunding(Amount)` — funding lands on `Free`; `Total` and `Free`
    grow by the same amount.
  - `WithInvestment(Amount)` — `Free → Invested`. `Total` unchanged.
  - `WithDebit(Amount)` — debit shrinks `Total` and `Free` by the same
    amount.

## Invariants

- `Total = Free + Locked + Invested` — enforced on every construction.
- All buckets are non-negative.
- Transformations preserve the invariant by construction (every builder
  returns a new `VaultMetrics` through the invariant-checking
  constructor).

## Consumers

- `VaultAccountAggregate.Metrics` — the authoritative aggregate-level
  snapshot.
- `VaultSliceEntity.Metrics` — the per-slice snapshot of capital
  attributed to each slice.

## Canonical Path

- `src/domain/economic-system/vault/metrics/value-object/VaultMetrics.cs`

No other layers are allocated — projections, handlers, controllers, and
topics for the vault domain are all owned by the `account` domain.
