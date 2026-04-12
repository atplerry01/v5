# Domain: Wallet

## Classification
economic-system

## Context
transaction

## Purpose
User-facing abstraction for initiating transactions. A wallet maps a user to an account and serves as the entry point for requesting transactions. The wallet does NOT hold balances — it references an account in the capital context.

## Core Responsibilities
- Mapping a user to an account
- Initiating transaction requests (signals to instruction domain)
- Ensuring the wallet has a valid account before operating

## Aggregate(s)
- **WalletAggregate**
  - Event-sourced, sealed. Manages wallet creation and transaction request signaling
  - Invariants: Active wallet must have AccountId (non-empty); OwnerId must be non-empty; Status is immutable after creation

## Entities
None

## Value Objects
- **WalletId** — Typed Guid wrapper for unique wallet identity
- **WalletStatus** — Enum: Active, Inactive

## Domain Events
- **WalletCreatedEvent** — Wallet initialized with owner and account mapping
- **TransactionRequestedEvent** — User requests a transaction via wallet (signal event, does NOT mutate wallet state)

## Specifications
- **CanInitiateTransactionSpecification** — Status=Active AND AccountId non-empty

## Domain Services
- **WalletTransactionService** — Retrieves and validates wallet account mapping; throws if not mapped

## Invariants (CRITICAL)
- Wallet must be mapped to an account — cannot operate without one
- Cannot request transactions on non-active wallets
- Transaction amount must be positive
- Destination account must be specified

## Policy Dependencies
- Account mapping enforcement for active wallets

## Integration Points
- **instruction** — Wallet transaction requests flow to the instruction domain
- **account** (capital context) — Wallet references account via AccountId; wallet does NOT hold balances

## Lifecycle
```
Create() -> Active (immutable status)
  RequestTransaction() -> signal event only, no state mutation
```

## Notes
- TransactionRequestedEvent is a signal event — it does not mutate wallet state
- Wallet does NOT hold balances; it is a thin reference to an account in the capital context
- Cross-domain references (OwnerId, AccountId) use raw Guid to avoid coupling
- All error methods are strongly typed via static WalletErrors class
