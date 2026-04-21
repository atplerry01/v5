# ServiceOption

**Classification:** business-system
**Context:** service
**Domain-Group:** service-core
**Domain:** service-option
**Namespace:** `Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption`

## Responsibility
Option/variant on a service definition.

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
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, service, service-option)`; the domain-group is a folder-level grouping and is not part of the route.
