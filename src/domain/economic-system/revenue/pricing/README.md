# Domain: Pricing

## Classification
economic-system

## Context
revenue

## Domain Responsibility
Defines pricing structures — recorded price definitions with model type, contract reference, and currency. Adjustments are immutable events; the aggregate enforces invariants on definition and adjustment but performs no calculation logic.

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
  AdjustPrice() -> price replaced, previous price captured in event (repeatable, with reason)
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
* Price must be >= 0 (invariant), > 0 at creation and adjustment.
* Price adjustments require a non-empty reason.

## Specifications
* **CanAdjustPriceSpecification** — Pricing must have a valid contract and positive current price for adjustment to be allowed.

## Errors
* **InvalidPrice** — Price must be greater than zero.
* **MissingContractReference** — Pricing must reference a contract.
* **MissingAdjustmentReason** — Adjustment must include a reason.
* **NegativePrice** — Invariant: price cannot be negative.
* **ContractReferenceMustExist** — Invariant: must reference a contract.

## Domain Services
* **PricingCalculationService** — Reserved for cross-aggregate pricing coordination within the pricing context. Empty placeholder in E1; expansion deferred to E2+.

## Boundary Statement
This domain defines pricing structure only and contains no price calculation logic. The `PricingCalculationService` placeholder reserves the slot for E2+ calculation work; it intentionally exposes no methods today.

## Status
**S4 — Invariants + Specifications Complete**
