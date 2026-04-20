# Bundle

**Classification:** business-system
**Context:** offering
**Domain-Group:** catalog-core
**Domain:** bundle
**Namespace:** `Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle`

## Responsibility
Composite offering grouping multiple products/services into a single sellable unit.

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

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, offering, bundle)`; the domain-group is a folder-level grouping and is not part of the route.
