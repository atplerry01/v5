# PriceBook

**Classification:** business-system
**Context:** pricing
**Domain-Group:** pricing-structure
**Domain:** price-book
**Namespace:** `Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook`

## Responsibility
Curated set of prices for offerings.

## Canonical Artifact Layout (DS-R3a)
- `aggregate/` — root aggregate
- `entity/` — domain entities (none required at E1)
- `error/` — domain-specific exceptions
- `event/` — domain events
- `service/` — domain services (none required at E1)
- `specification/` — business-rule specifications
- `value-object/` — value objects

## Status
S4 — E1 domain model implemented. Aggregate + VOs + events + specs + errors.
(superseded below)

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, pricing, price-book)`; the domain-group is a folder-level grouping and is not part of the route.
