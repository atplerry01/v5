# Amendment

**Classification:** business-system
**Context:** order
**Domain-Group:** order-change
**Domain:** amendment
**Namespace:** `Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment`

## Responsibility
Change to a placed order request (semantically distinct from agreement/change-control/amendment).

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

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, order, amendment)`; the domain-group is a folder-level grouping and is not part of the route.
