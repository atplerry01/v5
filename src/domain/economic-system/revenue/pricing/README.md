# Domain: Pricing

## Classification
economic-system

## Context
revenue

## Domain Responsibility
Defines pricing structures — the recorded price definitions with model type, contract reference, and currency. This domain defines pricing structure only and contains no price calculation logic.

## Aggregate
* **PricingAggregate** — Root aggregate representing a pricing definition.
  * Private constructor; created via `DefinePrice(PricingId, Guid, PricingModel, Amount, Currency, Timestamp)` factory method.
  * Price adjustments via `AdjustPrice()` method with reason tracking.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
DefinePrice() -> Defined (initial price set from model)
  AdjustPrice() -> Adjusted (repeatable, with reason tracking, previous price captured)
```

## Value Objects
* **PricingId** — Deterministic identifier (validated non-empty Guid).
* **PricingModel** — Enum: `Fixed`, `Tiered`, `PerUnit`, `Percentage`.

## Events
* **PriceDefinedEvent** — Raised when price is defined (captures model, contract, currency).
* **PriceAdjustedEvent** — Raised when price is adjusted (captures previous and new price with reason).

## Invariants
* PricingId must not be null/default.
* ContractId must not be empty.
* Price must be >= 0 (invariant), > 0 at creation.
* Must not calculate price — structure definition only.
* Price adjustments require a reason.

## Specifications
* **HasContractSpecification** — ContractId non-empty.
* **CanAdjustPriceSpecification** — Pricing must have a valid contract and positive price.

## Errors
* **InvalidPrice** — Price must be greater than zero.
* **MissingContractReference** — Pricing must reference a contract.
* **MissingAdjustmentReason** — Adjustment must include a reason.
* **NegativePrice** — Invariant: price cannot be negative.
* **ContractReferenceMustExist** — Invariant: must reference a contract.

## Domain Services
* **PricingService** — Reserved for cross-aggregate coordination within pricing context.

## Lifecycle Pattern
TERMINAL — Price definitions are immutable records; adjustments create new price events.

## Boundary Statement
This domain defines pricing structure only and contains no price calculation logic.

## Status
**S4 — Invariants + Specifications Complete**
