# Package

**Classification:** business-system
**Context:** offering
**Domain-Group:** commercial-shape
**Domain:** package
**Namespace:** `Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package`

## Responsibility
Commercial packaging of an offering (e.g. sizing/tier packaging).

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

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, offering, package)`; the domain-group is a folder-level grouping and is not part of the route.
