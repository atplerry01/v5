# Pricing Domain

**Path:** `content-system/monetization/pricing`
**Namespace:** `Whycespace.Domain.ContentSystem.Monetization.Pricing`

## Purpose
Owns the definition of a pricing plan: name, price, and publication
state.

## Lifecycle
```
Draft ── Publish ──► Published ── Deprecate ──► Deprecated (terminal)
```

## Events
- `PricingPlanDefinedEvent`
- `PricingPriceAdjustedEvent`
- `PricingPlanPublishedEvent`
- `PricingPlanDeprecatedEvent`

## Invariants
1. Name required.
2. Amount ≥ 0; currency is ISO 4217 3-letter.
3. Publishing requires a price.
4. Deprecated is terminal.
