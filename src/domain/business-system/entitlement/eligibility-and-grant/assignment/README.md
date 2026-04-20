# Assignment

**Classification:** business-system
**Context:** entitlement
**Domain-Group:** eligibility-and-grant
**Domain:** assignment
**Namespace:** `Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment`

## Responsibility
Assignment of a granted entitlement to a concrete beneficiary scope.

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
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, entitlement, assignment)`; the domain-group is a folder-level grouping and is not part of the route.
