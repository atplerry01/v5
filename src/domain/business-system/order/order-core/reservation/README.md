# Reservation

**Classification:** business-system
**Context:** order
**Domain-Group:** order-core
**Domain:** reservation
**Namespace:** `Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation`

## Responsibility
Reservation held against an order line/offer.

## Canonical Artifact Layout (DS-R3a)
- `aggregate/` — root aggregate (scaffold pending)
- `entity/` — domain entities (scaffold pending)
- `error/` — domain-specific exceptions (scaffold pending)
- `event/` — domain events (scaffold pending)
- `service/` — domain services (scaffold pending)
- `specification/` — business-rule specifications (scaffold pending)
- `value-object/` — value objects (scaffold pending)

## Status
S0 — scaffold only. Canonical artifacts to be materialised in a subsequent modelling pass.

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, order, reservation)`; the domain-group is a folder-level grouping and is not part of the route.
