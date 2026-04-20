# LineItem

**Classification:** business-system
**Context:** order
**Domain-Group:** order-core
**Domain:** line-item
**Namespace:** `Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem`

## Responsibility
Single line within an order.

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
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, order, line-item)`; the domain-group is a folder-level grouping and is not part of the route.
