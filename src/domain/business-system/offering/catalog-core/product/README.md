# Product

**Classification:** business-system
**Context:** offering
**Domain-Group:** catalog-core
**Domain:** product
**Namespace:** `Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product`

## Responsibility
Sellable product unit in the catalog.

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
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, offering, product)`; the domain-group is a folder-level grouping and is not part of the route.
