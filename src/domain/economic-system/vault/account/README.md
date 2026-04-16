# Vault Account Domain

Classification: `economic-system`
Context: `vault`
Domain Group: `vault`
Domain: `account`

## Purpose

Sole aggregate of the vault bounded context. Models a subject-owned vault
account that partitions capital across four doctrine-locked slices
(`Slice1`..`Slice4`) and tracks the running `VaultMetrics` view
(`Total` / `Free` / `Locked` / `Invested`).

The vault account is the liquidity endpoint for the economic-system flows:

- inbound funding,
- inbound revenue receipts (SPV profit),
- outbound payouts (debit / credit against Slice1),
- intra-vault investment moves (Slice1 → Slice2).

## Doctrine (locked)

- `Slice1` is the sole liquidity gateway — funding, revenue, payout debit,
  payout credit all land on `Slice1`.
- Investment is restricted to the path `Slice1 → Slice2`.
- `Slice3` / `Slice4` are reserved for future doctrine extension — no
  event path currently writes to them.
- `VaultMetrics.Total = Free + Locked + Invested` is enforced on every
  metrics recomputation.

## Aggregate

- `VaultAccountAggregate` — event-sourced, sealed. Owns exactly four
  `VaultSliceEntity` instances and a `VaultMetrics` value object.

## State Model

- `Active` — operational; accepts funding / revenue / investment /
  payout debit / payout credit.
- `Closed` — terminal; all mutating operations are rejected with
  `VaultAccountErrors.AccountIsClosed()`.

## Value Objects

- `VaultAccountId` — typed Guid wrapper (`From(Guid)` factory; empty guards).
- `VaultAccountStatus` — enum (`Active`, `Closed`).

## Events

- `VaultAccountCreatedEvent` — vault account registered for a subject.
- `VaultFundedEvent` — capital added to `Slice1`.
- `CapitalAllocatedToSliceEvent` — capital moved from `Slice1` to
  `Slice2`.
- `SpvProfitReceivedEvent` — SPV revenue booked to `Slice1`.
- `VaultDebitedEvent` — payout debit against `Slice1`.
- `VaultCreditedEvent` — payout credit against `Slice1`.

## Invariants

- The aggregate must own exactly four slices.
- Slice types must be unique across the slice collection.
- Currency on every inbound operation must equal the vault's declared
  currency.
- Amount > 0 for every inbound operation.
- Investment and debit cannot exceed `Metrics.Free`.
- `Slice1`-only policy for funding / revenue / payout debit / payout
  credit.
- `Slice1 → Slice2`-only policy for investment.

## Specifications

- `VaultAccountCanFundSpecification` — vault is Active, currency matches,
  amount > 0.
- `VaultAccountCanInvestSpecification` — vault is Active, currency matches,
  amount > 0, amount ≤ `Metrics.Free`.
- `VaultAccountCanPayoutSpecification` — vault is Active, slice is
  `Slice1`, amount > 0, amount ≤ `Metrics.Free`.

## Errors

- `VaultAccountErrors.InvalidAmount`
- `VaultAccountErrors.CurrencyMismatch`
- `VaultAccountErrors.AccountIsClosed`
- `VaultAccountErrors.OnlySlice1AcceptsFunding`
- `VaultAccountErrors.OnlySlice1ToSlice2Investment`
- `VaultAccountErrors.OnlySlice1Payout`
- `VaultAccountErrors.InsufficientFreeCapital`
- `VaultAccountErrors.SliceNotFound`
- `VaultAccountErrors.SliceCountInvariantViolation`
- `VaultAccountErrors.DuplicateSliceTypeInvariantViolation`

## Commands

- `CreateVaultAccountCommand`
- `FundVaultCommand`
- `InvestCommand`
- `ApplyRevenueCommand`
- `DebitSliceCommand`
- `CreditSliceCommand`

## Queries

- `GetVaultAccountByIdQuery` — read-model lookup by `VaultAccountId`.

## Projection / Read Model

- `VaultAccountReadModel`
  - Schema: `projection_economic_vault_account`
  - Table: `vault_account_read_model`
  - Aggregate type: `VaultAccount`
  - Tracks `TotalBalance` / `FreeBalance` / `LockedBalance` /
    `InvestedBalance` derived from the event stream.

## Policy Actions

- `whyce.economic.vault.account.create`
- `whyce.economic.vault.account.fund`
- `whyce.economic.vault.account.invest`
- `whyce.economic.vault.account.apply_revenue`
- `whyce.economic.vault.account.debit`
- `whyce.economic.vault.account.credit`

## Topic

- `whyce.economic.vault.account.commands`
- `whyce.economic.vault.account.events`
- `whyce.economic.vault.account.retry`
- `whyce.economic.vault.account.deadletter`

## API Surface

- `POST /api/economic/vault/account/create` — create a new vault account.
- `POST /api/economic/vault/account/fund` — fund `Slice1`.
- `POST /api/economic/vault/account/invest` — allocate `Slice1 → Slice2`.
- `POST /api/economic/vault/account/apply-revenue` — book SPV revenue to
  `Slice1`.
- `POST /api/economic/vault/account/debit` — payout debit against
  `Slice1`.
- `POST /api/economic/vault/account/credit` — payout credit against
  `Slice1`.
- `GET  /api/economic/vault/account/{id}` — fetch vault account read
  model.

## Canonical Path

- Domain:       `src/domain/economic-system/vault/account/`
- Commands:     `src/shared/contracts/economic/vault/account/`
- Events:       `src/shared/contracts/events/economic/vault/account/`
- Handler:      `src/engines/T2E/economic/vault/account/`
- Projection:   `src/projections/economic/vault/account/`
- Controller:   `src/platform/api/controllers/economic/vault/account/`
- Composition:  `src/platform/host/composition/economic/vault/account/`

## E2E Path

API → `ISystemIntentDispatcher` → Runtime Control Plane (8-stage pipeline)
→ T2E handler → `VaultAccountAggregate` → Event Store → WhyceChain Anchor
→ Outbox → `whyce.economic.vault.account.events` →
`VaultAccountProjectionHandler` →
`projection_economic_vault_account.vault_account_read_model` → Response.
