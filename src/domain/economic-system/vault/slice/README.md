# Vault Slice (Entity Sub-Domain)

Classification: `economic-system`
Context: `vault`
Domain Group: `vault`
Sub-Domain: `slice`

## Doctrine — Intentional Sub-Domain

`slice` is an **entity-only sub-domain** under the vault domain group.
It intentionally DOES NOT constitute an independent bounded context:

- no aggregate root — `VaultSliceEntity` is a child entity owned
  exclusively by `VaultAccountAggregate`,
- no slice-level domain events — state changes ride on the aggregate's
  business-level events (`VaultFundedEvent`, `CapitalAllocatedToSliceEvent`,
  `VaultDebitedEvent`, `VaultCreditedEvent`, `SpvProfitReceivedEvent`),
- no commands, handlers, or projection of its own,
- no Kafka topic,
- no API surface.

Per Phase 2B vault doctrine, a vault account owns exactly four slices
(`Slice1..Slice4`), and each slice's lifecycle is owned by the parent
aggregate. The parent aggregate is responsible for enforcing the
Slice-level routing invariants (funding → `Slice1`, investment →
`Slice1 → Slice2`, payout debit/credit → `Slice1`).

## Entities

- `VaultSliceEntity` — a single slice within a vault account. Exposes
  internal mutation methods (`Credit` / `MoveToInvested` / `Debit`)
  invoked only from the parent aggregate's `Apply` projection of the
  aggregate event stream.

## Value Objects

- `SliceType` — doctrine-locked enum:
  - `Slice1` — liquidity gateway (sole entry/exit point).
  - `Slice2` — investment staging (fed from `Slice1`).
  - `Slice3` — reserved for future doctrine extension.
  - `Slice4` — reserved for future doctrine extension.

## Invariants (enforced at the aggregate level)

- A vault account owns exactly four `VaultSliceEntity` instances.
- Slice types are unique across the owned collection.
- Only `Slice1` accepts inbound funding, revenue, and payout
  debit/credit.
- Investment is restricted to `Slice1 → Slice2`.
- `Slice3` / `Slice4` are currently quiescent — no event path writes to
  them.

## Consumers

- `VaultAccountAggregate.Slices` — read-only collection of the four
  owned slices.
- `VaultAccountAggregate.Apply(...)` — the sole writer of slice state;
  invokes `Credit` / `MoveToInvested` / `Debit` in response to
  aggregate events.

## Shared-Contract Mirror

For over-the-wire payloads, `SliceType` is mirrored as
`Whycespace.Shared.Contracts.Economic.Vault.Account.VaultSliceType` (and
again under `Whycespace.Shared.Contracts.Events.Economic.Vault.Account`
for stored schemas). The contract enum is not a domain type — its
integer values MUST remain byte-equal to the domain `SliceType` enum.

## Canonical Path

- `src/domain/economic-system/vault/slice/value-object/SliceType.cs`
- `src/domain/economic-system/vault/slice/entity/VaultSliceEntity.cs`

No other layers are allocated — projections, handlers, controllers, and
topics for the vault domain are all owned by the `account` domain.
