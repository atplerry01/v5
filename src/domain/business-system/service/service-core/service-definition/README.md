# ServiceDefinition

**Classification:** business-system
**Context:** service
**Domain-Group:** service-core
**Domain:** service-definition
**Namespace:** `Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition`

## Responsibility
Underlying business-meaning definition of a service (distinct from offering/service-offering).

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

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Routing
Per DS-R8, `DomainRoute` remains the 3-tuple `(business-system, service, service-definition)`; the domain-group is a folder-level grouping and is not part of the route.
