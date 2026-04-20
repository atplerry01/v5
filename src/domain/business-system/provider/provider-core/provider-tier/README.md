# ProviderTier

**Classification:** business-system
**Context:** provider
**Domain-Group:** provider-core
**Domain:** provider-tier
**Namespace:** `Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier`

## Responsibility
Tier/classification of a provider.

## Canonical Artifact Layout (DS-R3a)
- `aggregate/` — root aggregate
- `entity/` — domain entities
- `error/` — domain-specific exceptions
- `event/` — domain events
- `service/` — domain services
- `specification/` — business-rule specifications
- `value-object/` — value objects

## Status
S4 — E1 domain model implemented (Batch 3). Aggregate + VOs + events + specs + errors.

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, provider, provider-tier)`; the domain-group is a folder-level grouping and is not part of the route.
