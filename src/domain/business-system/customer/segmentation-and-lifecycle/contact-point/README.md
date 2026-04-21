# ContactPoint

**Classification:** business-system
**Context:** customer
**Domain-Group:** segmentation-and-lifecycle
**Domain:** contact-point
**Namespace:** `Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint`

## Responsibility
Business-meaning contact point for a customer.

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
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, customer, contact-point)`; the domain-group is a folder-level grouping and is not part of the route.
