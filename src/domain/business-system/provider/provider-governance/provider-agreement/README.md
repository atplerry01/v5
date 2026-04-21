# ProviderAgreement

**Classification:** business-system
**Context:** provider
**Domain-Group:** provider-governance
**Domain:** provider-agreement
**Namespace:** `Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement`

## Responsibility
Formal agreement governing the provider relationship (distinct from agreement/commitment/contract).

## Canonical Artifact Layout (DS-R3a)
- `aggregate/` — root aggregate
- `entity/` — domain entities
- `error/` — domain-specific exceptions
- `event/` — domain events
- `service/` — domain services
- `specification/` — business-rule specifications
- `value-object/` — value objects

## Status
S4 — E1 domain model implemented (Batch 2). Aggregate + VOs + events + specs + errors.

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, provider, provider-agreement)`; the domain-group is a folder-level grouping and is not part of the route.
