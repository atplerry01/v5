# ProviderAvailability

**Classification:** business-system
**Context:** provider
**Domain-Group:** provider-scope
**Domain:** provider-availability
**Namespace:** `Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability`

## Responsibility
Availability window/shape of a provider.

## Canonical Artifact Layout (DS-R3a)
- `aggregate/` — root aggregate
- `entity/` — domain entities
- `error/` — domain-specific exceptions
- `event/` — domain events
- `service/` — domain services
- `specification/` — business-rule specifications
- `value-object/` — value objects

## Status
S4 — E1 domain model implemented (Batch 4). Aggregate + VOs + events + specs + errors.

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, provider, provider-availability)`; the domain-group is a folder-level grouping and is not part of the route.
